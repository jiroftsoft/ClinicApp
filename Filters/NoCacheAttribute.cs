using System;
using System.Web;
using System.Web.Mvc;

namespace ClinicApp.Filters
{
    /// <summary>
    /// Prevents caching of sensitive pages (medical/admin environment)
    /// Security requirement: No caching for profile and sensitive pages
    /// </summary>
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var response = context.HttpContext.Response;
            
            // ✅ Remove all caching
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            
            // ✅ Additional headers
            response.Headers["Pragma"] = "no-cache";
            response.Headers["Expires"] = "0";
            
            base.OnResultExecuting(context);
        }
    }
}
