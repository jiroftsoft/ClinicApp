using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Admin;
using OfficeOpenXml;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// پیاده‌سازی داشبورد درآمد — واکشی از تراکنش‌ها و پذیرش، محاسبه KPI و داده نمودار برای تصمیم‌گیری مالی
    /// </summary>
    public class RevenueDashboardService : IRevenueDashboardService
    {
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IReceptionRepository _receptionRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        private const int MaxChartDays = 31;

        public RevenueDashboardService(
            IPaymentTransactionRepository paymentTransactionRepository,
            IReceptionRepository receptionRepository,
            ApplicationDbContext context,
            ILogger logger)
        {
            _paymentTransactionRepository = paymentTransactionRepository ?? throw new ArgumentNullException(nameof(paymentTransactionRepository));
            _receptionRepository = receptionRepository ?? throw new ArgumentNullException(nameof(receptionRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<RevenueDashboardViewModel>> GetDashboardAsync(RevenueDashboardFilterViewModel filter)
        {
            try
            {
                var (start, end) = ResolveDateRange(filter);
                if (start == null || end == null)
                {
                    return ServiceResult<RevenueDashboardViewModel>.Failed("بازه تاریخ معتبر نیست.", "VALIDATION");
                }

                var summaryResult = await GetSummaryAsync(filter);
                if (!summaryResult.Success)
                    return ServiceResult<RevenueDashboardViewModel>.Failed(summaryResult.Message, summaryResult.Code);

                var chartResult = await GetChartDataAsync(filter);
                if (!chartResult.Success)
                    return ServiceResult<RevenueDashboardViewModel>.Failed(chartResult.Message, chartResult.Code);

                var doctorRevenues = await GetDoctorRevenuesAsync(start.Value, end.Value, filter.DoctorId, filter.DepartmentId);
                var dailyTrend = await GetDailyTrendAsync(start.Value, end.Value);

                var model = new RevenueDashboardViewModel
                {
                    Filter = filter,
                    Summary = summaryResult.Data,
                    ChartData = chartResult.Data,
                    DoctorRevenues = doctorRevenues,
                    DailyTrend = dailyTrend
                };

                return ServiceResult<RevenueDashboardViewModel>.Successful(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در واکشی داشبورد درآمد");
                return ServiceResult<RevenueDashboardViewModel>.Failed("خطا در بارگذاری داشبورد درآمد.", "EXCEPTION");
            }
        }

        public async Task<ServiceResult<RevenueSummaryViewModel>> GetSummaryAsync(RevenueDashboardFilterViewModel filter)
        {
            try
            {
                var (start, end) = ResolveDateRange(filter);
                if (start == null || end == null)
                    return ServiceResult<RevenueSummaryViewModel>.Failed("بازه تاریخ معتبر نیست.", "VALIDATION");

                decimal totalRevenue;
                int totalTransactions;
                decimal cashRevenue;
                decimal posRevenue;
                decimal onlineRevenue;
                int receptionCount;

                if (HasFilter(filter))
                {
                    var baseQuery = GetFilteredTransactionsQuery(start.Value, end.Value, filter.DoctorId, filter.DepartmentId, filter.PaymentMethod);
                    totalRevenue = await baseQuery.SumAsync(pt => pt.Amount);
                    totalTransactions = await baseQuery.CountAsync();
                    cashRevenue = await baseQuery.Where(pt => pt.Method == PaymentMethod.Cash).SumAsync(pt => pt.Amount);
                    posRevenue = await baseQuery.Where(pt => pt.Method == PaymentMethod.POS).SumAsync(pt => pt.Amount);
                    onlineRevenue = await baseQuery.Where(pt => pt.Method == PaymentMethod.Online).SumAsync(pt => pt.Amount);
                    receptionCount = await baseQuery.Select(pt => pt.ReceptionId).Distinct().CountAsync();
                }
                else
                {
                    var stats = await _paymentTransactionRepository.GetStatisticsAsync(start.Value, end.Value);
                    var cashStats = await _paymentTransactionRepository.GetStatisticsByPaymentMethodAsync(PaymentMethod.Cash, start.Value, end.Value);
                    var posStats = await _paymentTransactionRepository.GetStatisticsByPaymentMethodAsync(PaymentMethod.POS, start.Value, end.Value);
                    var onlineStats = await _paymentTransactionRepository.GetStatisticsByPaymentMethodAsync(PaymentMethod.Online, start.Value, end.Value);
                    totalRevenue = stats.SuccessfulAmount;
                    totalTransactions = stats.SuccessfulTransactions;
                    cashRevenue = cashStats.SuccessfulAmount;
                    posRevenue = posStats.SuccessfulAmount;
                    onlineRevenue = onlineStats.SuccessfulAmount;
                    var receptions = await _receptionRepository.GetByDateRangeAsync(start.Value, end.Value);
                    receptionCount = receptions?.Count ?? 0;
                }

                var otherRevenue = totalRevenue - cashRevenue - posRevenue - onlineRevenue;
                if (otherRevenue < 0) otherRevenue = 0;

                var days = (end.Value - start.Value).Days + 1;
                var previousStart = start.Value.AddDays(-days);
                var previousEnd = start.Value.AddDays(-1);
                decimal previousRevenue;
                if (HasFilter(filter))
                {
                    var prevQuery = GetFilteredTransactionsQuery(previousStart, previousEnd, filter.DoctorId, filter.DepartmentId, filter.PaymentMethod);
                    previousRevenue = await prevQuery.SumAsync(pt => pt.Amount);
                }
                else
                {
                    var prevStats = await _paymentTransactionRepository.GetStatisticsAsync(previousStart, previousEnd);
                    previousRevenue = prevStats.SuccessfulAmount;
                }

                var growthRate = previousRevenue > 0
                    ? Math.Round((decimal)((totalRevenue - previousRevenue) / previousRevenue * 100), 1)
                    : (totalRevenue > 0 ? 100m : 0m);

                var summary = new RevenueSummaryViewModel
                {
                    TotalRevenue = totalRevenue,
                    CashRevenue = cashRevenue,
                    PosRevenue = posRevenue,
                    OnlineRevenue = onlineRevenue,
                    OtherRevenue = otherRevenue,
                    TotalTransactions = totalTransactions,
                    ReceptionCount = receptionCount,
                    AverageTransactionAmount = totalTransactions > 0 ? Math.Round(totalRevenue / totalTransactions, 0) : 0,
                    GrowthRatePercent = growthRate,
                    PreviousPeriodRevenue = previousRevenue,
                    PeriodLabel = PersianDateHelper.ToPersianDate(start.Value) + " تا " + PersianDateHelper.ToPersianDate(end.Value)
                };

                return ServiceResult<RevenueSummaryViewModel>.Successful(summary);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه خلاصه درآمد");
                return ServiceResult<RevenueSummaryViewModel>.Failed("خطا در محاسبه خلاصه درآمد.", "EXCEPTION");
            }
        }

        public async Task<ServiceResult<RevenueChartDataViewModel>> GetChartDataAsync(RevenueDashboardFilterViewModel filter)
        {
            try
            {
                var (start, end) = ResolveDateRange(filter);
                if (start == null || end == null)
                    return ServiceResult<RevenueChartDataViewModel>.Failed("بازه تاریخ معتبر نیست.", "VALIDATION");

                var chartEnd = end.Value;
                var days = (chartEnd - start.Value).Days + 1;
                if (days > MaxChartDays)
                    chartEnd = start.Value.AddDays(MaxChartDays - 1);

                var labels = new List<string>();
                var dailyValues = new List<decimal>();
                var cashValues = new List<decimal>();
                var posValues = new List<decimal>();
                var onlineValues = new List<decimal>();

                if (HasFilter(filter))
                {
                    for (var d = start.Value.Date; d <= chartEnd.Date; d = d.AddDays(1))
                    {
                        var dayStart = d.Date;
                        var dayEnd = d.Date.AddDays(1).AddSeconds(-1);
                        var dayQuery = GetFilteredTransactionsQuery(dayStart, dayEnd, filter.DoctorId, filter.DepartmentId, filter.PaymentMethod);
                        var dayTotal = await dayQuery.SumAsync(pt => pt.Amount);
                        var dayCash = await dayQuery.Where(pt => pt.Method == PaymentMethod.Cash).SumAsync(pt => pt.Amount);
                        var dayPos = await dayQuery.Where(pt => pt.Method == PaymentMethod.POS).SumAsync(pt => pt.Amount);
                        var dayOnline = await dayQuery.Where(pt => pt.Method == PaymentMethod.Online).SumAsync(pt => pt.Amount);
                        labels.Add(PersianDateHelper.ToPersianDate(d));
                        dailyValues.Add(dayTotal);
                        cashValues.Add(dayCash);
                        posValues.Add(dayPos);
                        onlineValues.Add(dayOnline);
                    }
                }
                else
                {
                    for (var d = start.Value.Date; d <= chartEnd.Date; d = d.AddDays(1))
                    {
                        var dayStats = await _paymentTransactionRepository.GetDailyStatisticsAsync(d);
                        labels.Add(PersianDateHelper.ToPersianDate(d));
                        dailyValues.Add(dayStats.SuccessfulAmount);
                        cashValues.Add(dayStats.CashAmount);
                        posValues.Add(dayStats.PosAmount);
                        onlineValues.Add(dayStats.OnlineAmount);
                    }
                }

                var chart = new RevenueChartDataViewModel
                {
                    Labels = labels,
                    DailyValues = dailyValues,
                    CashValues = cashValues,
                    PosValues = posValues,
                    OnlineValues = onlineValues
                };

                return ServiceResult<RevenueChartDataViewModel>.Successful(chart);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در واکشی داده نمودار درآمد");
                return ServiceResult<RevenueChartDataViewModel>.Failed("خطا در داده نمودار.", "EXCEPTION");
            }
        }

        public async Task<ServiceResult<byte[]>> ExportToExcelAsync(RevenueDashboardFilterViewModel filter)
        {
            try
            {
                var (start, end) = ResolveDateRange(filter);
                if (start == null || end == null)
                    return ServiceResult<byte[]>.Failed("بازه تاریخ معتبر نیست.", "VALIDATION");

                List<PaymentTransaction> list;
                if (HasFilter(filter))
                {
                    var query = GetFilteredTransactionsQuery(start.Value, end.Value, filter.DoctorId, filter.DepartmentId, filter.PaymentMethod)
                        .OrderByDescending(pt => pt.CreatedAt)
                        .Take(10000);
                    list = await query.ToListAsync();
                }
                else
                {
                    var transactions = await _paymentTransactionRepository.GetByDateRangeAsync(start.Value, end.Value, 1, 10000);
                    list = transactions?.Where(t => t.Status == PaymentStatus.Success && !t.IsDeleted).ToList() ?? new List<PaymentTransaction>();
                }

                using (var package = new ExcelPackage())
                {
                    var ws = package.Workbook.Worksheets.Add("درآمد");
                    ws.Cells[1, 1].Value = "تاریخ";
                    ws.Cells[1, 2].Value = "تاریخ شمسی";
                    ws.Cells[1, 3].Value = "روش پرداخت";
                    ws.Cells[1, 4].Value = "مبلغ";
                    ws.Cells[1, 5].Value = "پزشک";
                    ws.Cells[1, 6].Value = "شناسه پذیرش";
                    var row = 2;
                    foreach (var t in list.OrderByDescending(x => x.CreatedAt))
                    {
                        ws.Cells[row, 1].Value = t.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                        ws.Cells[row, 2].Value = PersianDateHelper.ToPersianDate(t.CreatedAt);
                        ws.Cells[row, 3].Value = t.Method.ToString();
                        ws.Cells[row, 4].Value = t.Amount;
                        ws.Cells[row, 5].Value = t.DoctorName ?? "";
                        ws.Cells[row, 6].Value = t.ReceptionId;
                        row++;
                    }
                    ws.Cells.AutoFitColumns();
                    return ServiceResult<byte[]>.Successful(package.GetAsByteArray());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در خروجی Excel داشبورد درآمد");
                return ServiceResult<byte[]>.Failed("خطا در خروجی Excel.", "EXCEPTION");
            }
        }

        private static bool HasFilter(RevenueDashboardFilterViewModel filter)
        {
            return filter.DoctorId.HasValue || filter.DepartmentId.HasValue ||
                   !string.IsNullOrWhiteSpace(filter.PaymentMethod);
        }

        private IQueryable<PaymentTransaction> GetFilteredTransactionsQuery(
            DateTime start,
            DateTime end,
            int? doctorId,
            int? departmentId,
            string paymentMethod)
        {
            var endInclusive = end.Date.AddDays(1).AddSeconds(-1);
            var query = _context.PaymentTransactions
                .AsNoTracking()
                .Where(pt => !pt.IsDeleted && pt.Status == PaymentStatus.Success &&
                             pt.CreatedAt >= start && pt.CreatedAt <= endInclusive &&
                             pt.ReceptionId != 0);

            if (doctorId.HasValue)
                query = query.Where(pt => pt.Reception.DoctorId == doctorId.Value);
            if (departmentId.HasValue)
                query = query.Where(pt => pt.Reception.DepartmentId == departmentId.Value);
            if (!string.IsNullOrWhiteSpace(paymentMethod) &&
                Enum.TryParse(paymentMethod, true, out PaymentMethod pm))
                query = query.Where(pt => pt.Method == pm);

            return query;
        }

        private (DateTime? start, DateTime? end) ResolveDateRange(RevenueDashboardFilterViewModel filter)
        {
            if (filter.StartDate.HasValue && filter.EndDate.HasValue)
            {
                if (filter.StartDate.Value <= filter.EndDate.Value)
                    return (filter.StartDate.Value.Date, filter.EndDate.Value.Date.AddDays(1).AddSeconds(-1));
                return (null, null);
            }
            if (!string.IsNullOrWhiteSpace(filter.StartDatePersian) && !string.IsNullOrWhiteSpace(filter.EndDatePersian))
            {
                var start = PersianDateHelper.ParsePersianDate(filter.StartDatePersian.Trim());
                var end = PersianDateHelper.ParsePersianDate(filter.EndDatePersian.Trim());
                if (start.HasValue && end.HasValue && start.Value <= end.Value)
                    return (start.Value.Date, end.Value.Date.AddDays(1).AddSeconds(-1));
                return (null, null);
            }
            var defEnd = DateTime.Now.Date;
            var defStart = new DateTime(defEnd.Year, defEnd.Month, 1);
            return (defStart, defEnd.AddDays(1).AddSeconds(-1));
        }

        private async Task<List<DoctorRevenueItemViewModel>> GetDoctorRevenuesAsync(DateTime start, DateTime end, int? doctorId, int? departmentId)
        {
            try
            {
                var query = _context.PaymentTransactions
                    .AsNoTracking()
                    .Where(pt => !pt.IsDeleted && pt.Status == PaymentStatus.Success && pt.CreatedAt >= start && pt.CreatedAt <= end && pt.ReceptionId != 0);

                if (doctorId.HasValue)
                    query = query.Where(pt => pt.Reception.DoctorId == doctorId.Value);
                if (departmentId.HasValue)
                    query = query.Where(pt => pt.Reception.DepartmentId == departmentId.Value);

                var grouped = await query
                    .GroupBy(pt => pt.Reception.DoctorId)
                    .Select(g => new { DoctorId = g.Key, Total = g.Sum(t => t.Amount), Count = g.Count() })
                    .ToListAsync();

                var totalSum = grouped.Sum(x => x.Total);
                var doctorIds = grouped.Select(x => x.DoctorId).Distinct().ToList();
                var doctorNames = await _context.Doctors
                    .AsNoTracking()
                    .Where(d => doctorIds.Contains(d.DoctorId))
                    .Select(d => new { d.DoctorId, Name = d.FirstName + " " + d.LastName })
                    .ToDictionaryAsync(x => x.DoctorId, x => x.Name ?? "—");

                var list = new List<DoctorRevenueItemViewModel>();
                foreach (var g in grouped.OrderByDescending(x => x.Total).Take(15))
                {
                    list.Add(new DoctorRevenueItemViewModel
                    {
                        DoctorId = g.DoctorId,
                        DoctorName = doctorNames.ContainsKey(g.DoctorId) ? doctorNames[g.DoctorId] : "—",
                        Revenue = g.Total,
                        TransactionCount = g.Count,
                        PercentShare = totalSum > 0 ? Math.Round((decimal)(g.Total / totalSum * 100), 1) : 0
                    });
                }
                return list;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "خطا در محاسبه درآمد به تفکیک پزشک");
                return new List<DoctorRevenueItemViewModel>();
            }
        }

        private async Task<List<DailyRevenueItemViewModel>> GetDailyTrendAsync(DateTime start, DateTime end)
        {
            var days = (end - start).Days + 1;
            if (days > MaxChartDays) end = start.AddDays(MaxChartDays - 1);

            var result = new List<DailyRevenueItemViewModel>();
            for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
            {
                var dayStats = await _paymentTransactionRepository.GetDailyStatisticsAsync(d);
                result.Add(new DailyRevenueItemViewModel
                {
                    Date = d,
                    DatePersian = PersianDateHelper.ToPersianDate(d),
                    Revenue = dayStats.SuccessfulAmount,
                    TransactionCount = dayStats.SuccessfulTransactions
                });
            }
            return result;
        }
    }
}
