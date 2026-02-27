using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Core;
using ClinicApp.ViewModels.Admin;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// داشبورد درآمد — خلاصه مالی، نمودار و خروجی Excel برای تصمیم‌گیری مدیریتی
    /// </summary>
    [Authorize(Roles = AppRoles.Admin)]
    public class RevenueDashboardController : Controller
    {
        private readonly IRevenueDashboardService _revenueDashboardService;
        private readonly ILogger _logger;

        public RevenueDashboardController(IRevenueDashboardService revenueDashboardService)
        {
            _revenueDashboardService = revenueDashboardService ?? throw new ArgumentNullException(nameof(revenueDashboardService));
            _logger = Log.ForContext<RevenueDashboardController>();
        }

        /// <summary>
        /// صفحه اصلی داشبورد درآمد
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(string startDatePersian, string endDatePersian)
        {
            try
            {
                _logger.Information("درخواست نمایش داشبورد درآمد توسط کاربر {User}", User?.Identity?.Name);

                var now = DateTime.Now.Date;
                var firstOfMonth = new DateTime(now.Year, now.Month, 1);
                var filter = new RevenueDashboardFilterViewModel
                {
                    StartDatePersian = !string.IsNullOrWhiteSpace(startDatePersian) ? startDatePersian.Trim() : PersianDateHelper.ToPersianDate(firstOfMonth),
                    EndDatePersian = !string.IsNullOrWhiteSpace(endDatePersian) ? endDatePersian.Trim() : PersianDateHelper.ToPersianDate(now)
                };
                filter.StartDate = PersianDateHelper.ParsePersianDate(filter.StartDatePersian) ?? firstOfMonth;
                filter.EndDate = PersianDateHelper.ParsePersianDate(filter.EndDatePersian) ?? now;
                if (filter.EndDate < filter.StartDate) filter.EndDate = filter.StartDate;

                var result = await _revenueDashboardService.GetDashboardAsync(filter);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message ?? "خطا در بارگذاری داشبورد درآمد.";
                    return View(new RevenueDashboardViewModel { Filter = filter });
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش داشبورد درآمد");
                TempData["ErrorMessage"] = "خطا در بارگذاری داشبورد درآمد.";
                return View(new RevenueDashboardViewModel());
            }
        }

        /// <summary>
        /// دریافت خلاصه KPI (JSON) برای آپدیت بدون رفرش
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetSummary(RevenueDashboardFilterViewModel filter)
        {
            try
            {
                var result = await _revenueDashboardService.GetSummaryAsync(filter);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در GetSummary داشبورد درآمد");
                return Json(new { success = false, message = "خطا در محاسبه خلاصه." });
            }
        }

        /// <summary>
        /// دریافت داده نمودار (JSON)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetChartData(RevenueDashboardFilterViewModel filter)
        {
            try
            {
                var result = await _revenueDashboardService.GetChartDataAsync(filter);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در GetChartData داشبورد درآمد");
                return Json(new { success = false, message = "خطا در داده نمودار." });
            }
        }

        /// <summary>
        /// خروجی Excel از داده‌های فیلتر شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExportToExcel(RevenueDashboardFilterViewModel filter)
        {
            try
            {
                var result = await _revenueDashboardService.ExportToExcelAsync(filter);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message ?? "خطا در خروجی Excel.";
                    return RedirectToAction("Index");
                }

                var fileName = $"RevenueDashboard_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ExportToExcel داشبورد درآمد");
                TempData["ErrorMessage"] = "خطا در خروجی Excel.";
                return RedirectToAction("Index");
            }
        }
    }
}
