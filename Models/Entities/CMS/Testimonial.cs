using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل نظرات و رضایت‌نامه‌های بیماران
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// </summary>
    public class Testimonial : ISoftDelete, ITrackable
    {
        public int TestimonialId { get; set; }

        [Required(ErrorMessage = "نام بیمار الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string PatientName { get; set; }

        [MaxLength(10)]
        public string PatientInitials { get; set; }

        [Required(ErrorMessage = "نظر الزامی است.")]
        [MaxLength(2000, ErrorMessage = "نظر نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        public string Comment { get; set; }

        [Range(0, 5, ErrorMessage = "امتیاز باید بین 0 تا 5 باشد.")]
        public decimal Rating { get; set; }

        [MaxLength(200)]
        public string DoctorName { get; set; }

        [MaxLength(500)]
        public string PhotoUrl { get; set; }

        [MaxLength(500)]
        public string VideoUrl { get; set; }

        public bool IsApproved { get; set; }

        public bool IsFeatured { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public int? PatientId { get; set; }

        public int? DoctorId { get; set; }

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

    public class TestimonialConfig : EntityTypeConfiguration<Testimonial>
    {
        public TestimonialConfig()
        {
            ToTable("Testimonials");
            HasKey(t => t.TestimonialId);

            Property(t => t.PatientName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Testimonial_PatientName")));

            Property(t => t.Rating)
                .IsRequired()
                .HasPrecision(3, 2)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Testimonial_Rating")));

            Property(t => t.IsApproved)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Testimonial_IsApproved")));

            Property(t => t.IsFeatured)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Testimonial_IsFeatured")));

            Property(t => t.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Testimonial_DisplayOrder")));

            Property(t => t.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Testimonial_IsDeleted")));

            HasOptional(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.UpdatedByUser)
                .WithMany()
                .HasForeignKey(t => t.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.DeletedByUser)
                .WithMany()
                .HasForeignKey(t => t.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasIndex(t => new { t.IsApproved, t.IsFeatured, t.IsDeleted, t.DisplayOrder })
                .HasName("IX_Testimonial_Approved_Featured_Deleted_Order");
        }
    }
}

