using System.ComponentModel;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// اولویت بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. تعریف اولویت‌های استاندارد بیمه
    /// 2. پشتیبانی از بیمه اصلی و تکمیلی
    /// 3. قابلیت توسعه برای اولویت‌های بیشتر
    /// 4. رعایت استانداردهای پزشکی ایران
    /// 
    /// نکته حیاتی: این enum برای مدیریت اولویت بیمه‌ها در سیستم استفاده می‌شود
    /// </summary>
    public enum InsurancePriority
    {
        /// <summary>
        /// بیمه اصلی - بالاترین اولویت
        /// این بیمه همیشه اولویت 1 دارد و در محاسبات اولویت دارد
        /// </summary>
        [Description("بیمه اصلی")]
        Primary = 1,

        /// <summary>
        /// بیمه تکمیلی اول - دومین اولویت
        /// اولین بیمه تکمیلی که بعد از بیمه اصلی استفاده می‌شود
        /// </summary>
        [Description("بیمه تکمیلی اول")]
        SupplementaryFirst = 2,

        /// <summary>
        /// بیمه تکمیلی دوم - سومین اولویت
        /// دومین بیمه تکمیلی که در صورت عدم پوشش کامل استفاده می‌شود
        /// </summary>
        [Description("بیمه تکمیلی دوم")]
        SupplementarySecond = 3,

        /// <summary>
        /// بیمه تکمیلی سوم - چهارمین اولویت
        /// سومین بیمه تکمیلی برای پوشش کامل‌تر
        /// </summary>
        [Description("بیمه تکمیلی سوم")]
        SupplementaryThird = 4,

        /// <summary>
        /// بیمه تکمیلی چهارم - پنجمین اولویت
        /// چهارمین بیمه تکمیلی
        /// </summary>
        [Description("بیمه تکمیلی چهارم")]
        SupplementaryFourth = 5,

        /// <summary>
        /// بیمه تکمیلی پنجم - ششمین اولویت
        /// پنجمین بیمه تکمیلی
        /// </summary>
        [Description("بیمه تکمیلی پنجم")]
        SupplementaryFifth = 6,

        /// <summary>
        /// بیمه تکمیلی ششم - هفتمین اولویت
        /// ششمین بیمه تکمیلی
        /// </summary>
        [Description("بیمه تکمیلی ششم")]
        SupplementarySixth = 7,

        /// <summary>
        /// بیمه تکمیلی هفتم - هشتمین اولویت
        /// هفتمین بیمه تکمیلی
        /// </summary>
        [Description("بیمه تکمیلی هفتم")]
        SupplementarySeventh = 8,

        /// <summary>
        /// بیمه تکمیلی هشتم - نهمین اولویت
        /// هشتمین بیمه تکمیلی
        /// </summary>
        [Description("بیمه تکمیلی هشتم")]
        SupplementaryEighth = 9,

        /// <summary>
        /// بیمه تکمیلی نهم - دهمین اولویت
        /// نهمین بیمه تکمیلی
        /// </summary>
        [Description("بیمه تکمیلی نهم")]
        SupplementaryNinth = 10
    }
}
