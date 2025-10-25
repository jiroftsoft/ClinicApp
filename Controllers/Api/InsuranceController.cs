using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers.Base;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.DTOs.Insurance;
using Serilog;
using System.Collections.Generic;

namespace ClinicApp.Controllers.Api
{
    /// <summary>
    /// API Controller مدیریت بیمه در پذیرش
    /// 
    /// Responsibilities:
    /// 1. محاسبه بیمه بیمار
    /// 2. اعتبارسنجی بیمه
    /// 3. دریافت وضعیت بیمه
    /// 4. مدیریت بیمه‌های بیمار
    /// 
    /// Architecture:
    /// ✅ Single Responsibility: فقط Insurance
    /// ✅ No Cache: طبق سیاست
    /// ✅ Conditional Authorization
    /// </summary>
    public class InsuranceController : BaseController
    {
        #region Fields

        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly ICombinedInsuranceCalculationService _combinedInsuranceCalculationService;
        private readonly IPatientInsuranceValidationService _patientInsuranceValidationService;

        #endregion

        #region Constructor

        public InsuranceController(
            IPatientInsuranceService patientInsuranceService,
            ICombinedInsuranceCalculationService combinedInsuranceCalculationService,
            IPatientInsuranceValidationService patientInsuranceValidationService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(currentUserService, logger)
        {
            _patientInsuranceService = patientInsuranceService ?? 
                throw new ArgumentNullException(nameof(patientInsuranceService));
            _combinedInsuranceCalculationService = combinedInsuranceCalculationService ?? 
                throw new ArgumentNullException(nameof(combinedInsuranceCalculationService));
            _patientInsuranceValidationService = patientInsuranceValidationService ?? 
                throw new ArgumentNullException(nameof(patientInsuranceValidationService));
        }

        #endregion

        #region Insurance Calculation

        /// <summary>
        /// محاسبه بیمه بیمار برای پذیرش
        /// </summary>
        /// <param name="request">درخواست محاسبه</param>
        /// <returns>نتیجه محاسبه</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Calculate(int patientId, int serviceId, DateTime? calculationDate = null)
        {
            using (StartPerformanceMonitoring("CalculatePatientInsurance"))
            {
                try
                {
                    _logger.Information(
                        "💰 محاسبه بیمه بیمار. بیمار: {PatientId}, خدمت: {ServiceId}, کاربر: {UserName}",
                        patientId, serviceId, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (patientId <= 0 || serviceId <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه بیمار و خدمت الزامی است"
                        });
                    }

                    var result = await _combinedInsuranceCalculationService
                        .CalculatePatientInsuranceForReceptionAsync(patientId, new System.Collections.Generic.List<int> { serviceId }, calculationDate ?? DateTime.Now);

                    if (!result.Success)
                    {
                        _logger.Warning("⚠️ خطا در محاسبه بیمه. پیام: {Message}", result.Message);
                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        });
                    }

                    _logger.Information("✅ محاسبه بیمه با موفقیت انجام شد");
                    
                    return SuccessResponse(result.Data, "محاسبه بیمه با موفقیت انجام شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "CalculatePatientInsurance", new { patientId, serviceId, calculationDate });
                }
            }
        }

        #endregion

        #region Insurance Retrieval

        /// <summary>
        /// دریافت بیمه‌های بیمار برای پذیرش
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست بیمه‌های بیمار</returns>
        [HttpGet]
        public async Task<JsonResult> GetPatientInsurances(int patientId)
        {
            using (StartPerformanceMonitoring("GetPatientInsurances"))
            {
                try
                {
                    _logger.Information(
                        "📋 دریافت بیمه‌های بیمار. بیمار: {PatientId}, کاربر: {UserName}",
                        patientId, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (patientId <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه بیمار نامعتبر است"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    var result = await _patientInsuranceService
                        .GetPatientInsurancesAsync(patientId, "", 1, 10);

                    if (result == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "بیمه‌ای یافت نشد"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return SuccessResponse(result, "بیمه‌های بیمار با موفقیت دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetPatientInsurances", new { patientId });
                }
            }
        }

        /// <summary>
        /// دریافت وضعیت بیمه بیمار برای پذیرش
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>وضعیت بیمه</returns>
        [HttpGet]
        public async Task<JsonResult> GetStatus(int patientId)
        {
            using (StartPerformanceMonitoring("GetPatientInsuranceStatus"))
            {
                try
                {
                    _logger.Information(
                        "📊 دریافت وضعیت بیمه بیمار. بیمار: {PatientId}, کاربر: {UserName}",
                        patientId, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (patientId <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه بیمار نامعتبر است"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    var result = await _patientInsuranceService
                        .GetPatientInsurancesAsync(patientId, "", 1, 10);

                    if (result == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "وضعیت بیمه یافت نشد"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return SuccessResponse(result, "وضعیت بیمه با موفقیت دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetPatientInsuranceStatus", new { patientId });
                }
            }
        }

        #endregion

        #region Insurance Validation

        /// <summary>
        /// اعتبارسنجی بیمه بیمار برای پذیرش
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        [HttpGet]
        public async Task<JsonResult> Validate(int patientId)
        {
            using (StartPerformanceMonitoring("ValidatePatientInsurance"))
            {
                try
                {
                    _logger.Information(
                        "✔️ اعتبارسنجی بیمه بیمار. بیمار: {PatientId}, کاربر: {UserName}",
                        patientId, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (patientId <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه بیمار نامعتبر است"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    var result = await _patientInsuranceValidationService
                        .ValidatePatientInsuranceAsync(patientId);

                    if (result == null)
                    {
                        _logger.Warning("⚠️ اعتبارسنجی بیمه ناموفق");
                        return Json(new
                        {
                            success = false,
                            message = "اعتبارسنجی بیمه ناموفق"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    _logger.Information("✅ اعتبارسنجی بیمه موفق");
                    
                    return SuccessResponse(result, "اعتبارسنجی بیمه با موفقیت انجام شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "ValidatePatientInsurance", new { patientId });
                }
            }
        }

        /// <summary>
        /// اعتبارسنجی سریع بیمه بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>نتیجه اعتبارسنجی سریع</returns>
        [HttpGet]
        public async Task<JsonResult> QuickValidate(int patientId)
        {
            using (StartPerformanceMonitoring("QuickValidatePatientInsurance"))
            {
                try
                {
                    _logger.Information(
                        "⚡ اعتبارسنجی سریع بیمه بیمار. بیمار: {PatientId}, کاربر: {UserName}",
                        patientId, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (patientId <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه بیمار نامعتبر است"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    var result = await _patientInsuranceValidationService
                        .ValidatePatientInsuranceAsync(patientId);

                    if (result == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "اعتبارسنجی سریع ناموفق"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return SuccessResponse(result, "اعتبارسنجی سریع با موفقیت انجام شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "QuickValidatePatientInsurance", new { patientId });
                }
            }
        }

        #endregion
    }
}

