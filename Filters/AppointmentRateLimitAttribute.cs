using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Caching;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace ClinicApp.Filters
{
    /// <summary>
    /// Attribute برای Rate Limiting رزرو نوبت
    /// جلوگیری از رزرو بیش از حد در بازه زمانی مشخص
    /// </summary>
    public class AppointmentRateLimitAttribute : ActionFilterAttribute
    {
        private readonly int _maxRequests;
        private readonly int _timeWindowMinutes;
        private readonly string _cacheKeyPrefix = "AppointmentRateLimit_";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxRequests">حداکثر تعداد درخواست</param>
        /// <param name="timeWindowMinutes">بازه زمانی به دقیقه</param>
        public AppointmentRateLimitAttribute(int maxRequests = 5, int timeWindowMinutes = 60)
        {
            _maxRequests = maxRequests;
            _timeWindowMinutes = timeWindowMinutes;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var httpContext = filterContext.HttpContext;
                var userId = httpContext.User?.Identity?.Name ?? httpContext.Request.UserHostAddress;
                var cacheKey = $"{_cacheKeyPrefix}{userId}";

                // دریافت لیست درخواست‌های قبلی
                var requestTimes = httpContext.Cache[cacheKey] as List<DateTime> ?? new List<DateTime>();

                // حذف درخواست‌های قدیمی (خارج از بازه زمانی)
                var cutoffTime = DateTime.Now.AddMinutes(-_timeWindowMinutes);
                requestTimes = requestTimes.Where(rt => rt > cutoffTime).ToList();

                // بررسی تعداد درخواست‌ها
                if (requestTimes.Count >= _maxRequests)
                {
                    var logger = Log.ForContext<AppointmentRateLimitAttribute>();
                    logger.Warning("Rate Limit exceeded - User: {UserId}, Requests: {Count}, Max: {Max}",
                        userId, requestTimes.Count, _maxRequests);

                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            success = false,
                            message = $"شما بیش از حد مجاز درخواست رزرو ارسال کرده‌اید. لطفاً {_timeWindowMinutes} دقیقه دیگر تلاش کنید."
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    return;
                }

                // اضافه کردن درخواست فعلی
                requestTimes.Add(DateTime.Now);

                // ذخیره در Cache با انقضای خودکار
                httpContext.Cache.Insert(
                    cacheKey,
                    requestTimes,
                    null,
                    DateTime.Now.AddMinutes(_timeWindowMinutes),
                    Cache.NoSlidingExpiration);

                base.OnActionExecuting(filterContext);
            }
            catch (Exception ex)
            {
                var logger = Log.ForContext<AppointmentRateLimitAttribute>();
                logger.Error(ex, "خطا در Rate Limiting");
                // در صورت خطا، اجازه می‌دهیم درخواست ادامه یابد
                base.OnActionExecuting(filterContext);
            }
        }
    }
}

