using System;
using System.Linq;
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
    /// اعتبارسنجی Anti-Forgery برای درخواست‌های POST/PUT/DELETE:
    /// - اگر JSON/Ajax و توکن در Header باشد → cookie+header را Validate می‌کند
    /// - در غیر اینصورت (فرم معمولی) → AntiForgery.Validate() استاندارد
    /// - در خطای Ajax، پاسخ JSON با وضعیت 400 برمی‌گرداند
    /// </summary>
    public class ValidateAntiForgeryTokenOnPostsAttribute : AuthorizeAttribute
    {
        private const string HeaderName1 = "RequestVerificationToken";
        private const string HeaderName2 = "X-RequestVerificationToken";
        private const string CookieFallbackName = "__RequestVerificationToken";

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var req = filterContext.HttpContext.Request;
            var path = req?.RawUrl ?? req?.Path ?? "unknown";
            
            // Log entry point
            Serilog.Log.Information("🔒 AntiForgery: OnAuthorization شروع شد - Path: {Path}, Method: {Method}, Controller: {Controller}, Action: {Action}",
                path, req?.HttpMethod, 
                filterContext.ActionDescriptor?.ControllerDescriptor?.ControllerName,
                filterContext.ActionDescriptor?.ActionName);

            // فقط روی POST/PUT/DELETE اعمال شود
            if (!(string.Equals(req.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(req.HttpMethod, "PUT", StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(req.HttpMethod, "DELETE", StringComparison.OrdinalIgnoreCase)))
            {
                Serilog.Log.Debug("🔒 AntiForgery: Method {Method} نیاز به validation ندارد", req?.HttpMethod);
                return;
            }
            
            Serilog.Log.Information("🔒 AntiForgery: در حال بررسی Token برای {Method} - Path: {Path}", req?.HttpMethod, path);

            try
            {
                // تلاش برای خواندن توکن از Header
                var headerToken = req.Headers[HeaderName1] ?? req.Headers[HeaderName2];

                if (!string.IsNullOrWhiteSpace(headerToken))
                {
                    // کوکی توکن - نام پیش‌فرض Anti-Forgery در MVC
                    // در MVC، cookie name معمولاً "__RequestVerificationToken" است اما ممکن است با prefix باشد
                    var cookieName = AntiForgeryConfig.CookieName;
                    if (string.IsNullOrWhiteSpace(cookieName))
                    {
                        // اگر CookieName تنظیم نشده، از نام پیش‌فرض استفاده کن
                        cookieName = "__RequestVerificationToken";
                    }
                    
                    var cookieToken = req.Cookies[cookieName]?.Value;
                    
                    // اگر cookie token پیدا نشد، از نام fallback استفاده کن
                    if (string.IsNullOrWhiteSpace(cookieToken))
                    {
                        cookieToken = req.Cookies[CookieFallbackName]?.Value;
                    }
                    
                    // اگر هنوز cookie token پیدا نشد، از تمام cookies با نام‌های مشابه استفاده کن
                    if (string.IsNullOrWhiteSpace(cookieToken))
                    {
                        foreach (string cookieKey in req.Cookies.AllKeys)
                        {
                            if (cookieKey != null && (cookieKey.Contains("RequestVerificationToken") || cookieKey.Contains("__RequestVerificationToken")))
                            {
                                cookieToken = req.Cookies[cookieKey]?.Value;
                                if (!string.IsNullOrWhiteSpace(cookieToken))
                                {
                                    Serilog.Log.Debug("Found anti-forgery cookie token in cookie: {CookieName}", cookieKey);
                                    break;
                                }
                            }
                        }
                    }

                    // پشتیبانی از الگوی "cookie:form" در یک هدر
                    string formToken = headerToken;
                    if (headerToken.Contains(":"))
                    {
                        var parts = headerToken.Split(':');
                        if (parts.Length == 2)
                        {
                            cookieToken = parts[0].Trim();
                            formToken = parts[1].Trim();
                        }
                    }

                    if (string.IsNullOrWhiteSpace(cookieToken))
                    {
                        Serilog.Log.Warning("Anti-forgery cookie token not found. CookieName: {CookieName}, Headers: {Headers}, Cookies: {Cookies}", 
                            cookieName,
                            string.Join(", ", req.Headers.AllKeys), 
                            string.Join(", ", req.Cookies.AllKeys));
                        throw new HttpAntiForgeryException("Anti-forgery cookie token is missing");
                    }

                    Serilog.Log.Debug("Validating anti-forgery token. Cookie token present: {HasCookie}, Form token present: {HasForm}", 
                        !string.IsNullOrWhiteSpace(cookieToken), 
                        !string.IsNullOrWhiteSpace(formToken));

                    AntiForgery.Validate(cookieToken, formToken);
                }
                else
                {
                    // حالت فرم معمولی (توکن Hidden)
                    AntiForgery.Validate();
                }
            }
            catch (HttpAntiForgeryException ex)
            {
                // ✅ گام 8: استفاده از Serilog برای لاگ
                Serilog.Log.Error(ex, "❌ AntiForgery: Token validation failed - Path: {Path}, Method: {Method}, Controller: {Controller}, Action: {Action}",
                    req?.RawUrl, req?.HttpMethod,
                    filterContext.ActionDescriptor?.ControllerDescriptor?.ControllerName,
                    filterContext.ActionDescriptor?.ActionName);

                bool isAjax = req.IsAjaxRequest() || (req.ContentType != null && req.ContentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0);

                if (isAjax)
                {
                    // ✅ گام 8: JSON استاندارد با کد ANTIFORGERY_MISSING + 400 (بدون 500)
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
                    filterContext.HttpContext.Response.StatusCode = 400;
                    filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                    return;
                }
                else
                {
                    // مسیر غیر AJAX: با پیام کاربرپسند برگردان
                    filterContext.Controller.TempData["ErrorMessage"] = "توکن امنیتی منقضی است. لطفاً صفحه را نوسازی کنید.";
                    filterContext.Result = new RedirectResult(req.UrlReferrer != null ? req.UrlReferrer.ToString() : "/");
                    return;
                }
            }
        }
    }
}
