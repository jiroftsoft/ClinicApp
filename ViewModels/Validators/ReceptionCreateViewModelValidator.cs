using ClinicApp.Models.Entities;
using FluentValidation;
using System;
using System.Linq;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Validators
{
    /// <summary>
    /// Validator اختصاصی برای ReceptionCreateViewModel
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی جامع اطلاعات پذیرش
    /// 2. اعتبارسنجی اطلاعات بیمار و پزشک
    /// 3. اعتبارسنجی اطلاعات خدمات و پرداخت
    /// 4. اعتبارسنجی اطلاعات بیمه
    /// 5. رعایت استانداردهای پزشکی ایران
    /// 6. پشتیبانی از پذیرش اورژانس و آنلاین
    /// </summary>
    public class ReceptionCreateViewModelValidator : AbstractValidator<ReceptionCreateViewModel>
    {
        public ReceptionCreateViewModelValidator()
        {
            // اعتبارسنجی اطلاعات بیمار
            RuleFor(x => x.PatientId)
                .GreaterThan(0)
                .WithMessage("باید یک بیمار معتبر انتخاب شود.")
                .WithErrorCode("INVALID_PATIENT_ID");

            RuleFor(x => x.PatientFullName)
                .NotEmpty()
                .WithMessage("نام بیمار الزامی است.")
                .WithErrorCode("REQUIRED_PATIENT_NAME")
                .Length(2, 100)
                .WithMessage("نام بیمار باید بین ۲ تا ۱۰۰ کاراکتر باشد.")
                .WithErrorCode("INVALID_PATIENT_NAME_LENGTH");

            // اعتبارسنجی اطلاعات پزشک
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("باید یک پزشک معتبر انتخاب شود.")
                .WithErrorCode("INVALID_DOCTOR_ID");

            // اعتبارسنجی اطلاعات خدمات
            RuleFor(x => x.SelectedServiceIds)
                .NotNull()
                .WithMessage("لیست خدمات نمی‌تواند خالی باشد.")
                .WithErrorCode("REQUIRED_SERVICES")
                .Must(services => services != null && services.Count > 0)
                .WithMessage("باید حداقل یک خدمت انتخاب شود.")
                .WithErrorCode("MINIMUM_ONE_SERVICE_REQUIRED");

            RuleFor(x => x.SelectedServiceIds)
                .Must(services => services != null && services.All(id => id > 0))
                .WithMessage("همه خدمات انتخاب شده باید معتبر باشند.")
                .WithErrorCode("INVALID_SERVICE_IDS");

            // اعتبارسنجی اطلاعات پذیرش
            RuleFor(x => x.ReceptionDate)
                .NotEmpty()
                .WithMessage("تاریخ پذیرش الزامی است.")
                .WithErrorCode("REQUIRED_RECEPTION_DATE")
                .Must(date => date >= DateTime.Today.AddDays(-30))
                .WithMessage("تاریخ پذیرش نمی‌تواند بیش از ۳۰ روز گذشته باشد.")
                .WithErrorCode("INVALID_RECEPTION_DATE_PAST")
                .Must(date => date <= DateTime.Today.AddDays(30))
                .WithMessage("تاریخ پذیرش نمی‌تواند بیش از ۳۰ روز آینده باشد.")
                .WithErrorCode("INVALID_RECEPTION_DATE_FUTURE");

            RuleFor(x => x.ReceptionDateShamsi)
                .NotEmpty()
                .WithMessage("تاریخ پذیرش شمسی الزامی است.")
                .WithErrorCode("REQUIRED_RECEPTION_DATE_SHAMSI")
                .Matches(@"^\d{4}/\d{2}/\d{2}$")
                .WithMessage("فرمت تاریخ شمسی باید yyyy/mm/dd باشد.")
                .WithErrorCode("INVALID_RECEPTION_DATE_SHAMSI_FORMAT");

            // اعتبارسنجی اطلاعات پرداخت
            RuleFor(x => x.TotalAmount)
                .GreaterThan(0)
                .WithMessage("مجموع مبلغ باید بزرگتر از صفر باشد.")
                .WithErrorCode("INVALID_TOTAL_AMOUNT")
                .LessThanOrEqualTo(100000000) // 100 میلیون تومان
                .WithMessage("مجموع مبلغ نمی‌تواند بیش از ۱۰۰ میلیون تومان باشد.")
                .WithErrorCode("TOTAL_AMOUNT_TOO_HIGH");

            RuleFor(x => x.PaymentMethod)
                .IsInEnum()
                .WithMessage("روش پرداخت نامعتبر است.")
                .WithErrorCode("INVALID_PAYMENT_METHOD");

            // اعتبارسنجی شناسه تراکنش POS
            When(x => x.PaymentMethod == PaymentMethod.Card && !string.IsNullOrEmpty(x.PosTransactionId), () =>
            {
                RuleFor(x => x.PosTransactionId)
                    .Length(1, 50)
                    .WithMessage("شناسه تراکنش POS باید بین ۱ تا ۵۰ کاراکتر باشد.")
                    .WithErrorCode("INVALID_POS_TRANSACTION_ID_LENGTH")
                    .Matches(@"^[A-Za-z0-9\-_]+$")
                    .WithMessage("شناسه تراکنش POS فقط می‌تواند شامل حروف، اعداد، خط تیره و زیرخط باشد.")
                    .WithErrorCode("INVALID_POS_TRANSACTION_ID_FORMAT");
            });

            // اعتبارسنجی اطلاعات بیمه
            When(x => x.PrimaryInsuranceId.HasValue, () =>
            {
                RuleFor(x => x.PrimaryInsuranceId)
                    .GreaterThan(0)
                    .WithMessage("بیمه اولیه باید معتبر باشد.")
                    .WithErrorCode("INVALID_PRIMARY_INSURANCE_ID");
            });

            When(x => x.SecondaryInsuranceId.HasValue, () =>
            {
                RuleFor(x => x.SecondaryInsuranceId)
                    .GreaterThan(0)
                    .WithMessage("بیمه تکمیلی باید معتبر باشد.")
                    .WithErrorCode("INVALID_SECONDARY_INSURANCE_ID");
            });

            // اعتبارسنجی سهم بیمه و بیمار
            RuleFor(x => x.InsuranceShare)
                .GreaterThanOrEqualTo(0)
                .WithMessage("سهم بیمه نمی‌تواند منفی باشد.")
                .WithErrorCode("INVALID_INSURANCE_SHARE")
                .LessThanOrEqualTo(x => x.TotalAmount)
                .WithMessage("سهم بیمه نمی‌تواند بیش از مجموع مبلغ باشد.")
                .WithErrorCode("INSURANCE_SHARE_EXCEEDS_TOTAL");

            RuleFor(x => x.PatientShare)
                .GreaterThanOrEqualTo(0)
                .WithMessage("سهم بیمار نمی‌تواند منفی باشد.")
                .WithErrorCode("INVALID_PATIENT_SHARE")
                .LessThanOrEqualTo(x => x.TotalAmount)
                .WithMessage("سهم بیمار نمی‌تواند بیش از مجموع مبلغ باشد.")
                .WithErrorCode("PATIENT_SHARE_EXCEEDS_TOTAL");

            // اعتبارسنجی مجموع سهم‌ها
            RuleFor(x => x)
                .Must(x => x.InsuranceShare + x.PatientShare <= x.TotalAmount)
                .WithMessage("مجموع سهم بیمه و بیمار نمی‌تواند بیش از مجموع مبلغ باشد.")
                .WithErrorCode("TOTAL_SHARES_EXCEED_TOTAL_AMOUNT");

            // اعتبارسنجی یادداشت‌ها
            When(x => !string.IsNullOrEmpty(x.Notes), () =>
            {
                RuleFor(x => x.Notes)
                    .Length(1, 1000)
                    .WithMessage("یادداشت‌ها باید بین ۱ تا ۱۰۰۰ کاراکتر باشد.")
                    .WithErrorCode("INVALID_NOTES_LENGTH");
            });

            // اعتبارسنجی اطلاعات استعلام
            When(x => !string.IsNullOrEmpty(x.NationalCodeForInquiry), () =>
            {
                RuleFor(x => x.NationalCodeForInquiry)
                    .Length(10, 10)
                    .WithMessage("کد ملی برای استعلام باید ۱۰ رقم باشد.")
                    .WithErrorCode("INVALID_NATIONAL_CODE_FOR_INQUIRY_LENGTH")
                    .Matches(@"^\d{10}$")
                    .WithMessage("کد ملی برای استعلام باید فقط شامل اعداد باشد.")
                    .WithErrorCode("INVALID_NATIONAL_CODE_FOR_INQUIRY_FORMAT");
            });

            When(x => x.BirthDateForInquiry.HasValue, () =>
            {
                RuleFor(x => x.BirthDateForInquiry)
                    .Must(date => date >= DateTime.Today.AddYears(-120) && date <= DateTime.Today)
                    .WithMessage("تاریخ تولد برای استعلام باید معتبر باشد.")
                    .WithErrorCode("INVALID_BIRTH_DATE_FOR_INQUIRY");
            });

            // اعتبارسنجی منطق کسب‌وکار
            RuleFor(x => x)
                .Must(x => !(x.IsEmergency && x.IsOnlineReception))
                .WithMessage("پذیرش نمی‌تواند همزمان اورژانس و آنلاین باشد.")
                .WithErrorCode("CONFLICTING_RECEPTION_TYPES");

            // اعتبارسنجی مبلغ پرداخت شده
            RuleFor(x => x.PaidAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ پرداخت شده نمی‌تواند منفی باشد.")
                .WithErrorCode("INVALID_PAID_AMOUNT")
                .LessThanOrEqualTo(x => x.TotalAmount)
                .WithMessage("مبلغ پرداخت شده نمی‌تواند بیش از مجموع مبلغ باشد.")
                .WithErrorCode("PAID_AMOUNT_EXCEEDS_TOTAL");

            // اعتبارسنجی مبلغ باقی‌مانده
            RuleFor(x => x.RemainingAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ باقی‌مانده نمی‌تواند منفی باشد.")
                .WithErrorCode("INVALID_REMAINING_AMOUNT")
                .Must((x, remaining) => remaining == x.TotalAmount - x.PaidAmount)
                .WithMessage("مبلغ باقی‌مانده باید برابر با تفاوت مجموع مبلغ و مبلغ پرداخت شده باشد.")
                .WithErrorCode("INVALID_REMAINING_AMOUNT_CALCULATION");
        }
    }
}
