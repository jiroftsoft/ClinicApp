using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Filters;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر تخصصی برای داشبورد پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. تمرکز صرف بر نمایش و مدیریت داده‌های داشبورد
    /// 2. رعایت استانداردهای پزشکی ایران در نمایش اطلاعات
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای نمایشی
    /// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 5. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این کنترلر بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
    /// </summary>
    [Authorize]
    [MedicalEnvironmentFilter]
    [CheckProfileCompletion]
    public class DoctorDashboardController : Controller
    {
        private readonly IDoctorDashboardService _dashboardService;
        private readonly ILogger _logger;

        public DoctorDashboardController(IDoctorDashboardService dashboardService)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
            _logger = Log.ForContext<DoctorDashboardController>();
        }

        #region Dashboard Actions (اکشن‌های داشبورد)

        /// <summary>
        /// نمایش داشبورد اصلی پزشکان
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(int? clinicId = null, int? departmentId = null)
        {
            try
            {
                _logger.Information("درخواست نمایش داشبورد اصلی پزشکان. کلینیک: {ClinicId}, دپارتمان: {DepartmentId}", 
                    clinicId?.ToString() ?? "همه", departmentId?.ToString() ?? "همه");

                var result = await _dashboardService.GetDashboardDataAsync(clinicId, departmentId);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت داده‌های داشبورد: {ErrorMessage}", result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return View(new DoctorDashboardIndexViewModel());
                }

                _logger.Information("داشبورد اصلی پزشکان با موفقیت نمایش داده شد");
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش داشبورد اصلی پزشکان");
                TempData["ErrorMessage"] = "خطا در بارگذاری داشبورد";
                return View(new DoctorDashboardIndexViewModel());
            }
        }

        /// <summary>
        /// نمایش جزئیات پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                _logger.Information("درخواست نمایش جزئیات پزشک {DoctorId}", id);

                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index");
                }

                var result = await _dashboardService.GetDoctorDetailsAsync(id);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت جزئیات پزشک {DoctorId}: {ErrorMessage}", id, result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _logger.Information("جزئیات پزشک {DoctorId} با موفقیت نمایش داده شد", id);
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات پزشک {DoctorId}", id);
                TempData["ErrorMessage"] = "خطا در بارگذاری جزئیات پزشک";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// نمایش انتسابات پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Assignments(int id)
        {
            try
            {
                _logger.Information("درخواست نمایش انتسابات پزشک {DoctorId}", id);

                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index");
                }

                var result = await _dashboardService.GetDoctorAssignmentsAsync(id);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت انتسابات پزشک {DoctorId}: {ErrorMessage}", id, result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _logger.Information("انتسابات پزشک {DoctorId} با موفقیت نمایش داده شد", id);
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش انتسابات پزشک {DoctorId}", id);
                TempData["ErrorMessage"] = "خطا در بارگذاری انتسابات پزشک";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Search Actions (اکشن‌های جستجو)

        /// <summary>
        /// جستجوی پزشکان
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Search(string searchTerm = null, int? clinicId = null, int? departmentId = null, int? specializationId = null, int page = 1)
        {
            try
            {
                _logger.Information("درخواست جستجوی پزشکان. جستجو: {SearchTerm}, صفحه: {Page}", searchTerm ?? "بدون فیلتر", page);

                var result = await _dashboardService.SearchDoctorsAsync(searchTerm, clinicId, departmentId, specializationId, page, 20);

                if (!result.Success)
                {
                    _logger.Warning("خطا در جستجوی پزشکان: {ErrorMessage}", result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return View(new DoctorSearchResultViewModel());
                }

                _logger.Information("جستجوی پزشکان با موفقیت انجام شد. تعداد نتایج: {TotalCount}", result.Data.TotalCount);
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پزشکان");
                TempData["ErrorMessage"] = "خطا در جستجوی پزشکان";
                return View(new DoctorSearchResultViewModel());
            }
        }

        #endregion

        #region Statistics Actions (اکشن‌های آمار)

        /// <summary>
        /// نمایش آمار کلی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Stats(int? clinicId = null)
        {
            try
            {
                _logger.Information("درخواست نمایش آمار کلی. کلینیک: {ClinicId}", clinicId?.ToString() ?? "همه");

                var result = await _dashboardService.GetDashboardStatsAsync(clinicId);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت آمار کلی: {ErrorMessage}", result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return View(new DashboardStatsViewModel());
                }

                _logger.Information("آمار کلی با موفقیت نمایش داده شد");
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش آمار کلی");
                TempData["ErrorMessage"] = "خطا در بارگذاری آمار";
                return View(new DashboardStatsViewModel());
            }
        }

        #endregion

        #region AJAX Actions (اکشن‌های AJAX)

        /// <summary>
        /// دریافت آمار پزشکان فعال (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetActiveDoctorsStats(int? clinicId = null, int? departmentId = null)
        {
            try
            {
                _logger.Information("درخواست AJAX آمار پزشکان فعال. کلینیک: {ClinicId}, دپارتمان: {DepartmentId}", 
                    clinicId?.ToString() ?? "همه", departmentId?.ToString() ?? "همه");

                var result = await _dashboardService.GetActiveDoctorsStatsAsync(clinicId, departmentId);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت آمار پزشکان فعال: {ErrorMessage}", result.Message);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                _logger.Information("آمار پزشکان فعال با موفقیت دریافت شد");
                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار پزشکان فعال");
                return Json(new { success = false, message = "خطا در دریافت آمار" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت عملیات سریع برای پزشک (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetQuickActions(int doctorId)
        {
            try
            {
                _logger.Information("درخواست AJAX عملیات سریع برای پزشک {DoctorId}", doctorId);

                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است" }, JsonRequestBehavior.AllowGet);
                }

                var result = await _dashboardService.GetQuickActionsAsync(doctorId);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت عملیات سریع پزشک {DoctorId}: {ErrorMessage}", doctorId, result.Message);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                _logger.Information("عملیات سریع پزشک {DoctorId} با موفقیت دریافت شد", doctorId);
                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت عملیات سریع پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در دریافت عملیات سریع" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی وضعیت پزشک (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDoctorStatus(int doctorId)
        {
            try
            {
                _logger.Information("درخواست AJAX وضعیت پزشک {DoctorId}", doctorId);

                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است" }, JsonRequestBehavior.AllowGet);
                }

                var result = await _dashboardService.GetDoctorStatusAsync(doctorId);

                if (!result.Success)
                {
                    _logger.Warning("خطا در بررسی وضعیت پزشک {DoctorId}: {ErrorMessage}", doctorId, result.Message);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                _logger.Information("وضعیت پزشک {DoctorId} با موفقیت بررسی شد", doctorId);
                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وضعیت پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در بررسی وضعیت پزشک" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
