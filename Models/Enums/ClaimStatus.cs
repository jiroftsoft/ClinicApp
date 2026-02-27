using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// وضعیت مطالبه بیمه (از ارسال تا واریز)
    /// </summary>
    public enum ClaimStatus : byte
    {
        [Display(Name = "در انتظار")]
        Pending = 1,

        [Display(Name = "تأیید شده")]
        Approved = 2,

        [Display(Name = "پرداخت جزئی")]
        PartiallyPaid = 3,

        [Display(Name = "پرداخت شده")]
        Paid = 4,

        [Display(Name = "رد شده")]
        Rejected = 5
    }
}
