using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.Models.Entities.Patient;

/// <summary>
/// مدل بیمه‌های بیمار - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت بیمه‌های متعدد برای هر بیمار (اصلی و تکمیلی)
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class PatientInsurance : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه بیمه بیمار
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int PatientInsuranceId { get; set; }

    /// <summary>
    /// شناسه بیمار
    /// این فیلد برای ارتباط با بیمار استفاده می‌شود
    /// </summary>
    [Required(ErrorMessage = "بیمار الزامی است.")]
    public int PatientId { get; set; }

    /// <summary>
    /// شناسه طرح بیمه
    /// این فیلد برای ارتباط با طرح بیمه استفاده می‌شود
    /// </summary>
    [Required(ErrorMessage = "طرح بیمه الزامی است.")]
    public int InsurancePlanId { get; set; }

    /// <summary>
    /// شماره بیمه‌نامه
    /// شماره بیمه‌نامه بیمار در سیستم بیمه
    /// </summary>
    [Required(ErrorMessage = "شماره بیمه‌نامه الزامی است.")]
    [MaxLength(100, ErrorMessage = "شماره بیمه‌نامه نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string PolicyNumber { get; set; }

    /// <summary>
    /// شماره کارت بیمه
    /// شماره کارت بیمه بیمار
    /// </summary>
    [MaxLength(50, ErrorMessage = "شماره کارت بیمه نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string CardNumber { get; set; }

    /// <summary>
    /// تاریخ شروع بیمه
    /// تاریخ شروع اعتبار بیمه بیمار
    /// </summary>
    [Required(ErrorMessage = "تاریخ شروع بیمه الزامی است.")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// تاریخ پایان بیمه
    /// تاریخ پایان اعتبار بیمه بیمار (اختیاری)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// آیا این بیمه اصلی است؟
    /// این فیلد برای تشخیص بیمه اصلی از تکمیلی استفاده می‌شود
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// آیا بیمه فعال است؟
    /// این فیلد برای غیرفعال کردن بیمه‌های منقضی شده استفاده می‌شود
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// اولویت بیمه
    /// 1 = اصلی، 2 = تکمیلی اول، 3 = تکمیلی دوم و...
    /// </summary>
    [Required(ErrorMessage = "اولویت بیمه الزامی است.")]
    [Range(1, 10, ErrorMessage = "اولویت بیمه باید بین 1 تا 10 باشد.")]
    public int Priority { get; set; } = 1;

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن بیمه بیمار
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف بیمه بیمار
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که بیمه بیمار را حذف کرده است
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
    /// تاریخ و زمان ایجاد بیمه بیمار
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که بیمه بیمار را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش بیمه بیمار
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که بیمه بیمار را ویرایش کرده است
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
    /// ارجاع به بیمار
    /// این ناوبری برای دسترسی مستقیم به اطلاعات بیمار ضروری است
    /// </summary>
    public virtual Patient Patient { get; set; }

    /// <summary>
    /// ارجاع به طرح بیمه
    /// این ناوبری برای دسترسی مستقیم به اطلاعات طرح بیمه ضروری است
    /// </summary>
    public virtual InsurancePlan InsurancePlan { get; set; }
    public virtual ICollection<InsuranceCalculation> InsuranceCalculations { get; set; } = new HashSet<InsuranceCalculation>();
    #endregion
}
/// <summary>
/// پیکربندی مدل بیمه‌های بیمار برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class PatientInsuranceConfig : EntityTypeConfiguration<PatientInsurance>
{
    public PatientInsuranceConfig()
    {
        ToTable("PatientInsurances");
        HasKey(pi => pi.PatientInsuranceId);

        // ویژگی‌های اصلی
        Property(pi => pi.PatientId)
            .IsRequired();

        Property(pi => pi.InsurancePlanId)
            .IsRequired();

        Property(pi => pi.PolicyNumber)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PatientInsurance_PolicyNumber")));

        Property(pi => pi.CardNumber)
            .IsOptional()
            .HasMaxLength(50)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PatientInsurance_CardNumber")));

        Property(pi => pi.StartDate)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PatientInsurance_StartDate")));

        Property(pi => pi.EndDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PatientInsurance_EndDate")));

        Property(pi => pi.IsPrimary)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PatientInsurance_IsPrimary")));

        Property(pi => pi.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PatientInsurance_IsActive")));

        Property(pi => pi.Priority)
            .IsRequired();

        // پیاده‌سازی ISoftDelete
        Property(pi => pi.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PatientInsurance_IsDeleted")));

        Property(pi => pi.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PatientInsurance_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(pi => pi.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PatientInsurance_CreatedAt")));

        Property(pi => pi.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        Property(pi => pi.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PatientInsurance_UpdatedAt")));

        Property(pi => pi.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        Property(pi => pi.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        // روابط
        HasRequired(pi => pi.Patient)
            .WithMany(p => p.PatientInsurances)
            .HasForeignKey(pi => pi.PatientId)
            .WillCascadeOnDelete(false);

        HasRequired(pi => pi.InsurancePlan)
            .WithMany(plan => plan.PatientInsurances)
            .HasForeignKey(pi => pi.InsurancePlanId)
            .WillCascadeOnDelete(false);

        HasOptional(pi => pi.CreatedByUser)
            .WithMany()
            .HasForeignKey(pi => pi.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(pi => pi.UpdatedByUser)
            .WithMany()
            .HasForeignKey(pi => pi.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(pi => pi.DeletedByUser)
            .WithMany()
            .HasForeignKey(pi => pi.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد
        HasIndex(pi => new { pi.PatientId, pi.IsActive, pi.IsDeleted })
            .HasName("IX_PatientInsurance_Patient_IsActive_IsDeleted");

        HasIndex(pi => new { pi.PatientId, pi.Priority, pi.IsActive })
            .HasName("IX_PatientInsurance_Patient_Priority_IsActive");

        HasIndex(pi => new { pi.InsurancePlanId, pi.IsActive, pi.IsDeleted })
            .HasName("IX_PatientInsurance_Plan_IsActive_IsDeleted");

        HasIndex(pi => new { pi.StartDate, pi.EndDate, pi.IsActive })
            .HasName("IX_PatientInsurance_Validity_IsActive");

        // ایندکس ترکیبی برای جستجوی سریع بیمه‌های فعال بیمار
        HasIndex(pi => new { pi.PatientId, pi.IsActive, pi.Priority, pi.IsDeleted })
            .HasName("IX_PatientInsurance_PatientId_IsActive_Priority_IsDeleted");

        // ایندکس ترکیبی برای جستجو بر اساس تاریخ شروع و پایان
        HasIndex(pi => new { pi.StartDate, pi.EndDate, pi.IsActive, pi.IsDeleted })
            .HasName("IX_PatientInsurance_StartDate_EndDate_IsActive_IsDeleted");
    }
}