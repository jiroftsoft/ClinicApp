using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Notification;

namespace ClinicApp.Interfaces;

/// اینترفیس اصلی سرویس اطلاع‌رسانی برای سیستم کلینیک درمانی
/// این سرویس مسئولیت ارسال اطلاع‌رسانی‌ها از طریق کانال‌های مختلف را بر عهده دارد
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// ارسال اطلاع‌رسانی به یک گیرنده خاص
    /// </summary>
    Task<NotificationResult> SendAsync(NotificationRequest request);

    /// <summary>
    /// ارسال اطلاع‌رسانی به گروهی از گیرندگان
    /// </summary>
    Task<NotificationBatchResult> SendBatchAsync(IEnumerable<NotificationRequest> requests);

    /// <summary>
    /// ارسال اطلاع‌رسانی به گیرنده بر اساس الگوی پیام
    /// </summary>
    Task<NotificationResult> SendTemplateAsync(string templateKey, object recipient, params object[] parameters);

    /// <summary>
    /// ارسال اطلاع‌رسانی جمعی به گروهی از گیرندگان بر اساس الگوی پیام
    /// </summary>
    Task<NotificationBatchResult> SendBatchTemplateAsync(string templateKey, IEnumerable<object> recipients, params object[] parameters);

    /// <summary>
    /// ثبت اطلاع‌رسانی برای ارسال در زمان مشخص
    /// </summary>
    Task<NotificationScheduleResult> ScheduleAsync(NotificationRequest request, DateTime scheduledTime);

    /// <summary>
    /// دریافت وضعیت یک اطلاع‌رسانی
    /// </summary>
    Task<NotificationStatus> GetStatusAsync(Guid notificationId);

    /// <summary>
    /// لغو یک اطلاع‌رسانی زمان‌بندی شده
    /// </summary>
    Task<bool> CancelScheduledAsync(Guid notificationId);
}