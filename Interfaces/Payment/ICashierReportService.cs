using ClinicApp.Helpers;
using ClinicApp.Models.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClinicApp.Interfaces.Payment
{
    /// <summary>
    /// سرویس گزارش‌گیری از عملکرد منشی‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. گزارش روزانه عملکرد منشی
    /// 2. گزارش ماهانه عملکرد منشی
    /// 3. خلاصه عملکرد تمام منشی‌ها
    /// 4. مقایسه عملکرد منشی‌ها
    /// 5. Export به Excel و PDF
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public interface ICashierReportService
    {
        /// <summary>
        /// دریافت گزارش روزانه عملکرد یک منشی
        /// </summary>
        /// <param name="cashierId">شناسه منشی</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <returns>گزارش روزانه</returns>
        Task<ServiceResult<CashierDailyReport>> GetDailyReportAsync(string cashierId, DateTime date);

        /// <summary>
        /// دریافت گزارش ماهانه عملکرد یک منشی
        /// </summary>
        /// <param name="cashierId">شناسه منشی</param>
        /// <param name="year">سال</param>
        /// <param name="month">ماه (1-12)</param>
        /// <returns>گزارش ماهانه</returns>
        Task<ServiceResult<CashierMonthlyReport>> GetMonthlyReportAsync(string cashierId, int year, int month);

        /// <summary>
        /// دریافت گزارش بازه‌زمانی یک منشی (تجمیع روزانه)
        /// </summary>
        /// <param name="cashierId">شناسه منشی</param>
        /// <param name="fromDate">از تاریخ</param>
        /// <param name="toDate">تا تاریخ</param>
        /// <returns>گزارش تجمیع‌شده بازه</returns>
        Task<ServiceResult<CashierDailyReport>> GetRangeReportAsync(string cashierId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// دریافت لیست منشی‌ها برای DropDown (کاربران با نقش Receptionist)
        /// </summary>
        Task<List<SelectListItem>> GetCashiersListAsync();

        /// <summary>
        /// دریافت خلاصه عملکرد تمام منشی‌ها در بازه زمانی مشخص
        /// </summary>
        /// <param name="fromDate">از تاریخ</param>
        /// <param name="toDate">تا تاریخ</param>
        /// <returns>لیست خلاصه عملکرد منشی‌ها</returns>
        Task<ServiceResult<List<CashierSummary>>> GetAllCashiersSummaryAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// مقایسه عملکرد چند منشی در بازه زمانی مشخص
        /// </summary>
        /// <param name="cashierIds">لیست شناسه‌های منشی‌ها</param>
        /// <param name="fromDate">از تاریخ</param>
        /// <param name="toDate">تا تاریخ</param>
        /// <returns>نتیجه مقایسه</returns>
        Task<ServiceResult<CashierPerformanceComparison>> CompareCashiersAsync(List<string> cashierIds, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Export گزارش به Excel
        /// </summary>
        /// <param name="cashierId">شناسه منشی</param>
        /// <param name="fromDate">از تاریخ</param>
        /// <param name="toDate">تا تاریخ</param>
        /// <returns>فایل Excel به صورت byte array</returns>
        Task<ServiceResult<byte[]>> ExportToExcelAsync(string cashierId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Export گزارش به PDF
        /// </summary>
        /// <param name="cashierId">شناسه منشی</param>
        /// <param name="fromDate">از تاریخ</param>
        /// <param name="toDate">تا تاریخ</param>
        /// <returns>فایل PDF به صورت byte array</returns>
        Task<ServiceResult<byte[]>> ExportToPdfAsync(string cashierId, DateTime fromDate, DateTime toDate);
    }
}

