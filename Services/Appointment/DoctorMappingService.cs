using System.Linq;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Models.DTOs.Appointment;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Services.Appointment
{
    /// <summary>
    /// پیاده‌سازی سرویس Mapping برای تبدیل Entity به DTO
    /// طبق appointment_controller_review.md - فاز 1
    /// </summary>
    public class DoctorMappingService : IDoctorMappingService
    {
        private readonly ILogger _logger;

        public DoctorMappingService(ILogger logger)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// تبدیل DoctorSchedule Entity به DTO
        /// جابجایی از Controller (55 خط) به Service
        /// </summary>
        public DoctorScheduleDisplayDto MapToScheduleDisplayDto(DoctorSchedule schedule)
        {
            if (schedule == null)
            {
                _logger.Debug("DoctorSchedule null است، بازگرداندن null");
                return null;
            }

            try
            {
                var dayNames = new[] { "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه" };
                var dayNamesShort = new[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };

                var dto = new DoctorScheduleDisplayDto
                {
                    ScheduleId = schedule.ScheduleId,
                    DoctorId = schedule.DoctorId,
                    AppointmentDuration = schedule.AppointmentDuration,
                    ConsultationFee = schedule.ConsultationFee,
                    IsActive = schedule.IsActive
                };

                // تبدیل WorkDays
                if (schedule.WorkDays != null)
                {
                    foreach (var workDay in schedule.WorkDays
                        .Where(wd => wd.IsActive && !wd.IsDeleted)
                        .OrderBy(wd => wd.DayOfWeek))
                    {
                        var workDayDto = new WorkDayDisplayDto
                        {
                            WorkDayId = workDay.WorkDayId,
                            DayOfWeek = workDay.DayOfWeek,
                            DayName = dayNames[workDay.DayOfWeek],
                            DayNameShort = dayNamesShort[workDay.DayOfWeek],
                            IsActive = workDay.IsActive
                        };

                        // تبدیل TimeRanges
                        if (workDay.TimeRanges != null)
                        {
                            foreach (var timeRange in workDay.TimeRanges
                                .Where(tr => tr.IsActive && !tr.IsDeleted)
                                .OrderBy(tr => tr.StartTime))
                            {
                                workDayDto.TimeRanges.Add(new TimeRangeDisplayDto
                                {
                                    TimeRangeId = timeRange.TimeRangeId,
                                    StartTime = timeRange.StartTime.ToString(@"hh\:mm"),
                                    EndTime = timeRange.EndTime.ToString(@"hh\:mm"),
                                    DisplayTime = TimeFormatHelper.FormatTimeToPersian(timeRange.StartTime),
                                    DisplayRange = TimeFormatHelper.FormatTimeRangeToPersian(
                                        timeRange.StartTime, timeRange.EndTime),
                                    IsActive = timeRange.IsActive
                                });
                            }
                        }

                        dto.WorkDays.Add(workDayDto);
                    }
                }

                _logger.Debug("DoctorSchedule به DTO تبدیل شد - ScheduleId: {ScheduleId}, WorkDaysCount: {Count}",
                    schedule.ScheduleId, dto.WorkDays.Count);

                return dto;
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex, "خطا در تبدیل DoctorSchedule به DTO - ScheduleId: {ScheduleId}",
                    schedule.ScheduleId);
                return null;
            }
        }
    }
}

