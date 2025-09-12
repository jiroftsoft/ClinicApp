using System;

namespace ClinicApp.Models.Statistics
{
    /// <summary>
    /// آمار تراکنش‌های پرداخت
    /// </summary>
    public class PaymentTransactionStatistics
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
        /// تعداد کل تراکنش‌ها
        /// </summary>
        public int TotalTransactions { get; set; }

        /// <summary>
        /// مبلغ کل تراکنش‌ها
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// تعداد تراکنش‌های موفق
        /// </summary>
        public int SuccessfulTransactions { get; set; }

        /// <summary>
        /// مبلغ تراکنش‌های موفق
        /// </summary>
        public decimal SuccessfulAmount { get; set; }

        /// <summary>
        /// تعداد تراکنش‌های ناموفق
        /// </summary>
        public int FailedTransactions { get; set; }

        /// <summary>
        /// مبلغ تراکنش‌های ناموفق
        /// </summary>
        public decimal FailedAmount { get; set; }

        /// <summary>
        /// تعداد تراکنش‌های در انتظار
        /// </summary>
        public int PendingTransactions { get; set; }

        /// <summary>
        /// مبلغ تراکنش‌های در انتظار
        /// </summary>
        public decimal PendingAmount { get; set; }

        /// <summary>
        /// تعداد تراکنش‌های لغو شده
        /// </summary>
        public int CanceledTransactions { get; set; }

        /// <summary>
        /// مبلغ تراکنش‌های لغو شده
        /// </summary>
        public decimal CanceledAmount { get; set; }

        /// <summary>
        /// زمان محاسبه
        /// </summary>
        public DateTime CalculatedAt { get; set; }
    }
}
