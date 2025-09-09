using System;
using FluentValidation;
using ClinicApp.ViewModels.Insurance.InsurancePlan;

namespace ClinicApp.ViewModels.Validators
{
    /// <summary>
    /// Validator اختصاصی برای InsurancePlanCreateEditViewModel
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی پیچیده‌تر از Data Annotations
    /// 2. اعتبارسنجی روابط بین فیلدها
    /// 3. اعتبارسنجی منطق کسب‌وکار
    /// 4. پیام‌های خطای فارسی و دقیق
    /// 5. رعایت استانداردهای پزشکی ایران
    /// 
    /// نکته حیاتی: این Validator بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
    /// </summary>
    public class InsurancePlanCreateEditViewModelValidator : AbstractValidator<InsurancePlanCreateEditViewModel>
    {
        public InsurancePlanCreateEditViewModelValidator()
        {
            // اعتبارسنجی نام طرح بیمه
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("نام طرح بیمه الزامی است")
                .WithErrorCode("REQUIRED")
                .Length(2, 200)
                .WithMessage("نام طرح بیمه باید بین 2 تا 200 کاراکتر باشد")
                .WithErrorCode("LENGTH")
                .Matches(@"^[\u0600-\u06FF\s\w\-\.]+$")
                .WithMessage("نام طرح بیمه فقط می‌تواند شامل حروف فارسی، انگلیسی، اعداد، فاصله، خط تیره و نقطه باشد")
                .WithErrorCode("INVALID_CHARS");

            // اعتبارسنجی کد طرح بیمه
            RuleFor(x => x.PlanCode)
                .NotEmpty()
                .WithMessage("کد طرح بیمه الزامی است")
                .WithErrorCode("REQUIRED")
                .Length(2, 50)
                .WithMessage("کد طرح بیمه باید بین 2 تا 50 کاراکتر باشد")
                .WithErrorCode("LENGTH")
                .Matches(@"^[A-Z0-9\-_]+$")
                .WithMessage("کد طرح بیمه فقط می‌تواند شامل حروف بزرگ انگلیسی، اعداد، خط تیره و زیرخط باشد")
                .WithErrorCode("INVALID_FORMAT");

            // اعتبارسنجی ارائه‌دهنده بیمه
            RuleFor(x => x.InsuranceProviderId)
                .GreaterThan(0)
                .WithMessage("ارائه‌دهنده بیمه الزامی است")
                .WithErrorCode("REQUIRED");

            // اعتبارسنجی درصد پوشش
            RuleFor(x => x.CoveragePercent)
                .InclusiveBetween(0, 100)
                .WithMessage("درصد پوشش باید بین 0 تا 100 باشد")
                .WithErrorCode("RANGE")
                .GreaterThan(0)
                .When(x => x.IsActive)
                .WithMessage("طرح فعال باید درصد پوشش داشته باشد")
                .WithErrorCode("ACTIVE_COVERAGE_REQUIRED");

            // اعتبارسنجی فرانشیز
            RuleFor(x => x.Deductible)
                .GreaterThanOrEqualTo(0)
                .WithMessage("فرانشیز باید بزرگتر یا مساوی صفر باشد")
                .WithErrorCode("MIN_VALUE");

            // اعتبارسنجی تاریخ شروع
            RuleFor(x => x.ValidFrom)
                .NotEmpty()
                .WithMessage("تاریخ شروع الزامی است")
                .WithErrorCode("REQUIRED")
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("تاریخ شروع نمی‌تواند قبل از امروز باشد")
                .WithErrorCode("PAST_DATE");

            // اعتبارسنجی تاریخ پایان
            RuleFor(x => x.ValidTo)
                .GreaterThan(x => x.ValidFrom)
                .When(x => x.ValidTo.HasValue)
                .WithMessage("تاریخ پایان باید بعد از تاریخ شروع باشد")
                .WithErrorCode("INVALID_DATE_RANGE")
                .GreaterThanOrEqualTo(DateTime.Today)
                .When(x => x.ValidTo.HasValue)
                .WithMessage("تاریخ پایان نمی‌تواند قبل از امروز باشد")
                .WithErrorCode("PAST_END_DATE");

            // اعتبارسنجی منطق کسب‌وکار
            RuleFor(x => x)
                .Must(HaveValidCoverageAndDeductible)
                .WithMessage("مجموع درصد پوشش و فرانشیز نباید از 100% بیشتر باشد")
                .WithErrorCode("INVALID_COVERAGE_DEDUCTIBLE");

            RuleFor(x => x)
                .Must(HaveReasonableValidityPeriod)
                .WithMessage("دوره اعتبار طرح بیمه نباید بیش از 5 سال باشد")
                .WithErrorCode("INVALID_VALIDITY_PERIOD");
        }

        /// <summary>
        /// بررسی منطق پوشش و فرانشیز
        /// </summary>
        private bool HaveValidCoverageAndDeductible(InsurancePlanCreateEditViewModel model)
        {
            if (model.CoveragePercent + (model.Deductible / 1000) > 100)
                return false;
            return true;
        }

        /// <summary>
        /// بررسی منطق دوره اعتبار
        /// </summary>
        private bool HaveReasonableValidityPeriod(InsurancePlanCreateEditViewModel model)
        {
            if (model.ValidTo.HasValue)
            {
                var validityPeriod = model.ValidTo.Value - model.ValidFrom;
                return validityPeriod.TotalDays <= 365 * 5; // حداکثر 5 سال
            }
            return true;
        }
    }
}
