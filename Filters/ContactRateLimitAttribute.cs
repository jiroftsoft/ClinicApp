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
    /// Attribute برای Rate Limiting فرم تماس
    /// جلوگیری از ارسال بیش از حد درخواست در بازه زمانی مشخص
    /// Production-Grade Anti-Spam Protection
    /// </summary>
    public class ContactRateLimitAttribute : ActionFilterAttribute
    {
        private readonly int _maxRequests;
        private readonly int _timeWindowMinutes;
        private readonly string _cacheKeyPrefix = "ContactRateLimit_";
        private static readonly ILogger _logger = Log.ForContext<ContactRateLimitAttribute>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxRequests">حداکثر تعداد درخواست (پیش‌فرض: 3)</param>
        /// <param name="timeWindowMinutes">بازه زمانی به دقیقه (پیش‌فرض: 15)</param>
        public ContactRateLimitAttribute(int maxRequests = 3, int timeWindowMinutes = 15)
        {
            _maxRequests = maxRequests;
            _timeWindowMinutes = timeWindowMinutes;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var httpContext = filterContext.HttpContext;
                
                // استفاده از IP Address برای شناسایی
                string ipAddress = httpContext.Request.UserHostAddress;
                if (httpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                {
                    ipAddress = httpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split(',')[0].Trim();
                }

                var cacheKey = $"{_cacheKeyPrefix}{ipAddress}";

                // دریافت لیست درخواست‌های قبلی
                var requestTimes = httpContext.Cache[cacheKey] as List<DateTime> ?? new List<DateTime>();

                // حذف درخواست‌های قدیمی (خارج از بازه زمانی)
                var cutoffTime = DateTime.Now.AddMinutes(-_timeWindowMinutes);
                requestTimes = requestTimes.Where(rt => rt > cutoffTime).ToList();

                // بررسی تعداد درخواست‌ها
                if (requestTimes.Count >= _maxRequests)
                {
                    _logger.Warning("Contact Form Rate Limit exceeded - IP: {IpAddress}, Requests: {Count}, Max: {Max}, TimeWindow: {TimeWindow} minutes",
                        ipAddress, requestTimes.Count, _maxRequests, _timeWindowMinutes);

                    filterContext.Result = new ViewResult
                    {
                        ViewName = "Index",
                        ViewData = new ViewDataDictionary
                        {
                            { "ErrorMessage", $"شما بیش از حد مجاز درخواست ارسال کرده‌اید. لطفاً {_timeWindowMinutes} دقیقه دیگر تلاش کنید." }
                        }
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
                _logger.Error(ex, "خطا در Contact Rate Limiting");
                // در صورت خطا، اجازه می‌دهیم درخواست ادامه یابد
                base.OnActionExecuting(filterContext);
            }
        }
    }
}
