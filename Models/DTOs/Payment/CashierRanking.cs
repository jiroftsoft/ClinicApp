using System;

namespace ClinicApp.Models.DTOs.Payment
{
    /// <summary>
    /// رتبه‌بندی و رنک یک منشی
    /// </summary>
    public class CashierRanking
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
        /// رتبه (1 = بهترین)
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// تعداد کل تراکنش‌ها
        /// </summary>
        public int TotalTransactions { get; set; }

        /// <summary>
        /// مبلغ کل (ریال)
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// نرخ موفقیت (درصد)
        /// </summary>
        public decimal SuccessRate { get; set; }

        /// <summary>
        /// تعداد اختلاف‌ها
        /// </summary>
        public int DiscrepancyCount { get; set; }

        /// <summary>
        /// مجموع مبلغ اختلاف‌ها (ریال)
        /// </summary>
        public decimal TotalDiscrepancyAmount { get; set; }

        /// <summary>
        /// زمان میانگین هر تراکنش (ثانیه)
        /// </summary>
        public decimal AverageTransactionTime { get; set; }

        /// <summary>
        /// تعداد جلسات
        /// </summary>
        public int SessionsCount { get; set; }

        /// <summary>
        /// امتیاز کلی (محاسبه شده)
        /// </summary>
        public decimal OverallScore { get; set; }
    }
}

