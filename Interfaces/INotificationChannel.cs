using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Notification;

namespace ClinicApp.Interfaces;

/// <summary>
/// اینترفیس کانال‌های اطلاع‌رسانی
/// هر کانال ارسال (پیامک، ایمیل، اپلیکیشن و غیره) باید از این اینترفیس ارث بری کند
/// </summary>
public interface INotificationChannel
{
    /// <summary>
    /// نوع کانال اطلاع‌رسانی
    /// </summary>
    NotificationChannelType ChannelType { get; }

    /// <summary>
    /// بررسی آماده‌بودن کانال برای ارسال
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// ارسال پیام از طریق این کانال
    /// </summary>
    Task<ChannelSendResult> SendAsync(NotificationMessage message);

    /// <summary>
    /// دریافت وضعیت کانال
    /// </summary>
    ChannelStatus GetStatus();
}