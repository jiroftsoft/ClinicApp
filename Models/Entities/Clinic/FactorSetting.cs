using System;
using ClinicApp.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.Entities.Clinic
{
/// <summary>
/// مدل تنظیمات ضرایب - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت مرکزی ضرایب فنی و حرفه‌ای
/// 2. پشتیبانی از تغییر تعرفه‌ها در طول زمان
/// 3. مدیریت ضرایب بر اساس نوع هشتگ
/// 4. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class FactorSetting : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه تنظیم ضریب
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int FactorSettingId { get; set; }

    /// <summary>
    /// نوع ضریب
    /// Technical = فنی، Professional = حرفه‌ای
    /// </summary>
    [Required(ErrorMessage = "نوع ضریب الزامی است.")]
    public ServiceComponentType FactorType { get; set; }

    /// <summary>
    /// آیا این ضریب مربوط به خدمات هشتگ‌دار است؟
    /// برای کای حرفه‌ای: همیشه false (ثابت برای همه)
    /// برای کای فنی: true = هشتگ‌دار، false = بدون هشتگ
    /// </summary>
    public bool IsHashtagged { get; set; }

    /// <summary>
    /// مقدار ضریب
    /// برای محاسبه مبلغ نهایی خدمت استفاده می‌شود
    /// </summary>
    [Required(ErrorMessage = "مقدار ضریب الزامی است.")]
    [Column(TypeName = "decimal")]
    [Range(0.01, 999999999.99, ErrorMessage = "مقدار ضریب باید بین 0.01 تا 999,999,999.99 باشد.")]
    public decimal Value { get; set; }

    /// <summary>
    /// تاریخ شروع اعتبار
    /// </summary>
    [Required(ErrorMessage = "تاریخ شروع اعتبار الزامی است.")]
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// تاریخ پایان اعتبار (اختیاری)
    /// اگر null باشد، ضریب تا زمان تعریف ضریب جدید معتبر است
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// سال مالی این ضریب
    /// مثال: 1404، 1405
    /// </summary>
    [Required(ErrorMessage = "سال مالی الزامی است.")]
    [Range(1300, 1500, ErrorMessage = "سال مالی باید بین 1300 تا 1500 باشد.")]
    public int FinancialYear { get; set; }

    /// <summary>
    /// آیا این ضریب برای سال مالی فعلی فعال است؟
    /// </summary>
    public bool IsActiveForCurrentYear { get; set; } = true;

    /// <summary>
    /// آیا محاسبات این سال مالی فریز شده است؟
    /// وقتی true باشد، دیگر نمی‌توان از این ضریب برای محاسبات جدید استفاده کرد
    /// </summary>
    public bool IsFrozen { get; set; } = false;

    /// <summary>
    /// تاریخ فریز شدن محاسبات
    /// </summary>
    public DateTime? FrozenAt { get; set; }

    /// <summary>
    /// کاربری که محاسبات را فریز کرده است
    /// </summary>
    public string FrozenByUserId { get; set; }
    public virtual ApplicationUser FrozenByUser { get; set; }

    /// <summary>
    /// آیا این تنظیم فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// توضیحات این تنظیم
    /// </summary>
    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Description { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن تنظیم ضریب
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف تنظیم ضریب
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که تنظیم ضریب را حذف کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان ایجاد تنظیم ضریب
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که تنظیم ضریب را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین بروزرسانی تنظیم ضریب
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که آخرین بار تنظیم ضریب را بروزرسانی کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر بروزرسانی کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion
}

#region FactorSettingConfig

/// <summary>
/// پیکربندی مدل تنظیمات ضرایب برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class FactorSettingConfig : EntityTypeConfiguration<FactorSetting>
{
    public FactorSettingConfig()
    {
        ToTable("FactorSettings");
        HasKey(fs => fs.FactorSettingId);

        // ویژگی‌های اصلی
        Property(fs => fs.FactorType)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_FactorSetting_FactorType")));

        Property(fs => fs.IsHashtagged)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_FactorSetting_IsHashtagged")));

        Property(fs => fs.Value)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_FactorSetting_Value")));

        Property(fs => fs.EffectiveFrom)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_FactorSetting_EffectiveFrom")));

        Property(fs => fs.EffectiveTo)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_FactorSetting_EffectiveTo")));

        Property(fs => fs.FinancialYear)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_FactorSetting_FinancialYear")));

        Property(fs => fs.IsActiveForCurrentYear)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_FactorSetting_IsActiveForCurrentYear")));

        Property(fs => fs.IsFrozen)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_FactorSetting_IsFrozen")));

        Property(fs => fs.FrozenAt)
            .IsOptional();

        Property(fs => fs.FrozenByUserId)
            .IsOptional()
            .HasMaxLength(128);

        Property(fs => fs.Description)
            .IsOptional()
            .HasMaxLength(500);

        // پیاده‌سازی ISoftDelete
        Property(fs => fs.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_FactorSetting_IsDeleted")));

        Property(fs => fs.DeletedAt)
            .IsOptional();

        Property(fs => fs.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        // پیاده‌سازی ITrackable
        Property(fs => fs.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_FactorSetting_CreatedAt")));

        Property(fs => fs.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        Property(fs => fs.UpdatedAt)
            .IsOptional();

        Property(fs => fs.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        // روابط
        HasOptional(fs => fs.DeletedByUser)
            .WithMany()
            .HasForeignKey(fs => fs.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(fs => fs.CreatedByUser)
            .WithMany()
            .HasForeignKey(fs => fs.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(fs => fs.UpdatedByUser)
            .WithMany()
            .HasForeignKey(fs => fs.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(fs => fs.FrozenByUser)
            .WithMany()
            .HasForeignKey(fs => fs.FrozenByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای جستجوهای رایج
        HasIndex(fs => new { fs.FactorType, fs.IsHashtagged, fs.FinancialYear, fs.IsActive, fs.IsDeleted })
            .HasName("IX_FactorSetting_FactorType_IsHashtagged_FinancialYear_IsActive_IsDeleted");

        HasIndex(fs => new { fs.FinancialYear, fs.IsFrozen, fs.IsDeleted })
            .HasName("IX_FactorSetting_FinancialYear_IsFrozen_IsDeleted");

        HasIndex(fs => new { fs.IsActiveForCurrentYear, fs.IsDeleted })
            .HasName("IX_FactorSetting_IsActiveForCurrentYear_IsDeleted");
    }
}

#endregion
}
