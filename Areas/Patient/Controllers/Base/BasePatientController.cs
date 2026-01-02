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
    /// ✅ Security: PatientRoleAuthorization ensures only Patient role users can access
    /// طبق: PATIENT_AUTH_INTEGRATION_ANALYSIS.md
    /// </summary>
    [PatientRoleAuthorization]
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
                    _logger.Warning("❌ User.Identity.GetUserId() returned null - User not authenticated");
                    return null;
                }

                _logger.Debug("🔍 Looking for Patient record - UserId: {UserId}", userId);

                // ✅ DIRECT DATABASE QUERY: Bypass CurrentUserService to avoid DI/caching issues
                // Use the same ApplicationDbContext that CurrentUserService uses
                var dbContext = System.Web.Mvc.DependencyResolver.Current.GetService<ClinicApp.Models.ApplicationDbContext>();
                
                if (dbContext == null)
                {
                    _logger.Error("❌ ApplicationDbContext not available from DI");
                    return null;
                }

                var patient = await dbContext.Patients
                    .Where(p => p.ApplicationUserId == userId && !p.IsDeleted)
                    .Select(p => new { p.PatientId })
                    .FirstOrDefaultAsync();

                if (patient == null)
                {
                    _logger.Warning("⚠️ Patient record not found for UserId: {UserId}", userId);
                    return null;
                }

                _logger.Debug("✅ Patient found - PatientId: {PatientId}", patient.PatientId);
                return patient.PatientId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Exception in GetCurrentPatientIdAsync");
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

