using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.Entities.Notification;

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