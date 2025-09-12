using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// وضعیت‌های مختلف اسلات نوبت
/// </summary>
public enum AppointmentSlotStatus : byte
{
    [Display(Name = "در دسترس")]
    Available = 0,
    [Display(Name = "رزرو شده")]
    Booked = 1,
    [Display(Name = "تکمیل شده")]
    Completed = 2,
    [Display(Name = "لغو شده")]
    Cancelled = 3,
    [Display(Name = "مسدود شده")]
    Blocked = 4,
    [Display(Name = "در انتظار تأیید")]
    PendingConfirmation = 5,
    [Display(Name = "نیاز به پرداخت")]
    NeedsPayment = 6
}