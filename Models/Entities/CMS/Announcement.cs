using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل اطلاعیه‌ها و اعلان‌های کلینیک
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// </summary>
    public class Announcement : ISoftDelete, ITrackable
    {
        public int AnnouncementId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(300, ErrorMessage = "عنوان نمی‌تواند بیش از 300 کاراکتر باشد.")]
        public string Title { get; set; }

        [MaxLength(2000, ErrorMessage = "محتوا نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        public string Content { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(500)]
        public string LinkUrl { get; set; }

        public bool IsActive { get; set; }

        public bool IsImportant { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } // "info", "warning", "success", "error"

        [MaxLength(100)]
        public string TargetAudience { get; set; } // "all", "patients", "doctors", "staff"

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

    public class AnnouncementConfig : EntityTypeConfiguration<Announcement>
    {
        public AnnouncementConfig()
        {
            ToTable("Announcements");
            HasKey(a => a.AnnouncementId);

            Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Announcement_Title")));

            Property(a => a.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Announcement_IsActive")));

            Property(a => a.IsImportant)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Announcement_IsImportant")));

            Property(a => a.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Announcement_DisplayOrder")));

            Property(a => a.Type)
                .HasMaxLength(50)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Announcement_Type")));

            Property(a => a.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Announcement_IsDeleted")));

            HasOptional(a => a.CreatedByUser)
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.UpdatedByUser)
                .WithMany()
                .HasForeignKey(a => a.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.DeletedByUser)
                .WithMany()
                .HasForeignKey(a => a.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasIndex(a => new { a.IsActive, a.IsDeleted, a.DisplayOrder, a.StartDate, a.EndDate })
                .HasName("IX_Announcement_Active_Deleted_Order_Date");
        }
    }
}

