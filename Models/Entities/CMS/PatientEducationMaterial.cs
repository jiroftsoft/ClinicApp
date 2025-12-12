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
    /// مدل مطالب آموزشی بیماران (Patient Education Materials)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// برای مدیریت فایل‌های آموزشی و راهنما برای بیماران
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class PatientEducationMaterial : ISoftDelete, ITrackable
    {
        public int PatientEducationMaterialId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(300, ErrorMessage = "عنوان نمی‌تواند بیش از 300 کاراکتر باشد.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "توضیحات الزامی است.")]
        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "محتوا الزامی است.")]
        [Column(TypeName = "ntext")]
        public string Content { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس فایل نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string FileUrl { get; set; } // PDF, Word, Excel

        [MaxLength(100, ErrorMessage = "نام فایل نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string FileName { get; set; }

        [MaxLength(50, ErrorMessage = "نوع فایل نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string FileType { get; set; } // PDF, DOC, DOCX, XLS, XLSX

        public long? FileSizeInBytes { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس ویدیو نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string VideoUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string ImageUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر کوچک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string ThumbnailUrl { get; set; }

        [Required(ErrorMessage = "دسته‌بندی الزامی است.")]
        public PatientEducationCategory Category { get; set; }

        [MaxLength(500, ErrorMessage = "برچسب‌ها نمی‌توانند بیش از 500 کاراکتر باشند.")]
        public string Tags { get; set; } // Comma-separated tags

        public DateTime? PublishedAt { get; set; }

        public bool IsPublished { get; set; }

        public bool IsFeatured { get; set; }

        public int DownloadCount { get; set; }

        public int ViewCount { get; set; }

        public int DisplayOrder { get; set; }

        [MaxLength(500, ErrorMessage = "عنوان متا نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string MetaTitle { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات متا نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string MetaDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Slug نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Slug { get; set; }

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
    /// پیکربندی Entity Framework برای PatientEducationMaterial
    /// بهینه‌سازی شده برای Query Performance
    /// </summary>
    public class PatientEducationMaterialConfig : EntityTypeConfiguration<PatientEducationMaterial>
    {
        public PatientEducationMaterialConfig()
        {
            ToTable("PatientEducationMaterials");
            HasKey(p => p.PatientEducationMaterialId);

            Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PatientEducationMaterial_Title")));

            Property(p => p.Category)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PatientEducationMaterial_Category")));

            Property(p => p.IsPublished)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PatientEducationMaterial_IsPublished")));

            Property(p => p.IsFeatured)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PatientEducationMaterial_IsFeatured")));

            Property(p => p.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PatientEducationMaterial_DisplayOrder")));

            Property(p => p.Slug)
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PatientEducationMaterial_Slug") { IsUnique = true }));

            Property(p => p.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PatientEducationMaterial_IsDeleted")));

            Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PatientEducationMaterial_CreatedAt")));

            HasOptional(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(p => p.UpdatedByUser)
                .WithMany()
                .HasForeignKey(p => p.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(p => p.DeletedByUser)
                .WithMany()
                .HasForeignKey(p => p.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس ترکیبی برای Query های رایج
            HasIndex(p => new { p.IsPublished, p.IsDeleted, p.CreatedAt })
                .HasName("IX_PatientEducationMaterial_Published_Deleted_CreatedAt");

            HasIndex(p => new { p.Category, p.IsPublished, p.IsDeleted })
                .HasName("IX_PatientEducationMaterial_Category_Published_Deleted");

            HasIndex(p => new { p.IsFeatured, p.IsPublished, p.DisplayOrder })
                .HasName("IX_PatientEducationMaterial_Featured_Published_Order");
        }
    }
}

