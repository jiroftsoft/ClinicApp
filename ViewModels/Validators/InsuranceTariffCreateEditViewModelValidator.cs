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

            // اعتبارسنجی شناسه طرح بیمه
            RuleFor(x => x.InsurancePlanId)
                .GreaterThan(0)
                .WithMessage("لطفاً یک طرح بیمه معتبر انتخاب کنید")
                .MustAsync(BeValidInsurancePlanAsync)
                .WithMessage("طرح بیمه انتخاب شده معتبر نیست یا حذف شده است");

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
                .When(x => !x.IsAllServices);

            // اعتبارسنجی معتبر بودن خدمت - فقط وقتی خدمت خاص انتخاب شده
            RuleFor(x => x)
                .MustAsync((model, cancellationToken) => BeValidServiceAsync(model.ServiceId, cancellationToken))
                .WithMessage("خدمت انتخاب شده معتبر نیست یا حذف شده است")
                .When(x => !x.IsAllServices && x.ServiceId.HasValue);

            // اعتبارسنجی قیمت تعرفه
            RuleFor(x => x.TariffPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("قیمت تعرفه نمی‌تواند منفی باشد")
                .LessThan(999999999.99m)
                .WithMessage("قیمت تعرفه بیش از حد مجاز است")
                .When(x => x.TariffPrice.HasValue);

            // اعتبارسنجی درصد سهم بیمه‌گر
            RuleFor(x => x.InsurerShare)
                .InclusiveBetween(0, 100)
                .WithMessage("درصد سهم بیمه‌گر باید بین 0 تا 100 باشد")
                .When(x => x.InsurerShare.HasValue);

            // اعتبارسنجی درصد سهم بیمار
            RuleFor(x => x.PatientShare)
                .InclusiveBetween(0, 100)
                .WithMessage("درصد سهم بیمار باید بین 0 تا 100 باشد")
                .When(x => x.PatientShare.HasValue);

            // اعتبارسنجی فعال بودن
            RuleFor(x => x.IsActive)
                .NotNull()
                .WithMessage("وضعیت فعال بودن الزامی است");

            // اعتبارسنجی عدم تکرار تعرفه برای همان طرح و خدمت
            RuleFor(x => x)
                .MustAsync(NotBeDuplicateTariffAsync)
                .WithMessage("تعرفه‌ای برای این طرح بیمه و خدمت قبلاً تعریف شده است")
                .When(x => x.InsurancePlanId > 0 && x.ServiceId > 0);

            // اعتبارسنجی منطقی بودن درصدها
            RuleFor(x => x)
                .Must(HaveValidPercentageSum)
                .WithMessage("مجموع درصد سهم بیمه‌گر و بیمار نباید بیش از 100 باشد")
                .When(x => x.InsurerShare.HasValue && x.PatientShare.HasValue);

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
        /// بررسی منطقی بودن مجموع درصدها
        /// </summary>
        private bool HaveValidPercentageSum(InsuranceTariffCreateEditViewModel model)
        {
            if (!model.InsurerShare.HasValue || !model.PatientShare.HasValue)
                return true;

            return (model.InsurerShare.Value + model.PatientShare.Value) <= 100;
        }

        /// <summary>
        /// بررسی منطقی بودن مبالغ
        /// </summary>
        private bool HaveValidAmounts(InsuranceTariffCreateEditViewModel model)
        {
            // این validation برای فیلدهای موجود در ViewModel اعمال نمی‌شود
            return true;
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
