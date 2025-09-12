using ClinicApp.ViewModels;
using ClinicApp.Models.Entities;
using FluentValidation;
using System;
using System.Text.RegularExpressions;

namespace ClinicApp.ViewModels.Validators
{
    /// <summary>
    /// Validator اختصاصی برای ReceptionSearchViewModel
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی جامع پارامترهای جستجو
    /// 2. اعتبارسنجی محدوده تاریخ
    /// 3. اعتبارسنجی محدوده مبلغ
    /// 4. اعتبارسنجی پارامترهای صفحه‌بندی
    /// 5. رعایت استانداردهای پزشکی ایران
    /// </summary>
    public class ReceptionSearchViewModelValidator : AbstractValidator<ReceptionSearchViewModel>
    {
        public ReceptionSearchViewModelValidator()
        {
            // اعتبارسنجی کد ملی بیمار
            When(x => !string.IsNullOrEmpty(x.PatientNationalCode), () =>
            {
                RuleFor(x => x.PatientNationalCode)
                    .Length(10, 10)
                    .WithMessage("کد ملی بیمار باید ۱۰ رقم باشد.")
                    .WithErrorCode("INVALID_PATIENT_NATIONAL_CODE_LENGTH")
                    .Matches(@"^\d{10}$")
                    .WithMessage("کد ملی بیمار باید فقط شامل اعداد باشد.")
                    .WithErrorCode("INVALID_PATIENT_NATIONAL_CODE_FORMAT");
            });

            // اعتبارسنجی نام بیمار
            When(x => !string.IsNullOrEmpty(x.PatientName), () =>
            {
                RuleFor(x => x.PatientName)
                    .Length(2, 100)
                    .WithMessage("نام بیمار باید بین ۲ تا ۱۰۰ کاراکتر باشد.")
                    .WithErrorCode("INVALID_PATIENT_NAME_LENGTH")
                    .Matches(@"^[\u0600-\u06FF\s\u200C\u200D]+$")
                    .WithMessage("نام بیمار باید فقط شامل حروف فارسی باشد.")
                    .WithErrorCode("INVALID_PATIENT_NAME_FORMAT");
            });

            // اعتبارسنجی نام پزشک
            When(x => !string.IsNullOrEmpty(x.DoctorName), () =>
            {
                RuleFor(x => x.DoctorName)
                    .Length(2, 100)
                    .WithMessage("نام پزشک باید بین ۲ تا ۱۰۰ کاراکتر باشد.")
                    .WithErrorCode("INVALID_DOCTOR_NAME_LENGTH")
                    .Matches(@"^[\u0600-\u06FF\s\u200C\u200D]+$")
                    .WithMessage("نام پزشک باید فقط شامل حروف فارسی باشد.")
                    .WithErrorCode("INVALID_DOCTOR_NAME_FORMAT");
            });

            // اعتبارسنجی محدوده تاریخ
            When(x => x.StartDate.HasValue, () =>
            {
                RuleFor(x => x.StartDate)
                    .Must(date => date >= DateTime.Today.AddYears(-5))
                    .WithMessage("تاریخ شروع نمی‌تواند بیش از ۵ سال گذشته باشد.")
                    .WithErrorCode("INVALID_START_DATE_PAST")
                    .Must(date => date <= DateTime.Today)
                    .WithMessage("تاریخ شروع نمی‌تواند در آینده باشد.")
                    .WithErrorCode("INVALID_START_DATE_FUTURE");
            });

            When(x => x.EndDate.HasValue, () =>
            {
                RuleFor(x => x.EndDate)
                    .Must(date => date >= DateTime.Today.AddYears(-5))
                    .WithMessage("تاریخ پایان نمی‌تواند بیش از ۵ سال گذشته باشد.")
                    .WithErrorCode("INVALID_END_DATE_PAST")
                    .Must(date => date <= DateTime.Today)
                    .WithMessage("تاریخ پایان نمی‌تواند در آینده باشد.")
                    .WithErrorCode("INVALID_END_DATE_FUTURE");
            });

            // اعتبارسنجی منطق محدوده تاریخ
            When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
            {
                RuleFor(x => x)
                    .Must(x => x.StartDate <= x.EndDate)
                    .WithMessage("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.")
                    .WithErrorCode("INVALID_DATE_RANGE")
                    .Must(x => (x.EndDate - x.StartDate).Value.Days <= 365)
                    .WithMessage("محدوده تاریخ نمی‌تواند بیش از یک سال باشد.")
                    .WithErrorCode("DATE_RANGE_TOO_LARGE");
            });

            // اعتبارسنجی وضعیت پذیرش
            When(x => x.Status.HasValue, () =>
            {
                RuleFor(x => x.Status)
                    .IsInEnum()
                    .WithMessage("وضعیت پذیرش نامعتبر است.")
                    .WithErrorCode("INVALID_RECEPTION_STATUS");
            });

            // اعتبارسنجی نوع پذیرش
            When(x => x.Type.HasValue, () =>
            {
                RuleFor(x => x.Type)
                    .IsInEnum()
                    .WithMessage("نوع پذیرش نامعتبر است.")
                    .WithErrorCode("INVALID_RECEPTION_TYPE");
            });

            // اعتبارسنجی محدوده مبلغ
            When(x => x.MinAmount.HasValue, () =>
            {
                RuleFor(x => x.MinAmount)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("مبلغ حداقل نمی‌تواند منفی باشد.")
                    .WithErrorCode("INVALID_MIN_AMOUNT")
                    .LessThanOrEqualTo(100000000) // 100 میلیون تومان
                    .WithMessage("مبلغ حداقل نمی‌تواند بیش از ۱۰۰ میلیون تومان باشد.")
                    .WithErrorCode("MIN_AMOUNT_TOO_HIGH");
            });

            When(x => x.MaxAmount.HasValue, () =>
            {
                RuleFor(x => x.MaxAmount)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("مبلغ حداکثر نمی‌تواند منفی باشد.")
                    .WithErrorCode("INVALID_MAX_AMOUNT")
                    .LessThanOrEqualTo(100000000) // 100 میلیون تومان
                    .WithMessage("مبلغ حداکثر نمی‌تواند بیش از ۱۰۰ میلیون تومان باشد.")
                    .WithErrorCode("MAX_AMOUNT_TOO_HIGH");
            });

            // اعتبارسنجی منطق محدوده مبلغ
            When(x => x.MinAmount.HasValue && x.MaxAmount.HasValue, () =>
            {
                RuleFor(x => x)
                    .Must(x => x.MinAmount <= x.MaxAmount)
                    .WithMessage("مبلغ حداقل نمی‌تواند بیش از مبلغ حداکثر باشد.")
                    .WithErrorCode("INVALID_AMOUNT_RANGE");
            });

            // اعتبارسنجی روش پرداخت
            When(x => x.PaymentMethod.HasValue, () =>
            {
                RuleFor(x => x.PaymentMethod)
                    .IsInEnum()
                    .WithMessage("روش پرداخت نامعتبر است.")
                    .WithErrorCode("INVALID_PAYMENT_METHOD");
            });

            // اعتبارسنجی بیمه
            When(x => x.InsuranceId.HasValue, () =>
            {
                RuleFor(x => x.InsuranceId)
                    .GreaterThan(0)
                    .WithMessage("شناسه بیمه باید معتبر باشد.")
                    .WithErrorCode("INVALID_INSURANCE_ID");
            });

            // اعتبارسنجی پارامترهای صفحه‌بندی
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("شماره صفحه باید بزرگتر از صفر باشد.")
                .WithErrorCode("INVALID_PAGE_NUMBER")
                .LessThanOrEqualTo(10000) // حداکثر ۱۰ هزار صفحه
                .WithMessage("شماره صفحه نمی‌تواند بیش از ۱۰ هزار باشد.")
                .WithErrorCode("PAGE_NUMBER_TOO_HIGH");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("اندازه صفحه باید بزرگتر از صفر باشد.")
                .WithErrorCode("INVALID_PAGE_SIZE")
                .LessThanOrEqualTo(100)
                .WithMessage("اندازه صفحه نمی‌تواند بیش از ۱۰۰ باشد.")
                .WithErrorCode("PAGE_SIZE_TOO_HIGH");

            // اعتبارسنجی مرتب‌سازی
            When(x => !string.IsNullOrEmpty(x.SortBy), () =>
            {
                RuleFor(x => x.SortBy)
                    .Must(sortBy => IsValidSortField(sortBy))
                    .WithMessage("فیلد مرتب‌سازی نامعتبر است.")
                    .WithErrorCode("INVALID_SORT_BY");
            });

            When(x => !string.IsNullOrEmpty(x.SortOrder), () =>
            {
                RuleFor(x => x.SortOrder)
                    .Must(sortOrder => sortOrder == "asc" || sortOrder == "desc")
                    .WithMessage("ترتیب مرتب‌سازی باید 'asc' یا 'desc' باشد.")
                    .WithErrorCode("INVALID_SORT_ORDER");
            });

            // اعتبارسنجی منطق کسب‌وکار
            RuleFor(x => x)
                .Must(x => !(x.IsEmergency.HasValue && x.IsEmergency.Value && 
                           x.IsOnlineReception.HasValue && x.IsOnlineReception.Value))
                .WithMessage("نمی‌توان همزمان اورژانس و آنلاین را فیلتر کرد.")
                .WithErrorCode("CONFLICTING_FILTERS");
        }

        /// <summary>
        /// بررسی معتبر بودن فیلد مرتب‌سازی
        /// </summary>
        /// <param name="sortBy">فیلد مرتب‌سازی</param>
        /// <returns>آیا فیلد معتبر است؟</returns>
        private bool IsValidSortField(string sortBy)
        {
            var validFields = new[]
            {
                "ReceptionDate",
                "PatientFullName",
                "DoctorFullName",
                "TotalAmount",
                "Status",
                "Type",
                "CreatedAt",
                "UpdatedAt"
            };

            return Array.Exists(validFields, field => 
                string.Equals(field, sortBy, StringComparison.OrdinalIgnoreCase));
        }
    }
}
