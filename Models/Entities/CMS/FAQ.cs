using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل سوالات متداول (FAQ)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class FAQ : ISoftDelete, ITrackable
    {
        public int FAQId { get; set; }

        [Required(ErrorMessage = "سوال الزامی است.")]
        [MaxLength(500, ErrorMessage = "سوال نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Question { get; set; }

        [Required(ErrorMessage = "پاسخ الزامی است.")]
        [Column(TypeName = "ntext")]
        public string Answer { get; set; }

        [MaxLength(100, ErrorMessage = "دسته‌بندی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string Category { get; set; } // "appointment", "insurance", "services", "costs", "general"

        [MaxLength(500, ErrorMessage = "برچسب‌ها نمی‌توانند بیش از 500 کاراکتر باشند.")]
        public string Tags { get; set; } // Comma-separated tags

        [MaxLength(500, ErrorMessage = "لینک مرتبط نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string RelatedLinkUrl { get; set; }

        public int ViewCount { get; set; }

        public bool IsActive { get; set; }

        public bool IsFeatured { get; set; } // نمایش در صفحه اصلی

        public int DisplayOrder { get; set; }

        [MaxLength(500, ErrorMessage = "عنوان متا نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string MetaTitle { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات متا نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string MetaDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Slug نمی‌تواند بیش از 200 کاراکتر باشد.")]
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

    /// <summary>
    /// پیکربندی Entity Framework برای FAQ
    /// بهینه‌سازی شده برای Query Performance
    /// </summary>
    public class FAQConfig : EntityTypeConfiguration<FAQ>
    {
        public FAQConfig()
        {
            ToTable("FAQs");
            HasKey(f => f.FAQId);

            Property(f => f.Question)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FAQ_Question")));

            Property(f => f.Category)
                .HasMaxLength(100)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FAQ_Category")));

            Property(f => f.Slug)
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FAQ_Slug") { IsUnique = true }));

            Property(f => f.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FAQ_IsActive")));

            Property(f => f.IsFeatured)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FAQ_IsFeatured")));

            Property(f => f.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FAQ_DisplayOrder")));

            Property(f => f.ViewCount)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FAQ_ViewCount")));

            Property(f => f.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_FAQ_IsDeleted")));

            HasOptional(f => f.CreatedByUser)
                .WithMany()
                .HasForeignKey(f => f.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(f => f.UpdatedByUser)
                .WithMany()
                .HasForeignKey(f => f.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(f => f.DeletedByUser)
                .WithMany()
                .HasForeignKey(f => f.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس ترکیبی برای Query های رایج
            HasIndex(f => new { f.IsActive, f.IsDeleted, f.Category })
                .HasName("IX_FAQ_Active_Deleted_Category");

            HasIndex(f => new { f.IsFeatured, f.IsActive, f.DisplayOrder })
                .HasName("IX_FAQ_Featured_Active_Order");
        }
    }
}

