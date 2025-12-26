using System;

namespace ClinicApp.Models.DTOs.Payment
{
    /// <summary>
    /// خلاصه اطلاعات یک اختلاف مالی
    /// </summary>
    public class DiscrepancySummary
    {
        /// <summary>
        /// شناسه اختلاف
        /// </summary>
        public int Id { get; set; }

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
        /// وضعیت
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// تاریخ گزارش
        /// </summary>
        public DateTime ReportedAt { get; set; }
    }
}

