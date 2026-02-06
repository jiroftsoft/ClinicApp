using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// تنظیمات فوتر سایت - برند، تماس، حقوقی، عنوان ساعات کاری
    /// یک رکورد فعال برای هر کلینیک (یا سراسری)
    /// </summary>
    public class FooterSettings : ITrackable
    {
        public int FooterSettingsId { get; set; }

        public int? ClinicId { get; set; }

        [MaxLength(200)]
        public string BrandClinicName { get; set; }

        [MaxLength(500)]
        public string BrandLogoUrl { get; set; }

        [MaxLength(300)]
        public string BrandTagline { get; set; }

        [MaxLength(1000)]
        public string BrandDescription { get; set; }

        [MaxLength(200)]
        public string BrandHomeUrl { get; set; }

        [MaxLength(50)]
        public string ContactPhone { get; set; }

        [MaxLength(50)]
        public string ContactEmergencyPhone { get; set; }

        [MaxLength(200)]
        public string ContactEmail { get; set; }

        [MaxLength(500)]
        public string ContactAddress { get; set; }

        [MaxLength(50)]
        public string ContactWhatsAppNumber { get; set; }

        [MaxLength(500)]
        public string LegalCopyrightText { get; set; }

        [MaxLength(500)]
        public string LegalPrivacyPolicyUrl { get; set; }

        [MaxLength(500)]
        public string LegalTermsOfServiceUrl { get; set; }

        [MaxLength(500)]
        public string LegalComplaintsUrl { get; set; }

        [MaxLength(1000)]
        public string LegalMedicalPrivacyNotice { get; set; }

        [MaxLength(100)]
        public string WorkingHoursTitle { get; set; }

        public bool IsActive { get; set; } = true;

        #region ITrackable
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedByUserId { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }
        #endregion
    }

    public class FooterSettingsConfig : EntityTypeConfiguration<FooterSettings>
    {
        public FooterSettingsConfig()
        {
            ToTable("FooterSettings");
            HasKey(f => f.FooterSettingsId);
            Property(f => f.ClinicId).HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FooterSettings_ClinicId")));
            Property(f => f.IsActive).HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FooterSettings_IsActive")));
        }
    }
}
