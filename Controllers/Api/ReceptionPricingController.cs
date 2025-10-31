using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Services.Pricing.Interfaces;
using ClinicApp.Services.Pricing.Models;
using ClinicApp.Helpers;
using ClinicApp.Filters;
using Serilog;

namespace ClinicApp.Controllers.Api
{
    /// <summary>
    /// Controller برای API محاسبه قیمت‌گذاری پذیرش
    /// </summary>
    [RoutePrefix("api/v1/reception")]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class ReceptionPricingController : Controller
    {
        private readonly IPricingEngine _engine;
        private readonly ILogger _log;

        public ReceptionPricingController(IPricingEngine engine, ILogger log)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// POST /api/v1/reception/pricing/quote
        /// پیش‌محاسبه قیمت خدمت با شکستن سهم‌ها
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryTokenOnPosts]
        [Route("pricing/quote")]
        public async Task<ActionResult> Quote(QuoteRequestDto dto, CancellationToken ct = default)
        {
            try
            {
                _log?.Information("💰 PRICING API: درخواست پیش‌محاسبه - ServiceId: {ServiceId}, ClinicId: {ClinicId}, DeptId: {DeptId}",
                    dto?.ServiceId, dto?.ClinicId, dto?.DepartmentId);

                if (dto == null)
                {
                    return Json(ServiceResult.Failed("درخواست نامعتبر است.", "VALIDATION"));
                }

                if (dto.ServiceId <= 0 || dto.ClinicId <= 0 || dto.DepartmentId <= 0)
                {
                    return Json(ServiceResult.Failed("ServiceId, ClinicId و DepartmentId الزامی هستند.", "VALIDATION"));
                }

                var res = await _engine.QuoteAsync(dto, ct);
                
                _log?.Information("✅ PRICING API: پیش‌محاسبه تکمیل شد - ServiceId: {ServiceId}, Approved: {Approved}, Primary: {Primary}, Supp: {Supp}, Patient: {Patient}",
                    dto.ServiceId, res.ApprovedTariff, res.Primary.Pays, res.Supplementary.Pays, res.PatientFinal);

                return Json(ServiceResult<QuoteResultDto>.Successful(res, "محاسبه با موفقیت انجام شد."));
            }
            catch (Exception ex)
            {
                _log?.Error(ex, "❌ PRICING API: خطا در پیش‌محاسبه - ServiceId: {ServiceId}", dto?.ServiceId);
                return Json(ServiceResult<QuoteResultDto>.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"));
            }
        }

        /// <summary>
        /// POST /api/v1/reception/pricing/reprice-all
        /// محاسبه مجدد همه آیتم‌های یک پذیرش (Reprice-on-change)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryTokenOnPosts]
        [Route("pricing/reprice-all")]
        public async Task<ActionResult> RepriceAll(RepriceRequestDto dto, CancellationToken ct = default)
        {
            try
            {
                _log?.Information("💰 PRICING API: درخواست محاسبه مجدد - ReceptionId: {ReceptionId}", dto?.ReceptionId);

                if (dto == null || dto.ReceptionId <= 0)
                {
                    return Json(ServiceResult.Failed("شناسه پذیرش نامعتبر است.", "VALIDATION"));
                }

                await _engine.RepriceReceptionAsync(dto.ReceptionId, ct);
                
                _log?.Information("✅ PRICING API: محاسبه مجدد تکمیل شد - ReceptionId: {ReceptionId}", dto.ReceptionId);

                return Json(ServiceResult.Successful("محاسبه مجدد با موفقیت انجام شد."));
            }
            catch (Exception ex)
            {
                _log?.Error(ex, "❌ PRICING API: خطا در محاسبه مجدد - ReceptionId: {ReceptionId}", dto?.ReceptionId);
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"));
            }
        }
    }

    /// <summary>
    /// DTO برای درخواست محاسبه مجدد
    /// </summary>
    public class RepriceRequestDto
    {
        public int ReceptionId { get; set; }
    }
}
