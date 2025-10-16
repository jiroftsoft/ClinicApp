using System;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// آمار سایدبار
    /// </summary>
    public class SidebarStatistics
    {
        public int TodayReceptions { get; set; }
        public int PendingReceptions { get; set; }
        public int CompletedReceptions { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// هشدار پزشکی
    /// </summary>
    public class MedicalAlert
    {
        public int Id { get; set; }
        public string Type { get; set; } // emergency, warning, info
        public string Title { get; set; }
        public string Message { get; set; }
        public string Priority { get; set; } // high, medium, low
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// وضعیت بیمه‌ها
    /// </summary>
    public class InsuranceStatus
    {
        public int ActiveInsurances { get; set; }
        public int ExpiredInsurances { get; set; }
        public int PendingValidations { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// وضعیت پرداخت‌ها
    /// </summary>
    public class PaymentStatus
    {
        public int TodayPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public int PendingPayments { get; set; }
        public DateTime LastUpdated { get; set; }
    }

}
