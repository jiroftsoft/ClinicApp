using System;
using ClinicApp.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Models.Entities.Insurance;

/// <summary>
/// مدل تعرفه بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. پشتیبانی از تعریف تعرفه‌های خاص برای هر خدمت
/// 2. امکان تعریف قیمت تعرفه‌ای متفاوت از قیمت پایه
/// 3. امکان تعریف سهم بیمار و بیمه متفاوت از سهم پیش‌فرض
/// 4. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class InsuranceTariff : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه تعرفه بیمه
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int InsuranceTariffId { get; set; }

    // InsuranceId حذف شد - از PlanService استفاده کنید

    /// <summary>
    /// شناسه خدمت
    /// این فیلد ارتباط با جدول خدمات را برقرار می‌کند
    /// </summary>
    [Required(ErrorMessage = "خدمت الزامی است.")]
    public int ServiceId { get; set; }

    /// <summary>
    /// مبلغ مشخص‌شده برای خدمت تحت پوشش این بیمه (اگر null باشد، از قیمت پایه Service استفاده می‌شود).
    /// </summary>
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal")]
    public decimal? TariffPrice { get; set; }

    /// <summary>
    /// سهم بیمار به مبلغ (تومان).
    /// اگر null باشد، از سهم پیش‌فرض بیمه استفاده می‌شود.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "سهم بیمار نمی‌تواند منفی باشد.")]
    public decimal? PatientShare { get; set; }

    /// <summary>
    /// سهم بیمه به مبلغ (تومان).
    /// اگر null باشد، از سهم پیش‌فرض بیمه استفاده می‌شود.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "سهم بیمه نمی‌تواند منفی باشد.")]
    public decimal? InsurerShare { get; set; }

    /// <summary>
    /// وضعیت فعال بودن تعرفه
    /// این فیلد برای کنترل دسترسی به تعرفه‌ها در سیستم‌های پزشکی ضروری است
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن تعرفه بیمه
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف تعرفه بیمه
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که تعرفه بیمه را حذف کرده است
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
    /// تاریخ و زمان ایجاد تعرفه
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که تعرفه را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش تعرفه
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که تعرفه را ویرایش کرده است
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
    // رابطه با مدل قدیمی Insurance حذف شد

    /// <summary>
    /// شناسه طرح بیمه (اختیاری - برای سازگاری با سیستم جدید)
    /// این فیلد برای اتصال تعرفه به طرح بیمه جدید استفاده می‌شود
    /// </summary>
    public int? InsurancePlanId { get; set; }

    /// <summary>
    /// ارجاع به طرح بیمه
    /// این ارتباط برای نمایش اطلاعات طرح بیمه در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual InsurancePlan InsurancePlan { get; set; }

    /// <summary>
    /// ارجاع به خدمت مرتبط با این تعرفه
    /// این ارتباط برای نمایش اطلاعات خدمت در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual Service Service { get; set; }

    #region فیلدهای بیمه تکمیلی

    /// <summary>
    /// نوع بیمه (اصلی/تکمیلی)
    /// این فیلد برای تمایز بین بیمه اصلی و تکمیلی استفاده می‌شود
    /// </summary>
    [Required(ErrorMessage = "نوع بیمه الزامی است.")]
    public InsuranceType InsuranceType { get; set; } = InsuranceType.Primary;

    /// <summary>
    /// تنظیمات خاص بیمه تکمیلی (JSON)
    /// این فیلد برای ذخیره تنظیمات پیچیده بیمه تکمیلی استفاده می‌شود
    /// </summary>
    [MaxLength(2000, ErrorMessage = "تنظیمات بیمه تکمیلی نمی‌تواند بیش از 2000 کاراکتر باشد.")]
    public string SupplementarySettings { get; set; }

    /// <summary>
    /// درصد پوشش بیمه تکمیلی
    /// این فیلد برای تعریف درصد پوشش بیمه تکمیلی استفاده می‌شود
    /// </summary>
    [Range(0, 100, ErrorMessage = "درصد پوشش بیمه تکمیلی باید بین 0 تا 100 باشد.")]
    public decimal? SupplementaryCoveragePercent { get; set; }

    /// <summary>
    /// سقف پرداخت بیمه تکمیلی
    /// این فیلد برای تعریف حداکثر مبلغ پرداخت بیمه تکمیلی استفاده می‌شود
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "سقف پرداخت بیمه تکمیلی نمی‌تواند منفی باشد.")]
    public decimal? SupplementaryMaxPayment { get; set; }

    /// <summary>
    /// اولویت تعرفه
    /// این فیلد برای تعیین اولویت تعرفه‌ها استفاده می‌شود
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// تاریخ شروع اعتبار تعرفه
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// تاریخ پایان اعتبار تعرفه
    /// </summary>
    public DateTime? EndDate { get; set; }

    #endregion
    #endregion
}
/// <summary>
/// پیکربندی مدل تعرفه بیمه برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class InsuranceTariffConfig : EntityTypeConfiguration<InsuranceTariff>
{
    public InsuranceTariffConfig()
    {
        ToTable("InsuranceTariffs");
        HasKey(t => t.InsuranceTariffId);

        // ویژگی‌های اصلی
        Property(t => t.TariffPrice)
            .IsOptional()
            .HasPrecision(18, 2)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_TariffPrice")));

        Property(t => t.PatientShare)
            .IsOptional()
            .HasPrecision(18, 2)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_PatientShare")));

        Property(t => t.InsurerShare)
            .IsOptional()
            .HasPrecision(18, 2)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_InsurerShare")));

        // پیاده‌سازی ISoftDelete
        Property(t => t.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_IsDeleted")));

        Property(t => t.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_CreatedAt")));

        Property(t => t.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_CreatedByUserId")));

        Property(t => t.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_UpdatedAt")));

        Property(t => t.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_UpdatedByUserId")));

        Property(t => t.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_DeletedByUserId")));

        // ایندکس InsuranceId حذف شد

        // فیلدهای جدید برای سیستم بیمه جدید
        Property(t => t.InsurancePlanId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_InsurancePlanId")));

        Property(t => t.ServiceId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_ServiceId")));

        // فیلدهای بیمه تکمیلی
        Property(t => t.InsuranceType)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_InsuranceType")));

        Property(t => t.SupplementarySettings)
            .IsOptional()
            .HasMaxLength(2000);

        Property(t => t.SupplementaryCoveragePercent)
            .IsOptional()
            .HasPrecision(5, 2)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_SupplementaryCoveragePercent")));

        Property(t => t.SupplementaryMaxPayment)
            .IsOptional()
            .HasPrecision(18, 2)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_SupplementaryMaxPayment")));

        // روابط
        // رابطه با مدل قدیمی Insurance حذف شد

        // رابطه با سیستم بیمه جدید
        HasOptional(t => t.InsurancePlan)
            .WithMany()
            .HasForeignKey(t => t.InsurancePlanId)
            .WillCascadeOnDelete(false);

        HasRequired(t => t.Service)
            .WithMany(s => s.Tariffs)
            .HasForeignKey(t => t.ServiceId)
            .WillCascadeOnDelete(false);

        HasOptional(t => t.DeletedByUser)
            .WithMany()
            .HasForeignKey(t => t.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(t => t.UpdatedByUser)
            .WithMany()
            .HasForeignKey(t => t.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس یونیک InsuranceId حذف شد
    }
}