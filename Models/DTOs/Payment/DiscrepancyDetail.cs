using System;

namespace ClinicApp.Models.DTOs.Payment
{
    /// <summary>
    /// جزئیات یک اختلاف مالی
    /// </summary>
    public class DiscrepancyDetail
    {
        /// <summary>
        /// شناسه اختلاف
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// شناسه تراکنش پرداخت (در صورت وجود)
        /// </summary>
        public int? PaymentTransactionId { get; set; }

        /// <summary>
        /// نوع اختلاف
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// مبلغ مورد انتظار (ریال)
        /// </summary>
        public decimal ExpectedAmount { get; set; }

        /// <summary>
        /// مبلغ واقعی (ریال)
        /// </summary>
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// تفاوت (ریال)
        /// </summary>
        public decimal Difference { get; set; }

        /// <summary>
        /// دلیل
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// راه‌حل
        /// </summary>
        public string Resolution { get; set; }

        /// <summary>
        /// وضعیت
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// تاریخ گزارش
        /// </summary>
        public DateTime ReportedAt { get; set; }

        /// <summary>
        /// گزارش‌دهنده
        /// </summary>
        public string ReportedBy { get; set; }

        /// <summary>
        /// تاریخ حل
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// حل‌کننده
        /// </summary>
        public string ResolvedBy { get; set; }
    }
}

