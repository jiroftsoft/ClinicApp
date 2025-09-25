using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using ClinicApp.ViewModels.Insurance.InsuranceTariff;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;

namespace ClinicApp.ViewModels.Validators
{
    /// <summary>
    /// اعتبارسنجی قوی و Strongly Typed برای InsuranceTariffCreateEditViewModel
    /// </summary>
    public class InsuranceTariffCreateEditViewModelValidator : AbstractValidator<InsuranceTariffCreateEditViewModel>
    {
        private readonly IInsurancePlanService _insurancePlanService;
        private readonly IServiceManagementService _serviceManagementService;
        private readonly IInsuranceTariffService _insuranceTariffService;

        public InsuranceTariffCreateEditViewModelValidator(
            IInsurancePlanService insurancePlanService,
            IServiceManagementService serviceManagementService,
            IInsuranceTariffService insuranceTariffService)
        {
            _insurancePlanService = insurancePlanService ?? throw new ArgumentNullException(nameof(insurancePlanService));
            _serviceManagementService = serviceManagementService ?? throw new ArgumentNullException(nameof(serviceManagementService));
            _insuranceTariffService = insuranceTariffService ?? throw new ArgumentNullException(nameof(insuranceTariffService));

            // اعتبارسنجی شناسه دپارتمان
            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("لطفاً یک دپارتمان معتبر انتخاب کنید")
                .WithErrorCode("REQUIRED_DEPARTMENT");

            // اعتبارسنجی شناسه طرح بیمه
            RuleFor(x => x.InsurancePlanId)
                .GreaterThan(0)
                .WithMessage("لطفاً یک طرح بیمه معتبر انتخاب کنید")
                .MustAsync(BeValidInsurancePlanAsync)
                .WithMessage("طرح بیمه انتخاب شده معتبر نیست یا حذف شده است")
                .WithErrorCode("INVALID_INSURANCE_PLAN");

            // اعتبارسنجی شناسه خدمت - فقط وقتی "همه خدمات" انتخاب نشده
            RuleFor(x => x)
                .Must((model) => 
                {
                    // اگر "همه خدمات" انتخاب شده، ServiceId می‌تواند null باشد
                    if (model.IsAllServices)
                        return true;
                    
                    // در غیر این صورت، ServiceId باید معتبر باشد
                    return model.ServiceId.HasValue && model.ServiceId.Value > 0;
                })
                .WithMessage("لطفاً یک خدمت معتبر انتخاب کنید یا گزینه 'همه خدمات' را انتخاب کنید")
                .WithErrorCode("REQUIRED_SERVICE")
                .When(x => !x.IsAllServices);

            // اعتبارسنجی معتبر بودن خدمت - فقط وقتی خدمت خاص انتخاب شده
            RuleFor(x => x)
                .MustAsync((model, cancellationToken) => BeValidServiceAsync(model.ServiceId, cancellationToken))
                .WithMessage("خدمت انتخاب شده معتبر نیست یا حذف شده است")
                .When(x => !x.IsAllServices && x.ServiceId.HasValue);

            // 🚀 FINANCIAL PRECISION: اعتبارسنجی دقیق قیمت تعرفه بر اساس ریال
            RuleFor(x => x.TariffPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("قیمت تعرفه نمی‌تواند منفی باشد")
                .WithErrorCode("INVALID_TARIFF_PRICE")
                .LessThan(999999999.99m)
                .WithMessage("قیمت تعرفه بیش از حد مجاز است (حداکثر 999,999,999.99 ریال)")
                .WithErrorCode("TARIFF_PRICE_TOO_HIGH")
                .Must(BeValidRialAmount)
                .WithMessage("قیمت تعرفه باید بر اساس ریال باشد (بدون اعشار)")
                .WithErrorCode("INVALID_RIAL_AMOUNT")
                .When(x => x.TariffPrice.HasValue);

            // 🚀 FINANCIAL PRECISION: اعتبارسنجی دقیق سهم بیمه‌گر بر اساس ریال
            RuleFor(x => x.InsurerShare)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ سهم بیمه‌گر نمی‌تواند منفی باشد")
                .WithErrorCode("INVALID_INSURER_SHARE")
                .LessThan(999999999.99m)
                .WithMessage("مبلغ سهم بیمه‌گر بیش از حد مجاز است (حداکثر 999,999,999.99 ریال)")
                .WithErrorCode("INSURER_SHARE_TOO_HIGH")
                .Must(BeValidRialAmount)
                .WithMessage("مبلغ سهم بیمه‌گر باید بر اساس ریال باشد (بدون اعشار)")
                .WithErrorCode("INVALID_RIAL_AMOUNT")
                .When(x => x.InsurerShare.HasValue);

            // 🚀 FINANCIAL PRECISION: اعتبارسنجی دقیق سهم بیمار بر اساس ریال
            RuleFor(x => x.PatientShare)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ سهم بیمار نمی‌تواند منفی باشد")
                .WithErrorCode("INVALID_PATIENT_SHARE")
                .LessThan(999999999.99m)
                .WithMessage("مبلغ سهم بیمار بیش از حد مجاز است (حداکثر 999,999,999.99 ریال)")
                .WithErrorCode("PATIENT_SHARE_TOO_HIGH")
                .Must(BeValidRialAmount)
                .WithMessage("مبلغ سهم بیمار باید بر اساس ریال باشد (بدون اعشار)")
                .WithErrorCode("INVALID_RIAL_AMOUNT")
                .When(x => x.PatientShare.HasValue);

            // اعتبارسنجی فعال بودن
            RuleFor(x => x.IsActive)
                .NotNull()
                .WithMessage("وضعیت فعال بودن الزامی است")
                .WithErrorCode("REQUIRED_IS_ACTIVE");

            // اعتبارسنجی عدم تکرار تعرفه برای همان طرح و خدمت - فقط برای خدمت خاص
            RuleFor(x => x)
                .MustAsync(NotBeDuplicateTariffAsync)
                .WithMessage("تعرفه‌ای برای این طرح بیمه و خدمت قبلاً تعریف شده است")
                .WithErrorCode("DUPLICATE_TARIFF")
                .When(x => x.InsurancePlanId > 0 && x.ServiceId.HasValue && x.ServiceId > 0 && !x.IsAllServices);

            // اعتبارسنجی منطقی بودن مبالغ
            RuleFor(x => x)
                .Must(HaveValidAmountSum)
                .WithMessage("مجموع مبلغ سهم بیمه‌گر و بیمار نباید بیش از قیمت تعرفه باشد")
                .WithErrorCode("INVALID_AMOUNT_SUM")
                .When(x => x.InsurerShare.HasValue && x.PatientShare.HasValue && x.TariffPrice.HasValue);

            // اعتبارسنجی منطقی بودن مبالغ - فعلاً غیرفعال
            // RuleFor(x => x)
            //     .Must(HaveValidAmounts)
            //     .WithMessage("مبلغ فرانشیز نمی‌تواند بیشتر از مبلغ سقف پوشش باشد")
            //     .When(x => x.DeductibleAmount.HasValue && x.CoverageLimit.HasValue);
        }

        /// <summary>
        /// بررسی معتبر بودن طرح بیمه
        /// </summary>
        private async Task<bool> BeValidInsurancePlanAsync(int planId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _insurancePlanService.GetPlanDetailsAsync(planId);
                return result.Success && result.Data != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// بررسی معتبر بودن خدمت
        /// </summary>
        private async Task<bool> BeValidServiceAsync(int? serviceId, CancellationToken cancellationToken)
        {
            try
            {
                // اگر serviceId null است، معتبر نیست (مگر اینکه IsAllServices = true باشد که در When condition بررسی می‌شود)
                if (!serviceId.HasValue)
                    return false;
                    
                var result = await _serviceManagementService.GetServiceDetailsAsync(serviceId.Value);
                return result.Success && result.Data != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// بررسی عدم تکرار تعرفه
        /// </summary>
        private async Task<bool> NotBeDuplicateTariffAsync(InsuranceTariffCreateEditViewModel model, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _insuranceTariffService.CheckTariffExistsAsync(
                    model.InsurancePlanId, 
                    model.ServiceId ?? 0, 
                    model.InsuranceTariffId > 0 ? model.InsuranceTariffId : null);
                
                return result.Success && !result.Data;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// بررسی منطقی بودن مجموع مبالغ
        /// </summary>
        private bool HaveValidAmountSum(InsuranceTariffCreateEditViewModel model)
        {
            if (!model.InsurerShare.HasValue || !model.PatientShare.HasValue || !model.TariffPrice.HasValue)
                return true;

            return (model.InsurerShare.Value + model.PatientShare.Value) <= model.TariffPrice.Value;
        }

        /// <summary>
        /// بررسی منطقی بودن مبالغ
        /// </summary>
        private bool HaveValidAmounts(InsuranceTariffCreateEditViewModel model)
        {
            // این validation برای فیلدهای موجود در ViewModel اعمال نمی‌شود
            return true;
        }

        /// <summary>
        /// 🚀 FINANCIAL PRECISION: بررسی صحت مبلغ بر اساس ریال
        /// </summary>
        private bool BeValidRialAmount(decimal? amount)
        {
            if (!amount.HasValue) return true;
            
            // بررسی اینکه مبلغ بر اساس ریال باشد (بدون اعشار)
            return amount.Value == Math.Round(amount.Value, 0, MidpointRounding.AwayFromZero);
        }
    }

    /// <summary>
    /// اعتبارسنجی برای InsuranceTariffFilterViewModel
    /// </summary>
    public class InsuranceTariffFilterViewModelValidator : AbstractValidator<InsuranceTariffFilterViewModel>
    {
        public InsuranceTariffFilterViewModelValidator()
        {
            // اعتبارسنجی شناسه طرح بیمه
            RuleFor(x => x.InsurancePlanId)
                .GreaterThanOrEqualTo(0)
                .WithMessage("شناسه طرح بیمه نامعتبر است")
                .When(x => x.InsurancePlanId.HasValue);

            // اعتبارسنجی شناسه خدمت
            RuleFor(x => x.ServiceId)
                .GreaterThanOrEqualTo(0)
                .WithMessage("شناسه خدمت نامعتبر است")
                .When(x => x.ServiceId.HasValue);

            // اعتبارسنجی شناسه ارائه‌دهنده بیمه
            RuleFor(x => x.InsuranceProviderId)
                .GreaterThanOrEqualTo(0)
                .WithMessage("شناسه ارائه‌دهنده بیمه نامعتبر است")
                .When(x => x.InsuranceProviderId.HasValue);

            // اعتبارسنجی شماره صفحه
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("شماره صفحه باید بزرگتر از صفر باشد");

            // اعتبارسنجی اندازه صفحه
            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("اندازه صفحه باید بین 1 تا 100 باشد");

            // اعتبارسنجی عبارت جستجو
            RuleFor(x => x.SearchTerm)
                .MaximumLength(200)
                .WithMessage("عبارت جستجو نمی‌تواند بیش از 200 کاراکتر باشد")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));
        }
    }
}
