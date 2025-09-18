using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Insurance;
using ClinicApp.ViewModels.Insurance.Supplementary;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس تخصصی بیمه تکمیلی
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. محاسبات پیچیده بیمه تکمیلی
    /// 2. مدیریت تنظیمات تخصصی
    /// 3. پشتیبانی از سناریوهای مختلف
    /// 4. رعایت استانداردهای پزشکی ایران
    /// </summary>
    public class SupplementaryInsuranceService : ISupplementaryInsuranceService
    {
        #region Fields and Constructor

        private readonly IPatientInsuranceRepository _patientInsuranceRepository;
        private readonly IInsuranceTariffRepository _tariffRepository;
        private readonly IInsurancePlanRepository _planRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ISupplementaryInsuranceCacheService _cacheService;
        private readonly ISupplementaryInsuranceMonitoringService _monitoringService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _log;

        public SupplementaryInsuranceService(
            IPatientInsuranceRepository patientInsuranceRepository,
            IInsuranceTariffRepository tariffRepository,
            IInsurancePlanRepository planRepository,
            IServiceRepository serviceRepository,
            ISupplementaryInsuranceCacheService cacheService,
            ISupplementaryInsuranceMonitoringService monitoringService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _patientInsuranceRepository = patientInsuranceRepository ?? throw new ArgumentNullException(nameof(patientInsuranceRepository));
            _tariffRepository = tariffRepository ?? throw new ArgumentNullException(nameof(tariffRepository));
            _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _log = logger.ForContext<SupplementaryInsuranceService>();
        }

        #endregion

        #region Supplementary Insurance Calculation

        /// <summary>
        /// محاسبه بیمه تکمیلی با تنظیمات خاص
        /// </summary>
        public async Task<ServiceResult<SupplementaryCalculationResult>> CalculateSupplementaryInsuranceAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            decimal primaryCoverage, 
            DateTime calculationDate)
        {
            var startTime = DateTime.UtcNow;
            var calculationEvent = new CalculationEvent
            {
                PatientId = patientId,
                ServiceId = serviceId,
                ServiceAmount = serviceAmount,
                PrimaryCoverage = primaryCoverage
            };

            try
            {
                _log.Information("🏥 MEDICAL: درخواست محاسبه بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, primaryCoverage, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت بیمه تکمیلی بیمار
                var supplementaryInsurance = await _patientInsuranceRepository.GetSupplementaryByPatientIdAsync(patientId);
                if (supplementaryInsurance == null || !supplementaryInsurance.Any())
                {
                    _log.Warning("🏥 MEDICAL: بیمه تکمیلی یافت نشد - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<SupplementaryCalculationResult>.Failed("بیمه تکمیلی فعال یافت نشد");
                }

                var activeSupplementary = supplementaryInsurance.FirstOrDefault(pi => pi.IsActive && 
                    (pi.EndDate == null || pi.EndDate > calculationDate));

                if (activeSupplementary == null)
                {
                    _log.Warning("🏥 MEDICAL: بیمه تکمیلی فعال یافت نشد - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<SupplementaryCalculationResult>.Failed("بیمه تکمیلی فعال یافت نشد");
                }

                // دریافت تعرفه بیمه تکمیلی
                var supplementaryTariffs = await _tariffRepository.GetByPlanIdAsync(activeSupplementary.InsurancePlanId);
                var tariff = supplementaryTariffs.FirstOrDefault(t => 
                    t.ServiceId == serviceId && 
                    t.InsuranceType == InsuranceType.Supplementary);

                if (tariff == null)
                {
                    _log.Warning("🏥 MEDICAL: تعرفه بیمه تکمیلی یافت نشد - ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        serviceId, activeSupplementary.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<SupplementaryCalculationResult>.Failed("تعرفه بیمه تکمیلی برای این خدمت تعریف نشده است");
                }

                // محاسبه مبلغ باقی‌مانده پس از بیمه اصلی
                decimal remainingAmount = serviceAmount - primaryCoverage;
                if (remainingAmount <= 0)
                {
                    _log.Information("🏥 MEDICAL: مبلغ باقی‌مانده صفر یا منفی - ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}. User: {UserName} (Id: {UserId})",
                        serviceAmount, primaryCoverage, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<SupplementaryCalculationResult>.Successful(new SupplementaryCalculationResult
                    {
                        PatientId = patientId,
                        ServiceId = serviceId,
                        ServiceAmount = serviceAmount,
                        PrimaryCoverage = primaryCoverage,
                        SupplementaryCoverage = 0,
                        FinalPatientShare = serviceAmount - primaryCoverage,
                        TotalCoverage = primaryCoverage,
                        CalculationDate = calculationDate,
                        Notes = "مبلغ باقی‌مانده صفر یا منفی است"
                    });
                }

                // محاسبه پوشش بیمه تکمیلی
                decimal supplementaryCoverage = 0;
                
                if (tariff.SupplementaryCoveragePercent.HasValue)
                {
                    supplementaryCoverage = remainingAmount * (tariff.SupplementaryCoveragePercent.Value / 100);
                }

                // اعمال سقف پرداخت
                if (tariff.SupplementaryMaxPayment.HasValue && supplementaryCoverage > tariff.SupplementaryMaxPayment.Value)
                {
                    supplementaryCoverage = tariff.SupplementaryMaxPayment.Value;
                }

                // محاسبه سهم نهایی بیمار
                decimal finalPatientShare = serviceAmount - primaryCoverage - supplementaryCoverage;

                var result = new SupplementaryCalculationResult
                {
                    PatientId = patientId,
                    ServiceId = serviceId,
                    ServiceAmount = serviceAmount,
                    PrimaryCoverage = primaryCoverage,
                    SupplementaryCoverage = supplementaryCoverage,
                    FinalPatientShare = finalPatientShare,
                    TotalCoverage = primaryCoverage + supplementaryCoverage,
                    CalculationDate = calculationDate,
                    Notes = $"محاسبه بر اساس تعرفه: {tariff.SupplementaryCoveragePercent}%"
                };

                _log.Information("🏥 MEDICAL: محاسبه بیمه تکمیلی تکمیل شد - PatientId: {PatientId}, ServiceId: {ServiceId}, SupplementaryCoverage: {SupplementaryCoverage}, FinalPatientShare: {FinalPatientShare}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, supplementaryCoverage, finalPatientShare, _currentUserService.UserName, _currentUserService.UserId);

                // ثبت رویداد محاسبه موفق
                calculationEvent.SupplementaryCoverage = supplementaryCoverage;
                calculationEvent.FinalPatientShare = finalPatientShare;
                calculationEvent.Duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                calculationEvent.Success = true;
                _monitoringService.LogCalculationEvent(calculationEvent);

                return ServiceResult<SupplementaryCalculationResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                // ثبت رویداد خطا
                var errorEvent = new ErrorEvent
                {
                    ErrorType = "CalculationError",
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    PatientId = patientId,
                    ServiceId = serviceId
                };
                _monitoringService.LogErrorEvent(errorEvent);

                // ثبت رویداد محاسبه ناموفق
                calculationEvent.Duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                calculationEvent.Success = false;
                calculationEvent.ErrorMessage = ex.Message;
                _monitoringService.LogCalculationEvent(calculationEvent);
                
                return ServiceResult<SupplementaryCalculationResult>.Failed("خطا در محاسبه بیمه تکمیلی");
            }
        }

        #endregion

        #region Supplementary Settings Management

        /// <summary>
        /// دریافت تنظیمات بیمه تکمیلی
        /// </summary>
        public async Task<ServiceResult<SupplementarySettings>> GetSupplementarySettingsAsync(int planId)
        {
            try
            {
                _log.Information("درخواست تنظیمات بیمه تکمیلی. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                var plan = await _planRepository.GetByIdAsync(planId);
                if (plan == null)
                {
                    return ServiceResult<SupplementarySettings>.Failed("طرح بیمه یافت نشد");
                }

                var supplementaryTariffs = await _tariffRepository.GetByPlanIdAsync(planId);
                var tariffs = supplementaryTariffs.Where(t => t.InsuranceType == InsuranceType.Supplementary).ToList();

                var settings = new SupplementarySettings
                {
                    PlanId = planId,
                    PlanName = plan.Name,
                    IsActive = plan.IsActive,
                    CoveragePercent = tariffs.Any() ? tariffs.Average(t => t.SupplementaryCoveragePercent ?? 0) : 0,
                    MaxPayment = tariffs.Any() ? tariffs.Max(t => t.SupplementaryMaxPayment ?? 0) : 0,
                    Deductible = 0, // می‌تواند از تنظیمات JSON استخراج شود
                    SettingsJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        TariffsCount = tariffs.Count,
                        AverageCoverage = tariffs.Any() ? tariffs.Average(t => t.SupplementaryCoveragePercent ?? 0) : 0,
                        MaxPayment = tariffs.Any() ? tariffs.Max(t => t.SupplementaryMaxPayment ?? 0) : 0
                    })
                };

                _log.Information("تنظیمات بیمه تکمیلی با موفقیت دریافت شد. PlanId: {PlanId}, TariffsCount: {Count}. User: {UserName} (Id: {UserId})",
                    planId, tariffs.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<SupplementarySettings>.Successful(settings);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت تنظیمات بیمه تکمیلی. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);
                
                return ServiceResult<SupplementarySettings>.Failed("خطا در دریافت تنظیمات بیمه تکمیلی");
            }
        }

        /// <summary>
        /// به‌روزرسانی تنظیمات بیمه تکمیلی
        /// </summary>
        public async Task<ServiceResult> UpdateSupplementarySettingsAsync(int planId, SupplementarySettings settings)
        {
            try
            {
                _log.Information("درخواست به‌روزرسانی تنظیمات بیمه تکمیلی. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                var plan = await _planRepository.GetByIdAsync(planId);
                if (plan == null)
                {
                    return ServiceResult.Failed("طرح بیمه یافت نشد");
                }

                // به‌روزرسانی اطلاعات طرح
                plan.Name = settings.PlanName;
                plan.IsActive = settings.IsActive;
                plan.UpdatedAt = DateTime.UtcNow;
                plan.UpdatedByUserId = _currentUserService.UserId;

                // استفاده از متد موجود در Repository
                _planRepository.Update(plan);

                _log.Information("تنظیمات بیمه تکمیلی با موفقیت به‌روزرسانی شد. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("تنظیمات بیمه تکمیلی با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در به‌روزرسانی تنظیمات بیمه تکمیلی. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);
                
                return ServiceResult.Failed("خطا در به‌روزرسانی تنظیمات بیمه تکمیلی");
            }
        }

        #endregion

        #region Supplementary Tariff Management

        /// <summary>
        /// دریافت تعرفه‌های بیمه تکمیلی
        /// </summary>
        public async Task<ServiceResult<List<SupplementaryTariffViewModel>>> GetSupplementaryTariffsAsync(int planId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست تعرفه‌های بیمه تکمیلی. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از Cache Service برای بهبود Performance
                return await _cacheService.GetCachedSupplementaryTariffsAsync(planId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت تعرفه‌های بیمه تکمیلی. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);
                
                return ServiceResult<List<SupplementaryTariffViewModel>>.Failed("خطا در دریافت تعرفه‌های بیمه تکمیلی");
            }
        }

        /// <summary>
        /// محاسبه پیشرفته بیمه تکمیلی با الگوریتم‌های پیچیده
        /// </summary>
        public async Task<ServiceResult<SupplementaryCalculationResult>> CalculateAdvancedSupplementaryInsuranceAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            decimal primaryCoverage, 
            DateTime calculationDate,
            Dictionary<string, object> advancedSettings = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست محاسبه پیشرفته بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, primaryCoverage, _currentUserService.UserName, _currentUserService.UserId);

                // محاسبه استاندارد
                var standardResult = await CalculateSupplementaryInsuranceAsync(patientId, serviceId, serviceAmount, primaryCoverage, calculationDate);
                if (!standardResult.Success)
                {
                    return standardResult;
                }

                var result = standardResult.Data;

                // اعمال تنظیمات پیشرفته
                if (advancedSettings != null && advancedSettings.Any())
                {
                    result = await ApplyAdvancedSettings(result, advancedSettings, calculationDate);
                }

                // محاسبه آمار و تحلیل
                result = await AddAdvancedAnalytics(result, calculationDate);

                _log.Information("🏥 MEDICAL: محاسبه پیشرفته بیمه تکمیلی تکمیل شد - FinalPatientShare: {FinalPatientShare}, TotalCoverage: {TotalCoverage}. User: {UserName} (Id: {UserId})",
                    result.FinalPatientShare, result.TotalCoverage, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<SupplementaryCalculationResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه پیشرفته بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<SupplementaryCalculationResult>.Failed("خطا در محاسبه پیشرفته بیمه تکمیلی");
            }
        }

        /// <summary>
        /// محاسبه مقایسه‌ای بیمه‌های تکمیلی مختلف
        /// </summary>
        public async Task<ServiceResult<List<SupplementaryCalculationResult>>> CompareSupplementaryInsuranceOptionsAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            decimal primaryCoverage, 
            DateTime calculationDate,
            List<int> supplementaryPlanIds = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع مقایسه گزینه‌های بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, _currentUserService.UserName, _currentUserService.UserId);

                var results = new List<SupplementaryCalculationResult>();

                // محاسبه با بیمه تکمیلی فعلی
                var currentResult = await CalculateSupplementaryInsuranceAsync(patientId, serviceId, serviceAmount, primaryCoverage, calculationDate);
                if (currentResult.Success)
                {
                    currentResult.Data.Notes = "بیمه تکمیلی فعلی";
                    results.Add(currentResult.Data);
                }

                // محاسبه با سایر گزینه‌های بیمه تکمیلی
                if (supplementaryPlanIds != null && supplementaryPlanIds.Any())
                {
                    foreach (var planId in supplementaryPlanIds)
                    {
                        var alternativeResult = await CalculateAlternativeSupplementaryInsuranceAsync(
                            patientId, serviceId, serviceAmount, primaryCoverage, calculationDate, planId);
                        
                        if (alternativeResult.Success)
                        {
                            alternativeResult.Data.Notes = $"گزینه بیمه تکمیلی - PlanId: {planId}";
                            results.Add(alternativeResult.Data);
                        }
                    }
                }

                _log.Information("🏥 MEDICAL: مقایسه گزینه‌های بیمه تکمیلی تکمیل شد - OptionsCount: {OptionsCount}. User: {UserName} (Id: {UserId})",
                    results.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<SupplementaryCalculationResult>>.Successful(results);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در مقایسه گزینه‌های بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<List<SupplementaryCalculationResult>>.Failed("خطا در مقایسه گزینه‌های بیمه تکمیلی");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// اعمال تنظیمات پیشرفته
        /// </summary>
        private async Task<SupplementaryCalculationResult> ApplyAdvancedSettings(
            SupplementaryCalculationResult result, 
            Dictionary<string, object> advancedSettings, 
            DateTime calculationDate)
        {
            try
            {
                // اعمال تخفیف خاص
                if (advancedSettings.ContainsKey("discountPercent") && 
                    decimal.TryParse(advancedSettings["discountPercent"].ToString(), out decimal discountPercent))
                {
                    var discountAmount = result.ServiceAmount * (discountPercent / 100);
                    result.ServiceAmount -= discountAmount;
                    result.FinalPatientShare = Math.Max(0, result.FinalPatientShare - discountAmount);
                    result.Notes += $" | تخفیف {discountPercent}% اعمال شد";
                }

                // اعمال سقف پرداخت خاص
                if (advancedSettings.ContainsKey("maxPatientPayment") && 
                    decimal.TryParse(advancedSettings["maxPatientPayment"].ToString(), out decimal maxPatientPayment))
                {
                    if (result.FinalPatientShare > maxPatientPayment)
                    {
                        var reduction = result.FinalPatientShare - maxPatientPayment;
                        result.FinalPatientShare = maxPatientPayment;
                        result.SupplementaryCoverage += reduction;
                        result.TotalCoverage += reduction;
                        result.Notes += $" | سقف پرداخت بیمار: {maxPatientPayment:N0} ریال";
                    }
                }

                // اعمال فرانشیز خاص
                if (advancedSettings.ContainsKey("deductible") && 
                    decimal.TryParse(advancedSettings["deductible"].ToString(), out decimal deductible))
                {
                    result.FinalPatientShare += deductible;
                    result.Notes += $" | فرانشیز: {deductible:N0} ریال";
                }

                // اعمال محدودیت زمانی
                if (advancedSettings.ContainsKey("timeRestriction") && 
                    bool.TryParse(advancedSettings["timeRestriction"].ToString(), out bool timeRestriction) && 
                    timeRestriction)
                {
                    // بررسی محدودیت زمانی (مثلاً فقط در ساعات اداری)
                    var currentHour = calculationDate.Hour;
                    if (currentHour < 8 || currentHour > 18)
                    {
                        result.SupplementaryCoverage *= 0.5m; // کاهش 50% در ساعات غیراداری
                        result.FinalPatientShare = result.ServiceAmount - result.PrimaryCoverage - result.SupplementaryCoverage;
                        result.TotalCoverage = result.PrimaryCoverage + result.SupplementaryCoverage;
                        result.Notes += " | محدودیت زمانی اعمال شد";
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در اعمال تنظیمات پیشرفته");
                return result;
            }
        }

        /// <summary>
        /// اضافه کردن آمار و تحلیل پیشرفته
        /// </summary>
        private async Task<SupplementaryCalculationResult> AddAdvancedAnalytics(
            SupplementaryCalculationResult result, 
            DateTime calculationDate)
        {
            try
            {
                // محاسبه درصد پوشش کل
                var totalCoveragePercent = result.ServiceAmount > 0 ? 
                    (result.TotalCoverage / result.ServiceAmount) * 100 : 0;

                // محاسبه درصد سهم بیمار
                var patientSharePercent = result.ServiceAmount > 0 ? 
                    (result.FinalPatientShare / result.ServiceAmount) * 100 : 0;

                // محاسبه صرفه‌جویی بیمار
                var patientSavings = result.ServiceAmount - result.FinalPatientShare;

                // محاسبه کارایی بیمه تکمیلی
                var supplementaryEfficiency = result.PrimaryCoverage > 0 ? 
                    (result.SupplementaryCoverage / result.PrimaryCoverage) * 100 : 0;

                // اضافه کردن آمار به یادداشت‌ها
                var analytics = $" | پوشش کل: {totalCoveragePercent:F1}% | سهم بیمار: {patientSharePercent:F1}% | صرفه‌جویی: {patientSavings:N0} ریال | کارایی تکمیلی: {supplementaryEfficiency:F1}%";
                result.Notes += analytics;

                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در محاسبه آمار و تحلیل پیشرفته");
                return result;
            }
        }

        /// <summary>
        /// محاسبه بیمه تکمیلی جایگزین
        /// </summary>
        private async Task<ServiceResult<SupplementaryCalculationResult>> CalculateAlternativeSupplementaryInsuranceAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            decimal primaryCoverage, 
            DateTime calculationDate, 
            int alternativePlanId)
        {
            try
            {
                // دریافت تعرفه بیمه تکمیلی جایگزین
                var alternativeTariffs = await _tariffRepository.GetByPlanIdAsync(alternativePlanId);
                var alternativeTariff = alternativeTariffs.FirstOrDefault(t => 
                    t.ServiceId == serviceId && 
                    t.InsuranceType == InsuranceType.Supplementary);

                if (alternativeTariff == null)
                {
                    return ServiceResult<SupplementaryCalculationResult>.Failed("تعرفه بیمه تکمیلی جایگزین یافت نشد");
                }

                // محاسبه با تعرفه جایگزین
                decimal remainingAmount = serviceAmount - primaryCoverage;
                decimal supplementaryCoverage = 0;

                if (remainingAmount > 0 && alternativeTariff.SupplementaryCoveragePercent.HasValue)
                {
                    supplementaryCoverage = remainingAmount * (alternativeTariff.SupplementaryCoveragePercent.Value / 100m);
                    
                    if (alternativeTariff.SupplementaryMaxPayment.HasValue && supplementaryCoverage > alternativeTariff.SupplementaryMaxPayment.Value)
                    {
                        supplementaryCoverage = alternativeTariff.SupplementaryMaxPayment.Value;
                    }
                }

                decimal finalPatientShare = remainingAmount - supplementaryCoverage;
                if (finalPatientShare < 0) finalPatientShare = 0;

                var result = new SupplementaryCalculationResult
                {
                    PatientId = patientId,
                    ServiceId = serviceId,
                    ServiceAmount = serviceAmount,
                    PrimaryCoverage = primaryCoverage,
                    SupplementaryCoverage = supplementaryCoverage,
                    FinalPatientShare = finalPatientShare,
                    TotalCoverage = primaryCoverage + supplementaryCoverage,
                    CalculationDate = calculationDate,
                    Notes = $"طرح بیمه تکمیلی جایگزین - PlanId: {alternativePlanId}"
                };

                return ServiceResult<SupplementaryCalculationResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در محاسبه بیمه تکمیلی جایگزین - PlanId: {PlanId}", alternativePlanId);
                return ServiceResult<SupplementaryCalculationResult>.Failed("خطا در محاسبه بیمه تکمیلی جایگزین");
            }
        }

        #endregion
    }
}
