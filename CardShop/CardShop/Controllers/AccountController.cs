﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Diagnostics;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using CardShop.Filters;
using CardShop.Models;
using CardShop.Auth;
using CardShop.Daos;

namespace CardShop.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller, IAccountController
    {

        private IPracticeGDVPDao dbContext { get; set; }
        private IWebSecurity WebSecurity { get; set; }

        #region Constructors

        public AccountController(IWebSecurity webSecurity)
        {
            WebSecurity = webSecurity;
        }

        public AccountController()
            : this(new WebSecurityWrapper())
        {
            
            dbContext = PracticeGDVPDao.GetInstance();
        }
        //
        // GET: /Account/Login

        #endregion

        #region Login/Logoff/Register Methods
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                //UserAuth.Current.User = dbContext.Users().Where(u => u.UserName == model.UserName).ToList()[0];
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("error", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout(); 
            //UserAuth.Current.Logout();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            RegisterModel model = new RegisterModel();

            List<SelectListItem> roleList = new List<SelectListItem>();

            roleList.Add(new SelectListItem()
            {
                Value = ((int)Role.User).ToString(),
                Text = "User"
            });
            roleList.Add(new SelectListItem()
            {
                Value = ((int)Role.StoreOwner).ToString(),
                Text = "Store Owner"
            });
            roleList.Add(new SelectListItem()
            {
                Value = ((int)Role.Admin).ToString(),
                Text = "Admin"
            });

            model.RoleList = new SelectList(roleList, "Value", "Text");
            return View(model);
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password, propertyValues: new
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        RoleId = Int32.Parse(model.RoleId),
                        isActive = true
                    });
                    WebSecurity.Login(model.UserName, model.Password);
                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("error", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        #endregion

        #region Manage Methods
        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        #endregion

        #region Password Reset Methods
        //
        // GET: /Account/PasswordReset

        [AllowAnonymous]
        public ActionResult PasswordReset()
        {
            return View();
        }

        //
        // POST: /Account/PasswordToken
        [HttpPost]
        [AllowAnonymous]
        public ActionResult PasswordToken(PasswordReset resetModel)
        {
            string ViewName = "";
            bool userExist = dbContext.Users().Any(u => u.UserName == resetModel.UserName);
            if (userExist)
            {
                ViewName = "PasswordResetFinal";
                // This token isn't used properly
                resetModel.ResetToken = WebSecurity.GeneratePasswordResetToken(resetModel.UserName);

                // here we would want to send the token to the users email to navigate back to the site confirming they are real.
                // or we would redirect them to a QuestionandAwnser page to fill out security questions.
            }
            else
            {
                ViewName = "PasswordUpdate";
                resetModel.Message = "I'm sorry we did not find account information linked to that User name. Please contact System admin";
            }

            return View(ViewName, resetModel);

        }

        //
        //POST: /Account/PasswordUpdate

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PasswordUpdate(PasswordReset model)
        {
            bool updatedPassword = WebSecurity.ResetPassword(model.ResetToken, model.NewPassword);
            if (updatedPassword)
            {
                model.Message = "Your password was succesfully updated";
            }
            else
            {
                model.Message = "Your Password has failed to be reset. Please contact system administrator.";
            }
            return View(model);
        }


        #endregion

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
