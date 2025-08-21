using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace ClinicApp.Models.Entities
{
    #region Enums
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
    #endregion

    #region Entities
    /// <summary>
    /// تاریخچه اطلاع‌رسانی برای سیستم کلینیک شفا
    /// این موجودیت برای ذخیره تاریخچه ارسال پیام‌ها و اعلان‌ها استفاده می‌شود
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از چندین کانال ارسال (پیامک، ایمیل، اپلیکیشن)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای رعایت استانداردهای پزشکی
    /// 3. مدیریت کامل وضعیت‌های ارسال (در صف، در حال ارسال، موفق، ناموفق)
    /// 4. ارتباط با موجودیت‌های سیستم (نوبت، پذیرش، بیمار و غیره)
    /// 5. پشتیبانی از ردیابی کامل برای سیستم‌های پزشکی
    /// </summary>
    public class NotificationHistory : ITrackable, ISoftDelete
    {
        /// <summary>
        /// شناسه یکتای تاریخچه
        /// </summary>
        [Key]
        public Guid HistoryId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// شناسه یکتای اطلاع‌رسانی
        /// </summary>
        [Required]
        public Guid NotificationId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// نوع کانال ارسال
        /// </summary>
        [Required]
        public NotificationChannelType ChannelType { get; set; }

        /// <summary>
        /// شماره/ایمیل گیرنده
        /// </summary>
        [Required, MaxLength(50)]
        public string Recipient { get; set; }

        /// <summary>
        /// موضوع پیام
        /// </summary>
        [MaxLength(200)]
        public string Subject { get; set; }

        /// <summary>
        /// متن اصلی پیام
        /// </summary>
        [Required, MaxLength(1000)]
        public string Message { get; set; }

        /// <summary>
        /// زمان ارسال به سرویس‌دهنده
        /// </summary>
        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// وضعیت فعلی ارسال
        /// </summary>
        [Required]
        public NotificationStatus Status { get; set; } = NotificationStatus.Queued;

        /// <summary>
        /// توضیحات تکمیلی وضعیت
        /// </summary>
        [MaxLength(500)]
        public string StatusDescription { get; set; }

        /// <summary>
        /// شناسه کاربر ارسال‌کننده
        /// </summary>
        [MaxLength(128)]
        public string SenderUserId { get; set; }

        /// <summary>
        /// شناسه موجودیت سیستمی مرتبط
        /// </summary>
        [MaxLength(50)]
        public string RelatedEntityId { get; set; }

        /// <summary>
        /// نوع موجودیت مرتبط (نوبت، پذیرش، بیمار و...)
        /// </summary>
        [MaxLength(100)]
        public string RelatedEntityType { get; set; }

        /// <summary>
        /// تعداد تلاش‌های ارسال
        /// </summary>
        [Required]
        public int AttemptCount { get; set; } = 0;

        /// <summary>
        /// شناسه پیام در سرویس‌دهنده خارجی
        /// </summary>
        [MaxLength(100)]
        public string ExternalMessageId { get; set; }

        // پیاده‌سازی ISoftDelete
        [Required]
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        [MaxLength(128)]
        public string DeletedByUserId { get; set; }

        // پیاده‌سازی ITrackable
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(128)]
        public string CreatedByUserId { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(128)]
        public string UpdatedByUserId { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }

        // روابط
        public virtual ApplicationUser SenderUser { get; set; }
        public virtual ApplicationUser DeletedByUser { get; set; }
    }

    /// <summary>
    /// الگوی اطلاع‌رسانی برای سیستم کلینیک شفا
    /// این موجودیت برای ذخیره الگوهای پیام‌های استاندارد استفاده می‌شود
    /// </summary>
    public class NotificationTemplate : ITrackable
    {
        /// <summary>
        /// کلید یکتای الگو (مقدار ثابت از NotificationTemplates)
        /// </summary>
        [Key, MaxLength(50)]
        public string Key { get; set; }

        /// <summary>
        /// عنوان نمایشی الگو
        /// </summary>
        [Required, MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات تکمیلی الگو
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// نوع کانال مورد استفاده
        /// </summary>
        [Required]
        public NotificationChannelType ChannelType { get; set; }

        /// <summary>
        /// متن الگو برای زبان فارسی
        /// </summary>
        [Required, MaxLength(1000)]
        public string PersianTemplate { get; set; }

        /// <summary>
        /// متن الگو برای زبان انگلیسی
        /// </summary>
        [MaxLength(1000)]
        public string EnglishTemplate { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن الگو
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        // پیاده‌سازی ITrackable
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(128)]
        public string CreatedByUserId { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(128)]
        public string UpdatedByUserId { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }
    }
    #endregion

    #region Configurations
    /// <summary>
    /// پیکربندی مدل تاریخچه اطلاع‌رسانی برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class NotificationHistoryConfig : EntityTypeConfiguration<NotificationHistory>
    {
        public NotificationHistoryConfig()
        {
            ToTable("NotificationHistories");
            HasKey(h => h.HistoryId);

            // تنظیمات ویژگی‌های اصلی
            Property(h => h.NotificationId).IsRequired();
            Property(h => h.Recipient).IsRequired().HasMaxLength(50);
            Property(h => h.Subject).HasMaxLength(200);
            Property(h => h.Message).IsRequired().HasMaxLength(1000);
            Property(h => h.SentAt).IsRequired();
            Property(h => h.Status).IsRequired();
            Property(h => h.StatusDescription).HasMaxLength(500);
            Property(h => h.SenderUserId).HasMaxLength(128);
            Property(h => h.RelatedEntityId).HasMaxLength(50);
            Property(h => h.RelatedEntityType).HasMaxLength(100);
            Property(h => h.AttemptCount).IsRequired();
            Property(h => h.ExternalMessageId).HasMaxLength(100);

            // تنظیمات Soft Delete
            Property(h => h.IsDeleted).IsRequired();
            Property(h => h.DeletedAt).IsOptional();
            Property(h => h.DeletedByUserId).HasMaxLength(128);

            // تنظیمات ردیابی
            Property(h => h.CreatedAt).IsRequired();
            Property(h => h.CreatedByUserId).HasMaxLength(128);
            Property(h => h.UpdatedAt).IsOptional();
            Property(h => h.UpdatedByUserId).HasMaxLength(128);

            // ایجاد ایندکس‌ها برای بهینه‌سازی پرس‌وجوها
            HasIndex(h => h.SentAt).IsClustered(false).HasName("IX_NotificationHistory_SentAt");
            HasIndex(h => h.Status).IsClustered(false).HasName("IX_NotificationHistory_Status");
            HasIndex(h => h.Recipient).IsClustered(false).HasName("IX_NotificationHistory_Recipient");
            HasIndex(h => h.RelatedEntityType).IsClustered(false).HasName("IX_NotificationHistory_RelatedEntityType");
            HasIndex(h => h.IsDeleted).IsClustered(false).HasName("IX_NotificationHistory_IsDeleted");
            HasIndex(h => h.CreatedByUserId).IsClustered(false).HasName("IX_NotificationHistory_CreatedByUserId");
            HasIndex(h => h.UpdatedByUserId).IsClustered(false).HasName("IX_NotificationHistory_UpdatedByUserId");

            // ایجاد ایندکس‌های ترکیبی برای پرس‌وجوهای رایج
            HasIndex(h => new { h.SentAt, h.Status, h.IsDeleted })
                .IsClustered(false)
                .HasName("IX_NotificationHistory_SentAt_Status_IsDeleted");

            HasIndex(h => new { h.Recipient, h.Status, h.SentAt })
                .IsClustered(false)
                .HasName("IX_NotificationHistory_Recipient_Status_SentAt");

            // تنظیمات روابط
            HasOptional(h => h.SenderUser)
                .WithMany()
                .HasForeignKey(h => h.SenderUserId)
                .WillCascadeOnDelete(false);

            HasOptional(h => h.DeletedByUser)
                .WithMany()
                .HasForeignKey(h => h.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(h => h.CreatedByUser)
                .WithMany()
                .HasForeignKey(h => h.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(h => h.UpdatedByUser)
                .WithMany()
                .HasForeignKey(h => h.UpdatedByUserId)
                .WillCascadeOnDelete(false);
        }
    }

    /// <summary>
    /// پیکربندی مدل الگوی اطلاع‌رسانی برای Entity Framework
    /// </summary>
    public class NotificationTemplateConfig : EntityTypeConfiguration<NotificationTemplate>
    {
        public NotificationTemplateConfig()
        {
            ToTable("NotificationTemplates");
            HasKey(t => t.Key);

            // تنظیمات ویژگی‌های اصلی
            Property(t => t.Key).IsRequired().HasMaxLength(50);
            Property(t => t.Title).IsRequired().HasMaxLength(200);
            Property(t => t.Description).HasMaxLength(500);
            Property(t => t.ChannelType).IsRequired();
            Property(t => t.PersianTemplate).IsRequired().HasMaxLength(1000);
            Property(t => t.EnglishTemplate).HasMaxLength(1000);
            Property(t => t.IsActive).IsRequired();

            // تنظیمات ردیابی
            Property(t => t.CreatedAt).IsRequired();
            Property(t => t.CreatedByUserId).HasMaxLength(128);
            Property(t => t.UpdatedAt).IsOptional();
            Property(t => t.UpdatedByUserId).HasMaxLength(128);

            // ایجاد ایندکس‌ها برای بهینه‌سازی
            HasIndex(t => t.ChannelType).IsClustered(false).HasName("IX_NotificationTemplate_ChannelType");
            HasIndex(t => t.IsActive).IsClustered(false).HasName("IX_NotificationTemplate_IsActive");
            HasIndex(t => t.CreatedByUserId).IsClustered(false).HasName("IX_NotificationTemplate_CreatedByUserId");
            HasIndex(t => t.UpdatedByUserId).IsClustered(false).HasName("IX_NotificationTemplate_UpdatedByUserId");

            // تنظیمات روابط
            HasOptional(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.UpdatedByUser)
                .WithMany()
                .HasForeignKey(t => t.UpdatedByUserId)
                .WillCascadeOnDelete(false);
        }
    }
    #endregion

    #region Constants
    /// <summary>
    /// کلیدهای ثابت برای الگوهای اطلاع‌رسانی در سیستم کلینیک
    /// </summary>
    public static class NotificationTemplates
    {
        /// <summary>
        /// الگوی ثبت‌نام بیمار جدید
        /// </summary>
        public const string Registration = "Patient_Registration";

        /// <summary>
        /// الگوی تأیید نوبت
        /// </summary>
        public const string AppointmentConfirmation = "Appointment_Confirmation";

        /// <summary>
        /// الگوی یادآوری نوبت
        /// </summary>
        public const string AppointmentReminder = "Appointment_Reminder";

        /// <summary>
        /// الگوی تبریک تولد
        /// </summary>
        public const string BirthdayWish = "Birthday_Wish";

        /// <summary>
        /// الگوی تأیید پرداخت
        /// </summary>
        public const string PaymentConfirmation = "Payment_Confirmation";

        /// <summary>
        /// الگوی تبلیغات خدمات
        /// </summary>
        public const string ServicePromotion = "Service_Promotion";

        /// <summary>
        /// الگوی تغییر برنامه کاری پزشک
        /// </summary>
        public const string DoctorScheduleChange = "Doctor_Schedule_Change";
    }
    #endregion
}