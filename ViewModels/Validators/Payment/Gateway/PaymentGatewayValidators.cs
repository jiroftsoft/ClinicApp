using ClinicApp.ViewModels.Payment.Gateway;
using FluentValidation;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Validators.Payment.Gateway
{
    #region Payment Gateway Validators

    /// <summary>
    /// Validator برای ایجاد درگاه پرداخت
    /// </summary>
    public class PaymentGatewayCreateViewModelValidator : AbstractValidator<PaymentGatewayCreateViewModel>
    {
        public PaymentGatewayCreateViewModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("نام درگاه الزامی است")
                .MaximumLength(100)
                .WithMessage("نام درگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.GatewayType)
                .IsInEnum()
                .WithMessage("نوع درگاه نامعتبر است");

            RuleFor(x => x.MerchantId)
                .NotEmpty()
                .WithMessage("MerchantId الزامی است")
                .MaximumLength(100)
                .WithMessage("MerchantId نمی‌تواند بیشتر از 100 کاراکتر باشد")
                .Matches(@"^[A-Za-z0-9\-_]+$")
                .WithMessage("MerchantId باید شامل حروف، اعداد، خط تیره و زیرخط باشد");

            RuleFor(x => x.ApiKey)
                .NotEmpty()
                .WithMessage("ApiKey الزامی است")
                .MaximumLength(200)
                .WithMessage("ApiKey نمی‌تواند بیشتر از 200 کاراکتر باشد");

            RuleFor(x => x.ApiSecret)
                .NotEmpty()
                .WithMessage("ApiSecret الزامی است")
                .MaximumLength(200)
                .WithMessage("ApiSecret نمی‌تواند بیشتر از 200 کاراکتر باشد");

            RuleFor(x => x.CallbackUrl)
                .NotEmpty()
                .WithMessage("آدرس Callback الزامی است")
                .MaximumLength(500)
                .WithMessage("آدرس Callback نمی‌تواند بیشتر از 500 کاراکتر باشد")
                .Must(BeValidUrl)
                .WithMessage("آدرس Callback باید معتبر باشد");

            RuleFor(x => x.WebhookUrl)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.WebhookUrl))
                .WithMessage("آدرس Webhook نمی‌تواند بیشتر از 500 کاراکتر باشد")
                .Must(BeValidUrl)
                .When(x => !string.IsNullOrWhiteSpace(x.WebhookUrl))
                .WithMessage("آدرس Webhook باید معتبر باشد");

            RuleFor(x => x.FeePercentage)
                .InclusiveBetween(0, 100)
                .WithMessage("درصد کارمزد باید بین 0 تا 100 باشد");

            RuleFor(x => x.FixedFee)
                .GreaterThanOrEqualTo(0)
                .WithMessage("کارمزد ثابت باید مثبت باشد")
                .LessThanOrEqualTo(1000000) // 1 میلیون تومان
                .WithMessage("کارمزد ثابت نمی‌تواند بیشتر از 1 میلیون تومان باشد");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد");

            // Validation for specific gateway types
            RuleFor(x => x.MerchantId)
                .Must(BeValidZarinPalMerchantId)
                .When(x => x.GatewayType == PaymentGatewayType.ZarinPal)
                .WithMessage("MerchantId برای ZarinPal نامعتبر است");

            RuleFor(x => x.MerchantId)
                .Must(BeValidPayPingMerchantId)
                .When(x => x.GatewayType == PaymentGatewayType.PayPing)
                .WithMessage("MerchantId برای PayPing نامعتبر است");
        }

        private bool BeValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out Uri result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        private bool BeValidZarinPalMerchantId(string merchantId)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
                return false;

            // ZarinPal merchant ID should be a GUID
            return Guid.TryParse(merchantId, out _);
        }

        private bool BeValidPayPingMerchantId(string merchantId)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
                return false;

            // PayPing merchant ID should be alphanumeric
            return Regex.IsMatch(merchantId, @"^[A-Za-z0-9]+$");
        }
    }

    /// <summary>
    /// Validator برای ویرایش درگاه پرداخت
    /// </summary>
    public class PaymentGatewayEditViewModelValidator : AbstractValidator<PaymentGatewayEditViewModel>
    {
        public PaymentGatewayEditViewModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("شناسه درگاه باید بیشتر از صفر باشد");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("نام درگاه الزامی است")
                .MaximumLength(100)
                .WithMessage("نام درگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.GatewayType)
                .IsInEnum()
                .WithMessage("نوع درگاه نامعتبر است");

            RuleFor(x => x.MerchantId)
                .NotEmpty()
                .WithMessage("MerchantId الزامی است")
                .MaximumLength(100)
                .WithMessage("MerchantId نمی‌تواند بیشتر از 100 کاراکتر باشد")
                .Matches(@"^[A-Za-z0-9\-_]+$")
                .WithMessage("MerchantId باید شامل حروف، اعداد، خط تیره و زیرخط باشد");

            RuleFor(x => x.ApiKey)
                .NotEmpty()
                .WithMessage("ApiKey الزامی است")
                .MaximumLength(200)
                .WithMessage("ApiKey نمی‌تواند بیشتر از 200 کاراکتر باشد");

            RuleFor(x => x.ApiSecret)
                .NotEmpty()
                .WithMessage("ApiSecret الزامی است")
                .MaximumLength(200)
                .WithMessage("ApiSecret نمی‌تواند بیشتر از 200 کاراکتر باشد");

            RuleFor(x => x.CallbackUrl)
                .NotEmpty()
                .WithMessage("آدرس Callback الزامی است")
                .MaximumLength(500)
                .WithMessage("آدرس Callback نمی‌تواند بیشتر از 500 کاراکتر باشد")
                .Must(BeValidUrl)
                .WithMessage("آدرس Callback باید معتبر باشد");

            RuleFor(x => x.WebhookUrl)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.WebhookUrl))
                .WithMessage("آدرس Webhook نمی‌تواند بیشتر از 500 کاراکتر باشد")
                .Must(BeValidUrl)
                .When(x => !string.IsNullOrWhiteSpace(x.WebhookUrl))
                .WithMessage("آدرس Webhook باید معتبر باشد");

            RuleFor(x => x.FeePercentage)
                .InclusiveBetween(0, 100)
                .WithMessage("درصد کارمزد باید بین 0 تا 100 باشد");

            RuleFor(x => x.FixedFee)
                .GreaterThanOrEqualTo(0)
                .WithMessage("کارمزد ثابت باید مثبت باشد")
                .LessThanOrEqualTo(1000000) // 1 میلیون تومان
                .WithMessage("کارمزد ثابت نمی‌تواند بیشتر از 1 میلیون تومان باشد");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد");

            // Validation for specific gateway types
            RuleFor(x => x.MerchantId)
                .Must(BeValidZarinPalMerchantId)
                .When(x => x.GatewayType == PaymentGatewayType.ZarinPal)
                .WithMessage("MerchantId برای ZarinPal نامعتبر است");

            RuleFor(x => x.MerchantId)
                .Must(BeValidPayPingMerchantId)
                .When(x => x.GatewayType == PaymentGatewayType.PayPing)
                .WithMessage("MerchantId برای PayPing نامعتبر است");
        }

        private bool BeValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out Uri result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        private bool BeValidZarinPalMerchantId(string merchantId)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
                return false;

            // ZarinPal merchant ID should be a GUID
            return Guid.TryParse(merchantId, out _);
        }

        private bool BeValidPayPingMerchantId(string merchantId)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
                return false;

            // PayPing merchant ID should be alphanumeric
            return Regex.IsMatch(merchantId, @"^[A-Za-z0-9]+$");
        }
    }

    /// <summary>
    /// Validator برای جستجوی درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewaySearchViewModelValidator : AbstractValidator<PaymentGatewaySearchViewModel>
    {
        public PaymentGatewaySearchViewModelValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Name))
                .WithMessage("نام درگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.GatewayType)
                .IsInEnum()
                .When(x => x.GatewayType.HasValue)
                .WithMessage("نوع درگاه نامعتبر است");

            RuleFor(x => x.MerchantId)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.MerchantId))
                .WithMessage("MerchantId نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("تاریخ شروع باید کمتر یا مساوی تاریخ پایان باشد");
        }
    }

    #endregion

    #region Online Payment Validators

    /// <summary>
    /// Validator برای ایجاد پرداخت آنلاین
    /// </summary>
    public class OnlinePaymentCreateViewModelValidator : AbstractValidator<OnlinePaymentCreateViewModel>
    {
        public OnlinePaymentCreateViewModelValidator()
        {
            RuleFor(x => x.ReceptionId)
                .GreaterThan(0)
                .WithMessage("شناسه پذیرش باید بیشتر از صفر باشد");

            RuleFor(x => x.PatientId)
                .GreaterThan(0)
                .WithMessage("شناسه بیمار باید بیشتر از صفر باشد");

            RuleFor(x => x.PaymentType)
                .IsInEnum()
                .WithMessage("نوع پرداخت نامعتبر است");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("مبلغ پرداخت باید بیشتر از صفر باشد")
                .LessThanOrEqualTo(100000000) // 100 میلیون تومان
                .WithMessage("مبلغ پرداخت نمی‌تواند بیشتر از 100 میلیون تومان باشد");

            RuleFor(x => x.PaymentGatewayId)
                .GreaterThan(0)
                .WithMessage("شناسه درگاه پرداخت باید بیشتر از صفر باشد");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد");

            RuleFor(x => x.UserIpAddress)
                .MaximumLength(45)
                .When(x => !string.IsNullOrWhiteSpace(x.UserIpAddress))
                .WithMessage("آدرس IP نمی‌تواند بیشتر از 45 کاراکتر باشد")
                .Must(BeValidIpAddress)
                .When(x => !string.IsNullOrWhiteSpace(x.UserIpAddress))
                .WithMessage("آدرس IP نامعتبر است");

            RuleFor(x => x.UserAgent)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.UserAgent))
                .WithMessage("User Agent نمی‌تواند بیشتر از 500 کاراکتر باشد");
        }

        private bool BeValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            return System.Net.IPAddress.TryParse(ipAddress, out _);
        }
    }

    /// <summary>
    /// Validator برای جستجوی پرداخت‌های آنلاین
    /// </summary>
    public class OnlinePaymentSearchViewModelValidator : AbstractValidator<OnlinePaymentSearchViewModel>
    {
        public OnlinePaymentSearchViewModelValidator()
        {
            RuleFor(x => x.ReceptionId)
                .GreaterThan(0)
                .When(x => x.ReceptionId.HasValue)
                .WithMessage("شناسه پذیرش باید بیشتر از صفر باشد");

            RuleFor(x => x.PatientId)
                .GreaterThan(0)
                .When(x => x.PatientId.HasValue)
                .WithMessage("شناسه بیمار باید بیشتر از صفر باشد");

            RuleFor(x => x.PaymentType)
                .IsInEnum()
                .When(x => x.PaymentType.HasValue)
                .WithMessage("نوع پرداخت نامعتبر است");

            RuleFor(x => x.Status)
                .IsInEnum()
                .When(x => x.Status.HasValue)
                .WithMessage("وضعیت پرداخت نامعتبر است");

            RuleFor(x => x.PaymentGatewayType)
                .IsInEnum()
                .When(x => x.PaymentGatewayType.HasValue)
                .WithMessage("نوع درگاه پرداخت نامعتبر است");

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

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("تاریخ شروع باید کمتر یا مساوی تاریخ پایان باشد");

            RuleFor(x => x.PaymentToken)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.PaymentToken))
                .WithMessage("توکن پرداخت نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.GatewayTransactionId)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.GatewayTransactionId))
                .WithMessage("شناسه تراکنش درگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.PatientName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.PatientName))
                .WithMessage("نام بیمار نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.DoctorName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.DoctorName))
                .WithMessage("نام پزشک نمی‌تواند بیشتر از 100 کاراکتر باشد");
        }
    }

    #endregion

    #region Gateway Statistics Validators

    /// <summary>
    /// Validator برای آمار درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewayStatisticsViewModelValidator : AbstractValidator<PaymentGatewayStatisticsViewModel>
    {
        public PaymentGatewayStatisticsViewModelValidator()
        {
            RuleFor(x => x.TotalGateways)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد کل درگاه‌ها باید مثبت باشد");

            RuleFor(x => x.ActiveGateways)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد درگاه‌های فعال باید مثبت باشد");

            RuleFor(x => x.InactiveGateways)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد درگاه‌های غیرفعال باید مثبت باشد");

            RuleFor(x => x.DefaultGateways)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد درگاه‌های پیش‌فرض باید مثبت باشد");

            RuleFor(x => x.TotalOnlinePayments)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد کل پرداخت‌های آنلاین باید مثبت باشد");

            RuleFor(x => x.SuccessfulPayments)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد پرداخت‌های موفق باید مثبت باشد");

            RuleFor(x => x.FailedPayments)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد پرداخت‌های ناموفق باید مثبت باشد");

            RuleFor(x => x.PendingPayments)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد پرداخت‌های در انتظار باید مثبت باشد");

            RuleFor(x => x.TotalAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ کل باید مثبت باشد");

            RuleFor(x => x.SuccessfulAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ موفق باید مثبت باشد");

            RuleFor(x => x.FailedAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ ناموفق باید مثبت باشد");

            RuleFor(x => x.PendingAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ در انتظار باید مثبت باشد");

            RuleFor(x => x.SuccessRate)
                .InclusiveBetween(0, 100)
                .WithMessage("نرخ موفقیت باید بین 0 تا 100 باشد");

            RuleFor(x => x.AverageResponseTime)
                .GreaterThanOrEqualTo(0)
                .WithMessage("زمان پاسخ متوسط باید مثبت باشد");
        }
    }

    #endregion

    #region Gateway Filter Validators

    /// <summary>
    /// Validator برای فیلترهای درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewayFilterViewModelValidator : AbstractValidator<PaymentGatewayFilterViewModel>
    {
        public PaymentGatewayFilterViewModelValidator()
        {
            RuleFor(x => x.PaymentGateways)
                .NotNull()
                .WithMessage("لیست درگاه‌های پرداخت نمی‌تواند null باشد");

            RuleFor(x => x.OnlinePaymentTypes)
                .NotNull()
                .WithMessage("لیست انواع پرداخت آنلاین نمی‌تواند null باشد");

            RuleFor(x => x.OnlinePaymentStatuses)
                .NotNull()
                .WithMessage("لیست وضعیت‌های پرداخت آنلاین نمی‌تواند null باشد");
        }
    }

    #endregion

    #region Gateway Lookup Validators

    /// <summary>
    /// Validator برای Lookup انواع درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewayTypeLookupViewModelValidator : AbstractValidator<PaymentGatewayTypeLookupViewModel>
    {
        public PaymentGatewayTypeLookupViewModelValidator()
        {
            RuleFor(x => x.Value)
                .IsInEnum()
                .WithMessage("نوع درگاه پرداخت نامعتبر است");

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
    /// Validator برای Lookup انواع پرداخت آنلاین
    /// </summary>
    public class OnlinePaymentTypeLookupViewModelValidator : AbstractValidator<OnlinePaymentTypeLookupViewModel>
    {
        public OnlinePaymentTypeLookupViewModelValidator()
        {
            RuleFor(x => x.Value)
                .IsInEnum()
                .WithMessage("نوع پرداخت آنلاین نامعتبر است");

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
    /// Validator برای Lookup وضعیت‌های پرداخت آنلاین
    /// </summary>
    public class OnlinePaymentStatusLookupViewModelValidator : AbstractValidator<OnlinePaymentStatusLookupViewModel>
    {
        public OnlinePaymentStatusLookupViewModelValidator()
        {
            RuleFor(x => x.Value)
                .IsInEnum()
                .WithMessage("وضعیت پرداخت آنلاین نامعتبر است");

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
