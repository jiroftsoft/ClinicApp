namespace ClinicApp.Models.Enums;

/// <summary>
/// وضعیت‌های رزرو اورژانس
/// </summary>
public enum EmergencyBookingStatus
{
    /// <summary>
    /// در انتظار تأیید
    /// </summary>
    Pending = 1,

    /// <summary>
    /// تأیید شده
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// لغو شده
    /// </summary>
    Canceled = 3,

    /// <summary>
    /// تکمیل شده
    /// </summary>
    Completed = 4,

    /// <summary>
    /// در حال انجام
    /// </summary>
    InProgress = 5
}