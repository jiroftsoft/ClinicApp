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
        private readonly IPatientService _patientService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public PatientDashboardService(
            IAppointmentBookingService appointmentService,
            IPatientService patientService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<PatientDashboardService>();
        }

        /// <summary>
        /// دریافت آمار سریع داشبورد بیمار
        /// </summary>
        public async Task<ServiceResult<DashboardQuickStatsViewModel>> GetQuickStatsAsync(int patientId)
        {
            try
            {
                // ✅ Security: Validate patientId against current user
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult<DashboardQuickStatsViewModel>.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }

                _logger.Information("دریافت آمار سریع داشبورد - PatientId: {PatientId}", patientId);

                // ✅ دریافت تمام نوبت‌ها برای محاسبه آمار
                var allAppointmentsResult = await _appointmentService.GetPatientAppointmentsAsync(patientId);
                if (!allAppointmentsResult.Success)
                {
                    return ServiceResult<DashboardQuickStatsViewModel>.Failed(
                        allAppointmentsResult.Message,
                        allAppointmentsResult.Code);
                }

                var appointments = allAppointmentsResult.Data ?? new List<PatientAppointmentDto>();
                var now = DateTime.Now;

                var stats = new DashboardQuickStatsViewModel
                {
                    TotalAppointments = appointments.Count,
                    UpcomingAppointments = appointments.Count(a => 
                        a.AppointmentDate > now && 
                        a.Status != AppointmentStatus.Cancelled),
                    CompletedAppointments = appointments.Count(a => 
                        a.Status == AppointmentStatus.Completed),
                    CancelledAppointments = appointments.Count(a => 
                        a.Status == AppointmentStatus.Cancelled),
                    TotalReceptions = 0 // TODO: دریافت از ReceptionService
                };

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
                // ✅ Security: Validate patientId
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult<DashboardAppointmentsSectionViewModel>.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }

                _logger.Information("دریافت نوبت‌های اخیر - PatientId: {PatientId}, Page: {Page}, PageSize: {PageSize}",
                    patientId, pageNumber, pageSize);

                // ✅ Validation
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 5;
                if (pageSize > 20) pageSize = 20; // Max limit

                // ✅ دریافت نوبت‌ها از PatientService (با pagination)
                var result = await _patientService.GetPatientAppointmentsAsync(patientId, pageNumber, pageSize);
                if (!result.Success)
                {
                    return ServiceResult<DashboardAppointmentsSectionViewModel>.Failed(
                        result.Message,
                        result.Code);
                }

                var appointments = result.Data ?? new List<ViewModels.PatientAppointmentViewModel>();
                var totalCount = appointments.Count; // TODO: دریافت totalCount از service

                var viewModel = new DashboardAppointmentsSectionViewModel
                {
                    Appointments = appointments.Select(a => new DashboardAppointmentItemViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        DoctorId = a.DoctorId,
                        DoctorName = a.DoctorName,
                        DoctorSpecialization = null, // TODO: از service دریافت شود
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
                    HasMore = (pageNumber * pageSize) < totalCount
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
        /// دریافت نوبت‌های آینده بیمار
        /// </summary>
        public async Task<ServiceResult<DashboardAppointmentsSectionViewModel>> GetUpcomingAppointmentsAsync(
            int patientId, 
            int pageNumber = 1, 
            int pageSize = 5)
        {
            try
            {
                // ✅ Security: Validate patientId
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult<DashboardAppointmentsSectionViewModel>.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }

                _logger.Information("دریافت نوبت‌های آینده - PatientId: {PatientId}, Page: {Page}, PageSize: {PageSize}",
                    patientId, pageNumber, pageSize);

                // ✅ Validation
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 5;
                if (pageSize > 20) pageSize = 20;

                // ✅ دریافت نوبت‌های آینده (از امروز به بعد)
                var result = await _appointmentService.GetPatientAppointmentsAsync(
                    patientId, 
                    startDate: DateTime.Today,
                    endDate: null);
                
                if (!result.Success)
                {
                    return ServiceResult<DashboardAppointmentsSectionViewModel>.Failed(
                        result.Message,
                        result.Code);
                }

                var allAppointments = result.Data ?? new List<PatientAppointmentDto>();
                var now = DateTime.Now;

                // ✅ فیلتر: فقط نوبت‌های آینده و غیر لغو شده
                var upcomingAppointments = allAppointments
                    .Where(a => a.AppointmentDate > now && a.Status != AppointmentStatus.Cancelled)
                    .OrderBy(a => a.AppointmentDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalCount = allAppointments.Count(a => 
                    a.AppointmentDate > now && a.Status != AppointmentStatus.Cancelled);

                var viewModel = new DashboardAppointmentsSectionViewModel
                {
                    Appointments = upcomingAppointments.Select(a => new DashboardAppointmentItemViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        DoctorId = a.DoctorId,
                        DoctorName = a.DoctorName,
                        DoctorSpecialization = null, // TODO: از service دریافت شود
                        AppointmentDate = a.AppointmentDate,
                        AppointmentDateShamsi = a.AppointmentDate.ToPersianDateTime(),
                        AppointmentTime = a.AppointmentDate.ToString("HH:mm"),
                        Status = a.Status.ToString(),
                        StatusText = GetAppointmentStatusText(a.Status),
                        Price = a.Price
                    }).ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    HasMore = (pageNumber * pageSize) < totalCount
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
                // ✅ Security: Validate patientId
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult<DashboardReceptionsSectionViewModel>.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }

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
                var totalCount = receptions.Count; // TODO: دریافت totalCount از service

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

        #region Private Helpers

        /// <summary>
        /// ✅ Security: Validate patient access
        /// </summary>
        private async Task<bool> ValidatePatientAccessAsync(int patientId)
        {
            try
            {
                var currentPatient = await _currentUserService.GetPatientInfoAsync();
                if (currentPatient == null)
                {
                    _logger.Warning("Patient info not found for current user - UserId: {UserId}",
                        _currentUserService.UserId);
                    return false;
                }

                if (currentPatient.PatientId != patientId)
                {
                    _logger.Warning("Unauthorized access attempt - RequestedPatientId: {RequestedId}, CurrentPatientId: {CurrentId}, UserId: {UserId}",
                        patientId, currentPatient.PatientId, _currentUserService.UserId);
                    return false;
                }

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

