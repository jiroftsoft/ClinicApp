using System;

namespace ClinicApp.Infrastructure
{
    /// <summary>
    /// ✅ ENTERPRISE-GRADE: اینترفیس برای فراهم کردن زمان فعلی
    /// طبق Best Practices پروژه‌های بزرگ (دیجی‌کالا، خانومی، مکت‌خونه):
    /// - استفاده از UTC در سرور
    /// - تبدیل به timezone محلی فقط برای نمایش
    /// - تست‌پذیری بالا
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// زمان فعلی UTC (برای ذخیره در دیتابیس)
        /// </summary>
        DateTime UtcNow { get; }
        
        /// <summary>
        /// زمان فعلی Local (برای استفاده در Business Logic)
        /// </summary>
        DateTime Now { get; }
        
        /// <summary>
        /// ✅ ENTERPRISE: تاریخ امروز در timezone ایران (فقط تاریخ، بدون زمان)
        /// برای استفاده در Business Logic و Validation
        /// </summary>
        DateTime GetIranToday();
        
        /// <summary>
        /// ✅ ENTERPRISE: زمان فعلی در timezone ایران
        /// برای استفاده در Business Logic
        /// </summary>
        DateTime GetIranNow();
        
        /// <summary>
        /// ✅ ENTERPRISE: تاریخ امروز به شمسی
        /// برای استفاده در UI و نمایش
        /// </summary>
        string GetIranTodayPersian();
        
        /// <summary>
        /// تبدیل UTC به زمان محلی ایران
        /// </summary>
        DateTime ToIranTime(DateTime utcTime);
        
        /// <summary>
        /// تبدیل زمان محلی ایران به UTC
        /// </summary>
        DateTime FromIranTime(DateTime iranTime);
        
        /// <summary>
        /// فرمت زمان برای نمایش به کاربران ایرانی
        /// </summary>
        string FormatForIran(DateTime utcTime);
    }

    /// <summary>
    /// ✅ ENTERPRISE-GRADE: پیاده‌سازی پیش‌فرض ITimeProvider
    /// طبق Best Practices پروژه‌های بزرگ:
    /// - استفاده از TimeZoneInfo برای تبدیل timezone
    /// - پشتیبانی از Fallback در صورت عدم دسترسی به timezone
    /// </summary>
    public class DefaultTimeProvider : ITimeProvider
    {
        private static readonly TimeZoneInfo _iranTimeZone;
        
        static DefaultTimeProvider()
        {
            // ✅ ENTERPRISE: دریافت timezone ایران (یک بار در static constructor)
            try
            {
                _iranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // ✅ Fallback: ایجاد custom timezone
                _iranTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                    "Iran Standard Time",
                    TimeSpan.FromHours(3.5),
                    "Iran Standard Time",
                    "Iran Standard Time");
            }
        }
        
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime Now => DateTime.Now;
        
        /// <summary>
        /// ✅ ENTERPRISE: تاریخ امروز در timezone ایران (فقط تاریخ، بدون زمان)
        /// </summary>
        public DateTime GetIranToday()
        {
            var utcNow = DateTime.UtcNow;
            var iranNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _iranTimeZone);
            return iranNow.Date; // فقط تاریخ
        }
        
        /// <summary>
        /// ✅ ENTERPRISE: زمان فعلی در timezone ایران
        /// </summary>
        public DateTime GetIranNow()
        {
            var utcNow = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, _iranTimeZone);
        }
        
        /// <summary>
        /// ✅ ENTERPRISE: تاریخ امروز به شمسی
        /// </summary>
        public string GetIranTodayPersian()
        {
            var iranToday = GetIranToday();
            return ClinicApp.Helpers.PersianDateHelper.ToPersianDate(iranToday);
        }
        
        public DateTime ToIranTime(DateTime utcTime)
        {
            // ✅ ENTERPRISE: استفاده از TimeZoneInfo برای تبدیل دقیق
            if (utcTime.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, _iranTimeZone);
            }
            else if (utcTime.Kind == DateTimeKind.Unspecified)
            {
                // اگر Unspecified است، به عنوان UTC در نظر می‌گیریم
                var utcDateTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _iranTimeZone);
            }
            else
            {
                // اگر Local است، ابتدا به UTC تبدیل می‌کنیم
                var utcDateTime = utcTime.ToUniversalTime();
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _iranTimeZone);
            }
        }
        
        public DateTime FromIranTime(DateTime iranTime)
        {
            // ✅ ENTERPRISE: تبدیل زمان ایران به UTC
            // ConvertTimeToUtc(..., sourceTimeZone) فقط وقتی dateTime.Kind == Unspecified مجاز است؛ با Local/Utc استثنا می‌دهد.
            var toConvert = iranTime.Kind == DateTimeKind.Unspecified
                ? iranTime
                : DateTime.SpecifyKind(iranTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(toConvert, _iranTimeZone);
        }
        
        public string FormatForIran(DateTime utcTime)
        {
            var iranTime = ToIranTime(utcTime);
            return iranTime.ToString("yyyy/MM/dd HH:mm:ss");
        }
    }
}