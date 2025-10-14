using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.DoctorManagementVM;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس مدیریت شیفت کاری - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت شیفت‌های کاری پزشکان
    /// 2. انتخاب خودکار شیفت بر اساس زمان فعلی
    /// 3. فیلتر کردن پزشکان بر اساس شیفت
    /// 4. مدیریت زمان‌بندی شیفت‌ها
    /// 5. سازگاری با سیستم‌های پزشکی
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط مدیریت شیفت کاری
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    public class ShiftHelperService
    {
        #region Fields and Constructor

        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ShiftHelperService(
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #endregion

        #region Shift Management

        /// <summary>
        /// دریافت شیفت فعلی بر اساس زمان سیستم
        /// </summary>
        /// <returns>نوع شیفت فعلی</returns>
        public ShiftType GetCurrentShift()
        {
            try
            {
                var hour = DateTime.Now.Hour;
                
                if (hour >= 6 && hour < 14)
                {
                    _logger.Debug("شیفت فعلی: صبح (ساعت: {Hour})", hour);
                    return ShiftType.Morning;
                }
                else if (hour >= 14 && hour < 22)
                {
                    _logger.Debug("شیفت فعلی: عصر (ساعت: {Hour})", hour);
                    return ShiftType.Evening;
                }
                else
                {
                    _logger.Debug("شیفت فعلی: شب (ساعت: {Hour})", hour);
                    return ShiftType.Night;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تعیین شیفت فعلی");
                return ShiftType.Morning; // Default fallback
            }
        }

        /// <summary>
        /// دریافت نام نمایشی شیفت
        /// </summary>
        /// <param name="shiftType">نوع شیفت</param>
        /// <returns>نام نمایشی شیفت</returns>
        public string GetShiftDisplayName(ShiftType shiftType)
        {
            return shiftType switch
            {
                ShiftType.Morning => "صبح",
                ShiftType.Evening => "عصر",
                ShiftType.Night => "شب",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// بررسی اینکه آیا شیفت فعال است
        /// </summary>
        /// <param name="shiftType">نوع شیفت</param>
        /// <returns>آیا شیفت فعال است</returns>
        public bool IsShiftActive(ShiftType shiftType)
        {
            var currentShift = GetCurrentShift();
            return shiftType == currentShift;
        }

        /// <summary>
        /// دریافت زمان شروع شیفت
        /// </summary>
        /// <param name="shiftType">نوع شیفت</param>
        /// <returns>زمان شروع شیفت</returns>
        public TimeSpan GetShiftStartTime(ShiftType shiftType)
        {
            return shiftType switch
            {
                ShiftType.Morning => new TimeSpan(6, 0, 0),
                ShiftType.Evening => new TimeSpan(14, 0, 0),
                ShiftType.Night => new TimeSpan(22, 0, 0),
                _ => new TimeSpan(6, 0, 0)
            };
        }

        /// <summary>
        /// دریافت زمان پایان شیفت
        /// </summary>
        /// <param name="shiftType">نوع شیفت</param>
        /// <returns>زمان پایان شیفت</returns>
        public TimeSpan GetShiftEndTime(ShiftType shiftType)
        {
            return shiftType switch
            {
                ShiftType.Morning => new TimeSpan(14, 0, 0),
                ShiftType.Evening => new TimeSpan(22, 0, 0),
                ShiftType.Night => new TimeSpan(6, 0, 0),
                _ => new TimeSpan(14, 0, 0)
            };
        }

        /// <summary>
        /// دریافت تمام شیفت‌های موجود
        /// </summary>
        /// <returns>لیست تمام شیفت‌ها</returns>
        public List<ShiftType> GetAllShifts()
        {
            return Enum.GetValues(typeof(ShiftType)).Cast<ShiftType>().ToList();
        }

        /// <summary>
        /// دریافت اطلاعات کامل شیفت
        /// </summary>
        /// <param name="shiftType">نوع شیفت</param>
        /// <returns>اطلاعات کامل شیفت</returns>
        public ShiftInfo GetShiftInfo(ShiftType shiftType)
        {
            return new ShiftInfo
            {
                ShiftType = shiftType,
                DisplayName = GetShiftDisplayName(shiftType),
                StartTime = GetShiftStartTime(shiftType),
                EndTime = GetShiftEndTime(shiftType),
                IsActive = IsShiftActive(shiftType)
            };
        }

        #endregion

        #region Doctor Shift Management

        /// <summary>
        /// فیلتر کردن پزشکان بر اساس شیفت
        /// </summary>
        /// <param name="doctors">لیست پزشکان</param>
        /// <param name="shiftType">نوع شیفت</param>
        /// <returns>لیست پزشکان شیفت</returns>
        public List<DoctorLookupViewModel> FilterDoctorsByShift(List<DoctorLookupViewModel> doctors, ShiftType shiftType)
        {
            try
            {
                _logger.Debug("فیلتر کردن پزشکان بر اساس شیفت: {ShiftType}", shiftType);

                // در اینجا می‌توانید منطق فیلتر کردن پزشکان بر اساس شیفت را پیاده‌سازی کنید
                // برای مثال، می‌توانید از DoctorSchedule استفاده کنید
                
                return doctors.Where(d => d.IsActive).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فیلتر کردن پزشکان بر اساس شیفت: {ShiftType}", shiftType);
                return new List<DoctorLookupViewModel>();
            }
        }

        /// <summary>
        /// دریافت پزشکان شیفت فعلی
        /// </summary>
        /// <param name="allDoctors">تمام پزشکان</param>
        /// <returns>پزشکان شیفت فعلی</returns>
        public List<DoctorLookupViewModel> GetCurrentShiftDoctors(List<DoctorLookupViewModel> allDoctors)
        {
            var currentShift = GetCurrentShift();
            return FilterDoctorsByShift(allDoctors, currentShift);
        }

        #endregion
    }
}
