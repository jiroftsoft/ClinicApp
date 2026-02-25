using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Serilog;

namespace ClinicApp.Filters
{
    /// <summary>
    /// محدودیت نرخ برای endpointهای اعتبارسنجی مدیریت کاربران (کد ملی / ایمیل)
    /// جلوگیری از سوءاستفاده و enumeration در محیط پروداکشن درمانی
    /// </summary>
    public class UserManagementValidationRateLimitAttribute : ActionFilterAttribute
    {
        private readonly int _maxRequests;
        private readonly int _timeWindowMinutes;
        private const string CacheKeyPrefix = "UserMgmt_Validation_";
        private static readonly ILogger Logger = Log.ForContext<UserManagementValidationRateLimitAttribute>();

        public UserManagementValidationRateLimitAttribute(int maxRequests = 30, int timeWindowMinutes = 1)
        {
            _maxRequests = maxRequests;
            _timeWindowMinutes = timeWindowMinutes;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var httpContext = filterContext.HttpContext;
                string clientId = httpContext.Request.UserHostAddress ?? "unknown";
                if (!string.IsNullOrEmpty(httpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]))
                    clientId = httpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split(',')[0].Trim();

                var cacheKey = CacheKeyPrefix + clientId;
                var cache = httpContext.Cache;

                var requestTimes = cache[cacheKey] as List<DateTime> ?? new List<DateTime>();
                var cutoff = DateTime.UtcNow.AddMinutes(-_timeWindowMinutes);
                requestTimes = requestTimes.Where(t => t > cutoff).ToList();

                if (requestTimes.Count >= _maxRequests)
                {
                    Logger.Warning("UserManagement validation rate limit exceeded - Client: {ClientId}, Count: {Count}", clientId, requestTimes.Count);
                    filterContext.Result = new JsonResult
                    {
                        Data = new { valid = false, message = "تعداد درخواست‌های اعتبارسنجی بیش از حد مجاز است. لطفاً یک دقیقه صبر کنید." }
                    };
                    filterContext.HttpContext.Response.StatusCode = 429;
                    return;
                }

                requestTimes.Add(DateTime.UtcNow);
                cache.Insert(cacheKey, requestTimes, null,
                    DateTime.UtcNow.AddMinutes(_timeWindowMinutes), Cache.NoSlidingExpiration);

                base.OnActionExecuting(filterContext);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "UserManagementValidationRateLimit error");
                base.OnActionExecuting(filterContext);
            }
        }
    }
}
