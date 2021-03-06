﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CardShop
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Admin_ManageInventory",
                url: "Admin/ManageInventory/{action}/{id}",
                defaults: new { controller = "ManageInventory", action = "Index", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: "Admin_ManageUser",
                url: "Admin/ManageUser/{action}/{id}",
                defaults: new { controller = "ManageUser", action = "Index", id = UrlParameter.Optional }
                );
            routes.MapRoute(
                name: "Admin_ManageStore",
                url: "Admin/ManageStore/{action}/{id}",
                defaults: new { controller = "ManageStore", action = "Index", id = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}