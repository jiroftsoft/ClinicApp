using System;
using System.Web.Mvc;
using ClinicApp.Helpers;

namespace ClinicApp.Controllers.Api
{
    /// <summary>
    /// API Controller برای مدیریت تاریخ شمسی
    /// طبق استانداردهای فرم‌های درمانی سطح سازمانی
    /// </summary>
    [RoutePrefix("api/persian-date")]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class PersianDateApiController : Controller
    {
        /// <summary>
        /// دریافت تاریخ امروز شمسی از سرور
        /// GET: /api/persian-date/today
        /// 
        /// این endpoint برای اطمینان از صحت تاریخ امروز در client-side استفاده می‌شود
        /// </summary>
        /// <returns>JSON response با تاریخ امروز شمسی</returns>
        [HttpGet]
        [Route("today")]
        [AllowAnonymous]
        /// <summary>
        /// ✅ ENTERPRISE-GRADE: دریافت تاریخ امروز شمسی از سرور
        /// طبق Best Practices پروژه‌های بزرگ (دیجی‌کالا، خانومی، مکت‌خونه):
        /// 1. استفاده از UTC در سرور
        /// 2. تبدیل به timezone ایران فقط برای نمایش
        /// 3. همیشه از سرور (نه client-side calculation)
        /// </summary>
        public JsonResult GetToday()
        {
            try
            {
                // ✅ Today را همیشه بر اساس TimeZone ایران محاسبه کن (مستقل از تنظیمات سرور)
                TimeZoneInfo iranTz;
                try
                {
                    iranTz = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
                }
                catch (TimeZoneNotFoundException)
                {
                    // ✅ Fallback: اگر timezone پیدا نشد، از offset استفاده می‌کنیم
                    iranTz = TimeZoneInfo.CreateCustomTimeZone(
                        "Iran Standard Time",
                        TimeSpan.FromHours(3.5),
                        "Iran Standard Time",
                        "Iran Standard Time");
                }
                
                var iranNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, iranTz);
                var iranMidnight = new DateTimeOffset(
                    iranNow.Year, iranNow.Month, iranNow.Day,
                    0, 0, 0,
                    iranNow.Offset
                );
                // ✅ CRITICAL FIX: تبدیل به UTC برای PersianDateHelper
                // PersianDateHelper برای UTC به درستی به Iran Time تبدیل می‌کند
                var todayUtc = iranMidnight.UtcDateTime;
                
                // ✅ تبدیل به تاریخ شمسی (PersianDateHelper UTC را به Iran Time تبدیل می‌کند)
                var persianToday = PersianDateHelper.ToPersianDate(todayUtc);
                
                // ✅ بررسی صحت تبدیل
                if (string.IsNullOrEmpty(persianToday) || persianToday == "0000/00/00")
                {
                    Serilog.Log.Warning("🔍 [GetToday] Invalid Persian date conversion: {PersianDate}", persianToday);
                    return Json(new
                    {
                        success = false,
                        message = "خطا در محاسبه تاریخ امروز"
                    }, JsonRequestBehavior.AllowGet);
                }

                // ✅ تبدیل به میلادی برای استفاده در DatePicker (از iranMidnight استفاده می‌کنیم)
                var gregorianToday = iranMidnight.DateTime.ToString("yyyy-MM-dd");
                
                // ✅ Logging برای Debug و Monitoring
                Serilog.Log.Information("🔍 [GetToday] UTC: {UtcNow}, Iran: {IranNow}, Persian: {PersianDate}, Gregorian: {GregorianDate}, Timezone: {Timezone}", 
                    DateTimeOffset.UtcNow, iranNow, persianToday, gregorianToday, iranTz.Id);

                return Json(new
                {
                    success = true,
                    persianDate = persianToday,
                    gregorianDate = gregorianToday,
                    // ✅ Unix time دقیق (بر مبنای نیمه‌شب ایران)
                    timestamp = iranMidnight.ToUnixTimeSeconds(),
                    timezone = iranTz.Id,
                    utcDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
                    iranDate = iranMidnight.DateTime.ToString("yyyy-MM-dd"),
                    // ✅ اضافه کردن اطلاعات برای Debug
                    debug = new
                    {
                        utcNow = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        iranNow = iranNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        iranToday = iranMidnight.DateTime.ToString("yyyy-MM-dd")
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // ✅ Logging خطا
                Serilog.Log.Error(ex, "❌ [GetToday] خطا در دریافت تاریخ امروز شمسی");
                
                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت تاریخ امروز"
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

