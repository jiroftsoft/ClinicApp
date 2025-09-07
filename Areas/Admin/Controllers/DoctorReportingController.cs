using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;
using System.Collections.Generic;
using ClinicApp.Models;
using ClinicApp.Repositories.ClinicAdmin;
using DoctorDependencyInfo = ClinicApp.Models.DoctorDependencyInfo; // Added for List

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر گزارش‌گیری و آمار پزشکان در سیستم کلینیک شفا
    /// مسئولیت: گزارش‌گیری، آمار و تحلیل عملکرد پزشکان
    /// </summary>
    [Authorize(Roles = "Admin,ClinicManager")]
    public class DoctorReportingController : Controller
    {
        private readonly IDoctorReportingService _doctorReportingService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public DoctorReportingController(
            IDoctorReportingService doctorReportingService,
            IDoctorCrudService doctorCrudService,
            ICurrentUserService currentUserService)
        {
            _doctorReportingService = doctorReportingService ?? throw new ArgumentNullException(nameof(doctorReportingService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<DoctorReportingController>();
        }

        #region Reports

        /// <summary>
        /// نمایش صفحه گزارش‌های پزشکان
        /// </summary>
        [HttpGet]
        public ActionResult Reports()
        {
            try
            {
                _logger.Information("درخواست نمایش صفحه گزارش‌های پزشکان توسط کاربر {UserId}", _currentUserService.UserId);

                return View();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه گزارش‌های پزشکان");
                TempData["Error"] = "خطا در بارگذاری صفحه گزارش‌ها";
                return View();
            }
        }

        /// <summary>
        /// گزارش پزشکان فعال
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ActiveDoctorsReport(int? clinicId = null, int? departmentId = null, 
            DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("درخواست گزارش پزشکان فعال توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مقادیر پیش‌فرض
                if (!startDate.HasValue)
                    startDate = DateTime.Today.AddDays(-30);
                if (!endDate.HasValue)
                    endDate = DateTime.Today;

                // دریافت گزارش پزشکان فعال
                var result = await _doctorReportingService.GetActiveDoctorsReportAsync(
                    clinicId ?? 0, departmentId, startDate.Value, endDate.Value);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(new ActiveDoctorsReportViewModel());
                }

                ViewBag.ClinicId = clinicId;
                ViewBag.DepartmentId = departmentId;
                ViewBag.StartDate = startDate.Value.ToString("yyyy/MM/dd");
                ViewBag.EndDate = endDate.Value.ToString("yyyy/MM/dd");

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت گزارش پزشکان فعال");
                TempData["Error"] = "خطا در دریافت گزارش پزشکان فعال";
                return View(new ActiveDoctorsReportViewModel());
            }
        }

        #endregion

        #region Dashboard

        /// <summary>
        /// داشبورد پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Dashboard(int doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش داشبورد پزشک {DoctorId} توسط کاربر {UserId}", doctorId, _currentUserService.UserId);

                if (doctorId <= 0)
                {
                    TempData["Error"] = "شناسه پزشک نامعتبر است.";
                    return RedirectToAction("Index", "Doctor");
                }

                // بررسی وجود پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    TempData["Error"] = "پزشک مورد نظر یافت نشد.";
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت داده‌های داشبورد
                var dashboardResult = await _doctorReportingService.GetDoctorDashboardDataAsync(doctorId);

                if (!dashboardResult.Success)
                {
                    TempData["Error"] = dashboardResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                ViewBag.Doctor = doctorResult.Data;
                return View(dashboardResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش داشبورد پزشک {DoctorId}", doctorId);
                TempData["Error"] = "خطا در بارگذاری داشبورد پزشک";
                return RedirectToAction("Index", "Doctor");
            }
        }

        #endregion

        #region Analytics

        /// <summary>
        /// تحلیل و آمار پزشکان
        /// </summary>
        [HttpGet]
        public ActionResult Analytics(int? clinicId = null, int? departmentId = null)
        {
            try
            {
                _logger.Information("درخواست نمایش تحلیل و آمار پزشکان توسط کاربر {UserId}", _currentUserService.UserId);

                ViewBag.ClinicId = clinicId;
                ViewBag.DepartmentId = departmentId;

                return View();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش تحلیل و آمار پزشکان");
                TempData["Error"] = "خطا در بارگذاری تحلیل و آمار";
                return View();
            }
        }

        #endregion

        #region Export

        /// <summary>
        /// خروجی گزارش پزشکان فعال (Excel)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ExportActiveDoctorsReport(int? clinicId = null, int? departmentId = null,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("درخواست خروجی گزارش پزشکان فعال توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مقادیر پیش‌فرض
                if (!startDate.HasValue)
                    startDate = DateTime.Today.AddDays(-30);
                if (!endDate.HasValue)
                    endDate = DateTime.Today;

                // دریافت گزارش پزشکان فعال
                var result = await _doctorReportingService.GetActiveDoctorsReportAsync(
                    clinicId ?? 0, departmentId, startDate.Value, endDate.Value);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("ActiveDoctorsReport");
                }

                // ایجاد فایل Excel
                var fileName = $"گزارش_پزشکان_فعال_{startDate.Value:yyyyMMdd}_{endDate.Value:yyyyMMdd}.xlsx";
                
                // اینجا می‌توانید از یک سرویس Excel Generator استفاده کنید
                // فعلاً یک نمونه ساده
                return File(new byte[0], "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در خروجی گزارش پزشکان فعال");
                TempData["Error"] = "خطا در ایجاد فایل خروجی";
                return RedirectToAction("ActiveDoctorsReport");
            }
        }

        #endregion

        #region AJAX Operations

        /// <summary>
        /// بررسی دسترسی پزشک به دسته‌بندی خدمات (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> CheckDoctorServiceCategoryAccess(int doctorId, int serviceCategoryId)
        {
            try
            {
                if (doctorId <= 0 || serviceCategoryId <= 0)
                {
                    return Json(new { success = false, message = "پارامترهای ورودی نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _doctorReportingService.HasAccessToServiceCategoryAsync(doctorId, serviceCategoryId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, hasAccess = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی پزشک {DoctorId} به دسته‌بندی خدمات {ServiceCategoryId}", doctorId, serviceCategoryId);
                return Json(new { success = false, message = "خطا در بررسی دسترسی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی دسترسی پزشک به خدمت (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> CheckDoctorServiceAccess(int doctorId, int serviceId)
        {
            try
            {
                if (doctorId <= 0 || serviceId <= 0)
                {
                    return Json(new { success = false, message = "پارامترهای ورودی نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _doctorReportingService.HasAccessToServiceAsync(doctorId, serviceId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, hasAccess = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی پزشک {DoctorId} به خدمت {ServiceId}", doctorId, serviceId);
                return Json(new { success = false, message = "خطا در بررسی دسترسی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت آمار داشبورد پزشک (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDoctorDashboardStats(int doctorId)
        {
            try
            {
                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _doctorReportingService.GetDoctorDashboardDataAsync(doctorId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var stats = new
                {
                    todayAppointments = result.Data?.TodayAppointments?.Count ?? 0,
                    tomorrowAppointments = result.Data?.TomorrowAppointments?.Count ?? 0,
                    totalServiceCategories = result.Data?.ServiceCategoryStats?.Count ?? 0,
                    dailyStats = result.Data?.DailyAppointmentStats
                };

                return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار داشبورد پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در دریافت آمار داشبورد" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Advanced Reporting

        /// <summary>
        /// نمایش گزارش عملکرد پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> PerformanceReport(int? doctorId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("درخواست گزارش عملکرد پزشک {DoctorId} توسط کاربر {UserId}", doctorId, _currentUserService.UserId);

                if (!doctorId.HasValue || doctorId.Value <= 0)
                {
                    TempData["Error"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Reports");
                }

                // تنظیم مقادیر پیش‌فرض
                if (!startDate.HasValue)
                    startDate = DateTime.Today.AddDays(-30);
                if (!endDate.HasValue)
                    endDate = DateTime.Today;

                // بررسی وجود پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    TempData["Error"] = "پزشک مورد نظر یافت نشد";
                    return RedirectToAction("Reports");
                }

                ViewBag.Doctor = doctorResult.Data;
                ViewBag.StartDate = startDate.Value.ToString("yyyy/MM/dd");
                ViewBag.EndDate = endDate.Value.ToString("yyyy/MM/dd");

                // دریافت آمار عملکرد
                var performanceStats = await GetDoctorPerformanceStatsAsync(doctorId.Value, startDate.Value, endDate.Value);

                _logger.Information("گزارش عملکرد پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(performanceStats);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش گزارش عملکرد پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                TempData["Error"] = "خطا در بارگذاری گزارش عملکرد";
                return RedirectToAction("Reports");
            }
        }

        /// <summary>
        /// نمایش گزارش مقایسه‌ای پزشکان
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ComparisonReport(int? clinicId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("درخواست گزارش مقایسه‌ای پزشکان توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مقادیر پیش‌فرض
                if (!startDate.HasValue)
                    startDate = DateTime.Today.AddDays(-30);
                if (!endDate.HasValue)
                    endDate = DateTime.Today;

                ViewBag.ClinicId = clinicId;
                ViewBag.StartDate = startDate.Value.ToString("yyyy/MM/dd");
                ViewBag.EndDate = endDate.Value.ToString("yyyy/MM/dd");

                // دریافت گزارش مقایسه‌ای
                var comparisonReport = await GetDoctorComparisonReportAsync(clinicId ?? 0, startDate.Value, endDate.Value);

                _logger.Information("گزارش مقایسه‌ای پزشکان با موفقیت نمایش داده شد");
                return View(comparisonReport);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش گزارش مقایسه‌ای پزشکان");
                TempData["Error"] = "خطا در بارگذاری گزارش مقایسه‌ای";
                return RedirectToAction("Reports");
            }
        }

        /// <summary>
        /// نمایش گزارش وابستگی‌های پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> DependencyReport(int? doctorId)
        {
            try
            {
                _logger.Information("درخواست گزارش وابستگی‌های پزشک {DoctorId} توسط کاربر {UserId}", doctorId, _currentUserService.UserId);

                if (!doctorId.HasValue || doctorId.Value <= 0)
                {
                    TempData["Error"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Reports");
                }

                // بررسی وجود پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    TempData["Error"] = "پزشک مورد نظر یافت نشد";
                    return RedirectToAction("Reports");
                }

                ViewBag.Doctor = doctorResult.Data;

                // دریافت اطلاعات وابستگی‌ها
                var dependencyInfo = await GetDoctorDependencyInfoAsync(doctorId.Value);

                _logger.Information("گزارش وابستگی‌های پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(dependencyInfo);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش گزارش وابستگی‌های پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                TempData["Error"] = "خطا در بارگذاری گزارش وابستگی‌ها";
                return RedirectToAction("Reports");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// دریافت آمار عملکرد پزشک
        /// </summary>
        private async Task<DoctorPerformanceStats> GetDoctorPerformanceStatsAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                // این متد باید از Repository استفاده کند
                // فعلاً یک نمونه ساده برمی‌گردانیم
                return new DoctorPerformanceStats
                {
                    DoctorId = doctorId,
                    TotalAppointments = 0,
                    CompletedAppointments = 0,
                    CancelledAppointments = 0,
                    PendingAppointments = 0,
                    StartDate = startDate,
                    EndDate = endDate
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار عملکرد پزشک {DoctorId}", doctorId);
                throw;
            }
        }

        /// <summary>
        /// دریافت گزارش مقایسه‌ای پزشکان
        /// </summary>
        private async Task<List<DoctorComparisonReport>> GetDoctorComparisonReportAsync(int clinicId, DateTime startDate, DateTime endDate)
        {
            try
            {
                // این متد باید از Repository استفاده کند
                // فعلاً یک لیست خالی برمی‌گردانیم
                return new List<DoctorComparisonReport>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت گزارش مقایسه‌ای پزشکان");
                throw;
            }
        }

        /// <summary>
        /// دریافت اطلاعات وابستگی‌های پزشک
        /// </summary>
        private async Task<DoctorDependencyInfo> GetDoctorDependencyInfoAsync(int doctorId)
        {
            try
            {
                // این متد باید از Repository استفاده کند
                // فعلاً یک نمونه ساده برمی‌گردانیم
                return new DoctorDependencyInfo
                {
                    CanBeDeleted = false,
                    DeletionErrorMessage = "بررسی وابستگی‌ها در حال انجام است",
                    TotalActiveAppointments = 0,
                    TotalDepartmentAssignments = 0,
                    TotalServiceCategoryAssignments = 0,
                    AppointmentCount = 0,
                    DepartmentAssignmentCount = 0,
                    ServiceCategoryAssignmentCount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات وابستگی‌های پزشک {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion
    }
}
