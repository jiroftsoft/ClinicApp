using System;
using ClinicApp.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.Entities.Clinic;

/// <summary>
/// مدل جزء خدمت - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. هر خدمت از دو جزء فنی و حرفه‌ای تشکیل می‌شود
/// 2. هر جزء دارای کای (ضریب) مخصوص خود است
/// 3. محاسبه مبلغ نهایی: (کای فنی × کای حرفه‌ای) = مبلغ خدمت
/// 4. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class ServiceComponent : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه جزء خدمت
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int ServiceComponentId { get; set; }

    /// <summary>
    /// شناسه خدمت مرتبط
    /// این فیلد ارتباط با جدول خدمات را برقرار می‌کند
    /// </summary>
    [Required(ErrorMessage = "خدمت الزامی است.")]
    public int ServiceId { get; set; }

    /// <summary>
    /// نوع جزء خدمت
    /// Technical = فنی، Professional = حرفه‌ای
    /// </summary>
    [Required(ErrorMessage = "نوع جزء الزامی است.")]
    public ServiceComponentType ComponentType { get; set; }

    /// <summary>
    /// کای (ضریب) این جزء
    /// برای محاسبه مبلغ نهایی خدمت استفاده می‌شود
    /// دقت بالا برای محاسبات دقیق بیمه‌ای
    /// </summary>
    [Required(ErrorMessage = "کای الزامی است.")]
    [DataType(DataType.Currency, ErrorMessage = "فرمت کای نامعتبر است.")]
    [Column(TypeName = "decimal")]
    [Range(0.01, 999999.99, ErrorMessage = "کای باید بین 0.01 تا 999999.99 باشد.")]
    public decimal Coefficient { get; set; }

    /// <summary>
    /// توضیحات این جزء
    /// </summary>
    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// آیا این جزء فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن جزء خدمت
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف جزء خدمت
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که جزء خدمت را حذف کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر حذف کننده ضروری است
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد جزء خدمت
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که جزء خدمت را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین بروزرسانی جزء خدمت
    /// این اطلاعات برای ردیابی تغییرات در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که آخرین بروزرسانی را انجام داده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ویرایش کننده ضروری است
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    #region روابط
    /// <summary>
    /// ارجاع به خدمت مرتبط
    /// این ارتباط برای نمایش اطلاعات خدمت در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual Service Service { get; set; }
    #endregion
}


#region ServiceComponent

/// <summary>
/// پیکربندی مدل جزء خدمت برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class ServiceComponentConfig : EntityTypeConfiguration<ServiceComponent>
{
    public ServiceComponentConfig()
    {
        ToTable("ServiceComponents");
        HasKey(sc => sc.ServiceComponentId);

        // ویژگی‌های اصلی
        Property(sc => sc.ServiceId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceComponent_ServiceId")));

        Property(sc => sc.ComponentType)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceComponent_ComponentType")));

        Property(sc => sc.Coefficient)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceComponent_Coefficient")));

        Property(sc => sc.Description)
            .IsOptional()
            .HasMaxLength(500);

        Property(sc => sc.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceComponent_IsActive")));

        // پیاده‌سازی ISoftDelete
        Property(sc => sc.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceComponent_IsDeleted")));

        Property(sc => sc.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceComponent_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(sc => sc.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceComponent_CreatedAt")));

        Property(sc => sc.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceComponent_CreatedByUserId")));

        Property(sc => sc.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceComponent_UpdatedAt")));

        Property(sc => sc.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceComponent_UpdatedByUserId")));

        Property(sc => sc.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceComponent_DeletedByUserId")));

        // روابط
        HasRequired(sc => sc.Service)
            .WithMany(s => s.ServiceComponents)
            .HasForeignKey(sc => sc.ServiceId)
            .WillCascadeOnDelete(false);

        HasOptional(sc => sc.DeletedByUser)
            .WithMany()
            .HasForeignKey(sc => sc.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(sc => sc.CreatedByUser)
            .WithMany()
            .HasForeignKey(sc => sc.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(sc => sc.UpdatedByUser)
            .WithMany()
            .HasForeignKey(sc => sc.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد
        HasIndex(sc => new { sc.ServiceId, sc.ComponentType, sc.IsDeleted })
            .IsUnique()
            .HasName("IX_ServiceComponent_ServiceId_ComponentType_IsDeleted");

        HasIndex(sc => new { sc.ServiceId, sc.IsActive, sc.IsDeleted })
            .HasName("IX_ServiceComponent_ServiceId_IsActive_IsDeleted");
    }
}

#endregion
