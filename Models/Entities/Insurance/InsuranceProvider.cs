using ClinicApp.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Insurance;

/// <summary>
/// مدل ارائه‌دهنده بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت ارائه‌دهندگان بیمه (تأمین اجتماعی، پارسیان، و...)
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class InsuranceProvider : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه ارائه‌دهنده بیمه
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int InsuranceProviderId { get; set; }

    /// <summary>
    /// نام ارائه‌دهنده بیمه
    /// مثال: "تأمین اجتماعی"، "بیمه پارسیان"، "بیمه نیروهای مسلح"
    /// </summary>
    [Required(ErrorMessage = "نام ارائه‌دهنده بیمه الزامی است.")]
    [MaxLength(250, ErrorMessage = "نام ارائه‌دهنده بیمه نمی‌تواند بیش از 250 کاراکتر باشد.")]
    public string Name { get; set; }

    /// <summary>
    /// کد ارائه‌دهنده بیمه
    /// مثال: "SSO", "PARSIAN", "IRAN"
    /// </summary>
    [Required(ErrorMessage = "کد ارائه‌دهنده بیمه الزامی است.")]
    [MaxLength(50, ErrorMessage = "کد ارائه‌دهنده بیمه نمی‌تواند بیش از 50 کاراکتر باشد.")]
    [Index("UX_InsuranceProvider_Code", IsUnique = true)]
    public string Code { get; set; }

    /// <summary>
    /// اطلاعات تماس ارائه‌دهنده بیمه
    /// شامل آدرس، تلفن، ایمیل و سایر اطلاعات تماس
    /// </summary>
    [MaxLength(1000, ErrorMessage = "اطلاعات تماس نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string ContactInfo { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }

    /// <summary>
    /// آیا ارائه‌دهنده بیمه فعال است؟
    /// این فیلد برای غیرفعال کردن ارائه‌دهندگان قدیمی یا منقضی شده استفاده می‌شود
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن ارائه‌دهنده بیمه
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف ارائه‌دهنده بیمه
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که ارائه‌دهنده بیمه را حذف کرده است
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
    /// تاریخ و زمان ایجاد ارائه‌دهنده بیمه
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که ارائه‌دهنده بیمه را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش ارائه‌دهنده بیمه
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که ارائه‌دهنده بیمه را ویرایش کرده است
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
    /// لیست طرح‌های بیمه مرتبط با این ارائه‌دهنده
    /// این لیست برای نمایش تمام طرح‌های بیمه موجود برای این ارائه‌دهنده استفاده می‌شود
    /// </summary>
    public virtual ICollection<InsurancePlan> InsurancePlans { get; set; } = new HashSet<InsurancePlan>();
    #endregion
}
/// <summary>
/// پیکربندی مدل ارائه‌دهنده بیمه برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class InsuranceProviderConfig : EntityTypeConfiguration<InsuranceProvider>
{
    public InsuranceProviderConfig()
    {
        ToTable("InsuranceProviders");
        HasKey(ip => ip.InsuranceProviderId);

        // ویژگی‌های اصلی
        Property(ip => ip.Name)
            .IsRequired()
            .HasMaxLength(250)
            .HasColumnType("nvarchar")
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceProvider_Name")));

        Property(ip => ip.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("nvarchar")
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceProvider_Code")));

        Property(ip => ip.ContactInfo)
            .IsOptional()
            .HasMaxLength(1000)
            .HasColumnType("nvarchar");

        // وضعیت فعال بودن
        Property(ip => ip.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceProvider_IsActive")));

        // پیاده‌سازی ISoftDelete
        Property(ip => ip.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceProvider_IsDeleted")));

        Property(ip => ip.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceProvider_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(ip => ip.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceProvider_CreatedAt")));

        Property(ip => ip.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        Property(ip => ip.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_InsuranceProvider_UpdatedAt")));

        Property(ip => ip.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        Property(ip => ip.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        // روابط
        HasOptional(ip => ip.CreatedByUser)
            .WithMany()
            .HasForeignKey(ip => ip.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(ip => ip.UpdatedByUser)
            .WithMany()
            .HasForeignKey(ip => ip.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(ip => ip.DeletedByUser)
            .WithMany()
            .HasForeignKey(ip => ip.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // رابطه با مدل قدیمی Insurance حذف شد

        HasMany(ip => ip.InsurancePlans)
            .WithRequired(plan => plan.InsuranceProvider)
            .HasForeignKey(plan => plan.InsuranceProviderId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد
        HasIndex(ip => new { ip.IsActive, ip.IsDeleted })
            .HasName("IX_InsuranceProvider_IsActive_IsDeleted");

        HasIndex(ip => new { ip.Code, ip.IsActive })
            .HasName("IX_InsuranceProvider_Code_IsActive");
    }
}