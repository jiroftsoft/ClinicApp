using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Triage;

/// <summary>
/// مدل علائم حیاتی تریاژ - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت کامل علائم حیاتی بیماران در فرآیند تریاژ
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class TriageVitalSigns : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه علائم حیاتی تریاژ
    /// </summary>
    public int TriageVitalSignsId { get; set; }

    /// <summary>
    /// شناسه ارزیابی تریاژ
    /// </summary>
    [Required(ErrorMessage = "ارزیابی تریاژ الزامی است.")]
    public int TriageAssessmentId { get; set; }

    /// <summary>
    /// فشار خون سیستولیک (mmHg)
    /// </summary>
    [Range(50, 300, ErrorMessage = "فشار خون سیستولیک باید بین 50 تا 300 باشد.")]
    public int? SystolicBP { get; set; }

    /// <summary>
    /// فشار خون دیاستولیک (mmHg)
    /// </summary>
    [Range(30, 200, ErrorMessage = "فشار خون دیاستولیک باید بین 30 تا 200 باشد.")]
    public int? DiastolicBP { get; set; }

    /// <summary>
    /// ضربان قلب (bpm)
    /// </summary>
    [Range(30, 300, ErrorMessage = "ضربان قلب باید بین 30 تا 300 باشد.")]
    public int? HeartRate { get; set; }

    /// <summary>
    /// دمای بدن (درجه سانتی‌گراد)
    /// </summary>
    [Range(30, 45, ErrorMessage = "دمای بدن باید بین 30 تا 45 درجه سانتی‌گراد باشد.")]
    public decimal? Temperature { get; set; }

    /// <summary>
    /// تعداد تنفس (breaths per minute)
    /// </summary>
    [Range(5, 60, ErrorMessage = "تعداد تنفس باید بین 5 تا 60 باشد.")]
    public int? RespiratoryRate { get; set; }

    /// <summary>
    /// اشباع اکسیژن خون (SpO2) - درصد
    /// </summary>
    [Range(0, 100, ErrorMessage = "اشباع اکسیژن باید بین 0 تا 100 درصد باشد.")]
    public int? OxygenSaturation { get; set; }

    /// <summary>
    /// سطح هوشیاری (Glasgow Coma Scale) - محاسبه شده از GCS جزء به جزء
    /// </summary>
    [NotMapped]
    public int? ConsciousnessLevel => GcsTotal;

    /// <summary>
    /// درد (مقیاس 0-10)
    /// </summary>
    [Range(0, 10, ErrorMessage = "سطح درد باید بین 0 تا 10 باشد.")]
    public int? PainLevel { get; set; }

    /// <summary>
    /// وزن (کیلوگرم)
    /// </summary>
    [Range(0, 500, ErrorMessage = "وزن باید بین 0 تا 500 کیلوگرم باشد.")]
    public decimal? Weight { get; set; }

    /// <summary>
    /// قد (سانتی‌متر)
    /// </summary>
    [Range(0, 300, ErrorMessage = "قد باید بین 0 تا 300 سانتی‌متر باشد.")]
    public decimal? Height { get; set; }

    /// <summary>
    /// شاخص توده بدنی (BMI) - محاسبه شده
    /// </summary>
    [NotMapped]
    public decimal? BMI
    {
        get
        {
            if (Weight.HasValue && Height.HasValue && Height > 0)
            {
                var heightInMeters = Height.Value / 100;
                return Math.Round(Weight.Value / (heightInMeters * heightInMeters), 2);
            }
            return null;
        }
    }

    #region GCS (Glasgow Coma Scale) - جزء به جزء

    /// <summary>
    /// GCS - Eye Opening (1-4)
    /// </summary>
    [Range(1, 4, ErrorMessage = "GCS Eye Opening باید بین 1 تا 4 باشد.")]
    public int? GcsE { get; set; }

    /// <summary>
    /// GCS - Verbal Response (1-5)
    /// </summary>
    [Range(1, 5, ErrorMessage = "GCS Verbal Response باید بین 1 تا 5 باشد.")]
    public int? GcsV { get; set; }

    /// <summary>
    /// GCS - Motor Response (1-6)
    /// </summary>
    [Range(1, 6, ErrorMessage = "GCS Motor Response باید بین 1 تا 6 باشد.")]
    public int? GcsM { get; set; }

    /// <summary>
    /// GCS Total - محاسبه شده
    /// </summary>
    [NotMapped]
    public int? GcsTotal => (GcsE ?? 0) + (GcsV ?? 0) + (GcsM ?? 0);

    #endregion

    #region اکسیژن و راه هوایی

    /// <summary>
    /// آیا بیمار روی اکسیژن است؟
    /// </summary>
    public bool OnOxygen { get; set; } = false;

    /// <summary>
    /// نوع دستگاه اکسیژن
    /// </summary>
    public OxygenDevice? OxygenDevice { get; set; }

    /// <summary>
    /// دبی اکسیژن (لیتر در دقیقه)
    /// </summary>
    [Range(0, 50, ErrorMessage = "دبی اکسیژن باید بین 0 تا 50 باشد.")]
    public decimal? O2FlowLpm { get; set; }

    #endregion

    /// <summary>
    /// علت ارزیابی مجدد
    /// </summary>
    public ReassessmentReason? ReassessmentReason { get; set; }

    /// <summary>
    /// تاریخ و زمان اندازه‌گیری
    /// </summary>
    [Required(ErrorMessage = "تاریخ اندازه‌گیری الزامی است.")]
    public DateTime MeasurementTime { get; set; }

    /// <summary>
    /// یادداشت‌های اضافی
    /// </summary>
    [MaxLength(500, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Notes { get; set; }

    /// <summary>
    /// آیا علائم حیاتی طبیعی است؟
    /// </summary>
    public bool IsNormal { get; set; } = true;

    /// <summary>
    /// آیا نیاز به مراقبت فوری دارد؟
    /// </summary>
    public bool RequiresImmediateAttention { get; set; } = false;

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن علائم حیاتی
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف علائم حیاتی
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که علائم حیاتی را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد علائم حیاتی
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// شناسه کاربری که علائم حیاتی را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش علائم حیاتی
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که علائم حیاتی را ویرایش کرده است
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
    /// ارجاع به ارزیابی تریاژ
    /// </summary>
    public virtual TriageAssessment TriageAssessment { get; set; }
    #endregion
}

/// <summary>
/// پیکربندی مدل علائم حیاتی تریاژ برای Entity Framework
/// </summary>
public class TriageVitalSignsConfig : EntityTypeConfiguration<TriageVitalSigns>
{
    public TriageVitalSignsConfig()
    {
        ToTable("TriageVitalSigns");
        HasKey(tvs => tvs.TriageVitalSignsId);

        // ویژگی‌های اصلی
        Property(tvs => tvs.TriageAssessmentId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageVitalSigns_TriageAssessmentId")));

        Property(tvs => tvs.MeasurementTime)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageVitalSigns_MeasurementTime")));

        Property(tvs => tvs.IsNormal)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageVitalSigns_IsNormal")));

        Property(tvs => tvs.RequiresImmediateAttention)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageVitalSigns_RequiresImmediateAttention")));

        // پیاده‌سازی ISoftDelete
        Property(tvs => tvs.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageVitalSigns_IsDeleted")));

        // پیاده‌سازی ITrackable
        Property(tvs => tvs.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_TriageVitalSigns_CreatedAt")));

        // روابط
        HasRequired(tvs => tvs.TriageAssessment)
            .WithMany(ta => ta.VitalSigns)
            .HasForeignKey(tvs => tvs.TriageAssessmentId)
            .WillCascadeOnDelete(false);

        // روابط User - صراحت کامل
        Property(tvs => tvs.CreatedByUserId)
            .HasMaxLength(128);
        HasOptional(tvs => tvs.CreatedByUser)
            .WithMany()
            .HasForeignKey(tvs => tvs.CreatedByUserId)
            .WillCascadeOnDelete(false);

        Property(tvs => tvs.UpdatedByUserId)
            .HasMaxLength(128);
        HasOptional(tvs => tvs.UpdatedByUser)
            .WithMany()
            .HasForeignKey(tvs => tvs.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        Property(tvs => tvs.DeletedByUserId)
            .HasMaxLength(128);
        HasOptional(tvs => tvs.DeletedByUser)
            .WithMany()
            .HasForeignKey(tvs => tvs.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی - در Migration اضافه می‌شوند
        // IX_TriageVitalSigns_Assessment_Time_IsDeleted
        // IX_TriageVitalSigns_Normal_Immediate_IsDeleted
        // IX_TriageVitalSigns_MeasurementTime_IsDeleted
    }
}
