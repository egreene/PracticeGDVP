﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CardShop.Models;
using CardShop.Utilities;
using CardShop.Daos;
using System.Data.Entity;

namespace CardShop.Service
{

    /// <summary>
    /// DiscountService class will handle all service type requests
    /// </summary>
    public class DiscountService : IDiscountService
    {
        /// <summary>
        /// CouponUtility
        /// </summary>
        public IUserDiscountUtility couponUtility { get; set; }
        public IPracticeGDVPDao dbContext { get; set; }
        private static DiscountService discountService;
        /// <summary>
        /// GetAllUsers gets a list of all the Users
        /// </summary>
        /// <returns> a list of all the users in the DB</returns>
        public virtual List<User> GetAllUsers()
        {
            List<User> users;
            using (var ctx = dbContext)
            {
                //get all users to a list
                var user = ctx.Users();
                users = user.ToList();
            }
            return users;
        }

        /// <summary>
        /// Get Coupon by userId and discountCode
        /// returns null if no UserDiscount is found
        /// Using outs to return additional error information
        /// Also checks for expiration and redemption status
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="DiscountCode"></param>
        /// <returns>UserDiscount</returns>
        /// <author>Paul Wroe</author>
        public UserDiscount GetCoupon(int userId, String discountCode, out bool isSuccess, out String error)
        {
            error = null;
            isSuccess = false;

            //  attempt to find list of coupons
            List<UserDiscount> couponList = getCouponListByUserIdAndCouponCode(userId, discountCode);

            //  check for exists, expired, or used, or not yet active
            //  and set success and error, then return resultant UserDiscount
            return ValidateCoupon(couponList, out isSuccess, out error);
        }


        public virtual List<UserDiscount> getCouponListByUserIdAndCouponCode(int userId, String discountCode)
        {
            List<UserDiscount> couponList = null;
            using (var ctx = dbContext)
            {
                //  get coupon by id and coupon code
                //  LINQ query instead of Lambda expression for clarity
                IQueryable<UserDiscount> coupon = from cup in ctx.UserDiscounts()
                                                  where
                                                      cup.DiscountCode == discountCode
                                                      &&
                                                      cup.UserId == userId
                                                  select cup;
                couponList = coupon.ToList();

            }
            return couponList;
        }


        /// <summary>
        /// Checks coupon to see if it is valid
        /// </summary>
        /// <param name="couponList"></param>
        /// <param name="isSuccess"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public UserDiscount ValidateCoupon
            (List<UserDiscount> couponList, out bool isSuccess, out String error)
        {
            error = null;
            isSuccess = false;
            UserDiscount returnCoupon = null;

            //  check for exists, expired, or used, or not yet active
            if (couponList.Count > 0)
            {
                returnCoupon = couponList.First(); //  fails if sequence contains no elements
                if (returnCoupon.Reedemed)  //  false is not redeemed
                {
                    error = "Coupon already redeemed.";
                }
                else if (DateTime.Compare(returnCoupon.EndDate, DateTime.Now) < 0)   // coupon is expired
                {
                    error = "Coupon is expired.";
                }
                else if (DateTime.Compare(returnCoupon.StartDate, DateTime.Now) > 0)  //  coupon is not yet active
                {
                    error = "Coupon is not yet active.  Coupon starts " + returnCoupon.StartDate;
                }
                else
                {
                    isSuccess = true;
                }
            }
            else
            {
                error = "Unable to find Coupon.";
            }

            return returnCoupon;
        }

        /// <summary>
        /// Mark Inputted coupon as redeemed.
        /// </summary>
        /// <returns>UserDisount</returns>
        /// <author>Paul Wroe</author>
        public virtual UserDiscount RedeemCoupon(UserDiscount coupon, out bool isSuccess)
        {
            isSuccess = false;
            
                // add coupon to context
                List<UserDiscount> userCoupon = GetCouponList(coupon);
                if (userCoupon.Count != 0)
                {
                    var theCoupon = userCoupon[0];
                    theCoupon.Reedemed = true;
                    dbContext.SaveChanges();
                    isSuccess = true;
                    
            }
            return coupon;
        }

        /// <summary>
        /// Returns a matching UserDiscount from the database
        /// </summary>
        /// <param name="coupon"></param>
        /// <returns></returns>
        public virtual List<UserDiscount> GetCouponList(UserDiscount coupon)
        {
            return dbContext.UserDiscounts().Where(p => p.UserDiscountId == coupon.UserDiscountId).ToList();
        }


        /// <summary>
        /// Create and submit coupon
        /// </summary>
        /// <param name="coupon">Coupon without code</param>
        /// <returns>Coupon with code</returns>
        public UserDiscount CreateCoupon(UserDiscount coupon)
        {
            // generate five digit coupon code
            if (coupon.StartDate < coupon.EndDate &&
                (coupon.DiscountRate > 0 && coupon.DiscountRate < 100))
            {
                coupon.DiscountCode = couponUtility.GenerateCoupon();
                using (var ctx = dbContext)
                {
                    // add coupon to context
                    var userCoupons = ctx.UserDiscounts().Add(coupon);
                    // save changes to context (saves to DB!)
                    ctx.SaveChanges();
                }
            }
            return coupon;
        }
        
        /// <summary>
        /// No-Args constructor, creates new CouponUtility();
        /// </summary>
        public DiscountService()
        {
            couponUtility = Factory.Instance.Create<UserDiscountUtility>();
            dbContext = PracticeGDVPDao.GetInstance();
        }

        public static IDiscountService GetInstance()
        {
            if (discountService == null)
            {
                discountService = new DiscountService();
            }
            return discountService;
        }


    }

}