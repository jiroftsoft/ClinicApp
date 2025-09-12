using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;

namespace ClinicApp.Services.Notification
{
    #region Enums
    /// <summary>
    /// اولویت‌های ارسال پیام در سیستم کلینیک
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// اولویت پایین (پیام‌های غیرفوری)
        /// </summary>
        Low = 1,

        /// <summary>
        /// اولویت عادی (پیش‌فرض سیستم)
        /// </summary>
        Normal = 2,

        /// <summary>
        /// اولویت بالا (پیام‌های مهم)
        /// </summary>
        High = 3,

        /// <summary>
        /// اولویت بحرانی (پیام‌های فوری پزشکی)
        /// </summary>
        Critical = 4
    }

    /// <summary>
    /// انواع پیام‌های اعلان در سیستم کلینیک
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// پیام موفقیت
        /// </summary>
        Success,

        /// <summary>
        /// پیام خطا
        /// </summary>
        Error,

        /// <summary>
        /// پیام هشدار
        /// </summary>
        Warning,

        /// <summary>
        /// پیام اطلاعاتی
        /// </summary>
        Info
    }
    #endregion

    #region Core Request Models
    /// <summary>
    /// درخواست ارسال اطلاع‌رسانی در سیستم کلینیک شفا
    /// این مدل برای ایجاد و ارسال پیام‌های اطلاع‌رسانی استفاده می‌شود
    /// </summary>
    public class NotificationRequest
    {
        /// <summary>
        /// شناسه یکتای اطلاع‌رسانی (به صورت خودکار تولید می‌شود)
        /// </summary>
        [Required]
        public Guid NotificationId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// نوع کانال اطلاع‌رسانی مورد استفاده
        /// </summary>
        [Required]
        public NotificationChannelType ChannelType { get; set; }

        /// <summary>
        /// شماره/ایمیل گیرنده پیام
        /// </summary>
        [Required, MaxLength(50)]
        public string Recipient { get; set; }

        /// <summary>
        /// موضوع پیام (برای ایمیل و پیام‌های داخلی)
        /// </summary>
        [MaxLength(200)]
        public string Subject { get; set; }

        /// <summary>
        /// متن اصلی پیام
        /// </summary>
        [Required, MaxLength(1000)]
        public string Message { get; set; }

        /// <summary>
        /// اولویت ارسال پیام
        /// </summary>
        [Required]
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        /// <summary>
        /// زمان زمان‌بندی شده برای ارسال (در صورت عدم تنظیم، بلافاصله ارسال می‌شود)
        /// </summary>
        public DateTime? ScheduledTime { get; set; }

        /// <summary>
        /// پارامترهای پیام برای جایگزینی در الگو
        /// </summary>
        [Required]
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// شناسه کاربر ارسال کننده
        /// </summary>
        [MaxLength(128)]
        public string SenderUserId { get; set; }

        /// <summary>
        /// شناسه سیستمی مرتبط (مثل شناسه پذیرش، نوبت و غیره)
        /// </summary>
        [MaxLength(50)]
        public string RelatedEntityId { get; set; }

        /// <summary>
        /// نوع موجودیت مرتبط (نوع سند یا موجودیت سیستمی)
        /// </summary>
        [MaxLength(100)]
        public string RelatedEntityType { get; set; }
    }
    #endregion

    #region Core Response Models
    /// <summary>
    /// نتیجه ارسال اطلاع‌رسانی در سیستم کلینیک
    /// این مدل برای بازگشت نتایج عملیات ارسال پیام استفاده می‌شود
    /// </summary>
    public class NotificationResult
    {
        /// <summary>
        /// آیا ارسال پیام با موفقیت انجام شد
        /// </summary>
        [Required]
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// شناسه اطلاع‌رسانی مرتبط
        /// </summary>
        [Required]
        public Guid NotificationId { get; set; }

        /// <summary>
        /// وضعیت فعلی ارسال
        /// </summary>
        [Required]
        public NotificationStatus Status { get; set; }

        /// <summary>
        /// توضیحات تکمیلی وضعیت ارسال
        /// </summary>
        [MaxLength(500)]
        public string StatusDescription { get; set; }

        /// <summary>
        /// شناسه پیام در سرویس ارسال خارجی
        /// </summary>
        [MaxLength(100)]
        public string ExternalMessageId { get; set; }

        /// <summary>
        /// زمان ارسال به سرویس‌دهنده
        /// </summary>
        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// نتیجه ارسال کانال اطلاع‌رسانی
    /// این مدل برای بازگشت نتایج عملیات ارسال به کانال خاص استفاده می‌شود
    /// </summary>
    public class ChannelSendResult
    {
        /// <summary>
        /// آیا ارسال از طریق کانال با موفقیت انجام شد
        /// </summary>
        [Required]
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// توضیحات وضعیت ارسال
        /// </summary>
        [MaxLength(500)]
        public string StatusDescription { get; set; }

        /// <summary>
        /// شناسه پیام در سرویس ارسال خارجی
        /// </summary>
        [MaxLength(100)]
        public string ExternalMessageId { get; set; }
    }
    #endregion

    #region Batch & Scheduling Models
    /// <summary>
    /// نتیجه ارسال دسته‌ای اطلاع‌رسانی در سیستم کلینیک
    /// این مدل برای بازگشت نتایج ارسال گروهی پیام‌ها استفاده می‌شود
    /// </summary>
    public class NotificationBatchResult
    {
        /// <summary>
        /// تعداد کل اطلاع‌رسانی‌های پردازش شده
        /// </summary>
        [Required]
        public int TotalCount { get; set; }

        /// <summary>
        /// تعداد اطلاع‌رسانی‌های ارسال شده با موفقیت
        /// </summary>
        [Required]
        public int SuccessfulCount { get; set; }

        /// <summary>
        /// تعداد اطلاع‌رسانی‌های ارسال نشده
        /// </summary>
        [Required]
        public int FailedCount { get; set; }

        /// <summary>
        /// نتایج جزئی هر اطلاع‌رسانی
        /// </summary>
        [Required]
        public List<NotificationResult> Results { get; set; } = new List<NotificationResult>();
    }

    /// <summary>
    /// نتیجه زمان‌بندی اطلاع‌رسانی در سیستم کلینیک
    /// این مدل برای بازگشت نتایج عملیات زمان‌بندی پیام استفاده می‌شود
    /// </summary>
    public class NotificationScheduleResult
    {
        /// <summary>
        /// آیا زمان‌بندی پیام با موفقیت انجام شد
        /// </summary>
        [Required]
        public bool IsScheduled { get; set; }

        /// <summary>
        /// شناسه اطلاع‌رسانی مرتبط
        /// </summary>
        [Required]
        public Guid NotificationId { get; set; }

        /// <summary>
        /// زمان برنامه‌ریزی شده برای ارسال
        /// </summary>
        [Required]
        public DateTime ScheduledTime { get; set; }

        /// <summary>
        /// توضیحات تکمیلی زمان‌بندی
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }
    }
    #endregion

    #region Utility Models
    /// <summary>
    /// مدل پیام اطلاع‌رسانی برای ارسال به کانال‌ها
    /// این کلاس ساده برای انتقال اطلاعات پیام به کانال‌های ارسال استفاده می‌شود
    /// </summary>
    public class NotificationMessage
    {
        /// <summary>
        /// گیرنده پیام (شماره/ایمیل)
        /// </summary>
        [Required, MaxLength(50)]
        public string Recipient { get; set; }

        /// <summary>
        /// موضوع پیام (برای کانال‌های پشتیبان)
        /// </summary>
        [MaxLength(200)]
        public string Subject { get; set; }

        /// <summary>
        /// متن اصلی پیام
        /// </summary>
        [Required, MaxLength(1000)]
        public string Message { get; set; }

        /// <summary>
        /// نوع پیام اعلان
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// زمان ایجاد پیام
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// آیکون پیام
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// کلاس CSS پیام
        /// </summary>
        public string CssClass { get; set; }

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        public NotificationMessage()
        {
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// سازنده با پارامتر
        /// </summary>
        public NotificationMessage(string message, NotificationType type)
        {
            Message = message;
            Type = type;
            Timestamp = DateTime.Now;

            // تنظیم آیکون و CSS class بر اساس نوع
            switch (type)
            {
                case NotificationType.Success:
                    Icon = "fas fa-check-circle";
                    CssClass = "alert-success";
                    break;
                case NotificationType.Error:
                    Icon = "fas fa-exclamation-circle";
                    CssClass = "alert-danger";
                    break;
                case NotificationType.Warning:
                    Icon = "fas fa-exclamation-triangle";
                    CssClass = "alert-warning";
                    break;
                case NotificationType.Info:
                    Icon = "fas fa-info-circle";
                    CssClass = "alert-info";
                    break;
            }
        }
    }

    /// <summary>
    /// وضعیت کانال اطلاع‌رسانی در سیستم کلینیک
    /// این مدل برای بررسی وضعیت کانال‌های ارسال استفاده می‌شود
    /// </summary>
    public class ChannelStatus
    {
        /// <summary>
        /// آیا کانال آماده ارسال است
        /// </summary>
        [Required]
        public bool IsReady { get; set; }

        /// <summary>
        /// وضعیت فعلی کانال (متن توصیفی)
        /// </summary>
        [MaxLength(200)]
        public string Status { get; set; }

        /// <summary>
        /// تعداد پیام‌های ارسال شده امروز
        /// </summary>
        [Range(0, int.MaxValue)]
        public int DailySentCount { get; set; }

        /// <summary>
        /// حداکثر تعداد پیام‌های روزانه مجاز
        /// </summary>
        [Range(0, int.MaxValue)]
        public int DailyLimit { get; set; }

        /// <summary>
        /// درصد استفاده از محدودیت روزانه
        /// </summary>
        public double UsagePercentage => DailyLimit > 0 ?
            Math.Round((double)DailySentCount / DailyLimit * 100, 1) : 0;
    }
    #endregion
}