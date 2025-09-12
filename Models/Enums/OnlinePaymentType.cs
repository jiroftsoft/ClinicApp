using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// نوع تراکنش پرداخت آنلاین
/// </summary>
public enum OnlinePaymentType
{
    [Display(Name = "پرداخت نوبت")]
    Appointment = 1,
    [Display(Name = "پرداخت پذیرش")]
    Reception = 2,
    [Display(Name = "پرداخت خدمت")]
    Service = 3,
    [Display(Name = "پرداخت بدهی")]
    Debt = 4,
    [Display(Name = "پیش‌پرداخت")]
    PrePayment = 5
}