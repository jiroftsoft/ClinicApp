using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers;
using ClinicApp.Core;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Services.Reception;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر تخصصی مدیریت اطلاعات هویتی و بیمه‌ای در فرم پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. جستجوی بیمار با کد ملی
    /// 2. بارگذاری بیمه‌های اصلی و تکمیلی
    /// 3. تغییر realtime بیمه‌ها توسط منشی
    /// 4. مدیریت بیمه‌های ترکیبی
    /// 5. بهینه‌سازی برای محیط درمانی
    /// 
    /// نکته حیاتی: این کنترلر از سرویس‌های تخصصی استفاده می‌کند
    /// </summary>
    [RoutePrefix("Reception/PatientIdentity")]
    public class ReceptionPatientIdentityController : BaseController
    {
        private readonly ReceptionPatientIdentityService _patientIdentityService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionPatientIdentityController(
            ReceptionPatientIdentityService patientIdentityService,
            ILogger logger,
            ICurrentUserService currentUserService) : base(logger)
        {
            _patientIdentityService = patientIdentityService ?? throw new ArgumentNullException(nameof(patientIdentityService));
            _logger = logger.ForContext<ReceptionPatientIdentityController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Patient Search by National Code

        /// <summary>
        /// جستجوی بیمار با کد ملی و بارگذاری اطلاعات کامل
        /// </summary>
        /// <param name="nationalCode">کد ملی بیمار</param>
        /// <returns>اطلاعات کامل بیمار و بیمه‌هایش</returns>
        [HttpPost]
        [Route("SearchByNationalCode")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SearchByNationalCode(string nationalCode)
        {
            try
            {
                _logger.Information("🔍 جستجوی بیمار با کد ملی: {NationalCode}, کاربر: {UserName}", 
                    nationalCode, _currentUserService.UserName);

                var result = await _patientIdentityService.SearchPatientByNationalCodeAsync(nationalCode);
                
                if (result.Success)
                {
                    _logger.Information("✅ بیمار یافت شد: {PatientId}, نام: {FullName}", 
                        result.Data.PatientId, result.Data.FullName);
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                _logger.Warning("⚠️ بیمار یافت نشد: {NationalCode}", nationalCode);
                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در جستجوی بیمار با کد ملی: {NationalCode}", nationalCode);
                return Json(new { success = false, message = "خطا در جستجوی بیمار" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Insurance Management

        /// <summary>
        /// دریافت بیمه‌گذاران برای تغییر بیمه
        /// </summary>
        /// <param name="insuranceType">نوع بیمه</param>
        /// <returns>لیست بیمه‌گذاران</returns>
        [HttpPost]
        [Route("GetInsuranceProviders")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetInsuranceProviders(int insuranceType)
        {
            try
            {
                _logger.Information("🏥 دریافت بیمه‌گذاران برای تغییر بیمه. Type: {InsuranceType}, User: {UserName}", 
                    insuranceType, _currentUserService.UserName);

                var result = await _patientIdentityService.GetInsuranceProvidersForUpdateAsync((InsuranceType)insuranceType);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت بیمه‌گذاران نوع {InsuranceType}", insuranceType);
                return Json(new { success = false, message = "خطا در دریافت بیمه‌گذاران" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت طرح‌های بیمه برای بیمه‌گذار انتخاب شده
        /// </summary>
        /// <param name="providerId">شناسه بیمه‌گذار</param>
        /// <param name="insuranceType">نوع بیمه</param>
        /// <returns>لیست طرح‌های بیمه</returns>
        [HttpPost]
        [Route("GetInsurancePlans")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetInsurancePlans(int providerId, int insuranceType)
        {
            try
            {
                _logger.Information("🏥 دریافت طرح‌های بیمه برای تغییر. ProviderId: {ProviderId}, Type: {InsuranceType}, User: {UserName}", 
                    providerId, insuranceType, _currentUserService.UserName);

                var result = await _patientIdentityService.GetInsurancePlansForUpdateAsync(providerId, (InsuranceType)insuranceType);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت طرح‌های بیمه. ProviderId: {ProviderId}, Type: {InsuranceType}", providerId, insuranceType);
                return Json(new { success = false, message = "خطا در دریافت طرح‌های بیمه" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Real-time Insurance Updates

        /// <summary>
        /// تغییر بیمه بیمار در فرم پذیرش (realtime)
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="primaryInsuranceId">شناسه بیمه اصلی جدید</param>
        /// <param name="supplementaryInsuranceId">شناسه بیمه تکمیلی جدید</param>
        /// <returns>نتیجه تغییر بیمه</returns>
        [HttpPost]
        [Route("UpdateInsurance")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateInsurance(int patientId, int? primaryInsuranceId, int? supplementaryInsuranceId)
        {
            try
            {
                _logger.Information("🔄 تغییر بیمه بیمار در فرم پذیرش. PatientId: {PatientId}, Primary: {Primary}, Supplementary: {Supplementary}, User: {UserName}", 
                    patientId, primaryInsuranceId, supplementaryInsuranceId, _currentUserService.UserName);

                var result = await _patientIdentityService.UpdatePatientInsuranceRealtimeAsync(
                    patientId, primaryInsuranceId, supplementaryInsuranceId);
                
                if (result.Success)
                {
                    _logger.Information("✅ بیمه بیمار با موفقیت تغییر یافت. PatientId: {PatientId}", patientId);
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در تغییر بیمه بیمار. PatientId: {PatientId}", patientId);
                return Json(new { success = false, message = "خطا در تغییر بیمه بیمار" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
