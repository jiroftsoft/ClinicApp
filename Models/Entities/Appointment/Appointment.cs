using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Payment;

namespace ClinicApp.Models.Entities.Appointment;

/// <summary>
/// مدل نوبت‌های پزشکی - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت صحیح نوبت‌های پزشکی با توجه به استانداردهای سیستم‌های درمانی
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
/// 5. پشتیبانی از سیستم پرداخت و یکپارچه‌سازی با تراکنش‌های مالی
/// </summary>
public class Appointment : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه نوبت پزشکی
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int AppointmentId { get; set; }

    /// <summary>
    /// شناسه پزشک
    /// </summary>
    [Required(ErrorMessage = "پزشک الزامی است.")]
    public int DoctorId { get; set; }

    /// <summary>
    /// شناسه بیمار (اختیاری - ممکن است نوبت توسط منشی ثبت شود)
    /// </summary>
    public int? PatientId { get; set; }

    /// <summary>
    /// تاریخ و زمان نوبت
    /// </summary>
    [Required(ErrorMessage = "تاریخ نوبت الزامی است.")]
    public DateTime AppointmentDate { get; set; }

    /// <summary>
    /// وضعیت نوبت (Scheduled, Cancelled, InProgress, NeedsAdditionalPayment)
    /// </summary>
    [Required(ErrorMessage = "وضعیت نوبت الزامی است.")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    /// <summary>
    /// مبلغ نوبت
    /// </summary>
    [Required(ErrorMessage = "مبلغ نوبت الزامی است.")]
    [DataType(DataType.Currency, ErrorMessage = "فرمت مبلغ نامعتبر است.")]
    [Column(TypeName = "decimal")]
    public decimal Price { get; set; }

    /// <summary>
    /// شناسه تراکنش پرداخت
    /// </summary>
    public int? PaymentTransactionId { get; set; }

    /// <summary>
    /// توضیحات نوبت
    /// </summary>
    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// آیا بیمار جدید است؟
    /// </summary>
    public bool IsNewPatient { get; set; } = false;

    /// <summary>
    /// نام بیمار (برای نوبت‌های بدون ثبت نام)
    /// </summary>
    [MaxLength(200, ErrorMessage = "نام بیمار نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string PatientName { get; set; }

    /// <summary>
    /// شماره تلفن بیمار (برای نوبت‌های بدون ثبت نام)
    /// </summary>
    [MaxLength(20, ErrorMessage = "شماره تلفن بیمار نمی‌تواند بیش از 20 کاراکتر باشد.")]
    public string PatientPhone { get; set; }

    /// <summary>
    /// شناسه دسته‌بندی خدمت
    /// </summary>
    public int? ServiceCategoryId { get; set; }

    /// <summary>
    /// مدت زمان ویزیت (به دقیقه)
    /// </summary>
    [Range(5, 480, ErrorMessage = "مدت زمان ویزیت باید بین 5 تا 480 دقیقه باشد.")]
    public int Duration { get; set; } = 30;

    /// <summary>
    /// اولویت نوبت
    /// </summary>
    [Required(ErrorMessage = "اولویت نوبت الزامی است.")]
    public AppointmentPriority Priority { get; set; } = AppointmentPriority.Normal;

    /// <summary>
    /// آیا نوبت اورژانس است؟
    /// </summary>
    public bool IsEmergency { get; set; } = false;

    /// <summary>
    /// آیا نوبت به صورت آنلاین رزرو شده است؟
    /// </summary>
    public bool IsOnlineBooking { get; set; } = false;

    /// <summary>
    /// راه‌حل تداخل نوبت (در صورت وجود)
    /// </summary>
    [MaxLength(500, ErrorMessage = "راه‌حل تداخل نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string ConflictResolution { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن نوبت
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// تاریخ و زمان حذف نوبت
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که نوبت را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد نوبت
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که نوبت را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش نوبت
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که نوبت را ویرایش کرده است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    #region روابط
    /// <summary>
    /// ارجاع به پزشک
    /// </summary>
    public virtual Doctor.Doctor Doctor { get; set; }

    /// <summary>
    /// ارجاع به بیمار
    /// </summary>
    public virtual Patient.Patient Patient { get; set; }

    /// <summary>
    /// ارجاع به تراکنش پرداخت
    /// </summary>
    public virtual PaymentTransaction PaymentTransaction { get; set; }
    public virtual ICollection<InsuranceCalculation> InsuranceCalculations { get; set; } = new HashSet<InsuranceCalculation>();

    /// <summary>
    /// ارجاع به دسته‌بندی خدمت
    /// </summary>
    public virtual ServiceCategory ServiceCategory { get; set; }
    #endregion
}
/// <summary>
/// پیکربندی مدل نوبت‌های پزشکی برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class AppointmentConfig : EntityTypeConfiguration<Appointment>
{
    public AppointmentConfig()
    {
        ToTable("Appointments");
        HasKey(a => a.AppointmentId);

        // ویژگی‌های اصلی
        Property(a => a.AppointmentDate)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Appointment_AppointmentDate")));

        Property(a => a.Status)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Appointment_Status")));

        Property(a => a.Price)
            .IsRequired()
            .HasPrecision(18, 4);

        Property(a => a.Description)
            .IsOptional()
            .HasMaxLength(500);

        // پیاده‌سازی ISoftDelete
        Property(a => a.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Appointment_IsDeleted")));

        Property(a => a.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Appointment_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(a => a.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Appointment_CreatedAt")));

        Property(a => a.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Appointment_CreatedByUserId")));

        Property(a => a.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Appointment_UpdatedAt")));

        Property(a => a.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Appointment_UpdatedByUserId")));

        Property(a => a.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Appointment_DeletedByUserId")));

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        Property(a => a.DoctorId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Appointment_DoctorId")));

        Property(a => a.PatientId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Appointment_PatientId")));

        Property(a => a.PaymentTransactionId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Appointment_PaymentTransactionId")));

        // روابط
        HasRequired(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .WillCascadeOnDelete(false);

        HasOptional(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .WillCascadeOnDelete(false);

        HasOptional(a => a.PaymentTransaction)
            .WithMany()
            .HasForeignKey(a => a.PaymentTransactionId)
            .WillCascadeOnDelete(false);

        HasOptional(a => a.DeletedByUser)
            .WithMany()
            .HasForeignKey(a => a.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(a => a.CreatedByUser)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(a => a.UpdatedByUser)
            .WithMany()
            .HasForeignKey(a => a.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
        HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.Status })
            .HasName("IX_Appointment_DoctorId_Date_Status");

        HasIndex(a => new { a.PatientId, a.Status, a.AppointmentDate })
            .HasName("IX_Appointment_PatientId_Status_Date");
    }
}
