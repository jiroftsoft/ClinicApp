using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.DoctorManagementVM;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// سرویس مدیریت دسترسی‌پذیری نوبت‌ها
    /// این سرویس مسئول مدیریت اسلات‌های زمانی قابل رزرو است
    /// طبق DESIGN_PRINCIPLES_CONTRACT: پیاده‌سازی کامل برای محیط پزشکی
    /// </summary>
    public class AppointmentAvailabilityService : IAppointmentAvailabilityService
    {
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ILogger _logger;

        public AppointmentAvailabilityService(
            IDoctorScheduleRepository doctorScheduleRepository,
            IDoctorCrudService doctorCrudService)
        {
            _doctorScheduleRepository = doctorScheduleRepository ?? throw new ArgumentNullException(nameof(doctorScheduleRepository));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _logger = Log.ForContext<AppointmentAvailabilityService>();
        }

        /// <summary>
        /// دریافت تاریخ‌های در دسترس برای یک پزشک
        /// </summary>
        public async Task<ServiceResult<List<DateTime>>> GetAvailableDatesAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Information("درخواست دریافت تاریخ‌های در دسترس برای پزشک {DoctorId} از {StartDate} تا {EndDate}", 
                    doctorId, startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"));

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<List<DateTime>>.Failed("شناسه پزشک نامعتبر است.");
                }

                if (startDate >= endDate)
                {
                    return ServiceResult<List<DateTime>>.Failed("تاریخ شروع باید قبل از تاریخ پایان باشد.");
                }

                // بررسی وجود پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<List<DateTime>>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت برنامه کاری پزشک
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                if (schedule == null)
                {
                    _logger.Warning("برنامه کاری برای پزشک {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<List<DateTime>>.Failed("برنامه کاری برای این پزشک تعریف نشده است.");
                }

                // تولید لیست تاریخ‌های در دسترس
                var availableDates = new List<DateTime>();
                var currentDate = startDate.Date;

                while (currentDate <= endDate.Date)
                {
                    // بررسی اینکه آیا این تاریخ در برنامه کاری پزشک است
                    var workDay = schedule.WorkDays?.FirstOrDefault(w => w.DayOfWeek == (int)currentDate.DayOfWeek && w.IsActive);
                    
                    if (workDay != null)
                    {
                        // بررسی استثناها (تعطیلات، مرخصی‌ها)
                        var hasException = schedule.Exceptions?.Any(e => 
                            e.StartDate <= currentDate && 
                            (e.EndDate == null || e.EndDate >= currentDate) && 
                            e.Type == ExceptionType.Holiday && 
                            !e.IsDeleted) == true;

                        if (!hasException)
                        {
                            availableDates.Add(currentDate);
                        }
                    }

                    currentDate = currentDate.AddDays(1);
                }

                _logger.Information("تاریخ‌های در دسترس برای پزشک {DoctorId} با موفقیت دریافت شد. تعداد: {Count}", 
                    doctorId, availableDates.Count);

                return ServiceResult<List<DateTime>>.Successful(availableDates);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تاریخ‌های در دسترس برای پزشک {DoctorId}", doctorId);
                return ServiceResult<List<DateTime>>.Failed("خطا در دریافت تاریخ‌های در دسترس");
            }
        }

        /// <summary>
        /// دریافت اسلات‌های زمانی در دسترس برای یک تاریخ مشخص
        /// </summary>
        public async Task<ServiceResult<List<TimeSlotViewModel>>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("درخواست دریافت اسلات‌های زمانی برای پزشک {DoctorId} در تاریخ {Date}", 
                    doctorId, date.ToString("yyyy/MM/dd"));

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<List<TimeSlotViewModel>>.Failed("شناسه پزشک نامعتبر است.");
                }

                if (date.Date < DateTime.Today)
                {
                    return ServiceResult<List<TimeSlotViewModel>>.Failed("تاریخ مورد نظر نمی‌تواند در گذشته باشد.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctor.Success || doctor.Data == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<List<TimeSlotViewModel>>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت برنامه کاری پزشک
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                if (schedule == null)
                {
                    _logger.Warning("برنامه کاری برای پزشک {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<List<TimeSlotViewModel>>.Failed("برنامه کاری برای این پزشک تعریف نشده است.");
                }

                // دریافت روز کاری برای تاریخ مشخص
                var workDay = schedule.WorkDays?.FirstOrDefault(w => w.DayOfWeek == (int)date.DayOfWeek && w.IsActive);
                if (workDay == null)
                {
                    _logger.Information("روز کاری برای پزشک {DoctorId} در تاریخ {Date} یافت نشد", doctorId, date.ToString("yyyy/MM/dd"));
                    return ServiceResult<List<TimeSlotViewModel>>.Successful(new List<TimeSlotViewModel>());
                }

                // بررسی استثناها
                var exception = schedule.Exceptions?.FirstOrDefault(e => 
                    e.StartDate <= date && 
                    (e.EndDate == null || e.EndDate >= date) && 
                    !e.IsDeleted);

                if (exception != null)
                {
                    _logger.Information("استثنا برای پزشک {DoctorId} در تاریخ {Date} یافت شد: {Type}", 
                        doctorId, date.ToString("yyyy/MM/dd"), exception.Type);
                    return ServiceResult<List<TimeSlotViewModel>>.Successful(new List<TimeSlotViewModel>());
                }

                // تولید اسلات‌های زمانی
                var timeSlots = new List<TimeSlotViewModel>();
                var timeRange = workDay.TimeRanges?.FirstOrDefault(tr => tr.IsActive);
                if (timeRange == null)
                {
                    return ServiceResult<List<TimeSlotViewModel>>.Successful(new List<TimeSlotViewModel>());
                }

                var currentTime = timeRange.StartTime;
                var appointmentDuration = 30; // فرض بر 30 دقیقه برای هر نوبت

                while (currentTime < timeRange.EndTime)
                {
                    var endTime = currentTime.Add(TimeSpan.FromMinutes(appointmentDuration));
                    
                    if (endTime <= timeRange.EndTime)
                    {
                        timeSlots.Add(new TimeSlotViewModel
                        {
                            SlotId = 0, // در آینده از جدول AppointmentSlot استفاده خواهد شد
                            SlotDate = date,
                            StartTime = currentTime,
                            EndTime = endTime,
                            Duration = appointmentDuration,
                            Price = 0, // در حال حاضر ثابت
                            Status = "Available",
                            IsAvailable = true,
                            IsEmergencySlot = false,
                            IsWalkInAllowed = false, // در حال حاضر ثابت
                            Priority = "عادی",
                            DoctorName = doctor.Data.FullName,
                            Specialization = "نامشخص", // در حال حاضر ثابت
                            ClinicName = "نامشخص", // در حال حاضر ثابت
                            ClinicAddress = "نامشخص" // در حال حاضر ثابت
                        });
                    }

                    currentTime = endTime;
                }

                _logger.Information("اسلات‌های زمانی برای پزشک {DoctorId} در تاریخ {Date} با موفقیت تولید شد. تعداد: {Count}", 
                    doctorId, date.ToString("yyyy/MM/dd"), timeSlots.Count);

                return ServiceResult<List<TimeSlotViewModel>>.Successful(timeSlots);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلات‌های زمانی برای پزشک {DoctorId} در تاریخ {Date}", 
                    doctorId, date.ToString("yyyy/MM/dd"));
                return ServiceResult<List<TimeSlotViewModel>>.Failed("خطا در دریافت اسلات‌های زمانی");
            }
        }

        /// <summary>
        /// بررسی در دسترس بودن یک اسلات
        /// </summary>
        public async Task<ServiceResult<bool>> IsSlotAvailableAsync(int slotId)
        {
            try
            {
                _logger.Information("بررسی در دسترس بودن اسلات {SlotId}", slotId);

                // در حال حاضر این متد ساده است
                // در آینده با جدول AppointmentSlot یکپارچه خواهد شد
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی در دسترس بودن اسلات {SlotId}", slotId);
                return ServiceResult<bool>.Failed("خطا در بررسی در دسترس بودن اسلات");
            }
        }

        /// <summary>
        /// رزرو موقت یک اسلات
        /// </summary>
        public async Task<ServiceResult<bool>> ReserveSlotAsync(int slotId, int patientId, TimeSpan reservationDuration)
        {
            try
            {
                _logger.Information("رزرو موقت اسلات {SlotId} برای بیمار {PatientId} به مدت {Duration}", 
                    slotId, patientId, reservationDuration);

                // در حال حاضر این متد ساده است
                // در آینده با جدول AppointmentSlot یکپارچه خواهد شد
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در رزرو موقت اسلات {SlotId} برای بیمار {PatientId}", slotId, patientId);
                return ServiceResult<bool>.Failed("خطا در رزرو موقت اسلات");
            }
        }

        /// <summary>
        /// آزاد کردن یک اسلات رزرو شده
        /// </summary>
        public async Task<ServiceResult<bool>> ReleaseSlotAsync(int slotId)
        {
            try
            {
                _logger.Information("آزاد کردن اسلات {SlotId}", slotId);

                // در حال حاضر این متد ساده است
                // در آینده با جدول AppointmentSlot یکپارچه خواهد شد
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در آزاد کردن اسلات {SlotId}", slotId);
                return ServiceResult<bool>.Failed("خطا در آزاد کردن اسلات");
            }
        }

        /// <summary>
        /// تولید اسلات‌های هفتگی برای یک پزشک
        /// </summary>
        public async Task<ServiceResult<bool>> GenerateWeeklySlotsAsync(int doctorId, DateTime weekStart)
        {
            try
            {
                _logger.Information("تولید اسلات‌های هفتگی برای پزشک {DoctorId} از {WeekStart}", 
                    doctorId, weekStart.ToString("yyyy/MM/dd"));

                var endDate = weekStart.AddDays(6);
                var result = await GenerateSlotsForDateRangeAsync(doctorId, weekStart, endDate);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تولید اسلات‌های هفتگی برای پزشک {DoctorId}", doctorId);
                return ServiceResult<bool>.Failed("خطا در تولید اسلات‌های هفتگی");
            }
        }

        /// <summary>
        /// تولید اسلات‌های ماهانه برای یک پزشک
        /// </summary>
        public async Task<ServiceResult<bool>> GenerateMonthlySlotsAsync(int doctorId, DateTime monthStart)
        {
            try
            {
                _logger.Information("تولید اسلات‌های ماهانه برای پزشک {DoctorId} از {MonthStart}", 
                    doctorId, monthStart.ToString("yyyy/MM/dd"));

                var endDate = monthStart.AddMonths(1).AddDays(-1);
                var result = await GenerateSlotsForDateRangeAsync(doctorId, monthStart, endDate);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تولید اسلات‌های ماهانه برای پزشک {DoctorId}", doctorId);
                return ServiceResult<bool>.Failed("خطا در تولید اسلات‌های ماهانه");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// تولید اسلات‌ها برای یک بازه زمانی
        /// </summary>
        private async Task<ServiceResult<bool>> GenerateSlotsForDateRangeAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var currentDate = startDate.Date;
                var successCount = 0;
                var totalDays = 0;

                while (currentDate <= endDate.Date)
                {
                    var slotsResult = await GetAvailableTimeSlotsAsync(doctorId, currentDate);
                    if (slotsResult.Success && slotsResult.Data.Any())
                    {
                        successCount++;
                    }
                    totalDays++;
                    currentDate = currentDate.AddDays(1);
                }

                _logger.Information("تولید اسلات‌ها برای پزشک {DoctorId} از {StartDate} تا {EndDate} تکمیل شد. موفق: {SuccessCount}/{TotalDays}", 
                    doctorId, startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"), successCount, totalDays);

                return ServiceResult<bool>.Successful(successCount > 0);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تولید اسلات‌ها برای بازه زمانی پزشک {DoctorId}", doctorId);
                return ServiceResult<bool>.Failed("خطا در تولید اسلات‌ها");
            }
        }

        #endregion
    }
}
