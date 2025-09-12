using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// وضعیت پرداخت آنلاین
/// </summary>
public enum OnlinePaymentStatus
{
    [Display(Name = "در انتظار")]
    Pending = 1,
    [Display(Name = "در حال پردازش")]
    Processing = 2,
    [Display(Name = "موفق")]
    Success = 3,
    [Display(Name = "موفق")]
    Successful = 3,
    [Display(Name = "ناموفق")]
    Failed = 4,
    [Display(Name = "لغو شده")]
    Canceled = 5,
    [Display(Name = "برگشت خورده")]
    Refunded = 6,
    [Display(Name = "منقضی شده")]
    Expired = 7
}