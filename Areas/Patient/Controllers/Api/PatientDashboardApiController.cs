using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Areas.Patient.Controllers.Base;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.ViewModels.Patient;
using Microsoft.AspNet.Identity;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers.Api
{
    /// <summary>
    /// API Controller برای داشبورد بیمار
    /// Single Responsibility: ارائه API endpoints برای AJAX loading sections
    /// 
    /// ✅ Enterprise-Grade: ServiceResult Enhanced, Authorization, AJAX-First
    /// طبق: CLINICAPP_PATIENT_DASHBOARD_BEAST_ROADMAP_PROMPT.md
    /// </summary>
    [Authorize]
    public class PatientDashboardApiController : BasePatientController
    {
        private readonly IPatientDashboardService _dashboardService;

        public PatientDashboardApiController(
            IPatientDashboardService dashboardService,
            ILogger logger,
            ICurrentUserService currentUserService,
            ApplicationDbContext context)
            : base(logger, currentUserService, context)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        /// <summary>
        /// 🔍 DIAGNOSTIC: Check authentication and patient mapping
        /// GET: /Patient/Api/PatientDashboard/DiagnoseAuth
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> DiagnoseAuth()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var userName = User.Identity.Name;
                var isAuthenticated = User.Identity.IsAuthenticated;
                var isPatientRole = User.IsInRole("Patient");
                
                var patientId = await GetCurrentPatientIdAsync();
                
                var diagnostic = new
                {
                    userId,
                    userName,
                    isAuthenticated,
                    isPatientRole,
                    patientId,
                    hasPatientRecord = patientId.HasValue,
                    message = patientId.HasValue 
                        ? $"✅ Patient record found - PatientId: {patientId}" 
                        : "❌ Patient record NOT FOUND - User has Patient role but no Patient record in database"
                };
                
                _logger.Warning("🔍 DIAGNOSTIC: {@Diagnostic}", diagnostic);
                
                return Json(new { success = true, data = diagnostic }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error in DiagnoseAuth");
                return ErrorJsonResult("خطا در تشخیص");
            }
        }

        /// <summary>
        /// دریافت آمار سریع داشبورد — Real-Time، بدون کش (کوئری سبک COUNT).
        /// GET: /Patient/Api/PatientDashboard/GetQuickStats
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetQuickStats()
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    _logger.Warning("❌ GetQuickStats: GetCurrentPatientIdAsync returned null - returning error");
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }

                _logger.Information("✅ GetQuickStats: PatientId={PatientId}, calling service", patientId.Value);
                
                var result = await _dashboardService.GetQuickStatsAsync(patientId.Value);
                if (!result.Success)
                {
                    _logger.Warning("⚠️ GetQuickStats: Service returned failure - Message: {Message}", result.Message);
                    return ErrorJsonResult(result.Message);
                }

                return SuccessJsonResult(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Exception in GetQuickStats");
                return ErrorJsonResult("خطا در دریافت آمار");
            }
        }

        /// <summary>
        /// دریافت یک‌جا Overview داشبورد (آمار + نوبت‌های اخیر/آینده + پذیرش‌ها) — یک درخواست به‌جای چهار (فاز ۳.۳).
        /// GET: /Patient/Api/PatientDashboard/GetOverview
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetOverview()
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }

                var result = await _dashboardService.GetOverviewAsync(patientId.Value);
                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }

                return SuccessJsonResult(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در GetOverview");
                return ErrorJsonResult("خطا در بارگذاری داشبورد");
            }
        }

        /// <summary>
        /// دریافت نوبت‌های اخیر
        /// GET: /Patient/Api/PatientDashboard/GetRecentAppointments?pageNumber=1&pageSize=5
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetRecentAppointments(int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }

                var result = await _dashboardService.GetRecentAppointmentsAsync(patientId.Value, pageNumber, pageSize);
                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }

                return SuccessJsonResult(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نوبت‌های اخیر");
                return ErrorJsonResult("خطا در دریافت نوبت‌های اخیر");
            }
        }

        /// <summary>
        /// دریافت نوبت‌های آینده
        /// GET: /Patient/Api/PatientDashboard/GetUpcomingAppointments?pageNumber=1&pageSize=5
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetUpcomingAppointments(int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }

                var result = await _dashboardService.GetUpcomingAppointmentsAsync(patientId.Value, pageNumber, pageSize);
                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }

                return SuccessJsonResult(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نوبت‌های آینده");
                return ErrorJsonResult("خطا در دریافت نوبت‌های آینده");
            }
        }

        /// <summary>
        /// دریافت پذیرش‌های اخیر
        /// GET: /Patient/Api/PatientDashboard/GetRecentReceptions?pageNumber=1&pageSize=5
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetRecentReceptions(int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }

                var result = await _dashboardService.GetRecentReceptionsAsync(patientId.Value, pageNumber, pageSize);
                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }

                return SuccessJsonResult(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌های اخیر");
                return ErrorJsonResult("خطا در دریافت پذیرش‌های اخیر");
            }
        }
    }
}

