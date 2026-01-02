using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.ViewModels.Diagnostics;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// 🔍 DIAGNOSTICS CONTROLLER - PRODUCTION DEBUG ONLY
    /// Use this to diagnose authentication and role issues
    /// </summary>
    [Authorize]
    public class DiagnosticsController : Controller
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _log;

        public DiagnosticsController(
            ICurrentUserService currentUserService,
            ApplicationUserManager userManager,
            ApplicationDbContext context,
            ILogger logger)
        {
            _currentUserService = currentUserService;
            _userManager = userManager;
            _context = context;
            _log = logger.ForContext<DiagnosticsController>();
        }

        /// <summary>
        /// GET: /Diagnostics/AuthCheck
        /// نمایش اطلاعات کامل Authentication و Claims
        /// </summary>
        public async Task<ActionResult> AuthCheck()
        {
            try
            {
                // ✅ CRITICAL TEST: Direct Claims access
                string directUserId = null;
                if (User?.Identity is ClaimsIdentity claimsIdentity)
                {
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    directUserId = claim?.Value;
                }

                var model = new AuthDiagnosticsViewModel
                {
                    // Basic Auth Info
                    RequestIsAuthenticated = Request.IsAuthenticated,
                    UserIdentityIsAuthenticated = User?.Identity?.IsAuthenticated ?? false,
                    UserIdentityName = User?.Identity?.Name ?? "NULL",
                    UserId = User?.Identity?.GetUserId() ?? "NULL",

                    // CurrentUserService Info
                    // ✅ BYPASS CurrentUserService - use direct value for testing
                    CurrentUserServiceUserId = directUserId ?? "NULL (Direct test)",
                    CurrentUserServiceIsAuthenticated = User.Identity.IsAuthenticated,
                    CurrentUserServiceIsPatient = User.IsInRole("Patient"),
                    CurrentUserServiceIsAdmin = User.IsInRole("Admin"),
                    CurrentUserServiceIsDoctor = User.IsInRole("Doctor"),

                    // Claims (بسیار مهم!)
                    Claims = User.Identity is ClaimsIdentity claims
                        ? claims.Claims.Select(c => new ClaimInfo { Type = c.Type, Value = c.Value }).ToList()
                        : new List<ClaimInfo>(),

                    // Database Roles
                    DatabaseRoles = await GetDatabaseRolesAsync(),

                    // Patient Record - DIRECT TEST
                    PatientRecord = await GetPatientRecordDirectAsync(directUserId),

                    // Cookie Info
                    Cookies = Request.Cookies.AllKeys
                        .Where(k => k.Contains("AspNet") || k.Contains("Clinic"))
                        .Select(k => new CookieInfo { Key = k, HasValue = !string.IsNullOrEmpty(Request.Cookies[k]?.Value) })
                        .ToList()
                };

                ViewBag.DiagData = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error in AuthCheck diagnostics");
                ViewBag.Error = ex.ToString();
                return View();
            }
        }

        private async Task<object> GetDatabaseRolesAsync()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return "User ID is null";

                var roles = await _userManager.GetRolesAsync(userId);
                return new
                {
                    UserId = userId,
                    Roles = roles,
                    Count = roles.Count
                };
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private async Task<object> GetPatientRecordInfoAsync()
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return "CurrentUserService.UserId is null";

                var patient = await _context.Patients
                    .Include(p => p.ApplicationUser)
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == userId && !p.IsDeleted);

                if (patient == null)
                {
                    return new
                    {
                        Found = false,
                        Message = "Patient record NOT found in database"
                    };
                }

                return new
                {
                    Found = true,
                    PatientId = patient.PatientId,
                    FullName = $"{patient.FirstName} {patient.LastName}",
                    NationalCode = patient.NationalCode,
                    PhoneNumber = patient.PhoneNumber,
                    CreatedAt = patient.CreatedAt
                };
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private async Task<object> GetPatientRecordDirectAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return "UserId is null (from direct claims)";

                var patient = await _context.Patients
                    .Include(p => p.ApplicationUser)
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == userId && !p.IsDeleted);

                if (patient == null)
                {
                    return new
                    {
                        Found = false,
                        Message = "Patient record NOT found in database",
                        SearchedUserId = userId
                    };
                }

                return new
                {
                    Found = true,
                    PatientId = patient.PatientId,
                    FullName = $"{patient.FirstName} {patient.LastName}",
                    NationalCode = patient.NationalCode,
                    PhoneNumber = patient.PhoneNumber,
                    CreatedAt = patient.CreatedAt
                };
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}

