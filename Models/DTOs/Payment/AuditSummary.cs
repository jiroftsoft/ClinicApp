using System;
using System.Collections.Generic;

namespace ClinicApp.Models.DTOs.Payment
{
    /// <summary>
    /// خلاصه Audit Trail یک جلسه صندوق
    /// </summary>
    public class AuditSummary
    {
        /// <summary>
        /// شناسه جلسه صندوق
        /// </summary>
        public int CashSessionId { get; set; }

        /// <summary>
        /// تعداد کل لاگ‌ها
        /// </summary>
        public int TotalLogs { get; set; }

        /// <summary>
        /// تعداد اقدامات مختلف
        /// </summary>
        public Dictionary<string, int> ActionCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// اولین لاگ
        /// </summary>
        public DateTime? FirstLogDate { get; set; }

        /// <summary>
        /// آخرین لاگ
        /// </summary>
        public DateTime? LastLogDate { get; set; }

        /// <summary>
        /// تعداد کاربران مختلف
        /// </summary>
        public int UniqueUserCount { get; set; }

        /// <summary>
        /// لیست کاربران
        /// </summary>
        public List<string> UserNames { get; set; } = new List<string>();
    }
}
