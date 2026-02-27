using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Admin;
using OfficeOpenXml;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    public class InsuranceRevenueService : IInsuranceRevenueService
    {
        private readonly IInsuranceClaimRepository _claimRepository;
        private readonly IInsuranceBatchRepository _batchRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public InsuranceRevenueService(
            IInsuranceClaimRepository claimRepository,
            IInsuranceBatchRepository batchRepository,
            ApplicationDbContext context,
            ILogger logger)
        {
            _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
            _batchRepository = batchRepository ?? throw new ArgumentNullException(nameof(batchRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<InsuranceRevenueDashboardViewModel>> GetDashboardDataAsync(InsuranceRevenueFilterViewModel filter)
        {
            try
            {
                var (start, end) = ResolveDateRange(filter);
                if (start == null || end == null)
                    return ServiceResult<InsuranceRevenueDashboardViewModel>.Failed("بازه تاریخ معتبر نیست.", "VALIDATION");

                var kpiResult = await GetKPIsAsync(filter);
                if (!kpiResult.Success)
                    return ServiceResult<InsuranceRevenueDashboardViewModel>.Failed(kpiResult.Message, kpiResult.Code);

                var agingResult = await GetAgingReportAsync(null);
                var breakdownResult = await GetProviderBreakdownAsync(start.Value, end.Value, filter.InsuranceProviderId);
                var chartResult = await GetChartDataAsync(filter);

                var model = new InsuranceRevenueDashboardViewModel
                {
                    Filter = filter,
                    KPIs = kpiResult.Data,
                    AgingItems = agingResult.Success ? agingResult.Data : new List<InsuranceAgingItemViewModel>(),
                    ProviderBreakdown = breakdownResult.Success ? breakdownResult.Data : new List<InsuranceProviderBreakdownViewModel>(),
                    ChartData = chartResult.Success ? chartResult.Data : new InsuranceChartDataViewModel()
                };

                return ServiceResult<InsuranceRevenueDashboardViewModel>.Successful(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در واکشی داشبورد درآمد بیمه");
                return ServiceResult<InsuranceRevenueDashboardViewModel>.Failed("خطا در بارگذاری داشبورد.", "EXCEPTION");
            }
        }

        public async Task<ServiceResult<InsuranceKPIViewModel>> GetKPIsAsync(InsuranceRevenueFilterViewModel filter)
        {
            try
            {
                var (start, end) = ResolveDateRange(filter);
                if (start == null || end == null)
                    return ServiceResult<InsuranceKPIViewModel>.Failed("بازه تاریخ معتبر نیست.", "VALIDATION");

                ClaimStatus? statusFilter = null;
                if (!string.IsNullOrWhiteSpace(filter.ClaimStatus) && Enum.TryParse<ClaimStatus>(filter.ClaimStatus, true, out var s))
                    statusFilter = s;

                var claims = await _claimRepository.GetByDateRangeAsync(start.Value, end.Value, filter.InsuranceProviderId, statusFilter);

                var nonRejected = claims.Where(c => c.Status != ClaimStatus.Rejected).ToList();
                var totalClaims = nonRejected.Sum(c => c.ClaimedAmount);
                var totalRealized = claims.Where(c => c.Status == ClaimStatus.Paid).Sum(c => c.FinalSettlement);
                var totalDeduction = claims.Sum(c => c.DeductionAmount);
                var outstanding = claims.Where(c => c.Status != ClaimStatus.Paid && c.Status != ClaimStatus.Rejected).Sum(c => c.ApprovedAmount - c.FinalSettlement);
                if (outstanding < 0) outstanding = 0;

                var paidWithDate = claims.Where(c => c.Status == ClaimStatus.Paid && c.PaymentDate.HasValue).ToList();
                var avgDays = paidWithDate.Count > 0
                    ? paidWithDate.Average(c => (c.PaymentDate.Value - c.SubmissionDate).TotalDays)
                    : 0;

                var kpi = new InsuranceKPIViewModel
                {
                    TotalClaims = totalClaims,
                    TotalRealized = totalRealized,
                    Outstanding = outstanding,
                    TotalDeduction = totalDeduction,
                    DeductionRatePercent = totalClaims > 0 ? Math.Round((decimal)((double)totalDeduction / (double)totalClaims * 100), 1) : 0,
                    AverageSettlementDays = Math.Round(avgDays, 1),
                    ClaimCount = claims.Count
                };

                return ServiceResult<InsuranceKPIViewModel>.Successful(kpi);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه KPI درآمد بیمه");
                return ServiceResult<InsuranceKPIViewModel>.Failed("خطا در محاسبه KPI.", "EXCEPTION");
            }
        }

        public async Task<ServiceResult<List<InsuranceAgingItemViewModel>>> GetAgingReportAsync(DateTime? asOfDate = null)
        {
            try
            {
                var rows = await _claimRepository.GetAgingReportAsync(asOfDate);
                var list = rows.Select(r => new InsuranceAgingItemViewModel
                {
                    AgeGroup = r.AgeGroup,
                    TotalClaimed = r.TotalClaimed,
                    TotalApproved = r.TotalApproved,
                    ClaimCount = r.ClaimCount
                }).ToList();
                return ServiceResult<List<InsuranceAgingItemViewModel>>.Successful(list);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در گزارش Aging مطالبات بیمه");
                return ServiceResult<List<InsuranceAgingItemViewModel>>.Failed("خطا در گزارش Aging.", "EXCEPTION");
            }
        }

        public async Task<ServiceResult<InsuranceChartDataViewModel>> GetChartDataAsync(InsuranceRevenueFilterViewModel filter)
        {
            try
            {
                var (start, end) = ResolveDateRange(filter);
                if (start == null || end == null)
                    return ServiceResult<InsuranceChartDataViewModel>.Failed("بازه تاریخ معتبر نیست.", "VALIDATION");

                var claims = await _claimRepository.GetByDateRangeAsync(start.Value, end.Value, filter.InsuranceProviderId, null);

                var byMonth = claims
                    .GroupBy(c => new { c.SubmissionDate.Year, c.SubmissionDate.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .ToList();

                var labels = byMonth.Select(g => g.Key.Year + "/" + g.Key.Month.ToString("00")).ToList();
                var claimedValues = byMonth.Select(g => g.Sum(c => c.ClaimedAmount)).ToList();
                var approvedValues = byMonth.Select(g => g.Sum(c => c.ApprovedAmount)).ToList();
                var deductionValues = byMonth.Select(g => g.Sum(c => c.DeductionAmount)).ToList();

                var breakdown = await _claimRepository.GetProviderBreakdownAsync(start.Value, end.Value);
                var pieLabels = breakdown.Select(b => b.ProviderName).ToList();
                var pieValues = breakdown.Select(b => b.TotalClaimed).ToList();

                var chart = new InsuranceChartDataViewModel
                {
                    Labels = labels,
                    ClaimedValues = claimedValues,
                    ApprovedValues = approvedValues,
                    DeductionValues = deductionValues,
                    PieLabels = pieLabels,
                    PieValues = pieValues
                };

                return ServiceResult<InsuranceChartDataViewModel>.Successful(chart);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در داده نمودار درآمد بیمه");
                return ServiceResult<InsuranceChartDataViewModel>.Failed("خطا در داده نمودار.", "EXCEPTION");
            }
        }

        public async Task<ServiceResult<List<InsuranceProviderBreakdownViewModel>>> GetProviderBreakdownAsync(DateTime start, DateTime end, int? insuranceProviderId = null)
        {
            try
            {
                var rows = await _claimRepository.GetProviderBreakdownAsync(start, end);
                if (insuranceProviderId.HasValue)
                    rows = rows.Where(r => r.InsuranceProviderId == insuranceProviderId.Value).ToList();

                var list = rows.Select(r => new InsuranceProviderBreakdownViewModel
                {
                    InsuranceProviderId = r.InsuranceProviderId,
                    ProviderName = r.ProviderName,
                    TotalClaimed = r.TotalClaimed,
                    TotalPaid = r.TotalPaid,
                    Outstanding = r.TotalPending,
                    TotalDeduction = r.TotalDeduction,
                    DeductionRatePercent = r.DeductionRatePercent,
                    AverageSettlementDays = r.AverageSettlementDays,
                    ClaimCount = r.ClaimCount
                }).ToList();

                return ServiceResult<List<InsuranceProviderBreakdownViewModel>>.Successful(list);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تحلیل تفکیک بیمه‌گذار");
                return ServiceResult<List<InsuranceProviderBreakdownViewModel>>.Failed("خطا در تحلیل.", "EXCEPTION");
            }
        }

        public async Task<ServiceResult<object>> CreateBatchAsync(int providerId, List<int> claimIds)
        {
            try
            {
                if (claimIds == null || claimIds.Count == 0)
                    return ServiceResult<object>.Failed("لیست مطالبات خالی است.", "VALIDATION");

                var batchNumber = "BATCH-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                var batch = new InsuranceBatch
                {
                    BatchNumber = batchNumber,
                    InsuranceProviderId = providerId,
                    SubmissionDate = DateTime.Now.Date,
                    Status = BatchStatus.Submitted,
                    TotalClaimed = 0,
                    TotalApproved = 0,
                    TotalDeduction = 0
                };

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        batch = await _batchRepository.AddAsync(batch);

                        decimal totalClaimed = 0, totalApproved = 0, totalDeduction = 0;
                        foreach (var claimId in claimIds)
                        {
                            var claim = await _claimRepository.GetByIdWithDetailsAsync(claimId);
                            if (claim == null || claim.InsurancePlan == null || claim.InsurancePlan.InsuranceProviderId != providerId) continue;
                            claim.BatchId = batch.Id;
                            await _claimRepository.UpdateAsync(claim);
                            totalClaimed += claim.ClaimedAmount;
                            totalApproved += claim.ApprovedAmount;
                            totalDeduction += claim.DeductionAmount;
                        }

                        batch.TotalClaimed = totalClaimed;
                        batch.TotalApproved = totalApproved;
                        batch.TotalDeduction = totalDeduction;
                        await _batchRepository.UpdateAsync(batch);

                        transaction.Commit();
                        _logger.Information("دسته مطالبه {BatchNumber} با {Count} مطالبه ایجاد شد", batchNumber, claimIds.Count);
                        return ServiceResult<object>.Successful(new { batch.Id, batch.BatchNumber });
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد دسته مطالبه");
                return ServiceResult<object>.Failed("خطا در ایجاد دسته.", "EXCEPTION");
            }
        }

        public async Task<ServiceResult<byte[]>> ExportToExcelAsync(InsuranceRevenueFilterViewModel filter)
        {
            try
            {
                var (start, end) = ResolveDateRange(filter);
                if (start == null || end == null)
                    return ServiceResult<byte[]>.Failed("بازه تاریخ معتبر نیست.", "VALIDATION");

                var claims = await _claimRepository.GetByDateRangeAsync(start.Value, end.Value, filter.InsuranceProviderId, null);
                var breakdown = await _claimRepository.GetProviderBreakdownAsync(start.Value, end.Value);
                var aging = await _claimRepository.GetAgingReportAsync(null);

                using (var package = new ExcelPackage())
                {
                    var wsSummary = package.Workbook.Worksheets.Add("خلاصه");
                    wsSummary.Cells[1, 1].Value = "کل مطالبات";
                    wsSummary.Cells[1, 2].Value = claims.Where(c => c.Status != ClaimStatus.Rejected).Sum(c => c.ClaimedAmount);
                    wsSummary.Cells[2, 1].Value = "دریافتی واقعی";
                    wsSummary.Cells[2, 2].Value = claims.Where(c => c.Status == ClaimStatus.Paid).Sum(c => c.FinalSettlement);
                    wsSummary.Cells[3, 1].Value = "کسورات";
                    wsSummary.Cells[3, 2].Value = claims.Sum(c => c.DeductionAmount);

                    var wsProvider = package.Workbook.Worksheets.Add("تفکیک بیمه");
                    wsProvider.Cells[1, 1].Value = "بیمه‌گذار";
                    wsProvider.Cells[1, 2].Value = "کل مطالبات";
                    wsProvider.Cells[1, 3].Value = "دریافتی";
                    wsProvider.Cells[1, 4].Value = "کسورات";
                    wsProvider.Cells[1, 5].Value = "درصد کسری";
                    int row = 2;
                    foreach (var b in breakdown)
                    {
                        wsProvider.Cells[row, 1].Value = SafeExcelCellValue(b.ProviderName);
                        wsProvider.Cells[row, 2].Value = b.TotalClaimed;
                        wsProvider.Cells[row, 3].Value = b.TotalPaid;
                        wsProvider.Cells[row, 4].Value = b.TotalDeduction;
                        wsProvider.Cells[row, 5].Value = b.DeductionRatePercent;
                        row++;
                    }

                    var wsAging = package.Workbook.Worksheets.Add("Aging");
                    wsAging.Cells[1, 1].Value = "بازه";
                    wsAging.Cells[1, 2].Value = "کل مطالبات";
                    wsAging.Cells[1, 3].Value = "تأیید شده";
                    wsAging.Cells[1, 4].Value = "تعداد";
                    row = 2;
                    foreach (var a in aging)
                    {
                        wsAging.Cells[row, 1].Value = SafeExcelCellValue(a.AgeGroup);
                        wsAging.Cells[row, 2].Value = a.TotalClaimed;
                        wsAging.Cells[row, 3].Value = a.TotalApproved;
                        wsAging.Cells[row, 4].Value = a.ClaimCount;
                        row++;
                    }

                    wsSummary.Cells.AutoFitColumns();
                    wsProvider.Cells.AutoFitColumns();
                    wsAging.Cells.AutoFitColumns();

                    return ServiceResult<byte[]>.Successful(package.GetAsByteArray());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در خروجی Excel درآمد بیمه");
                return ServiceResult<byte[]>.Failed("خطا در خروجی Excel.", "EXCEPTION");
            }
        }

        /// <summary>
        /// جلوگیری از Excel Formula Injection: مقادیر متنی که با کاراکترهای فرمول شروع می‌شوند به‌صورت متن ذخیره می‌شوند.
        /// </summary>
        private static string SafeExcelCellValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var trimmed = value.TrimStart();
            if (trimmed.Length > 0 && (trimmed[0] == '=' || trimmed[0] == '+' || trimmed[0] == '-' || trimmed[0] == '@' || trimmed[0] == '\t' || trimmed[0] == '\r'))
                return "'" + value;
            return value;
        }

        private (DateTime? start, DateTime? end) ResolveDateRange(InsuranceRevenueFilterViewModel filter)
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
            var defStart = new DateTime(defEnd.Year, defEnd.Month, 1).AddMonths(-5);
            return (defStart, defEnd.AddDays(1).AddSeconds(-1));
        }
    }
}
