using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.Payment.Web
{
    /// <summary>
    /// Service Interface برای مدیریت پرداخت‌های آنلاین
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت منطق کسب‌وکار پرداخت‌های وب
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل پرداخت‌های آنلاین
    /// 2. یکپارچه‌سازی با درگاه‌های پرداخت
    /// 3. مدیریت Callback ها و Webhook ها
    /// 4. پردازش پرداخت‌های غیرهمزمان
    /// 5. بهینه‌سازی برای عملکرد بالا
    /// </summary>
    public interface IWebPaymentService
    {
        #region Payment Gateway Integration

        /// <summary>
        /// ایجاد درخواست پرداخت در درگاه
        /// </summary>
        /// <param name="request">درخواست پرداخت</param>
        /// <returns>نتیجه ایجاد درخواست</returns>
        Task<ServiceResult<PaymentGatewayResponse>> CreatePaymentRequestAsync(CreatePaymentRequest request);

        /// <summary>
        /// پردازش Callback درگاه پرداخت
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="callbackData">داده‌های Callback</param>
        /// <returns>نتیجه پردازش</returns>
        Task<ServiceResult<PaymentCallbackResult>> ProcessPaymentCallbackAsync(PaymentGatewayType gatewayType, PaymentCallbackData callbackData);

        /// <summary>
        /// پردازش Webhook درگاه پرداخت
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="webhookData">داده‌های Webhook</param>
        /// <returns>نتیجه پردازش</returns>
        Task<ServiceResult<PaymentWebhookResult>> ProcessPaymentWebhookAsync(PaymentGatewayType gatewayType, PaymentWebhookData webhookData);

        /// <summary>
        /// بررسی وضعیت پرداخت در درگاه
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>وضعیت پرداخت</returns>
        Task<ServiceResult<PaymentStatus>> CheckPaymentStatusAsync(PaymentGatewayType gatewayType, string transactionId);

        /// <summary>
        /// لغو پرداخت در درگاه
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>نتیجه لغو</returns>
        Task<ServiceResult> CancelPaymentInGatewayAsync(PaymentGatewayType gatewayType, string transactionId);

        #endregion

        #region Payment Processing

        /// <summary>
        /// پردازش پرداخت آنلاین
        /// </summary>
        /// <param name="request">درخواست پرداخت</param>
        /// <returns>نتیجه پردازش</returns>
        Task<ServiceResult<WebPaymentResult>> ProcessWebPaymentAsync(WebPaymentRequest request);

        /// <summary>
        /// تکمیل پرداخت آنلاین
        /// </summary>
        /// <param name="paymentToken">توکن پرداخت</param>
        /// <param name="callbackData">داده‌های Callback</param>
        /// <returns>نتیجه تکمیل</returns>
        Task<ServiceResult<WebPaymentResult>> CompleteWebPaymentAsync(string paymentToken, PaymentCallbackData callbackData);

        /// <summary>
        /// لغو پرداخت آنلاین
        /// </summary>
        /// <param name="paymentToken">توکن پرداخت</param>
        /// <param name="reason">دلیل لغو</param>
        /// <returns>نتیجه لغو</returns>
        Task<ServiceResult> CancelWebPaymentAsync(string paymentToken, string reason);

        /// <summary>
        /// برگشت پرداخت آنلاین
        /// </summary>
        /// <param name="paymentToken">توکن پرداخت</param>
        /// <param name="refundAmount">مبلغ برگشت</param>
        /// <param name="reason">دلیل برگشت</param>
        /// <returns>نتیجه برگشت</returns>
        Task<ServiceResult<WebRefundResult>> RefundWebPaymentAsync(string paymentToken, decimal refundAmount, string reason);

        #endregion

        #region Payment Gateway Management

        /// <summary>
        /// دریافت درگاه‌های فعال
        /// </summary>
        /// <returns>لیست درگاه‌های فعال</returns>
        Task<ServiceResult<IEnumerable<PaymentGateway>>> GetActivePaymentGatewaysAsync();

        /// <summary>
        /// دریافت درگاه پیش‌فرض
        /// </summary>
        /// <returns>درگاه پیش‌فرض</returns>
        Task<ServiceResult<PaymentGateway>> GetDefaultPaymentGatewayAsync();

        /// <summary>
        /// تنظیم درگاه پیش‌فرض
        /// </summary>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه تنظیم</returns>
        Task<ServiceResult> SetDefaultPaymentGatewayAsync(int gatewayId, string userId);

        /// <summary>
        /// تست اتصال درگاه پرداخت
        /// </summary>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <returns>نتیجه تست</returns>
        Task<ServiceResult<GatewayConnectionTest>> TestGatewayConnectionAsync(int gatewayId);

        #endregion

        #region Payment Validation

        /// <summary>
        /// اعتبارسنجی درخواست پرداخت
        /// </summary>
        /// <param name="request">درخواست پرداخت</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePaymentRequestAsync(WebPaymentRequest request);

        /// <summary>
        /// اعتبارسنجی Callback
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="callbackData">داده‌های Callback</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePaymentCallbackAsync(PaymentGatewayType gatewayType, PaymentCallbackData callbackData);

        /// <summary>
        /// اعتبارسنجی Webhook
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="webhookData">داده‌های Webhook</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePaymentWebhookAsync(PaymentGatewayType gatewayType, PaymentWebhookData webhookData);

        #endregion

        #region Payment Statistics

        /// <summary>
        /// دریافت آمار پرداخت‌های آنلاین
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار پرداخت‌های آنلاین</returns>
        Task<ServiceResult<WebPaymentStatistics>> GetWebPaymentStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت آمار درگاه‌های پرداخت
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار درگاه‌ها</returns>
        Task<ServiceResult<PaymentGatewayStatistics>> GetPaymentGatewayStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت آمار روزانه پرداخت‌های آنلاین
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار روزانه</returns>
        Task<ServiceResult<DailyWebPaymentStatistics>> GetDailyWebPaymentStatisticsAsync(DateTime date);

        #endregion
    }

    #region Request/Response Models

    /// <summary>
    /// درخواست ایجاد پرداخت
    /// </summary>
    public class CreatePaymentRequest
    {
        public int OnlinePaymentId { get; set; }
        public PaymentGatewayType GatewayType { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string CallbackUrl { get; set; }
        public string UserIpAddress { get; set; }
        public string UserAgent { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }

    /// <summary>
    /// پاسخ درگاه پرداخت
    /// </summary>
    public class PaymentGatewayResponse
    {
        public bool Success { get; set; }
        public string GatewayTransactionId { get; set; }
        public string PaymentUrl { get; set; }
        public string PaymentToken { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }

    /// <summary>
    /// داده‌های Callback پرداخت
    /// </summary>
    public class PaymentCallbackData
    {
        public string TransactionId { get; set; }
        public string ReferenceCode { get; set; }
        public string PaymentToken { get; set; }
        public string Status { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public decimal? Amount { get; set; }
        public string GatewaySignature { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }

    /// <summary>
    /// داده‌های Webhook پرداخت
    /// </summary>
    public class PaymentWebhookData
    {
        public string TransactionId { get; set; }
        public string ReferenceCode { get; set; }
        public string PaymentToken { get; set; }
        public string Status { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public decimal? Amount { get; set; }
        public string GatewaySignature { get; set; }
        public string WebhookId { get; set; }
        public DateTime WebhookTimestamp { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }

    /// <summary>
    /// درخواست پرداخت وب
    /// </summary>
    public class WebPaymentRequest
    {
        public int ReceptionId { get; set; }
        public int? AppointmentId { get; set; }
        public int PatientId { get; set; }
        public OnlinePaymentType PaymentType { get; set; }
        public decimal Amount { get; set; }
        public int PaymentGatewayId { get; set; }
        public string Description { get; set; }
        public string UserIpAddress { get; set; }
        public string UserAgent { get; set; }
        public string CreatedByUserId { get; set; }
        public string CallbackUrl { get; set; }
    }

    /// <summary>
    /// نتیجه پرداخت وب
    /// </summary>
    public class WebPaymentResult
    {
        public int OnlinePaymentId { get; set; }
        public string PaymentToken { get; set; }
        public string PaymentUrl { get; set; }
        public OnlinePaymentStatus Status { get; set; }
        public string GatewayTransactionId { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// نتیجه Callback پرداخت
    /// </summary>
    public class PaymentCallbackResult
    {
        public bool Success { get; set; }
        public string PaymentToken { get; set; }
        public OnlinePaymentStatus Status { get; set; }
        public string GatewayTransactionId { get; set; }
        public string ErrorMessage { get; set; }
        public int? PaymentTransactionId { get; set; }
    }

    /// <summary>
    /// نتیجه Webhook پرداخت
    /// </summary>
    public class PaymentWebhookResult
    {
        public bool Success { get; set; }
        public string PaymentToken { get; set; }
        public OnlinePaymentStatus Status { get; set; }
        public string GatewayTransactionId { get; set; }
        public string ErrorMessage { get; set; }
        public string WebhookId { get; set; }
    }

    /// <summary>
    /// نتیجه برگشت پرداخت وب
    /// </summary>
    public class WebRefundResult
    {
        public bool Success { get; set; }
        public string RefundId { get; set; }
        public decimal RefundAmount { get; set; }
        public string GatewayRefundId { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime RefundedAt { get; set; }
    }

    /// <summary>
    /// تست اتصال درگاه
    /// </summary>
    public class GatewayConnectionTest
    {
        public int GatewayId { get; set; }
        public string GatewayName { get; set; }
        public bool IsConnected { get; set; }
        public int ResponseTime { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime TestedAt { get; set; }
    }

    /// <summary>
    /// آمار پرداخت‌های آنلاین
    /// </summary>
    public class WebPaymentStatistics
    {
        public int TotalPayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public int PendingPayments { get; set; }
        public int CanceledPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessfulAmount { get; set; }
        public decimal FailedAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal CanceledAmount { get; set; }
        public decimal SuccessRate { get; set; }
        public Dictionary<PaymentGatewayType, int> PaymentsByGateway { get; set; }
        public Dictionary<OnlinePaymentType, int> PaymentsByType { get; set; }
        public Dictionary<OnlinePaymentStatus, int> PaymentsByStatus { get; set; }
    }

    /// <summary>
    /// آمار درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewayStatistics
    {
        public int TotalGateways { get; set; }
        public int ActiveGateways { get; set; }
        public int InactiveGateways { get; set; }
        public Dictionary<PaymentGatewayType, int> GatewaysByType { get; set; }
        public Dictionary<PaymentGatewayType, decimal> AmountByGateway { get; set; }
        public Dictionary<PaymentGatewayType, int> TransactionCountByGateway { get; set; }
        public Dictionary<PaymentGatewayType, decimal> SuccessRateByGateway { get; set; }
    }

    /// <summary>
    /// آمار روزانه پرداخت‌های آنلاین
    /// </summary>
    public class DailyWebPaymentStatistics
    {
        public DateTime Date { get; set; }
        public int TotalPayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public int PendingPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessfulAmount { get; set; }
        public decimal FailedAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public Dictionary<PaymentGatewayType, int> PaymentsByGateway { get; set; }
        public Dictionary<OnlinePaymentType, int> PaymentsByType { get; set; }
    }

    #endregion
}
