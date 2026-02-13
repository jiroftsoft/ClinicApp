using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Patient
{
    /// <summary>
    /// تنظیمات اختیاری بیمار (اعلان‌ها، حریم خصوصی و غیره)
    /// یک رکورد به ازای هر بیمار (اختیاری؛ در صورت نبود رکورد از مقادیر پیش‌فرض استفاده می‌شود)
    /// </summary>
    [Table("PatientSettings")]
    public class PatientSetting
    {
        /// <summary>شناسه بیمار (PK و FK)</summary>
        [Key]
        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        /// <summary>اعلان از طریق ایمیل</summary>
        public bool EmailNotifications { get; set; } = true;

        /// <summary>اعلان از طریق پیامک</summary>
        public bool SmsNotifications { get; set; } = true;

        /// <summary>یادآوری نوبت</summary>
        public bool AppointmentReminders { get; set; } = true;

        /// <summary>آخرین به‌روزرسانی</summary>
        public DateTime? UpdatedAt { get; set; }

        public virtual Patient Patient { get; set; }
    }

    /// <summary>
    /// پیکربندی EF برای PatientSetting
    /// </summary>
    public class PatientSettingConfig : EntityTypeConfiguration<PatientSetting>
    {
        public PatientSettingConfig()
        {
            ToTable("PatientSettings");
            HasKey(s => s.PatientId);
            HasRequired(s => s.Patient)
                .WithOptional()
                .WillCascadeOnDelete(true);
        }
    }
}
