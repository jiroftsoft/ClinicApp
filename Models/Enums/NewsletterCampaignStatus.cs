using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// وضعیت Campaign خبرنامه
    /// </summary>
    public enum NewsletterCampaignStatus : byte
    {
        /// <summary>
        /// پیش‌نویس
        /// </summary>
        [Description("پیش‌نویس")]
        [Display(Name = "پیش‌نویس")]
        Draft = 1,

        /// <summary>
        /// زمان‌بندی شده
        /// </summary>
        [Description("زمان‌بندی شده")]
        [Display(Name = "زمان‌بندی شده")]
        Scheduled = 2,

        /// <summary>
        /// در حال ارسال
        /// </summary>
        [Description("در حال ارسال")]
        [Display(Name = "در حال ارسال")]
        Sending = 3,

        /// <summary>
        /// ارسال شده
        /// </summary>
        [Description("ارسال شده")]
        [Display(Name = "ارسال شده")]
        Sent = 4,

        /// <summary>
        /// ناموفق
        /// </summary>
        [Description("ناموفق")]
        [Display(Name = "ناموفق")]
        Failed = 5
    }
}

