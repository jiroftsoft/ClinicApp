using System;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Admin;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// سرویس داشبورد تحلیلی درآمد بیمه‌ها و مدیریت مطالبات
    /// </summary>
    public interface IInsuranceRevenueService
    {
        Task<ServiceResult<InsuranceRevenueDashboardViewModel>> GetDashboardDataAsync(InsuranceRevenueFilterViewModel filter);

        Task<ServiceResult<InsuranceKPIViewModel>> GetKPIsAsync(InsuranceRevenueFilterViewModel filter);

        Task<ServiceResult<System.Collections.Generic.List<InsuranceAgingItemViewModel>>> GetAgingReportAsync(DateTime? asOfDate = null);

        Task<ServiceResult<InsuranceChartDataViewModel>> GetChartDataAsync(InsuranceRevenueFilterViewModel filter);

        Task<ServiceResult<System.Collections.Generic.List<InsuranceProviderBreakdownViewModel>>> GetProviderBreakdownAsync(DateTime start, DateTime end, int? insuranceProviderId = null);

        Task<ServiceResult<object>> CreateBatchAsync(int providerId, System.Collections.Generic.List<int> claimIds);

        Task<ServiceResult<byte[]>> ExportToExcelAsync(InsuranceRevenueFilterViewModel filter);
    }
}
