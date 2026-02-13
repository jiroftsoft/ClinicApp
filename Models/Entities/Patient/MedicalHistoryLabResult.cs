using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Patient;

/// <summary>
/// نتیجه آزمایش کلیدی مرتبط با یک رکورد تاریخچه پزشکی.
/// مثلاً CK-MB، Troponin، CRP با تاریخ و مقدار.
/// </summary>
public class MedicalHistoryLabResult
{
    public int Id { get; set; }

    public int MedicalHistoryId { get; set; }

    /// <summary>نام آزمایش — مثلاً CK-MB، Troponin I، HbA1c</summary>
    [Required(ErrorMessage = "نام آزمایش الزامی است.")]
    [MaxLength(100)]
    public string LabName { get; set; }

    /// <summary>مقدار — عدد یا متن (مثلاً positive/negative)</summary>
    [MaxLength(50)]
    public string Value { get; set; }

    /// <summary>واحد — مثلاً ng/mL، U/L، mg/dL</summary>
    [MaxLength(50)]
    public string Unit { get; set; }

    /// <summary>تاریخ انجام آزمایش</summary>
    [Column(TypeName = "date")]
    public DateTime LabDate { get; set; }

    /// <summary>محدوده مرجع — مثلاً 0-5، نرمال</summary>
    [MaxLength(100)]
    public string ReferenceRange { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(128)]
    public string CreatedByUserId { get; set; }

    public virtual ApplicationUser CreatedByUser { get; set; }
    public virtual MedicalHistory MedicalHistory { get; set; }
}

/// <summary>
/// پیکربندی EF برای MedicalHistoryLabResult
/// </summary>
public class MedicalHistoryLabResultConfig : EntityTypeConfiguration<MedicalHistoryLabResult>
{
    public MedicalHistoryLabResultConfig()
    {
        ToTable("MedicalHistoryLabResults");
        HasKey(x => x.Id);

        Property(x => x.LabName).IsRequired().HasMaxLength(100);
        Property(x => x.Value).IsOptional().HasMaxLength(50);
        Property(x => x.Unit).IsOptional().HasMaxLength(50);
        Property(x => x.LabDate).IsRequired();
        Property(x => x.ReferenceRange).IsOptional().HasMaxLength(100);
        Property(x => x.CreatedAt).IsRequired();

        HasRequired(x => x.MedicalHistory)
            .WithMany(mh => mh.LabResults)
            .HasForeignKey(x => x.MedicalHistoryId)
            .WillCascadeOnDelete(true);

        HasOptional(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasIndex(x => x.MedicalHistoryId).HasName("IX_MedicalHistoryLabResult_MedicalHistoryId");
        HasIndex(x => new { x.MedicalHistoryId, x.LabDate }).HasName("IX_MedicalHistoryLabResult_HistoryId_Date");
    }
}
