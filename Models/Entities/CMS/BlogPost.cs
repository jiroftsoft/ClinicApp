using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل مقالات و پست‌های بلاگ
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// </summary>
    public class BlogPost : ISoftDelete, ITrackable
    {
        public int BlogPostId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(500, ErrorMessage = "عنوان نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Title { get; set; }

        [MaxLength(1000, ErrorMessage = "خلاصه نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string Summary { get; set; }

        [Required(ErrorMessage = "محتوا الزامی است.")]
        [Column(TypeName = "ntext")]
        public string Content { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(200)]
        public string ThumbnailUrl { get; set; }

        [MaxLength(100)]
        public string AuthorName { get; set; }

        [MaxLength(50)]
        public string CategoryName { get; set; }

        public DateTime? PublishedAt { get; set; }

        public bool IsPublished { get; set; }

        public bool IsFeatured { get; set; }

        public int ViewCount { get; set; }

        public int? DisplayOrder { get; set; }

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

    public class BlogPostConfig : EntityTypeConfiguration<BlogPost>
    {
        public BlogPostConfig()
        {
            ToTable("BlogPosts");
            HasKey(b => b.BlogPostId);

            Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPost_Title")));

            Property(b => b.Slug)
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPost_Slug") { IsUnique = true }));

            Property(b => b.IsPublished)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPost_IsPublished")));

            Property(b => b.IsFeatured)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPost_IsFeatured")));

            Property(b => b.PublishedAt)
                .IsOptional()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPost_PublishedAt")));

            Property(b => b.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPost_IsDeleted")));

            HasOptional(b => b.CreatedByUser)
                .WithMany()
                .HasForeignKey(b => b.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(b => b.UpdatedByUser)
                .WithMany()
                .HasForeignKey(b => b.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(b => b.DeletedByUser)
                .WithMany()
                .HasForeignKey(b => b.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasIndex(b => new { b.IsPublished, b.IsDeleted, b.PublishedAt })
                .HasName("IX_BlogPost_Published_Deleted_Date");
        }
    }
}

