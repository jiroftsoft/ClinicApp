using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مجوز/اعتبار در فوتر
    /// </summary>
    public class FooterCertification : ISoftDelete, ITrackable
    {
        public int FooterCertificationId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(500)]
        public string LinkUrl { get; set; }

        [MaxLength(100)]
        public string LicenseNumber { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public int? ClinicId { get; set; }

        #region ISoftDelete
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedByUserId { get; set; }
        public virtual ApplicationUser DeletedByUser { get; set; }
        #endregion

        #region ITrackable
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedByUserId { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }
        #endregion
    }

    public class FooterCertificationConfig : EntityTypeConfiguration<FooterCertification>
    {
        public FooterCertificationConfig()
        {
            ToTable("FooterCertifications");
            HasKey(f => f.FooterCertificationId);
            Property(f => f.IsActive).HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FooterCertification_IsActive")));
            Property(f => f.IsDeleted).HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FooterCertification_IsDeleted")));
        }
    }
}
