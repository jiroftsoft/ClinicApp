using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Models.Entities.Insurance;
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
        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly IPatientInsuranceValidationService _patientInsuranceValidationService;
        private readonly ICombinedInsuranceCalculationService _combinedInsuranceCalculationService;
        private readonly IInsuranceProviderService _insuranceProviderService;
        private readonly IInsurancePlanService _insurancePlanService;
        private readonly ISupplementaryInsuranceService _supplementaryInsuranceService;
        private readonly IReceptionService _receptionService;
        private readonly IInsuranceCalculationService _insuranceCalculationService;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionInsuranceController(
            IPatientInsuranceService patientInsuranceService,
            IPatientInsuranceValidationService patientInsuranceValidationService,
            ICombinedInsuranceCalculationService combinedInsuranceCalculationService,
            IInsuranceProviderService insuranceProviderService,
            IInsurancePlanService insurancePlanService,
            ISupplementaryInsuranceService supplementaryInsuranceService,
            IReceptionService receptionService,
            IInsuranceCalculationService insuranceCalculationService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(logger)
        {
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));
            _patientInsuranceValidationService = patientInsuranceValidationService ?? throw new ArgumentNullException(nameof(patientInsuranceValidationService));
            _combinedInsuranceCalculationService = combinedInsuranceCalculationService ?? throw new ArgumentNullException(nameof(combinedInsuranceCalculationService));
            _insuranceProviderService = insuranceProviderService ?? throw new ArgumentNullException(nameof(insuranceProviderService));
            _insurancePlanService = insurancePlanService ?? throw new ArgumentNullException(nameof(insurancePlanService));
            _supplementaryInsuranceService = supplementaryInsuranceService ?? throw new ArgumentNullException(nameof(supplementaryInsuranceService));
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _insuranceCalculationService = insuranceCalculationService ?? throw new ArgumentNullException(nameof(insuranceCalculationService));
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
        /// بارگذاری اطلاعات بیمه بیمار - Production Ready
        /// </summary>
        [HttpPost]
        [Route("Load")]
        public async Task<JsonResult> Load(int patientId)
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            try
            {
                _logger.Information("[{RequestId}] 🏥 بارگذاری اطلاعات بیمه بیمار: {PatientId}, کاربر: {UserName}", 
                    requestId, patientId, _currentUserService.UserName);

                if (patientId <= 0)
                {
                    _logger.Warning("[{RequestId}] شناسه بیمار نامعتبر: {PatientId}", requestId, patientId);
                    return Json(new { success = false, message = "شناسه بیمار نامعتبر است" });
                }

                var result = await _patientInsuranceService.GetPatientInsuranceStatusForReceptionAsync(patientId);
                
                if (result.Success)
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _logger.Information("[{RequestId}] ✅ بارگذاری موفق اطلاعات بیمه در {Duration}ms", requestId, duration);
                    
                    return Json(new { 
                        success = true, 
                        data = result.Data,
                        message = "اطلاعات بیمه با موفقیت بارگذاری شد"
                    });
                }
                else
                {
                    _logger.Warning("[{RequestId}] خطا در بارگذاری اطلاعات بیمه: {Error}", requestId, result.Message);
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[{RequestId}] خطا در بارگذاری اطلاعات بیمه برای بیمار: {PatientId}", requestId, patientId);
                return Json(new { success = false, message = "خطا در بارگذاری اطلاعات بیمه. لطفاً دوباره تلاش کنید." });
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

                // استعلام هویت بیمار از سیستم خارجی
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
        /// دریافت ارائه‌دهندگان بیمه - Production Ready
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetInsuranceProviders()
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            try
            {
                _logger.Information("[{RequestId}] 🏥 دریافت ارائه‌دهندگان بیمه، کاربر: {UserName}", 
                    requestId, _currentUserService.UserName);

                // دریافت ارائه‌دهندگان بیمه فعال از دیتابیس
                var result = await _insuranceProviderService.GetProvidersAsync("", 1, 100);
                
                if (result.Success)
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _logger.Information("[{RequestId}] ✅ ارائه‌دهندگان بیمه با موفقیت دریافت شدند در {Duration}ms", 
                        requestId, duration);
                    
                    return Json(new { 
                        success = true, 
                        data = result.Data,
                        message = "ارائه‌دهندگان بیمه با موفقیت دریافت شدند"
                    });
                }
                else
                {
                    _logger.Warning("[{RequestId}] خطا در دریافت ارائه‌دهندگان بیمه: {Error}", 
                        requestId, result.Message);
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[{RequestId}] خطا در دریافت ارائه‌دهندگان بیمه", requestId);
                return Json(new { success = false, message = "خطا در دریافت ارائه‌دهندگان بیمه. لطفاً دوباره تلاش کنید." });
            }
        }

        /// <summary>
        /// دریافت ارائه‌دهندگان بیمه پایه - Production Ready
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetPrimaryInsuranceProviders()
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            try
            {
                _logger.Information("[{RequestId}] 🏥 دریافت ارائه‌دهندگان بیمه پایه، کاربر: {UserName}", 
                    requestId, _currentUserService.UserName);

                // دریافت ارائه‌دهندگان بیمه پایه فعال از دیتابیس
                var result = await _insuranceProviderService.GetProvidersByTypeAsync(InsuranceType.Primary);
                
                if (result.Success)
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _logger.Information("[{RequestId}] ✅ ارائه‌دهندگان بیمه پایه با موفقیت دریافت شدند در {Duration}ms", 
                        requestId, duration);
                    
                    return Json(new { 
                        success = true, 
                        data = result.Data,
                        message = "ارائه‌دهندگان بیمه پایه با موفقیت دریافت شدند"
                    });
                }
                else
                {
                    _logger.Warning("[{RequestId}] خطا در دریافت ارائه‌دهندگان بیمه پایه: {Error}", 
                        requestId, result.Message);
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[{RequestId}] خطا در دریافت ارائه‌دهندگان بیمه پایه", requestId);
                return Json(new { success = false, message = "خطا در دریافت ارائه‌دهندگان بیمه پایه. لطفاً دوباره تلاش کنید." });
            }
        }

        /// <summary>
        /// دریافت ارائه‌دهندگان بیمه تکمیلی - Production Ready
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetSupplementaryInsuranceProviders()
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            try
            {
                _logger.Information("[{RequestId}] 🏥 دریافت ارائه‌دهندگان بیمه تکمیلی، کاربر: {UserName}", 
                    requestId, _currentUserService.UserName);

                // دریافت ارائه‌دهندگان بیمه تکمیلی فعال از دیتابیس
                var result = await _insuranceProviderService.GetProvidersByTypeAsync(InsuranceType.Supplementary);
                
                if (result.Success)
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _logger.Information("[{RequestId}] ✅ ارائه‌دهندگان بیمه تکمیلی با موفقیت دریافت شدند در {Duration}ms", 
                        requestId, duration);
                    
                    return Json(new { 
                        success = true, 
                        data = result.Data,
                        message = "ارائه‌دهندگان بیمه تکمیلی با موفقیت دریافت شدند"
                    });
                }
                else
                {
                    _logger.Warning("[{RequestId}] خطا در دریافت ارائه‌دهندگان بیمه تکمیلی: {Error}", 
                        requestId, result.Message);
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[{RequestId}] خطا در دریافت ارائه‌دهندگان بیمه تکمیلی", requestId);
                return Json(new { success = false, message = "خطا در دریافت ارائه‌دهندگان بیمه تکمیلی. لطفاً دوباره تلاش کنید." });
            }
        }

        /// <summary>
        /// دریافت طرح‌های بیمه بر اساس ارائه‌دهنده - Production Ready
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetInsurancePlans(int providerId)
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            try
            {
                _logger.Information("[{RequestId}] 🏥 دریافت طرح‌های بیمه برای ارائه‌دهنده: {ProviderId}, کاربر: {UserName}", 
                    requestId, providerId, _currentUserService.UserName);

                if (providerId <= 0)
                {
                    _logger.Warning("[{RequestId}] شناسه ارائه‌دهنده بیمه نامعتبر: {ProviderId}", requestId, providerId);
                    return Json(new { success = false, message = "شناسه ارائه‌دهنده بیمه نامعتبر است" });
                }

                // دریافت طرح‌های بیمه فعال از دیتابیس
                var result = await _insurancePlanService.GetPlansByProviderAsync(providerId);
                
                if (result.Success)
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _logger.Information("[{RequestId}] ✅ طرح‌های بیمه با موفقیت دریافت شدند در {Duration}ms", 
                        requestId, duration);
                    
                    return Json(new { 
                        success = true, 
                        data = result.Data,
                        message = "طرح‌های بیمه با موفقیت دریافت شدند"
                    });
                }
                else
                {
                    _logger.Warning("[{RequestId}] خطا در دریافت طرح‌های بیمه: {Error}", 
                        requestId, result.Message);
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[{RequestId}] خطا در دریافت طرح‌های بیمه برای ارائه‌دهنده: {ProviderId}", 
                    requestId, providerId);
                return Json(new { success = false, message = "خطا در دریافت طرح‌های بیمه. لطفاً دوباره تلاش کنید." });
            }
        }

        /// <summary>
        /// دریافت طرح‌های بیمه پایه بر اساس ارائه‌دهنده - Production Ready
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetPrimaryInsurancePlans(int providerId)
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            try
            {
                _logger.Information("[{RequestId}] 🏥 دریافت طرح‌های بیمه پایه برای ارائه‌دهنده: {ProviderId}, کاربر: {UserName}", 
                    requestId, providerId, _currentUserService.UserName);

                if (providerId <= 0)
                {
                    _logger.Warning("[{RequestId}] شناسه ارائه‌دهنده بیمه نامعتبر: {ProviderId}", requestId, providerId);
                    return Json(new { success = false, message = "شناسه ارائه‌دهنده بیمه نامعتبر است" });
                }

                // دریافت طرح‌های بیمه پایه فعال از دیتابیس
                var result = await _insurancePlanService.GetPrimaryInsurancePlansByProviderAsync(providerId);
                
                if (result.Success)
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _logger.Information("[{RequestId}] ✅ طرح‌های بیمه پایه با موفقیت دریافت شدند در {Duration}ms", 
                        requestId, duration);
                    
                    return Json(new { 
                        success = true, 
                        data = result.Data,
                        message = "طرح‌های بیمه پایه با موفقیت دریافت شدند"
                    });
                }
                else
                {
                    _logger.Warning("[{RequestId}] خطا در دریافت طرح‌های بیمه پایه: {Error}", 
                        requestId, result.Message);
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[{RequestId}] خطا در دریافت طرح‌های بیمه پایه برای ارائه‌دهنده: {ProviderId}", 
                    requestId, providerId);
                return Json(new { success = false, message = "خطا در دریافت طرح‌های بیمه پایه. لطفاً دوباره تلاش کنید." });
            }
        }

        /// <summary>
        /// دریافت طرح‌های بیمه تکمیلی بر اساس ارائه‌دهنده - Production Ready
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetSupplementaryInsurancePlans(int providerId)
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            try
            {
                _logger.Information("[{RequestId}] 🏥 دریافت طرح‌های بیمه تکمیلی برای ارائه‌دهنده: {ProviderId}, کاربر: {UserName}", 
                    requestId, providerId, _currentUserService.UserName);

                if (providerId <= 0)
                {
                    _logger.Warning("[{RequestId}] شناسه ارائه‌دهنده بیمه نامعتبر: {ProviderId}", requestId, providerId);
                    return Json(new { success = false, message = "شناسه ارائه‌دهنده بیمه نامعتبر است" });
                }

                // دریافت طرح‌های بیمه تکمیلی فعال از دیتابیس
                var result = await _insurancePlanService.GetSupplementaryInsurancePlansByProviderAsync(providerId);
                
                if (result.Success)
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _logger.Information("[{RequestId}] ✅ طرح‌های بیمه تکمیلی با موفقیت دریافت شدند در {Duration}ms", 
                        requestId, duration);

                return Json(new { 
                    success = true, 
                    data = result.Data,
                        message = "طرح‌های بیمه تکمیلی با موفقیت دریافت شدند"
                    });
                }
                else
                {
                    _logger.Warning("[{RequestId}] خطا در دریافت طرح‌های بیمه تکمیلی: {Error}", 
                        requestId, result.Message);
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[{RequestId}] خطا در دریافت طرح‌های بیمه تکمیلی برای ارائه‌دهنده: {ProviderId}", 
                    requestId, providerId);
                return Json(new { success = false, message = "خطا در دریافت طرح‌های بیمه تکمیلی. لطفاً دوباره تلاش کنید." });
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

                // دریافت بیمه‌های تکمیلی بر اساس بیمه پایه
                var result = await _patientInsuranceService.GetSupplementaryInsurancesByPatientAsync(baseInsuranceId);
                
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

                // محاسبه سهم بیمه و بیمار
                // TODO: پیاده‌سازی محاسبه سهم بیمه با سرویس مناسب
                var result = ServiceResult<object>.Failed("محاسبه سهم بیمه در حال پیاده‌سازی است");
                
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
        /// ذخیره اطلاعات بیمه بیمار - Production Ready
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Save")]
        public async Task<JsonResult> Save(PatientInsuranceReceptionFormViewModel model)
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            try
            {
                _logger.Information("[{RequestId}] 💾 ذخیره اطلاعات بیمه بیمار: {PatientId}, کاربر: {UserName}", 
                    requestId, model.PatientId, _currentUserService.UserName);

                // اعتبارسنجی ورودی‌ها
                if (model.PatientId <= 0)
                {
                    _logger.Warning("[{RequestId}] شناسه بیمار نامعتبر: {PatientId}", requestId, model.PatientId);
                    return Json(new { success = false, message = "شناسه بیمار نامعتبر است" });
                }

                // ذخیره بیمه پایه
                if (model.PrimaryInsuranceId.HasValue)
                {
                    var primaryResult = await _patientInsuranceService.UpdatePatientPrimaryInsuranceAsync(
                        model.PatientId, 
                        model.PrimaryInsuranceId.Value,
                        model.PrimaryPolicyNumber,
                        model.PrimaryCardNumber
                    );
                    
                    if (!primaryResult.Success)
                    {
                        _logger.Warning("[{RequestId}] خطا در ذخیره بیمه پایه: {Error}", requestId, primaryResult.Message);
                        return Json(new { success = false, message = $"خطا در ذخیره بیمه پایه: {primaryResult.Message}" });
                    }
                }

                // ذخیره بیمه تکمیلی
                if (model.SupplementaryInsuranceId.HasValue)
                {
                    var supplementaryResult = await _patientInsuranceService.UpdatePatientSupplementaryInsuranceAsync(
                        model.PatientId, 
                        model.SupplementaryInsuranceId.Value,
                        model.SupplementaryPolicyNumber,
                        model.SupplementaryExpiryDate
                    );
                    
                    if (!supplementaryResult.Success)
                    {
                        _logger.Warning("[{RequestId}] خطا در ذخیره بیمه تکمیلی: {Error}", requestId, supplementaryResult.Message);
                        return Json(new { success = false, message = $"خطا در ذخیره بیمه تکمیلی: {supplementaryResult.Message}" });
                    }
                }

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Information("[{RequestId}] ✅ ذخیره اطلاعات بیمه موفق در {Duration}ms", requestId, duration);

                return Json(new { 
                    success = true, 
                    message = "اطلاعات بیمه با موفقیت ذخیره شد",
                    data = new {
                        PatientId = model.PatientId,
                        PrimaryInsuranceId = model.PrimaryInsuranceId,
                        SupplementaryInsuranceId = model.SupplementaryInsuranceId,
                        SavedAt = DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[{RequestId}] ❌ خطا در ذخیره اطلاعات بیمه بیمار: {PatientId}", requestId, model.PatientId);
                return Json(new { success = false, message = "خطا در ذخیره اطلاعات بیمه. لطفاً دوباره تلاش کنید." });
            }
        }

        /// <summary>
        /// تغییر بیمه بیمار - Production Ready
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ChangePatientInsurance")]
        public async Task<JsonResult> ChangePatientInsurance(int patientId, int baseInsuranceId, int? supplementaryInsuranceId)
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            try
            {
                _logger.Information("[{RequestId}] 🔄 تغییر بیمه بیمار: {PatientId}, پایه {BaseInsuranceId}, تکمیلی {SupplementaryInsuranceId}, کاربر: {UserName}", 
                    requestId, patientId, baseInsuranceId, supplementaryInsuranceId, _currentUserService.UserName);

                // اعتبارسنجی ورودی‌ها
                if (patientId <= 0)
                {
                    _logger.Warning("[{RequestId}] شناسه بیمار نامعتبر: {PatientId}", requestId, patientId);
                    return Json(new { success = false, message = "شناسه بیمار نامعتبر است" });
                }

                // تغییر بیمه پایه
                var primaryResult = await _patientInsuranceService.ChangePatientPrimaryInsuranceAsync(
                    patientId, 
                    baseInsuranceId
                );
                
                if (!primaryResult.Success)
                {
                    _logger.Warning("[{RequestId}] خطا در تغییر بیمه پایه: {Error}", requestId, primaryResult.Message);
                    return Json(new { success = false, message = $"خطا در تغییر بیمه پایه: {primaryResult.Message}" });
                }

                // تغییر بیمه تکمیلی (اگر انتخاب شده باشد)
                if (supplementaryInsuranceId.HasValue)
                {
                    var supplementaryResult = await _patientInsuranceService.ChangePatientSupplementaryInsuranceAsync(
                        patientId, 
                        supplementaryInsuranceId.Value
                    );
                    
                    if (!supplementaryResult.Success)
                    {
                        _logger.Warning("[{RequestId}] خطا در تغییر بیمه تکمیلی: {Error}", requestId, supplementaryResult.Message);
                        return Json(new { success = false, message = $"خطا در تغییر بیمه تکمیلی: {supplementaryResult.Message}" });
                    }
                }

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Information("[{RequestId}] ✅ تغییر بیمه بیمار موفق در {Duration}ms", requestId, duration);

                return Json(new { 
                    success = true, 
                    message = "بیمه بیمار با موفقیت تغییر کرد",
                    data = new {
                        PatientId = patientId,
                        BaseInsuranceId = baseInsuranceId,
                        SupplementaryInsuranceId = supplementaryInsuranceId,
                        ChangedAt = DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[{RequestId}] ❌ خطا در تغییر بیمه بیمار: {PatientId}", requestId, patientId);
                return Json(new { success = false, message = "خطا در تغییر بیمه بیمار. لطفاً دوباره تلاش کنید." });
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
