using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// دسته‌بندی پیام‌های فرم تماس
    /// </summary>
    public enum ContactFormCategory : byte
    {
        /// <summary>
        /// سوال
        /// </summary>
        [Description("سوال")]
        [Display(Name = "سوال")]
        Question = 1,

        /// <summary>
        /// پیشنهاد
        /// </summary>
        [Description("پیشنهاد")]
        [Display(Name = "پیشنهاد")]
        Suggestion = 2,

        /// <summary>
        /// شکایت
        /// </summary>
        [Description("شکایت")]
        [Display(Name = "شکایت")]
        Complaint = 3,

        /// <summary>
        /// نوبت‌دهی
        /// </summary>
        [Description("نوبت‌دهی")]
        [Display(Name = "نوبت‌دهی")]
        Appointment = 4,

        /// <summary>
        /// سایر
        /// </summary>
        [Description("سایر")]
        [Display(Name = "سایر")]
        Other = 5
    }
}

