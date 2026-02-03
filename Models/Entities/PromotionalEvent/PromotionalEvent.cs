using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.PromotionalEvent;

/// <summary>
/// مدل ایونت تبلیغاتی (Promotional Event)
/// برای مدیریت تخفیف‌های دوره‌ای و ویژه
/// 
/// ویژگی‌های کلیدی:
/// 1. پشتیبانی از تخفیف درصدی و مبلغ ثابت
/// 2. محدودیت تعداد استفاده (مثلاً 5 از 10 نوبت)
/// 3. محدودیت پزشک (فقط برای پزشکان خاص)
/// 4. محدودیت تاریخ (StartDate/EndDate)
/// 5. سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات
/// </summary>
public class PromotionalEvent : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه ایونت تبلیغاتی
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// عنوان ایونت (مثلاً: "تخفیف ویژه نوروز")
    /// </summary>
    [Required(ErrorMessage = "عنوان ایونت الزامی است.")]
    [MaxLength(200, ErrorMessage = "عنوان ایونت نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string Title { get; set; }

    /// <summary>
    /// توضیحات ایونت
    /// </summary>
    [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// تاریخ شروع ایونت
    /// </summary>
    [Required(ErrorMessage = "تاریخ شروع الزامی است.")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// تاریخ پایان ایونت
    /// </summary>
    [Required(ErrorMessage = "تاریخ پایان الزامی است.")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// نوع تخفیف (درصدی یا مبلغ ثابت)
    /// </summary>
    [Required(ErrorMessage = "نوع تخفیف الزامی است.")]
    public DiscountType DiscountType { get; set; }

    /// <summary>
    /// مقدار تخفیف (درصد یا مبلغ به ریال)
    /// طبق قرارداد مالی: decimal(18,0) برای مبالغ IRR
    /// Note: Precision در PromotionalEventConfig تنظیم می‌شود
    /// </summary>
    [Required(ErrorMessage = "مقدار تخفیف الزامی است.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "مقدار تخفیف باید بیشتر از صفر باشد.")]
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// تعداد کل نوبت‌های قابل استفاده (NULL = نامحدود)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "تعداد کل نوبت‌ها باید بیشتر از صفر باشد.")]
    public int? TotalSlots { get; set; }

    /// <summary>
    /// تعداد نوبت‌های استفاده شده
    /// </summary>
    public int UsedSlots { get; set; } = 0;

    /// <summary>
    /// آیا فقط برای پزشکان خاص است؟
    /// </summary>
    public bool IsDoctorSpecific { get; set; } = false;

    /// <summary>
    /// لیست شناسه‌های پزشکان (JSON Array)
    /// مثال: "[1,2,3]"
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string DoctorIds { get; set; }

    /// <summary>
    /// آیا ایونت فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)

    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن ایونت
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// تاریخ و زمان حذف ایونت
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که ایونت را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }

    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)

    /// <summary>
    /// تاریخ و زمان ایجاد ایونت
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که ایونت را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش ایونت
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که ایونت را ویرایش کرده است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }

    #endregion

    #region روابط

    /// <summary>
    /// لیست نوبت‌های استفاده شده از این ایونت
    /// </summary>
    public virtual ICollection<Appointment.Appointment> Appointments { get; set; }

    #endregion
}

/// <summary>
/// پیکربندی Entity Framework برای PromotionalEvent
/// بهینه‌سازی شده برای Query Performance
/// </summary>
public class PromotionalEventConfig : EntityTypeConfiguration<PromotionalEvent>
{
    public PromotionalEventConfig()
    {
        ToTable("PromotionalEvents");
        HasKey(e => e.EventId);

        // ویژگی‌های اصلی
        Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PromotionalEvent_Title")));

        Property(e => e.Description)
            .IsOptional()
            .HasMaxLength(1000);

        Property(e => e.StartDate)
            .IsRequired()
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PromotionalEvent_StartDate")));

        Property(e => e.EndDate)
            .IsRequired()
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PromotionalEvent_EndDate")));

        Property(e => e.DiscountType)
            .IsRequired();

        Property(e => e.DiscountValue)
            .IsRequired()
            .HasPrecision(18, 0); // ✅ CRITICAL: decimal(18,0) برای مبالغ IRR (طبق قرارداد مالی)

        Property(e => e.TotalSlots)
            .IsOptional();

        Property(e => e.UsedSlots)
            .IsRequired();

        Property(e => e.IsDoctorSpecific)
            .IsRequired();

        Property(e => e.DoctorIds)
            .IsOptional()
            .HasColumnType("nvarchar(max)");

        Property(e => e.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PromotionalEvent_IsActive")));

        // پیاده‌سازی ISoftDelete
        Property(e => e.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PromotionalEvent_IsDeleted")));

        Property(e => e.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PromotionalEvent_DeletedAt")));

        Property(e => e.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PromotionalEvent_DeletedByUserId")));

        // پیاده‌سازی ITrackable
        Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PromotionalEvent_CreatedAt")));

        Property(e => e.CreatedByUserId)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PromotionalEvent_CreatedByUserId")));

        Property(e => e.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PromotionalEvent_UpdatedAt")));

        Property(e => e.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_PromotionalEvent_UpdatedByUserId")));

        // روابط
        HasMany(e => e.Appointments)
            .WithOptional(a => a.PromotionalEvent)
            .HasForeignKey(a => a.PromotionalEventId)
            .WillCascadeOnDelete(false);

        // Composite Index برای Query Performance
        HasIndex(e => new { e.StartDate, e.EndDate, e.IsActive })
            .HasName("IX_PromotionalEvent_StartDate_EndDate_IsActive");

        HasIndex(e => new { e.IsActive, e.IsDeleted })
            .HasName("IX_PromotionalEvent_IsActive_IsDeleted");
    }
}

