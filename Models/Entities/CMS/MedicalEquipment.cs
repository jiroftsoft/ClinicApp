using ClinicApp.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل تجهیزات پزشکی (Medical Equipment)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// برای معرفی تجهیزات و تکنولوژی‌های کلینیک
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class MedicalEquipment : ISoftDelete, ITrackable
    {
        public int MedicalEquipmentId { get; set; }

        [Required(ErrorMessage = "نام تجهیز الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام تجهیز نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string EquipmentName { get; set; }

        [MaxLength(100, ErrorMessage = "مدل نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string Model { get; set; }

        [MaxLength(200, ErrorMessage = "سازنده نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Manufacturer { get; set; }

        [Required(ErrorMessage = "دسته‌بندی الزامی است.")]
        [MaxLength(100, ErrorMessage = "دسته‌بندی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string Category { get; set; } // Imaging, Laboratory, Surgery, Monitoring, etc.

        [MaxLength(2000, ErrorMessage = "توضیحات نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        public string Description { get; set; }

        [MaxLength(5000, ErrorMessage = "مشخصات فنی نمی‌تواند بیش از 5000 کاراکتر باشد.")]
        public string TechnicalSpecifications { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر اصلی نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// لیست تصاویر اضافی (JSON Array)
        /// </summary>
        [MaxLength(2000, ErrorMessage = "لیست تصاویر نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        public string ImageUrls { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس ویدیو نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string VideoUrl { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public DateTime? InstallationDate { get; set; }

        public DateTime? WarrantyExpiryDate { get; set; }

        [MaxLength(50, ErrorMessage = "وضعیت نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string Status { get; set; } // Active, Maintenance, Inactive

        public bool IsActive { get; set; }

        public bool IsFeatured { get; set; } // برای نمایش در صفحه اصلی

        public int DisplayOrder { get; set; }

        /// <summary>
        /// لیست ویژگی‌ها (JSON Array)
        /// </summary>
        [MaxLength(2000, ErrorMessage = "لیست ویژگی‌ها نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        public string Features { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات کوتاه نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string ShortDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Slug نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Slug { get; set; }

        [MaxLength(500, ErrorMessage = "عنوان متا نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string MetaTitle { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات متا نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string MetaDescription { get; set; }

        /// <summary>
        /// تعداد بازدید
        /// </summary>
        public int ViewCount { get; set; }

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
    /// پیکربندی Entity Framework برای MedicalEquipment
    /// بهینه‌سازی شده برای Query Performance
    /// </summary>
    public class MedicalEquipmentConfig : EntityTypeConfiguration<MedicalEquipment>
    {
        public MedicalEquipmentConfig()
        {
            ToTable("MedicalEquipments");
            HasKey(e => e.MedicalEquipmentId);

            Property(e => e.EquipmentName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalEquipment_EquipmentName")));

            Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalEquipment_Category")));

            Property(e => e.Slug)
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalEquipment_Slug") { IsUnique = true }));

            Property(e => e.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalEquipment_IsActive")));

            Property(e => e.IsFeatured)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalEquipment_IsFeatured")));

            Property(e => e.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalEquipment_DisplayOrder")));

            Property(e => e.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalEquipment_IsDeleted")));

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
            HasIndex(e => new { e.IsActive, e.IsDeleted, e.Category })
                .HasName("IX_MedicalEquipment_Active_Deleted_Category");

            HasIndex(e => new { e.IsFeatured, e.IsActive, e.DisplayOrder })
                .HasName("IX_MedicalEquipment_Featured_Active_Order");
        }
    }
}

