using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// نوع تخفیف برای ایونت‌های تبلیغاتی
/// </summary>
public enum DiscountType : byte
{
    /// <summary>
    /// تخفیف درصدی (مثلاً 20%)
    /// </summary>
    [Display(Name = "درصدی")]
    Percentage = 1,

    /// <summary>
    /// تخفیف مبلغ ثابت (مثلاً 100,000 ریال)
    /// </summary>
    [Display(Name = "مبلغ ثابت")]
    FixedAmount = 2
}

