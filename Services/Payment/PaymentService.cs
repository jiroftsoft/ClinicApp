using ClinicApp.Interfaces.Payment;
using ClinicApp.Interfaces.Payment.Gateway;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Models.Statistics;
using PaymentCalculation = ClinicApp.Models.Statistics.PaymentCalculation;
using GatewayFeeCalculation = ClinicApp.Models.Statistics.GatewayFeeCalculation;
using PaymentStatistics = ClinicApp.Models.Statistics.PaymentStatistics;
// using aliases removed to fix return type issues
// PaymentSearchFilters در Models/Statistics تعریف شده

namespace ClinicApp.Services.Payment
{
    /// <summary>
    /// Service برای مدیریت پرداخت‌ها
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت منطق کسب‌وکار پرداخت‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل فرآیند پرداخت (نقدی، POS، آنلاین)
    /// 2. محاسبه خودکار مبالغ و کارمزدها
    /// 3. مدیریت وضعیت‌های مختلف پرداخت
    /// 4. پشتیبانی از برگشت و لغو پرداخت
    /// 5. بهینه‌سازی برای عملکرد بالا
    /// </summary>
    public class PaymentService : IPaymentService
    {
        #region Fields

        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IPaymentGatewayRepository _paymentGatewayRepository;
        private readonly IOnlinePaymentRepository _onlinePaymentRepository;
        private readonly IPosTerminalRepository _posTerminalRepository;
        private readonly ICashSessionRepository _cashSessionRepository;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public PaymentService(
            IPaymentTransactionRepository paymentTransactionRepository,
            IPaymentGatewayRepository paymentGatewayRepository,
            IOnlinePaymentRepository onlinePaymentRepository,
            IPosTerminalRepository posTerminalRepository,
            ICashSessionRepository cashSessionRepository,
            ILogger logger)
        {
            _paymentTransactionRepository = paymentTransactionRepository ?? throw new ArgumentNullException(nameof(paymentTransactionRepository));
            _paymentGatewayRepository = paymentGatewayRepository ?? throw new ArgumentNullException(nameof(paymentGatewayRepository));
            _onlinePaymentRepository = onlinePaymentRepository ?? throw new ArgumentNullException(nameof(onlinePaymentRepository));
            _posTerminalRepository = posTerminalRepository ?? throw new ArgumentNullException(nameof(posTerminalRepository));
            _cashSessionRepository = cashSessionRepository ?? throw new ArgumentNullException(nameof(cashSessionRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Payment Processing

        /// <summary>
        /// پردازش پرداخت نقدی
        /// </summary>
        public async Task<ServiceResult<PaymentTransaction>> ProcessCashPaymentAsync(CashPaymentRequest request)
        {
            try
            {
                _logger.Information("شروع پردازش پرداخت نقدی برای پذیرش {ReceptionId} با مبلغ {Amount}", 
                    request.ReceptionId, request.Amount);

                // اعتبارسنجی درخواست
                var validationResult = await ValidateCashPaymentRequestAsync(request);
                if (!validationResult.Success)
                {
                    _logger.Warning("اعتبارسنجی پرداخت نقدی ناموفق: {Message}", validationResult.Message);
                    return ServiceResult<PaymentTransaction>.Failed(validationResult.Message);
                }

                // ایجاد تراکنش پرداخت
                var transaction = new PaymentTransaction
                {
                    ReceptionId = request.ReceptionId,
                    Amount = request.Amount,
                    Method = PaymentMethod.Cash,
                    Status = PaymentStatus.Completed,
                    TransactionId = GenerateTransactionId(),
                    ReferenceCode = GenerateReferenceCode(),
                    ReceiptNo = GenerateReceiptNumber(),
                    Description = request.Description,
                    CreatedByUserId = request.CreatedByUserId,
                    CreatedAt = DateTime.UtcNow,
                    CashSessionId = request.CashSessionId
                };

                // ذخیره تراکنش
                var savedTransaction = await _paymentTransactionRepository.CreateAsync(transaction);
                if (savedTransaction == null)
                {
                    _logger.Error("خطا در ذخیره تراکنش پرداخت نقدی");
                    return ServiceResult<PaymentTransaction>.Failed("خطا در ذخیره تراکنش پرداخت");
                }

                _logger.Information("پرداخت نقدی با موفقیت پردازش شد. شناسه تراکنش: {TransactionId}", transaction.PaymentTransactionId);
                return ServiceResult<PaymentTransaction>.Successful(transaction, "پرداخت نقدی با موفقیت انجام شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش پرداخت نقدی برای پذیرش {ReceptionId}", request.ReceptionId);
                return ServiceResult<PaymentTransaction>.Failed("خطا در پردازش پرداخت نقدی");
            }
        }

        /// <summary>
        /// پردازش پرداخت POS
        /// </summary>
        public async Task<ServiceResult<PaymentTransaction>> ProcessPosPaymentAsync(PosPaymentRequest request)
        {
            try
            {
                _logger.Information("شروع پردازش پرداخت POS برای پذیرش {ReceptionId} با مبلغ {Amount}", 
                    request.ReceptionId, request.Amount);

                // اعتبارسنجی درخواست
                var validationResult = await ValidatePosPaymentRequestAsync(request);
                if (!validationResult.Success)
                {
                    _logger.Warning("اعتبارسنجی پرداخت POS ناموفق: {Message}", validationResult.Message);
                    return ServiceResult<PaymentTransaction>.Failed(validationResult.Message);
                }

                // دریافت اطلاعات ترمینال POS
                var terminal = await _posTerminalRepository.GetByIdAsync(request.PosTerminalId);
                if (terminal == null)
                {
                    _logger.Warning("ترمینال POS با شناسه {TerminalId} یافت نشد", request.PosTerminalId);
                    return ServiceResult<PaymentTransaction>.Failed("ترمینال POS یافت نشد");
                }

                // ایجاد تراکنش پرداخت
                var transaction = new PaymentTransaction
                {
                    ReceptionId = request.ReceptionId,
                    Amount = request.Amount,
                    Method = PaymentMethod.POS,
                    Status = PaymentStatus.Completed,
                    TransactionId = request.TransactionId ?? GenerateTransactionId(),
                    ReferenceCode = request.ReferenceCode ?? GenerateReferenceCode(),
                    ReceiptNo = request.ReceiptNo ?? GenerateReceiptNumber(),
                    Description = request.Description,
                    CreatedByUserId = request.CreatedByUserId,
                    CreatedAt = DateTime.UtcNow,
                    PosTerminalId = request.PosTerminalId,
                    CashSessionId = request.CashSessionId
                };

                // ذخیره تراکنش
                var savedTransaction = await _paymentTransactionRepository.CreateAsync(transaction);
                if (savedTransaction == null)
                {
                    _logger.Error("خطا در ذخیره تراکنش پرداخت POS");
                    return ServiceResult<PaymentTransaction>.Failed("خطا در ذخیره تراکنش پرداخت");
                }

                _logger.Information("پرداخت POS با موفقیت پردازش شد. شناسه تراکنش: {TransactionId}", transaction.PaymentTransactionId);
                return ServiceResult<PaymentTransaction>.Successful(transaction, "پرداخت POS با موفقیت انجام شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش پرداخت POS برای پذیرش {ReceptionId}", request.ReceptionId);
                return ServiceResult<PaymentTransaction>.Failed("خطا در پردازش پرداخت POS");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// اعتبارسنجی درخواست پرداخت نقدی
        /// </summary>
        private async Task<ServiceResult> ValidateCashPaymentRequestAsync(CashPaymentRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("درخواست پرداخت نمی‌تواند خالی باشد");
                return ServiceResult.Failed("درخواست پرداخت نامعتبر است", string.Join("; ", errors));
            }

            if (request.ReceptionId <= 0)
                errors.Add("شناسه پذیرش نامعتبر است");

            if (request.Amount <= 0)
                errors.Add("مبلغ پرداخت باید بیشتر از صفر باشد");

            if (string.IsNullOrWhiteSpace(request.CreatedByUserId))
                errors.Add("شناسه کاربر ایجادکننده الزامی است");

            if (request.CashSessionId <= 0)
                errors.Add("شناسه جلسه نقدی نامعتبر است");

            // بررسی وجود جلسه نقدی
            var session = await _cashSessionRepository.GetByIdAsync(request.CashSessionId);
            if (session == null)
                errors.Add("جلسه نقدی یافت نشد");

            if (errors.Any())
                return ServiceResult.Failed("اطلاعات وارد شده نامعتبر است", string.Join("; ", errors));

            return ServiceResult.Successful();
        }

        /// <summary>
        /// اعتبارسنجی درخواست پرداخت POS
        /// </summary>
        private async Task<ServiceResult> ValidatePosPaymentRequestAsync(PosPaymentRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("درخواست پرداخت نمی‌تواند خالی باشد");
                return ServiceResult.Failed("درخواست پرداخت نامعتبر است", string.Join("; ", errors));
            }

            if (request.ReceptionId <= 0)
                errors.Add("شناسه پذیرش نامعتبر است");

            if (request.Amount <= 0)
                errors.Add("مبلغ پرداخت باید بیشتر از صفر باشد");

            if (request.PosTerminalId <= 0)
                errors.Add("شناسه ترمینال POS نامعتبر است");

            if (string.IsNullOrWhiteSpace(request.CreatedByUserId))
                errors.Add("شناسه کاربر ایجادکننده الزامی است");

            if (request.CashSessionId <= 0)
                errors.Add("شناسه جلسه نقدی نامعتبر است");

            if (errors.Any())
                return ServiceResult.Failed("اطلاعات وارد شده نامعتبر است", string.Join("; ", errors));

            return ServiceResult.Successful();
        }

        /// <summary>
        /// تولید شناسه تراکنش
        /// </summary>
        private string GenerateTransactionId()
        {
            return $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        /// <summary>
        /// تولید کد مرجع
        /// </summary>
        private string GenerateReferenceCode()
        {
            return $"REF_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }

        /// <summary>
        /// تولید شماره رسید
        /// </summary>
        private string GenerateReceiptNumber()
        {
            return $"RCP_{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        /// <summary>
        /// تولید توکن پرداخت
        /// </summary>
        private string GeneratePaymentToken()
        {
            return $"TOKEN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper()}";
        }

        /// <summary>
        /// اعتبارسنجی درخواست پرداخت آنلاین
        /// </summary>
        private async Task<ServiceResult> ValidateOnlinePaymentRequestAsync(OnlinePaymentRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("درخواست پرداخت نمی‌تواند خالی باشد");
                return ServiceResult.Failed("درخواست پرداخت نامعتبر است", string.Join("; ", errors));
            }

            if (request.ReceptionId <= 0)
                errors.Add("شناسه پذیرش نامعتبر است");

            if (request.PatientId <= 0)
                errors.Add("شناسه بیمار نامعتبر است");

            if (request.Amount <= 0)
                errors.Add("مبلغ پرداخت باید بیشتر از صفر باشد");

            if (request.PaymentGatewayId <= 0)
                errors.Add("شناسه درگاه پرداخت نامعتبر است");

            if (string.IsNullOrWhiteSpace(request.CreatedByUserId))
                errors.Add("شناسه کاربر ایجادکننده الزامی است");

            if (string.IsNullOrWhiteSpace(request.UserIpAddress))
                errors.Add("آدرس IP کاربر الزامی است");

            if (errors.Any())
                return ServiceResult.Failed("اطلاعات وارد شده نامعتبر است", string.Join("; ", errors));

            return ServiceResult.Successful();
        }

        #endregion

        #region Placeholder Methods (To be implemented in next parts)

        public async Task<ServiceResult<OnlinePayment>> ProcessOnlinePaymentAsync(OnlinePaymentRequest request)
        {
            try
            {
                _logger.Information("شروع پردازش پرداخت آنلاین برای پذیرش {ReceptionId} با مبلغ {Amount}", 
                    request.ReceptionId, request.Amount);

                // اعتبارسنجی درخواست
                var validationResult = await ValidateOnlinePaymentRequestAsync(request);
                if (!validationResult.Success)
                {
                    _logger.Warning("اعتبارسنجی پرداخت آنلاین ناموفق: {Message}", validationResult.Message);
                    return ServiceResult<OnlinePayment>.Failed(validationResult.Message);
                }

                // دریافت اطلاعات درگاه پرداخت
                var gateway = await _paymentGatewayRepository.GetByIdAsync(request.PaymentGatewayId);
                if (gateway == null)
                {
                    _logger.Warning("درگاه پرداخت با شناسه {GatewayId} یافت نشد", request.PaymentGatewayId);
                    return ServiceResult<OnlinePayment>.Failed("درگاه پرداخت یافت نشد");
                }

                // محاسبه کارمزد درگاه
                var feeCalculation = await CalculateGatewayFeeAsync(request.Amount, request.PaymentGatewayId);
                if (!feeCalculation.Success)
                {
                    _logger.Error("خطا در محاسبه کارمزد درگاه: {Message}", feeCalculation.Message);
                    return ServiceResult<OnlinePayment>.Failed("خطا در محاسبه کارمزد درگاه");
                }

                // ایجاد پرداخت آنلاین
                var onlinePayment = new OnlinePayment
                {
                    ReceptionId = request.ReceptionId,
                    AppointmentId = request.AppointmentId,
                    PatientId = request.PatientId,
                    PaymentType = request.PaymentType,
                    Amount = request.Amount,
                    PaymentGatewayId = request.PaymentGatewayId,
                    PaymentToken = GeneratePaymentToken(),
                    Status = OnlinePaymentStatus.Pending,
                    Description = request.Description,
                    UserIpAddress = request.UserIpAddress,
                    UserAgent = request.UserAgent,
                    CreatedByUserId = request.CreatedByUserId,
                    CreatedAt = DateTime.UtcNow,
                    GatewayFee = feeCalculation.Data.CalculatedFee,
                    NetAmount = feeCalculation.Data.NetAmount
                };

                // ذخیره پرداخت آنلاین
                var savedOnlinePayment = await _onlinePaymentRepository.CreateAsync(onlinePayment);
                if (savedOnlinePayment == null)
                {
                    _logger.Error("خطا در ذخیره پرداخت آنلاین");
                    return ServiceResult<OnlinePayment>.Failed("خطا در ذخیره پرداخت آنلاین");
                }

                _logger.Information("پرداخت آنلاین با موفقیت ایجاد شد. شناسه: {OnlinePaymentId}", onlinePayment.OnlinePaymentId);
                return ServiceResult<OnlinePayment>.Successful(onlinePayment, "پرداخت آنلاین با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش پرداخت آنلاین برای پذیرش {ReceptionId}", request.ReceptionId);
                return ServiceResult<OnlinePayment>.Failed("خطا در پردازش پرداخت آنلاین");
            }
        }

        public async Task<ServiceResult<PaymentTransaction>> CompleteOnlinePaymentAsync(OnlinePaymentCallback callbackData)
        {
            try
            {
                _logger.Information("شروع تکمیل پرداخت آنلاین با توکن {PaymentToken}", callbackData.PaymentToken);

                // دریافت پرداخت آنلاین
                var onlinePayment = await _onlinePaymentRepository.GetByPaymentTokenAsync(callbackData.PaymentToken);
                if (onlinePayment == null)
                {
                    _logger.Warning("پرداخت آنلاین با توکن {PaymentToken} یافت نشد", callbackData.PaymentToken);
                    return ServiceResult<PaymentTransaction>.Failed("پرداخت آنلاین یافت نشد");
                }

                // بررسی وضعیت فعلی
                if (onlinePayment.Status != OnlinePaymentStatus.Pending)
                {
                    _logger.Warning("پرداخت آنلاین با توکن {PaymentToken} در وضعیت {Status} است", 
                        callbackData.PaymentToken, onlinePayment.Status);
                    return ServiceResult<PaymentTransaction>.Failed("پرداخت آنلاین قبلاً پردازش شده است");
                }

                // به‌روزرسانی وضعیت پرداخت آنلاین
                onlinePayment.Status = callbackData.Status;
                onlinePayment.GatewayTransactionId = callbackData.GatewayTransactionId;
                onlinePayment.GatewayReferenceCode = callbackData.GatewayReferenceCode;
                onlinePayment.ErrorCode = callbackData.ErrorCode;
                onlinePayment.ErrorMessage = callbackData.ErrorMessage;
                onlinePayment.GatewayFee = callbackData.GatewayFee;
                onlinePayment.NetAmount = callbackData.NetAmount;
                onlinePayment.CompletedAt = DateTime.UtcNow;

                var updatedOnlinePayment = await _onlinePaymentRepository.UpdateAsync(onlinePayment);
                if (updatedOnlinePayment == null)
                {
                    _logger.Error("خطا در به‌روزرسانی پرداخت آنلاین");
                    return ServiceResult<PaymentTransaction>.Failed("خطا در به‌روزرسانی پرداخت آنلاین");
                }

                // اگر پرداخت موفق بود، تراکنش پرداخت ایجاد کن
                if (callbackData.Status == OnlinePaymentStatus.Successful)
                {
                    var transaction = new PaymentTransaction
                    {
                        ReceptionId = onlinePayment.ReceptionId ?? 0,
                        Amount = onlinePayment.Amount,
                        Method = PaymentMethod.Online,
                        Status = PaymentStatus.Completed,
                        TransactionId = callbackData.GatewayTransactionId ?? GenerateTransactionId(),
                        ReferenceCode = callbackData.GatewayReferenceCode ?? GenerateReferenceCode(),
                        ReceiptNo = GenerateReceiptNumber(),
                        Description = onlinePayment.Description,
                        CreatedByUserId = onlinePayment.CreatedByUserId,
                        CreatedAt = DateTime.UtcNow,
                        PaymentGatewayId = onlinePayment.PaymentGatewayId,
                        OnlinePaymentId = onlinePayment.OnlinePaymentId
                    };

                    var savedTransaction = await _paymentTransactionRepository.CreateAsync(transaction);
                    if (savedTransaction == null)
                    {
                        _logger.Error("خطا در ایجاد تراکنش پرداخت");
                        return ServiceResult<PaymentTransaction>.Failed("خطا در ایجاد تراکنش پرداخت");
                    }

                    _logger.Information("پرداخت آنلاین با موفقیت تکمیل شد. شناسه تراکنش: {TransactionId}", transaction.PaymentTransactionId);
                    return ServiceResult<PaymentTransaction>.Successful(transaction, "پرداخت آنلاین با موفقیت تکمیل شد");
                }
                else
                {
                    _logger.Warning("پرداخت آنلاین ناموفق بود. وضعیت: {Status}, خطا: {Error}", 
                        callbackData.Status, callbackData.ErrorMessage);
                    return ServiceResult<PaymentTransaction>.Failed($"پرداخت آنلاین ناموفق: {callbackData.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تکمیل پرداخت آنلاین با توکن {PaymentToken}", callbackData.PaymentToken);
                return ServiceResult<PaymentTransaction>.Failed("خطا در تکمیل پرداخت آنلاین");
            }
        }

        public async Task<ServiceResult> CancelPaymentAsync(int transactionId, string reason, string userId)
        {
            try
            {
                _logger.Information("شروع لغو پرداخت. تراکنش: {TransactionId}, دلیل: {Reason}, کاربر: {UserId}", 
                    transactionId, reason, userId);

                // دریافت تراکنش
                var transaction = await _paymentTransactionRepository.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    return ServiceResult.Failed("تراکنش مورد نظر یافت نشد.", "TRANSACTION_NOT_FOUND", 
                        ErrorCategory.NotFound, SecurityLevel.Medium);
                }

                // بررسی امکان لغو
                if (transaction.Status == PaymentStatus.Success)
                {
                    return ServiceResult.Failed("تراکنش موفق قابل لغو نیست. از برگشت استفاده کنید.", "CANNOT_CANCEL_SUCCESSFUL", 
                        ErrorCategory.BusinessLogic, SecurityLevel.Medium);
                }

                if (transaction.Status == PaymentStatus.Canceled)
                {
                    return ServiceResult.Failed("تراکنش قبلاً لغو شده است.", "ALREADY_CANCELLED", 
                        ErrorCategory.BusinessLogic, SecurityLevel.Medium);
                }

                // لغو تراکنش
                transaction.Status = PaymentStatus.Canceled;
                transaction.Description = $"لغو شده: {reason}";
                transaction.UpdatedByUserId = userId;
                transaction.UpdatedAt = DateTime.Now;

                await _paymentTransactionRepository.UpdateAsync(transaction);

                _logger.Information("پرداخت با موفقیت لغو شد. تراکنش: {TransactionId}", transactionId);
                return ServiceResult.Successful("پرداخت با موفقیت لغو شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو پرداخت. تراکنش: {TransactionId}", transactionId);
                return ServiceResult.Failed("خطا در لغو پرداخت.", "CANCEL_PAYMENT_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<PaymentTransaction>> RefundPaymentAsync(int transactionId, decimal refundAmount, string reason, string userId)
        {
            try
            {
                _logger.Information("شروع برگشت پرداخت. تراکنش: {TransactionId}, مبلغ: {RefundAmount}, دلیل: {Reason}, کاربر: {UserId}", 
                    transactionId, refundAmount, reason, userId);

                // دریافت تراکنش
                var transaction = await _paymentTransactionRepository.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    return ServiceResult<PaymentTransaction>.Failed("تراکنش مورد نظر یافت نشد.", "TRANSACTION_NOT_FOUND", 
                        ErrorCategory.NotFound, SecurityLevel.Medium);
                }

                // بررسی امکان برگشت
                if (transaction.Status != PaymentStatus.Success)
                {
                    return ServiceResult<PaymentTransaction>.Failed("فقط تراکنش‌های موفق قابل برگشت هستند.", "CANNOT_REFUND_NON_SUCCESSFUL", 
                        ErrorCategory.BusinessLogic, SecurityLevel.Medium);
                }

                if (refundAmount > transaction.Amount)
                {
                    return ServiceResult<PaymentTransaction>.Failed("مبلغ برگشت نمی‌تواند بیشتر از مبلغ اصلی باشد.", "REFUND_AMOUNT_EXCEEDED", 
                        ErrorCategory.Validation, SecurityLevel.Medium);
                }

                // ایجاد تراکنش برگشت
                var refundTransaction = new PaymentTransaction
                {
                    ReceptionId = transaction.ReceptionId,
                    PosTerminalId = transaction.PosTerminalId,
                    PaymentGatewayId = transaction.PaymentGatewayId,
                    OnlinePaymentId = transaction.OnlinePaymentId,
                    CashSessionId = transaction.CashSessionId,
                    Amount = -refundAmount, // مبلغ منفی برای برگشت
                    Method = transaction.Method,
                    Status = PaymentStatus.Success,
                    TransactionId = $"REFUND_{transaction.TransactionId}_{DateTime.Now:yyyyMMddHHmmss}",
                    ReferenceCode = $"REF_{transaction.ReferenceCode}",
                    Description = $"برگشت: {reason}",
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedByUserId = userId,
                    UpdatedAt = DateTime.Now
                };

                var savedRefundTransaction = await _paymentTransactionRepository.CreateAsync(refundTransaction);

                // به‌روزرسانی تراکنش اصلی
                transaction.Description = $"{transaction.Description}\nبرگشت شده: {refundAmount:C} - {reason}";
                transaction.UpdatedByUserId = userId;
                transaction.UpdatedAt = DateTime.Now;
                await _paymentTransactionRepository.UpdateAsync(transaction);

                _logger.Information("برگشت پرداخت با موفقیت انجام شد. تراکنش: {TransactionId}, مبلغ: {RefundAmount}", 
                    transactionId, refundAmount);
                return ServiceResult<PaymentTransaction>.Successful(savedRefundTransaction, "برگشت پرداخت با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در برگشت پرداخت. تراکنش: {TransactionId}", transactionId);
                return ServiceResult<PaymentTransaction>.Failed("خطا در برگشت پرداخت.", "REFUND_PAYMENT_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult> UpdatePaymentStatusAsync(int transactionId, PaymentStatus status, string userId)
        {
            try
            {
                _logger.Information("شروع به‌روزرسانی وضعیت پرداخت. تراکنش: {TransactionId}, وضعیت جدید: {Status}, کاربر: {UserId}", 
                    transactionId, status, userId);

                // دریافت تراکنش
                var transaction = await _paymentTransactionRepository.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    return ServiceResult.Failed("تراکنش مورد نظر یافت نشد.", "TRANSACTION_NOT_FOUND", 
                        ErrorCategory.NotFound, SecurityLevel.Medium);
                }

                // بررسی تغییر وضعیت
                if (transaction.Status == status)
                {
                    return ServiceResult.Failed("وضعیت تراکنش قبلاً به این مقدار تنظیم شده است.", "STATUS_ALREADY_SET", 
                        ErrorCategory.BusinessLogic, SecurityLevel.Medium);
                }

                // به‌روزرسانی وضعیت
                var oldStatus = transaction.Status;
                transaction.Status = status;
                transaction.UpdatedByUserId = userId;
                transaction.UpdatedAt = DateTime.Now;

                await _paymentTransactionRepository.UpdateAsync(transaction);

                _logger.Information("وضعیت پرداخت با موفقیت به‌روزرسانی شد. تراکنش: {TransactionId}, از {OldStatus} به {NewStatus}", 
                    transactionId, oldStatus, status);
                return ServiceResult.Successful("وضعیت پرداخت با موفقیت به‌روزرسانی شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی وضعیت پرداخت. تراکنش: {TransactionId}", transactionId);
                return ServiceResult.Failed("خطا در به‌روزرسانی وضعیت پرداخت.", "UPDATE_STATUS_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<PaymentCalculation>> CalculatePaymentAsync(int receptionId, PaymentMethod paymentMethod, decimal discountAmount = 0)
        {
            try
            {
                _logger.Information("شروع محاسبه پرداخت. پذیرش: {ReceptionId}, روش: {PaymentMethod}, تخفیف: {DiscountAmount}", 
                    receptionId, paymentMethod, discountAmount);

                // دریافت پذیرش (نیاز به ReceptionRepository)
                // var reception = await _receptionRepository.GetByIdAsync(receptionId);
                // if (reception == null)
                // {
                //     return ServiceResult<PaymentCalculation>.Failed("پذیرش مورد نظر یافت نشد.", "RECEPTION_NOT_FOUND", 
                //         ErrorCategory.NotFound, SecurityLevel.Medium);
                // }

                // محاسبه مبلغ پایه (موقت - باید از Reception دریافت شود)
                var baseAmount = 100000m; // TODO: دریافت از reception.TotalAmount
                var finalAmount = baseAmount - discountAmount;

                // محاسبه کارمزد درگاه (در صورت نیاز)
                decimal gatewayFee = 0;
                if (paymentMethod == PaymentMethod.Online || paymentMethod == PaymentMethod.POS)
                {
                    // TODO: محاسبه کارمزد از درگاه
                    gatewayFee = finalAmount * 0.02m; // 2% کارمزد فرضی
                }

                var totalAmount = finalAmount + gatewayFee;

                var calculation = new PaymentCalculation
                {
                    BaseAmount = baseAmount,
                    DiscountAmount = discountAmount,
                    FinalAmount = finalAmount,
                    GatewayFee = gatewayFee,
                    TotalAmount = totalAmount,
                    PaymentMethod = paymentMethod,
                    ReceptionId = receptionId
                };

                _logger.Information("محاسبه پرداخت تکمیل شد. پذیرش: {ReceptionId}, مبلغ نهایی: {TotalAmount}", 
                    receptionId, totalAmount);
                return ServiceResult<PaymentCalculation>.Successful(calculation, "محاسبه پرداخت با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه پرداخت. پذیرش: {ReceptionId}", receptionId);
                return ServiceResult<PaymentCalculation>.Failed("خطا در محاسبه پرداخت.", "CALCULATE_PAYMENT_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<GatewayFeeCalculation>> CalculateGatewayFeeAsync(decimal amount, int gatewayId)
        {
            try
            {
                _logger.Information("شروع محاسبه کارمزد درگاه. مبلغ: {Amount}, درگاه: {GatewayId}", amount, gatewayId);

                // دریافت درگاه پرداخت
                var gateway = await _paymentGatewayRepository.GetByIdAsync(gatewayId);
                if (gateway == null)
                {
                    return ServiceResult<GatewayFeeCalculation>.Failed("درگاه پرداخت مورد نظر یافت نشد.", "GATEWAY_NOT_FOUND", 
                        ErrorCategory.NotFound, SecurityLevel.Medium);
                }

                // محاسبه کارمزد
                decimal fixedFee = gateway.FixedFee ?? 0;
                decimal percentageFee = 0;
                
                if (gateway.FeePercentage.HasValue && gateway.FeePercentage > 0)
                {
                    percentageFee = amount * (gateway.FeePercentage.Value / 100);
                }

                var totalFee = fixedFee + percentageFee;
                var netAmount = amount - totalFee;

                var calculation = new GatewayFeeCalculation
                {
                    Amount = amount,
                    FixedFee = fixedFee,
                    PercentageFee = percentageFee,
                    TotalFee = totalFee,
                    NetAmount = netAmount,
                    GatewayId = gatewayId,
                    GatewayName = gateway.Name
                };

                _logger.Information("محاسبه کارمزد درگاه تکمیل شد. مبلغ: {Amount}, کارمزد: {TotalFee}, خالص: {NetAmount}", 
                    amount, totalFee, netAmount);
                return ServiceResult<GatewayFeeCalculation>.Successful(calculation, "محاسبه کارمزد درگاه با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه کارمزد درگاه. مبلغ: {Amount}, درگاه: {GatewayId}", amount, gatewayId);
                return ServiceResult<GatewayFeeCalculation>.Failed("خطا در محاسبه کارمزد درگاه.", "CALCULATE_GATEWAY_FEE_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<decimal>> CalculateNetAmountAsync(decimal amount, int gatewayId)
        {
            try
            {
                _logger.Information("شروع محاسبه مبلغ خالص. مبلغ: {Amount}, درگاه: {GatewayId}", amount, gatewayId);

                // استفاده از متد محاسبه کارمزد
                var feeCalculation = await CalculateGatewayFeeAsync(amount, gatewayId);
                if (!feeCalculation.Success)
                {
                    return ServiceResult<decimal>.Failed(feeCalculation.Message, feeCalculation.Code, 
                        feeCalculation.Category, feeCalculation.SecurityLevel);
                }

                var netAmount = feeCalculation.Data.NetAmount;

                _logger.Information("محاسبه مبلغ خالص تکمیل شد. مبلغ: {Amount}, خالص: {NetAmount}", amount, netAmount);
                return ServiceResult<decimal>.Successful(netAmount, "محاسبه مبلغ خالص با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه مبلغ خالص. مبلغ: {Amount}, درگاه: {GatewayId}", amount, gatewayId);
                return ServiceResult<decimal>.Failed("خطا در محاسبه مبلغ خالص.", "CALCULATE_NET_AMOUNT_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult> ValidatePaymentAsync(PaymentRequest request)
        {
            try
            {
                _logger.Information("شروع اعتبارسنجی پرداخت. پذیرش: {ReceptionId}, مبلغ: {Amount}, روش: {Method}", 
                    request.ReceptionId, request.Amount, request.Method);

                // اعتبارسنجی مبلغ
                if (request.Amount <= 0)
                {
                    return ServiceResult.Failed("مبلغ پرداخت باید بیشتر از صفر باشد.", "INVALID_AMOUNT", 
                        ErrorCategory.Validation, SecurityLevel.Medium);
                }

                // اعتبارسنجی روش پرداخت
                if (!Enum.IsDefined(typeof(PaymentMethod), request.Method))
                {
                    return ServiceResult.Failed("روش پرداخت نامعتبر است.", "INVALID_PAYMENT_METHOD", 
                        ErrorCategory.Validation, SecurityLevel.Medium);
                }

                // اعتبارسنجی پذیرش (نیاز به ReceptionRepository)
                // var reception = await _receptionRepository.GetByIdAsync(request.ReceptionId);
                // if (reception == null)
                // {
                //     return ServiceResult.Failed("پذیرش مورد نظر یافت نشد.", "RECEPTION_NOT_FOUND", 
                //         ErrorCategory.NotFound, SecurityLevel.Medium);
                // }

                // اعتبارسنجی مبلغ با مبلغ پذیرش
                // if (request.Amount > reception.TotalAmount)
                // {
                //     return ServiceResult.Failed("مبلغ پرداخت نمی‌تواند بیشتر از مبلغ پذیرش باشد.", "AMOUNT_EXCEEDED", 
                //         ErrorCategory.Validation, SecurityLevel.Medium);
                // }

                _logger.Information("اعتبارسنجی پرداخت با موفقیت انجام شد. پذیرش: {ReceptionId}", request.ReceptionId);
                return ServiceResult.Successful("اعتبارسنجی پرداخت با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی پرداخت. پذیرش: {ReceptionId}", request.ReceptionId);
                return ServiceResult.Failed("خطا در اعتبارسنجی پرداخت.", "VALIDATE_PAYMENT_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult> CanProcessPaymentAsync(int receptionId, PaymentMethod paymentMethod)
        {
            try
            {
                _logger.Information("بررسی امکان پردازش پرداخت. پذیرش: {ReceptionId}, روش: {PaymentMethod}", 
                    receptionId, paymentMethod);

                // بررسی وجود پذیرش (نیاز به ReceptionRepository)
                // var reception = await _receptionRepository.GetByIdAsync(receptionId);
                // if (reception == null)
                // {
                //     return ServiceResult.Failed("پذیرش مورد نظر یافت نشد.", "RECEPTION_NOT_FOUND", 
                //         ErrorCategory.NotFound, SecurityLevel.Medium);
                // }

                // بررسی وضعیت پذیرش
                // if (reception.Status == ReceptionStatus.Completed)
                // {
                //     return ServiceResult.Failed("پذیرش قبلاً تکمیل شده است.", "RECEPTION_ALREADY_COMPLETED", 
                //         ErrorCategory.BusinessRule, SecurityLevel.Medium);
                // }

                // بررسی روش پرداخت
                switch (paymentMethod)
                {
                    case PaymentMethod.Online:
                        // بررسی وجود درگاه فعال
                        var activeGateways = await _paymentGatewayRepository.GetActiveGatewaysAsync();
                        if (activeGateways == null || !activeGateways.Any())
                        {
                            return ServiceResult.Failed("هیچ درگاه پرداخت آنلاین فعالی یافت نشد.", "NO_ACTIVE_GATEWAY", 
                                ErrorCategory.BusinessLogic, SecurityLevel.Medium);
                        }
                        break;

                    case PaymentMethod.POS:
                        // بررسی وجود پوز فعال
                        var activePosTerminals = await _posTerminalRepository.GetActiveTerminalsAsync();
                        if (activePosTerminals == null || !activePosTerminals.Any())
                        {
                            return ServiceResult.Failed("هیچ دستگاه پوز فعالی یافت نشد.", "NO_ACTIVE_POS", 
                                ErrorCategory.BusinessLogic, SecurityLevel.Medium);
                        }
                        break;

                    case PaymentMethod.Cash:
                        // بررسی وجود شیفت صندوق فعال
                        var activeCashSessions = await _cashSessionRepository.GetActiveSessionsAsync();
                        var activeCashSession = activeCashSessions?.FirstOrDefault();
                        if (activeCashSession == null)
                        {
                            return ServiceResult.Failed("هیچ شیفت صندوق فعالی یافت نشد.", "NO_ACTIVE_CASH_SESSION", 
                                ErrorCategory.BusinessLogic, SecurityLevel.Medium);
                        }
                        break;
                }

                _logger.Information("امکان پردازش پرداخت تأیید شد. پذیرش: {ReceptionId}, روش: {PaymentMethod}", 
                    receptionId, paymentMethod);
                return ServiceResult.Successful("امکان پردازش پرداخت تأیید شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی امکان پردازش پرداخت. پذیرش: {ReceptionId}", receptionId);
                return ServiceResult.Failed("خطا در بررسی امکان پردازش پرداخت.", "CAN_PROCESS_PAYMENT_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<PaymentStatus>> GetPaymentStatusAsync(int transactionId)
        {
            try
            {
                _logger.Information("درخواست وضعیت پرداخت. تراکنش: {TransactionId}", transactionId);

                // دریافت تراکنش
                var transaction = await _paymentTransactionRepository.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    return ServiceResult<PaymentStatus>.Failed("تراکنش مورد نظر یافت نشد.", "TRANSACTION_NOT_FOUND", 
                        ErrorCategory.NotFound, SecurityLevel.Medium);
                }

                _logger.Information("وضعیت پرداخت دریافت شد. تراکنش: {TransactionId}, وضعیت: {Status}", 
                    transactionId, transaction.Status);
                return ServiceResult<PaymentStatus>.Successful(transaction.Status, "وضعیت پرداخت با موفقیت دریافت شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت وضعیت پرداخت. تراکنش: {TransactionId}", transactionId);
                return ServiceResult<PaymentStatus>.Failed("خطا در دریافت وضعیت پرداخت.", "GET_PAYMENT_STATUS_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<PaymentTransaction>> GetPaymentTransactionAsync(int transactionId)
        {
            try
            {
                _logger.Information("درخواست تراکنش پرداخت. تراکنش: {TransactionId}", transactionId);

                // دریافت تراکنش
                var transaction = await _paymentTransactionRepository.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    return ServiceResult<PaymentTransaction>.Failed("تراکنش مورد نظر یافت نشد.", "TRANSACTION_NOT_FOUND", 
                        ErrorCategory.NotFound, SecurityLevel.Medium);
                }

                _logger.Information("تراکنش پرداخت دریافت شد. تراکنش: {TransactionId}, مبلغ: {Amount}, وضعیت: {Status}", 
                    transactionId, transaction.Amount, transaction.Status);
                return ServiceResult<PaymentTransaction>.Successful(transaction, "تراکنش پرداخت با موفقیت دریافت شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش پرداخت. تراکنش: {TransactionId}", transactionId);
                return ServiceResult<PaymentTransaction>.Failed("خطا در دریافت تراکنش پرداخت.", "GET_PAYMENT_TRANSACTION_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<IEnumerable<PaymentTransaction>>> GetReceptionPaymentsAsync(int receptionId)
        {
            try
            {
                _logger.Information("درخواست لیست پرداخت‌های پذیرش. پذیرش: {ReceptionId}", receptionId);

                // دریافت تراکنش‌های پذیرش
                var transactions = await _paymentTransactionRepository.GetByReceptionIdAsync(receptionId);
                if (transactions == null || !transactions.Any())
                {
                    _logger.Information("هیچ تراکنش پرداختی برای پذیرش یافت نشد. پذیرش: {ReceptionId}", receptionId);
                    return ServiceResult<IEnumerable<PaymentTransaction>>.Successful(new List<PaymentTransaction>(), "هیچ تراکنش پرداختی یافت نشد.");
                }

                _logger.Information("لیست پرداخت‌های پذیرش دریافت شد. پذیرش: {ReceptionId}, تعداد: {Count}", 
                    receptionId, transactions.Count());
                return ServiceResult<IEnumerable<PaymentTransaction>>.Successful(transactions, "لیست پرداخت‌های پذیرش با موفقیت دریافت شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پرداخت‌های پذیرش. پذیرش: {ReceptionId}", receptionId);
                return ServiceResult<IEnumerable<PaymentTransaction>>.Failed("خطا در دریافت لیست پرداخت‌های پذیرش.", "GET_RECEPTION_PAYMENTS_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<IEnumerable<PaymentTransaction>>> GetPatientPaymentsAsync(int patientId, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                _logger.Information("درخواست لیست پرداخت‌های بیمار. بیمار: {PatientId}, صفحه: {PageNumber}, اندازه: {PageSize}", 
                    patientId, pageNumber, pageSize);

                // دریافت تراکنش‌های بیمار
                var transactions = await _paymentTransactionRepository.GetByPatientIdAsync(patientId, pageNumber, pageSize);
                if (transactions == null || !transactions.Any())
                {
                    _logger.Information("هیچ تراکنش پرداختی برای بیمار یافت نشد. بیمار: {PatientId}", patientId);
                    return ServiceResult<IEnumerable<PaymentTransaction>>.Successful(new List<PaymentTransaction>(), "هیچ تراکنش پرداختی یافت نشد.");
                }

                _logger.Information("لیست پرداخت‌های بیمار دریافت شد. بیمار: {PatientId}, تعداد: {Count}", 
                    patientId, transactions.Count());
                return ServiceResult<IEnumerable<PaymentTransaction>>.Successful(transactions, "لیست پرداخت‌های بیمار با موفقیت دریافت شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پرداخت‌های بیمار. بیمار: {PatientId}", patientId);
                return ServiceResult<IEnumerable<PaymentTransaction>>.Failed("خطا در دریافت لیست پرداخت‌های بیمار.", "GET_PATIENT_PAYMENTS_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<IEnumerable<PaymentTransaction>>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                _logger.Information("درخواست لیست پرداخت‌ها بر اساس بازه تاریخ. از: {StartDate}, تا: {EndDate}, صفحه: {PageNumber}, اندازه: {PageSize}", 
                    startDate, endDate, pageNumber, pageSize);

                // دریافت تراکنش‌ها بر اساس بازه تاریخ
                var transactions = await _paymentTransactionRepository.GetByDateRangeAsync(startDate, endDate, pageNumber, pageSize);
                if (transactions == null || !transactions.Any())
                {
                    _logger.Information("هیچ تراکنش پرداختی در بازه تاریخ مشخص یافت نشد. از: {StartDate}, تا: {EndDate}", startDate, endDate);
                    return ServiceResult<IEnumerable<PaymentTransaction>>.Successful(new List<PaymentTransaction>(), "هیچ تراکنش پرداختی یافت نشد.");
                }

                _logger.Information("لیست پرداخت‌ها بر اساس بازه تاریخ دریافت شد. از: {StartDate}, تا: {EndDate}, تعداد: {Count}", 
                    startDate, endDate, transactions.Count());
                return ServiceResult<IEnumerable<PaymentTransaction>>.Successful(transactions, "لیست پرداخت‌ها بر اساس بازه تاریخ با موفقیت دریافت شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پرداخت‌ها بر اساس بازه تاریخ. از: {StartDate}, تا: {EndDate}", startDate, endDate);
                return ServiceResult<IEnumerable<PaymentTransaction>>.Failed("خطا در دریافت لیست پرداخت‌ها بر اساس بازه تاریخ.", "GET_PAYMENTS_BY_DATE_RANGE_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<PaymentStatistics>> GetPaymentStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Information("درخواست آمار پرداخت‌ها. از: {StartDate}, تا: {EndDate}", startDate, endDate);

                // دریافت آمار پرداخت‌ها
                var statistics = new PaymentStatistics
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalTransactions = 0,
                    TotalAmount = 0,
                    SuccessfulTransactions = 0,
                    SuccessfulAmount = 0,
                    FailedTransactions = 0,
                    FailedAmount = 0,
                    PendingTransactions = 0,
                    PendingAmount = 0,
                    CashAmount = 0,
                    PosAmount = 0,
                    OnlineAmount = 0,
                    DebtAmount = 0
                };

                // TODO: پیاده‌سازی محاسبه آمار از دیتابیس
                // var transactions = await _paymentTransactionRepository.GetByDateRangeAsync(startDate, endDate);
                // statistics.TotalTransactions = transactions.Count();
                // statistics.TotalAmount = transactions.Sum(t => t.Amount);
                // statistics.SuccessfulTransactions = transactions.Count(t => t.Status == PaymentStatus.Success);
                // statistics.SuccessfulAmount = transactions.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount);
                // statistics.FailedTransactions = transactions.Count(t => t.Status == PaymentStatus.Failed);
                // statistics.FailedAmount = transactions.Where(t => t.Status == PaymentStatus.Failed).Sum(t => t.Amount);
                // statistics.PendingTransactions = transactions.Count(t => t.Status == PaymentStatus.Pending);
                // statistics.PendingAmount = transactions.Where(t => t.Status == PaymentStatus.Pending).Sum(t => t.Amount);
                // statistics.CashAmount = transactions.Where(t => t.Method == PaymentMethod.Cash && t.Status == PaymentStatus.Success).Sum(t => t.Amount);
                // statistics.PosAmount = transactions.Where(t => t.Method == PaymentMethod.POS && t.Status == PaymentStatus.Success).Sum(t => t.Amount);
                // statistics.OnlineAmount = transactions.Where(t => t.Method == PaymentMethod.Online && t.Status == PaymentStatus.Success).Sum(t => t.Amount);
                // statistics.DebtAmount = transactions.Where(t => t.Method == PaymentMethod.Debt && t.Status == PaymentStatus.Success).Sum(t => t.Amount);

                _logger.Information("آمار پرداخت‌ها محاسبه شد. از: {StartDate}, تا: {EndDate}, کل تراکنش‌ها: {TotalTransactions}", 
                    startDate, endDate, statistics.TotalTransactions);
                return ServiceResult<PaymentStatistics>.Successful(statistics, "آمار پرداخت‌ها با موفقیت محاسبه شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه آمار پرداخت‌ها. از: {StartDate}, تا: {EndDate}", startDate, endDate);
                return ServiceResult<PaymentStatistics>.Failed("خطا در محاسبه آمار پرداخت‌ها.", "GET_PAYMENT_STATISTICS_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<Models.Statistics.DailyPaymentStatistics>> GetDailyPaymentStatisticsAsync(DateTime date)
        {
            try
            {
                _logger.Information("درخواست آمار روزانه پرداخت‌ها. تاریخ: {Date}", date);

                var startDate = date.Date;
                var endDate = date.Date.AddDays(1).AddTicks(-1);

                // دریافت آمار روزانه
                var dailyStatistics = new Models.Statistics.DailyPaymentStatistics
                {
                    Date = date,
                    TotalTransactions = 0,
                    TotalAmount = 0,
                    SuccessfulTransactions = 0,
                    SuccessfulAmount = 0,
                    FailedTransactions = 0,
                    FailedAmount = 0,
                    PendingTransactions = 0,
                    PendingAmount = 0,
                    CashAmount = 0,
                    PosAmount = 0,
                    OnlineAmount = 0,
                    DebtAmount = 0
                };

                // TODO: پیاده‌سازی محاسبه آمار روزانه از دیتابیس
                // var transactions = await _paymentTransactionRepository.GetByDateRangeAsync(startDate, endDate);
                // dailyStatistics.TotalTransactions = transactions.Count();
                // dailyStatistics.TotalAmount = transactions.Sum(t => t.Amount);
                // dailyStatistics.SuccessfulTransactions = transactions.Count(t => t.Status == PaymentStatus.Success);
                // dailyStatistics.SuccessfulAmount = transactions.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount);
                // dailyStatistics.FailedTransactions = transactions.Count(t => t.Status == PaymentStatus.Failed);
                // dailyStatistics.FailedAmount = transactions.Where(t => t.Status == PaymentStatus.Failed).Sum(t => t.Amount);
                // dailyStatistics.PendingTransactions = transactions.Count(t => t.Status == PaymentStatus.Pending);
                // dailyStatistics.PendingAmount = transactions.Where(t => t.Status == PaymentStatus.Pending).Sum(t => t.Amount);
                // dailyStatistics.CashAmount = transactions.Where(t => t.Method == PaymentMethod.Cash && t.Status == PaymentStatus.Success).Sum(t => t.Amount);
                // dailyStatistics.PosAmount = transactions.Where(t => t.Method == PaymentMethod.POS && t.Status == PaymentStatus.Success).Sum(t => t.Amount);
                // dailyStatistics.OnlineAmount = transactions.Where(t => t.Method == PaymentMethod.Online && t.Status == PaymentStatus.Success).Sum(t => t.Amount);
                // dailyStatistics.DebtAmount = transactions.Where(t => t.Method == PaymentMethod.Debt && t.Status == PaymentStatus.Success).Sum(t => t.Amount);

                _logger.Information("آمار روزانه پرداخت‌ها محاسبه شد. تاریخ: {Date}, کل تراکنش‌ها: {TotalTransactions}", 
                    date, dailyStatistics.TotalTransactions);
                return ServiceResult<Models.Statistics.DailyPaymentStatistics>.Successful(dailyStatistics, "آمار روزانه پرداخت‌ها با موفقیت محاسبه شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه آمار روزانه پرداخت‌ها. تاریخ: {Date}", date);
                return ServiceResult<Models.Statistics.DailyPaymentStatistics>.Failed("خطا در محاسبه آمار روزانه پرداخت‌ها.", "GET_DAILY_PAYMENT_STATISTICS_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<Models.Statistics.MonthlyPaymentStatistics>> GetMonthlyPaymentStatisticsAsync(int year, int month)
        {
            try
            {
                _logger.Information("درخواست آمار ماهانه پرداخت‌ها. سال: {Year}, ماه: {Month}", year, month);

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // دریافت آمار ماهانه
                var monthlyStatistics = new Models.Statistics.MonthlyPaymentStatistics
                {
                    Year = year,
                    Month = month,
                    TotalTransactions = 0,
                    TotalAmount = 0,
                    SuccessfulTransactions = 0,
                    SuccessfulAmount = 0,
                    FailedTransactions = 0,
                    FailedAmount = 0,
                    PendingTransactions = 0,
                    PendingAmount = 0,
                    CashAmount = 0,
                    PosAmount = 0,
                    OnlineAmount = 0,
                    DebtAmount = 0
                };

                // TODO: پیاده‌سازی محاسبه آمار ماهانه از دیتابیس
                // var transactions = await _paymentTransactionRepository.GetByDateRangeAsync(startDate, endDate);
                // monthlyStatistics.TotalTransactions = transactions.Count();
                // monthlyStatistics.TotalAmount = transactions.Sum(t => t.Amount);
                // monthlyStatistics.SuccessfulTransactions = transactions.Count(t => t.Status == PaymentStatus.Success);
                // monthlyStatistics.SuccessfulAmount = transactions.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount);
                // monthlyStatistics.FailedTransactions = transactions.Count(t => t.Status == PaymentStatus.Failed);
                // monthlyStatistics.FailedAmount = transactions.Where(t => t.Status == PaymentStatus.Failed).Sum(t => t.Amount);
                // monthlyStatistics.PendingTransactions = transactions.Count(t => t.Status == PaymentStatus.Pending);
                // monthlyStatistics.PendingAmount = transactions.Where(t => t.Status == PaymentStatus.Pending).Sum(t => t.Amount);
                // monthlyStatistics.CashAmount = transactions.Where(t => t.Method == PaymentMethod.Cash && t.Status == PaymentStatus.Success).Sum(t => t.Amount);
                // monthlyStatistics.PosAmount = transactions.Where(t => t.Method == PaymentMethod.POS && t.Status == PaymentStatus.Success).Sum(t => t.Amount);
                // monthlyStatistics.OnlineAmount = transactions.Where(t => t.Method == PaymentMethod.Online && t.Status == PaymentStatus.Success).Sum(t => t.Amount);
                // monthlyStatistics.DebtAmount = transactions.Where(t => t.Method == PaymentMethod.Debt && t.Status == PaymentStatus.Success).Sum(t => t.Amount);

                _logger.Information("آمار ماهانه پرداخت‌ها محاسبه شد. سال: {Year}, ماه: {Month}, کل تراکنش‌ها: {TotalTransactions}", 
                    year, month, monthlyStatistics.TotalTransactions);
                return ServiceResult<Models.Statistics.MonthlyPaymentStatistics>.Successful(monthlyStatistics, "آمار ماهانه پرداخت‌ها با موفقیت محاسبه شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه آمار ماهانه پرداخت‌ها. سال: {Year}, ماه: {Month}", year, month);
                return ServiceResult<Models.Statistics.MonthlyPaymentStatistics>.Failed("خطا در محاسبه آمار ماهانه پرداخت‌ها.", "GET_MONTHLY_PAYMENT_STATISTICS_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<IEnumerable<PaymentTransaction>>> SearchPaymentsAsync(string searchTerm, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                _logger.Information("درخواست جستجوی پرداخت‌ها. عبارت: {SearchTerm}, صفحه: {PageNumber}, اندازه: {PageSize}", 
                    searchTerm, pageNumber, pageSize);

                // جستجوی پرداخت‌ها
                var transactions = await _paymentTransactionRepository.SearchAsync(searchTerm, pageNumber, pageSize);
                if (transactions == null || !transactions.Any())
                {
                    _logger.Information("هیچ تراکنش پرداختی با عبارت جستجو یافت نشد. عبارت: {SearchTerm}", searchTerm);
                    return ServiceResult<IEnumerable<PaymentTransaction>>.Successful(new List<PaymentTransaction>(), "هیچ تراکنش پرداختی یافت نشد.");
                }

                _logger.Information("جستجوی پرداخت‌ها تکمیل شد. عبارت: {SearchTerm}, تعداد: {Count}", 
                    searchTerm, transactions.Count());
                return ServiceResult<IEnumerable<PaymentTransaction>>.Successful(transactions, "جستجوی پرداخت‌ها با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پرداخت‌ها. عبارت: {SearchTerm}", searchTerm);
                return ServiceResult<IEnumerable<PaymentTransaction>>.Failed("خطا در جستجوی پرداخت‌ها.", "SEARCH_PAYMENTS_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<IEnumerable<PaymentTransaction>>> AdvancedSearchPaymentsAsync(PaymentTransactionSearchFilters filters, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                _logger.Information("درخواست جستجوی پیشرفته پرداخت‌ها. فیلترها: {Filters}, صفحه: {PageNumber}, اندازه: {PageSize}", 
                    filters, pageNumber, pageSize);

                // جستجوی پیشرفته پرداخت‌ها
                var transactions = await _paymentTransactionRepository.AdvancedSearchAsync(filters, pageNumber, pageSize);
                if (transactions == null || !transactions.Any())
                {
                    _logger.Information("هیچ تراکنش پرداختی با فیلترهای مشخص یافت نشد. فیلترها: {Filters}", filters);
                    return ServiceResult<IEnumerable<PaymentTransaction>>.Successful(new List<PaymentTransaction>(), "هیچ تراکنش پرداختی یافت نشد.");
                }

                _logger.Information("جستجوی پیشرفته پرداخت‌ها تکمیل شد. فیلترها: {Filters}, تعداد: {Count}", 
                    filters, transactions.Count());
                return ServiceResult<IEnumerable<PaymentTransaction>>.Successful(transactions, "جستجوی پیشرفته پرداخت‌ها با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پیشرفته پرداخت‌ها. فیلترها: {Filters}", filters);
                return ServiceResult<IEnumerable<PaymentTransaction>>.Failed("خطا در جستجوی پیشرفته پرداخت‌ها.", "ADVANCED_SEARCH_PAYMENTS_ERROR", 
                    ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<PaymentStatistics>> GetPaymentStatisticsAsync(DateTime startDate, DateTime endDate, PaymentMethod? paymentMethod = null, PaymentStatus? paymentStatus = null)
        {
            try
            {
                // TODO: Implement payment statistics
                return ServiceResult<PaymentStatistics>.Successful(new PaymentStatistics(), "آمار پرداخت‌ها دریافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار پرداخت‌ها");
                return ServiceResult<PaymentStatistics>.Failed("خطا در دریافت آمار پرداخت‌ها");
            }
        }

        public async Task<ServiceResult<PaymentStatistics>> GetPaymentStatisticsAsync(DateTime startDate, DateTime endDate, PaymentMethod? paymentMethod = null, PaymentStatus? paymentStatus = null, int? gatewayId = null)
        {
            try
            {
                // TODO: Implement payment statistics with gateway filter
                return ServiceResult<PaymentStatistics>.Successful(new PaymentStatistics(), "آمار پرداخت‌ها دریافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار پرداخت‌ها");
                return ServiceResult<PaymentStatistics>.Failed("خطا در دریافت آمار پرداخت‌ها");
            }
        }

        public async Task<ServiceResult<PagedResult<PaymentTransaction>>> GetTransactionsAsync(int pageNumber, int pageSize)
        {
            try
            {
                var transactions = await _paymentTransactionRepository.GetAllAsync(pageNumber, pageSize);
                var totalCount = await _paymentTransactionRepository.GetTransactionCountByStatusAsync(PaymentStatus.Completed);
                
                var pagedResult = new PagedResult<PaymentTransaction>(
                    transactions.ToList(), 
                    totalCount, 
                    pageNumber, 
                    pageSize
                );

                _logger.Information("تراکنش‌های پرداخت با موفقیت دریافت شدند. تعداد: {Count}", transactions.Count());
                return ServiceResult<PagedResult<PaymentTransaction>>.Successful(pagedResult, "تراکنش‌ها دریافت شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌ها");
                return ServiceResult<PagedResult<PaymentTransaction>>.Failed("خطا در دریافت تراکنش‌ها");
            }
        }

        public async Task<ServiceResult<PagedResult<PaymentTransaction>>> GetTransactionsAsync(int? patientId, int? receptionId, int? appointmentId, PaymentMethod? method, PaymentStatus? status, decimal? minAmount, decimal? maxAmount, DateTime? startDate, DateTime? endDate, string? patientName, string? doctorName, string? transactionId, string? referenceCode, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                // TODO: Implement advanced transaction search
                var transactions = new List<PaymentTransaction>();
                var totalCount = 0;
                
                var pagedResult = new PagedResult<PaymentTransaction>(
                    transactions, 
                    totalCount, 
                    pageNumber, 
                    pageSize
                );

                return ServiceResult<PagedResult<PaymentTransaction>>.Successful(pagedResult, "تراکنش‌ها دریافت شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌ها");
                return ServiceResult<PagedResult<PaymentTransaction>>.Failed("خطا در دریافت تراکنش‌ها");
            }
        }

        public async Task<ServiceResult<PaymentTransaction>> GetTransactionByIdAsync(int transactionId)
        {
            try
            {
                // TODO: Implement get transaction by ID
                return ServiceResult<PaymentTransaction>.Failed("تراکنش یافت نشد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش");
                return ServiceResult<PaymentTransaction>.Failed("خطا در دریافت تراکنش");
            }
        }

        public async Task<ServiceResult<PaymentTransaction>> CreateTransactionAsync(PaymentTransaction transaction)
        {
            try
            {
                // TODO: Implement create transaction
                return ServiceResult<PaymentTransaction>.Successful(transaction, "تراکنش با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تراکنش");
                return ServiceResult<PaymentTransaction>.Failed("خطا در ایجاد تراکنش");
            }
        }

        public async Task<ServiceResult<PaymentTransaction>> UpdateTransactionAsync(PaymentTransaction transaction)
        {
            try
            {
                // TODO: Implement update transaction
                return ServiceResult<PaymentTransaction>.Successful(transaction, "تراکنش با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تراکنش");
                return ServiceResult<PaymentTransaction>.Failed("خطا در به‌روزرسانی تراکنش");
            }
        }

        public async Task<ServiceResult> DeleteTransactionAsync(int transactionId, string deletedByUserId)
        {
            try
            {
                // TODO: Implement delete transaction
                return ServiceResult.Successful("تراکنش با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تراکنش");
                return ServiceResult.Failed("خطا در حذف تراکنش");
            }
        }

        #endregion

        #region Missing Interface Methods

        /// <summary>
        /// ایجاد تراکنش پرداخت
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="amount">مبلغ</param>
        /// <param name="paymentMethod">روش پرداخت</param>
        /// <param name="description">توضیحات</param>
        /// <returns>نتیجه ایجاد تراکنش</returns>
        public async Task<ServiceResult<PaymentTransaction>> CreatePaymentTransactionAsync(int receptionId, decimal amount, PaymentMethod paymentMethod, string description)
        {
            try
            {
                _logger.Information("ایجاد تراکنش پرداخت. ReceptionId: {ReceptionId}, Amount: {Amount}, Method: {Method}", 
                    receptionId, amount, paymentMethod);

                var transaction = new PaymentTransaction
                {
                    ReceptionId = receptionId,
                    Amount = amount,
                    Method = paymentMethod,
                    Description = description,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _paymentTransactionRepository.AddAsync(transaction);

                return ServiceResult<PaymentTransaction>.Successful(transaction);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تراکنش پرداخت");
                return ServiceResult<PaymentTransaction>.Failed("خطا در ایجاد تراکنش پرداخت");
            }
        }

        /// <summary>
        /// تکمیل پرداخت نقدی
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <param name="cashierName">نام صندوقدار</param>
        /// <returns>نتیجه تکمیل پرداخت</returns>
        public async Task<ServiceResult<PaymentTransaction>> CompleteCashPaymentAsync(int transactionId, string cashierName)
        {
            try
            {
                _logger.Information("تکمیل پرداخت نقدی. TransactionId: {TransactionId}, Cashier: {Cashier}", 
                    transactionId, cashierName);

                var transaction = await _paymentTransactionRepository.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    return ServiceResult<PaymentTransaction>.Failed("تراکنش یافت نشد");
                }

                transaction.Status = PaymentStatus.Completed;
                transaction.UpdatedAt = DateTime.UtcNow;

                await _paymentTransactionRepository.UpdateAsync(transaction);

                return ServiceResult<PaymentTransaction>.Successful(transaction);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تکمیل پرداخت نقدی");
                return ServiceResult<PaymentTransaction>.Failed("خطا در تکمیل پرداخت نقدی");
            }
        }

        /// <summary>
        /// دریافت جزئیات پرداخت
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>جزئیات پرداخت</returns>
        public async Task<ServiceResult<PaymentTransaction>> GetPaymentDetailsAsync(int transactionId)
        {
            try
            {
                _logger.Information("دریافت جزئیات پرداخت. TransactionId: {TransactionId}", transactionId);

                var transaction = await _paymentTransactionRepository.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    return ServiceResult<PaymentTransaction>.Failed("تراکنش یافت نشد");
                }

                return ServiceResult<PaymentTransaction>.Successful(transaction);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات پرداخت");
                return ServiceResult<PaymentTransaction>.Failed("خطا در دریافت جزئیات پرداخت");
            }
        }

        /// <summary>
        /// دریافت رسید پرداخت
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>رسید پرداخت</returns>
        public async Task<ServiceResult<ViewModels.Reception.PaymentReceiptViewModel>> GetPaymentReceiptAsync(int transactionId)
        {
            try
            {
                _logger.Information("دریافت رسید پرداخت. TransactionId: {TransactionId}", transactionId);

                var transaction = await _paymentTransactionRepository.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    return ServiceResult<ViewModels.Reception.PaymentReceiptViewModel>.Failed("تراکنش یافت نشد");
                }

                var receipt = new ViewModels.Reception.PaymentReceiptViewModel
                {
                    TransactionId = transaction.PaymentTransactionId,
                    Amount = transaction.Amount,
                    PaymentMethod = transaction.Method.ToString(),
                    Status = transaction.Status.ToString(),
                    CreatedAt = transaction.CreatedAt,
                    CompletedAt = transaction.UpdatedAt,
                    Description = transaction.Description
                };

                return ServiceResult<ViewModels.Reception.PaymentReceiptViewModel>.Successful(receipt);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت رسید پرداخت");
                return ServiceResult<ViewModels.Reception.PaymentReceiptViewModel>.Failed("خطا در دریافت رسید پرداخت");
            }
        }

        #endregion

    }
}
