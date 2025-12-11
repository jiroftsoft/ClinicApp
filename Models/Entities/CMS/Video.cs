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
    /// مدل ویدیوها برای سیستم مدیریت محتوا (CMS)
    /// طراحی شده بر اساس اصول SRP و برای محیط Production درمانی
    /// </summary>
    public class Video : ISoftDelete, ITrackable
    {
        public int VideoId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(500, ErrorMessage = "عنوان نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Title { get; set; }

        [MaxLength(2000, ErrorMessage = "توضیحات نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "آدرس ویدیو الزامی است.")]
        [MaxLength(1000)]
        public string VideoUrl { get; set; }

        /// <summary>
        /// نوع منبع ویدیو (YouTube, Vimeo, DirectUpload)
        /// </summary>
        [Required]
        public VideoType VideoType { get; set; }

        [MaxLength(500)]
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// دسته‌بندی ویدیو (مثل "endoscopy", "surgery", "general")
        /// </summary>
        [MaxLength(100)]
        public string Category { get; set; }

        /// <summary>
        /// مدت زمان ویدیو به ثانیه
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// تعداد بازدید
        /// </summary>
        public int ViewCount { get; set; } = 0;

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

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
    /// پیکربندی Entity Framework برای Video
    /// طراحی شده برای بهینه‌سازی Performance و Indexing
    /// </summary>
    public class VideoConfig : EntityTypeConfiguration<Video>
    {
        public VideoConfig()
        {
            ToTable("Videos");
            HasKey(v => v.VideoId);

            Property(v => v.Title)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Video_Title")));

            Property(v => v.VideoUrl)
                .IsRequired()
                .HasMaxLength(1000);

            Property(v => v.VideoType)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Video_VideoType")));

            Property(v => v.Category)
                .HasMaxLength(100)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Video_Category")));

            Property(v => v.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Video_IsActive")));

            Property(v => v.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Video_DisplayOrder")));

            Property(v => v.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Video_IsDeleted")));

            Property(v => v.ViewCount)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Video_ViewCount")));

            HasOptional(v => v.CreatedByUser)
                .WithMany()
                .HasForeignKey(v => v.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(v => v.UpdatedByUser)
                .WithMany()
                .HasForeignKey(v => v.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(v => v.DeletedByUser)
                .WithMany()
                .HasForeignKey(v => v.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس ترکیبی برای بهبود Performance در جستجوهای رایج
            HasIndex(v => new { v.IsActive, v.IsDeleted, v.DisplayOrder, v.Category })
                .HasName("IX_Video_Active_Deleted_Order_Category");
        }
    }
}

