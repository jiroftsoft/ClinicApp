using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Models.Statistics;

namespace ClinicApp.Interfaces.Payment.Gateway
{
    /// <summary>
    /// Service Interface برای مدیریت درگاه‌های پرداخت
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت منطق کسب‌وکار درگاه‌های پرداخت
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل درگاه‌های پرداخت
    /// 2. یکپارچه‌سازی با درگاه‌های مختلف (ZarinPal, PayPing, etc.)
    /// 3. مدیریت تنظیمات و پیکربندی
    /// 4. تست اتصال و سلامت درگاه‌ها
    /// 5. بهینه‌سازی برای عملکرد بالا
    /// </summary>
    public interface IPaymentGatewayService
    {
        #region Gateway Management

        /// <summary>
        /// ایجاد درگاه پرداخت جدید
        /// </summary>
        /// <param name="request">درخواست ایجاد درگاه</param>
        /// <returns>نتیجه ایجاد</returns>
        Task<ServiceResult<PaymentGateway>> CreatePaymentGatewayAsync(CreatePaymentGatewayRequest request);

        /// <summary>
        /// ایجاد درگاه پرداخت (برای سازگاری با Controller)
        /// </summary>
        /// <param name="gateway">درگاه پرداخت</param>
        /// <returns>نتیجه ایجاد</returns>
        Task<ServiceResult<PaymentGateway>> CreateGatewayAsync(PaymentGateway gateway);

        /// <summary>
        /// به‌روزرسانی درگاه پرداخت
        /// </summary>
        /// <param name="request">درخواست به‌روزرسانی</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult<PaymentGateway>> UpdatePaymentGatewayAsync(UpdatePaymentGatewayRequest request);

        /// <summary>
        /// به‌روزرسانی درگاه پرداخت (برای سازگاری با Controller)
        /// </summary>
        /// <param name="gateway">درگاه پرداخت</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult<PaymentGateway>> UpdateGatewayAsync(PaymentGateway gateway);

        /// <summary>
        /// فعال/غیرفعال کردن درگاه پرداخت
        /// </summary>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <param name="isActive">وضعیت فعال</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult> TogglePaymentGatewayStatusAsync(int gatewayId, bool isActive, string userId);

        /// <summary>
        /// دریافت درگاه پرداخت
        /// </summary>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <returns>درگاه پرداخت</returns>
        Task<ServiceResult<PaymentGateway>> GetPaymentGatewayAsync(int gatewayId);

        /// <summary>
        /// دریافت درگاه پرداخت (برای سازگاری با Controller)
        /// </summary>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <returns>درگاه پرداخت</returns>
        Task<ServiceResult<PaymentGateway>> GetGatewayByIdAsync(int gatewayId);

        /// <summary>
        /// دریافت درگاه‌های فعال
        /// </summary>
        /// <returns>لیست درگاه‌های فعال</returns>
        Task<ServiceResult<IEnumerable<PaymentGateway>>> GetActivePaymentGatewaysAsync();

        /// <summary>
        /// دریافت درگاه‌ها (برای سازگاری با Controller)
        /// </summary>
        /// <returns>لیست درگاه‌ها</returns>
        Task<ServiceResult<IEnumerable<PaymentGateway>>> GetGatewaysAsync();
        Task<ServiceResult<PagedResult<PaymentGateway>>> GetGatewaysAsync(string? name, PaymentGatewayType? type, bool? isActive, bool? isDefault, string? createdByUserId, DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 50);

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
        /// حذف درگاه پرداخت (برای سازگاری با Controller)
        /// </summary>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <returns>نتیجه حذف</returns>
        Task<ServiceResult> DeleteGatewayAsync(int gatewayId);
        Task<ServiceResult> DeleteGatewayAsync(int gatewayId, string userId);

        #endregion

        #region Gateway Operations

        /// <summary>
        /// ایجاد درخواست پرداخت در درگاه
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="request">درخواست پرداخت</param>
        /// <returns>نتیجه ایجاد درخواست</returns>
        Task<ServiceResult<GatewayPaymentResponse>> CreatePaymentRequestAsync(PaymentGatewayType gatewayType, GatewayPaymentRequest request);

        /// <summary>
        /// پردازش Callback درگاه
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="callbackData">داده‌های Callback</param>
        /// <returns>نتیجه پردازش</returns>
        Task<ServiceResult<GatewayCallbackResult>> ProcessCallbackAsync(PaymentGatewayType gatewayType, GatewayCallbackData callbackData);

        /// <summary>
        /// پردازش Webhook درگاه
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="webhookData">داده‌های Webhook</param>
        /// <returns>نتیجه پردازش</returns>
        Task<ServiceResult<GatewayWebhookResult>> ProcessWebhookAsync(PaymentGatewayType gatewayType, GatewayWebhookData webhookData);

        /// <summary>
        /// بررسی وضعیت پرداخت در درگاه
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>وضعیت پرداخت</returns>
        Task<ServiceResult<GatewayPaymentStatus>> CheckPaymentStatusAsync(PaymentGatewayType gatewayType, string transactionId);

        /// <summary>
        /// لغو پرداخت در درگاه
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>نتیجه لغو</returns>
        Task<ServiceResult> CancelPaymentAsync(PaymentGatewayType gatewayType, string transactionId);

        /// <summary>
        /// برگشت پرداخت در درگاه
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <param name="refundAmount">مبلغ برگشت</param>
        /// <param name="reason">دلیل برگشت</param>
        /// <returns>نتیجه برگشت</returns>
        Task<ServiceResult<GatewayRefundResult>> RefundPaymentAsync(PaymentGatewayType gatewayType, string transactionId, decimal refundAmount, string reason);

        #endregion

        #region Gateway Testing

        /// <summary>
        /// تست اتصال درگاه پرداخت
        /// </summary>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <returns>نتیجه تست</returns>
        Task<ServiceResult<GatewayConnectionTest>> TestGatewayConnectionAsync(int gatewayId);

        /// <summary>
        /// تست اتصال درگاه بر اساس نوع
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <returns>نتیجه تست</returns>
        Task<ServiceResult<GatewayConnectionTest>> TestGatewayConnectionByTypeAsync(PaymentGatewayType gatewayType);

        /// <summary>
        /// تست پرداخت آزمایشی
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="testAmount">مبلغ آزمایشی</param>
        /// <returns>نتیجه تست</returns>
        Task<ServiceResult<GatewayTestPaymentResult>> TestPaymentAsync(PaymentGatewayType gatewayType, decimal testAmount);

        #endregion

        #region Gateway Configuration

        /// <summary>
        /// دریافت تنظیمات درگاه
        /// </summary>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <returns>تنظیمات درگاه</returns>
        Task<ServiceResult<GatewayConfiguration>> GetGatewayConfigurationAsync(int gatewayId);

        /// <summary>
        /// به‌روزرسانی تنظیمات درگاه
        /// </summary>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <param name="configuration">تنظیمات جدید</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult> UpdateGatewayConfigurationAsync(int gatewayId, GatewayConfiguration configuration, string userId);

        /// <summary>
        /// اعتبارسنجی تنظیمات درگاه
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="configuration">تنظیمات</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateGatewayConfigurationAsync(PaymentGatewayType gatewayType, GatewayConfiguration configuration);

        #endregion

        #region Gateway Statistics

        /// <summary>
        /// دریافت آمار درگاه‌های پرداخت
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار درگاه‌ها</returns>
        Task<ServiceResult<PaymentGatewayStatistics>> GetPaymentGatewayStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت آمار درگاه خاص
        /// </summary>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار درگاه</returns>
        Task<ServiceResult<SingleGatewayStatistics>> GetSingleGatewayStatisticsAsync(int gatewayId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت آمار روزانه درگاه‌ها
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار روزانه</returns>
        Task<ServiceResult<DailyGatewayStatistics>> GetDailyGatewayStatisticsAsync(DateTime date);

        /// <summary>
        /// دریافت آمار درگاه‌ها (برای سازگاری با Controller)
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار درگاه‌ها</returns>
        Task<ServiceResult<PaymentGatewayStatistics>> GetGatewayStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<ServiceResult<PaymentGatewayStatistics>> GetGatewayStatisticsAsync();

        #endregion

        #region Online Payment Management

        /// <summary>
        /// دریافت پرداخت آنلاین
        /// </summary>
        /// <param name="onlinePaymentId">شناسه پرداخت آنلاین</param>
        /// <returns>پرداخت آنلاین</returns>
        Task<ServiceResult<OnlinePayment>> GetOnlinePaymentByIdAsync(int onlinePaymentId);

        /// <summary>
        /// دریافت پرداخت‌های آنلاین
        /// </summary>
        /// <returns>لیست پرداخت‌های آنلاین</returns>
        Task<ServiceResult<IEnumerable<OnlinePayment>>> GetOnlinePaymentsAsync();
        Task<ServiceResult<PagedResult<OnlinePayment>>> GetOnlinePaymentsAsync(int? patientId, int? receptionId, int? appointmentId, int? paymentGatewayId, OnlinePaymentType? paymentType, OnlinePaymentStatus? status, decimal? minAmount, decimal? maxAmount, DateTime? startDate, DateTime? endDate, string? patientName, string? doctorName, string? transactionId, string? referenceCode, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// ایجاد پرداخت آنلاین
        /// </summary>
        /// <param name="onlinePayment">پرداخت آنلاین</param>
        /// <returns>نتیجه ایجاد</returns>
        Task<ServiceResult<OnlinePayment>> CreateOnlinePaymentAsync(OnlinePayment onlinePayment);

        #endregion
    }

    #region Request/Response Models

    /// <summary>
    /// درخواست ایجاد درگاه پرداخت
    /// </summary>
    public class CreatePaymentGatewayRequest
    {
        public string Name { get; set; }
        public PaymentGatewayType GatewayType { get; set; }
        public string MerchantId { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string CallbackUrl { get; set; }
        public string WebhookUrl { get; set; }
        public decimal FeePercentage { get; set; }
        public decimal FixedFee { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public string Description { get; set; }
        public string CreatedByUserId { get; set; }
    }

    /// <summary>
    /// درخواست به‌روزرسانی درگاه پرداخت
    /// </summary>
    public class UpdatePaymentGatewayRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PaymentGatewayType GatewayType { get; set; }
        public string MerchantId { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string CallbackUrl { get; set; }
        public string WebhookUrl { get; set; }
        public decimal FeePercentage { get; set; }
        public decimal FixedFee { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public string Description { get; set; }
        public string UpdatedByUserId { get; set; }
    }

    /// <summary>
    /// درخواست پرداخت درگاه
    /// </summary>
    public class GatewayPaymentRequest
    {
        public string PaymentToken { get; set; }
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
    public class GatewayPaymentResponse
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
    /// داده‌های Callback درگاه
    /// </summary>
    public class GatewayCallbackData
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
    /// داده‌های Webhook درگاه
    /// </summary>
    public class GatewayWebhookData
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
    /// نتیجه Callback درگاه
    /// </summary>
    public class GatewayCallbackResult
    {
        public bool Success { get; set; }
        public string PaymentToken { get; set; }
        public OnlinePaymentStatus Status { get; set; }
        public string GatewayTransactionId { get; set; }
        public string ErrorMessage { get; set; }
        public int? PaymentTransactionId { get; set; }
    }

    /// <summary>
    /// نتیجه Webhook درگاه
    /// </summary>
    public class GatewayWebhookResult
    {
        public bool Success { get; set; }
        public string PaymentToken { get; set; }
        public OnlinePaymentStatus Status { get; set; }
        public string GatewayTransactionId { get; set; }
        public string ErrorMessage { get; set; }
        public string WebhookId { get; set; }
    }

    /// <summary>
    /// وضعیت پرداخت درگاه
    /// </summary>
    public class GatewayPaymentStatus
    {
        public string TransactionId { get; set; }
        public OnlinePaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime LastChecked { get; set; }
    }

    /// <summary>
    /// نتیجه برگشت درگاه
    /// </summary>
    public class GatewayRefundResult
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
        public PaymentGatewayType GatewayType { get; set; }
        public bool IsConnected { get; set; }
        public int ResponseTime { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime TestedAt { get; set; }
    }

    /// <summary>
    /// نتیجه تست پرداخت
    /// </summary>
    public class GatewayTestPaymentResult
    {
        public bool Success { get; set; }
        public string TestTransactionId { get; set; }
        public string TestPaymentUrl { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime TestedAt { get; set; }
    }

    /// <summary>
    /// تنظیمات درگاه
    /// </summary>
    public class GatewayConfiguration
    {
        public string MerchantId { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string CallbackUrl { get; set; }
        public string WebhookUrl { get; set; }
        public decimal FeePercentage { get; set; }
        public decimal FixedFee { get; set; }
        public Dictionary<string, string> AdditionalSettings { get; set; }
    }

    /// <summary>
    /// آمار درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewayStatistics
    {
        public int TotalGateways { get; set; }
        public int ActiveGateways { get; set; }
        public int InactiveGateways { get; set; }
        public int TotalOnlinePayments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessRate { get; set; }
        public int SuccessfulPayments { get; set; }
        public decimal SuccessfulAmount { get; set; }
        public int PendingPayments { get; set; }
        public decimal PendingAmount { get; set; }
        public Dictionary<OnlinePaymentType, int> PaymentsByType { get; set; }
        public Dictionary<OnlinePaymentStatus, int> PaymentsByStatus { get; set; }
        public int FailedPayments { get; set; }
        public decimal FailedAmount { get; set; }
        public int DefaultGateways { get; set; }
        public decimal AverageResponseTime { get; set; }
        public Dictionary<PaymentGatewayType, int> GatewaysByType { get; set; }
        public Dictionary<PaymentGatewayType, decimal> AmountByGateway { get; set; }
        public Dictionary<PaymentGatewayType, int> TransactionCountByGateway { get; set; }
        public Dictionary<PaymentGatewayType, decimal> SuccessRateByGateway { get; set; }
    }

    /// <summary>
    /// آمار درگاه خاص
    /// </summary>
    public class SingleGatewayStatistics
    {
        public int GatewayId { get; set; }
        public string GatewayName { get; set; }
        public PaymentGatewayType GatewayType { get; set; }
        public int TotalTransactions { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessfulAmount { get; set; }
        public decimal FailedAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageTransactionAmount { get; set; }
        public DateTime LastTransactionDate { get; set; }
    }

    /// <summary>
    /// آمار روزانه درگاه‌ها
    /// </summary>
    public class DailyGatewayStatistics
    {
        public DateTime Date { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public Dictionary<PaymentGatewayType, int> TransactionsByGateway { get; set; }
        public Dictionary<PaymentGatewayType, decimal> AmountByGateway { get; set; }
        public Dictionary<PaymentGatewayType, decimal> SuccessRateByGateway { get; set; }
    }

    #endregion
}
