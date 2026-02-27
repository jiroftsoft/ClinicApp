using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Payment.POS;
using ClinicApp.ViewModels.Validators.Payment.POS;
using FluentValidation;
using Microsoft.AspNet.Identity;
using Serilog;

namespace ClinicApp.Controllers.Payment.POS
{
    /// <summary>
    /// کنترلر مدیریت ترمینال‌های POS و جلسات نقدی
    /// </summary>
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    public class PosManagementController : BaseController
    {
        private readonly IPosManagementService _posManagementService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICashSessionAuditService _cashSessionAuditService;
        private readonly IValidator<PosTerminalCreateViewModel> _terminalCreateValidator;
        private readonly IValidator<PosTerminalEditViewModel> _terminalEditValidator;
        private readonly IValidator<PosTerminalSearchViewModel> _terminalSearchValidator;
        private readonly IValidator<CashSessionStartViewModel> _sessionStartValidator;
        private readonly IValidator<CashSessionEndViewModel> _sessionEndValidator;
        private readonly IValidator<CashSessionSearchViewModel> _sessionSearchValidator;

        public PosManagementController(
            IPosManagementService posManagementService,
            ICurrentUserService currentUserService,
            ICashSessionAuditService cashSessionAuditService,
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
            _cashSessionAuditService = cashSessionAuditService ?? throw new ArgumentNullException(nameof(cashSessionAuditService));
            _terminalCreateValidator = terminalCreateValidator ?? throw new ArgumentNullException(nameof(terminalCreateValidator));
            _terminalEditValidator = terminalEditValidator ?? throw new ArgumentNullException(nameof(terminalEditValidator));
            _terminalSearchValidator = terminalSearchValidator ?? throw new ArgumentNullException(nameof(terminalSearchValidator));
            _sessionStartValidator = sessionStartValidator ?? throw new ArgumentNullException(nameof(sessionStartValidator));
            _sessionEndValidator = sessionEndValidator ?? throw new ArgumentNullException(nameof(sessionEndValidator));
            _sessionSearchValidator = sessionSearchValidator ?? throw new ArgumentNullException(nameof(sessionSearchValidator));
        }

        /// <summary>
        /// تشخیص یکسان UserId در تمام اکشن‌های جلسه صندوق تا لیست و «جلسه فعال» برای یک کاربر باشند.
        /// اول CurrentUserService، در صورت خالی بودن و احراز هویت، Identity.GetUserId().
        /// </summary>
        private string GetCurrentUserIdForCashSession()
        {
            var userId = _currentUserService?.UserId;
            if (string.IsNullOrWhiteSpace(userId) && User?.Identity?.IsAuthenticated == true)
                userId = User.Identity.GetUserId();
            return userId;
        }

        #region Index Actions

        /// <summary>
        /// صفحه اصلی مدیریت POS
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// لیست جلسات صندوق
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Sessions()
        {
            try
            {
                _logger.Information("درخواست لیست جلسات صندوق - کاربر: {UserName}", _currentUserService?.UserName ?? "Unknown");

                var userId = GetCurrentUserIdForCashSession();
                if (!string.IsNullOrWhiteSpace(userId))
                    _logger.Information("دریافت جلسات برای UserId: {UserId}", userId);
                if (string.IsNullOrWhiteSpace(userId))
                    _logger.Information("UserId در دسترس نیست - نمایش تمام جلسات. IsAuthenticated: {IsAuth}", _currentUserService?.IsAuthenticated ?? false);

                // دریافت جلسات کاربر فعلی (یا تمام جلسات اگر UserId null باشد)
                var result = await _posManagementService.GetUserCashSessionsAsync(userId, 1, 50);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message ?? "خطا در دریافت جلسات صندوق";
                    return View(new List<CashSession>());
                }

                // ✅ پیام اطلاع‌رسانی فقط وقتی نمایش «همه جلسات» است: تفاوت بین کاربر لاگین‌نشده و فیلتر ناموفق
                if (string.IsNullOrWhiteSpace(userId))
                {
                    TempData["InfoMessage"] = _currentUserService?.IsAuthenticated == true
                        ? "در حال نمایش تمام جلسات صندوق (فیلتر بر اساس کاربر اعمال نشد)"
                        : "در حال نمایش تمام جلسات صندوق (کاربر لاگین نشده است)";
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش لیست جلسات صندوق");
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
                    if (Request.IsAjaxRequest())
                    {
                        return HandleServiceError(result);
                    }
                    // برای درخواست‌های غیر-AJAX، به صفحه Index redirect می‌کنیم
                    TempData["ErrorMessage"] = result.Message ?? "ترمینال POS یافت نشد";
                    return RedirectToAction("Index");
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
                _logger.Error(ex, "خطا در نمایش جزئیات ترمینال POS. شناسه: {TerminalId}", id);
                if (Request.IsAjaxRequest())
                {
                    return HandleException(ex, "نمایش جزئیات ترمینال POS");
                }
                // برای درخواست‌های غیر-AJAX، به صفحه Index redirect می‌کنیم
                TempData["ErrorMessage"] = "خطا در نمایش جزئیات ترمینال POS. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
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
                    foreach (var error in validation.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    return View(model);
                }

                // ایجاد ترمینال
                var userId = _currentUserService.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("UserId از CurrentUserService null یا empty است. استفاده از fallback");
                    // استفاده از SystemUsers یا مقدار پیش‌فرض
                    userId = SystemUsers.SystemUserId ?? SystemUsers.AdminUserId ?? "00000000-0000-0000-0000-000000000000";
                    _logger.Information("استفاده از UserId fallback: {UserId}", userId);
                }

                var createRequest = new CreatePosTerminalRequest
                {
                    Name = model.Name,
                    SerialNumber = model.SerialNumber,
                    TerminalId = model.TerminalId,
                    MerchantId = model.MerchantId,
                    ProviderType = model.ProviderType,
                    Protocol = model.Protocol,
                    IpAddress = model.IpAddress,
                    Port = model.Port,
                    MacAddress = model.MacAddress,
                    ConnectionString = $"{model.IpAddress}:{model.Port}", // ساخت ConnectionString از IP و Port
                    Description = model.Description,
                    IsDefault = model.IsDefault,
                    CreatedByUserId = userId
                };

                var result = await _posManagementService.CreatePosTerminalAsync(createRequest);

                if (!result.Success)
                {
                    await PopulateTerminalCreateViewModel(model);
                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }

                _logger.Information("ترمینال POS با موفقیت ایجاد شد. شناسه: {TerminalId}, کاربر: {UserName}",
                    result.Data?.Id, _currentUserService.UserName);

                return RedirectToAction("TerminalDetails", new { id = result.Data?.PosTerminalId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد ترمینال POS");
                await PopulateTerminalCreateViewModel(model);
                ModelState.AddModelError("", "خطا در ایجاد ترمینال POS. لطفاً مجدداً تلاش کنید.");
                return View(model);
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
                    TerminalId = result.Data.TerminalId,
                    MerchantId = result.Data.MerchantId,
                    ProviderType = result.Data.ProviderType,
                    Protocol = result.Data.Protocol,
                    IpAddress = result.Data.IpAddress,
                    Port = result.Data.Port,
                    MacAddress = result.Data.MacAddress,
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
                    foreach (var error in validation.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    return View(model);
                }

                // ویرایش ترمینال
                var userId = _currentUserService.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("UserId از CurrentUserService null یا empty است. استفاده از fallback");
                    // استفاده از SystemUsers یا مقدار پیش‌فرض
                    userId = SystemUsers.SystemUserId ?? SystemUsers.AdminUserId ?? "00000000-0000-0000-0000-000000000000";
                    _logger.Information("استفاده از UserId fallback: {UserId}", userId);
                }

                var updateRequest = new UpdatePosTerminalRequest
                {
                    Id = model.Id,
                    Name = model.Name,
                    Title = model.Name,
                    SerialNumber = model.SerialNumber,
                    TerminalId = model.TerminalId,
                    MerchantId = model.MerchantId,
                    ProviderType = model.ProviderType,
                    Provider = model.ProviderType,
                    Protocol = model.Protocol,
                    IpAddress = model.IpAddress,
                    Port = model.Port, // می‌تواند null باشد
                    MacAddress = model.MacAddress,
                    ConnectionString = model.Port.HasValue ? $"{model.IpAddress}:{model.Port}" : null, // ساخت ConnectionString از IP و Port (فقط اگر Port داشته باشد)
                    Description = model.Description,
                    IsActive = model.IsActive,
                    IsDefault = model.IsDefault,
                    UpdatedByUserId = userId
                };

                var result = await _posManagementService.UpdatePosTerminalAsync(updateRequest);

                if (!result.Success)
                {
                    await PopulateTerminalEditViewModel(model);
                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }

                _logger.Information("ترمینال POS با موفقیت ویرایش شد. شناسه: {TerminalId}, کاربر: {UserName}",
                    model.Id, _currentUserService.UserName);

                return RedirectToAction("TerminalDetails", new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش ترمینال POS");
                await PopulateTerminalEditViewModel(model);
                ModelState.AddModelError("", "خطا در ویرایش ترمینال POS. لطفاً مجدداً تلاش کنید.");
                return View(model);
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

                var userId = _currentUserService.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("UserId از CurrentUserService null یا empty است. استفاده از fallback");
                    userId = SystemUsers.SystemUserId ?? SystemUsers.AdminUserId ?? "00000000-0000-0000-0000-000000000000";
                    _logger.Information("استفاده از UserId fallback: {UserId}", userId);
                }

                var result = await _posManagementService.DeleteTerminalAsync(id, userId);
                if (!result.Success)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return StandardJsonResponse(false, result.Message);
                    }
                    return HandleServiceError(result);
                }

                _logger.Information("ترمینال POS با موفقیت حذف شد. شناسه: {TerminalId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                if (Request.IsAjaxRequest())
                {
                    return StandardJsonResponse(true, "ترمینال با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف ترمینال POS");
                if (Request.IsAjaxRequest())
                {
                    return StandardJsonResponse(false, "خطا در حذف ترمینال POS");
                }
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

                // ✅ محاسبه تعداد تراکنش‌ها
                var totalTransactions = result.Data.TotalTransactions;
                var cashTransactions = result.Data.CashTransactions?.Count() ?? 0;
                var posTransactions = result.Data.PosTransactions?.Count() ?? 0;
                
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
                    TotalTransactions = totalTransactions,
                    CashTransactions = cashTransactions,
                    PosTransactions = posTransactions,
                    Duration = result.Data.Duration
                };

                // لاگ‌های Audit برای ردیابی (منشی/ادمین با نقش خود دسترسی دارند — همان صفحه SessionDetails)
                var auditResult = await _cashSessionAuditService.GetAuditLogsAsync(id);
                ViewBag.AuditLogs = auditResult.Success && auditResult.Data != null ? auditResult.Data : new List<ClinicApp.Models.Entities.Payment.CashSessionAuditLog>();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش جزئیات جلسه نقدی");
            }
        }

        /// <summary>
        /// نمایش فرم شروع جلسه نقدی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> StartSession()
        {
            try
            {
                var userId = GetCurrentUserIdForCashSession();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    TempData["Error"] = "شناسه کاربر در دسترس نیست. لطفاً مجدداً وارد شوید.";
                    return RedirectToAction("Sessions");
                }
                // بررسی اینکه آیا جلسه فعالی وجود دارد یا نه (همان UserId لیست جلسات)
                var activeSessionResult = await _posManagementService.GetActiveCashSessionAsync(userId);
                if (activeSessionResult.Success && activeSessionResult.Data != null)
                {
                    TempData["Warning"] = "شما در حال حاضر یک جلسه صندوق باز دارید. لطفاً ابتدا جلسه قبلی را ببندید.";
                    return RedirectToAction("SessionDetails", new { id = activeSessionResult.Data.CashSessionId });
                }

                var model = new CashSessionStartViewModel
                {
                    InitialCashAmount = 0,
                    Description = string.Empty
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "نمایش فرم شروع جلسه");
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
                    model?.InitialCashAmount, _currentUserService.UserName);

                if (model == null)
                {
                    TempData["Error"] = "اطلاعات ارسالی نامعتبر است.";
                    return RedirectToAction("StartSession");
                }

                var validation = await _sessionStartValidator.ValidateAsync(model);
                if (!validation.IsValid)
                {
                    foreach (var error in validation.Errors)
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    return View(model);
                }

                var userId = GetCurrentUserIdForCashSession();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("UserId از CurrentUserService و Identity هر دو null یا empty است. استفاده از fallback سیستمی");
                    userId = SystemUsers.SystemUserId ?? SystemUsers.AdminUserId ?? "00000000-0000-0000-0000-000000000000";
                    _logger.Information("استفاده از UserId fallback: {UserId}", userId);
                }

                var result = await _posManagementService.StartCashSessionAsync(
                    userId,
                    model.InitialCashAmount,
                    model.Description);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message ?? "خطا در شروع جلسه صندوق.";
                    return View(model);
                }

                _logger.Information("جلسه نقدی با موفقیت شروع شد. شناسه: {SessionId}, کاربر: {UserName}",
                    result.Data?.Id, _currentUserService.UserName);

                return RedirectToAction("SessionDetails", new { id = result.Data?.CashSessionId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شروع جلسه نقدی");
                TempData["Error"] = "خطا در شروع جلسه صندوق. لطفاً مجدداً تلاش کنید.";
                return View(model ?? new CashSessionStartViewModel());
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
                    var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
                    if (Request.IsAjaxRequest())
                        return HandleValidationErrors(errors);
                    NotificationHelper.SetError(TempData, "اطلاعات وارد شده نامعتبر است: " + string.Join(" ", errors));
                    return RedirectToAction("SessionDetails", new { id = sessionId });
                }

                // ✅ HIS Production: بررسی مالکیت جلسه — فقط صاحب جلسه یا ادمین می‌تواند ببندد
                var sessionResult = await _posManagementService.GetSessionByIdAsync(sessionId);
                if (!sessionResult.Success || sessionResult.Data == null)
                {
                    if (Request.IsAjaxRequest())
                        return HandleServiceError(sessionResult);
                    NotificationHelper.SetError(TempData, sessionResult.Message ?? "جلسه یافت نشد.");
                    return RedirectToAction("SessionDetails", new { id = sessionId });
                }
                var session = sessionResult.Data;
                // تشخیص کاربر با همان منطق StartSession/Sessions تا صاحب جلسه به‌درستی شناسایی شود (ضد باگ مالکیت)
                var userId = GetCurrentUserIdForCashSession();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("در EndSession شناسه کاربر از CurrentUser و Identity در دسترس نیست؛ فقط ادمین مجاز است.");
                    userId = SystemUsers.SystemUserId ?? SystemUsers.AdminUserId ?? "00000000-0000-0000-0000-000000000000";
                }
                var isAdmin = User?.IsInRole(AppRoles.Admin) ?? false;
                // فقط صاحب جلسه (همان کاربری که جلسه را شروع کرده) یا ادمین می‌تواند جلسه را ببندد — پروداکشن و ضد تقلب
                if (session.UserId != userId && !isAdmin)
                {
                    _logger.Warning("کاربر {UserId} (نام: {UserName}) تلاش برای بستن جلسه {SessionId} متعلق به {OwnerId}", userId, _currentUserService?.UserName, sessionId, session.UserId);
                    if (Request.IsAjaxRequest())
                        return HandleServiceError(ServiceResult.Failed("شما مجوز بستن این جلسه را ندارید. فقط صاحب جلسه یا مدیر سیستم می‌توانند جلسه را ببندند.", "FORBIDDEN"));
                    NotificationHelper.SetError(TempData, "شما مجوز بستن این جلسه را ندارید. فقط صاحب جلسه یا مدیر سیستم می‌توانند جلسه را ببندند.");
                    return RedirectToAction("SessionDetails", new { id = sessionId });
                }

                // پایان جلسه
                var result = await _posManagementService.EndCashSessionAsync(
                    sessionId,
                    model.FinalCashAmount,
                    model.Description ?? string.Empty,
                    userId);

                if (!result.Success)
                {
                    if (Request.IsAjaxRequest())
                        return HandleServiceError(result);
                    NotificationHelper.SetError(TempData, result.Message ?? "پایان جلسه انجام نشد.");
                    return RedirectToAction("SessionDetails", new { id = sessionId });
                }

                _logger.Information("جلسه نقدی با موفقیت پایان یافت. شناسه: {SessionId}, کاربر: {UserName}",
                    sessionId, _currentUserService.UserName);

                NotificationHelper.SetSuccess(TempData, "جلسه صندوق با موفقیت بسته شد.");
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
                    Name = t.Title,
                    Title = t.Title,
                    TerminalId = t.TerminalId,
                    MerchantId = t.MerchantId,
                    SerialNumber = t.SerialNumber,
                    IpAddress = t.IpAddress,
                    Port = t.Port,
                    MacAddress = t.MacAddress,
                    ProviderType = t.Provider,
                    Protocol = t.Protocol,
                    IsActive = t.IsActive,
                    IsDefault = t.IsDefault,
                    CreatedAt = t.CreatedAt,
                    CreatedByUserName = t.CreatedByUser?.UserName ?? "نامشخص",
                    TotalTransactions = 0, // TODO: Calculate from Transactions
                    TotalAmount = 0, // TODO: Calculate from Transactions
                    SuccessRate = 0 // TODO: Calculate from Transactions
                }).ToList() ?? new List<PosTerminalListViewModel>();

                return StandardJsonResponse(true, "لیست ترمینال‌های POS با موفقیت دریافت شد", new
                {
                    terminals,
                    totalCount = terminals.Count,
                    pageNumber,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)terminals.Count / pageSize)
                }, null);
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
