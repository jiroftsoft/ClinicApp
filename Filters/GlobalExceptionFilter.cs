using System.Net;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled) return;

            var ex = filterContext.Exception;
            Log.Error(ex, "Unhandled exception");

            var result = ServiceResult.Failed("خطای غیرمنتظره رخ داد.", "UNHANDLED");
            filterContext.Result = new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            filterContext.ExceptionHandled = true;
        }
    }
}
