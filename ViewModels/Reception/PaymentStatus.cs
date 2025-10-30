using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// کلاس وضعیت پرداخت برای سایدبار پذیرش
    /// </summary>
    public class PaymentStatusInfo
    {
        /// <summary>
        /// تعداد پرداخت‌های امروز
        /// </summary>
        public int TodayPayments { get; set; }

        /// <summary>
        /// مجموع مبلغ پرداخت‌های امروز
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// تعداد پرداخت‌های در انتظار
        /// </summary>
        public int PendingPayments { get; set; }

        /// <summary>
        /// آخرین به‌روزرسانی
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}
