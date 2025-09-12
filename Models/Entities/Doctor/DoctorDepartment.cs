using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Models.Entities.Doctor;

/// <summary>
/// جدول واسط برای رابطه چند-به-چند بین پزشک و دپارتمان
/// 
/// ویژگی‌های کلیدی:
/// 1. ارتباط Many-to-Many بین Doctor و Department
/// 2. مدیریت Audit کامل (کی، کی ایجاد/ویرایش کرده)
/// 3. کلید ترکیبی (Composite Key) برای جلوگیری از تکرار
/// 4. پشتیبانی از سناریوهای پیچیده (پزشک در چند دپارتمان)
/// 
/// مثال کاربرد:
/// - دکتر احمدی: عضو دپارتمان "اورژانس" و "تزریقات"
/// - دکتر محمدی: عضو دپارتمان "داخلی" و "اورژانس"
/// </summary>
public class DoctorDepartment : ITrackable
{
    /// <summary>
    /// شناسه پزشک - بخشی از کلید ترکیبی
    /// </summary>
    [Required(ErrorMessage = "شناسه پزشک الزامی است.")]
    public int DoctorId { get; set; }

    /// <summary>
    /// شناسه دپارتمان - بخشی از کلید ترکیبی
    /// </summary>
    [Required(ErrorMessage = "شناسه دپارتمان الزامی است.")]
    public int DepartmentId { get; set; }

    /// <summary>
    /// نقش یا سمت پزشک در این دپارتمان (اختیاری)
    /// مثال: "رئیس دپارتمان"، "پزشک معاون"، "پزشک عادی"
    /// </summary>
    [MaxLength(100, ErrorMessage = "نقش نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string Role { get; set; }

    /// <summary>
    /// وضعیت فعال بودن پزشک در این دپارتمان
    /// می‌تواند پزشک در دپارتمان عضو باشد ولی موقتاً غیرفعال باشد
    /// </summary>
    [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// تاریخ شروع فعالیت پزشک در این دپارتمان
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// تاریخ پایان فعالیت پزشک در این دپارتمان (در صورت وجود)
    /// </summary>
    public DateTime? EndDate { get; set; }

    #region پیاده‌سازی ITrackable
    /// <summary>
    /// تاریخ و زمان ایجاد این ارتباط
    /// مهم برای ردیابی زمان اضافه شدن پزشک به دپارتمان
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که این ارتباط را ایجاد کرده
    /// مهم برای سیستم‌های پزشکی - چه کسی پزشک را به دپارتمان اضافه کرده
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش این ارتباط
    /// مهم برای ردیابی تغییرات (مثل تغییر نقش، وضعیت فعال/غیرفعال)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که این ارتباط را ویرایش کرده
    /// مهم برای سیستم‌های پزشکی - چه کسی تغییرات را اعمال کرده
    /// </summary>
    public string UpdatedByUserId { get; set; }
    #endregion

    #region Navigation Properties
    /// <summary>
    /// ارجاع به پزشک مرتبط
    /// برای دسترسی مستقیم به اطلاعات پزشک از طریق این رابطه
    /// </summary>
    public virtual Doctor Doctor { get; set; }

    /// <summary>
    /// ارجاع به دپارتمان مرتبط
    /// برای دسترسی مستقیم به اطلاعات دپارتمان از طریق این رابطه
    /// </summary>
    public virtual Department Department { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// برای دسترسی مستقیم به اطلاعات کاربری که این ارتباط را ایجاد کرده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// برای دسترسی مستقیم به اطلاعات کاربری که این ارتباط را ویرایش کرده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string DeletedByUserId { get; set; }
    public virtual ApplicationUser DeletedByUser { get; set; }

    #endregion
}
#region DoctorDepartment

/// <summary>
/// پیکربندی Entity Framework برای جدول DoctorDepartment
/// این پیکربندی با استانداردهای سیستم‌های پزشکی طراحی شده است
/// </summary>
public class DoctorDepartmentConfig : EntityTypeConfiguration<DoctorDepartment>
{
    public DoctorDepartmentConfig()
    {
        // تنظیمات جدول
        ToTable("DoctorDepartments");

        // کلید ترکیبی (Composite Key)
        HasKey(dd => new { dd.DoctorId, dd.DepartmentId });

        // تنظیمات Property ها
        Property(dd => dd.DoctorId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_DoctorId")));

        Property(dd => dd.DepartmentId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_DepartmentId")));

        Property(dd => dd.Role)
            .IsOptional()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_Role")));

        Property(dd => dd.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_IsActive")));

        Property(dd => dd.StartDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_StartDate")));

        Property(dd => dd.EndDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_EndDate")));

        // تنظیمات ITrackable
        Property(dd => dd.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_CreatedAt")));

        Property(dd => dd.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_CreatedByUserId")));

        Property(dd => dd.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_UpdatedAt")));

        Property(dd => dd.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_UpdatedByUserId")));

        // روابط اصلی
        HasRequired(dd => dd.Doctor)
            .WithMany(d => d.DoctorDepartments)
            .HasForeignKey(dd => dd.DoctorId)
            .WillCascadeOnDelete(false);

        HasRequired(dd => dd.Department)
            .WithMany(d => d.DoctorDepartments)
            .HasForeignKey(dd => dd.DepartmentId)
            .WillCascadeOnDelete(false);

        // روابط Audit
        HasOptional(dd => dd.CreatedByUser)
            .WithMany()
            .HasForeignKey(dd => dd.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(dd => dd.UpdatedByUser)
            .WithMany()
            .HasForeignKey(dd => dd.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(dd => dd.DeletedByUser)
            .WithMany()
            .HasForeignKey(dd => dd.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود کارایی
        HasIndex(dd => new { dd.DoctorId, dd.IsActive })
            .HasName("IX_DoctorDepartment_DoctorId_IsActive");

        HasIndex(dd => new { dd.DepartmentId, dd.IsActive })
            .HasName("IX_DoctorDepartment_DepartmentId_IsActive");

        HasIndex(dd => new { dd.DoctorId, dd.DepartmentId, dd.IsActive })
            .HasName("IX_DoctorDepartment_DoctorId_DepartmentId_IsActive");

        HasIndex(dd => new { dd.StartDate, dd.EndDate })
            .HasName("IX_DoctorDepartment_StartDate_EndDate");
    }
}

#endregion