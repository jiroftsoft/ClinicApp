namespace ClinicApp.Models.Enums;

/// <summary>
/// انواع کانال‌های اطلاع‌رسانی در سیستم کلینیک
/// </summary>
public enum NotificationChannelType
{
    /// <summary>
    /// ارسال از طریق پیامک
    /// </summary>
    Sms = 1,

    /// <summary>
    /// ارسال از طریق ایمیل
    /// </summary>
    Email = 2,

    /// <summary>
    /// ارسال از طریق پوش‌نوتیفیکیشن موبایل
    /// </summary>
    AppPush = 3,

    /// <summary>
    /// ارسال داخلی از طریق پنل کاربری
    /// </summary>
    InApp = 4
}