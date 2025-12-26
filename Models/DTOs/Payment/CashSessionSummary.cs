using System;

namespace ClinicApp.Models.DTOs.Payment
{
    /// <summary>
    /// خلاصه اطلاعات یک جلسه صندوق
    /// </summary>
    public class CashSessionSummary
    {
        /// <summary>
        /// شناسه جلسه
        /// </summary>
        public int CashSessionId { get; set; }

        /// <summary>
        /// شماره جلسه
        /// </summary>
        public string SessionNumber { get; set; }

        /// <summary>
        /// زمان باز شدن
        /// </summary>
        public DateTime OpenedAt { get; set; }

        /// <summary>
        /// زمان بسته شدن
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// مدت زمان (دقیقه)
        /// </summary>
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// مانده اولیه (ریال)
        /// </summary>
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// مانده نقدی (ریال)
        /// </summary>
        public decimal CashBalance { get; set; }

        /// <summary>
        /// مانده POS (ریال)
        /// </summary>
        public decimal PosBalance { get; set; }

        /// <summary>
        /// تعداد تراکنش‌ها
        /// </summary>
        public int TransactionCount { get; set; }

        /// <summary>
        /// وضعیت جلسه
        /// </summary>
        public string Status { get; set; }
    }
}

