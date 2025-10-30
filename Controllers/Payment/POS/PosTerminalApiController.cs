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
                request.Id = id;
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
    }
}


