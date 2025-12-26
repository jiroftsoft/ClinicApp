using System;
using System.Collections.Generic;

namespace ClinicApp.Models.DTOs.Payment
{
    /// <summary>
    /// گزارش ماهانه عملکرد منشی
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class CashierMonthlyReport
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
        /// سال
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// ماه (1-12)
        /// </summary>
        public int Month { get; set; }

        #region Summary

        /// <summary>
        /// تعداد کل جلسات
        /// </summary>
        public int TotalSessions { get; set; }

        /// <summary>
        /// تعداد کل تراکنش‌ها
        /// </summary>
        public int TotalTransactions { get; set; }

        /// <summary>
        /// مبلغ کل (ریال)
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// تعداد کل اختلاف‌ها
        /// </summary>
        public int TotalDiscrepancies { get; set; }

        #endregion

        #region Daily Breakdown

        /// <summary>
        /// گزارش روزانه (31 روز)
        /// </summary>
        public List<CashierDailyReport> DailyReports { get; set; } = new List<CashierDailyReport>();

        #endregion

        #region Performance Metrics

        /// <summary>
        /// میانگین زمان تراکنش (ثانیه)
        /// </summary>
        public decimal AverageTransactionTime { get; set; }

        /// <summary>
        /// میانگین نرخ موفقیت (درصد)
        /// </summary>
        public decimal AverageSuccessRate { get; set; }

        /// <summary>
        /// بهترین روز (بیشترین تراکنش)
        /// </summary>
        public DateTime? BestDay { get; set; }

        /// <summary>
        /// بدترین روز (کمترین تراکنش)
        /// </summary>
        public DateTime? WorstDay { get; set; }

        #endregion
    }
}

