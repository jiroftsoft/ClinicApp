using System.Collections.Generic;
using ClinicApp.Services.Notification;

namespace ClinicApp.Services;

/// <summary>
/// سرویس مدیریت اعلان‌ها و پیام‌ها در سیستم کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت پیام‌های Success، Error، Warning و Info
/// 2. پشتیبانی از Session برای persistence
/// 3. پشتیبانی از TempData برای redirect scenarios
/// 4. Logging کامل برای audit trail
/// 5. پشتیبانی از multiple message types
/// 6. رعایت استانداردهای پزشکی ایران
/// 
/// نکته حیاتی: این سرویس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
/// </summary>
public interface IMessageNotificationService
{
    /// <summary>
    /// افزودن پیام موفقیت
    /// </summary>
    void AddSuccessMessage(string message);

    /// <summary>
    /// افزودن پیام خطا
    /// </summary>
    void AddErrorMessage(string message);

    /// <summary>
    /// افزودن پیام هشدار
    /// </summary>
    void AddWarningMessage(string message);

    /// <summary>
    /// افزودن پیام اطلاعاتی
    /// </summary>
    void AddInfoMessage(string message);

    /// <summary>
    /// دریافت تمام پیام‌ها
    /// </summary>
    List<NotificationMessage> GetMessages();

    /// <summary>
    /// پاک کردن تمام پیام‌ها
    /// </summary>
    void ClearMessages();

    /// <summary>
    /// بررسی وجود پیام
    /// </summary>
    bool HasMessages();

    /// <summary>
    /// دریافت پیام‌ها بر اساس نوع
    /// </summary>
    List<NotificationMessage> GetMessagesByType(NotificationType type);
}