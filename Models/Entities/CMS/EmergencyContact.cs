using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل تماس‌های اضطراری (Emergency Contact)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// برای نمایش اطلاعات تماس اضطراری و اورژانس در سایت
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class EmergencyContact : ISoftDelete, ITrackable
    {
        public int EmergencyContactId { get; set; }

        [Required(ErrorMessage = "نوع تماس الزامی است.")]
        [MaxLength(50, ErrorMessage = "نوع تماس نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string ContactType { get; set; } // Emergency, Ambulance, Poison Control, Fire, Police

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "شماره تماس الزامی است.")]
        [MaxLength(50, ErrorMessage = "شماره تماس نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string PhoneNumber { get; set; }

        [MaxLength(50, ErrorMessage = "شماره تماس دوم نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string SecondaryPhoneNumber { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Address { get; set; }

        [MaxLength(2000, ErrorMessage = "دستورالعمل‌ها نمی‌توانند بیش از 2000 کاراکتر باشند.")]
        public string Instructions { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس نقشه نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string MapUrl { get; set; }

        [MaxLength(500, ErrorMessage = "لینک واتساپ نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string WhatsAppUrl { get; set; }

        [MaxLength(500, ErrorMessage = "لینک تلگرام نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string TelegramUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس ایمیل نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        public string Email { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس وب‌سایت نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string WebsiteUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس آیکون نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string IconUrl { get; set; }

        public bool IsActive { get; set; }

        public bool IsAlwaysVisible { get; set; } // نمایش همیشه در دسترس (مثلاً در Header)

        public int DisplayOrder { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات کوتاه نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string ShortDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Slug نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Slug { get; set; }

        [MaxLength(500, ErrorMessage = "عنوان متا نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string MetaTitle { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات متا نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string MetaDescription { get; set; }

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
    /// پیکربندی Entity Framework برای EmergencyContact
    /// بهینه‌سازی شده برای Query Performance
    /// </summary>
    public class EmergencyContactConfig : EntityTypeConfiguration<EmergencyContact>
    {
        public EmergencyContactConfig()
        {
            ToTable("EmergencyContacts");
            HasKey(e => e.EmergencyContactId);

            Property(e => e.ContactType)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_EmergencyContact_ContactType")));

            Property(e => e.Slug)
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_EmergencyContact_Slug") { IsUnique = true }));

            Property(e => e.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_EmergencyContact_IsActive")));

            Property(e => e.IsAlwaysVisible)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_EmergencyContact_IsAlwaysVisible")));

            Property(e => e.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_EmergencyContact_DisplayOrder")));

            Property(e => e.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_EmergencyContact_IsDeleted")));

            HasOptional(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(e => e.DeletedByUser)
                .WithMany()
                .HasForeignKey(e => e.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس ترکیبی برای Query های رایج
            HasIndex(e => new { e.IsActive, e.IsDeleted, e.ContactType })
                .HasName("IX_EmergencyContact_Active_Deleted_Type");

            HasIndex(e => new { e.IsAlwaysVisible, e.IsActive, e.DisplayOrder })
                .HasName("IX_EmergencyContact_AlwaysVisible_Active_Order");
        }
    }
}

