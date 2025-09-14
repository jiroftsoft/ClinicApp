using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ClinicApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            // Enable Attribute Routing
            routes.MapMvcAttributeRoutes();

            // 🔒 Medical Environment Routes - با اطمینان 100%
            routes.MapRoute(
                name: "ServiceIndex",
                url: "Admin/Service",
                defaults: new { controller = "Service", action = "Index", area = "Admin" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute(
                name: "ServiceIndexWithCategory",
                url: "Admin/Service/{serviceCategoryId}",
                defaults: new { controller = "Service", action = "Index", area = "Admin" },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET"),
                    serviceCategoryId = @"^\d+$" // فقط اعداد
                }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
