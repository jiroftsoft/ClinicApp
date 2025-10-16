using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers;
using ClinicApp.Services.Reception;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر بیمه خودکار در ماژول پذیرش
    /// </summary>
    [RoutePrefix("Reception/InsuranceAuto")]
    public class ReceptionInsuranceAutoController : BaseController
    {
        private readonly ReceptionInsuranceAutoService _insuranceAutoService;

        public ReceptionInsuranceAutoController(
            ReceptionInsuranceAutoService insuranceAutoService,
            ILogger logger) : base(logger)
        {
            _insuranceAutoService = insuranceAutoService ?? throw new ArgumentNullException(nameof(insuranceAutoService));
        }

        /// <summary>
        /// بایندینگ خودکار بیمه‌های موجود بیمار
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> AutoBindPatientInsurance(int patientId)
        {
            try
            {
                _logger.Information($"درخواست بایندینگ خودکار بیمه‌های بیمار: {patientId}");

                var result = await _insuranceAutoService.AutoBindPatientInsuranceAsync(patientId);

                if (result.Success)
                {
                    _logger.Information($"بایندینگ خودکار بیمه‌های بیمار {patientId} با موفقیت انجام شد");
                    return Json(new { 
                        success = true, 
                        data = result.Data, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning($"بایندینگ خودکار بیمه‌های بیمار {patientId} با خطا مواجه شد: {result.Message}");
                    return Json(new { 
                        success = false, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در بایندینگ خودکار بیمه‌های بیمار {patientId}");
                
                return Json(new { 
                    success = false, 
                    message = "خطا در بایندینگ بیمه‌های بیمار" 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست بیمه‌گذاران برای Select2
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetInsuranceProviders()
        {
            try
            {
                _logger.Information("درخواست دریافت لیست بیمه‌گذاران");

                var result = await _insuranceAutoService.GetInsuranceProvidersAsync();

                if (result.Success)
                {
                    _logger.Information($"لیست بیمه‌گذاران با موفقیت دریافت شد - تعداد: {result.Data.Count}");
                    return Json(new { 
                        success = true, 
                        data = result.Data, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning($"دریافت لیست بیمه‌گذاران با خطا مواجه شد: {result.Message}");
                    return Json(new { 
                        success = false, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست بیمه‌گذاران");
                
                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت لیست بیمه‌گذاران" 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست طرح‌های بیمه بر اساس بیمه‌گذار
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetInsurancePlans(int providerId)
        {
            try
            {
                _logger.Information($"درخواست دریافت لیست طرح‌های بیمه برای بیمه‌گذار: {providerId}");

                var result = await _insuranceAutoService.GetInsurancePlansAsync(providerId);

                if (result.Success)
                {
                    _logger.Information($"لیست طرح‌های بیمه با موفقیت دریافت شد - تعداد: {result.Data.Count}");
                    return Json(new { 
                        success = true, 
                        data = result.Data, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning($"دریافت لیست طرح‌های بیمه با خطا مواجه شد: {result.Message}");
                    return Json(new { 
                        success = false, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت لیست طرح‌های بیمه برای بیمه‌گذار {providerId}");
                
                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت لیست طرح‌های بیمه" 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// محاسبه Real-time سهم بیمه و بیمار
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateInsuranceShare(InsuranceCalculationRequest request)
        {
            try
            {
                _logger.Information($"درخواست محاسبه سهم بیمه برای بیمار {request.PatientId}");

                var result = await _insuranceAutoService.CalculateInsuranceShareAsync(
                    request.PatientId, 
                    request.PrimaryPlanId, 
                    request.SupplementaryPlanId, 
                    request.ServiceAmount
                );

                if (result.Success)
                {
                    _logger.Information($"محاسبه سهم بیمه با موفقیت انجام شد - سهم بیمه: {result.Data.TotalInsuranceShare}, سهم بیمار: {result.Data.PatientShare}");
                    return Json(new { 
                        success = true, 
                        data = result.Data, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning($"محاسبه سهم بیمه با خطا مواجه شد: {result.Message}");
                    return Json(new { 
                        success = false, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در محاسبه سهم بیمه برای بیمار {request.PatientId}");
                
                return Json(new { 
                    success = false, 
                    message = "خطا در محاسبه سهم بیمه" 
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }

    /// <summary>
    /// درخواست محاسبه سهم بیمه
    /// </summary>
    public class InsuranceCalculationRequest
    {
        public int PatientId { get; set; }
        public int PrimaryPlanId { get; set; }
        public int? SupplementaryPlanId { get; set; }
        public decimal ServiceAmount { get; set; }
    }
}
