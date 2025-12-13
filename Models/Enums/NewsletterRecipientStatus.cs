using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// وضعیت Recipient در Campaign
    /// </summary>
    public enum NewsletterRecipientStatus : byte
    {
        /// <summary>
        /// در انتظار ارسال
        /// </summary>
        [Description("در انتظار")]
        [Display(Name = "در انتظار")]
        Pending = 1,

        /// <summary>
        /// ارسال شده
        /// </summary>
        [Description("ارسال شده")]
        [Display(Name = "ارسال شده")]
        Sent = 2,

        /// <summary>
        /// ناموفق
        /// </summary>
        [Description("ناموفق")]
        [Display(Name = "ناموفق")]
        Failed = 3,

        /// <summary>
        /// بازگشت خورده (Bounced)
        /// </summary>
        [Description("بازگشت خورده")]
        [Display(Name = "بازگشت خورده")]
        Bounced = 4
    }
}

