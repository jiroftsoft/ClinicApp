using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Clinic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل اطلاعات خدمات پزشکی (Medical Service Information)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// لینک به Service موجود در سیستم برای اطلاعات CMS اضافی
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class MedicalServiceInfo : ISoftDelete, ITrackable
    {
        public int MedicalServiceInfoId { get; set; }

        [Required(ErrorMessage = "خدمت الزامی است.")]
        public int ServiceId { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات کوتاه نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Description { get; set; }

        [Column(TypeName = "ntext")]
        public string FullDescription { get; set; } // توضیحات کامل

        [MaxLength(2000, ErrorMessage = "ویژگی‌ها نمی‌توانند بیش از 2000 کاراکتر باشند.")]
        public string Features { get; set; } // JSON یا comma-separated list

        [MaxLength(500, ErrorMessage = "آدرس تصویر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string ImageUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر کوچک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string ThumbnailUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس ویدیو نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string VideoUrl { get; set; }

        public decimal? Price { get; set; } // قیمت نمایشی (اختیاری - می‌تواند از Service بگیرد)

        [MaxLength(2000, ErrorMessage = "اطلاعات پوشش بیمه نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        public string InsuranceCoverage { get; set; } // JSON یا comma-separated list

        [MaxLength(500, ErrorMessage = "مدت زمان تقریبی نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string EstimatedDuration { get; set; } // مثال: "30 دقیقه"

        [MaxLength(500, ErrorMessage = "مدارک مورد نیاز نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string RequiredDocuments { get; set; } // JSON یا comma-separated list

        public bool IsActive { get; set; }

        public bool IsFeatured { get; set; } // نمایش در صفحه اصلی

        public int DisplayOrder { get; set; }

        public int ViewCount { get; set; }

        [MaxLength(500, ErrorMessage = "عنوان متا نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string MetaTitle { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات متا نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string MetaDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Slug نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Slug { get; set; }

        [MaxLength(500, ErrorMessage = "لینک مرتبط نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string RelatedLinkUrl { get; set; }

        // Navigation Property
        public virtual Service Service { get; set; }

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
    /// پیکربندی Entity Framework برای MedicalServiceInfo
    /// بهینه‌سازی شده برای Query Performance
    /// </summary>
    public class MedicalServiceInfoConfig : EntityTypeConfiguration<MedicalServiceInfo>
    {
        public MedicalServiceInfoConfig()
        {
            ToTable("MedicalServiceInfos");
            HasKey(m => m.MedicalServiceInfoId);

            Property(m => m.ServiceId)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalServiceInfo_ServiceId")));

            Property(m => m.Slug)
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalServiceInfo_Slug") { IsUnique = true }));

            Property(m => m.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalServiceInfo_IsActive")));

            Property(m => m.IsFeatured)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalServiceInfo_IsFeatured")));

            Property(m => m.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalServiceInfo_DisplayOrder")));

            Property(m => m.ViewCount)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalServiceInfo_ViewCount")));

            Property(m => m.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_MedicalServiceInfo_IsDeleted")));

            // رابطه با Service
            HasRequired(m => m.Service)
                .WithMany()
                .HasForeignKey(m => m.ServiceId)
                .WillCascadeOnDelete(false);

            HasOptional(m => m.CreatedByUser)
                .WithMany()
                .HasForeignKey(m => m.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(m => m.UpdatedByUser)
                .WithMany()
                .HasForeignKey(m => m.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(m => m.DeletedByUser)
                .WithMany()
                .HasForeignKey(m => m.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس ترکیبی برای Query های رایج
            HasIndex(m => new { m.IsActive, m.IsDeleted, m.ServiceId })
                .HasName("IX_MedicalServiceInfo_Active_Deleted_ServiceId");

            HasIndex(m => new { m.IsFeatured, m.IsActive, m.DisplayOrder })
                .HasName("IX_MedicalServiceInfo_Featured_Active_Order");
        }
    }
}

