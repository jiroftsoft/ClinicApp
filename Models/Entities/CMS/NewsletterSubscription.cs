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
    /// مدل اشتراک خبرنامه (Newsletter Subscription)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// برای مدیریت مشترکین خبرنامه
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class NewsletterSubscription : ISoftDelete, ITrackable
    {
        public int NewsletterSubscriptionId { get; set; }

        [Required(ErrorMessage = "ایمیل الزامی است.")]
        [MaxLength(200, ErrorMessage = "ایمیل نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        public string Email { get; set; }

        [MaxLength(200, ErrorMessage = "نام و نام خانوادگی نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string FullName { get; set; }

        [MaxLength(50, ErrorMessage = "شماره تماس نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// دسته‌بندی‌های انتخاب شده (JSON Array)
        /// مثال: ["Articles", "Announcements"]
        /// </summary>
        [Column(TypeName = "ntext")]
        public string Categories { get; set; }

        [Required(ErrorMessage = "منبع ثبت‌نام الزامی است.")]
        public NewsletterSubscriptionSource Source { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// آیا ایمیل تایید شده است (Double Opt-in)
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Token برای تایید ایمیل (Double Opt-in)
        /// </summary>
        [MaxLength(100, ErrorMessage = "Verification Token نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string VerificationToken { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public DateTime? UnsubscribedAt { get; set; }

        /// <summary>
        /// Token برای لغو اشتراک
        /// </summary>
        [MaxLength(100, ErrorMessage = "Unsubscribe Token نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string UnsubscribeToken { get; set; }

        [MaxLength(500, ErrorMessage = "IP Address نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string IpAddress { get; set; }

        [MaxLength(500, ErrorMessage = "User Agent نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string UserAgent { get; set; }

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
    /// پیکربندی Entity Framework برای NewsletterSubscription
    /// بهینه‌سازی شده برای Query Performance
    /// </summary>
    public class NewsletterSubscriptionConfig : EntityTypeConfiguration<NewsletterSubscription>
    {
        public NewsletterSubscriptionConfig()
        {
            ToTable("NewsletterSubscriptions");
            HasKey(n => n.NewsletterSubscriptionId);

            Property(n => n.Email)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterSubscription_Email") { IsUnique = true }));

            Property(n => n.FullName)
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterSubscription_FullName")));

            Property(n => n.Source)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterSubscription_Source")));

            Property(n => n.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterSubscription_IsActive")));

            Property(n => n.IsVerified)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterSubscription_IsVerified")));

            Property(n => n.VerificationToken)
                .HasMaxLength(100)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterSubscription_VerificationToken") { IsUnique = true }));

            Property(n => n.UnsubscribeToken)
                .HasMaxLength(100)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterSubscription_UnsubscribeToken") { IsUnique = true }));

            Property(n => n.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterSubscription_IsDeleted")));

            Property(n => n.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_NewsletterSubscription_CreatedAt")));

            HasOptional(n => n.CreatedByUser)
                .WithMany()
                .HasForeignKey(n => n.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(n => n.UpdatedByUser)
                .WithMany()
                .HasForeignKey(n => n.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(n => n.DeletedByUser)
                .WithMany()
                .HasForeignKey(n => n.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس ترکیبی برای Query های رایج
            HasIndex(n => new { n.IsActive, n.IsVerified, n.IsDeleted, n.Source })
                .HasName("IX_NewsletterSubscription_Active_Verified_Deleted_Source");

            HasIndex(n => new { n.IsActive, n.IsVerified, n.CreatedAt })
                .HasName("IX_NewsletterSubscription_Active_Verified_CreatedAt");
        }
    }
}

