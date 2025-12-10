using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;
using ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Helpers;
using ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Validators;
using ClinicApp.ViewModels.DoctorManagementVM;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// سرویس بهینه‌سازی برنامه کاری پزشکان
    /// این سرویس مسئول بهینه‌سازی زمان‌بندی و توزیع بار کاری است
    /// طبق DESIGN_PRINCIPLES_CONTRACT: پیاده‌سازی کامل برای محیط پزشکی
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: Orchestration و هماهنگی Strategy ها
    /// - Dependency Inversion: وابستگی به Interfaces
    /// - Open/Closed: قابل توسعه بدون تغییر کد موجود
    /// </summary>
    public class ScheduleOptimizationService : IScheduleOptimizationService
    {
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly IWorkloadAnalyzer _workloadAnalyzer;
        private readonly IBreakTimeOptimizer _breakTimeOptimizer;
        private readonly IPriorityManager _priorityManager;
        private readonly IPatientDistributor _patientDistributor;
        private readonly IEmergencySlotManager _emergencySlotManager;
        private readonly ICostAnalyzer _costAnalyzer;
        private readonly ILogger _logger;

        public ScheduleOptimizationService(
            IDoctorScheduleRepository doctorScheduleRepository,
            IDoctorCrudService doctorCrudService,
            IWorkloadAnalyzer workloadAnalyzer,
            IBreakTimeOptimizer breakTimeOptimizer,
            IPriorityManager priorityManager,
            IPatientDistributor patientDistributor,
            IEmergencySlotManager emergencySlotManager,
            ICostAnalyzer costAnalyzer,
            ILogger logger)
        {
            _doctorScheduleRepository = doctorScheduleRepository ?? throw new ArgumentNullException(nameof(doctorScheduleRepository));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _workloadAnalyzer = workloadAnalyzer ?? throw new ArgumentNullException(nameof(workloadAnalyzer));
            _breakTimeOptimizer = breakTimeOptimizer ?? throw new ArgumentNullException(nameof(breakTimeOptimizer));
            _priorityManager = priorityManager ?? throw new ArgumentNullException(nameof(priorityManager));
            _patientDistributor = patientDistributor ?? throw new ArgumentNullException(nameof(patientDistributor));
            _emergencySlotManager = emergencySlotManager ?? throw new ArgumentNullException(nameof(emergencySlotManager));
            _costAnalyzer = costAnalyzer ?? throw new ArgumentNullException(nameof(costAnalyzer));
            _logger = logger?.ForContext<ScheduleOptimizationService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// بهینه‌سازی برنامه کاری روزانه
        /// </summary>
        public async Task<ServiceResult<WorkloadBalanceResult>> OptimizeDailyScheduleAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("شروع بهینه‌سازی برنامه کاری روزانه - DoctorId: {DoctorId}, Date: {Date}", 
                    doctorId, date.ToString("yyyy/MM/dd"));

                // ✅ اعتبارسنجی
                var validation = OptimizationRequestValidator.ValidateDailyOptimizationRequest(doctorId, date);
                if (!validation.IsValid)
                {
                    return ServiceResult<WorkloadBalanceResult>.Failed(validation.ErrorMessage);
                }

                // ✅ بررسی وجود پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<WorkloadBalanceResult>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // ✅ دریافت برنامه کاری پزشک
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                if (schedule == null)
                {
                    _logger.Warning("برنامه کاری برای پزشک {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<WorkloadBalanceResult>.Failed("برنامه کاری برای این پزشک تعریف نشده است.");
                }

                // ✅ تحلیل بار کاری روزانه با استفاده از WorkloadAnalyzer
                var workloadResult = await _workloadAnalyzer.AnalyzeDailyWorkloadAsync(doctorId, date);
                if (!workloadResult.Success || workloadResult.Data == null)
                {
                    return ServiceResult<WorkloadBalanceResult>.Failed(workloadResult.Message ?? "خطا در تحلیل بار کاری");
                }

                var workloadAnalysis = workloadResult.Data;

                // ✅ بهینه‌سازی زمان‌های استراحت
                var workDay = schedule.WorkDays?.FirstOrDefault(w => w.DayOfWeek == (int)date.DayOfWeek && w.IsActive);
                var timeRange = workDay?.TimeRanges?.FirstOrDefault(tr => tr.IsActive);
                
                List<BreakTimeSlot> breakSlots = new List<BreakTimeSlot>();
                if (timeRange != null && workloadAnalysis.TotalWorkMinutes > 0)
                {
                    var breakResult = await _breakTimeOptimizer.OptimizeBreakTimesAsync(
                        doctorId,
                        date,
                        timeRange.StartTime,
                        timeRange.EndTime,
                        workloadAnalysis.TotalWorkMinutes);

                    if (breakResult.Success && breakResult.Data != null)
                    {
                        breakSlots = breakResult.Data;
                    }
                }

                // ✅ تولید اسلات‌های بهینه شده
                var optimizedSlots = new List<TimeSlotViewModel>();
                if (workDay != null && timeRange != null)
                {
                    optimizedSlots = TimeSlotGenerator.GenerateTimeSlotsWithBreaks(
                        date,
                        timeRange.StartTime,
                        timeRange.EndTime,
                        schedule.AppointmentDuration,
                        breakSlots,
                        doctorResult.Data.FullName);
                }

                // ✅ تولید توصیه‌ها
                var recommendations = RecommendationGenerator.GenerateRecommendations(
                    workloadAnalysis.Status,
                    workloadAnalysis.CurrentAppointments,
                    workloadAnalysis.MaxCapacity,
                    workloadAnalysis.BreakTimeMinutes);

                var result = new WorkloadBalanceResult
                {
                    Status = workloadAnalysis.Status,
                    Message = GetStatusMessage(workloadAnalysis.Status),
                    TotalAppointments = workloadAnalysis.CurrentAppointments,
                    TotalWorkMinutes = workloadAnalysis.TotalWorkMinutes,
                    BreakTimeMinutes = workloadAnalysis.BreakTimeMinutes,
                    OptimizedSlots = optimizedSlots,
                    Recommendations = recommendations
                };

                _logger.Information("بهینه‌سازی برنامه کاری روزانه تکمیل شد - DoctorId: {DoctorId}, Date: {Date}, Status: {Status}", 
                    doctorId, date.ToString("yyyy/MM/dd"), workloadAnalysis.Status);

                return ServiceResult<WorkloadBalanceResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی برنامه کاری روزانه - DoctorId: {DoctorId}, Date: {Date}", 
                    doctorId, date.ToString("yyyy/MM/dd"));
                return ServiceResult<WorkloadBalanceResult>.Failed("خطا در بهینه‌سازی برنامه کاری روزانه");
            }
        }

        /// <summary>
        /// بهینه‌سازی برنامه کاری هفتگی
        /// </summary>
        public async Task<ServiceResult<List<WorkloadBalanceResult>>> OptimizeWeeklyScheduleAsync(int doctorId, DateTime weekStart)
        {
            try
            {
                _logger.Information("درخواست بهینه‌سازی برنامه کاری هفتگی برای پزشک {DoctorId} از {WeekStart}", 
                    doctorId, weekStart.ToString("yyyy/MM/dd"));

                var results = new List<WorkloadBalanceResult>();
                var currentDate = weekStart.Date;

                for (int i = 0; i < 7; i++)
                {
                    var dailyResult = await OptimizeDailyScheduleAsync(doctorId, currentDate);
                    if (dailyResult.Success)
                    {
                        results.Add(dailyResult.Data);
                    }
                    currentDate = currentDate.AddDays(1);
                }

                _logger.Information("بهینه‌سازی برنامه کاری هفتگی برای پزشک {DoctorId} با موفقیت انجام شد. تعداد روزها: {Count}", 
                    doctorId, results.Count);

                return ServiceResult<List<WorkloadBalanceResult>>.Successful(results);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی برنامه کاری هفتگی برای پزشک {DoctorId}", doctorId);
                return ServiceResult<List<WorkloadBalanceResult>>.Failed("خطا در بهینه‌سازی برنامه کاری هفتگی");
            }
        }

        /// <summary>
        /// بهینه‌سازی برنامه کاری ماهانه
        /// </summary>
        public async Task<ServiceResult<Dictionary<string, List<WorkloadBalanceResult>>>> OptimizeMonthlyScheduleAsync(int doctorId, DateTime monthStart)
        {
            try
            {
                _logger.Information("درخواست بهینه‌سازی برنامه کاری ماهانه برای پزشک {DoctorId} از {MonthStart}", 
                    doctorId, monthStart.ToString("yyyy/MM"));

                var results = new Dictionary<string, List<WorkloadBalanceResult>>();
                var currentDate = monthStart.Date;
                var endDate = monthStart.AddMonths(1).AddDays(-1);

                while (currentDate <= endDate)
                {
                    var weekStart = currentDate.AddDays(-(int)currentDate.DayOfWeek);
                    var weekKey = $"هفته {weekStart.ToString("MM/dd")} - {weekStart.AddDays(6).ToString("MM/dd")}";

                    if (!results.ContainsKey(weekKey))
                    {
                        var weeklyResult = await OptimizeWeeklyScheduleAsync(doctorId, weekStart);
                        if (weeklyResult.Success)
                        {
                            results[weekKey] = weeklyResult.Data;
                        }
                    }

                    currentDate = currentDate.AddDays(7);
                }

                _logger.Information("بهینه‌سازی برنامه کاری ماهانه برای پزشک {DoctorId} با موفقیت انجام شد. تعداد هفته‌ها: {Count}", 
                    doctorId, results.Count);

                return ServiceResult<Dictionary<string, List<WorkloadBalanceResult>>>.Successful(results);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی برنامه کاری ماهانه برای پزشک {DoctorId}", doctorId);
                return ServiceResult<Dictionary<string, List<WorkloadBalanceResult>>>.Failed("خطا در بهینه‌سازی برنامه کاری ماهانه");
            }
        }

        /// <summary>
        /// متعادل‌سازی بار کاری
        /// </summary>
        public async Task<ServiceResult<bool>> BalanceWorkloadAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Information("درخواست متعادل‌سازی بار کاری برای پزشک {DoctorId} از {StartDate} تا {EndDate}", 
                    doctorId, startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"));

                // در حال حاضر این متد ساده است
                // در آینده با الگوریتم‌های پیشرفته پیاده‌سازی خواهد شد
                var result = await OptimizeWeeklyScheduleAsync(doctorId, startDate);

                return ServiceResult<bool>.Successful(result.Success);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در متعادل‌سازی بار کاری برای پزشک {DoctorId}", doctorId);
                return ServiceResult<bool>.Failed("خطا در متعادل‌سازی بار کاری");
            }
        }

        /// <summary>
        /// بهینه‌سازی زمان‌های استراحت
        /// </summary>
        public async Task<ServiceResult<List<BreakTimeSlot>>> OptimizeBreakTimesAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("شروع بهینه‌سازی زمان‌های استراحت - DoctorId: {DoctorId}, Date: {Date}", 
                    doctorId, date.ToString("yyyy/MM/dd"));

                // ✅ اعتبارسنجی
                var validation = OptimizationRequestValidator.ValidateDailyOptimizationRequest(doctorId, date);
                if (!validation.IsValid)
                {
                    return ServiceResult<List<BreakTimeSlot>>.Failed(validation.ErrorMessage);
                }

                // ✅ دریافت برنامه کاری
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                if (schedule == null)
                {
                    return ServiceResult<List<BreakTimeSlot>>.Failed("برنامه کاری برای این پزشک تعریف نشده است.");
                }

                var workDay = schedule.WorkDays?.FirstOrDefault(w => w.DayOfWeek == (int)date.DayOfWeek && w.IsActive);
                var timeRange = workDay?.TimeRanges?.FirstOrDefault(tr => tr.IsActive);

                if (timeRange == null)
                {
                    return ServiceResult<List<BreakTimeSlot>>.Successful(new List<BreakTimeSlot>());
                }

                // ✅ محاسبه کل زمان کار
                var totalWorkMinutes = WorkloadCalculator.CalculateTotalWorkMinutes(
                    timeRange.StartTime,
                    timeRange.EndTime);

                // ✅ استفاده از BreakTimeOptimizer
                var result = await _breakTimeOptimizer.OptimizeBreakTimesAsync(
                    doctorId,
                    date,
                    timeRange.StartTime,
                    timeRange.EndTime,
                    totalWorkMinutes);

                if (!result.Success)
                {
                    return ServiceResult<List<BreakTimeSlot>>.Failed(result.Message);
                }

                _logger.Information("بهینه‌سازی زمان‌های استراحت تکمیل شد - DoctorId: {DoctorId}, Count: {Count}",
                    doctorId, result.Data?.Count ?? 0);

                return ServiceResult<List<BreakTimeSlot>>.Successful(result.Data ?? new List<BreakTimeSlot>());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی زمان‌های استراحت - DoctorId: {DoctorId}", doctorId);
                return ServiceResult<List<BreakTimeSlot>>.Failed("خطا در بهینه‌سازی زمان‌های استراحت");
            }
        }

        /// <summary>
        /// بهینه‌سازی اولویت‌های نوبت‌ها
        /// </summary>
        public async Task<ServiceResult<bool>> OptimizeAppointmentPrioritiesAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("شروع بهینه‌سازی اولویت‌های نوبت‌ها - DoctorId: {DoctorId}, Date: {Date}", 
                    doctorId, date.ToString("yyyy/MM/dd"));

                // ✅ اعتبارسنجی
                var validation = OptimizationRequestValidator.ValidateDailyOptimizationRequest(doctorId, date);
                if (!validation.IsValid)
                {
                    return ServiceResult<bool>.Failed(validation.ErrorMessage);
                }

                // ✅ استفاده از PriorityManager
                var result = await _priorityManager.OptimizeAppointmentPrioritiesAsync(doctorId, date);

                _logger.Information("بهینه‌سازی اولویت‌های نوبت‌ها تکمیل شد - DoctorId: {DoctorId}, Success: {Success}",
                    doctorId, result.Success);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی اولویت‌های نوبت‌ها - DoctorId: {DoctorId}", doctorId);
                return ServiceResult<bool>.Failed("خطا در بهینه‌سازی اولویت‌های نوبت‌ها");
            }
        }

        /// <summary>
        /// بهینه‌سازی توزیع بیماران
        /// </summary>
        public async Task<ServiceResult<PatientDistributionResult>> OptimizePatientDistributionAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("شروع بهینه‌سازی توزیع بیماران - DoctorId: {DoctorId}, Date: {Date}", 
                    doctorId, date.ToString("yyyy/MM/dd"));

                // ✅ اعتبارسنجی
                var validation = OptimizationRequestValidator.ValidateDailyOptimizationRequest(doctorId, date);
                if (!validation.IsValid)
                {
                    return ServiceResult<PatientDistributionResult>.Failed(validation.ErrorMessage);
                }

                // ✅ استفاده از PatientDistributor
                var result = await _patientDistributor.OptimizePatientDistributionAsync(doctorId, date);

                _logger.Information("بهینه‌سازی توزیع بیماران تکمیل شد - DoctorId: {DoctorId}, Total: {Total}",
                    doctorId, result.Data?.TotalPatients ?? 0);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی توزیع بیماران - DoctorId: {DoctorId}", doctorId);
                return ServiceResult<PatientDistributionResult>.Failed("خطا در بهینه‌سازی توزیع بیماران");
            }
        }

        /// <summary>
        /// بهینه‌سازی زمان‌های اورژانس
        /// </summary>
        public async Task<ServiceResult<List<EmergencyTimeSlot>>> OptimizeEmergencyTimesAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("شروع بهینه‌سازی زمان‌های اورژانس - DoctorId: {DoctorId}, Date: {Date}", 
                    doctorId, date.ToString("yyyy/MM/dd"));

                // ✅ اعتبارسنجی
                var validation = OptimizationRequestValidator.ValidateDailyOptimizationRequest(doctorId, date);
                if (!validation.IsValid)
                {
                    return ServiceResult<List<EmergencyTimeSlot>>.Failed(validation.ErrorMessage);
                }

                // ✅ استفاده از EmergencySlotManager
                var result = await _emergencySlotManager.OptimizeEmergencyTimesAsync(doctorId, date);

                _logger.Information("بهینه‌سازی زمان‌های اورژانس تکمیل شد - DoctorId: {DoctorId}, Count: {Count}",
                    doctorId, result.Data?.Count ?? 0);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی زمان‌های اورژانس - DoctorId: {DoctorId}", doctorId);
                return ServiceResult<List<EmergencyTimeSlot>>.Failed("خطا در بهینه‌سازی زمان‌های اورژانس");
            }
        }

        /// <summary>
        /// بهینه‌سازی قالب‌های برنامه کاری
        /// </summary>
        public async Task<ServiceResult<bool>> OptimizeScheduleTemplatesAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست بهینه‌سازی قالب‌های برنامه کاری برای پزشک {DoctorId}", doctorId);

                // در حال حاضر این متد ساده است
                // در آینده با الگوریتم‌های پیشرفته پیاده‌سازی خواهد شد
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی قالب‌های برنامه کاری برای پزشک {DoctorId}", doctorId);
                return ServiceResult<bool>.Failed("خطا در بهینه‌سازی قالب‌های برنامه کاری");
            }
        }

        /// <summary>
        /// بهینه‌سازی تعادل کار و زندگی
        /// </summary>
        public async Task<ServiceResult<WorkLifeBalanceReport>> OptimizeWorkLifeBalanceAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Information("درخواست بهینه‌سازی تعادل کار و زندگی برای پزشک {DoctorId} از {StartDate} تا {EndDate}", 
                    doctorId, startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"));

                var report = new WorkLifeBalanceReport
                {
                    Status = WorkLifeBalanceStatus.Balanced,
                    TotalWorkHours = 0,
                    TotalBreakHours = 0,
                    Recommendations = new List<string>()
                };

                // در حال حاضر این متد ساده است
                // در آینده با الگوریتم‌های پیشرفته پیاده‌سازی خواهد شد
                return ServiceResult<WorkLifeBalanceReport>.Successful(report);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی تعادل کار و زندگی برای پزشک {DoctorId}", doctorId);
                return ServiceResult<WorkLifeBalanceReport>.Failed("خطا در بهینه‌سازی تعادل کار و زندگی");
            }
        }

        /// <summary>
        /// بهینه‌سازی هزینه‌ها
        /// </summary>
        public async Task<ServiceResult<CostOptimizationReport>> OptimizeCostsAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Information("شروع بهینه‌سازی هزینه‌ها - DoctorId: {DoctorId}, From: {StartDate}, To: {EndDate}", 
                    doctorId, startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"));

                // ✅ اعتبارسنجی
                var validation = OptimizationRequestValidator.ValidateCostOptimizationRequest(doctorId, startDate, endDate);
                if (!validation.IsValid)
                {
                    return ServiceResult<CostOptimizationReport>.Failed(validation.ErrorMessage);
                }

                // ✅ استفاده از CostAnalyzer
                var result = await _costAnalyzer.OptimizeCostsAsync(doctorId, startDate, endDate);

                _logger.Information("بهینه‌سازی هزینه‌ها تکمیل شد - DoctorId: {DoctorId}, Revenue: {Revenue}, Costs: {Costs}",
                    doctorId, result.Data?.TotalRevenue ?? 0, result.Data?.TotalCosts ?? 0);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی هزینه‌ها - DoctorId: {DoctorId}", doctorId);
                return ServiceResult<CostOptimizationReport>.Failed("خطا در بهینه‌سازی هزینه‌ها");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// دریافت پیام وضعیت
        /// </summary>
        private string GetStatusMessage(WorkloadBalanceStatus status)
        {
            switch (status)
            {
                case WorkloadBalanceStatus.Light:
                    return "بار کاری سبک - امکان افزایش تعداد نوبت‌ها";
                case WorkloadBalanceStatus.Balanced:
                    return "بار کاری متعادل - وضعیت مطلوب";
                case WorkloadBalanceStatus.Heavy:
                    return "بار کاری سنگین - نیاز به بهینه‌سازی";
                case WorkloadBalanceStatus.Overloaded:
                    return "بار کاری بیش از حد - نیاز به کاهش فوری";
                case WorkloadBalanceStatus.NoWorkDay:
                    return "روز کاری برای این تاریخ تعریف نشده است";
                default:
                    return "وضعیت نامشخص";
            }
        }

        #endregion
    }
}
