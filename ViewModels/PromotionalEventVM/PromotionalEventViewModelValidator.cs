using System;
using ClinicApp.Models.Enums;
using FluentValidation;
using ClinicApp.ViewModels.PromotionalEventVM;

namespace ClinicApp.ViewModels.PromotionalEventVM
{
    /// <summary>
    /// Validator برای PromotionalEventCreateEditViewModel
    /// استفاده از FluentValidation برای اعتبارسنجی
    /// </summary>
    public class PromotionalEventViewModelValidator : AbstractValidator<PromotionalEventCreateEditViewModel>
    {
        public PromotionalEventViewModelValidator()
        {
            // اعتبارسنجی Title
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("عنوان ایونت الزامی است.")
                .WithErrorCode("TITLE_REQUIRED")
                .MaximumLength(200)
                .WithMessage("عنوان ایونت نمی‌تواند بیش از 200 کاراکتر باشد.")
                .WithErrorCode("TITLE_TOO_LONG");

            // اعتبارسنجی Description
            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")
                .WithErrorCode("DESCRIPTION_TOO_LONG")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            // اعتبارسنجی StartDate
            RuleFor(x => x.StartDate)
                .NotEmpty()
                .WithMessage("تاریخ شروع الزامی است.")
                .WithErrorCode("START_DATE_REQUIRED");

            // اعتبارسنجی EndDate
            RuleFor(x => x.EndDate)
                .NotEmpty()
                .WithMessage("تاریخ پایان الزامی است.")
                .WithErrorCode("END_DATE_REQUIRED");

            // اعتبارسنجی منطقی بودن تاریخ‌ها
            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("تاریخ پایان باید بعد از تاریخ شروع باشد.")
                .WithErrorCode("END_DATE_BEFORE_START_DATE")
                .When(x => x.StartDate != default(DateTime) && x.EndDate != default(DateTime));

            // اعتبارسنجی DiscountType
            RuleFor(x => x.DiscountType)
                .IsInEnum()
                .WithMessage("نوع تخفیف نامعتبر است.")
                .WithErrorCode("INVALID_DISCOUNT_TYPE");

            // اعتبارسنجی DiscountValue
            RuleFor(x => x.DiscountValue)
                .GreaterThan(0)
                .WithMessage("مقدار تخفیف باید بیشتر از صفر باشد.")
                .WithErrorCode("DISCOUNT_VALUE_INVALID");

            // اعتبارسنجی DiscountValue برای Percentage
            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100)
                .WithMessage("تخفیف درصدی نمی‌تواند بیشتر از 100% باشد.")
                .WithErrorCode("DISCOUNT_PERCENTAGE_TOO_HIGH")
                .When(x => x.DiscountType == DiscountType.Percentage);

            // اعتبارسنجی TotalSlots
            RuleFor(x => x.TotalSlots)
                .GreaterThan(0)
                .WithMessage("تعداد کل نوبت‌ها باید بیشتر از صفر باشد.")
                .WithErrorCode("TOTAL_SLOTS_INVALID")
                .When(x => x.TotalSlots.HasValue);

            // اعتبارسنجی IsDoctorSpecific
            RuleFor(x => x.SelectedDoctorIds)
                .NotEmpty()
                .WithMessage("در صورت انتخاب محدودیت پزشک، حداقل یک پزشک باید انتخاب شود.")
                .WithErrorCode("NO_DOCTORS_SELECTED")
                .When(x => x.IsDoctorSpecific);
        }
    }
}

