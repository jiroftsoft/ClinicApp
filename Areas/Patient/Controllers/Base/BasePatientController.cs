using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Filters;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using Microsoft.AspNet.Identity;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers.Base
{
    /// <summary>
    /// Base Controller برای تمام Patient Area Controllers
    /// طبق appointment_controller_review.md - فاز 1
    /// 
    /// ⚠️ TEMPORARY: Authorization موقتاً غیرفعال شده است برای رفع مشکل redirect
    /// 
    /// ✅ MODERN STANDARD: برای فعال‌سازی Claims-Based Authorization:
    /// 1. Uncomment خط زیر و از PatientClaimAuthorizationAttribute استفاده کنید
    /// 2. PatientClaimAuthorizationAttribute از Claims استفاده می‌کند (روش استاندارد امروزی)
    /// 3. این روش در تمام پروژه‌های مدرن استفاده می‌شود و با ASP.NET Core Identity سازگار است
    /// 
    /// [PatientClaimAuthorization] // ✅ MODERN: Claims-Based Authorization (روش استاندارد)
    /// [PatientRoleAuthorization] // ❌ OLD: Role-Based Authorization (legacy)
    /// </summary>
    // ⚠️ TEMPORARY: موقتاً غیرفعال برای رفع مشکل redirect
    public abstract class BasePatientController : Controller
    {
        protected readonly ILogger _logger;
        protected readonly ICurrentUserService _currentUserService;
        protected readonly ApplicationDbContext _context;

        // ✅ BACKWARD COMPATIBLE: Keep old constructor
        protected BasePatientController(
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? 
                throw new ArgumentNullException(nameof(currentUserService));
        }

        // ✅ NEW: Constructor with ApplicationDbContext
        protected BasePatientController(
            ILogger logger,
            ICurrentUserService currentUserService,
            ApplicationDbContext context) : this(logger, currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// کلید کش درخواستی برای PatientId — یک بار در هر request محاسبه، بقیه از کش
        /// </summary>
        private const string PatientIdCacheKey = "__CurrentPatientId";
        private const string PatientIdNotFoundKey = "__CurrentPatientId_NotFound";

        /// <summary>
        /// دریافت شناسه بیمار از کاربر فعلی
        /// ✅ STANDARD ASP.NET Identity approach - استفاده مستقیم از User.Identity و Database
        /// ✅ Request-scoped cache: در همان درخواست چند بار فراخوانی نشود (کاهش کوئری تکراری)
        /// ⚠️ CRITICAL: DO NOT use CurrentUserService - it has caching/DI issues
        /// </summary>
        protected async Task<int?> GetCurrentPatientIdAsync()
        {
            try
            {
                if (HttpContext?.Items != null)
                {
                    if (HttpContext.Items[PatientIdCacheKey] is int cachedId)
                    {
                        _logger.Debug("[PatientLink] GetCurrentPatientIdAsync - cache HIT, PatientId={PatientId}", cachedId);
                        return cachedId;
                    }
                    if (HttpContext.Items[PatientIdNotFoundKey] != null)
                    {
                        _logger.Debug("[PatientLink] GetCurrentPatientIdAsync - cache NOT FOUND (cached null)");
                        return null;
                    }
                }

                var userId = User.Identity.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.Warning("[PatientLink] GetCurrentPatientIdAsync - UserId is NULL (not authenticated)");
                    return null;
                }

                var userName = User.Identity.Name;
                var isAuthenticated = User.Identity.IsAuthenticated;
                var isPatientRole = User.IsInRole("Patient");
                _logger.Information("[PatientLink] GetCurrentPatientIdAsync - UserId={UserId}, UserName={UserName}, IsAuth={IsAuth}, IsPatientRole={IsPatient}", userId, userName ?? "NULL", isAuthenticated, isPatientRole);

                // ✅ DIRECT DATABASE QUERY: Bypass CurrentUserService to avoid DI/caching issues
                var dbContext = _context ?? System.Web.Mvc.DependencyResolver.Current.GetService<ClinicApp.Models.ApplicationDbContext>();
                
                if (dbContext == null)
                {
                    _logger.Error("❌ GetCurrentPatientIdAsync: ApplicationDbContext not available from DI");
                    return null;
                }

                // ✅ Enhanced Query: Log SQL query and results
                _logger.Debug("🔍 Querying Patients table - ApplicationUserId: {UserId}", userId);
                
                var patient = await dbContext.Patients
                    .Where(p => p.ApplicationUserId == userId && !p.IsDeleted)
                    .Select(p => new { p.PatientId, p.FirstName, p.LastName, p.NationalCode })
                    .FirstOrDefaultAsync();

                if (patient == null)
                {
                    if (HttpContext?.Items != null)
                        HttpContext.Items[PatientIdNotFoundKey] = true;
                    var totalPatientsCount = await dbContext.Patients.CountAsync(p => !p.IsDeleted);
                    var patientsWithThisUserId = await dbContext.Patients
                        .Where(p => p.ApplicationUserId == userId)
                        .Select(p => new { p.PatientId, p.IsDeleted })
                        .ToListAsync();
                    _logger.Warning("[PatientLink] GetCurrentPatientIdAsync - NOT FOUND. UserId={UserId}, TotalPatients={Total}, RowsWithThisUserId={Count} (IsDeleted?)", userId, totalPatientsCount, patientsWithThisUserId.Count);
                    if (patientsWithThisUserId.Any())
                        _logger.Warning("[PatientLink] GetCurrentPatientIdAsync - Those rows: {@Patients}", patientsWithThisUserId);
                    return null;
                }

                if (HttpContext?.Items != null)
                    HttpContext.Items[PatientIdCacheKey] = patient.PatientId;
                _logger.Information("[PatientLink] GetCurrentPatientIdAsync - FOUND PatientId={PatientId}, Name={Name}", patient.PatientId, $"{patient.FirstName} {patient.LastName}");
                    
                return patient.PatientId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Exception in GetCurrentPatientIdAsync - UserId: {UserId}", User?.Identity?.GetUserId());
                return null;
            }
        }

        /// <summary>
        /// پاک کردن کش درخواستی PatientId تا بار بعد GetCurrentPatientIdAsync دوباره از دیتابیس بخواند.
        /// </summary>
        protected void ClearPatientIdRequestCache()
        {
            if (HttpContext?.Items == null) return;
            HttpContext.Items.Remove(PatientIdCacheKey);
            HttpContext.Items.Remove(PatientIdNotFoundKey);
        }

        /// <summary>
        /// تنظیم کش درخواستی PatientId (مثلاً بعد از Ensure که PatientId از سرویس برگشته و نیازی به کوئری دوم نیست).
        /// </summary>
        protected void SetPatientIdRequestCache(int patientId)
        {
            if (HttpContext?.Items == null) return;
            HttpContext.Items.Remove(PatientIdNotFoundKey);
            HttpContext.Items[PatientIdCacheKey] = patientId;
        }

        /// <summary>
        /// JSON Result موفق
        /// </summary>
        protected JsonResult SuccessJsonResult(object data, string message = null)
        {
            // ✅ استفاده از if-else به جای conditional expression برای جلوگیری از Type Inference Error
            if (message != null)
            {
                return Json(new { success = true, data, message }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// JSON Result خطا
        /// </summary>
        protected JsonResult ErrorJsonResult(string message)
        {
            return Json(new { success = false, message }, JsonRequestBehavior.AllowGet);
        }
    }
}

