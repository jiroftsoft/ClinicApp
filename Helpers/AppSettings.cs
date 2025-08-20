// ClinicApp/Helpers/AppSettings.cs
using System.Configuration;
using ClinicApp.Interfaces;
using ClinicApp.Helpers;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// پیاده‌سازی تنظیمات سیستم با استفاده از Web.config - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. خواندن تنظیمات از Web.config
    /// 2. استفاده از مقادیر پیش‌فرض در صورت عدم وجود تنظیمات
    /// 3. رعایت استانداردهای سیستم‌های پزشکی
    /// </summary>
    public class AppSettings : IAppSettings
    {
        public int DefaultPageSize { get; }
        public int MaxLoginAttempts { get; }
        public int RateLimitMinutes { get; }

        public AppSettings()
        {
            // خواندن تنظیمات از Web.config
            int defaultPageSize;
            int maxLoginAttempts;
            int rateLimitMinutes;

            // خواندن DefaultPageSize
            if (int.TryParse(ConfigurationManager.AppSettings["AppSettings:DefaultPageSize"], out defaultPageSize))
            {
                DefaultPageSize = defaultPageSize;
            }
            else
            {
                DefaultPageSize = SystemConstants.DefaultPageSize;
            }

            // خواندن MaxLoginAttempts
            if (int.TryParse(ConfigurationManager.AppSettings["AppSettings:MaxLoginAttempts"], out maxLoginAttempts))
            {
                MaxLoginAttempts = maxLoginAttempts;
            }
            else
            {
                MaxLoginAttempts = SystemConstants.MaxLoginAttempts;
            }

            // خواندن RateLimitMinutes
            if (int.TryParse(ConfigurationManager.AppSettings["AppSettings:RateLimitMinutes"], out rateLimitMinutes))
            {
                RateLimitMinutes = rateLimitMinutes;
            }
            else
            {
                RateLimitMinutes = SystemConstants.RateLimitMinutes;
            }
        }
    }
}