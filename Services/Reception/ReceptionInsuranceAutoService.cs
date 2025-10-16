using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Core;
using ClinicApp.Constants;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس تخصصی بیمه خودکار در ماژول پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. بایندینگ خودکار بیمه‌های موجود بیمار
    /// 2. مدیریت بیمه اصلی و تکمیلی
    /// 3. محاسبه Real-time سهم بیمه و بیمار
    /// 4. بهینه‌سازی برای محیط درمانی
    /// 5. پشتیبانی از Select2 برای انتخاب بیمه
    /// </summary>
    public class ReceptionInsuranceAutoService
    {
        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly IInsuranceProviderService _insuranceProviderService;
        private readonly IInsurancePlanService _insurancePlanService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ReceptionInsuranceAutoService(
            IPatientInsuranceService patientInsuranceService,
            IInsuranceProviderService insuranceProviderService,
            IInsurancePlanService insurancePlanService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));
            _insuranceProviderService = insuranceProviderService ?? throw new ArgumentNullException(nameof(insuranceProviderService));
            _insurancePlanService = insurancePlanService ?? throw new ArgumentNullException(nameof(insurancePlanService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger.ForContext<ReceptionInsuranceAutoService>();
        }

        #region Auto Insurance Binding

        /// <summary>
        /// بایندینگ خودکار بیمه‌های موجود بیمار
        /// </summary>
        public async Task<ServiceResult<InsuranceAccordionViewModel>> AutoBindPatientInsuranceAsync(int patientId)
        {
            try
            {
                _logger.Information($"بایندینگ خودکار بیمه‌های بیمار: {patientId}");

                // دریافت بیمه‌های موجود بیمار
                var patientInsurancesResult = await _patientInsuranceService.GetPatientInsurancesByPatientAsync(patientId);
                
                if (!patientInsurancesResult.Success)
                {
                    _logger.Warning($"بیمه‌ای برای بیمار {patientId} یافت نشد");
                    return ServiceResult<InsuranceAccordionViewModel>.Failed(
                        ReceptionFormConstants.Messages.InsuranceNotFound
                    );
                }

                var patientInsurances = patientInsurancesResult.Data;
                var insuranceViewModel = new InsuranceAccordionViewModel();

                // بایندینگ بیمه اصلی
                var primaryInsurance = patientInsurances.FirstOrDefault(pi => pi.IsPrimary);
                if (primaryInsurance != null)
                {
                    insuranceViewModel.PrimaryInsurance = new PrimaryInsuranceViewModel
                    {
                        ProviderId = null, // TODO: Add InsuranceProviderId to PatientInsuranceIndexViewModel
                        ProviderName = primaryInsurance.InsuranceProviderName,
                        PlanId = primaryInsurance.InsurancePlanId,
                        PlanName = primaryInsurance.InsurancePlanName,
                        PolicyNumber = primaryInsurance.PolicyNumber
                    };
                }

                // بایندینگ بیمه تکمیلی
                var supplementaryInsurance = patientInsurances.FirstOrDefault(pi => !pi.IsPrimary);
                if (supplementaryInsurance != null)
                {
                    insuranceViewModel.SupplementaryInsurance = new SupplementaryInsuranceViewModel
                    {
                        ProviderId = null, // TODO: Add InsuranceProviderId to PatientInsuranceIndexViewModel
                        ProviderName = supplementaryInsurance.InsuranceProviderName,
                        PlanId = supplementaryInsurance.InsurancePlanId,
                        PlanName = supplementaryInsurance.InsurancePlanName,
                        PolicyNumber = supplementaryInsurance.PolicyNumber
                    };
                }

                insuranceViewModel.StatusMessage = ReceptionFormConstants.Messages.InsuranceLoaded;
                insuranceViewModel.StatusCssClass = "text-success";

                _logger.Information($"بیمه‌های بیمار {patientId} با موفقیت بایند شدند");

                return ServiceResult<InsuranceAccordionViewModel>.Successful(
                    insuranceViewModel,
                    ReceptionFormConstants.Messages.InsuranceLoaded
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در بایندینگ خودکار بیمه‌های بیمار {patientId}");
                
                return ServiceResult<InsuranceAccordionViewModel>.Failed(
                    "خطا در بایندینگ بیمه‌های بیمار"
                );
            }
        }

        /// <summary>
        /// دریافت لیست بیمه‌گذاران برای Select2
        /// </summary>
        public async Task<ServiceResult<List<InsuranceProviderLookupViewModel>>> GetInsuranceProvidersAsync()
        {
            try
            {
                _logger.Information("دریافت لیست بیمه‌گذاران");

                var providersResult = await _insuranceProviderService.GetProvidersAsync("", 1, 1000);
                
                if (!providersResult.Success)
                {
                    return ServiceResult<List<InsuranceProviderLookupViewModel>>.Failed(
                        "خطا در دریافت لیست بیمه‌گذاران"
                    );
                }

                var providers = providersResult.Data.Select(p => new InsuranceProviderLookupViewModel
                {
                    Id = p.InsuranceProviderId,
                    Name = p.Name,
                    Code = p.Code,
                    IsActive = p.IsActive
                }).ToList();

                _logger.Information($"تعداد بیمه‌گذاران دریافت شده: {providers.Count}");

                return ServiceResult<List<InsuranceProviderLookupViewModel>>.Successful(
                    providers,
                    "لیست بیمه‌گذاران با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست بیمه‌گذاران");
                
                return ServiceResult<List<InsuranceProviderLookupViewModel>>.Failed(
                    "خطا در دریافت لیست بیمه‌گذاران"
                );
            }
        }

        /// <summary>
        /// دریافت لیست طرح‌های بیمه بر اساس بیمه‌گذار
        /// </summary>
        public async Task<ServiceResult<List<InsurancePlanLookupViewModel>>> GetInsurancePlansAsync(int providerId)
        {
            try
            {
                _logger.Information($"دریافت لیست طرح‌های بیمه برای بیمه‌گذار: {providerId}");

                var plansResult = await _insurancePlanService.GetPlansAsync(providerId, "", 1, 1000);
                
                if (!plansResult.Success)
                {
                    return ServiceResult<List<InsurancePlanLookupViewModel>>.Failed(
                        "خطا در دریافت لیست طرح‌های بیمه"
                    );
                }

                var plans = plansResult.Data.Select(p => new InsurancePlanLookupViewModel
                {
                    Id = p.InsurancePlanId,
                    Name = p.Name,
                    Code = p.PlanCode,
                    ProviderId = 0, // TODO: Add InsuranceProviderId to InsurancePlanIndexViewModel
                    ProviderName = p.InsuranceProviderName,
                    IsActive = p.IsActive
                }).ToList();

                _logger.Information($"تعداد طرح‌های بیمه دریافت شده: {plans.Count}");

                return ServiceResult<List<InsurancePlanLookupViewModel>>.Successful(
                    plans,
                    "لیست طرح‌های بیمه با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت لیست طرح‌های بیمه برای بیمه‌گذار {providerId}");
                
                return ServiceResult<List<InsurancePlanLookupViewModel>>.Failed(
                    "خطا در دریافت لیست طرح‌های بیمه"
                );
            }
        }

        /// <summary>
        /// محاسبه Real-time سهم بیمه و بیمار
        /// </summary>
        public async Task<ServiceResult<InsuranceCalculationResult>> CalculateInsuranceShareAsync(
            int patientId, 
            int primaryPlanId, 
            int? supplementaryPlanId, 
            decimal serviceAmount)
        {
            try
            {
                _logger.Information($"محاسبه سهم بیمه برای بیمار {patientId} و مبلغ {serviceAmount}");

                // دریافت اطلاعات بیمه بیمار
                var patientInsurancesResult = await _patientInsuranceService.GetPatientInsurancesByPatientAsync(patientId);
                
                if (!patientInsurancesResult.Success)
                {
                    return ServiceResult<InsuranceCalculationResult>.Failed(
                        "اطلاعات بیمه بیمار یافت نشد"
                    );
                }

                var patientInsurances = patientInsurancesResult.Data;
                var primaryInsurance = patientInsurances.FirstOrDefault(pi => pi.InsurancePlanId == primaryPlanId);
                var supplementaryInsurance = supplementaryPlanId.HasValue 
                    ? patientInsurances.FirstOrDefault(pi => pi.InsurancePlanId == supplementaryPlanId.Value)
                    : null;

                if (primaryInsurance == null)
                {
                    return ServiceResult<InsuranceCalculationResult>.Failed(
                        "بیمه اصلی انتخاب شده یافت نشد"
                    );
                }

                // محاسبه سهم بیمه اصلی
                var primaryShare = CalculatePrimaryInsuranceShare(primaryInsurance, serviceAmount);
                
                // محاسبه سهم بیمه تکمیلی
                var supplementaryShare = supplementaryInsurance != null 
                    ? CalculateSupplementaryInsuranceShare(supplementaryInsurance, serviceAmount - primaryShare)
                    : 0;

                var totalInsuranceShare = primaryShare + supplementaryShare;
                var patientShare = serviceAmount - totalInsuranceShare;

                var result = new InsuranceCalculationResult
                {
                    ServiceAmount = serviceAmount,
                    PrimaryInsuranceShare = primaryShare,
                    SupplementaryInsuranceShare = supplementaryShare,
                    TotalInsuranceShare = totalInsuranceShare,
                    PatientShare = patientShare,
                    IsValid = true
                };

                _logger.Information($"محاسبه سهم بیمه تکمیل شد - سهم بیمه: {totalInsuranceShare}, سهم بیمار: {patientShare}");

                return ServiceResult<InsuranceCalculationResult>.Successful(
                    result,
                    "محاسبه سهم بیمه با موفقیت انجام شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در محاسبه سهم بیمه برای بیمار {patientId}");
                
                return ServiceResult<InsuranceCalculationResult>.Failed(
                    "خطا در محاسبه سهم بیمه"
                );
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// محاسبه سهم بیمه اصلی
        /// </summary>
        private decimal CalculatePrimaryInsuranceShare(dynamic primaryInsurance, decimal serviceAmount)
        {
            // TODO: پیاده‌سازی منطق محاسبه سهم بیمه اصلی
            // اینجا باید از InsuranceTariffService استفاده شود
            return serviceAmount * 0.7m; // مثال: 70% سهم بیمه
        }

        /// <summary>
        /// محاسبه سهم بیمه تکمیلی
        /// </summary>
        private decimal CalculateSupplementaryInsuranceShare(dynamic supplementaryInsurance, decimal remainingAmount)
        {
            // TODO: پیاده‌سازی منطق محاسبه سهم بیمه تکمیلی
            // اینجا باید از InsuranceTariffService استفاده شود
            return remainingAmount * 0.5m; // مثال: 50% سهم بیمه تکمیلی
        }

        #endregion
    }

    /// <summary>
    /// ViewModel برای نمایش بیمه‌گذار در Select2
    /// </summary>
    public class InsuranceProviderLookupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش طرح بیمه در Select2
    /// </summary>
    public class InsurancePlanLookupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// نتیجه محاسبه سهم بیمه
    /// </summary>
    public class InsuranceCalculationResult
    {
        public decimal ServiceAmount { get; set; }
        public decimal PrimaryInsuranceShare { get; set; }
        public decimal SupplementaryInsuranceShare { get; set; }
        public decimal TotalInsuranceShare { get; set; }
        public decimal PatientShare { get; set; }
        public bool IsValid { get; set; }
    }
}
