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
    /// مدل فرم تماس (Contact Form)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// برای مدیریت پیام‌های دریافتی از فرم تماس سایت
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class ContactForm : ISoftDelete, ITrackable
    {
        public int ContactFormId { get; set; }

        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام و نام خانوادگی نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "ایمیل الزامی است.")]
        [MaxLength(200, ErrorMessage = "ایمیل نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "شماره تماس الزامی است.")]
        [MaxLength(50, ErrorMessage = "شماره تماس نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "موضوع الزامی است.")]
        [MaxLength(500, ErrorMessage = "موضوع نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "پیام الزامی است.")]
        [MaxLength(5000, ErrorMessage = "پیام نمی‌تواند بیش از 5000 کاراکتر باشد.")]
        public string Message { get; set; }

        [Required(ErrorMessage = "دسته‌بندی الزامی است.")]
        public ContactFormCategory Category { get; set; }

        [Required(ErrorMessage = "وضعیت الزامی است.")]
        public ContactFormStatus Status { get; set; }

        [MaxLength(5000, ErrorMessage = "پاسخ نمی‌تواند بیش از 5000 کاراکتر باشد.")]
        public string ReplyMessage { get; set; }

        public DateTime? RepliedAt { get; set; }

        public string RepliedByUserId { get; set; }
        public virtual ApplicationUser RepliedByUser { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }

        public string ReadByUserId { get; set; }
        public virtual ApplicationUser ReadByUser { get; set; }

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
    /// پیکربندی Entity Framework برای ContactForm
    /// بهینه‌سازی شده برای Query Performance
    /// </summary>
    public class ContactFormConfig : EntityTypeConfiguration<ContactForm>
    {
        public ContactFormConfig()
        {
            ToTable("ContactForms");
            HasKey(c => c.ContactFormId);

            Property(c => c.FullName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ContactForm_FullName")));

            Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ContactForm_Email")));

            Property(c => c.PhoneNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ContactForm_PhoneNumber")));

            Property(c => c.Subject)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ContactForm_Subject")));

            Property(c => c.Category)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ContactForm_Category")));

            Property(c => c.Status)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ContactForm_Status")));

            Property(c => c.IsRead)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ContactForm_IsRead")));

            Property(c => c.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ContactForm_IsDeleted")));

            Property(c => c.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ContactForm_CreatedAt")));

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

            HasOptional(c => c.RepliedByUser)
                .WithMany()
                .HasForeignKey(c => c.RepliedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.ReadByUser)
                .WithMany()
                .HasForeignKey(c => c.ReadByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس ترکیبی برای Query های رایج
            HasIndex(c => new { c.Status, c.IsDeleted, c.CreatedAt })
                .HasName("IX_ContactForm_Status_Deleted_CreatedAt");

            HasIndex(c => new { c.Category, c.Status, c.IsDeleted })
                .HasName("IX_ContactForm_Category_Status_Deleted");

            HasIndex(c => new { c.IsRead, c.Status, c.IsDeleted })
                .HasName("IX_ContactForm_IsRead_Status_Deleted");
        }
    }
}

