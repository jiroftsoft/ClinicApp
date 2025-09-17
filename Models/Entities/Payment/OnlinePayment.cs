using System;
using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Payment;

/// <summary>
/// مدل پرداخت‌های آنلاین - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت کامل پرداخت‌های آنلاین (نوبت‌ها، پذیرش‌ها، خدمات)
/// 2. پشتیبانی از درگاه‌های مختلف ایرانی
/// 3. مدیریت Callback و Webhook
/// 4. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات مالی
/// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// 6. پشتیبانی از انواع مختلف تراکنش‌های آنلاین
/// </summary>
public class OnlinePayment : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه پرداخت آنلاین
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int OnlinePaymentId { get; set; }

    /// <summary>
    /// شناسه (برای سازگاری با ViewModels)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه درگاه پرداخت
    /// </summary>
    [Required(ErrorMessage = "درگاه پرداخت الزامی است.")]
    public int PaymentGatewayId { get; set; }

    /// <summary>
    /// شناسه پذیرش (در صورت پرداخت پذیرش)
    /// </summary>
    public int? ReceptionId { get; set; }

    /// <summary>
    /// شناسه نوبت (در صورت پرداخت نوبت)
    /// </summary>
    public int? AppointmentId { get; set; }

    /// <summary>
    /// شناسه بیمار
    /// </summary>
    [Required(ErrorMessage = "بیمار الزامی است.")]
    public int PatientId { get; set; }

    /// <summary>
    /// نوع پرداخت آنلاین
    /// </summary>
    [Required(ErrorMessage = "نوع پرداخت الزامی است.")]
    public OnlinePaymentType PaymentType { get; set; }

    /// <summary>
    /// وضعیت پرداخت آنلاین
    /// </summary>
    [Required(ErrorMessage = "وضعیت پرداخت الزامی است.")]
    public OnlinePaymentStatus Status { get; set; }

    /// <summary>
    /// مبلغ پرداخت (ریال)
    /// </summary>
    [Required(ErrorMessage = "مبلغ پرداخت الزامی است.")]
    [Column(TypeName = "decimal")]
    public decimal Amount { get; set; }

    /// <summary>
    /// مبلغ کارمزد درگاه (ریال)
    /// </summary>
    [Column(TypeName = "decimal")]
    public decimal? GatewayFee { get; set; }

    /// <summary>
    /// مبلغ خالص دریافتی (ریال)
    /// </summary>
    [Column(TypeName = "decimal")]
    public decimal? NetAmount { get; set; }

    /// <summary>
    /// شماره تراکنش درگاه
    /// </summary>
    [MaxLength(100, ErrorMessage = "شماره تراکنش نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string GatewayTransactionId { get; set; }

    /// <summary>
    /// شماره مرجع درگاه (RRN)
    /// </summary>
    [MaxLength(100, ErrorMessage = "شماره مرجع نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string GatewayReferenceCode { get; set; }

    /// <summary>
    /// شماره تراکنش داخلی
    /// </summary>
    [MaxLength(100, ErrorMessage = "شماره تراکنش داخلی نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string InternalTransactionId { get; set; }

    /// <summary>
    /// توکن پرداخت
    /// </summary>
    [MaxLength(500, ErrorMessage = "توکن پرداخت نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string PaymentToken { get; set; }

    /// <summary>
    /// URL پرداخت
    /// </summary>
    [MaxLength(1000, ErrorMessage = "URL پرداخت نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string PaymentUrl { get; set; }

    /// <summary>
    /// تاریخ شروع پرداخت
    /// </summary>
    public DateTime? PaymentStartDate { get; set; }

    /// <summary>
    /// تاریخ تکمیل پرداخت
    /// </summary>
    public DateTime? PaymentCompletionDate { get; set; }

    /// <summary>
    /// تاریخ انقضای پرداخت
    /// </summary>
    public DateTime? PaymentExpiryDate { get; set; }

    /// <summary>
    /// تاریخ انقضا (برای سازگاری)
    /// </summary>
    public DateTime? ExpiresAt => PaymentExpiryDate;

    /// <summary>
    /// IP آدرس کاربر
    /// </summary>
    [MaxLength(50, ErrorMessage = "IP آدرس نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string UserIpAddress { get; set; }

    /// <summary>
    /// User Agent مرورگر
    /// </summary>
    [MaxLength(500, ErrorMessage = "User Agent نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string UserAgent { get; set; }

    /// <summary>
    /// کد خطا (در صورت ناموفق بودن)
    /// </summary>
    [MaxLength(50, ErrorMessage = "کد خطا نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string ErrorCode { get; set; }

    /// <summary>
    /// پیام خطا
    /// </summary>
    [MaxLength(1000, ErrorMessage = "پیام خطا نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string ErrorMessage { get; set; }

    /// <summary>
    /// کد مرجع (برای سازگاری)
    /// </summary>
    [MaxLength(100, ErrorMessage = "کد مرجع نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string ReferenceCode { get; set; }

    /// <summary>
    /// توضیحات پرداخت
    /// </summary>
    [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// اطلاعات اضافی (JSON)
    /// </summary>
    [MaxLength(2000, ErrorMessage = "اطلاعات اضافی نمی‌تواند بیش از 2000 کاراکتر باشد.")]
    public string AdditionalData { get; set; }

    /// <summary>
    /// آیا پرداخت برگشت خورده است؟
    /// </summary>
    public bool IsRefunded { get; set; } = false;

    /// <summary>
    /// تاریخ برگشت
    /// </summary>
    public DateTime? RefundDate { get; set; }

    /// <summary>
    /// تاریخ برگشت (برای سازگاری)
    /// </summary>
    public DateTime? RefundedAt => RefundDate;

    /// <summary>
    /// مبلغ برگشت (ریال)
    /// </summary>
    [Column(TypeName = "decimal")]
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// دلیل برگشت
    /// </summary>
    [MaxLength(500, ErrorMessage = "دلیل برگشت نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string RefundReason { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن پرداخت
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مالی مجاز نیست
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف پرداخت
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که پرداخت را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد پرداخت
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که پرداخت را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش پرداخت
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که پرداخت را ویرایش کرده است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    #region Properties برای سازگاری با ViewModels
    /// <summary>
    /// تاریخ تکمیل پرداخت (برای سازگاری با ViewModels)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// شماره پذیرش (برای سازگاری با ViewModels)
    /// </summary>
    public string ReceptionNumber { get; set; } = string.Empty;

    /// <summary>
    /// نام بیمار (برای سازگاری با ViewModels)
    /// </summary>
    public string PatientName { get; set; } = string.Empty;

    /// <summary>
    /// نام پزشک (برای سازگاری با ViewModels)
    /// </summary>
    public string DoctorName { get; set; } = string.Empty;

    /// <summary>
    /// نام درگاه پرداخت (برای سازگاری با ViewModels)
    /// </summary>
    public string PaymentGatewayName { get; set; } = string.Empty;

    /// <summary>
    /// نام کاربر ایجادکننده (برای سازگاری با ViewModels)
    /// </summary>
    public string CreatedByUserName { get; set; } = string.Empty;
    #endregion

    #region روابط
    /// <summary>
    /// ارجاع به درگاه پرداخت
    /// </summary>
    public virtual PaymentGateway PaymentGateway { get; set; }

    /// <summary>
    /// ارجاع به پذیرش (در صورت پرداخت پذیرش)
    /// </summary>
    public virtual Reception.Reception Reception { get; set; }

    /// <summary>
    /// ارجاع به نوبت (در صورت پرداخت نوبت)
    /// </summary>
    public virtual Appointment.Appointment Appointment { get; set; }

    /// <summary>
    /// ارجاع به بیمار
    /// </summary>
    public virtual Patient.Patient Patient { get; set; }
    #endregion
}

/// <summary>
/// پیکربندی مدل پرداخت‌های آنلاین برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class OnlinePaymentConfig : EntityTypeConfiguration<OnlinePayment>
{
    public OnlinePaymentConfig()
    {
        ToTable("OnlinePayments");
        HasKey(op => op.OnlinePaymentId);

        // ویژگی‌های اصلی
        Property(op => op.PaymentGatewayId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_PaymentGatewayId")));

        Property(op => op.ReceptionId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_ReceptionId")));

        Property(op => op.AppointmentId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_AppointmentId")));

        Property(op => op.PatientId)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_PatientId")));

        Property(op => op.PaymentType)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_PaymentType")));

        Property(op => op.Status)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_Status")));

        Property(op => op.Amount)
            .IsRequired()
            .HasPrecision(18, 4)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_Amount")));

        Property(op => op.GatewayFee)
            .IsOptional()
            .HasPrecision(18, 4);

        Property(op => op.NetAmount)
            .IsOptional()
            .HasPrecision(18, 4);

        Property(op => op.GatewayTransactionId)
            .IsOptional()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_GatewayTransactionId")));

        Property(op => op.GatewayReferenceCode)
            .IsOptional()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_GatewayReferenceCode")));

        Property(op => op.InternalTransactionId)
            .IsOptional()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_InternalTransactionId")));

        Property(op => op.PaymentToken)
            .IsOptional()
            .HasMaxLength(500)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_PaymentToken")));

        Property(op => op.PaymentUrl)
            .IsOptional()
            .HasMaxLength(1000);

        Property(op => op.PaymentStartDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_PaymentStartDate")));

        Property(op => op.PaymentCompletionDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_PaymentCompletionDate")));

        Property(op => op.PaymentExpiryDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_PaymentExpiryDate")));

        Property(op => op.UserIpAddress)
            .IsOptional()
            .HasMaxLength(50);

        Property(op => op.UserAgent)
            .IsOptional()
            .HasMaxLength(500);

        Property(op => op.ErrorCode)
            .IsOptional()
            .HasMaxLength(50);

        Property(op => op.ErrorMessage)
            .IsOptional()
            .HasMaxLength(1000);

        Property(op => op.Description)
            .IsOptional()
            .HasMaxLength(1000);

        Property(op => op.AdditionalData)
            .IsOptional()
            .HasMaxLength(2000);

        Property(op => op.IsRefunded)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_IsRefunded")));

        Property(op => op.RefundDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_RefundDate")));

        Property(op => op.RefundAmount)
            .IsOptional()
            .HasPrecision(18, 2);

        Property(op => op.RefundReason)
            .IsOptional()
            .HasMaxLength(500);

        // پیاده‌سازی ISoftDelete
        Property(op => op.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_IsDeleted")));

        Property(op => op.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_DeletedAt")));

        Property(op => op.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_DeletedByUserId")));

        // پیاده‌سازی ITrackable
        Property(op => op.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_CreatedAt")));

        Property(op => op.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_CreatedByUserId")));

        Property(op => op.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_UpdatedAt")));

        Property(op => op.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OnlinePayment_UpdatedByUserId")));

        // روابط
        HasRequired(op => op.PaymentGateway)
            .WithMany(pg => pg.OnlinePayments)
            .HasForeignKey(op => op.PaymentGatewayId)
            .WillCascadeOnDelete(false);

        HasOptional(op => op.Reception)
            .WithMany()
            .HasForeignKey(op => op.ReceptionId)
            .WillCascadeOnDelete(false);

        HasOptional(op => op.Appointment)
            .WithMany()
            .HasForeignKey(op => op.AppointmentId)
            .WillCascadeOnDelete(false);

        HasRequired(op => op.Patient)
            .WithMany()
            .HasForeignKey(op => op.PatientId)
            .WillCascadeOnDelete(false);

        HasOptional(op => op.DeletedByUser)
            .WithMany()
            .HasForeignKey(op => op.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(op => op.CreatedByUser)
            .WithMany()
            .HasForeignKey(op => op.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(op => op.UpdatedByUser)
            .WithMany()
            .HasForeignKey(op => op.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // Index های ترکیبی
        HasIndex(op => new { op.PaymentGatewayId, op.Status })
            .HasName("IX_OnlinePayment_GatewayId_Status");

        HasIndex(op => new { op.PatientId, op.PaymentType })
            .HasName("IX_OnlinePayment_PatientId_PaymentType");

        HasIndex(op => new { op.Status, op.CreatedAt })
            .HasName("IX_OnlinePayment_Status_CreatedAt");

        HasIndex(op => new { op.PaymentType, op.Status, op.CreatedAt })
            .HasName("IX_OnlinePayment_Type_Status_CreatedAt");
    }
}