using System;
using System.Web;
using System.Web.Mvc;

namespace ClinicApp.Filters
{
    public sealed class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext ctx)
        {
            var r = ctx.HttpContext.Response;
            r.Cache.SetCacheability(HttpCacheability.NoCache);
            r.Cache.SetNoStore();
            r.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            r.Cache.SetMaxAge(TimeSpan.Zero);
            r.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            r.Headers["Pragma"] = "no-cache";
            base.OnResultExecuting(ctx);
        }
    }
}
