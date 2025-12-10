using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// مدل ساعات کاری کلینیک (Clinic Working Hours)
    /// طراحی شده برای سیستم مدیریت محتوا (CMS)
    /// برای نمایش ساعات کاری عمومی کلینیک در صفحه تماس و اطلاع‌رسانی
    /// اصول: SRP, Strongly-Typed, Bulletproof
    /// 
    /// تفاوت با DoctorSchedule:
    /// - DoctorSchedule: برنامه کاری هر پزشک به صورت جداگانه (برای نوبت‌دهی)
    /// - ClinicWorkingHours: ساعات کاری عمومی کلینیک (برای اطلاع‌رسانی)
    /// </summary>
    public class ClinicWorkingHours : ISoftDelete, ITrackable
    {
        public int ClinicWorkingHoursId { get; set; }

        /// <summary>
        /// شناسه کلینیک (اختیاری - اگر چند کلینیک داریم)
        /// اگر null باشد، برای کلینیک پیش‌فرض است
        /// </summary>
        public int? ClinicId { get; set; }

        /// <summary>
        /// روز هفته (0=شنبه, 1=یکشنبه, 2=دوشنبه, ..., 6=جمعه)
        /// </summary>
        [Required(ErrorMessage = "روز هفته الزامی است.")]
        [Range(0, 6, ErrorMessage = "روز هفته باید بین 0 تا 6 باشد.")]
        public int DayOfWeek { get; set; }

        /// <summary>
        /// نام روز هفته (برای نمایش)
        /// </summary>
        [Required(ErrorMessage = "نام روز هفته الزامی است.")]
        [MaxLength(20, ErrorMessage = "نام روز هفته نمی‌تواند بیش از 20 کاراکتر باشد.")]
        public string DayName { get; set; }

        /// <summary>
        /// زمان شروع کار
        /// </summary>
        [Required(ErrorMessage = "زمان شروع الزامی است.")]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان کار
        /// </summary>
        [Required(ErrorMessage = "زمان پایان الزامی است.")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// آیا در این روز کلینیک باز است؟
        /// </summary>
        public bool IsOpen { get; set; } = true;

        /// <summary>
        /// آیا این رکورد فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// توضیحات اضافی (اختیاری)
        /// مثال: "فقط پذیرش با نوبت قبلی"
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Notes { get; set; }

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
    /// پیکربندی Entity Framework برای ClinicWorkingHours
    /// بهینه‌سازی شده برای Query Performance
    /// </summary>
    public class ClinicWorkingHoursConfig : EntityTypeConfiguration<ClinicWorkingHours>
    {
        public ClinicWorkingHoursConfig()
        {
            ToTable("ClinicWorkingHours");
            HasKey(c => c.ClinicWorkingHoursId);

            Property(c => c.DayOfWeek)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ClinicWorkingHours_DayOfWeek")));

            Property(c => c.IsOpen)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ClinicWorkingHours_IsOpen")));

            Property(c => c.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ClinicWorkingHours_IsActive")));

            Property(c => c.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ClinicWorkingHours_IsDeleted")));

            Property(c => c.ClinicId)
                .IsOptional()
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_ClinicWorkingHours_ClinicId")));

            HasOptional(c => c.CreatedByUser)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.UpdatedByUser)
                .WithMany()
                .HasForeignKey(c => c.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.DeletedByUser)
                .WithMany()
                .HasForeignKey(c => c.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس ترکیبی برای Query های رایج
            HasIndex(c => new { c.ClinicId, c.DayOfWeek, c.IsActive, c.IsDeleted })
                .HasName("IX_ClinicWorkingHours_ClinicId_DayOfWeek_Active_Deleted");

            HasIndex(c => new { c.IsActive, c.IsDeleted, c.DisplayOrder })
                .HasName("IX_ClinicWorkingHours_Active_Deleted_Order");
        }
    }
}

