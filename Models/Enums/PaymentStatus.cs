using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// وضعیت‌های تراکنش
/// </summary>
public enum PaymentStatus
{
    [Display(Name = "در انتظار")]
    Pending = 1,
    [Display(Name = "موفق")]
    Success = 2,
    [Display(Name = "موفق")]
    Successful = 2,
    [Display(Name = "ناموفق")]
    Failed = 3,
    [Display(Name = "لغو شده")]
    Canceled = 4,
    [Display(Name = "در حال بررسی")]
    UnderReview = 5,
    Completed
}