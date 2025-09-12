using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Patient;

/// <summary>
/// مدل تاریخچه پزشکی بیماران - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت کامل تاریخچه پزشکی بیماران با توجه به استانداردهای سیستم‌های درمانی
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
/// 5. دسته‌بندی انواع مختلف تاریخچه پزشکی (بیماری، جراحی، دارو، آلرژی و...)
/// </summary>
public class MedicalHistory : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه تاریخچه پزشکی
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int MedicalHistoryId { get; set; }

    /// <summary>
    /// شناسه بیمار
    /// </summary>
    [Required(ErrorMessage = "بیمار الزامی است.")]
    public int PatientId { get; set; }

    /// <summary>
    /// نوع تاریخچه پزشکی
    /// </summary>
    [Required(ErrorMessage = "نوع تاریخچه پزشکی الزامی است.")]
    public MedicalHistoryType Type { get; set; }

    /// <summary>
    /// عنوان تاریخچه پزشکی
    /// </summary>
    [Required(ErrorMessage = "عنوان الزامی است.")]
    [MaxLength(200, ErrorMessage = "عنوان نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string Title { get; set; }

    /// <summary>
    /// توضیحات کامل
    /// </summary>
    [MaxLength(2000, ErrorMessage = "توضیحات نمی‌تواند بیش از 2000 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// تاریخ شروع
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// تاریخ پایان
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// آیا هنوز فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// شدت (برای بیماری‌ها)
    /// </summary>
    [MaxLength(50, ErrorMessage = "شدت نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string Severity { get; set; }

    /// <summary>
    /// نام پزشک معالج
    /// </summary>
    [MaxLength(100, ErrorMessage = "نام پزشک نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string DoctorName { get; set; }

    /// <summary>
    /// نام بیمارستان یا مرکز درمانی
    /// </summary>
    [MaxLength(200, ErrorMessage = "نام مرکز درمانی نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string MedicalCenter { get; set; }

    /// <summary>
    /// فایل‌های ضمیمه (مسیر فایل‌ها)
    /// </summary>
    [MaxLength(1000, ErrorMessage = "مسیر فایل‌ها نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Attachments { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن تاریخچه پزشکی
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// تاریخ و زمان حذف تاریخچه پزشکی
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که تاریخچه پزشکی را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد تاریخچه پزشکی
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که تاریخچه پزشکی را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش تاریخچه پزشکی
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که تاریخچه پزشکی را ویرایش کرده است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    #region روابط
    /// <summary>
    /// ارجاع به بیمار
    /// </summary>
    public virtual Patient Patient { get; set; }
    #endregion
}

#region MedicalHistory

/// <summary>
/// پیکربندی مدل تاریخچه پزشکی برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class MedicalHistoryConfig : EntityTypeConfiguration<MedicalHistory>
{
    public MedicalHistoryConfig()
    {
        ToTable("MedicalHistories");
        HasKey(mh => mh.MedicalHistoryId);

        // ویژگی‌های اصلی
        Property(mh => mh.Type)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_MedicalHistory_Type")));

        Property(mh => mh.Title)
            .IsRequired()
            .HasMaxLength(200);

        Property(mh => mh.Description)
            .IsOptional()
            .HasMaxLength(2000);

        Property(mh => mh.StartDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_MedicalHistory_StartDate")));

        Property(mh => mh.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_MedicalHistory_IsActive")));

        // پیاده‌سازی ISoftDelete
        Property(mh => mh.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_MedicalHistory_IsDeleted")));

        Property(mh => mh.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_MedicalHistory_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(mh => mh.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_MedicalHistory_CreatedAt")));

        Property(mh => mh.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_MedicalHistory_CreatedByUserId")));

        Property(mh => mh.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_MedicalHistory_UpdatedAt")));

        Property(mh => mh.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_MedicalHistory_UpdatedByUserId")));

        Property(mh => mh.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_MedicalHistory_DeletedByUserId")));

        // روابط
        HasRequired(mh => mh.Patient)
            .WithMany(p => p.MedicalHistories)
            .HasForeignKey(mh => mh.PatientId)
            .WillCascadeOnDelete(false);

        HasOptional(mh => mh.CreatedByUser)
            .WithMany()
            .HasForeignKey(mh => mh.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(mh => mh.UpdatedByUser)
            .WithMany()
            .HasForeignKey(mh => mh.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(mh => mh.DeletedByUser)
            .WithMany()
            .HasForeignKey(mh => mh.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد
        HasIndex(mh => new { mh.PatientId, mh.Type, mh.IsActive })
            .HasName("IX_MedicalHistory_PatientId_Type_IsActive");

        HasIndex(mh => new { mh.PatientId, mh.IsDeleted })
            .HasName("IX_MedicalHistory_PatientId_IsDeleted");

        HasIndex(mh => new { mh.Type, mh.StartDate, mh.IsActive })
            .HasName("IX_MedicalHistory_Type_StartDate_IsActive");
    }
}

#endregion