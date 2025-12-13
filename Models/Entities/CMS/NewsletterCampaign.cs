using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل Campaign خبرنامه (Newsletter Campaign)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// برای مدیریت Campaign های ارسال خبرنامه
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class NewsletterCampaign : ISoftDelete, ITrackable
    {
        public int NewsletterCampaignId { get; set; }

        [Required(ErrorMessage = "عنوان Campaign الزامی است.")]
        [MaxLength(300, ErrorMessage = "عنوان Campaign نمی‌تواند بیش از 300 کاراکتر باشد.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "موضوع ایمیل الزامی است.")]
        [MaxLength(500, ErrorMessage = "موضوع ایمیل نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Subject { get; set; }

        /// <summary>
        /// محتوای Campaign (HTML)
        /// </summary>
        [Required(ErrorMessage = "محتوای Campaign الزامی است.")]
        [Column(TypeName = "ntext")]
        public string Content { get; set; }

        /// <summary>
        /// Template استفاده شده (اختیاری)
        /// </summary>
        public int? NewsletterTemplateId { get; set; }
        public virtual NewsletterTemplate Template { get; set; }

        /// <summary>
        /// دسته‌بندی‌های انتخاب شده (JSON Array)
        /// مثال: ["Articles", "Announcements"]
        /// </summary>
        [Column(TypeName = "ntext")]
        public string Categories { get; set; }

        /// <summary>
        /// آیا به تمام مشترکین ارسال شود
        /// </summary>
        public bool SendToAll { get; set; }

        /// <summary>
        /// تاریخ زمان‌بندی شده برای ارسال
        /// </summary>
        public DateTime? ScheduledAt { get; set; }

        /// <summary>
        /// تاریخ ارسال واقعی
        /// </summary>
        public DateTime? SentAt { get; set; }

        [Required(ErrorMessage = "وضعیت Campaign الزامی است.")]
        public NewsletterCampaignStatus Status { get; set; }

        /// <summary>
        /// تعداد کل Recipients
        /// </summary>
        public int TotalRecipients { get; set; }

        /// <summary>
        /// تعداد ارسال شده
        /// </summary>
        public int SentCount { get; set; }

        /// <summary>
        /// تعداد ناموفق
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// تعداد باز شده (Opened)
        /// </summary>
        public int OpenedCount { get; set; }

        /// <summary>
        /// تعداد کلیک شده (Clicked)
        /// </summary>
        public int ClickedCount { get; set; }

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
    /// پیکربندی Entity Framework برای NewsletterCampaign
    /// بهینه‌سازی شده برای Query Performance
    /// </summary>
    public class NewsletterCampaignConfig : EntityTypeConfiguration<NewsletterCampaign>
    {
        public NewsletterCampaignConfig()
        {
            ToTable("NewsletterCampaigns");
            HasKey(c => c.NewsletterCampaignId);

            Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterCampaign_Title")));

            Property(c => c.Subject)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterCampaign_Subject")));

            Property(c => c.Status)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterCampaign_Status")));

            Property(c => c.ScheduledAt)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterCampaign_ScheduledAt")));

            Property(c => c.SentAt)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterCampaign_SentAt")));

            Property(c => c.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterCampaign_IsDeleted")));

            Property(c => c.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterCampaign_CreatedAt")));

            HasOptional(c => c.Template)
                .WithMany()
                .HasForeignKey(c => c.NewsletterTemplateId)
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

            // ایندکس ترکیبی برای Query های رایج
            HasIndex(c => new { c.Status, c.IsDeleted, c.CreatedAt })
                .HasName("IX_NewsletterCampaign_Status_Deleted_CreatedAt");

            HasIndex(c => new { c.ScheduledAt, c.Status })
                .HasName("IX_NewsletterCampaign_ScheduledAt_Status");
        }
    }
}

