using System.Collections.Generic;
using System.Linq;
using ClinicApp.Services.Notification;
using Serilog;

namespace ClinicApp.Services;

/// <summary>
/// پیاده‌سازی سرویس مدیریت اعلان‌ها
/// </summary>
public class MessageNotificationService : IMessageNotificationService
{
    private readonly ILogger _logger;
    private const string SESSION_KEY = "NotificationMessages";

    public MessageNotificationService(ILogger logger)
    {
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// افزودن پیام موفقیت
    /// </summary>
    public void AddSuccessMessage(string message)
    {
        AddMessage(message, NotificationType.Success);
        _logger.Information("پیام موفقیت اضافه شد: {Message}", message);
    }

    /// <summary>
    /// افزودن پیام خطا
    /// </summary>
    public void AddErrorMessage(string message)
    {
        AddMessage(message, NotificationType.Error);
        _logger.Error("پیام خطا اضافه شد: {Message}", message);
    }

    /// <summary>
    /// افزودن پیام هشدار
    /// </summary>
    public void AddWarningMessage(string message)
    {
        AddMessage(message, NotificationType.Warning);
        _logger.Warning("پیام هشدار اضافه شد: {Message}", message);
    }

    /// <summary>
    /// افزودن پیام اطلاعاتی
    /// </summary>
    public void AddInfoMessage(string message)
    {
        AddMessage(message, NotificationType.Info);
        _logger.Information("پیام اطلاعاتی اضافه شد: {Message}", message);
    }

    /// <summary>
    /// دریافت تمام پیام‌ها
    /// </summary>
    public List<NotificationMessage> GetMessages()
    {
        var messages = System.Web.HttpContext.Current?.Session?[SESSION_KEY] as List<NotificationMessage>;
        return messages ?? new List<NotificationMessage>();
    }

    /// <summary>
    /// پاک کردن تمام پیام‌ها
    /// </summary>
    public void ClearMessages()
    {
        System.Web.HttpContext.Current?.Session?.Remove(SESSION_KEY);
        _logger.Information("تمام پیام‌های اعلان پاک شدند");
    }

    /// <summary>
    /// بررسی وجود پیام
    /// </summary>
    public bool HasMessages()
    {
        return GetMessages().Count > 0;
    }

    /// <summary>
    /// دریافت پیام‌ها بر اساس نوع
    /// </summary>
    public List<NotificationMessage> GetMessagesByType(NotificationType type)
    {
        return GetMessages().Where(m => m.Type == type).ToList();
    }

    /// <summary>
    /// افزودن پیام به Session
    /// </summary>
    private void AddMessage(string message, NotificationType type)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var messages = GetMessages();
        messages.Add(new NotificationMessage(message, type));
            
        System.Web.HttpContext.Current.Session[SESSION_KEY] = messages;
    }


}