using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// نوع مخاطب برای ارسال پیامک ایونت تبلیغاتی
    /// </summary>
    public enum PromotionalEventAudience : byte
    {
        /// <summary>
        /// بیماران دارای شماره موبایل (Patient.PhoneNumber)
        /// </summary>
        [Display(Name = "بیماران دارای شماره موبایل")]
        PatientsWithPhone = 1,

        /// <summary>
        /// مشترکین خبرنامه دارای شماره (NewsletterSubscription.PhoneNumber)
        /// </summary>
        [Display(Name = "مشترکین خبرنامه")]
        NewsletterSubscribers = 2,

        /// <summary>
        /// هر دو (اتحاد با حذف تکراری)
        /// </summary>
        [Display(Name = "بیماران + مشترکین خبرنامه")]
        Both = 3
    }
}
