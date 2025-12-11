using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل کامنت‌های مقالات بلاگ
    /// طراحی شده بر اساس اصول SRP و Production-Ready
    /// </summary>
    public class BlogPostComment : ISoftDelete, ITrackable
    {
        public int BlogPostCommentId { get; set; }

        [Required]
        public int BlogPostId { get; set; }

        [Required(ErrorMessage = "متن کامنت الزامی است.")]
        [MaxLength(2000, ErrorMessage = "متن کامنت نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        public string CommentText { get; set; }

        [MaxLength(200, ErrorMessage = "نام نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string AuthorName { get; set; }

        [MaxLength(500, ErrorMessage = "ایمیل نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        public string AuthorEmail { get; set; }

        [MaxLength(50, ErrorMessage = "شماره تماس نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string AuthorPhone { get; set; }

        // Foreign Key به ApplicationUser (اگر کاربر لاگین باشد)
        public string AuthorUserId { get; set; }

        // وضعیت کامنت
        public bool IsApproved { get; set; } // تأیید شده توسط ادمین
        public bool IsSpam { get; set; } // اسپم
        public bool IsReported { get; set; } // گزارش شده

        // کامنت والد (برای پاسخ به کامنت)
        public int? ParentCommentId { get; set; }

        // IP Address برای امنیت
        [MaxLength(50)]
        public string IpAddress { get; set; }

        // User Agent برای امنیت
        [MaxLength(500)]
        public string UserAgent { get; set; }

        // Navigation Properties
        public virtual BlogPost BlogPost { get; set; }
        public virtual ApplicationUser AuthorUser { get; set; }
        public virtual BlogPostComment ParentComment { get; set; }
        public virtual System.Collections.Generic.ICollection<BlogPostComment> Replies { get; set; }

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

    public class BlogPostCommentConfig : EntityTypeConfiguration<BlogPostComment>
    {
        public BlogPostCommentConfig()
        {
            ToTable("BlogPostComments");
            HasKey(c => c.BlogPostCommentId);

            Property(c => c.CommentText)
                .IsRequired()
                .HasMaxLength(2000);

            Property(c => c.BlogPostId)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPostComment_BlogPostId")));

            Property(c => c.IsApproved)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPostComment_IsApproved")));

            Property(c => c.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPostComment_IsDeleted")));

            Property(c => c.ParentCommentId)
                .IsOptional()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPostComment_ParentCommentId")));

            Property(c => c.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_BlogPostComment_CreatedAt")));

            // Relationships
            HasRequired(c => c.BlogPost)
                .WithMany()
                .HasForeignKey(c => c.BlogPostId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.AuthorUser)
                .WithMany()
                .HasForeignKey(c => c.AuthorUserId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.CreatedByUser)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.UpdatedByUser)
                .WithMany()
                .HasForeignKey(c => c.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.DeletedByUser)
                .WithMany()
                .HasForeignKey(c => c.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // Composite Index
            HasIndex(c => new { c.BlogPostId, c.IsApproved, c.IsDeleted, c.CreatedAt })
                .HasName("IX_BlogPostComment_BlogPost_Approved_Deleted_Date");
        }
    }
}

