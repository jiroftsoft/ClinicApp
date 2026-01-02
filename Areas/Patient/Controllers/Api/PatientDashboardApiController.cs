using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Areas.Patient.Controllers.Base;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels.Patient;
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
            ICurrentUserService currentUserService)
            : base(logger, currentUserService)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        /// <summary>
        /// دریافت آمار سریع داشبورد
        /// GET: /Patient/Api/PatientDashboard/GetQuickStats
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 30, VaryByCustom = "User")] // ✅ Cache for 30 seconds per user
        public async Task<JsonResult> GetQuickStats()
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }

                var result = await _dashboardService.GetQuickStatsAsync(patientId.Value);
                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }

                return SuccessJsonResult(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار سریع داشبورد");
                return ErrorJsonResult("خطا در دریافت آمار");
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

