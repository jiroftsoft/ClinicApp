using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Doctor;

namespace ClinicApp.Models.Entities.Triage;

/// <summary>
/// مدل ارزیابی تریاژ - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت کامل فرآیند تریاژ بیماران با استانداردهای بین‌المللی
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
/// 5. پشتیبانی از علائم حیاتی، شکایت اصلی و ارزیابی اولویت
/// </summary>
public class TriageAssessment : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه ارزیابی تریاژ
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int TriageAssessmentId { get; set; }

    /// <summary>
    /// شماره ارزیابی تریاژ (برای سازگاری با ViewModels)
    /// </summary>
    public string AssessmentNumber => $"T{TriageAssessmentId:D6}";

    /// <summary>
    /// شناسه بیمار
    /// </summary>
    [Required(ErrorMessage = "بیمار الزامی است.")]
    public int PatientId { get; set; }

    /// <summary>
    /// شناسه ارزیاب (پزشک، پرستار یا کاربر بالینی)
    /// </summary>
    [Required(ErrorMessage = "ارزیاب الزامی است.")]
    public string AssessorUserId { get; set; }

    /// <summary>
    /// سطح تریاژ ESI (1-5) - استاندارد بین‌المللی
    /// </summary>
    [Required(ErrorMessage = "سطح تریاژ الزامی است.")]
    public TriageLevel Level { get; set; }

    /// <summary>
    /// امتیاز ESI (Emergency Severity Index) - استاندارد بین‌المللی
    /// </summary>
    [Range(1, 5, ErrorMessage = "امتیاز ESI باید بین 1 تا 5 باشد.")]
    public int? EsiScore { get; set; }

    /// <summary>
    /// امتیاز NEWS2 (National Early Warning Score 2) - برای بزرگسالان
    /// </summary>
    [Range(0, 20, ErrorMessage = "امتیاز NEWS2 باید بین 0 تا 20 باشد.")]
    public int? News2Score { get; set; }

    /// <summary>
    /// امتیاز PEWS (Pediatric Early Warning Score) - برای اطفال
    /// </summary>
    [Range(0, 15, ErrorMessage = "امتیاز PEWS باید بین 0 تا 15 باشد.")]
    public int? PewsScore { get; set; }

    /// <summary>
    /// شکایت اصلی بیمار
    /// </summary>
    [Required(ErrorMessage = "شکایت اصلی الزامی است.")]
    [MaxLength(500, ErrorMessage = "شکایت اصلی نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string ChiefComplaint { get; set; }

    /// <summary>
    /// کد شکایت اصلی (ICD-10-CM یا کد داخلی)
    /// </summary>
    [MaxLength(20, ErrorMessage = "کد شکایت اصلی نمی‌تواند بیش از 20 کاراکتر باشد.")]
    public string ChiefComplaintCode { get; set; }

    /// <summary>
    /// تاریخ و زمان ورود بیمار
    /// </summary>
    [Required(ErrorMessage = "زمان ورود الزامی است.")]
    public DateTime ArrivalAt { get; set; }

    /// <summary>
    /// تاریخ و زمان شروع تریاژ
    /// </summary>
    [Required(ErrorMessage = "زمان شروع تریاژ الزامی است.")]
    public DateTime TriageStartAt { get; set; }

    /// <summary>
    /// تاریخ و زمان پایان تریاژ
    /// </summary>
    public DateTime? TriageEndAt { get; set; }

    /// <summary>
    /// تاریخ و زمان اولین تماس با پزشک
    /// </summary>
    public DateTime? FirstPhysicianContactAt { get; set; }

    /// <summary>
    /// آیا ارزیابی تریاژ باز است؟ (یک ارزیابی فعال برای هر بیمار)
    /// </summary>
    public bool IsOpen { get; set; } = true;

    /// <summary>
    /// وضعیت ارزیابی تریاژ
    /// </summary>
    [Required(ErrorMessage = "وضعیت ارزیابی الزامی است.")]
    public TriageStatus Status { get; set; } = TriageStatus.Pending;

    /// <summary>
    /// یادداشت‌های ارزیابی
    /// </summary>
    [MaxLength(1000, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string AssessmentNotes { get; set; }

    /// <summary>
    /// علائم بیمار
    /// </summary>
    [MaxLength(500, ErrorMessage = "علائم نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Symptoms { get; set; }

    /// <summary>
    /// سابقه پزشکی بیمار
    /// </summary>
    [MaxLength(1000, ErrorMessage = "سابقه پزشکی نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string MedicalHistory { get; set; }

    /// <summary>
    /// حساسیت‌های بیمار
    /// </summary>
    [MaxLength(500, ErrorMessage = "حساسیت‌ها نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Allergies { get; set; }

    /// <summary>
    /// داروهای مصرفی بیمار
    /// </summary>
    [MaxLength(1000, ErrorMessage = "داروها نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Medications { get; set; }

    /// <summary>
    /// آیا بیمار نیاز به مراقبت فوری دارد؟
    /// </summary>
    public bool RequiresImmediateCare { get; set; } = false;

    /// <summary>
    /// آیا بیمار نیاز به انتقال به بخش ویژه دارد؟
    /// </summary>
    public bool RequiresICU { get; set; } = false;

    /// <summary>
    /// آیا بیمار نیاز به جراحی فوری دارد؟
    /// </summary>
    public bool RequiresEmergencySurgery { get; set; } = false;

    #region Red Flags - پرچم‌های خطر حیاتی

    /// <summary>
    /// پرچم خطر سپسیس
    /// </summary>
    public bool RedFlag_Sepsis { get; set; } = false;

    /// <summary>
    /// پرچم خطر سکته مغزی (FAST-Stroke)
    /// </summary>
    public bool RedFlag_Stroke { get; set; } = false;

    /// <summary>
    /// پرچم خطر سندرم حاد کرونری (ACS)
    /// </summary>
    public bool RedFlag_ACS { get; set; } = false;

    /// <summary>
    /// پرچم خطر تروما
    /// </summary>
    public bool RedFlag_Trauma { get; set; } = false;

    /// <summary>
    /// آیا بیمار باردار است؟
    /// </summary>
    public bool IsPregnant { get; set; } = false;

    /// <summary>
    /// نوع ایزولاسیون (Contact/Droplet/Airborne)
    /// </summary>
    public IsolationType? Isolation { get; set; }

    #endregion

    /// <summary>
    /// زمان انتظار پیش‌بینی شده (به دقیقه)
    /// </summary>
    public int? EstimatedWaitTimeMinutes { get; set; }

    /// <summary>
    /// شناسه دپارتمان پیشنهادی
    /// </summary>
    public int? RecommendedDepartmentId { get; set; }

    /// <summary>
    /// شناسه پزشک پیشنهادی
    /// </summary>
    public int? RecommendedDoctorId { get; set; }

    /// <summary>
    /// اولویت عددی (1-10)
    /// </summary>
    [Range(1, 10, ErrorMessage = "اولویت باید بین 1 تا 10 باشد.")]
    public int Priority { get; set; } = 5;

    /// <summary>
    /// امتیاز تریاژ (محاسبه شده)
    /// </summary>
    public decimal? TriageScore { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن ارزیابی تریاژ
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف ارزیابی تریاژ
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که ارزیابی تریاژ را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد ارزیابی تریاژ
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// شناسه کاربری که ارزیابی تریاژ را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش ارزیابی تریاژ
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که ارزیابی تریاژ را ویرایش کرده است
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
    /// ارجاع به ارزیاب (پزشک، پرستار یا کاربر بالینی)
    /// </summary>
    public virtual ApplicationUser Assessor { get; set; }

    /// <summary>
    /// ارجاع به دپارتمان پیشنهادی
    /// </summary>
    public virtual Clinic.Department RecommendedDepartment { get; set; }

    /// <summary>
    /// ارجاع به پزشک پیشنهادی
    /// </summary>
    public virtual Doctor.Doctor RecommendedDoctor { get; set; }

    /// <summary>
    /// لیست علائم حیاتی
    /// </summary>
    public virtual ICollection<TriageVitalSigns> VitalSigns { get; set; } = new HashSet<TriageVitalSigns>();

    /// <summary>
    /// لیست صف تریاژ
    /// </summary>
    public virtual ICollection<TriageQueue> TriageQueues { get; set; } = new HashSet<TriageQueue>();

    /// <summary>
    /// لیست پروتکل‌های اعمال شده
    /// </summary>
    public virtual ICollection<TriageProtocol> AppliedProtocols { get; set; } = new HashSet<TriageProtocol>();

    /// <summary>
    /// پروتکل‌های اعمال شده (برای سازگاری)
    /// </summary>
    [NotMapped]
    public virtual ICollection<TriageProtocol> Protocols => AppliedProtocols;

    /// <summary>
    /// آخرین ارزیابی مجدد
    /// </summary>
    [NotMapped]
    public DateTime? LastReassessmentAt { get; set; }

    /// <summary>
    /// تعداد ارزیابی‌های مجدد
    /// </summary>
    [NotMapped]
    public int ReassessmentCount { get; set; }

    /// <summary>
    /// لیست ارزیابی‌های مجدد
    /// </summary>
    public virtual ICollection<TriageReassessment> Reassessments { get; set; } = new HashSet<TriageReassessment>();
    #endregion
}

/// <summary>
/// پیکربندی مدل ارزیابی تریاژ برای Entity Framework
/// </summary>
public class TriageAssessmentConfig : EntityTypeConfiguration<TriageAssessment>
{
    public TriageAssessmentConfig()
    {
        ToTable("TriageAssessments");
        HasKey(ta => ta.TriageAssessmentId);

        // ویژگی‌های اصلی
        Property(ta => ta.PatientId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageAssessment_PatientId")));

        Property(ta => ta.AssessorUserId)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageAssessment_AssessorUserId")));

        Property(ta => ta.Level)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageAssessment_Level")));

        Property(ta => ta.Status)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageAssessment_Status")));

        Property(ta => ta.TriageStartAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageAssessment_TriageStartAt")));

        Property(ta => ta.IsOpen)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageAssessment_IsOpen")));

        Property(ta => ta.Priority)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageAssessment_Priority")));

        // پیاده‌سازی ISoftDelete
        Property(ta => ta.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageAssessment_IsDeleted")));

        // پیاده‌سازی ITrackable
        Property(ta => ta.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageAssessment_CreatedAt")));

        // روابط
        HasRequired(ta => ta.Patient)
            .WithMany(p => p.TriageAssessments)
            .HasForeignKey(ta => ta.PatientId)
            .WillCascadeOnDelete(false);

        HasRequired(ta => ta.Assessor)
            .WithMany()
            .HasForeignKey(ta => ta.AssessorUserId)
            .WillCascadeOnDelete(false);

        HasOptional(ta => ta.RecommendedDepartment)
            .WithMany()
            .HasForeignKey(ta => ta.RecommendedDepartmentId)
            .WillCascadeOnDelete(false);

        HasOptional(ta => ta.RecommendedDoctor)
            .WithMany(d => d.RecommendedTriageAssessments)
            .HasForeignKey(ta => ta.RecommendedDoctorId)
            .WillCascadeOnDelete(false);

        // روابط User - صراحت کامل
        Property(ta => ta.CreatedByUserId)
            .HasMaxLength(128);
        HasOptional(ta => ta.CreatedByUser)
            .WithMany()
            .HasForeignKey(ta => ta.CreatedByUserId)
            .WillCascadeOnDelete(false);

        Property(ta => ta.UpdatedByUserId)
            .HasMaxLength(128);
        HasOptional(ta => ta.UpdatedByUser)
            .WithMany()
            .HasForeignKey(ta => ta.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        Property(ta => ta.DeletedByUserId)
            .HasMaxLength(128);
        HasOptional(ta => ta.DeletedByUser)
            .WithMany()
            .HasForeignKey(ta => ta.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // نگاشت Many-to-Many برای پروتکل‌های تریاژ
        HasMany(ta => ta.AppliedProtocols)
            .WithMany(tp => tp.AppliedAssessments)
            .Map(m =>
            {
                m.ToTable("TriageAssessmentProtocols");
                m.MapLeftKey("TriageAssessmentId");
                m.MapRightKey("TriageProtocolId");
            });

        // ایندکس‌های ترکیبی - در Migration اضافه می‌شوند
        // IX_TriageAssessment_Level_Status_IsDeleted
        // IX_TriageAssessment_Time_Level_IsDeleted
        // IX_TriageAssessment_Patient_Time_IsDeleted
        // IX_TriageAssessment_IsOpen_PatientId
    }
}
