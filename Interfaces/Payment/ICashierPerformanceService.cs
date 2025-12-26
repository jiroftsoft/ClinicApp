using ClinicApp.Helpers;
using ClinicApp.Models.DTOs.Payment;
using ClinicApp.Models.Entities.Payment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces.Payment
{
    /// <summary>
    /// سرویس محاسبه و مدیریت متریک‌های عملکرد منشی‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. محاسبه خودکار متریک‌های روزانه
    /// 2. ذخیره متریک‌ها در دیتابیس
    /// 3. دریافت متریک‌های ذخیره شده
    /// 4. شناسایی بهترین عملکردها
    /// 5. پشتیبانی از Scheduled Jobs
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public interface ICashierPerformanceService
    {
        /// <summary>
        /// محاسبه و ذخیره متریک‌های روزانه یک منشی
        /// </summary>
        /// <param name="cashierId">شناسه منشی</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult<CashierPerformanceMetrics>> CalculateDailyMetricsAsync(string cashierId, DateTime date);

        /// <summary>
        /// محاسبه و ذخیره متریک‌های روزانه تمام منشی‌ها برای یک تاریخ
        /// </summary>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult<int>> CalculateAllCashiersDailyMetricsAsync(DateTime date);

        /// <summary>
        /// دریافت متریک‌های ذخیره شده یک منشی برای یک تاریخ
        /// </summary>
        /// <param name="cashierId">شناسه منشی</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <returns>متریک‌های عملکرد</returns>
        Task<ServiceResult<CashierPerformanceMetrics>> GetMetricsAsync(string cashierId, DateTime date);

        /// <summary>
        /// دریافت متریک‌های یک منشی در بازه زمانی
        /// </summary>
        /// <param name="cashierId">شناسه منشی</param>
        /// <param name="fromDate">از تاریخ</param>
        /// <param name="toDate">تا تاریخ</param>
        /// <returns>لیست متریک‌ها</returns>
        Task<ServiceResult<List<CashierPerformanceMetrics>>> GetMetricsRangeAsync(string cashierId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// دریافت بهترین عملکردها (Top N)
        /// </summary>
        /// <param name="fromDate">از تاریخ</param>
        /// <param name="toDate">تا تاریخ</param>
        /// <param name="topN">تعداد منشی‌های برتر</param>
        /// <param name="sortBy">معیار مرتب‌سازی (TotalTransactions, TotalAmount, SuccessRate)</param>
        /// <returns>لیست منشی‌های برتر</returns>
        Task<ServiceResult<List<CashierRanking>>> GetTopPerformersAsync(DateTime fromDate, DateTime toDate, int topN = 10, string sortBy = "TotalTransactions");

        /// <summary>
        /// دریافت رتبه یک منشی در بازه زمانی
        /// </summary>
        /// <param name="cashierId">شناسه منشی</param>
        /// <param name="fromDate">از تاریخ</param>
        /// <param name="toDate">تا تاریخ</param>
        /// <param name="sortBy">معیار مرتب‌سازی</param>
        /// <returns>رتبه منشی</returns>
        Task<ServiceResult<CashierRanking>> GetCashierRankingAsync(string cashierId, DateTime fromDate, DateTime toDate, string sortBy = "TotalTransactions");
    }
}

