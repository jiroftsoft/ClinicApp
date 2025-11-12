using ClinicApp.Interfaces;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس اعتبارسنجی قواعد دامنه برای تعرفه‌های بیمه
    /// Domain Validation Service for Insurance Tariffs
    /// </summary>
    public class TariffDomainValidationService : ITariffDomainValidationService
    {
        private readonly ILogger _logger;
        private readonly IInsuranceTariffRepository _tariffRepository;

        public TariffDomainValidationService(ILogger logger, IInsuranceTariffRepository tariffRepository)
        {
            _logger = logger.ForContext<TariffDomainValidationService>();
            _tariffRepository = tariffRepository;
        }

        /// <summary>
        /// اعتبارسنجی قواعد مالی تعرفه
        /// </summary>
        public ServiceResult ValidateFinancialRules(InsuranceTariff tariff)
        {
            try
            {
                _logger.Debug("🔍 DOMAIN: شروع اعتبارسنجی قواعد مالی - TariffId: {TariffId}", tariff.InsuranceTariffId);

                var errors = new List<string>();

                // 1. قواعد مبلغ تعرفه
                if (tariff.TariffPrice <= 0)
                {
                    errors.Add("مبلغ تعرفه باید بزرگتر از صفر باشد");
                }

                // 2. قواعد سهم بیمه‌گذار
                if (tariff.PatientShare < 0)
                {
                    errors.Add("سهم بیمه‌گذار نمی‌تواند منفی باشد");
                }

                if (tariff.PatientShare > tariff.TariffPrice)
                {
                    errors.Add("سهم بیمه‌گذار نمی‌تواند از مبلغ کل تعرفه بیشتر باشد");
                }

                // 3. قواعد سهم بیمه‌گر
                if (tariff.InsurerShare < 0)
                {
                    errors.Add("سهم بیمه‌گر نمی‌تواند منفی باشد");
                }

                if (tariff.InsurerShare > tariff.TariffPrice)
                {
                    errors.Add("سهم بیمه‌گر نمی‌تواند از مبلغ کل تعرفه بیشتر باشد");
                }

                // 4. قواعد مجموع سهم‌ها
                var totalShare = (tariff.PatientShare ?? 0) + (tariff.InsurerShare ?? 0);
                if (Math.Abs(totalShare - (tariff.TariffPrice ?? 0)) > 0.01m) // تحمل 1 سنت
                {
                    errors.Add($"مجموع سهم بیمه‌گذار ({tariff.PatientShare}) و بیمه‌گر ({tariff.InsurerShare}) باید برابر مبلغ کل تعرفه ({tariff.TariffPrice}) باشد");
                }

                // 5. قواعد درصدی
                var patientPercentage = ((tariff.PatientShare ?? 0) / (tariff.TariffPrice ?? 1)) * 100;
                if (patientPercentage < 0 || patientPercentage > 100)
                {
                    errors.Add("درصد سهم بیمه‌گذار باید بین 0 تا 100 باشد");
                }

                var result = errors.Count == 0 
                    ? ServiceResult.Successful("قواعد مالی معتبر است")
                    : ServiceResult.Failed(string.Join("; ", errors));

                _logger.Information("🔍 DOMAIN: اعتبارسنجی قواعد مالی تکمیل شد - Success: {Success}, ErrorsCount: {ErrorsCount}", 
                    result.Success, errors.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی قواعد مالی");
                return ServiceResult.Failed("خطا در اعتبارسنجی قواعد مالی");
            }
        }

        /// <summary>
        /// اعتبارسنجی قواعد کسب‌وکار تعرفه
        /// </summary>
        public async Task<ServiceResult> ValidateBusinessRulesAsync(InsuranceTariff tariff)
        {
            try
            {
                _logger.Debug("🔍 DOMAIN: شروع اعتبارسنجی قواعد کسب‌وکار - TariffId: {TariffId}, PlanId: {PlanId}, ServiceId: {ServiceId}", 
                    tariff.InsuranceTariffId, tariff.InsurancePlanId, tariff.ServiceId);

                var errors = new List<string>();

                // 1. قواعد یکتایی - بررسی وجود تعرفه مشابه (شامل تعرفه‌های غیرفعال برای validation)
                var existingTariff = await _tariffRepository.GetByPlanAndServiceAsync(tariff.InsurancePlanId ?? 0, tariff.ServiceId, includeInactive: true);
                if (existingTariff != null && existingTariff.InsuranceTariffId != tariff.InsuranceTariffId)
                {
                    errors.Add($"تعرفه‌ای برای این طرح بیمه و خدمت قبلاً وجود دارد (ID: {existingTariff.InsuranceTariffId})");
                }

                // 2. قواعد تاریخ
                if (tariff.CreatedAt > DateTime.Now)
                {
                    errors.Add("تاریخ ایجاد نمی‌تواند در آینده باشد");
                }

                if (tariff.UpdatedAt.HasValue && tariff.UpdatedAt.Value > DateTime.Now)
                {
                    errors.Add("تاریخ بروزرسانی نمی‌تواند در آینده باشد");
                }

                if (tariff.UpdatedAt.HasValue && tariff.UpdatedAt.Value < tariff.CreatedAt)
                {
                    errors.Add("تاریخ بروزرسانی نمی‌تواند قبل از تاریخ ایجاد باشد");
                }

                // 3. قواعد وضعیت
                if (tariff.IsDeleted && tariff.IsActive)
                {
                    errors.Add("تعرفه حذف شده نمی‌تواند فعال باشد");
                }

                var result = errors.Count == 0 
                    ? ServiceResult.Successful("قواعد کسب‌وکار معتبر است")
                    : ServiceResult.Failed(string.Join("; ", errors));

                _logger.Information("🔍 DOMAIN: اعتبارسنجی قواعد کسب‌وکار تکمیل شد - Success: {Success}, ErrorsCount: {ErrorsCount}", 
                    result.Success, errors.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی قواعد کسب‌وکار");
                return ServiceResult.Failed("خطا در اعتبارسنجی قواعد کسب‌وکار");
            }
        }

        /// <summary>
        /// اعتبارسنجی قواعد رَوندینگ
        /// </summary>
        public ServiceResult ValidateRoundingRules(InsuranceTariff tariff)
        {
            try
            {
                _logger.Debug("🔍 DOMAIN: شروع اعتبارسنجی قواعد رَوندینگ - TariffId: {TariffId}", tariff.InsuranceTariffId);

                var errors = new List<string>();

                // 1. قواعد دقت اعشار مبالغ (2 رقم اعشار)
                var tariffPrice = tariff.TariffPrice ?? 0;
                var patientShare = tariff.PatientShare ?? 0;
                var insurerShare = tariff.InsurerShare ?? 0;

                if (tariffPrice != Math.Round(tariffPrice, 2))
                {
                    errors.Add("مبلغ تعرفه باید حداکثر 2 رقم اعشار داشته باشد");
                }

                if (patientShare != Math.Round(patientShare, 2))
                {
                    errors.Add("سهم بیمه‌گذار باید حداکثر 2 رقم اعشار داشته باشد");
                }

                if (insurerShare != Math.Round(insurerShare, 2))
                {
                    errors.Add("سهم بیمه‌گر باید حداکثر 2 رقم اعشار داشته باشد");
                }

                // 2. قواعد رَوندینگ خودکار
                var roundedTariffPrice = Math.Round(tariffPrice, 2, MidpointRounding.AwayFromZero);
                var roundedPatientShare = Math.Round(patientShare, 2, MidpointRounding.AwayFromZero);
                var roundedInsurerShare = Math.Round(insurerShare, 2, MidpointRounding.AwayFromZero);

                if (tariffPrice != roundedTariffPrice)
                {
                    _logger.Warning("🔍 DOMAIN: مبلغ تعرفه رَوندینگ شد - Original: {Original}, Rounded: {Rounded}", 
                        tariffPrice, roundedTariffPrice);
                }

                var result = errors.Count == 0 
                    ? ServiceResult.Successful("قواعد رَوندینگ معتبر است")
                    : ServiceResult.Failed(string.Join("; ", errors));

                _logger.Information("🔍 DOMAIN: اعتبارسنجی قواعد رَوندینگ تکمیل شد - Success: {Success}, ErrorsCount: {ErrorsCount}", 
                    result.Success, errors.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی قواعد رَوندینگ");
                return ServiceResult.Failed("خطا در اعتبارسنجی قواعد رَوندینگ");
            }
        }

        /// <summary>
        /// اعتبارسنجی کامل تعرفه (همه قواعد)
        /// </summary>
        public async Task<ServiceResult> ValidateTariffAsync(InsuranceTariff tariff)
        {
            try
            {
                _logger.Debug("🔍 DOMAIN: شروع اعتبارسنجی کامل تعرفه - TariffId: {TariffId}", tariff.InsuranceTariffId);

                var allErrors = new List<string>();

                // 1. اعتبارسنجی قواعد مالی
                var financialResult = ValidateFinancialRules(tariff);
                if (!financialResult.Success)
                {
                    allErrors.Add(financialResult.Message);
                }

                // 2. اعتبارسنجی قواعد کسب‌وکار
                var businessResult = await ValidateBusinessRulesAsync(tariff);
                if (!businessResult.Success)
                {
                    allErrors.Add(businessResult.Message);
                }

                // 3. اعتبارسنجی قواعد رَوندینگ
                var roundingResult = ValidateRoundingRules(tariff);
                if (!roundingResult.Success)
                {
                    allErrors.Add(roundingResult.Message);
                }

                var result = allErrors.Count == 0 
                    ? ServiceResult.Successful("همه قواعد دامنه معتبر است")
                    : ServiceResult.Failed(string.Join("; ", allErrors));

                _logger.Information("🔍 DOMAIN: اعتبارسنجی کامل تعرفه تکمیل شد - Success: {Success}, TotalErrors: {TotalErrors}", 
                    result.Success, allErrors.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی کامل تعرفه");
                return ServiceResult.Failed("خطا در اعتبارسنجی کامل تعرفه");
            }
        }
    }

}
