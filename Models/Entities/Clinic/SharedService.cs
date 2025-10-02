using System;
using ClinicApp.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Clinic;

/// <summary>
/// مدل خدمات مشترک - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت خدمات مشترک بین دپارتمان‌ها (مثل ویزیت پزشک متخصص، ویزیت پزشک عمومی)
/// 2. هر خدمت مشترک می‌تواند در چندین دپارتمان استفاده شود
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class SharedService : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه خدمت مشترک
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int SharedServiceId { get; set; }

    /// <summary>
    /// شناسه خدمت اصلی
    /// این فیلد ارتباط با جدول خدمات را برقرار می‌کند
    /// </summary>
    [Required(ErrorMessage = "خدمت الزامی است.")]
    public int ServiceId { get; set; }

    /// <summary>
    /// شناسه دپارتمان
    /// این فیلد مشخص می‌کند که این خدمت در کدام دپارتمان قابل استفاده است
    /// </summary>
    [Required(ErrorMessage = "دپارتمان الزامی است.")]
    public int DepartmentId { get; set; }

    /// <summary>
    /// آیا این خدمت در این دپارتمان فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// کای فنی Override برای این دپارتمان
    /// اگر null باشد، از کای فنی اصلی خدمت استفاده می‌شود
    /// </summary>
    [Column(TypeName = "decimal")]
    public decimal? OverrideTechnicalFactor { get; set; }

    /// <summary>
    /// کای حرفه‌ای Override برای این دپارتمان
    /// اگر null باشد، از کای حرفه‌ای اصلی خدمت استفاده می‌شود
    /// </summary>
    [Column(TypeName = "decimal")]
    public decimal? OverrideProfessionalFactor { get; set; }

    /// <summary>
    /// توضیحات خاص این دپارتمان برای این خدمت
    /// </summary>
    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string DepartmentSpecificNotes { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن خدمت مشترک
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف خدمت مشترک
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که خدمت مشترک را حذف کرده است
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
    /// تاریخ و زمان ایجاد خدمت مشترک
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که خدمت مشترک را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین بروزرسانی خدمت مشترک
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
    /// ارجاع به خدمت اصلی
    /// این ارتباط برای نمایش اطلاعات خدمت در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual Service Service { get; set; }

    /// <summary>
    /// ارجاع به دپارتمان
    /// این ارتباط برای نمایش اطلاعات دپارتمان در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual Department Department { get; set; }
    #endregion
}

#region SharedService

/// <summary>
/// پیکربندی مدل خدمت مشترک برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
/// <summary>پیکربندی EF6 برای SharedService</summary>
public class SharedServiceConfig : EntityTypeConfiguration<SharedService>
{
    public SharedServiceConfig()
    {
        ToTable("SharedServices");
        HasKey(ss => ss.SharedServiceId);

        // کلید خارجی‌ها + ایندکس‌های ساده
        Property(ss => ss.ServiceId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_ServiceId")));

        Property(ss => ss.DepartmentId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_DepartmentId")));

        Property(ss => ss.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_IsActive")));

        // Precision ضرایب Override
        Property(ss => ss.OverrideTechnicalFactor)
            .IsOptional()
            .HasPrecision(18, 4);
        Property(ss => ss.OverrideProfessionalFactor)
            .IsOptional()
            .HasPrecision(18, 4);

        Property(ss => ss.DepartmentSpecificNotes)
            .IsOptional()
            .HasMaxLength(500);

        // Soft Delete + Audit ایندکس‌ها
        Property(ss => ss.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_IsDeleted")));

        Property(ss => ss.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_DeletedAt")));

        Property(ss => ss.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_CreatedAt")));

        Property(ss => ss.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(450)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_CreatedByUserId")));

        Property(ss => ss.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_UpdatedAt")));

        Property(ss => ss.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(450)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_UpdatedByUserId")));

        Property(ss => ss.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(450)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_DeletedByUserId")));

        // روابط
        HasRequired(ss => ss.Service)
            .WithMany(s => s.SharedServices)
            .HasForeignKey(ss => ss.ServiceId)
            .WillCascadeOnDelete(false);

        HasRequired(ss => ss.Department)
            .WithMany()
            .HasForeignKey(ss => ss.DepartmentId)
            .WillCascadeOnDelete(false);

        HasOptional(ss => ss.DeletedByUser).WithMany().HasForeignKey(ss => ss.DeletedByUserId).WillCascadeOnDelete(false);
        HasOptional(ss => ss.CreatedByUser).WithMany().HasForeignKey(ss => ss.CreatedByUserId).WillCascadeOnDelete(false);
        HasOptional(ss => ss.UpdatedByUser).WithMany().HasForeignKey(ss => ss.UpdatedByUserId).WillCascadeOnDelete(false);

        // =============================
        // ایندکس‌های مرکب (EF6-style)
        // =============================

        // 1) Unique: جلوگیری از رکورد تکراری برای هر خدمت/دپارتمان (با درنظرگرفتن SoftDelete)
        Property(ss => ss.ServiceId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_Service_Department_Deleted", 1) { IsUnique = true }));
        Property(ss => ss.DepartmentId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_Service_Department_Deleted", 2) { IsUnique = true }));
        Property(ss => ss.IsDeleted)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_Service_Department_Deleted", 3) { IsUnique = true }));

        // 2) برای لیست‌گیری سریع در یک دپارتمان (Active + NotDeleted)
        Property(ss => ss.DepartmentId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_Department_Active_Deleted", 1)));
        Property(ss => ss.IsActive)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_Department_Active_Deleted", 2)));
        Property(ss => ss.IsDeleted)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_SharedService_Department_Active_Deleted", 3)));
    }
}

#endregion
