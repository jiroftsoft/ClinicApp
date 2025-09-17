using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace ClinicApp.Models.Entities.Payment;

/// <summary>
/// مدل شیفت کاری صندوق - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت شیفت‌های صندوق برای گزارش‌گیری روزانه
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات مالی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// 5. محاسبه خودکار مانده شیفت
/// </summary>
public class CashSession : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه شیفت صندوق
    /// </summary>
    public int CashSessionId { get; set; }

    /// <summary>
    /// شناسه شیفت صندوق (برای سازگاری با ViewModels)
    /// </summary>
    public int Id => CashSessionId;

    /// <summary>
    /// شماره شیفت (برای سازگاری با ViewModels)
    /// </summary>
    public string SessionNumber => $"CS{CashSessionId:D6}";

    /// <summary>
    /// شناسه کاربر (منشی صندوق‌دار)
    /// </summary>
    [Required(ErrorMessage = "منشی صندوق‌دار الزامی است.")]
    public string UserId { get; set; }

    /// <summary>
    /// نام کاربر (برای سازگاری با ViewModels)
    /// </summary>
    public string UserName => User?.UserName ?? string.Empty;

    /// <summary>
    /// تاریخ و زمان باز شدن شیفت
    /// </summary>
    [Required(ErrorMessage = "تاریخ باز شدن شیفت الزامی است.")]
    public DateTime OpenedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// زمان شروع (برای سازگاری با ViewModels)
    /// </summary>
    public DateTime StartTime => OpenedAt;

    /// <summary>
    /// تاریخ و زمان بسته شدن شیفت
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// زمان پایان (برای سازگاری با ViewModels)
    /// </summary>
    public DateTime? EndTime => ClosedAt;

    /// <summary>
    /// مدت زمان (برای سازگاری با ViewModels)
    /// </summary>
    public TimeSpan? Duration => ClosedAt.HasValue ? ClosedAt.Value - OpenedAt : null;

    /// <summary>
    /// مانده اولیه شیفت
    /// </summary>
    [Required(ErrorMessage = "مانده اولیه الزامی است.")]
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal")]
    public decimal OpeningBalance { get; set; } = 0;

    /// <summary>
    /// مانده اولیه شیفت (برای سازگاری با ViewModels)
    /// </summary>
    public decimal InitialCashAmount => OpeningBalance;

    /// <summary>
    /// مانده نقدی شیفت
    /// </summary>
    [Required(ErrorMessage = "مانده نقدی الزامی است.")]
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal")]
    public decimal CashBalance { get; set; } = 0;

    /// <summary>
    /// مانده نهایی نقدی (برای سازگاری با ViewModels)
    /// </summary>
    public decimal FinalCashAmount => CashBalance;

    /// <summary>
    /// مانده پوز شیفت
    /// </summary>
    [Required(ErrorMessage = "مانده پوز الزامی است.")]
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal")]
    public decimal PosBalance { get; set; } = 0;

    /// <summary>
    /// کل درآمد (برای سازگاری با ViewModels)
    /// </summary>
    public decimal TotalIncome => CashBalance + PosBalance;

    /// <summary>
    /// کل هزینه (برای سازگاری با ViewModels)
    /// </summary>
    public decimal TotalExpense => 0; // در صورت نیاز می‌توان محاسبه کرد

    /// <summary>
    /// مانده فعلی (برای سازگاری با ViewModels)
    /// </summary>
    public decimal CurrentBalance => CashBalance;

    /// <summary>
    /// مانده مورد انتظار (برای سازگاری با ViewModels)
    /// </summary>
    public decimal ExpectedBalance => OpeningBalance + TotalIncome - TotalExpense;

    /// <summary>
    /// تفاوت (برای سازگاری با ViewModels)
    /// </summary>
    public decimal Difference => CurrentBalance - ExpectedBalance;

    /// <summary>
    /// وضعیت شیفت
    /// </summary>
    public CashSessionStatus Status { get; set; } = CashSessionStatus.Open;

    /// <summary>
    /// توضیحات (برای سازگاری با ViewModels)
    /// </summary>
    public string Description => $"شیفت {SessionNumber} - {UserName}";

    /// <summary>
    /// شناسه کاربر پایان‌دهنده (برای سازگاری با ViewModels)
    /// </summary>
    public string EndedByUserId => UpdatedByUserId;

    /// <summary>
    /// نام کاربر پایان‌دهنده (برای سازگاری با ViewModels)
    /// </summary>
    public string EndedByUserName => UpdatedByUser?.UserName ?? string.Empty;

    /// <summary>
    /// تعداد کل تراکنش‌ها (برای سازگاری با ViewModels)
    /// </summary>
    public int TotalTransactions => Transactions?.Count ?? 0;

    /// <summary>
    /// تراکنش‌های نقدی (برای سازگاری با ViewModels)
    /// </summary>
    public IEnumerable<PaymentTransaction> CashTransactions => Transactions?.Where(t => t.Method == PaymentMethod.Cash) ?? Enumerable.Empty<PaymentTransaction>();

    /// <summary>
    /// تراکنش‌های پوز (برای سازگاری با ViewModels)
    /// </summary>
    public IEnumerable<PaymentTransaction> PosTransactions => Transactions?.Where(t => t.Method == PaymentMethod.POS) ?? Enumerable.Empty<PaymentTransaction>();

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن شیفت
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مالی مجاز نیست
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف شیفت
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که شیفت را حذف کرده است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد شیفت
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که شیفت را ایجاد کرده است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش شیفت
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که شیفت را ویرایش کرده است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    #region روابط
    /// <summary>
    /// ارجاع به کاربر (منشی صندوق‌دار)
    /// </summary>
    public virtual ApplicationUser User { get; set; }

    /// <summary>
    /// لیست تراکنش‌های مربوط به این شیفت
    /// </summary>
    public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new HashSet<PaymentTransaction>();
    #endregion
}

/// <summary>
/// پیکربندی مدل شیفت کاری صندوق برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class CashSessionConfig : EntityTypeConfiguration<CashSession>
{
    public CashSessionConfig()
    {
        ToTable("CashSessions");
        HasKey(cs => cs.CashSessionId);

        // ویژگی‌های اصلی
        Property(cs => cs.UserId)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_CashSession_UserId")));

        Property(cs => cs.OpenedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_CashSession_OpenedAt")));

        Property(cs => cs.ClosedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_CashSession_ClosedAt")));

        Property(cs => cs.OpeningBalance)
            .IsRequired()
            .HasPrecision(18, 4);

        Property(cs => cs.CashBalance)
            .IsRequired()
            .HasPrecision(18, 4);

        Property(cs => cs.PosBalance)
            .IsRequired()
            .HasPrecision(18, 4);

        Property(cs => cs.Status)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_CashSession_Status")));

        // پیاده‌سازی ISoftDelete
        Property(cs => cs.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_CashSession_IsDeleted")));

        Property(cs => cs.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_CashSession_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(cs => cs.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_CashSession_CreatedAt")));

        Property(cs => cs.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_CashSession_CreatedByUserId")));

        Property(cs => cs.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_CashSession_UpdatedAt")));

        Property(cs => cs.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_CashSession_UpdatedByUserId")));

        Property(cs => cs.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_CashSession_DeletedByUserId")));

        // روابط
        HasRequired(cs => cs.User)
            .WithMany()
            .HasForeignKey(cs => cs.UserId)
            .WillCascadeOnDelete(false);

        HasMany(cs => cs.Transactions)
            .WithRequired(t => t.CashSession)
            .HasForeignKey(t => t.CashSessionId)
            .WillCascadeOnDelete(false);

        HasOptional(cs => cs.DeletedByUser)
            .WithMany()
            .HasForeignKey(cs => cs.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(cs => cs.CreatedByUser)
            .WithMany()
            .HasForeignKey(cs => cs.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(cs => cs.UpdatedByUser)
            .WithMany()
            .HasForeignKey(cs => cs.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        HasIndex(cs => new { cs.UserId, cs.Status, cs.OpenedAt })
            .HasName("IX_CashSession_UserId_Status_OpenedAt");
    }
}