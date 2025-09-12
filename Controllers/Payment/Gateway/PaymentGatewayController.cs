using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment.Gateway;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Models.Statistics;
using ClinicApp.ViewModels.Payment.Gateway;
using ClinicApp.ViewModels.Validators.Payment.Gateway;
using FluentValidation;
using Serilog;

namespace ClinicApp.Controllers.Payment.Gateway
{
    /// <summary>
    /// کنترلر مدیریت درگاه‌های پرداخت و پرداخت‌های آنلاین
    /// </summary>
    [Authorize(Roles = "Admin,Accountant")]
    public class PaymentGatewayController : BaseController
    {
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<PaymentGatewayCreateViewModel> _gatewayCreateValidator;
        private readonly IValidator<PaymentGatewayEditViewModel> _gatewayEditValidator;
        private readonly IValidator<PaymentGatewaySearchViewModel> _gatewaySearchValidator;
        private readonly IValidator<OnlinePaymentCreateViewModel> _onlinePaymentCreateValidator;
        private readonly IValidator<OnlinePaymentSearchViewModel> _onlinePaymentSearchValidator;

        public PaymentGatewayController(
            IPaymentGatewayService paymentGatewayService,
            ICurrentUserService currentUserService,
            IValidator<PaymentGatewayCreateViewModel> gatewayCreateValidator,
            IValidator<PaymentGatewayEditViewModel> gatewayEditValidator,
            IValidator<PaymentGatewaySearchViewModel> gatewaySearchValidator,
            IValidator<OnlinePaymentCreateViewModel> onlinePaymentCreateValidator,
            IValidator<OnlinePaymentSearchViewModel> onlinePaymentSearchValidator,
            ILogger logger) : base(logger)
        {
            _paymentGatewayService = paymentGatewayService ?? throw new ArgumentNullException(nameof(paymentGatewayService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _gatewayCreateValidator = gatewayCreateValidator ?? throw new ArgumentNullException(nameof(gatewayCreateValidator));
            _gatewayEditValidator = gatewayEditValidator ?? throw new ArgumentNullException(nameof(gatewayEditValidator));
            _gatewaySearchValidator = gatewaySearchValidator ?? throw new ArgumentNullException(nameof(gatewaySearchValidator));
            _onlinePaymentCreateValidator = onlinePaymentCreateValidator ?? throw new ArgumentNullException(nameof(onlinePaymentCreateValidator));
            _onlinePaymentSearchValidator = onlinePaymentSearchValidator ?? throw new ArgumentNullException(nameof(onlinePaymentSearchValidator));
        }

        #region Index Actions

        /// <summary>
        /// صفحه اصلی مدیریت درگاه‌های پرداخت
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(PaymentGatewaySearchViewModel gatewaySearchModel,
            OnlinePaymentSearchViewModel paymentSearchModel, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                _logger.Information("درخواست صفحه اصلی مدیریت درگاه‌های پرداخت. صفحه: {PageNumber}, اندازه: {PageSize}, کاربر: {UserName}",
                    pageNumber, pageSize, _currentUserService.UserName);

                // دریافت لیست درگاه‌ها
                var gatewaysResult = await _paymentGatewayService.GetGatewaysAsync(
                    gatewaySearchModel.Name,
                    gatewaySearchModel.GatewayType,
                    gatewaySearchModel.IsActive,
                    gatewaySearchModel.IsDefault,
                    gatewaySearchModel.CreatedByUserId,
                    gatewaySearchModel.StartDate,
                    gatewaySearchModel.EndDate,
                    pageNumber,
                    pageSize);

                if (!gatewaysResult.Success)
                {
                    return HandleServiceError(gatewaysResult);
                }

                // دریافت پرداخت‌های اخیر
                var recentPaymentsResult = await _paymentGatewayService.GetOnlinePaymentsAsync(
                    paymentSearchModel.PatientId,
                    paymentSearchModel.ReceptionId,
                    null, // appointmentId
                    null, // paymentGatewayId
                    paymentSearchModel.PaymentType,
                    paymentSearchModel.Status,
                    paymentSearchModel.MinAmount,
                    paymentSearchModel.MaxAmount,
                    paymentSearchModel.StartDate,
                    paymentSearchModel.EndDate,
                    paymentSearchModel.PatientName,
                    paymentSearchModel.DoctorName,
                    paymentSearchModel.GatewayTransactionId,
                    paymentSearchModel.ReferenceCode,
                    1,
                    10);

                if (!recentPaymentsResult.Success)
                {
                    return HandleServiceError(recentPaymentsResult);
                }

                // دریافت آمار
                var statisticsResult = await _paymentGatewayService.GetGatewayStatisticsAsync();
                if (!statisticsResult.Success)
                {
                    return HandleServiceError(statisticsResult);
                }

                // ایجاد ViewModel
                var viewModel = new PaymentGatewayIndexViewModel
                {
                    Gateways = gatewaysResult.Data?.Items?.Select(g => new PaymentGatewayListViewModel
                    {
                        Id = g.Id,
                        Name = g.Name,
                        GatewayType = g.GatewayType,
                        MerchantId = g.MerchantId,
                        FeePercentage = g.FeePercentage ?? 0,
                        FixedFee = g.FixedFee ?? 0,
                        IsActive = g.IsActive,
                        IsDefault = g.IsDefault,
                        CreatedAt = g.CreatedAt,
                        CreatedByUserName = g.CreatedByUserName,
                        TotalTransactions = g.TotalTransactions,
                        TotalAmount = g.TotalAmount,
                        SuccessRate = g.SuccessRate,
                        AverageResponseTime = g.AverageResponseTime
                    }).ToList() ?? new List<PaymentGatewayListViewModel>(),
                    RecentPayments = recentPaymentsResult.Data?.Items?.Select(p => new OnlinePaymentListViewModel
                    {
                        Id = p.Id,
                        ReceptionId = p.ReceptionId ?? 0,
                        PatientId = p.PatientId,
                        PaymentType = p.PaymentType,
                        Amount = p.Amount,
                        PaymentToken = p.PaymentToken,
                        Status = p.Status,
                        GatewayTransactionId = p.GatewayTransactionId,
                        CreatedAt = p.CreatedAt,
                        CompletedAt = p.CompletedAt,
                        ReceptionNumber = p.ReceptionNumber,
                        PatientName = p.PatientName,
                        DoctorName = p.DoctorName,
                        PaymentGatewayName = p.PaymentGatewayName
                    }).ToList() ?? new List<OnlinePaymentListViewModel>(),
                    SearchModel = gatewaySearchModel,
                    PaymentSearchModel = paymentSearchModel,
                    Statistics = new PaymentGatewayStatisticsViewModel
                    {
                        TotalGateways = statisticsResult.Data?.TotalGateways ?? 0,
                        ActiveGateways = statisticsResult.Data?.ActiveGateways ?? 0,
                        InactiveGateways = statisticsResult.Data?.InactiveGateways ?? 0,
                        DefaultGateways = statisticsResult.Data?.DefaultGateways ?? 0,
                        TotalOnlinePayments = statisticsResult.Data?.TotalOnlinePayments ?? 0,
                        SuccessfulPayments = statisticsResult.Data?.SuccessfulPayments ?? 0,
                        FailedPayments = statisticsResult.Data?.FailedPayments ?? 0,
                        PendingPayments = statisticsResult.Data?.PendingPayments ?? 0,
                        TotalAmount = statisticsResult.Data?.TotalAmount ?? 0,
                        SuccessfulAmount = statisticsResult.Data?.SuccessfulAmount ?? 0,
                        FailedAmount = statisticsResult.Data?.FailedAmount ?? 0,
                        PendingAmount = statisticsResult.Data?.PendingAmount ?? 0,
                        SuccessRate = statisticsResult.Data?.SuccessRate ?? 0,
                        AverageResponseTime = statisticsResult.Data?.AverageResponseTime ?? 0,
                        GatewaysByType = statisticsResult.Data?.GatewaysByType ?? new Dictionary<PaymentGatewayType, int>(),
                        PaymentsByType = statisticsResult.Data?.PaymentsByType ?? new Dictionary<OnlinePaymentType, int>(),
                        PaymentsByStatus = statisticsResult.Data?.PaymentsByStatus ?? new Dictionary<OnlinePaymentStatus, int>()
                    },
                    TotalGatewayCount = gatewaysResult.Data?.TotalItems ?? 0,
                    TotalPaymentCount = recentPaymentsResult.Data?.TotalItems ?? 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)(gatewaysResult.Data?.TotalItems ?? 0) / pageSize)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش صفحه اصلی مدیریت درگاه‌های پرداخت");
            }
        }

        #endregion

        #region Payment Gateway CRUD Actions

        /// <summary>
        /// نمایش جزئیات درگاه پرداخت
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GatewayDetails(int id)
        {
            try
            {
                _logger.Information("درخواست جزئیات درگاه پرداخت. شناسه: {GatewayId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                var result = await _paymentGatewayService.GetGatewayByIdAsync(id);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                var viewModel = new PaymentGatewayDetailsViewModel
                {
                    Id = result.Data.Id,
                    Name = result.Data.Name,
                    GatewayType = result.Data.GatewayType,
                    MerchantId = result.Data.MerchantId,
                    ApiKey = result.Data.ApiKey,
                    ApiSecret = result.Data.ApiSecret,
                    CallbackUrl = result.Data.CallbackUrl,
                    WebhookUrl = result.Data.WebhookUrl,
                    FeePercentage = result.Data.FeePercentage ?? 0,
                    FixedFee = result.Data.FixedFee ?? 0,
                    IsActive = result.Data.IsActive,
                    IsDefault = result.Data.IsDefault,
                    Description = result.Data.Description,
                    CreatedByUserId = result.Data.CreatedByUserId,
                    CreatedAt = result.Data.CreatedAt,
                    UpdatedByUserId = result.Data.UpdatedByUserId,
                    UpdatedAt = result.Data.UpdatedAt,
                    CreatedByUserName = result.Data.CreatedByUserName,
                    UpdatedByUserName = result.Data.UpdatedByUserName,
                    TotalTransactions = result.Data.TotalTransactions,
                    TotalAmount = result.Data.TotalAmount,
                    SuccessRate = result.Data.SuccessRate,
                    AverageResponseTime = result.Data.AverageResponseTime,
                    LastTransactionDate = result.Data.LastTransactionDate
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش جزئیات درگاه پرداخت");
            }
        }

        /// <summary>
        /// نمایش فرم ایجاد درگاه پرداخت
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> CreateGateway()
        {
            try
            {
                _logger.Information("درخواست فرم ایجاد درگاه پرداخت. کاربر: {UserName}",
                    _currentUserService.UserName);

                var viewModel = new PaymentGatewayCreateViewModel();

                // دریافت لیست‌های مورد نیاز
                await PopulateGatewayCreateViewModel(viewModel);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش فرم ایجاد درگاه پرداخت");
            }
        }

        /// <summary>
        /// ایجاد درگاه پرداخت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateGateway(PaymentGatewayCreateViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد درگاه پرداخت. نام: {Name}, نوع: {GatewayType}, کاربر: {UserName}",
                    model.Name, model.GatewayType, _currentUserService.UserName);

                // اعتبارسنجی مدل
                var validation = await _gatewayCreateValidator.ValidateAsync(model);
                if (!validation.IsValid)
                {
                    await PopulateGatewayCreateViewModel(model);
                    return HandleValidationErrors(validation.Errors.Select(e => e.ErrorMessage));
                }

                // ایجاد درگاه
                var result = await _paymentGatewayService.CreateGatewayAsync(new PaymentGateway
                {
                    Name = model.Name,
                    GatewayType = model.GatewayType,
                    MerchantId = model.MerchantId,
                    ApiKey = model.ApiKey,
                    ApiSecret = model.ApiSecret,
                    CallbackUrl = model.CallbackUrl,
                    WebhookUrl = model.WebhookUrl,
                    FeePercentage = model.FeePercentage,
                    FixedFee = model.FixedFee,
                    IsActive = model.IsActive,
                    IsDefault = model.IsDefault,
                    Description = model.Description,
                    CreatedByUserId = _currentUserService.UserId
                });

                if (!result.Success)
                {
                    await PopulateGatewayCreateViewModel(model);
                    return HandleServiceError(result);
                }

                _logger.Information("درگاه پرداخت با موفقیت ایجاد شد. شناسه: {GatewayId}, کاربر: {UserName}",
                    result.Data?.Id, _currentUserService.UserName);

                return RedirectToAction("GatewayDetails", new { id = result.Data?.Id });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ایجاد درگاه پرداخت");
            }
        }

        /// <summary>
        /// نمایش فرم ویرایش درگاه پرداخت
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> EditGateway(int id)
        {
            try
            {
                _logger.Information("درخواست فرم ویرایش درگاه پرداخت. شناسه: {GatewayId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                var result = await _paymentGatewayService.GetGatewayByIdAsync(id);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                var viewModel = new PaymentGatewayEditViewModel
                {
                    Id = result.Data.Id,
                    Name = result.Data.Name,
                    GatewayType = result.Data.GatewayType,
                    MerchantId = result.Data.MerchantId,
                    ApiKey = result.Data.ApiKey,
                    ApiSecret = result.Data.ApiSecret,
                    CallbackUrl = result.Data.CallbackUrl,
                    WebhookUrl = result.Data.WebhookUrl,
                    FeePercentage = result.Data.FeePercentage ?? 0,
                    FixedFee = result.Data.FixedFee ?? 0,
                    IsActive = result.Data.IsActive,
                    IsDefault = result.Data.IsDefault,
                    Description = result.Data.Description
                };

                // دریافت لیست‌های مورد نیاز
                await PopulateGatewayEditViewModel(viewModel);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش فرم ویرایش درگاه پرداخت");
            }
        }

        /// <summary>
        /// ویرایش درگاه پرداخت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditGateway(PaymentGatewayEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ویرایش درگاه پرداخت. شناسه: {GatewayId}, کاربر: {UserName}",
                    model.Id, _currentUserService.UserName);

                // اعتبارسنجی مدل
                var validation = await _gatewayEditValidator.ValidateAsync(model);
                if (!validation.IsValid)
                {
                    await PopulateGatewayEditViewModel(model);
                    return HandleValidationErrors(validation.Errors.Select(e => e.ErrorMessage));
                }

                // ویرایش درگاه
                var result = await _paymentGatewayService.UpdateGatewayAsync(new PaymentGateway
                {
                    Id = model.Id,
                    Name = model.Name,
                    GatewayType = model.GatewayType,
                    MerchantId = model.MerchantId,
                    ApiKey = model.ApiKey,
                    ApiSecret = model.ApiSecret,
                    CallbackUrl = model.CallbackUrl,
                    WebhookUrl = model.WebhookUrl,
                    FeePercentage = model.FeePercentage,
                    FixedFee = model.FixedFee,
                    IsActive = model.IsActive,
                    IsDefault = model.IsDefault,
                    Description = model.Description,
                    UpdatedByUserId = _currentUserService.UserId
                });

                if (!result.Success)
                {
                    await PopulateGatewayEditViewModel(model);
                    return HandleServiceError(result);
                }

                _logger.Information("درگاه پرداخت با موفقیت ویرایش شد. شناسه: {GatewayId}, کاربر: {UserName}",
                    model.Id, _currentUserService.UserName);

                return RedirectToAction("GatewayDetails", new { id = model.Id });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ویرایش درگاه پرداخت");
            }
        }

        /// <summary>
        /// حذف درگاه پرداخت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteGateway(int id)
        {
            try
            {
                _logger.Information("درخواست حذف درگاه پرداخت. شناسه: {GatewayId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                var result = await _paymentGatewayService.DeleteGatewayAsync(id, _currentUserService.UserId);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                _logger.Information("درگاه پرداخت با موفقیت حذف شد. شناسه: {GatewayId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "حذف درگاه پرداخت");
            }
        }

        #endregion

        #region Online Payment Actions

        /// <summary>
        /// نمایش جزئیات پرداخت آنلاین
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> OnlinePaymentDetails(int id)
        {
            try
            {
                _logger.Information("درخواست جزئیات پرداخت آنلاین. شناسه: {PaymentId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                var result = await _paymentGatewayService.GetOnlinePaymentByIdAsync(id);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                var viewModel = new OnlinePaymentDetailsViewModel
                {
                    Id = result.Data.Id,
                    ReceptionId = result.Data.ReceptionId ?? 0,
                    AppointmentId = result.Data.AppointmentId,
                    PatientId = result.Data.PatientId,
                    PaymentType = result.Data.PaymentType,
                    Amount = result.Data.Amount,
                    PaymentGatewayId = result.Data.PaymentGatewayId,
                    PaymentToken = result.Data.PaymentToken,
                    Status = result.Data.Status,
                    GatewayTransactionId = result.Data.GatewayTransactionId,
                    GatewayReferenceCode = result.Data.GatewayReferenceCode,
                    ErrorCode = result.Data.ErrorCode,
                    ErrorMessage = result.Data.ErrorMessage,
                    Description = result.Data.Description,
                    UserIpAddress = result.Data.UserIpAddress,
                    UserAgent = result.Data.UserAgent,
                    GatewayFee = result.Data.GatewayFee,
                    NetAmount = result.Data.NetAmount,
                    CreatedByUserId = result.Data.CreatedByUserId,
                    CreatedAt = result.Data.CreatedAt,
                    CompletedAt = result.Data.CompletedAt,
                    ReceptionNumber = result.Data.ReceptionNumber,
                    PatientName = result.Data.PatientName,
                    DoctorName = result.Data.DoctorName,
                    PaymentGatewayName = result.Data.PaymentGatewayName,
                    CreatedByUserName = result.Data.CreatedByUserName
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش جزئیات پرداخت آنلاین");
            }
        }

        /// <summary>
        /// ایجاد پرداخت آنلاین
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOnlinePayment(OnlinePaymentCreateViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد پرداخت آنلاین. شناسه پذیرش: {ReceptionId}, مبلغ: {Amount}, کاربر: {UserName}",
                    model.ReceptionId, model.Amount, _currentUserService.UserName);

                // اعتبارسنجی مدل
                var validation = await _onlinePaymentCreateValidator.ValidateAsync(model);
                if (!validation.IsValid)
                {
                    return HandleValidationErrors(validation.Errors.Select(e => e.ErrorMessage));
                }

                // ایجاد پرداخت آنلاین
                var result = await _paymentGatewayService.CreateOnlinePaymentAsync(new OnlinePayment
                {
                    ReceptionId = model.ReceptionId,
                    AppointmentId = model.AppointmentId,
                    PatientId = model.PatientId,
                    PaymentType = model.PaymentType,
                    Amount = model.Amount,
                    PaymentGatewayId = model.PaymentGatewayId,
                    Description = model.Description,
                    UserIpAddress = model.UserIpAddress,
                    UserAgent = model.UserAgent,
                    CreatedByUserId = _currentUserService.UserId
                });

                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                _logger.Information("پرداخت آنلاین با موفقیت ایجاد شد. شناسه: {PaymentId}, کاربر: {UserName}",
                    result.Data?.Id, _currentUserService.UserName);

                return RedirectToAction("OnlinePaymentDetails", new { id = result.Data?.Id });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ایجاد پرداخت آنلاین");
            }
        }

        #endregion

        #region AJAX Actions

        /// <summary>
        /// دریافت لیست درگاه‌های پرداخت (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetGateways(PaymentGatewaySearchViewModel searchModel, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                _logger.Information("درخواست AJAX لیست درگاه‌های پرداخت. صفحه: {PageNumber}, اندازه: {PageSize}, کاربر: {UserName}",
                    pageNumber, pageSize, _currentUserService.UserName);

                // اعتبارسنجی مدل جستجو
                var searchValidation = await _gatewaySearchValidator.ValidateAsync(searchModel);
                if (!searchValidation.IsValid)
                {
                    return StandardJsonResponse(false, "اطلاعات جستجو نامعتبر است", null, searchValidation.Errors.Select(e => e.ErrorMessage).ToList());
                }

                // دریافت لیست درگاه‌ها
                var result = await _paymentGatewayService.GetGatewaysAsync(
                    searchModel.Name,
                    searchModel.GatewayType,
                    searchModel.IsActive,
                    searchModel.IsDefault,
                    searchModel.CreatedByUserId,
                    searchModel.StartDate,
                    searchModel.EndDate,
                    pageNumber,
                    pageSize);

                if (!result.Success)
                {
                    return StandardJsonResponse(false, result.Message);
                }

                var gateways = result.Data?.Items?.Select(g => new PaymentGatewayListViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    GatewayType = g.GatewayType,
                    MerchantId = g.MerchantId,
                    FeePercentage = g.FeePercentage ?? 0,
                    FixedFee = g.FixedFee ?? 0,
                    IsActive = g.IsActive,
                    IsDefault = g.IsDefault,
                    CreatedAt = g.CreatedAt,
                    CreatedByUserName = g.CreatedByUserName,
                    TotalTransactions = g.TotalTransactions,
                    TotalAmount = g.TotalAmount,
                    SuccessRate = g.SuccessRate,
                    AverageResponseTime = g.AverageResponseTime
                }).ToList() ?? new List<PaymentGatewayListViewModel>();

                return StandardJsonResponse(true, "لیست درگاه‌های پرداخت با موفقیت دریافت شد", new
                {
                    gateways,
                    totalCount = result.Data?.TotalItems ?? 0,
                    pageNumber,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)(result.Data?.TotalItems ?? 0) / pageSize)
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "دریافت لیست درگاه‌های پرداخت");
            }
        }

        /// <summary>
        /// دریافت آمار درگاه‌های پرداخت (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetStatistics()
        {
            try
            {
                _logger.Information("درخواست AJAX آمار درگاه‌های پرداخت. کاربر: {UserName}",
                    _currentUserService.UserName);

                var result = await _paymentGatewayService.GetGatewayStatisticsAsync();
                if (!result.Success)
                {
                    return StandardJsonResponse(false, result.Message);
                }

                var statistics = new PaymentGatewayStatisticsViewModel
                {
                    TotalGateways = result.Data?.TotalGateways ?? 0,
                    ActiveGateways = result.Data?.ActiveGateways ?? 0,
                    InactiveGateways = result.Data?.InactiveGateways ?? 0,
                    DefaultGateways = result.Data?.DefaultGateways ?? 0,
                    TotalOnlinePayments = result.Data?.TotalOnlinePayments ?? 0,
                    SuccessfulPayments = result.Data?.SuccessfulPayments ?? 0,
                    FailedPayments = result.Data?.FailedPayments ?? 0,
                    PendingPayments = result.Data?.PendingPayments ?? 0,
                    TotalAmount = result.Data?.TotalAmount ?? 0,
                    SuccessfulAmount = result.Data?.SuccessfulAmount ?? 0,
                    FailedAmount = result.Data?.FailedAmount ?? 0,
                    PendingAmount = result.Data?.PendingAmount ?? 0,
                    SuccessRate = result.Data?.SuccessRate ?? 0,
                    AverageResponseTime = result.Data?.AverageResponseTime ?? 0,
                    GatewaysByType = result.Data?.GatewaysByType ?? new Dictionary<PaymentGatewayType, int>(),
                    PaymentsByType = result.Data?.PaymentsByType ?? new Dictionary<OnlinePaymentType, int>(),
                    PaymentsByStatus = result.Data?.PaymentsByStatus ?? new Dictionary<OnlinePaymentStatus, int>()
                };

                return StandardJsonResponse(true, "آمار درگاه‌های پرداخت با موفقیت دریافت شد", statistics);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "دریافت آمار درگاه‌های پرداخت");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// پر کردن ViewModel ایجاد درگاه با لیست‌های مورد نیاز
        /// </summary>
        private async Task PopulateGatewayCreateViewModel(PaymentGatewayCreateViewModel model)
        {
            // TODO: دریافت لیست‌های مورد نیاز از سرویس‌ها
            // model.GatewayTypes = await GetGatewayTypes();
        }

        /// <summary>
        /// پر کردن ViewModel ویرایش درگاه با لیست‌های مورد نیاز
        /// </summary>
        private async Task PopulateGatewayEditViewModel(PaymentGatewayEditViewModel model)
        {
            // TODO: دریافت لیست‌های مورد نیاز از سرویس‌ها
            // model.GatewayTypes = await GetGatewayTypes();
        }

        #endregion
    }
}
