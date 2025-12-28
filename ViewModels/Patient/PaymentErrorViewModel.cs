using System;

namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// ViewModel برای صفحه خطای پرداخت
    /// </summary>
    public class PaymentErrorViewModel
    {
        /// <summary>
        /// پیام خطا
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// شناسه نوبت
        /// </summary>
        public int? AppointmentId { get; set; }

        /// <summary>
        /// شناسه پرداخت آنلاین
        /// </summary>
        public int? OnlinePaymentId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        public string DoctorName { get; set; }

        /// <summary>
        /// تاریخ و زمان نوبت
        /// </summary>
        public DateTime? AppointmentDate { get; set; }
    }
}

