using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Interfaces.Payment.Management;
using ClinicApp.Interfaces.Payment.Web;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Admin.PaymentManagement;
using ClinicApp.ViewModels.Shared;
using Serilog;

namespace ClinicApp.Services.Payment.Management
{
    /// <summary>
    /// Service برای مدیریت پرداخت‌ها (Admin)
    /// طراحی شده طبق اصول SRP - مسئولیت: Business Logic برای Payment Management
    /// 
    /// Architecture Principles Applied:
    /// ✅ Single Responsibility: Owns all payment management business logic
    /// ✅ Dependency Inversion: Depends on repository abstractions
    /// ✅ Clean Architecture: Service layer orchestrates domain operations
    /// ✅ Medical Standards: Implements healthcare industry best practices
    /// ✅ Security: Complete audit trail and validation
    /// 
    /// Flow: Controller -> PaymentManagementService -> PaymentManagementRepository -> Database
    /// </summary>
    public class PaymentManagementService : IPaymentManagementService
    {
        #region Fields and Constructor

        private readonly IPaymentManagementRepository _repository;
        private readonly IWebPaymentService _webPaymentService;
        private readonly IOnlinePaymentRepository _onlinePaymentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public PaymentManagementService(
            IPaymentManagementRepository repository,
            IWebPaymentService webPaymentService,
            IOnlinePaymentRepository onlinePaymentRepository,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _webPaymentService = webPaymentService ?? throw new ArgumentNullException(nameof(webPaymentService));
            _onlinePaymentRepository = onlinePaymentRepository ?? throw new ArgumentNullException(nameof(onlinePaymentRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<PaymentManagementService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Get Payments

        /// <summary>
        /// دریافت لیست پرداخت‌ها
        /// </summary>
        public async Task<ServiceResult<PaymentIndexViewModel>> GetPaymentsAsync(
            PaymentSearchFilter filter,
            int page,
            int pageSize)
        {
            try
            {
                _logger.Information("درخواست لیست پرداخت‌ها - Filter: {@Filter}, Page: {Page}, PageSize: {PageSize}. User: {UserId}",
                    filter, page, pageSize, _currentUserService.UserId);

                // ✅ Validation
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100; // حداکثر 100 آیتم در هر صفحه

                // ✅ دریافت داده از Repository
                var pagedResult = await _repository.GetPaymentsAsync(filter, page, pageSize);
                var statistics = await _repository.GetPaymentStatisticsAsync(filter);

                // ✅ تبدیل به ViewModel
                var payments = pagedResult.Items.Select(p => MapToListItemViewModel(p)).ToList();

                // ✅ ایجاد PaginationViewModel
                var pagingInfo = new PaginationViewModel
                {
                    CurrentPage = pagedResult.PageNumber,
                    TotalPages = pagedResult.TotalPages,
                    TotalCount = pagedResult.TotalItems,
                    PageSize = pagedResult.PageSize
                    // HasPreviousPage و HasNextPage به صورت خودکار محاسبه می‌شوند
                };

                var viewModel = new PaymentIndexViewModel
                {
                    Payments = payments,
                    Filter = filter ?? new PaymentSearchFilter(),
                    PagingInfo = pagingInfo,
                    Statistics = statistics
                };

                _logger.Information("✅ لیست پرداخت‌ها با موفقیت دریافت شد - Count: {Count}, Total: {Total}. User: {UserId}",
                    payments.Count, pagedResult.TotalItems, _currentUserService.UserId);

                return ServiceResult<PaymentIndexViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پرداخت‌ها. User: {UserId}", _currentUserService.UserId);
                return ServiceResult<PaymentIndexViewModel>.Failed("خطا در دریافت لیست پرداخت‌ها");
            }
        }

        #endregion

        #region Get Payment Details

        /// <summary>
        /// دریافت جزئیات پرداخت
        /// </summary>
        public async Task<ServiceResult<PaymentDetailsViewModel>> GetPaymentDetailsAsync(int onlinePaymentId)
        {
            try
            {
                _logger.Information("درخواست جزئیات پرداخت - OnlinePaymentId: {OnlinePaymentId}. User: {UserId}",
                    onlinePaymentId, _currentUserService.UserId);

                // ✅ Validation
                if (onlinePaymentId <= 0)
                {
                    return ServiceResult<PaymentDetailsViewModel>.Failed("شناسه پرداخت نامعتبر است");
                }

                // ✅ دریافت داده از Repository
                var payment = await _repository.GetPaymentDetailsAsync(onlinePaymentId);
                if (payment == null)
                {
                    _logger.Warning("پرداخت یافت نشد - OnlinePaymentId: {OnlinePaymentId}. User: {UserId}",
                        onlinePaymentId, _currentUserService.UserId);
                    return ServiceResult<PaymentDetailsViewModel>.Failed("پرداخت یافت نشد");
                }

                // ✅ دریافت Timeline
                var timeline = await _repository.GetPaymentTimelineAsync(onlinePaymentId);

                // ✅ تبدیل به ViewModel
                var viewModel = MapToDetailsViewModel(payment, timeline);

                _logger.Information("✅ جزئیات پرداخت با موفقیت دریافت شد - OnlinePaymentId: {OnlinePaymentId}. User: {UserId}",
                    onlinePaymentId, _currentUserService.UserId);

                return ServiceResult<PaymentDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات پرداخت - OnlinePaymentId: {OnlinePaymentId}. User: {UserId}",
                    onlinePaymentId, _currentUserService.UserId);
                return ServiceResult<PaymentDetailsViewModel>.Failed("خطا در دریافت جزئیات پرداخت");
            }
        }

        #endregion

        #region Retry Payment

        /// <summary>
        /// Retry پرداخت
        /// </summary>
        public async Task<ServiceResult> RetryPaymentAsync(int onlinePaymentId, string userId)
        {
            try
            {
                _logger.Information("درخواست Retry پرداخت - OnlinePaymentId: {OnlinePaymentId}, UserId: {UserId}",
                    onlinePaymentId, userId);

                // ✅ Validation
                if (onlinePaymentId <= 0)
                {
                    return ServiceResult.Failed("شناسه پرداخت نامعتبر است");
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    userId = _currentUserService.UserId;
                }

                // ✅ دریافت پرداخت
                var payment = await _repository.GetPaymentDetailsAsync(onlinePaymentId);
                if (payment == null)
                {
                    return ServiceResult.Failed("پرداخت یافت نشد");
                }

                // ✅ بررسی وضعیت - فقط پرداخت‌های ناموفق یا لغو شده قابل Retry هستند
                if (payment.Status != OnlinePaymentStatus.Failed && payment.Status != OnlinePaymentStatus.Canceled)
                {
                    return ServiceResult.Failed($"پرداخت با وضعیت {GetStatusDisplay(payment.Status)} قابل Retry نیست");
                }

                // ✅ یکپارچه‌سازی با WebPaymentService
                // Reset کردن OnlinePayment و ایجاد درخواست پرداخت جدید
                if (string.IsNullOrWhiteSpace(payment.PaymentToken))
                {
                    return ServiceResult.Failed("توکن پرداخت یافت نشد. امکان Retry وجود ندارد");
                }

                // ✅ Reset کردن وضعیت به Pending
                payment.Status = OnlinePaymentStatus.Pending;
                payment.ErrorCode = null;
                payment.ErrorMessage = null;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.UpdatedByUserId = userId;

                // ✅ به‌روزرسانی OnlinePayment
                var updateResult = await _onlinePaymentRepository.UpdateAsync(payment);
                if (updateResult == null)
                {
                    _logger.Error("❌ Retry Payment: خطا در به‌روزرسانی OnlinePayment");
                    return ServiceResult.Failed("خطا در به‌روزرسانی پرداخت");
                }

                // ✅ ایجاد درخواست پرداخت جدید در درگاه
                var createPaymentRequest = new CreatePaymentRequest
                {
                    OnlinePaymentId = payment.OnlinePaymentId,
                    GatewayType = payment.PaymentGateway.GatewayType,
                    Amount = payment.Amount,
                    Description = payment.Description ?? "پرداخت مجدد",
                    CallbackUrl = payment.PaymentUrl?.Replace(payment.PaymentToken, "{token}") ?? string.Empty
                };

                var gatewayResponse = await _webPaymentService.CreatePaymentRequestAsync(createPaymentRequest);
                if (!gatewayResponse.Success || gatewayResponse.Data == null)
                {
                    _logger.Error("❌ Retry Payment: خطا در ایجاد درخواست پرداخت جدید - {Message}", gatewayResponse.Message);
                    return ServiceResult.Failed(gatewayResponse.Message ?? "خطا در ایجاد درخواست پرداخت جدید");
                }

                // ✅ به‌روزرسانی OnlinePayment با اطلاعات جدید
                updateResult.PaymentToken = gatewayResponse.Data.PaymentToken ?? gatewayResponse.Data.GatewayTransactionId;
                updateResult.GatewayTransactionId = gatewayResponse.Data.GatewayTransactionId;
                updateResult.PaymentUrl = gatewayResponse.Data.PaymentUrl;
                updateResult.UpdatedAt = DateTime.UtcNow;
                updateResult.UpdatedByUserId = userId;

                var finalUpdateResult = await _onlinePaymentRepository.UpdateAsync(updateResult);
                if (finalUpdateResult == null)
                {
                    _logger.Error("❌ Retry Payment: خطا در به‌روزرسانی نهایی OnlinePayment");
                    return ServiceResult.Failed("خطا در به‌روزرسانی نهایی پرداخت");
                }

                _logger.Information("✅ Retry Payment موفق - OnlinePaymentId: {OnlinePaymentId}, New PaymentUrl: {PaymentUrl}",
                    onlinePaymentId, finalUpdateResult.PaymentUrl);

                return ServiceResult.Successful("درخواست پرداخت مجدد با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در Retry پرداخت - OnlinePaymentId: {OnlinePaymentId}, UserId: {UserId}, Exception: {Exception}",
                    onlinePaymentId, userId, ex.Message);
                return ServiceResult.Failed(
                    "خطا در Retry پرداخت. لطفاً با پشتیبانی تماس بگیرید.",
                    "RETRY_ERROR");
            }
        }

        #endregion

        #region Cancel Payment

        /// <summary>
        /// Cancel پرداخت
        /// </summary>
        public async Task<ServiceResult> CancelPaymentAsync(int onlinePaymentId, string reason, string userId)
        {
            try
            {
                _logger.Information("درخواست Cancel پرداخت - OnlinePaymentId: {OnlinePaymentId}, Reason: {Reason}, UserId: {UserId}",
                    onlinePaymentId, reason, userId);

                // ✅ Validation
                if (onlinePaymentId <= 0)
                {
                    return ServiceResult.Failed("شناسه پرداخت نامعتبر است");
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    userId = _currentUserService.UserId;
                }

                // ✅ دریافت پرداخت
                var payment = await _repository.GetPaymentDetailsAsync(onlinePaymentId);
                if (payment == null)
                {
                    return ServiceResult.Failed("پرداخت یافت نشد");
                }

                // ✅ بررسی وضعیت - فقط پرداخت‌های Pending قابل Cancel هستند
                if (payment.Status != OnlinePaymentStatus.Pending)
                {
                    return ServiceResult.Failed($"پرداخت با وضعیت {GetStatusDisplay(payment.Status)} قابل Cancel نیست");
                }

                // ✅ یکپارچه‌سازی با WebPaymentService
                if (string.IsNullOrWhiteSpace(payment.PaymentToken))
                {
                    return ServiceResult.Failed("توکن پرداخت یافت نشد");
                }

                var cancelResult = await _webPaymentService.CancelWebPaymentAsync(payment.PaymentToken, reason);
                if (!cancelResult.Success)
                {
                    _logger.Error("❌ Cancel Payment: خطا در لغو پرداخت - {Message}", cancelResult.Message);
                    return ServiceResult.Failed(cancelResult.Message ?? "خطا در لغو پرداخت");
                }

                // ✅ به‌روزرسانی Audit Trail
                payment.UpdatedByUserId = userId;
                payment.UpdatedAt = DateTime.UtcNow;

                _logger.Information("✅ Cancel Payment موفق - OnlinePaymentId: {OnlinePaymentId}, Reason: {Reason}",
                    onlinePaymentId, reason);

                return ServiceResult.Successful("پرداخت با موفقیت لغو شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در Cancel پرداخت - OnlinePaymentId: {OnlinePaymentId}, UserId: {UserId}, Reason: {Reason}, Exception: {Exception}",
                    onlinePaymentId, userId, reason, ex.Message);
                return ServiceResult.Failed(
                    "خطا در Cancel پرداخت. لطفاً با پشتیبانی تماس بگیرید.",
                    "CANCEL_ERROR");
            }
        }

        #endregion

        #region Refund Payment

        /// <summary>
        /// Refund پرداخت
        /// </summary>
        public async Task<ServiceResult> RefundPaymentAsync(int onlinePaymentId, decimal? refundAmount, string reason, string userId)
        {
            try
            {
                _logger.Information("درخواست Refund پرداخت - OnlinePaymentId: {OnlinePaymentId}, RefundAmount: {RefundAmount}, Reason: {Reason}, UserId: {UserId}",
                    onlinePaymentId, refundAmount, reason, userId);

                // ✅ Validation
                if (onlinePaymentId <= 0)
                {
                    return ServiceResult.Failed("شناسه پرداخت نامعتبر است");
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    userId = _currentUserService.UserId;
                }

                // ✅ دریافت پرداخت
                var payment = await _repository.GetPaymentDetailsAsync(onlinePaymentId);
                if (payment == null)
                {
                    return ServiceResult.Failed("پرداخت یافت نشد");
                }

                // ✅ بررسی وضعیت - فقط پرداخت‌های Successful قابل Refund هستند
                if (payment.Status != OnlinePaymentStatus.Successful)
                {
                    return ServiceResult.Failed($"پرداخت با وضعیت {GetStatusDisplay(payment.Status)} قابل Refund نیست");
                }

                // ✅ Validation مبلغ
                if (refundAmount.HasValue && refundAmount.Value <= 0)
                {
                    return ServiceResult.Failed("مبلغ Refund باید بیشتر از صفر باشد");
                }

                if (refundAmount.HasValue && refundAmount.Value > payment.Amount)
                {
                    return ServiceResult.Failed("مبلغ Refund نمی‌تواند بیشتر از مبلغ پرداخت باشد");
                }

                // ✅ استفاده از مبلغ پرداخت در صورت عدم تعیین
                var finalRefundAmount = refundAmount ?? payment.Amount;

                // ✅ یکپارچه‌سازی با WebPaymentService
                if (string.IsNullOrWhiteSpace(payment.PaymentToken))
                {
                    return ServiceResult.Failed("توکن پرداخت یافت نشد");
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    reason = "برگشت وجه توسط مدیر";
                }

                var refundResult = await _webPaymentService.RefundWebPaymentAsync(
                    payment.PaymentToken,
                    finalRefundAmount,
                    reason);

                if (!refundResult.Success || refundResult.Data == null)
                {
                    _logger.Error("❌ Refund Payment: خطا در برگشت وجه - {Message}", refundResult.Message);
                    return ServiceResult.Failed(refundResult.Message ?? "خطا در برگشت وجه");
                }

                // ✅ به‌روزرسانی OnlinePayment
                payment.Status = OnlinePaymentStatus.Refunded;
                payment.ErrorMessage = $"برگشت وجه: {reason}";
                payment.UpdatedAt = DateTime.UtcNow;
                payment.UpdatedByUserId = userId;

                _logger.Information("✅ Refund Payment موفق - OnlinePaymentId: {OnlinePaymentId}, RefundAmount: {Amount}, RefundId: {RefundId}",
                    onlinePaymentId, finalRefundAmount, refundResult.Data.RefundId);

                return ServiceResult.Successful($"برگشت وجه با موفقیت انجام شد. شناسه برگشت: {refundResult.Data.RefundId}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در Refund پرداخت - OnlinePaymentId: {OnlinePaymentId}, UserId: {UserId}, RefundAmount: {RefundAmount}, Exception: {Exception}",
                    onlinePaymentId, userId, refundAmount, ex.Message);
                return ServiceResult.Failed(
                    "خطا در Refund پرداخت. لطفاً با پشتیبانی تماس بگیرید.",
                    "REFUND_ERROR");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// تبدیل OnlinePayment به PaymentListItemViewModel
        /// </summary>
        private PaymentListItemViewModel MapToListItemViewModel(OnlinePayment payment)
        {
            return new PaymentListItemViewModel
            {
                OnlinePaymentId = payment.OnlinePaymentId,
                AppointmentId = payment.AppointmentId,
                PatientId = payment.PatientId,
                PatientName = payment.Patient?.FullName ?? "نامشخص",
                PatientNationalCode = payment.Patient?.NationalCode ?? string.Empty,
                DoctorId = payment.Appointment?.DoctorId,
                DoctorName = payment.Appointment?.Doctor?.FullName ?? "نامشخص",
                PaymentType = payment.PaymentType,
                PaymentTypeDisplay = GetPaymentTypeDisplay(payment.PaymentType),
                Status = payment.Status,
                StatusDisplay = GetStatusDisplay(payment.Status),
                Amount = payment.Amount,
                AmountDisplay = payment.Amount.ToString("N0") + " ریال",
                GatewayName = payment.PaymentGateway?.Name ?? "نامشخص",
                PaymentToken = payment.PaymentToken,
                GatewayReferenceCode = payment.GatewayReferenceCode,
                CreatedAt = payment.CreatedAt,
                CreatedAtDisplay = payment.CreatedAt.ToPersianDateTime(false),
                PaymentCompletionDate = payment.PaymentCompletionDate,
                PaymentCompletionDateDisplay = payment.PaymentCompletionDate?.ToPersianDateTime(false) ?? string.Empty
            };
        }

        /// <summary>
        /// تبدیل OnlinePayment به PaymentDetailsViewModel
        /// </summary>
        private PaymentDetailsViewModel MapToDetailsViewModel(OnlinePayment payment, List<PaymentTimelineItemViewModel> timeline)
        {
            var viewModel = new PaymentDetailsViewModel
            {
                OnlinePaymentId = payment.OnlinePaymentId,
                AppointmentId = payment.AppointmentId,
                ReceptionId = payment.ReceptionId,
                PatientId = payment.PatientId,
                PatientName = payment.Patient?.FullName ?? "نامشخص",
                PatientNationalCode = payment.Patient?.NationalCode ?? string.Empty,
                PatientPhoneNumber = payment.Patient?.PhoneNumber ?? string.Empty,
                DoctorId = payment.Appointment?.DoctorId,
                DoctorName = payment.Appointment?.Doctor?.FullName ?? "نامشخص",
                DoctorSpecialization = GetDoctorSpecialization(payment.Appointment?.Doctor),
                PaymentType = payment.PaymentType,
                PaymentTypeDisplay = GetPaymentTypeDisplay(payment.PaymentType),
                Status = payment.Status,
                StatusDisplay = GetStatusDisplay(payment.Status),
                Amount = payment.Amount,
                AmountDisplay = payment.Amount.ToString("N0") + " ریال",
                GatewayFee = payment.GatewayFee,
                GatewayFeeDisplay = payment.GatewayFee?.ToString("N0") + " ریال" ?? "0 ریال",
                NetAmount = payment.NetAmount,
                NetAmountDisplay = payment.NetAmount?.ToString("N0") + " ریال" ?? "0 ریال",
                PaymentGatewayId = payment.PaymentGatewayId,
                GatewayName = payment.PaymentGateway?.Name ?? "نامشخص",
                GatewayType = payment.PaymentGateway?.GatewayType ?? PaymentGatewayType.ZarinPal,
                GatewayTypeDisplay = GetGatewayTypeDisplay(payment.PaymentGateway?.GatewayType ?? PaymentGatewayType.ZarinPal),
                PaymentToken = payment.PaymentToken,
                GatewayTransactionId = payment.GatewayTransactionId,
                GatewayReferenceCode = payment.GatewayReferenceCode,
                InternalTransactionId = payment.InternalTransactionId,
                PaymentUrl = payment.PaymentUrl,
                PaymentStartDate = payment.PaymentStartDate,
                PaymentStartDateDisplay = payment.PaymentStartDate?.ToPersianDateTime(false) ?? string.Empty,
                PaymentCompletionDate = payment.PaymentCompletionDate,
                PaymentCompletionDateDisplay = payment.PaymentCompletionDate?.ToPersianDateTime(false) ?? string.Empty,
                PaymentExpiryDate = payment.PaymentExpiryDate,
                PaymentExpiryDateDisplay = payment.PaymentExpiryDate?.ToPersianDateTime(false) ?? string.Empty,
                UserIpAddress = payment.UserIpAddress,
                UserAgent = payment.UserAgent,
                ErrorCode = payment.ErrorCode,
                ErrorMessage = payment.ErrorMessage,
                Description = payment.Description,
                CreatedAt = payment.CreatedAt,
                CreatedAtDisplay = payment.CreatedAt.ToPersianDateTime(false),
                CreatedByUserName = payment.CreatedByUser?.FullName ?? "سیستم",
                UpdatedAt = payment.UpdatedAt,
                UpdatedAtDisplay = payment.UpdatedAt?.ToPersianDateTime(false) ?? string.Empty,
                UpdatedByUserName = payment.UpdatedByUser?.FullName,
                Timeline = timeline,
                CanRetry = payment.Status == OnlinePaymentStatus.Failed || payment.Status == OnlinePaymentStatus.Canceled,
                CanCancel = payment.Status == OnlinePaymentStatus.Pending,
                CanRefund = payment.Status == OnlinePaymentStatus.Successful
            };

            return viewModel;
        }

        /// <summary>
        /// دریافت نمایش فارسی نوع پرداخت
        /// </summary>
        private string GetPaymentTypeDisplay(OnlinePaymentType paymentType)
        {
            switch (paymentType)
            {
                case OnlinePaymentType.Appointment:
                    return "نوبت‌دهی";
                case OnlinePaymentType.Reception:
                    return "پذیرش";
                case OnlinePaymentType.Service:
                    return "خدمات";
                case OnlinePaymentType.Debt:
                    return "بدهی";
                case OnlinePaymentType.PrePayment:
                    return "پیش‌پرداخت";
                default:
                    return paymentType.ToString();
            }
        }

        /// <summary>
        /// دریافت نمایش فارسی وضعیت پرداخت
        /// </summary>
        private string GetStatusDisplay(OnlinePaymentStatus status)
        {
            switch (status)
            {
                case OnlinePaymentStatus.Pending:
                    return "در انتظار";
                case OnlinePaymentStatus.Successful:
                    return "موفق";
                case OnlinePaymentStatus.Failed:
                    return "ناموفق";
                case OnlinePaymentStatus.Canceled:
                    return "لغو شده";
                case OnlinePaymentStatus.Expired:
                    return "منقضی شده";
                default:
                    return status.ToString();
            }
        }

        /// <summary>
        /// دریافت نمایش فارسی نوع درگاه
        /// </summary>
        private string GetGatewayTypeDisplay(PaymentGatewayType gatewayType)
        {
            switch (gatewayType)
            {
                case PaymentGatewayType.ZarinPal:
                    return "زرین‌پال";
                case PaymentGatewayType.PayPing:
                    return "پی‌پینگ";
                case PaymentGatewayType.Saman:
                    return "سامان";
                case PaymentGatewayType.Mellat:
                    return "ملت";
                default:
                    return gatewayType.ToString();
            }
        }

        /// <summary>
        /// دریافت تخصص پزشک
        /// </summary>
        private string GetDoctorSpecialization(dynamic doctor)
        {
            if (doctor == null) return string.Empty;

            try
            {
                // TODO: اگر DoctorSpecializations وجود دارد، از آن استفاده کن
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion
    }
}

