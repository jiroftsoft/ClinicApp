using System;
using System.Collections.Generic;

namespace ClinicApp.Models.DTOs.Payment
{
    /// <summary>
    /// گزارش تطبیق موجودی جلسه صندوق
    /// </summary>
    public class ReconciliationReport
    {
        /// <summary>
        /// شناسه جلسه صندوق
        /// </summary>
        public int CashSessionId { get; set; }

        /// <summary>
        /// مانده نقدی مورد انتظار (ریال)
        /// </summary>
        public decimal ExpectedCashBalance { get; set; }

        /// <summary>
        /// مانده نقدی واقعی (ریال)
        /// </summary>
        public decimal ActualCashBalance { get; set; }

        /// <summary>
        /// تفاوت نقدی (ریال)
        /// </summary>
        public decimal CashDifference { get; set; }

        /// <summary>
        /// مانده POS مورد انتظار (ریال)
        /// </summary>
        public decimal ExpectedPosBalance { get; set; }

        /// <summary>
        /// مانده POS واقعی (ریال)
        /// </summary>
        public decimal ActualPosBalance { get; set; }

        /// <summary>
        /// تفاوت POS (ریال)
        /// </summary>
        public decimal PosDifference { get; set; }

        /// <summary>
        /// آیا تطبیق انجام شده است؟
        /// </summary>
        public bool IsReconciled { get; set; }

        /// <summary>
        /// لیست اختلاف‌ها
        /// </summary>
        public List<DiscrepancyDetail> Discrepancies { get; set; } = new List<DiscrepancyDetail>();

        /// <summary>
        /// تاریخ تطبیق
        /// </summary>
        public DateTime? ReconciledAt { get; set; }

        /// <summary>
        /// کاربر انجام‌دهنده تطبیق
        /// </summary>
        public string ReconciledBy { get; set; }
    }
}

