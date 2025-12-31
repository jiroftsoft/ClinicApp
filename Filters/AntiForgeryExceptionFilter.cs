using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Filters
{
    /// <summary>
    /// Exception Filter برای handle کردن HttpAntiForgeryException از [ValidateAntiForgeryToken] attribute
    /// این filter قبل از GlobalExceptionFilter اجرا می‌شود و exception را handle می‌کند
    /// </summary>
    public class AntiForgeryExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null || filterContext.ExceptionHandled) return;

            // فقط HttpAntiForgeryException را handle کن
            if (!(filterContext.Exception is HttpAntiForgeryException)) return;

            var ex = filterContext.Exception as HttpAntiForgeryException;
            var req = filterContext.HttpContext?.Request;
            
            // ✅ لاگ خطا
            var controllerName = filterContext.Controller?.GetType()?.Name?.Replace("Controller", "") ?? "Unknown";
            var actionName = filterContext.RouteData?.Values["action"]?.ToString() ?? "Unknown";
            Serilog.Log.Error(ex, "❌ AntiForgery Exception (from [ValidateAntiForgeryToken]): Path: {Path}, Method: {Method}, Controller: {Controller}, Action: {Action}",
                req?.RawUrl, req?.HttpMethod, controllerName, actionName);

            // ✅ فقط برای درخواست‌های AJAX/JSON پاسخ JSON برگردان
            bool isAjax = req?.IsAjaxRequest() == true || 
                         (req?.ContentType != null && req.ContentType.IndexOf("application/json", System.StringComparison.OrdinalIgnoreCase) >= 0);

            // ✅ CRITICAL FIX: Check if headers have been sent before setting Result
            // If headers are already sent, we cannot set JsonResult (which requires ContentType)
            if (filterContext.HttpContext.Response.HeadersWritten)
            {
                // Headers already sent - cannot set Result - just mark as handled
                Serilog.Log.Warning("⚠️ AntiForgery Exception: Headers already sent, cannot set Result. Exception marked as handled.");
                filterContext.ExceptionHandled = true;
                return;
            }

            if (isAjax)
            {
                try
                {
                    // ✅ CRITICAL FIX: Double-check HeadersWritten right before setting Result
                    // This prevents race conditions where headers might be sent between checks
                    if (filterContext.HttpContext.Response.HeadersWritten)
                    {
                        Serilog.Log.Warning("⚠️ AntiForgery Exception: Headers sent between checks, cannot set Result. Exception marked as handled.");
                        filterContext.ExceptionHandled = true;
                        return;
                    }

                    filterContext.Result = new JsonResult
                    {
                        Data = ServiceResult.Failed(
                            "توکن امنیتی منقضی یا نامعتبر است. صفحه را نوسازی کنید.",
                            code: "ANTIFORGERY_MISSING",
                            category: ErrorCategory.Security,
                            securityLevel: SecurityLevel.High
                        ).WithExceptionDev(ex),
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    
                    // ✅ CRITICAL FIX: Check if headers have been sent before setting status code
                    if (!filterContext.HttpContext.Response.HeadersWritten)
                    {
                        filterContext.HttpContext.Response.StatusCode = 400;
                        filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                    }
                }
                catch (HttpException httpEx)
                {
                    // Headers already sent - cannot set Result or status code
                    Serilog.Log.Warning(httpEx, "⚠️ AntiForgery Exception: Cannot set Result/Status - headers already sent. Exception marked as handled.");
                    filterContext.ExceptionHandled = true;
                    return;
                }
            }
            else
            {
                // برای درخواست‌های غیر AJAX، redirect با پیام خطا
                try
                {
                    filterContext.Controller.TempData["ErrorMessage"] = "توکن امنیتی منقضی است. لطفاً صفحه را نوسازی کنید.";
                    filterContext.Result = new RedirectResult(req?.UrlReferrer != null ? req.UrlReferrer.ToString() : "/");
                }
                catch (HttpException httpEx)
                {
                    // Headers already sent - cannot set Result
                    Serilog.Log.Warning(httpEx, "⚠️ AntiForgery Exception: Cannot set Redirect Result - headers already sent. Exception marked as handled.");
                    filterContext.ExceptionHandled = true;
                    return;
                }
            }

            filterContext.ExceptionHandled = true;
        }
    }
}

