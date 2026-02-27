using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// وضعیت دسته‌صورت‌حساب بیمه
    /// </summary>
    public enum BatchStatus : byte
    {
        [Display(Name = "ارسال شده")]
        Submitted = 1,

        [Display(Name = "در حال بررسی")]
        UnderReview = 2,

        [Display(Name = "تسویه شده")]
        Settled = 3
    }
}
