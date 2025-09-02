using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// رابط سرویس بهینه‌سازی برنامه کاری پزشکان
    /// </summary>
    public interface IScheduleOptimizationService
    {
        /// <summary>
        /// بهینه‌سازی برنامه کاری روزانه
        /// </summary>
        Task<ServiceResult<WorkloadBalanceResult>> OptimizeDailyScheduleAsync(int doctorId, DateTime date);

        /// <summary>
        /// بهینه‌سازی برنامه کاری هفتگی
        /// </summary>
        Task<ServiceResult<List<WorkloadBalanceResult>>> OptimizeWeeklyScheduleAsync(int doctorId, DateTime weekStart);

        /// <summary>
        /// بهینه‌سازی برنامه کاری ماهانه
        /// </summary>
        Task<ServiceResult<Dictionary<string, List<WorkloadBalanceResult>>>> OptimizeMonthlyScheduleAsync(int doctorId, DateTime monthStart);

        /// <summary>
        /// متعادل‌سازی بار کاری
        /// </summary>
        Task<ServiceResult<bool>> BalanceWorkloadAsync(int doctorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// بهینه‌سازی زمان‌های استراحت
        /// </summary>
        Task<ServiceResult<List<BreakTimeSlot>>> OptimizeBreakTimesAsync(int doctorId, DateTime date);

        /// <summary>
        /// بهینه‌سازی اولویت‌های نوبت‌ها
        /// </summary>
        Task<ServiceResult<bool>> OptimizeAppointmentPrioritiesAsync(int doctorId, DateTime date);

        /// <summary>
        /// بهینه‌سازی توزیع بیماران
        /// </summary>
        Task<ServiceResult<PatientDistributionResult>> OptimizePatientDistributionAsync(int doctorId, DateTime date);

        /// <summary>
        /// بهینه‌سازی زمان‌های اورژانس
        /// </summary>
        Task<ServiceResult<List<EmergencyTimeSlot>>> OptimizeEmergencyTimesAsync(int doctorId, DateTime date);

        /// <summary>
        /// بهینه‌سازی قالب‌های برنامه کاری
        /// </summary>
        Task<ServiceResult<bool>> OptimizeScheduleTemplatesAsync(int doctorId);

        /// <summary>
        /// بهینه‌سازی تعادل کار و زندگی
        /// </summary>
        Task<ServiceResult<WorkLifeBalanceReport>> OptimizeWorkLifeBalanceAsync(int doctorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// بهینه‌سازی هزینه‌ها
        /// </summary>
        Task<ServiceResult<CostOptimizationReport>> OptimizeCostsAsync(int doctorId, DateTime startDate, DateTime endDate);
    }
}
