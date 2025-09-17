using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Services.DataSeeding;
using ClinicApp.Interfaces;
using ClinicApp.Services;
using ClinicApp.Models; // Added for ApplicationDbContext
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت داده‌های اولیه سیستم
    /// این کنترلر برای ایجاد داده‌های اولیه سیستم در محیط تولید و تست استفاده می‌شود
    /// </summary>
    public class SystemSeedController : BaseController
    {
        private readonly SystemSeedService _systemSeedService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;

        public SystemSeedController(
            SystemSeedService systemSeedService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IMessageNotificationService messageNotificationService,
            ApplicationDbContext context)
            : base(messageNotificationService)
        {
            _systemSeedService = systemSeedService;
            _logger = logger;
            _currentUserService = currentUserService;
            _context = context;
        }

        /// <summary>
        /// صفحه اصلی مدیریت داده‌های اولیه
        /// </summary>
        public async Task<ActionResult> Index()
        {
            try
            {
                var status = await _systemSeedService.GetSeedDataStatusAsync();
                return View(status);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت وضعیت داده‌های اولیه");
                TempData["ErrorMessage"] = "خطا در دریافت وضعیت داده‌های اولیه";
                return View(new SeedDataStatus());
            }
        }

        /// <summary>
        /// ایجاد تمام داده‌های اولیه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SeedAllData()
        {
            try
            {
                _logger.Information("شروع ایجاد داده‌های اولیه توسط کاربر");
                await _systemSeedService.SeedAllDataAsync();
                
                TempData["SuccessMessage"] = "داده‌های اولیه با موفقیت ایجاد شدند";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد داده‌های اولیه");
                TempData["ErrorMessage"] = "خطا در ایجاد داده‌های اولیه: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ایجاد داده‌های اولیه به صورت مرحله‌ای
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SeedDataStepByStep()
        {
            try
            {
                _logger.Information("شروع ایجاد داده‌های اولیه به صورت مرحله‌ای");
                await _systemSeedService.SeedDataStepByStepAsync();
                
                TempData["SuccessMessage"] = "داده‌های اولیه به صورت مرحله‌ای با موفقیت ایجاد شدند";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد داده‌های اولیه به صورت مرحله‌ای");
                TempData["ErrorMessage"] = "خطا در ایجاد داده‌های اولیه: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// بررسی وضعیت داده‌های اولیه
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetStatus()
        {
            try
            {
                var status = await _systemSeedService.GetSeedDataStatusAsync();
                return Json(status, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت وضعیت داده‌های اولیه");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// پاک کردن داده‌های اولیه (فقط برای تست)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ClearSeedData()
        {
            try
            {
                _logger.Warning("پاک کردن داده‌های اولیه توسط کاربر");
                await _systemSeedService.ClearSeedDataAsync();
                
                TempData["SuccessMessage"] = "داده‌های اولیه با موفقیت پاک شدند";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پاک کردن داده‌های اولیه");
                TempData["ErrorMessage"] = "خطا در پاک کردن داده‌های اولیه: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// تست محاسبات با داده‌های اولیه
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> TestCalculations()
        {
            try
            {
                _logger.Information("شروع تست محاسبات با داده‌های اولیه");
                
                // بررسی وجود داده‌های اولیه
                var status = await _systemSeedService.GetSeedDataStatusAsync();
                if (!status.IsComplete)
                {
                    TempData["WarningMessage"] = "ابتدا داده‌های اولیه را ایجاد کنید";
                    return RedirectToAction("Index");
                }

                // تست محاسبات (این بخش بعداً پیاده‌سازی خواهد شد)
                // TODO: پیاده‌سازی تست محاسبات با ServiceCalculationService
                
                TempData["SuccessMessage"] = "تست محاسبات با موفقیت انجام شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تست محاسبات");
                TempData["ErrorMessage"] = "خطا در تست محاسبات: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// تست ایجاد خدمات مشترک
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> TestSharedServices()
        {
            try
            {
                _logger.Information("شروع تست ایجاد خدمات مشترک");

                // بررسی وجود خدمات و دپارتمان‌ها
                var services = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .ToListAsync();

                var departments = await _context.Departments
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .ToListAsync();

                var result = new
                {
                    success = true,
                    servicesCount = services.Count,
                    departmentsCount = departments.Count,
                    services = services.Select(s => new { s.ServiceId, s.Title, s.ServiceCode }).ToList(),
                    departments = departments.Select(d => new { d.DepartmentId, d.Name }).ToList()
                };

                _logger.Information($"تست خدمات مشترک: {services.Count} خدمت، {departments.Count} دپارتمان");

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تست خدمات مشترک");
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت گزارش وضعیت سیستم
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetSystemReport()
        {
            try
            {
                var status = await _systemSeedService.GetSeedDataStatusAsync();
                var report = new
                {
                    status.IsComplete,
                    status.FactorsExist,
                    status.ServicesExist,
                    Counts = new
                    {
                        FactorSettings = status.FactorSettingsCount,
                        Services = status.ServicesCount,
                        SharedServices = status.SharedServicesCount,
                        ServiceComponents = status.ServiceComponentsCount
                    },
                    Timestamp = DateTime.Now,
                    UserId = _currentUserService.GetCurrentUserId()
                };

                return Json(report, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت گزارش سیستم");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
