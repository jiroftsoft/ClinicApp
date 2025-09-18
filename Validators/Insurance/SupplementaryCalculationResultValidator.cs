using System;
using System.Linq;
using FluentValidation;
using ClinicApp.ViewModels.Insurance.Supplementary;

namespace ClinicApp.Validators.Insurance
{
    /// <summary>
    /// Validator برای SupplementaryCalculationResult
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل نتایج محاسبه بیمه تکمیلی
    /// 2. رعایت استانداردهای پزشکی ایران
    /// 3. پشتیبانی از سناریوهای مختلف
    /// 4. مدیریت خطاهای پیچیده
    /// </summary>
    public class SupplementaryCalculationResultValidator : AbstractValidator<SupplementaryCalculationResult>
    {
        public SupplementaryCalculationResultValidator()
        {
            // اعتبارسنجی شناسه بیمار
            RuleFor(x => x.PatientId)
                .GreaterThan(0)
                .WithMessage("شناسه بیمار الزامی است");

            // اعتبارسنجی شناسه خدمت
            RuleFor(x => x.ServiceId)
                .GreaterThan(0)
                .WithMessage("شناسه خدمت الزامی است");

            // اعتبارسنجی مبلغ خدمت
            RuleFor(x => x.ServiceAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ خدمت نمی‌تواند منفی باشد");

            // اعتبارسنجی پوشش بیمه اصلی
            RuleFor(x => x.PrimaryCoverage)
                .GreaterThanOrEqualTo(0)
                .WithMessage("پوشش بیمه اصلی نمی‌تواند منفی باشد");

            // اعتبارسنجی پوشش بیمه تکمیلی
            RuleFor(x => x.SupplementaryCoverage)
                .GreaterThanOrEqualTo(0)
                .WithMessage("پوشش بیمه تکمیلی نمی‌تواند منفی باشد");

            // اعتبارسنجی سهم نهایی بیمار
            RuleFor(x => x.FinalPatientShare)
                .GreaterThanOrEqualTo(0)
                .WithMessage("سهم نهایی بیمار نمی‌تواند منفی باشد");

            // اعتبارسنجی مجموع پوشش
            RuleFor(x => x.TotalCoverage)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مجموع پوشش نمی‌تواند منفی باشد");

            // اعتبارسنجی تاریخ محاسبه
            RuleFor(x => x.CalculationDate)
                .NotEmpty()
                .WithMessage("تاریخ محاسبه الزامی است")
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("تاریخ محاسبه نمی‌تواند در آینده باشد");

            // اعتبارسنجی منطق کسب‌وکار
            RuleFor(x => x)
                .Must(ValidateBusinessLogic)
                .WithMessage("نتایج محاسبه بیمه تکمیلی منطقی نیست");
        }

        /// <summary>
        /// اعتبارسنجی منطق کسب‌وکار
        /// </summary>
        private bool ValidateBusinessLogic(SupplementaryCalculationResult model)
        {
            // مجموع پوشش نباید بیشتر از مبلغ خدمت باشد
            if (model.TotalCoverage > model.ServiceAmount)
                return false;

            // سهم نهایی بیمار نباید بیشتر از مبلغ خدمت باشد
            if (model.FinalPatientShare > model.ServiceAmount)
                return false;

            // مجموع پوشش و سهم نهایی بیمار باید برابر مبلغ خدمت باشد
            if (Math.Abs(model.TotalCoverage + model.FinalPatientShare - model.ServiceAmount) > 0.01m)
                return false;

            // پوشش بیمه اصلی نباید بیشتر از مبلغ خدمت باشد
            if (model.PrimaryCoverage > model.ServiceAmount)
                return false;

            // پوشش بیمه تکمیلی نباید بیشتر از مبلغ خدمت باشد
            if (model.SupplementaryCoverage > model.ServiceAmount)
                return false;

            // مجموع پوشش اصلی و تکمیلی باید برابر TotalCoverage باشد
            if (Math.Abs(model.PrimaryCoverage + model.SupplementaryCoverage - model.TotalCoverage) > 0.01m)
                return false;

            return true;
        }
    }
}
