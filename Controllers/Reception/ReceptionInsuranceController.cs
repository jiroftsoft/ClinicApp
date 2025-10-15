using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر تخصصی مدیریت بیمه در پذیرش - رعایت اصل SRP
    /// مسئولیت: فقط مدیریت بیمه (اعتبارسنجی، محاسبه، استعلام)
    /// </summary>
    [RoutePrefix("Reception/Insurance")]
    public class ReceptionInsuranceController : BaseController
    {
        private readonly IReceptionService _receptionService;
        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly IPatientInsuranceValidationService _patientInsuranceValidationService;
        private readonly ICombinedInsuranceCalculationService _combinedInsuranceCalculationService;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionInsuranceController(
            IReceptionService receptionService,
            IPatientInsuranceService patientInsuranceService,
            IPatientInsuranceValidationService patientInsuranceValidationService,
            ICombinedInsuranceCalculationService combinedInsuranceCalculationService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));
            _patientInsuranceValidationService = patientInsuranceValidationService ?? throw new ArgumentNullException(nameof(patientInsuranceValidationService));
            _combinedInsuranceCalculationService = combinedInsuranceCalculationService ?? throw new ArgumentNullException(nameof(combinedInsuranceCalculationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Insurance Validation

        /// <summary>
        /// اعتبارسنجی بیمه بیمار برای پذیرش
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ValidatePatientInsurance(int patientId)
        {
            try
            {
                _logger.Information("🔍 اعتبارسنجی بیمه بیمار: {PatientId}, کاربر: {UserName}", 
                    patientId, _currentUserService.UserName);

                var result = await _patientInsuranceValidationService.ValidatePatientInsuranceForReceptionAsync(patientId, new List<int>(), DateTime.Now);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "اعتبارسنجی بیمه با موفقیت انجام شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی بیمه بیمار: {PatientId}", patientId);
                return Json(new { success = false, message = "خطا در اعتبارسنجی بیمه" });
            }
        }

        /// <summary>
        /// بررسی سریع اعتبار بیمه بیمار
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> QuickValidateInsurance(int patientId)
        {
            try
            {
                _logger.Information("⚡ بررسی سریع اعتبار بیمه بیمار: {PatientId}, کاربر: {UserName}", 
                    patientId, _currentUserService.UserName);

                var result = await _patientInsuranceValidationService.QuickValidatePatientInsuranceAsync(patientId, DateTime.Now);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "بررسی اعتبار بیمه با موفقیت انجام شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی سریع اعتبار بیمه بیمار: {PatientId}", patientId);
                return Json(new { success = false, message = "خطا در بررسی اعتبار بیمه" });
            }
        }

        /// <summary>
        /// دریافت وضعیت کامل بیمه بیمار
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetPatientInsuranceStatus(int patientId)
        {
            try
            {
                _logger.Information("📊 دریافت وضعیت بیمه بیمار: {PatientId}, کاربر: {UserName}", 
                    patientId, _currentUserService.UserName);

                var result = await _patientInsuranceService.GetPatientInsuranceStatusForReceptionAsync(patientId);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "وضعیت بیمه با موفقیت دریافت شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت وضعیت بیمه بیمار: {PatientId}", patientId);
                return Json(new { success = false, message = "خطا در دریافت وضعیت بیمه" });
            }
        }

        #endregion

        #region Insurance Calculation

        /// <summary>
        /// محاسبه سهم بیمه برای پذیرش
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateInsuranceShare(int patientId, int serviceId, decimal serviceAmount, DateTime? calculationDate = null)
        {
            try
            {
                _logger.Information("💰 محاسبه سهم بیمه: بیمار {PatientId}, خدمت {ServiceId}, مبلغ {Amount}, کاربر: {UserName}", 
                    patientId, serviceId, serviceAmount, _currentUserService.UserName);

                var result = await _combinedInsuranceCalculationService.CalculatePatientInsuranceForReceptionAsync(
                    patientId, new List<int> { serviceId }, calculationDate ?? DateTime.Now);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "محاسبه سهم بیمه با موفقیت انجام شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه سهم بیمه: بیمار {PatientId}, خدمت {ServiceId}", patientId, serviceId);
                return Json(new { success = false, message = "خطا در محاسبه سهم بیمه" });
            }
        }

        /// <summary>
        /// دریافت بیمه‌های بیمار برای پذیرش
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetPatientInsurances(int patientId)
        {
            try
            {
                _logger.Information("📋 دریافت بیمه‌های بیمار: {PatientId}, کاربر: {UserName}", 
                    patientId, _currentUserService.UserName);

                var result = await _patientInsuranceService.GetPatientInsurancesForReceptionAsync(patientId);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "بیمه‌های بیمار با موفقیت دریافت شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه‌های بیمار: {PatientId}", patientId);
                return Json(new { success = false, message = "خطا در دریافت بیمه‌ها" });
            }
        }

        #endregion

        #region External Inquiry

        /// <summary>
        /// استعلام هویت بیمار از سیستم خارجی
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> InquiryPatientIdentity(string nationalCode, DateTime birthDate)
        {
            try
            {
                _logger.Information("🔍 استعلام هویت بیمار: کد ملی {NationalCode}, تاریخ تولد {BirthDate}, کاربر: {UserName}", 
                    nationalCode, birthDate, _currentUserService.UserName);

                if (string.IsNullOrWhiteSpace(nationalCode))
                {
                    return Json(new { success = false, message = "کد ملی الزامی است" });
                }

                var result = await _receptionService.InquiryPatientIdentityAsync(nationalCode, birthDate);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "استعلام هویت با موفقیت انجام شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در استعلام هویت بیمار: کد ملی {NationalCode}", nationalCode);
                return Json(new { success = false, message = "خطا در استعلام هویت" });
            }
        }

        #endregion

        #region Insurance Management

        /// <summary>
        /// دریافت بیمه‌های پایه و تکمیلی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetInsuranceProviders()
        {
            try
            {
                _logger.Information("🏥 دریافت بیمه‌های پایه و تکمیلی, کاربر: {UserName}", _currentUserService.UserName);

                var result = await _receptionService.GetInsuranceProvidersAsync();
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "بیمه‌ها با موفقیت دریافت شدند"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت بیمه‌ها");
                return Json(new { success = false, message = "خطا در دریافت بیمه‌ها" });
            }
        }

        /// <summary>
        /// دریافت بیمه‌های تکمیلی بر اساس بیمه پایه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetSupplementaryInsurances(int baseInsuranceId)
        {
            try
            {
                _logger.Information("🔄 دریافت بیمه‌های تکمیلی برای بیمه پایه: {BaseInsuranceId}, کاربر: {UserName}", 
                    baseInsuranceId, _currentUserService.UserName);

                var result = await _receptionService.GetSupplementaryInsurancesAsync(baseInsuranceId);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "بیمه‌های تکمیلی با موفقیت دریافت شدند"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت بیمه‌های تکمیلی: {BaseInsuranceId}", baseInsuranceId);
                return Json(new { success = false, message = "خطا در دریافت بیمه‌های تکمیلی" });
            }
        }

        /// <summary>
        /// محاسبه بیمه برای پذیرش
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CalculateInsurance(int baseInsuranceId, int? supplementaryInsuranceId, int serviceId)
        {
            try
            {
                _logger.Information("💰 محاسبه بیمه: پایه {BaseInsuranceId}, تکمیلی {SupplementaryInsuranceId}, خدمت {ServiceId}, کاربر: {UserName}", 
                    baseInsuranceId, supplementaryInsuranceId, serviceId, _currentUserService.UserName);

                var result = await _receptionService.CalculateInsuranceAsync(baseInsuranceId, supplementaryInsuranceId, serviceId);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "محاسبه بیمه با موفقیت انجام شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در محاسبه بیمه: پایه {BaseInsuranceId}, خدمت {ServiceId}", 
                    baseInsuranceId, serviceId);
                return Json(new { success = false, message = "خطا در محاسبه بیمه" });
            }
        }

        /// <summary>
        /// تغییر بیمه بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ChangePatientInsurance(int patientId, int baseInsuranceId, int? supplementaryInsuranceId)
        {
            try
            {
                _logger.Information("🔄 تغییر بیمه بیمار: {PatientId}, پایه {BaseInsuranceId}, تکمیلی {SupplementaryInsuranceId}, کاربر: {UserName}", 
                    patientId, baseInsuranceId, supplementaryInsuranceId, _currentUserService.UserName);

                var result = await _receptionService.ChangePatientInsuranceAsync(patientId, baseInsuranceId, supplementaryInsuranceId);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    message = "بیمه بیمار با موفقیت تغییر کرد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در تغییر بیمه بیمار: {PatientId}", patientId);
                return Json(new { success = false, message = "خطا در تغییر بیمه بیمار" });
            }
        }

        #endregion

        #region Insurance Status

        /// <summary>
        /// دریافت وضعیت بیمه‌ها برای سایدبار
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetInsuranceStatus()
        {
            try
            {
                _logger.Information("🏥 دریافت وضعیت بیمه‌ها برای سایدبار. کاربر: {UserName}", _currentUserService.UserName);

                // دریافت آمار بیمه‌ها
                var activeInsurances = await _patientInsuranceService.GetActiveInsurancesCountAsync();
                var expiredInsurances = await _patientInsuranceService.GetExpiredInsurancesCountAsync();

                var result = new
                {
                    success = true,
                    data = new
                    {
                        activeInsurances = activeInsurances,
                        expiredInsurances = expiredInsurances
                    }
                };

                _logger.Information("✅ وضعیت بیمه‌ها دریافت شد: فعال={Active}, منقضی={Expired}", activeInsurances, expiredInsurances);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت وضعیت بیمه‌ها");
                return Json(new { success = false, message = "خطا در دریافت وضعیت بیمه‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
