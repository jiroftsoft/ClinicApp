using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Interfaces.Payment.Reporting;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Payment;
using FluentValidation;
using Serilog;
using PaymentTransactionCreateViewModel = ClinicApp.ViewModels.Payment.PaymentTransactionCreateViewModel;

namespace ClinicApp.Controllers.Payment
{
    /// <summary>
    /// کنترلر مدیریت تراکنش‌های پرداخت
    /// </summary>
    //[Authorize(Roles = "Receptionist,Admin,Accountant")]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentReportingService _paymentReportingService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<PaymentTransactionCreateViewModel> _createValidator;
        private readonly IValidator<PaymentTransactionEditViewModel> _editValidator;
        private readonly IValidator<PaymentTransactionSearchViewModel> _searchValidator;

        public PaymentController(
            IPaymentService paymentService,
            IPaymentReportingService paymentReportingService,
            ICurrentUserService currentUserService,
            IValidator<PaymentTransactionCreateViewModel> createValidator,
            IValidator<PaymentTransactionEditViewModel> editValidator,
            IValidator<PaymentTransactionSearchViewModel> searchValidator,
            ILogger logger) : base(logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _paymentReportingService = paymentReportingService ?? throw new ArgumentNullException(nameof(paymentReportingService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _editValidator = editValidator ?? throw new ArgumentNullException(nameof(editValidator));
            _searchValidator = searchValidator ?? throw new ArgumentNullException(nameof(searchValidator));
        }

        #region Index Actions

        /// <summary>
        /// صفحه اصلی مدیریت پرداخت‌ها
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(PaymentTransactionSearchViewModel searchModel, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                _logger.Information("درخواست صفحه اصلی پرداخت‌ها. صفحه: {PageNumber}, اندازه: {PageSize}, کاربر: {UserName}",
                    pageNumber, pageSize, _currentUserService.UserName);

                // اعتبارسنجی مدل جستجو
                var searchValidation = await _searchValidator.ValidateAsync(searchModel);
                if (!searchValidation.IsValid)
                {
                    return HandleValidationErrors(searchValidation.Errors.Select(e => e.ErrorMessage));
                }

                // دریافت لیست تراکنش‌ها
                var transactionsResult = await _paymentService.GetTransactionsAsync(
                    searchModel.PatientId,
                    searchModel.ReceptionId,
                    null, // appointmentId
                    searchModel.Method,
                    searchModel.Status,
                    searchModel.MinAmount,
                    searchModel.MaxAmount,
                    searchModel.StartDate,
                    searchModel.EndDate,
                    searchModel.PatientName,
                    searchModel.DoctorName,
                    searchModel.TransactionId,
                    searchModel.ReferenceCode,
                    pageNumber,
                    pageSize);

                if (!transactionsResult.Success)
                {
                    return HandleServiceError(transactionsResult);
                }

                // دریافت آمار
                var statisticsResult = await _paymentReportingService.GetPaymentStatisticsAsync(
                    searchModel.StartDate ?? DateTime.Today.AddDays(-30),
                    searchModel.EndDate ?? DateTime.Today,
                    searchModel.Method,
                    searchModel.Status);

                if (!statisticsResult.Success)
                {
                    return HandleServiceError(statisticsResult);
                }

                // ایجاد ViewModel
                var viewModel = new PaymentIndexViewModel
                {
                    Transactions = transactionsResult.Data?.Items?.Select(t => new PaymentTransactionListViewModel
                    {
                        Id = t.PaymentTransactionId,
                        ReceptionId = t.ReceptionId,
                        Amount = t.Amount,
                        Method = t.Method,
                        Status = t.Status,
                        TransactionId = t.TransactionId,
                        ReferenceCode = t.ReferenceCode,
                        ReceiptNo = t.ReceiptNo,
                        CreatedAt = t.CreatedAt,
                        CreatedByUserName = t.CreatedByUserName,
                        ReceptionNumber = t.ReceptionNumber,
                        PatientName = t.PatientName,
                        DoctorName = t.DoctorName
                    }).ToList() ?? new List<PaymentTransactionListViewModel>(),
                    SearchModel = searchModel,
                    Statistics = statisticsResult.Data,
                    PagedResult = new PagedResult<PaymentTransactionListViewModel>(
                        transactionsResult.Data?.Items?.Select(t => new PaymentTransactionListViewModel
                        {
                            TransactionId = t.TransactionId,
                            PatientName = t.PatientName,
                            Amount = t.Amount,
                            PaymentMethod = t.Method,
                            Status = t.Status,
                            TransactionDate = t.CreatedAt,
                            Description = t.Description,
                            DoctorName = t.DoctorName
                        }).ToList() ?? new List<PaymentTransactionListViewModel>(),
                        transactionsResult.Data?.TotalItems ?? 0,
                        transactionsResult.Data?.PageNumber ?? 1,
                        transactionsResult.Data?.PageSize ?? 20
                    )
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش صفحه اصلی پرداخت‌ها");
            }
        }

        #endregion

        #region CRUD Actions

        /// <summary>
        /// نمایش جزئیات تراکنش پرداخت
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                _logger.Information("درخواست جزئیات تراکنش پرداخت. شناسه: {TransactionId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                var result = await _paymentService.GetTransactionByIdAsync(id);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                // ✅ استفاده از Factory Method Pattern
                var viewModel = PaymentTransactionDetailsViewModel.FromEntity(result.Data);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش جزئیات تراکنش پرداخت");
            }
        }

        /// <summary>
        /// نمایش فرم ایجاد تراکنش پرداخت
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Create(int? receptionId = null)
        {
            try
            {
                _logger.Information("درخواست فرم ایجاد تراکنش پرداخت. شناسه پذیرش: {ReceptionId}, کاربر: {UserName}",
                    receptionId, _currentUserService.UserName);

                // ✅ استفاده از Factory Method Pattern
                var viewModel = new PaymentTransactionCreateViewModel
                {
                    ReceptionId = receptionId ?? 0
                };

                // دریافت لیست‌های مورد نیاز
                await PopulateCreateViewModel(viewModel);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش فرم ایجاد تراکنش پرداخت");
            }
        }

        /// <summary>
        /// ایجاد تراکنش پرداخت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PaymentTransactionCreateViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد تراکنش پرداخت. شناسه پذیرش: {ReceptionId}, مبلغ: {Amount}, روش: {Method}, کاربر: {UserName}",
                    model.ReceptionId, model.Amount, model.Method, _currentUserService.UserName);

                // اعتبارسنجی مدل
                var validation = await _createValidator.ValidateAsync(model);
                if (!validation.IsValid)
                {
                    await PopulateCreateViewModel(model);
                    return HandleValidationErrors(validation.Errors.Select(e => e.ErrorMessage));
                }

                // ایجاد تراکنش
                var result = await _paymentService.CreateTransactionAsync(new PaymentTransaction
                {
                    ReceptionId = model.ReceptionId,
                    Amount = model.Amount,
                    Method = model.Method,
                    Description = model.Description,
                    PosTerminalId = model.PosTerminalId ?? 0,
                    PaymentGatewayId = model.PaymentGatewayId ?? 0,
                    CashSessionId = model.CashSessionId ?? 0,
                    TransactionId = model.TransactionId,
                    ReferenceCode = model.ReferenceCode,
                    ReceiptNo = model.ReceiptNo,
                    CreatedByUserId = _currentUserService.UserId
                });

                if (!result.Success)
                {
                    await PopulateCreateViewModel(model);
                    return HandleServiceError(result);
                }

                _logger.Information("تراکنش پرداخت با موفقیت ایجاد شد. شناسه: {TransactionId}, کاربر: {UserName}",
                    result.Data?.PaymentTransactionId, _currentUserService.UserName);

                return RedirectToAction("Details", new { id = result.Data?.PaymentTransactionId });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ایجاد تراکنش پرداخت");
            }
        }

        /// <summary>
        /// نمایش فرم ویرایش تراکنش پرداخت
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                _logger.Information("درخواست فرم ویرایش تراکنش پرداخت. شناسه: {TransactionId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                var result = await _paymentService.GetTransactionByIdAsync(id);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                // ✅ استفاده از Factory Method Pattern
                var viewModel = PaymentTransactionEditViewModel.FromEntity(result.Data);

                // دریافت لیست‌های مورد نیاز
                await PopulateEditViewModel(viewModel);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش فرم ویرایش تراکنش پرداخت");
            }
        }

        /// <summary>
        /// ویرایش تراکنش پرداخت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PaymentTransactionEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ویرایش تراکنش پرداخت. شناسه: {TransactionId}, کاربر: {UserName}",
                    model.PaymentGatewayId, _currentUserService.UserName);

                // اعتبارسنجی مدل
                var validation = await _editValidator.ValidateAsync(model);
                if (!validation.IsValid)
                {
                    await PopulateEditViewModel(model);
                    return HandleValidationErrors(validation.Errors.Select(e => e.ErrorMessage));
                }

                // ویرایش تراکنش
                var result = await _paymentService.UpdateTransactionAsync(new PaymentTransaction
                {
                    ReceptionId = model.ReceptionId,
                    Amount = model.Amount,
                    Method = model.Method,
                    Status = model.Status,
                    Description = model.Description,
                    PosTerminalId = model.PosTerminalId ?? 0,
                    PaymentGatewayId = model.PaymentGatewayId ?? 0,
                    CashSessionId = model.CashSessionId ?? 0,
                    TransactionId = model.TransactionId,
                    ReferenceCode = model.ReferenceCode,
                    ReceiptNo = model.ReceiptNo,
                    UpdatedByUserId = _currentUserService.UserId
                });

                if (!result.Success)
                {
                    await PopulateEditViewModel(model);
                    return HandleServiceError(result);
                }

                _logger.Information("تراکنش پرداخت با موفقیت ویرایش شد. شناسه: {TransactionId}, کاربر: {UserName}",
                    model.PaymentTransactionId, _currentUserService.UserName);

                return RedirectToAction("Details", new { id = model.PaymentGatewayId });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ویرایش تراکنش پرداخت");
            }
        }

        /// <summary>
        /// حذف تراکنش پرداخت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.Information("درخواست حذف تراکنش پرداخت. شناسه: {TransactionId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                var result = await _paymentService.DeleteTransactionAsync(id, _currentUserService.UserId);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                _logger.Information("تراکنش پرداخت با موفقیت حذف شد. شناسه: {TransactionId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "حذف تراکنش پرداخت");
            }
        }

        #endregion

        #region AJAX Actions

        /// <summary>
        /// دریافت لیست تراکنش‌های پرداخت (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetTransactions(PaymentTransactionSearchViewModel searchModel, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                _logger.Information("درخواست AJAX لیست تراکنش‌های پرداخت. صفحه: {PageNumber}, اندازه: {PageSize}, کاربر: {UserName}",
                    pageNumber, pageSize, _currentUserService.UserName);

                // اعتبارسنجی مدل جستجو
                var searchValidation = await _searchValidator.ValidateAsync(searchModel);
                if (!searchValidation.IsValid)
                {
                    return StandardJsonResponse(false, "اطلاعات جستجو نامعتبر است", null, searchValidation.Errors.Select(e => e.ErrorMessage).ToList());
                }

                // دریافت لیست تراکنش‌ها
                var result = await _paymentService.GetTransactionsAsync(
                    searchModel.PatientId,
                    searchModel.ReceptionId,
                    null, // appointmentId
                    searchModel.Method,
                    searchModel.Status,
                    searchModel.MinAmount,
                    searchModel.MaxAmount,
                    searchModel.StartDate,
                    searchModel.EndDate,
                    searchModel.PatientName,
                    searchModel.DoctorName,
                    searchModel.TransactionId,
                    searchModel.ReferenceCode,
                    pageNumber,
                    pageSize);

                if (!result.Success)
                {
                    return StandardJsonResponse(false, result.Message);
                }

                // ✅ استفاده از Factory Method Pattern
                var transactions = PaymentTransactionListViewModel.FromEntities(result.Data?.Items) ?? new List<PaymentTransactionListViewModel>();

                return StandardJsonResponse(true, "لیست تراکنش‌ها با موفقیت دریافت شد", new
                {
                    transactions,
                    totalCount = result.Data?.TotalItems ?? 0,
                    pageNumber,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)(result.Data?.TotalItems ?? 0) / pageSize)
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "دریافت لیست تراکنش‌های پرداخت");
            }
        }

        /// <summary>
        /// دریافت آمار پرداخت‌ها (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetStatistics(DateTime? startDate = null, DateTime? endDate = null, 
            PaymentMethod? method = null, PaymentStatus? status = null)
        {
            try
            {
                _logger.Information("درخواست AJAX آمار پرداخت‌ها. تاریخ از: {StartDate}, تاریخ تا: {EndDate}, روش: {Method}, وضعیت: {Status}, کاربر: {UserName}",
                    startDate, endDate, method, status, _currentUserService.UserName);

                var result = await _paymentReportingService.GetPaymentStatisticsAsync(
                    startDate ?? DateTime.Today.AddDays(-30), 
                    endDate ?? DateTime.Today, 
                    method, 
                    status);
                if (!result.Success)
                {
                    return StandardJsonResponse(false, result.Message);
                }

                var statistics = new PaymentStatisticsViewModel
                {
                    TotalTransactions = result.Data?.TotalTransactions ?? 0,
                    TotalAmount = result.Data?.TotalAmount ?? 0,
                    AverageAmount = result.Data?.AverageAmount ?? 0,
                    SuccessfulTransactions = result.Data?.SuccessfulTransactions ?? 0,
                    FailedTransactions = result.Data?.FailedTransactions ?? 0,
                    PendingTransactions = result.Data?.PendingTransactions ?? 0,
                    SuccessRate = result.Data?.SuccessRate ?? 0,
                    CashAmount = result.Data?.CashAmount ?? 0,
                    PosAmount = result.Data?.PosAmount ?? 0,
                    OnlineAmount = result.Data?.OnlineAmount ?? 0,
                    DebtAmount = result.Data?.DebtAmount ?? 0,
                    TransactionsByMethod = result.Data?.TransactionsByMethod ?? new Dictionary<PaymentMethod, int>(),
                    TransactionsByStatus = result.Data?.TransactionsByStatus ?? new Dictionary<PaymentStatus, int>()
                };

                return StandardJsonResponse(true, "آمار پرداخت‌ها با موفقیت دریافت شد", statistics);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "دریافت آمار پرداخت‌ها");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// پر کردن ViewModel ایجاد با لیست‌های مورد نیاز
        /// </summary>
        private async Task PopulateCreateViewModel(PaymentTransactionCreateViewModel model)
        {
            // TODO: دریافت لیست‌های مورد نیاز از سرویس‌ها
            // model.PaymentMethods = await GetPaymentMethods();
            // model.PosTerminals = await GetPosTerminals();
            // model.PaymentGateways = await GetPaymentGateways();
            // model.CashSessions = await GetCashSessions();
        }

        /// <summary>
        /// پر کردن ViewModel ویرایش با لیست‌های مورد نیاز
        /// </summary>
        private async Task PopulateEditViewModel(PaymentTransactionEditViewModel model)
        {
            // TODO: دریافت لیست‌های مورد نیاز از سرویس‌ها
            // model.PaymentMethods = await GetPaymentMethods();
            // model.PaymentStatuses = await GetPaymentStatuses();
            // model.PosTerminals = await GetPosTerminals();
            // model.PaymentGateways = await GetPaymentGateways();
            // model.CashSessions = await GetCashSessions();
        }

        #endregion
    }
}
