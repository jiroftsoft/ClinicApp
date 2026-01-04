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
        /// دریافت شناسه بیمار از کاربر فعلی
        /// ✅ STANDARD ASP.NET Identity approach - استفاده مستقیم از User.Identity و Database
        /// ⚠️ CRITICAL: DO NOT use CurrentUserService - it has caching/DI issues
        /// </summary>
        protected async Task<int?> GetCurrentPatientIdAsync()
        {
            try
            {
                // ✅ STANDARD: Get UserId from User.Identity (Controller base property)
                // This is the ONLY reliable source in ASP.NET MVC Controllers
                var userId = User.Identity.GetUserId();
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.Warning("❌ GetCurrentPatientIdAsync: User.Identity.GetUserId() returned null - User not authenticated");
                    return null;
                }

                // ✅ Enhanced Logging: Log user info for debugging
                var userName = User.Identity.Name;
                var isAuthenticated = User.Identity.IsAuthenticated;
                var isPatientRole = User.IsInRole("Patient");
                
                _logger.Information("🔍 GetCurrentPatientIdAsync - UserId: {UserId}, UserName: {UserName}, IsAuthenticated: {IsAuthenticated}, IsPatientRole: {IsPatientRole}", 
                    userId, userName, isAuthenticated, isPatientRole);

                // ✅ DIRECT DATABASE QUERY: Bypass CurrentUserService to avoid DI/caching issues
                var dbContext = System.Web.Mvc.DependencyResolver.Current.GetService<ClinicApp.Models.ApplicationDbContext>();
                
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
                    // ✅ CRITICAL: Log detailed info when Patient not found
                    var totalPatientsCount = await dbContext.Patients.CountAsync(p => !p.IsDeleted);
                    var patientsWithThisUserId = await dbContext.Patients
                        .Where(p => p.ApplicationUserId == userId)
                        .Select(p => new { p.PatientId, p.IsDeleted })
                        .ToListAsync();
                    
                    _logger.Warning("⚠️ Patient record NOT FOUND - UserId: {UserId}, UserName: {UserName}, TotalPatients: {TotalCount}, PatientsWithThisUserId: {Count}", 
                        userId, userName, totalPatientsCount, patientsWithThisUserId.Count);
                    
                    if (patientsWithThisUserId.Any())
                    {
                        _logger.Warning("⚠️ Found {Count} Patient records with ApplicationUserId={UserId} but IsDeleted=true: {@Patients}", 
                            patientsWithThisUserId.Count, userId, patientsWithThisUserId);
                    }
                    
                    return null;
                }

                _logger.Information("✅ Patient found - PatientId: {PatientId}, Name: {FullName}, NationalCode: {NationalCode}", 
                    patient.PatientId, $"{patient.FirstName} {patient.LastName}", patient.NationalCode);
                    
                return patient.PatientId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Exception in GetCurrentPatientIdAsync - UserId: {UserId}", User?.Identity?.GetUserId());
                return null;
            }
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

