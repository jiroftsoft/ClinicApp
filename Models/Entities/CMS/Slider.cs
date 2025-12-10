using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل اسلایدرهای صفحه اصلی
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// </summary>
    public class Slider : ISoftDelete, ITrackable
    {
        public int SliderId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "تصویر الزامی است.")]
        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(500)]
        public string ThumbnailUrl { get; set; }

        [MaxLength(500)]
        public string LinkUrl { get; set; }

        [MaxLength(100)]
        public string ButtonText { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(50)]
        public string Position { get; set; } // "hero", "sidebar", "footer"

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

    public class SliderConfig : EntityTypeConfiguration<Slider>
    {
        public SliderConfig()
        {
            ToTable("Sliders");
            HasKey(s => s.SliderId);

            Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Slider_Title")));

            Property(s => s.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Slider_IsActive")));

            Property(s => s.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Slider_DisplayOrder")));

            Property(s => s.Position)
                .HasMaxLength(50)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Slider_Position")));

            Property(s => s.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Slider_IsDeleted")));

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

            HasIndex(s => new { s.IsActive, s.IsDeleted, s.DisplayOrder })
                .HasName("IX_Slider_Active_Deleted_Order");
        }
    }
}

