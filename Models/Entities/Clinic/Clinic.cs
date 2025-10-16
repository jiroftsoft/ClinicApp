using ClinicApp.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Clinic;

/// <summary>
/// مدل کلینیک‌ها - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان → دسته‌بندی خدمات)
/// 2. مدیریت صحیح فیلدهای ردیابی (Audit Trail) برای رعایت استانداردهای پزشکی
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 4. ارتباط با دپارتمان‌ها و پزشکان برای سازماندهی بهتر خدمات
/// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class Clinic : AuditableEntity
{
    /// <summary>
    /// شناسه کلینیک
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int ClinicId { get; set; }

    /// <summary>
    /// نام کلینیک
    /// مثال: "کلینیک تخصصی دندانپزشکی شفا"
    /// </summary>
    [Required(ErrorMessage = "نام کلینیک الزامی است.")]
    [MaxLength(200, ErrorMessage = "نام کلینیک نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string Name { get; set; }

    /// <summary>
    /// آدرس کلینیک
    /// </summary>
    [MaxLength(500, ErrorMessage = "آدرس کلینیک نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Address { get; set; }

    /// <summary>
    /// شماره تلفن کلینیک
    /// </summary>
    [MaxLength(50, ErrorMessage = "شماره تلفن کلینیک نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// کد کلینیک
    /// </summary>
    [MaxLength(20, ErrorMessage = "کد کلینیک نمی‌تواند بیش از 20 کاراکتر باشد.")]
    public string Code { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن کلینیک
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف کلینیک
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که کلینیک را حذف کرده است
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
    /// تاریخ و زمان ایجاد کلینیک
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که کلینیک را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین بروزرسانی کلینیک
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
    /// لیست دپارتمان‌های مرتبط با این کلینیک
    /// این لیست برای نمایش تمام دپارتمان‌های موجود در این کلینیک استفاده می‌شود
    /// </summary>
    public virtual ICollection<Department> Departments { get; set; } = new HashSet<Department>();

    /// <summary>
    /// لیست پزشکان مرتبط با این کلینیک
    /// این لیست برای نمایش تمام پزشکان فعال در این کلینیک استفاده می‌شود
    /// </summary>
    public virtual ICollection<Doctor.Doctor> Doctors { get; set; } = new HashSet<Doctor.Doctor>();

    [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
    public bool IsActive { get; set; } = true;

    #endregion
}
#region Clinic

/// <summary>
/// پیکربندی مدل کلینیک‌ها برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class ClinicConfig : EntityTypeConfiguration<Clinic>
{
    public ClinicConfig()
    {
        ToTable("Clinics");
        HasKey(c => c.ClinicId);

        // ویژگی‌های اصلی
        Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Clinic_Name")));

        Property(c => c.Address)
            .IsOptional()
            .HasMaxLength(500);

        Property(c => c.PhoneNumber)
            .IsOptional()
            .HasMaxLength(50)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Clinic_PhoneNumber")));

        // پیاده‌سازی ISoftDelete
        Property(c => c.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Clinic_IsDeleted")));

        Property(c => c.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Clinic_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Clinic_CreatedAt")));

        Property(c => c.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Clinic_CreatedByUserId")));

        Property(c => c.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Clinic_UpdatedAt")));

        Property(c => c.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Clinic_UpdatedByUserId")));

        Property(c => c.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Clinic_DeletedByUserId")));

        // روابط
        HasMany(c => c.Departments)
            .WithRequired(d => d.Clinic)
            .HasForeignKey(d => d.ClinicId)
            .WillCascadeOnDelete(false);

        HasMany(c => c.Doctors)
            .WithOptional(d => d.Clinic)
            .HasForeignKey(d => d.ClinicId)
            .WillCascadeOnDelete(false);

        HasOptional(c => c.DeletedByUser)
            .WithMany()
            .HasForeignKey(c => c.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(c => c.UpdatedByUser)
            .WithMany()
            .HasForeignKey(c => c.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
        HasIndex(c => new { c.IsDeleted, c.CreatedAt })
            .HasName("IX_Clinic_IsDeleted_CreatedAt");
    }
}

#endregion