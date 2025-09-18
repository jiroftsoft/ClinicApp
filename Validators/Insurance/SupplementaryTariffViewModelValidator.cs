using System;
using System.Linq;
using FluentValidation;
using ClinicApp.ViewModels.Insurance.Supplementary;

namespace ClinicApp.Validators.Insurance
{
    /// <summary>
    /// Validator برای SupplementaryTariffViewModel
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل فیلدهای بیمه تکمیلی
    /// 2. رعایت استانداردهای پزشکی ایران
    /// 3. پشتیبانی از سناریوهای مختلف
    /// 4. مدیریت خطاهای پیچیده
    /// </summary>
    public class SupplementaryTariffViewModelValidator : AbstractValidator<SupplementaryTariffViewModel>
    {
        public SupplementaryTariffViewModelValidator()
        {
            // اعتبارسنجی شناسه تعرفه بیمه
            RuleFor(x => x.TariffId)
                .GreaterThan(0)
                .WithMessage("شناسه تعرفه بیمه باید بزرگتر از صفر باشد");

            // اعتبارسنجی شناسه طرح بیمه
            RuleFor(x => x.PlanId)
                .GreaterThan(0)
                .WithMessage("شناسه طرح بیمه الزامی است");

            // اعتبارسنجی شناسه خدمت
            RuleFor(x => x.ServiceId)
                .GreaterThan(0)
                .WithMessage("شناسه خدمت الزامی است");

            // اعتبارسنجی درصد پوشش تکمیلی
            RuleFor(x => x.CoveragePercent)
                .InclusiveBetween(0, 100)
                .WithMessage("درصد پوشش تکمیلی باید بین 0 تا 100 باشد");

            // اعتبارسنجی سقف پرداخت تکمیلی
            RuleFor(x => x.MaxPayment)
                .GreaterThanOrEqualTo(0)
                .WithMessage("سقف پرداخت تکمیلی نمی‌تواند منفی باشد");

            // اعتبارسنجی تنظیمات JSON
            RuleFor(x => x.Settings)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrEmpty(x.Settings))
                .WithMessage("تنظیمات تکمیلی نمی‌تواند بیش از 2000 کاراکتر باشد");

            // اعتبارسنجی منطق کسب‌وکار
            RuleFor(x => x)
                .Must(ValidateBusinessLogic)
                .WithMessage("تنظیمات بیمه تکمیلی منطقی نیست");
        }

        /// <summary>
        /// اعتبارسنجی منطق کسب‌وکار
        /// </summary>
        private bool ValidateBusinessLogic(SupplementaryTariffViewModel model)
        {
            // درصد پوشش باید منطقی باشد
            if (model.CoveragePercent < 0 || model.CoveragePercent > 100)
                return false;

            // سقف پرداخت باید منطقی باشد
            if (model.MaxPayment < 0)
                return false;

            // فرانشیز باید منطقی باشد
            if (model.Deductible < 0)
                return false;

            // فرانشیز نباید بیشتر از سقف پرداخت باشد
            if (model.Deductible > model.MaxPayment)
                return false;

            return true;
        }
    }
}
