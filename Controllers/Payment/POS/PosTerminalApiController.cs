using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Models.Enums;
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
        private readonly ILogger _logger;

        public PosTerminalApiController(IPosManagementService service, ILogger logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
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
                    t.PosTerminalId,
                    t.Title,
                    t.TerminalId,
                    t.MerchantId,
                    t.Provider,
                    t.Protocol,
                    t.IpAddress,
                    t.Port,
                    t.MacAddress,
                    t.IsActive,
                    t.IsDefault
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
                return Json(res, JsonRequestBehavior.AllowGet);
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
        [HttpPut, ValidateAntiForgeryToken, Route("terminals/{id:int}")]
        public async Task<ActionResult> Update(int id, UpdatePosTerminalRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(ServiceResult.Failed("درخواست نامعتبر است"));
                }
                
                request.Id = id;
                request.UpdatedByUserId = User?.Identity?.Name;
                
                var res = await _service.UpdatePosTerminalAsync(request);
                return Json(res);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "POS: update error");
                return Json(ServiceResult.Failed("خطا در به‌روزرسانی ترمینال"));
            }
        }

        // POST /api/v1/pos/terminals/{id}/default
        [HttpPost, ValidateAntiForgeryToken, Route("terminals/{id:int}/default")]
        public async Task<ActionResult> SetDefault(int id)
        {
            try
            {
                var res = await _service.SetDefaultPosTerminalAsync(id, User?.Identity?.Name);
                return Json(res);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "POS: set default error");
                return Json(ServiceResult.Failed("خطا در تنظیم پیش‌فرض"));
            }
        }

        // POST /api/v1/pos/terminals/{id}/active
        [HttpPost, ValidateAntiForgeryToken, Route("terminals/{id:int}/active")]
        public async Task<ActionResult> ToggleActive(int id, bool isActive)
        {
            try
            {
                var res = await _service.TogglePosTerminalStatusAsync(id, isActive, User?.Identity?.Name);
                return Json(res);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "POS: toggle active error");
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
        [HttpPost, ValidateAntiForgeryToken, Route("process-payment")]
        public async Task<ActionResult> ProcessPayment(ProcessPosPaymentRequest request)
        {
            try
            {
                if (request == null || request.ReceptionId <= 0 || request.AmountIRR <= 0)
                {
                    return Json(ServiceResult.Failed("درخواست نامعتبر است"));
                }

                // دریافت ترمینال پیش‌فرض
                var terminalResult = await _service.GetDefaultPosTerminalAsync();
                if (!terminalResult.Success || terminalResult.Data == null)
                {
                    return Json(ServiceResult.Failed("ترمینال POS پیش‌فرض یافت نشد. لطفاً ابتدا ترمینال را تنظیم کنید."));
                }

                var terminal = terminalResult.Data;

                // TODO: اینجا باید با دستگاه کارتخوان ارتباط برقرار شود
                // برای حال حاضر، یک شبیه‌سازی ساده انجام می‌دهیم
                // در آینده باید با SDK دستگاه کارتخوان (مثل سامان کیش، آسان پرداخت و...) ارتباط برقرار شود
                
                // شبیه‌سازی پردازش پرداخت
                // در واقعیت، اینجا باید:
                // 1. اتصال به دستگاه کارتخوان از طریق IP/MAC
                // 2. ارسال مبلغ به دستگاه
                // 3. دریافت پاسخ از دستگاه (RRN، TraceNo، TerminalId، CardLast4)
                // 4. بررسی موفقیت تراکنش

                // برای حال حاضر، یک پاسخ شبیه‌سازی شده برمی‌گردانیم
                var simulatedResponse = new
                {
                    success = true,
                    rrn = $"RRN{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}",
                    traceNo = $"{DateTime.Now:HHmmss}{new Random().Next(100, 999)}",
                    terminalId = terminal.TerminalId,
                    cardLast4 = $"****{new Random().Next(1000, 9999)}",
                    message = "پرداخت با موفقیت انجام شد"
                };

                return Json(ServiceResult<object>.Successful(simulatedResponse));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "POS: process payment error");
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


