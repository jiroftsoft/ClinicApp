using ClinicApp.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Doctor;

/// <summary>
/// مدل روز کاری پزشک - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت روزهای کاری هفتگی پزشکان
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
/// </summary>
public class DoctorWorkDay : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه روز کاری
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int WorkDayId { get; set; }

    /// <summary>
    /// شناسه برنامه کاری پزشک
    /// </summary>
    [Required(ErrorMessage = "برنامه کاری الزامی است.")]
    public int ScheduleId { get; set; }

    /// <summary>
    /// شماره روز هفته (0 = یکشنبه، 1 = دوشنبه، ...، 6 = شنبه)
    /// </summary>
    [Range(0, 6, ErrorMessage = "شماره روز هفته باید بین 0 تا 6 باشد.")]
    public int DayOfWeek { get; set; }

    /// <summary>
    /// نشان‌دهنده فعال بودن روز کاری
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
    public virtual DoctorSchedule Schedule { get; set; }
    public virtual ApplicationUser CreatedByUser { get; set; }
    public virtual ApplicationUser UpdatedByUser { get; set; }
    public virtual ApplicationUser DeletedByUser { get; set; }
    public virtual ICollection<DoctorTimeRange> TimeRanges { get; set; } = new List<DoctorTimeRange>();
}

/// <summary>
/// کانفیگ Entity Framework برای مدل DoctorWorkDay
/// </summary>
public class DoctorWorkDayConfiguration : EntityTypeConfiguration<DoctorWorkDay>
{
    public DoctorWorkDayConfiguration()
    {
        // نام جدول
        ToTable("DoctorWorkDays");

        // کلید اصلی
        HasKey(wd => wd.WorkDayId);

        // پراپرتی‌های اصلی
        Property(wd => wd.WorkDayId)
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_WorkDayId")));

        Property(wd => wd.ScheduleId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_ScheduleId")));

        Property(wd => wd.DayOfWeek)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_DayOfWeek")));

        Property(wd => wd.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_IsActive")));

        // پیاده‌سازی ISoftDelete
        Property(wd => wd.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_IsDeleted")));

        Property(wd => wd.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_DeletedAt")));

        Property(wd => wd.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_DeletedByUserId")));

        // پیاده‌سازی ITrackable
        Property(wd => wd.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_CreatedAt")));

        Property(wd => wd.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_CreatedByUserId")));

        Property(wd => wd.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_UpdatedAt")));

        Property(wd => wd.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_UpdatedByUserId")));

        // روابط
        HasRequired(wd => wd.Schedule)
            .WithMany(ds => ds.WorkDays)
            .HasForeignKey(wd => wd.ScheduleId)
            .WillCascadeOnDelete(true); // حذف cascade با Schedule

        HasOptional(wd => wd.CreatedByUser)
            .WithMany()
            .HasForeignKey(wd => wd.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(wd => wd.UpdatedByUser)
            .WithMany()
            .HasForeignKey(wd => wd.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(wd => wd.DeletedByUser)
            .WithMany()
            .HasForeignKey(wd => wd.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // روابط با TimeRanges
        HasMany(wd => wd.TimeRanges)
            .WithRequired(tr => tr.WorkDay)
            .HasForeignKey(tr => tr.WorkDayId)
            .WillCascadeOnDelete(true); // حذف cascade برای TimeRanges

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        HasIndex(wd => new { wd.ScheduleId, wd.DayOfWeek })
            .HasName("IX_DoctorWorkDay_ScheduleId_DayOfWeek");

        HasIndex(wd => new { wd.ScheduleId, wd.IsActive })
            .HasName("IX_DoctorWorkDay_ScheduleId_IsActive");

        HasIndex(wd => new { wd.ScheduleId, wd.IsDeleted })
            .HasName("IX_DoctorWorkDay_ScheduleId_IsDeleted");

        HasIndex(wd => new { wd.CreatedAt, wd.IsDeleted })
            .HasName("IX_DoctorWorkDay_CreatedAt_IsDeleted");

        // ایندکس منحصر به فرد برای هر برنامه کاری فقط یک روز کاری برای هر روز هفته
        HasIndex(wd => new { wd.ScheduleId, wd.DayOfWeek, wd.IsDeleted })
            .HasName("IX_DoctorWorkDay_ScheduleId_DayOfWeek_IsDeleted_Unique")
            .IsUnique();
    }
}