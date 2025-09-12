using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Payment;

/// <summary>
/// مدل تراکنش‌های مالی - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. پشتیبانی از انواع روش‌های پرداخت (POS، نقدی، بدهی)
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات مالی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// 5. پشتیبانی از انواع پروتکل‌های پوز بانکی
/// </summary>
public class PaymentTransaction : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه تراکنش مالی
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int PaymentTransactionId { get; set; }

    /// <summary>
    /// شناسه تراکنش (برای سازگاری با ViewModels)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه پذیرش مربوطه
    /// </summary>
    [Required(ErrorMessage = "پذیرش الزامی است.")]
    public int ReceptionId { get; set; }

    /// <summary>
    /// شناسه دستگاه پوز (در صورت استفاده از پوز)
    /// </summary>
    public int? PosTerminalId { get; set; }

    /// <summary>
    /// شناسه درگاه پرداخت آنلاین (در صورت استفاده از پرداخت آنلاین)
    /// </summary>
    public int? PaymentGatewayId { get; set; }

    /// <summary>
    /// شناسه پرداخت آنلاین (در صورت استفاده از پرداخت آنلاین)
    /// </summary>
    public int? OnlinePaymentId { get; set; }

    /// <summary>
    /// شناسه شیفت صندوق
    /// </summary>
    [Required(ErrorMessage = "شیفت صندوق الزامی است.")]
    public int CashSessionId { get; set; }

    /// <summary>
    /// مبلغ تراکنش
    /// </summary>
    [Required(ErrorMessage = "مبلغ تراکنش الزامی است.")]
    [DataType(DataType.Currency, ErrorMessage = "فرمت مبلغ نامعتبر است.")]
    [Column(TypeName = "decimal")]
    public decimal Amount { get; set; }

    /// <summary>
    /// روش پرداخت (POS / Cash / Debt)
    /// </summary>
    [Required(ErrorMessage = "روش پرداخت الزامی است.")]
    public PaymentMethod Method { get; set; }

    /// <summary>
    /// وضعیت تراکنش (Pending / Success / Failed / Canceled)
    /// </summary>
    [Required(ErrorMessage = "وضعیت تراکنش الزامی است.")]
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// شماره تراکنش بانکی
    /// </summary>
    [MaxLength(100, ErrorMessage = "شماره تراکنش نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string TransactionId { get; set; }

    /// <summary>
    /// شماره مرجع (RRN)
    /// </summary>
    [MaxLength(100, ErrorMessage = "شماره مرجع نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string ReferenceCode { get; set; }

    /// <summary>
    /// شماره قبض داخلی
    /// </summary>
    [MaxLength(50, ErrorMessage = "شماره قبض نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string ReceiptNo { get; set; }

    /// <summary>
    /// توضیحات تراکنش
    /// </summary>
    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// نام بیمار (برای سازگاری با ViewModels)
    /// </summary>
    [MaxLength(200)]
    public string? PatientName { get; set; }

    /// <summary>
    /// نام پزشک (برای سازگاری با ViewModels)
    /// </summary>
    [MaxLength(200)]
    public string? DoctorName { get; set; }

    /// <summary>
    /// شماره پذیرش (برای سازگاری با ViewModels)
    /// </summary>
    [MaxLength(50)]
    public string? ReceptionNumber { get; set; }

    /// <summary>
    /// نام کاربر ایجادکننده (برای سازگاری با ViewModels)
    /// </summary>
    [MaxLength(200)]
    public string? CreatedByUserName { get; set; }

    /// <summary>
    /// روش پرداخت (برای سازگاری با ViewModels)
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن تراکنش
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مالی مجاز نیست
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف تراکنش
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که تراکنش را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد تراکنش
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که تراکنش را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش تراکنش
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که تراکنش را ویرایش کرده است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    #region روابط
    /// <summary>
    /// ارجاع به پذیرش مربوطه
    /// </summary>
    public virtual Reception.Reception Reception { get; set; }

    /// <summary>
    /// ارجاع به دستگاه پوز (در صورت استفاده از پوز)
    /// </summary>
    public virtual PosTerminal PosTerminal { get; set; }

    /// <summary>
    /// ارجاع به درگاه پرداخت آنلاین (در صورت استفاده از پرداخت آنلاین)
    /// </summary>
    public virtual PaymentGateway PaymentGateway { get; set; }

    /// <summary>
    /// ارجاع به پرداخت آنلاین (در صورت استفاده از پرداخت آنلاین)
    /// </summary>
    public virtual OnlinePayment OnlinePayment { get; set; }

    /// <summary>
    /// ارجاع به شیفت صندوق
    /// </summary>
    public virtual CashSession CashSession { get; set; }
    #endregion
}
/// <summary>
/// پیکربندی مدل تراکنش‌های مالی برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class PaymentTransactionConfig : EntityTypeConfiguration<PaymentTransaction>
{
    public PaymentTransactionConfig()
    {
        ToTable("PaymentTransactions");
        HasKey(t => t.PaymentTransactionId);

        // ویژگی‌های اصلی
        Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_Amount")));

        Property(t => t.Method)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_Method")));

        Property(t => t.Status)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_Status")));

        Property(t => t.TransactionId)
            .IsOptional()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_TransactionId")));

        Property(t => t.ReferenceCode)
            .IsOptional()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_ReferenceCode")));

        Property(t => t.ReceiptNo)
            .IsOptional()
            .HasMaxLength(50)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_ReceiptNo")));

        Property(t => t.Description)
            .IsOptional()
            .HasMaxLength(500);

        // پیاده‌سازی ISoftDelete
        Property(t => t.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_IsDeleted")));

        Property(t => t.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_CreatedAt")));

        Property(t => t.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_CreatedByUserId")));

        Property(t => t.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_UpdatedAt")));

        Property(t => t.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_UpdatedByUserId")));

        Property(t => t.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_DeletedByUserId")));

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        Property(t => t.ReceptionId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_ReceptionId")));

        Property(t => t.PosTerminalId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_PosTerminalId")));

        Property(t => t.PaymentGatewayId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_PaymentGatewayId")));

        Property(t => t.OnlinePaymentId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_OnlinePaymentId")));

        Property(t => t.CashSessionId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_CashSessionId")));

        // روابط
        HasRequired(t => t.Reception)
            .WithMany(r => r.Transactions)
            .HasForeignKey(t => t.ReceptionId)
            .WillCascadeOnDelete(false);

        HasOptional(t => t.PosTerminal)
            .WithMany(p => p.Transactions)
            .HasForeignKey(t => t.PosTerminalId)
            .WillCascadeOnDelete(false);

        HasOptional(t => t.PaymentGateway)
            .WithMany()
            .HasForeignKey(t => t.PaymentGatewayId)
            .WillCascadeOnDelete(false);

        HasOptional(t => t.OnlinePayment)
            .WithMany()
            .HasForeignKey(t => t.OnlinePaymentId)
            .WillCascadeOnDelete(false);

        HasRequired(t => t.CashSession)
            .WithMany(cs => cs.Transactions)
            .HasForeignKey(t => t.CashSessionId)
            .WillCascadeOnDelete(false);

        HasOptional(t => t.DeletedByUser)
            .WithMany()
            .HasForeignKey(t => t.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(t => t.UpdatedByUser)
            .WithMany()
            .HasForeignKey(t => t.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
        HasIndex(t => new { t.CashSessionId, t.Status, t.CreatedAt })
            .HasName("IX_PaymentTransaction_CashSessionId_Status_CreatedAt");

        HasIndex(t => new { t.ReceptionId, t.Status })
            .HasName("IX_PaymentTransaction_ReceptionId_Status");
    }
}