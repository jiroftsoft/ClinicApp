using System.Net;
using System.Web.Mvc;
using System.Configuration;
using ClinicApp.Helpers;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Filters
{
    /// <summary>
    /// ✅ گام 8 - فیلتر جهانی خطا برای API (JSON واحد + فارسی)
    /// هر Exception بی‌صاحب در اکشن‌های JSON به پاسخ استاندارد با پیام فارسی تبدیل می‌شود
    /// در Dev شامل Exception/StackTrace نیز خواهد بود
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null || filterContext.ExceptionHandled) return;

            var ex = filterContext.Exception;
            
            // ✅ گام 8: لاگ با جزئیات کامل
            Log.Error(ex, "Unhandled API exception at {Path}", filterContext.HttpContext?.Request?.RawUrl);

            // ✅ گام 8: استفاده از WithExceptionDev برای افزودن جزئیات در Dev
            var result = ServiceResult.Failed("خطای غیرمنتظره رخ داد.", code: "UNHANDLED")
                                      .WithExceptionDev(ex);

            // ✅ گام 8: فقط برای درخواست‌های AJAX/JSON پاسخ JSON برگردان
            var req = filterContext.HttpContext?.Request;
            bool isAjax = req?.IsAjaxRequest() == true || 
                         (req?.ContentType != null && req.ContentType.IndexOf("application/json", System.StringComparison.OrdinalIgnoreCase) >= 0);

            if (isAjax)
            {
                filterContext.Result = new JsonResult 
                { 
                    Data = result, 
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet 
                };
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
            else
            {
                // برای درخواست‌های غیر AJAX، از HandleErrorAttribute استفاده می‌شود
                // اینجا Exception را handle نمی‌کنیم تا HandleErrorAttribute آن را بگیرد
                return;
            }

            filterContext.ExceptionHandled = true;
        }
    }
}
