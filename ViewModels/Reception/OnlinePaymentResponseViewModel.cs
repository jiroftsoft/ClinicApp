using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای پاسخ پرداخت آنلاین
    /// </summary>
    public class OnlinePaymentResponseViewModel
    {
        /// <summary>
        /// شناسه تراکنش درگاه
        /// </summary>
        public string GatewayTransactionId { get; set; }

        /// <summary>
        /// شناسه تراکنش
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// URL پرداخت
        /// </summary>
        public string PaymentUrl { get; set; }

        /// <summary>
        /// URL بازگشت
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// شناسه مرجع درگاه
        /// </summary>
        public string PaymentGatewayRefId { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ انقضا
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// وضعیت
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// آیا موفق است
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// پیام
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// کد تأیید
        /// </summary>
        public string VerificationCode { get; set; }

        /// <summary>
        /// URL بازگشت
        /// </summary>
        public string CallbackUrl { get; set; }

        /// <summary>
        /// پاسخ درگاه
        /// </summary>
        public string GatewayResponse { get; set; }
    }
}
