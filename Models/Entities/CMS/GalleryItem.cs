using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل آیتم‌های گالری تصاویر
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// </summary>
    public class GalleryItem : ISoftDelete, ITrackable
    {
        public int GalleryItemId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Title { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "تصویر الزامی است.")]
        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(500)]
        public string ThumbnailUrl { get; set; }

        [MaxLength(100)]
        public string Category { get; set; } // "clinic", "doctors", "equipment", "events"

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }

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

    public class GalleryItemConfig : EntityTypeConfiguration<GalleryItem>
    {
        public GalleryItemConfig()
        {
            ToTable("GalleryItems");
            HasKey(g => g.GalleryItemId);

            Property(g => g.Title)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_GalleryItem_Title")));

            Property(g => g.Category)
                .HasMaxLength(100)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_GalleryItem_Category")));

            Property(g => g.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_GalleryItem_IsActive")));

            Property(g => g.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_GalleryItem_DisplayOrder")));

            Property(g => g.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_GalleryItem_IsDeleted")));

            HasOptional(g => g.CreatedByUser)
                .WithMany()
                .HasForeignKey(g => g.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(g => g.UpdatedByUser)
                .WithMany()
                .HasForeignKey(g => g.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(g => g.DeletedByUser)
                .WithMany()
                .HasForeignKey(g => g.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasIndex(g => new { g.IsActive, g.IsDeleted, g.DisplayOrder, g.Category })
                .HasName("IX_GalleryItem_Active_Deleted_Order_Category");
        }
    }
}

