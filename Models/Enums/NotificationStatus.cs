namespace ClinicApp.Models.Enums;

/// <summary>
/// وضعیت‌های ارسال پیام در سیستم کلینیک
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// پیام در صف ارسال قرار دارد
    /// </summary>
    Queued = 1,

    /// <summary>
    /// در حال ارسال به سرویس‌دهنده
    /// </summary>
    Sending = 2,

    /// <summary>
    /// ارسال با موفقیت انجام شده
    /// </summary>
    Sent = 3,

    /// <summary>
    /// ارسال با خطا مواجه شده
    /// </summary>
    Failed = 4,

    /// <summary>
    /// ارسال لغو شده است
    /// </summary>
    Canceled = 5,

    /// <summary>
    /// ارسال به صورت زمان‌بندی شده
    /// </summary>
    Scheduled = 6
}