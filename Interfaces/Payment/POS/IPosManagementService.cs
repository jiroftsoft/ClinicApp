using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.Payment.POS
{
    /// <summary>
    /// Service Interface برای مدیریت ترمینال‌های POS و جلسات نقدی
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت منطق کسب‌وکار POS
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل ترمینال‌های POS
    /// 2. مدیریت جلسات نقدی
    /// 3. محاسبه موجودی و تراز
    /// 4. گزارش‌گیری از تراکنش‌های POS
    /// 5. بهینه‌سازی برای عملکرد بالا
    /// </summary>
    public interface IPosManagementService
    {
        #region POS Terminal Management

        /// <summary>
        /// ایجاد ترمینال POS جدید
        /// </summary>
        /// <param name="request">درخواست ایجاد ترمینال</param>
        /// <returns>نتیجه ایجاد</returns>
        Task<ServiceResult<PosTerminal>> CreatePosTerminalAsync(CreatePosTerminalRequest request);

        /// <summary>
        /// به‌روزرسانی ترمینال POS
        /// </summary>
        /// <param name="request">درخواست به‌روزرسانی</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult<PosTerminal>> UpdatePosTerminalAsync(UpdatePosTerminalRequest request);

        /// <summary>
        /// فعال/غیرفعال کردن ترمینال POS
        /// </summary>
        /// <param name="terminalId">شناسه ترمینال</param>
        /// <param name="isActive">وضعیت فعال</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult> TogglePosTerminalStatusAsync(int terminalId, bool isActive, string userId);

        /// <summary>
        /// دریافت ترمینال POS
        /// </summary>
        /// <param name="terminalId">شناسه ترمینال</param>
        /// <returns>ترمینال POS</returns>
        Task<ServiceResult<PosTerminal>> GetPosTerminalAsync(int terminalId);

        /// <summary>
        /// دریافت ترمینال‌های فعال
        /// </summary>
        /// <returns>لیست ترمینال‌های فعال</returns>
        Task<ServiceResult<IEnumerable<PosTerminal>>> GetActivePosTerminalsAsync();

        /// <summary>
        /// دریافت ترمینال پیش‌فرض
        /// </summary>
        /// <returns>ترمینال پیش‌فرض</returns>
        Task<ServiceResult<PosTerminal>> GetDefaultPosTerminalAsync();

        /// <summary>
        /// تنظیم ترمینال پیش‌فرض
        /// </summary>
        /// <param name="terminalId">شناسه ترمینال</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه تنظیم</returns>
        Task<ServiceResult> SetDefaultPosTerminalAsync(int terminalId, string userId);

        #endregion

        #region Cash Session Management

        /// <summary>
        /// شروع جلسه نقدی جدید
        /// </summary>
        /// <param name="request">درخواست شروع جلسه</param>
        /// <returns>نتیجه شروع جلسه</returns>
        Task<ServiceResult<CashSession>> StartCashSessionAsync(StartCashSessionRequest request);

        /// <summary>
        /// شروع جلسه نقدی جدید (ساده)
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="initialAmount">مبلغ اولیه</param>
        /// <param name="description">توضیحات</param>
        /// <returns>نتیجه شروع جلسه</returns>
        Task<ServiceResult<CashSession>> StartCashSessionAsync(string userId, decimal initialAmount, string description);

        /// <summary>
        /// پایان جلسه نقدی
        /// </summary>
        /// <param name="sessionId">شناسه جلسه</param>
        /// <param name="request">درخواست پایان جلسه</param>
        /// <returns>نتیجه پایان جلسه</returns>
        Task<ServiceResult<CashSession>> EndCashSessionAsync(int sessionId, EndCashSessionRequest request);

        /// <summary>
        /// پایان جلسه نقدی (ساده)
        /// </summary>
        /// <param name="sessionId">شناسه جلسه</param>
        /// <param name="finalAmount">مبلغ نهایی</param>
        /// <param name="description">توضیحات</param>
        /// <param name="endedByUserId">شناسه کاربر پایان دهنده</param>
        /// <returns>نتیجه پایان جلسه</returns>
        Task<ServiceResult<CashSession>> EndCashSessionAsync(int sessionId, decimal finalAmount, string description, string endedByUserId);

        /// <summary>
        /// دریافت جلسه نقدی فعال
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>جلسه نقدی فعال</returns>
        Task<ServiceResult<CashSession>> GetActiveCashSessionAsync(string userId);

        /// <summary>
        /// دریافت جلسات نقدی کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست جلسات نقدی</returns>
        Task<ServiceResult<IEnumerable<CashSession>>> GetUserCashSessionsAsync(string userId, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت جلسات نقدی بر اساس تاریخ
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست جلسات نقدی</returns>
        Task<ServiceResult<IEnumerable<CashSession>>> GetCashSessionsByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 50);

        #endregion

        #region Cash Balance Management

        /// <summary>
        /// محاسبه موجودی نقدی
        /// </summary>
        /// <param name="sessionId">شناسه جلسه</param>
        /// <returns>موجودی نقدی</returns>
        Task<ServiceResult<CashBalance>> CalculateCashBalanceAsync(int sessionId);

        /// <summary>
        /// محاسبه موجودی نقدی کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>موجودی نقدی</returns>
        Task<ServiceResult<CashBalance>> CalculateUserCashBalanceAsync(string userId);

        /// <summary>
        /// محاسبه موجودی نقدی روزانه
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>موجودی نقدی روزانه</returns>
        Task<ServiceResult<DailyCashBalance>> CalculateDailyCashBalanceAsync(DateTime date);

        /// <summary>
        /// اضافه کردن موجودی نقدی
        /// </summary>
        /// <param name="sessionId">شناسه جلسه</param>
        /// <param name="amount">مبلغ</param>
        /// <param name="description">توضیحات</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه اضافه کردن</returns>
        Task<ServiceResult> AddCashBalanceAsync(int sessionId, decimal amount, string description, string userId);

        /// <summary>
        /// کسر موجودی نقدی
        /// </summary>
        /// <param name="sessionId">شناسه جلسه</param>
        /// <param name="amount">مبلغ</param>
        /// <param name="description">توضیحات</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه کسر</returns>
        Task<ServiceResult> SubtractCashBalanceAsync(int sessionId, decimal amount, string description, string userId);

        #endregion

        #region POS Statistics

        /// <summary>
        /// دریافت آمار ترمینال‌های POS
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار ترمینال‌ها</returns>
        Task<ServiceResult<PosTerminalStatistics>> GetPosTerminalStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت آمار جلسات نقدی
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار جلسات نقدی</returns>
        Task<ServiceResult<CashSessionStatistics>> GetCashSessionStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت آمار روزانه POS
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار روزانه</returns>
        Task<ServiceResult<DailyPosStatistics>> GetDailyPosStatisticsAsync(DateTime date);

        #endregion

        #region POS Validation

        /// <summary>
        /// اعتبارسنجی ترمینال POS
        /// </summary>
        /// <param name="terminalId">شناسه ترمینال</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePosTerminalAsync(int terminalId);

        /// <summary>
        /// بررسی امکان استفاده از ترمینال
        /// </summary>
        /// <param name="terminalId">شناسه ترمینال</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult> CanUsePosTerminalAsync(int terminalId);

        /// <summary>
        /// اعتبارسنجی جلسه نقدی
        /// </summary>
        /// <param name="sessionId">شناسه جلسه</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateCashSessionAsync(int sessionId);

        #endregion

        #region CRUD Operations

        /// <summary>
        /// دریافت لیست ترمینال‌های POS
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست ترمینال‌ها</returns>
        Task<ServiceResult<IEnumerable<PosTerminal>>> GetTerminalsAsync(int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت ترمینال POS بر اساس شناسه
        /// </summary>
        /// <param name="terminalId">شناسه ترمینال</param>
        /// <returns>ترمینال POS</returns>
        Task<ServiceResult<PosTerminal>> GetTerminalByIdAsync(int terminalId);

        /// <summary>
        /// ایجاد ترمینال POS جدید
        /// </summary>
        /// <param name="terminal">ترمینال POS</param>
        /// <returns>نتیجه ایجاد</returns>
        Task<ServiceResult<PosTerminal>> CreateTerminalAsync(PosTerminal terminal);

        /// <summary>
        /// به‌روزرسانی ترمینال POS
        /// </summary>
        /// <param name="terminal">ترمینال POS</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult<PosTerminal>> UpdateTerminalAsync(PosTerminal terminal);

        /// <summary>
        /// حذف ترمینال POS
        /// </summary>
        /// <param name="terminalId">شناسه ترمینال</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه حذف</returns>
        Task<ServiceResult> DeleteTerminalAsync(int terminalId, string userId);

        /// <summary>
        /// دریافت جلسه نقدی بر اساس شناسه
        /// </summary>
        /// <param name="sessionId">شناسه جلسه</param>
        /// <returns>جلسه نقدی</returns>
        Task<ServiceResult<CashSession>> GetSessionByIdAsync(int sessionId);

        /// <summary>
        /// دریافت جلسات نقدی فعال
        /// </summary>
        /// <returns>لیست جلسات فعال</returns>
        Task<ServiceResult<IEnumerable<CashSession>>> GetActiveSessionsAsync();

        /// <summary>
        /// دریافت آمار POS
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار POS</returns>
        Task<ServiceResult<PosStatistics>> GetPosStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت آمار POS ViewModel (برای سازگاری با Controller)
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار POS ViewModel</returns>
        Task<ServiceResult<ClinicApp.ViewModels.Payment.POS.PosStatisticsViewModel>> GetPosStatisticsViewModelAsync(DateTime startDate, DateTime endDate);

        #endregion
    }

    #region Request/Response Models

    /// <summary>
    /// درخواست ایجاد ترمینال POS
    /// </summary>
    public class CreatePosTerminalRequest
    {
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public PosProviderType ProviderType { get; set; }
        public PosProtocol Protocol { get; set; }
        public string ConnectionString { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public string CreatedByUserId { get; set; }
    }

    /// <summary>
    /// درخواست به‌روزرسانی ترمینال POS
    /// </summary>
    public class UpdatePosTerminalRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public PosProviderType ProviderType { get; set; }
        public PosProtocol Protocol { get; set; }
        public string ConnectionString { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public string UpdatedByUserId { get; set; }
    }

    /// <summary>
    /// درخواست شروع جلسه نقدی
    /// </summary>
    public class StartCashSessionRequest
    {
        public string UserId { get; set; }
        public decimal InitialCashAmount { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// درخواست پایان جلسه نقدی
    /// </summary>
    public class EndCashSessionRequest
    {
        public decimal FinalCashAmount { get; set; }
        public string Description { get; set; }
        public string EndedByUserId { get; set; }
    }

    /// <summary>
    /// موجودی نقدی
    /// </summary>
    public class CashBalance
    {
        public int SessionId { get; set; }
        public string UserId { get; set; }
        public decimal InitialAmount { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal ExpectedBalance { get; set; }
        public decimal Difference { get; set; }
        public DateTime CalculatedAt { get; set; }
    }

    /// <summary>
    /// موجودی نقدی روزانه
    /// </summary>
    public class DailyCashBalance
    {
        public DateTime Date { get; set; }
        public int TotalSessions { get; set; }
        public decimal TotalInitialAmount { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalCurrentBalance { get; set; }
        public decimal TotalExpectedBalance { get; set; }
        public decimal TotalDifference { get; set; }
        public Dictionary<string, decimal> BalanceByUser { get; set; }
    }

    /// <summary>
    /// آمار ترمینال‌های POS
    /// </summary>
    public class PosTerminalStatistics
    {
        public int TotalTerminals { get; set; }
        public int ActiveTerminals { get; set; }
        public int InactiveTerminals { get; set; }
        public int DefaultTerminals { get; set; }
        public Dictionary<PosProviderType, int> TerminalsByProvider { get; set; }
        public Dictionary<PosProtocol, int> TerminalsByProtocol { get; set; }
        public decimal TotalTransactionAmount { get; set; }
        public int TotalTransactionCount { get; set; }
        public decimal AverageTransactionAmount { get; set; }
    }


    /// <summary>
    /// آمار روزانه POS
    /// </summary>
    public class DailyPosStatistics
    {
        public DateTime Date { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal PosAmount { get; set; }
        public int ActiveSessions { get; set; }
        public int CompletedSessions { get; set; }
        public decimal TotalCashBalance { get; set; }
        public Dictionary<int, decimal> AmountByTerminal { get; set; }
        public Dictionary<string, decimal> AmountByUser { get; set; }
    }

    /// <summary>
    /// آمار کلی POS
    /// </summary>
    public class PosStatistics
    {
        public int TotalTerminals { get; set; }
        public int ActiveTerminals { get; set; }
        public int TotalSessions { get; set; }
        public int ActiveSessions { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalTransactions { get; set; }
        public Dictionary<PosProviderType, int> TerminalsByProvider { get; set; }
        public Dictionary<PosProtocol, int> TerminalsByProtocol { get; set; }
        public Dictionary<CashSessionStatus, int> SessionsByStatus { get; set; }
    }

    #endregion
}
