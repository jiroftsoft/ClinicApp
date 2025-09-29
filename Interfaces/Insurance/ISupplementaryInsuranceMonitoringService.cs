using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Insurance;
using ClinicApp.ViewModels.Insurance.Supplementary;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Interface برای سرویس Monitoring و Analytics بیمه تکمیلی
    /// طراحی شده برای نظارت و تحلیل عملکرد سیستم‌های پزشکی
    /// </summary>
    public interface ISupplementaryInsuranceMonitoringService
    {
        /// <summary>
        /// ثبت رویداد محاسبه
        /// </summary>
        /// <param name="calculationEvent">رویداد محاسبه</param>
        void LogCalculationEvent(CalculationEvent calculationEvent);

        /// <summary>
        /// ثبت رویداد خطا
        /// </summary>
        /// <param name="errorEvent">رویداد خطا</param>
        void LogErrorEvent(ErrorEvent errorEvent);

        /// <summary>
        /// دریافت آمار عملکرد
        /// </summary>
        /// <param name="fromDate">تاریخ شروع (اختیاری)</param>
        /// <param name="toDate">تاریخ پایان (اختیاری)</param>
        /// <returns>گزارش عملکرد</returns>
        Models.Insurance.PerformanceReport GetPerformanceReport(DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// دریافت آمار استفاده
        /// </summary>
        /// <param name="fromDate">تاریخ شروع (اختیاری)</param>
        /// <param name="toDate">تاریخ پایان (اختیاری)</param>
        /// <returns>آمار استفاده</returns>
        UsageStatistics GetUsageStatistics(DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// بررسی سلامت سیستم
        /// </summary>
        /// <returns>وضعیت سلامت سیستم</returns>
        SystemHealthStatus GetSystemHealthStatus();
    }
}
