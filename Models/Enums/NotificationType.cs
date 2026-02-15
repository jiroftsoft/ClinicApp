namespace ClinicApp.Models.Enums;

/// <summary>
/// نوع اعلان برای Idempotency و تفکیک قالب‌ها
/// </summary>
public enum NotificationType
{
    /// <summary>اعلان رزرو موفق نوبت (بلافاصله بعد از Commit)</summary>
    AppointmentBookingConfirmation = 1,

    /// <summary>اعلان تأیید پرداخت (بعد از PaymentCallback Commit)</summary>
    PaymentConfirmation = 2,

    /// <summary>یادآوری نوبت — 24 ساعت قبل</summary>
    AppointmentReminder24h = 3,

    /// <summary>یادآوری نوبت — 3 ساعت قبل</summary>
    AppointmentReminder3h = 4,

    /// <summary>یادآوری نوبت — 30 دقیقه قبل (اختیاری)</summary>
    AppointmentReminder30min = 5
}
