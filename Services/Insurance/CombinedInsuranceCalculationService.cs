using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using ClinicApp.ViewModels.Insurance.Supplementary;
using ClinicApp.ViewModels.Insurance;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس محاسبه بیمه ترکیبی (اصلی + تکمیلی) - طراحی شده برای کلینیک‌های درمانی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. محاسبه ترکیبی بیمه اصلی و تکمیلی
    /// 2. رعایت سقف‌های پرداخت و فرانشیز
    /// 3. محاسبه دقیق سهم بیمار نهایی
    /// 4. پشتیبانی از انواع مختلف بیمه‌های تکمیلی
    /// 5. رعایت استانداردهای پزشکی ایران
    /// 
    /// مثال محاسبه:
    /// - خدمت: 1,000,000 تومان
    /// - بیمه اصلی: 70% = 700,000 تومان
    /// - بیمه تکمیلی: 20% از باقی‌مانده = 60,000 تومان
    /// - سهم بیمار: 10% = 100,000 تومان
    /// </summary>
    public class CombinedInsuranceCalculationService : ICombinedInsuranceCalculationService
    {
        private readonly IPatientInsuranceRepository _patientInsuranceRepository;
        private readonly IInsuranceCalculationService _insuranceCalculationService;
        private readonly ISupplementaryInsuranceService _supplementaryInsuranceService;
        private readonly IServiceRepository _serviceRepository;
        private readonly IInsuranceTariffRepository _tariffRepository;
        private readonly IPatientService _patientService;
        private readonly ApplicationDbContext _context;
        private readonly IFactorSettingService _factorSettingService;
        // حذف مرجع دایره‌ای - PatientInsuranceService نباید در CombinedInsuranceCalculationService استفاده شود
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public CombinedInsuranceCalculationService(
            IPatientInsuranceRepository patientInsuranceRepository,
            IInsuranceCalculationService insuranceCalculationService,
            ISupplementaryInsuranceService supplementaryInsuranceService,
            IServiceRepository serviceRepository,
            IInsuranceTariffRepository tariffRepository,
            IPatientService patientService,
            ApplicationDbContext context,
            IFactorSettingService factorSettingService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _patientInsuranceRepository = patientInsuranceRepository ?? throw new ArgumentNullException(nameof(patientInsuranceRepository));
            _insuranceCalculationService = insuranceCalculationService ?? throw new ArgumentNullException(nameof(insuranceCalculationService));
            _supplementaryInsuranceService = supplementaryInsuranceService ?? throw new ArgumentNullException(nameof(supplementaryInsuranceService));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _tariffRepository = tariffRepository ?? throw new ArgumentNullException(nameof(tariffRepository));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _factorSettingService = factorSettingService ?? throw new ArgumentNullException(nameof(factorSettingService));
            _log = logger.ForContext<CombinedInsuranceCalculationService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region ICombinedInsuranceCalculationService Implementation

        /// <summary>
        /// محاسبه ترکیبی بیمه اصلی و تکمیلی برای یک خدمت - بهینه شده برای محیط عملیاتی درمانی
        /// </summary>
        public async Task<ServiceResult<CombinedInsuranceCalculationResult>> CalculateCombinedInsuranceAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime calculationDate)
        {
            var startTime = DateTime.UtcNow;
            var calculationId = Guid.NewGuid();
            
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه بیمه ترکیبی - CalculationId: {CalculationId}, PatientId: {PatientId}, ServiceId: {ServiceId}, Amount: {Amount}, Date: {Date}. User: {UserName} (Id: {UserId})",
                    calculationId, patientId, serviceId, serviceAmount, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی ورودی‌ها با جزئیات بیشتر
                var validationResult = await ValidateInputsAsync(patientId, serviceId, serviceAmount, calculationDate);
                if (!validationResult.Success)
                {
                    _log.Warning("🏥 MEDICAL: اعتبارسنجی ناموفق - CalculationId: {CalculationId}, PatientId: {PatientId}, ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        calculationId, patientId, serviceId, validationResult.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<CombinedInsuranceCalculationResult>.Failed(validationResult.Message);
                }

                // دریافت بیمه‌های بیمار
                var patientInsurancesResult = await GetPatientInsurancesAsync(patientId);
                if (!patientInsurancesResult.Success)
                {
                    return ServiceResult<CombinedInsuranceCalculationResult>.Failed(patientInsurancesResult.Message);
                }

                var patientInsurances = patientInsurancesResult.Data;
                
                // بررسی وجود بیمه اصلی
                var primaryInsurance = patientInsurances.FirstOrDefault(pi => pi.IsPrimary && pi.IsActive);
                if (primaryInsurance == null)
                {
                    _log.Warning("🏥 MEDICAL: بیمه اصلی یافت نشد - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<CombinedInsuranceCalculationResult>.Failed("بیمه اصلی فعال یافت نشد");
                }

                // محاسبه بیمه اصلی
                var primaryCalculationResult = await CalculatePrimaryInsuranceAsync(
                    primaryInsurance, serviceId, serviceAmount, calculationDate);
                
                if (!primaryCalculationResult.Success)
                {
                    return ServiceResult<CombinedInsuranceCalculationResult>.Failed(
                        $"خطا در محاسبه بیمه اصلی: {primaryCalculationResult.Message}");
                }

                var primaryResult = primaryCalculationResult.Data;

                // دریافت بیمه‌های تکمیلی بر اساس اولویت
                var supplementaryInsurances = patientInsurances
                    .Where(pi => !pi.IsPrimary && pi.IsActive)
                    .OrderBy(pi => pi.Priority)
                    .ToList();
                
                CombinedInsuranceCalculationResult finalResult;

                if (supplementaryInsurances.Any())
                {
                    // محاسبه بیمه ترکیبی (اصلی + تکمیلی‌های متعدد)
                    finalResult = await CalculateMultipleSupplementaryInsuranceAsync(
                        primaryResult, supplementaryInsurances, serviceId, serviceAmount, calculationDate, patientId);
                    
                    _log.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی تکمیل شد - PrimaryCoverage: {PrimaryCoverage}, SupplementaryCount: {SupplementaryCount}, TotalSupplementaryCoverage: {TotalSupplementaryCoverage}, FinalPatientShare: {FinalPatientShare}. User: {UserName} (Id: {UserId})",
                        primaryResult.InsuranceCoverage, supplementaryInsurances.Count, finalResult.SupplementaryCoverage, finalResult.FinalPatientShare, _currentUserService.UserName, _currentUserService.UserId);
                }
                else
                {
                    // فقط بیمه اصلی
                    finalResult = new CombinedInsuranceCalculationResult
                    {
                        PatientId = patientId,
                        ServiceId = serviceId,
                        ServiceAmount = serviceAmount,
                        PrimaryInsuranceId = primaryInsurance.PatientInsuranceId,
                        PrimaryCoverage = primaryResult.InsuranceCoverage,
                        PrimaryCoveragePercent = primaryResult.CoveragePercent,
                        SupplementaryInsuranceId = null,
                        SupplementaryCoverage = 0,
                        SupplementaryCoveragePercent = 0,
                        FinalPatientShare = primaryResult.PatientPayment,
                        TotalInsuranceCoverage = primaryResult.InsuranceCoverage,
                        CalculationDate = calculationDate,
                        HasSupplementaryInsurance = false
                    };

                    _log.Information("🏥 MEDICAL: فقط بیمه اصلی محاسبه شد - PrimaryCoverage: {PrimaryCoverage}, PatientShare: {PatientShare}. User: {UserName} (Id: {UserId})",
                        primaryResult.InsuranceCoverage, primaryResult.PatientPayment, _currentUserService.UserName, _currentUserService.UserId);
                }

                return ServiceResult<CombinedInsuranceCalculationResult>.Successful(finalResult);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}, Amount: {Amount}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, _currentUserService.UserName, _currentUserService.UserId);
                
                return ServiceResult<CombinedInsuranceCalculationResult>.Failed("خطا در محاسبه بیمه ترکیبی");
            }
        }

        /// <summary>
        /// محاسبه بیمه ترکیبی برای چندین خدمت
        /// </summary>
        public async Task<ServiceResult<List<CombinedInsuranceCalculationResult>>> CalculateCombinedInsuranceForServicesAsync(
            int patientId, 
            List<int> serviceIds, 
            List<decimal> serviceAmounts, 
            DateTime calculationDate)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه بیمه ترکیبی برای چندین خدمت - PatientId: {PatientId}, ServiceCount: {ServiceCount}, Date: {Date}. User: {UserName} (Id: {UserId})",
                    patientId, serviceIds.Count, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                if (serviceIds.Count != serviceAmounts.Count)
                {
                    return ServiceResult<List<CombinedInsuranceCalculationResult>>.Failed("تعداد خدمات و مبالغ مطابقت ندارد");
                }

                var results = new List<CombinedInsuranceCalculationResult>();

                for (int i = 0; i < serviceIds.Count; i++)
                {
                    var serviceResult = await CalculateCombinedInsuranceAsync(
                        patientId, serviceIds[i], serviceAmounts[i], calculationDate);
                    
                    if (serviceResult.Success)
                    {
                        results.Add(serviceResult.Data);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: خطا در محاسبه خدمت - ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                            serviceIds[i], serviceResult.Message, _currentUserService.UserName, _currentUserService.UserId);
                    }
                }

                _log.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی برای چندین خدمت تکمیل شد - SuccessCount: {SuccessCount}, TotalCount: {TotalCount}. User: {UserName} (Id: {UserId})",
                    results.Count, serviceIds.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<CombinedInsuranceCalculationResult>>.Successful(results);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه بیمه ترکیبی برای چندین خدمت - PatientId: {PatientId}, ServiceCount: {ServiceCount}. User: {UserName} (Id: {UserId})",
                    patientId, serviceIds.Count, _currentUserService.UserName, _currentUserService.UserId);
                
                return ServiceResult<List<CombinedInsuranceCalculationResult>>.Failed("خطا در محاسبه بیمه ترکیبی برای چندین خدمت");
            }
        }

        /// <summary>
        /// محاسبه پیشرفته بیمه ترکیبی با در نظر گیری تنظیمات خاص
        /// </summary>
        public async Task<ServiceResult<CombinedInsuranceCalculationResult>> CalculateAdvancedCombinedInsuranceAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime calculationDate,
            Dictionary<string, object> customSettings = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه پیشرفته بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}, Amount: {Amount}, Date: {Date}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                // محاسبه استاندارد
                var standardResult = await CalculateCombinedInsuranceAsync(patientId, serviceId, serviceAmount, calculationDate);
                if (!standardResult.Success)
                {
                    return standardResult;
                }

                var result = standardResult.Data;

                // اعمال تنظیمات خاص اگر ارائه شده باشد
                if (customSettings != null && customSettings.Any())
                {
                    result = ApplyCustomSettings(result, customSettings);
                }

                // محاسبه آمار و تحلیل
                result = await AddCalculationAnalytics(result, calculationDate);

                _log.Information("🏥 MEDICAL: محاسبه پیشرفته بیمه ترکیبی تکمیل شد - FinalPatientShare: {FinalPatientShare}, TotalCoverage: {TotalCoverage}. User: {UserName} (Id: {UserId})",
                    result.FinalPatientShare, result.TotalInsuranceCoverage, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<CombinedInsuranceCalculationResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه پیشرفته بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                
                return ServiceResult<CombinedInsuranceCalculationResult>.Failed("خطا در محاسبه پیشرفته بیمه ترکیبی");
            }
        }

        /// <summary>
        /// محاسبه مقایسه‌ای بیمه‌های مختلف
        /// </summary>
        public async Task<ServiceResult<List<CombinedInsuranceCalculationResult>>> CompareInsuranceOptionsAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime calculationDate,
            List<int> insurancePlanIds = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع مقایسه گزینه‌های بیمه - PatientId: {PatientId}, ServiceId: {ServiceId}, Amount: {Amount}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, _currentUserService.UserName, _currentUserService.UserId);

                var results = new List<CombinedInsuranceCalculationResult>();

                // محاسبه با بیمه فعلی
                var currentResult = await CalculateCombinedInsuranceAsync(patientId, serviceId, serviceAmount, calculationDate);
                if (currentResult.Success)
                {
                    currentResult.Data.Notes = "بیمه فعلی";
                    results.Add(currentResult.Data);
                }

                // محاسبه با سایر گزینه‌های بیمه اگر ارائه شده باشد
                if (insurancePlanIds != null && insurancePlanIds.Any())
                {
                    foreach (var planId in insurancePlanIds)
                    {
                        var alternativeResult = await CalculateAlternativeInsuranceAsync(
                            patientId, serviceId, serviceAmount, calculationDate, planId);
                        
                        if (alternativeResult.Success)
                        {
                            alternativeResult.Data.Notes = $"گزینه بیمه - PlanId: {planId}";
                            results.Add(alternativeResult.Data);
                        }
                    }
                }

                _log.Information("🏥 MEDICAL: مقایسه گزینه‌های بیمه تکمیل شد - OptionsCount: {OptionsCount}. User: {UserName} (Id: {UserId})",
                    results.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<CombinedInsuranceCalculationResult>>.Successful(results);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در مقایسه گزینه‌های بیمه - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                
                return ServiceResult<List<CombinedInsuranceCalculationResult>>.Failed("خطا در مقایسه گزینه‌های بیمه");
            }
        }

        /// <summary>
        /// دریافت بیمه‌های بیمار
        /// </summary>
        public async Task<ServiceResult<List<PatientInsurance>>> GetPatientInsurancesAsync(int patientId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست دریافت بیمه‌های بیمار - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                var patientInsurances = await _patientInsuranceRepository.GetByPatientIdAsync(patientId);
                
                if (patientInsurances == null || !patientInsurances.Any())
                {
                    _log.Warning("🏥 MEDICAL: بیمه‌ای برای بیمار یافت نشد - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<List<PatientInsurance>>.Failed("بیمه‌ای برای این بیمار یافت نشد");
                }

                _log.Information("🏥 MEDICAL: بیمه‌های بیمار با موفقیت دریافت شد - PatientId: {PatientId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                    patientId, patientInsurances.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<PatientInsurance>>.Successful(patientInsurances.ToList());
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت بیمه‌های بیمار - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                
                return ServiceResult<List<PatientInsurance>>.Failed("خطا در دریافت بیمه‌های بیمار");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// اعتبارسنجی پیشرفته ورودی‌ها برای محیط عملیاتی درمانی
        /// </summary>
        private async Task<ServiceResult> ValidateInputsAsync(int patientId, int serviceId, decimal serviceAmount, DateTime calculationDate)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع اعتبارسنجی ورودی‌ها - PatientId: {PatientId}, ServiceId: {ServiceId}, Amount: {Amount}, Date: {Date}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی شناسه بیمار
                if (patientId <= 0)
                {
                    return ServiceResult.Failed("شناسه بیمار نامعتبر است");
                }

                // اعتبارسنجی شناسه خدمت
                if (serviceId <= 0)
                {
                    return ServiceResult.Failed("شناسه خدمت نامعتبر است");
                }

                // اعتبارسنجی مبلغ خدمت
                if (serviceAmount <= 0)
                {
                    return ServiceResult.Failed("مبلغ خدمت باید بیشتر از صفر باشد");
                }

                if (serviceAmount > 100000000) // 100 میلیون تومان
                {
                    return ServiceResult.Failed("مبلغ خدمت بیش از حد مجاز است");
                }

                // اعتبارسنجی تاریخ محاسبه
                if (calculationDate > DateTime.Now.AddDays(1))
                {
                    return ServiceResult.Failed("تاریخ محاسبه نمی‌تواند در آینده باشد");
                }

                if (calculationDate < DateTime.Now.AddYears(-1))
                {
                    return ServiceResult.Failed("تاریخ محاسبه نمی‌تواند بیش از یک سال گذشته باشد");
                }

                // بررسی وجود بیمار
                var patientExists = await _patientService.GetPatientDetailsAsync(patientId);
                if (!patientExists.Success || patientExists.Data == null)
                {
                    return ServiceResult.Failed("بیمار یافت نشد");
                }

                // بررسی وجود خدمت
                var serviceExists = await _serviceRepository.DoesServiceExistByIdAsync(serviceId);
                if (!serviceExists)
                {
                    return ServiceResult.Failed("خدمت یافت نشد");
                }

                _log.Information("🏥 MEDICAL: اعتبارسنجی ورودی‌ها موفق - PatientId: {PatientId}, ServiceId: {ServiceId}, Amount: {Amount}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("اعتبارسنجی موفق");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی ورودی‌ها - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Failed("خطا در اعتبارسنجی ورودی‌ها");
            }
        }

        /// <summary>
        /// اعتبارسنجی ورودی‌ها
        /// </summary>
        private ServiceResult ValidateInputs(int patientId, int serviceId, decimal serviceAmount, DateTime calculationDate)
        {
            if (patientId <= 0)
                return ServiceResult.Failed("شناسه بیمار نامعتبر است");
            
            if (serviceId <= 0)
                return ServiceResult.Failed("شناسه خدمت نامعتبر است");
            
            if (serviceAmount <= 0)
                return ServiceResult.Failed("مبلغ خدمت باید بیشتر از صفر باشد");
            
            if (calculationDate > DateTime.Now.AddDays(1))
                return ServiceResult.Failed("تاریخ محاسبه نمی‌تواند در آینده باشد");

            return ServiceResult.Successful();
        }


        /// <summary>
        /// محاسبه بیمه اصلی با پشتیبانی از تعرفه‌های ناقص
        /// </summary>
        private async Task<ServiceResult<InsuranceCalculationResultViewModel>> CalculatePrimaryInsuranceAsync(
            PatientInsurance primaryInsurance, int serviceId, decimal serviceAmount, DateTime calculationDate)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه بیمه اصلی - PatientInsuranceId: {PatientInsuranceId}, ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    primaryInsurance.PatientInsuranceId, serviceId, serviceAmount, primaryInsurance.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                // ابتدا سعی کن از سرویس عادی محاسبه کن
                var result = await _insuranceCalculationService.CalculatePatientShareAsync(
                    primaryInsurance.PatientId, serviceId, calculationDate);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: محاسبه بیمه اصلی موفق - InsuranceCoverage: {InsuranceCoverage}, PatientPayment: {PatientPayment}. User: {UserName} (Id: {UserId})",
                        result.Data.InsuranceCoverage, result.Data.PatientPayment, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<InsuranceCalculationResultViewModel>.Successful(result.Data);
                }

                // اگر محاسبه عادی ناموفق بود، از fallback logic استفاده کن
                _log.Warning("🏥 MEDICAL: محاسبه عادی بیمه اصلی ناموفق، استفاده از fallback logic - Error: {Error}. User: {UserName} (Id: {UserId})",
                    result.Message, _currentUserService.UserName, _currentUserService.UserId);

                var fallbackResult = await CalculatePrimaryInsuranceFallbackAsync(
                    primaryInsurance, serviceId, serviceAmount, calculationDate);

                if (fallbackResult.Success)
                {
                    _log.Information("🏥 MEDICAL: محاسبه fallback بیمه اصلی موفق - InsuranceCoverage: {InsuranceCoverage}, PatientPayment: {PatientPayment}. User: {UserName} (Id: {UserId})",
                        fallbackResult.Data.InsuranceCoverage, fallbackResult.Data.PatientPayment, _currentUserService.UserName, _currentUserService.UserId);
                    return fallbackResult;
                }

                return ServiceResult<InsuranceCalculationResultViewModel>.Failed($"خطا در محاسبه بیمه اصلی: {result.Message}");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه بیمه اصلی - PatientInsuranceId: {PatientInsuranceId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    primaryInsurance.PatientInsuranceId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<InsuranceCalculationResultViewModel>.Failed("خطا در محاسبه بیمه اصلی");
            }
        }

        /// <summary>
        /// محاسبه fallback بیمه اصلی با استفاده از درصد پوشش طرح بیمه
        /// </summary>
        private async Task<ServiceResult<InsuranceCalculationResultViewModel>> CalculatePrimaryInsuranceFallbackAsync(
            PatientInsurance primaryInsurance, int serviceId, decimal serviceAmount, DateTime calculationDate)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه fallback بیمه اصلی - PlanId: {PlanId}, ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}. User: {UserName} (Id: {UserId})",
                    primaryInsurance.InsurancePlanId, serviceId, serviceAmount, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه بیمه اصلی از دیتابیس
                var primaryTariff = await _context.InsuranceTariffs
                    .Where(t => t.ServiceId == serviceId && 
                                t.InsurancePlanId == primaryInsurance.InsurancePlanId && 
                                t.InsuranceType == InsuranceType.Primary &&
                                !t.IsDeleted && t.IsActive)
                    .FirstOrDefaultAsync();
                
                decimal coveragePercent;
                decimal deductibleAmount;

                if (primaryTariff == null)
                {
                    _log.Warning("🏥 MEDICAL: تعرفه بیمه اصلی یافت نشد - ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        serviceId, primaryInsurance.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    // Fallback به طرح بیمه
                    var insurancePlan = await _context.InsurancePlans
                        .Where(ip => ip.InsurancePlanId == primaryInsurance.InsurancePlanId && !ip.IsDeleted)
                        .FirstOrDefaultAsync();
                    
                    if (insurancePlan == null)
                    {
                        return ServiceResult<InsuranceCalculationResultViewModel>.Failed("اطلاعات طرح بیمه یافت نشد");
                    }

                    // استفاده از درصد پوشش طرح بیمه از جدول InsurancePlans
                    coveragePercent = insurancePlan.CoveragePercent; // پویا از جدول
                    deductibleAmount = insurancePlan.Deductible;
                    
                    _log.Information("🏥 MEDICAL: استفاده از fallback طرح بیمه - CoveragePercent: {CoveragePercent}, Deductible: {Deductible}. User: {UserName} (Id: {UserId})",
                        coveragePercent, deductibleAmount, _currentUserService.UserName, _currentUserService.UserId);
                }
                else
                {
                    // استفاده از تعرفه بیمه اصلی
                    coveragePercent = (decimal)primaryTariff.InsurerShare / (decimal)primaryTariff.TariffPrice * 100;
                    deductibleAmount = 0m; // فرانشیز در تعرفه محاسبه شده
                    
                    _log.Information("🏥 MEDICAL: استفاده از تعرفه بیمه اصلی - TariffId: {TariffId}, CoveragePercent: {CoveragePercent}. User: {UserName} (Id: {UserId})",
                        primaryTariff.InsuranceTariffId, coveragePercent, _currentUserService.UserName, _currentUserService.UserId);
                }

                // محاسبه مبلغ قابل پوشش (بعد از کسر فرانشیز)
                var coverableAmount = Math.Max(0, serviceAmount - deductibleAmount);

                // محاسبه مبلغ پوشش بیمه با دقت مالی
                var insuranceCoverage = Math.Round(coverableAmount * (coveragePercent / 100), 2, MidpointRounding.AwayFromZero);

                // محاسبه مبلغ باقی‌مانده بعد از بیمه پایه
                var remainingAmount = Math.Max(0, coverableAmount - insuranceCoverage);

                // محاسبه مبلغ پرداخت بیمار = فرانشیز + مبلغ باقی‌مانده
                var patientPayment = Math.Round(deductibleAmount + remainingAmount, 2, MidpointRounding.AwayFromZero);

                var result = new InsuranceCalculationResultViewModel
                {
                    PatientId = primaryInsurance.PatientId,
                    ServiceId = serviceId,
                    TotalAmount = serviceAmount,
                    DeductibleAmount = deductibleAmount,
                    CoverableAmount = coverableAmount,
                    CoveragePercent = coveragePercent,
                    InsuranceCoverage = insuranceCoverage,
                    PatientPayment = patientPayment
                };

                _log.Information("🏥 MEDICAL: محاسبه fallback بیمه اصلی تکمیل شد - CoveragePercent: {CoveragePercent}, InsuranceCoverage: {InsuranceCoverage}, PatientPayment: {PatientPayment}. User: {UserName} (Id: {UserId})",
                    coveragePercent, insuranceCoverage, patientPayment, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsuranceCalculationResultViewModel>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه fallback بیمه اصلی - PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    primaryInsurance.InsurancePlanId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<InsuranceCalculationResultViewModel>.Failed("خطا در محاسبه fallback بیمه اصلی");
            }
        }

        /// <summary>
        /// محاسبه بیمه ترکیبی با چندین بیمه تکمیلی - بهینه شده برای محیط عملیاتی درمانی
        /// </summary>
        private async Task<CombinedInsuranceCalculationResult> CalculateMultipleSupplementaryInsuranceAsync(
            InsuranceCalculationResultViewModel primaryResult,
            List<PatientInsurance> supplementaryInsurances,
            int serviceId,
            decimal serviceAmount,
            DateTime calculationDate,
            int patientId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه بیمه ترکیبی پیشرفته - ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}, SupplementaryCount: {SupplementaryCount}. User: {UserName} (Id: {UserId})",
                    serviceId, serviceAmount, primaryResult.InsuranceCoverage, supplementaryInsurances.Count, _currentUserService.UserName, _currentUserService.UserId);

                var totalSupplementaryCoverage = 0m;
                // 🚨 PROFESSIONAL: remainingAmount = سهم بیمار از بیمه پایه (نه سهم بیمه پایه)
                // این همان مبلغی است که بیمه تکمیلی باید پوشش دهد
                var remainingAmount = serviceAmount - primaryResult.InsuranceCoverage; // سهم بیمار از بیمه پایه
                var supplementaryDetails = new List<SupplementaryInsuranceDetail>();
                var primaryInsuranceId = supplementaryInsurances.FirstOrDefault()?.PatientInsuranceId ?? 0; // استفاده از PatientInsuranceId صحیح

                _log.Information("🏥 MEDICAL: محاسبه سهم بیمار از بیمه پایه - ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}, PatientShareFromPrimary: {PatientShare} (این همان مبلغی است که بیمه تکمیلی باید پوشش دهد). User: {UserName} (Id: {UserId})",
                    serviceAmount, primaryResult.InsuranceCoverage, remainingAmount, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه‌های بیمه تکمیلی برای این خدمت (بهینه‌سازی شده)
                var supplementaryTariffs = await _tariffRepository.GetSupplementaryTariffsAsync(serviceId);

                _log.Information("🏥 MEDICAL: تعرفه‌های بیمه تکمیلی یافت شد - ServiceId: {ServiceId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                    serviceId, supplementaryTariffs.Count, _currentUserService.UserName, _currentUserService.UserId);

                // بررسی اینکه آیا بیمه اصلی کل مبلغ را پوشش داده یا نه (یعنی سهم بیمار صفر است)
                if (remainingAmount <= 0)
                {
                    _log.Information("🏥 MEDICAL: بیمه اصلی کل مبلغ را پوشش داده - ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}. User: {UserName} (Id: {UserId})",
                        serviceAmount, primaryResult.InsuranceCoverage, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return new CombinedInsuranceCalculationResult
                    {
                        PatientId = patientId,
                        ServiceId = serviceId,
                        ServiceAmount = serviceAmount,
                        PrimaryInsuranceId = primaryInsuranceId,
                        PrimaryCoverage = primaryResult.InsuranceCoverage,
                        PrimaryCoveragePercent = primaryResult.CoveragePercent,
                        SupplementaryInsuranceId = null,
                        SupplementaryCoverage = 0,
                        SupplementaryCoveragePercent = 0,
                        FinalPatientShare = 0,
                        TotalInsuranceCoverage = primaryResult.InsuranceCoverage,
                        CalculationDate = calculationDate,
                        HasSupplementaryInsurance = true,
                        Notes = "بیمه اصلی کل مبلغ را پوشش داده است"
                    };
                }

                // محاسبه تدریجی بیمه‌های تکمیلی با استفاده از تعرفه‌ها
                foreach (var supplementaryInsurance in supplementaryInsurances)
                {
                    if (remainingAmount <= 0) break;

                    // پیدا کردن تعرفه بیمه تکمیلی مناسب
                    var supplementaryTariff = supplementaryTariffs
                        .Where(t => t.InsurancePlanId == supplementaryInsurance.InsurancePlanId)
                        .FirstOrDefault();

                    if (supplementaryTariff != null)
                    {
                        // 🚨 PROFESSIONAL FIX: محاسبه بر اساس تعرفه بیمه تکمیلی
                        // بیمه تکمیلی باید سهم بیمار از بیمه پایه را پوشش دهد (نه سهم بیمه پایه)
                        // remainingAmount = سهم بیمار از بیمه پایه
                        var supplementaryCoverage = 0m;
                        
                        // استفاده از درصد پوشش در تعرفه (اگر تعریف شده باشد)
                        if (supplementaryTariff.SupplementaryCoveragePercent.HasValue && 
                            supplementaryTariff.SupplementaryCoveragePercent.Value > 0)
                        {
                            var coveragePercent = supplementaryTariff.SupplementaryCoveragePercent.Value / 100m;
                            // محاسبه: درصد پوشش × سهم بیمار از بیمه پایه
                            supplementaryCoverage = remainingAmount * coveragePercent;
                            
                            _log.Information("🏥 MEDICAL: محاسبه بیمه تکمیلی بر اساس درصد تعرفه - PatientShareFromPrimary: {PatientShare} (سهم بیمار از بیمه پایه), CoveragePercent: {CoveragePercent}%, SupplementaryCoverage: {Coverage}. User: {UserName} (Id: {UserId})",
                                remainingAmount, supplementaryTariff.SupplementaryCoveragePercent.Value, supplementaryCoverage, _currentUserService.UserName, _currentUserService.UserId);
                        }
                        else
                        {
                            // Fallback: بیمه تکمیلی 100% سهم بیمار را پوشش می‌دهد (طبق استانداردهای پزشکی ایران)
                            supplementaryCoverage = remainingAmount; // 100% از سهم بیمار از بیمه پایه
                            
                            _log.Information("🏥 MEDICAL: محاسبه بیمه تکمیلی بدون درصد تعرفه (100% پیش‌فرض) - PatientShareFromPrimary: {PatientShare} (سهم بیمار از بیمه پایه), SupplementaryCoverage: {Coverage} (100% پوشش). User: {UserName} (Id: {UserId})",
                                remainingAmount, supplementaryCoverage, _currentUserService.UserName, _currentUserService.UserId);
                        }
                        
                        // اعمال سقف پرداخت اگر تعریف شده باشد
                        if (supplementaryTariff.SupplementaryMaxPayment.HasValue && 
                            supplementaryCoverage > supplementaryTariff.SupplementaryMaxPayment.Value)
                        {
                            _log.Information("🏥 MEDICAL: اعمال سقف پرداخت بیمه تکمیلی - Coverage: {Coverage}, MaxPayment: {MaxPayment}. User: {UserName} (Id: {UserId})",
                                supplementaryCoverage, supplementaryTariff.SupplementaryMaxPayment.Value, _currentUserService.UserName, _currentUserService.UserId);
                            
                            supplementaryCoverage = supplementaryTariff.SupplementaryMaxPayment.Value;
                        }

                        totalSupplementaryCoverage += supplementaryCoverage;
                        // به‌روزرسانی سهم باقی‌مانده بیمار (بعد از پوشش بیمه تکمیلی)
                        remainingAmount -= supplementaryCoverage;

                        supplementaryDetails.Add(new SupplementaryInsuranceDetail
                        {
                            InsuranceId = supplementaryInsurance.PatientInsuranceId,
                            Coverage = supplementaryCoverage,
                            Priority = supplementaryInsurance.Priority
                        });

                        _log.Information("🏥 MEDICAL: بیمه تکمیلی محاسبه شد - InsuranceId: {InsuranceId}, Priority: {Priority}, SupplementaryCoverage: {Coverage}, RemainingPatientShare: {RemainingAmount} (سهم باقی‌مانده بیمار), TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                            supplementaryInsurance.PatientInsuranceId, supplementaryInsurance.Priority, supplementaryCoverage, remainingAmount, supplementaryTariff.InsuranceTariffId, _currentUserService.UserName, _currentUserService.UserId);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: تعرفه بیمه تکمیلی یافت نشد - InsuranceId: {InsuranceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                            supplementaryInsurance.PatientInsuranceId, supplementaryInsurance.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                    }
                }

                // محاسبه درصد پوشش بیمه تکمیلی
                decimal supplementaryCoveragePercent = 0;
                if (serviceAmount > 0)
                {
                    supplementaryCoveragePercent = (totalSupplementaryCoverage / serviceAmount) * 100;
                }

                var finalResult = new CombinedInsuranceCalculationResult
                {
                    PatientId = patientId,
                    ServiceId = serviceId,
                    ServiceAmount = serviceAmount,
                    PrimaryInsuranceId = primaryInsuranceId,
                    PrimaryCoverage = primaryResult.InsuranceCoverage,
                    PrimaryCoveragePercent = primaryResult.CoveragePercent,
                    SupplementaryInsuranceId = supplementaryInsurances.FirstOrDefault()?.PatientInsuranceId,
                    SupplementaryCoverage = totalSupplementaryCoverage,
                    SupplementaryCoveragePercent = supplementaryCoveragePercent,
                    FinalPatientShare = remainingAmount, // سهم نهایی بیمار (بعد از پوشش بیمه تکمیلی)
                    TotalInsuranceCoverage = primaryResult.InsuranceCoverage + totalSupplementaryCoverage,
                    CalculationDate = calculationDate,
                    HasSupplementaryInsurance = true,
                    Notes = $"بیمه اصلی: {primaryResult.CoveragePercent:F1}%, بیمه تکمیلی: {supplementaryCoveragePercent:F1}% (تعداد: {supplementaryDetails.Count})"
                };

                _log.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی پیشرفته تکمیل شد - ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage} (پوشش بیمه پایه), TotalSupplementaryCoverage: {TotalSupplementaryCoverage} (پوشش بیمه تکمیلی از سهم بیمار), FinalPatientShare: {FinalPatientShare} (سهم نهایی بیمار), TotalCoverage: {TotalCoverage}. User: {UserName} (Id: {UserId})",
                    serviceAmount, primaryResult.InsuranceCoverage, totalSupplementaryCoverage, 
                    finalResult.FinalPatientShare, finalResult.TotalInsuranceCoverage, _currentUserService.UserName, _currentUserService.UserId);

                return finalResult;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه بیمه ترکیبی پیشرفته - ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}. User: {UserName} (Id: {UserId})",
                    serviceId, serviceAmount, _currentUserService.UserName, _currentUserService.UserId);
                throw;
            }
        }

        /// <summary>
        /// محاسبه بیمه ترکیبی (اصلی + تکمیلی) با استفاده از سرویس تخصصی بیمه تکمیلی
        /// </summary>
        private async Task<CombinedInsuranceCalculationResult> CalculateCombinedInsuranceAsync(
            InsuranceCalculationResultViewModel primaryResult,
            PatientInsurance supplementaryInsurance,
            int serviceId,
            decimal serviceAmount,
            DateTime calculationDate)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه بیمه ترکیبی پیشرفته - ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}. User: {UserName} (Id: {UserId})",
                    serviceId, serviceAmount, primaryResult.InsuranceCoverage, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از سرویس تخصصی بیمه تکمیلی برای محاسبه دقیق
                var supplementaryCalculationResult = await _supplementaryInsuranceService.CalculateSupplementaryInsuranceAsync(
                    primaryResult.PatientId, 
                    serviceId, 
                    serviceAmount, 
                    primaryResult.InsuranceCoverage, 
                    calculationDate);

                if (!supplementaryCalculationResult.Success)
                {
                    _log.Warning("🏥 MEDICAL: خطا در محاسبه بیمه تکمیلی - ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        serviceId, supplementaryCalculationResult.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    // در صورت خطا، فقط بیمه اصلی را در نظر می‌گیریم
                    return new CombinedInsuranceCalculationResult
                    {
                        PatientId = primaryResult.PatientId,
                        ServiceId = serviceId,
                        ServiceAmount = serviceAmount,
                        PrimaryInsuranceId = primaryResult.PatientId,
                        PrimaryCoverage = primaryResult.InsuranceCoverage,
                        PrimaryCoveragePercent = primaryResult.CoveragePercent,
                        SupplementaryInsuranceId = supplementaryInsurance.PatientInsuranceId,
                        SupplementaryCoverage = 0,
                        SupplementaryCoveragePercent = 0,
                        FinalPatientShare = primaryResult.PatientPayment,
                        TotalInsuranceCoverage = primaryResult.InsuranceCoverage,
                        CalculationDate = calculationDate,
                        HasSupplementaryInsurance = true,
                        Notes = $"خطا در محاسبه بیمه تکمیلی: {supplementaryCalculationResult.Message}"
                    };
                }

                var supplementaryResult = supplementaryCalculationResult.Data;

                // بررسی اینکه آیا بیمه اصلی کل مبلغ را پوشش داده یا نه
                if (primaryResult.InsuranceCoverage >= serviceAmount)
                {
                    _log.Information("🏥 MEDICAL: بیمه اصلی کل مبلغ را پوشش داده - ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}. User: {UserName} (Id: {UserId})",
                        serviceAmount, primaryResult.InsuranceCoverage, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return new CombinedInsuranceCalculationResult
                    {
                        PatientId = primaryResult.PatientId,
                        ServiceId = serviceId,
                        ServiceAmount = serviceAmount,
                        PrimaryInsuranceId = primaryResult.PatientId,
                        PrimaryCoverage = primaryResult.InsuranceCoverage,
                        PrimaryCoveragePercent = primaryResult.CoveragePercent,
                        SupplementaryInsuranceId = supplementaryInsurance.PatientInsuranceId,
                        SupplementaryCoverage = 0,
                        SupplementaryCoveragePercent = 0,
                        FinalPatientShare = 0,
                        TotalInsuranceCoverage = primaryResult.InsuranceCoverage,
                        CalculationDate = calculationDate,
                        HasSupplementaryInsurance = true,
                        Notes = "بیمه اصلی کل مبلغ را پوشش داده است"
                    };
                }

                // محاسبه درصد پوشش بیمه تکمیلی
                decimal supplementaryCoveragePercent = 0;
                if (serviceAmount > 0)
                {
                    supplementaryCoveragePercent = (supplementaryResult.SupplementaryCoverage / serviceAmount) * 100;
                }

                var result = new CombinedInsuranceCalculationResult
                {
                    PatientId = primaryResult.PatientId,
                    ServiceId = serviceId,
                    ServiceAmount = serviceAmount,
                    PrimaryInsuranceId = primaryResult.PatientId,
                    PrimaryCoverage = primaryResult.InsuranceCoverage,
                    PrimaryCoveragePercent = primaryResult.CoveragePercent,
                    SupplementaryInsuranceId = supplementaryInsurance.PatientInsuranceId,
                    SupplementaryCoverage = supplementaryResult.SupplementaryCoverage,
                    SupplementaryCoveragePercent = supplementaryCoveragePercent,
                    FinalPatientShare = supplementaryResult.FinalPatientShare,
                    TotalInsuranceCoverage = supplementaryResult.TotalCoverage,
                    CalculationDate = calculationDate,
                    HasSupplementaryInsurance = true,
                    Notes = supplementaryResult.Notes ?? $"بیمه اصلی: {primaryResult.CoveragePercent:F1}%, بیمه تکمیلی: {supplementaryCoveragePercent:F1}%"
                };

                _log.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی پیشرفته تکمیل شد - ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}, SupplementaryCoverage: {SupplementaryCoverage}, FinalPatientShare: {FinalPatientShare}, TotalCoverage: {TotalCoverage}. User: {UserName} (Id: {UserId})",
                    serviceAmount, primaryResult.InsuranceCoverage, supplementaryResult.SupplementaryCoverage, 
                    supplementaryResult.FinalPatientShare, supplementaryResult.TotalCoverage, _currentUserService.UserName, _currentUserService.UserId);

                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه بیمه ترکیبی پیشرفته - ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}. User: {UserName} (Id: {UserId})",
                    serviceId, serviceAmount, _currentUserService.UserName, _currentUserService.UserId);
                throw;
            }
        }

        /// <summary>
        /// اعمال تنظیمات خاص
        /// </summary>
        private CombinedInsuranceCalculationResult ApplyCustomSettings(
            CombinedInsuranceCalculationResult result, 
            Dictionary<string, object> customSettings)
        {
            try
            {
                // اعمال تخفیف خاص
                if (customSettings.ContainsKey("discountPercent") && 
                    decimal.TryParse(customSettings["discountPercent"].ToString(), out decimal discountPercent))
                {
                    var discountAmount = result.ServiceAmount * (discountPercent / 100);
                    result.ServiceAmount -= discountAmount;
                    result.FinalPatientShare = Math.Max(0, result.FinalPatientShare - discountAmount);
                    result.Notes += $" | تخفیف {discountPercent}% اعمال شد";
                }

                // اعمال سقف پرداخت خاص
                if (customSettings.ContainsKey("maxPatientPayment") && 
                    decimal.TryParse(customSettings["maxPatientPayment"].ToString(), out decimal maxPatientPayment))
                {
                    if (result.FinalPatientShare > maxPatientPayment)
                    {
                        var reduction = result.FinalPatientShare - maxPatientPayment;
                        result.FinalPatientShare = maxPatientPayment;
                        result.TotalInsuranceCoverage += reduction;
                        result.Notes += $" | سقف پرداخت بیمار: {maxPatientPayment:N0} ریال";
                    }
                }

                // اعمال فرانشیز خاص
                if (customSettings.ContainsKey("deductible") && 
                    decimal.TryParse(customSettings["deductible"].ToString(), out decimal deductible))
                {
                    result.FinalPatientShare += deductible;
                    result.Notes += $" | فرانشیز: {deductible:N0} ریال";
                }

                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در اعمال تنظیمات خاص");
                return result;
            }
        }

        /// <summary>
        /// اضافه کردن آمار و تحلیل
        /// </summary>
        private async Task<CombinedInsuranceCalculationResult> AddCalculationAnalytics(
            CombinedInsuranceCalculationResult result, 
            DateTime calculationDate)
        {
            try
            {
                // محاسبه درصد پوشش کل
                var totalCoveragePercent = result.ServiceAmount > 0 ? 
                    (result.TotalInsuranceCoverage / result.ServiceAmount) * 100 : 0;

                // محاسبه درصد سهم بیمار
                var patientSharePercent = result.ServiceAmount > 0 ? 
                    (result.FinalPatientShare / result.ServiceAmount) * 100 : 0;

                // محاسبه صرفه‌جویی بیمار
                var patientSavings = result.ServiceAmount - result.FinalPatientShare;

                // اضافه کردن آمار به یادداشت‌ها
                var analytics = $" | پوشش کل: {totalCoveragePercent:F1}% | سهم بیمار: {patientSharePercent:F1}% | صرفه‌جویی: {patientSavings:N0} ریال";
                result.Notes += analytics;

                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در محاسبه آمار و تحلیل");
                return result;
            }
        }

        /// <summary>
        /// محاسبه بیمه جایگزین
        /// </summary>
        private async Task<ServiceResult<CombinedInsuranceCalculationResult>> CalculateAlternativeInsuranceAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime calculationDate, 
            int alternativePlanId)
        {
            try
            {
                // این متد می‌تواند برای محاسبه با طرح‌های بیمه مختلف استفاده شود
                // فعلاً همان محاسبه استاندارد را برمی‌گرداند
                var result = await CalculateCombinedInsuranceAsync(patientId, serviceId, serviceAmount, calculationDate);
                
                if (result.Success)
                {
                    result.Data.Notes = $"طرح بیمه جایگزین - PlanId: {alternativePlanId}";
                }

                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در محاسبه بیمه جایگزین - PlanId: {PlanId}", alternativePlanId);
                return ServiceResult<CombinedInsuranceCalculationResult>.Failed("خطا در محاسبه بیمه جایگزین");
            }
        }

        /// <summary>
        /// دریافت لیست بیماران فعال برای محاسبه بیمه
        /// </summary>
        public async Task<ServiceResult<List<PatientLookupItem>>> GetActivePatientsAsync()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست لیست بیماران فعال. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // دریافت بیماران فعال از Database
                var result = await _patientService.GetActivePatientsForLookupAsync();
                
                if (result.Success)
                {
                    var patients = result.Data.Select(p => new PatientLookupItem
                    {
                        Id = p.PatientId,
                        Name = p.FullName,
                        NationalId = p.NationalCode,
                        PhoneNumber = p.PhoneNumber,
                        IsActive = !p.IsDeleted
                    }).ToList();

                    _log.Information("🏥 MEDICAL: لیست بیماران فعال از Database دریافت شد - Count: {Count}. User: {UserName} (Id: {UserId})",
                        patients.Count, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<List<PatientLookupItem>>.Successful(patients, $"لیست بیماران فعال ({patients.Count} مورد) دریافت شد");
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت لیست بیماران از Database - Error: {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<List<PatientLookupItem>>.Failed(result.Message);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت لیست بیماران فعال. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<PatientLookupItem>>.Failed("خطا در دریافت لیست بیماران فعال");
            }
        }

        /// <summary>
        /// دریافت لیست خدمات فعال برای محاسبه بیمه
        /// </summary>
        public async Task<ServiceResult<List<ServiceLookupItem>>> GetActiveServicesAsync()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست لیست خدمات فعال. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // دریافت خدمات فعال از Database
                var result = await _serviceRepository.GetActiveServicesForLookupAsync();
                
                if (result.Success)
                {
                    var services = result.Data.Select(s => new ServiceLookupItem
                    {
                        Id = s.ServiceId,
                        Name = s.Title,
                        ServiceCode = s.ServiceCode,
                        Category = s.ServiceCategory?.Title ?? "نامشخص",
                        BasePrice = s.Price,
                        IsActive = !s.IsDeleted
                    }).ToList();

                    _log.Information("🏥 MEDICAL: لیست خدمات فعال از Database دریافت شد - Count: {Count}. User: {UserName} (Id: {UserId})",
                        services.Count, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<List<ServiceLookupItem>>.Successful(services, $"لیست خدمات فعال ({services.Count} مورد) دریافت شد");
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت لیست خدمات از Database - Error: {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<List<ServiceLookupItem>>.Failed(result.Message);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت لیست خدمات فعال. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<ServiceLookupItem>>.Failed("خطا در دریافت لیست خدمات فعال");
            }
        }

        /// <summary>
        /// محاسبه قیمت خدمت با استفاده از FactorSetting
        /// </summary>
        private async Task<decimal> CalculateServicePriceWithFactorSettingAsync(ClinicApp.Models.Entities.Clinic.Service service, int currentFinancialYear)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه قیمت خدمت با FactorSetting - ServiceId: {ServiceId}, FinancialYear: {FinancialYear}. User: {UserName} (Id: {UserId})",
                    service.ServiceId, currentFinancialYear, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت کای فنی
                var technicalFactor = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(
                    ClinicApp.Models.Enums.ServiceComponentType.Technical, service.IsHashtagged, currentFinancialYear);

                // دریافت کای حرفه‌ای
                // ✅ FIX: ضریب حرفه‌ای بر اساس نوع خدمت (هشتگ‌دار: 770k، عادی: 1.37M)
                var professionalFactor = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(
                    ClinicApp.Models.Enums.ServiceComponentType.Professional, service.IsHashtagged, currentFinancialYear); // ✅ بر اساس نوع خدمت

                if (technicalFactor == null || professionalFactor == null)
                {
                    _log.Warning("🏥 MEDICAL: کای‌های مورد نیاز یافت نشد - TechnicalFactor: {TechnicalFactor}, ProfessionalFactor: {ProfessionalFactor}. User: {UserName} (Id: {UserId})",
                        technicalFactor != null, professionalFactor != null, _currentUserService.UserName, _currentUserService.UserId);
                    
                    // Fallback به قیمت ثابت
                    return service.Price;
                }

                // محاسبه قیمت بر اساس اجزای خدمت
                decimal calculatedPrice = 0m;

                if (service.ServiceComponents != null && service.ServiceComponents.Any())
                {
                    foreach (var component in service.ServiceComponents)
                    {
                        if (component.ComponentType == ClinicApp.Models.Enums.ServiceComponentType.Technical)
                        {
                            calculatedPrice += component.Coefficient * technicalFactor.Value;
                        }
                        else if (component.ComponentType == ClinicApp.Models.Enums.ServiceComponentType.Professional)
                        {
                            calculatedPrice += component.Coefficient * professionalFactor.Value;
                        }
                    }
                }
                else
                {
                    // اگر اجزای خدمت وجود نداشت، از قیمت ثابت استفاده کن
                    calculatedPrice = service.Price;
                }

                _log.Information("🏥 MEDICAL: محاسبه قیمت خدمت با FactorSetting تکمیل شد - ServiceId: {ServiceId}, CalculatedPrice: {CalculatedPrice}. User: {UserName} (Id: {UserId})",
                    service.ServiceId, calculatedPrice, _currentUserService.UserName, _currentUserService.UserId);

                return calculatedPrice;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه قیمت خدمت با FactorSetting - ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    service.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                
                // Fallback به قیمت ثابت
                return service.Price;
            }
        }

        /// <summary>
        /// محاسبه بیمه بیمار برای پذیرش (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه محاسبه بیمه</returns>
        public async Task<ServiceResult<object>> CalculatePatientInsuranceForReceptionAsync(int patientId, System.Collections.Generic.List<int> serviceIds, System.DateTime receptionDate)
        {
            // TODO: پیاده‌سازی منطق محاسبه بیمه برای پذیرش
            return ServiceResult<object>.Successful(new { PatientId = patientId, ServiceIds = serviceIds, ReceptionDate = receptionDate, TotalAmount = 0m });
        }

        #endregion
    }
}
