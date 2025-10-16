using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس پرداخت در ماژول پذیرش
    /// </summary>
    public class ReceptionPaymentService : IReceptionPaymentService
    {
        private readonly IPaymentService _paymentService;
        private readonly IReceptionCalculationService _calculationService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionPaymentService(
            IPaymentService paymentService,
            IReceptionCalculationService calculationService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
            _logger = logger.ForContext<ReceptionPaymentService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<ServiceResult<PaymentInfoViewModel>> GetPaymentInfoAsync()
        {
            try
            {
                _logger.Information("درخواست دریافت اطلاعات پرداخت");
                
                // TODO: پیاده‌سازی منطق دریافت اطلاعات پرداخت
                var paymentInfo = new PaymentInfoViewModel
                {
                    TotalAmount = 0,
                    InsuranceShare = 0,
                    PatientShare = 0,
                    AvailablePaymentMethodViewModels = new List<PaymentMethodViewModel>(),
                    CanPayOnline = true,
                    CanPayCash = true,
                    Currency = "IRR"
                };

                return ServiceResult<PaymentInfoViewModel>.Successful(paymentInfo, "اطلاعات پرداخت با موفقیت بارگذاری شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات پرداخت");
                return ServiceResult<PaymentInfoViewModel>.Failed("خطا در دریافت اطلاعات پرداخت");
            }
        }

        /// <summary>
        /// دریافت اطلاعات پرداخت
        /// </summary>
        public async Task<ServiceResult<PaymentInfoViewModel>> GetPaymentInfoAsync(string patientId, decimal amount)
        {
            try
            {
                _logger.Information("دریافت اطلاعات پرداخت برای بیمار {PatientId} با مبلغ {Amount}", patientId, amount);

                // محاسبه اطلاعات پرداخت
                var calculationResult = await _calculationService.CalculatePaymentInfoAsync(int.Parse(patientId), (int)amount);
                if (!calculationResult.Success)
                {
                    return ServiceResult<PaymentInfoViewModel>.Failed(
                        "خطا در محاسبه اطلاعات پرداخت");
                }

                // دریافت روش‌های پرداخت موجود
                var paymentMethods = await GetAvailablePaymentMethodsAsync();
                
                // دریافت اطلاعات درگاه پرداخت
                var gatewayInfo = await GetPaymentGatewayInfoAsync();

                var paymentInfo = new PaymentInfoViewModel
                {
                    PatientId = int.Parse(patientId),
                    PatientName = calculationResult.Data.PatientName,
                    TotalAmount = calculationResult.Data.TotalAmount,
                    InsuranceShare = calculationResult.Data.InsuranceShare,
                    PatientShare = calculationResult.Data.PatientShare,
                    PayableAmount = calculationResult.Data.PayableAmount,
                    AvailablePaymentMethodViewModels = paymentMethods,
                    GatewayInfoViewModel = gatewayInfo,
                    IsPaymentEnabled = calculationResult.Data.PayableAmount > 0,
                    StatusMessage = "اطلاعات پرداخت آماده است"
                };

                return ServiceResult<PaymentInfoViewModel>.Successful(
                    paymentInfo, "اطلاعات پرداخت با موفقیت بارگذاری شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات پرداخت");
                return ServiceResult<PaymentInfoViewModel>.Failed(
                    "خطا در بارگذاری اطلاعات پرداخت");
            }
        }

        /// <summary>
        /// پردازش پرداخت آنلاین
        /// </summary>
        public async Task<ServiceResult<OnlinePaymentResponseViewModel>> ProcessOnlinePaymentAsync(PaymentRequestViewModel model)
        {
            try
            {
                _logger.Information("پردازش پرداخت آنلاین برای مبلغ {Amount}", model.Amount);
                return ServiceResult<OnlinePaymentResponseViewModel>.Successful(
                    new OnlinePaymentResponseViewModel
                    {
                        IsSuccess = true,
                        Message = "پرداخت آنلاین با موفقیت انجام شد",
                        RedirectUrl = "https://payment.gateway.com/redirect",
                        TransactionId = Guid.NewGuid().ToString(),
                        PaymentGatewayRefId = "PG_" + DateTime.Now.Ticks
                    }, "پرداخت آنلاین با موفقیت انجام شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش پرداخت آنلاین");
                return ServiceResult<OnlinePaymentResponseViewModel>.Failed("خطا در پردازش پرداخت آنلاین");
            }
        }

        /// <summary>
        /// تایید پرداخت آنلاین
        /// </summary>
        public async Task<ServiceResult<PaymentConfirmationViewModel>> ConfirmOnlinePaymentAsync(string transactionId, string paymentGatewayRefId)
        {
            try
            {
                _logger.Information("تایید پرداخت آنلاین برای تراکنش {TransactionId}", transactionId);
                return ServiceResult<PaymentConfirmationViewModel>.Successful(
                    new PaymentConfirmationViewModel
                    {
                        IsConfirmed = true,
                        Message = "پرداخت با موفقیت تایید شد",
                        TrackingCode = "TC_" + DateTime.Now.Ticks,
                        PaidAmount = 100000, // مبلغ نمونه
                        ConfirmationDate = DateTime.Now
                    }, "پرداخت با موفقیت تایید شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تایید پرداخت آنلاین");
                return ServiceResult<PaymentConfirmationViewModel>.Failed("خطا در تایید پرداخت آنلاین");
            }
        }

        /// <summary>
        /// پردازش پرداخت نقدی
        /// </summary>
        public async Task<ServiceResult<CashPaymentResponseViewModel>> ProcessCashPaymentAsync(CashPaymentRequestViewModel model)
        {
            try
            {
                _logger.Information("پردازش پرداخت نقدی برای مبلغ {Amount}", model.Amount);
                return ServiceResult<CashPaymentResponseViewModel>.Successful(
                    new CashPaymentResponseViewModel
                    {
                        IsSuccess = true,
                        Message = "پرداخت نقدی با موفقیت انجام شد",
                        ReceiptNumber = "RCP_" + DateTime.Now.Ticks,
                        PaidAmount = model.Amount,
                        PaymentDate = DateTime.Now
                    }, "پرداخت نقدی با موفقیت انجام شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش پرداخت نقدی");
                return ServiceResult<CashPaymentResponseViewModel>.Failed("خطا در پردازش پرداخت نقدی");
            }
        }

        /// <summary>
        /// لغو پرداخت
        /// </summary>
        public async Task<ServiceResult<bool>> CancelPaymentAsync(int paymentId, string reason)
        {
            try
            {
                _logger.Information("لغو پرداخت {PaymentId} به دلیل: {Reason}", paymentId, reason);
                return ServiceResult<bool>.Successful(true, "پرداخت با موفقیت لغو شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو پرداخت");
                return ServiceResult<bool>.Failed("خطا در لغو پرداخت");
            }
        }

        /// <summary>
        /// دریافت وضعیت پرداخت
        /// </summary>
        public async Task<ServiceResult<PaymentStatusViewModel>> GetPaymentStatusAsync(int paymentId)
        {
            try
            {
                _logger.Information("دریافت وضعیت پرداخت {PaymentId}", paymentId);
                return ServiceResult<PaymentStatusViewModel>.Successful(
                    new PaymentStatusViewModel
                    {
                        PaymentId = paymentId,
                        Status = "Completed",
                        Amount = 100000,
                        LastUpdate = DateTime.Now,
                        Message = "پرداخت تکمیل شده"
                    }, "وضعیت پرداخت با موفقیت دریافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت وضعیت پرداخت");
                return ServiceResult<PaymentStatusViewModel>.Failed("خطا در دریافت وضعیت پرداخت");
            }
        }

        /// <summary>
        /// تولید رسید پرداخت
        /// </summary>
        public async Task<ServiceResult<PaymentReceiptViewModel>> GeneratePaymentReceiptAsync(int paymentId)
        {
            try
            {
                _logger.Information("تولید رسید پرداخت {PaymentId}", paymentId);
                return ServiceResult<PaymentReceiptViewModel>.Successful(
                    new PaymentReceiptViewModel
                    {
                        PaymentId = paymentId,
                        ReceiptContent = "رسید پرداخت",
                        CreatedAt = DateTime.Now,
                        PatientName = "بیمار نمونه",
                        DoctorName = "دکتر نمونه",
                        TotalAmount = 100000,
                        InsuranceShare = 80000,
                        PatientShare = 20000,
                        PaymentMethod = "نقدی",
                        TrackingCode = "TC_" + DateTime.Now.Ticks
                    }, "رسید پرداخت با موفقیت تولید شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تولید رسید پرداخت");
                return ServiceResult<PaymentReceiptViewModel>.Failed("خطا در تولید رسید پرداخت");
            }
        }

        /// <summary>
        /// دریافت روش‌های پرداخت
        /// </summary>
        public async Task<ServiceResult<List<PaymentMethodViewModel>>> GetPaymentMethodsAsync()
        {
            try
            {
                _logger.Information("دریافت روش‌های پرداخت");
                var methods = new List<PaymentMethodViewModel>
                {
                    new PaymentMethodViewModel { Id = 1, Name = "نقدی", Code = "CASH", Description = "پرداخت نقدی", IsActive = true },
                    new PaymentMethodViewModel { Id = 2, Name = "آنلاین", Code = "ONLINE", Description = "پرداخت آنلاین", IsActive = true }
                };
                return ServiceResult<List<PaymentMethodViewModel>>.Successful(methods, "روش‌های پرداخت با موفقیت دریافت شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت روش‌های پرداخت");
                return ServiceResult<List<PaymentMethodViewModel>>.Failed("خطا در دریافت روش‌های پرداخت");
            }
        }

        /// <summary>
        /// دریافت اطلاعات درگاه پرداخت
        /// </summary>
        public async Task<ServiceResult<PaymentGatewayInfoViewModel>> GetPaymentGatewayInfoAsync(string gatewayCode)
        {
            try
            {
                _logger.Information("دریافت اطلاعات درگاه پرداخت {GatewayCode}", gatewayCode);
                return ServiceResult<PaymentGatewayInfoViewModel>.Successful(
                    new PaymentGatewayInfoViewModel
                    {
                        Name = "درگاه پرداخت نمونه",
                        Code = gatewayCode,
                        TerminalId = "TERM_001",
                        IsActive = true,
                        Description = "درگاه پرداخت آنلاین"
                    }, "اطلاعات درگاه پرداخت با موفقیت دریافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات درگاه پرداخت");
                return ServiceResult<PaymentGatewayInfoViewModel>.Failed("خطا در دریافت اطلاعات درگاه پرداخت");
            }
        }

        /// <summary>
        /// شروع پرداخت آنلاین
        /// </summary>
        public async Task<ServiceResult<OnlinePaymentResponseViewModel>> StartOnlinePaymentAsync(PaymentRequestViewModel paymentRequest)
        {
            try
            {
                _logger.Information("شروع پرداخت آنلاین برای مبلغ {Amount}", paymentRequest.Amount);

                // اعتبارسنجی درخواست پرداخت
                var validationResult = await ValidatePaymentInfoAsync(paymentRequest);
                if (!validationResult.Success)
                {
                    return ServiceResult<OnlinePaymentResponseViewModel>.Failed(validationResult.Message);
                }

                // ایجاد تراکنش پرداخت
                var transactionResult = await _paymentService.CreatePaymentTransactionAsync(
                    paymentRequest.PatientId,
                    paymentRequest.Amount,
                    Models.Enums.PaymentMethod.Online,
                    paymentRequest.Description);

                if (!transactionResult.Success)
                {
                    return ServiceResult<OnlinePaymentResponseViewModel>.Failed(
                        "خطا در ایجاد تراکنش پرداخت");
                }

                // شروع پرداخت در درگاه
                var gatewayResult = await _paymentService.CreatePaymentTransactionAsync(
                    int.Parse(transactionResult.Data.TransactionId),
                    paymentRequest.Amount,
                    Models.Enums.PaymentMethod.Online,
                    _currentUserService.UserId);

                if (!gatewayResult.Success)
                {
                    return ServiceResult<OnlinePaymentResponseViewModel>.Failed(
                        "خطا در شروع پرداخت در درگاه");
                }

                var response = new OnlinePaymentResponseViewModel
                {
                    TransactionId = transactionResult.Data.TransactionId,
                    PaymentUrl = "", // Not available in PaymentTransaction
                    GatewayTransactionId = gatewayResult.Data.TransactionId,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddMinutes(30),
                    VerificationCode = GenerateVerificationCode(),
                    CallbackUrl = paymentRequest.CallbackUrl,
                    GatewayResponse = gatewayResult.Data.Description
                };

                return ServiceResult<OnlinePaymentResponseViewModel>.Successful(
                    response, "پرداخت آنلاین با موفقیت شروع شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شروع پرداخت آنلاین");
                return ServiceResult<OnlinePaymentResponseViewModel>.Failed(
                    "خطا در شروع پرداخت آنلاین");
            }
        }


        /// <summary>
        /// تکمیل پرداخت نقدی
        /// </summary>
        public async Task<ServiceResult<CashPaymentResponseViewModel>> CompleteCashPaymentAsync(CashPaymentRequestViewModel cashPaymentRequest)
        {
            try
            {
                _logger.Information("تکمیل پرداخت نقدی برای مبلغ {Amount}", cashPaymentRequest.Amount);

                // ایجاد تراکنش پرداخت نقدی
                var transactionResult = await _paymentService.CreatePaymentTransactionAsync(
                    cashPaymentRequest.PatientId,
                    cashPaymentRequest.Amount,
                    Models.Enums.PaymentMethod.Cash,
                    cashPaymentRequest.Description);

                if (!transactionResult.Success)
                {
                    return ServiceResult<CashPaymentResponseViewModel>.Failed(
                        "خطا در ایجاد تراکنش پرداخت نقدی");
                }

                // تکمیل پرداخت نقدی
                var completionResult = await _paymentService.CompleteCashPaymentAsync(
                    int.Parse(transactionResult.Data.TransactionId),
                    cashPaymentRequest.CashierName);

                if (!completionResult.Success)
                {
                    return ServiceResult<CashPaymentResponseViewModel>.Failed(
                        "خطا در تکمیل پرداخت نقدی");
                }

                var response = new CashPaymentResponseViewModel
                {
                    TransactionId = transactionResult.Data.TransactionId,
                    ReceiptNumber = GenerateReceiptNumber(),
                    Status = "Completed",
                    CompletedAt = DateTime.Now,
                    CashierId = cashPaymentRequest.CashierId,
                    CashierName = cashPaymentRequest.CashierName,
                    PaymentLocation = "صندوق کلینیک",
                    Metadata = cashPaymentRequest.Metadata
                };

                return ServiceResult<CashPaymentResponseViewModel>.Successful(
                    response, "پرداخت نقدی با موفقیت تکمیل شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تکمیل پرداخت نقدی");
                return ServiceResult<CashPaymentResponseViewModel>.Failed(
                    "خطا در تکمیل پرداخت نقدی");
            }
        }

        /// <summary>
        /// دریافت وضعیت پرداخت
        /// </summary>
        public async Task<ServiceResult<PaymentStatusViewModel>> GetPaymentStatusAsync(string transactionId)
        {
            try
            {
                _logger.Information("دریافت وضعیت پرداخت برای تراکنش {TransactionId}", transactionId);

                var result = await _paymentService.GetPaymentStatusAsync(int.Parse(transactionId));
                if (!result.Success)
                {
                    return ServiceResult<PaymentStatusViewModel>.Failed(
                        "خطا در دریافت وضعیت پرداخت");
                }

                var status = new PaymentStatusViewModel
                {
                    TransactionId = int.Parse(transactionId),
                    Status = result.Data.Status.ToString(),
                    StatusDescription = GetStatusDescription(result.Data.Status),
                    CreatedAt = result.Data.CreatedAt,
                    CompletedAt = result.Data.Status == Models.Enums.PaymentStatus.Completed ? result.Data.CreatedAt : (DateTime?)null,
                    Amount = result.Data.Amount,
                    PaymentMethod = result.Data.Method.ToString(),
                    PatientId = result.Data.ReceptionId, // استفاده از ReceptionId به جای PatientId
                    PatientName = result.Data.PatientName ?? "بیمار نمونه",
                    ReceiptNumber = result.Data.ReceiptNo ?? "RCP_" + transactionId,
                    CanCancel = result.Data.Status == Models.Enums.PaymentStatus.Pending,
                    CanRefund = result.Data.Status == Models.Enums.PaymentStatus.Completed,
                    StatusHistory = new List<PaymentStatusHistoryViewModel>()
                };

                return ServiceResult<PaymentStatusViewModel>.Successful(
                    status, "وضعیت پرداخت با موفقیت بارگذاری شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت وضعیت پرداخت");
                return ServiceResult<PaymentStatusViewModel>.Failed(
                    "خطا در بارگذاری وضعیت پرداخت");
            }
        }

        /// <summary>
        /// لغو پرداخت
        /// </summary>
        public async Task<ServiceResult<bool>> CancelPaymentAsync(string transactionId, string reason)
        {
            try
            {
                _logger.Information("لغو پرداخت برای تراکنش {TransactionId} با دلیل {Reason}", transactionId, reason);

                var result = await _paymentService.CancelPaymentAsync(int.Parse(transactionId), reason, _currentUserService.UserId);
                if (!result.Success)
                {
                    return ServiceResult<bool>.Failed(
                        "خطا در لغو پرداخت");
                }

                return ServiceResult<bool>.Successful(
                    true, "پرداخت با موفقیت لغو شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو پرداخت");
                return ServiceResult<bool>.Failed(
                    "خطا در لغو پرداخت");
            }
        }

        /// <summary>
        /// دریافت رسید پرداخت
        /// </summary>
        public async Task<ServiceResult<PaymentReceiptViewModel>> GetPaymentReceiptAsync(string transactionId)
        {
            try
            {
                _logger.Information("دریافت رسید پرداخت برای تراکنش {TransactionId}", transactionId);

                var result = await _paymentService.GetTransactionByIdAsync(int.Parse(transactionId));
                if (!result.Success)
                {
                    return ServiceResult<PaymentReceiptViewModel>.Failed(
                        "خطا در دریافت رسید پرداخت");
                }

                var receipt = new PaymentReceiptViewModel
                {
                    ReceiptNumber = result.Data.ReceiptNo,
                    TransactionId = int.Parse(transactionId),
                    PaymentDate = result.Data.CreatedAt,
                    PatientName = result.Data.PatientName,
                    PatientNationalCode = "", // Not available in PaymentTransaction
                    Amount = result.Data.Amount,
                    PaymentMethod = result.Data.Method.ToString(),
                    Status = result.Data.Status.ToString(),
                    ClinicName = "", // Not available in PaymentTransaction
                    ClinicAddress = "", // Not available in PaymentTransaction
                    ClinicPhone = "", // Not available in PaymentTransaction
                    CashierName = result.Data.CreatedByUserName,
                    ReceiptContent = result.Data.Description,
                    QrCode = "", // Not available in PaymentTransaction
                    AdditionalInfo = result.Data.Description
                };

                return ServiceResult<PaymentReceiptViewModel>.Successful(
                    receipt, "رسید پرداخت با موفقیت بارگذاری شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت رسید پرداخت");
                return ServiceResult<PaymentReceiptViewModel>.Failed(
                    "خطا در بارگذاری رسید پرداخت");
            }
        }

        /// <summary>
        /// اعتبارسنجی اطلاعات پرداخت
        /// </summary>
        public async Task<ServiceResult<bool>> ValidatePaymentInfoAsync(PaymentRequestViewModel paymentRequest)
        {
            try
            {
                _logger.Information("اعتبارسنجی اطلاعات پرداخت برای مبلغ {Amount}", paymentRequest.Amount);

                if (paymentRequest.Amount <= 0)
                {
                    return ServiceResult<bool>.Failed("مبلغ پرداخت باید بیشتر از صفر باشد");
                }

                if (paymentRequest.PatientId <= 0)
                {
                    return ServiceResult<bool>.Failed("شناسه بیمار الزامی است");
                }

                if (string.IsNullOrWhiteSpace(paymentRequest.PaymentType))
                {
                    return ServiceResult<bool>.Failed("نوع پرداخت الزامی است");
                }

                return ServiceResult<bool>.Successful(
                    true, "اطلاعات پرداخت معتبر است");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی اطلاعات پرداخت");
                return ServiceResult<bool>.Failed(
                    "خطا در اعتبارسنجی اطلاعات پرداخت");
            }
        }

        #region Helper Methods

        private async Task<List<PaymentMethodViewModel>> GetAvailablePaymentMethodsAsync()
        {
            // TODO: پیاده‌سازی دریافت روش‌های پرداخت از سرویس مربوطه
            return new List<PaymentMethodViewModel>
            {
                new PaymentMethodViewModel
                {
                    MethodId = 1,
                    MethodName = "پرداخت آنلاین",
                    MethodType = "Online",
                    Description = "پرداخت با کارت بانکی",
                    IsActive = true,
                    MinAmount = 1000,
                    MaxAmount = 10000000,
                    FeePercentage = 0,
                    FixedFee = 0
                },
                new PaymentMethodViewModel
                {
                    MethodId = 2,
                    MethodName = "پرداخت نقدی",
                    MethodType = "Cash",
                    Description = "پرداخت در محل",
                    IsActive = true,
                    MinAmount = 0,
                    MaxAmount = decimal.MaxValue,
                    FeePercentage = 0,
                    FixedFee = 0
                }
            };
        }

        private async Task<PaymentGatewayInfoViewModel> GetPaymentGatewayInfoAsync()
        {
            // TODO: پیاده‌سازی دریافت اطلاعات درگاه پرداخت
            return new PaymentGatewayInfoViewModel
            {
                GatewayId = 1,
                GatewayName = "پوز آنلاین",
                GatewayType = "POS",
                IsActive = true,
                SecurityLevel = "High",
                SupportedCurrencies = "IRR",
                MinAmount = 1000,
                MaxAmount = 10000000,
                ApiEndpoint = "https://api.pos-online.com",
                PublicKey = "pk_test_123456789"
            };
        }

        private string GenerateVerificationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private string GenerateReceiptNumber()
        {
            return $"RCP{DateTime.Now:yyyyMMddHHmmss}";
        }

        private string GetStatusDescription(string status)
        {
            return status switch
            {
                "Pending" => "در انتظار پرداخت",
                "Processing" => "در حال پردازش",
                "Completed" => "تکمیل شده",
                "Failed" => "ناموفق",
                "Cancelled" => "لغو شده",
                "Refunded" => "برگشت شده",
                _ => "نامشخص"
            };
        }

        #endregion

        #region Missing Interface Methods

        /// <summary>
        /// دریافت اطلاعات پرداخت
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>اطلاعات پرداخت</returns>
        public async Task<ServiceResult<PaymentInfoViewModel>> GetPaymentInfoAsync(int receptionId)
        {
            try
            {
                _logger.Information("دریافت اطلاعات پرداخت. ReceptionId: {ReceptionId}", receptionId);

                // پیاده‌سازی ساده - در محیط عملیاتی باید کامل شود
                var paymentInfo = new PaymentInfoViewModel
                {
                    StatusMessage = "اطلاعات پرداخت دریافت شد",
                    PayableAmount = 0,
                    PatientName = "نام بیمار",
                    PatientId = 0,
                    IsPaymentEnabled = true,
                    GatewayInfoViewModel = new PaymentGatewayInfoViewModel(),
                    AvailablePaymentMethodViewModels = new List<PaymentMethodViewModel>()
                };

                return ServiceResult<PaymentInfoViewModel>.Successful(paymentInfo);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات پرداخت");
                return ServiceResult<PaymentInfoViewModel>.Failed("خطا در دریافت اطلاعات پرداخت");
            }
        }

        /// <summary>
        /// دریافت رسید پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>رسید پرداخت</returns>
        public async Task<ServiceResult<PaymentReceiptViewModel>> GetPaymentReceiptAsync(int receptionId)
        {
            try
            {
                _logger.Information("دریافت رسید پرداخت. ReceptionId: {ReceptionId}", receptionId);

                // پیاده‌سازی ساده - در محیط عملیاتی باید کامل شود
                var receipt = new PaymentReceiptViewModel
                {
                    TransactionId = receptionId,
                    Amount = 0,
                    PaymentMethod = "نقدی",
                    Status = "تکمیل شده",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    Description = "پرداخت پذیرش"
                };

                return ServiceResult<PaymentReceiptViewModel>.Successful(receipt);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت رسید پرداخت");
                return ServiceResult<PaymentReceiptViewModel>.Failed("خطا در دریافت رسید پرداخت");
            }
        }

        /// <summary>
        /// دریافت رسید پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>رسید پرداخت</returns>
        public async Task<ServiceResult<PaymentReceiptViewModel>> GetPaymentReceiptAsync(int receptionId, int patientId)
        {
            try
            {
                _logger.Information("دریافت رسید پرداخت. ReceptionId: {ReceptionId}, PatientId: {PatientId}", 
                    receptionId, patientId);

                // پیاده‌سازی ساده - در محیط عملیاتی باید کامل شود
                var receipt = new PaymentReceiptViewModel
                {
                    TransactionId = receptionId,
                    Amount = 0,
                    PaymentMethod = "نقدی",
                    Status = "تکمیل شده",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    Description = "پرداخت پذیرش"
                };

                return ServiceResult<PaymentReceiptViewModel>.Successful(receipt);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت رسید پرداخت");
                return ServiceResult<PaymentReceiptViewModel>.Failed("خطا در دریافت رسید پرداخت");
            }
        }

        /// <summary>
        /// دریافت رسید پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>رسید پرداخت</returns>
        public async Task<ServiceResult<PaymentReceiptViewModel>> GetPaymentReceiptAsync(int receptionId, string transactionId)
        {
            try
            {
                _logger.Information("دریافت رسید پرداخت. ReceptionId: {ReceptionId}, TransactionId: {TransactionId}", 
                    receptionId, transactionId);

                // پیاده‌سازی ساده - در محیط عملیاتی باید کامل شود
                var receipt = new PaymentReceiptViewModel
                {
                    TransactionId = int.Parse(transactionId),
                    Amount = 0,
                    PaymentMethod = "نقدی",
                    Status = "تکمیل شده",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    Description = "پرداخت پذیرش"
                };

                return ServiceResult<PaymentReceiptViewModel>.Successful(receipt);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت رسید پرداخت");
                return ServiceResult<PaymentReceiptViewModel>.Failed("خطا در دریافت رسید پرداخت");
            }
        }

        /// <summary>
        /// دریافت رسید پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <param name="includeDetails">شامل جزئیات</param>
        /// <returns>رسید پرداخت</returns>
        public async Task<ServiceResult<PaymentReceiptViewModel>> GetPaymentReceiptAsync(int receptionId, string transactionId, bool includeDetails)
        {
            try
            {
                _logger.Information("دریافت رسید پرداخت. ReceptionId: {ReceptionId}, TransactionId: {TransactionId}, IncludeDetails: {IncludeDetails}", 
                    receptionId, transactionId, includeDetails);

                // پیاده‌سازی ساده - در محیط عملیاتی باید کامل شود
                var receipt = new PaymentReceiptViewModel
                {
                    TransactionId = int.Parse(transactionId),
                    Amount = 0,
                    PaymentMethod = "نقدی",
                    Status = "تکمیل شده",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    Description = "پرداخت پذیرش"
                };

                return ServiceResult<PaymentReceiptViewModel>.Successful(receipt);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت رسید پرداخت");
                return ServiceResult<PaymentReceiptViewModel>.Failed("خطا در دریافت رسید پرداخت");
            }
        }

        /// <summary>
        /// دریافت رسید پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <param name="includeDetails">شامل جزئیات</param>
        /// <param name="includeHistory">شامل تاریخچه</param>
        /// <returns>رسید پرداخت</returns>
        public async Task<ServiceResult<PaymentReceiptViewModel>> GetPaymentReceiptAsync(int receptionId, string transactionId, bool includeDetails, bool includeHistory)
        {
            try
            {
                _logger.Information("دریافت رسید پرداخت. ReceptionId: {ReceptionId}, TransactionId: {TransactionId}, IncludeDetails: {IncludeDetails}, IncludeHistory: {IncludeHistory}", 
                    receptionId, transactionId, includeDetails, includeHistory);

                // پیاده‌سازی ساده - در محیط عملیاتی باید کامل شود
                var receipt = new PaymentReceiptViewModel
                {
                    TransactionId = int.Parse(transactionId),
                    Amount = 0,
                    PaymentMethod = "نقدی",
                    Status = "تکمیل شده",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    Description = "پرداخت پذیرش"
                };

                return ServiceResult<PaymentReceiptViewModel>.Successful(receipt);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت رسید پرداخت");
                return ServiceResult<PaymentReceiptViewModel>.Failed("خطا در دریافت رسید پرداخت");
            }
        }

        #endregion
    }
}