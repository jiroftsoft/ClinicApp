using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization;
using ClinicApp.Models.Enums;
using ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Helpers;
using ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Validators;
using ClinicApp.ViewModels.DoctorManagementVM;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Strategies
{
    /// <summary>
    /// پیاده‌سازی مدیریت زمان‌های اورژانس
    /// 
    /// مسئولیت (SRP):
    /// - مدیریت اسلات‌های اورژانس
    /// - بهینه‌سازی زمان‌های اورژانس
    /// - رزرو و آزادسازی اسلات‌های اورژانس
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط مدیریت اورژانس
    /// - Dependency Inversion: وابستگی به interfaces
    /// - Open/Closed: قابل توسعه بدون تغییر کد موجود
    /// </summary>
    public class EmergencySlotManager : IEmergencySlotManager
    {
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger _logger;

        // ✅ ثوابت برای مدیریت اورژانس (قابل تنظیم)
        private const int EMERGENCY_SLOT_PERCENTAGE = 10; // 10% از کل اسلات‌ها برای اورژانس
        private const int MIN_EMERGENCY_SLOTS = 1; // حداقل 1 اسلات اورژانس
        private const int EMERGENCY_SLOT_DURATION = 30; // مدت زمان هر اسلات اورژانس (دقیقه)

        public EmergencySlotManager(
            IDoctorScheduleRepository doctorScheduleRepository,
            IAppointmentRepository appointmentRepository,
            ILogger logger)
        {
            _doctorScheduleRepository = doctorScheduleRepository ?? throw new ArgumentNullException(nameof(doctorScheduleRepository));
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _logger = logger?.ForContext<EmergencySlotManager>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// بهینه‌سازی زمان‌های اورژانس برای یک روز
        /// </summary>
        public async Task<ServiceResult<List<EmergencyTimeSlot>>> OptimizeEmergencyTimesAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("شروع بهینه‌سازی زمان‌های اورژانس - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));

                // ✅ اعتبارسنجی
                var validation = ScheduleOptimizationValidator.ValidateDoctorId(doctorId);
                if (!validation.IsValid)
                {
                    return ServiceResult<List<EmergencyTimeSlot>>.Failed(validation.ErrorMessage);
                }

                validation = ScheduleOptimizationValidator.ValidateDate(date, allowPastDates: false);
                if (!validation.IsValid)
                {
                    return ServiceResult<List<EmergencyTimeSlot>>.Failed(validation.ErrorMessage);
                }

                // ✅ دریافت برنامه کاری پزشک
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                if (schedule == null)
                {
                    _logger.Warning("برنامه کاری برای پزشک {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<List<EmergencyTimeSlot>>.Failed("برنامه کاری برای این پزشک تعریف نشده است.");
                }

                // ✅ دریافت روز کاری
                var dayOfWeek = (int)date.DayOfWeek;
                var workDay = schedule.WorkDays?
                    .FirstOrDefault(w => w.DayOfWeek == dayOfWeek && w.IsActive);

                if (workDay == null)
                {
                    _logger.Information("روز کاری برای پزشک {DoctorId} در تاریخ {Date} یافت نشد",
                        doctorId, date.ToString("yyyy/MM/dd"));
                    return ServiceResult<List<EmergencyTimeSlot>>.Successful(new List<EmergencyTimeSlot>());
                }

                // ✅ دریافت بازه زمانی
                var timeRange = workDay.TimeRanges?.FirstOrDefault(tr => tr.IsActive);
                if (timeRange == null)
                {
                    _logger.Information("بازه زمانی برای روز کاری پزشک {DoctorId} در تاریخ {Date} یافت نشد",
                        doctorId, date.ToString("yyyy/MM/dd"));
                    return ServiceResult<List<EmergencyTimeSlot>>.Successful(new List<EmergencyTimeSlot>());
                }

                // ✅ محاسبه تعداد کل اسلات‌ها
                var appointmentDuration = schedule.AppointmentDuration > 0 ? schedule.AppointmentDuration : EMERGENCY_SLOT_DURATION;
                var totalSlots = WorkloadCalculator.CalculateAvailableAppointments(
                    timeRange.StartTime,
                    timeRange.EndTime,
                    appointmentDuration);

                // ✅ محاسبه تعداد اسلات‌های اورژانس مورد نیاز
                var requiredEmergencySlots = CalculateRequiredEmergencySlots(doctorId, date, totalSlots);

                // ✅ تولید اسلات‌های اورژانس
                var emergencySlots = GenerateEmergencySlots(
                    date,
                    timeRange.StartTime,
                    timeRange.EndTime,
                    requiredEmergencySlots,
                    appointmentDuration);

                _logger.Information("بهینه‌سازی زمان‌های اورژانس تکمیل شد - DoctorId: {DoctorId}, Date: {Date}, Slots: {Count}",
                    doctorId, date.ToString("yyyy/MM/dd"), emergencySlots.Count);

                return ServiceResult<List<EmergencyTimeSlot>>.Successful(emergencySlots);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی زمان‌های اورژانس - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));
                return ServiceResult<List<EmergencyTimeSlot>>.Failed("خطا در بهینه‌سازی زمان‌های اورژانس");
            }
        }

        /// <summary>
        /// محاسبه تعداد اسلات‌های اورژانس مورد نیاز
        /// </summary>
        public int CalculateRequiredEmergencySlots(int doctorId, DateTime date, int totalSlots)
        {
            if (totalSlots <= 0)
            {
                return 0;
            }

            // ✅ محاسبه بر اساس درصد
            var percentageBased = (int)Math.Ceiling(totalSlots * (EMERGENCY_SLOT_PERCENTAGE / 100.0));

            // ✅ حداقل یک اسلات اورژانس
            var required = Math.Max(MIN_EMERGENCY_SLOTS, percentageBased);

            // ✅ حداکثر 20% از کل اسلات‌ها
            var maxAllowed = (int)Math.Ceiling(totalSlots * 0.2);
            required = Math.Min(required, maxAllowed);

            _logger.Information("محاسبه اسلات‌های اورژانس - DoctorId: {DoctorId}, Total: {Total}, Required: {Required}",
                doctorId, totalSlots, required);

            return required;
        }

        /// <summary>
        /// بررسی در دسترس بودن اسلات اورژانس
        /// </summary>
        public async Task<ServiceResult<bool>> IsEmergencySlotAvailableAsync(int doctorId, DateTime date, TimeSpan time)
        {
            try
            {
                // ✅ بررسی اینکه آیا در این زمان نوبت دیگری وجود دارد
                var appointmentDateTime = date.Date.Add(time);
                var isAvailable = await _appointmentRepository.CheckSlotAvailabilityAsync(
                    doctorId,
                    appointmentDateTime,
                    time,
                    time.Add(TimeSpan.FromMinutes(EMERGENCY_SLOT_DURATION)));

                return ServiceResult<bool>.Successful(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی‌پذیری اسلات اورژانس - DoctorId: {DoctorId}, Date: {Date}, Time: {Time}",
                    doctorId, date.ToString("yyyy/MM/dd"), time.ToString(@"hh\:mm"));
                return ServiceResult<bool>.Failed("خطا در بررسی دسترسی‌پذیری اسلات اورژانس");
            }
        }

        /// <summary>
        /// رزرو اسلات اورژانس
        /// </summary>
        public async Task<ServiceResult<EmergencyTimeSlot>> ReserveEmergencySlotAsync(
            int doctorId,
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime,
            EmergencyPriority priority)
        {
            try
            {
                _logger.Information("رزرو اسلات اورژانس - DoctorId: {DoctorId}, Date: {Date}, Time: {StartTime}-{EndTime}",
                    doctorId, date.ToString("yyyy/MM/dd"), startTime.ToString(@"hh\:mm"), endTime.ToString(@"hh\:mm"));

                // ✅ بررسی دسترسی‌پذیری
                var availabilityResult = await IsEmergencySlotAvailableAsync(doctorId, date, startTime);
                if (!availabilityResult.Success || !availabilityResult.Data)
                {
                    return ServiceResult<EmergencyTimeSlot>.Failed("اسلات اورژانس در این زمان در دسترس نیست");
                }

                // ✅ ایجاد اسلات اورژانس
                var emergencySlot = new EmergencyTimeSlot
                {
                    Date = date,
                    StartTime = startTime,
                    EndTime = endTime,
                    Duration = (int)(endTime - startTime).TotalMinutes,
                    Priority = priority,
                    Type = EmergencyType.Critical,
                    IsAvailable = true
                };

                _logger.Information("اسلات اورژانس رزرو شد - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));

                return ServiceResult<EmergencyTimeSlot>.Successful(emergencySlot);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در رزرو اسلات اورژانس - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));
                return ServiceResult<EmergencyTimeSlot>.Failed("خطا در رزرو اسلات اورژانس");
            }
        }

        /// <summary>
        /// تولید اسلات‌های اورژانس
        /// </summary>
        private List<EmergencyTimeSlot> GenerateEmergencySlots(
            DateTime date,
            TimeSpan workStartTime,
            TimeSpan workEndTime,
            int requiredSlots,
            int slotDuration)
        {
            var slots = new List<EmergencyTimeSlot>();

            if (requiredSlots <= 0 || workStartTime >= workEndTime)
            {
                return slots;
            }

            try
            {
                // ✅ توزیع اسلات‌های اورژانس در طول روز
                var totalWorkMinutes = (workEndTime - workStartTime).TotalMinutes;
                var interval = totalWorkMinutes / (requiredSlots + 1);

                for (int i = 1; i <= requiredSlots; i++)
                {
                    var slotStart = workStartTime.Add(TimeSpan.FromMinutes(interval * i));
                    var slotEnd = slotStart.Add(TimeSpan.FromMinutes(slotDuration));

                    // ✅ اطمینان از اینکه اسلات در بازه کاری است
                    if (slotEnd <= workEndTime)
                    {
                        // ✅ تعیین اولویت بر اساس زمان (صبح اولویت بالاتر)
                        var priority = slotStart.Hours < 12 
                            ? EmergencyPriority.High 
                            : EmergencyPriority.Medium;

                        slots.Add(new EmergencyTimeSlot
                        {
                            Date = date,
                            StartTime = slotStart,
                            EndTime = slotEnd,
                            Duration = slotDuration,
                            Priority = priority,
                            Type = EmergencyType.Critical,
                            IsAvailable = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در تولید اسلات‌های اورژانس");
            }

            return slots.OrderBy(s => s.StartTime).ToList();
        }
    }
}

