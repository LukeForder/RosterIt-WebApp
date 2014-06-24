using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using System.Web.Optimization;
using RosterIt.Web.App_Start;
using SimpleInjector;
using RosterIt.Web.Models;
using System.Threading;

namespace RosterIt.Web
{
    public class Global : HttpApplication
    {
        private static readonly Container _container = new Container();

        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            SimpleInjectorConfig.RegisterTypes(_container);
            MongoMapConfig.Configure();
        }

        void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            var principal =  new MongoPrincipal(User.Identity, _container.GetInstance<MongoRoleProvider>());

            HttpContext.Current.User = principal;
            Thread.CurrentPrincipal = principal;
        }
    }
}