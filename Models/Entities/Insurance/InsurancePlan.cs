using ClinicApp.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Patient;

namespace ClinicApp.Models.Entities.Insurance;

/// <summary>
/// مدل طرح بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت طرح‌های بیمه تحت هر ارائه‌دهنده
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class InsurancePlan : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه طرح بیمه
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int InsurancePlanId { get; set; }

    /// <summary>
    /// شناسه ارائه‌دهنده بیمه
    /// این فیلد برای ارتباط با ارائه‌دهنده بیمه استفاده می‌شود
    /// </summary>
    [Required(ErrorMessage = "ارائه‌دهنده بیمه الزامی است.")]
    public int InsuranceProviderId { get; set; }

    /// <summary>
    /// کد طرح بیمه
    /// مثال: "SSO-BASIC", "PARSIAN-PREMIUM", "IRAN-STANDARD"
    /// </summary>
    [Required(ErrorMessage = "کد طرح بیمه الزامی است.")]
    [MaxLength(100, ErrorMessage = "کد طرح بیمه نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string PlanCode { get; set; }

    /// <summary>
    /// نام طرح بیمه
    /// مثال: "طرح پایه تأمین اجتماعی"، "طرح طلایی پارسیان"
    /// </summary>
    [Required(ErrorMessage = "نام طرح بیمه الزامی است.")]
    [MaxLength(250, ErrorMessage = "نام طرح بیمه نمی‌تواند بیش از 250 کاراکتر باشد.")]
    public string Name { get; set; }

    /// <summary>
    /// توضیحات طرح بیمه
    /// این فیلد برای ذخیره اطلاعات اضافی و توضیحات مربوط به طرح بیمه استفاده می‌شود
    /// </summary>
    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// درصد پوشش بیمه
    /// مثال: 70 = 70% پوشش بیمه
    /// </summary>
    [Required(ErrorMessage = "درصد پوشش بیمه الزامی است.")]
    [Range(0, 100, ErrorMessage = "درصد پوشش بیمه باید بین 0 تا 100 باشد.")]
    public decimal CoveragePercent { get; set; }

    /// <summary>
    /// فرانشیز (مبلغی که بیمار باید پرداخت کند)
    /// مثال: 50000 = 50,000 تومان فرانشیز
    /// </summary>
    [Required(ErrorMessage = "فرانشیز الزامی است.")]
    [Range(0, double.MaxValue, ErrorMessage = "فرانشیز نمی‌تواند منفی باشد.")]
    public decimal Deductible { get; set; }

    /// <summary>
    /// تاریخ اعتبار از
    /// تاریخ شروع اعتبار طرح بیمه
    /// </summary>
    [Required(ErrorMessage = "تاریخ شروع اعتبار الزامی است.")]
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// تاریخ اعتبار تا
    /// تاریخ پایان اعتبار طرح بیمه
    /// </summary>
    [Required(ErrorMessage = "تاریخ پایان اعتبار الزامی است.")]
    public DateTime ValidTo { get; set; }

    /// <summary>
    /// آیا طرح بیمه فعال است؟
    /// این فیلد برای غیرفعال کردن طرح‌های قدیمی یا منقضی شده استفاده می‌شود
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن طرح بیمه
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف طرح بیمه
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که طرح بیمه را حذف کرده است
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
    /// تاریخ و زمان ایجاد طرح بیمه
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که طرح بیمه را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش طرح بیمه
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که طرح بیمه را ویرایش کرده است
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
    /// ارجاع به ارائه‌دهنده بیمه
    /// این ناوبری برای دسترسی مستقیم به اطلاعات ارائه‌دهنده بیمه ضروری است
    /// </summary>
    public virtual InsuranceProvider InsuranceProvider { get; set; }

    /// <summary>
    /// لیست خدمات طرح بیمه
    /// این لیست برای نمایش تمام خدمات موجود برای این طرح بیمه استفاده می‌شود
    /// </summary>
    public virtual ICollection<PlanService> PlanServices { get; set; } = new HashSet<PlanService>();

    /// <summary>
    /// لیست بیمه‌های بیماران مرتبط با این طرح
    /// این لیست برای نمایش تمام بیمارانی که تحت پوشش این طرح هستند استفاده می‌شود
    /// </summary>
    public virtual ICollection<PatientInsurance> PatientInsurances { get; set; } = new HashSet<PatientInsurance>();
    public virtual ICollection<InsuranceCalculation> InsuranceCalculations { get; set; } = new HashSet<InsuranceCalculation>();
    #endregion
}
/// <summary>
/// پیکربندی مدل طرح بیمه برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class InsurancePlanConfig : EntityTypeConfiguration<InsurancePlan>
{
    public InsurancePlanConfig()
    {
        ToTable("InsurancePlans");
        HasKey(plan => plan.InsurancePlanId);

        // ویژگی‌های اصلی
        Property(plan => plan.InsuranceProviderId)
            .IsRequired();

        Property(plan => plan.PlanCode)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsurancePlan_PlanCode")));

        Property(plan => plan.Name)
            .IsRequired()
            .HasMaxLength(250)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsurancePlan_Name")));

        Property(plan => plan.Description)
            .IsOptional()
            .HasMaxLength(500);

        Property(plan => plan.CoveragePercent)
            .IsRequired()
            .HasPrecision(5, 2);

        Property(plan => plan.Deductible)
            .IsRequired()
            .HasPrecision(18, 2);

        Property(plan => plan.ValidFrom)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsurancePlan_ValidFrom")));

        Property(plan => plan.ValidTo)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsurancePlan_ValidTo")));

        // وضعیت فعال بودن
        Property(plan => plan.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsurancePlan_IsActive")));

        // پیاده‌سازی ISoftDelete
        Property(plan => plan.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsurancePlan_IsDeleted")));

        Property(plan => plan.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsurancePlan_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(plan => plan.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsurancePlan_CreatedAt")));

        Property(plan => plan.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        Property(plan => plan.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsurancePlan_UpdatedAt")));

        Property(plan => plan.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        Property(plan => plan.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        // روابط
        HasRequired(plan => plan.InsuranceProvider)
            .WithMany(ip => ip.InsurancePlans)
            .HasForeignKey(plan => plan.InsuranceProviderId)
            .WillCascadeOnDelete(false);

        HasOptional(plan => plan.CreatedByUser)
            .WithMany()
            .HasForeignKey(plan => plan.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(plan => plan.UpdatedByUser)
            .WithMany()
            .HasForeignKey(plan => plan.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(plan => plan.DeletedByUser)
            .WithMany()
            .HasForeignKey(plan => plan.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasMany(plan => plan.PlanServices)
            .WithRequired(ps => ps.InsurancePlan)
            .HasForeignKey(ps => ps.InsurancePlanId)
            .WillCascadeOnDelete(false);

        HasMany(plan => plan.PatientInsurances)
            .WithRequired(pi => pi.InsurancePlan)
            .HasForeignKey(pi => pi.InsurancePlanId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد
        HasIndex(plan => new { plan.InsuranceProviderId, plan.IsActive, plan.IsDeleted })
            .HasName("IX_InsurancePlan_Provider_IsActive_IsDeleted");

        HasIndex(plan => new { plan.ValidFrom, plan.ValidTo, plan.IsActive })
            .HasName("IX_InsurancePlan_Validity_IsActive");
    }
}