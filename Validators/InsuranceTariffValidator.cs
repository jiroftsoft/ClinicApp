using ClinicApp.ViewModels.Insurance.InsuranceTariff;
using FluentValidation;
using System;

namespace ClinicApp.Validators
{
    /// <summary>
    /// اعتبارسنجی سرور برای InsuranceTariffCreateEditViewModel
    /// منبع حقیقت برای تمام قواعد دامنه
    /// </summary>
    public class InsuranceTariffValidator : AbstractValidator<InsuranceTariffCreateEditViewModel>
    {
        public InsuranceTariffValidator()
        {
            // اعتبارسنجی فیلدهای الزامی
            RuleFor(x => x.ServiceId)
                .GreaterThan(0)
                .WithMessage("انتخاب خدمت الزامی است.");

            RuleFor(x => x.InsurancePlanId)
                .GreaterThan(0)
                .WithMessage("انتخاب طرح بیمه الزامی است.");

            RuleFor(x => x.InsuranceProviderId)
                .GreaterThan(0)
                .WithMessage("انتخاب ارائه‌دهنده بیمه الزامی است.");

            // اعتبارسنجی مقادیر عددی
            RuleFor(x => x.TariffPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("قیمت تعرفه نمی‌تواند منفی باشد.")
                .When(x => x.TariffPrice.HasValue);

            RuleFor(x => x.PatientShare)
                .GreaterThanOrEqualTo(0)
                .WithMessage("سهم بیمار نمی‌تواند منفی باشد.")
                .When(x => x.PatientShare.HasValue);

            RuleFor(x => x.InsurerShare)
                .GreaterThanOrEqualTo(0)
                .WithMessage("سهم بیمه نمی‌تواند منفی باشد.")
                .When(x => x.InsurerShare.HasValue);

            // اعتبارسنجی درصدها
            RuleFor(x => x.PatientSharePercent)
                .InclusiveBetween(0, 100)
                .WithMessage("درصد سهم بیمار باید بین 0 تا 100 باشد.")
                .When(x => x.PatientSharePercent.HasValue);

            RuleFor(x => x.InsurerSharePercent)
                .InclusiveBetween(0, 100)
                .WithMessage("درصد سهم بیمه باید بین 0 تا 100 باشد.")
                .When(x => x.InsurerSharePercent.HasValue);

            // 🔍 MEDICAL: اعتبارسنجی جمع درصدها ≤ 100
            RuleFor(x => x)
                .Must(HaveValidPercentageSum)
                .WithMessage("مجموع درصد سهم بیمار و بیمه نمی‌تواند بیش از 100 باشد.")
                .When(x => x.PatientSharePercent.HasValue && x.InsurerSharePercent.HasValue);

            // اعتبارسنجی تاریخ‌ها (اگر در ViewModel موجود باشند)
            // RuleFor(x => x.EffectiveFrom)
            //     .LessThanOrEqualTo(x => x.EffectiveTo)
            //     .WithMessage("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.")
            //     .When(x => x.EffectiveFrom.HasValue && x.EffectiveTo.HasValue);

            // اعتبارسنجی منطق دامنه
            RuleFor(x => x)
                .Must(HaveValidTariffLogic)
                .WithMessage("حداقل یکی از قیمت تعرفه، سهم بیمار یا سهم بیمه باید مشخص باشد.");
        }

        /// <summary>
        /// بررسی صحت جمع درصدها
        /// </summary>
        private bool HaveValidPercentageSum(InsuranceTariffCreateEditViewModel model)
        {
            if (!model.PatientSharePercent.HasValue || !model.InsurerSharePercent.HasValue)
                return true;

            var sum = model.PatientSharePercent.Value + model.InsurerSharePercent.Value;
            return sum <= 100;
        }

        /// <summary>
        /// بررسی منطق دامنه تعرفه
        /// </summary>
        private bool HaveValidTariffLogic(InsuranceTariffCreateEditViewModel model)
        {
            return model.TariffPrice.HasValue || 
                   model.PatientShare.HasValue || 
                   model.InsurerShare.HasValue;
        }
    }
}
