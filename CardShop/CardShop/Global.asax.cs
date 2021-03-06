﻿using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CardShop.Utilities;
using CardShop.Auth;

namespace CardShop
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            // Use our custom controller factory to create the appropriate controller
            //ControllerBuilder.Current.SetControllerFactory(new ControllerFactory());

            UnityConfiguration.ConfigureIoCContainer();
        }
        void Session_Start(object sender, EventArgs e)
        {
            UserAuth.CreateSession();
        }
    }
}