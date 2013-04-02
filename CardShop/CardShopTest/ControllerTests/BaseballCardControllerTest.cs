﻿using System;
using Moq;
using CardShop.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using CardShop;
using CardShop.Models;
using CardShop.Daos;
using System.Data.Entity;

namespace CardShopTest.ControllerTests
{
    [TestClass]
    public class BaseballCardControllerTest
    {

        [TestMethod]
        public void TestIndexHasListOfBaseballCards()
        {
            var mockFishRepository = new Mock<IBaseballCardRepository>();

            List<BaseballCard> mockCards = new List<BaseballCard>();
            mockCards.Add(new BaseballCard() { 
                BaseballCardId = 0, 
                Cost = new Decimal(2.50), 
                Player = "Cole Hammels", 
                Team = "Phillies" });

            mockCards.Add(new BaseballCard() { 
                BaseballCardId = 0, 
                Cost = new Decimal(2.50), 
                Player = "Cole Hammels", 
                Team = "Phillies" });

            mockCards.Add(new BaseballCard() { 
                BaseballCardId = 0, 
                Cost = new Decimal(2.50), 
                Player = "Cole Hammels", 
                Team = "Phillies" });

            // Setup mock behavior for the GetAllFish() method in our repository
            mockFishRepository.Setup(x => x.GetAllCards()).Returns(mockCards);

            // ISSUE: We now need to pass the repository into the controller like this:
            var controller = new BaseballCardController(mockFishRepository.Object);

            var result = controller.Index() as ViewResult;
            var fish = (List<BaseballCard>)result.ViewData.Model;


            // Test the view model contains a list of fish
            Assert.IsNotNull(fish, "The list of cards does not exist");
            Assert.IsTrue(fish.Count == mockCards.Count);
        }

        [TestMethod]
        public void TestDetailsViewNameReturn()
        {

        }
    }
}
