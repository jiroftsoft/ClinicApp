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
/// نماینده یک دپارتمان در سیستم‌های پزشکی
/// این کلاس تمام اطلاعات دپارتمان‌ها را شامل می‌شود و برای سیستم‌های پزشکی طراحی شده است
/// 
/// ویژگی‌های کلیدی:
/// 1. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان)
/// 2. مدیریت صحیح فیلدهای ردیابی (Audit Trail) برای رعایت استانداردهای پزشکی
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 4. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
/// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// 6. **رابطه Many-to-Many با پزشکان برای انتساب چندگانه**
/// </summary>
public class Department : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه دپارتمان
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// نام دپارتمان
    /// مثال: "دندانپزشکی"، "چشم پزشکی"، "اورولوژی"، "تزریقات"، "اورژانس"
    /// </summary>
    [Required(ErrorMessage = "نام دپارتمان الزامی است.")]
    [MaxLength(200, ErrorMessage = "نام دپارتمان نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string Name { get; set; }

    /// <summary>
    /// کد دپارتمان
    /// </summary>
    [MaxLength(20, ErrorMessage = "کد دپارتمان نمی‌تواند بیش از 20 کاراکتر باشد.")]
    public string Code { get; set; }

    /// <summary>
    /// شناسه کلینیک مرتبط با این دپارتمان
    /// این فیلد ارتباط با جدول کلینیک‌ها را برقرار می‌کند
    /// </summary>
    [Required(ErrorMessage = "کلینیک الزامی است.")]
    public int ClinicId { get; set; }

    /// <summary>
    /// وضعیت فعال/غیرفعال بودن دپارتمان
    /// دپارتمان‌های غیرفعال در سیستم نوبت‌دهی نمایش داده نمی‌شوند
    /// </summary>
    [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
    public bool IsActive { get; set; } = true;

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن دپارتمان
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف دپارتمان
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که دپارتمان را حذف کرده است
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
    /// تاریخ و زمان ایجاد دپارتمان
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که دپارتمان را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین بروزرسانی دپارتمان
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
    /// ارجاع به کلینیک مرتبط با این دپارتمان
    /// این ارتباط برای نمایش اطلاعات کلینیک در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual Clinic Clinic { get; set; }

    /// <summary>
    /// لیست ارتباطات Many-to-Many با پزشکان
    /// این رابطه برای مشخص کردن اینکه کدام پزشکان در این دپارتمان فعالیت می‌کنند استفاده می‌شود
    /// مثال: دکتر احمدی هم در دپارتمان اورژانس و هم در دپارتمان تزریقات فعال است
    /// </summary>
    public virtual ICollection<DoctorDepartment> DoctorDepartments { get; set; } = new HashSet<DoctorDepartment>();

    /// <summary>
    /// لیست دسته‌بندی‌های خدمات مرتبط با این دپارتمان
    /// این لیست برای نمایش تمام دسته‌بندی‌های خدمات موجود در این دپارتمان استفاده می‌شود
    /// مثال: دپارتمان تزریقات شامل "تزریقات عضلانی"، "تزریقات وریدی" و "تزریقات زیبایی"
    /// </summary>
    public virtual ICollection<ServiceCategory> ServiceCategories { get; set; } = new HashSet<ServiceCategory>();

    public string Description { get; set; }

    #endregion
}

#region Department

/// <summary>
/// پیکربندی مدل دپارتمان‌ها برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class DepartmentConfig : EntityTypeConfiguration<Department>
{
    public DepartmentConfig()
    {
        ToTable("Departments");
        HasKey(d => d.DepartmentId);

        // ویژگی‌های اصلی
        Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Department_Name")));

        Property(d => d.ClinicId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Department_ClinicId")));

        Property(d => d.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Department_IsActive")));

        // پیاده‌سازی ISoftDelete
        Property(d => d.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Department_IsDeleted")));

        Property(d => d.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Department_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(d => d.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Department_CreatedAt")));

        Property(d => d.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Department_CreatedByUserId")));

        Property(d => d.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Department_UpdatedAt")));

        Property(d => d.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Department_UpdatedByUserId")));

        Property(d => d.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Department_DeletedByUserId")));

        // روابط
        HasRequired(d => d.Clinic)
            .WithMany(c => c.Departments)
            .HasForeignKey(d => d.ClinicId)
            .WillCascadeOnDelete(false);

        // ✅ رابطه Many-to-Many با پزشکان
        HasMany(d => d.DoctorDepartments)
            .WithRequired(dd => dd.Department)
            .HasForeignKey(dd => dd.DepartmentId)
            .WillCascadeOnDelete(false);

        // رابطه با دسته‌بندی خدمات
        HasMany(d => d.ServiceCategories)
            .WithRequired(sc => sc.Department)
            .HasForeignKey(sc => sc.DepartmentId)
            .WillCascadeOnDelete(false);

        // روابط Audit
        HasOptional(d => d.DeletedByUser)
            .WithMany()
            .HasForeignKey(d => d.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(d => d.CreatedByUser)
            .WithMany()
            .HasForeignKey(d => d.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(d => d.UpdatedByUser)
            .WithMany()
            .HasForeignKey(d => d.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
        HasIndex(d => new { d.ClinicId, d.IsDeleted })
            .HasName("IX_Department_ClinicId_IsDeleted");

        HasIndex(d => new { d.ClinicId, d.IsActive, d.IsDeleted })
            .HasName("IX_Department_ClinicId_IsActive_IsDeleted");
    }
}

#endregion