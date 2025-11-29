using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Payment.POS;
using ClinicApp.ViewModels.Payment.POS;
using Serilog;

namespace ClinicApp.Controllers.Payment.POS
{
   
    [RoutePrefix("api/v1/pos")] 
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [NoCache]
    public class PosTerminalApiController : Controller
    {
        private readonly IPosManagementService _service;
        private readonly IPosDeviceService _posDeviceService;
        private readonly PosPaymentOrchestrator _paymentOrchestrator;
        private readonly ILogger _logger;

        public PosTerminalApiController(
            IPosManagementService service, 
            IPosDeviceService posDeviceService,
            PosPaymentOrchestrator paymentOrchestrator,
            ILogger logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _posDeviceService = posDeviceService ?? throw new ArgumentNullException(nameof(posDeviceService));
            _paymentOrchestrator = paymentOrchestrator ?? throw new ArgumentNullException(nameof(paymentOrchestrator));
            _logger = logger.ForContext<PosTerminalApiController>();
        }

        // GET /api/v1/pos/sessions/active
        [HttpGet, Route("sessions/active")]
        public async Task<ActionResult> ActiveSessions()
        {
            try
            {
                var res = await _service.GetActiveSessionsAsync();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "POS: active sessions error");
                return Json(ServiceResult.Failed("خطا در دریافت جلسات فعال"), JsonRequestBehavior.AllowGet);
            }
        }

        // GET /api/v1/pos/terminals?provider=&protocol=&active=
        [HttpGet, Route("terminals")]
        public async Task<ActionResult> List(PosProviderType? provider = null, PosProtocol? protocol = null, bool? active = null, int page = 1, int pageSize = 50)
        {
            try
            {
                var all = await _service.GetActivePosTerminalsAsync(); // minimal: reuse existing; filter client-side for now
                var data = all.Data ?? Enumerable.Empty<Models.Entities.Payment.PosTerminal>();

                if (provider.HasValue) data = data.Where(t => t.Provider == provider.Value);
                if (protocol.HasValue) data = data.Where(t => t.Protocol == protocol.Value);
                if (active.HasValue) data = data.Where(t => t.IsActive == active.Value);

                var pageData = data.Skip((page - 1) * pageSize).Take(pageSize).Select(t => new
                {
                    Id = t.PosTerminalId,
                    PosTerminalId = t.PosTerminalId,
                    Title = t.Title,
                    Name = t.Title,
                    TerminalId = t.TerminalId,
                    MerchantId = t.MerchantId,
                    SerialNumber = t.SerialNumber,
                    Provider = t.Provider,
                    ProviderType = t.Provider,
                    Protocol = t.Protocol, // enum به صورت عدد برمی‌گردد
                    IpAddress = t.IpAddress,
                    Port = t.Port,
                    MacAddress = t.MacAddress,
                    IsActive = t.IsActive,
                    IsDefault = t.IsDefault
                });
                return Json(ServiceResult<object>.Successful(new { items = pageData }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "POS: list error");
                return Json(ServiceResult.Failed("خطا در دریافت ترمینال‌ها"), JsonRequestBehavior.AllowGet);
            }
        }

        // GET /api/v1/pos/terminals/{id}
        [HttpGet, Route("terminals/{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            try
            {
                var res = await _service.GetPosTerminalAsync(id);
                if (!res.Success || res.Data == null)
                {
                    return Json(ServiceResult.Failed("ترمینال POS یافت نشد"), JsonRequestBehavior.AllowGet);
                }
                
                var terminal = res.Data;
                
                // ساخت DTO ساده بدون navigation properties برای جلوگیری از circular reference
                var dto = new
                {
                    Id = terminal.PosTerminalId,
                    PosTerminalId = terminal.PosTerminalId,
                    Title = terminal.Title,
                    Name = terminal.Title,
                    TerminalId = terminal.TerminalId,
                    MerchantId = terminal.MerchantId,
                    SerialNumber = terminal.SerialNumber,
                    Provider = terminal.Provider,
                    ProviderType = terminal.Provider,
                    Protocol = terminal.Protocol,
                    IpAddress = terminal.IpAddress,
                    Port = terminal.Port,
                    MacAddress = terminal.MacAddress,
                    IsActive = terminal.IsActive,
                    IsDefault = terminal.IsDefault,
                    CreatedByUserId = terminal.CreatedByUserId,
                    UpdatedByUserId = terminal.UpdatedByUserId,
                    CreatedAt = terminal.CreatedAt,
                    UpdatedAt = terminal.UpdatedAt
                };
                
                return Json(ServiceResult<object>.Successful(dto), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "POS: get error");
                return Json(ServiceResult.Failed("خطا در دریافت ترمینال"), JsonRequestBehavior.AllowGet);
            }
        }

        // POST /api/v1/pos/terminals
        [HttpPost, ValidateAntiForgeryToken, Route("terminals")]
        public async Task<ActionResult> Create(CreatePosTerminalRequest request)
        {
            try
            {
                var res = await _service.CreatePosTerminalAsync(request);
                return Json(res);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "POS: create error");
                return Json(ServiceResult.Failed("خطا در ایجاد ترمینال"));
            }
        }

        // PUT /api/v1/pos/terminals/{id}
        [HttpPut, ValidateAntiForgeryTokenOnPosts, Route("terminals/{id:int}")]
        public async Task<ActionResult> Update(int id, UpdatePosTerminalRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(ServiceResult.Failed("درخواست نامعتبر است"));
                }
                
                request.Id = id;
                
                // دریافت userId با fallback
                var userId = User?.Identity?.Name;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("UserId از User.Identity.Name null یا empty است. استفاده از fallback");
                    userId = SystemUsers.SystemUserId ?? SystemUsers.AdminUserId ?? "00000000-0000-0000-0000-000000000000";
                    _logger.Information("استفاده از UserId fallback: {UserId}", userId);
                }
                request.UpdatedByUserId = userId;
                
                _logger.Information("درخواست به‌روزرسانی ترمینال POS. شناسه: {TerminalId}, Protocol: {Protocol}, IP: {IpAddress}, Port: {Port}, کاربر: {UserId}", 
                    id, request.Protocol, request.IpAddress, request.Port, userId);
                
                var res = await _service.UpdatePosTerminalAsync(request);
                return Json(res);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "POS: update error. TerminalId: {TerminalId}", id);
                return Json(ServiceResult.Failed("خطا در به‌روزرسانی ترمینال"));
            }
        }

        // POST /api/v1/pos/terminals/{id}/default
        [HttpPost, ValidateAntiForgeryTokenOnPosts, Route("terminals/{id:int}/default")]
        public async Task<ActionResult> SetDefault(int id)
        {
            try
            {
                // دریافت userId با fallback
                var userId = User?.Identity?.Name;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("UserId از User.Identity.Name null یا empty است. استفاده از fallback");
                    userId = SystemUsers.SystemUserId ?? SystemUsers.AdminUserId ?? "00000000-0000-0000-0000-000000000000";
                    _logger.Information("استفاده از UserId fallback: {UserId}", userId);
                }
                
                _logger.Information("درخواست تنظیم ترمینال پیش‌فرض POS. شناسه: {TerminalId}, کاربر: {UserId}", id, userId);
                
                var res = await _service.SetDefaultPosTerminalAsync(id, userId);
                return Json(res);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "POS: set default error. TerminalId: {TerminalId}", id);
                return Json(ServiceResult.Failed("خطا در تنظیم پیش‌فرض"));
            }
        }

        // POST /api/v1/pos/terminals/{id}/active
        [HttpPost, ValidateAntiForgeryTokenOnPosts, Route("terminals/{id:int}/active")]
        public async Task<ActionResult> ToggleActive(int id)
        {
            try
            {
                // دریافت isActive از query string یا form data
                var isActiveParam = Request.QueryString["isActive"];
                bool isActive = true; // default value
                
                if (!string.IsNullOrEmpty(isActiveParam))
                {
                    if (!bool.TryParse(isActiveParam, out isActive))
                    {
                        // اگر parse نشد، از form data یا JSON body استفاده کن
                        var formValue = Request.Form["isActive"];
                        if (!string.IsNullOrEmpty(formValue))
                        {
                            bool.TryParse(formValue, out isActive);
                        }
                    }
                }
                else
                {
                    // اگر در query string نبود، از form data استفاده کن
                    var formValue = Request.Form["isActive"];
                    if (!string.IsNullOrEmpty(formValue))
                    {
                        bool.TryParse(formValue, out isActive);
                    }
                }

                // دریافت userId با fallback
                var userId = User?.Identity?.Name;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("UserId از User.Identity.Name null یا empty است. استفاده از fallback");
                    userId = SystemUsers.SystemUserId ?? SystemUsers.AdminUserId ?? "00000000-0000-0000-0000-000000000000";
                    _logger.Information("استفاده از UserId fallback: {UserId}", userId);
                }

                _logger.Information("درخواست تغییر وضعیت ترمینال POS. شناسه: {TerminalId}, وضعیت: {IsActive}, کاربر: {UserId}", 
                    id, isActive, userId);

                var res = await _service.TogglePosTerminalStatusAsync(id, isActive, userId);
                return Json(res);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "POS: toggle active error. TerminalId: {TerminalId}", id);
                return Json(ServiceResult.Failed("خطا در تغییر وضعیت ترمینال"));
            }
        }

        // GET /api/v1/pos/terminals/default
        [HttpGet, Route("terminals/default")]
        public async Task<ActionResult> GetDefault()
        {
            try
            {
                var res = await _service.GetDefaultPosTerminalAsync();
                if (res.Success && res.Data != null)
                {
                    return Json(ServiceResult<object>.Successful(new
                    {
                        posTerminalId = res.Data.PosTerminalId,
                        title = res.Data.Title,
                        terminalId = res.Data.TerminalId,
                        merchantId = res.Data.MerchantId,
                        provider = res.Data.Provider.ToString(),
                        protocol = res.Data.Protocol.ToString(),
                        ipAddress = res.Data.IpAddress,
                        port = res.Data.Port,
                        macAddress = res.Data.MacAddress,
                        isActive = res.Data.IsActive,
                        isDefault = res.Data.IsDefault
                    }), JsonRequestBehavior.AllowGet);
                }
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "POS: get default terminal error");
                return Json(ServiceResult.Failed("خطا در دریافت ترمینال پیش‌فرض"), JsonRequestBehavior.AllowGet);
            }
        }

        // POST /api/v1/pos/process-payment
        [HttpPost, ValidateAntiForgeryTokenOnPosts, Route("process-payment")]
        public async Task<ActionResult> ProcessPayment(ProcessPosPaymentRequest request)
        {
            try
            {
                if (request == null || request.ReceptionId <= 0 || request.AmountIRR <= 0)
                {
                    return Json(ServiceResult.Failed("درخواست نامعتبر است"));
                }

                _logger.Information("🏥 POS Controller: Processing payment - ReceptionId: {ReceptionId}, AmountIRR: {AmountIRR}",
                    request.ReceptionId, request.AmountIRR);

                // دریافت ترمینال
                var terminalResult = await _service.GetDefaultPosTerminalAsync();
                if (!terminalResult.Success || terminalResult.Data == null)
                {
                    _logger.Warning("⚠️ POS Controller: Default terminal not found");
                    return Json(ServiceResult.Failed("ترمینال POS پیش‌فرض یافت نشد. لطفاً ابتدا ترمینال را تنظیم کنید."));
                }

                // ✅ استفاده از PosPaymentOrchestrator (Production-Ready با Retry و لاگ کامل)
                var paymentResult = await _paymentOrchestrator.ProcessPaymentAsync(
                    receptionId: request.ReceptionId,
                    amountIRR: request.AmountIRR,
                    terminalId: request.PosTerminalId,
                    userId: null);

                if (!paymentResult.Success)
                {
                    _logger.Error("❌ POS Controller: Payment processing failed - OperationId: {OperationId}, Error: {Error}, ErrorCode: {ErrorCode}",
                        paymentResult.OperationId, paymentResult.Message, paymentResult.ErrorCode);
                    
                    return Json(ServiceResult<object>.Failed(paymentResult.Message, paymentResult.ErrorCode ?? "PAYMENT_FAILED"));
                }

                // ✅ ساخت Response با فرمت سازگار با Frontend
                var response = new
                {
                    success = true,
                    operationId = paymentResult.OperationId,
                    rrn = paymentResult.RRN,
                    traceNo = paymentResult.TraceNo,
                    terminalId = paymentResult.TerminalId,
                    cardLast4 = paymentResult.CardLast4,
                    message = paymentResult.Message,
                    durationMs = paymentResult.DurationMs,
                    retryCount = paymentResult.RetryCount
                };

                _logger.Information("✅ POS Controller: Payment processed successfully - OperationId: {OperationId}, RRN: {RRN}, TraceNo: {TraceNo}, Duration: {Duration}ms",
                    paymentResult.OperationId, paymentResult.RRN, paymentResult.TraceNo, paymentResult.DurationMs);

                return Json(ServiceResult<object>.Successful(response));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ POS Controller: Unexpected error in payment processing");
                return Json(ServiceResult.Failed("خطا در پردازش پرداخت POS"));
            }
        }
    }

    /// <summary>
    /// درخواست پردازش پرداخت POS
    /// </summary>
    public class ProcessPosPaymentRequest
    {
        public int ReceptionId { get; set; }
        public decimal AmountIRR { get; set; }
        public int? PosTerminalId { get; set; }
    }
}


