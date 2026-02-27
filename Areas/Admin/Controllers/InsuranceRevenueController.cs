using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Core;
using ClinicApp.ViewModels.Admin;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// داشبورد تحلیلی درآمد بیمه‌ها و مدیریت مطالبات — فقط اکشن‌های مرتبط (SRP)
    /// </summary>
    [Authorize(Roles = AppRoles.Admin + ",Finance")]
    public class InsuranceRevenueController : Controller
    {
        private readonly IInsuranceRevenueService _insuranceRevenueService;
        private readonly IInsuranceProviderRepository _insuranceProviderRepository;
        private readonly ILogger _logger;

        public InsuranceRevenueController(
            IInsuranceRevenueService insuranceRevenueService,
            IInsuranceProviderRepository insuranceProviderRepository)
        {
            _insuranceRevenueService = insuranceRevenueService ?? throw new ArgumentNullException(nameof(insuranceRevenueService));
            _insuranceProviderRepository = insuranceProviderRepository ?? throw new ArgumentNullException(nameof(insuranceProviderRepository));
            _logger = Log.ForContext<InsuranceRevenueController>();
        }

        [HttpGet]
        public async Task<ActionResult> Index(string startDatePersian, string endDatePersian, int? insuranceProviderId, string claimStatus)
        {
            try
            {
                _logger.Information("درخواست نمایش داشبورد درآمد بیمه توسط کاربر {User}", User?.Identity?.Name);

                var now = DateTime.Now.Date;
                var firstOfMonth = new DateTime(now.Year, now.Month, 1);
                var filter = new InsuranceRevenueFilterViewModel
                {
                    StartDatePersian = !string.IsNullOrWhiteSpace(startDatePersian) ? startDatePersian.Trim() : PersianDateHelper.ToPersianDate(firstOfMonth),
                    EndDatePersian = !string.IsNullOrWhiteSpace(endDatePersian) ? endDatePersian.Trim() : PersianDateHelper.ToPersianDate(now),
                    InsuranceProviderId = insuranceProviderId,
                    ClaimStatus = claimStatus
                };
                filter.StartDate = PersianDateHelper.ParsePersianDate(filter.StartDatePersian) ?? firstOfMonth;
                filter.EndDate = PersianDateHelper.ParsePersianDate(filter.EndDatePersian) ?? now;
                if (filter.EndDate < filter.StartDate) filter.EndDate = filter.StartDate;

                var providers = await _insuranceProviderRepository.GetActiveAsync();
                ViewBag.InsuranceProviders = providers
                    .Select(p => new SelectListItem
                    {
                        Value = p.InsuranceProviderId.ToString(),
                        Text = p.Name ?? p.Code ?? p.InsuranceProviderId.ToString(),
                        Selected = filter.InsuranceProviderId == p.InsuranceProviderId
                    })
                    .ToList();

                var result = await _insuranceRevenueService.GetDashboardDataAsync(filter);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message ?? "خطا در بارگذاری داشبورد درآمد بیمه.";
                    return View(new InsuranceRevenueDashboardViewModel { Filter = filter });
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش داشبورد درآمد بیمه");
                TempData["ErrorMessage"] = "خطا در بارگذاری داشبورد.";
                return View(new InsuranceRevenueDashboardViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetKPIs(InsuranceRevenueFilterViewModel filter)
        {
            try
            {
                var result = await _insuranceRevenueService.GetKPIsAsync(filter);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در GetKPIs درآمد بیمه");
                return Json(new { success = false, message = "خطا در محاسبه KPI." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetAgingData(DateTime? asOfDate)
        {
            try
            {
                var result = await _insuranceRevenueService.GetAgingReportAsync(asOfDate);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در GetAgingData");
                return Json(new { success = false, message = "خطا در گزارش Aging." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetChartData(InsuranceRevenueFilterViewModel filter)
        {
            try
            {
                var result = await _insuranceRevenueService.GetChartDataAsync(filter);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در GetChartData درآمد بیمه");
                return Json(new { success = false, message = "خطا در داده نمودار." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExportToExcel(InsuranceRevenueFilterViewModel filter)
        {
            try
            {
                var result = await _insuranceRevenueService.ExportToExcelAsync(filter);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message ?? "خطا در خروجی Excel.";
                    return RedirectToAction("Index");
                }

                var fileName = $"InsuranceRevenue_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ExportToExcel درآمد بیمه");
                TempData["ErrorMessage"] = "خطا در خروجی Excel.";
                return RedirectToAction("Index");
            }
        }
    }
}
