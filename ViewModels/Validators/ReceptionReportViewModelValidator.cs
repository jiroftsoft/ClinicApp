using ClinicApp.ViewModels;
using ClinicApp.Models.Entities;
using FluentValidation;
using System;
using System.Text.RegularExpressions;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.ViewModels.Validators
{
    /// <summary>
    /// Validator اختصاصی برای ReceptionReportViewModel
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی جامع پارامترهای گزارش‌گیری
    /// 2. اعتبارسنجی محدوده تاریخ گزارش
    /// 3. اعتبارسنجی فیلترهای گزارش
    /// 4. اعتبارسنجی فرمت خروجی
    /// 5. رعایت استانداردهای پزشکی ایران
    /// </summary>
    public class ReceptionReportViewModelValidator : AbstractValidator<ReceptionReportViewModel>
    {
        public ReceptionReportViewModelValidator()
        {
            // اعتبارسنجی عنوان گزارش
            RuleFor(x => x.ReportTitle)
                .NotEmpty()
                .WithMessage("عنوان گزارش الزامی است.")
                .WithErrorCode("REQUIRED_REPORT_TITLE")
                .Length(1, 200)
                .WithMessage("عنوان گزارش باید بین ۱ تا ۲۰۰ کاراکتر باشد.")
                .WithErrorCode("INVALID_REPORT_TITLE_LENGTH")
                .Matches(@"^[\u0600-\u06FF\s\u200C\u200D\w\-_]+$")
                .WithMessage("عنوان گزارش باید شامل حروف فارسی، انگلیسی، اعداد، خط تیره و زیرخط باشد.")
                .WithErrorCode("INVALID_REPORT_TITLE_FORMAT");

            // اعتبارسنجی نوع گزارش
            RuleFor(x => x.ReportType)
                .IsInEnum()
                .WithMessage("نوع گزارش نامعتبر است.")
                .WithErrorCode("INVALID_REPORT_TYPE");

            // اعتبارسنجی تاریخ شروع
            RuleFor(x => x.StartDate)
                .NotEmpty()
                .WithMessage("تاریخ شروع الزامی است.")
                .WithErrorCode("REQUIRED_START_DATE")
                .Must(date => date >= DateTime.Today.AddYears(-5))
                .WithMessage("تاریخ شروع نمی‌تواند بیش از ۵ سال گذشته باشد.")
                .WithErrorCode("INVALID_START_DATE_PAST")
                .Must(date => date <= DateTime.Today)
                .WithMessage("تاریخ شروع نمی‌تواند در آینده باشد.")
                .WithErrorCode("INVALID_START_DATE_FUTURE");

            // اعتبارسنجی تاریخ پایان
            RuleFor(x => x.EndDate)
                .NotEmpty()
                .WithMessage("تاریخ پایان الزامی است.")
                .WithErrorCode("REQUIRED_END_DATE")
                .Must(date => date >= DateTime.Today.AddYears(-5))
                .WithMessage("تاریخ پایان نمی‌تواند بیش از ۵ سال گذشته باشد.")
                .WithErrorCode("INVALID_END_DATE_PAST")
                .Must(date => date <= DateTime.Today)
                .WithMessage("تاریخ پایان نمی‌تواند در آینده باشد.")
                .WithErrorCode("INVALID_END_DATE_FUTURE");

            // اعتبارسنجی منطق محدوده تاریخ
            RuleFor(x => x)
                .Must(x => x.StartDate <= x.EndDate)
                .WithMessage("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.")
                .WithErrorCode("INVALID_DATE_RANGE")
                .Must(x => (x.EndDate - x.StartDate).Days <= 365)
                .WithMessage("محدوده تاریخ نمی‌تواند بیش از یک سال باشد.")
                .WithErrorCode("DATE_RANGE_TOO_LARGE");

            // اعتبارسنجی گروه‌بندی
            When(x => !string.IsNullOrEmpty(x.GroupBy), () =>
            {
                RuleFor(x => x.GroupBy)
                    .Must(groupBy => IsValidGroupByField(groupBy))
                    .WithMessage("فیلد گروه‌بندی نامعتبر است.")
                    .WithErrorCode("INVALID_GROUP_BY");
            });

            // اعتبارسنجی فیلتر وضعیت
            When(x => x.StatusFilter.HasValue, () =>
            {
                RuleFor(x => x.StatusFilter)
                    .IsInEnum()
                    .WithMessage("فیلتر وضعیت نامعتبر است.")
                    .WithErrorCode("INVALID_STATUS_FILTER");
            });

            // اعتبارسنجی فیلتر نوع
            When(x => x.TypeFilter.HasValue, () =>
            {
                RuleFor(x => x.TypeFilter)
                    .IsInEnum()
                    .WithMessage("فیلتر نوع نامعتبر است.")
                    .WithErrorCode("INVALID_TYPE_FILTER");
            });

            // اعتبارسنجی فیلتر پزشک
            When(x => x.DoctorFilter.HasValue, () =>
            {
                RuleFor(x => x.DoctorFilter)
                    .GreaterThan(0)
                    .WithMessage("فیلتر پزشک باید معتبر باشد.")
                    .WithErrorCode("INVALID_DOCTOR_FILTER");
            });

            // اعتبارسنجی فیلتر بیمه
            When(x => x.InsuranceFilter.HasValue, () =>
            {
                RuleFor(x => x.InsuranceFilter)
                    .GreaterThan(0)
                    .WithMessage("فیلتر بیمه باید معتبر باشد.")
                    .WithErrorCode("INVALID_INSURANCE_FILTER");
            });

            // اعتبارسنجی فرمت خروجی
            When(x => !string.IsNullOrEmpty(x.OutputFormat), () =>
            {
                RuleFor(x => x.OutputFormat)
                    .Must(format => IsValidOutputFormat(format))
                    .WithMessage("فرمت خروجی نامعتبر است.")
                    .WithErrorCode("INVALID_OUTPUT_FORMAT");
            });

            // اعتبارسنجی منطق کسب‌وکار
            RuleFor(x => x)
                .Must(x => !(x.IncludeEmergency == false && x.IncludeOnline == false))
                .WithMessage("باید حداقل یکی از انواع پذیرش (عادی، اورژانس، آنلاین) را شامل شود.")
                .WithErrorCode("NO_RECEPTION_TYPE_INCLUDED");

            // اعتبارسنجی محدودیت‌های نوع گزارش
            When(x => x.ReportType == ReportType.Daily, () =>
            {
                RuleFor(x => x)
                    .Must(x => (x.EndDate - x.StartDate).Days <= 30)
                    .WithMessage("گزارش روزانه نمی‌تواند بیش از ۳۰ روز باشد.")
                    .WithErrorCode("DAILY_REPORT_RANGE_TOO_LARGE");
            });

            When(x => x.ReportType == ReportType.Monthly, () =>
            {
                RuleFor(x => x)
                    .Must(x => (x.EndDate - x.StartDate).Days <= 365)
                    .WithMessage("گزارش ماهانه نمی‌تواند بیش از یک سال باشد.")
                    .WithErrorCode("MONTHLY_REPORT_RANGE_TOO_LARGE");
            });

            When(x => x.ReportType == ReportType.Yearly, () =>
            {
                RuleFor(x => x)
                    .Must(x => (x.EndDate - x.StartDate).Days <= 365 * 5)
                    .WithMessage("گزارش سالانه نمی‌تواند بیش از ۵ سال باشد.")
                    .WithErrorCode("YEARLY_REPORT_RANGE_TOO_LARGE");
            });

            // اعتبارسنجی نتایج گزارش
            When(x => x.ReportData != null, () =>
            {
                RuleFor(x => x.ReportData)
                    .Must(data => data.Count <= 10000)
                    .WithMessage("تعداد رکوردهای گزارش نمی‌تواند بیش از ۱۰ هزار باشد.")
                    .WithErrorCode("REPORT_DATA_TOO_LARGE");
            });

            When(x => x.Summary != null, () =>
            {
                RuleFor(x => x.Summary)
                    .Must(summary => summary.TotalReceptions >= 0)
                    .WithMessage("تعداد کل پذیرش‌ها نمی‌تواند منفی باشد.")
                    .WithErrorCode("INVALID_TOTAL_RECEPTIONS")
                    .Must(summary => summary.TotalAmount >= 0)
                    .WithMessage("کل مبلغ نمی‌تواند منفی باشد.")
                    .WithErrorCode("INVALID_TOTAL_AMOUNT");
            });
        }

        /// <summary>
        /// بررسی معتبر بودن فیلد گروه‌بندی
        /// </summary>
        /// <param name="groupBy">فیلد گروه‌بندی</param>
        /// <returns>آیا فیلد معتبر است؟</returns>
        private bool IsValidGroupByField(string groupBy)
        {
            var validFields = new[]
            {
                "Date",
                "Doctor",
                "Insurance",
                "PaymentMethod",
                "Status",
                "Type",
                "Emergency",
                "Online"
            };

            return Array.Exists(validFields, field => 
                string.Equals(field, groupBy, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// بررسی معتبر بودن فرمت خروجی
        /// </summary>
        /// <param name="format">فرمت خروجی</param>
        /// <returns>آیا فرمت معتبر است؟</returns>
        private bool IsValidOutputFormat(string format)
        {
            var validFormats = new[]
            {
                "PDF",
                "Excel",
                "CSV",
                "HTML"
            };

            return Array.Exists(validFormats, f => 
                string.Equals(f, format, StringComparison.OrdinalIgnoreCase));
        }
    }
}
