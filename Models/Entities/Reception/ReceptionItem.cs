using System;
using ClinicApp.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Models.Entities.Reception;

/// <summary>
/// مدل آیتم‌های پذیرش - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت صحیح آیتم‌های پذیرش با توجه به استانداردهای سیستم‌های درمانی
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
/// 5. محاسبه دقیق سهم بیمار و بیمه برای هر خدمت
/// </summary>
public class ReceptionItem : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه آیتم پذیرش
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int ReceptionItemId { get; set; }

    /// <summary>
    /// شناسه پذیرش مرتبط
    /// </summary>
    [Required(ErrorMessage = "پذیرش الزامی است.")]
    public int ReceptionId { get; set; }

    /// <summary>
    /// شناسه خدمت مرتبط
    /// </summary>
    [Required(ErrorMessage = "خدمت الزامی است.")]
    public int ServiceId { get; set; }

    /// <summary>
    /// تعداد
    /// </summary>
    [Required(ErrorMessage = "تعداد الزامی است.")]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// قیمت هر واحد
    /// </summary>
    [Required(ErrorMessage = "قیمت هر واحد الزامی است.")]
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// مبلغ سهم بیمار
    /// </summary>
    [Required(ErrorMessage = "مبلغ سهم بیمار الزامی است.")]
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal")]
    public decimal PatientShareAmount { get; set; }

    /// <summary>
    /// مبلغ سهم بیمه
    /// </summary>
    [Required(ErrorMessage = "مبلغ سهم بیمه الزامی است.")]
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal")]
    public decimal InsurerShareAmount { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن آیتم پذیرش
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف آیتم پذیرش
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که آیتم پذیرش را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد آیتم پذیرش
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که آیتم پذیرش را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش آیتم پذیرش
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که آیتم پذیرش را ویرایش کرده است
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
    /// ارجاع به پذیرش مرتبط
    /// این ارتباط برای نمایش اطلاعات پذیرش در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual Reception Reception { get; set; }

    /// <summary>
    /// ارجاع به خدمت مرتبط
    /// این ارتباط برای نمایش اطلاعات خدمت در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual Service Service { get; set; }
    #endregion
}
/// <summary>
/// پیکربندی مدل آیتم‌های پذیرش برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class ReceptionItemConfig : EntityTypeConfiguration<ReceptionItem>
{
    public ReceptionItemConfig()
    {
        ToTable("ReceptionItems");
        HasKey(ri => ri.ReceptionItemId);

        // ویژگی‌های اصلی
        Property(ri => ri.Quantity)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_Quantity")));

        Property(ri => ri.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 0)  // ✅ ریال - بدون اعشار
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_UnitPrice")));

        Property(ri => ri.PatientShareAmount)
            .IsRequired()
            .HasPrecision(18, 0)  // ✅ ریال - بدون اعشار
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_PatientShareAmount")));

        Property(ri => ri.InsurerShareAmount)
            .IsRequired()
            .HasPrecision(18, 0)  // ✅ ریال - بدون اعشار
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_InsurerShareAmount")));

        // پیاده‌سازی ISoftDelete
        Property(ri => ri.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_IsDeleted")));

        Property(ri => ri.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(ri => ri.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_CreatedAt")));

        Property(ri => ri.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_CreatedByUserId")));

        Property(ri => ri.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_UpdatedAt")));

        Property(ri => ri.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_UpdatedByUserId")));

        Property(ri => ri.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_DeletedByUserId")));

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        Property(ri => ri.ReceptionId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_ReceptionId")));

        Property(ri => ri.ServiceId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_ServiceId")));

        // روابط
        HasRequired(ri => ri.Reception)
            .WithMany(r => r.ReceptionItems)
            .HasForeignKey(ri => ri.ReceptionId)
            .WillCascadeOnDelete(true);

        HasRequired(ri => ri.Service)
            .WithMany(s => s.ReceptionItems)
            .HasForeignKey(ri => ri.ServiceId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
        HasIndex(ri => new { ri.ReceptionId, ri.ServiceId })
            .HasName("IX_ReceptionItem_ReceptionId_ServiceId");

        HasIndex(ri => new { ri.ServiceId, ri.CreatedAt })
            .HasName("IX_ReceptionItem_ServiceId_CreatedAt");
    }
}