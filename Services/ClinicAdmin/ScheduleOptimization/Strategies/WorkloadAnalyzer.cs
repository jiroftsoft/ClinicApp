using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;
using ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Helpers;
using ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Validators;
using ClinicApp.ViewModels.DoctorManagementVM;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Strategies
{
    /// <summary>
    /// پیاده‌سازی تحلیل بار کاری پزشکان
    /// 
    /// مسئولیت (SRP):
    /// - تحلیل بار کاری روزانه/هفتگی/ماهانه
    /// - محاسبه تعداد نوبت‌ها
    /// - تشخیص وضعیت بار کاری
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط تحلیل بار کاری
    /// - Dependency Inversion: وابستگی به interfaces
    /// - Open/Closed: قابل توسعه بدون تغییر کد موجود
    /// </summary>
    public class WorkloadAnalyzer : IWorkloadAnalyzer
    {
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger _logger;

        // ✅ ثوابت برای محاسبه بار کاری (قابل تنظیم)
        private const int DEFAULT_APPOINTMENT_DURATION = 30; // دقیقه
        private const int LIGHT_THRESHOLD_PERCENTAGE = 50;
        private const int BALANCED_THRESHOLD_PERCENTAGE = 75;
        private const int HEAVY_THRESHOLD_PERCENTAGE = 90;

        public WorkloadAnalyzer(
            IDoctorScheduleRepository doctorScheduleRepository,
            IAppointmentRepository appointmentRepository,
            ILogger logger)
        {
            _doctorScheduleRepository = doctorScheduleRepository ?? throw new ArgumentNullException(nameof(doctorScheduleRepository));
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _logger = logger?.ForContext<WorkloadAnalyzer>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// تحلیل بار کاری روزانه برای یک پزشک
        /// </summary>
        public async Task<ServiceResult<WorkloadAnalysisResult>> AnalyzeDailyWorkloadAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("شروع تحلیل بار کاری روزانه - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));

                // ✅ اعتبارسنجی ورودی‌ها
                var validation = ScheduleOptimizationValidator.ValidateDoctorId(doctorId);
                if (!validation.IsValid)
                {
                    return ServiceResult<WorkloadAnalysisResult>.Failed(validation.ErrorMessage);
                }

                validation = ScheduleOptimizationValidator.ValidateDate(date, allowPastDates: false);
                if (!validation.IsValid)
                {
                    return ServiceResult<WorkloadAnalysisResult>.Failed(validation.ErrorMessage);
                }

                // ✅ دریافت برنامه کاری پزشک
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                if (schedule == null)
                {
                    _logger.Warning("برنامه کاری برای پزشک {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<WorkloadAnalysisResult>.Failed("برنامه کاری برای این پزشک تعریف نشده است.");
                }

                // ✅ دریافت روز کاری مربوط به تاریخ
                var dayOfWeek = (int)date.DayOfWeek;
                var workDay = schedule.WorkDays?
                    .FirstOrDefault(w => w.DayOfWeek == dayOfWeek && w.IsActive);

                if (workDay == null)
                {
                    _logger.Information("روز کاری برای پزشک {DoctorId} در تاریخ {Date} یافت نشد",
                        doctorId, date.ToString("yyyy/MM/dd"));
                    
                    return ServiceResult<WorkloadAnalysisResult>.Successful(new WorkloadAnalysisResult
                    {
                        AnalysisDate = date,
                        CurrentAppointments = 0,
                        AvailableAppointments = 0,
                        MaxCapacity = 0,
                        UtilizationPercentage = 0,
                        Status = WorkloadBalanceStatus.NoWorkDay,
                        TotalWorkMinutes = 0,
                        BreakTimeMinutes = 0,
                        FreeTimeMinutes = 0
                    });
                }

                // ✅ دریافت بازه زمانی فعال
                var timeRange = workDay.TimeRanges?.FirstOrDefault(tr => tr.IsActive);
                if (timeRange == null)
                {
                    _logger.Information("بازه زمانی برای روز کاری پزشک {DoctorId} در تاریخ {Date} یافت نشد",
                        doctorId, date.ToString("yyyy/MM/dd"));
                    
                    return ServiceResult<WorkloadAnalysisResult>.Successful(new WorkloadAnalysisResult
                    {
                        AnalysisDate = date,
                        CurrentAppointments = 0,
                        AvailableAppointments = 0,
                        MaxCapacity = 0,
                        UtilizationPercentage = 0,
                        Status = WorkloadBalanceStatus.NoWorkDay,
                        TotalWorkMinutes = 0,
                        BreakTimeMinutes = 0,
                        FreeTimeMinutes = 0
                    });
                }

                // ✅ محاسبه کل زمان کار
                var totalWorkMinutes = WorkloadCalculator.CalculateTotalWorkMinutes(
                    timeRange.StartTime, 
                    timeRange.EndTime);

                // ✅ دریافت نوبت‌های فعلی
                var appointments = await _appointmentRepository.GetDoctorAppointmentsByDateAsync(doctorId, date);
                var currentAppointments = appointments.Count;

                // ✅ محاسبه حداکثر ظرفیت (تعداد نوبت‌های قابل رزرو)
                var appointmentDuration = schedule.AppointmentDuration > 0 ? schedule.AppointmentDuration : DEFAULT_APPOINTMENT_DURATION;
                var maxCapacity = WorkloadCalculator.CalculateAvailableAppointments(
                    timeRange.StartTime,
                    timeRange.EndTime,
                    appointmentDuration,
                    0); // بدون در نظر گیری استراحت برای محاسبه ظرفیت

                // ✅ محاسبه زمان استراحت
                var breakTimeMinutes = WorkloadCalculator.CalculateMinimumBreakTime(totalWorkMinutes);

                // ✅ محاسبه تعداد نوبت‌های قابل رزرو (با در نظر گیری استراحت)
                var availableAppointments = WorkloadCalculator.CalculateAvailableAppointments(
                    timeRange.StartTime,
                    timeRange.EndTime,
                    appointmentDuration,
                    breakTimeMinutes);

                // ✅ محاسبه درصد استفاده از ظرفیت
                var utilizationPercentage = WorkloadCalculator.CalculateUtilizationPercentage(
                    currentAppointments,
                    maxCapacity);

                // ✅ تشخیص وضعیت بار کاری
                var status = DetermineWorkloadStatus(currentAppointments, maxCapacity);

                // ✅ محاسبه زمان خالی
                var freeTimeMinutes = WorkloadCalculator.CalculateFreeTimeMinutes(
                    totalWorkMinutes,
                    currentAppointments,
                    appointmentDuration,
                    breakTimeMinutes);

                var result = new WorkloadAnalysisResult
                {
                    AnalysisDate = date,
                    CurrentAppointments = currentAppointments,
                    AvailableAppointments = availableAppointments,
                    MaxCapacity = maxCapacity,
                    UtilizationPercentage = utilizationPercentage,
                    Status = status,
                    TotalWorkMinutes = totalWorkMinutes,
                    BreakTimeMinutes = breakTimeMinutes,
                    FreeTimeMinutes = freeTimeMinutes
                };

                _logger.Information("تحلیل بار کاری روزانه تکمیل شد - DoctorId: {DoctorId}, Date: {Date}, " +
                    "Current: {Current}, Max: {Max}, Status: {Status}",
                    doctorId, date.ToString("yyyy/MM/dd"), currentAppointments, maxCapacity, status);

                return ServiceResult<WorkloadAnalysisResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تحلیل بار کاری روزانه - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));
                return ServiceResult<WorkloadAnalysisResult>.Failed("خطا در تحلیل بار کاری روزانه");
            }
        }

        /// <summary>
        /// تحلیل بار کاری هفتگی برای یک پزشک
        /// </summary>
        public async Task<ServiceResult<List<WorkloadAnalysisResult>>> AnalyzeWeeklyWorkloadAsync(int doctorId, DateTime weekStart)
        {
            try
            {
                _logger.Information("شروع تحلیل بار کاری هفتگی - DoctorId: {DoctorId}, WeekStart: {WeekStart}",
                    doctorId, weekStart.ToString("yyyy/MM/dd"));

                var results = new List<WorkloadAnalysisResult>();
                var currentDate = weekStart.Date;

                // ✅ تحلیل برای 7 روز هفته
                for (int i = 0; i < 7; i++)
                {
                    var dailyResult = await AnalyzeDailyWorkloadAsync(doctorId, currentDate);
                    if (dailyResult.Success && dailyResult.Data != null)
                    {
                        results.Add(dailyResult.Data);
                    }
                    currentDate = currentDate.AddDays(1);
                }

                _logger.Information("تحلیل بار کاری هفتگی تکمیل شد - DoctorId: {DoctorId}, Count: {Count}",
                    doctorId, results.Count);

                return ServiceResult<List<WorkloadAnalysisResult>>.Successful(results);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تحلیل بار کاری هفتگی - DoctorId: {DoctorId}",
                    doctorId);
                return ServiceResult<List<WorkloadAnalysisResult>>.Failed("خطا در تحلیل بار کاری هفتگی");
            }
        }

        /// <summary>
        /// تحلیل بار کاری ماهانه برای یک پزشک
        /// </summary>
        public async Task<ServiceResult<Dictionary<string, List<WorkloadAnalysisResult>>>> AnalyzeMonthlyWorkloadAsync(int doctorId, DateTime monthStart)
        {
            try
            {
                _logger.Information("شروع تحلیل بار کاری ماهانه - DoctorId: {DoctorId}, MonthStart: {MonthStart}",
                    doctorId, monthStart.ToString("yyyy/MM"));

                var results = new Dictionary<string, List<WorkloadAnalysisResult>>();
                var currentDate = monthStart.Date;
                var endDate = monthStart.AddMonths(1).AddDays(-1);

                // ✅ تحلیل به تفکیک هفته
                while (currentDate <= endDate)
                {
                    // پیدا کردن شروع هفته
                    var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)currentDate.DayOfWeek + 7) % 7;
                    var weekStart = currentDate.AddDays(-daysUntilSunday);

                    var weekKey = $"هفته {weekStart.ToString("MM/dd")} - {weekStart.AddDays(6).ToString("MM/dd")}";

                    if (!results.ContainsKey(weekKey))
                    {
                        var weeklyResult = await AnalyzeWeeklyWorkloadAsync(doctorId, weekStart);
                        if (weeklyResult.Success && weeklyResult.Data != null)
                        {
                            results[weekKey] = weeklyResult.Data;
                        }
                    }

                    currentDate = currentDate.AddDays(7);
                }

                _logger.Information("تحلیل بار کاری ماهانه تکمیل شد - DoctorId: {DoctorId}, Weeks: {Weeks}",
                    doctorId, results.Count);

                return ServiceResult<Dictionary<string, List<WorkloadAnalysisResult>>>.Successful(results);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تحلیل بار کاری ماهانه - DoctorId: {DoctorId}",
                    doctorId);
                return ServiceResult<Dictionary<string, List<WorkloadAnalysisResult>>>.Failed("خطا در تحلیل بار کاری ماهانه");
            }
        }

        /// <summary>
        /// محاسبه تعداد نوبت‌های قابل رزرو برای یک بازه زمانی
        /// </summary>
        public async Task<ServiceResult<int>> CalculateAvailableAppointmentsAsync(int doctorId, DateTime startTime, DateTime endTime, int appointmentDuration)
        {
            try
            {
                // ✅ اعتبارسنجی
                var validation = ScheduleOptimizationValidator.ValidateAppointmentDuration(appointmentDuration);
                if (!validation.IsValid)
                {
                    return ServiceResult<int>.Failed(validation.ErrorMessage);
                }

                if (startTime >= endTime)
                {
                    return ServiceResult<int>.Failed("زمان شروع باید قبل از زمان پایان باشد.");
                }

                // ✅ محاسبه تعداد نوبت‌ها
                var count = WorkloadCalculator.CalculateAvailableAppointments(
                    startTime.TimeOfDay,
                    endTime.TimeOfDay,
                    appointmentDuration);

                return ServiceResult<int>.Successful(count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه تعداد نوبت‌های قابل رزرو - DoctorId: {DoctorId}",
                    doctorId);
                return ServiceResult<int>.Failed("خطا در محاسبه تعداد نوبت‌های قابل رزرو");
            }
        }

        /// <summary>
        /// تشخیص وضعیت بار کاری بر اساس تعداد نوبت‌ها
        /// </summary>
        public WorkloadBalanceStatus DetermineWorkloadStatus(int appointmentCount, int maxCapacity)
        {
            return WorkloadCalculator.DetermineWorkloadStatus(appointmentCount, maxCapacity);
        }
    }
}

