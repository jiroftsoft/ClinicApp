using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.Payment.Validation
{
    /// <summary>
    /// Service Interface برای اعتبارسنجی پرداخت‌ها
    /// طراحی شده طبق اصول SRP - مسئولیت: اعتبارسنجی منطق کسب‌وکار پرداخت‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل پرداخت‌ها
    /// 2. اعتبارسنجی درگاه‌ها و ترمینال‌ها
    /// 3. اعتبارسنجی جلسات نقدی
    /// 4. اعتبارسنجی تراکنش‌ها
    /// 5. بهینه‌سازی برای عملکرد بالا
    /// </summary>
    public interface IPaymentValidationService
    {
        #region Payment Validation

        /// <summary>
        /// اعتبارسنجی درخواست پرداخت نقدی
        /// </summary>
        /// <param name="request">درخواست پرداخت نقدی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateCashPaymentRequestAsync(CashPaymentValidationRequest request);

        /// <summary>
        /// اعتبارسنجی درخواست پرداخت POS
        /// </summary>
        /// <param name="request">درخواست پرداخت POS</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePosPaymentRequestAsync(PosPaymentValidationRequest request);

        /// <summary>
        /// اعتبارسنجی درخواست پرداخت آنلاین
        /// </summary>
        /// <param name="request">درخواست پرداخت آنلاین</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateOnlinePaymentRequestAsync(OnlinePaymentValidationRequest request);

        /// <summary>
        /// اعتبارسنجی تراکنش پرداخت
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePaymentTransactionAsync(int transactionId);

        /// <summary>
        /// اعتبارسنجی امکان پرداخت
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="paymentMethod">روش پرداخت</param>
        /// <param name="amount">مبلغ</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePaymentPossibilityAsync(int receptionId, PaymentMethod paymentMethod, decimal amount);

        #endregion

        #region Gateway Validation

        /// <summary>
        /// اعتبارسنجی درگاه پرداخت
        /// </summary>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePaymentGatewayAsync(int gatewayId);

        /// <summary>
        /// اعتبارسنجی درگاه بر اساس نوع
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePaymentGatewayByTypeAsync(PaymentGatewayType gatewayType);

        /// <summary>
        /// اعتبارسنجی تنظیمات درگاه
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="configuration">تنظیمات</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateGatewayConfigurationAsync(PaymentGatewayType gatewayType, GatewayConfigurationValidation configuration);

        /// <summary>
        /// اعتبارسنجی Callback درگاه
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="callbackData">داده‌های Callback</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateGatewayCallbackAsync(PaymentGatewayType gatewayType, GatewayCallbackValidationData callbackData);

        /// <summary>
        /// اعتبارسنجی Webhook درگاه
        /// </summary>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <param name="webhookData">داده‌های Webhook</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateGatewayWebhookAsync(PaymentGatewayType gatewayType, GatewayWebhookValidationData webhookData);

        #endregion

        #region POS Terminal Validation

        /// <summary>
        /// اعتبارسنجی ترمینال POS
        /// </summary>
        /// <param name="terminalId">شناسه ترمینال</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePosTerminalAsync(int terminalId);

        /// <summary>
        /// اعتبارسنجی امکان استفاده از ترمینال
        /// </summary>
        /// <param name="terminalId">شناسه ترمینال</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePosTerminalUsageAsync(int terminalId, string userId);

        /// <summary>
        /// اعتبارسنجی تنظیمات ترمینال
        /// </summary>
        /// <param name="terminalId">شناسه ترمینال</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePosTerminalConfigurationAsync(int terminalId);

        #endregion

        #region Cash Session Validation

        /// <summary>
        /// اعتبارسنجی جلسه نقدی
        /// </summary>
        /// <param name="sessionId">شناسه جلسه</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateCashSessionAsync(int sessionId);

        /// <summary>
        /// اعتبارسنجی امکان شروع جلسه نقدی
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateCashSessionStartAsync(string userId);

        /// <summary>
        /// اعتبارسنجی امکان پایان جلسه نقدی
        /// </summary>
        /// <param name="sessionId">شناسه جلسه</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateCashSessionEndAsync(int sessionId, string userId);

        /// <summary>
        /// اعتبارسنجی موجودی نقدی
        /// </summary>
        /// <param name="sessionId">شناسه جلسه</param>
        /// <param name="amount">مبلغ</param>
        /// <param name="operation">عملیات (اضافه/کسر)</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateCashBalanceAsync(int sessionId, decimal amount, CashBalanceOperation operation);

        #endregion

        #region Business Rules Validation

        /// <summary>
        /// اعتبارسنجی قوانین کسب‌وکار پرداخت
        /// </summary>
        /// <param name="request">درخواست اعتبارسنجی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateBusinessRulesAsync(PaymentBusinessRulesValidationRequest request);

        /// <summary>
        /// اعتبارسنجی محدودیت‌های پرداخت
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="amount">مبلغ</param>
        /// <param name="paymentMethod">روش پرداخت</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePaymentLimitsAsync(string userId, decimal amount, PaymentMethod paymentMethod);

        /// <summary>
        /// اعتبارسنجی مجوزهای پرداخت
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="paymentMethod">روش پرداخت</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePaymentPermissionsAsync(string userId, PaymentMethod paymentMethod);

        /// <summary>
        /// اعتبارسنجی زمان پرداخت
        /// </summary>
        /// <param name="paymentMethod">روش پرداخت</param>
        /// <param name="requestTime">زمان درخواست</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePaymentTimeAsync(PaymentMethod paymentMethod, DateTime requestTime);

        #endregion

        #region Security Validation

        /// <summary>
        /// اعتبارسنجی امنیتی پرداخت
        /// </summary>
        /// <param name="request">درخواست اعتبارسنجی امنیتی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePaymentSecurityAsync(PaymentSecurityValidationRequest request);

        /// <summary>
        /// اعتبارسنجی امضای دیجیتال
        /// </summary>
        /// <param name="data">داده‌ها</param>
        /// <param name="signature">امضا</param>
        /// <param name="gatewayType">نوع درگاه</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateDigitalSignatureAsync(string data, string signature, PaymentGatewayType gatewayType);

        /// <summary>
        /// اعتبارسنجی IP کاربر
        /// </summary>
        /// <param name="userIpAddress">آدرس IP</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateUserIpAddressAsync(string userIpAddress, string userId);

        /// <summary>
        /// اعتبارسنجی نرخ درخواست
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="operation">عملیات</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateRequestRateAsync(string userId, string operation);

        #endregion
    }

    #region Request/Response Models

    /// <summary>
    /// درخواست اعتبارسنجی پرداخت نقدی
    /// </summary>
    public class CashPaymentValidationRequest
    {
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string CreatedByUserId { get; set; }
        public int CashSessionId { get; set; }
    }

    /// <summary>
    /// درخواست اعتبارسنجی پرداخت POS
    /// </summary>
    public class PosPaymentValidationRequest
    {
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public int PosTerminalId { get; set; }
        public string TransactionId { get; set; }
        public string ReferenceCode { get; set; }
        public string ReceiptNo { get; set; }
        public string Description { get; set; }
        public string CreatedByUserId { get; set; }
        public int CashSessionId { get; set; }
    }

    /// <summary>
    /// درخواست اعتبارسنجی پرداخت آنلاین
    /// </summary>
    public class OnlinePaymentValidationRequest
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
    }

    /// <summary>
    /// تنظیمات درگاه برای اعتبارسنجی
    /// </summary>
    public class GatewayConfigurationValidation
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
    /// داده‌های Callback برای اعتبارسنجی
    /// </summary>
    public class GatewayCallbackValidationData
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
    /// داده‌های Webhook برای اعتبارسنجی
    /// </summary>
    public class GatewayWebhookValidationData
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
    /// عملیات موجودی نقدی
    /// </summary>
    public enum CashBalanceOperation
    {
        Add,
        Subtract
    }

    /// <summary>
    /// درخواست اعتبارسنجی قوانین کسب‌وکار
    /// </summary>
    public class PaymentBusinessRulesValidationRequest
    {
        public int ReceptionId { get; set; }
        public int PatientId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string UserId { get; set; }
        public DateTime RequestTime { get; set; }
        public string UserIpAddress { get; set; }
    }

    /// <summary>
    /// درخواست اعتبارسنجی امنیتی
    /// </summary>
    public class PaymentSecurityValidationRequest
    {
        public string UserId { get; set; }
        public string UserIpAddress { get; set; }
        public string UserAgent { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public DateTime RequestTime { get; set; }
        public string SessionId { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }

    #endregion
}
