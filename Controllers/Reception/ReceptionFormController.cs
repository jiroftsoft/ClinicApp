using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر تخصصی مدیریت فرم پذیرش - رعایت اصل SRP
    /// مسئولیت: فقط مدیریت فرم پذیرش
    /// </summary>
    [RoutePrefix("Reception/Form")]
    public class ReceptionFormController : BaseController
    {
        private readonly IReceptionFormService _receptionFormService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ReceptionFormController(
            IReceptionFormService receptionFormService,
            ILogger logger,
            ICurrentUserService currentUserService) : base(logger)
        {
            _receptionFormService = receptionFormService ?? throw new ArgumentNullException(nameof(receptionFormService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger.ForContext<ReceptionFormController>();
        }

        #region Reception Form Management

        /// <summary>
        /// ایجاد پذیرش از فرم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateReception(ReceptionFormViewModel model)
        {
            try
            {
                _logger.Information("🏥 RECEPTION_FORM: درخواست ایجاد پذیرش از فرم - PatientId: {PatientId}, ServiceCount: {ServiceCount}. User: {UserName}",
                    model.PatientId, model.SelectedServices?.Count ?? 0, _currentUserService.UserName);

                var result = await _receptionFormService.CreateReceptionFromFormAsync(model);

                if (result.Success)
                {
                    _logger.Information("✅ پذیرش با موفقیت ایجاد شد - ReceptionId: {ReceptionId}, TotalAmount: {TotalAmount}. User: {UserName}",
                        result.Data.ReceptionId, result.Data.TotalAmount, _currentUserService.UserName);
                    return Json(ServiceResult<ReceptionFormResult>.Successful(result.Data));
                }

                _logger.Warning("⚠️ خطا در ایجاد پذیرش: {Message}. User: {UserName}", result.Message, _currentUserService.UserName);
                return Json(ServiceResult<ReceptionFormResult>.Failed(result.Message));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ایجاد پذیرش از فرم - PatientId: {PatientId}. User: {UserName}",
                    model.PatientId, _currentUserService.UserName);
                return Json(ServiceResult<ReceptionFormResult>.Failed("خطا در ایجاد پذیرش"));
            }
        }

        /// <summary>
        /// محاسبه فرم پذیرش
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateForm(ReceptionFormCalculationRequest request)
        {
            try
            {
                _logger.Information("🏥 RECEPTION_FORM: درخواست محاسبه فرم - PatientId: {PatientId}, ServiceCount: {ServiceCount}. User: {UserName}",
                    request.PatientId, request.ServiceIds?.Count ?? 0, _currentUserService.UserName);

                var result = await _receptionFormService.CalculateReceptionFormAsync(request);

                if (result.Success)
                {
                    _logger.Information("✅ محاسبه فرم موفق - TotalAmount: {TotalAmount}, PatientShare: {PatientShare}. User: {UserName}",
                        result.Data.TotalServiceAmount, result.Data.TotalPatientShare, _currentUserService.UserName);
                    return Json(ServiceResult<ReceptionFormCalculation>.Successful(result.Data));
                }

                _logger.Warning("⚠️ خطا در محاسبه فرم: {Message}. User: {UserName}", result.Message, _currentUserService.UserName);
                return Json(ServiceResult<ReceptionFormCalculation>.Failed(result.Message));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در محاسبه فرم - PatientId: {PatientId}. User: {UserName}",
                    request.PatientId, _currentUserService.UserName);
                return Json(ServiceResult<ReceptionFormCalculation>.Failed("خطا در محاسبه فرم"));
            }
        }

        /// <summary>
        /// دریافت اطلاعات فرم پذیرش
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetFormInfo(int patientId)
        {
            try
            {
                _logger.Information("🏥 RECEPTION_FORM: درخواست اطلاعات فرم - PatientId: {PatientId}. User: {UserName}",
                    patientId, _currentUserService.UserName);

                var result = await _receptionFormService.GetReceptionFormInfoAsync(patientId);

                if (result.Success)
                {
                    _logger.Information("✅ اطلاعات فرم دریافت شد - PatientName: {PatientName}. User: {UserName}",
                        result.Data.PatientName, _currentUserService.UserName);
                    return Json(ServiceResult<ReceptionFormInfo>.Successful(result.Data), JsonRequestBehavior.AllowGet);
                }

                _logger.Warning("⚠️ خطا در دریافت اطلاعات فرم: {Message}. User: {UserName}", result.Message, _currentUserService.UserName);
                return Json(ServiceResult<ReceptionFormInfo>.Failed(result.Message), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت اطلاعات فرم - PatientId: {PatientId}. User: {UserName}",
                    patientId, _currentUserService.UserName);
                return Json(ServiceResult<ReceptionFormInfo>.Failed("خطا در دریافت اطلاعات فرم"), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}