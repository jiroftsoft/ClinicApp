using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.Statistics
{
    /// <summary>
    /// مدل آمار پرداخت‌ها
    /// </summary>
    public class PaymentStatistics
    {
        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// کل تراکنش‌ها
        /// </summary>
        public int TotalTransactions { get; set; }

        /// <summary>
        /// کل مبلغ
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// میانگین مبلغ
        /// </summary>
        public decimal AverageAmount { get; set; }

        /// <summary>
        /// حداقل مبلغ
        /// </summary>
        public decimal MinAmount { get; set; }

        /// <summary>
        /// حداکثر مبلغ
        /// </summary>
        public decimal MaxAmount { get; set; }

        /// <summary>
        /// تراکنش‌های موفق
        /// </summary>
        public int SuccessfulTransactions { get; set; }

        /// <summary>
        /// مبلغ تراکنش‌های موفق
        /// </summary>
        public decimal SuccessfulAmount { get; set; }

        /// <summary>
        /// تراکنش‌های ناموفق
        /// </summary>
        public int FailedTransactions { get; set; }

        /// <summary>
        /// مبلغ تراکنش‌های ناموفق
        /// </summary>
        public decimal FailedAmount { get; set; }

        /// <summary>
        /// تراکنش‌های در انتظار
        /// </summary>
        public int PendingTransactions { get; set; }

        /// <summary>
        /// مبلغ تراکنش‌های در انتظار
        /// </summary>
        public decimal PendingAmount { get; set; }

        /// <summary>
        /// تراکنش‌های لغو شده
        /// </summary>
        public int CanceledTransactions { get; set; }

        /// <summary>
        /// مبلغ نقدی
        /// </summary>
        public decimal CashAmount { get; set; }

        /// <summary>
        /// مبلغ POS
        /// </summary>
        public decimal PosAmount { get; set; }

        /// <summary>
        /// مبلغ آنلاین
        /// </summary>
        public decimal OnlineAmount { get; set; }

        /// <summary>
        /// مبلغ بدهی
        /// </summary>
        public decimal DebtAmount { get; set; }

        /// <summary>
        /// نرخ موفقیت (درصد)
        /// </summary>
        public decimal SuccessRate { get; set; }

        /// <summary>
        /// تراکنش‌ها بر اساس روش پرداخت
        /// </summary>
        public Dictionary<PaymentMethod, int> TransactionsByMethod { get; set; } = new();

        /// <summary>
        /// تراکنش‌ها بر اساس وضعیت
        /// </summary>
        public Dictionary<PaymentStatus, int> TransactionsByStatus { get; set; } = new();

        /// <summary>
        /// تاریخ محاسبه
        /// </summary>
        public DateTime CalculatedAt { get; set; } = DateTime.Now;
    }
}
