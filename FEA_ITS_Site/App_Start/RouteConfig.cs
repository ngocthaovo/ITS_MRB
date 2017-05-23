using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FEA_ITS_Site
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "User", action = "Login"}
            );
            routes.MapRoute(
                name: "Change language",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "ChangeCurrentCulture", id = UrlParameter.Optional }
            );
        }
    }
}