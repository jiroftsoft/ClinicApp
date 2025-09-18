using System;
using System.Collections.Generic;
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
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public CombinedInsuranceCalculationService(
            IPatientInsuranceRepository patientInsuranceRepository,
            IInsuranceCalculationService insuranceCalculationService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _patientInsuranceRepository = patientInsuranceRepository ?? throw new ArgumentNullException(nameof(patientInsuranceRepository));
            _insuranceCalculationService = insuranceCalculationService ?? throw new ArgumentNullException(nameof(insuranceCalculationService));
            _log = logger.ForContext<CombinedInsuranceCalculationService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region ICombinedInsuranceCalculationService Implementation

        /// <summary>
        /// محاسبه ترکیبی بیمه اصلی و تکمیلی برای یک خدمت
        /// </summary>
        public async Task<ServiceResult<CombinedInsuranceCalculationResult>> CalculateCombinedInsuranceAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime calculationDate)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}, Amount: {Amount}, Date: {Date}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی ورودی‌ها
                var validationResult = ValidateInputs(patientId, serviceId, serviceAmount, calculationDate);
                if (!validationResult.Success)
                {
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

                // بررسی وجود بیمه تکمیلی
                var supplementaryInsurance = patientInsurances.FirstOrDefault(pi => !pi.IsPrimary && pi.IsActive);
                
                CombinedInsuranceCalculationResult finalResult;

                if (supplementaryInsurance != null)
                {
                    // محاسبه بیمه ترکیبی (اصلی + تکمیلی)
                    finalResult = await CalculateCombinedInsuranceAsync(
                        primaryResult, supplementaryInsurance, serviceId, serviceAmount, calculationDate);
                    
                    _log.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی تکمیل شد - PrimaryCoverage: {PrimaryCoverage}, SupplementaryCoverage: {SupplementaryCoverage}, FinalPatientShare: {FinalPatientShare}. User: {UserName} (Id: {UserId})",
                        primaryResult.InsuranceCoverage, finalResult.SupplementaryCoverage, finalResult.FinalPatientShare, _currentUserService.UserName, _currentUserService.UserId);
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

        #endregion

        #region Private Methods

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
        /// دریافت بیمه‌های بیمار
        /// </summary>
        private async Task<ServiceResult<List<PatientInsurance>>> GetPatientInsurancesAsync(int patientId)
        {
            try
            {
                var result = await _patientInsuranceRepository.GetActiveByPatientAsync(patientId);
                if (!result.Success)
                {
                    return ServiceResult<List<PatientInsurance>>.Failed("خطا در دریافت بیمه‌های بیمار");
                }

                var patientInsurances = new List<PatientInsurance>();
                
                // بیمه اصلی
                if (result.Data != null)
                {
                    patientInsurances.Add(result.Data);
                }

                // بیمه‌های تکمیلی
                var supplementaryResult = await _patientInsuranceRepository.GetSupplementaryByPatientIdAsync(patientId);
                if (supplementaryResult != null && supplementaryResult.Any())
                {
                    patientInsurances.AddRange(supplementaryResult);
                }

                return ServiceResult<List<PatientInsurance>>.Successful(patientInsurances);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت بیمه‌های بیمار - PatientId: {PatientId}", patientId);
                return ServiceResult<List<PatientInsurance>>.Failed("خطا در دریافت بیمه‌های بیمار");
            }
        }

        /// <summary>
        /// محاسبه بیمه اصلی
        /// </summary>
        private async Task<ServiceResult<InsuranceCalculationResultViewModel>> CalculatePrimaryInsuranceAsync(
            PatientInsurance primaryInsurance, int serviceId, decimal serviceAmount, DateTime calculationDate)
        {
            try
            {
                var result = await _insuranceCalculationService.CalculatePatientShareAsync(
                    primaryInsurance.PatientId, serviceId, calculationDate);

                if (!result.Success)
                {
                    return ServiceResult<InsuranceCalculationResultViewModel>.Failed(result.Message);
                }

                return ServiceResult<InsuranceCalculationResultViewModel>.Successful(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در محاسبه بیمه اصلی - PatientInsuranceId: {PatientInsuranceId}, ServiceId: {ServiceId}",
                    primaryInsurance.PatientInsuranceId, serviceId);
                return ServiceResult<InsuranceCalculationResultViewModel>.Failed("خطا در محاسبه بیمه اصلی");
            }
        }

        /// <summary>
        /// محاسبه بیمه ترکیبی (اصلی + تکمیلی)
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
                // محاسبه بیمه تکمیلی بر اساس باقی‌مانده از بیمه اصلی
                decimal remainingAmount = serviceAmount - primaryResult.InsuranceCoverage;
                
                if (remainingAmount <= 0)
                {
                    // بیمه اصلی کل مبلغ را پوشش داده
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

                // محاسبه پوشش بیمه تکمیلی
                var supplementaryResult = await _insuranceCalculationService.CalculatePatientShareAsync(
                    supplementaryInsurance.PatientId, serviceId, calculationDate);

                decimal supplementaryCoverage = 0;
                decimal supplementaryCoveragePercent = 0;

                if (supplementaryResult.Success)
                {
                    supplementaryCoverage = supplementaryResult.Data.InsuranceCoverage;
                    supplementaryCoveragePercent = supplementaryResult.Data.CoveragePercent;
                }

                // محاسبه سهم نهایی بیمار
                decimal finalPatientShare = serviceAmount - primaryResult.InsuranceCoverage - supplementaryCoverage;

                var result = new CombinedInsuranceCalculationResult
                {
                    PatientId = primaryResult.PatientId,
                    ServiceId = serviceId,
                    ServiceAmount = serviceAmount,
                    PrimaryInsuranceId = primaryResult.PatientId,
                    PrimaryCoverage = primaryResult.InsuranceCoverage,
                    PrimaryCoveragePercent = primaryResult.CoveragePercent,
                    SupplementaryInsuranceId = supplementaryInsurance.PatientInsuranceId,
                    SupplementaryCoverage = supplementaryCoverage,
                    SupplementaryCoveragePercent = supplementaryCoveragePercent,
                    FinalPatientShare = finalPatientShare,
                    TotalInsuranceCoverage = primaryResult.InsuranceCoverage + supplementaryCoverage,
                    CalculationDate = calculationDate,
                    HasSupplementaryInsurance = true,
                    Notes = $"بیمه اصلی: {primaryResult.CoveragePercent:F1}%, بیمه تکمیلی: {supplementaryCoveragePercent:F1}%"
                };

                _log.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی - ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}, SupplementaryCoverage: {SupplementaryCoverage}, FinalPatientShare: {FinalPatientShare}",
                    serviceAmount, primaryResult.InsuranceCoverage, supplementaryCoverage, finalPatientShare);

                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در محاسبه بیمه ترکیبی - ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}",
                    serviceId, serviceAmount);
                throw;
            }
        }

        #endregion
    }
}
