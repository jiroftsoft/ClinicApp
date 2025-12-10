using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization;
using ClinicApp.Models.Enums;
using ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Helpers;
using ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Validators;
using ClinicApp.ViewModels.DoctorManagementVM;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Strategies
{
    /// <summary>
    /// پیاده‌سازی تحلیل و بهینه‌سازی هزینه‌ها
    /// 
    /// مسئولیت (SRP):
    /// - محاسبه درآمد و هزینه‌ها
    /// - تحلیل سودآوری
    /// - پیشنهاد بهینه‌سازی هزینه
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط تحلیل هزینه
    /// - Dependency Inversion: وابستگی به interfaces
    /// - Open/Closed: قابل توسعه بدون تغییر کد موجود
    /// </summary>
    public class CostAnalyzer : ICostAnalyzer
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger _logger;

        // ✅ ثوابت برای تحلیل هزینه (قابل تنظیم)
        private const decimal ESTIMATED_COST_PER_APPOINTMENT = 100000m; // هزینه تخمینی هر نوبت (ریال)
        private const decimal ESTIMATED_OVERHEAD_COST = 500000m; // هزینه سربار روزانه (ریال)
        private const decimal TARGET_PROFIT_MARGIN = 30m; // حاشیه سود هدف (درصد)

        public CostAnalyzer(
            IAppointmentRepository appointmentRepository,
            ILogger logger)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _logger = logger?.ForContext<CostAnalyzer>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// بهینه‌سازی هزینه‌ها برای یک بازه زمانی
        /// </summary>
        public async Task<ServiceResult<CostOptimizationReport>> OptimizeCostsAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Information("شروع بهینه‌سازی هزینه‌ها - DoctorId: {DoctorId}, From: {StartDate}, To: {EndDate}",
                    doctorId, startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"));

                // ✅ اعتبارسنجی
                var validation = ScheduleOptimizationValidator.ValidateDateRange(startDate, endDate);
                if (!validation.IsValid)
                {
                    return ServiceResult<CostOptimizationReport>.Failed(validation.ErrorMessage);
                }

                // ✅ محاسبه درآمد
                var revenueResult = await CalculateTotalRevenueAsync(doctorId, startDate, endDate);
                if (!revenueResult.Success)
                {
                    return ServiceResult<CostOptimizationReport>.Failed(revenueResult.Message);
                }

                // ✅ محاسبه هزینه‌ها
                var costsResult = await CalculateTotalCostsAsync(doctorId, startDate, endDate);
                if (!costsResult.Success)
                {
                    return ServiceResult<CostOptimizationReport>.Failed(costsResult.Message);
                }

                var revenue = revenueResult.Data;
                var costs = costsResult.Data;

                // ✅ محاسبه سود خالص
                var netProfit = CalculateNetProfit(revenue, costs);

                // ✅ محاسبه درصد حاشیه سود
                var profitMargin = revenue > 0 ? ((revenue - costs) / revenue) * 100 : 0;

                // ✅ تولید پیشنهادات
                var suggestions = GenerateCostOptimizationSuggestions(costs, revenue);

                var report = new CostOptimizationReport
                {
                    DoctorId = doctorId,
                    ReportDate = DateTime.Now,
                    TotalRevenue = revenue,
                    TotalCosts = costs,
                    NetProfit = netProfit,
                    CurrentCosts = costs,
                    OptimizedCosts = CalculateOptimizedCosts(costs, suggestions),
                    Savings = costs - CalculateOptimizedCosts(costs, suggestions),
                    SavingsPercentage = costs > 0 ? ((costs - CalculateOptimizedCosts(costs, suggestions)) / costs) * 100 : 0,
                    Suggestions = suggestions
                };

                _logger.Information("بهینه‌سازی هزینه‌ها تکمیل شد - DoctorId: {DoctorId}, Revenue: {Revenue}, Costs: {Costs}, Profit: {Profit}",
                    doctorId, revenue, costs, netProfit);

                return ServiceResult<CostOptimizationReport>.Successful(report);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بهینه‌سازی هزینه‌ها - DoctorId: {DoctorId}",
                    doctorId);
                return ServiceResult<CostOptimizationReport>.Failed("خطا در بهینه‌سازی هزینه‌ها");
            }
        }

        /// <summary>
        /// محاسبه درآمد کل
        /// </summary>
        public async Task<ServiceResult<decimal>> CalculateTotalRevenueAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Information("شروع محاسبه درآمد کل - DoctorId: {DoctorId}, From: {StartDate}, To: {EndDate}",
                    doctorId, startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"));

                // ✅ دریافت نوبت‌های تکمیل شده در بازه زمانی
                // توجه: در حال حاضر از AppointmentRepository استفاده می‌کنیم
                // در آینده می‌توانیم Repository مخصوص مالی ایجاد کنیم

                var totalRevenue = 0m;
                var currentDate = startDate.Date;

                while (currentDate <= endDate.Date)
                {
                    var appointments = await _appointmentRepository.GetDoctorAppointmentsByDateAsync(doctorId, currentDate);
                    
                    if (appointments != null && appointments.Any())
                    {
                        // ✅ محاسبه درآمد از نوبت‌های تکمیل شده
                        var completedRevenue = appointments
                            .Where(a => a.Status == AppointmentStatus.Completed)
                            .Sum(a => a.Price);

                        totalRevenue += completedRevenue;
                    }

                    currentDate = currentDate.AddDays(1);
                }

                _logger.Information("محاسبه درآمد کل تکمیل شد - DoctorId: {DoctorId}, Revenue: {Revenue}",
                    doctorId, totalRevenue);

                return ServiceResult<decimal>.Successful(totalRevenue);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه درآمد کل - DoctorId: {DoctorId}",
                    doctorId);
                return ServiceResult<decimal>.Failed("خطا در محاسبه درآمد کل");
            }
        }

        /// <summary>
        /// محاسبه هزینه‌های کل
        /// </summary>
        public async Task<ServiceResult<decimal>> CalculateTotalCostsAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Information("شروع محاسبه هزینه‌های کل - DoctorId: {DoctorId}, From: {StartDate}, To: {EndDate}",
                    doctorId, startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"));

                // ✅ در حال حاضر از هزینه‌های تخمینی استفاده می‌کنیم
                // در آینده می‌توانیم از سیستم هزینه‌یابی واقعی استفاده کنیم

                var totalCosts = 0m;
                var currentDate = startDate.Date;
                var daysCount = 0;

                while (currentDate <= endDate.Date)
                {
                    var appointments = await _appointmentRepository.GetDoctorAppointmentsByDateAsync(doctorId, currentDate);
                    var appointmentCount = appointments?.Count(a => a.Status != AppointmentStatus.Cancelled) ?? 0;

                    // ✅ هزینه‌های متغیر (بر اساس تعداد نوبت‌ها)
                    var variableCosts = appointmentCount * ESTIMATED_COST_PER_APPOINTMENT;

                    // ✅ هزینه‌های ثابت (سربار روزانه)
                    var fixedCosts = ESTIMATED_OVERHEAD_COST;

                    totalCosts += variableCosts + fixedCosts;
                    daysCount++;
                    currentDate = currentDate.AddDays(1);
                }

                _logger.Information("محاسبه هزینه‌های کل تکمیل شد - DoctorId: {DoctorId}, Costs: {Costs}, Days: {Days}",
                    doctorId, totalCosts, daysCount);

                return ServiceResult<decimal>.Successful(totalCosts);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه هزینه‌های کل - DoctorId: {DoctorId}",
                    doctorId);
                return ServiceResult<decimal>.Failed("خطا در محاسبه هزینه‌های کل");
            }
        }

        /// <summary>
        /// محاسبه سود خالص
        /// </summary>
        public decimal CalculateNetProfit(decimal revenue, decimal costs)
        {
            return revenue - costs;
        }

        /// <summary>
        /// تولید پیشنهادات بهینه‌سازی هزینه
        /// </summary>
        public List<CostOptimizationSuggestion> GenerateCostOptimizationSuggestions(decimal currentCosts, decimal revenue)
        {
            var suggestions = new List<CostOptimizationSuggestion>();

            try
            {
                if (currentCosts <= 0 || revenue <= 0)
                {
                    return suggestions;
                }

                var profitMargin = ((revenue - currentCosts) / revenue) * 100;

                // ✅ پیشنهاد 1: کاهش هزینه‌های عملیاتی
                if (currentCosts > revenue * 0.7m) // اگر هزینه‌ها بیش از 70% درآمد باشد
                {
                    suggestions.Add(new CostOptimizationSuggestion
                    {
                        SuggestionId = 1,
                        Title = "کاهش هزینه‌های عملیاتی",
                        Description = "هزینه‌های عملیاتی بیش از 70% درآمد است. بررسی راه‌های کاهش هزینه‌ها",
                        CostSavings = currentCosts * 0.1m, // 10% صرفه‌جویی
                        ImplementationPriority = 1,
                        Difficulty = "متوسط",
                        EstimatedImplementationDays = 30
                    });
                }

                // ✅ پیشنهاد 2: افزایش کارایی
                if (profitMargin < TARGET_PROFIT_MARGIN)
                {
                    suggestions.Add(new CostOptimizationSuggestion
                    {
                        SuggestionId = 2,
                        Title = "افزایش کارایی عملیاتی",
                        Description = $"حاشیه سود ({profitMargin:F1}%) کمتر از هدف ({TARGET_PROFIT_MARGIN}%) است",
                        CostSavings = currentCosts * 0.05m, // 5% صرفه‌جویی
                        ImplementationPriority = 2,
                        Difficulty = "آسان",
                        EstimatedImplementationDays = 14
                    });
                }

                // ✅ پیشنهاد 3: بهینه‌سازی برنامه کاری
                if (revenue < currentCosts)
                {
                    suggestions.Add(new CostOptimizationSuggestion
                    {
                        SuggestionId = 3,
                        Title = "بهینه‌سازی برنامه کاری",
                        Description = "درآمد کمتر از هزینه‌ها است. نیاز به افزایش تعداد نوبت‌ها یا کاهش هزینه‌ها",
                        CostSavings = currentCosts * 0.15m, // 15% صرفه‌جویی
                        ImplementationPriority = 1,
                        Difficulty = "متوسط",
                        EstimatedImplementationDays = 21
                    });
                }

                // ✅ پیشنهاد 4: استفاده از تکنولوژی
                suggestions.Add(new CostOptimizationSuggestion
                {
                    SuggestionId = 4,
                    Title = "استفاده از سیستم‌های خودکار",
                    Description = "استفاده از سیستم‌های خودکار برای کاهش هزینه‌های دستی",
                    CostSavings = currentCosts * 0.08m, // 8% صرفه‌جویی
                    ImplementationPriority = 3,
                    Difficulty = "سخت",
                    EstimatedImplementationDays = 60
                });
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در تولید پیشنهادات بهینه‌سازی هزینه");
            }

            return suggestions.OrderBy(s => s.ImplementationPriority).ToList();
        }

        /// <summary>
        /// محاسبه هزینه‌های بهینه شده
        /// </summary>
        private decimal CalculateOptimizedCosts(decimal currentCosts, List<CostOptimizationSuggestion> suggestions)
        {
            if (suggestions == null || !suggestions.Any())
            {
                return currentCosts;
            }

            var totalSavings = suggestions.Sum(s => s.CostSavings);
            return Math.Max(0, currentCosts - totalSavings);
        }
    }
}

