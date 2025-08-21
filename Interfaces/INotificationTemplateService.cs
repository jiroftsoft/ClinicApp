using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;

namespace ClinicApp.Interfaces;

/// <summary>
/// اینترفیس سرویس مدیریت الگوهای اطلاع‌رسانی
/// این سرویس مسئولیت مدیریت الگوهای پیام‌های استاندارد را بر عهده دارد
/// </summary>
public interface INotificationTemplateService
{
    /// <summary>
    /// دریافت الگوی پیام بر اساس کلید
    /// </summary>
    Task<NotificationTemplate> GetTemplateAsync(string key);

    /// <summary>
    /// دریافت تمام الگوهای پیام
    /// </summary>
    Task<IEnumerable<NotificationTemplate>> GetAllTemplatesAsync();

    /// <summary>
    /// افزودن یا به‌روزرسانی الگوی پیام
    /// </summary>
    Task<bool> UpsertTemplateAsync(NotificationTemplate template);

    /// <summary>
    /// حذف الگوی پیام
    /// </summary>
    Task<bool> DeleteTemplateAsync(string key);

    /// <summary>
    /// ترجمه پیام بر اساس پارامترها
    /// </summary>
    string TranslateMessage(string message, params object[] parameters);
}