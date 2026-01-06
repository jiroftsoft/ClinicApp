using ClinicApp.Models;
using System.Threading.Tasks;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces.Payment.Security
{
    /// <summary>
    /// ✅ ENTERPRISE-GRADE: Interface برای اعتبارسنجی امنیتی پرداخت‌ها
    /// 
    /// طبق: PAYMENT_SYSTEM_ENTERPRISE_REDESIGN.md
    /// </summary>
    public interface IPaymentSecurityService
    {
        #region Rate Limiting

        /// <summary>
        /// بررسی Rate Limit برای کاربر
        /// </summary>
        Task<ServiceResult> ValidateUserRateLimitAsync(string userId, string correlationId);

        /// <summary>
        /// بررسی Rate Limit برای IP
        /// </summary>
        Task<ServiceResult> ValidateIpRateLimitAsync(string ipAddress, string correlationId);

        /// <summary>
        /// بررسی Rate Limit برای نوبت
        /// </summary>
        Task<ServiceResult> ValidateAppointmentRateLimitAsync(int appointmentId, string correlationId);

        #endregion

        #region IP Validation

        /// <summary>
        /// اعتبارسنجی آدرس IP
        /// </summary>
        ServiceResult ValidateIpAddress(string ipAddress, string correlationId);

        #endregion

        #region User Agent Validation

        /// <summary>
        /// اعتبارسنجی User Agent
        /// </summary>
        ServiceResult ValidateUserAgent(string userAgent, string correlationId);

        #endregion

        #region Amount Validation

        /// <summary>
        /// اعتبارسنجی مبلغ پرداخت
        /// </summary>
        ServiceResult ValidateAmount(decimal amount, string correlationId);

        /// <summary>
        /// تشخیص ناهنجاری در مبلغ (Anti-Fraud)
        /// </summary>
        Task<ServiceResult> DetectAmountAnomalyAsync(decimal amount, int patientId, string correlationId);

        #endregion

        #region Comprehensive Security Validation

        /// <summary>
        /// اعتبارسنجی امنیتی جامع برای درخواست پرداخت
        /// </summary>
        Task<ServiceResult> ValidatePaymentRequestSecurityAsync(PaymentSecurityValidationRequest request);

        #endregion
    }

    /// <summary>
    /// درخواست اعتبارسنجی امنیتی
    /// </summary>
    public class PaymentSecurityValidationRequest
    {
        public string CorrelationId { get; set; }
        public string UserId { get; set; }
        public int? PatientId { get; set; }
        public int? AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public string UserIpAddress { get; set; }
        public string UserAgent { get; set; }
    }
}

