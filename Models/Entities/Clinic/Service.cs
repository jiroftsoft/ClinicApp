using System;
using ClinicApp.Models.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Reception;

namespace ClinicApp.Models.Entities.Clinic;

/// <summary>
/// مدل خدمات پزشکی - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. پشتیبانی از ساختار سازمانی پزشکی (کلینیک -> دپارتمان -> دسته‌بندی خدمات -> خدمات)
/// 2. مدیریت صحیح فیلدهای ردیابی (Audit Trail) برای رعایت استانداردهای پزشکی
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 4. ارتباط با دسته‌بندی‌های خدمات برای سازماندهی بهتر خدمات
/// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class Service : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه خدمات پزشکی
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// عنوان خدمات پزشکی
    /// مثال: "معاینه اولیه دندانپزشکی"، "تزریق واکسن فصلی"
    /// </summary>
    [Required(ErrorMessage = "عنوان خدمات الزامی است.")]
    [MaxLength(250, ErrorMessage = "عنوان خدمات نمی‌تواند بیش از 250 کاراکتر باشد.")]
    public string Title { get; set; }

    /// <summary>
    /// کد خدمات پزشکی
    /// این کد باید منحصر به فرد باشد و برای شناسایی سریع خدمات استفاده می‌شود
    /// مثال: "DNT-001"، "EYE-105"
    /// </summary>
    [Required(ErrorMessage = "کد خدمات الزامی است.")]
    [MaxLength(50, ErrorMessage = "کد خدمات نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string ServiceCode { get; set; }

    /// <summary>
    /// قیمت پایه خدمات پزشکی
    /// </summary>
    [Required(ErrorMessage = "قیمت الزامی است.")]
    [DataType(DataType.Currency, ErrorMessage = "فرمت قیمت نامعتبر است.")]
    [Column(TypeName = "decimal")]
    public decimal Price { get; set; }

    /// <summary>
    /// توضیحات خدمات پزشکی
    /// </summary>
    [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// شناسه دسته‌بندی خدمات مرتبط با این خدمات
    /// این فیلد ارتباط با جدول دسته‌بندی‌ها را برقرار می‌کند
    /// </summary>
    [Required(ErrorMessage = "دسته‌بندی خدمات الزامی است.")]
    public int ServiceCategoryId { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن خدمات
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف خدمات
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که خدمات را حذف کرده است
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
    /// تاریخ و زمان ایجاد خدمات
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که خدمات را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین بروزرسانی خدمات
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
    /// ارجاع به دسته‌بندی خدمات
    /// این ارتباط برای نمایش اطلاعات دسته‌بندی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual ServiceCategory ServiceCategory { get; set; }

    /// <summary>
    /// لیست آیتم‌های پذیرش مرتبط با این خدمات
    /// این لیست برای نمایش تمام آیتم‌های پذیرش مرتبط با این خدمت استفاده می‌شود
    /// </summary>
    public virtual ICollection<ReceptionItem> ReceptionItems { get; set; } = new HashSet<ReceptionItem>();

    /// <summary>
    /// لیست تعرفه‌های بیمه‌ای مرتبط با این خدمات
    /// این لیست برای نمایش تمام تعرفه‌های بیمه‌ای موجود برای این خدمت استفاده می‌شود
    /// </summary>
    public virtual ICollection<InsuranceTariff> Tariffs { get; set; } = new HashSet<InsuranceTariff>();
    public virtual ICollection<InsuranceCalculation> InsuranceCalculations { get; set; } = new HashSet<InsuranceCalculation>();

    public bool IsActive { get; set; }
    public string Notes { get; set; }

    #endregion
}
#region Service

/// <summary>
/// پیکربندی مدل خدمات پزشکی برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class ServiceConfig : EntityTypeConfiguration<Service>
{
    public ServiceConfig()
    {
        ToTable("Services");
        HasKey(s => s.ServiceId);

        // ویژگی‌های اصلی
        Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(250)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Service_Title")));

        Property(s => s.ServiceCode)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Service_ServiceCode") { IsUnique = true }));

        Property(s => s.Price)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Service_Price")));

        Property(s => s.Description)
            .IsOptional()
            .HasMaxLength(1000);

        // پیاده‌سازی ISoftDelete
        Property(s => s.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Service_IsDeleted")));

        Property(s => s.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Service_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(s => s.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Service_CreatedAt")));

        Property(s => s.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Service_CreatedByUserId")));

        Property(s => s.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Service_UpdatedAt")));

        Property(s => s.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Service_UpdatedByUserId")));

        Property(s => s.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Service_DeletedByUserId")));

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        Property(s => s.ServiceCategoryId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Service_ServiceCategoryId")));

        // روابط
        HasRequired(s => s.ServiceCategory)
            .WithMany(d => d.Services)
            .HasForeignKey(s => s.ServiceCategoryId)
            .WillCascadeOnDelete(false);

        HasMany(s => s.ReceptionItems)
            .WithRequired(ri => ri.Service)
            .HasForeignKey(ri => ri.ServiceId)
            .WillCascadeOnDelete(false);

        HasMany(s => s.Tariffs)
            .WithRequired(t => t.Service)
            .HasForeignKey(t => t.ServiceId)
            .WillCascadeOnDelete(false);

        HasOptional(s => s.DeletedByUser)
            .WithMany()
            .HasForeignKey(s => s.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(s => s.CreatedByUser)
            .WithMany()
            .HasForeignKey(s => s.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(s => s.UpdatedByUser)
            .WithMany()
            .HasForeignKey(s => s.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
        HasIndex(s => new { s.ServiceCategoryId, s.IsDeleted })
            .HasName("IX_Service_ServiceCategoryId_IsDeleted");
    }
}

#endregion