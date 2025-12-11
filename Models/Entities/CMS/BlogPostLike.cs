using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل لایک‌های مقالات بلاگ
    /// طراحی شده بر اساس اصول SRP و Production-Ready
    /// </summary>
    public class BlogPostLike : ITrackable
    {
        public int BlogPostLikeId { get; set; }

        [Required]
        public int BlogPostId { get; set; }

        // Foreign Key به ApplicationUser (اگر کاربر لاگین باشد)
        public string UserId { get; set; }

        // برای کاربران غیر لاگین - شناسه منحصر به فرد (مثلاً از Cookie)
        [MaxLength(100)]
        public string GuestIdentifier { get; set; }

        // IP Address برای امنیت
        [MaxLength(50)]
        public string IpAddress { get; set; }

        // User Agent برای امنیت
        [MaxLength(500)]
        public string UserAgent { get; set; }

        // Navigation Properties
        public virtual BlogPost BlogPost { get; set; }
        public virtual ApplicationUser User { get; set; }

        #region ITrackable
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedByUserId { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }
        #endregion
    }

    public class BlogPostLikeConfig : EntityTypeConfiguration<BlogPostLike>
    {
        public BlogPostLikeConfig()
        {
            ToTable("BlogPostLikes");
            HasKey(l => l.BlogPostLikeId);

            Property(l => l.BlogPostId)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPostLike_BlogPostId")));

            Property(l => l.UserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPostLike_UserId")));

            Property(l => l.GuestIdentifier)
                .IsOptional()
                .HasMaxLength(100)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPostLike_GuestIdentifier")));

            // Relationships
            HasRequired(l => l.BlogPost)
                .WithMany()
                .HasForeignKey(l => l.BlogPostId)
                .WillCascadeOnDelete(false);

            HasOptional(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .WillCascadeOnDelete(false);

            HasOptional(l => l.CreatedByUser)
                .WithMany()
                .HasForeignKey(l => l.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(l => l.UpdatedByUser)
                .WithMany()
                .HasForeignKey(l => l.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            // Unique Constraint: یک کاربر نمی‌تواند دو بار یک پست را لایک کند
            // Note: در EF 6 نمی‌توان از HasFilter استفاده کرد، بنابراین باید در Service/Repository بررسی شود
            HasIndex(l => new { l.BlogPostId, l.UserId })
                .HasName("IX_BlogPostLike_BlogPost_User");

            HasIndex(l => new { l.BlogPostId, l.GuestIdentifier })
                .HasName("IX_BlogPostLike_BlogPost_Guest");
        }
    }
}

