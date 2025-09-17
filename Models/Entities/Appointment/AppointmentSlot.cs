using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Patient;

namespace ClinicApp.Models.Entities.Appointment;

/// <summary>
/// موجودیت اسلات نوبت برای مدیریت دقیق زمان‌های رزرو
/// این موجودیت امکان مدیریت اسلات‌های فردی را فراهم می‌کند
/// </summary>
public class AppointmentSlot : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه اسلات نوبت
    /// </summary>
    public int SlotId { get; set; }

    /// <summary>
    /// شناسه برنامه کاری پزشک
    /// </summary>
    [Required(ErrorMessage = "برنامه کاری الزامی است.")]
    public int ScheduleId { get; set; }

    /// <summary>
    /// تاریخ اسلات
    /// </summary>
    [Required(ErrorMessage = "تاریخ اسلات الزامی است.")]
    public DateTime SlotDate { get; set; }

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
    /// وضعیت اسلات
    /// </summary>
    [Required(ErrorMessage = "وضعیت اسلات الزامی است.")]
    public AppointmentSlotStatus Status { get; set; } = AppointmentSlotStatus.Available;

    /// <summary>
    /// قیمت اسلات (ریال)
    /// </summary>
    [Range(0, 10000000, ErrorMessage = "قیمت باید بین 0 تا 10,000,000 ریال باشد.")]
    public decimal Price { get; set; }

    /// <summary>
    /// شناسه بیمار (اختیاری)
    /// </summary>
    public int? PatientId { get; set; }

    /// <summary>
    /// شناسه نوبت (اختیاری)
    /// </summary>
    public int? AppointmentId { get; set; }

    /// <summary>
    /// آیا اسلات اورژانس است؟
    /// </summary>
    public bool IsEmergencySlot { get; set; } = false;

    /// <summary>
    /// آیا امکان مراجعه بدون رزرو است؟
    /// </summary>
    public bool IsWalkInAllowed { get; set; } = false;

    /// <summary>
    /// اولویت اسلات (0 = عادی، 1 = بالا، 2 = خیلی بالا)
    /// </summary>
    [Range(0, 2, ErrorMessage = "اولویت باید بین 0 تا 2 باشد.")]
    public int Priority { get; set; } = 0;

    /// <summary>
    /// توضیحات اضافی
    /// </summary>
    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Notes { get; set; }

    /// <summary>
    /// شناسه بیمه فعال بیمار در زمان رزرو (اختیاری)
    /// این فیلد برای ردیابی بیمه‌ای که در زمان رزرو فعال بوده است
    /// </summary>
    public int? ActivePatientInsuranceId { get; set; }

    /// <summary>
    /// ارجاع به بیمه فعال بیمار در زمان رزرو
    /// این ارتباط برای نمایش اطلاعات بیمه‌ای که در زمان رزرو فعال بوده است
    /// </summary>
    public virtual PatientInsurance ActivePatientInsurance { get; set; }

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
    public virtual Patient.Patient Patient { get; set; }
    public virtual Appointment Appointment { get; set; }
    public bool IsActive { get; set; }
}
/// <summary>
/// کانفیگ Entity Framework برای مدل AppointmentSlot
/// </summary>
public class AppointmentSlotConfiguration : EntityTypeConfiguration<AppointmentSlot>
{
    public AppointmentSlotConfiguration()
    {
        // نام جدول
        ToTable("AppointmentSlots");

        // کلید اصلی
        HasKey(aps => aps.SlotId);

        // پراپرتی‌های اصلی
        Property(aps => aps.SlotId)
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        Property(aps => aps.ScheduleId)
            .IsRequired();

        Property(aps => aps.SlotDate)
            .IsRequired();

        Property(aps => aps.StartTime)
            .IsRequired();

        Property(aps => aps.EndTime)
            .IsRequired();

        Property(aps => aps.Status)
            .IsRequired();

        Property(aps => aps.Price)
            .HasPrecision(18, 4);

        Property(aps => aps.Notes)
            .HasMaxLength(500);

        // فیلدهای جدید برای سیستم بیمه جدید
        Property(aps => aps.ActivePatientInsuranceId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_AppointmentSlot_ActivePatientInsuranceId")));

        // پیاده‌سازی ISoftDelete
        Property(aps => aps.IsDeleted)
            .IsRequired();

        Property(aps => aps.DeletedAt)
            .IsOptional();

        Property(aps => aps.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        // پیاده‌سازی ITrackable
        Property(aps => aps.CreatedAt)
            .IsRequired();

        Property(aps => aps.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        Property(aps => aps.UpdatedAt)
            .IsOptional();

        Property(aps => aps.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        // روابط
        HasRequired(aps => aps.Schedule)
            .WithMany(ds => ds.AppointmentSlots)
            .HasForeignKey(aps => aps.ScheduleId)
            .WillCascadeOnDelete(true);

        HasOptional(aps => aps.Patient)
            .WithMany()
            .HasForeignKey(aps => aps.PatientId)
            .WillCascadeOnDelete(false);

        HasOptional(aps => aps.Appointment)
            .WithMany()
            .HasForeignKey(aps => aps.AppointmentId)
            .WillCascadeOnDelete(false);

        // رابطه با سیستم بیمه جدید
        HasOptional(aps => aps.ActivePatientInsurance)
            .WithMany()
            .HasForeignKey(aps => aps.ActivePatientInsuranceId)
            .WillCascadeOnDelete(false);

        HasOptional(aps => aps.CreatedByUser)
            .WithMany()
            .HasForeignKey(aps => aps.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(aps => aps.UpdatedByUser)
            .WithMany()
            .HasForeignKey(aps => aps.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(aps => aps.DeletedByUser)
            .WithMany()
            .HasForeignKey(aps => aps.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌ها برای بهبود عملکرد
        HasIndex(aps => new { aps.ScheduleId, aps.SlotDate, aps.Status })
            .HasName("IX_AppointmentSlot_ScheduleId_Date_Status");

        HasIndex(aps => new { aps.SlotDate, aps.StartTime, aps.Status })
            .HasName("IX_AppointmentSlot_Date_Time_Status");

        HasIndex(aps => new { aps.PatientId, aps.SlotDate })
            .HasName("IX_AppointmentSlot_PatientId_Date");

        HasIndex(aps => aps.IsDeleted)
            .HasName("IX_AppointmentSlot_IsDeleted");
    }
}