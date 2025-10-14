using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Core;

namespace ClinicApp.Models.Entities.Doctor;

/// <summary>
/// مدل تخصص‌های پزشکی - طراحی شده برای مدیریت تخصص‌ها در کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت متمرکز تخصص‌های پزشکی
/// 2. امکان فعال/غیرفعال کردن تخصص‌ها
/// 3. ردیابی کامل تغییرات برای امنیت سیستم
/// 4. پشتیبانی از سیستم حذف نرم
/// </summary>
public class Specialization : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه تخصص
    /// </summary>
    public int SpecializationId { get; set; }

    /// <summary>
    /// نام تخصص
    /// مثال: "متخصص داخلی", "متخصص پوست", "متخصص قلب"
    /// </summary>
    [Required(ErrorMessage = "نام تخصص الزامی است.")]
    [MaxLength(100, ErrorMessage = "نام تخصص نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string Name { get; set; }

    /// <summary>
    /// توضیحات تخصص
    /// </summary>
    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// وضعیت فعال/غیرفعال بودن تخصص
    /// </summary>
    [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ترتیب نمایش تخصص
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    #region پیاده‌سازی ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string DeletedByUserId { get; set; }
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string CreatedByUserId { get; set; }
    public virtual ApplicationUser CreatedByUser { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedByUserId { get; set; }
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    #region روابط
    /// <summary>
    /// لیست روابط پزشک-تخصص
    /// </summary>
    public virtual ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new HashSet<DoctorSpecialization>();
    #endregion
}

#region Specialization Entity

/// <summary>
/// پیکربندی مدل تخصص‌ها برای Entity Framework
/// </summary>
public class SpecializationConfig : EntityTypeConfiguration<Specialization>
{
    public SpecializationConfig()
    {
        ToTable("Specializations");
        HasKey(s => s.SpecializationId);

        Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Specialization_Name")));

        Property(s => s.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Specialization_IsActive")));

        Property(s => s.DisplayOrder)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Specialization_DisplayOrder")));

        // پیاده‌سازی ISoftDelete
        Property(s => s.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Specialization_IsDeleted")));

        Property(s => s.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Specialization_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(s => s.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Specialization_CreatedAt")));

        Property(s => s.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Specialization_CreatedByUserId")));

        Property(s => s.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Specialization_UpdatedAt")));

        Property(s => s.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Specialization_UpdatedByUserId")));

        Property(s => s.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Specialization_DeletedByUserId")));

        // روابط Many-to-Many با پزشکان
        HasMany(s => s.DoctorSpecializations)
            .WithRequired(ds => ds.Specialization)
            .HasForeignKey(ds => ds.SpecializationId)
            .WillCascadeOnDelete(false);

        // روابط Audit
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
    }
}
#endregion