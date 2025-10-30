using System;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Helpers;

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

            // فقط روی POST/PUT/DELETE اعمال شود
            if (!(string.Equals(req.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(req.HttpMethod, "PUT", StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(req.HttpMethod, "DELETE", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            try
            {
                // تلاش برای خواندن توکن از Header
                var headerToken = req.Headers[HeaderName1] ?? req.Headers[HeaderName2];

                if (!string.IsNullOrWhiteSpace(headerToken))
                {
                    // کوکی توکن
                    var cookieName = AntiForgeryConfig.CookieName ?? CookieFallbackName;
                    var cookieToken = req.Cookies[cookieName]?.Value ?? req.Cookies[CookieFallbackName]?.Value;

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

                    AntiForgery.Validate(cookieToken, formToken);
                }
                else
                {
                    // حالت فرم معمولی (توکن Hidden)
                    AntiForgery.Validate();
                }
            }
            catch (HttpAntiForgeryException)
            {
                // پاسخ JSON برای Ajax/JSON
                if (req.IsAjaxRequest() || (req.ContentType != null && req.ContentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = ServiceResult.Failed(
                            "توکن امنیتی نامعتبر یا موجود نیست.",
                            code: "ANTIFORGERY_INVALID",
                            category: ErrorCategory.Security,
                            securityLevel: SecurityLevel.High
                        ),
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    filterContext.HttpContext.Response.StatusCode = 400;
                    return;
                }

                // برای فرم‌ها اجازه بده Exception بالا برود
                throw;
            }
        }
    }
}
