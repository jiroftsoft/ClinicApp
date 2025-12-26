using System;
using System.Collections.Generic;

namespace ClinicApp.Models.DTOs.Payment
{
    /// <summary>
    /// گزارش روزانه عملکرد منشی
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class CashierDailyReport
    {
        /// <summary>
        /// شناسه منشی
        /// </summary>
        public string CashierId { get; set; }

        /// <summary>
        /// نام منشی
        /// </summary>
        public string CashierName { get; set; }

        /// <summary>
        /// تاریخ گزارش
        /// </summary>
        public DateTime Date { get; set; }

        #region Session Information

        /// <summary>
        /// تعداد جلسات باز شده
        /// </summary>
        public int SessionsOpened { get; set; }

        /// <summary>
        /// تعداد جلسات بسته شده
        /// </summary>
        public int SessionsClosed { get; set; }

        /// <summary>
        /// لیست خلاصه جلسات
        /// </summary>
        public List<CashSessionSummary> Sessions { get; set; } = new List<CashSessionSummary>();

        #endregion

        #region Transaction Information

        /// <summary>
        /// تعداد کل تراکنش‌ها
        /// </summary>
        public int TotalTransactions { get; set; }

        /// <summary>
        /// تعداد تراکنش‌های POS
        /// </summary>
        public int PosTransactions { get; set; }

        /// <summary>
        /// تعداد تراکنش‌های نقدی
        /// </summary>
        public int CashTransactions { get; set; }

        /// <summary>
        /// مبلغ کل (ریال)
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// مبلغ POS (ریال)
        /// </summary>
        public decimal PosAmount { get; set; }

        /// <summary>
        /// مبلغ نقدی (ریال)
        /// </summary>
        public decimal CashAmount { get; set; }

        #endregion

        #region Performance Metrics

        /// <summary>
        /// زمان میانگین هر تراکنش (ثانیه)
        /// </summary>
        public decimal AverageTransactionTime { get; set; }

        /// <summary>
        /// نرخ موفقیت (درصد)
        /// </summary>
        public decimal SuccessRate { get; set; }

        /// <summary>
        /// تعداد تراکنش‌های موفق
        /// </summary>
        public int SuccessfulTransactions { get; set; }

        /// <summary>
        /// تعداد تراکنش‌های ناموفق
        /// </summary>
        public int FailedTransactions { get; set; }

        #endregion

        #region Discrepancy Information

        /// <summary>
        /// تعداد اختلاف‌ها
        /// </summary>
        public int DiscrepancyCount { get; set; }

        /// <summary>
        /// مجموع مبلغ اختلاف‌ها (ریال)
        /// </summary>
        public decimal TotalDiscrepancy { get; set; }

        /// <summary>
        /// لیست اختلاف‌ها
        /// </summary>
        public List<DiscrepancySummary> Discrepancies { get; set; } = new List<DiscrepancySummary>();

        #endregion
    }
}

