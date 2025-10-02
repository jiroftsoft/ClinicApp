using ClinicApp.Interfaces.Payment.Reporting;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Payment.Gateway;
using ClinicApp.Models.Enums;
using PaymentStatistics = ClinicApp.Models.Statistics.PaymentStatistics;

namespace ClinicApp.Services.Payment.Reporting
{
    /// <summary>
    /// Service برای گزارش‌گیری از پرداخت‌ها
    /// طراحی شده طبق اصول SRP - مسئولیت: گزارش‌گیری و تحلیل منطق کسب‌وکار پرداخت‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. گزارش‌گیری کامل از پرداخت‌ها
    /// 2. آمار و تحلیل‌های پرداخت
    /// 3. گزارش‌های مالی و مدیریتی
    /// 4. گزارش‌های عملکردی
    /// 5. بهینه‌سازی برای عملکرد بالا
    /// </summary>
    public class PaymentReportingService : IPaymentReportingService
    {
        #region Fields

        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IPaymentGatewayRepository _paymentGatewayRepository;
        private readonly IPosTerminalRepository _posTerminalRepository;
        private readonly ICashSessionRepository _cashSessionRepository;
        private readonly IOnlinePaymentRepository _onlinePaymentRepository;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public PaymentReportingService(
            IPaymentTransactionRepository paymentTransactionRepository,
            IPaymentGatewayRepository paymentGatewayRepository,
            IPosTerminalRepository posTerminalRepository,
            ICashSessionRepository cashSessionRepository,
            IOnlinePaymentRepository onlinePaymentRepository,
            ILogger logger)
        {
            _paymentTransactionRepository = paymentTransactionRepository ?? throw new ArgumentNullException(nameof(paymentTransactionRepository));
            _paymentGatewayRepository = paymentGatewayRepository ?? throw new ArgumentNullException(nameof(paymentGatewayRepository));
            _posTerminalRepository = posTerminalRepository ?? throw new ArgumentNullException(nameof(posTerminalRepository));
            _cashSessionRepository = cashSessionRepository ?? throw new ArgumentNullException(nameof(cashSessionRepository));
            _onlinePaymentRepository = onlinePaymentRepository ?? throw new ArgumentNullException(nameof(onlinePaymentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Payment Reports

        /// <summary>
        /// گزارش پرداخت‌های روزانه
        /// </summary>
        public async Task<ServiceResult<DailyPaymentReport>> GetDailyPaymentReportAsync(DateTime date)
        {
            try
            {
                _logger.Information("شروع تولید گزارش پرداخت‌های روزانه برای تاریخ {Date}", date.ToString("yyyy-MM-dd"));

                var startDate = date.Date;
                var endDate = startDate.AddDays(1).AddTicks(-1);

                // دریافت تراکنش‌های روز
                var transactions = (await _paymentTransactionRepository.GetByDateRangeAsync(startDate, endDate)).ToList();

                // محاسبه آمار
                var report = new DailyPaymentReport
                {
                    Date = date,
                    TotalTransactions = transactions.Count,
                    TotalAmount = transactions.Sum(t => t.Amount),
                    CashAmount = transactions.Where(t => t.Method == PaymentMethod.Cash).Sum(t => t.Amount),
                    PosAmount = transactions.Where(t => t.Method == PaymentMethod.POS).Sum(t => t.Amount),
                    OnlineAmount = transactions.Where(t => t.Method == PaymentMethod.Online).Sum(t => t.Amount),
                    DebtAmount = transactions.Where(t => t.Method == PaymentMethod.Debt).Sum(t => t.Amount),
                    SuccessfulTransactions = transactions.Count(t => t.Status == PaymentStatus.Success),
                    FailedTransactions = transactions.Count(t => t.Status == PaymentStatus.Failed),
                    PendingTransactions = transactions.Count(t => t.Status == PaymentStatus.Pending),
                    SuccessRate = transactions.Count > 0 ? (decimal)transactions.Count(t => t.Status == PaymentStatus.Success) / transactions.Count * 100 : 0,
                    PaymentMethodSummary = CalculatePaymentMethodSummary(transactions),
                    UserPaymentSummary = CalculateUserPaymentSummary(transactions),
                    TopTransactions = transactions.OrderByDescending(t => t.Amount).Take(10).ToList()
                };

                _logger.Information("گزارش پرداخت‌های روزانه با موفقیت تولید شد. تعداد تراکنش‌ها: {Count}", report.TotalTransactions);
                return ServiceResult<DailyPaymentReport>.Successful(report, "گزارش پرداخت‌های روزانه با موفقیت تولید شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تولید گزارش پرداخت‌های روزانه برای تاریخ {Date}", date.ToString("yyyy-MM-dd"));
                return ServiceResult<DailyPaymentReport>.Failed("خطا در تولید گزارش پرداخت‌های روزانه");
            }
        }

        /// <summary>
        /// گزارش پرداخت‌های هفتگی
        /// </summary>
        public async Task<ServiceResult<WeeklyPaymentReport>> GetWeeklyPaymentReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Information("شروع تولید گزارش پرداخت‌های هفتگی از {StartDate} تا {EndDate}", 
                    startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

                // دریافت تراکنش‌های هفته
                var transactions = (await _paymentTransactionRepository.GetByDateRangeAsync(startDate, endDate)).ToList();

                // محاسبه آمار روزانه
                var dailySummaries = new Dictionary<DateTime, DailyPaymentSummary>();
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    var dayTransactions = transactions.Where(t => t.CreatedAt.Date == date).ToList();
                    dailySummaries[date] = new DailyPaymentSummary
                    {
                        Date = date,
                        TransactionCount = dayTransactions.Count,
                        TotalAmount = dayTransactions.Sum(t => t.Amount),
                        CashAmount = dayTransactions.Where(t => t.Method == PaymentMethod.Cash).Sum(t => t.Amount),
                        PosAmount = dayTransactions.Where(t => t.Method == PaymentMethod.POS).Sum(t => t.Amount),
                        OnlineAmount = dayTransactions.Where(t => t.Method == PaymentMethod.Online).Sum(t => t.Amount),
                        DebtAmount = dayTransactions.Where(t => t.Method == PaymentMethod.Debt).Sum(t => t.Amount)
                    };
                }

                var report = new WeeklyPaymentReport
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalTransactions = transactions.Count,
                    TotalAmount = transactions.Sum(t => t.Amount),
                    AverageDailyAmount = transactions.Count > 0 ? transactions.Sum(t => t.Amount) / (endDate.Date - startDate.Date).Days : 0,
                    DailySummaries = dailySummaries,
                    PaymentMethodSummary = CalculatePaymentMethodSummary(transactions),
                    TopTransactions = transactions.OrderByDescending(t => t.Amount).Take(10).ToList()
                };

                _logger.Information("گزارش پرداخت‌های هفتگی با موفقیت تولید شد. تعداد تراکنش‌ها: {Count}", report.TotalTransactions);
                return ServiceResult<WeeklyPaymentReport>.Successful(report, "گزارش پرداخت‌های هفتگی با موفقیت تولید شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تولید گزارش پرداخت‌های هفتگی");
                return ServiceResult<WeeklyPaymentReport>.Failed("خطا در تولید گزارش پرداخت‌های هفتگی");
            }
        }

        /// <summary>
        /// گزارش پرداخت‌های ماهانه
        /// </summary>
        public async Task<ServiceResult<MonthlyPaymentReport>> GetMonthlyPaymentReportAsync(int year, int month)
        {
            try
            {
                _logger.Information("شروع تولید گزارش پرداخت‌های ماهانه برای {Year}/{Month}", year, month);

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddTicks(-1);

                // دریافت تراکنش‌های ماه
                var transactions = (await _paymentTransactionRepository.GetByDateRangeAsync(startDate, endDate)).ToList();

                // محاسبه آمار روزانه
                var dailySummaries = new Dictionary<DateTime, DailyPaymentSummary>();
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var dayTransactions = transactions.Where(t => t.CreatedAt.Date == date.Date).ToList();
                    dailySummaries[date] = new DailyPaymentSummary
                    {
                        Date = date,
                        TransactionCount = dayTransactions.Count,
                        TotalAmount = dayTransactions.Sum(t => t.Amount),
                        CashAmount = dayTransactions.Where(t => t.Method == PaymentMethod.Cash).Sum(t => t.Amount),
                        PosAmount = dayTransactions.Where(t => t.Method == PaymentMethod.POS).Sum(t => t.Amount),
                        OnlineAmount = dayTransactions.Where(t => t.Method == PaymentMethod.Online).Sum(t => t.Amount),
                        DebtAmount = dayTransactions.Where(t => t.Method == PaymentMethod.Debt).Sum(t => t.Amount)
                    };
                }

                var report = new MonthlyPaymentReport
                {
                    Year = year,
                    Month = month,
                    TotalTransactions = transactions.Count,
                    TotalAmount = transactions.Sum(t => t.Amount),
                    AverageDailyAmount = transactions.Count > 0 ? transactions.Sum(t => t.Amount) / DateTime.DaysInMonth(year, month) : 0,
                    DailySummaries = dailySummaries,
                    PaymentMethodSummary = CalculatePaymentMethodSummary(transactions),
                    GatewaySummary = CalculateGatewaySummary(transactions),
                    TopTransactions = transactions.OrderByDescending(t => t.Amount).Take(10).ToList()
                };

                _logger.Information("گزارش پرداخت‌های ماهانه با موفقیت تولید شد. تعداد تراکنش‌ها: {Count}", report.TotalTransactions);
                return ServiceResult<MonthlyPaymentReport>.Successful(report, "گزارش پرداخت‌های ماهانه با موفقیت تولید شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تولید گزارش پرداخت‌های ماهانه برای {Year}/{Month}", year, month);
                return ServiceResult<MonthlyPaymentReport>.Failed("خطا در تولید گزارش پرداخت‌های ماهانه");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// محاسبه خلاصه روش‌های پرداخت
        /// </summary>
        private Dictionary<PaymentMethod, PaymentMethodSummary> CalculatePaymentMethodSummary(List<PaymentTransaction> transactions)
        {
            var summary = new Dictionary<PaymentMethod, PaymentMethodSummary>();
            var totalAmount = transactions.Sum(t => t.Amount);

            foreach (PaymentMethod method in Enum.GetValues(typeof(PaymentMethod)))
            {
                var methodTransactions = transactions.Where(t => t.Method == method).ToList();
                if (methodTransactions.Any())
                {
                    summary[method] = new PaymentMethodSummary
                    {
                        Method = method,
                        TransactionCount = methodTransactions.Count,
                        TotalAmount = methodTransactions.Sum(t => t.Amount),
                        AverageAmount = methodTransactions.Average(t => t.Amount),
                        Percentage = totalAmount > 0 ? (methodTransactions.Sum(t => t.Amount) / totalAmount) * 100 : 0
                    };
                }
            }

            return summary;
        }

        /// <summary>
        /// محاسبه خلاصه کاربران پرداخت
        /// </summary>
        private Dictionary<string, UserPaymentSummary> CalculateUserPaymentSummary(List<PaymentTransaction> transactions)
        {
            var summary = new Dictionary<string, UserPaymentSummary>();

            var userGroups = transactions.GroupBy(t => t.CreatedByUserId);
            foreach (var group in userGroups)
            {
                var userTransactions = group.ToList();
                summary[group.Key] = new UserPaymentSummary
                {
                    UserId = group.Key,
                    UserName = group.Key, // TODO: دریافت نام کاربر از سرویس کاربران
                    TransactionCount = userTransactions.Count,
                    TotalAmount = userTransactions.Sum(t => t.Amount),
                    AverageAmount = userTransactions.Average(t => t.Amount),
                    LastTransactionDate = userTransactions.Max(t => t.CreatedAt)
                };
            }

            return summary;
        }

        /// <summary>
        /// محاسبه خلاصه درگاه‌ها
        /// </summary>
        private Dictionary<PaymentGatewayType, GatewayPaymentSummary> CalculateGatewaySummary(List<PaymentTransaction> transactions)
        {
            var summary = new Dictionary<PaymentGatewayType, GatewayPaymentSummary>();

            var gatewayGroups = transactions.Where(t => t.PaymentGatewayId.HasValue).GroupBy(t => t.PaymentGateway?.GatewayType);
            foreach (var group in gatewayGroups)
            {
                if (group.Key.HasValue)
                {
                    var gatewayTransactions = group.ToList();
                    summary[group.Key.Value] = new GatewayPaymentSummary
                    {
                        GatewayType = group.Key.Value,
                        TransactionCount = gatewayTransactions.Count,
                        TotalAmount = gatewayTransactions.Sum(t => t.Amount),
                        AverageAmount = gatewayTransactions.Average(t => t.Amount),
                        SuccessRate = gatewayTransactions.Count > 0 ? (decimal)gatewayTransactions.Count(t => t.Status == PaymentStatus.Completed) / gatewayTransactions.Count * 100 : 0,
                        AverageResponseTime = 0 // TODO: محاسبه زمان پاسخگویی
                    };
                }
            }

            return summary;
        }

        #endregion

        #region Placeholder Methods (To be implemented in next parts)

        public async Task<ServiceResult<YearlyPaymentReport>> GetYearlyPaymentReportAsync(int year)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetYearlyPaymentReportAsync will be implemented in next part");
        }

        public async Task<ServiceResult<DateRangePaymentReport>> GetDateRangePaymentReportAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetDateRangePaymentReportAsync will be implemented in next part");
        }

        Task<ServiceResult<PaymentStatistics>> IPaymentReportingService.GetPaymentStatisticsAsync(DateTime startDate, DateTime endDate, PaymentMethod? paymentMethod, PaymentStatus? paymentStatus)
        {
            return GetPaymentStatisticsAsync(startDate, endDate, paymentMethod, paymentStatus);
        }

        public async Task<ServiceResult<PaymentStatistics>> GetPaymentStatisticsAsync(DateTime startDate, DateTime endDate, PaymentMethod? paymentMethod = null, PaymentStatus? paymentStatus = null)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetPaymentStatisticsAsync will be implemented in next part");
        }

        public async Task<ServiceResult<PaymentMethodStatistics>> GetPaymentMethodStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetPaymentMethodStatisticsAsync will be implemented in next part");
        }

        public async Task<ServiceResult<PaymentStatusStatistics>> GetPaymentStatusStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetPaymentStatusStatisticsAsync will be implemented in next part");
        }

        public async Task<ServiceResult<UserPaymentStatistics>> GetUserPaymentStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetUserPaymentStatisticsAsync will be implemented in next part");
        }

        public async Task<ServiceResult<DailyFinancialReport>> GetDailyFinancialReportAsync(DateTime date)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetDailyFinancialReportAsync will be implemented in next part");
        }

        public async Task<ServiceResult<MonthlyFinancialReport>> GetMonthlyFinancialReportAsync(int year, int month)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetMonthlyFinancialReportAsync will be implemented in next part");
        }

        public async Task<ServiceResult<IncomeExpenseReport>> GetIncomeExpenseReportAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetIncomeExpenseReportAsync will be implemented in next part");
        }

        public async Task<ServiceResult<GatewayFeeReport>> GetGatewayFeeReportAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetGatewayFeeReportAsync will be implemented in next part");
        }

        public async Task<ServiceResult<GatewayPerformanceReport>> GetGatewayPerformanceReportAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetGatewayPerformanceReportAsync will be implemented in next part");
        }

        public async Task<ServiceResult<PosTerminalPerformanceReport>> GetPosTerminalPerformanceReportAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetPosTerminalPerformanceReportAsync will be implemented in next part");
        }

        public async Task<ServiceResult<CashSessionPerformanceReport>> GetCashSessionPerformanceReportAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetCashSessionPerformanceReportAsync will be implemented in next part");
        }

        public async Task<ServiceResult<ResponseTimeReport>> GetResponseTimeReportAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetResponseTimeReportAsync will be implemented in next part");
        }

        public async Task<ServiceResult<CustomPaymentReport>> GetCustomPaymentReportAsync(CustomPaymentReportRequest request)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetCustomPaymentReportAsync will be implemented in next part");
        }

        public async Task<ServiceResult<ComparativePaymentReport>> GetComparativePaymentReportAsync(ComparativePaymentReportRequest request)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetComparativePaymentReportAsync will be implemented in next part");
        }

        public async Task<ServiceResult<TrendPaymentReport>> GetTrendPaymentReportAsync(TrendPaymentReportRequest request)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetTrendPaymentReportAsync will be implemented in next part");
        }


        Task<ServiceResult<byte[]>> IPaymentReportingService.ExportReportToPdfAsync(ReportType reportType, Dictionary<string, object> parameters)
        {
            return ExportReportToPdfAsync(reportType, parameters);
        }

        Task<ServiceResult<byte[]>> IPaymentReportingService.ExportReportToCsvAsync(ReportType reportType, Dictionary<string, object> parameters)
        {
            return ExportReportToCsvAsync(reportType, parameters);
        }

        public async Task<ServiceResult<byte[]>> ExportReportToExcelAsync(ReportType reportType, Dictionary<string, object> parameters)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("ExportReportToExcelAsync will be implemented in next part");
        }

        public async Task<ServiceResult<byte[]>> ExportReportToPdfAsync(ReportType reportType, Dictionary<string, object> parameters)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("ExportReportToPdfAsync will be implemented in next part");
        }

        public async Task<ServiceResult<byte[]>> ExportReportToCsvAsync(ReportType reportType, Dictionary<string, object> parameters)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("ExportReportToCsvAsync will be implemented in next part");
        }

        #endregion
    }
}
