using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل صفحه "درباره ما" (About Page)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class AboutPage : ISoftDelete, ITrackable
    {
        public int AboutPageId { get; set; }

        [Required(ErrorMessage = "نام کلینیک الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام کلینیک نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string ClinicName { get; set; }

        [Required(ErrorMessage = "توضیحات کلینیک الزامی است.")]
        [Column(TypeName = "ntext")]
        public string ClinicDescription { get; set; }

        [MaxLength(50)]
        public string EstablishedYear { get; set; }

        // مأموریت و رویکرد درمانی (JSON)
        [Column(TypeName = "ntext")]
        public string MissionValuesJson { get; set; }

        // مجوزها و اعتبارها (JSON)
        [Column(TypeName = "ntext")]
        public string LicensesJson { get; set; }

        [MaxLength(500)]
        public string RegulatoryBody { get; set; }

        // توضیحات کادر درمان
        [MaxLength(1000)]
        public string MedicalTeamDescription { get; set; }

        // توضیحات تجهیزات
        [MaxLength(1000)]
        public string InfrastructureDescription { get; set; }

        // تعهدات اخلاقی (JSON)
        [Column(TypeName = "ntext")]
        public string EthicalCommitmentsJson { get; set; }

        // تصویر Hero Section
        [MaxLength(500)]
        public string HeroImageUrl { get; set; }

        // تصویر Background
        [MaxLength(500)]
        public string BackgroundImageUrl { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }

        // SEO Fields
        [MaxLength(500)]
        public string MetaTitle { get; set; }

        [MaxLength(1000)]
        public string MetaDescription { get; set; }

        [MaxLength(200)]
        public string Slug { get; set; }

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

    #region Entity Configuration

    /// <summary>
    /// پیکربندی Entity Framework برای AboutPage
    /// </summary>
    public class AboutPageConfig : EntityTypeConfiguration<AboutPage>
    {
        public AboutPageConfig()
        {
            ToTable("AboutPages");
            HasKey(a => a.AboutPageId);

            // Indexes
            Property(a => a.ClinicName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_AboutPage_ClinicName")));

            Property(a => a.Slug)
                .IsOptional()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_AboutPage_Slug")));

            Property(a => a.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_AboutPage_IsActive")));

            Property(a => a.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_AboutPage_IsDeleted")));

            // Relationships
            HasOptional(a => a.CreatedByUser)
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.UpdatedByUser)
                .WithMany()
                .HasForeignKey(a => a.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.DeletedByUser)
                .WithMany()
                .HasForeignKey(a => a.DeletedByUserId)
                .WillCascadeOnDelete(false);
        }
    }

    #endregion
}
