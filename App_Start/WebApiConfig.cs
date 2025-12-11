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
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}