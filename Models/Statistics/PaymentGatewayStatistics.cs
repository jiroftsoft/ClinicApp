using System;
using System.Collections.Generic;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.Statistics
{
    /// <summary>
    /// آمار درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewayStatistics
    {
        /// <summary>
        /// تعداد کل درگاه‌ها
        /// </summary>
        public int TotalGateways { get; set; }

        /// <summary>
        /// تعداد درگاه‌های فعال
        /// </summary>
        public int ActiveGateways { get; set; }

        /// <summary>
        /// تعداد درگاه‌های غیرفعال
        /// </summary>
        public int InactiveGateways { get; set; }

        /// <summary>
        /// تعداد درگاه‌های پیش‌فرض
        /// </summary>
        public int DefaultGateways { get; set; }

        /// <summary>
        /// تعداد کل پرداخت‌های آنلاین
        /// </summary>
        public int TotalOnlinePayments { get; set; }

        /// <summary>
        /// تعداد پرداخت‌های موفق
        /// </summary>
        public int SuccessfulPayments { get; set; }

        /// <summary>
        /// تعداد پرداخت‌های ناموفق
        /// </summary>
        public int FailedPayments { get; set; }

        /// <summary>
        /// تعداد پرداخت‌های در انتظار
        /// </summary>
        public int PendingPayments { get; set; }

        /// <summary>
        /// مجموع کل مبالغ
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// مجموع مبالغ موفق
        /// </summary>
        public decimal SuccessfulAmount { get; set; }

        /// <summary>
        /// مجموع مبالغ ناموفق
        /// </summary>
        public decimal FailedAmount { get; set; }

        /// <summary>
        /// مجموع مبالغ در انتظار
        /// </summary>
        public decimal PendingAmount { get; set; }

        /// <summary>
        /// نرخ موفقیت (درصد)
        /// </summary>
        public decimal SuccessRate { get; set; }

        /// <summary>
        /// میانگین زمان پاسخ (میلی‌ثانیه)
        /// </summary>
        public decimal AverageResponseTime { get; set; }

        /// <summary>
        /// پرداخت‌ها بر اساس نوع
        /// </summary>
        public Dictionary<OnlinePaymentType, int> PaymentsByType { get; set; }

        /// <summary>
        /// پرداخت‌ها بر اساس وضعیت
        /// </summary>
        public Dictionary<OnlinePaymentStatus, int> PaymentsByStatus { get; set; }

        /// <summary>
        /// تاریخ شروع دوره
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان دوره
        /// </summary>
        public DateTime EndDate { get; set; }

        public PaymentGatewayStatistics()
        {
            PaymentsByType = new Dictionary<OnlinePaymentType, int>();
            PaymentsByStatus = new Dictionary<OnlinePaymentStatus, int>();
        }
    }
}
