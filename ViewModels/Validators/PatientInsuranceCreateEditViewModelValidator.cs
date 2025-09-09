using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using ClinicApp.Interfaces.Insurance;

namespace ClinicApp.ViewModels.Validators
{
    /// <summary>
    /// Validator برای PatientInsuranceCreateEditViewModel
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل فیلدهای بیمه بیمار
    /// 2. رعایت استانداردهای پزشکی ایران
    /// 3. بررسی منحصر به فرد بودن شماره بیمه
    /// 4. بررسی تداخل تاریخ‌ها
    /// 5. بررسی بیمه اصلی
    /// 6. پیام‌های خطای فارسی
    /// 7. پشتیبانی از WithErrorCode
    /// 8. اعتبارسنجی Async برای بررسی‌های دیتابیس
    /// 
    /// نکته حیاتی: این Validator بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
    /// </summary>
    public class PatientInsuranceCreateEditViewModelValidator : AbstractValidator<PatientInsuranceCreateEditViewModel>
    {
        private readonly IPatientInsuranceService _patientInsuranceService;

        public PatientInsuranceCreateEditViewModelValidator(IPatientInsuranceService patientInsuranceService)
        {
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));

            // اعتبارسنجی شناسه بیمار
            RuleFor(x => x.PatientId)
                .GreaterThan(0)
                .WithMessage("بیمار الزامی است")
                .WithErrorCode("PATIENT_REQUIRED");

            // اعتبارسنجی شماره بیمه
            RuleFor(x => x.PolicyNumber)
                .NotEmpty()
                .WithMessage("شماره بیمه الزامی است")
                .WithErrorCode("POLICY_NUMBER_REQUIRED")
                .MaximumLength(100)
                .WithMessage("شماره بیمه نمی‌تواند بیشتر از 100 کاراکتر باشد")
                .WithErrorCode("POLICY_NUMBER_TOO_LONG")
                .Matches(@"^[0-9\-]+$")
                .WithMessage("شماره بیمه باید شامل اعداد و خط تیره باشد")
                .WithErrorCode("POLICY_NUMBER_INVALID_FORMAT");

            // اعتبارسنجی منحصر به فرد بودن شماره بیمه
            RuleFor(x => x)
                .MustAsync(BeUniquePolicyNumber)
                .WithMessage("شماره بیمه تکراری است")
                .WithErrorCode("POLICY_NUMBER_DUPLICATE");

            // اعتبارسنجی شناسه طرح بیمه
            RuleFor(x => x.InsurancePlanId)
                .GreaterThan(0)
                .WithMessage("طرح بیمه الزامی است")
                .WithErrorCode("INSURANCE_PLAN_REQUIRED");

            // اعتبارسنجی تاریخ شروع
            RuleFor(x => x.StartDate)
                .NotEmpty()
                .WithMessage("تاریخ شروع الزامی است")
                .WithErrorCode("START_DATE_REQUIRED")
                .LessThanOrEqualTo(DateTime.Now.AddYears(1))
                .WithMessage("تاریخ شروع نمی‌تواند بیشتر از یک سال آینده باشد")
                .WithErrorCode("START_DATE_TOO_FUTURE")
                .GreaterThanOrEqualTo(DateTime.Now.AddYears(-10))
                .WithMessage("تاریخ شروع نمی‌تواند کمتر از 10 سال گذشته باشد")
                .WithErrorCode("START_DATE_TOO_PAST");

            // اعتبارسنجی تاریخ پایان
            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .When(x => x.EndDate.HasValue)
                .WithMessage("تاریخ پایان باید بعد از تاریخ شروع باشد")
                .WithErrorCode("END_DATE_BEFORE_START")
                .LessThanOrEqualTo(DateTime.Now.AddYears(10))
                .When(x => x.EndDate.HasValue)
                .WithMessage("تاریخ پایان نمی‌تواند بیشتر از 10 سال آینده باشد")
                .WithErrorCode("END_DATE_TOO_FUTURE");

            // اعتبارسنجی منطقی: مدت بیمه
            RuleFor(x => x)
                .Must(x => !x.EndDate.HasValue || (x.EndDate.Value - x.StartDate).TotalDays >= 1)
                .WithMessage("مدت بیمه باید حداقل یک روز باشد")
                .WithErrorCode("INSURANCE_DURATION_TOO_SHORT")
                .Must(x => !x.EndDate.HasValue || (x.EndDate.Value - x.StartDate).TotalDays <= 3650) // 10 سال
                .WithMessage("مدت بیمه نمی‌تواند بیشتر از 10 سال باشد")
                .WithErrorCode("INSURANCE_DURATION_TOO_LONG");

            // اعتبارسنجی تداخل تاریخ‌ها
            RuleFor(x => x)
                .MustAsync(NotOverlapWithExistingInsurances)
                .WithMessage("تداخل تاریخ با بیمه‌های موجود بیمار")
                .WithErrorCode("DATE_OVERLAP_EXISTS");

            // اعتبارسنجی بیمه اصلی
            RuleFor(x => x)
                .MustAsync(ValidatePrimaryInsurance)
                .WithMessage("بیمه اصلی نامعتبر است")
                .WithErrorCode("PRIMARY_INSURANCE_INVALID");

            // اعتبارسنجی منطقی: بیمه فعال
            RuleFor(x => x)
                .Must(x => x.IsActive || !x.IsPrimary)
                .WithMessage("بیمه غیرفعال نمی‌تواند بیمه اصلی باشد")
                .WithErrorCode("INACTIVE_PRIMARY_INSURANCE");
        }

        /// <summary>
        /// بررسی منحصر به فرد بودن شماره بیمه
        /// </summary>
        private async Task<bool> BeUniquePolicyNumber(PatientInsuranceCreateEditViewModel model, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(model.PolicyNumber))
                return true;

            try
            {
                var result = await _patientInsuranceService.DoesPolicyNumberExistAsync(
                    model.PolicyNumber, 
                    model.PatientInsuranceId > 0 ? model.PatientInsuranceId : null);
                return !result.Success || !result.Data;
            }
            catch
            {
                // در صورت خطا، اعتبارسنجی را رد نکنیم
                return true;
            }
        }

        /// <summary>
        /// بررسی عدم تداخل تاریخ‌ها با بیمه‌های موجود
        /// </summary>
        private async Task<bool> NotOverlapWithExistingInsurances(PatientInsuranceCreateEditViewModel model, CancellationToken cancellationToken)
        {
            if (model.PatientId <= 0)
                return true;

            try
            {
                var endDate = model.EndDate ?? DateTime.MaxValue;
                var result = await _patientInsuranceService.DoesDateOverlapExistAsync(
                    model.PatientId, 
                    model.StartDate, 
                    endDate, 
                    model.PatientInsuranceId > 0 ? model.PatientInsuranceId : null);

                return !result.Success || !result.Data;
            }
            catch
            {
                // در صورت خطا، اعتبارسنجی را رد نکنیم
                return true;
            }
        }


        /// <summary>
        /// اعتبارسنجی بیمه اصلی
        /// </summary>
        private async Task<bool> ValidatePrimaryInsurance(PatientInsuranceCreateEditViewModel model, CancellationToken cancellationToken)
        {
            if (!model.IsPrimary || model.PatientId <= 0)
                return true;

            try
            {
                // اگر این بیمه اصلی است، بررسی کنیم که آیا بیمه اصلی دیگری وجود دارد
                var result = await _patientInsuranceService.DoesPrimaryInsuranceExistAsync(
                    model.PatientId, 
                    model.PatientInsuranceId > 0 ? model.PatientInsuranceId : null);

                // اگر بیمه اصلی دیگری وجود دارد و این بیمه جدید است، خطا
                if (result.Success && result.Data && model.PatientInsuranceId == 0)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                // در صورت خطا، اعتبارسنجی را رد نکنیم
                return true;
            }
        }
    }
}
