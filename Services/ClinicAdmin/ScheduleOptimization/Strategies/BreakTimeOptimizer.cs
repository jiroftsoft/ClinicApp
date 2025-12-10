using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
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
    /// پیاده‌سازی بهینه‌سازی زمان‌های استراحت پزشکان
    /// 
    /// مسئولیت (SRP):
    /// - محاسبه زمان‌های استراحت بهینه
    /// - توزیع استراحت در طول روز
    /// - در نظر گیری قوانین کار و سلامت
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط بهینه‌سازی استراحت
    /// - Dependency Inversion: وابستگی به interfaces
    /// - Open/Closed: قابل توسعه بدون تغییر کد موجود
    /// </summary>
    public class BreakTimeOptimizer : IBreakTimeOptimizer
    {
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;
        private readonly ILogger _logger;

        // ✅ ثوابت برای بهینه‌سازی استراحت (قابل تنظیم)
        private const int MINIMUM_BREAK_DURATION = 15; // حداقل 15 دقیقه استراحت
        private const int OPTIMAL_BREAK_DURATION = 30; // استراحت بهینه 30 دقیقه
        private const int LUNCH_BREAK_DURATION = 60; // استراحت ناهار 60 دقیقه
        private const int WORK_HOURS_BEFORE_BREAK = 4; // هر 4 ساعت یک استراحت

        public BreakTimeOptimizer(
            IDoctorScheduleRepository doctorScheduleRepository,
            ILogger logger)
        {
            _doctorScheduleRepository = doctorScheduleRepository ?? throw new ArgumentNullException(nameof(doctorScheduleRepository));
            _logger = logger?.ForContext<BreakTimeOptimizer>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// بهینه‌سازی زمان‌های استراحت برای یک روز
        /// </summary>
        public async Task<ServiceResult<List<BreakTimeSlot>>> OptimizeBreakTimesAsync(
            int doctorId,
            DateTime date,
            TimeSpan workStartTime,
            TimeSpan workEndTime,
            int totalWorkMinutes)
        {
            try
            {
                _logger.Information("شروع بهینه‌سازی زمان‌های استراحت - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));

                // ✅ اعتبارسنجی
                var validation = ScheduleOptimizationValidator.ValidateWorkTime(workStartTime, workEndTime);
                if (!validation.IsValid)
                {
                    return ServiceResult<List<BreakTimeSlot>>.Failed(validation.ErrorMessage);
                }

                if (totalWorkMinutes <= 0)
                {
                    return ServiceResult<List<BreakTimeSlot>>.Failed("کل زمان کار باید بزرگتر از صفر باشد.");
                }

                var breakSlots = new List<BreakTimeSlot>();

                // ✅ محاسبه حداقل زمان استراحت
                var minimumBreakTime = CalculateMinimumBreakTime(totalWorkMinutes);

                // ✅ اگر زمان کار کمتر از 4 ساعت است، استراحت اختیاری است
                if (totalWorkMinutes < 240) // کمتر از 4 ساعت
                {
                    _logger.Information("زمان کار کمتر از 4 ساعت است - استراحت اختیاری - DoctorId: {DoctorId}",
                        doctorId);
                    return ServiceResult<List<BreakTimeSlot>>.Successful(breakSlots);
                }

                // ✅ محاسبه تعداد استراحت‌های مورد نیاز
                var workHours = totalWorkMinutes / 60.0;
                var requiredBreaks = (int)Math.Ceiling(workHours / WORK_HOURS_BEFORE_BREAK);

                // ✅ توزیع استراحت‌ها در طول روز
                var breakDuration = minimumBreakTime / requiredBreaks;
                if (breakDuration < MINIMUM_BREAK_DURATION)
                {
                    breakDuration = MINIMUM_BREAK_DURATION;
                }

                // ✅ استراحت ناهار (اگر زمان کار بیش از 6 ساعت باشد)
                if (totalWorkMinutes >= 360) // 6 ساعت
                {
                    var lunchBreakStart = workStartTime.Add(TimeSpan.FromHours(4)); // بعد از 4 ساعت کار
                    var lunchBreakEnd = lunchBreakStart.Add(TimeSpan.FromMinutes(LUNCH_BREAK_DURATION));

                    // ✅ اطمینان از اینکه استراحت ناهار در بازه کاری است
                    if (lunchBreakEnd <= workEndTime)
                    {
                        breakSlots.Add(new BreakTimeSlot
                        {
                            Date = date,
                            StartTime = lunchBreakStart,
                            EndTime = lunchBreakEnd,
                            Duration = LUNCH_BREAK_DURATION,
                            Type = BreakType.Lunch,
                            Priority = 1, // بالاترین اولویت
                            IsMandatory = true,
                            IsOptimized = true
                        });

                        _logger.Information("استراحت ناهار اضافه شد - DoctorId: {DoctorId}, Time: {StartTime}-{EndTime}",
                            doctorId, lunchBreakStart.ToString(@"hh\:mm"), lunchBreakEnd.ToString(@"hh\:mm"));
                    }
                }

                // ✅ استراحت‌های کوتاه (هر 4 ساعت)
                var currentTime = workStartTime;
                var breakCount = 0;

                while (currentTime < workEndTime && breakCount < requiredBreaks)
                {
                    // ✅ اگر استراحت ناهار اضافه شده، از آن عبور می‌کنیم
                    var lunchBreak = breakSlots.FirstOrDefault(bt => bt.Type == BreakType.Lunch);
                    if (lunchBreak != null && 
                        currentTime >= lunchBreak.StartTime && 
                        currentTime < lunchBreak.EndTime)
                    {
                        currentTime = lunchBreak.EndTime;
                        continue;
                    }

                    // ✅ محاسبه زمان استراحت بعدی (بعد از 4 ساعت کار)
                    var nextBreakStart = currentTime.Add(TimeSpan.FromHours(WORK_HOURS_BEFORE_BREAK));
                    var nextBreakEnd = nextBreakStart.Add(TimeSpan.FromMinutes(breakDuration));

                    // ✅ بررسی اینکه استراحت در بازه کاری است
                    if (nextBreakEnd <= workEndTime)
                    {
                        // ✅ بررسی تداخل با استراحت ناهار
                        var conflictsWithLunch = lunchBreak != null &&
                            nextBreakStart < lunchBreak.EndTime &&
                            nextBreakEnd > lunchBreak.StartTime;

                        if (!conflictsWithLunch)
                        {
                            breakSlots.Add(new BreakTimeSlot
                            {
                                Date = date,
                                StartTime = nextBreakStart,
                                EndTime = nextBreakEnd,
                                Duration = breakDuration,
                                Type = BreakType.Short,
                                Priority = 2,
                                IsMandatory = true,
                                IsOptimized = true
                            });

                            breakCount++;
                            currentTime = nextBreakEnd;

                            _logger.Information("استراحت کوتاه اضافه شد - DoctorId: {DoctorId}, Time: {StartTime}-{EndTime}",
                                doctorId, nextBreakStart.ToString(@"hh\:mm"), nextBreakEnd.ToString(@"hh\:mm"));
                        }
                        else
                        {
                            currentTime = lunchBreak.EndTime;
                        }
                    }
                    else
                    {
                        break; // دیگر نمی‌توانیم استراحت اضافه کنیم
                    }
                }

                // ✅ مرتب‌سازی بر اساس زمان شروع
                breakSlots = breakSlots.OrderBy(bt => bt.StartTime).ToList();

                _logger.Information("بهینه‌سازی زمان‌های استراحت تکمیل شد - DoctorId: {DoctorId}, Count: {Count}",
                    doctorId, breakSlots.Count);

                return ServiceResult<List<BreakTimeSlot>>.Successful(breakSlots);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی زمان‌های استراحت - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));
                return ServiceResult<List<BreakTimeSlot>>.Failed("خطا در بهینه‌سازی زمان‌های استراحت");
            }
        }

        /// <summary>
        /// محاسبه حداقل زمان استراحت مورد نیاز
        /// </summary>
        public int CalculateMinimumBreakTime(int totalWorkMinutes)
        {
            return WorkloadCalculator.CalculateMinimumBreakTime(totalWorkMinutes);
        }

        /// <summary>
        /// بررسی اینکه آیا زمان استراحت کافی است
        /// </summary>
        public bool IsBreakTimeSufficient(int totalWorkMinutes, int breakTimeMinutes)
        {
            var minimumRequired = CalculateMinimumBreakTime(totalWorkMinutes);
            return breakTimeMinutes >= minimumRequired;
        }

        /// <summary>
        /// پیشنهاد زمان‌های استراحت بر اساس الگوهای بهینه
        /// </summary>
        public List<BreakTimeSuggestion> SuggestBreakTimes(TimeSpan workStartTime, TimeSpan workEndTime, int totalWorkMinutes)
        {
            var suggestions = new List<BreakTimeSuggestion>();

            try
            {
                // ✅ اگر زمان کار کمتر از 4 ساعت است، استراحت اختیاری است
                if (totalWorkMinutes < 240)
                {
                    suggestions.Add(new BreakTimeSuggestion
                    {
                        StartTime = workStartTime.Add(TimeSpan.FromHours(2)),
                        EndTime = workStartTime.Add(TimeSpan.FromHours(2)).Add(TimeSpan.FromMinutes(15)),
                        Duration = 15,
                        Type = BreakType.Short,
                        Priority = 3,
                        Reason = "استراحت اختیاری برای حفظ انرژی"
                    });
                    return suggestions;
                }

                // ✅ استراحت ناهار (اگر زمان کار بیش از 6 ساعت باشد)
                if (totalWorkMinutes >= 360)
                {
                    var lunchStart = workStartTime.Add(TimeSpan.FromHours(4));
                    var lunchEnd = lunchStart.Add(TimeSpan.FromMinutes(LUNCH_BREAK_DURATION));

                    if (lunchEnd <= workEndTime)
                    {
                        suggestions.Add(new BreakTimeSuggestion
                        {
                            StartTime = lunchStart,
                            EndTime = lunchEnd,
                            Duration = LUNCH_BREAK_DURATION,
                            Type = BreakType.Lunch,
                            Priority = 1,
                            Reason = "استراحت ناهار برای حفظ سلامت و انرژی"
                        });
                    }
                }

                // ✅ استراحت‌های کوتاه (هر 4 ساعت)
                var workHours = totalWorkMinutes / 60.0;
                var requiredBreaks = (int)Math.Ceiling(workHours / WORK_HOURS_BEFORE_BREAK);
                var breakDuration = OPTIMAL_BREAK_DURATION;

                var currentTime = workStartTime;
                for (int i = 0; i < requiredBreaks; i++)
                {
                    var breakStart = currentTime.Add(TimeSpan.FromHours(WORK_HOURS_BEFORE_BREAK));
                    var breakEnd = breakStart.Add(TimeSpan.FromMinutes(breakDuration));

                    if (breakEnd <= workEndTime)
                    {
                        // بررسی تداخل با استراحت ناهار
                        var lunchSuggestion = suggestions.FirstOrDefault(s => s.Type == BreakType.Lunch);
                        var conflictsWithLunch = lunchSuggestion != null &&
                            breakStart < lunchSuggestion.EndTime &&
                            breakEnd > lunchSuggestion.StartTime;

                        if (!conflictsWithLunch)
                        {
                            suggestions.Add(new BreakTimeSuggestion
                            {
                                StartTime = breakStart,
                                EndTime = breakEnd,
                                Duration = breakDuration,
                                Type = BreakType.Short,
                                Priority = 2,
                                Reason = $"استراحت کوتاه بعد از {WORK_HOURS_BEFORE_BREAK} ساعت کار"
                            });
                            currentTime = breakEnd;
                        }
                        else
                        {
                            currentTime = lunchSuggestion.EndTime;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در تولید پیشنهادات استراحت");
            }

            return suggestions.OrderBy(s => s.StartTime).ToList();
        }
    }
}

