using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Doctor;

/// <summary>
/// موجودیت قالب برنامه کاری برای استفاده مجدد از تنظیمات برنامه‌ریزی
/// این موجودیت امکان ذخیره و بازیابی قالب‌های برنامه کاری را فراهم می‌کند
/// </summary>
public class ScheduleTemplate : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه قالب
    /// </summary>
    public int TemplateId { get; set; }

    /// <summary>
    /// شناسه برنامه کاری پزشک
    /// </summary>
    [Required(ErrorMessage = "برنامه کاری الزامی است.")]
    public int ScheduleId { get; set; }

    /// <summary>
    /// نام قالب
    /// </summary>
    [Required(ErrorMessage = "نام قالب الزامی است.")]
    [MaxLength(100, ErrorMessage = "نام قالب نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string TemplateName { get; set; }

    /// <summary>
    /// توضیحات قالب
    /// </summary>
    [MaxLength(500, ErrorMessage = "توضیحات قالب نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// آیا قالب فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// آیا قالب پیش‌فرض است؟
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// نوع قالب
    /// </summary>
    [Required(ErrorMessage = "نوع قالب الزامی است.")]
    public TemplateType Type { get; set; }

    /// <summary>
    /// تنظیمات قالب (JSON)
    /// </summary>
    [MaxLength(4000, ErrorMessage = "تنظیمات قالب نمی‌تواند بیش از 4000 کاراکتر باشد.")]
    public string TemplateData { get; set; }

    #region پیاده‌سازی ISoftDelete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    [MaxLength(128)]
    public string DeletedByUserId { get; set; }
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [MaxLength(128)]
    public string CreatedByUserId { get; set; }
    public virtual ApplicationUser CreatedByUser { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [MaxLength(128)]
    public string UpdatedByUserId { get; set; }
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    // Navigation Properties
    public virtual DoctorSchedule Schedule { get; set; }

}
/// <summary>
/// کانفیگ Entity Framework برای مدل ScheduleTemplate
/// </summary>
public class ScheduleTemplateConfiguration : EntityTypeConfiguration<ScheduleTemplate>
{
    public ScheduleTemplateConfiguration()
    {
        // نام جدول
        ToTable("ScheduleTemplates");

        // کلید اصلی
        HasKey(st => st.TemplateId);

        // پراپرتی‌های اصلی
        Property(st => st.TemplateId)
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        Property(st => st.ScheduleId)
            .IsRequired();

        Property(st => st.TemplateName)
            .IsRequired()
            .HasMaxLength(100);

        Property(st => st.Description)
            .IsOptional()
            .HasMaxLength(500);

        Property(st => st.Type)
            .IsRequired();

        Property(st => st.TemplateData)
            .IsOptional()
            .HasMaxLength(4000);

        // پیاده‌سازی ISoftDelete
        Property(st => st.IsDeleted)
            .IsRequired();

        Property(st => st.DeletedAt)
            .IsOptional();

        Property(st => st.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        // پیاده‌سازی ITrackable
        Property(st => st.CreatedAt)
            .IsRequired();

        Property(st => st.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        Property(st => st.UpdatedAt)
            .IsOptional();

        Property(st => st.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128);

        // روابط
        HasRequired(st => st.Schedule)
            .WithMany(ds => ds.Templates)
            .HasForeignKey(st => st.ScheduleId)
            .WillCascadeOnDelete(true);

        HasOptional(st => st.CreatedByUser)
            .WithMany()
            .HasForeignKey(st => st.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(st => st.UpdatedByUser)
            .WithMany()
            .HasForeignKey(st => st.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(st => st.DeletedByUser)
            .WithMany()
            .HasForeignKey(se => se.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌ها برای بهبود عملکرد
        HasIndex(st => new { st.ScheduleId, st.Type, st.IsActive })
            .HasName("IX_ScheduleTemplate_ScheduleId_Type_IsActive");

        HasIndex(st => new { st.IsDefault, st.IsActive, st.IsDeleted })
            .HasName("IX_ScheduleTemplate_Default_Active_Deleted");

        HasIndex(st => st.IsDeleted)
            .HasName("IX_ScheduleTemplate_IsDeleted");
    }
}