using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Services.Interfaces;
using ClinicApp.Services.Insurance;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.Insurance
{
    /// <summary>
    /// کنترلر مدیریت بیمه بیماران - AJAX Endpoints
    /// </summary>
    public class PatientInsuranceManagementController : Controller
    {
        private readonly IPatientInsuranceManagementService _patientInsuranceManagementService;
        private readonly ILogger _logger;

        public PatientInsuranceManagementController(
            IPatientInsuranceManagementService patientInsuranceManagementService,
            ILogger logger)
        {
            _patientInsuranceManagementService = patientInsuranceManagementService;
            _logger = logger;
        }

        #region انتخاب بیمه پایه

        /// <summary>
        /// انتخاب بیمه پایه - AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SelectPrimaryInsurance(int patientId, int insurancePlanId, 
            string policyNumber, DateTime startDate, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("🏥 AJAX: انتخاب بیمه پایه. PatientId: {PatientId}, PlanId: {PlanId}", 
                    patientId, insurancePlanId);

                var result = await _patientInsuranceManagementService.SelectPrimaryInsuranceAsync(
                    patientId, insurancePlanId, policyNumber, startDate, endDate);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = result.Data.Message,
                        data = new
                        {
                            patientId = result.Data.PatientId,
                            insuranceId = result.Data.InsuranceId,
                            insuranceType = result.Data.InsuranceType,
                            insuranceName = result.Data.InsuranceName,
                            policyNumber = result.Data.PolicyNumber,
                            startDate = result.Data.StartDate.ToString("yyyy/MM/dd"),
                            endDate = result.Data.EndDate?.ToString("yyyy/MM/dd"),
                            isActive = result.Data.IsActive
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        errors = new[] { result.Message }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در AJAX انتخاب بیمه پایه. PatientId: {PatientId}", patientId);
                return Json(new
                {
                    success = false,
                    message = "خطای غیرمنتظره در انتخاب بیمه پایه",
                    errors = new[] { ex.Message }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region انتخاب بیمه تکمیلی

        /// <summary>
        /// انتخاب بیمه تکمیلی - AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SelectSupplementaryInsurance(int patientId, int insurancePlanId, 
            string policyNumber, DateTime startDate, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("🏥 AJAX: انتخاب بیمه تکمیلی. PatientId: {PatientId}, PlanId: {PlanId}", 
                    patientId, insurancePlanId);

                var result = await _patientInsuranceManagementService.SelectSupplementaryInsuranceAsync(
                    patientId, insurancePlanId, policyNumber, startDate, endDate);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = result.Data.Message,
                        data = new
                        {
                            patientId = result.Data.PatientId,
                            insuranceId = result.Data.InsuranceId,
                            insuranceType = result.Data.InsuranceType,
                            insuranceName = result.Data.InsuranceName,
                            policyNumber = result.Data.PolicyNumber,
                            startDate = result.Data.StartDate.ToString("yyyy/MM/dd"),
                            endDate = result.Data.EndDate?.ToString("yyyy/MM/dd"),
                            isActive = result.Data.IsActive
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        errors = new[] { result.Message }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در AJAX انتخاب بیمه تکمیلی. PatientId: {PatientId}", patientId);
                return Json(new
                {
                    success = false,
                    message = "خطای غیرمنتظره در انتخاب بیمه تکمیلی",
                    errors = new[] { ex.Message }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region تغییر بیمه

        /// <summary>
        /// تغییر بیمه پایه - AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ChangePrimaryInsurance(int patientId, int newInsurancePlanId, 
            string newPolicyNumber, DateTime startDate, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("🔄 AJAX: تغییر بیمه پایه. PatientId: {PatientId}, NewPlanId: {NewPlanId}", 
                    patientId, newInsurancePlanId);

                var result = await _patientInsuranceManagementService.ChangePrimaryInsuranceAsync(
                    patientId, newInsurancePlanId, newPolicyNumber, startDate, endDate);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "بیمه پایه با موفقیت تغییر کرد",
                        data = new
                        {
                            patientId = result.Data.PatientId,
                            insuranceId = result.Data.InsuranceId,
                            insuranceType = result.Data.InsuranceType,
                            insuranceName = result.Data.InsuranceName,
                            policyNumber = result.Data.PolicyNumber,
                            startDate = result.Data.StartDate.ToString("yyyy/MM/dd"),
                            endDate = result.Data.EndDate?.ToString("yyyy/MM/dd"),
                            isActive = result.Data.IsActive
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        errors = new[] { result.Message }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در AJAX تغییر بیمه پایه. PatientId: {PatientId}", patientId);
                return Json(new
                {
                    success = false,
                    message = "خطای غیرمنتظره در تغییر بیمه پایه",
                    errors = new[] { ex.Message }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// تغییر بیمه تکمیلی - AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ChangeSupplementaryInsurance(int patientId, int newInsurancePlanId, 
            string newPolicyNumber, DateTime startDate, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("🔄 AJAX: تغییر بیمه تکمیلی. PatientId: {PatientId}, NewPlanId: {NewPlanId}", 
                    patientId, newInsurancePlanId);

                var result = await _patientInsuranceManagementService.ChangeSupplementaryInsuranceAsync(
                    patientId, newInsurancePlanId, newPolicyNumber, startDate, endDate);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "بیمه تکمیلی با موفقیت تغییر کرد",
                        data = new
                        {
                            patientId = result.Data.PatientId,
                            insuranceId = result.Data.InsuranceId,
                            insuranceType = result.Data.InsuranceType,
                            insuranceName = result.Data.InsuranceName,
                            policyNumber = result.Data.PolicyNumber,
                            startDate = result.Data.StartDate.ToString("yyyy/MM/dd"),
                            endDate = result.Data.EndDate?.ToString("yyyy/MM/dd"),
                            isActive = result.Data.IsActive
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        errors = new[] { result.Message }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در AJAX تغییر بیمه تکمیلی. PatientId: {PatientId}", patientId);
                return Json(new
                {
                    success = false,
                    message = "خطای غیرمنتظره در تغییر بیمه تکمیلی",
                    errors = new[] { ex.Message }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region دریافت وضعیت

        /// <summary>
        /// دریافت وضعیت بیمه بیمار - AJAX
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetPatientInsuranceStatus(int patientId)
        {
            try
            {
                _logger.Information("📊 AJAX: دریافت وضعیت بیمه. PatientId: {PatientId}", patientId);

                var result = await _patientInsuranceManagementService.GetPatientInsuranceStatusAsync(patientId);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            patientId = result.Data.PatientId,
                            hasPrimaryInsurance = result.Data.HasPrimaryInsurance,
                            hasSupplementaryInsurance = result.Data.HasSupplementaryInsurance,
                            primaryInsurance = result.Data.PrimaryInsurance != null ? new
                            {
                                id = result.Data.PrimaryInsurance.Id,
                                name = result.Data.PrimaryInsurance.Name,
                                policyNumber = result.Data.PrimaryInsurance.PolicyNumber,
                                startDate = result.Data.PrimaryInsurance.StartDate.ToString("yyyy/MM/dd"),
                                endDate = result.Data.PrimaryInsurance.EndDate?.ToString("yyyy/MM/dd"),
                                isActive = result.Data.PrimaryInsurance.IsActive
                            } : null,
                            supplementaryInsurance = result.Data.SupplementaryInsurance != null ? new
                            {
                                id = result.Data.SupplementaryInsurance.Id,
                                name = result.Data.SupplementaryInsurance.Name,
                                policyNumber = result.Data.SupplementaryInsurance.PolicyNumber,
                                startDate = result.Data.SupplementaryInsurance.StartDate.ToString("yyyy/MM/dd"),
                                endDate = result.Data.SupplementaryInsurance.EndDate?.ToString("yyyy/MM/dd"),
                                isActive = result.Data.SupplementaryInsurance.IsActive
                            } : null
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        errors = new[] { result.Message }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در AJAX دریافت وضعیت بیمه. PatientId: {PatientId}", patientId);
                return Json(new
                {
                    success = false,
                    message = "خطای غیرمنتظره در دریافت وضعیت بیمه",
                    errors = new[] { ex.Message }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region غیرفعال کردن

        /// <summary>
        /// غیرفعال کردن بیمه پایه - AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeactivatePrimaryInsurance(int patientId)
        {
            try
            {
                _logger.Information("🚫 AJAX: غیرفعال کردن بیمه پایه. PatientId: {PatientId}", patientId);

                var result = await _patientInsuranceManagementService.DeactivatePrimaryInsuranceAsync(patientId);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "بیمه پایه با موفقیت غیرفعال شد"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        errors = new[] { result.Message }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در AJAX غیرفعال کردن بیمه پایه. PatientId: {PatientId}", patientId);
                return Json(new
                {
                    success = false,
                    message = "خطای غیرمنتظره در غیرفعال کردن بیمه پایه",
                    errors = new[] { ex.Message }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// غیرفعال کردن بیمه تکمیلی - AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeactivateSupplementaryInsurance(int patientId)
        {
            try
            {
                _logger.Information("🚫 AJAX: غیرفعال کردن بیمه تکمیلی. PatientId: {PatientId}", patientId);

                var result = await _patientInsuranceManagementService.DeactivateSupplementaryInsuranceAsync(patientId);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "بیمه تکمیلی با موفقیت غیرفعال شد"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        errors = new[] { result.Message }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در AJAX غیرفعال کردن بیمه تکمیلی. PatientId: {PatientId}", patientId);
                return Json(new
                {
                    success = false,
                    message = "خطای غیرمنتظره در غیرفعال کردن بیمه تکمیلی",
                    errors = new[] { ex.Message }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
