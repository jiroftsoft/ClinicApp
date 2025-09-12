using ClinicApp.ViewModels.Payment.POS;
using FluentValidation;
using System;
using System.Linq;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Validators.Payment.POS
{
    #region POS Terminal Validators

    /// <summary>
    /// Validator برای ایجاد ترمینال POS
    /// </summary>
    public class PosTerminalCreateViewModelValidator : AbstractValidator<PosTerminalCreateViewModel>
    {
        public PosTerminalCreateViewModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("نام ترمینال الزامی است")
                .MaximumLength(100)
                .WithMessage("نام ترمینال نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.SerialNumber)
                .NotEmpty()
                .WithMessage("شماره سریال الزامی است")
                .MaximumLength(50)
                .WithMessage("شماره سریال نمی‌تواند بیشتر از 50 کاراکتر باشد")
                .Matches(@"^[A-Za-z0-9\-_]+$")
                .WithMessage("شماره سریال باید شامل حروف، اعداد، خط تیره و زیرخط باشد");

            RuleFor(x => x.ProviderType)
                .IsInEnum()
                .WithMessage("نوع ارائه‌دهنده نامعتبر است");

            RuleFor(x => x.Protocol)
                .IsInEnum()
                .WithMessage("پروتکل نامعتبر است");

            RuleFor(x => x.ConnectionString)
                .NotEmpty()
                .WithMessage("رشته اتصال الزامی است")
                .MaximumLength(500)
                .WithMessage("رشته اتصال نمی‌تواند بیشتر از 500 کاراکتر باشد");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد");

            // Validation for specific provider types
            RuleFor(x => x.ConnectionString)
                .Must(BeValidConnectionString)
                .When(x => x.ProviderType == PosProviderType.SamanKish)
                .WithMessage("رشته اتصال برای Samank نامعتبر است");

            RuleFor(x => x.ConnectionString)
                .Must(BeValidConnectionString)
                .When(x => x.ProviderType == PosProviderType.AsanPardakht)
                .WithMessage("رشته اتصال برای Parsian نامعتبر است");
        }

        private bool BeValidConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return false;

            // Basic validation for connection string format
            return connectionString.Contains("=") && connectionString.Contains(";");
        }
    }

    /// <summary>
    /// Validator برای ویرایش ترمینال POS
    /// </summary>
    public class PosTerminalEditViewModelValidator : AbstractValidator<PosTerminalEditViewModel>
    {
        public PosTerminalEditViewModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("شناسه ترمینال باید بیشتر از صفر باشد");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("نام ترمینال الزامی است")
                .MaximumLength(100)
                .WithMessage("نام ترمینال نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.SerialNumber)
                .NotEmpty()
                .WithMessage("شماره سریال الزامی است")
                .MaximumLength(50)
                .WithMessage("شماره سریال نمی‌تواند بیشتر از 50 کاراکتر باشد")
                .Matches(@"^[A-Za-z0-9\-_]+$")
                .WithMessage("شماره سریال باید شامل حروف، اعداد، خط تیره و زیرخط باشد");

            RuleFor(x => x.ProviderType)
                .IsInEnum()
                .WithMessage("نوع ارائه‌دهنده نامعتبر است");

            RuleFor(x => x.Protocol)
                .IsInEnum()
                .WithMessage("پروتکل نامعتبر است");

            RuleFor(x => x.ConnectionString)
                .NotEmpty()
                .WithMessage("رشته اتصال الزامی است")
                .MaximumLength(500)
                .WithMessage("رشته اتصال نمی‌تواند بیشتر از 500 کاراکتر باشد");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد");

            // Validation for specific provider types
            RuleFor(x => x.ConnectionString)
                .Must(BeValidConnectionString)
                .When(x => x.ProviderType == PosProviderType.SamanKish)
                .WithMessage("رشته اتصال برای Samank نامعتبر است");

            RuleFor(x => x.ConnectionString)
                .Must(BeValidConnectionString)
                .When(x => x.ProviderType == PosProviderType.AsanPardakht)
                .WithMessage("رشته اتصال برای Parsian نامعتبر است");
        }

        private bool BeValidConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return false;

            // Basic validation for connection string format
            return connectionString.Contains("=") && connectionString.Contains(";");
        }
    }

    /// <summary>
    /// Validator برای جستجوی ترمینال‌های POS
    /// </summary>
    public class PosTerminalSearchViewModelValidator : AbstractValidator<PosTerminalSearchViewModel>
    {
        public PosTerminalSearchViewModelValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Name))
                .WithMessage("نام ترمینال نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.SerialNumber)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.SerialNumber))
                .WithMessage("شماره سریال نمی‌تواند بیشتر از 50 کاراکتر باشد");

            RuleFor(x => x.ProviderType)
                .IsInEnum()
                .When(x => x.ProviderType.HasValue)
                .WithMessage("نوع ارائه‌دهنده نامعتبر است");

            RuleFor(x => x.Protocol)
                .IsInEnum()
                .When(x => x.Protocol.HasValue)
                .WithMessage("پروتکل نامعتبر است");

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("تاریخ شروع باید کمتر یا مساوی تاریخ پایان باشد");
        }
    }

    #endregion

    #region Cash Session Validators

    /// <summary>
    /// Validator برای شروع جلسه نقدی
    /// </summary>
    public class CashSessionStartViewModelValidator : AbstractValidator<CashSessionStartViewModel>
    {
        public CashSessionStartViewModelValidator()
        {
            RuleFor(x => x.InitialCashAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ اولیه نقدی باید مثبت باشد")
                .LessThanOrEqualTo(100000000) // 100 میلیون تومان
                .WithMessage("مبلغ اولیه نقدی نمی‌تواند بیشتر از 100 میلیون تومان باشد");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد");
        }
    }

    /// <summary>
    /// Validator برای پایان جلسه نقدی
    /// </summary>
    public class CashSessionEndViewModelValidator : AbstractValidator<CashSessionEndViewModel>
    {
        public CashSessionEndViewModelValidator()
        {
            RuleFor(x => x.FinalCashAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ نهایی نقدی باید مثبت باشد")
                .LessThanOrEqualTo(100000000) // 100 میلیون تومان
                .WithMessage("مبلغ نهایی نقدی نمی‌تواند بیشتر از 100 میلیون تومان باشد");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد");
        }
    }

    /// <summary>
    /// Validator برای جستجوی جلسات نقدی
    /// </summary>
    public class CashSessionSearchViewModelValidator : AbstractValidator<CashSessionSearchViewModel>
    {
        public CashSessionSearchViewModelValidator()
        {
            RuleFor(x => x.SessionNumber)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.SessionNumber))
                .WithMessage("شماره جلسه نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.UserName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.UserName))
                .WithMessage("نام کاربر نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.Status)
                .IsInEnum()
                .When(x => x.Status.HasValue)
                .WithMessage("وضعیت جلسه نقدی نامعتبر است");

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("تاریخ شروع باید کمتر یا مساوی تاریخ پایان باشد");

            RuleFor(x => x.MinAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinAmount.HasValue)
                .WithMessage("مبلغ حداقل باید مثبت باشد");

            RuleFor(x => x.MaxAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxAmount.HasValue)
                .WithMessage("مبلغ حداکثر باید مثبت باشد");

            RuleFor(x => x.MaxAmount)
                .GreaterThanOrEqualTo(x => x.MinAmount)
                .When(x => x.MinAmount.HasValue && x.MaxAmount.HasValue)
                .WithMessage("مبلغ حداکثر باید بیشتر یا مساوی مبلغ حداقل باشد");
        }
    }

    #endregion

    #region POS Statistics Validators

    /// <summary>
    /// Validator برای آمار POS
    /// </summary>
    public class PosStatisticsViewModelValidator : AbstractValidator<PosStatisticsViewModel>
    {
        public PosStatisticsViewModelValidator()
        {
            RuleFor(x => x.TotalTerminals)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد کل ترمینال‌ها باید مثبت باشد");

            RuleFor(x => x.ActiveTerminals)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد ترمینال‌های فعال باید مثبت باشد");

            RuleFor(x => x.InactiveTerminals)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد ترمینال‌های غیرفعال باید مثبت باشد");

            RuleFor(x => x.DefaultTerminals)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد ترمینال‌های پیش‌فرض باید مثبت باشد");

            RuleFor(x => x.TotalSessions)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد کل جلسات باید مثبت باشد");

            RuleFor(x => x.ActiveSessions)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد جلسات فعال باید مثبت باشد");

            RuleFor(x => x.CompletedSessions)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد جلسات تکمیل شده باید مثبت باشد");

            RuleFor(x => x.TotalCashHandled)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ کل نقدی باید مثبت باشد");

            RuleFor(x => x.TotalPosAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ کل POS باید مثبت باشد");

            RuleFor(x => x.TotalCashAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ کل نقدی باید مثبت باشد");

            RuleFor(x => x.AverageSessionAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ متوسط جلسه باید مثبت باشد");

            RuleFor(x => x.AverageSessionDuration)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مدت زمان متوسط جلسه باید مثبت باشد");
        }
    }

    #endregion

    #region POS Filter Validators

    /// <summary>
    /// Validator برای فیلترهای POS
    /// </summary>
    public class PosFilterViewModelValidator : AbstractValidator<PosFilterViewModel>
    {
        public PosFilterViewModelValidator()
        {
            RuleFor(x => x.PosProviders)
                .NotNull()
                .WithMessage("لیست ارائه‌دهندگان POS نمی‌تواند null باشد");

            RuleFor(x => x.PosProtocols)
                .NotNull()
                .WithMessage("لیست پروتکل‌های POS نمی‌تواند null باشد");

            RuleFor(x => x.CashSessionStatuses)
                .NotNull()
                .WithMessage("لیست وضعیت‌های جلسه نقدی نمی‌تواند null باشد");

            RuleFor(x => x.Users)
                .NotNull()
                .WithMessage("لیست کاربران نمی‌تواند null باشد");
        }
    }

    #endregion

    #region POS Lookup Validators

    /// <summary>
    /// Validator برای Lookup ارائه‌دهندگان POS
    /// </summary>
    public class PosProviderLookupViewModelValidator : AbstractValidator<PosProviderLookupViewModel>
    {
        public PosProviderLookupViewModelValidator()
        {
            RuleFor(x => x.Value)
                .IsInEnum()
                .WithMessage("نوع ارائه‌دهنده POS نامعتبر است");

            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .WithMessage("نام نمایشی الزامی است")
                .MaximumLength(100)
                .WithMessage("نام نمایشی نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد");
        }
    }

    /// <summary>
    /// Validator برای Lookup پروتکل‌های POS
    /// </summary>
    public class PosProtocolLookupViewModelValidator : AbstractValidator<PosProtocolLookupViewModel>
    {
        public PosProtocolLookupViewModelValidator()
        {
            RuleFor(x => x.Value)
                .IsInEnum()
                .WithMessage("پروتکل POS نامعتبر است");

            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .WithMessage("نام نمایشی الزامی است")
                .MaximumLength(100)
                .WithMessage("نام نمایشی نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد");
        }
    }

    /// <summary>
    /// Validator برای Lookup وضعیت‌های جلسه نقدی
    /// </summary>
    public class CashSessionStatusLookupViewModelValidator : AbstractValidator<CashSessionStatusLookupViewModel>
    {
        public CashSessionStatusLookupViewModelValidator()
        {
            RuleFor(x => x.Value)
                .IsInEnum()
                .WithMessage("وضعیت جلسه نقدی نامعتبر است");

            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .WithMessage("نام نمایشی الزامی است")
                .MaximumLength(100)
                .WithMessage("نام نمایشی نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد");
        }
    }

    #endregion
}
