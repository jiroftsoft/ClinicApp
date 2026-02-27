using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Models.Core;
using ClinicApp.ViewModels.Payment;
using Serilog;

namespace ClinicApp.Controllers.Payment
{
    /// <summary>
    /// کنترلر گزارشات صندوق - SRP محور
    /// مدیریت گزارش‌گیری از عملکرد منشی‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. گزارش روزانه عملکرد منشی
    /// 2. گزارش ماهانه عملکرد منشی
    /// 3. گزارش بازه زمانی
    /// 4. خلاصه تمام منشی‌ها
    /// 5. مقایسه عملکرد منشی‌ها
    /// 6. Export به Excel و PDF
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md, DEVELOPMENT_CONTRACT.md
    /// </summary>
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    public class CashierReportController : BaseController
    {
        #region Fields

        private readonly ICashierReportService _reportService;
        private readonly ICurrentUserService _currentUserService;

        #endregion

        #region Constructor

        public CashierReportController(
            ICashierReportService reportService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(currentUserService, logger)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #endregion

        #region Index

        /// <summary>
        /// صفحه اصلی گزارش‌ها
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.Information("📊 Loading Cashier Reports Index for User: {UserId}", _currentUserService.UserId);

                var filter = new CashierReportFilterViewModel
                {
                    StartDate = DateTime.Today.AddDays(-7),
                    EndDate = DateTime.Today,
                    ReportType = ReportType.Daily
                };
                filter.StartDateShamsi = PersianDateHelper.ToPersianDate(filter.StartDate.Value);
                filter.EndDateShamsi = PersianDateHelper.ToPersianDate(filter.EndDate.Value);

                var model = new CashierReportIndexViewModel
                {
                    Filter = filter,
                    Cashiers = await _reportService.GetCashiersListAsync(),
                    SelectedReportType = ReportType.Daily
                };

                _logger.Information("✅ Cashier Reports Index loaded successfully");

                return View("~/Views/Payment/CashierReport/Index.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error loading Cashier Reports Index for User: {UserId}", _currentUserService.UserId);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری صفحه گزارش‌ها");
                return View("~/Views/Payment/CashierReport/Index.cshtml", new CashierReportIndexViewModel());
            }
        }

        #endregion

        #region Daily Report

        /// <summary>
        /// گزارش روزانه (GET) — اعتبارسنجی QueryString، Audit Log، نقش‌مجاز از طریق [Authorize]
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> DailyReport(string cashierId, DateTime? date)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cashierId))
                {
                    NotificationHelper.SetWarning(TempData, "لطفاً منشی را انتخاب کنید");
                    return RedirectToAction("Index");
                }

                if (!IsValidCashierId(cashierId))
                {
                    _logger.Warning("⚠️ DailyReport invalid cashierId format. Length: {Length}", cashierId?.Length ?? 0);
                    NotificationHelper.SetError(TempData, "شناسه منشی نامعتبر است.");
                    return RedirectToAction("Index");
                }

                var reportDate = (date ?? DateTime.Today).Date;
                var today = DateTime.Today;
                if (reportDate > today)
                {
                    _logger.Warning("⚠️ DailyReport future date requested: {Date}", reportDate);
                    reportDate = today;
                }
                if (reportDate < today.AddYears(-5))
                {
                    NotificationHelper.SetWarning(TempData, "تاریخ خارج از بازه مجاز است.");
                    return RedirectToAction("Index");
                }

                _logger.Information("📊 Getting daily report for Cashier: {CashierId}, Date: {Date}", cashierId, reportDate);

                var result = await _reportService.GetDailyReportAsync(cashierId, reportDate);
                if (!result.Success)
                {
                    _logger.Warning("⚠️ Failed to get daily report: {Message}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                _logger.Information("AUDIT DailyReport UserId={UserId} CashierId={CashierId} Date={Date}",
                    _currentUserService?.UserId ?? "anonymous", cashierId, reportDate);

                var model = new CashierDailyReportViewModel
                {
                    Report = result.Data,
                    Filter = new CashierReportFilterViewModel
                    {
                        CashierId = cashierId,
                        StartDate = reportDate,
                        EndDate = reportDate,
                        ReportType = ReportType.Daily
                    },
                    GeneratedAtUtc = DateTime.UtcNow,
                    ReportDatePersian = PersianDateHelper.ToPersianDate(reportDate)
                };

                _logger.Information("✅ Daily report loaded successfully");

                return View("~/Views/Payment/CashierReport/DailyReport.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting daily report for Cashier: {CashierId}, Date: {Date}", cashierId, date);
                NotificationHelper.SetError(TempData, "خطا در دریافت گزارش روزانه");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// گزارش روزانه (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DailyReport(CashierReportFilterViewModel filter)
        {
            try
            {
                var date = ParseDateFromFilter(filter, useStart: true) ?? this.ParseDateFromHiddenInput("StartDate", _logger) ?? DateTime.Today;
                date = date.Date;

                if (string.IsNullOrWhiteSpace(filter?.CashierId) || !IsValidCashierId(filter.CashierId))
                {
                    NotificationHelper.SetWarning(TempData, "لطفاً منشی معتبر را انتخاب کنید");
                    return RedirectToAction("Index");
                }

                _logger.Information("📊 POST Daily Report - Cashier: {CashierId}, Date: {Date}", filter.CashierId, date);

                return RedirectToAction("DailyReport", new { cashierId = filter.CashierId, date = date });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error in POST DailyReport");
                NotificationHelper.SetError(TempData, "خطا در دریافت گزارش");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Monthly Report

        /// <summary>
        /// گزارش ماهانه (GET)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> MonthlyReport(string cashierId, int? year, int? month)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cashierId))
                {
                    NotificationHelper.SetWarning(TempData, "لطفاً منشی را انتخاب کنید");
                    return RedirectToAction("Index");
                }

                var reportYear = year ?? DateTime.Today.Year;
                var reportMonth = month ?? DateTime.Today.Month;

                _logger.Information("📊 Getting monthly report for Cashier: {CashierId}, Year: {Year}, Month: {Month}", 
                    cashierId, reportYear, reportMonth);

                var result = await _reportService.GetMonthlyReportAsync(cashierId, reportYear, reportMonth);
                if (!result.Success)
                {
                    _logger.Warning("⚠️ Failed to get monthly report: {Message}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                var model = new CashierMonthlyReportViewModel
                {
                    Report = result.Data,
                    Filter = new CashierReportFilterViewModel
                    {
                        CashierId = cashierId,
                        Year = reportYear,
                        Month = reportMonth,
                        ReportType = ReportType.Monthly
                    }
                };

                _logger.Information("✅ Monthly report loaded successfully");

                return View("~/Views/Payment/CashierReport/MonthlyReport.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting monthly report for Cashier: {CashierId}, Year: {Year}, Month: {Month}", 
                    cashierId, year, month);
                NotificationHelper.SetError(TempData, "خطا در دریافت گزارش ماهانه");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// گزارش ماهانه (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MonthlyReport(CashierReportFilterViewModel filter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filter.CashierId))
                {
                    NotificationHelper.SetWarning(TempData, "لطفاً منشی را انتخاب کنید");
                    return RedirectToAction("Index");
                }

                var reportYear = filter.Year ?? DateTime.Today.Year;
                var reportMonth = filter.Month ?? DateTime.Today.Month;

                _logger.Information("📊 POST Monthly Report - Cashier: {CashierId}, Year: {Year}, Month: {Month}", 
                    filter.CashierId, reportYear, reportMonth);

                return RedirectToAction("MonthlyReport", new { cashierId = filter.CashierId, year = reportYear, month = reportMonth });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error in POST MonthlyReport");
                NotificationHelper.SetError(TempData, "خطا در دریافت گزارش");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Range Report

        /// <summary>
        /// گزارش بازه زمانی (GET)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> RangeReport(string cashierId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cashierId))
                {
                    NotificationHelper.SetWarning(TempData, "لطفاً منشی را انتخاب کنید");
                    return RedirectToAction("Index");
                }

                var startDate = fromDate ?? DateTime.Today.AddDays(-7);
                var endDate = toDate ?? DateTime.Today;

                if (startDate > endDate)
                {
                    NotificationHelper.SetWarning(TempData, "تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد");
                    return RedirectToAction("Index");
                }

                _logger.Information("📊 Getting range report for Cashier: {CashierId}, From: {FromDate}, To: {ToDate}", 
                    cashierId, startDate, endDate);

                var result = await _reportService.GetRangeReportAsync(cashierId, startDate, endDate);
                if (!result.Success)
                {
                    _logger.Warning("⚠️ Failed to get range report: {Message}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                var model = new CashierRangeReportViewModel
                {
                    Report = result.Data,
                    Filter = new CashierReportFilterViewModel
                    {
                        CashierId = cashierId,
                        StartDate = startDate,
                        EndDate = endDate,
                        ReportType = ReportType.Range
                    }
                };

                _logger.Information("✅ Range report loaded successfully");

                return View("~/Views/Payment/CashierReport/RangeReport.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting range report for Cashier: {CashierId}", cashierId);
                NotificationHelper.SetError(TempData, "خطا در دریافت گزارش بازه زمانی");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// گزارش بازه زمانی (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RangeReport(CashierReportFilterViewModel filter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filter.CashierId))
                {
                    NotificationHelper.SetWarning(TempData, "لطفاً منشی را انتخاب کنید");
                    return RedirectToAction("Index");
                }

                var startDate = ParseDateFromFilter(filter, useStart: true) ?? this.ParseDateFromHiddenInput("StartDate", _logger) ?? DateTime.Today.AddDays(-7);
                var endDate = ParseDateFromFilter(filter, useStart: false) ?? this.ParseDateFromHiddenInput("EndDate", _logger) ?? DateTime.Today;

                if (startDate > endDate)
                {
                    NotificationHelper.SetWarning(TempData, "تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد");
                    return RedirectToAction("Index");
                }

                _logger.Information("📊 POST Range Report - Cashier: {CashierId}, From: {FromDate}, To: {ToDate}", 
                    filter.CashierId, startDate, endDate);

                return RedirectToAction("RangeReport", new { cashierId = filter.CashierId, fromDate = startDate, toDate = endDate });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error in POST RangeReport");
                NotificationHelper.SetError(TempData, "خطا در دریافت گزارش");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region All Cashiers Summary

        /// <summary>
        /// خلاصه تمام منشی‌ها (GET)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> AllCashiersSummary(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var startDate = (fromDate ?? DateTime.Today.AddDays(-30)).Date;
                var endDate = (toDate ?? DateTime.Today).Date;
                if (startDate > endDate)
                    endDate = startDate;

                _logger.Information("📊 Getting all cashiers summary from {FromDate} to {ToDate}", startDate, endDate);

                var result = await _reportService.GetAllCashiersSummaryAsync(startDate, endDate);
                if (!result.Success)
                {
                    _logger.Warning("⚠️ Failed to get all cashiers summary: {Message}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                var model = new CashierAllCashiersSummaryViewModel
                {
                    Summaries = result.Data,
                    Filter = new CashierReportFilterViewModel
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        ReportType = ReportType.AllCashiers
                    }
                };

                _logger.Information("✅ All cashiers summary loaded successfully. Count: {Count}", result.Data.Count);

                return View("~/Views/Payment/CashierReport/AllCashiersSummary.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting all cashiers summary");
                NotificationHelper.SetError(TempData, "خطا در دریافت خلاصه منشی‌ها");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// خلاصه تمام منشی‌ها (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AllCashiersSummary(CashierReportFilterViewModel filter)
        {
            try
            {
                var startDate = ParseDateFromFilter(filter, useStart: true) ?? this.ParseDateFromHiddenInput("StartDate", _logger) ?? DateTime.Today.AddDays(-30);
                var endDate = ParseDateFromFilter(filter, useStart: false) ?? this.ParseDateFromHiddenInput("EndDate", _logger) ?? DateTime.Today;
                startDate = startDate.Date;
                endDate = endDate.Date;
                if (startDate > endDate)
                {
                    NotificationHelper.SetWarning(TempData, "تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد");
                    return RedirectToAction("Index");
                }

                _logger.Information("📊 POST All Cashiers Summary - From: {FromDate}, To: {ToDate}", startDate, endDate);

                return RedirectToAction("AllCashiersSummary", new { fromDate = startDate, toDate = endDate });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error in POST AllCashiersSummary");
                NotificationHelper.SetError(TempData, "خطا در دریافت گزارش");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Compare Cashiers

        /// <summary>
        /// مقایسه منشی‌ها (GET)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> CompareCashiers(List<string> cashierIds, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var startDate = (fromDate ?? DateTime.Today.AddDays(-30)).Date;
                var endDate = (toDate ?? DateTime.Today).Date;
                if (startDate > endDate)
                    endDate = startDate;

                _logger.Information("📊 Getting compare cashiers from {FromDate} to {ToDate}", startDate, endDate);

                var filter = new CashierReportFilterViewModel
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    ReportType = ReportType.Compare
                };
                filter.StartDateShamsi = PersianDateHelper.ToPersianDate(filter.StartDate.Value);
                filter.EndDateShamsi = PersianDateHelper.ToPersianDate(filter.EndDate.Value);

                var model = new CashierCompareCashiersViewModel
                {
                    Filter = filter,
                    AvailableCashiers = await _reportService.GetCashiersListAsync(),
                    SelectedCashierIds = cashierIds ?? new List<string>()
                };

                // اگر منشی‌هایی انتخاب شده‌اند، گزارش را دریافت کن (همیشه منشی‌های انتخاب‌شده با داده یا صفر نمایش داده می‌شوند)
                if (cashierIds != null && cashierIds.Count > 0)
                {
                    var result = await _reportService.CompareCashiersAsync(cashierIds, startDate, endDate);
                    if (result.Success)
                    {
                        model.Comparison = result.Data;
                    }
                    else
                    {
                        _logger.Warning("⚠️ Failed to compare cashiers: {Message}", result.Message);
                        NotificationHelper.SetWarning(TempData, result.Message);
                    }
                }

                _logger.Information("✅ Compare cashiers loaded successfully");

                return View("~/Views/Payment/CashierReport/CompareCashiers.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting compare cashiers");
                NotificationHelper.SetError(TempData, "خطا در مقایسه منشی‌ها");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// مقایسه منشی‌ها (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompareCashiers(CashierCompareCashiersViewModel model)
        {
            try
            {
                var filter = model?.Filter;
                var startDate = ParseDateFromFilter(filter, useStart: true) ?? this.ParseDateFromHiddenInput("StartDate", _logger) ?? DateTime.Today.AddDays(-30);
                var endDate = ParseDateFromFilter(filter, useStart: false) ?? this.ParseDateFromHiddenInput("EndDate", _logger) ?? DateTime.Today;

                if (startDate > endDate)
                {
                    NotificationHelper.SetWarning(TempData, "تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد");
                    return RedirectToAction("Index");
                }

                if (model.SelectedCashierIds == null || model.SelectedCashierIds.Count == 0)
                {
                    NotificationHelper.SetWarning(TempData, "لطفاً حداقل یک منشی را انتخاب کنید");
                    return RedirectToAction("CompareCashiers", new { fromDate = startDate, toDate = endDate });
                }

                _logger.Information("📊 POST Compare Cashiers - CashierIds: {CashierIds}, From: {FromDate}, To: {ToDate}", 
                    string.Join(", ", model.SelectedCashierIds), startDate, endDate);

                return RedirectToAction("CompareCashiers", new { cashierIds = model.SelectedCashierIds, fromDate = startDate, toDate = endDate });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error in POST CompareCashiers");
                NotificationHelper.SetError(TempData, "خطا در مقایسه منشی‌ها");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Export

        /// <summary>
        /// Export به Excel (GET برای لینک مستقیم از صفحه گزارش؛ POST با توکن برای فرم)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ExportToExcel(string cashierId, DateTime? fromDate, DateTime? toDate, string reportType)
        {
            var from = (fromDate ?? DateTime.Today.AddDays(-7)).Date;
            var to = (toDate ?? DateTime.Today).Date;
            if (from > to) to = from;
            return await ExportToExcelCore(cashierId ?? "", from, to);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExportToExcelPost(string cashierId, DateTime? fromDate, DateTime? toDate)
        {
            var from = (fromDate ?? DateTime.Today.AddDays(-7)).Date;
            var to = (toDate ?? DateTime.Today).Date;
            if (from > to) to = from;
            return await ExportToExcelCore(cashierId ?? "", from, to);
        }

        private async Task<ActionResult> ExportToExcelCore(string cashierId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (!string.IsNullOrEmpty(cashierId) && cashierId != "all" && !IsValidCashierId(cashierId))
                {
                    _logger.Warning("⚠️ ExportToExcel invalid cashierId");
                    NotificationHelper.SetError(TempData, "شناسه منشی نامعتبر است.");
                    return RedirectToAction("Index");
                }
                _logger.Information("📊 Exporting to Excel - Cashier: {CashierId}, From: {FromDate}, To: {ToDate}", 
                    cashierId, fromDate, toDate);

                var result = await _reportService.ExportToExcelAsync(cashierId, fromDate, toDate);
                if (!result.Success)
                {
                    _logger.Warning("⚠️ Failed to export to Excel: {Message}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                var fileName = $"CashierReport_{cashierId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";
                _logger.Information("✅ Excel export completed successfully");
                return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error exporting to Excel");
                NotificationHelper.SetError(TempData, "خطا در Export به Excel");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Export به PDF (GET برای لینک مستقیم از صفحه گزارش)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ExportToPdf(string cashierId, DateTime? fromDate, DateTime? toDate, string reportType)
        {
            var from = (fromDate ?? DateTime.Today.AddDays(-7)).Date;
            var to = (toDate ?? DateTime.Today).Date;
            if (from > to) to = from;
            return await ExportToPdfCore(cashierId ?? "", from, to);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExportToPdfPost(string cashierId, DateTime? fromDate, DateTime? toDate)
        {
            var from = (fromDate ?? DateTime.Today.AddDays(-7)).Date;
            var to = (toDate ?? DateTime.Today).Date;
            if (from > to) to = from;
            return await ExportToPdfCore(cashierId ?? "", from, to);
        }

        private async Task<ActionResult> ExportToPdfCore(string cashierId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (!string.IsNullOrEmpty(cashierId) && cashierId != "all" && !IsValidCashierId(cashierId))
                {
                    _logger.Warning("⚠️ ExportToPdf invalid cashierId");
                    NotificationHelper.SetError(TempData, "شناسه منشی نامعتبر است.");
                    return RedirectToAction("Index");
                }
                _logger.Information("📊 Exporting to PDF - Cashier: {CashierId}, From: {FromDate}, To: {ToDate}", 
                    cashierId, fromDate, toDate);

                var result = await _reportService.ExportToPdfAsync(cashierId, fromDate, toDate);
                if (!result.Success)
                {
                    _logger.Warning("⚠️ Failed to export to PDF: {Message}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                var fileName = $"CashierReport_{cashierId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
                _logger.Information("✅ PDF export completed successfully");
                return File(result.Data, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error exporting to PDF");
                NotificationHelper.SetError(TempData, "خطا در Export به PDF");
                return RedirectToAction("Index");
            }
        }

        #endregion

        /// <summary>
        /// اعتبارسنجی شناسه منشی برای جلوگیری از QueryString نامعتبر (GUID یا شناسه معتبر، حداکثر طول).
        /// </summary>
        private static bool IsValidCashierId(string cashierId)
        {
            if (string.IsNullOrWhiteSpace(cashierId)) return false;
            if (cashierId.Length > 128) return false;
            return Guid.TryParse(cashierId, out _);
        }

        /// <summary>
        /// پارس تاریخ شمسی از فیلتر (StartDateShamsi یا EndDateShamsi) برای استفاده در POST.
        /// </summary>
        private static DateTime? ParseDateFromFilter(CashierReportFilterViewModel filter, bool useStart)
        {
            if (filter == null) return null;
            var shamsi = useStart ? filter.StartDateShamsi : filter.EndDateShamsi;
            if (string.IsNullOrWhiteSpace(shamsi)) return null;
            return PersianDateHelper.ParsePersianDate(shamsi);
        }

        #region AJAX Actions

        /// <summary>
        /// دریافت لیست منشی‌ها (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetCashiersList()
        {
            try
            {
                var cashiers = await _reportService.GetCashiersListAsync();
                return Json(new { success = true, data = cashiers }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting cashiers list");
                return Json(new { success = false, message = "خطا در دریافت لیست منشی‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}

