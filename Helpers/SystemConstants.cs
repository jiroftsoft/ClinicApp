// ClinicApp/Helpers/SystemConstants.cs
using System;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس ثابت حاوی مقادیر سیستمی - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. تعریف ثوابت سیستمی برای کاهش خطا در کد
    /// 2. رعایت استانداردهای سیستم‌های پزشکی در نام‌گذاری
    /// 3. افزایش قابلیت نگهداری کد
    /// 4. پشتیبانی از محیط‌های ایرانی
    /// </summary>
    public static class SystemConstants
    {
        /// <summary>
        /// نام بیمه آزاد که به عنوان پیش‌فرض برای بیماران بدون بیمه استفاده می‌شود
        /// </summary>
        public const string FreeInsuranceName = "بیمه آزاد";

        /// <summary>
        /// نقش پیش‌فرض برای بیماران
        /// </summary>
        public const string PatientRole = "Patient";

        /// <summary>
        /// تعداد پیش‌فرض آیتم‌ها در هر صفحه
        /// </summary>
        public const int DefaultPageSize = 15;

        /// <summary>
        /// حداکثر تعداد تلاش‌های ورود
        /// </summary>
        public const int MaxLoginAttempts = 5;

        /// <summary>
        /// زمان محدودیت نرخ برای ورود (دقیقه)
        /// </summary>
        public const int RateLimitMinutes = 5;
    }
}