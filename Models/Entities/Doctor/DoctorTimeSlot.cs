using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Doctor;

/// <summary>
/// مدل اسلات زمانی نوبت‌دهی پزشک - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت اسلات‌های زمانی قابل رزرو برای نوبت‌دهی
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
/// 5. پشتیبانی از وضعیت‌های مختلف نوبت (در دسترس، رزرو شده، تکمیل شده)
/// </summary>
public class DoctorTimeSlot : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه اسلات زمانی
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int TimeSlotId { get; set; }

    /// <summary>
    /// شناسه پزشک
    /// </summary>
    [Required(ErrorMessage = "پزشک الزامی است.")]
    public int DoctorId { get; set; }

    /// <summary>
    /// تاریخ نوبت
    /// </summary>
    [Required(ErrorMessage = "تاریخ نوبت الزامی است.")]
    public DateTime AppointmentDate { get; set; }

    /// <summary>
    /// زمان شروع اسلات
    /// </summary>
    [Required(ErrorMessage = "زمان شروع الزامی است.")]
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// زمان پایان اسلات
    /// </summary>
    [Required(ErrorMessage = "زمان پایان الزامی است.")]
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// مدت زمان اسلات (به دقیقه)
    /// </summary>
    [Range(5, 120, ErrorMessage = "مدت زمان اسلات باید بین 5 تا 120 دقیقه باشد.")]
    public int Duration { get; set; }

    /// <summary>
    /// وضعیت اسلات (Available, Booked, Completed, Cancelled, NoShow)
    /// </summary>
    [Required(ErrorMessage = "وضعیت اسلات الزامی است.")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Available;

    /// <summary>
    /// شناسه نوبت (در صورت رزرو شده بودن)
    /// </summary>
    public int? AppointmentId { get; set; }

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
    public virtual Doctor Doctor { get; set; }
    public virtual Appointment.Appointment Appointment { get; set; }
    public virtual ApplicationUser CreatedByUser { get; set; }
    public virtual ApplicationUser UpdatedByUser { get; set; }
    public virtual ApplicationUser DeletedByUser { get; set; }
}

/// <summary>
/// کانفیگ Entity Framework برای مدل DoctorTimeSlot
/// </summary>
public class DoctorTimeSlotConfiguration : EntityTypeConfiguration<DoctorTimeSlot>
{
    public DoctorTimeSlotConfiguration()
    {
        // نام جدول
        ToTable("DoctorTimeSlots");

        // کلید اصلی
        HasKey(ts => ts.TimeSlotId);

        // پراپرتی‌های اصلی
        Property(ts => ts.TimeSlotId)
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_TimeSlotId")));

        Property(ts => ts.DoctorId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_DoctorId")));

        Property(ts => ts.AppointmentDate)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_AppointmentDate")));

        Property(ts => ts.StartTime)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_StartTime")));

        Property(ts => ts.EndTime)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_EndTime")));

        Property(ts => ts.Duration)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_Duration")));

        Property(ts => ts.Status)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_Status")));

        Property(ts => ts.AppointmentId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_AppointmentId")));

        // پیاده‌سازی ISoftDelete
        Property(ts => ts.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_IsDeleted")));

        Property(ts => ts.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_DeletedAt")));

        Property(ts => ts.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_DeletedByUserId")));

        // پیاده‌سازی ITrackable
        Property(ts => ts.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_CreatedAt")));

        Property(ts => ts.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_CreatedByUserId")));

        Property(ts => ts.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_UpdatedAt")));

        Property(ts => ts.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_UpdatedByUserId")));

        // روابط
        HasRequired(ts => ts.Doctor)
            .WithMany(d => d.TimeSlots)
            .HasForeignKey(ts => ts.DoctorId)
            .WillCascadeOnDelete(false); // حتماً false برای رعایت استانداردهای پزشکی

        HasOptional(ts => ts.Appointment)
            .WithMany()
            .HasForeignKey(ts => ts.AppointmentId)
            .WillCascadeOnDelete(false);

        HasOptional(ts => ts.CreatedByUser)
            .WithMany()
            .HasForeignKey(ts => ts.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(ts => ts.UpdatedByUser)
            .WithMany()
            .HasForeignKey(ts => ts.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(ts => ts.DeletedByUser)
            .WithMany()
            .HasForeignKey(ts => ts.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        HasIndex(ts => new { ts.DoctorId, ts.AppointmentDate })
            .HasName("IX_DoctorTimeSlot_DoctorId_AppointmentDate");

        HasIndex(ts => new { ts.DoctorId, ts.AppointmentDate, ts.Status })
            .HasName("IX_DoctorTimeSlot_DoctorId_AppointmentDate_Status");

        HasIndex(ts => new { ts.DoctorId, ts.IsDeleted })
            .HasName("IX_DoctorTimeSlot_DoctorId_IsDeleted");

        HasIndex(ts => new { ts.AppointmentDate, ts.Status, ts.IsDeleted })
            .HasName("IX_DoctorTimeSlot_AppointmentDate_Status_IsDeleted");

        HasIndex(ts => new { ts.CreatedAt, ts.IsDeleted })
            .HasName("IX_DoctorTimeSlot_CreatedAt_IsDeleted");

        // ایندکس برای جستجوی اسلات‌های خالی
        HasIndex(ts => new { ts.DoctorId, ts.AppointmentDate, ts.StartTime, ts.Status })
            .HasName("IX_DoctorTimeSlot_DoctorId_AppointmentDate_StartTime_Status");

        // ایندکس منحصر به فرد برای جلوگیری از تداخل زمانی
        HasIndex(ts => new { ts.DoctorId, ts.AppointmentDate, ts.StartTime, ts.EndTime, ts.IsDeleted })
            .HasName("IX_DoctorTimeSlot_DoctorId_AppointmentDate_StartTime_EndTime_IsDeleted_Unique")
            .IsUnique();
    }
}
