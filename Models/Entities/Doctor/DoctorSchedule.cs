using ClinicApp.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Appointment;

namespace ClinicApp.Models.Entities.Doctor;

/// <summary>
/// مدل برنامه کاری پزشک - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت برنامه کاری هفتگی پزشکان
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
/// 5. پشتیبانی از برنامه‌ریزی نوبت‌دهی هوشمند
/// </summary>
public class DoctorSchedule : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه برنامه کاری
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int ScheduleId { get; set; }

    /// <summary>
    /// شناسه پزشک
    /// </summary>
    [Required(ErrorMessage = "پزشک الزامی است.")]
    public int DoctorId { get; set; }

    /// <summary>
    /// مدت زمان هر نوبت (به دقیقه)
    /// </summary>
    [Range(5, 120, ErrorMessage = "مدت زمان نوبت باید بین 5 تا 120 دقیقه باشد.")]
    public int AppointmentDuration { get; set; } = 30;

    /// <summary>
    /// زمان شروع پیش‌فرض روز کاری
    /// </summary>
    public TimeSpan? DefaultStartTime { get; set; }

    /// <summary>
    /// زمان پایان پیش‌فرض روز کاری
    /// </summary>
    public TimeSpan? DefaultEndTime { get; set; }

    /// <summary>
    /// وضعیت فعال/غیرفعال بودن برنامه کاری
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ========== ویژگی‌های جدید برای برنامه‌ریزی پیشرفته ==========

    /// <summary>
    /// حداکثر تعداد نوبت در روز
    /// </summary>
    [Range(1, 200, ErrorMessage = "حداکثر نوبت در روز باید بین 1 تا 200 باشد.")]
    public int MaxAppointmentsPerDay { get; set; } = 50;

    /// <summary>
    /// حداقل روزهای پیش‌رزرو (0 = همان روز)
    /// </summary>
    [Range(0, 365, ErrorMessage = "حداقل روزهای پیش‌رزرو باید بین 0 تا 365 باشد.")]
    public int MinAdvanceBookingDays { get; set; } = 1;

    /// <summary>
    /// حداکثر روزهای پیش‌رزرو
    /// </summary>
    [Range(1, 365, ErrorMessage = "حداکثر روزهای پیش‌رزرو باید بین 1 تا 365 باشد.")]
    public int MaxAdvanceBookingDays { get; set; } = 90;

    /// <summary>
    /// امکان رزرو همان روز
    /// </summary>
    public bool AllowSameDayBooking { get; set; } = true;

    /// <summary>
    /// نیاز به ثبت‌نام بیمار
    /// </summary>
    public bool RequirePatientRegistration { get; set; } = false;

    /// <summary>
    /// هزینه ویزیت پایه (ریال)
    /// </summary>
    [Range(0, 10000000, ErrorMessage = "هزینه ویزیت باید بین 0 تا 10,000,000 ریال باشد.")]
    public decimal ConsultationFee { get; set; } = 0;

    /// <summary>
    /// هزینه لغو نوبت (ریال)
    /// </summary>
    [Range(0, 1000000, ErrorMessage = "هزینه لغو باید بین 0 تا 1,000,000 ریال باشد.")]
    public decimal CancellationFee { get; set; } = 0;

    /// <summary>
    /// ساعت‌های اخطار لغو (0 = بدون اخطار)
    /// </summary>
    [Range(0, 168, ErrorMessage = "ساعت‌های اخطار لغو باید بین 0 تا 168 باشد.")]
    public int CancellationNoticeHours { get; set; } = 24;

    /// <summary>
    /// امکان رزرو اورژانس
    /// </summary>
    public bool AllowEmergencyBooking { get; set; } = true;

    /// <summary>
    /// امکان مراجعه بدون رزرو
    /// </summary>
    public bool AllowWalkInPatients { get; set; } = false;

    /// <summary>
    /// حداکثر تعداد بیماران بدون رزرو در روز
    /// </summary>
    [Range(0, 50, ErrorMessage = "حداکثر بیماران بدون رزرو باید بین 0 تا 50 باشد.")]
    public int MaxWalkInPatientsPerDay { get; set; } = 5;

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
    public virtual ApplicationUser CreatedByUser { get; set; }
    public virtual ApplicationUser UpdatedByUser { get; set; }
    public virtual ApplicationUser DeletedByUser { get; set; }
    public virtual ICollection<DoctorWorkDay> WorkDays { get; set; } = new List<DoctorWorkDay>();

    // ========== Navigation Properties جدید برای برنامه‌ریزی پیشرفته ==========

    /// <summary>
    /// استثناهای برنامه کاری (تعطیلات، روزهای خاص)
    /// </summary>
    public virtual ICollection<ScheduleException> Exceptions { get; set; } = new List<ScheduleException>();

    /// <summary>
    /// قالب‌های برنامه کاری قابل استفاده مجدد
    /// </summary>
    public virtual ICollection<ScheduleTemplate> Templates { get; set; } = new List<ScheduleTemplate>();

    /// <summary>
    /// اسلات‌های نوبت تولید شده
    /// </summary>
    public virtual ICollection<AppointmentSlot> AppointmentSlots { get; set; } = new List<AppointmentSlot>();
}

/// <summary>
/// کانفیگ Entity Framework برای مدل DoctorSchedule
/// </summary>
public class DoctorScheduleConfiguration : EntityTypeConfiguration<DoctorSchedule>
{
    public DoctorScheduleConfiguration()
    {
        // نام جدول
        ToTable("DoctorSchedules");

        // کلید اصلی
        HasKey(ds => ds.ScheduleId);

        // پراپرتی‌های اصلی
        Property(ds => ds.ScheduleId)
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_ScheduleId")));

        Property(ds => ds.DoctorId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_DoctorId")));

        Property(ds => ds.AppointmentDuration)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_AppointmentDuration")));

        Property(ds => ds.DefaultStartTime)
            .IsOptional();

        Property(ds => ds.DefaultEndTime)
            .IsOptional();

        Property(ds => ds.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_IsActive")));

        // پیاده‌سازی ISoftDelete
        Property(ds => ds.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_IsDeleted")));

        Property(ds => ds.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_DeletedAt")));

        Property(ds => ds.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_DeletedByUserId")));

        // پیاده‌سازی ITrackable
        Property(ds => ds.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_CreatedAt")));

        Property(ds => ds.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_CreatedByUserId")));

        Property(ds => ds.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_UpdatedAt")));

        Property(ds => ds.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_UpdatedByUserId")));

        // روابط
        HasRequired(ds => ds.Doctor)
            .WithMany(d => d.Schedules)
            .HasForeignKey(ds => ds.DoctorId)
            .WillCascadeOnDelete(false); // حتماً false برای رعایت استانداردهای پزشکی

        HasOptional(ds => ds.CreatedByUser)
            .WithMany()
            .HasForeignKey(ds => ds.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(ds => ds.UpdatedByUser)
            .WithMany()
            .HasForeignKey(ds => ds.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(ds => ds.DeletedByUser)
            .WithMany()
            .HasForeignKey(ds => ds.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // روابط جدید برای برنامه‌ریزی پیشرفته
        HasMany(ds => ds.Exceptions)
            .WithRequired(se => se.Schedule)
            .HasForeignKey(se => se.ScheduleId)
            .WillCascadeOnDelete(true); // حذف cascade برای استثناها

        HasMany(ds => ds.Templates)
            .WithRequired(st => st.Schedule)
            .HasForeignKey(st => st.ScheduleId)
            .WillCascadeOnDelete(true); // حذف cascade برای قالب‌ها

        HasMany(ds => ds.AppointmentSlots)
            .WithRequired(aps => aps.Schedule)
            .HasForeignKey(aps => aps.ScheduleId)
            .WillCascadeOnDelete(true); // حذف cascade برای اسلات‌ها

        // روابط با WorkDays
        HasMany(ds => ds.WorkDays)
            .WithRequired(wd => wd.Schedule)
            .HasForeignKey(wd => wd.ScheduleId)
            .WillCascadeOnDelete(true); // حذف cascade برای WorkDays

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        HasIndex(ds => new { ds.DoctorId, ds.IsActive })
            .HasName("IX_DoctorSchedule_DoctorId_IsActive");

        HasIndex(ds => new { ds.DoctorId, ds.IsDeleted })
            .HasName("IX_DoctorSchedule_DoctorId_IsDeleted");

        HasIndex(ds => new { ds.CreatedAt, ds.IsDeleted })
            .HasName("IX_DoctorSchedule_CreatedAt_IsDeleted");

        // ایندکس منحصر به فرد برای هر پزشک فقط یک برنامه کاری فعال
        HasIndex(ds => new { ds.DoctorId, ds.IsActive, ds.IsDeleted })
            .HasName("IX_DoctorSchedule_DoctorId_IsActive_IsDeleted_Unique")
            .IsUnique();
    }
}
