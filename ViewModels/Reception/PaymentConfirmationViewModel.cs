using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای تایید پرداخت
    /// </summary>
    public class PaymentConfirmationViewModel
    {
        /// <summary>
        /// شناسه تراکنش
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// وضعیت
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// پیام
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// مبلغ
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// مبلغ پرداخت شده
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// کد مرجع
        /// </summary>
        public string ReferenceCode { get; set; }

        /// <summary>
        /// کد پیگیری
        /// </summary>
        public string TrackingCode { get; set; }

        /// <summary>
        /// تاریخ پرداخت
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// تاریخ تأیید
        /// </summary>
        public DateTime ConfirmationDate { get; set; }

        /// <summary>
        /// آیا تأیید شده است
        /// </summary>
        public bool IsConfirmed { get; set; }
    }
}