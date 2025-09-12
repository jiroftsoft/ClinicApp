using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.Payment.Reporting
{
    /// <summary>
    /// Service Interface برای گزارش‌گیری از پرداخت‌ها
    /// طراحی شده طبق اصول SRP - مسئولیت: گزارش‌گیری و تحلیل منطق کسب‌وکار پرداخت‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. گزارش‌گیری کامل از پرداخت‌ها
    /// 2. آمار و تحلیل‌های پرداخت
    /// 3. گزارش‌های مالی و مدیریتی
    /// 4. گزارش‌های عملکردی
    /// 5. بهینه‌سازی برای عملکرد بالا
    /// </summary>
    public interface IPaymentReportingService
    {
        #region Payment Reports

        /// <summary>
        /// گزارش پرداخت‌های روزانه
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>گزارش روزانه</returns>
        Task<ServiceResult<DailyPaymentReport>> GetDailyPaymentReportAsync(DateTime date);

        /// <summary>
        /// گزارش پرداخت‌های هفتگی
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش هفتگی</returns>
        Task<ServiceResult<WeeklyPaymentReport>> GetWeeklyPaymentReportAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// گزارش پرداخت‌های ماهانه
        /// </summary>
        /// <param name="year">سال</param>
        /// <param name="month">ماه</param>
        /// <returns>گزارش ماهانه</returns>
        Task<ServiceResult<MonthlyPaymentReport>> GetMonthlyPaymentReportAsync(int year, int month);

        /// <summary>
        /// گزارش پرداخت‌های سالانه
        /// </summary>
        /// <param name="year">سال</param>
        /// <returns>گزارش سالانه</returns>
        Task<ServiceResult<YearlyPaymentReport>> GetYearlyPaymentReportAsync(int year);

        /// <summary>
        /// گزارش پرداخت‌های بازه زمانی
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش بازه زمانی</returns>
        Task<ServiceResult<DateRangePaymentReport>> GetDateRangePaymentReportAsync(DateTime startDate, DateTime endDate);

        #endregion

        #region Payment Statistics

        /// <summary>
        /// آمار کلی پرداخت‌ها
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="paymentMethod">روش پرداخت (اختیاری)</param>
        /// <param name="paymentStatus">وضعیت پرداخت (اختیاری)</param>
        /// <returns>آمار کلی</returns>
        Task<ServiceResult<PaymentStatistics>> GetPaymentStatisticsAsync(DateTime startDate, DateTime endDate, PaymentMethod? paymentMethod = null, PaymentStatus? paymentStatus = null);

        /// <summary>
        /// آمار پرداخت‌ها بر اساس روش پرداخت
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار بر اساس روش پرداخت</returns>
        Task<ServiceResult<PaymentMethodStatistics>> GetPaymentMethodStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// آمار پرداخت‌ها بر اساس وضعیت
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار بر اساس وضعیت</returns>
        Task<ServiceResult<PaymentStatusStatistics>> GetPaymentStatusStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// آمار پرداخت‌ها بر اساس کاربر
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار بر اساس کاربر</returns>
        Task<ServiceResult<UserPaymentStatistics>> GetUserPaymentStatisticsAsync(DateTime startDate, DateTime endDate);

        #endregion

        #region Financial Reports

        /// <summary>
        /// گزارش مالی روزانه
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>گزارش مالی روزانه</returns>
        Task<ServiceResult<DailyFinancialReport>> GetDailyFinancialReportAsync(DateTime date);

        /// <summary>
        /// گزارش مالی ماهانه
        /// </summary>
        /// <param name="year">سال</param>
        /// <param name="month">ماه</param>
        /// <returns>گزارش مالی ماهانه</returns>
        Task<ServiceResult<MonthlyFinancialReport>> GetMonthlyFinancialReportAsync(int year, int month);

        /// <summary>
        /// گزارش درآمد و هزینه
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش درآمد و هزینه</returns>
        Task<ServiceResult<IncomeExpenseReport>> GetIncomeExpenseReportAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// گزارش کارمزد درگاه‌ها
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش کارمزد درگاه‌ها</returns>
        Task<ServiceResult<GatewayFeeReport>> GetGatewayFeeReportAsync(DateTime startDate, DateTime endDate);

        #endregion

        #region Performance Reports

        /// <summary>
        /// گزارش عملکرد درگاه‌های پرداخت
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش عملکرد درگاه‌ها</returns>
        Task<ServiceResult<GatewayPerformanceReport>> GetGatewayPerformanceReportAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// گزارش عملکرد ترمینال‌های POS
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش عملکرد ترمینال‌ها</returns>
        Task<ServiceResult<PosTerminalPerformanceReport>> GetPosTerminalPerformanceReportAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// گزارش عملکرد جلسات نقدی
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش عملکرد جلسات نقدی</returns>
        Task<ServiceResult<CashSessionPerformanceReport>> GetCashSessionPerformanceReportAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// گزارش زمان پاسخگویی
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش زمان پاسخگویی</returns>
        Task<ServiceResult<ResponseTimeReport>> GetResponseTimeReportAsync(DateTime startDate, DateTime endDate);

        #endregion

        #region Custom Reports

        /// <summary>
        /// گزارش سفارشی پرداخت‌ها
        /// </summary>
        /// <param name="request">درخواست گزارش سفارشی</param>
        /// <returns>گزارش سفارشی</returns>
        Task<ServiceResult<CustomPaymentReport>> GetCustomPaymentReportAsync(CustomPaymentReportRequest request);

        /// <summary>
        /// گزارش مقایسه‌ای پرداخت‌ها
        /// </summary>
        /// <param name="request">درخواست گزارش مقایسه‌ای</param>
        /// <returns>گزارش مقایسه‌ای</returns>
        Task<ServiceResult<ComparativePaymentReport>> GetComparativePaymentReportAsync(ComparativePaymentReportRequest request);

        /// <summary>
        /// گزارش روند پرداخت‌ها
        /// </summary>
        /// <param name="request">درخواست گزارش روند</param>
        /// <returns>گزارش روند</returns>
        Task<ServiceResult<TrendPaymentReport>> GetTrendPaymentReportAsync(TrendPaymentReportRequest request);

        #endregion

        #region Export Reports

        /// <summary>
        /// صادرات گزارش به Excel
        /// </summary>
        /// <param name="reportType">نوع گزارش</param>
        /// <param name="parameters">پارامترهای گزارش</param>
        /// <returns>فایل Excel</returns>
        Task<ServiceResult<byte[]>> ExportReportToExcelAsync(ReportType reportType, Dictionary<string, object> parameters);

        /// <summary>
        /// صادرات گزارش به PDF
        /// </summary>
        /// <param name="reportType">نوع گزارش</param>
        /// <param name="parameters">پارامترهای گزارش</param>
        /// <returns>فایل PDF</returns>
        Task<ServiceResult<byte[]>> ExportReportToPdfAsync(ReportType reportType, Dictionary<string, object> parameters);

        /// <summary>
        /// صادرات گزارش به CSV
        /// </summary>
        /// <param name="reportType">نوع گزارش</param>
        /// <param name="parameters">پارامترهای گزارش</param>
        /// <returns>فایل CSV</returns>
        Task<ServiceResult<byte[]>> ExportReportToCsvAsync(ReportType reportType, Dictionary<string, object> parameters);

        #endregion
    }

    #region Report Models

    /// <summary>
    /// گزارش پرداخت‌های روزانه
    /// </summary>
    public class DailyPaymentReport
    {
        public DateTime Date { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal PosAmount { get; set; }
        public decimal OnlineAmount { get; set; }
        public decimal DebtAmount { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public decimal SuccessRate { get; set; }
        public Dictionary<PaymentMethod, PaymentMethodSummary> PaymentMethodSummary { get; set; }
        public Dictionary<string, UserPaymentSummary> UserPaymentSummary { get; set; }
        public List<PaymentTransaction> TopTransactions { get; set; }
    }

    /// <summary>
    /// گزارش پرداخت‌های هفتگی
    /// </summary>
    public class WeeklyPaymentReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageDailyAmount { get; set; }
        public Dictionary<DateTime, DailyPaymentSummary> DailySummaries { get; set; }
        public Dictionary<PaymentMethod, PaymentMethodSummary> PaymentMethodSummary { get; set; }
        public List<PaymentTransaction> TopTransactions { get; set; }
    }

    /// <summary>
    /// گزارش پرداخت‌های ماهانه
    /// </summary>
    public class MonthlyPaymentReport
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageDailyAmount { get; set; }
        public Dictionary<DateTime, DailyPaymentSummary> DailySummaries { get; set; }
        public Dictionary<PaymentMethod, PaymentMethodSummary> PaymentMethodSummary { get; set; }
        public Dictionary<PaymentGatewayType, GatewayPaymentSummary> GatewaySummary { get; set; }
        public List<PaymentTransaction> TopTransactions { get; set; }
    }

    /// <summary>
    /// گزارش پرداخت‌های سالانه
    /// </summary>
    public class YearlyPaymentReport
    {
        public int Year { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageMonthlyAmount { get; set; }
        public Dictionary<int, MonthlyPaymentSummary> MonthlySummaries { get; set; }
        public Dictionary<PaymentMethod, PaymentMethodSummary> PaymentMethodSummary { get; set; }
        public Dictionary<PaymentGatewayType, GatewayPaymentSummary> GatewaySummary { get; set; }
        public List<PaymentTransaction> TopTransactions { get; set; }
    }

    /// <summary>
    /// گزارش پرداخت‌های بازه زمانی
    /// </summary>
    public class DateRangePaymentReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageDailyAmount { get; set; }
        public Dictionary<DateTime, DailyPaymentSummary> DailySummaries { get; set; }
        public Dictionary<PaymentMethod, PaymentMethodSummary> PaymentMethodSummary { get; set; }
        public Dictionary<PaymentGatewayType, GatewayPaymentSummary> GatewaySummary { get; set; }
        public List<PaymentTransaction> TopTransactions { get; set; }
    }

    /// <summary>
    /// آمار کلی پرداخت‌ها
    /// </summary>
    public class PaymentStatistics
    {
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public int CanceledTransactions { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal PosAmount { get; set; }
        public decimal OnlineAmount { get; set; }
        public decimal DebtAmount { get; set; }
        public decimal CashAmount { get; set; }
        public Dictionary<PaymentMethod, int> TransactionsByMethod { get; set; }
        public Dictionary<PaymentStatus, int> TransactionsByStatus { get; set; }
        public Dictionary<PaymentGatewayType, int> TransactionsByGateway { get; set; }
    }

    /// <summary>
    /// آمار بر اساس روش پرداخت
    /// </summary>
    public class PaymentMethodStatistics
    {
        public Dictionary<PaymentMethod, PaymentMethodSummary> MethodSummaries { get; set; }
        public PaymentMethod MostUsedMethod { get; set; }
        public PaymentMethod LeastUsedMethod { get; set; }
        public decimal TotalCashAmount { get; set; }
        public decimal TotalPosAmount { get; set; }
        public decimal TotalOnlineAmount { get; set; }
        public decimal TotalDebtAmount { get; set; }
    }

    /// <summary>
    /// آمار بر اساس وضعیت
    /// </summary>
    public class PaymentStatusStatistics
    {
        public Dictionary<PaymentStatus, PaymentStatusSummary> StatusSummaries { get; set; }
        public PaymentStatus MostCommonStatus { get; set; }
        public decimal OverallSuccessRate { get; set; }
        public decimal OverallFailureRate { get; set; }
    }

    /// <summary>
    /// آمار بر اساس کاربر
    /// </summary>
    public class UserPaymentStatistics
    {
        public Dictionary<string, UserPaymentSummary> UserSummaries { get; set; }
        public string TopUserByAmount { get; set; }
        public string TopUserByCount { get; set; }
        public decimal AverageAmountPerUser { get; set; }
        public int AverageTransactionsPerUser { get; set; }
    }

    /// <summary>
    /// گزارش مالی روزانه
    /// </summary>
    public class DailyFinancialReport
    {
        public DateTime Date { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public decimal GatewayFees { get; set; }
        public decimal CashBalance { get; set; }
        public Dictionary<PaymentMethod, decimal> IncomeByMethod { get; set; }
        public Dictionary<PaymentGatewayType, decimal> GatewayFeesByType { get; set; }
    }

    /// <summary>
    /// گزارش مالی ماهانه
    /// </summary>
    public class MonthlyFinancialReport
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public decimal GatewayFees { get; set; }
        public decimal AverageDailyIncome { get; set; }
        public Dictionary<DateTime, DailyFinancialSummary> DailySummaries { get; set; }
        public Dictionary<PaymentMethod, decimal> IncomeByMethod { get; set; }
        public Dictionary<PaymentGatewayType, decimal> GatewayFeesByType { get; set; }
    }

    /// <summary>
    /// گزارش درآمد و هزینه
    /// </summary>
    public class IncomeExpenseReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public decimal GatewayFees { get; set; }
        public decimal OperatingExpenses { get; set; }
        public decimal ProfitMargin { get; set; }
        public Dictionary<DateTime, DailyFinancialSummary> DailySummaries { get; set; }
    }

    /// <summary>
    /// گزارش کارمزد درگاه‌ها
    /// </summary>
    public class GatewayFeeReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalGatewayFees { get; set; }
        public Dictionary<PaymentGatewayType, GatewayFeeSummary> GatewayFeeSummaries { get; set; }
        public PaymentGatewayType HighestFeeGateway { get; set; }
        public PaymentGatewayType LowestFeeGateway { get; set; }
        public decimal AverageFeePercentage { get; set; }
    }

    /// <summary>
    /// گزارش عملکرد درگاه‌های پرداخت
    /// </summary>
    public class GatewayPerformanceReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<PaymentGatewayType, GatewayPerformanceSummary> GatewaySummaries { get; set; }
        public PaymentGatewayType BestPerformingGateway { get; set; }
        public PaymentGatewayType WorstPerformingGateway { get; set; }
        public decimal OverallSuccessRate { get; set; }
        public decimal OverallAverageResponseTime { get; set; }
    }

    /// <summary>
    /// گزارش عملکرد ترمینال‌های POS
    /// </summary>
    public class PosTerminalPerformanceReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<int, PosTerminalPerformanceSummary> TerminalSummaries { get; set; }
        public int BestPerformingTerminal { get; set; }
        public int WorstPerformingTerminal { get; set; }
        public decimal OverallSuccessRate { get; set; }
        public decimal OverallAverageResponseTime { get; set; }
    }

    /// <summary>
    /// گزارش عملکرد جلسات نقدی
    /// </summary>
    public class CashSessionPerformanceReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalSessions { get; set; }
        public int ActiveSessions { get; set; }
        public int CompletedSessions { get; set; }
        public decimal TotalCashHandled { get; set; }
        public decimal AverageSessionAmount { get; set; }
        public decimal AverageSessionDuration { get; set; }
        public Dictionary<string, UserCashSessionSummary> UserSummaries { get; set; }
    }

    /// <summary>
    /// گزارش زمان پاسخگویی
    /// </summary>
    public class ResponseTimeReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal AverageResponseTime { get; set; }
        public decimal MinResponseTime { get; set; }
        public decimal MaxResponseTime { get; set; }
        public Dictionary<PaymentMethod, decimal> ResponseTimeByMethod { get; set; }
        public Dictionary<PaymentGatewayType, decimal> ResponseTimeByGateway { get; set; }
        public List<ResponseTimeRecord> SlowestTransactions { get; set; }
    }

    /// <summary>
    /// گزارش سفارشی پرداخت‌ها
    /// </summary>
    public class CustomPaymentReport
    {
        public string ReportName { get; set; }
        public DateTime GeneratedAt { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public List<PaymentTransaction> Transactions { get; set; }
        public Dictionary<string, object> Summary { get; set; }
    }

    /// <summary>
    /// گزارش مقایسه‌ای پرداخت‌ها
    /// </summary>
    public class ComparativePaymentReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<DateTime, DailyPaymentSummary> CurrentPeriod { get; set; }
        public Dictionary<DateTime, DailyPaymentSummary> PreviousPeriod { get; set; }
        public decimal GrowthRate { get; set; }
        public decimal AmountDifference { get; set; }
        public int TransactionCountDifference { get; set; }
        public Dictionary<PaymentMethod, decimal> MethodGrowthRates { get; set; }
    }

    /// <summary>
    /// گزارش روند پرداخت‌ها
    /// </summary>
    public class TrendPaymentReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TrendType { get; set; }
        public Dictionary<DateTime, decimal> TrendData { get; set; }
        public decimal TrendSlope { get; set; }
        public string TrendDirection { get; set; }
        public decimal CorrelationCoefficient { get; set; }
    }

    #endregion

    #region Summary Models

    /// <summary>
    /// خلاصه پرداخت روزانه
    /// </summary>
    public class DailyPaymentSummary
    {
        public DateTime Date { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal PosAmount { get; set; }
        public decimal OnlineAmount { get; set; }
        public decimal DebtAmount { get; set; }
    }

    /// <summary>
    /// خلاصه پرداخت ماهانه
    /// </summary>
    public class MonthlyPaymentSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal PosAmount { get; set; }
        public decimal OnlineAmount { get; set; }
        public decimal DebtAmount { get; set; }
    }

    /// <summary>
    /// خلاصه روش پرداخت
    /// </summary>
    public class PaymentMethodSummary
    {
        public PaymentMethod Method { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// خلاصه وضعیت پرداخت
    /// </summary>
    public class PaymentStatusSummary
    {
        public PaymentStatus Status { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// خلاصه کاربر پرداخت
    /// </summary>
    public class UserPaymentSummary
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public DateTime LastTransactionDate { get; set; }
    }

    /// <summary>
    /// خلاصه درگاه پرداخت
    /// </summary>
    public class GatewayPaymentSummary
    {
        public PaymentGatewayType GatewayType { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageResponseTime { get; set; }
    }

    /// <summary>
    /// خلاصه مالی روزانه
    /// </summary>
    public class DailyFinancialSummary
    {
        public DateTime Date { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal NetIncome { get; set; }
        public decimal GatewayFees { get; set; }
    }

    /// <summary>
    /// خلاصه کارمزد درگاه
    /// </summary>
    public class GatewayFeeSummary
    {
        public PaymentGatewayType GatewayType { get; set; }
        public decimal TotalFees { get; set; }
        public decimal FeePercentage { get; set; }
        public decimal FixedFee { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageFeePerTransaction { get; set; }
    }

    /// <summary>
    /// خلاصه عملکرد درگاه
    /// </summary>
    public class GatewayPerformanceSummary
    {
        public PaymentGatewayType GatewayType { get; set; }
        public int TransactionCount { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageResponseTime { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
    }

    /// <summary>
    /// خلاصه عملکرد ترمینال POS
    /// </summary>
    public class PosTerminalPerformanceSummary
    {
        public int TerminalId { get; set; }
        public string TerminalName { get; set; }
        public int TransactionCount { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageResponseTime { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
    }

    /// <summary>
    /// خلاصه جلسه نقدی کاربر
    /// </summary>
    public class UserCashSessionSummary
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int SessionCount { get; set; }
        public decimal TotalCashHandled { get; set; }
        public decimal AverageSessionAmount { get; set; }
        public decimal AverageSessionDuration { get; set; }
    }

    /// <summary>
    /// رکورد زمان پاسخگویی
    /// </summary>
    public class ResponseTimeRecord
    {
        public int TransactionId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal ResponseTime { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    #endregion

    #region Request Models

    /// <summary>
    /// درخواست گزارش سفارشی
    /// </summary>
    public class CustomPaymentReportRequest
    {
        public string ReportName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<PaymentMethod> PaymentMethods { get; set; }
        public List<PaymentStatus> PaymentStatuses { get; set; }
        public List<PaymentGatewayType> PaymentGateways { get; set; }
        public List<string> UserIds { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public Dictionary<string, object> AdditionalFilters { get; set; }
    }

    /// <summary>
    /// درخواست گزارش مقایسه‌ای
    /// </summary>
    public class ComparativePaymentReportRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime PreviousStartDate { get; set; }
        public DateTime PreviousEndDate { get; set; }
        public List<PaymentMethod> PaymentMethods { get; set; }
        public List<PaymentGatewayType> PaymentGateways { get; set; }
    }

    /// <summary>
    /// درخواست گزارش روند
    /// </summary>
    public class TrendPaymentReportRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TrendType { get; set; }
        public List<PaymentMethod> PaymentMethods { get; set; }
        public List<PaymentGatewayType> PaymentGateways { get; set; }
    }

  

    #endregion
}
