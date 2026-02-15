using System;
using System.Web;
using Hangfire.Dashboard;
using ClinicApp.Models.Core;

namespace ClinicApp.Infrastructure.Hangfire
{
    /// <summary>
    /// محدودیت دسترسی به داشبورد Hangfire — فقط نقش ادمین یا در محیط Development بدون لاگین.
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly bool _allowAnonymousInDevelopment;

        public HangfireAuthorizationFilter(bool allowAnonymousInDevelopment = false)
        {
            _allowAnonymousInDevelopment = allowAnonymousInDevelopment;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
                return false;

            // در محیط Development می‌توان دسترسی بدون لاگین را مجاز کرد (فقط روی localhost توصیه می‌شود)
            if (_allowAnonymousInDevelopment && IsLocalRequest(httpContext))
                return true;

            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            // فقط کاربران با نقش ادمین
            return httpContext.User.IsInRole(AppRoles.Admin);
        }

        private static bool IsLocalRequest(HttpContext httpContext)
        {
            try
            {
                var local = httpContext.Request.IsLocal;
                return local;
            }
            catch
            {
                return false;
            }
        }
    }
}
