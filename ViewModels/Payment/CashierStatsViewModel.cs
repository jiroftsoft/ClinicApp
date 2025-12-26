using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Payment
{
    /// <summary>
    /// ViewModel برای آمار منشی
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class CashierStatsViewModel
    {
        /// <summary>
        /// تعداد کل تراکنش‌ها
        /// </summary>
        [Display(Name = "تعداد کل تراکنش‌ها")]
        public int TotalTransactions { get; set; }

        /// <summary>
        /// مبلغ کل (ریال)
        /// </summary>
        [Display(Name = "مبلغ کل")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// نرخ موفقیت (درصد)
        /// </summary>
        [Display(Name = "نرخ موفقیت")]
        public decimal SuccessRate { get; set; }

        /// <summary>
        /// زمان میانگین هر تراکنش (ثانیه)
        /// </summary>
        [Display(Name = "زمان میانگین تراکنش")]
        public decimal AverageTransactionTime { get; set; }

        /// <summary>
        /// تعداد اختلاف‌ها
        /// </summary>
        [Display(Name = "تعداد اختلاف‌ها")]
        public int DiscrepancyCount { get; set; }

        /// <summary>
        /// تعداد جلسات باز شده
        /// </summary>
        [Display(Name = "جلسات باز شده")]
        public int SessionsOpened { get; set; }

        /// <summary>
        /// تعداد جلسات بسته شده
        /// </summary>
        [Display(Name = "جلسات بسته شده")]
        public int SessionsClosed { get; set; }
    }
}

