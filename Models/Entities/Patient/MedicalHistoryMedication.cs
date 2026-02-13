using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Patient;

/// <summary>
/// داروی مرتبط با یک رکورد تاریخچه پزشکی.
/// برای نوع «دارو» یک رکورد؛ برای بیماری (مثلاً قلبی) چند دارو قابل ثبت.
/// </summary>
public class MedicalHistoryMedication
{
    public int Id { get; set; }

    [Required]
    public int MedicalHistoryId { get; set; }

    /// <summary>نام دارو (الزامی)</summary>
    [Required(ErrorMessage = "نام دارو الزامی است.")]
    [MaxLength(200)]
    public string DrugName { get; set; }

    /// <summary>دوز — مثلاً 5، 100</summary>
    [MaxLength(100)]
    public string Dosage { get; set; }

    /// <summary>واحد دوز — مثلاً mg، قرص، واحد بین‌المللی</summary>
    [MaxLength(50)]
    public string DosageUnit { get; set; }

    /// <summary>نحوه مصرف — مثلاً روزانه، دو بار در روز، هر ۸ ساعت</summary>
    [MaxLength(100)]
    public string Frequency { get; set; }

    /// <summary>راه مصرف — خوراکی، تزریقی، موضعی</summary>
    [MaxLength(50)]
    public string Route { get; set; }

    /// <summary>تاریخ شروع مصرف</summary>
    [Column(TypeName = "date")]
    public DateTime? StartDate { get; set; }

    /// <summary>تاریخ پایان مصرف (در صورت قطع)</summary>
    [Column(TypeName = "date")]
    public DateTime? EndDate { get; set; }

    /// <summary>دلیل مصرف / اندیکاسیون</summary>
    [MaxLength(300)]
    public string Indication { get; set; }

    /// <summary>پزشک تجویزکننده</summary>
    [MaxLength(100)]
    public string PrescribingDoctor { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>ترتیب نمایش در لیست</summary>
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(128)]
    public string CreatedByUserId { get; set; }

    public virtual ApplicationUser CreatedByUser { get; set; }
    public virtual MedicalHistory MedicalHistory { get; set; }
}

/// <summary>
/// پیکربندی EF برای MedicalHistoryMedication
/// </summary>
public class MedicalHistoryMedicationConfig : EntityTypeConfiguration<MedicalHistoryMedication>
{
    public MedicalHistoryMedicationConfig()
    {
        ToTable("MedicalHistoryMedications");
        HasKey(x => x.Id);

        Property(x => x.DrugName).IsRequired().HasMaxLength(200);
        Property(x => x.Dosage).IsOptional().HasMaxLength(100);
        Property(x => x.DosageUnit).IsOptional().HasMaxLength(50);
        Property(x => x.Frequency).IsOptional().HasMaxLength(100);
        Property(x => x.Route).IsOptional().HasMaxLength(50);
        Property(x => x.Indication).IsOptional().HasMaxLength(300);
        Property(x => x.PrescribingDoctor).IsOptional().HasMaxLength(100);
        Property(x => x.IsActive).IsRequired();
        Property(x => x.DisplayOrder).IsRequired();
        Property(x => x.CreatedAt).IsRequired();

        HasRequired(x => x.MedicalHistory)
            .WithMany(mh => mh.Medications)
            .HasForeignKey(x => x.MedicalHistoryId)
            .WillCascadeOnDelete(true);

        HasOptional(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasIndex(x => x.MedicalHistoryId).HasName("IX_MedicalHistoryMedication_MedicalHistoryId");
        HasIndex(x => new { x.MedicalHistoryId, x.IsActive }).HasName("IX_MedicalHistoryMedication_HistoryId_Active");
    }
}
