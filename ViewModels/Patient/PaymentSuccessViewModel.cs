using System;

namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// ViewModel برای صفحه موفقیت پرداخت
    /// </summary>
    public class PaymentSuccessViewModel
    {
        /// <summary>
        /// شناسه نوبت
        /// </summary>
        public int? AppointmentId { get; set; }

        /// <summary>
        /// شناسه پرداخت آنلاین
        /// </summary>
        public int? OnlinePaymentId { get; set; }

        /// <summary>
        /// شماره مرجع از درگاه (RefId)
        /// </summary>
        public string RefId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        public string DoctorName { get; set; }

        /// <summary>
        /// تاریخ و زمان نوبت
        /// </summary>
        public DateTime? AppointmentDate { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        public string PatientName { get; set; }
    }
}

