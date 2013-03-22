﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CardShop.Service;
using CardShop.Models;
using CardShopTest.TestHelper;
using Moq;
using CardShop.Daos;
using System.Collections.Generic;

namespace CardShopTest.ServiceTests
{
    [TestClass]
    public class AdminServiceTests
    {
        AdminService adminService;
        Store storeOne;
        Store storeTwo;
        List<Store> stores;

        [TestInitialize]
        public void Setup()
        {
            adminService = new AdminService();
            stores = StoreTest.CreateStores(2);
            storeOne = stores[0];
            storeTwo = stores[1];
        }

        [TestMethod]
        public void EditStoreGoodTests()
        {

            var mockDbContext = new Mock<IPracticeGDVPDao>();

            adminService.context = mockDbContext.Object;

            mockDbContext.Setup(mock => mock.SaveChanges()).Returns(1);

            Assert.IsTrue(adminService.EditStore(storeOne,
                storeTwo).DiscountRate ==
                storeTwo.DiscountRate);

            mockDbContext.Verify(mock => mock.SaveChanges());
        }

        [TestMethod]
        public void EditStoreBadTests()
        {
            storeTwo.DiscountRate = 102;
            Assert.IsFalse(adminService.EditStore(storeOne, storeTwo).
                DiscountRate == storeTwo.DiscountRate);
        }

        [TestMethod]
        public void StoreOwnedGoodTest()
        {

            // Work in progress.
        //    var mockDbContext = new Mock<IPracticeGDVPDao>();
          //  adminService.context = mockDbContext.Object;

            //mockDbContext.Setup(mock => mock.Stores().
            // Where(s => s.UserId == 1).ToList()).Returns(stores);

          //  Assert.IsTrue(adminService.OwnedStore(1).StoreId != 0);
        }
    }
}