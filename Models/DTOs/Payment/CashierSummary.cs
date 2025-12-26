using System;

namespace ClinicApp.Models.DTOs.Payment
{
    /// <summary>
    /// خلاصه عملکرد یک منشی در بازه زمانی
    /// </summary>
    public class CashierSummary
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
        /// تعداد جلسات
        /// </summary>
        public int SessionCount { get; set; }

        /// <summary>
        /// تعداد تراکنش‌ها
        /// </summary>
        public int TransactionCount { get; set; }

        /// <summary>
        /// مبلغ کل (ریال)
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// تعداد اختلاف‌ها
        /// </summary>
        public int DiscrepancyCount { get; set; }

        /// <summary>
        /// نرخ موفقیت (درصد)
        /// </summary>
        public decimal SuccessRate { get; set; }

        /// <summary>
        /// رتبه عملکرد (1 = بهترین)
        /// </summary>
        public int? Rank { get; set; }
    }
}

