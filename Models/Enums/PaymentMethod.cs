using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// روش‌های پرداخت (ادغام شده برای کل سیستم)
/// </summary>
public enum PaymentMethod : byte
{
    [Display(Name = "نقدی")] // از Payment قدیمی
    Cash = 0,
    [Display(Name = "پوز")] // از Payment قدیمی
    POS = 1,
    [Display(Name = "آنلاین")] // از Payment قدیمی
    Online = 2,
    [Display(Name = "بدهی")] // جدید از PaymentTransaction
    Debt = 3,
    [Display(Name = "کارت به کارت")] // جدید از PaymentTransaction
    Card = 4,
    [Display(Name = "حواله بانکی")] // جدید از PaymentTransaction
    BankTransfer = 5,
    Insurance
}