using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل استوری‌ها (Stories) - مشابه دیجی‌کالا
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// </summary>
    public class Story : ISoftDelete, ITrackable
    {
        public int StoryId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Description { get; set; }

        /// <summary>
        /// آدرس ویدیو (مستقیم یا YouTube/Vimeo)
        /// </summary>
        [MaxLength(1000)]
        public string VideoUrl { get; set; }

        /// <summary>
        /// نوع منبع ویدیو (YouTube, Vimeo, DirectUpload)
        /// </summary>
        [MaxLength(50)]
        public string VideoType { get; set; } // "YouTube", "Vimeo", "DirectUpload"

        /// <summary>
        /// تصویر Thumbnail برای استوری
        /// </summary>
        [Required(ErrorMessage = "تصویر Thumbnail الزامی است.")]
        [MaxLength(500)]
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// لینک کلیک (اختیاری - برای هدایت به صفحه خاص)
        /// </summary>
        [MaxLength(500)]
        public string LinkUrl { get; set; }

        /// <summary>
        /// متن دکمه (اختیاری)
        /// </summary>
        [MaxLength(100)]
        public string ButtonText { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ترتیب نمایش (برای مرتب‌سازی)
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// تاریخ شروع نمایش (اختیاری)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان نمایش (اختیاری)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// تعداد بازدید
        /// </summary>
        public int ViewCount { get; set; } = 0;

        /// <summary>
        /// مدت زمان ویدیو به ثانیه (اختیاری)
        /// </summary>
        public int? Duration { get; set; }

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
    /// پیکربندی Entity Framework برای Story
    /// طراحی شده برای بهینه‌سازی Performance و Indexing
    /// </summary>
    public class StoryConfig : EntityTypeConfiguration<Story>
    {
        public StoryConfig()
        {
            ToTable("Stories");
            HasKey(s => s.StoryId);

            Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Story_Title")));

            Property(s => s.ThumbnailUrl)
                .IsRequired()
                .HasMaxLength(500);

            Property(s => s.VideoType)
                .HasMaxLength(50)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Story_VideoType")));

            Property(s => s.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Story_IsActive")));

            Property(s => s.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Story_DisplayOrder")));

            Property(s => s.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Story_IsDeleted")));

            Property(s => s.ViewCount)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Story_ViewCount")));

            HasOptional(s => s.CreatedByUser)
                .WithMany()
                .HasForeignKey(s => s.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(s => s.UpdatedByUser)
                .WithMany()
                .HasForeignKey(s => s.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(s => s.DeletedByUser)
                .WithMany()
                .HasForeignKey(s => s.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس ترکیبی برای بهبود Performance در جستجوهای رایج
            HasIndex(s => new { s.IsActive, s.IsDeleted, s.DisplayOrder, s.StartDate, s.EndDate })
                .HasName("IX_Story_Active_Deleted_Order_Dates");
        }
    }
}
