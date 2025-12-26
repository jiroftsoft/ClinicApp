using System;
using System.Collections.Generic;

namespace ClinicApp.Models.DTOs.Payment
{
    /// <summary>
    /// مقایسه عملکرد چند منشی
    /// </summary>
    public class CashierPerformanceComparison
    {
        /// <summary>
        /// از تاریخ
        /// </summary>
        public DateTime FromDate { get; set; }

        /// <summary>
        /// تا تاریخ
        /// </summary>
        public DateTime ToDate { get; set; }

        /// <summary>
        /// لیست خلاصه عملکرد منشی‌ها
        /// </summary>
        public List<CashierSummary> Cashiers { get; set; } = new List<CashierSummary>();

        /// <summary>
        /// بهترین منشی (بیشترین تراکنش)
        /// </summary>
        public CashierSummary TopPerformer { get; set; }

        /// <summary>
        /// میانگین تعداد تراکنش‌ها
        /// </summary>
        public decimal AverageTransactionCount { get; set; }

        /// <summary>
        /// میانگین مبلغ کل
        /// </summary>
        public decimal AverageTotalAmount { get; set; }

        /// <summary>
        /// میانگین نرخ موفقیت
        /// </summary>
        public decimal AverageSuccessRate { get; set; }
    }
}

