using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Services;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.Insurance
{
    /// <summary>
    /// کنترلر محاسبه بیمه ترکیبی (اصلی + تکمیلی)
    /// طراحی شده برای کلینیک‌های درمانی
    /// </summary>
    [RouteArea("Admin")]
    [RoutePrefix("Insurance/CombinedCalculation")]
    public class CombinedInsuranceCalculationController : BaseController
    {
        private readonly ICombinedInsuranceCalculationService _combinedInsuranceCalculationService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public CombinedInsuranceCalculationController(
            ICombinedInsuranceCalculationService combinedInsuranceCalculationService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IMessageNotificationService messageNotificationService)
            : base(messageNotificationService)
        {
            _combinedInsuranceCalculationService = combinedInsuranceCalculationService ?? throw new ArgumentNullException(nameof(combinedInsuranceCalculationService));
            _log = logger.ForContext<CombinedInsuranceCalculationController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// صفحه اصلی محاسبه بیمه ترکیبی
        /// </summary>
        [HttpGet]
        [Route("")]
        public ActionResult Index()
        {
            _log.Information("🏥 MEDICAL: بازدید از صفحه محاسبه بیمه ترکیبی. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            return View();
        }

        /// <summary>
        /// محاسبه بیمه ترکیبی برای یک خدمت
        /// </summary>
        [HttpPost]
        [Route("Calculate")]
        public async Task<JsonResult> CalculateCombinedInsurance(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime? calculationDate = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست محاسبه بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}, Amount: {Amount}, Date: {Date}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                var effectiveDate = calculationDate ?? DateTime.Now;

                var result = await _combinedInsuranceCalculationService.CalculateCombinedInsuranceAsync(
                    patientId, serviceId, serviceAmount, effectiveDate);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی موفق - PatientId: {PatientId}, ServiceId: {ServiceId}, TotalCoverage: {TotalCoverage}, PatientShare: {PatientShare}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, result.Data.TotalInsuranceCoverage, result.Data.FinalPatientShare, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "محاسبه بیمه ترکیبی با موفقیت انجام شد"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در محاسبه بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = result.Message
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در محاسبه بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطا در محاسبه بیمه ترکیبی"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// محاسبه بیمه ترکیبی برای چندین خدمت
        /// </summary>
        [HttpPost]
        [Route("CalculateMultiple")]
        public async Task<JsonResult> CalculateCombinedInsuranceForServices(
            int patientId, 
            List<int> serviceIds, 
            List<decimal> serviceAmounts, 
            DateTime? calculationDate = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست محاسبه بیمه ترکیبی برای چندین خدمت - PatientId: {PatientId}, ServiceCount: {ServiceCount}, Date: {Date}. User: {UserName} (Id: {UserId})",
                    patientId, serviceIds?.Count ?? 0, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                if (serviceIds == null || serviceAmounts == null || serviceIds.Count != serviceAmounts.Count)
                {
                    return Json(new
                    {
                        success = false,
                        message = "لیست خدمات و مبالغ نامعتبر است"
                    }, JsonRequestBehavior.AllowGet);
                }

                var effectiveDate = calculationDate ?? DateTime.Now;

                var result = await _combinedInsuranceCalculationService.CalculateCombinedInsuranceForServicesAsync(
                    patientId, serviceIds, serviceAmounts, effectiveDate);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی برای چندین خدمت موفق - PatientId: {PatientId}, SuccessCount: {SuccessCount}, TotalCount: {TotalCount}. User: {UserName} (Id: {UserId})",
                        patientId, result.Data.Count, serviceIds.Count, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = $"محاسبه بیمه ترکیبی برای {result.Data.Count} خدمت با موفقیت انجام شد"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در محاسبه بیمه ترکیبی برای چندین خدمت - PatientId: {PatientId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        patientId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = result.Message
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در محاسبه بیمه ترکیبی برای چندین خدمت - PatientId: {PatientId}, ServiceCount: {ServiceCount}. User: {UserName} (Id: {UserId})",
                    patientId, serviceIds?.Count ?? 0, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطا در محاسبه بیمه ترکیبی برای چندین خدمت"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت اطلاعات بیمه‌های بیمار
        /// </summary>
        [HttpGet]
        [Route("PatientInsurances/{patientId}")]
        public async Task<JsonResult> GetPatientInsurances(int patientId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست دریافت اطلاعات بیمه‌های بیمار - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // این متد باید در PatientInsuranceService پیاده‌سازی شود
                // فعلاً یک پاسخ نمونه برمی‌گردانیم
                var sampleData = new
                {
                    primaryInsurance = new
                    {
                        id = 1,
                        name = "تامین اجتماعی",
                        coveragePercent = 70,
                        isActive = true
                    },
                    supplementaryInsurances = new[]
                    {
                        new
                        {
                            id = 2,
                            name = "بیمه تکمیلی پارسیان",
                            coveragePercent = 20,
                            isActive = true
                        }
                    }
                };

                return Json(new
                {
                    success = true,
                    data = sampleData,
                    message = "اطلاعات بیمه‌های بیمار دریافت شد"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت اطلاعات بیمه‌های بیمار - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات بیمه‌های بیمار"
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
