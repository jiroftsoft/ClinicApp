using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.ViewModels.Insurance;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس محاسبه پیشرفته بیمه - طراحی شده برای محیط‌های درمانی پیچیده
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. محاسبه بهینه با الگوریتم‌های پیشرفته
    /// 2. پشتیبانی از محدودیت‌های پیچیده
    /// 3. محاسبه تدریجی بیمه‌های متعدد
    /// 4. رعایت اولویت‌ها و سقف‌ها
    /// 5. پشتیبانی از سناریوهای پیچیده درمانی
    /// </summary>
    public class AdvancedInsuranceCalculationService : IAdvancedInsuranceCalculationService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _log;

        public AdvancedInsuranceCalculationService(
            ICurrentUserService currentUserService,
            ILogger log)
        {
            _currentUserService = currentUserService;
            _log = log;
        }

        /// <summary>
        /// محاسبه بهینه پوشش بیمه با الگوریتم پیشرفته
        /// </summary>
        public async Task<ServiceResult<AdvancedCalculationResult>> CalculateOptimalCoverageAsync(
            decimal serviceAmount,
            List<InsuranceCoverage> coverages,
            DateTime calculationDate,
            Dictionary<string, object> advancedSettings = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه بهینه پوشش بیمه - ServiceAmount: {ServiceAmount}, CoverageCount: {CoverageCount}, CalculationDate: {CalculationDate}. User: {UserName} (Id: {UserId})",
                    serviceAmount, coverages.Count, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                var result = new AdvancedCalculationResult
                {
                    ServiceAmount = serviceAmount,
                    CalculationDate = calculationDate,
                    Coverages = new List<InsuranceCoverageDetail>()
                };

                var remainingAmount = serviceAmount;
                var totalCoverage = 0m;

                // مرتب‌سازی بیمه‌ها بر اساس اولویت
                var sortedCoverages = coverages
                    .OrderBy(c => c.Priority)
                    .ToList();

                foreach (var coverage in sortedCoverages)
                {
                    if (remainingAmount <= 0) break;

                    var coverageDetail = await CalculateSingleCoverageAsync(
                        coverage, remainingAmount, calculationDate, advancedSettings);

                    if (coverageDetail.Success)
                    {
                        result.Coverages.Add(coverageDetail.Data);
                        totalCoverage += coverageDetail.Data.ActualCoverage;
                        remainingAmount -= coverageDetail.Data.ActualCoverage;

                        _log.Debug("🏥 MEDICAL: پوشش بیمه محاسبه شد - InsuranceId: {InsuranceId}, Priority: {Priority}, ActualCoverage: {ActualCoverage}, RemainingAmount: {RemainingAmount}",
                            coverage.InsuranceId, coverage.Priority, coverageDetail.Data.ActualCoverage, remainingAmount);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: خطا در محاسبه پوشش بیمه - InsuranceId: {InsuranceId}, Error: {Error}",
                            coverage.InsuranceId, coverageDetail.Message);
                    }
                }

                result.TotalCoverage = totalCoverage;
                result.FinalPatientShare = remainingAmount;
                result.CoveragePercentage = serviceAmount > 0 ? (totalCoverage / serviceAmount) * 100 : 0;

                _log.Information("🏥 MEDICAL: محاسبه بهینه پوشش بیمه تکمیل شد - ServiceAmount: {ServiceAmount}, TotalCoverage: {TotalCoverage}, FinalPatientShare: {FinalPatientShare}, CoveragePercentage: {CoveragePercentage}%. User: {UserName} (Id: {UserId})",
                    serviceAmount, totalCoverage, remainingAmount, result.CoveragePercentage, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<AdvancedCalculationResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه بهینه پوشش بیمه - ServiceAmount: {ServiceAmount}. User: {UserName} (Id: {UserId})",
                    serviceAmount, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<AdvancedCalculationResult>.Failed("خطا در محاسبه بهینه پوشش بیمه");
            }
        }

        /// <summary>
        /// محاسبه پوشش یک بیمه خاص
        /// </summary>
        private async Task<ServiceResult<InsuranceCoverageDetail>> CalculateSingleCoverageAsync(
            InsuranceCoverage coverage,
            decimal remainingAmount,
            DateTime calculationDate,
            Dictionary<string, object> advancedSettings)
        {
            try
            {
                var coverageAmount = 0m;

                // محاسبه بر اساس درصد
                if (coverage.Percentage > 0)
                {
                    coverageAmount = remainingAmount * (coverage.Percentage / 100);
                }
                else
                {
                    coverageAmount = remainingAmount;
                }

                // اعمال سقف پرداخت
                if (coverage.MaxAmount.HasValue && coverageAmount > coverage.MaxAmount.Value)
                {
                    coverageAmount = coverage.MaxAmount.Value;
                }

                // اعمال حداقل پرداخت
                if (coverage.MinAmount.HasValue && coverageAmount < coverage.MinAmount.Value)
                {
                    coverageAmount = coverage.MinAmount.Value;
                }

                // اعمال تنظیمات پیشرفته
                if (advancedSettings != null && advancedSettings.ContainsKey($"coverage_{coverage.InsuranceId}"))
                {
                    var customSettings = advancedSettings[$"coverage_{coverage.InsuranceId}"] as Dictionary<string, object>;
                    coverageAmount = await ApplyCustomSettings(coverageAmount, customSettings, calculationDate);
                }

                var result = new InsuranceCoverageDetail
                {
                    InsuranceId = coverage.InsuranceId,
                    InsuranceName = coverage.InsuranceName,
                    Priority = coverage.Priority,
                    Percentage = coverage.Percentage,
                    MaxAmount = coverage.MaxAmount,
                    MinAmount = coverage.MinAmount,
                    CalculatedCoverage = coverageAmount,
                    ActualCoverage = Math.Min(coverageAmount, remainingAmount),
                    IsApplied = coverageAmount > 0 && remainingAmount > 0,
                    CalculationDate = calculationDate
                };

                return ServiceResult<InsuranceCoverageDetail>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه پوشش بیمه - InsuranceId: {InsuranceId}",
                    coverage.InsuranceId);
                return ServiceResult<InsuranceCoverageDetail>.Failed("خطا در محاسبه پوشش بیمه");
            }
        }

        /// <summary>
        /// اعمال تنظیمات خاص
        /// </summary>
        private async Task<decimal> ApplyCustomSettings(
            decimal baseAmount,
            Dictionary<string, object> customSettings,
            DateTime calculationDate)
        {
            try
            {
                var adjustedAmount = baseAmount;

                // اعمال ضریب خاص
                if (customSettings.ContainsKey("multiplier"))
                {
                    var multiplier = Convert.ToDecimal(customSettings["multiplier"]);
                    adjustedAmount *= multiplier;
                }

                // اعمال تخفیف
                if (customSettings.ContainsKey("discount_percent"))
                {
                    var discountPercent = Convert.ToDecimal(customSettings["discount_percent"]);
                    adjustedAmount *= (1 - discountPercent / 100);
                }

                // اعمال محدودیت زمانی
                if (customSettings.ContainsKey("time_limit_hours"))
                {
                    var timeLimitHours = Convert.ToInt32(customSettings["time_limit_hours"]);
                    var hoursSinceCalculation = (DateTime.Now - calculationDate).TotalHours;
                    
                    if (hoursSinceCalculation > timeLimitHours)
                    {
                        adjustedAmount *= 0.5m; // کاهش 50% برای محاسبات قدیمی
                    }
                }

                return adjustedAmount;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اعمال تنظیمات خاص");
                return baseAmount;
            }
        }
    }
}
