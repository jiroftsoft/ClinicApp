using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Doctor;

namespace ClinicApp.Models.Entities.Triage;

/// <summary>
/// مدل صف تریاژ - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت صف تریاژ بیماران با اولویت‌بندی هوشمند
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class TriageQueue : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه صف تریاژ
    /// </summary>
    public int TriageQueueId { get; set; }

    /// <summary>
    /// شماره صف (برای نمایش به بیمار)
    /// </summary>
    public string QueueNumber => $"Q{TriageQueueId:D4}";

    /// <summary>
    /// شناسه بیمار
    /// </summary>
    [Required(ErrorMessage = "بیمار الزامی است.")]
    public int PatientId { get; set; }

    /// <summary>
    /// شناسه ارزیابی تریاژ
    /// </summary>
    [Required(ErrorMessage = "ارزیابی تریاژ الزامی است.")]
    public int TriageAssessmentId { get; set; }

    /// <summary>
    /// اولویت صف (1-10)
    /// </summary>
    [Required(ErrorMessage = "اولویت الزامی است.")]
    [Range(1, 10, ErrorMessage = "اولویت باید بین 1 تا 10 باشد.")]
    public int Priority { get; set; }

    /// <summary>
    /// تاریخ و زمان ورود به صف
    /// </summary>
    [Required(ErrorMessage = "تاریخ ورود به صف الزامی است.")]
    public DateTime QueueTime { get; set; }

    /// <summary>
    /// تاریخ و زمان فراخوانی
    /// </summary>
    public DateTime? CalledTime { get; set; }

    /// <summary>
    /// تاریخ و زمان تکمیل
    /// </summary>
    public DateTime? CompletedTime { get; set; }

    /// <summary>
    /// وضعیت صف
    /// </summary>
    [Required(ErrorMessage = "وضعیت صف الزامی است.")]
    public TriageQueueStatus Status { get; set; } = TriageQueueStatus.Waiting;

    /// <summary>
    /// زمان انتظار (به دقیقه)
    /// </summary>
    public int? WaitTimeMinutes { get; set; }

    /// <summary>
    /// زمان انتظار پیش‌بینی شده (به دقیقه) - منبع حقیقت
    /// </summary>
    public int? EstimatedWaitTimeMinutes { get; set; }

    /// <summary>
    /// شناسه دپارتمان مقصد
    /// </summary>
    public int? TargetDepartmentId { get; set; }

    /// <summary>
    /// شناسه پزشک مقصد
    /// </summary>
    public int? TargetDoctorId { get; set; }

    /// <summary>
    /// یادداشت‌های صف
    /// </summary>
    [MaxLength(500, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string QueueNotes { get; set; }

    /// <summary>
    /// آیا بیمار در صف اولویت دارد؟
    /// </summary>
    public bool HasPriority { get; set; } = false;

    /// <summary>
    /// آیا بیمار نیاز به مراقبت فوری دارد؟
    /// </summary>
    public bool RequiresImmediateCare { get; set; } = false;

    /// <summary>
    /// زمان ارزیابی مجدد بعدی (بر اساس ESI)
    /// </summary>
    public DateTime? NextReassessmentDueAt { get; set; }

    /// <summary>
    /// تعداد ارزیابی‌های مجدد انجام شده
    /// </summary>
    public int ReassessmentCount { get; set; } = 0;

    /// <summary>
    /// آخرین زمان ارزیابی مجدد
    /// </summary>
    public DateTime? LastReassessmentAt { get; set; }

    /// <summary>
    /// آیا بیمار نیاز به انتقال به بخش ویژه دارد؟
    /// </summary>
    public bool RequiresICU { get; set; } = false;

    /// <summary>
    /// آیا بیمار نیاز به جراحی فوری دارد؟
    /// </summary>
    public bool RequiresEmergencySurgery { get; set; } = false;

    /// <summary>
    /// شناسه کاربری که بیمار را فراخوانده است
    /// </summary>
    public string CalledByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر فراخواننده
    /// </summary>
    public virtual ApplicationUser CalledByUser { get; set; }

    /// <summary>
    /// شناسه کاربری که بیمار را تکمیل کرده است
    /// </summary>
    public string CompletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر تکمیل کننده
    /// </summary>
    public virtual ApplicationUser CompletedByUser { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن صف تریاژ
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف صف تریاژ
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که صف تریاژ را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد صف تریاژ
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// شناسه کاربری که صف تریاژ را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش صف تریاژ
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که صف تریاژ را ویرایش کرده است
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
    /// ارجاع به بیمار
    /// </summary>
    public virtual Patient.Patient Patient { get; set; }

    /// <summary>
    /// ارجاع به ارزیابی تریاژ
    /// </summary>
    public virtual TriageAssessment TriageAssessment { get; set; }

    /// <summary>
    /// ارجاع به دپارتمان مقصد
    /// </summary>
    public virtual Clinic.Department TargetDepartment { get; set; }

    /// <summary>
    /// ارجاع به پزشک مقصد
    /// </summary>
    public virtual Doctor.Doctor TargetDoctor { get; set; }
    #endregion

    #region محاسبات
    /// <summary>
    /// محاسبه زمان انتظار (به دقیقه) - فقط برای کش/گزارش
    /// </summary>
    [NotMapped]
    public int? CalculatedWaitTime
    {
        get
        {
            if (CalledTime.HasValue)
            {
                return (int)(CalledTime.Value - QueueTime).TotalMinutes;
            }
            else if (CompletedTime.HasValue)
            {
                return (int)(CompletedTime.Value - QueueTime).TotalMinutes;
            }
            else
            {
                return (int)(DateTime.UtcNow - QueueTime).TotalMinutes;
            }
        }
    }

    /// <summary>
    /// آیا بیمار در صف انتظار است؟
    /// </summary>
    [NotMapped]
    public bool IsWaiting => Status == TriageQueueStatus.Waiting;

    /// <summary>
    /// آیا بیمار فراخوانده شده است؟
    /// </summary>
    [NotMapped]
    public bool IsCalled => Status == TriageQueueStatus.Called;

    /// <summary>
    /// آیا بیمار تکمیل شده است؟
    /// </summary>
    [NotMapped]
    public bool IsCompleted => Status == TriageQueueStatus.Completed;

    /// <summary>
    /// موقعیت در صف (برای سازگاری)
    /// </summary>
    [NotMapped]
    public int QueuePosition { get; set; }
    #endregion
}

/// <summary>
/// پیکربندی مدل صف تریاژ برای Entity Framework
/// </summary>
public class TriageQueueConfig : EntityTypeConfiguration<TriageQueue>
{
    public TriageQueueConfig()
    {
        ToTable("TriageQueues");
        HasKey(tq => tq.TriageQueueId);

        // ویژگی‌های اصلی
        Property(tq => tq.PatientId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageQueue_PatientId")));

        Property(tq => tq.TriageAssessmentId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageQueue_TriageAssessmentId")));

        Property(tq => tq.Priority)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageQueue_Priority")));

        Property(tq => tq.QueueTime)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageQueue_QueueTime")));

        Property(tq => tq.Status)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageQueue_Status")));

        Property(tq => tq.HasPriority)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageQueue_HasPriority")));

        Property(tq => tq.RequiresImmediateCare)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageQueue_RequiresImmediateCare")));

        Property(tq => tq.NextReassessmentDueAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageQueue_NextReassessmentDueAt")));

        // پیاده‌سازی ISoftDelete
        Property(tq => tq.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageQueue_IsDeleted")));

        // پیاده‌سازی ITrackable
        Property(tq => tq.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageQueue_CreatedAt")));

        // روابط
        HasRequired(tq => tq.Patient)
            .WithMany(p => p.TriageQueues)
            .HasForeignKey(tq => tq.PatientId)
            .WillCascadeOnDelete(false);

        HasRequired(tq => tq.TriageAssessment)
            .WithMany(ta => ta.TriageQueues)
            .HasForeignKey(tq => tq.TriageAssessmentId)
            .WillCascadeOnDelete(false);

        HasOptional(tq => tq.TargetDepartment)
            .WithMany()
            .HasForeignKey(tq => tq.TargetDepartmentId)
            .WillCascadeOnDelete(false);

        HasOptional(tq => tq.TargetDoctor)
            .WithMany()
            .HasForeignKey(tq => tq.TargetDoctorId)
            .WillCascadeOnDelete(false);

        // روابط User - صراحت کامل
        Property(tq => tq.CalledByUserId)
            .HasMaxLength(128);
        HasOptional(tq => tq.CalledByUser)
            .WithMany()
            .HasForeignKey(tq => tq.CalledByUserId)
            .WillCascadeOnDelete(false);

        Property(tq => tq.CompletedByUserId)
            .HasMaxLength(128);
        HasOptional(tq => tq.CompletedByUser)
            .WithMany()
            .HasForeignKey(tq => tq.CompletedByUserId)
            .WillCascadeOnDelete(false);

        Property(tq => tq.CreatedByUserId)
            .HasMaxLength(128);
        HasOptional(tq => tq.CreatedByUser)
            .WithMany()
            .HasForeignKey(tq => tq.CreatedByUserId)
            .WillCascadeOnDelete(false);

        Property(tq => tq.UpdatedByUserId)
            .HasMaxLength(128);
        HasOptional(tq => tq.UpdatedByUser)
            .WithMany()
            .HasForeignKey(tq => tq.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        Property(tq => tq.DeletedByUserId)
            .HasMaxLength(128);
        HasOptional(tq => tq.DeletedByUser)
            .WithMany()
            .HasForeignKey(tq => tq.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی - در Migration اضافه می‌شوند
        // IX_TriageQueue_Status_Priority_IsDeleted
        // IX_TriageQueue_Time_Status_IsDeleted
        // IX_TriageQueue_Patient_Status_IsDeleted
        // IX_TriageQueue_Immediate_Status_IsDeleted
        // IX_TriageQueue_NextReassessment_Status
    }
}
