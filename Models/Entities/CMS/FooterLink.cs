using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// لینک فوتر - سریع یا خدمات
    /// </summary>
    public class FooterLink : ISoftDelete, ITrackable
    {
        public int FooterLinkId { get; set; }

        /// <summary>1=QuickLink, 2=ServiceLink</summary>
        public byte LinkType { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required, MaxLength(500)]
        public string Url { get; set; }

        [MaxLength(100)]
        public string Icon { get; set; }

        public bool IsExternal { get; set; }

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

    public class FooterLinkConfig : EntityTypeConfiguration<FooterLink>
    {
        public FooterLinkConfig()
        {
            ToTable("FooterLinks");
            HasKey(f => f.FooterLinkId);
            Property(f => f.LinkType).HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FooterLink_LinkType")));
            Property(f => f.IsActive).HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FooterLink_IsActive")));
            Property(f => f.IsDeleted).HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FooterLink_IsDeleted")));
        }
    }
}
