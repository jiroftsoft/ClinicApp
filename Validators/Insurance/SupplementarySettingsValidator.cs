using System;
using System.Linq;
using FluentValidation;
using ClinicApp.ViewModels.Insurance.Supplementary;

namespace ClinicApp.Validators.Insurance
{
    /// <summary>
    /// Validator برای SupplementarySettings
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل تنظیمات بیمه تکمیلی
    /// 2. رعایت استانداردهای پزشکی ایران
    /// 3. پشتیبانی از سناریوهای مختلف
    /// 4. مدیریت خطاهای پیچیده
    /// </summary>
    public class SupplementarySettingsValidator : AbstractValidator<SupplementarySettings>
    {
        public SupplementarySettingsValidator()
        {
            // اعتبارسنجی شناسه طرح بیمه
            RuleFor(x => x.PlanId)
                .GreaterThan(0)
                .WithMessage("شناسه طرح بیمه الزامی است");

            // اعتبارسنجی نام طرح بیمه
            RuleFor(x => x.PlanName)
                .NotEmpty()
                .WithMessage("نام طرح بیمه الزامی است")
                .MaximumLength(200)
                .WithMessage("نام طرح بیمه نمی‌تواند بیش از 200 کاراکتر باشد");

            // اعتبارسنجی درصد پوشش
            RuleFor(x => x.CoveragePercent)
                .InclusiveBetween(0, 100)
                .WithMessage("درصد پوشش باید بین 0 تا 100 باشد");

            // اعتبارسنجی سقف پرداخت
            RuleFor(x => x.MaxPayment)
                .GreaterThanOrEqualTo(0)
                .WithMessage("سقف پرداخت نمی‌تواند منفی باشد");

            // اعتبارسنجی فرانشیز
            RuleFor(x => x.Deductible)
                .GreaterThanOrEqualTo(0)
                .WithMessage("فرانشیز نمی‌تواند منفی باشد");

            // اعتبارسنجی تنظیمات JSON
            RuleFor(x => x.SettingsJson)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrEmpty(x.SettingsJson))
                .WithMessage("تنظیمات JSON نمی‌تواند بیش از 2000 کاراکتر باشد");

            // اعتبارسنجی منطق کسب‌وکار
            RuleFor(x => x)
                .Must(ValidateBusinessLogic)
                .WithMessage("تنظیمات بیمه تکمیلی منطقی نیست");
        }

        /// <summary>
        /// اعتبارسنجی منطق کسب‌وکار
        /// </summary>
        private bool ValidateBusinessLogic(SupplementarySettings model)
        {
            // سقف پرداخت باید منطقی باشد
            if (model.MaxPayment < 0)
                return false;

            // درصد پوشش باید منطقی باشد
            if (model.CoveragePercent < 0 || model.CoveragePercent > 100)
                return false;

            // فرانشیز باید منطقی باشد
            if (model.Deductible < 0)
                return false;

            // اگر سقف پرداخت و فرانشیز هر دو تعریف شده باشند
            if (model.MaxPayment > 0 && model.Deductible > 0)
            {
                // فرانشیز نباید بیشتر از سقف پرداخت باشد
                if (model.Deductible > model.MaxPayment)
                    return false;
            }

            return true;
        }
    }
}
