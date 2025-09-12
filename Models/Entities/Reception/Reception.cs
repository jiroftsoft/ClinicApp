using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Entities.Receipt;

namespace ClinicApp.Models.Entities.Reception;

/// <summary>
/// مدل پذیرش بیماران - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت صحیح پذیرش بیماران با توجه به استانداردهای سیستم‌های درمانی
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
/// 5. محاسبه دقیق سهم بیمار و بیمه بر اساس سیستم تعرفه‌ها
/// </summary>
public class Reception : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه پذیرش
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int ReceptionId { get; set; }

    /// <summary>
    /// شماره پذیرش (برای سازگاری با ViewModels)
    /// </summary>
    public string ReceptionNumber => $"R{ReceptionId:D6}";

    /// <summary>
    /// شناسه بیمار
    /// </summary>
    [Required(ErrorMessage = "بیمار الزامی است.")]
    public int PatientId { get; set; }

    /// <summary>
    /// شناسه پزشک
    /// </summary>
    [Required(ErrorMessage = "پزشک الزامی است.")]
    public int DoctorId { get; set; }

    // InsuranceId حذف شد - از PatientInsurance استفاده کنید

    /// <summary>
    /// تاریخ پذیرش
    /// </summary>
    [Required(ErrorMessage = "تاریخ پذیرش الزامی است.")]
    public DateTime ReceptionDate { get; set; }

    /// <summary>
    /// جمع کل هزینه‌ها
    /// </summary>
    [Required(ErrorMessage = "جمع کل الزامی است.")]
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// سهم پرداختی بیمار
    /// </summary>
    [Required(ErrorMessage = "سهم پرداختی بیمار الزامی است.")]
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal")]
    public decimal PatientCoPay { get; set; }

    /// <summary>
    /// سهم بیمه
    /// </summary>
    [Required(ErrorMessage = "سهم بیمه الزامی است.")]
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal")]
    public decimal InsurerShareAmount { get; set; }

    /// <summary>
    /// وضعیت پذیرش
    /// </summary>
    public ReceptionStatus Status { get; set; } = ReceptionStatus.Pending;

    /// <summary>
    /// نوع پذیرش
    /// </summary>
    [Required(ErrorMessage = "نوع پذیرش الزامی است.")]
    public ReceptionType Type { get; set; } = ReceptionType.Normal;

    /// <summary>
    /// اولویت پذیرش
    /// </summary>
    [Required(ErrorMessage = "اولویت پذیرش الزامی است.")]
    public AppointmentPriority Priority { get; set; } = AppointmentPriority.Normal;

    /// <summary>
    /// یادداشت‌های پذیرش
    /// </summary>
    [MaxLength(1000, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Notes { get; set; }

    /// <summary>
    /// آیا پذیرش اورژانس است؟
    /// </summary>
    public bool IsEmergency { get; set; } = false;

    /// <summary>
    /// آیا پذیرش به صورت آنلاین انجام شده است؟
    /// </summary>
    public bool IsOnlineReception { get; set; } = false;

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن پذیرش
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف پذیرش
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که پذیرش را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد پذیرش
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که پذیرش را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش پذیرش
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که پذیرش را ویرایش کرده است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    #region روابط
    /// <summary>
    /// ارجاع به بیمار
    /// این ارتباط برای نمایش اطلاعات بیمار در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual Patient.Patient Patient { get; set; }

    /// <summary>
    /// ارجاع به پزشک
    /// این ارتباط برای نمایش اطلاعات پزشک در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual Doctor.Doctor Doctor { get; set; }

    // رابطه با مدل قدیمی Insurance حذف شد

    /// <summary>
    /// شناسه بیمه فعال بیمار در زمان پذیرش
    /// این فیلد برای ردیابی بیمه‌ای که در زمان پذیرش فعال بوده است
    /// </summary>
    public int? ActivePatientInsuranceId { get; set; }

    /// <summary>
    /// ارجاع به بیمه فعال بیمار در زمان پذیرش
    /// این ارتباط برای نمایش اطلاعات بیمه‌ای که در زمان پذیرش فعال بوده است
    /// </summary>
    public virtual PatientInsurance ActivePatientInsurance { get; set; }

    /// <summary>
    /// لیست آیتم‌های پذیرش
    /// این لیست برای نمایش تمام خدمات ارائه شده در این پذیرش استفاده می‌شود
    /// </summary>
    public virtual ICollection<ReceptionItem> ReceptionItems { get; set; } = new HashSet<ReceptionItem>();

    /// <summary>
    /// لیست تراکنش‌های مالی
    /// این لیست برای نمایش تمام پرداخت‌ها و تراکنش‌های مالی مرتبط با این پذیرش استفاده می‌شود
    /// </summary>
    public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new HashSet<PaymentTransaction>();

    /// <summary>
    /// لیست چاپ‌های رسید
    /// این لیست برای نمایش تمام چاپ‌های رسید مرتبط با این پذیرش استفاده می‌شود
    /// </summary>
    public virtual ICollection<ReceiptPrint> ReceiptPrints { get; set; } = new HashSet<ReceiptPrint>();
    public virtual ICollection<InsuranceCalculation> InsuranceCalculations { get; set; } = new HashSet<InsuranceCalculation>();
    #endregion

    // محاسبه هوشمند پرداخت
    [NotMapped]
    public bool IsPaid => Transactions?.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount) >= TotalAmount;
}

/// <summary>
/// پیکربندی مدل پذیرش بیماران برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class ReceptionConfig : EntityTypeConfiguration<Reception>
{
    public ReceptionConfig()
    {
        ToTable("Receptions");
        HasKey(r => r.ReceptionId);

        // ویژگی‌های اصلی
        Property(r => r.ReceptionDate)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Reception_ReceptionDate")));

        Property(r => r.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        Property(r => r.PatientCoPay)
            .IsRequired()
            .HasPrecision(18, 2);

        Property(r => r.InsurerShareAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        Property(r => r.Status)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Reception_Status")));

        // پیاده‌سازی ISoftDelete
        Property(r => r.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Reception_IsDeleted")));

        Property(r => r.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Reception_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(r => r.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Reception_CreatedAt")));

        Property(r => r.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Reception_CreatedByUserId")));

        Property(r => r.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Reception_UpdatedAt")));

        Property(r => r.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Reception_UpdatedByUserId")));

        Property(r => r.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Reception_DeletedByUserId")));

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        Property(r => r.PatientId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Reception_PatientId")));

        Property(r => r.DoctorId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Reception_DoctorId")));

        // ایندکس InsuranceId حذف شد

        // فیلدهای جدید برای سیستم بیمه جدید
        Property(r => r.ActivePatientInsuranceId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Reception_ActivePatientInsuranceId")));

        // روابط
        HasRequired(r => r.Patient)
            .WithMany(p => p.Receptions)
            .HasForeignKey(r => r.PatientId)
            .WillCascadeOnDelete(false);

        HasRequired(r => r.Doctor)
            .WithMany(d => d.Receptions)
            .HasForeignKey(r => r.DoctorId)
            .WillCascadeOnDelete(false);

        // رابطه با مدل قدیمی Insurance حذف شد

        // رابطه با سیستم بیمه جدید
        HasOptional(r => r.ActivePatientInsurance)
            .WithMany()
            .HasForeignKey(r => r.ActivePatientInsuranceId)
            .WillCascadeOnDelete(false);

        HasMany(r => r.ReceptionItems)
            .WithRequired(ri => ri.Reception)
            .HasForeignKey(ri => ri.ReceptionId)
            .WillCascadeOnDelete(true);

        HasMany(r => r.Transactions)
            .WithRequired(t => t.Reception)
            .HasForeignKey(r => r.ReceptionId)
            .WillCascadeOnDelete(false);

        HasMany(r => r.ReceiptPrints)
            .WithRequired(rp => rp.Reception)
            .HasForeignKey(rp => rp.ReceptionId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
        HasIndex(r => new { r.PatientId, r.ReceptionDate, r.Status })
            .HasName("IX_Reception_PatientId_Date_Status");

        HasIndex(r => new { r.DoctorId, r.ReceptionDate, r.Status })
            .HasName("IX_Reception_DoctorId_Date_Status");

        // ایندکس ترکیبی InsuranceId حذف شد
    }
}

