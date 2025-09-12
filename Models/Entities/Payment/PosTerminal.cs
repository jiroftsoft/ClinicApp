using System;
using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace ClinicApp.Models.Entities.Payment;

/// <summary>
/// مدل دستگاه‌های پوز بانکی - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. پشتیبانی از انواع پروتکل‌های پوز (سامان کیش، آسان پرداخت، به‌پرداخت و...)
/// 2. مدیریت اتصال به پوزها از طریق IP و MacAddress
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات
/// 4. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی
/// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class PosTerminal : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه دستگاه پوز
    /// </summary>
    public int PosTerminalId { get; set; }

    /// <summary>
    /// شناسه ترمینال (برای سازگاری با ViewModels)
    /// </summary>
    public int Id => PosTerminalId;

    /// <summary>
    /// عنوان دستگاه
    /// مثال: "پوز بانک ملی"
    /// </summary>
    [Required(ErrorMessage = "عنوان دستگاه الزامی است.")]
    [MaxLength(100, ErrorMessage = "عنوان دستگاه نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string Title { get; set; }

    /// <summary>
    /// نام دستگاه (برای سازگاری با ViewModels)
    /// </summary>
    public string Name => Title;

    /// <summary>
    /// شماره ترمینال
    /// </summary>
    [Required(ErrorMessage = "شماره ترمینال الزامی است.")]
    [MaxLength(50, ErrorMessage = "شماره ترمینال نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string TerminalId { get; set; }

    /// <summary>
    /// شماره پذیرنده
    /// </summary>
    [Required(ErrorMessage = "شماره پذیرنده الزامی است.")]
    [MaxLength(50, ErrorMessage = "شماره پذیرنده نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string MerchantId { get; set; }

    /// <summary>
    /// شماره سریال دستگاه
    /// </summary>
    [MaxLength(100, ErrorMessage = "شماره سریال نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string SerialNumber { get; set; }

    /// <summary>
    /// آدرس IP دستگاه در شبکه داخلی
    /// </summary>
    [MaxLength(50, ErrorMessage = "آدرس IP نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string IpAddress { get; set; }

    /// <summary>
    /// آدرس MAC دستگاه
    /// </summary>
    [MaxLength(50, ErrorMessage = "آدرس MAC نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string MacAddress { get; set; }

    /// <summary>
    /// نوع ارائه‌دهنده پوز
    /// </summary>
    [Required(ErrorMessage = "نوع ارائه‌دهنده الزامی است.")]
    public PosProviderType Provider { get; set; }

    /// <summary>
    /// نوع ارائه‌دهنده پوز (برای سازگاری با ViewModels)
    /// </summary>
    public PosProviderType ProviderType => Provider;

    /// <summary>
    /// آیا دستگاه فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// پروتکل ارتباطی
    /// </summary>
    public PosProtocol Protocol { get; set; } = PosProtocol.Tcp;

    /// <summary>
    /// رشته اتصال (برای سازگاری با ViewModels)
    /// </summary>
    public string ConnectionString => $"{IpAddress}:{Port}";

    /// <summary>
    /// توضیحات (برای سازگاری با ViewModels)
    /// </summary>
    public string Description => $"{Title} - {Provider}";

    /// <summary>
    /// آیا پیش‌فرض است؟ (برای سازگاری با ViewModels)
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// پورت ارتباطی
    /// </summary>
    public int? Port { get; set; }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن دستگاه
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف دستگاه
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که دستگاه را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد دستگاه
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که دستگاه را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش دستگاه
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که دستگاه را ویرایش کرده است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }

    /// <summary>
    /// نام کاربر ایجاد کننده (برای سازگاری با ViewModels)
    /// </summary>
    public string CreatedByUserName => CreatedByUser?.UserName ?? string.Empty;

    /// <summary>
    /// نام کاربر ویرایش کننده (برای سازگاری با ViewModels)
    /// </summary>
    public string UpdatedByUserName => UpdatedByUser?.UserName ?? string.Empty;

    /// <summary>
    /// تعداد کل تراکنش‌ها (برای سازگاری با ViewModels)
    /// </summary>
    public int TotalTransactions => Transactions?.Count ?? 0;

    /// <summary>
    /// کل مبلغ تراکنش‌ها (برای سازگاری با ViewModels)
    /// </summary>
    public decimal TotalAmount => Transactions?.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount) ?? 0;

    /// <summary>
    /// نرخ موفقیت (برای سازگاری با ViewModels)
    /// </summary>
    public decimal SuccessRate => TotalTransactions > 0 ? (decimal)Transactions?.Count(t => t.Status == PaymentStatus.Success) / TotalTransactions * 100 : 0;

    /// <summary>
    /// تاریخ آخرین تراکنش (برای سازگاری با ViewModels)
    /// </summary>
    public DateTime? LastTransactionDate => Transactions?.Where(t => t.Status == PaymentStatus.Success).Max(t => t.CreatedAt);
    #endregion

    #region روابط
    /// <summary>
    /// لیست تراکنش‌های انجام شده با این دستگاه
    /// </summary>
    public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new HashSet<PaymentTransaction>();
    #endregion
}
/// <summary>
/// پیکربندی مدل دستگاه‌های پوز بانکی برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class PosTerminalConfig : EntityTypeConfiguration<PosTerminal>
{
    public PosTerminalConfig()
    {
        ToTable("PosTerminals");
        HasKey(p => p.PosTerminalId);

        // ویژگی‌های اصلی
        Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PosTerminal_Title")));

        Property(p => p.TerminalId)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PosTerminal_TerminalId")));

        Property(p => p.MerchantId)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PosTerminal_MerchantId")));

        Property(p => p.SerialNumber)
            .IsOptional()
            .HasMaxLength(100);

        Property(p => p.IpAddress)
            .IsOptional()
            .HasMaxLength(50);

        Property(p => p.MacAddress)
            .IsOptional()
            .HasMaxLength(50);

        Property(p => p.Provider)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PosTerminal_Provider")));

        Property(p => p.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PosTerminal_IsActive")));

        Property(p => p.Protocol)
            .IsRequired();

        Property(p => p.Port)
            .IsOptional();

        // پیاده‌سازی ISoftDelete
        Property(p => p.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PosTerminal_IsDeleted")));

        Property(p => p.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PosTerminal_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PosTerminal_CreatedAt")));

        Property(p => p.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PosTerminal_CreatedByUserId")));

        Property(p => p.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PosTerminal_UpdatedAt")));

        Property(p => p.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PosTerminal_UpdatedByUserId")));

        Property(p => p.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_PosTerminal_DeletedByUserId")));

        // روابط
        HasMany(p => p.Transactions)
            .WithOptional(t => t.PosTerminal)
            .HasForeignKey(t => t.PosTerminalId)
            .WillCascadeOnDelete(false);

        HasOptional(p => p.DeletedByUser)
            .WithMany()
            .HasForeignKey(p => p.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(p => p.UpdatedByUser)
            .WithMany()
            .HasForeignKey(p => p.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        HasIndex(p => new { p.IsActive, p.Provider })
            .HasName("IX_PosTerminal_IsActive_Provider");
    }
}