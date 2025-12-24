using System.Web.Http;

namespace ClinicApp
{
    /// <summary>
    /// Configuration class for ASP.NET Web API
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Register Web API routes and configuration
        /// </summary>
        /// <param name="config">HTTP Configuration</param>
        public static void Register(HttpConfiguration config)
        {
            // Enable Attribute Routing
            config.MapHttpAttributeRoutes();

            // Convention-based Routing
            // ⚠️ مهم: این route فقط برای controller هایی که نامشان بدون hyphen است کار می‌کند
            // Controller های با hyphen (مثل persian-date) باید توسط MVC routing پردازش شوند
            // Constraint: فقط controller names که با حرف شروع می‌شوند و hyphen ندارند
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: new { 
                    controller = @"^[a-zA-Z][a-zA-Z0-9]*$" // فقط controller names بدون hyphen
                }
            );
        }
    }
}