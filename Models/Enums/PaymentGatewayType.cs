using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// نوع درگاه پرداخت آنلاین
/// </summary>
public enum PaymentGatewayType
{
    [Display(Name = "زرین‌پال")]
    ZarinPal = 1,
    [Display(Name = "پی‌پینگ")]
    PayPing = 2,
    [Display(Name = "آی‌دی‌پی")]
    IDPay = 3,
    [Display(Name = "پاسارگاد")]
    Pasargad = 4,
    [Display(Name = "ملت")]
    Mellat = 5,
    [Display(Name = "پارسیان")]
    Parsian = 6,
    [Display(Name = "سامان")]
    Saman = 7,
    [Display(Name = "سپهر")]
    Sepah = 8
}