using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Notification;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces;

/// <summary>
/// اینترفیس سرویس مدیریت تاریخچه اطلاع‌رسانی
/// این سرویس مسئولیت ذخیره و بازیابی تاریخچه ارسال پیام‌ها را بر عهده دارد
/// </summary>
public interface INotificationHistoryService
{
    /// <summary>
    /// ذخیره تاریخچه اطلاع‌رسانی
    /// </summary>
    Task<bool> SaveHistoryAsync(NotificationHistory history);

    /// <summary>
    /// دریافت تاریخچه اطلاع‌رسانی بر اساس شناسه
    /// </summary>
    Task<NotificationHistory> GetHistoryByIdAsync(Guid id);

    /// <summary>
    /// دریافت تاریخچه اطلاع‌رسانی بر اساس شناسه اطلاع‌رسانی
    /// </summary>
    Task<NotificationHistory> GetHistoryByNotificationIdAsync(Guid notificationId);

    /// <summary>
    /// دریافت تاریخچه اطلاع‌رسانی برای یک گیرنده
    /// </summary>
    Task<IEnumerable<NotificationHistory>> GetHistoryByRecipientAsync(string recipient, int page = 1, int pageSize = 20);

    /// <summary>
    /// دریافت تاریخچه اطلاع‌رسانی برای یک کاربر
    /// </summary>
    Task<IEnumerable<NotificationHistory>> GetHistoryByUserIdAsync(string userId, int page = 1, int pageSize = 20);

    /// <summary>
    /// دریافت تاریخچه اطلاع‌رسانی بر اساس نوع
    /// </summary>
    Task<IEnumerable<NotificationHistory>> GetHistoryByTypeAsync(NotificationChannelType channelType, int page = 1, int pageSize = 20);

    /// <summary>
    /// دریافت تاریخچه اطلاع‌رسانی بر اساس بازه زمانی
    /// </summary>
    Task<IEnumerable<NotificationHistory>> GetHistoryByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 20);
}