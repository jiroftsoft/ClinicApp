using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Admin.PaymentManagement;

namespace ClinicApp.Interfaces.Payment.Management
{
    /// <summary>
    /// Repository Interface برای مدیریت پرداخت‌ها (Admin)
    /// طراحی شده طبق اصول SRP - مسئولیت: Data Access برای Payment Management
    /// </summary>
    public interface IPaymentManagementRepository
    {
        /// <summary>
        /// دریافت لیست پرداخت‌ها با فیلتر و Pagination
        /// </summary>
        Task<PagedResult<OnlinePayment>> GetPaymentsAsync(
            PaymentSearchFilter filter,
            int page,
            int pageSize);

        /// <summary>
        /// دریافت جزئیات پرداخت
        /// </summary>
        Task<OnlinePayment> GetPaymentDetailsAsync(int onlinePaymentId);

        /// <summary>
        /// دریافت آمار پرداخت‌ها
        /// </summary>
        Task<PaymentStatisticsViewModel> GetPaymentStatisticsAsync(
            PaymentSearchFilter filter);

        /// <summary>
        /// دریافت Timeline پرداخت
        /// </summary>
        Task<List<PaymentTimelineItemViewModel>> GetPaymentTimelineAsync(int onlinePaymentId);

        /// <summary>
        /// تعداد اختلاف‌های مالی حل‌نشده (وضعیت Pending)
        /// </summary>
        Task<int> GetPendingDiscrepancyCountAsync();
    }
}

