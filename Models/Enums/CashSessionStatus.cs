using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// وضعیت شیفت‌های صندوق
/// </summary>
public enum CashSessionStatus
{
    [Display(Name = "باز")]
    Open = 1,
    [Display(Name = "فعال")]
    Active = 1,
    [Display(Name = "بسته")]
    Closed = 2,
    [Display(Name = "در حال بررسی")]
    UnderReview = 3
}