using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// وضعیت پیام‌های فرم تماس
    /// </summary>
    public enum ContactFormStatus : byte
    {
        /// <summary>
        /// جدید
        /// </summary>
        [Description("جدید")]
        [Display(Name = "جدید")]
        New = 1,

        /// <summary>
        /// در حال بررسی
        /// </summary>
        [Description("در حال بررسی")]
        [Display(Name = "در حال بررسی")]
        InProgress = 2,

        /// <summary>
        /// پاسخ داده شده
        /// </summary>
        [Description("پاسخ داده شده")]
        [Display(Name = "پاسخ داده شده")]
        Replied = 3,

        /// <summary>
        /// بسته شده
        /// </summary>
        [Description("بسته شده")]
        [Display(Name = "بسته شده")]
        Closed = 4
    }
}

