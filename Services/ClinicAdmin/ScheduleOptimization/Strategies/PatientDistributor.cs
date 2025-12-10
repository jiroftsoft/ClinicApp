using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization;
using ClinicApp.Models.Enums;
using ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Validators;
using ClinicApp.ViewModels.DoctorManagementVM;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Strategies
{
    /// <summary>
    /// پیاده‌سازی بهینه‌سازی توزیع بیماران
    /// 
    /// مسئولیت (SRP):
    /// - توزیع بیماران بر اساس نوع
    /// - بهینه‌سازی توزیع در طول روز
    /// - تحلیل الگوهای توزیع
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط توزیع بیماران
    /// - Dependency Inversion: وابستگی به interfaces
    /// - Open/Closed: قابل توسعه بدون تغییر کد موجود
    /// </summary>
    public class PatientDistributor : IPatientDistributor
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger _logger;

        public PatientDistributor(
            IAppointmentRepository appointmentRepository,
            ILogger logger)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _logger = logger?.ForContext<PatientDistributor>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// بهینه‌سازی توزیع بیماران برای یک روز
        /// </summary>
        public async Task<ServiceResult<PatientDistributionResult>> OptimizePatientDistributionAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("شروع بهینه‌سازی توزیع بیماران - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));

                // ✅ اعتبارسنجی
                var validation = ScheduleOptimizationValidator.ValidateDoctorId(doctorId);
                if (!validation.IsValid)
                {
                    return ServiceResult<PatientDistributionResult>.Failed(validation.ErrorMessage);
                }

                validation = ScheduleOptimizationValidator.ValidateDate(date, allowPastDates: false);
                if (!validation.IsValid)
                {
                    return ServiceResult<PatientDistributionResult>.Failed(validation.ErrorMessage);
                }

                // ✅ دریافت نوبت‌های روز
                var appointments = await _appointmentRepository.GetDoctorAppointmentsByDateAsync(doctorId, date);

                // ✅ تحلیل توزیع بر اساس نوع
                var distributionByType = await AnalyzeDistributionByTypeAsync(doctorId, date);
                if (!distributionByType.Success)
                {
                    return ServiceResult<PatientDistributionResult>.Failed(distributionByType.Message);
                }

                // ✅ تحلیل توزیع ساعتی
                var hourlyDistribution = await AnalyzeHourlyDistributionAsync(doctorId, date);
                if (!hourlyDistribution.Success)
                {
                    return ServiceResult<PatientDistributionResult>.Failed(hourlyDistribution.Message);
                }

                // ✅ محاسبه توزیع بهینه
                var optimalDistribution = CalculateOptimalDistribution(appointments?.Count ?? 0);

                // ✅ تولید پیشنهادات
                var recommendations = SuggestDistributionImprovements(
                    distributionByType.Data,
                    optimalDistribution);

                var result = new PatientDistributionResult
                {
                    TotalPatients = appointments?.Count ?? 0,
                    DistributionByType = distributionByType.Data,
                    Recommendations = recommendations
                };

                _logger.Information("بهینه‌سازی توزیع بیماران تکمیل شد - DoctorId: {DoctorId}, Date: {Date}, Total: {Total}",
                    doctorId, date.ToString("yyyy/MM/dd"), result.TotalPatients);

                return ServiceResult<PatientDistributionResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی توزیع بیماران - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));
                return ServiceResult<PatientDistributionResult>.Failed("خطا در بهینه‌سازی توزیع بیماران");
            }
        }

        /// <summary>
        /// تحلیل توزیع بیماران بر اساس نوع
        /// </summary>
        public async Task<ServiceResult<Dictionary<string, int>>> AnalyzeDistributionByTypeAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("شروع تحلیل توزیع بر اساس نوع - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));

                var appointments = await _appointmentRepository.GetDoctorAppointmentsByDateAsync(doctorId, date);

                var distribution = new Dictionary<string, int>
                {
                    { "بیماران جدید", 0 },
                    { "بیماران قدیمی", 0 },
                    { "نوبت‌های اورژانس", 0 },
                    { "نوبت‌های عادی", 0 },
                    { "نوبت‌های آنلاین", 0 },
                    { "نوبت‌های حضوری", 0 }
                };

                if (appointments != null && appointments.Any())
                {
                    foreach (var appointment in appointments)
                    {
                        // ✅ توزیع بر اساس نوع بیمار
                        if (appointment.IsNewPatient)
                        {
                            distribution["بیماران جدید"]++;
                        }
                        else
                        {
                            distribution["بیماران قدیمی"]++;
                        }

                        // ✅ توزیع بر اساس اولویت
                        if (appointment.IsEmergency || 
                            appointment.Priority == AppointmentPriority.Emergency ||
                            appointment.Priority == AppointmentPriority.Urgent ||
                            appointment.Priority == AppointmentPriority.Critical)
                        {
                            distribution["نوبت‌های اورژانس"]++;
                        }
                        else
                        {
                            distribution["نوبت‌های عادی"]++;
                        }

                        // ✅ توزیع بر اساس روش رزرو
                        if (appointment.IsOnlineBooking)
                        {
                            distribution["نوبت‌های آنلاین"]++;
                        }
                        else
                        {
                            distribution["نوبت‌های حضوری"]++;
                        }
                    }
                }

                _logger.Information("تحلیل توزیع بر اساس نوع تکمیل شد - DoctorId: {DoctorId}, Types: {Count}",
                    doctorId, distribution.Count);

                return ServiceResult<Dictionary<string, int>>.Successful(distribution);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تحلیل توزیع بر اساس نوع - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));
                return ServiceResult<Dictionary<string, int>>.Failed("خطا در تحلیل توزیع بر اساس نوع");
            }
        }

        /// <summary>
        /// تحلیل توزیع بیماران در طول روز
        /// </summary>
        public async Task<ServiceResult<Dictionary<int, int>>> AnalyzeHourlyDistributionAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("شروع تحلیل توزیع ساعتی - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));

                var appointments = await _appointmentRepository.GetDoctorAppointmentsByDateAsync(doctorId, date);

                var hourlyDistribution = new Dictionary<int, int>();

                // ✅ Initialize برای تمام ساعات روز (0-23)
                for (int hour = 0; hour < 24; hour++)
                {
                    hourlyDistribution[hour] = 0;
                }

                if (appointments != null && appointments.Any())
                {
                    foreach (var appointment in appointments)
                    {
                        var hour = appointment.AppointmentDate.Hour;
                        if (hourlyDistribution.ContainsKey(hour))
                        {
                            hourlyDistribution[hour]++;
                        }
                    }
                }

                _logger.Information("تحلیل توزیع ساعتی تکمیل شد - DoctorId: {DoctorId}, Hours: {Count}",
                    doctorId, hourlyDistribution.Count);

                return ServiceResult<Dictionary<int, int>>.Successful(hourlyDistribution);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تحلیل توزیع ساعتی - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));
                return ServiceResult<Dictionary<int, int>>.Failed("خطا در تحلیل توزیع ساعتی");
            }
        }

        /// <summary>
        /// پیشنهاد بهینه‌سازی توزیع
        /// </summary>
        public List<string> SuggestDistributionImprovements(
            Dictionary<string, int> currentDistribution,
            Dictionary<string, int> optimalDistribution)
        {
            var suggestions = new List<string>();

            try
            {
                if (currentDistribution == null || !currentDistribution.Any())
                {
                    suggestions.Add("هیچ نوبتی برای تحلیل وجود ندارد");
                    return suggestions;
                }

                // ✅ تحلیل نسبت بیماران جدید به قدیمی
                var newPatients = currentDistribution.ContainsKey("بیماران جدید") ? currentDistribution["بیماران جدید"] : 0;
                var oldPatients = currentDistribution.ContainsKey("بیماران قدیمی") ? currentDistribution["بیماران قدیمی"] : 0;
                var totalPatients = newPatients + oldPatients;

                if (totalPatients > 0)
                {
                    var newPatientRatio = (decimal)newPatients / totalPatients * 100;

                    if (newPatientRatio > 70)
                    {
                        suggestions.Add("نسبت بیماران جدید بالا است - در نظر گیری زمان بیشتر برای ویزیت اول");
                    }
                    else if (newPatientRatio < 20)
                    {
                        suggestions.Add("نسبت بیماران جدید پایین است - بررسی علل کاهش بیماران جدید");
                    }
                }

                // ✅ تحلیل نسبت نوبت‌های اورژانس
                var emergencyAppointments = currentDistribution.ContainsKey("نوبت‌های اورژانس") 
                    ? currentDistribution["نوبت‌های اورژانس"] 
                    : 0;

                if (totalPatients > 0)
                {
                    var emergencyRatio = (decimal)emergencyAppointments / totalPatients * 100;

                    if (emergencyRatio > 30)
                    {
                        suggestions.Add("نسبت نوبت‌های اورژانس بالا است - در نظر گیری اسلات‌های رزرو برای اورژانس");
                    }
                }

                // ✅ تحلیل نسبت نوبت‌های آنلاین
                var onlineAppointments = currentDistribution.ContainsKey("نوبت‌های آنلاین") 
                    ? currentDistribution["نوبت‌های آنلاین"] 
                    : 0;

                if (totalPatients > 0)
                {
                    var onlineRatio = (decimal)onlineAppointments / totalPatients * 100;

                    if (onlineRatio > 50)
                    {
                        suggestions.Add("نسبت نوبت‌های آنلاین بالا است - بررسی زیرساخت و پشتیبانی");
                    }
                    else if (onlineRatio < 10)
                    {
                        suggestions.Add("نسبت نوبت‌های آنلاین پایین است - بررسی علل و بهبود سیستم رزرو آنلاین");
                    }
                }

                // ✅ پیشنهادات عمومی
                if (totalPatients == 0)
                {
                    suggestions.Add("هیچ نوبتی برای این تاریخ ثبت نشده است");
                }
                else if (totalPatients < 5)
                {
                    suggestions.Add("تعداد نوبت‌ها کم است - بررسی علل و بهبود بازاریابی");
                }
                else if (totalPatients > 20)
                {
                    suggestions.Add("تعداد نوبت‌ها زیاد است - بررسی کیفیت خدمات و زمان کافی برای هر نوبت");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در تولید پیشنهادات توزیع");
                suggestions.Add("خطا در تحلیل توزیع");
            }

            return suggestions;
        }

        /// <summary>
        /// محاسبه توزیع بهینه
        /// </summary>
        private Dictionary<string, int> CalculateOptimalDistribution(int totalAppointments)
        {
            var optimal = new Dictionary<string, int>();

            if (totalAppointments == 0)
            {
                return optimal;
            }

            // ✅ توزیع بهینه: 30% بیماران جدید، 70% بیماران قدیمی
            optimal["بیماران جدید"] = (int)(totalAppointments * 0.3m);
            optimal["بیماران قدیمی"] = (int)(totalAppointments * 0.7m);

            // ✅ توزیع بهینه: 10% اورژانس، 90% عادی
            optimal["نوبت‌های اورژانس"] = (int)(totalAppointments * 0.1m);
            optimal["نوبت‌های عادی"] = (int)(totalAppointments * 0.9m);

            // ✅ توزیع بهینه: 40% آنلاین، 60% حضوری
            optimal["نوبت‌های آنلاین"] = (int)(totalAppointments * 0.4m);
            optimal["نوبت‌های حضوری"] = (int)(totalAppointments * 0.6m);

            return optimal;
        }
    }
}

