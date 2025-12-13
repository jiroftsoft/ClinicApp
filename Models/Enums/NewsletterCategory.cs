using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// دسته‌بندی‌های خبرنامه
    /// </summary>
    public enum NewsletterCategory : byte
    {
        /// <summary>
        /// مقالات
        /// </summary>
        [Description("مقالات")]
        [Display(Name = "مقالات")]
        Articles = 1,

        /// <summary>
        /// اطلاعیه‌ها
        /// </summary>
        [Description("اطلاعیه‌ها")]
        [Display(Name = "اطلاعیه‌ها")]
        Announcements = 2,

        /// <summary>
        /// خدمات جدید
        /// </summary>
        [Description("خدمات جدید")]
        [Display(Name = "خدمات جدید")]
        NewServices = 3,

        /// <summary>
        /// نکات سلامتی
        /// </summary>
        [Description("نکات سلامتی")]
        [Display(Name = "نکات سلامتی")]
        HealthTips = 4,

        /// <summary>
        /// رویدادها
        /// </summary>
        [Description("رویدادها")]
        [Display(Name = "رویدادها")]
        Events = 5,

        /// <summary>
        /// تخفیف‌ها
        /// </summary>
        [Description("تخفیف‌ها")]
        [Display(Name = "تخفیف‌ها")]
        Promotions = 6
    }
}

