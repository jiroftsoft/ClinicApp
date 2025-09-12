using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Doctor;

/// <summary>
/// موجودیت استثناهای برنامه کاری برای مدیریت تعطیلات و روزهای خاص
/// این موجودیت امکان تعریف روزهای تعطیل، مرخصی و تغییرات موقت را فراهم می‌کند
/// </summary>
public class ScheduleException : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه استثنا
    /// </summary>
    public int ExceptionId { get; set; }

    /// <summary>
    /// شناسه برنامه کاری پزشک
    /// </summary>
    [Required(ErrorMessage = "برنامه کاری الزامی است.")]
    public int ScheduleId { get; set; }

    /// <summary>
    /// نوع استثنا
    /// </summary>
    [Required(ErrorMessage = "نوع استثنا الزامی است.")]
    public ExceptionType Type { get; set; }

    /// <summary>
    /// تاریخ شروع استثنا
    /// </summary>
    [Required(ErrorMessage = "تاریخ شروع الزامی است.")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// تاریخ پایان استثنا (اختیاری - برای استثناهای یک روزه)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// زمان شروع (اختیاری - برای استثناهای جزئی روز)
    /// </summary>
    public TimeSpan? StartTime { get; set; }

    /// <summary>
    /// زمان پایان (اختیاری - برای استثناهای جزئی روز)
    /// </summary>
    public TimeSpan? EndTime { get; set; }

    /// <summary>
    /// دلیل استثنا
    /// </summary>
    [Required(ErrorMessage = "دلیل استثنا الزامی است.")]
    [MaxLength(200, ErrorMessage = "دلیل استثنا نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string Reason { get; set; }

    /// <summary>
    /// توضیحات اضافی
    /// </summary>
    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// آیا استثنا تکرار می‌شود؟
    /// </summary>
    public bool IsRecurring { get; set; } = false;

    /// <summary>
    /// الگوی تکرار (اختیاری)
    /// </summary>
    [MaxLength(100, ErrorMessage = "الگوی تکرار نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string RecurrencePattern { get; set; }

    #region پیاده‌سازی ISoftDelete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    [MaxLength(128)]
    public string DeletedByUserId { get; set; }
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [MaxLength(128)]
    public string CreatedByUserId { get; set; }
    public virtual ApplicationUser CreatedByUser { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [MaxLength(128)]
    public string UpdatedByUserId { get; set; }
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    // Navigation Properties
    public virtual DoctorSchedule Schedule { get; set; }
    public bool IsActive { get; set; }
}
/// <summary>
/// کانفیگ Entity Framework برای مدل ScheduleException
/// </summary>
public class ScheduleExceptionConfiguration : EntityTypeConfiguration<ScheduleException>
{
    public ScheduleExceptionConfiguration()
    {
        // نام جدول
        ToTable("ScheduleExceptions");

        // کلید اصلی
        HasKey(se => se.ExceptionId);

        // پراپرتی‌های اصلی
        Property(se => se.ExceptionId)
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        Property(se => se.ScheduleId)
            .IsRequired();

        Property(se => se.Type)
            .IsRequired();

        Property(se => se.StartDate)
            .IsRequired();

        Property(se => se.EndDate)
            .IsOptional();

        Property(se => se.StartTime)
            .IsOptional();

        Property(se => se.EndTime)
            .IsOptional();

        Property(se => se.Reason)
            .IsRequired()
            .HasMaxLength(200);

        Property(se => se.Description)
            .IsOptional()
            .HasMaxLength(500);

        Property(se => se.RecurrencePattern)
            .IsOptional()
            .HasMaxLength(100);

        // پیاده‌سازی ISoftDelete
        Property(se => se.IsDeleted)
            .IsRequired();

        Property(se => se.DeletedAt)
            .IsOptional();

        Property(se => se.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        // پیاده‌سازی ITrackable
        Property(se => se.CreatedAt)
            .IsRequired();

        Property(se => se.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        Property(se => se.UpdatedAt)
            .IsOptional();

        Property(se => se.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        // روابط
        HasRequired(se => se.Schedule)
            .WithMany(ds => ds.Exceptions)
            .HasForeignKey(se => se.ScheduleId)
            .WillCascadeOnDelete(true);

        HasOptional(se => se.CreatedByUser)
            .WithMany()
            .HasForeignKey(se => se.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(se => se.UpdatedByUser)
            .WithMany()
            .HasForeignKey(se => se.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(se => se.DeletedByUser)
            .WithMany()
            .HasForeignKey(se => se.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌ها برای بهبود عملکرد
        HasIndex(se => new { se.ScheduleId, se.StartDate, se.EndDate })
            .HasName("IX_ScheduleException_ScheduleId_DateRange");

        HasIndex(se => new { se.StartDate, se.Type, se.IsDeleted })
            .HasName("IX_ScheduleException_StartDate_Type_IsDeleted");

        HasIndex(se => se.IsDeleted)
            .HasName("IX_ScheduleException_IsDeleted");
    }
}