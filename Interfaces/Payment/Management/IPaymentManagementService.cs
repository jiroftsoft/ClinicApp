using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models;
using ClinicApp.ViewModels.Admin.PaymentManagement;

namespace ClinicApp.Interfaces.Payment.Management
{
    /// <summary>
    /// Service Interface برای مدیریت پرداخت‌ها (Admin)
    /// طراحی شده طبق اصول SRP - مسئولیت: Business Logic برای Payment Management
    /// </summary>
    public interface IPaymentManagementService
    {
        /// <summary>
        /// دریافت لیست پرداخت‌ها
        /// </summary>
        Task<ServiceResult<PaymentIndexViewModel>> GetPaymentsAsync(
            PaymentSearchFilter filter,
            int page,
            int pageSize);

        /// <summary>
        /// دریافت جزئیات پرداخت
        /// </summary>
        Task<ServiceResult<PaymentDetailsViewModel>> GetPaymentDetailsAsync(int onlinePaymentId);

        /// <summary>
        /// Retry پرداخت
        /// </summary>
        Task<ServiceResult> RetryPaymentAsync(int onlinePaymentId, string userId);

        /// <summary>
        /// Cancel پرداخت
        /// </summary>
        Task<ServiceResult> CancelPaymentAsync(int onlinePaymentId, string reason, string userId);

        /// <summary>
        /// Refund پرداخت
        /// </summary>
        Task<ServiceResult> RefundPaymentAsync(int onlinePaymentId, decimal? refundAmount, string reason, string userId);
    }
}

