using System;
using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Patient;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.Entities.Insurance;

/// <summary>
/// مدل محاسبات بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. ذخیره تاریخچه محاسبات بیمه برای خدمات مختلف
/// 2. پشتیبانی از انواع مختلف محاسبات (Service, Reception, Appointment)
/// 3. مدیریت سهم بیمار و بیمه
/// 4. پشتیبانی از Copay و Coverage Override
/// 5. ردیابی کامل تغییرات و Audit Trail
/// 6. بهینه‌سازی عملکرد با Indexing
/// 7. رعایت استانداردهای پزشکی ایران
/// 
/// نکته حیاتی: این مدل برای ذخیره تاریخچه محاسبات بیمه طراحی شده است
/// </summary>
public class InsuranceCalculation : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه محاسبه بیمه
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int InsuranceCalculationId { get; set; }

    /// <summary>
    /// شناسه بیمار
    /// </summary>
    public int PatientId { get; set; }

    /// <summary>
    /// شناسه خدمت
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// شناسه طرح بیمه
    /// </summary>
    public int InsurancePlanId { get; set; }

    /// <summary>
    /// شناسه بیمه بیمار
    /// </summary>
    public int PatientInsuranceId { get; set; }

    /// <summary>
    /// مبلغ کل خدمت
    /// </summary>
    public decimal ServiceAmount { get; set; }

    /// <summary>
    /// سهم بیمه
    /// </summary>
    public decimal InsuranceShare { get; set; }

    /// <summary>
    /// سهم بیمار
    /// </summary>
    public decimal PatientShare { get; set; }

    /// <summary>
    /// Copay (سهم بیمار ثابت)
    /// </summary>
    public decimal? Copay { get; set; }

    /// <summary>
    /// Coverage Override (پوشش خاص)
    /// </summary>
    public decimal? CoverageOverride { get; set; }

    /// <summary>
    /// درصد پوشش بیمه
    /// </summary>
    public decimal CoveragePercent { get; set; }

    /// <summary>
    /// فرانشیز
    /// </summary>
    public decimal? Deductible { get; set; }

    /// <summary>
    /// تاریخ محاسبه
    /// </summary>
    public DateTime CalculationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// نوع محاسبه (Service, Reception, Appointment)
    /// </summary>
    public InsuranceCalculationType CalculationType  { get; set; }

    /// <summary>
    /// وضعیت اعتبار محاسبه
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// توضیحات
    /// </summary>
    public string Notes { get; set; }

    /// <summary>
    /// شناسه پذیرش (در صورت وجود)
    /// </summary>
    public int? ReceptionId { get; set; }

    /// <summary>
    /// شناسه قرار ملاقات (در صورت وجود)
    /// </summary>
    public int? AppointmentId { get; set; }

    #region Navigation Properties

    /// <summary>
    /// بیمار
    /// </summary>
    public virtual Patient.Patient Patient { get; set; }

    /// <summary>
    /// خدمت
    /// </summary>
    public virtual Service Service { get; set; }

    /// <summary>
    /// طرح بیمه
    /// </summary>
    public virtual InsurancePlan InsurancePlan { get; set; }

    /// <summary>
    /// بیمه بیمار
    /// </summary>
    public virtual PatientInsurance PatientInsurance { get; set; }

    /// <summary>
    /// پذیرش (در صورت وجود)
    /// </summary>
    public virtual Reception.Reception Reception { get; set; }

    /// <summary>
    /// قرار ملاقات (در صورت وجود)
    /// </summary>
    public virtual Appointment.Appointment Appointment { get; set; }

    /// <summary>
    /// کاربر ایجادکننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// کاربر به‌روزرسانی‌کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }

    /// <summary>
    /// کاربر حذف‌کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }

    #endregion

    #region ISoftDelete Implementation

    /// <summary>
    /// وضعیت حذف نرم
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ حذف
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربر حذف‌کننده
    /// </summary>
    public string DeletedByUserId { get; set; }

    #endregion

    #region ITrackable Implementation

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// شناسه کاربر ایجادکننده
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// تاریخ به‌روزرسانی
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربر به‌روزرسانی‌کننده
    /// </summary>
    public string UpdatedByUserId { get; set; }

    #endregion
}
/// <summary>
/// پیکربندی Entity Framework برای مدل InsuranceCalculation
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class InsuranceCalculationConfig : EntityTypeConfiguration<InsuranceCalculation>
{
    public InsuranceCalculationConfig()
    {
        ToTable("InsuranceCalculations");
        HasKey(ic => ic.InsuranceCalculationId);

        // ویژگی‌های اصلی
        Property(ic => ic.ServiceAmount)
            .IsRequired()
            .HasPrecision(18, 0)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_ServiceAmount")));

        Property(ic => ic.InsuranceShare)
            .IsRequired()
            .HasPrecision(18, 0)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_InsuranceShare")));

        Property(ic => ic.PatientShare)
            .IsRequired()
            .HasPrecision(18, 0)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_PatientShare")));

        Property(ic => ic.Copay)
            .IsOptional()
            .HasPrecision(18, 0)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_Copay")));

        Property(ic => ic.CoverageOverride)
            .IsOptional()
            .HasPrecision(18, 0)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_CoverageOverride")));

        Property(ic => ic.CoveragePercent)
            .IsRequired()
            .HasPrecision(5, 2)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_CoveragePercent")));

        Property(ic => ic.Deductible)
            .IsOptional()
            .HasPrecision(18, 0)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_Deductible")));

        Property(ic => ic.CalculationDate)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_CalculationDate")));

        Property(ic => ic.CalculationType)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_CalculationType")));

        Property(ic => ic.IsValid)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_IsValid")));

        Property(ic => ic.Notes)
            .IsOptional()
            .HasMaxLength(1000);

        // پیاده‌سازی ISoftDelete
        Property(ic => ic.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_IsDeleted")));

        Property(ic => ic.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(ic => ic.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_CreatedAt")));

        Property(ic => ic.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_CreatedByUserId")));

        Property(ic => ic.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_UpdatedAt")));

        Property(ic => ic.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_UpdatedByUserId")));

        Property(ic => ic.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceCalculation_DeletedByUserId")));

        // ایندکس‌های ترکیبی
        HasIndex(ic => new { ic.PatientId, ic.CalculationDate })
            .HasName("IX_InsuranceCalculation_PatientId_CalculationDate");

        HasIndex(ic => new { ic.InsurancePlanId, ic.CalculationDate })
            .HasName("IX_InsuranceCalculation_InsurancePlanId_CalculationDate");

        HasIndex(ic => new { ic.ServiceId, ic.CalculationDate })
            .HasName("IX_InsuranceCalculation_ServiceId_CalculationDate");

        HasIndex(ic => new { ic.CalculationType, ic.IsValid })
            .HasName("IX_InsuranceCalculation_CalculationType_IsValid");

        // روابط
        HasRequired(ic => ic.Patient)
            .WithMany(p => p.InsuranceCalculations)
            .HasForeignKey(ic => ic.PatientId)
            .WillCascadeOnDelete(false);

        HasRequired(ic => ic.Service)
            .WithMany(s => s.InsuranceCalculations)
            .HasForeignKey(ic => ic.ServiceId)
            .WillCascadeOnDelete(false);

        HasRequired(ic => ic.InsurancePlan)
            .WithMany(ip => ip.InsuranceCalculations)
            .HasForeignKey(ic => ic.InsurancePlanId)
            .WillCascadeOnDelete(false);

        HasRequired(ic => ic.PatientInsurance)
            .WithMany(pi => pi.InsuranceCalculations)
            .HasForeignKey(ic => ic.PatientInsuranceId)
            .WillCascadeOnDelete(false);

        HasOptional(ic => ic.Reception)
            .WithMany(r => r.InsuranceCalculations)
            .HasForeignKey(ic => ic.ReceptionId)
            .WillCascadeOnDelete(false);

        HasOptional(ic => ic.Appointment)
            .WithMany(a => a.InsuranceCalculations)
            .HasForeignKey(ic => ic.AppointmentId)
            .WillCascadeOnDelete(false);

        // روابط با ApplicationUser
        HasOptional(ic => ic.CreatedByUser)
            .WithMany()
            .HasForeignKey(ic => ic.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(ic => ic.UpdatedByUser)
            .WithMany()
            .HasForeignKey(ic => ic.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(ic => ic.DeletedByUser)
            .WithMany()
            .HasForeignKey(ic => ic.DeletedByUserId)
            .WillCascadeOnDelete(false);
    }
}