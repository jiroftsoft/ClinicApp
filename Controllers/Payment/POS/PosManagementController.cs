using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Payment.POS;
using ClinicApp.ViewModels.Validators.Payment.POS;
using FluentValidation;
using Serilog;

namespace ClinicApp.Controllers.Payment.POS
{
    /// <summary>
    /// کنترلر مدیریت ترمینال‌های POS و جلسات نقدی
    /// </summary>
    [Authorize(Roles = "Admin,Accountant")]
    public class PosManagementController : BaseController
    {
        private readonly IPosManagementService _posManagementService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<PosTerminalCreateViewModel> _terminalCreateValidator;
        private readonly IValidator<PosTerminalEditViewModel> _terminalEditValidator;
        private readonly IValidator<PosTerminalSearchViewModel> _terminalSearchValidator;
        private readonly IValidator<CashSessionStartViewModel> _sessionStartValidator;
        private readonly IValidator<CashSessionEndViewModel> _sessionEndValidator;
        private readonly IValidator<CashSessionSearchViewModel> _sessionSearchValidator;

        public PosManagementController(
            IPosManagementService posManagementService,
            ICurrentUserService currentUserService,
            IValidator<PosTerminalCreateViewModel> terminalCreateValidator,
            IValidator<PosTerminalEditViewModel> terminalEditValidator,
            IValidator<PosTerminalSearchViewModel> terminalSearchValidator,
            IValidator<CashSessionStartViewModel> sessionStartValidator,
            IValidator<CashSessionEndViewModel> sessionEndValidator,
            IValidator<CashSessionSearchViewModel> sessionSearchValidator,
            ILogger logger) : base(logger)
        {
            _posManagementService = posManagementService ?? throw new ArgumentNullException(nameof(posManagementService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _terminalCreateValidator = terminalCreateValidator ?? throw new ArgumentNullException(nameof(terminalCreateValidator));
            _terminalEditValidator = terminalEditValidator ?? throw new ArgumentNullException(nameof(terminalEditValidator));
            _terminalSearchValidator = terminalSearchValidator ?? throw new ArgumentNullException(nameof(terminalSearchValidator));
            _sessionStartValidator = sessionStartValidator ?? throw new ArgumentNullException(nameof(sessionStartValidator));
            _sessionEndValidator = sessionEndValidator ?? throw new ArgumentNullException(nameof(sessionEndValidator));
            _sessionSearchValidator = sessionSearchValidator ?? throw new ArgumentNullException(nameof(sessionSearchValidator));
        }

        #region Index Actions

        /// <summary>
        /// صفحه اصلی مدیریت POS
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(PosTerminalSearchViewModel terminalSearchModel, 
            CashSessionSearchViewModel sessionSearchModel, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                _logger.Information("درخواست صفحه اصلی مدیریت POS. صفحه: {PageNumber}, اندازه: {PageSize}, کاربر: {UserName}",
                    pageNumber, pageSize, _currentUserService.UserName);

                // دریافت لیست ترمینال‌ها
                var terminalsResult = await _posManagementService.GetTerminalsAsync(pageNumber, pageSize);

                if (!terminalsResult.Success)
                {
                    return HandleServiceError(terminalsResult);
                }

                // دریافت جلسات فعال
                var activeSessionsResult = await _posManagementService.GetActiveSessionsAsync();
                if (!activeSessionsResult.Success)
                {
                    return HandleServiceError(activeSessionsResult);
                }

                // دریافت آمار
                var statisticsResult = await _posManagementService.GetPosStatisticsViewModelAsync(DateTime.Today.AddDays(-30), DateTime.Today);
                if (!statisticsResult.Success)
                {
                    return HandleServiceError(statisticsResult);
                }

                // ایجاد ViewModel
                var viewModel = new PosIndexViewModel
                {
                    Terminals = terminalsResult.Data?.Select(t => new PosTerminalListViewModel
                    {
                        Id = t.PosTerminalId,
                        Name = t.Name,
                        SerialNumber = t.SerialNumber,
                        ProviderType = t.ProviderType,
                        Protocol = t.Protocol,
                        IsActive = t.IsActive,
                        IsDefault = t.IsDefault,
                        CreatedAt = t.CreatedAt,
                        CreatedByUserName = t.CreatedByUserName,
                        TotalTransactions = t.TotalTransactions,
                        TotalAmount = t.TotalAmount,
                        SuccessRate = t.SuccessRate
                    }).ToList() ?? new List<PosTerminalListViewModel>(),
                    ActiveSessions = activeSessionsResult.Data?.Select(s => new CashSessionListViewModel
                    {
                        Id = s.CashSessionId,
                        SessionNumber = s.SessionNumber,
                        UserName = s.UserName,
                        InitialCashAmount = s.InitialCashAmount,
                        FinalCashAmount = s.FinalCashAmount,
                        TotalIncome = s.TotalIncome,
                        TotalExpense = s.TotalExpense,
                        CurrentBalance = s.CurrentBalance,
                        Difference = s.Difference,
                        Status = s.Status,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        Duration = s.Duration,
                        TotalTransactions = s.TotalTransactions
                    }).ToList() ?? new List<CashSessionListViewModel>(),
                    TerminalSearchModel = terminalSearchModel,
                    SessionSearchModel = sessionSearchModel,
                    Statistics = new PosStatisticsViewModel
                    {
                        TotalTerminals = statisticsResult.Data?.TotalTerminals ?? 0,
                        ActiveTerminals = statisticsResult.Data?.ActiveTerminals ?? 0,
                        InactiveTerminals = statisticsResult.Data?.InactiveTerminals ?? 0,
                        DefaultTerminals = statisticsResult.Data?.DefaultTerminals ?? 0,
                        TotalSessions = statisticsResult.Data?.TotalSessions ?? 0,
                        ActiveSessions = statisticsResult.Data?.ActiveSessions ?? 0,
                        CompletedSessions = statisticsResult.Data?.CompletedSessions ?? 0,
                        TotalCashHandled = statisticsResult.Data?.TotalCashHandled ?? 0,
                        TotalPosAmount = statisticsResult.Data?.TotalPosAmount ?? 0,
                        TotalCashAmount = statisticsResult.Data?.TotalCashAmount ?? 0,
                        AverageSessionAmount = statisticsResult.Data?.AverageSessionAmount ?? 0,
                        AverageSessionDuration = statisticsResult.Data?.AverageSessionDuration ?? 0,
                        TerminalsByProvider = statisticsResult.Data?.TerminalsByProvider ?? new Dictionary<PosProviderType, int>(),
                        TerminalsByProtocol = statisticsResult.Data?.TerminalsByProtocol ?? new Dictionary<PosProtocol, int>(),
                        SessionsByStatus = statisticsResult.Data?.SessionsByStatus ?? new Dictionary<CashSessionStatus, int>()
                    },
                    TotalTerminalCount = terminalsResult.Data?.Count() ?? 0,
                    TotalSessionCount = activeSessionsResult.Data?.Count() ?? 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)(terminalsResult.Data?.Count() ?? 0) / pageSize)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش صفحه اصلی مدیریت POS");
            }
        }

        #endregion

        #region POS Terminal CRUD Actions

        /// <summary>
        /// نمایش جزئیات ترمینال POS
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> TerminalDetails(int id)
        {
            try
            {
                _logger.Information("درخواست جزئیات ترمینال POS. شناسه: {TerminalId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                var result = await _posManagementService.GetTerminalByIdAsync(id);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                var viewModel = new PosTerminalDetailsViewModel
                {
                    Id = result.Data.PosTerminalId,
                    Name = result.Data.Name,
                    SerialNumber = result.Data.SerialNumber,
                    ProviderType = result.Data.ProviderType,
                    Protocol = result.Data.Protocol,
                    ConnectionString = result.Data.ConnectionString,
                    Description = result.Data.Description,
                    IsActive = result.Data.IsActive,
                    IsDefault = result.Data.IsDefault,
                    CreatedByUserId = result.Data.CreatedByUserId,
                    CreatedAt = result.Data.CreatedAt,
                    UpdatedByUserId = result.Data.UpdatedByUserId,
                    UpdatedAt = result.Data.UpdatedAt,
                    CreatedByUserName = result.Data.CreatedByUserName,
                    UpdatedByUserName = result.Data.UpdatedByUserName,
                    TotalTransactions = 0, // PosTerminal entity doesn't have this property
                    TotalAmount = 0, // PosTerminal entity doesn't have this property
                    SuccessRate = 0, // PosTerminal entity doesn't have this property
                    LastTransactionDate = null // PosTerminal entity doesn't have this property
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش جزئیات ترمینال POS");
            }
        }

        /// <summary>
        /// نمایش فرم ایجاد ترمینال POS
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> CreateTerminal()
        {
            try
            {
                _logger.Information("درخواست فرم ایجاد ترمینال POS. کاربر: {UserName}",
                    _currentUserService.UserName);

                var viewModel = new PosTerminalCreateViewModel();

                // دریافت لیست‌های مورد نیاز
                await PopulateTerminalCreateViewModel(viewModel);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش فرم ایجاد ترمینال POS");
            }
        }

        /// <summary>
        /// ایجاد ترمینال POS
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateTerminal(PosTerminalCreateViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد ترمینال POS. نام: {Name}, سریال: {SerialNumber}, کاربر: {UserName}",
                    model.Name, model.SerialNumber, _currentUserService.UserName);

                // اعتبارسنجی مدل
                var validation = await _terminalCreateValidator.ValidateAsync(model);
                if (!validation.IsValid)
                {
                    await PopulateTerminalCreateViewModel(model);
                    return HandleValidationErrors(validation.Errors.Select(e => e.ErrorMessage));
                }

                // ایجاد ترمینال
                var result = await _posManagementService.CreateTerminalAsync(new PosTerminal
                {
                    Title = model.Name,
                    SerialNumber = model.SerialNumber,
                    Provider = model.ProviderType,
                    Protocol = model.Protocol,
                    IsDefault = model.IsDefault,
                    CreatedByUserId = _currentUserService.UserId
                });

                if (!result.Success)
                {
                    await PopulateTerminalCreateViewModel(model);
                    return HandleServiceError(result);
                }

                _logger.Information("ترمینال POS با موفقیت ایجاد شد. شناسه: {TerminalId}, کاربر: {UserName}",
                    result.Data?.Id, _currentUserService.UserName);

                return RedirectToAction("TerminalDetails", new { id = result.Data?.PosTerminalId });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ایجاد ترمینال POS");
            }
        }

        /// <summary>
        /// نمایش فرم ویرایش ترمینال POS
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> EditTerminal(int id)
        {
            try
            {
                _logger.Information("درخواست فرم ویرایش ترمینال POS. شناسه: {TerminalId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                var result = await _posManagementService.GetTerminalByIdAsync(id);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                var viewModel = new PosTerminalEditViewModel
                {
                    Id = result.Data.PosTerminalId,
                    Name = result.Data.Name,
                    SerialNumber = result.Data.SerialNumber,
                    ProviderType = result.Data.ProviderType,
                    Protocol = result.Data.Protocol,
                    ConnectionString = result.Data.ConnectionString,
                    Description = result.Data.Description,
                    IsActive = result.Data.IsActive,
                    IsDefault = result.Data.IsDefault
                };

                // دریافت لیست‌های مورد نیاز
                await PopulateTerminalEditViewModel(viewModel);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش فرم ویرایش ترمینال POS");
            }
        }

        /// <summary>
        /// ویرایش ترمینال POS
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditTerminal(PosTerminalEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ویرایش ترمینال POS. شناسه: {TerminalId}, کاربر: {UserName}",
                    model.Id, _currentUserService.UserName);

                // اعتبارسنجی مدل
                var validation = await _terminalEditValidator.ValidateAsync(model);
                if (!validation.IsValid)
                {
                    await PopulateTerminalEditViewModel(model);
                    return HandleValidationErrors(validation.Errors.Select(e => e.ErrorMessage));
                }

                // ویرایش ترمینال
                var result = await _posManagementService.UpdateTerminalAsync(new PosTerminal
                {
                    PosTerminalId = model.Id,
                    Title = model.Name,
                    SerialNumber = model.SerialNumber,
                    Provider = model.ProviderType,
                    Protocol = model.Protocol,
                    IsActive = model.IsActive,
                    IsDefault = model.IsDefault,
                    UpdatedByUserId = _currentUserService.UserId
                });

                if (!result.Success)
                {
                    await PopulateTerminalEditViewModel(model);
                    return HandleServiceError(result);
                }

                _logger.Information("ترمینال POS با موفقیت ویرایش شد. شناسه: {TerminalId}, کاربر: {UserName}",
                    model.Id, _currentUserService.UserName);

                return RedirectToAction("TerminalDetails", new { id = model.Id });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ویرایش ترمینال POS");
            }
        }

        /// <summary>
        /// حذف ترمینال POS
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteTerminal(int id)
        {
            try
            {
                _logger.Information("درخواست حذف ترمینال POS. شناسه: {TerminalId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                var result = await _posManagementService.DeleteTerminalAsync(id, _currentUserService.UserId);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                _logger.Information("ترمینال POS با موفقیت حذف شد. شناسه: {TerminalId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "حذف ترمینال POS");
            }
        }

        #endregion

        #region Cash Session Actions

        /// <summary>
        /// نمایش جزئیات جلسه نقدی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> SessionDetails(int id)
        {
            try
            {
                _logger.Information("درخواست جزئیات جلسه نقدی. شناسه: {SessionId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                var result = await _posManagementService.GetSessionByIdAsync(id);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                var viewModel = new CashSessionDetailsViewModel
                {
                    Id = result.Data.CashSessionId,
                    SessionNumber = result.Data.SessionNumber,
                    UserId = result.Data.UserId,
                    UserName = result.Data.UserName,
                    InitialCashAmount = result.Data.InitialCashAmount,
                    FinalCashAmount = result.Data.FinalCashAmount,
                    TotalIncome = result.Data.TotalIncome,
                    TotalExpense = result.Data.TotalExpense,
                    CurrentBalance = result.Data.CurrentBalance,
                    ExpectedBalance = result.Data.ExpectedBalance,
                    Difference = result.Data.Difference,
                    Status = result.Data.Status,
                    StartTime = result.Data.StartTime,
                    EndTime = result.Data.EndTime,
                    Description = result.Data.Description,
                    EndedByUserId = result.Data.EndedByUserId,
                    EndedByUserName = result.Data.EndedByUserName,
                    TotalTransactions = 0, // CashSession entity doesn't have this property
                    CashTransactions = 0, // CashSession entity doesn't have this property
                    PosTransactions = 0, // CashSession entity doesn't have this property
                    Duration = result.Data.Duration
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش جزئیات جلسه نقدی");
            }
        }

        /// <summary>
        /// شروع جلسه نقدی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StartSession(CashSessionStartViewModel model)
        {
            try
            {
                _logger.Information("درخواست شروع جلسه نقدی. مبلغ اولیه: {InitialAmount}, کاربر: {UserName}",
                    model.InitialCashAmount, _currentUserService.UserName);

                // اعتبارسنجی مدل
                var validation = await _sessionStartValidator.ValidateAsync(model);
                if (!validation.IsValid)
                {
                    return HandleValidationErrors(validation.Errors.Select(e => e.ErrorMessage));
                }

                // شروع جلسه
                var result = await _posManagementService.StartCashSessionAsync(
                    _currentUserService.UserId,
                    model.InitialCashAmount,
                    model.Description);

                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                _logger.Information("جلسه نقدی با موفقیت شروع شد. شناسه: {SessionId}, کاربر: {UserName}",
                    result.Data?.Id, _currentUserService.UserName);

                return RedirectToAction("SessionDetails", new { id = result.Data?.CashSessionId });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "شروع جلسه نقدی");
            }
        }

        /// <summary>
        /// پایان جلسه نقدی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EndSession(int sessionId, CashSessionEndViewModel model)
        {
            try
            {
                _logger.Information("درخواست پایان جلسه نقدی. شناسه: {SessionId}, مبلغ نهایی: {FinalAmount}, کاربر: {UserName}",
                    sessionId, model.FinalCashAmount, _currentUserService.UserName);

                // اعتبارسنجی مدل
                var validation = await _sessionEndValidator.ValidateAsync(model);
                if (!validation.IsValid)
                {
                    return HandleValidationErrors(validation.Errors.Select(e => e.ErrorMessage));
                }

                // پایان جلسه
                var result = await _posManagementService.EndCashSessionAsync(
                    sessionId,
                    model.FinalCashAmount,
                    _currentUserService.UserId,
                    model.Description);

                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                _logger.Information("جلسه نقدی با موفقیت پایان یافت. شناسه: {SessionId}, کاربر: {UserName}",
                    sessionId, _currentUserService.UserName);

                return RedirectToAction("SessionDetails", new { id = sessionId });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "پایان جلسه نقدی");
            }
        }

        #endregion

        #region AJAX Actions

        /// <summary>
        /// دریافت لیست ترمینال‌های POS (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetTerminals(PosTerminalSearchViewModel searchModel, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                _logger.Information("درخواست AJAX لیست ترمینال‌های POS. صفحه: {PageNumber}, اندازه: {PageSize}, کاربر: {UserName}",
                    pageNumber, pageSize, _currentUserService.UserName);

                // اعتبارسنجی مدل جستجو
                var searchValidation = await _terminalSearchValidator.ValidateAsync(searchModel);
                if (!searchValidation.IsValid)
                {
                    return StandardJsonResponse(false, "اطلاعات جستجو نامعتبر است", null, searchValidation.Errors.Select(e => e.ErrorMessage).ToList());
                }

                // دریافت لیست ترمینال‌ها
                var result = await _posManagementService.GetTerminalsAsync(pageNumber, pageSize);

                if (!result.Success)
                {
                    return StandardJsonResponse(false, result.Message);
                }

                var terminals = result.Data?.Select(t => new PosTerminalListViewModel
                {
                    Id = t.PosTerminalId,
                    Name = t.Name,
                    SerialNumber = t.SerialNumber,
                    ProviderType = t.ProviderType,
                    Protocol = t.Protocol,
                    IsActive = t.IsActive,
                    IsDefault = t.IsDefault,
                    CreatedAt = t.CreatedAt,
                    CreatedByUserName = t.CreatedByUserName,
                    TotalTransactions = t.TotalTransactions,
                    TotalAmount = t.TotalAmount,
                    SuccessRate = t.SuccessRate
                }).ToList() ?? new List<PosTerminalListViewModel>();

                return StandardJsonResponse(true, "لیست ترمینال‌های POS با موفقیت دریافت شد", new
                {
                    terminals,
                    totalCount = result.Data?.Count() ?? 0,
                    pageNumber,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)(result.Data?.Count() ?? 0) / pageSize)
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "دریافت لیست ترمینال‌های POS");
            }
        }

        /// <summary>
        /// دریافت آمار POS (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetStatistics()
        {
            try
            {
                _logger.Information("درخواست AJAX آمار POS. کاربر: {UserName}",
                    _currentUserService.UserName);

                var result = await _posManagementService.GetPosStatisticsViewModelAsync(DateTime.Today.AddDays(-30), DateTime.Today);
                if (!result.Success)
                {
                    return StandardJsonResponse(false, result.Message);
                }

                var statistics = new PosStatisticsViewModel
                {
                    TotalTerminals = result.Data?.TotalTerminals ?? 0,
                    ActiveTerminals = result.Data?.ActiveTerminals ?? 0,
                    InactiveTerminals = result.Data?.InactiveTerminals ?? 0,
                    DefaultTerminals = result.Data?.DefaultTerminals ?? 0,
                    TotalSessions = result.Data?.TotalSessions ?? 0,
                    ActiveSessions = result.Data?.ActiveSessions ?? 0,
                    CompletedSessions = result.Data?.CompletedSessions ?? 0,
                    TotalCashHandled = result.Data?.TotalCashHandled ?? 0,
                    TotalPosAmount = result.Data?.TotalPosAmount ?? 0,
                    TotalCashAmount = result.Data?.TotalCashAmount ?? 0,
                    AverageSessionAmount = result.Data?.AverageSessionAmount ?? 0,
                    AverageSessionDuration = result.Data?.AverageSessionDuration ?? 0,
                    TerminalsByProvider = result.Data?.TerminalsByProvider ?? new Dictionary<PosProviderType, int>(),
                    TerminalsByProtocol = result.Data?.TerminalsByProtocol ?? new Dictionary<PosProtocol, int>(),
                    SessionsByStatus = result.Data?.SessionsByStatus ?? new Dictionary<CashSessionStatus, int>()
                };

                return StandardJsonResponse(true, "آمار POS با موفقیت دریافت شد", statistics);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "دریافت آمار POS");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// پر کردن ViewModel ایجاد ترمینال با لیست‌های مورد نیاز
        /// </summary>
        private async Task PopulateTerminalCreateViewModel(PosTerminalCreateViewModel model)
        {
            // TODO: دریافت لیست‌های مورد نیاز از سرویس‌ها
            // model.ProviderTypes = await GetProviderTypes();
            // model.Protocols = await GetProtocols();
        }

        /// <summary>
        /// پر کردن ViewModel ویرایش ترمینال با لیست‌های مورد نیاز
        /// </summary>
        private async Task PopulateTerminalEditViewModel(PosTerminalEditViewModel model)
        {
            // TODO: دریافت لیست‌های مورد نیاز از سرویس‌ها
            // model.ProviderTypes = await GetProviderTypes();
            // model.Protocols = await GetProtocols();
        }

        #endregion
    }
}
