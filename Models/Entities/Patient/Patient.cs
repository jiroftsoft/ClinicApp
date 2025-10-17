using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.Models.Entities.Patient;

/// <summary>
/// مدل بیماران - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 2. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
/// 3. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
/// 4. پشتیبانی از سیستم بیمه‌ها با توجه به نیازهای سیستم‌های درمانی
/// 5. مدیریت صحیح کد ملی بیماران با امکان رمزنگاری
/// </summary>
public class Patient : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه بیمار
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int PatientId { get; set; }

    /// <summary>
    /// کد ملی بیمار
    /// این فیلد باید منحصر به فرد باشد و برای شناسایی بیمار استفاده می‌شود
    /// </summary>
    [Required(ErrorMessage = "کد ملی الزامی است.")]
    [MaxLength(10, ErrorMessage = "کد ملی نمی‌تواند بیش از 10 کاراکتر باشد.")]
    public string NationalCode { get; set; }

    /// <summary>
    /// نام بیمار
    /// </summary>
    [Required(ErrorMessage = "نام الزامی است.")]
    [MaxLength(100, ErrorMessage = "نام نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string FirstName { get; set; }

    /// <summary>
    /// نام خانوادگی بیمار
    /// </summary>
    [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
    [MaxLength(100, ErrorMessage = "نام خانوادگی نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string LastName { get; set; }

    /// <summary>
    /// نام کامل بیمار (محاسبه شده)
    /// </summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// کد بیمار (منحصر به فرد)
    /// </summary>
    [MaxLength(20, ErrorMessage = "کد بیمار نمی‌تواند بیش از 20 کاراکتر باشد.")]
    public string PatientCode { get; set; }

    /// <summary>
    /// تاریخ تولد بیمار
    /// </summary>
    [Column(TypeName = "date")]
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// آدرس بیمار
    /// </summary>
    [MaxLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Address { get; set; }

    /// <summary>
    /// شماره تلفن بیمار
    /// </summary>
    [MaxLength(50, ErrorMessage = "شماره تلفن نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// آدرس ایمیل بیمار
    /// </summary>
    [MaxLength(256, ErrorMessage = "آدرس ایمیل نمی‌تواند بیش از 256 کاراکتر باشد.")]
    [EmailAddress(ErrorMessage = "فرمت آدرس ایمیل صحیح نیست.")]
    public string Email { get; set; }
    /// <summary>
    /// جنسیت بیمار
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    [Required(ErrorMessage = "جنسیت الزامی است.")]
    public Gender Gender { get; set; }

    // InsuranceId حذف شد - از PatientInsurance استفاده کنید

    /// <summary>
    /// تاریخ آخرین ورود بیمار به سیستم
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// گروه خونی بیمار
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    [MaxLength(10, ErrorMessage = "گروه خونی نمی‌تواند بیش از 10 کاراکتر باشد.")]
    public string BloodType { get; set; }

    /// <summary>
    /// حساسیت‌های بیمار
    /// این اطلاعات برای جلوگیری از عوارض جانبی داروها ضروری است
    /// </summary>
    [MaxLength(1000, ErrorMessage = "حساسیت‌ها نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Allergies { get; set; }

    /// <summary>
    /// بیماری‌های مزمن بیمار
    /// این اطلاعات برای تشخیص و درمان صحیح ضروری است
    /// </summary>
    [MaxLength(1000, ErrorMessage = "بیماری‌های مزمن نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string ChronicDiseases { get; set; }

    /// <summary>
    /// نام تماس اضطراری
    /// </summary>
    [MaxLength(100, ErrorMessage = "نام تماس اضطراری نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string EmergencyContactName { get; set; }

    /// <summary>
    /// شماره تماس اضطراری
    /// </summary>
    [MaxLength(50, ErrorMessage = "شماره تماس اضطراری نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string EmergencyContactPhone { get; set; }

    /// <summary>
    /// رابطه با تماس اضطراری
    /// </summary>
    [MaxLength(50, ErrorMessage = "رابطه با تماس اضطراری نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string EmergencyContactRelationship { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن بیمار
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف بیمار
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که بیمار را حذف کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر حذف کننده ضروری است
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد بیمار
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که بیمار را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش بیمار
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که بیمار را ویرایش کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ویرایش کننده ضروری است
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    #region روابط
    /// <summary>
    /// شناسه کاربر مرتبط با این بیمار
    /// </summary>
    public string ApplicationUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر مرتبط با این بیمار
    /// این ارتباط برای احراز هویت و دسترسی به سیستم ضروری است
    /// </summary>
    public virtual ApplicationUser ApplicationUser { get; set; }

    // رابطه با مدل قدیمی Insurance حذف شد

    /// <summary>
    /// لیست پذیرش‌های مرتبط با این بیمار
    /// این لیست برای نمایش تمام پذیرش‌های انجام شده توسط این بیمار استفاده می‌شود
    /// </summary>
    public virtual ICollection<Reception.Reception> Receptions { get; set; } = new HashSet<Reception.Reception>();

    /// <summary>
    /// لیست نوبت‌های مرتبط با این بیمار
    /// این لیست برای نمایش تمام نوبت‌های ثبت شده توسط این بیمار استفاده می‌شود
    /// </summary>
    public virtual ICollection<Appointment.Appointment> Appointments { get; set; } = new HashSet<Appointment.Appointment>();

    /// <summary>
    /// لیست بیمه‌های مرتبط با این بیمار
    /// این لیست برای نمایش تمام بیمه‌های ثبت شده برای این بیمار استفاده می‌شود
    /// </summary>
    public virtual ICollection<PatientInsurance> PatientInsurances { get; set; } = new HashSet<PatientInsurance>();
    public virtual ICollection<InsuranceCalculation> InsuranceCalculations { get; set; } = new HashSet<InsuranceCalculation>();

    /// <summary>
    /// لیست تاریخچه پزشکی بیمار
    /// این لیست برای نمایش تمام تاریخچه پزشکی بیمار استفاده می‌شود
    /// </summary>
    public virtual ICollection<MedicalHistory> MedicalHistories { get; set; } = new HashSet<MedicalHistory>();

    /// <summary>
    /// لیست ارزیابی‌های تریاژ بیمار
    /// این لیست برای نمایش تمام ارزیابی‌های تریاژ انجام شده برای این بیمار استفاده می‌شود
    /// </summary>
    public virtual ICollection<Triage.TriageAssessment> TriageAssessments { get; set; } = new HashSet<Triage.TriageAssessment>();

    /// <summary>
    /// لیست صف تریاژ بیمار
    /// این لیست برای نمایش تمام صف‌های تریاژ این بیمار استفاده می‌شود
    /// </summary>
    public virtual ICollection<Triage.TriageQueue> TriageQueues { get; set; } = new HashSet<Triage.TriageQueue>();

    #endregion
}

/// <summary>
/// پیکربندی مدل بیماران برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class PatientConfig : EntityTypeConfiguration<Patient>
{
    public PatientConfig()
    {
        ToTable("Patients");
        HasKey(p => p.PatientId);

        // ویژگی‌های اصلی
        Property(p => p.NationalCode)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_NationalCode") { IsUnique = true }));

        Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_FirstName")));

        Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_LastName")));

        Property(p => p.BirthDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_BirthDate")));

        Property(p => p.Address)
            .IsOptional()
            .HasMaxLength(500);

        Property(p => p.PhoneNumber)
            .IsOptional()
            .HasMaxLength(50)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_PhoneNumber")));

        Property(p => p.Email)
            .IsOptional()
            .HasMaxLength(256)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_Email")));

        // پیاده‌سازی ISoftDelete
        Property(p => p.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_IsDeleted")));

        Property(p => p.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_CreatedAt")));

        Property(p => p.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_CreatedByUserId")));

        Property(p => p.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_UpdatedAt")));

        Property(p => p.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_UpdatedByUserId")));

        Property(p => p.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_DeletedByUserId")));

        // ایندکس InsuranceId حذف شد

        Property(p => p.LastLoginDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_LastLoginDate")));

        // روابط
        HasRequired(p => p.ApplicationUser)
            .WithMany(u => u.Patients)
            .HasForeignKey(p => p.ApplicationUserId)
            .WillCascadeOnDelete(false);

        // رابطه با مدل قدیمی Insurance حذف شد

        HasMany(p => p.Receptions)
            .WithRequired(r => r.Patient)
            .HasForeignKey(r => r.PatientId)
            .WillCascadeOnDelete(false);

        HasMany(p => p.Appointments)
            .WithOptional(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .WillCascadeOnDelete(false);

        HasMany(p => p.PatientInsurances)
            .WithRequired(pi => pi.Patient)
            .HasForeignKey(pi => pi.PatientId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
        HasIndex(p => new { p.LastName, p.FirstName })
            .HasName("IX_Patient_LastName_FirstName");

        // ایندکس ترکیبی برای جستجوی سریع
        HasIndex(p => new { p.IsDeleted, p.CreatedAt })
            .HasName("IX_Patient_IsDeleted_CreatedAt");

        // ایندکس ترکیبی برای جستجو بر اساس کد ملی و وضعیت حذف
        HasIndex(p => new { p.NationalCode, p.IsDeleted })
            .HasName("IX_Patient_NationalCode_IsDeleted");

        // ایندکس ترکیبی برای جستجو بر اساس شماره تلفن و وضعیت حذف
        HasIndex(p => new { p.PhoneNumber, p.IsDeleted })
            .HasName("IX_Patient_PhoneNumber_IsDeleted");

        // ایندکس ترکیبی برای جستجو بر اساس نام و نام خانوادگی
        HasIndex(p => new { p.FirstName, p.LastName, p.IsDeleted })
            .HasName("IX_Patient_FirstName_LastName_IsDeleted");

        // ایندکس ترکیبی InsuranceId حذف شد
    }
}