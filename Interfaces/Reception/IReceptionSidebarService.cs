using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// رابط سرویس مدیریت سایدبار پذیرش
    /// مسئولیت: مدیریت کامل سایدبار پذیرش
    /// </summary>
    public interface IReceptionSidebarService
    {
        /// <summary>
        /// دریافت آمار امروز برای سایدبار
        /// </summary>
        /// <returns>آمار امروز</returns>
        Task<ServiceResult<SidebarStatistics>> GetTodayStatisticsAsync();

        /// <summary>
        /// دریافت هشدارهای پزشکی برای سایدبار
        /// </summary>
        /// <returns>لیست هشدارهای پزشکی</returns>
        Task<ServiceResult<List<MedicalAlert>>> GetMedicalAlertsAsync();

        /// <summary>
        /// دریافت وضعیت بیمه‌ها برای سایدبار
        /// </summary>
        /// <returns>وضعیت بیمه‌ها</returns>
        Task<ServiceResult<InsuranceStatus>> GetInsuranceStatusAsync();

        /// <summary>
        /// دریافت وضعیت پرداخت‌ها برای سایدبار
        /// </summary>
        /// <returns>وضعیت پرداخت‌ها</returns>
        Task<ServiceResult<PaymentStatus>> GetPaymentStatusAsync();

        /// <summary>
        /// دریافت اقدامات سریع برای سایدبار
        /// </summary>
        /// <returns>لیست اقدامات سریع</returns>
        Task<ServiceResult<List<QuickAction>>> GetQuickActionsAsync();
    }
}
