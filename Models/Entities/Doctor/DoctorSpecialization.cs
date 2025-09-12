using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Doctor;

/// <summary>
/// مدل رابطه Many-to-Many بین پزشکان و تخصص‌ها
/// این کلاس برای مدیریت رابطه بین پزشکان و تخصص‌های آن‌ها استفاده می‌شود
/// </summary>
public class DoctorSpecialization
{
    /// <summary>
    /// شناسه تخصص
    /// </summary>
    public int SpecializationId { get; set; }

    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int DoctorId { get; set; }

    #region روابط
    /// <summary>
    /// تخصص مرتبط
    /// </summary>
    public virtual Specialization Specialization { get; set; }

    /// <summary>
    /// پزشک مرتبط
    /// </summary>
    public virtual Doctor Doctor { get; set; }
    #endregion
}
#region DoctorSpecialization Entity (رابطه Many-to-Many)

/// <summary>
/// پیکربندی مدل رابطه پزشک-تخصص برای Entity Framework
/// </summary>
public class DoctorSpecializationConfig : EntityTypeConfiguration<DoctorSpecialization>
{
    public DoctorSpecializationConfig()
    {
        ToTable("DoctorSpecializations");

        // تعریف کلید مرکب
        HasKey(ds => new { ds.SpecializationId, ds.DoctorId });

        // روابط - تنظیم صحیح Foreign Keys
        HasRequired(ds => ds.Specialization)
            .WithMany(s => s.DoctorSpecializations)
            .HasForeignKey(ds => ds.SpecializationId)
            .WillCascadeOnDelete(false);

        HasRequired(ds => ds.Doctor)
            .WithMany(d => d.DoctorSpecializations)
            .HasForeignKey(ds => ds.DoctorId)
            .WillCascadeOnDelete(false);
    }
}
#endregion