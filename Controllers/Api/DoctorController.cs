using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers.Base;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Controllers.Api
{
    /// <summary>
    /// API Controller مدیریت پزشکان در پذیرش
    /// 
    /// Responsibilities:
    /// 1. دریافت لیست پزشکان
    /// 2. دریافت دپارتمان‌های پزشک
    /// 3. دریافت پزشکان بر اساس شیفت
    /// 4. دریافت شیفت فعلی
    /// 5. دریافت اطلاعات شیفت
    /// 
    /// Architecture:
    /// ✅ Single Responsibility: فقط Doctor
    /// ✅ No Cache: طبق سیاست
    /// ✅ Conditional Authorization
    /// </summary>
    public class DoctorController : ReceptionBaseController
    {
        #region Fields

        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor

        public DoctorController(
            ApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger logger) : base(currentUserService, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #endregion

        #region Doctor Actions

        /// <summary>
        /// دریافت لیست پزشکان فعال
        /// </summary>
        /// <returns>لیست پزشکان</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetAll()
        {
            using (StartPerformanceMonitoring("GetDoctors"))
            {
                try
                {
                    _logger.Information("👨‍⚕️ دریافت لیست پزشکان. کاربر: {UserName}", 
                        _currentUserService.UserName);

                    AddSecurityHeaders();

                    var doctors = await _context.Doctors
                        .Where(d => d.IsActive)
                        .OrderBy(d => d.LastName)
                        .ThenBy(d => d.FirstName)
                        .Select(d => new
                        {
                            d.DoctorId,
                            FullName = d.FirstName + " " + d.LastName,
                            d.DoctorCode,
                            d.IsActive,
                            DepartmentCount = d.DoctorDepartments.Count(dd => dd.IsActive),
                            Departments = d.DoctorDepartments
                                .Where(dd => dd.IsActive)
                                .Select(dd => new
                                {
                                    dd.Department.DepartmentId,
                                    dd.Department.Name
                                })
                                .ToList()
                        })
                        .ToListAsync();

                    _logger.Information("✅ {Count} پزشک دریافت شد", doctors.Count);

                    return SuccessResponse(doctors, $"{doctors.Count} پزشک دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetDoctors");
                }
            }
        }

        /// <summary>
        /// دریافت دپارتمان‌های پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>لیست دپارتمان‌ها</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetDepartments(int doctorId)
        {
            using (StartPerformanceMonitoring("GetDoctorDepartments"))
            {
                try
                {
                    _logger.Information(
                        "🏥 دریافت دپارتمان‌های پزشک. پزشک: {DoctorId}, کاربر: {UserName}",
                        doctorId, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (doctorId <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه پزشک نامعتبر است"
                        });
                    }

                    var departments = await _context.DoctorDepartments
                        .Where(dd => dd.DoctorId == doctorId && dd.IsActive)
                        .Select(dd => new
                        {
                            dd.Department.DepartmentId,
                            dd.Department.Name,
                            dd.Department.Code,
                            dd.Department.Description,
                            dd.IsActive,
                            ClinicName = dd.Department.Clinic.Name
                        })
                        .OrderBy(d => d.Name)
                        .ToListAsync();

                    _logger.Information("✅ {Count} دپارتمان دریافت شد", departments.Count);

                    return SuccessResponse(departments, $"{departments.Count} دپارتمان دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetDoctorDepartments", new { doctorId });
                }
            }
        }

        #endregion

        #region Shift Management Actions

        /// <summary>
        /// دریافت پزشکان بر اساس شیفت
        /// </summary>
        /// <param name="shiftType">نوع شیفت</param>
        /// <returns>لیست پزشکان</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetByShift(ShiftType shiftType)
        {
            using (StartPerformanceMonitoring("GetDoctorsByShift"))
            {
                try
                {
                    _logger.Information(
                        "⏰ دریافت پزشکان بر اساس شیفت. شیفت: {ShiftType}, کاربر: {UserName}",
                        shiftType, _currentUserService.UserName);

                    AddSecurityHeaders();

                    var doctors = await _context.Doctors
                        .Where(d => d.IsActive)
                        .Where(d => d.DoctorSchedules.Any(ds => 
                            ds.ShiftType == shiftType && 
                            ds.IsActive))
                        .OrderBy(d => d.LastName)
                        .ThenBy(d => d.FirstName)
                        .Select(d => new
                        {
                            d.DoctorId,
                            FullName = d.FirstName + " " + d.LastName,
                            d.DoctorCode,
                            ShiftCount = d.DoctorSchedules.Count(ds => 
                                ds.ShiftType == shiftType && 
                                ds.IsActive)
                        })
                        .ToListAsync();

                    _logger.Information("✅ {Count} پزشک در شیفت {ShiftType} دریافت شد", 
                        doctors.Count, shiftType);

                    return SuccessResponse(doctors, 
                        $"{doctors.Count} پزشک در شیفت {shiftType} دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetDoctorsByShift", new { shiftType });
                }
            }
        }

        /// <summary>
        /// دریافت شیفت فعلی
        /// </summary>
        /// <returns>شیفت فعلی</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetCurrentShift()
        {
            using (StartPerformanceMonitoring("GetCurrentShift"))
            {
                try
                {
                    _logger.Information("⏰ دریافت شیفت فعلی. کاربر: {UserName}", 
                        _currentUserService.UserName);

                    AddSecurityHeaders();

                    var currentTime = DateTime.Now.TimeOfDay;
                    ShiftType currentShift;

                    // تعیین شیفت بر اساس ساعت فعلی
                    if (currentTime >= new TimeSpan(7, 0, 0) && currentTime < new TimeSpan(15, 0, 0))
                    {
                        currentShift = ShiftType.Morning;
                    }
                    else if (currentTime >= new TimeSpan(15, 0, 0) && currentTime < new TimeSpan(23, 0, 0))
                    {
                        currentShift = ShiftType.Evening;
                    }
                    else
                    {
                        currentShift = ShiftType.Night;
                    }

                    var result = new
                    {
                        ShiftType = currentShift,
                        ShiftName = GetShiftName(currentShift),
                        CurrentTime = DateTime.Now,
                        CurrentHour = DateTime.Now.Hour
                    };

                    _logger.Information("✅ شیفت فعلی: {ShiftType}", currentShift);

                    return await Task.FromResult(SuccessResponse(result, 
                        $"شیفت فعلی: {GetShiftName(currentShift)}"));
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetCurrentShift");
                }
            }
        }

        /// <summary>
        /// دریافت اطلاعات شیفت
        /// </summary>
        /// <param name="shiftType">نوع شیفت</param>
        /// <returns>اطلاعات شیفت</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetShiftInfo(ShiftType shiftType)
        {
            using (StartPerformanceMonitoring("GetShiftInfo"))
            {
                try
                {
                    _logger.Information(
                        "ℹ️ دریافت اطلاعات شیفت. شیفت: {ShiftType}, کاربر: {UserName}",
                        shiftType, _currentUserService.UserName);

                    AddSecurityHeaders();

                    var shiftInfo = new
                    {
                        ShiftType = shiftType,
                        ShiftName = GetShiftName(shiftType),
                        StartTime = GetShiftStartTime(shiftType),
                        EndTime = GetShiftEndTime(shiftType),
                        DoctorCount = await _context.DoctorSchedules
                            .Where(ds => ds.ShiftType == shiftType && ds.IsActive)
                            .Select(ds => ds.DoctorId)
                            .Distinct()
                            .CountAsync()
                    };

                    _logger.Information("✅ اطلاعات شیفت {ShiftType} دریافت شد", shiftType);

                    return SuccessResponse(shiftInfo, $"اطلاعات شیفت {GetShiftName(shiftType)} دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetShiftInfo", new { shiftType });
                }
            }
        }

        #endregion

        #region Helper Methods

        private string GetShiftName(ShiftType shiftType)
        {
            switch (shiftType)
            {
                case ShiftType.Morning:
                    return "صبح";
                case ShiftType.Evening:
                    return "عصر";
                case ShiftType.Night:
                    return "شب";
                default:
                    return "نامشخص";
            }
        }

        private string GetShiftStartTime(ShiftType shiftType)
        {
            switch (shiftType)
            {
                case ShiftType.Morning:
                    return "07:00";
                case ShiftType.Evening:
                    return "15:00";
                case ShiftType.Night:
                    return "23:00";
                default:
                    return "00:00";
            }
        }

        private string GetShiftEndTime(ShiftType shiftType)
        {
            switch (shiftType)
            {
                case ShiftType.Morning:
                    return "15:00";
                case ShiftType.Evening:
                    return "23:00";
                case ShiftType.Night:
                    return "07:00";
                default:
                    return "00:00";
            }
        }

        #endregion
    }
}

