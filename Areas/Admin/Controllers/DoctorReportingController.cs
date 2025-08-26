using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;

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
        private readonly IMapper _mapper;

        public DoctorReportingController(
            IDoctorReportingService doctorReportingService,
            IDoctorCrudService doctorCrudService,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _doctorReportingService = doctorReportingService ?? throw new ArgumentNullException(nameof(doctorReportingService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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
    }
}
