using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Receipt;

/// <summary>
/// مدل چاپ رسیدهای پذیرش در کلینیک شفا
/// این مدل برای ذخیره اطلاعات چاپ رسیدهای پذیرش (فیش‌های پرداخت) استفاده می‌شود
/// 
/// ویژگی‌های کلیدی:
/// 1. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 2. مدیریت صحیح فیلدهای ردیابی (Audit Trail) برای رعایت استانداردهای پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی دقیق
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// 5. پشتیبانی از ذخیره‌سازی محتوای کامل رسید برای مراجعات بعدی
/// </summary>
public class ReceiptPrint : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه چاپ رسید
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int ReceiptPrintId { get; set; }

    /// <summary>
    /// شناسه پذیرش مرتبط با این رسید
    /// این فیلد ارتباط با جدول پذیرش‌ها را برقرار می‌کند
    /// </summary>
    [Required(ErrorMessage = "پذیرش الزامی است.")]
    public int ReceptionId { get; set; }

    /// <summary>
    /// محتوای کامل رسید (حداکثر 1000 کاراکتر کافی برای رسیدهای پزشکی)
    /// </summary>
    [Required(ErrorMessage = "محتوای رسید الزامی است.")]
    [MaxLength(1000, ErrorMessage = "محتوای رسید نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string ReceiptContent { get; set; }

    /// <summary>
    /// هش محتوای رسید برای ایندکس‌گذاری
    /// این فیلد برای جستجوی سریع و یکتایی استفاده می‌شود
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string ReceiptHash { get; set; }
    /// <summary>
    /// تاریخ چاپ رسید
    /// </summary>
    [Required(ErrorMessage = "تاریخ چاپ الزامی است.")]
    public DateTime PrintDate { get; set; } = DateTime.Now;

    /// <summary>
    /// نام کاربری که رسید را چاپ کرده است
    /// این فیلد برای نمایش در UI استفاده می‌شود
    /// </summary>
    [MaxLength(250, ErrorMessage = "نام کاربری چاپ‌کننده نمی‌تواند بیش از 250 کاراکتر باشد.")]
    public string PrintedBy { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن رسید
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف رسید
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که رسید را حذف کرده است
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
    /// تاریخ و زمان ایجاد چاپ رسید
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که چاپ رسید را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }
    /// <summary>
    /// شناسه کاربری که رسید را چاپ کرده است
    /// این فیلد برای ارتباط با کاربر چاپ‌کننده ضروری است
    /// </summary>
    public string PrintedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش چاپ رسید
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که چاپ رسید را ویرایش کرده است
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
    /// ارجاع به پذیرش مرتبط با این رسید
    /// این ارتباط برای نمایش اطلاعات پذیرش در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual Reception.Reception Reception { get; set; }

    /// <summary>
    /// ارجاع به کاربر چاپ‌کننده
    /// این ارتباط برای نمایش اطلاعات کاربر چاپ‌کننده در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual ApplicationUser PrintedByUser { get; set; }
    #endregion
}

/// <summary>
/// پیکربندی مدل چاپ رسیدها برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class ReceiptPrintConfig : EntityTypeConfiguration<ReceiptPrint>
{
    public ReceiptPrintConfig()
    {
        ToTable("ReceiptPrints");
        HasKey(rp => rp.ReceiptPrintId);

        // ویژگی‌های اصلی - اصلاح شده برای رفع خطا
        Property(rp => rp.ReceiptContent)
            .IsRequired()
            .HasMaxLength(1000) // تغییر از IsMaxLength() به MaxLength(1000)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_ReceiptContent")));
        // افزودن ستون Hash برای جستجوهای پیشرفته
        Property(rp => rp.ReceiptHash)
            .IsRequired()
            .HasMaxLength(64)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_ReceiptHash") { IsUnique = true }));

        Property(rp => rp.PrintDate)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_PrintDate")));

        Property(rp => rp.PrintedBy)
            .IsOptional()
            .HasMaxLength(250)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_PrintedBy")));

        // پیاده‌سازی ISoftDelete
        Property(rp => rp.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_IsDeleted")));

        Property(rp => rp.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(rp => rp.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_CreatedAt")));

        Property(rp => rp.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_CreatedByUserId")));

        Property(rp => rp.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_UpdatedAt")));

        Property(rp => rp.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_UpdatedByUserId")));

        Property(rp => rp.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_DeletedByUserId")));

        // روابط
        HasRequired(rp => rp.Reception)
            .WithMany(r => r.ReceiptPrints)
            .HasForeignKey(rp => rp.ReceptionId)
            .WillCascadeOnDelete(false); // حتماً false برای رعایت استانداردهای پزشکی

        HasOptional(rp => rp.PrintedByUser)
            .WithMany()
            .HasForeignKey(rp => rp.PrintedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(rp => rp.CreatedByUser)
            .WithMany()
            .HasForeignKey(rp => rp.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(rp => rp.UpdatedByUser)
            .WithMany()
            .HasForeignKey(rp => rp.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(rp => rp.DeletedByUser)
            .WithMany()
            .HasForeignKey(rp => rp.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        HasIndex(rp => new { rp.ReceptionId, rp.PrintDate })
            .HasName("IX_ReceiptPrint_ReceptionId_PrintDate");

        HasIndex(rp => new { rp.PrintDate, rp.IsDeleted })
            .HasName("IX_ReceiptPrint_PrintDate_IsDeleted");

        HasIndex(rp => new { rp.CreatedAt, rp.IsDeleted })
            .HasName("IX_ReceiptPrint_CreatedAt_IsDeleted");
    }
}