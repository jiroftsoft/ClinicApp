// ClinicApp/Interfaces/IAppSettings.cs
namespace ClinicApp.Interfaces
{
    /// <summary>
    /// رابط تنظیمات سیستم - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت تنظیمات سیستم در یک نقطه
    /// 2. پشتیبانی از محیط‌های مختلف (Development, Production)
    /// 3. امکان تغییر تنظیمات بدون تغییر کد
    /// 4. رعایت استانداردهای سیستم‌های پزشکی
    /// </summary>
    public interface IAppSettings
    {
        /// <summary>
        /// تعداد پیش‌فرض آیتم‌ها در هر صفحه
        /// </summary>
        int DefaultPageSize { get; }

        /// <summary>
        /// حداکثر تعداد تلاش‌های ورود
        /// </summary>
        int MaxLoginAttempts { get; }

        /// <summary>
        /// زمان محدودیت نرخ برای ورود (دقیقه)
        /// </summary>
        int RateLimitMinutes { get; }
    }
}