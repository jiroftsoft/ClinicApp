using System;
using FluentValidation;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;

namespace ClinicApp.ViewModels.Validators
{
    /// <summary>
    /// Validator برای InsuranceCalculationViewModel
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل فیلدهای محاسبه بیمه
    /// 2. رعایت استانداردهای پزشکی ایران
    /// 3. بررسی محدودیت‌های منطقی
    /// 4. پیام‌های خطای فارسی
    /// 5. پشتیبانی از WithErrorCode
    /// 
    /// نکته حیاتی: این Validator بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
    /// </summary>
    public class InsuranceCalculationViewModelValidator : AbstractValidator<InsuranceCalculationViewModel>
    {
        public InsuranceCalculationViewModelValidator()
        {
            // اعتبارسنجی شناسه بیمار
            RuleFor(x => x.PatientId)
                .GreaterThan(0)
                .WithMessage("بیمار الزامی است")
                .WithErrorCode("PATIENT_REQUIRED");

            // اعتبارسنجی شناسه بیمه بیمار
            RuleFor(x => x.PatientInsuranceId)
                .GreaterThan(0)
                .WithMessage("بیمه بیمار الزامی است")
                .WithErrorCode("PATIENT_INSURANCE_REQUIRED");

            // اعتبارسنجی شناسه طرح بیمه
            RuleFor(x => x.InsurancePlanId)
                .GreaterThan(0)
                .WithMessage("طرح بیمه الزامی است")
                .WithErrorCode("INSURANCE_PLAN_REQUIRED");

            // اعتبارسنجی شناسه خدمت
            RuleFor(x => x.ServiceId)
                .GreaterThan(0)
                .WithMessage("خدمت الزامی است")
                .WithErrorCode("SERVICE_REQUIRED");

            // اعتبارسنجی شناسه دسته‌بندی خدمت
            RuleFor(x => x.ServiceCategoryId)
                .GreaterThan(0)
                .WithMessage("دسته‌بندی خدمت الزامی است")
                .WithErrorCode("SERVICE_CATEGORY_REQUIRED");

            // اعتبارسنجی مبلغ خدمت
            RuleFor(x => x.ServiceAmount)
                .GreaterThan(0)
                .WithMessage("مبلغ خدمت باید بیشتر از صفر باشد")
                .WithErrorCode("SERVICE_AMOUNT_INVALID")
                .LessThanOrEqualTo(100000000) // حداکثر 100 میلیون ریال
                .WithMessage("مبلغ خدمت نمی‌تواند بیشتر از 100,000,000 ریال باشد")
                .WithErrorCode("SERVICE_AMOUNT_TOO_HIGH");

            // اعتبارسنجی درصد پوشش
            RuleFor(x => x.CoveragePercent)
                .InclusiveBetween(0, 100)
                .WithMessage("درصد پوشش باید بین 0 تا 100 باشد")
                .WithErrorCode("COVERAGE_PERCENT_INVALID");

            // اعتبارسنجی فرانشیز
            RuleFor(x => x.Deductible)
                .GreaterThanOrEqualTo(0)
                .WithMessage("فرانشیز نمی‌تواند منفی باشد")
                .WithErrorCode("DEDUCTIBLE_INVALID")
                .LessThanOrEqualTo(10000000) // حداکثر 10 میلیون ریال
                .WithMessage("فرانشیز نمی‌تواند بیشتر از 10,000,000 ریال باشد")
                .WithErrorCode("DEDUCTIBLE_TOO_HIGH");

            // اعتبارسنجی سهم بیمه
            RuleFor(x => x.InsuranceShare)
                .GreaterThanOrEqualTo(0)
                .WithMessage("سهم بیمه نمی‌تواند منفی باشد")
                .WithErrorCode("INSURANCE_SHARE_INVALID")
                .LessThanOrEqualTo(x => x.ServiceAmount)
                .WithMessage("سهم بیمه نمی‌تواند بیشتر از مبلغ خدمت باشد")
                .WithErrorCode("INSURANCE_SHARE_TOO_HIGH");

            // اعتبارسنجی سهم بیمار
            RuleFor(x => x.PatientShare)
                .GreaterThanOrEqualTo(0)
                .WithMessage("سهم بیمار نمی‌تواند منفی باشد")
                .WithErrorCode("PATIENT_SHARE_INVALID")
                .LessThanOrEqualTo(x => x.ServiceAmount)
                .WithMessage("سهم بیمار نمی‌تواند بیشتر از مبلغ خدمت باشد")
                .WithErrorCode("PATIENT_SHARE_TOO_HIGH");

            // اعتبارسنجی کوپه
            RuleFor(x => x.Copay)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Copay.HasValue)
                .WithMessage("کوپه نمی‌تواند منفی باشد")
                .WithErrorCode("COPAY_INVALID")
                .LessThanOrEqualTo(x => x.ServiceAmount)
                .When(x => x.Copay.HasValue)
                .WithMessage("کوپه نمی‌تواند بیشتر از مبلغ خدمت باشد")
                .WithErrorCode("COPAY_TOO_HIGH");

            // اعتبارسنجی پوشش خاص
            RuleFor(x => x.CoverageOverride)
                .GreaterThanOrEqualTo(0)
                .When(x => x.CoverageOverride.HasValue)
                .WithMessage("پوشش خاص نمی‌تواند منفی باشد")
                .WithErrorCode("COVERAGE_OVERRIDE_INVALID")
                .LessThanOrEqualTo(100)
                .When(x => x.CoverageOverride.HasValue)
                .WithMessage("پوشش خاص نمی‌تواند بیشتر از 100 درصد باشد")
                .WithErrorCode("COVERAGE_OVERRIDE_TOO_HIGH");

            // اعتبارسنجی تاریخ محاسبه
            RuleFor(x => x.CalculationDate)
                .NotEmpty()
                .WithMessage("تاریخ محاسبه الزامی است")
                .WithErrorCode("CALCULATION_DATE_REQUIRED")
                .LessThanOrEqualTo(System.DateTime.Now)
                .WithMessage("تاریخ محاسبه نمی‌تواند در آینده باشد")
                .WithErrorCode("CALCULATION_DATE_FUTURE");

            // اعتبارسنجی نوع محاسبه
            RuleFor(x => x.CalculationType)
                .NotEmpty()
                .WithMessage("نوع محاسبه الزامی است")
                .WithErrorCode("CALCULATION_TYPE_REQUIRED")
                .MaximumLength(50)
                .WithMessage("نوع محاسبه نمی‌تواند بیشتر از 50 کاراکتر باشد")
                .WithErrorCode("CALCULATION_TYPE_TOO_LONG");

            // اعتبارسنجی یادداشت‌ها
            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrEmpty(x.Notes))
                .WithMessage("یادداشت‌ها نمی‌تواند بیشتر از 1000 کاراکتر باشد")
                .WithErrorCode("NOTES_TOO_LONG");

            // اعتبارسنجی منطقی: مجموع سهم بیمه و بیمار
            RuleFor(x => x)
                .Must(x => Math.Abs((x.InsuranceShare + x.PatientShare) - x.ServiceAmount) < 0.01m)
                .WithMessage("مجموع سهم بیمه و بیمار باید برابر با مبلغ خدمت باشد")
                .WithErrorCode("SHARE_SUM_MISMATCH");

            // اعتبارسنجی منطقی: کوپه و سهم بیمار
            RuleFor(x => x)
                .Must(x => !x.Copay.HasValue || x.Copay.Value <= x.PatientShare)
                .WithMessage("کوپه نمی‌تواند بیشتر از سهم بیمار باشد")
                .WithErrorCode("COPAY_EXCEEDS_PATIENT_SHARE");
        }
    }
}
