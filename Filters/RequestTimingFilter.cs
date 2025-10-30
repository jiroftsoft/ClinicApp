using System.Diagnostics;
using System.Web.Mvc;
using Serilog;

namespace ClinicApp.Filters
{
    public class RequestTimingFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var sw = new Stopwatch();
            filterContext.HttpContext.Items["__sw"] = sw;
            sw.Start();
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var sw = filterContext.HttpContext.Items["__sw"] as Stopwatch;
            if (sw != null)
            {
                sw.Stop();
                var route = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName + "/" + filterContext.ActionDescriptor.ActionName;
                Log.Information("RequestTiming {Route} took {Elapsed} ms", route, sw.ElapsedMilliseconds);
            }
            base.OnActionExecuted(filterContext);
        }
    }
}


