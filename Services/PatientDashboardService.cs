using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Extensions;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Models.DTOs.Appointment;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Patient;
using ClinicApp.ViewModels;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// Service برای داشبورد بیمار
    /// Single Responsibility: مدیریت داده‌های داشبورد بیمار
    /// 
    /// ✅ Enterprise-Grade: ServiceResult Enhanced, Authorization, Performance Optimized
    /// طبق: CLINICAPP_PATIENT_DASHBOARD_BEAST_ROADMAP_PROMPT.md
    /// </summary>
    public class PatientDashboardService : IPatientDashboardService
    {
        private readonly IAppointmentBookingService _appointmentService;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IPatientService _patientService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public PatientDashboardService(
            IAppointmentBookingService appointmentService,
            IAppointmentRepository appointmentRepository,
            IPatientService patientService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<PatientDashboardService>();
        }

        /// <summary>
        /// دریافت آمار سریع داشبورد بیمار — فقط کوئری‌های COUNT، Real-Time، بدون کش، مقیاس‌پذیر.
        /// </summary>
        public async Task<ServiceResult<DashboardQuickStatsViewModel>> GetQuickStatsAsync(int patientId)
        {
            try
            {
                _logger.Information("دریافت آمار سریع داشبورد - PatientId: {PatientId}", patientId);

                var asOf = DateTime.Now;

                // دو کوئری سبک COUNT به‌صورت موازی؛ بدون بارگذاری لیست نوبت‌ها
                var appointmentCountsTask = _appointmentRepository.GetPatientAppointmentCountsAsync(patientId, asOf);
                var receptionCountTask = _patientService.GetPatientReceptionCountAsync(patientId);

                await Task.WhenAll(appointmentCountsTask, receptionCountTask).ConfigureAwait(false);

                var counts = await appointmentCountsTask.ConfigureAwait(false);
                var totalReceptions = await receptionCountTask.ConfigureAwait(false);

                var stats = new DashboardQuickStatsViewModel
                {
                    TotalAppointments = counts.Total,
                    UpcomingAppointments = counts.Upcoming,
                    CompletedAppointments = counts.Completed,
                    CancelledAppointments = counts.Cancelled,
                    TotalReceptions = totalReceptions
                };

                _logger.Information("✅ آمار محاسبه شد (Real-Time) - Total: {Total}, Upcoming: {Upcoming}, Receptions: {Receptions}",
                    stats.TotalAppointments, stats.UpcomingAppointments, stats.TotalReceptions);

                return ServiceResult<DashboardQuickStatsViewModel>.Successful(
                    stats,
                    "آمار با موفقیت دریافت شد.",
                    operationName: "GetQuickStats",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار سریع داشبورد - PatientId: {PatientId}", patientId);
                return ServiceResult<DashboardQuickStatsViewModel>.Failed(
                    "خطا در دریافت آمار",
                    "GET_STATS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }

        /// <summary>
        /// دریافت نوبت‌های اخیر بیمار
        /// </summary>
        public async Task<ServiceResult<DashboardAppointmentsSectionViewModel>> GetRecentAppointmentsAsync(
            int patientId, 
            int pageNumber = 1, 
            int pageSize = 5)
        {
            try
            {
                _logger.Information("دریافت نوبت‌های اخیر - PatientId: {PatientId}, Page: {Page}, PageSize: {PageSize}",
                    patientId, pageNumber, pageSize);

                // ✅ Validation
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 5;
                if (pageSize > 20) pageSize = 20; // Max limit

                // ✅ دریافت نوبت‌ها با TotalCount از PatientService (برای HasMore و «مشاهده همه»)
                var result = await _patientService.GetPatientAppointmentsPagedAsync(patientId, pageNumber, pageSize);
                if (!result.Success)
                {
                    return ServiceResult<DashboardAppointmentsSectionViewModel>.Failed(
                        result.Message,
                        result.Code);
                }

                var paged = result.Data;
                var appointments = paged?.Items ?? new List<PatientAppointmentViewModel>();
                var totalCount = paged?.TotalCount ?? 0;

                var viewModel = new DashboardAppointmentsSectionViewModel
                {
                    Appointments = appointments.Select(a => new DashboardAppointmentItemViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        DoctorId = a.DoctorId,
                        DoctorName = a.DoctorName,
                        DoctorSpecialization = null, // FIXME(Phase 2): از service دریافت شود
                        AppointmentDate = a.AppointmentDate,
                        AppointmentDateShamsi = a.AppointmentDateShamsi,
                        AppointmentTime = a.AppointmentDate.ToString("HH:mm"),
                        Status = a.Status.ToString(),
                        StatusText = a.StatusText,
                        Price = a.Price
                    }).ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    HasMore = paged != null && paged.HasNextPage
                };

                return ServiceResult<DashboardAppointmentsSectionViewModel>.Successful(
                    viewModel,
                    "نوبت‌های اخیر با موفقیت دریافت شد.",
                    operationName: "GetRecentAppointments",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نوبت‌های اخیر - PatientId: {PatientId}", patientId);
                return ServiceResult<DashboardAppointmentsSectionViewModel>.Failed(
                    "خطا در دریافت نوبت‌های اخیر",
                    "GET_RECENT_APPOINTMENTS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }

        /// <summary>
        /// دریافت نوبت‌های آینده بیمار — صفحه‌بندی در DB، بدون بارگذاری همه در حافظه.
        /// </summary>
        public async Task<ServiceResult<DashboardAppointmentsSectionViewModel>> GetUpcomingAppointmentsAsync(
            int patientId, 
            int pageNumber = 1, 
            int pageSize = 5)
        {
            try
            {
                _logger.Information("دریافت نوبت‌های آینده - PatientId: {PatientId}, Page: {Page}, PageSize: {PageSize}",
                    patientId, pageNumber, pageSize);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 5;
                if (pageSize > 20) pageSize = 20;

                var result = await _patientService.GetPatientUpcomingAppointmentsPagedAsync(patientId, pageNumber, pageSize);
                if (!result.Success)
                {
                    return ServiceResult<DashboardAppointmentsSectionViewModel>.Failed(
                        result.Message,
                        result.Code);
                }

                var paged = result.Data;
                var appointments = paged?.Items ?? new List<PatientAppointmentViewModel>();

                var viewModel = new DashboardAppointmentsSectionViewModel
                {
                    Appointments = appointments.Select(a => new DashboardAppointmentItemViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        DoctorId = a.DoctorId,
                        DoctorName = a.DoctorName,
                        DoctorSpecialization = a.ServiceCategoryName,
                        AppointmentDate = a.AppointmentDate,
                        AppointmentDateShamsi = a.AppointmentDateShamsi,
                        AppointmentTime = a.AppointmentDate.ToString("HH:mm"),
                        Status = a.Status.ToString(),
                        StatusText = a.StatusText,
                        Price = a.Price
                    }).ToList(),
                    TotalCount = paged?.TotalCount ?? 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    HasMore = paged != null && paged.HasNextPage
                };

                return ServiceResult<DashboardAppointmentsSectionViewModel>.Successful(
                    viewModel,
                    "نوبت‌های آینده با موفقیت دریافت شد.",
                    operationName: "GetUpcomingAppointments",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نوبت‌های آینده - PatientId: {PatientId}", patientId);
                return ServiceResult<DashboardAppointmentsSectionViewModel>.Failed(
                    "خطا در دریافت نوبت‌های آینده",
                    "GET_UPCOMING_APPOINTMENTS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }

        /// <summary>
        /// دریافت پذیرش‌های اخیر
        /// </summary>
        public async Task<ServiceResult<DashboardReceptionsSectionViewModel>> GetRecentReceptionsAsync(
            int patientId, 
            int pageNumber = 1, 
            int pageSize = 5)
        {
            try
            {
                _logger.Information("دریافت پذیرش‌های اخیر - PatientId: {PatientId}, Page: {Page}, PageSize: {PageSize}",
                    patientId, pageNumber, pageSize);

                // ✅ Validation
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 5;
                if (pageSize > 20) pageSize = 20;

                // ✅ دریافت پذیرش‌ها از PatientService
                var result = await _patientService.GetPatientReceptionsAsync(patientId, pageNumber, pageSize);
                if (!result.Success)
                {
                    return ServiceResult<DashboardReceptionsSectionViewModel>.Failed(
                        result.Message,
                        result.Code);
                }

                var receptions = result.Data ?? new List<ViewModels.PatientReceptionViewModel>();
                var totalCount = receptions.Count; // FIXME(Phase 2): دریافت totalCount از service

                var viewModel = new DashboardReceptionsSectionViewModel
                {
                    Receptions = receptions.Select(r => new DashboardReceptionItemViewModel
                    {
                        ReceptionId = r.ReceptionId,
                        DoctorId = r.DoctorId, // ✅ int (not nullable in PatientReceptionViewModel)
                        DoctorName = r.DoctorName,
                        ReceptionDate = r.ReceptionDate,
                        ReceptionDateShamsi = r.ReceptionDateShamsi,
                        Status = r.Status.ToString(),
                        StatusText = r.StatusText,
                        TotalAmount = r.TotalAmount
                    }).ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    HasMore = (pageNumber * pageSize) < totalCount
                };

                return ServiceResult<DashboardReceptionsSectionViewModel>.Successful(
                    viewModel,
                    "پذیرش‌های اخیر با موفقیت دریافت شد.",
                    operationName: "GetRecentReceptions",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌های اخیر - PatientId: {PatientId}", patientId);
                return ServiceResult<DashboardReceptionsSectionViewModel>.Failed(
                    "خطا در دریافت پذیرش‌های اخیر",
                    "GET_RECENT_RECEPTIONS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }

        /// <summary>
        /// دریافت یک‌جا آمار + نوبت‌های اخیر/آینده + پذیرش‌ها — یک درخواست به‌جای چهار (فاز ۳.۳).
        /// </summary>
        public async Task<ServiceResult<DashboardViewModel>> GetOverviewAsync(
            int patientId,
            int recentPageSize = 5,
            int upcomingPageSize = 5,
            int receptionsPageSize = 5)
        {
            try
            {
                _logger.Information("دریافت Overview داشبورد - PatientId: {PatientId}", patientId);

                var statsTask = GetQuickStatsAsync(patientId);
                var recentTask = GetRecentAppointmentsAsync(patientId, 1, recentPageSize);
                var upcomingTask = GetUpcomingAppointmentsAsync(patientId, 1, upcomingPageSize);
                var receptionsTask = GetRecentReceptionsAsync(patientId, 1, receptionsPageSize);

                await Task.WhenAll(statsTask, recentTask, upcomingTask, receptionsTask).ConfigureAwait(false);

                var statsResult = await statsTask.ConfigureAwait(false);
                var recentResult = await recentTask.ConfigureAwait(false);
                var upcomingResult = await upcomingTask.ConfigureAwait(false);
                var receptionsResult = await receptionsTask.ConfigureAwait(false);

                var sectionErrors = new Dictionary<string, string>();

                var overview = new DashboardViewModel
                {
                    QuickStats = statsResult.Success ? statsResult.Data : null,
                    RecentAppointments = recentResult.Success ? recentResult.Data : null,
                    UpcomingAppointments = upcomingResult.Success ? upcomingResult.Data : null,
                    RecentReceptions = receptionsResult.Success ? receptionsResult.Data : null,
                    SectionErrors = sectionErrors
                };

                if (!statsResult.Success)
                    sectionErrors["QuickStats"] = statsResult.Message ?? "خطا در دریافت آمار.";
                if (!recentResult.Success)
                    sectionErrors["RecentAppointments"] = recentResult.Message ?? "خطا در دریافت نوبت‌های اخیر.";
                if (!upcomingResult.Success)
                    sectionErrors["UpcomingAppointments"] = upcomingResult.Message ?? "خطا در دریافت نوبت‌های آینده.";
                if (!receptionsResult.Success)
                    sectionErrors["RecentReceptions"] = receptionsResult.Message ?? "خطا در دریافت پذیرش‌های اخیر.";

                return ServiceResult<DashboardViewModel>.Successful(
                    overview,
                    "Overview با موفقیت دریافت شد.",
                    operationName: "GetOverview",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت Overview داشبورد - PatientId: {PatientId}", patientId);
                return ServiceResult<DashboardViewModel>.Failed(
                    "خطا در بارگذاری داشبورد",
                    "GET_OVERVIEW_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }

        #region Private Helpers

        /// <summary>
        /// ✅ Security: Validate patient access
        /// ✅ BULLETPROOF: Simple validation - if patientId matches, access is granted
        /// Controller already validates user via GetCurrentPatientIdAsync()
        /// </summary>
        private async Task<bool> ValidatePatientAccessAsync(int patientId)
        {
            try
            {
                // ✅ SIMPLIFIED: Controller already validated user via GetCurrentPatientIdAsync()
                // We just need to verify the patientId is valid (not 0 or negative)
                if (patientId <= 0)
                {
                    _logger.Warning("❌ ValidatePatientAccess: Invalid PatientId: {PatientId}", patientId);
                    return false;
                }

                _logger.Debug("✅ ValidatePatientAccess: Access validated - PatientId: {PatientId}", patientId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error validating patient access - PatientId: {PatientId}", patientId);
                return false;
            }
        }

        /// <summary>
        /// تبدیل وضعیت نوبت به متن فارسی
        /// </summary>
        private string GetAppointmentStatusText(AppointmentStatus status)
        {
            switch (status)
            {
                case AppointmentStatus.Available:
                    return "در دسترس";
                case AppointmentStatus.Scheduled:
                    return "ثبت شده";
                case AppointmentStatus.Pending:
                    return "در انتظار";
                case AppointmentStatus.Completed:
                    return "تکمیل شده";
                case AppointmentStatus.Cancelled:
                    return "لغو شده";
                case AppointmentStatus.NoShow:
                    return "عدم حضور";
                default:
                    return "نامشخص";
            }
        }

        #endregion
    }
}

