using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.Entities.Notification;

/// <summary>
/// آیتم صف اعلان — برای ارسال فوری یا زمان‌بندی‌شده (یادآوری)
/// Production-safe: IdempotencyKey، Retry، ErrorLog
/// </summary>
public class NotificationQueueItem
{
    [Key]
    public long Id { get; set; }

    [MaxLength(128)]
    public string UserId { get; set; }

    public int? PatientId { get; set; }

    public int? AppointmentId { get; set; }

    [Required]
    public NotificationType NotificationType { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; }

    [Required, MaxLength(2000)]
    public string Message { get; set; }

    [Required]
    public NotificationChannelType Channel { get; set; }

    [Required]
    public NotificationStatus Status { get; set; } = NotificationStatus.Queued;

    public int RetryCount { get; set; }

    [Required]
    public int MaxRetries { get; set; } = 3;

    /// <summary>زمان ارسال برنامه‌ریزی‌شده (برای یادآوری)</summary>
    public DateTime? ScheduledTime { get; set; }

    public DateTime? SentTime { get; set; }

    [MaxLength(2000)]
    public string ErrorLog { get; set; }

    /// <summary>کلید یکتا برای جلوگیری از ارسال تکراری (Idempotency)</summary>
    [Required, MaxLength(256)]
    public string IdempotencyKey { get; set; }

    [Required, MaxLength(100)]
    public string Recipient { get; set; }

    [MaxLength(500)]
    public string Subject { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class NotificationQueueItemConfig : EntityTypeConfiguration<NotificationQueueItem>
{
    public NotificationQueueItemConfig()
    {
        ToTable("NotificationQueue");
        HasKey(e => e.Id);

        Property(e => e.IdempotencyKey).IsRequired().HasMaxLength(256);
        Property(e => e.AppointmentId).IsOptional();
        Property(e => e.PatientId).IsOptional();
        Property(e => e.UserId).HasMaxLength(128);
        Property(e => e.Recipient).IsRequired().HasMaxLength(100);
        Property(e => e.Subject).HasMaxLength(500);

        HasIndex(e => e.Status).HasName("IX_NotificationQueue_Status");
        HasIndex(e => e.ScheduledTime).HasName("IX_NotificationQueue_ScheduledTime");
        HasIndex(e => e.IdempotencyKey).HasName("IX_NotificationQueue_IdempotencyKey");
        HasIndex(e => new { e.AppointmentId, e.NotificationType, e.Channel }).HasName("IX_NotificationQueue_Appointment_Type_Channel");
    }
}
