using ClinicApp.ViewModels.Payment;
using FluentValidation;
using System;
using System.Linq;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.ViewModels.Validators.Payment
{
    #region Payment Transaction Validators

    /// <summary>
    /// Validator برای ایجاد تراکنش پرداخت
    /// </summary>
    public class PaymentTransactionCreateViewModelValidator : AbstractValidator<PaymentTransactionCreateViewModel>
    {
        public PaymentTransactionCreateViewModelValidator()
        {
            RuleFor(x => x.ReceptionId)
                .GreaterThan(0)
                .WithMessage("شناسه پذیرش باید بیشتر از صفر باشد");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("مبلغ پرداخت باید بیشتر از صفر باشد")
                .LessThanOrEqualTo(100000000) // 100 میلیون تومان
                .WithMessage("مبلغ پرداخت نمی‌تواند بیشتر از 100 میلیون تومان باشد");

            RuleFor(x => x.Method)
                .IsInEnum()
                .WithMessage("روش پرداخت نامعتبر است");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد");

            RuleFor(x => x.PosTerminalId)
                .GreaterThan(0)
                .When(x => x.Method == PaymentMethod.POS)
                .WithMessage("شناسه ترمینال POS برای پرداخت POS الزامی است");

            RuleFor(x => x.PaymentGatewayId)
                .GreaterThan(0)
                .When(x => x.Method == PaymentMethod.Online)
                .WithMessage("شناسه درگاه پرداخت برای پرداخت آنلاین الزامی است");

            RuleFor(x => x.TransactionId)
                .MaximumLength(100)
                .WithMessage("شناسه تراکنش نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.ReferenceCode)
                .MaximumLength(100)
                .WithMessage("کد مرجع نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.ReceiptNo)
                .MaximumLength(100)
                .WithMessage("شماره رسید نمی‌تواند بیشتر از 100 کاراکتر باشد");
        }
    }

    /// <summary>
    /// Validator برای ویرایش تراکنش پرداخت
    /// </summary>
    public class PaymentTransactionEditViewModelValidator : AbstractValidator<PaymentTransactionEditViewModel>
    {
        public PaymentTransactionEditViewModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("شناسه تراکنش باید بیشتر از صفر باشد");

            RuleFor(x => x.ReceptionId)
                .GreaterThan(0)
                .WithMessage("شناسه پذیرش باید بیشتر از صفر باشد");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("مبلغ پرداخت باید بیشتر از صفر باشد")
                .LessThanOrEqualTo(100000000) // 100 میلیون تومان
                .WithMessage("مبلغ پرداخت نمی‌تواند بیشتر از 100 میلیون تومان باشد");

            RuleFor(x => x.Method)
                .IsInEnum()
                .WithMessage("روش پرداخت نامعتبر است");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("وضعیت پرداخت نامعتبر است");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد");

            RuleFor(x => x.PosTerminalId)
                .GreaterThan(0)
                .When(x => x.Method == PaymentMethod.POS)
                .WithMessage("شناسه ترمینال POS برای پرداخت POS الزامی است");

            RuleFor(x => x.PaymentGatewayId)
                .GreaterThan(0)
                .When(x => x.Method == PaymentMethod.Online)
                .WithMessage("شناسه درگاه پرداخت برای پرداخت آنلاین الزامی است");

            RuleFor(x => x.TransactionId)
                .MaximumLength(100)
                .WithMessage("شناسه تراکنش نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.ReferenceCode)
                .MaximumLength(100)
                .WithMessage("کد مرجع نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.ReceiptNo)
                .MaximumLength(100)
                .WithMessage("شماره رسید نمی‌تواند بیشتر از 100 کاراکتر باشد");
        }
    }

    /// <summary>
    /// Validator برای جستجوی تراکنش‌های پرداخت
    /// </summary>
    public class PaymentTransactionSearchViewModelValidator : AbstractValidator<ClinicApp.ViewModels.Payment.PaymentTransactionListViewModel.PaymentTransactionSearchViewModel>
    {
        public PaymentTransactionSearchViewModelValidator()
        {
            RuleFor(x => x.ReceptionId)
                .GreaterThan(0)
                .When(x => x.ReceptionId.HasValue)
                .WithMessage("شناسه پذیرش باید بیشتر از صفر باشد");

            RuleFor(x => x.PatientId)
                .GreaterThan(0)
                .When(x => x.PatientId.HasValue)
                .WithMessage("شناسه بیمار باید بیشتر از صفر باشد");

            RuleFor(x => x.Method)
                .IsInEnum()
                .When(x => x.Method.HasValue)
                .WithMessage("روش پرداخت نامعتبر است");

            RuleFor(x => x.Status)
                .IsInEnum()
                .When(x => x.Status.HasValue)
                .WithMessage("وضعیت پرداخت نامعتبر است");

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

            RuleFor(x => x.TransactionId)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.TransactionId))
                .WithMessage("شناسه تراکنش نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.ReferenceCode)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.ReferenceCode))
                .WithMessage("کد مرجع نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.ReceiptNo)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.ReceiptNo))
                .WithMessage("شماره رسید نمی‌تواند بیشتر از 100 کاراکتر باشد");

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

    #region Payment Receipt Validators

    /// <summary>
    /// Validator برای رسید پرداخت
    /// </summary>
    public class PaymentReceiptViewModelValidator : AbstractValidator<PaymentReceiptViewModel>
    {
        public PaymentReceiptViewModelValidator()
        {
            RuleFor(x => x.TransactionId)
                .NotEmpty()
                .WithMessage("شناسه تراکنش الزامی است");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("مبلغ پرداخت باید بیشتر از صفر باشد");

            RuleFor(x => x.Method)
                .IsInEnum()
                .WithMessage("روش پرداخت نامعتبر است");

            RuleFor(x => x.TransactionNumber)
                .NotEmpty()
                .WithMessage("شماره تراکنش الزامی است")
                .MaximumLength(100)
                .WithMessage("شماره تراکنش نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.ReceiptNumber)
                .NotEmpty()
                .WithMessage("شماره رسید الزامی است")
                .MaximumLength(100)
                .WithMessage("شماره رسید نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.PatientName)
                .NotEmpty()
                .WithMessage("نام بیمار الزامی است")
                .MaximumLength(100)
                .WithMessage("نام بیمار نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.DoctorName)
                .NotEmpty()
                .WithMessage("نام پزشک الزامی است")
                .MaximumLength(100)
                .WithMessage("نام پزشک نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.ClinicName)
                .NotEmpty()
                .WithMessage("نام کلینیک الزامی است")
                .MaximumLength(100)
                .WithMessage("نام کلینیک نمی‌تواند بیشتر از 100 کاراکتر باشد");
        }
    }

    #endregion

    #region Payment Statistics Validators

    /// <summary>
    /// Validator برای آمار پرداخت‌ها
    /// </summary>
    public class PaymentStatisticsViewModelValidator : AbstractValidator<PaymentStatisticsViewModel>
    {
        public PaymentStatisticsViewModelValidator()
        {
            RuleFor(x => x.TotalTransactions)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد کل تراکنش‌ها باید مثبت باشد");

            RuleFor(x => x.TotalAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ کل باید مثبت باشد");

            RuleFor(x => x.AverageAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ متوسط باید مثبت باشد");

            RuleFor(x => x.SuccessfulTransactions)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد تراکنش‌های موفق باید مثبت باشد");

            RuleFor(x => x.FailedTransactions)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد تراکنش‌های ناموفق باید مثبت باشد");

            RuleFor(x => x.PendingTransactions)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد تراکنش‌های در انتظار باید مثبت باشد");

            RuleFor(x => x.SuccessRate)
                .InclusiveBetween(0, 100)
                .WithMessage("نرخ موفقیت باید بین 0 تا 100 باشد");

            RuleFor(x => x.CashAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ نقدی باید مثبت باشد");

            RuleFor(x => x.PosAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ POS باید مثبت باشد");

            RuleFor(x => x.OnlineAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ آنلاین باید مثبت باشد");

            RuleFor(x => x.DebtAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ بدهی باید مثبت باشد");
        }
    }

    #endregion

    #region Payment Filter Validators

    /// <summary>
    /// Validator برای فیلترهای پرداخت
    /// </summary>
    public class PaymentFilterViewModelValidator : AbstractValidator<PaymentFilterViewModel>
    {
        public PaymentFilterViewModelValidator()
        {
            RuleFor(x => x.PaymentMethods)
                .NotNull()
                .WithMessage("لیست روش‌های پرداخت نمی‌تواند null باشد");

            RuleFor(x => x.PaymentStatuses)
                .NotNull()
                .WithMessage("لیست وضعیت‌های پرداخت نمی‌تواند null باشد");

            RuleFor(x => x.PaymentGateways)
                .NotNull()
                .WithMessage("لیست درگاه‌های پرداخت نمی‌تواند null باشد");

            RuleFor(x => x.PosProviders)
                .NotNull()
                .WithMessage("لیست ارائه‌دهندگان POS نمی‌تواند null باشد");

            RuleFor(x => x.PosProtocols)
                .NotNull()
                .WithMessage("لیست پروتکل‌های POS نمی‌تواند null باشد");
        }
    }

    #endregion

    #region Payment Lookup Validators

    /// <summary>
    /// Validator برای Lookup روش‌های پرداخت
    /// </summary>
    public class PaymentMethodLookupViewModelValidator : AbstractValidator<PaymentMethodLookupViewModel>
    {
        public PaymentMethodLookupViewModelValidator()
        {
            RuleFor(x => x.Value)
                .IsInEnum()
                .WithMessage("روش پرداخت نامعتبر است");

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
    /// Validator برای Lookup وضعیت‌های پرداخت
    /// </summary>
    public class PaymentStatusLookupViewModelValidator : AbstractValidator<PaymentStatusLookupViewModel>
    {
        public PaymentStatusLookupViewModelValidator()
        {
            RuleFor(x => x.Value)
                .IsInEnum()
                .WithMessage("وضعیت پرداخت نامعتبر است");

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
    /// Validator برای Lookup درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewayLookupViewModelValidator : AbstractValidator<PaymentGatewayLookupViewModel>
    {
        public PaymentGatewayLookupViewModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("شناسه درگاه پرداخت باید بیشتر از صفر باشد");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("نام درگاه پرداخت الزامی است")
                .MaximumLength(100)
                .WithMessage("نام درگاه پرداخت نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.GatewayType)
                .IsInEnum()
                .WithMessage("نوع درگاه پرداخت نامعتبر است");
        }
    }

    /// <summary>
    /// Validator برای Lookup ترمینال‌های POS
    /// </summary>
    public class PosTerminalLookupViewModelValidator : AbstractValidator<PosTerminalLookupViewModel>
    {
        public PosTerminalLookupViewModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("شناسه ترمینال POS باید بیشتر از صفر باشد");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("نام ترمینال POS الزامی است")
                .MaximumLength(100)
                .WithMessage("نام ترمینال POS نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.SerialNumber)
                .NotEmpty()
                .WithMessage("شماره سریال الزامی است")
                .MaximumLength(50)
                .WithMessage("شماره سریال نمی‌تواند بیشتر از 50 کاراکتر باشد");

            RuleFor(x => x.ProviderType)
                .IsInEnum()
                .WithMessage("نوع ارائه‌دهنده نامعتبر است");
        }
    }

    /// <summary>
    /// Validator برای Lookup جلسات نقدی
    /// </summary>
    public class CashSessionLookupViewModelValidator : AbstractValidator<CashSessionLookupViewModel>
    {
        public CashSessionLookupViewModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("شناسه جلسه نقدی باید بیشتر از صفر باشد");

            RuleFor(x => x.SessionNumber)
                .NotEmpty()
                .WithMessage("شماره جلسه الزامی است")
                .MaximumLength(100)
                .WithMessage("شماره جلسه نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("نام کاربر الزامی است")
                .MaximumLength(100)
                .WithMessage("نام کاربر نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("وضعیت جلسه نقدی نامعتبر است");
        }
    }

    #endregion
}
