using System;
using System.Web;
using System.Web.Mvc;
using Serilog.Context;

namespace ClinicApp.Filters
{
    public class CorrelationIdFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var corr = request.Headers["X-Correlation-Id"] ?? Guid.NewGuid().ToString();
            filterContext.HttpContext.Items["CorrelationId"] = corr;
            filterContext.HttpContext.Response.Headers["X-Correlation-Id"] = corr;

            var user = filterContext.HttpContext.User?.Identity?.Name ?? "anonymous";
            LogContext.PushProperty("CorrelationId", corr);
            LogContext.PushProperty("UserName", user);

            base.OnActionExecuting(filterContext);
        }
    }
}
