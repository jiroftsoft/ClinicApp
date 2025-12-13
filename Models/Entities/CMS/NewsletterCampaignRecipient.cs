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
    /// مدل Recipient Campaign خبرنامه (Newsletter Campaign Recipient)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// برای Tracking هر Recipient در Campaign
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class NewsletterCampaignRecipient : ITrackable
    {
        public int NewsletterCampaignRecipientId { get; set; }

        public int NewsletterCampaignId { get; set; }
        public virtual NewsletterCampaign Campaign { get; set; }

        public int NewsletterSubscriptionId { get; set; }
        public virtual NewsletterSubscription Subscription { get; set; }

        [Required(ErrorMessage = "ایمیل الزامی است.")]
        [MaxLength(200, ErrorMessage = "ایمیل نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "وضعیت Recipient الزامی است.")]
        public NewsletterRecipientStatus Status { get; set; }

        /// <summary>
        /// تاریخ ارسال
        /// </summary>
        public DateTime? SentAt { get; set; }

        /// <summary>
        /// تاریخ باز شدن ایمیل
        /// </summary>
        public DateTime? OpenedAt { get; set; }

        /// <summary>
        /// تاریخ کلیک روی لینک
        /// </summary>
        public DateTime? ClickedAt { get; set; }

        /// <summary>
        /// URL کلیک شده
        /// </summary>
        [MaxLength(1000, ErrorMessage = "URL نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string ClickedUrl { get; set; }

        /// <summary>
        /// پیام خطا در صورت ناموفق بودن ارسال
        /// </summary>
        [MaxLength(1000, ErrorMessage = "پیام خطا نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string ErrorMessage { get; set; }

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
    /// پیکربندی Entity Framework برای NewsletterCampaignRecipient
    /// بهینه‌سازی شده برای Query Performance
    /// </summary>
    public class NewsletterCampaignRecipientConfig : EntityTypeConfiguration<NewsletterCampaignRecipient>
    {
        public NewsletterCampaignRecipientConfig()
        {
            ToTable("NewsletterCampaignRecipients");
            HasKey(r => r.NewsletterCampaignRecipientId);

            Property(r => r.Email)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterCampaignRecipient_Email")));

            Property(r => r.Status)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterCampaignRecipient_Status")));

            Property(r => r.SentAt)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterCampaignRecipient_SentAt")));

            Property(r => r.OpenedAt)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterCampaignRecipient_OpenedAt")));

            Property(r => r.ClickedAt)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterCampaignRecipient_ClickedAt")));

            HasRequired(r => r.Campaign)
                .WithMany()
                .HasForeignKey(r => r.NewsletterCampaignId)
                .WillCascadeOnDelete(true);

            HasRequired(r => r.Subscription)
                .WithMany()
                .HasForeignKey(r => r.NewsletterSubscriptionId)
                .WillCascadeOnDelete(false);

            HasOptional(r => r.CreatedByUser)
                .WithMany()
                .HasForeignKey(r => r.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(r => r.UpdatedByUser)
                .WithMany()
                .HasForeignKey(r => r.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس ترکیبی برای Query های رایج
            HasIndex(r => new { r.NewsletterCampaignId, r.Status })
                .HasName("IX_NewsletterCampaignRecipient_CampaignId_Status");

            HasIndex(r => new { r.NewsletterSubscriptionId, r.NewsletterCampaignId })
                .HasName("IX_NewsletterCampaignRecipient_SubscriptionId_CampaignId");
        }
    }
}

