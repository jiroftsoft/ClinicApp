using ClinicApp.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.Entities.Triage;

/// <summary>
/// مدل پروتکل تریاژ - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت پروتکل‌های تریاژ استاندارد
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class TriageProtocol : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه پروتکل تریاژ
    /// </summary>
    public int TriageProtocolId { get; set; }

    /// <summary>
    /// کد پروتکل
    /// </summary>
    [Required(ErrorMessage = "کد پروتکل الزامی است.")]
    [MaxLength(50, ErrorMessage = "کد پروتکل نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string ProtocolCode { get; set; }

    /// <summary>
    /// نام پروتکل
    /// </summary>
    [Required(ErrorMessage = "نام پروتکل الزامی است.")]
    [MaxLength(200, ErrorMessage = "نام پروتکل نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string Name { get; set; }

    /// <summary>
    /// توضیحات پروتکل
    /// </summary>
    [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// نوع پروتکل
    /// </summary>
    [Required(ErrorMessage = "نوع پروتکل الزامی است.")]
    public TriageProtocolType Type { get; set; }

    /// <summary>
    /// سطح تریاژ هدف
    /// </summary>
    [Required(ErrorMessage = "سطح تریاژ هدف الزامی است.")]
    public TriageLevel TargetLevel { get; set; }

    /// <summary>
    /// معیارهای اعمال پروتکل (JSON)
    /// </summary>
    [MaxLength(2000, ErrorMessage = "معیارها نمی‌تواند بیش از 2000 کاراکتر باشد.")]
    public string Criteria { get; set; }

    /// <summary>
    /// اقدامات مورد نیاز (JSON)
    /// </summary>
    [MaxLength(2000, ErrorMessage = "اقدامات نمی‌تواند بیش از 2000 کاراکتر باشد.")]
    public string RequiredActions { get; set; }

    /// <summary>
    /// زمان اجرای پروتکل (به دقیقه)
    /// </summary>
    public int? ExecutionTimeMinutes { get; set; }

    /// <summary>
    /// آیا پروتکل فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// تاریخ شروع اعتبار
    /// </summary>
    [Required(ErrorMessage = "تاریخ شروع اعتبار الزامی است.")]
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// تاریخ پایان اعتبار
    /// </summary>
    public DateTime? ValidTo { get; set; }

    /// <summary>
    /// اولویت پروتکل (1-10)
    /// </summary>
    [Range(1, 10, ErrorMessage = "اولویت باید بین 1 تا 10 باشد.")]
    public int Priority { get; set; } = 5;

    /// <summary>
    /// آیا پروتکل اجباری است؟
    /// </summary>
    public bool IsMandatory { get; set; } = false;

    /// <summary>
    /// آیا پروتکل نیاز به تأیید پزشک دارد؟
    /// </summary>
    public bool RequiresDoctorApproval { get; set; } = false;

    /// <summary>
    /// یادداشت‌های اضافی
    /// </summary>
    [MaxLength(500, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Notes { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن پروتکل تریاژ
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف پروتکل تریاژ
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که پروتکل تریاژ را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد پروتکل تریاژ
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// شناسه کاربری که پروتکل تریاژ را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش پروتکل تریاژ
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که پروتکل تریاژ را ویرایش کرده است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }

    /// <summary>
    /// RowVersion برای همزمانی (Concurrency)
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; }
    #endregion

    #region روابط
    /// <summary>
    /// لیست ارزیابی‌های تریاژ که این پروتکل در آن‌ها اعمال شده است
    /// </summary>
    public virtual ICollection<TriageAssessment> AppliedAssessments { get; set; } = new HashSet<TriageAssessment>();

    /// <summary>
    /// نوع پروتکل (برای سازگاری)
    /// </summary>
    [NotMapped]
    public TriageProtocolType ProtocolType => Type;

    /// <summary>
    /// اقدامات (برای سازگاری)
    /// </summary>
    [NotMapped]
    public string Actions => RequiredActions ?? string.Empty;
    #endregion
}

/// <summary>
/// پیکربندی مدل پروتکل تریاژ برای Entity Framework
/// </summary>
public class TriageProtocolConfig : EntityTypeConfiguration<TriageProtocol>
{
    public TriageProtocolConfig()
    {
        ToTable("TriageProtocols");
        HasKey(tp => tp.TriageProtocolId);

        // ویژگی‌های اصلی
        Property(tp => tp.ProtocolCode)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageProtocol_ProtocolCode") { IsUnique = true }));

        Property(tp => tp.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageProtocol_Name")));

        Property(tp => tp.Type)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageProtocol_Type")));

        Property(tp => tp.TargetLevel)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageProtocol_TargetLevel")));

        Property(tp => tp.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageProtocol_IsActive")));

        Property(tp => tp.ValidFrom)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageProtocol_ValidFrom")));

        Property(tp => tp.Priority)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageProtocol_Priority")));

        Property(tp => tp.IsMandatory)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageProtocol_IsMandatory")));

        Property(tp => tp.RequiresDoctorApproval)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageProtocol_RequiresDoctorApproval")));

        // پیاده‌سازی ISoftDelete
        Property(tp => tp.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageProtocol_IsDeleted")));

        // پیاده‌سازی ITrackable
        Property(tp => tp.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageProtocol_CreatedAt")));

        // ایندکس‌های ترکیبی - در Migration اضافه می‌شوند
        // IX_TriageProtocol_Type_Level_Active_IsDeleted
        // IX_TriageProtocol_Validity_Active_IsDeleted
        // IX_TriageProtocol_Priority_Mandatory_Active_IsDeleted
        // IX_TriageProtocol_IsActive_IsDeleted

        // روابط User - صراحت کامل
        Property(tp => tp.CreatedByUserId)
            .HasMaxLength(128);
        HasOptional(tp => tp.CreatedByUser)
            .WithMany()
            .HasForeignKey(tp => tp.CreatedByUserId)
            .WillCascadeOnDelete(false);

        Property(tp => tp.UpdatedByUserId)
            .HasMaxLength(128);
        HasOptional(tp => tp.UpdatedByUser)
            .WithMany()
            .HasForeignKey(tp => tp.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        Property(tp => tp.DeletedByUserId)
            .HasMaxLength(128);
        HasOptional(tp => tp.DeletedByUser)
            .WithMany()
            .HasForeignKey(tp => tp.DeletedByUserId)
            .WillCascadeOnDelete(false);
    }
}
