using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل نکات سلامت (Health Tips)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class HealthTip : ISoftDelete, ITrackable
    {
        public int HealthTipId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(300, ErrorMessage = "عنوان نمی‌تواند بیش از 300 کاراکتر باشد.")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "خلاصه نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Summary { get; set; }

        [Required(ErrorMessage = "محتوا الزامی است.")]
        [Column(TypeName = "ntext")]
        public string Content { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string ImageUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر کوچک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string ThumbnailUrl { get; set; }

        [MaxLength(100, ErrorMessage = "دسته‌بندی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string Category { get; set; } // "prevention", "nutrition", "exercise", "diseases", "general"

        [MaxLength(500, ErrorMessage = "برچسب‌ها نمی‌توانند بیش از 500 کاراکتر باشند.")]
        public string Tags { get; set; } // Comma-separated tags

        public DateTime? PublishedAt { get; set; }

        public DateTime? ExpiryDate { get; set; } // برای نکات موقت

        public bool IsPublished { get; set; }

        public bool IsFeatured { get; set; } // نمایش در صفحه اصلی

        public int ViewCount { get; set; }

        public int ShareCount { get; set; } // تعداد اشتراک‌گذاری

        public int DisplayOrder { get; set; }

        [MaxLength(500, ErrorMessage = "عنوان متا نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string MetaTitle { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات متا نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string MetaDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Slug نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Slug { get; set; }

        [MaxLength(500, ErrorMessage = "لینک مرتبط نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string RelatedLinkUrl { get; set; }

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
    /// پیکربندی Entity Framework برای HealthTip
    /// بهینه‌سازی شده برای Query Performance
    /// </summary>
    public class HealthTipConfig : EntityTypeConfiguration<HealthTip>
    {
        public HealthTipConfig()
        {
            ToTable("HealthTips");
            HasKey(h => h.HealthTipId);

            Property(h => h.Title)
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_HealthTip_Title")));

            Property(h => h.Category)
                .HasMaxLength(100)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_HealthTip_Category")));

            Property(h => h.Slug)
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_HealthTip_Slug") { IsUnique = true }));

            Property(h => h.IsPublished)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_HealthTip_IsPublished")));

            Property(h => h.IsFeatured)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_HealthTip_IsFeatured")));

            Property(h => h.PublishedAt)
                .IsOptional()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_HealthTip_PublishedAt")));

            Property(h => h.ExpiryDate)
                .IsOptional()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_HealthTip_ExpiryDate")));

            Property(h => h.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_HealthTip_DisplayOrder")));

            Property(h => h.ViewCount)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_HealthTip_ViewCount")));

            Property(h => h.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_HealthTip_IsDeleted")));

            HasOptional(h => h.CreatedByUser)
                .WithMany()
                .HasForeignKey(h => h.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(h => h.UpdatedByUser)
                .WithMany()
                .HasForeignKey(h => h.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(h => h.DeletedByUser)
                .WithMany()
                .HasForeignKey(h => h.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس ترکیبی برای Query های رایج
            HasIndex(h => new { h.IsPublished, h.IsDeleted, h.PublishedAt, h.ExpiryDate })
                .HasName("IX_HealthTip_Published_Deleted_Dates");

            HasIndex(h => new { h.IsFeatured, h.IsPublished, h.DisplayOrder })
                .HasName("IX_HealthTip_Featured_Published_Order");

            HasIndex(h => new { h.Category, h.IsPublished, h.IsDeleted })
                .HasName("IX_HealthTip_Category_Published_Deleted");
        }
    }
}

