using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization
{
    /// <summary>
    /// Interface برای بهینه‌سازی توزیع بیماران
    /// 
    /// مسئولیت (SRP):
    /// - توزیع بیماران بر اساس نوع
    /// - بهینه‌سازی توزیع در طول روز
    /// - تحلیل الگوهای توزیع
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط توزیع بیماران
    /// - Interface Segregation: Interface کوچک و متمرکز
    /// - Dependency Inversion: وابستگی به abstraction
    /// </summary>
    public interface IPatientDistributor
    {
        /// <summary>
        /// بهینه‌سازی توزیع بیماران برای یک روز
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <returns>نتیجه بهینه‌سازی توزیع</returns>
        Task<ServiceResult<PatientDistributionResult>> OptimizePatientDistributionAsync(int doctorId, DateTime date);

        /// <summary>
        /// تحلیل توزیع بیماران بر اساس نوع
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <returns>تحلیل توزیع</returns>
        Task<ServiceResult<Dictionary<string, int>>> AnalyzeDistributionByTypeAsync(int doctorId, DateTime date);

        /// <summary>
        /// تحلیل توزیع بیماران در طول روز
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <returns>تحلیل توزیع ساعتی</returns>
        Task<ServiceResult<Dictionary<int, int>>> AnalyzeHourlyDistributionAsync(int doctorId, DateTime date);

        /// <summary>
        /// پیشنهاد بهینه‌سازی توزیع
        /// </summary>
        /// <param name="currentDistribution">توزیع فعلی</param>
        /// <param name="optimalDistribution">توزیع بهینه</param>
        /// <returns>لیست پیشنهادات</returns>
        List<string> SuggestDistributionImprovements(Dictionary<string, int> currentDistribution, Dictionary<string, int> optimalDistribution);
    }
}

