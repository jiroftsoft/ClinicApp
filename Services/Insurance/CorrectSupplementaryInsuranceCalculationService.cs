using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces.Repositories;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.ViewModels.Insurance.Supplementary;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس محاسبه صحیح بیمه تکمیلی - نسخه اصلاح شده
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. محاسبه صحیح بر اساس سهم بیمار (نه مبلغ باقی‌مانده)
    /// 2. منطق اقتصادی صحیح برای محیط درمانی
    /// 3. پشتیبانی از سناریوهای مختلف بیمه تکمیلی
    /// 4. رعایت استانداردهای مالی ایران
    /// </summary>
    public class CorrectSupplementaryInsuranceCalculationService : ISupplementaryInsuranceCalculationService
    {
        private readonly IPatientInsuranceRepository _patientInsuranceRepository;
        private readonly IInsuranceTariffRepository _tariffRepository;
        private readonly IInsuranceCalculationService _insuranceCalculationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _log;

        public CorrectSupplementaryInsuranceCalculationService(
            IPatientInsuranceRepository patientInsuranceRepository,
            IInsuranceTariffRepository tariffRepository,
            IInsuranceCalculationService insuranceCalculationService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _patientInsuranceRepository = patientInsuranceRepository ?? throw new ArgumentNullException(nameof(patientInsuranceRepository));
            _tariffRepository = tariffRepository ?? throw new ArgumentNullException(nameof(tariffRepository));
            _insuranceCalculationService = insuranceCalculationService ?? throw new ArgumentNullException(nameof(insuranceCalculationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _log = logger.ForContext<CorrectSupplementaryInsuranceCalculationService>();
        }

        /// <summary>
        /// محاسبه صحیح بیمه تکمیلی بر اساس سهم بیمار
        /// </summary>
        public async Task<ServiceResult<SupplementaryInsuranceCalculationResult>> CalculateSupplementaryInsuranceAsync(
            int patientId,
            int serviceId,
            decimal serviceAmount,
            decimal primaryCoverage,
            DateTime calculationDate)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه صحیح بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, primaryCoverage, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت بیمه تکمیلی بیمار
                var supplementaryInsurance = await _patientInsuranceRepository.GetSupplementaryByPatientIdAsync(patientId);
                if (supplementaryInsurance == null || !supplementaryInsurance.Any())
                {
                    _log.Warning("🏥 MEDICAL: بیمه تکمیلی یافت نشد - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<SupplementaryInsuranceCalculationResult>.Failed("بیمه تکمیلی فعال یافت نشد");
                }

                var activeSupplementary = supplementaryInsurance.FirstOrDefault(pi => pi.IsActive && 
                    (pi.EndDate == null || pi.EndDate > calculationDate));

                if (activeSupplementary == null)
                {
                    _log.Warning("🏥 MEDICAL: بیمه تکمیلی فعال یافت نشد - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<SupplementaryInsuranceCalculationResult>.Failed("بیمه تکمیلی فعال یافت نشد");
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
                    
                    return ServiceResult<SupplementaryInsuranceCalculationResult>.Failed("تعرفه بیمه تکمیلی برای این خدمت تعریف نشده است");
                }

                // 🔧 CRITICAL FIX: محاسبه صحیح سهم بیمار از بیمه اصلی
                decimal patientShareFromPrimary = serviceAmount - primaryCoverage;
                
                if (patientShareFromPrimary <= 0)
                {
                    _log.Information("🏥 MEDICAL: سهم بیمار صفر یا منفی - ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}, PatientShare: {PatientShare}. User: {UserName} (Id: {UserId})",
                        serviceAmount, primaryCoverage, patientShareFromPrimary, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<SupplementaryInsuranceCalculationResult>.Successful(new SupplementaryInsuranceCalculationResult
                    {
                        PatientId = patientId,
                        ServiceId = serviceId,
                        ServiceAmount = serviceAmount,
                        PrimaryInsuranceCoverage = primaryCoverage,
                        SupplementaryInsuranceCoverage = 0,
                        FinalPatientShare = patientShareFromPrimary,
                        CalculationDate = calculationDate
                    });
                }

                // 🔧 CRITICAL FIX: محاسبه صحیح پوشش بیمه تکمیلی بر اساس سهم بیمار
                decimal supplementaryCoverage = 0;
                
                if (tariff.SupplementaryCoveragePercent.HasValue)
                {
                    // محاسبه بر اساس درصدی از سهم بیمار (نه مبلغ باقی‌مانده)
                    supplementaryCoverage = patientShareFromPrimary * (tariff.SupplementaryCoveragePercent.Value / 100m);
                    
                    _log.Debug("🏥 MEDICAL: محاسبه پوشش تکمیلی - PatientShare: {PatientShare}, CoveragePercent: {CoveragePercent}, SupplementaryCoverage: {SupplementaryCoverage}",
                        patientShareFromPrimary, tariff.SupplementaryCoveragePercent.Value, supplementaryCoverage);
                }

                // اعمال سقف پرداخت بیمه تکمیلی
                if (tariff.SupplementaryMaxPayment.HasValue && supplementaryCoverage > tariff.SupplementaryMaxPayment.Value)
                {
                    _log.Debug("🏥 MEDICAL: اعمال سقف پرداخت - SupplementaryCoverage: {SupplementaryCoverage}, MaxPayment: {MaxPayment}",
                        supplementaryCoverage, tariff.SupplementaryMaxPayment.Value);
                    supplementaryCoverage = tariff.SupplementaryMaxPayment.Value;
                }

                // 🔧 CRITICAL FIX: محاسبه صحیح سهم نهایی بیمار
                decimal finalPatientShare = Math.Max(0, patientShareFromPrimary - supplementaryCoverage);

                var result = new SupplementaryInsuranceCalculationResult
                {
                    PatientId = patientId,
                    ServiceId = serviceId,
                    ServiceAmount = serviceAmount,
                    PrimaryInsuranceCoverage = primaryCoverage,
                    SupplementaryInsuranceCoverage = supplementaryCoverage,
                    FinalPatientShare = finalPatientShare,
                    CalculationDate = calculationDate
                };

                _log.Information("🏥 MEDICAL: محاسبه صحیح بیمه تکمیلی تکمیل شد - PatientId: {PatientId}, ServiceId: {ServiceId}, PatientShare: {PatientShare}, SupplementaryCoverage: {SupplementaryCoverage}, FinalPatientShare: {FinalPatientShare}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, patientShareFromPrimary, supplementaryCoverage, finalPatientShare, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<SupplementaryInsuranceCalculationResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه صحیح بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<SupplementaryInsuranceCalculationResult>.Failed($"خطا در محاسبه بیمه تکمیلی: {ex.Message}");
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
                _log.Information("🏥 MEDICAL: شروع محاسبه بیمه تکمیلی برای چندین خدمت - PatientId: {PatientId}, ServiceCount: {ServiceCount}. User: {UserName} (Id: {UserId})",
                    patientId, serviceIds.Count, _currentUserService.UserName, _currentUserService.UserId);

                var results = new List<SupplementaryInsuranceCalculationResult>();

                for (int i = 0; i < serviceIds.Count; i++)
                {
                    var serviceId = serviceIds[i];
                    var serviceAmount = serviceAmounts[i];

                    // محاسبه بیمه اصلی (فرض: 70% پوشش)
                    var primaryCoverage = serviceAmount * 0.7m;

                    // محاسبه بیمه تکمیلی
                    var result = CalculateForSpecificScenario(serviceAmount, primaryCoverage, 100m);
                    result.PatientId = patientId;
                    result.ServiceId = serviceId;
                    result.CalculationDate = calculationDate;

                    results.Add(result);
                }

                _log.Information("🏥 MEDICAL: محاسبه بیمه تکمیلی برای چندین خدمت تکمیل شد - PatientId: {PatientId}, ResultsCount: {ResultsCount}. User: {UserName} (Id: {UserId})",
                    patientId, results.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<SupplementaryInsuranceCalculationResult>>.Successful(results);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه بیمه تکمیلی برای چندین خدمت - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<List<SupplementaryInsuranceCalculationResult>>.Failed($"خطا در محاسبه بیمه تکمیلی: {ex.Message}");
            }
        }

        /// <summary>
        /// محاسبه صحیح بیمه تکمیلی برای سناریو خاص
        /// </summary>
        public SupplementaryInsuranceCalculationResult CalculateForSpecificScenario(
            decimal serviceAmount,
            decimal primaryCoverage,
            decimal supplementaryCoveragePercent,
            decimal? supplementaryMaxPayment = null)
        {
            // محاسبه سهم بیمار از بیمه اصلی
            decimal patientShareFromPrimary = serviceAmount - primaryCoverage;
            
            // محاسبه پوشش بیمه تکمیلی (درصدی از سهم بیمار)
            decimal supplementaryCoverage = patientShareFromPrimary * (supplementaryCoveragePercent / 100m);
            
            // اعمال سقف پرداخت
            if (supplementaryMaxPayment.HasValue && supplementaryCoverage > supplementaryMaxPayment.Value)
            {
                supplementaryCoverage = supplementaryMaxPayment.Value;
            }
            
            // محاسبه سهم نهایی بیمار
            decimal finalPatientShare = Math.Max(0, patientShareFromPrimary - supplementaryCoverage);
            
                return new SupplementaryInsuranceCalculationResult
                {
                    ServiceAmount = serviceAmount,
                    PrimaryInsuranceCoverage = primaryCoverage,
                    SupplementaryInsuranceCoverage = supplementaryCoverage,
                    FinalPatientShare = finalPatientShare,
                    CalculationDate = DateTime.Now
                };
        }
    }
}
