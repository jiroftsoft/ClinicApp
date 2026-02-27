using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Admin;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// سرویس داشبورد درآمد — خلاصه، نمودار، جزئیات و خروجی Excel برای تصمیم‌گیری مالی و مدیریتی
    /// </summary>
    public interface IRevenueDashboardService
    {
        /// <summary>
        /// واکشی مدل کامل داشبورد درآمد بر اساس فیلتر (بازه تاریخ، پزشک، دپارتمان، روش پرداخت)
        /// </summary>
        Task<ServiceResult<RevenueDashboardViewModel>> GetDashboardAsync(RevenueDashboardFilterViewModel filter);

        /// <summary>
        /// خلاصه KPI درآمد (برای آپدیت AJAX)
        /// </summary>
        Task<ServiceResult<RevenueSummaryViewModel>> GetSummaryAsync(RevenueDashboardFilterViewModel filter);

        /// <summary>
        /// داده‌های نمودار (روند روزانه و تفکیک روش پرداخت)
        /// </summary>
        Task<ServiceResult<RevenueChartDataViewModel>> GetChartDataAsync(RevenueDashboardFilterViewModel filter);

        /// <summary>
        /// خروجی Excel از داده‌های فیلتر شده
        /// </summary>
        Task<ServiceResult<byte[]>> ExportToExcelAsync(RevenueDashboardFilterViewModel filter);
    }
}
