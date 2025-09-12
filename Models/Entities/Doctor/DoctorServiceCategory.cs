using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Clinic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Doctor;

/// <summary>
/// جدول واسط برای رابطه چند-به-چند بین پزشک و دسته‌بندی خدمات مجاز
/// 
/// ویژگی‌های کلیدی:
/// 1. کنترل دسترسی پزشکان به خدمات خاص
/// 2. مدیریت صلاحیت‌های پزشکی (Medical Authorization)
/// 3. کلید ترکیبی (Composite Key) برای جلوگیری از تکرار
/// 4. پشتیبانی از سناریوهای پیچیده (صلاحیت‌های مختلف)
/// 
/// مثال کاربرد:
/// - دکتر احمدی: مجاز به "تزریقات عضلانی" و "تزریقات وریدی"
/// - دکتر محمدی: مجاز به "بررسی‌های اولیه" ولی غیرمجاز به "تزریقات زیبایی"
/// </summary>
public class DoctorServiceCategory : ITrackable
{
    /// <summary>
    /// شناسه پزشک - بخشی از کلید ترکیبی
    /// </summary>
    [Required(ErrorMessage = "شناسه پزشک الزامی است.")]
    public int DoctorId { get; set; }

    /// <summary>
    /// شناسه دسته‌بندی خدمات - بخشی از کلید ترکیبی
    /// </summary>
    [Required(ErrorMessage = "شناسه دسته‌بندی خدمات الزامی است.")]
    public int ServiceCategoryId { get; set; }

    /// <summary>
    /// سطح صلاحیت پزشک در این دسته خدمات (اختیاری)
    /// مثال: "مبتدی"، "متوسط"، "پیشرفته"، "متخصص"
    /// </summary>
    [MaxLength(50, ErrorMessage = "سطح صلاحیت نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string AuthorizationLevel { get; set; }

    /// <summary>
    /// وضعیت فعال بودن این صلاحیت
    /// می‌تواند پزشک صلاحیت داشته باشد ولی موقتاً غیرفعال باشد
    /// </summary>
    [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// تاریخ اعطای صلاحیت
    /// </summary>
    public DateTime? GrantedDate { get; set; }

    /// <summary>
    /// تاریخ انقضای صلاحیت (در صورت وجود)
    /// برای خدمات خاص که نیاز به تمدید دارند
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// شماره گواهی یا مجوز (اختیاری)
    /// برای خدمات خاص که نیاز به گواهی‌نامه دارند
    /// </summary>
    [MaxLength(100, ErrorMessage = "شماره گواهی نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string CertificateNumber { get; set; }

    /// <summary>
    /// توضیحات اضافی در مورد این صلاحیت
    /// </summary>
    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Notes { get; set; }

    #region پیاده‌سازی ITrackable
    /// <summary>
    /// تاریخ و زمان ایجاد این صلاحیت
    /// مهم برای ردیابی زمان اعطای صلاحیت به پزشک
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که این صلاحیت را اعطا کرده
    /// مهم برای سیستم‌های پزشکی - چه کسی صلاحیت را اعطا کرده
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش این صلاحیت
    /// مهم برای ردیابی تغییرات (مثل تغییر سطح، تمدید انقضا)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که این صلاحیت را ویرایش کرده
    /// مهم برای سیستم‌های پزشکی - چه کسی تغییرات را اعمال کرده
    /// </summary>
    public string UpdatedByUserId { get; set; }
    #endregion

    #region Navigation Properties
    /// <summary>
    /// ارجاع به پزشک مرتبط
    /// برای دسترسی مستقیم به اطلاعات پزشک از طریق این صلاحیت
    /// </summary>
    public virtual Doctor Doctor { get; set; }

    /// <summary>
    /// ارجاع به دسته‌بندی خدمات مرتبط
    /// برای دسترسی مستقیم به اطلاعات دسته خدمات از طریق این صلاحیت
    /// </summary>
    public virtual ServiceCategory ServiceCategory { get; set; }

    /// <summary>
    /// ارجاع به کاربر اعطا کننده صلاحیت
    /// برای دسترسی مستقیم به اطلاعات کاربری که این صلاحیت را اعطا کرده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده صلاحیت
    /// برای دسترسی مستقیم به اطلاعات کاربری که این صلاحیت را ویرایش کرده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده صلاحیت
    /// برای دسترسی مستقیم به اطلاعات کاربری که این صلاحیت را حذف کرده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }

    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف این صلاحیت
    /// مهم برای ردیابی زمان حذف صلاحیت از پزشک
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که این صلاحیت را حذف کرده
    /// مهم برای سیستم‌های پزشکی - چه کسی صلاحیت را حذف کرده
    /// </summary>
    public string DeletedByUserId { get; set; }

    #endregion
}
#region DoctorServiceCategory

/// <summary>
/// پیکربندی Entity Framework برای جدول DoctorServiceCategory
/// این پیکربندی با استانداردهای سیستم‌های پزشکی طراحی شده است
/// </summary>
public class DoctorServiceCategoryConfig : EntityTypeConfiguration<DoctorServiceCategory>
{
    public DoctorServiceCategoryConfig()
    {
        // تنظیمات جدول
        ToTable("DoctorServiceCategories");

        // کلید ترکیبی (Composite Key)
        HasKey(dsc => new { dsc.DoctorId, dsc.ServiceCategoryId });

        // تنظیمات Property ها
        Property(dsc => dsc.DoctorId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_DoctorId")));

        Property(dsc => dsc.ServiceCategoryId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_ServiceCategoryId")));

        Property(dsc => dsc.AuthorizationLevel)
            .IsOptional()
            .HasMaxLength(50)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_AuthorizationLevel")));

        Property(dsc => dsc.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_IsActive")));

        Property(dsc => dsc.GrantedDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_GrantedDate")));

        Property(dsc => dsc.ExpiryDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_ExpiryDate")));

        Property(dsc => dsc.CertificateNumber)
            .IsOptional()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_CertificateNumber")));

        Property(dsc => dsc.Notes)
            .IsOptional()
            .HasMaxLength(500);

        // تنظیمات ITrackable
        Property(dsc => dsc.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_CreatedAt")));

        Property(dsc => dsc.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_CreatedByUserId")));

        Property(dsc => dsc.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_UpdatedAt")));

        Property(dsc => dsc.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_UpdatedByUserId")));

        // تنظیمات ISoftDelete
        Property(dsc => dsc.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_IsDeleted")));

        Property(dsc => dsc.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_DeletedAt")));

        Property(dsc => dsc.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_DeletedByUserId")));

        // روابط اصلی
        HasRequired(dsc => dsc.Doctor)
            .WithMany(d => d.DoctorServiceCategories)
            .HasForeignKey(dsc => dsc.DoctorId)
            .WillCascadeOnDelete(false);

        HasRequired(dsc => dsc.ServiceCategory)
            .WithMany(sc => sc.DoctorServiceCategories)
            .HasForeignKey(dsc => dsc.ServiceCategoryId)
            .WillCascadeOnDelete(false);

        // روابط Audit
        HasOptional(dsc => dsc.CreatedByUser)
            .WithMany()
            .HasForeignKey(dsc => dsc.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(dsc => dsc.UpdatedByUser)
            .WithMany()
            .HasForeignKey(dsc => dsc.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(dsc => dsc.DeletedByUser)
            .WithMany()
            .HasForeignKey(dsc => dsc.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود کارایی
        HasIndex(dsc => new { dsc.DoctorId, dsc.IsActive })
            .HasName("IX_DoctorServiceCategory_DoctorId_IsActive");

        HasIndex(dsc => new { dsc.ServiceCategoryId, dsc.IsActive })
            .HasName("IX_DoctorServiceCategory_ServiceCategoryId_IsActive");

        HasIndex(dsc => new { dsc.DoctorId, dsc.ServiceCategoryId, dsc.IsActive })
            .HasName("IX_DoctorServiceCategory_DoctorId_ServiceCategoryId_IsActive");

        HasIndex(dsc => new { dsc.ExpiryDate, dsc.IsActive })
            .HasName("IX_DoctorServiceCategory_ExpiryDate_IsActive");

        HasIndex(dsc => new { dsc.AuthorizationLevel, dsc.IsActive })
            .HasName("IX_DoctorServiceCategory_AuthorizationLevel_IsActive");
    }
}

#endregion