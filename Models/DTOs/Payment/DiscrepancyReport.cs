using System;
using System.Collections.Generic;

namespace ClinicApp.Models.DTOs.Payment
{
    /// <summary>
    /// گزارش اختلاف‌های مالی
    /// </summary>
    public class DiscrepancyReport
    {
        /// <summary>
        /// شناسه جلسه صندوق
        /// </summary>
        public int CashSessionId { get; set; }

        /// <summary>
        /// تعداد کل اختلاف‌ها
        /// </summary>
        public int TotalDiscrepancies { get; set; }

        /// <summary>
        /// تعداد اختلاف‌های حل نشده
        /// </summary>
        public int UnresolvedCount { get; set; }

        /// <summary>
        /// مجموع مبلغ اختلاف‌ها (ریال)
        /// </summary>
        public decimal TotalDiscrepancyAmount { get; set; }

        /// <summary>
        /// لیست اختلاف‌ها
        /// </summary>
        public List<DiscrepancyDetail> Discrepancies { get; set; } = new List<DiscrepancyDetail>();
    }
}

