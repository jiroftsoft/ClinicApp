using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// وضعیت پذیرش
/// </summary>
public enum ReceptionStatus : byte
{
    [Display(Name = "در انتظار")]
    Pending = 0,
    [Display(Name = "تکمیل شده")]
    Completed = 1,
    [Display(Name = "لغو شده")]
    Cancelled = 2,
    [Display(Name = "در حال انجام")]
    InProgress = 3,
    [Display(Name = "نیاز به پرداخت بیشتر")]
    NeedsAdditionalPayment = 4,
    NoShow,
    Confirmed
}