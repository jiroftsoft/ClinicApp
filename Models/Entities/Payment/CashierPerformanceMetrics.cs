using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Core;

namespace ClinicApp.Models.Entities.Payment
{
    /// <summary>
    /// متریک‌های عملکرد روزانه منشی‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. محاسبه خودکار متریک‌های عملکرد روزانه
    /// 2. ذخیره آمار تراکنش‌ها (تعداد، مبلغ، نوع)
    /// 3. محاسبه نرخ موفقیت و زمان میانگین
    /// 4. ذخیره آمار اختلاف‌ها
    /// 5. ذخیره آمار جلسات صندوق
    /// 
    /// موارد استفاده:
    /// - Dashboard منشی‌ها
    /// - گزارش عملکرد
    /// - مقایسه منشی‌ها
    /// - شناسایی بهترین/ضعیف‌ترین عملکردها
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class CashierPerformanceMetrics
    {
        /// <summary>
        /// شناسه
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه منشی
        /// </summary>
        [Required(ErrorMessage = "شناسه منشی الزامی است.")]
        [MaxLength(128)]
        public string CashierId { get; set; }

        /// <summary>
        /// تاریخ (بدون ساعت)
        /// </summary>
        [Required(ErrorMessage = "تاریخ الزامی است.")]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        #region Transaction Metrics

        /// <summary>
        /// تعداد کل تراکنش‌ها
        /// </summary>
        [Required]
        public int TotalTransactions { get; set; } = 0;

        /// <summary>
        /// تعداد تراکنش‌های POS
        /// </summary>
        [Required]
        public int PosTransactions { get; set; } = 0;

        /// <summary>
        /// تعداد تراکنش‌های نقدی
        /// </summary>
        [Required]
        public int CashTransactions { get; set; } = 0;

        /// <summary>
        /// مبلغ کل (ریال)
        /// </summary>
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal TotalAmount { get; set; } = 0;

        /// <summary>
        /// مبلغ POS (ریال)
        /// </summary>
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal PosAmount { get; set; } = 0;

        /// <summary>
        /// مبلغ نقدی (ریال)
        /// </summary>
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal CashAmount { get; set; } = 0;

        #endregion

        #region Performance Metrics

        /// <summary>
        /// زمان میانگین هر تراکنش (ثانیه)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal")]
        public decimal AverageTransactionTime { get; set; } = 0;

        /// <summary>
        /// تعداد تراکنش‌های موفق
        /// </summary>
        [Required]
        public int SuccessfulTransactions { get; set; } = 0;

        /// <summary>
        /// تعداد تراکنش‌های ناموفق
        /// </summary>
        [Required]
        public int FailedTransactions { get; set; } = 0;

        /// <summary>
        /// نرخ موفقیت (درصد)
        /// محاسبه: (SuccessfulTransactions / TotalTransactions) * 100
        /// </summary>
        [Required]
        [Column(TypeName = "decimal")]
        public decimal SuccessRate { get; set; } = 0;

        #endregion

        #region Discrepancy Metrics

        /// <summary>
        /// تعداد اختلاف‌ها
        /// </summary>
        [Required]
        public int DiscrepancyCount { get; set; } = 0;

        /// <summary>
        /// مجموع مبلغ اختلاف‌ها (ریال)
        /// </summary>
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal TotalDiscrepancy { get; set; } = 0;

        #endregion

        #region Session Metrics

        /// <summary>
        /// تعداد جلسات باز شده
        /// </summary>
        [Required]
        public int SessionsOpened { get; set; } = 0;

        /// <summary>
        /// تعداد جلسات بسته شده
        /// </summary>
        [Required]
        public int SessionsClosed { get; set; } = 0;

        /// <summary>
        /// مدت زمان میانگین جلسات (TimeSpan)
        /// </summary>
        [Column(TypeName = "time")]
        public TimeSpan? AverageSessionDuration { get; set; }

        #endregion

        #region Tracking

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// ارجاع به منشی
        /// </summary>
        public virtual ApplicationUser Cashier { get; set; }

        #endregion
    }

    /// <summary>
    /// پیکربندی Entity Framework برای CashierPerformanceMetrics
    /// 
    /// Indexes:
    /// - CashierId (برای جستجوی متریک‌های یک منشی)
    /// - Date (برای جستجوی بر اساس تاریخ)
    /// - Composite: CashierId + Date (برای یکتایی و جستجوی بهینه)
    /// 
    /// Unique Constraint: یک منشی در یک روز فقط یک رکورد متریک دارد
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class CashierPerformanceMetricsConfig : EntityTypeConfiguration<CashierPerformanceMetrics>
    {
        public CashierPerformanceMetricsConfig()
        {
            // ✅ Table Name
            ToTable("CashierPerformanceMetrics");

            // ✅ Primary Key
            HasKey(x => x.Id);

            // ✅ Properties Configuration
            Property(x => x.CashierId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashierPerformanceMetrics_CashierId")));

            Property(x => x.Date)
                .IsRequired()
                .HasColumnType("date")
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashierPerformanceMetrics_Date")));

            // Transaction Metrics
            Property(x => x.TotalTransactions).IsRequired();
            Property(x => x.PosTransactions).IsRequired();
            Property(x => x.CashTransactions).IsRequired();
            Property(x => x.TotalAmount).IsRequired().HasPrecision(18, 0);
            Property(x => x.PosAmount).IsRequired().HasPrecision(18, 0);
            Property(x => x.CashAmount).IsRequired().HasPrecision(18, 0);

            // Performance Metrics
            Property(x => x.AverageTransactionTime).IsRequired().HasPrecision(10, 2);
            Property(x => x.SuccessfulTransactions).IsRequired();
            Property(x => x.FailedTransactions).IsRequired();
            Property(x => x.SuccessRate).IsRequired().HasPrecision(5, 2);

            // Discrepancy Metrics
            Property(x => x.DiscrepancyCount).IsRequired();
            Property(x => x.TotalDiscrepancy).IsRequired().HasPrecision(18, 0);

            // Session Metrics
            Property(x => x.SessionsOpened).IsRequired();
            Property(x => x.SessionsClosed).IsRequired();
            Property(x => x.AverageSessionDuration).IsOptional().HasColumnType("time");

            // Tracking
            Property(x => x.CreatedAt).IsRequired();
            Property(x => x.UpdatedAt).IsOptional();

            // ✅ Relationships
            HasRequired(x => x.Cashier)
                .WithMany()
                .HasForeignKey(x => x.CashierId)
                .WillCascadeOnDelete(false);

            // ✅ Unique Constraint: یک منشی در یک روز فقط یک رکورد
            HasIndex(x => new { x.CashierId, x.Date })
                .IsUnique()
                .HasName("IX_CashierPerformanceMetrics_CashierId_Date_Unique");
        }
    }
}

