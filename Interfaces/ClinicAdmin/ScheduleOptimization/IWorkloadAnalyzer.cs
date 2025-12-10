using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization
{
    /// <summary>
    /// Interface برای تحلیل و محاسبه بار کاری پزشکان
    /// 
    /// مسئولیت (SRP):
    /// - تحلیل بار کاری روزانه/هفتگی/ماهانه
    /// - محاسبه تعداد نوبت‌ها
    /// - تشخیص وضعیت بار کاری (Light/Balanced/Heavy/Overloaded)
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط تحلیل بار کاری
    /// - Interface Segregation: Interface کوچک و متمرکز
    /// - Dependency Inversion: وابستگی به abstraction
    /// </summary>
    public interface IWorkloadAnalyzer
    {
        /// <summary>
        /// تحلیل بار کاری روزانه برای یک پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <returns>نتیجه تحلیل بار کاری روزانه</returns>
        Task<ServiceResult<WorkloadAnalysisResult>> AnalyzeDailyWorkloadAsync(int doctorId, DateTime date);

        /// <summary>
        /// تحلیل بار کاری هفتگی برای یک پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="weekStart">شروع هفته</param>
        /// <returns>لیست نتایج تحلیل برای هر روز هفته</returns>
        Task<ServiceResult<List<WorkloadAnalysisResult>>> AnalyzeWeeklyWorkloadAsync(int doctorId, DateTime weekStart);

        /// <summary>
        /// تحلیل بار کاری ماهانه برای یک پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="monthStart">شروع ماه</param>
        /// <returns>دیکشنری نتایج تحلیل به تفکیک هفته</returns>
        Task<ServiceResult<Dictionary<string, List<WorkloadAnalysisResult>>>> AnalyzeMonthlyWorkloadAsync(int doctorId, DateTime monthStart);

        /// <summary>
        /// محاسبه تعداد نوبت‌های قابل رزرو برای یک بازه زمانی
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="startTime">زمان شروع</param>
        /// <param name="endTime">زمان پایان</param>
        /// <param name="appointmentDuration">مدت زمان هر نوبت (دقیقه)</param>
        /// <returns>تعداد نوبت‌های قابل رزرو</returns>
        Task<ServiceResult<int>> CalculateAvailableAppointmentsAsync(int doctorId, DateTime startTime, DateTime endTime, int appointmentDuration);

        /// <summary>
        /// تشخیص وضعیت بار کاری بر اساس تعداد نوبت‌ها
        /// </summary>
        /// <param name="appointmentCount">تعداد نوبت‌ها</param>
        /// <param name="maxCapacity">حداکثر ظرفیت</param>
        /// <returns>وضعیت بار کاری</returns>
        WorkloadBalanceStatus DetermineWorkloadStatus(int appointmentCount, int maxCapacity);
    }

    /// <summary>
    /// نتیجه تحلیل بار کاری
    /// </summary>
    public class WorkloadAnalysisResult
    {
        /// <summary>
        /// تاریخ تحلیل
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// تعداد نوبت‌های فعلی
        /// </summary>
        public int CurrentAppointments { get; set; }

        /// <summary>
        /// تعداد نوبت‌های قابل رزرو
        /// </summary>
        public int AvailableAppointments { get; set; }

        /// <summary>
        /// حداکثر ظرفیت
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// درصد استفاده از ظرفیت
        /// </summary>
        public decimal UtilizationPercentage { get; set; }

        /// <summary>
        /// وضعیت بار کاری
        /// </summary>
        public WorkloadBalanceStatus Status { get; set; }

        /// <summary>
        /// کل زمان کار (دقیقه)
        /// </summary>
        public int TotalWorkMinutes { get; set; }

        /// <summary>
        /// زمان استراحت (دقیقه)
        /// </summary>
        public int BreakTimeMinutes { get; set; }

        /// <summary>
        /// زمان خالی (دقیقه)
        /// </summary>
        public int FreeTimeMinutes { get; set; }
    }
}

