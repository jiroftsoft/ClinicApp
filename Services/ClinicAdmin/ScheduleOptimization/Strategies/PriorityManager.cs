using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization;
using AppointmentEntity = ClinicApp.Models.Entities.Appointment.Appointment;
using ClinicApp.Models.Enums;
using ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Validators;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Strategies
{
    /// <summary>
    /// پیاده‌سازی مدیریت اولویت‌های نوبت‌ها
    /// 
    /// مسئولیت (SRP):
    /// - اولویت‌بندی نوبت‌ها
    /// - مدیریت نوبت‌های اورژانس
    /// - بهینه‌سازی ترتیب نوبت‌ها
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط مدیریت اولویت‌ها
    /// - Dependency Inversion: وابستگی به interfaces
    /// - Open/Closed: قابل توسعه بدون تغییر کد موجود
    /// </summary>
    public class PriorityManager : IPriorityManager
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger _logger;

        // ✅ وزن‌های اولویت (قابل تنظیم)
        private const int EMERGENCY_PRIORITY_WEIGHT = 1000;
        private const int HIGH_PRIORITY_WEIGHT = 500;
        private const int NORMAL_PRIORITY_WEIGHT = 100;
        private const int LOW_PRIORITY_WEIGHT = 50;
        private const int NEW_PATIENT_BONUS = 50;
        private const int FOLLOW_UP_BONUS = 30;

        public PriorityManager(
            IAppointmentRepository appointmentRepository,
            ILogger logger)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _logger = logger?.ForContext<PriorityManager>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// بهینه‌سازی اولویت‌های نوبت‌ها برای یک روز
        /// </summary>
        public async Task<ServiceResult<bool>> OptimizeAppointmentPrioritiesAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("شروع بهینه‌سازی اولویت‌های نوبت‌ها - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));

                // ✅ اعتبارسنجی
                var validation = ScheduleOptimizationValidator.ValidateDoctorId(doctorId);
                if (!validation.IsValid)
                {
                    return ServiceResult<bool>.Failed(validation.ErrorMessage);
                }

                validation = ScheduleOptimizationValidator.ValidateDate(date, allowPastDates: false);
                if (!validation.IsValid)
                {
                    return ServiceResult<bool>.Failed(validation.ErrorMessage);
                }

                // ✅ دریافت نوبت‌های روز
                var appointments = await _appointmentRepository.GetDoctorAppointmentsByDateAsync(doctorId, date);
                
                if (appointments == null || !appointments.Any())
                {
                    _logger.Information("نوبتی برای بهینه‌سازی یافت نشد - DoctorId: {DoctorId}, Date: {Date}",
                        doctorId, date.ToString("yyyy/MM/dd"));
                    return ServiceResult<bool>.Successful(true);
                }

                // ✅ محاسبه اولویت برای هر نوبت
                var prioritizedAppointments = appointments
                    .Select(a => new
                    {
                        Appointment = a,
                        PriorityScore = CalculatePriority(a)
                    })
                    .OrderByDescending(x => x.PriorityScore)
                    .ThenBy(x => x.Appointment.AppointmentDate)
                    .ToList();

                // ✅ بررسی نیاز به جابجایی
                var reorderingSuggestions = SuggestReordering(appointments);
                
                if (reorderingSuggestions.Any())
                {
                    _logger.Information("پیشنهادات جابجایی تولید شد - DoctorId: {DoctorId}, Count: {Count}",
                        doctorId, reorderingSuggestions.Count);
                    
                    // در حال حاضر فقط پیشنهاد می‌دهیم
                    // در آینده می‌توانیم جابجایی خودکار را پیاده‌سازی کنیم
                }

                _logger.Information("بهینه‌سازی اولویت‌های نوبت‌ها تکمیل شد - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی اولویت‌های نوبت‌ها - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));
                return ServiceResult<bool>.Failed("خطا در بهینه‌سازی اولویت‌های نوبت‌ها");
            }
        }

        /// <summary>
        /// محاسبه اولویت یک نوبت
        /// </summary>
        public int CalculatePriority(AppointmentEntity appointment)
        {
            if (appointment == null)
            {
                return 0;
            }

            int priorityScore = 0;

            // ✅ اولویت بر اساس نوع نوبت
            switch (appointment.Priority)
            {
                case AppointmentPriority.Emergency:
                case AppointmentPriority.Urgent:
                case AppointmentPriority.Critical:
                    priorityScore += EMERGENCY_PRIORITY_WEIGHT;
                    break;
                case AppointmentPriority.High:
                    priorityScore += HIGH_PRIORITY_WEIGHT;
                    break;
                case AppointmentPriority.Normal:
                case AppointmentPriority.Medium:
                    priorityScore += NORMAL_PRIORITY_WEIGHT;
                    break;
                case AppointmentPriority.Low:
                    priorityScore += LOW_PRIORITY_WEIGHT;
                    break;
            }

            // ✅ اولویت بر اساس وضعیت اورژانس
            if (appointment.IsEmergency)
            {
                priorityScore += EMERGENCY_PRIORITY_WEIGHT;
            }

            // ✅ اولویت بر اساس وضعیت بیمار (جدید/قدیمی)
            if (appointment.IsNewPatient)
            {
                priorityScore += NEW_PATIENT_BONUS;
            }
            else
            {
                priorityScore += FOLLOW_UP_BONUS; // بیماران قدیمی معمولاً follow-up هستند
            }

            // ✅ اولویت بر اساس وضعیت نوبت
            switch (appointment.Status)
            {
                case AppointmentStatus.Scheduled:
                    // نوبت‌های scheduled اولویت بالاتری دارند
                    priorityScore += 20;
                    break;
                case AppointmentStatus.Pending:
                    // نوبت‌های pending اولویت بالاتری دارند
                    priorityScore += 30;
                    break;
                case AppointmentStatus.Completed:
                    // نوبت‌های تکمیل شده اولویت پایین‌تری دارند
                    priorityScore += 10;
                    break;
            }

            // ✅ اولویت بر اساس زمان (نوبت‌های زودتر اولویت بالاتری دارند)
            var timeFactor = (int)(appointment.AppointmentDate.TimeOfDay.TotalMinutes / 10);
            priorityScore += (1440 - timeFactor); // هرچه زودتر، امتیاز بالاتر

            return priorityScore;
        }

        /// <summary>
        /// مرتب‌سازی نوبت‌ها بر اساس اولویت
        /// </summary>
        public List<AppointmentEntity> SortByPriority(List<AppointmentEntity> appointments)
        {
            if (appointments == null || !appointments.Any())
            {
                return new List<AppointmentEntity>();
            }

            return appointments
                .OrderByDescending(a => CalculatePriority(a))
                .ThenBy(a => a.AppointmentDate)
                .ToList();
        }

        /// <summary>
        /// بررسی امکان جابجایی نوبت‌ها برای بهینه‌سازی
        /// </summary>
        public List<PriorityReorderingSuggestion> SuggestReordering(List<AppointmentEntity> appointments)
        {
            var suggestions = new List<PriorityReorderingSuggestion>();

            if (appointments == null || appointments.Count < 2)
            {
                return suggestions;
            }

            try
            {
                // ✅ مرتب‌سازی فعلی
                var currentOrder = appointments
                    .OrderBy(a => a.AppointmentDate)
                    .ToList();

                // ✅ مرتب‌سازی بهینه بر اساس اولویت
                var optimalOrder = SortByPriority(appointments);

                // ✅ مقایسه و پیدا کردن جابجایی‌های پیشنهادی
                for (int i = 0; i < currentOrder.Count; i++)
                {
                    var currentAppointment = currentOrder[i];
                    var optimalIndex = optimalOrder.IndexOf(currentAppointment);

                    if (optimalIndex != i && optimalIndex >= 0)
                    {
                        var targetAppointment = optimalOrder[i];
                        
                        // ✅ محاسبه امتیاز بهبود
                        var currentScore = CalculatePriority(currentAppointment);
                        var targetScore = CalculatePriority(targetAppointment);
                        var improvementScore = targetScore - currentScore;

                        if (improvementScore > 0)
                        {
                            suggestions.Add(new PriorityReorderingSuggestion
                            {
                                AppointmentId1 = currentAppointment.AppointmentId,
                                AppointmentId2 = targetAppointment.AppointmentId,
                                Reason = $"جابجایی برای بهبود اولویت‌بندی - بهبود امتیاز: {improvementScore}",
                                ImprovementScore = improvementScore
                            });
                        }
                    }
                }

                // ✅ مرتب‌سازی بر اساس امتیاز بهبود
                suggestions = suggestions
                    .OrderByDescending(s => s.ImprovementScore)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در تولید پیشنهادات جابجایی");
            }

            return suggestions;
        }
    }
}

