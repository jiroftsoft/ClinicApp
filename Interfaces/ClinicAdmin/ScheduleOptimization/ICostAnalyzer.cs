using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization
{
    /// <summary>
    /// Interface برای تحلیل و بهینه‌سازی هزینه‌ها
    /// 
    /// مسئولیت (SRP):
    /// - محاسبه درآمد و هزینه‌ها
    /// - تحلیل سودآوری
    /// - پیشنهاد بهینه‌سازی هزینه
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط تحلیل هزینه
    /// - Interface Segregation: Interface کوچک و متمرکز
    /// - Dependency Inversion: وابستگی به abstraction
    /// </summary>
    public interface ICostAnalyzer
    {
        /// <summary>
        /// بهینه‌سازی هزینه‌ها برای یک بازه زمانی
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش بهینه‌سازی هزینه</returns>
        Task<ServiceResult<CostOptimizationReport>> OptimizeCostsAsync(int doctorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// محاسبه درآمد کل
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>درآمد کل (ریال)</returns>
        Task<ServiceResult<decimal>> CalculateTotalRevenueAsync(int doctorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// محاسبه هزینه‌های کل
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>هزینه‌های کل (ریال)</returns>
        Task<ServiceResult<decimal>> CalculateTotalCostsAsync(int doctorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// محاسبه سود خالص
        /// </summary>
        /// <param name="revenue">درآمد</param>
        /// <param name="costs">هزینه‌ها</param>
        /// <returns>سود خالص (ریال)</returns>
        decimal CalculateNetProfit(decimal revenue, decimal costs);

        /// <summary>
        /// تولید پیشنهادات بهینه‌سازی هزینه
        /// </summary>
        /// <param name="currentCosts">هزینه‌های فعلی</param>
        /// <param name="revenue">درآمد</param>
        /// <returns>لیست پیشنهادات</returns>
        List<CostOptimizationSuggestion> GenerateCostOptimizationSuggestions(decimal currentCosts, decimal revenue);
    }
}

