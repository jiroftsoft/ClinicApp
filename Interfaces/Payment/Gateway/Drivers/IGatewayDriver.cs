using ClinicApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces.Payment.Gateway.Drivers
{
    /// <summary>
    /// Interface برای درایورهای درگاه پرداخت
    /// هر درایور مسئول ارتباط با یک درگاه پرداخت خاص است (مثلاً زرین‌پال، پی‌پینگ و ...)
    /// 
    /// طراحی شده طبق اصول:
    /// - SRP: هر درایور فقط مسئول یک درگاه است
    /// - Open/Closed: می‌توان درایورهای جدید اضافه کرد بدون تغییر کد موجود
    /// - Dependency Inversion: وابستگی به Interface است نه Implementation
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public interface IGatewayDriver
    {
        /// <summary>
        /// ایجاد درخواست پرداخت در درگاه
        /// </summary>
        /// <param name="request">درخواست پرداخت</param>
        /// <returns>نتیجه درخواست پرداخت</returns>
        Task<ServiceResult<PaymentRequestResult>> RequestPaymentAsync(PaymentRequest request);

        /// <summary>
        /// تأیید پرداخت پس از بازگشت از درگاه
        /// </summary>
        /// <param name="request">درخواست تأیید پرداخت</param>
        /// <returns>نتیجه تأیید پرداخت</returns>
        Task<ServiceResult<PaymentVerificationResult>> VerifyPaymentAsync(PaymentVerificationRequest request);

        /// <summary>
        /// بررسی وضعیت یک تراکنش در درگاه
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <param name="amount">مبلغ</param>
        /// <returns>نتیجه بررسی وضعیت</returns>
        Task<ServiceResult<PaymentStatusResult>> CheckPaymentStatusAsync(string transactionId, decimal amount);

        /// <summary>
        /// برگشت وجه (Refund)
        /// </summary>
        /// <param name="request">درخواست برگشت وجه</param>
        /// <returns>نتیجه برگشت وجه</returns>
        Task<ServiceResult<RefundResult>> RefundPaymentAsync(RefundRequest request);
    }

    #region Request/Response Models

    /// <summary>
    /// درخواست پرداخت
    /// </summary>
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string CallbackUrl { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Metadata { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
        /// <summary>
        /// CorrelationId برای Tracing در لاگ‌ها
        /// </summary>
        public string CorrelationId { get; set; }
    }

    /// <summary>
    /// نتیجه درخواست پرداخت
    /// </summary>
    public class PaymentRequestResult
    {
        public bool Success { get; set; }
        public string Authority { get; set; }
        public string PaymentToken { get; set; }
        public string PaymentUrl { get; set; }
        public string GatewayTransactionId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }

    /// <summary>
    /// درخواست تأیید پرداخت
    /// </summary>
    public class PaymentVerificationRequest
    {
        public string Authority { get; set; }
        public decimal Amount { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }

    /// <summary>
    /// نتیجه تأیید پرداخت
    /// </summary>
    public class PaymentVerificationResult
    {
        public bool Success { get; set; }
        public bool IsVerified { get; set; }
        public string Authority { get; set; }
        public string PaymentToken { get; set; }
        public string RefId { get; set; }
        public string GatewayTransactionId { get; set; }
        public string PaymentTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }

    /// <summary>
    /// نتیجه بررسی وضعیت پرداخت
    /// </summary>
    public class PaymentStatusResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }

    /// <summary>
    /// درخواست برگشت وجه
    /// </summary>
    public class RefundRequest
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }

    /// <summary>
    /// نتیجه برگشت وجه
    /// </summary>
    public class RefundResult
    {
        public bool Success { get; set; }
        public string RefundId { get; set; }
        public string GatewayRefundId { get; set; }
        public decimal RefundAmount { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }

    #endregion
}

