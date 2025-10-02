using System;
using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Payment;

/// <summary>
/// مدل درگاه‌های پرداخت آنلاین - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت متمرکز درگاه‌های پرداخت آنلاین
/// 2. پشتیبانی از درگاه‌های ایرانی (زرین‌پال، پی‌پینگ، آی‌دی‌پی و...)
/// 3. مدیریت کلیدهای API و تنظیمات امنیتی
/// 4. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات مالی
/// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// 6. پشتیبانی از تنظیمات پیشرفته درگاه‌ها
/// </summary>
public class PaymentGateway : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه درگاه پرداخت
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int PaymentGatewayId { get; set; }

    /// <summary>
    /// نام درگاه پرداخت
    /// مثال: "زرین‌پال اصلی", "پی‌پینگ تست"
    /// </summary>
    [Required(ErrorMessage = "نام درگاه الزامی است.")]
    [MaxLength(100, ErrorMessage = "نام درگاه نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string Name { get; set; }

    /// <summary>
    /// نوع درگاه پرداخت
    /// </summary>
    [Required(ErrorMessage = "نوع درگاه الزامی است.")]
    public PaymentGatewayType GatewayType { get; set; }

    /// <summary>
    /// شناسه مرچنت (Merchant ID)
    /// </summary>
    [Required(ErrorMessage = "شناسه مرچنت الزامی است.")]
    [MaxLength(100, ErrorMessage = "شناسه مرچنت نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string MerchantId { get; set; }

    /// <summary>
    /// کلید API درگاه
    /// </summary>
    [Required(ErrorMessage = "کلید API الزامی است.")]
    [MaxLength(500, ErrorMessage = "کلید API نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string ApiKey { get; set; }

    /// <summary>
    /// کلید خصوصی درگاه
    /// </summary>
    [MaxLength(500, ErrorMessage = "کلید خصوصی نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string PrivateKey { get; set; }

    /// <summary>
    /// URL درگاه پرداخت
    /// </summary>
    [Required(ErrorMessage = "URL درگاه الزامی است.")]
    [MaxLength(500, ErrorMessage = "URL درگاه نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string GatewayUrl { get; set; }

    /// <summary>
    /// URL بازگشت (Callback URL)
    /// </summary>
    [Required(ErrorMessage = "URL بازگشت الزامی است.")]
    [MaxLength(500, ErrorMessage = "URL بازگشت نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string CallbackUrl { get; set; }

    /// <summary>
    /// URL بازگشت موفقیت
    /// </summary>
    [MaxLength(500, ErrorMessage = "URL بازگشت موفقیت نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string SuccessUrl { get; set; }

    /// <summary>
    /// کلید مخفی API (برای سازگاری با ViewModels)
    /// </summary>
    public string ApiSecret { get; set; }

    /// <summary>
    /// URL Webhook (برای سازگاری با ViewModels)
    /// </summary>
    public string WebhookUrl { get; set; }

    /// <summary>
    /// آیا درگاه پیش‌فرض است؟ (برای سازگاری با ViewModels)
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// نام کاربر ایجادکننده (برای سازگاری با ViewModels)
    /// </summary>
    public string CreatedByUserName { get; set; } = string.Empty;

    /// <summary>
    /// نام کاربر به‌روزرسانی‌کننده (برای سازگاری با ViewModels)
    /// </summary>
    public string UpdatedByUserName { get; set; } = string.Empty;

    /// <summary>
    /// تعداد کل تراکنش‌ها (برای سازگاری با ViewModels)
    /// </summary>
    public int TotalTransactions { get; set; } = 0;

    /// <summary>
    /// مجموع مبلغ تراکنش‌ها (برای سازگاری با ViewModels)
    /// </summary>
    public decimal TotalAmount { get; set; } = 0;

    /// <summary>
    /// نرخ موفقیت (برای سازگاری با ViewModels)
    /// </summary>
    public decimal SuccessRate { get; set; } = 0;

    /// <summary>
    /// میانگین زمان پاسخ (برای سازگاری با ViewModels)
    /// </summary>
    public decimal AverageResponseTime { get; set; } = 0;

    /// <summary>
    /// تاریخ آخرین تراکنش (برای سازگاری با ViewModels)
    /// </summary>
    public DateTime? LastTransactionDate { get; set; }

    /// <summary>
    /// URL بازگشت خطا
    /// </summary>
    [MaxLength(500, ErrorMessage = "URL بازگشت خطا نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string ErrorUrl { get; set; }

    /// <summary>
    /// آیا درگاه فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// آیا درگاه در حالت تست است؟
    /// </summary>
    public bool IsTestMode { get; set; } = true;

    /// <summary>
    /// حداقل مبلغ تراکنش (ریال)
    /// </summary>
    [Column(TypeName = "decimal")]
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// حداکثر مبلغ تراکنش (ریال)
    /// </summary>
    [Column(TypeName = "decimal")]
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// درصد کارمزد درگاه
    /// </summary>
    [Column(TypeName = "decimal")]
    public decimal? FeePercentage { get; set; }

    /// <summary>
    /// مبلغ ثابت کارمزد (ریال)
    /// </summary>
    [Column(TypeName = "decimal")]
    public decimal? FixedFee { get; set; }

    /// <summary>
    /// توضیحات درگاه
    /// </summary>
    [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// تنظیمات اضافی درگاه (JSON)
    /// </summary>
    [MaxLength(2000, ErrorMessage = "تنظیمات اضافی نمی‌تواند بیش از 2000 کاراکتر باشد.")]
    public string AdditionalSettings { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن درگاه
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مالی مجاز نیست
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف درگاه
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که درگاه را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد درگاه
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که درگاه را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش درگاه
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که درگاه را ویرایش کرده است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    #region روابط
    /// <summary>
    /// لیست پرداخت‌های آنلاین انجام شده با این درگاه
    /// </summary>
    public virtual ICollection<OnlinePayment> OnlinePayments { get; set; } = new HashSet<OnlinePayment>();
    #endregion
}
/// <summary>
/// پیکربندی مدل درگاه‌های پرداخت آنلاین برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class PaymentGatewayConfig : EntityTypeConfiguration<PaymentGateway>
{
    public PaymentGatewayConfig()
    {
        ToTable("PaymentGateways");
        HasKey(pg => pg.PaymentGatewayId);

        // ویژگی‌های اصلی
        Property(pg => pg.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentGateway_Name")));

        Property(pg => pg.GatewayType)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentGateway_GatewayType")));

        Property(pg => pg.MerchantId)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentGateway_MerchantId")));

        Property(pg => pg.ApiKey)
            .IsRequired()
            .HasMaxLength(500);

        Property(pg => pg.PrivateKey)
            .IsOptional()
            .HasMaxLength(500);

        Property(pg => pg.GatewayUrl)
            .IsRequired()
            .HasMaxLength(500);

        Property(pg => pg.CallbackUrl)
            .IsRequired()
            .HasMaxLength(500);

        Property(pg => pg.SuccessUrl)
            .IsOptional()
            .HasMaxLength(500);

        Property(pg => pg.ErrorUrl)
            .IsOptional()
            .HasMaxLength(500);

        Property(pg => pg.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentGateway_IsActive")));

        Property(pg => pg.IsTestMode)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentGateway_IsTestMode")));

        Property(pg => pg.MinAmount)
            .IsOptional()
            .HasPrecision(18, 0);

        Property(pg => pg.MaxAmount)
            .IsOptional()
            .HasPrecision(18, 0);

        Property(pg => pg.FeePercentage)
            .IsOptional()
            .HasPrecision(5, 2);

        Property(pg => pg.FixedFee)
            .IsOptional()
            .HasPrecision(18, 0);

        Property(pg => pg.Description)
            .IsOptional()
            .HasMaxLength(1000);

        Property(pg => pg.AdditionalSettings)
            .IsOptional()
            .HasMaxLength(2000);

        // پیاده‌سازی ISoftDelete
        Property(pg => pg.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentGateway_IsDeleted")));

        Property(pg => pg.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentGateway_DeletedAt")));

        Property(pg => pg.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentGateway_DeletedByUserId")));

        // پیاده‌سازی ITrackable
        Property(pg => pg.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentGateway_CreatedAt")));

        Property(pg => pg.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentGateway_CreatedByUserId")));

        Property(pg => pg.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentGateway_UpdatedAt")));

        Property(pg => pg.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentGateway_UpdatedByUserId")));

        // روابط
        HasOptional(pg => pg.DeletedByUser)
            .WithMany()
            .HasForeignKey(pg => pg.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(pg => pg.CreatedByUser)
            .WithMany()
            .HasForeignKey(pg => pg.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(pg => pg.UpdatedByUser)
            .WithMany()
            .HasForeignKey(pg => pg.UpdatedByUserId)
            .WillCascadeOnDelete(false);
    }
}