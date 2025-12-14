using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل Template خبرنامه (Newsletter Template)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// برای مدیریت Template های خبرنامه
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class NewsletterTemplate : ISoftDelete, ITrackable
    {
        public int NewsletterTemplateId { get; set; }

        [Required(ErrorMessage = "نام Template الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام Template نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "موضوع ایمیل الزامی است.")]
        [MaxLength(500, ErrorMessage = "موضوع ایمیل نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Subject { get; set; }

        /// <summary>
        /// توضیحات کوتاه درباره Template (اختیاری)
        /// برای کمک به کاربران در انتخاب Template مناسب
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Description { get; set; }

        /// <summary>
        /// محتوای Template (HTML) با Variables
        /// مثال: "سلام {{FullName}}، ..."
        /// </summary>
        [Required(ErrorMessage = "محتوای Template الزامی است.")]
        [Column(TypeName = "ntext")]
        public string Content { get; set; }

        public bool IsActive { get; set; }

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
    /// پیکربندی Entity Framework برای NewsletterTemplate
    /// بهینه‌سازی شده برای Query Performance
    /// </summary>
    public class NewsletterTemplateConfig : EntityTypeConfiguration<NewsletterTemplate>
    {
        public NewsletterTemplateConfig()
        {
            ToTable("NewsletterTemplates");
            HasKey(t => t.NewsletterTemplateId);

            Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterTemplate_Name")));

            Property(t => t.Subject)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterTemplate_Subject")));

            Property(t => t.Description)
                .IsOptional()
                .HasMaxLength(500);

            Property(t => t.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterTemplate_IsActive")));

            Property(t => t.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterTemplate_IsDeleted")));

            Property(t => t.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterTemplate_CreatedAt")));

            HasOptional(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.UpdatedByUser)
                .WithMany()
                .HasForeignKey(t => t.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.DeletedByUser)
                .WithMany()
                .HasForeignKey(t => t.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس ترکیبی برای Query های رایج
            HasIndex(t => new { t.IsActive, t.IsDeleted, t.CreatedAt })
                .HasName("IX_NewsletterTemplate_Active_Deleted_CreatedAt");
        }
    }
}

