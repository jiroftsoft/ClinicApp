using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Triage;

/// <summary>
/// مدل ارزیابی مجدد تریاژ - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. ثبت تغییرات سطح تریاژ و علل آن
/// 2. مدیریت اقدامات انجام شده (اکسیژن/ECG/IV/...)
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 4. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// </summary>
public class TriageReassessment : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه ارزیابی مجدد تریاژ
    /// </summary>
    public int TriageReassessmentId { get; set; }

    /// <summary>
    /// شناسه ارزیابی تریاژ اصلی
    /// </summary>
    [Required(ErrorMessage = "ارزیابی تریاژ الزامی است.")]
    public int TriageAssessmentId { get; set; }

    /// <summary>
    /// تاریخ و زمان ارزیابی مجدد
    /// </summary>
    [Required(ErrorMessage = "تاریخ ارزیابی مجدد الزامی است.")]
    public DateTime ReassessmentAt { get; set; }

    /// <summary>
    /// سطح تریاژ جدید (در صورت تغییر)
    /// </summary>
    public TriageLevel? NewLevel { get; set; }

    /// <summary>
    /// سطح تریاژ قبلی
    /// </summary>
    public TriageLevel? PreviousLevel { get; set; }

    /// <summary>
    /// آیا سطح تریاژ تغییر کرده است؟
    /// </summary>
    [NotMapped]
    public bool LevelChanged => NewLevel.HasValue && NewLevel != PreviousLevel;

    /// <summary>
    /// خلاصه تغییرات بالینی
    /// </summary>
    [Required(ErrorMessage = "خلاصه تغییرات الزامی است.")]
    [MaxLength(1000, ErrorMessage = "خلاصه تغییرات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Changes { get; set; }

    /// <summary>
    /// اقدامات انجام شده (اکسیژن/نوار قلب/IV/...)
    /// </summary>
    [MaxLength(500, ErrorMessage = "اقدامات نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Actions { get; set; }

    /// <summary>
    /// علت ارزیابی مجدد
    /// </summary>
    [Required(ErrorMessage = "علت ارزیابی مجدد الزامی است.")]
    public ReassessmentReason Reason { get; set; }

    /// <summary>
    /// شناسه کاربر ارزیابی کننده
    /// </summary>
    [Required(ErrorMessage = "ارزیابی کننده الزامی است.")]
    public string AssessorUserId { get; set; }

    /// <summary>
    /// یادداشت‌های اضافی
    /// </summary>
    [MaxLength(500, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Notes { get; set; }

    #region پیاده‌سازی ISoftDelete (حذف نرم)
    /// <summary>
    /// آیا ارزیابی مجدد حذف شده است؟
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// تاریخ و زمان حذف ارزیابی مجدد
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که ارزیابی مجدد را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد ارزیابی مجدد
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// شناسه کاربری که ارزیابی مجدد را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش ارزیابی مجدد
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که ارزیابی مجدد را ویرایش کرده است
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

    /// <summary>
    /// شناسه کاربری که ارزیابی مجدد را انجام داده است (برای سازگاری)
    /// </summary>
    [NotMapped]
    public string ReassessedByUserId => AssessorUserId;

    /// <summary>
    /// زمان ارزیابی مجدد (UTC) (برای سازگاری)
    /// </summary>
    [NotMapped]
    public DateTime AtUtc => ReassessmentAt;

    /// <summary>
    /// علائم حیاتی (برای سازگاری)
    /// </summary>
    [NotMapped]
    public string Vitals => Notes;
    #endregion

    #region روابط
    /// <summary>
    /// ارجاع به ارزیابی تریاژ اصلی
    /// </summary>
    public virtual TriageAssessment TriageAssessment { get; set; }

    /// <summary>
    /// ارجاع به کاربر ارزیابی کننده
    /// </summary>
    public virtual ApplicationUser Assessor { get; set; }
    #endregion
}

/// <summary>
/// پیکربندی مدل ارزیابی مجدد تریاژ برای Entity Framework
/// </summary>
public class TriageReassessmentConfig : EntityTypeConfiguration<TriageReassessment>
{
    public TriageReassessmentConfig()
    {
        ToTable("TriageReassessments");
        HasKey(tr => tr.TriageReassessmentId);

        // ویژگی‌های اصلی
        Property(tr => tr.TriageAssessmentId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageReassessment_TriageAssessmentId")));

        Property(tr => tr.ReassessmentAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageReassessment_ReassessmentAt")));

        Property(tr => tr.NewLevel)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageReassessment_NewLevel")));

        Property(tr => tr.Reason)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageReassessment_Reason")));

        Property(tr => tr.AssessorUserId)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageReassessment_AssessorUserId")));

        // پیاده‌سازی ISoftDelete
        Property(tr => tr.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageReassessment_IsDeleted")));

        // پیاده‌سازی ITrackable
        Property(tr => tr.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageReassessment_CreatedAt")));

        // ایندکس‌های ترکیبی - در Migration اضافه می‌شوند
        // IX_TriageReassessment_Assessment_Time_IsDeleted
        // IX_TriageReassessment_Level_Reason_IsDeleted
        // IX_TriageReassessment_ReassessmentAt_IsDeleted

        // روابط
        HasRequired(tr => tr.TriageAssessment)
            .WithMany(ta => ta.Reassessments)
            .HasForeignKey(tr => tr.TriageAssessmentId)
            .WillCascadeOnDelete(false);

        HasRequired(tr => tr.Assessor)
            .WithMany()
            .HasForeignKey(tr => tr.AssessorUserId)
            .WillCascadeOnDelete(false);

        // روابط User - صراحت کامل
        Property(tr => tr.CreatedByUserId)
            .HasMaxLength(128);
        HasOptional(tr => tr.CreatedByUser)
            .WithMany()
            .HasForeignKey(tr => tr.CreatedByUserId)
            .WillCascadeOnDelete(false);

        Property(tr => tr.UpdatedByUserId)
            .HasMaxLength(128);
        HasOptional(tr => tr.UpdatedByUser)
            .WithMany()
            .HasForeignKey(tr => tr.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        Property(tr => tr.DeletedByUserId)
            .HasMaxLength(128);
        HasOptional(tr => tr.DeletedByUser)
            .WithMany()
            .HasForeignKey(tr => tr.DeletedByUserId)
            .WillCascadeOnDelete(false);
    }
}
