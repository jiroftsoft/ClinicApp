using System;
using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities.Insurance;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// Service for managing supplementary insurance combinations
    /// سرویس مدیریت ترکیبات بیمه تکمیلی
    /// </summary>
    public class SupplementaryCombinationService : ISupplementaryCombinationService
    {
        private readonly IInsuranceTariffRepository _tariffRepository;
        private readonly IInsurancePlanService _planService;
        private readonly IServiceRepository _serviceRepository;
        private readonly IPatientInsuranceRepository _patientInsuranceRepository;
        private readonly IInsuranceCalculationService _insuranceCalculationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _log;

        public SupplementaryCombinationService(
            IInsuranceTariffRepository tariffRepository,
            IInsurancePlanService planService,
            IServiceRepository serviceRepository,
            IPatientInsuranceRepository patientInsuranceRepository,
            IInsuranceCalculationService insuranceCalculationService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _tariffRepository = tariffRepository;
            _planService = planService;
            _serviceRepository = serviceRepository;
            _patientInsuranceRepository = patientInsuranceRepository;
            _insuranceCalculationService = insuranceCalculationService;
            _currentUserService = currentUserService;
            _log = logger;
        }

        /// <summary>
        /// Creates a supplementary insurance combination
        /// ایجاد ترکیب بیمه تکمیلی
        /// </summary>
        public async Task<InsuranceTariff> CreateCombinationAsync(
            int serviceId, 
            int primaryPlanId, 
            int supplementaryPlanId, 
            decimal coveragePercent, 
            decimal maxPayment)
        {
            try
            {
                _log.Information("🏥 MEDICAL: Creating supplementary combination - ServiceId: {ServiceId}, PrimaryPlanId: {PrimaryPlanId}, SupplementaryPlanId: {SupplementaryPlanId}",
                    serviceId, primaryPlanId, supplementaryPlanId);

                // بررسی تکراری بودن
                if (await IsDuplicateCombinationAsync(serviceId, primaryPlanId, supplementaryPlanId))
                {
                    throw new InvalidOperationException("ترکیب بیمه تکراری است");
                }

                // دریافت اطلاعات خدمت
                var service = await _serviceRepository.GetByIdAsync(serviceId);
                if (service == null)
                {
                    throw new ArgumentException("خدمت یافت نشد");
                }

                var serviceAmount = service.Price;
                if (serviceAmount <= 0)
                {
                    throw new ArgumentException("قیمت خدمت نامعتبر است");
                }

                // دریافت اطلاعات طرح‌های بیمه
                var planTasks = new[]
                {
                    _planService.GetPlanDetailsAsync(primaryPlanId),
                    _planService.GetPlanDetailsAsync(supplementaryPlanId)
                };

                await Task.WhenAll(planTasks);
                
                var primaryPlan = planTasks[0].Result;
                var supplementaryPlan = planTasks[1].Result;

                if (!primaryPlan.Success || !supplementaryPlan.Success)
                {
                    throw new ArgumentException("طرح بیمه یافت نشد");
                }

                // محاسبه پوشش بیمه اصلی
                var primaryCoverageAmount = 0m;
                if (primaryPlan.Data != null)
                {
                    var deductible = primaryPlan.Data.Deductible;
                    var primaryCoveragePercent = primaryPlan.Data.CoveragePercent;
                    var coverableAmount = Math.Max(0, serviceAmount - deductible);
                    primaryCoverageAmount = coverableAmount * (primaryCoveragePercent / 100m);
                }

                // محاسبه صحیح بیمه تکمیلی
                var correctCalculationService = new CorrectSupplementaryInsuranceCalculationService(
                    _patientInsuranceRepository,
                    _tariffRepository,
                    _insuranceCalculationService,
                    _currentUserService,
                    _log);

                var calculationResult = correctCalculationService.CalculateForSpecificScenario(
                    serviceAmount: serviceAmount,
                    primaryCoverage: primaryCoverageAmount,
                    supplementaryCoveragePercent: coveragePercent,
                    supplementaryMaxPayment: maxPayment > 0 ? maxPayment : (decimal?)null);

                // 🔧 CRITICAL FIX: مپینگ صحیح فیلدهای تعرفه تکمیلی
                var supplementaryTariff = new InsuranceTariff
                {
                    ServiceId = serviceId,
                    TariffPrice = serviceAmount, // مبلغ کل خدمت
                    PatientShare = calculationResult.FinalPatientShare, // سهم نهایی بیمار
                    InsurerShare = calculationResult.SupplementaryInsuranceCoverage, // سهم بیمه تکمیلی
                    InsuranceType = InsuranceType.Supplementary,
                    InsurancePlanId = supplementaryPlanId,
                    SupplementaryCoveragePercent = coveragePercent,
                    SupplementaryMaxPayment = maxPayment > 0 ? maxPayment : (decimal?)null,
                    Priority = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow, // 🔧 FIX: استفاده از UTC
                    CreatedByUserId = _currentUserService.UserId,
                    Notes = $"بیمه تکمیلی {supplementaryPlan.Data.Name} - پوشش {coveragePercent}% از سهم بیمار"
                };

                // ذخیره‌سازی
                var savedTariff = await _tariffRepository.AddAsync(supplementaryTariff);
                if (savedTariff == null)
                {
                    throw new InvalidOperationException("خطا در ذخیره‌سازی تعرفه بیمه تکمیلی");
                }

                _log.Information("🏥 MEDICAL: Supplementary combination created successfully - TariffId: {TariffId}",
                    savedTariff.InsuranceTariffId);

                return savedTariff;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: Error creating supplementary combination");
                throw;
            }
        }

        /// <summary>
        /// Validates if a combination is possible
        /// اعتبارسنجی امکان ترکیب
        /// </summary>
        public async Task<bool> ValidateCombinationAsync(int primaryPlanId, int supplementaryPlanId)
        {
            try
            {
                var primaryPlan = await _planService.GetPlanDetailsAsync(primaryPlanId);
                var supplementaryPlan = await _planService.GetPlanDetailsAsync(supplementaryPlanId);

                return primaryPlan.Success && supplementaryPlan.Success;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: Error validating combination");
                return false;
            }
        }

        /// <summary>
        /// Checks for duplicate combinations
        /// بررسی ترکیبات تکراری
        /// </summary>
        public async Task<bool> IsDuplicateCombinationAsync(int serviceId, int primaryPlanId, int supplementaryPlanId)
        {
            try
            {
                // TODO: پیاده‌سازی بررسی تکراری در دیتابیس
                // این باید در Repository پیاده‌سازی شود
                return false;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: Error checking duplicate combination");
                return false;
            }
        }
    }
}
