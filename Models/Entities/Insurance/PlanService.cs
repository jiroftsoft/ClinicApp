using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Clinic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Insurance;

/// <summary>
/// مدل خدمات طرح بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت خدمات تحت پوشش هر طرح بیمه
/// 2. امکان تعریف سهم بیمار و پوشش خاص برای هر خدمت
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 4. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
/// </summary>
public class PlanService : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه خدمات طرح بیمه
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int PlanServiceId { get; set; }

    /// <summary>
    /// شناسه طرح بیمه
    /// این فیلد برای ارتباط با طرح بیمه استفاده می‌شود
    /// </summary>
    [Required(ErrorMessage = "طرح بیمه الزامی است.")]
    public int InsurancePlanId { get; set; }

    /// <summary>
    /// شناسه دسته‌بندی خدمت
    /// این فیلد برای ارتباط با دسته‌بندی خدمت استفاده می‌شود
    /// </summary>
    [Required(ErrorMessage = "دسته‌بندی خدمت الزامی است.")]
    public int ServiceCategoryId { get; set; }

    /// <summary>
    /// سهم بیمار به درصد (اختیاری)
    /// اگر تعریف نشود، از سهم پیش‌فرض طرح بیمه استفاده می‌شود
    /// مثال: 30 = 30% سهم بیمار
    /// </summary>
    [Range(0, 100, ErrorMessage = "سهم بیمار باید بین 0 تا 100 درصد باشد.")]
    public decimal? PatientSharePercent { get; set; } // was: Copay

    /// <summary>
    /// پوشش خاص به درصد (اختیاری)
    /// اگر تعریف نشود، از پوشش پیش‌فرض طرح بیمه استفاده می‌شود
    /// مثال: 70 = 70% پوشش بیمه
    /// </summary>
    [Range(0, 100, ErrorMessage = "پوشش بیمه باید بین 0 تا 100 درصد باشد.")]
    public decimal? CoverageOverride { get; set; }

    /// <summary>
    /// آیا این خدمت تحت پوشش است؟
    /// این فیلد برای تعریف خدماتی که تحت پوشش نیستند استفاده می‌شود
    /// </summary>
    public bool IsCovered { get; set; } = true;

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن خدمات طرح بیمه
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف خدمات طرح بیمه
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که خدمات طرح بیمه را حذف کرده است
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
    /// تاریخ و زمان ایجاد خدمات طرح بیمه
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که خدمات طرح بیمه را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش خدمات طرح بیمه
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که خدمات طرح بیمه را ویرایش کرده است
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
    /// ارجاع به طرح بیمه
    /// این ناوبری برای دسترسی مستقیم به اطلاعات طرح بیمه ضروری است
    /// </summary>
    public virtual InsurancePlan InsurancePlan { get; set; }

    /// <summary>
    /// ارجاع به دسته‌بندی خدمت
    /// این ناوبری برای دسترسی مستقیم به اطلاعات دسته‌بندی خدمت ضروری است
    /// </summary>
    public virtual ServiceCategory ServiceCategory { get; set; }
    #endregion
}
/// <summary>
/// پیکربندی مدل خدمات طرح بیمه برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class PlanServiceConfig : EntityTypeConfiguration<PlanService>
{
    public PlanServiceConfig()
    {
        ToTable("PlanServices");
        HasKey(ps => ps.PlanServiceId);

        // ویژگی‌های اصلی
        Property(ps => ps.InsurancePlanId)
            .IsRequired();

        Property(ps => ps.ServiceCategoryId)
            .IsRequired();


        Property(ps => ps.PatientSharePercent)   // was: Copay
            .IsOptional()
            .HasPrecision(5, 2);

        Property(ps => ps.CoverageOverride)
            .IsOptional()
            .HasPrecision(5, 2);

        Property(ps => ps.IsCovered)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PlanService_IsCovered")));

        // پیاده‌سازی ISoftDelete
        Property(ps => ps.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PlanService_IsDeleted")));

        Property(ps => ps.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PlanService_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(ps => ps.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PlanService_CreatedAt")));

        Property(ps => ps.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        Property(ps => ps.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PlanService_UpdatedAt")));

        Property(ps => ps.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        Property(ps => ps.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        // روابط
        HasRequired(ps => ps.InsurancePlan)
            .WithMany(plan => plan.PlanServices)
            .HasForeignKey(ps => ps.InsurancePlanId)
            .WillCascadeOnDelete(false);

        HasRequired(ps => ps.ServiceCategory)
            .WithMany()
            .HasForeignKey(ps => ps.ServiceCategoryId)
            .WillCascadeOnDelete(false);

        HasOptional(ps => ps.CreatedByUser)
            .WithMany()
            .HasForeignKey(ps => ps.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(ps => ps.UpdatedByUser)
            .WithMany()
            .HasForeignKey(ps => ps.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(ps => ps.DeletedByUser)
            .WithMany()
            .HasForeignKey(ps => ps.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد
        HasIndex(ps => new { ps.InsurancePlanId, ps.ServiceCategoryId, ps.IsDeleted })
            .IsUnique()
            .HasName("IX_PlanService_Plan_Category_IsDeleted");

        HasIndex(ps => new { ps.InsurancePlanId, ps.IsCovered, ps.IsDeleted })
            .HasName("IX_PlanService_Plan_IsCovered_IsDeleted");
    }
}