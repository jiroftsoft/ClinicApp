using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل اطلاعات بیمه (Insurance Information)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class InsuranceInfo : ISoftDelete, ITrackable
    {
        public int InsuranceInfoId { get; set; }

        [Required(ErrorMessage = "نام بیمه الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام بیمه نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string InsuranceName { get; set; }

        [MaxLength(100, ErrorMessage = "نوع بیمه نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string InsuranceType { get; set; } // "basic", "supplementary", "private", "government"

        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Description { get; set; }

        [Column(TypeName = "ntext")]
        public string FullDescription { get; set; } // توضیحات کامل

        [MaxLength(500, ErrorMessage = "آدرس تصویر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string LogoUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر کوچک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string ThumbnailUrl { get; set; }

        [MaxLength(200, ErrorMessage = "شماره تماس نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string ContactPhone { get; set; }

        [MaxLength(200, ErrorMessage = "وب‌سایت نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string WebsiteUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Address { get; set; }

        public decimal? CoveragePercentage { get; set; } // درصد پوشش

        public bool IsActive { get; set; } // آیا این بیمه فعال است

        public bool IsFeatured { get; set; } // نمایش در صفحه اصلی

        public int DisplayOrder { get; set; }

        public int ViewCount { get; set; }

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
    /// پیکربندی Entity Framework برای InsuranceInfo
    /// بهینه‌سازی شده برای Query Performance
    /// </summary>
    public class InsuranceInfoConfig : EntityTypeConfiguration<InsuranceInfo>
    {
        public InsuranceInfoConfig()
        {
            ToTable("InsuranceInfos");
            HasKey(i => i.InsuranceInfoId);

            Property(i => i.InsuranceName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_InsuranceInfo_Name")));

            Property(i => i.InsuranceType)
                .HasMaxLength(100)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_InsuranceInfo_Type")));

            Property(i => i.Slug)
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_InsuranceInfo_Slug") { IsUnique = true }));

            Property(i => i.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_InsuranceInfo_IsActive")));

            Property(i => i.IsFeatured)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_InsuranceInfo_IsFeatured")));

            Property(i => i.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_InsuranceInfo_DisplayOrder")));

            Property(i => i.ViewCount)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_InsuranceInfo_ViewCount")));

            Property(i => i.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_InsuranceInfo_IsDeleted")));

            HasOptional(i => i.CreatedByUser)
                .WithMany()
                .HasForeignKey(i => i.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(i => i.UpdatedByUser)
                .WithMany()
                .HasForeignKey(i => i.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(i => i.DeletedByUser)
                .WithMany()
                .HasForeignKey(i => i.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس ترکیبی برای Query های رایج
            HasIndex(i => new { i.IsActive, i.IsDeleted, i.InsuranceType })
                .HasName("IX_InsuranceInfo_Active_Deleted_Type");

            HasIndex(i => new { i.IsFeatured, i.IsActive, i.DisplayOrder })
                .HasName("IX_InsuranceInfo_Featured_Active_Order");
        }
    }
}

