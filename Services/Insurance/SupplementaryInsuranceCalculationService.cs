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
using ClinicApp.ViewModels.Insurance.Supplementary;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس محاسبه بیمه تکمیلی - طراحی شده برای محیط درمانی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. محاسبه دقیق سهم بیمه تکمیلی بر اساس باقی‌مانده بیمه اصلی
    /// 2. پشتیبانی از تنظیمات پیچیده بیمه تکمیلی
    /// 3. رعایت سقف‌های پرداخت و فرانشیز
    /// 4. محاسبه نهایی سهم بیمار
    /// </summary>
    public class SupplementaryInsuranceCalculationService : ISupplementaryInsuranceCalculationService
    {
        private readonly IInsuranceTariffRepository _tariffRepository;
        private readonly IPatientInsuranceRepository _patientInsuranceRepository;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public SupplementaryInsuranceCalculationService(
            IInsuranceTariffRepository tariffRepository,
            IPatientInsuranceRepository patientInsuranceRepository,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _tariffRepository = tariffRepository ?? throw new ArgumentNullException(nameof(tariffRepository));
            _patientInsuranceRepository = patientInsuranceRepository ?? throw new ArgumentNullException(nameof(patientInsuranceRepository));
            _log = logger.ForContext<SupplementaryInsuranceCalculationService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// محاسبه بیمه تکمیلی بر اساس باقی‌مانده بیمه اصلی
        /// </summary>
        public async Task<ServiceResult<SupplementaryInsuranceCalculationResult>> CalculateSupplementaryInsuranceAsync(
            int patientId,
            int serviceId,
            decimal serviceAmount,
            decimal primaryInsuranceCoverage,
            DateTime calculationDate)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}, Date: {Date}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, primaryInsuranceCoverage, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت بیمه تکمیلی بیمار - استفاده از متد موجود
                var supplementaryInsurance = await _patientInsuranceRepository.GetByPatientIdAsync(patientId);
                var supplementary = supplementaryInsurance?.FirstOrDefault(pi => !pi.IsPrimary);
                if (supplementary == null)
                {
                    _log.Warning("🏥 MEDICAL: بیمه تکمیلی برای بیمار یافت نشد - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<SupplementaryInsuranceCalculationResult>.Failed("بیمه تکمیلی برای این بیمار تعریف نشده است");
                }

                // دریافت تعرفه بیمه تکمیلی - استفاده از متد موجود
                var tariffResult = await _tariffRepository.GetByServiceIdAsync(serviceId);

                if (tariffResult == null || !tariffResult.Any())
                {
                    _log.Warning("🏥 MEDICAL: تعرفه بیمه تکمیلی یافت نشد - ServiceId: {ServiceId}, PlanId: {PlanId}, Date: {Date}. User: {UserName} (Id: {UserId})",
                        serviceId, supplementary.InsurancePlanId, calculationDate, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<SupplementaryInsuranceCalculationResult>.Failed("تعرفه بیمه تکمیلی برای این خدمت تعریف نشده است");
                }

                var tariff = tariffResult.FirstOrDefault();

                // محاسبه باقی‌مانده پس از بیمه اصلی
                var remainingAmount = serviceAmount - primaryInsuranceCoverage;
                if (remainingAmount <= 0)
                {
                    _log.Information("🏥 MEDICAL: بیمه اصلی کل مبلغ را پوشش می‌دهد - ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}. User: {UserName} (Id: {UserId})",
                        serviceId, serviceAmount, primaryInsuranceCoverage, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<SupplementaryInsuranceCalculationResult>.Successful(new SupplementaryInsuranceCalculationResult
                    {
                        PatientId = patientId,
                        ServiceId = serviceId,
                        ServiceAmount = serviceAmount,
                        PrimaryInsuranceCoverage = primaryInsuranceCoverage,
                        SupplementaryInsuranceCoverage = 0,
                        FinalPatientShare = 0,
                        CalculationDate = calculationDate,
                        IsFullyCovered = true
                    });
                }

                // محاسبه پوشش بیمه تکمیلی
                var supplementaryCoverage = CalculateSupplementaryCoverage(remainingAmount, tariff);

                // محاسبه سهم نهایی بیمار
                var finalPatientShare = serviceAmount - primaryInsuranceCoverage - supplementaryCoverage;

                var result = new SupplementaryInsuranceCalculationResult
                {
                    PatientId = patientId,
                    ServiceId = serviceId,
                    ServiceAmount = serviceAmount,
                    PrimaryInsuranceCoverage = primaryInsuranceCoverage,
                    SupplementaryInsuranceCoverage = supplementaryCoverage,
                    FinalPatientShare = Math.Max(0, finalPatientShare),
                    CalculationDate = calculationDate,
                    IsFullyCovered = finalPatientShare <= 0,
                    TariffId = tariff.InsuranceTariffId,
                    SupplementaryPlanId = supplementary.InsurancePlanId
                };

                _log.Information("🏥 MEDICAL: محاسبه بیمه تکمیلی تکمیل شد - ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}, SupplementaryCoverage: {SupplementaryCoverage}, FinalPatientShare: {FinalPatientShare}. User: {UserName} (Id: {UserId})",
                    serviceId, serviceAmount, primaryInsuranceCoverage, supplementaryCoverage, finalPatientShare, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<SupplementaryInsuranceCalculationResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<SupplementaryInsuranceCalculationResult>.Failed("خطا در محاسبه بیمه تکمیلی");
            }
        }

        /// <summary>
        /// محاسبه پوشش بیمه تکمیلی بر اساس تعرفه
        /// </summary>
        private decimal CalculateSupplementaryCoverage(decimal remainingAmount, InsuranceTariff tariff)
        {
            try
            {
                decimal coverage = 0;

                // استفاده از درصد پوشش بیمه تکمیلی
                if (tariff.SupplementaryCoveragePercent.HasValue)
                {
                    coverage = remainingAmount * (tariff.SupplementaryCoveragePercent.Value / 100);
                    _log.Debug("🏥 MEDICAL: محاسبه بر اساس درصد پوشش - RemainingAmount: {RemainingAmount}, CoveragePercent: {CoveragePercent}, Coverage: {Coverage}",
                        remainingAmount, tariff.SupplementaryCoveragePercent.Value, coverage);
                }
                else
                {
                    // استفاده از درصد پوشش استاندارد (90%)
                    coverage = remainingAmount * 0.9m;
                    _log.Debug("🏥 MEDICAL: محاسبه بر اساس درصد استاندارد - RemainingAmount: {RemainingAmount}, Coverage: {Coverage}",
                        remainingAmount, coverage);
                }

                // اعمال سقف پرداخت بیمه تکمیلی
                if (tariff.SupplementaryMaxPayment.HasValue && coverage > tariff.SupplementaryMaxPayment.Value)
                {
                    _log.Debug("🏥 MEDICAL: اعمال سقف پرداخت - Coverage: {Coverage}, MaxPayment: {MaxPayment}",
                        coverage, tariff.SupplementaryMaxPayment.Value);
                    coverage = tariff.SupplementaryMaxPayment.Value;
                }

                // اطمینان از عدم منفی بودن
                coverage = Math.Max(0, coverage);

                _log.Debug("🏥 MEDICAL: پوشش بیمه تکمیلی محاسبه شد - RemainingAmount: {RemainingAmount}, FinalCoverage: {FinalCoverage}",
                    remainingAmount, coverage);

                return coverage;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه پوشش بیمه تکمیلی - RemainingAmount: {RemainingAmount}, TariffId: {TariffId}",
                    remainingAmount, tariff?.InsuranceTariffId);
                return 0;
            }
        }

        /// <summary>
        /// محاسبه بیمه تکمیلی برای چندین خدمت
        /// </summary>
        public async Task<ServiceResult<List<SupplementaryInsuranceCalculationResult>>> CalculateSupplementaryInsuranceForServicesAsync(
            int patientId,
            List<int> serviceIds,
            List<decimal> serviceAmounts,
            DateTime calculationDate)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه بیمه تکمیلی برای چندین خدمت - PatientId: {PatientId}, ServiceCount: {ServiceCount}, Date: {Date}. User: {UserName} (Id: {UserId})",
                    patientId, serviceIds.Count, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                var results = new List<SupplementaryInsuranceCalculationResult>();

                for (int i = 0; i < serviceIds.Count; i++)
                {
                    var serviceId = serviceIds[i];
                    var serviceAmount = serviceAmounts[i];

                    // محاسبه بیمه اصلی برای این خدمت
                    var primaryCalculationResult = await CalculatePrimaryInsuranceForServiceAsync(patientId, serviceId, serviceAmount, calculationDate);
                    if (!primaryCalculationResult.Success)
                    {
                        _log.Warning("🏥 MEDICAL: خطا در محاسبه بیمه اصلی برای خدمت - ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                            serviceId, primaryCalculationResult.Message, _currentUserService.UserName, _currentUserService.UserId);
                        continue;
                    }

                    // محاسبه بیمه تکمیلی
                    var supplementaryResult = await CalculateSupplementaryInsuranceAsync(
                        patientId, 
                        serviceId, 
                        serviceAmount, 
                        primaryCalculationResult.Data.InsuranceCoverage, 
                        calculationDate);

                    if (supplementaryResult.Success)
                    {
                        results.Add(supplementaryResult.Data);
                    }
                }

                _log.Information("🏥 MEDICAL: محاسبه بیمه تکمیلی برای چندین خدمت تکمیل شد - PatientId: {PatientId}, SuccessCount: {SuccessCount}, TotalCount: {TotalCount}. User: {UserName} (Id: {UserId})",
                    patientId, results.Count, serviceIds.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<SupplementaryInsuranceCalculationResult>>.Successful(results);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه بیمه تکمیلی برای چندین خدمت - PatientId: {PatientId}, ServiceCount: {ServiceCount}. User: {UserName} (Id: {UserId})",
                    patientId, serviceIds.Count, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<List<SupplementaryInsuranceCalculationResult>>.Failed("خطا در محاسبه بیمه تکمیلی برای چندین خدمت");
            }
        }

        /// <summary>
        /// محاسبه بیمه اصلی برای خدمت (برای استفاده در محاسبات ترکیبی)
        /// </summary>
        private async Task<ServiceResult<InsuranceCalculationResultViewModel>> CalculatePrimaryInsuranceForServiceAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime calculationDate)
        {
            try
            {
                // دریافت بیمه اصلی بیمار
                var primaryInsurance = await _patientInsuranceRepository.GetPrimaryInsuranceByPatientIdAsync(patientId);
                if (primaryInsurance == null)
                {
                    return ServiceResult<InsuranceCalculationResultViewModel>.Failed("بیمه اصلی برای این بیمار تعریف نشده است");
                }

                // دریافت تعرفه بیمه اصلی - استفاده از متد موجود
                var tariffResult = await _tariffRepository.GetByServiceIdAsync(serviceId);
                if (tariffResult == null || !tariffResult.Any())
                {
                    return ServiceResult<InsuranceCalculationResultViewModel>.Failed("تعرفه بیمه اصلی برای این خدمت تعریف نشده است");
                }

                var tariff = tariffResult.FirstOrDefault();
                var insuranceCoverage = serviceAmount * (tariff.InsurerShare ?? 0 / tariff.TariffPrice ?? 1);

                return ServiceResult<InsuranceCalculationResultViewModel>.Successful(new InsuranceCalculationResultViewModel
                {
                    PatientId = patientId,
                    ServiceId = serviceId,
                    TotalAmount = serviceAmount,
                    InsuranceCoverage = insuranceCoverage,
                    PatientShare = serviceAmount - insuranceCoverage,
                    CalculationDate = calculationDate
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه بیمه اصلی - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<InsuranceCalculationResultViewModel>.Failed("خطا در محاسبه بیمه اصلی");
            }
        }
    }
}
