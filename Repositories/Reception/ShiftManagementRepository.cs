using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Repositories.Reception
{
    /// <summary>
    /// Repository تخصصی برای مدیریت شیفت‌ها در ماژول پذیرش
    /// </summary>
    public class ShiftManagementRepository : IShiftManagementRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public ShiftManagementRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger.ForContext<ShiftManagementRepository>();
        }

        /// <summary>
        /// دریافت شیفت‌های فعال
        /// </summary>
        public async Task<ServiceResult<List<ShiftLookupViewModel>>> GetActiveShiftsAsync()
        {
            try
            {
                _logger.Information("🕐 دریافت شیفت‌های فعال");

                // دریافت شیفت‌های فعال از DoctorSchedules
                var shifts = await _context.DoctorSchedules
                    .AsNoTracking()
                    .Where(ds => ds.IsShiftActive && !ds.IsDeleted)
                    .GroupBy(ds => new { ds.ShiftType, ds.ShiftStartTime, ds.ShiftEndTime })
                    .Select(g => new ShiftLookupViewModel
                    {
                        ShiftId = (int)g.Key.ShiftType,
                        ShiftName = GetShiftDisplayName(g.Key.ShiftType.ToString()),
                        ShiftType = g.Key.ShiftType.ToString(),
                        StartTime = g.Key.ShiftStartTime.ToString(@"hh\:mm"),
                        EndTime = g.Key.ShiftEndTime.ToString(@"hh\:mm"),
                        IsActive = true
                    })
                    .OrderBy(s => s.ShiftId)
                    .ToListAsync();

                // اگر هیچ شیفتی در دیتابیس نباشد، شیفت‌های پیش‌فرض را برگردان
                if (!shifts.Any())
                {
                    shifts = new List<ShiftLookupViewModel>
                    {
                        new ShiftLookupViewModel
                        {
                            ShiftId = 0,
                            ShiftName = "شیفت صبح",
                            ShiftType = "Morning",
                            StartTime = "08:00",
                            EndTime = "16:00",
                            IsActive = true
                        },
                        new ShiftLookupViewModel
                        {
                            ShiftId = 1,
                            ShiftName = "شیفت عصر",
                            ShiftType = "Evening",
                            StartTime = "16:00",
                            EndTime = "24:00",
                            IsActive = true
                        },
                        new ShiftLookupViewModel
                        {
                            ShiftId = 2,
                            ShiftName = "شیفت شب",
                            ShiftType = "Night",
                            StartTime = "00:00",
                            EndTime = "08:00",
                            IsActive = true
                        }
                    };
                }

                _logger.Information("✅ {Count} شیفت فعال یافت شد", shifts.Count);

                return ServiceResult<List<ShiftLookupViewModel>>.Successful(
                    shifts, "شیفت‌های فعال با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت شیفت‌های فعال");
                return ServiceResult<List<ShiftLookupViewModel>>.Failed(
                    "خطا در بارگذاری شیفت‌های فعال");
            }
        }

        /// <summary>
        /// بررسی فعال بودن شیفت
        /// </summary>
        public async Task<ServiceResult<bool>> IsShiftActiveAsync(int shiftId)
        {
            try
            {
                _logger.Information("🕐 بررسی فعال بودن شیفت: {ShiftId}", shiftId);

                // TODO: پیاده‌سازی منطق بررسی شیفت - فعلاً true برمی‌گردانیم
                var isActive = true;

                _logger.Information("✅ شیفت {ShiftId} فعال است: {IsActive}", shiftId, isActive);

                return ServiceResult<bool>.Successful(
                    isActive, "وضعیت شیفت با موفقیت بررسی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بررسی وضعیت شیفت {ShiftId}", shiftId);
                return ServiceResult<bool>.Failed(
                    "خطا در بررسی وضعیت شیفت");
            }
        }

        /// <summary>
        /// دریافت اطلاعات شیفت فعلی
        /// </summary>
        public async Task<ServiceResult<ShiftInfoViewModel>> GetCurrentShiftInfoAsync()
        {
            try
            {
                _logger.Information("🕐 دریافت اطلاعات شیفت فعلی");

                var currentHour = DateTime.Now.Hour;
                string currentShift;

                if (currentHour >= 8 && currentHour < 16)
                {
                    currentShift = "Morning";
                }
                else if (currentHour >= 16 && currentHour < 24)
                {
                    currentShift = "Evening";
                }
                else
                {
                    currentShift = "Night";
                }

                var shiftInfo = new ShiftInfoViewModel
                {
                    ShiftId = currentShift == "Morning" ? 1 : currentShift == "Evening" ? 2 : 3,
                    ShiftName = currentShift == "Morning" ? "شیفت صبح" : currentShift == "Evening" ? "شیفت عصر" : "شیفت شب",
                    ShiftType = currentShift,
                    IsActive = true,
                    CurrentTime = DateTime.Now.ToString("HH:mm"),
                    StartTime = currentShift == "Morning" ? "08:00" : currentShift == "Evening" ? "16:00" : "00:00",
                    EndTime = currentShift == "Morning" ? "16:00" : currentShift == "Evening" ? "24:00" : "08:00"
                };

                _logger.Information("✅ شیفت فعلی: {ShiftName}", shiftInfo.ShiftName);

                return ServiceResult<ShiftInfoViewModel>.Successful(
                    shiftInfo, "اطلاعات شیفت فعلی با موفقیت بارگذاری شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت اطلاعات شیفت فعلی");
                return ServiceResult<ShiftInfoViewModel>.Failed(
                    "خطا در بارگذاری اطلاعات شیفت فعلی");
            }
        }

        /// <summary>
        /// دریافت تخصص‌های فعال
        /// </summary>
        public async Task<ServiceResult<List<SpecializationLookupViewModel>>> GetActiveSpecializationsAsync()
        {
            try
            {
                _logger.Information("🎓 دریافت تخصص‌های فعال");

                var specializations = await _context.Specializations
                    .AsNoTracking()
                    .Where(s => s.IsActive && !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .Select(s => new SpecializationLookupViewModel
                    {
                        SpecializationId = s.SpecializationId,
                        SpecializationName = s.Name,
                        SpecializationCode = s.Name.Substring(0, Math.Min(4, s.Name.Length)).ToUpper(),
                        Description = s.Description,
                        IsActive = s.IsActive
                    })
                    .ToListAsync();

                _logger.Information("✅ {Count} تخصص فعال یافت شد", specializations.Count);

                return ServiceResult<List<SpecializationLookupViewModel>>.Successful(
                    specializations, "تخصص‌های فعال با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت تخصص‌های فعال");
                return ServiceResult<List<SpecializationLookupViewModel>>.Failed(
                    "خطا در بارگذاری تخصص‌های فعال");
            }
        }

        /// <summary>
        /// دریافت تخصص‌های دپارتمان
        /// </summary>
        public async Task<ServiceResult<List<SpecializationLookupViewModel>>> GetDepartmentSpecializationsAsync(int departmentId)
        {
            try
            {
                _logger.Information("🎓 دریافت تخصص‌های دپارتمان: {DepartmentId}", departmentId);

                // دریافت تخصص‌های پزشکان فعال در این دپارتمان
                var specializations = await _context.DoctorSpecializations
                    .AsNoTracking()
                    .Where(ds => ds.Doctor.DoctorDepartments.Any(dd => dd.DepartmentId == departmentId && !dd.IsDeleted))
                    .Where(ds => ds.Doctor.IsActive && !ds.Doctor.IsDeleted)
                    .Where(ds => ds.Specialization.IsActive && !ds.Specialization.IsDeleted)
                    .Include(ds => ds.Specialization)
                    .Select(ds => ds.Specialization)
                    .Distinct()
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .Select(s => new SpecializationLookupViewModel
                    {
                        SpecializationId = s.SpecializationId,
                        SpecializationName = s.Name,
                        SpecializationCode = s.Name.Substring(0, Math.Min(4, s.Name.Length)).ToUpper(),
                        Description = s.Description,
                        IsActive = s.IsActive
                    })
                    .ToListAsync();

                _logger.Information("✅ {Count} تخصص برای دپارتمان {DepartmentId} یافت شد", specializations.Count, departmentId);

                return ServiceResult<List<SpecializationLookupViewModel>>.Successful(
                    specializations, "تخصص‌های دپارتمان با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت تخصص‌های دپارتمان {DepartmentId}", departmentId);
                return ServiceResult<List<SpecializationLookupViewModel>>.Failed(
                    "خطا در بارگذاری تخصص‌های دپارتمان");
            }
        }

        #region Helper Methods

        /// <summary>
        /// دریافت نام نمایشی شیفت
        /// </summary>
        private string GetShiftDisplayName(string shiftType)
        {
            return shiftType switch
            {
                "Morning" => "شیفت صبح",
                "Evening" => "شیفت عصر",
                "Night" => "شیفت شب",
                _ => "شیفت نامشخص"
            };
        }

        #endregion
    }
}
