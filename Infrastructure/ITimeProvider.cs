using System;

namespace ClinicApp.Infrastructure
{
    /// <summary>
    /// اینترفیس برای فراهم کردن زمان فعلی، جهت تست‌پذیری و یکپارچگی در سیستم
    /// </summary>
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
        DateTime Now { get; }
        
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
    /// پیاده‌سازی پیش‌فرض ITimeProvider که از DateTime.UtcNow و DateTime.Now استفاده می‌کند.
    /// </summary>
    public class DefaultTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime Now => DateTime.Now;
        
        public DateTime ToIranTime(DateTime utcTime)
        {
            // ایران UTC+3:30 (بدون تغییرات ساعت تابستانی)
            return utcTime.AddHours(3.5);
        }
        
        public DateTime FromIranTime(DateTime iranTime)
        {
            // تبدیل زمان ایران به UTC
            return iranTime.AddHours(-3.5);
        }
        
        public string FormatForIran(DateTime utcTime)
        {
            var iranTime = ToIranTime(utcTime);
            return iranTime.ToString("yyyy/MM/dd HH:mm:ss");
        }
    }
}