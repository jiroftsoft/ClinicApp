using System;
using ClinicApp.Models.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Doctor;

namespace ClinicApp.Models.Entities.Clinic;

/// <summary>
/// نماینده یک دسته‌بندی برای گروه‌بندی خدمات بالینی در سیستم‌های پزشکی
/// این دسته‌بندی‌ها برای سازماندهی خدمات مانند "تزریقات"، "معاینات تخصصی"، "آزمایش‌ها" و سایر خدمات پزشکی استفاده می‌شوند
/// 
/// ویژگی‌های کلیدی:
/// 1. پشتیبانی از ساختار سازمانی پزشکی (کلینیک -> دپارتمان -> دسته‌بندی خدمات)
/// 2. مدیریت صحیح فیلدهای ردیابی (Audit Trail) برای رعایت استانداردهای پزشکی
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 4. ارتباط با دپارتمان‌های پزشکی برای سازماندهی بهتر خدمات
/// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// 6. **رابطه Many-to-Many با پزشکان برای انتساب خدمات**
/// </summary>
public class ServiceCategory : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه دسته‌بندی خدمات پزشکی
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int ServiceCategoryId { get; set; }

    /// <summary>
    /// عنوان دسته‌بندی خدمات پزشکی
    /// مثال: "تزریقات"، "معاینات تخصصی"، "آزمایش‌های تشخیصی"
    /// </summary>
    [Required(ErrorMessage = "عنوان دسته‌بندی الزامی است.")]
    [MaxLength(200, ErrorMessage = "عنوان دسته‌بندی نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string Title { get; set; }

    /// <summary>
    /// شناسه دپارتمان مرتبط با این دسته‌بندی خدمات
    /// این فیلد ارتباط با جدول دپارتمان‌ها را برقرار می‌کند
    /// </summary>
    [Required(ErrorMessage = "دپارتمان الزامی است.")]
    public int DepartmentId { get; set; }

    /// <summary>
    /// آیا دسته‌بندی فعال است؟
    /// این فیلد برای غیرفعال کردن دسته‌بندی‌های قدیمی یا منقضی شده استفاده می‌شود
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن دسته‌بندی
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف دسته‌بندی
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که دسته‌بندی را حذف کرده است
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
    /// تاریخ و زمان ایجاد دسته‌بندی
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که دسته‌بندی را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین بروزرسانی دسته‌بندی
    /// این اطلاعات برای ردیابی تغییرات در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که آخرین بروزرسانی را انجام داده است
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
    /// ارجاع به دپارتمان مرتبط با این دسته‌بندی خدمات
    /// این ارتباط برای نمایش اطلاعات دپارتمان در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual Department Department { get; set; }

    /// <summary>
    /// لیست خدمات پزشکی مرتبط با این دسته‌بندی
    /// این لیست برای نمایش تمام خدمات موجود در این دسته‌بندی استفاده می‌شود
    /// </summary>
    public virtual ICollection<Service> Services { get; set; } = new HashSet<Service>();

    /// <summary>
    /// لیست ارتباطات Many-to-Many با پزشکان
    /// این رابطه برای مشخص کردن اینکه کدام پزشکان مجاز به ارائه این دسته خدمات هستند استفاده می‌شود
    /// مثال: دکتر احمدی مجاز به ارائه خدمات "تزریقات عضلانی" است ولی مجاز به "تزریق بوتاکس" نیست
    /// </summary>
    public virtual ICollection<DoctorServiceCategory> DoctorServiceCategories { get; set; } = new HashSet<DoctorServiceCategory>();

    public string Description { get; set; }

    #endregion
}

#region ServiceCategory

/// <summary>
/// پیکربندی مدل دسته‌بندی خدمات پزشکی برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class ServiceCategoryConfig : EntityTypeConfiguration<ServiceCategory>
{
    public ServiceCategoryConfig()
    {
        ToTable("ServiceCategories");
        HasKey(sc => sc.ServiceCategoryId);

        // ویژگی‌های اصلی
        Property(sc => sc.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_Title")));

        Property(sc => sc.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_IsActive")));

        // پیاده‌سازی ISoftDelete
        Property(sc => sc.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_IsDeleted")));

        Property(sc => sc.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(sc => sc.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_CreatedAt")));

        Property(sc => sc.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_CreatedByUserId")));

        Property(sc => sc.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_UpdatedAt")));

        Property(sc => sc.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_UpdatedByUserId")));

        Property(sc => sc.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_DeletedByUserId")));

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        Property(sc => sc.DepartmentId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_DepartmentId")));

        // روابط
        HasRequired(sc => sc.Department)
            .WithMany(d => d.ServiceCategories)
            .HasForeignKey(sc => sc.DepartmentId)
            .WillCascadeOnDelete(false);

        // ✅ رابطه Many-to-Many با پزشکان
        HasMany(sc => sc.DoctorServiceCategories)
            .WithRequired(dsc => dsc.ServiceCategory)
            .HasForeignKey(dsc => dsc.ServiceCategoryId)
            .WillCascadeOnDelete(false);

        HasOptional(sc => sc.DeletedByUser)
            .WithMany()
            .HasForeignKey(sc => sc.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(sc => sc.CreatedByUser)
            .WithMany()
            .HasForeignKey(sc => sc.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(sc => sc.UpdatedByUser)
            .WithMany()
            .HasForeignKey(sc => sc.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
        HasIndex(sc => new { sc.DepartmentId, sc.IsActive, sc.IsDeleted })
            .HasName("IX_ServiceCategory_DepartmentId_IsActive_IsDeleted");
    }
}

#endregion