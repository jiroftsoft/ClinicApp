using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Report;

/// <summary>
/// مدل گزارش‌های سیستم - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت کامل گزارش‌های سیستم با توجه به استانداردهای سیستم‌های درمانی
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات گزارش‌ها
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
/// 5. دسته‌بندی انواع مختلف گزارش‌ها (مالی، بیماران، نوبت‌ها، پذیرش و...)
/// </summary>
public class Report : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه گزارش
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// نام گزارش
    /// </summary>
    [Required(ErrorMessage = "نام گزارش الزامی است.")]
    [MaxLength(200, ErrorMessage = "نام گزارش نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string Name { get; set; }

    /// <summary>
    /// نوع گزارش
    /// </summary>
    [Required(ErrorMessage = "نوع گزارش الزامی است.")]
    public ReportType Type { get; set; }

    /// <summary>
    /// توضیحات گزارش
    /// </summary>
    [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// پارامترهای گزارش (JSON)
    /// </summary>
    [MaxLength(2000, ErrorMessage = "پارامترها نمی‌تواند بیش از 2000 کاراکتر باشد.")]
    public string Parameters { get; set; }

    /// <summary>
    /// تاریخ شروع گزارش
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// تاریخ پایان گزارش
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// مسیر فایل گزارش
    /// </summary>
    [MaxLength(500, ErrorMessage = "مسیر فایل نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string FilePath { get; set; }

    /// <summary>
    /// وضعیت تولید گزارش
    /// </summary>
    [MaxLength(50, ErrorMessage = "وضعیت نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// تعداد رکوردهای گزارش
    /// </summary>
    public int? RecordCount { get; set; }

    /// <summary>
    /// حجم فایل گزارش (بایت)
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// آیا گزارش قابل دانلود است؟
    /// </summary>
    public bool IsDownloadable { get; set; } = true;

    /// <summary>
    /// تاریخ انقضای گزارش
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن گزارش
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// تاریخ و زمان حذف گزارش
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که گزارش را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد گزارش
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که گزارش را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش گزارش
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که گزارش را ویرایش کرده است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion
}
/// <summary>
/// پیکربندی مدل گزارش‌ها برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class ReportConfig : EntityTypeConfiguration<Report>
{
    public ReportConfig()
    {
        ToTable("Reports");
        HasKey(r => r.ReportId);

        // ویژگی‌های اصلی
        Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        Property(r => r.Type)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Report_Type")));

        Property(r => r.Description)
            .IsOptional()
            .HasMaxLength(1000);

        Property(r => r.Parameters)
            .IsOptional()
            .HasMaxLength(2000);

        Property(r => r.StartDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Report_StartDate")));

        Property(r => r.EndDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Report_EndDate")));

        Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Report_Status")));

        Property(r => r.IsDownloadable)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Report_IsDownloadable")));

        // پیاده‌سازی ISoftDelete
        Property(r => r.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Report_IsDeleted")));

        Property(r => r.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Report_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(r => r.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Report_CreatedAt")));

        Property(r => r.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Report_CreatedByUserId")));

        Property(r => r.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Report_UpdatedAt")));

        Property(r => r.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Report_UpdatedByUserId")));

        Property(r => r.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Report_DeletedByUserId")));

        // روابط
        HasOptional(r => r.CreatedByUser)
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(r => r.UpdatedByUser)
            .WithMany()
            .HasForeignKey(r => r.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(r => r.DeletedByUser)
            .WithMany()
            .HasForeignKey(r => r.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد
        HasIndex(r => new { r.Type, r.Status, r.IsDeleted })
            .HasName("IX_Report_Type_Status_IsDeleted");

        HasIndex(r => new { r.CreatedByUserId, r.CreatedAt })
            .HasName("IX_Report_CreatedByUserId_CreatedAt");

        HasIndex(r => new { r.StartDate, r.EndDate, r.Type })
            .HasName("IX_Report_StartDate_EndDate_Type");
    }
}