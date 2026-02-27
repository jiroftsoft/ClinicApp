using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Admin;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// داشبورد درآمد — خلاصه مالی، نمودار و خروجی Excel برای تصمیم‌گیری مدیریتی
    /// داده‌های مهم (فیلتر، دراپ‌داون‌ها) به‌صورت strongly-typed از طریق مدل ارسال می‌شوند.
    /// </summary>
    [Authorize(Roles = AppRoles.Admin)]
    public class RevenueDashboardController : Controller
    {
        private readonly IRevenueDashboardService _revenueDashboardService;
        private readonly IDoctorDepartmentService _doctorDepartmentService;
        private readonly ILogger _logger;

        public RevenueDashboardController(
            IRevenueDashboardService revenueDashboardService,
            IDoctorDepartmentService doctorDepartmentService)
        {
            _revenueDashboardService = revenueDashboardService ?? throw new ArgumentNullException(nameof(revenueDashboardService));
            _doctorDepartmentService = doctorDepartmentService ?? throw new ArgumentNullException(nameof(doctorDepartmentService));
            _logger = Log.ForContext<RevenueDashboardController>();
        }

        /// <summary>
        /// صفحه اصلی داشبورد درآمد — فیلتر از query string و لیست‌های دراپ‌داون از مدل (strongly-typed)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(
            string startDatePersian,
            string endDatePersian,
            int? doctorId,
            int? departmentId,
            string paymentMethod)
        {
            try
            {
                _logger.Information("درخواست نمایش داشبورد درآمد توسط کاربر {User}", User?.Identity?.Name);

                var now = DateTime.Now.Date;
                var firstOfMonth = new DateTime(now.Year, now.Month, 1);
                var filter = new RevenueDashboardFilterViewModel
                {
                    StartDatePersian = !string.IsNullOrWhiteSpace(startDatePersian) ? startDatePersian.Trim() : PersianDateHelper.ToPersianDate(firstOfMonth),
                    EndDatePersian = !string.IsNullOrWhiteSpace(endDatePersian) ? endDatePersian.Trim() : PersianDateHelper.ToPersianDate(now),
                    DoctorId = doctorId,
                    DepartmentId = departmentId,
                    PaymentMethod = !string.IsNullOrWhiteSpace(paymentMethod) ? paymentMethod.Trim() : null
                };
                filter.StartDate = PersianDateHelper.ParsePersianDate(filter.StartDatePersian) ?? firstOfMonth;
                filter.EndDate = PersianDateHelper.ParsePersianDate(filter.EndDatePersian) ?? now;
                if (filter.EndDate < filter.StartDate) filter.EndDate = filter.StartDate;

                var result = await _revenueDashboardService.GetDashboardAsync(filter);
                var model = result.Success ? result.Data : new RevenueDashboardViewModel { Filter = filter };

                model.Doctors = await BuildDoctorsSelectListAsync(filter.DoctorId);
                model.Departments = await BuildDepartmentsSelectListAsync(filter.DepartmentId);
                model.PaymentMethods = BuildPaymentMethodsSelectList(filter.PaymentMethod);

                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message ?? "خطا در بارگذاری داشبورد درآمد.";
                    return View(model);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش داشبورد درآمد");
                TempData["ErrorMessage"] = "خطا در بارگذاری داشبورد درآمد.";
                var fallback = new RevenueDashboardViewModel
                {
                    Doctors = new List<RevenueDashboardSelectItem>(),
                    Departments = new List<RevenueDashboardSelectItem>(),
                    PaymentMethods = BuildPaymentMethodsSelectList(null)
                };
                return View(fallback);
            }
        }

        private async Task<List<RevenueDashboardSelectItem>> BuildDoctorsSelectListAsync(int? selectedId)
        {
            var result = await _doctorDepartmentService.GetActiveDoctorsForLookupAsync(null, null);
            var list = new List<RevenueDashboardSelectItem>
            {
                new RevenueDashboardSelectItem { Value = "", Text = "همه پزشکان", Selected = !selectedId.HasValue }
            };
            if (result?.Success == true && result.Data != null)
                list.AddRange(result.Data.Select(x => new RevenueDashboardSelectItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name ?? x.Text ?? "",
                    Selected = selectedId.HasValue && selectedId.Value == x.Id
                }));
            return list;
        }

        private async Task<List<RevenueDashboardSelectItem>> BuildDepartmentsSelectListAsync(int? selectedId)
        {
            var result = await _doctorDepartmentService.GetAllDepartmentsAsync();
            var list = new List<RevenueDashboardSelectItem>
            {
                new RevenueDashboardSelectItem { Value = "", Text = "همه دپارتمان‌ها", Selected = !selectedId.HasValue }
            };
            if (result?.Success == true && result.Data != null)
                list.AddRange(result.Data.Select(x => new RevenueDashboardSelectItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name ?? x.Text ?? "",
                    Selected = selectedId.HasValue && selectedId.Value == x.Id
                }));
            return list;
        }

        private static List<RevenueDashboardSelectItem> BuildPaymentMethodsSelectList(string selectedValue)
        {
            var items = new List<RevenueDashboardSelectItem>
            {
                new RevenueDashboardSelectItem { Value = "", Text = "همه روش‌ها", Selected = string.IsNullOrWhiteSpace(selectedValue) },
                new RevenueDashboardSelectItem { Value = nameof(PaymentMethod.Cash), Text = "نقدی", Selected = selectedValue == nameof(PaymentMethod.Cash) },
                new RevenueDashboardSelectItem { Value = nameof(PaymentMethod.POS), Text = "پوز", Selected = selectedValue == nameof(PaymentMethod.POS) },
                new RevenueDashboardSelectItem { Value = nameof(PaymentMethod.Online), Text = "آنلاین", Selected = selectedValue == nameof(PaymentMethod.Online) }
            };
            return items;
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
