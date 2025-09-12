using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Doctor;

/// <summary>
/// مدل بازه زمانی کاری پزشک - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت بازه‌های زمانی کاری در هر روز
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
/// </summary>
public class DoctorTimeRange : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه بازه زمانی
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int TimeRangeId { get; set; }

    /// <summary>
    /// شناسه روز کاری
    /// </summary>
    [Required(ErrorMessage = "روز کاری الزامی است.")]
    public int WorkDayId { get; set; }

    /// <summary>
    /// زمان شروع بازه
    /// </summary>
    [Required(ErrorMessage = "زمان شروع الزامی است.")]
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// زمان پایان بازه
    /// </summary>
    [Required(ErrorMessage = "زمان پایان الزامی است.")]
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// نشان‌دهنده فعال بودن بازه زمانی
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// وضعیت حذف نرم
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// تاریخ حذف
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربر حذف کننده
    /// </summary>
    [MaxLength(128)]
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// شناسه کاربر ایجاد کننده
    /// </summary>
    [MaxLength(128)]
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// تاریخ آخرین ویرایش
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربر آخرین ویرایش کننده
    /// </summary>
    [MaxLength(128)]
    public string UpdatedByUserId { get; set; }

    // Navigation Properties
    public virtual DoctorWorkDay WorkDay { get; set; }
    public virtual ApplicationUser CreatedByUser { get; set; }
    public virtual ApplicationUser UpdatedByUser { get; set; }
    public virtual ApplicationUser DeletedByUser { get; set; }
}

/// <summary>
/// کانفیگ Entity Framework برای مدل DoctorTimeRange
/// </summary>
public class DoctorTimeRangeConfiguration : EntityTypeConfiguration<DoctorTimeRange>
{
    public DoctorTimeRangeConfiguration()
    {
        // نام جدول
        ToTable("DoctorTimeRanges");

        // کلید اصلی
        HasKey(tr => tr.TimeRangeId);

        // پراپرتی‌های اصلی
        Property(tr => tr.TimeRangeId)
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_TimeRangeId")));

        Property(tr => tr.WorkDayId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_WorkDayId")));

        Property(tr => tr.StartTime)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_StartTime")));

        Property(tr => tr.EndTime)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_EndTime")));

        Property(tr => tr.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_IsActive")));

        // پیاده‌سازی ISoftDelete
        Property(tr => tr.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_IsDeleted")));

        Property(tr => tr.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_DeletedAt")));

        Property(tr => tr.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_DeletedByUserId")));

        // پیاده‌سازی ITrackable
        Property(tr => tr.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_CreatedAt")));

        Property(tr => tr.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_CreatedByUserId")));

        Property(tr => tr.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_UpdatedAt")));

        Property(tr => tr.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_UpdatedByUserId")));

        // روابط
        HasRequired(tr => tr.WorkDay)
            .WithMany(wd => wd.TimeRanges)
            .HasForeignKey(tr => tr.WorkDayId)
            .WillCascadeOnDelete(true); // حذف cascade با WorkDay

        HasOptional(tr => tr.CreatedByUser)
            .WithMany()
            .HasForeignKey(tr => tr.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(tr => tr.UpdatedByUser)
            .WithMany()
            .HasForeignKey(tr => tr.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(tr => tr.DeletedByUser)
            .WithMany()
            .HasForeignKey(tr => tr.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        HasIndex(tr => new { tr.WorkDayId, tr.StartTime })
            .HasName("IX_DoctorTimeRange_WorkDayId_StartTime");

        HasIndex(tr => new { tr.WorkDayId, tr.IsActive })
            .HasName("IX_DoctorTimeRange_WorkDayId_IsActive");

        HasIndex(tr => new { tr.WorkDayId, tr.IsDeleted })
            .HasName("IX_DoctorTimeRange_WorkDayId_IsDeleted");

        HasIndex(tr => new { tr.CreatedAt, tr.IsDeleted })
            .HasName("IX_DoctorTimeRange_CreatedAt_IsDeleted");

        // ایندکس برای جستجوی بازه‌های زمانی همپوشان
        HasIndex(tr => new { tr.WorkDayId, tr.StartTime, tr.EndTime })
            .HasName("IX_DoctorTimeRange_WorkDayId_StartTime_EndTime");
    }
}