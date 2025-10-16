using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای پاسخ پرداخت نقدی
    /// </summary>
    public class CashPaymentResponseViewModel
    {
        /// <summary>
        /// آیا موفق بود
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// پیام
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// شناسه تراکنش
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// وضعیت
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// تاریخ تکمیل
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// تاریخ پرداخت
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// مبلغ پرداخت شده
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// شماره رسید
        /// </summary>
        public string ReceiptNumber { get; set; }

        /// <summary>
        /// شناسه صندوقدار
        /// </summary>
        public int CashierId { get; set; }

        /// <summary>
        /// نام صندوقدار
        /// </summary>
        public string CashierName { get; set; }

        /// <summary>
        /// مکان پرداخت
        /// </summary>
        public string PaymentLocation { get; set; }

        /// <summary>
        /// متادیتا
        /// </summary>
        public string Metadata { get; set; }
    }
}
