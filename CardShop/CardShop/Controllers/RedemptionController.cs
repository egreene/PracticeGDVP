﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardShop.Service;
using CardShop.Models;
using System.Web.Services;

namespace CardShop.Controllers
{
    public class RedemptionController : Controller
    {
        // Fields
        public IDiscountService discountService { get; set; }

        // Constructor
        public RedemptionController()
        {
            this.discountService = new DiscountService();
        }

        //
        // GET: /Redemption/
        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("Redeem");
        }

        [HttpGet]
        public ActionResult Redeem()
        {
            ViewBag.listOfUsers = discountService.GetAllUsers().ToList();
            
            return View();
        }


        /// <summary>
        /// Method for retrieveing a discount to see if the coupon
        /// code is valid
        /// </summary>
        /// <returns>Returns UserDiscount</returns>
        [HttpPost]
        public ActionResult RedeemDiscount(int userId, String couponCode)
        {
            bool isSuccess;

            //  comes back null if there is no match.  isSuccess should be false too.
            UserDiscount coupon = discountService.GetCoupon(userId, couponCode, out isSuccess);

            // if true, redeem coupon, else coupon doesn't exist



            // set redemption to true and persist

            UserDiscount coupon2 = discountService.RedeemCoupon(coupon, out isSuccess);

            var returnObject = Json(new { });

            if (isSuccess)
            {
                returnObject = Json(new
                {
                    UserDiscountId = coupon.UserDiscountId,
                    DiscountRate = coupon.DiscountRate,
                    StartDate = coupon.StartDate,
                    EndDate = coupon.EndDate,
                    DiscountCode = coupon.DiscountCode,
                    UserId = coupon.UserId,
                });
            }
            return returnObject;
        }


       
    }
}
