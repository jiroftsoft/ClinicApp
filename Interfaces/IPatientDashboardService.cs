using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Patient;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Service interface for Patient Dashboard
    /// Single Responsibility: Define contract for patient dashboard data retrieval
    /// 
    /// ✅ Enterprise-Grade: API-First, ServiceResult Enhanced
    /// طبق: CLINICAPP_PATIENT_DASHBOARD_BEAST_ROADMAP_PROMPT.md
    /// </summary>
    public interface IPatientDashboardService
    {
        /// <summary>
        /// دریافت آمار سریع داشبورد بیمار
        /// </summary>
        Task<ServiceResult<DashboardQuickStatsViewModel>> GetQuickStatsAsync(int patientId);

        /// <summary>
        /// دریافت نوبت‌های اخیر بیمار (با pagination)
        /// </summary>
        Task<ServiceResult<DashboardAppointmentsSectionViewModel>> GetRecentAppointmentsAsync(
            int patientId, 
            int pageNumber = 1, 
            int pageSize = 5);

        /// <summary>
        /// دریافت نوبت‌های آینده بیمار
        /// </summary>
        Task<ServiceResult<DashboardAppointmentsSectionViewModel>> GetUpcomingAppointmentsAsync(
            int patientId, 
            int pageNumber = 1, 
            int pageSize = 5);

        /// <summary>
        /// دریافت پذیرش‌های اخیر (اگر موجود باشد)
        /// </summary>
        Task<ServiceResult<DashboardReceptionsSectionViewModel>> GetRecentReceptionsAsync(
            int patientId, 
            int pageNumber = 1, 
            int pageSize = 5);

        /// <summary>
        /// دریافت یک‌جا آمار + نوبت‌های اخیر/آینده + پذیرش‌ها (یک درخواست به‌جای چهار).
        /// </summary>
        Task<ServiceResult<DashboardViewModel>> GetOverviewAsync(
            int patientId,
            int recentPageSize = 5,
            int upcomingPageSize = 5,
            int receptionsPageSize = 5);
    }
}

