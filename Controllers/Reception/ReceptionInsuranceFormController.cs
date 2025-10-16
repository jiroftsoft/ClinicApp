using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers;
using ClinicApp.Core;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;
using ClinicApp.ViewModels.Insurance.InsurancePlan;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر تخصصی مدیریت بیمه در فرم پذیرش - رعایت اصل SRP
    /// مسئولیت: فقط مدیریت بیمه در فرم پذیرش (بارگذاری dropdown ها، انتخاب بیمه، محاسبات)
    /// </summary>
    [RoutePrefix("Reception/InsuranceForm")]
    public class ReceptionInsuranceFormController : BaseController
    {
        private readonly IInsuranceProviderService _insuranceProviderService;
        private readonly IInsurancePlanService _insurancePlanService;
        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly ICombinedInsuranceCalculationService _combinedInsuranceCalculationService;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionInsuranceFormController(
            IInsuranceProviderService insuranceProviderService,
            IInsurancePlanService insurancePlanService,
            IPatientInsuranceService patientInsuranceService,
            ICombinedInsuranceCalculationService combinedInsuranceCalculationService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(logger)
        {
            _insuranceProviderService = insuranceProviderService ?? throw new ArgumentNullException(nameof(insuranceProviderService));
            _insurancePlanService = insurancePlanService ?? throw new ArgumentNullException(nameof(insurancePlanService));
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));
            _combinedInsuranceCalculationService = combinedInsuranceCalculationService ?? throw new ArgumentNullException(nameof(combinedInsuranceCalculationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Insurance Provider Management

        /// <summary>
        /// دریافت بیمه‌گذاران فعال برای فرم پذیرش
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetInsuranceProviders()
        {
            try
            {
                _logger.Information("🏥 دریافت بیمه‌گذاران برای فرم پذیرش. کاربر: {UserName}", _currentUserService.UserName);

                var result = await _insuranceProviderService.GetAllActiveProvidersAsync();
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "بیمه‌گذاران با موفقیت دریافت شدند"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت بیمه‌گذاران");
                return Json(new { success = false, message = "خطا در دریافت بیمه‌گذاران" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت بیمه‌گذاران بر اساس نوع بیمه
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetInsuranceProvidersByType(int insuranceType)
        {
            try
            {
                _logger.Information("🏥 دریافت بیمه‌گذاران نوع {InsuranceType} برای فرم پذیرش. کاربر: {UserName}", 
                    insuranceType, _currentUserService.UserName);

                var result = await _insuranceProviderService.GetProvidersByTypeAsync((InsuranceType)insuranceType);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "بیمه‌گذاران با موفقیت دریافت شدند"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت بیمه‌گذاران نوع {InsuranceType}", insuranceType);
                return Json(new { success = false, message = "خطا در دریافت بیمه‌گذاران" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Insurance Plan Management

        /// <summary>
        /// دریافت طرح‌های بیمه بر اساس بیمه‌گذار و نوع بیمه
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetInsurancePlans(int providerId, int insuranceType)
        {
            try
            {
                _logger.Information("🏥 دریافت طرح‌های بیمه برای بیمه‌گذار {ProviderId} و نوع {InsuranceType}. کاربر: {UserName}", 
                    providerId, insuranceType, _currentUserService.UserName);

                var result = await _insurancePlanService.GetPlansByProviderAndTypeAsync(providerId, (InsuranceType)insuranceType);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "طرح‌های بیمه با موفقیت دریافت شدند"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت طرح‌های بیمه برای بیمه‌گذار {ProviderId}", providerId);
                return Json(new { success = false, message = "خطا در دریافت طرح‌های بیمه" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت تمام طرح‌های بیمه فعال
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetAllInsurancePlans()
        {
            try
            {
                _logger.Information("🏥 دریافت تمام طرح‌های بیمه فعال برای فرم پذیرش. کاربر: {UserName}", _currentUserService.UserName);

                var result = await _insurancePlanService.GetAllActivePlansAsync();
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "طرح‌های بیمه با موفقیت دریافت شدند"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت طرح‌های بیمه");
                return Json(new { success = false, message = "خطا در دریافت طرح‌های بیمه" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Patient Insurance Management

        /// <summary>
        /// دریافت بیمه‌های بیمار برای فرم پذیرش
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetPatientInsurances(int patientId)
        {
            try
            {
                _logger.Information("🏥 دریافت بیمه‌های بیمار {PatientId} برای فرم پذیرش. کاربر: {UserName}", 
                    patientId, _currentUserService.UserName);

                var result = await _patientInsuranceService.GetPatientInsurancesForReceptionAsync(patientId);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "بیمه‌های بیمار با موفقیت دریافت شدند"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت بیمه‌های بیمار {PatientId}", patientId);
                return Json(new { success = false, message = "خطا در دریافت بیمه‌های بیمار" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ذخیره یا به‌روزرسانی بیمه بیمار در فرم پذیرش
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> SavePatientInsurance(PatientInsuranceReceptionFormViewModel model)
        {
            try
            {
                _logger.Information("🏥 ذخیره بیمه بیمار {PatientId} در فرم پذیرش. کاربر: {UserName}", 
                    model.PatientId, _currentUserService.UserName);

                // TODO: پیاده‌سازی منطق ذخیره بیمه بیمار
                // این متد باید بیمه‌های انتخاب شده توسط منشی را ذخیره کند

                return Json(new { 
                    success = true, 
                    message = "بیمه بیمار با موفقیت ذخیره شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ذخیره بیمه بیمار {PatientId}", model.PatientId);
                return Json(new { success = false, message = "خطا در ذخیره بیمه بیمار" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Insurance Calculation

        /// <summary>
        /// محاسبه سهم بیمه برای پذیرش
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateInsuranceShare(int patientId, int serviceId, decimal serviceAmount, int? primaryInsuranceId = null, int? supplementaryInsuranceId = null)
        {
            try
            {
                _logger.Information("💰 محاسبه سهم بیمه: بیمار {PatientId}, خدمت {ServiceId}, مبلغ {Amount}, بیمه پایه {PrimaryId}, بیمه تکمیلی {SupplementaryId}. کاربر: {UserName}", 
                    patientId, serviceId, serviceAmount, primaryInsuranceId, supplementaryInsuranceId, _currentUserService.UserName);

                var result = await _combinedInsuranceCalculationService.CalculatePatientInsuranceForReceptionAsync(
                    patientId, new List<int> { serviceId }, DateTime.Now);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "محاسبه سهم بیمه با موفقیت انجام شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در محاسبه سهم بیمه: بیمار {PatientId}, خدمت {ServiceId}", patientId, serviceId);
                return Json(new { success = false, message = "خطا در محاسبه سهم بیمه" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
