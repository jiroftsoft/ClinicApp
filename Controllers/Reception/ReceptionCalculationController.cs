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
    /// کنترلر تخصصی محاسبات پذیرش - رعایت اصل SRP
    /// مسئولیت: فقط محاسبات تخصصی پذیرش
    /// </summary>
    [RoutePrefix("Reception/Calculation")]
    public class ReceptionCalculationController : BaseController
    {
        private readonly IReceptionCalculationService _receptionCalculationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ReceptionCalculationController(
            IReceptionCalculationService receptionCalculationService,
            ILogger logger,
            ICurrentUserService currentUserService) : base(logger)
        {
            _receptionCalculationService = receptionCalculationService ?? throw new ArgumentNullException(nameof(receptionCalculationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger.ForContext<ReceptionCalculationController>();
        }

        #region Reception Calculations

        /// <summary>
        /// محاسبه کامل پذیرش
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateReception(int patientId, List<int> serviceIds, DateTime receptionDate)
        {
            try
            {
                _logger.Information("🏥 RECEPTION: درخواست محاسبه پذیرش - PatientId: {PatientId}, ServiceCount: {ServiceCount}, Date: {Date}. User: {UserName}",
                    patientId, serviceIds.Count, receptionDate, _currentUserService.UserName);

                var result = await _receptionCalculationService.CalculateReceptionAsync(patientId, serviceIds, receptionDate);

                if (result.Success)
                {
                    _logger.Information("✅ محاسبه پذیرش موفق - TotalAmount: {TotalAmount}, PatientShare: {PatientShare}. User: {UserName}",
                        result.Data.TotalServiceAmount, result.Data.TotalPatientShare, _currentUserService.UserName);
                    return Json(ServiceResult<ReceptionCalculationResult>.Successful(result.Data));
                }

                _logger.Warning("⚠️ خطا در محاسبه پذیرش: {Message}. User: {UserName}", result.Message, _currentUserService.UserName);
                return Json(ServiceResult<ReceptionCalculationResult>.Failed(result.Message));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در محاسبه پذیرش - PatientId: {PatientId}. User: {UserName}",
                    patientId, _currentUserService.UserName);
                return Json(ServiceResult<ReceptionCalculationResult>.Failed("خطا در محاسبه پذیرش"));
            }
        }

        /// <summary>
        /// محاسبه یک خدمت
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateService(int patientId, int serviceId, DateTime receptionDate)
        {
            try
            {
                _logger.Information("🏥 RECEPTION: درخواست محاسبه خدمت - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName}",
                    patientId, serviceId, _currentUserService.UserName);

                var result = await _receptionCalculationService.CalculateServiceForReceptionAsync(patientId, serviceId, receptionDate);

                if (result.Success)
                {
                    _logger.Information("✅ محاسبه خدمت موفق - ServiceId: {ServiceId}, PatientShare: {PatientShare}. User: {UserName}",
                        serviceId, result.Data.PatientShare, _currentUserService.UserName);
                    return Json(ServiceResult<ViewModels.Reception.ServiceCalculationResult>.Successful(result.Data));
                }

                _logger.Warning("⚠️ خطا در محاسبه خدمت: {Message}. User: {UserName}", result.Message, _currentUserService.UserName);
                return Json(ServiceResult<ViewModels.Reception.ServiceCalculationResult>.Failed(result.Message));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در محاسبه خدمت - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName}",
                    patientId, serviceId, _currentUserService.UserName);
                return Json(ServiceResult<ViewModels.Reception.ServiceCalculationResult>.Failed("خطا در محاسبه خدمت"));
            }
        }

        /// <summary>
        /// محاسبه سریع برای نمایش در فرم
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateQuick(int patientId, int serviceId, decimal? customAmount = null)
        {
            try
            {
                _logger.Information("🏥 RECEPTION: درخواست محاسبه سریع - PatientId: {PatientId}, ServiceId: {ServiceId}, CustomAmount: {CustomAmount}. User: {UserName}",
                    patientId, serviceId, customAmount, _currentUserService.UserName);

                var result = await _receptionCalculationService.CalculateQuickReceptionAsync(patientId, serviceId, customAmount);

                if (result.Success)
                {
                    _logger.Information("✅ محاسبه سریع موفق - ServiceId: {ServiceId}, PatientShare: {PatientShare}, Coverage: {Coverage}%. User: {UserName}",
                        serviceId, result.Data.PatientShare, result.Data.CoveragePercent, _currentUserService.UserName);
                    return Json(ServiceResult<QuickReceptionCalculation>.Successful(result.Data));
                }

                _logger.Warning("⚠️ خطا در محاسبه سریع: {Message}. User: {UserName}", result.Message, _currentUserService.UserName);
                return Json(ServiceResult<QuickReceptionCalculation>.Failed(result.Message));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در محاسبه سریع - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName}",
                    patientId, serviceId, _currentUserService.UserName);
                return Json(ServiceResult<QuickReceptionCalculation>.Failed("خطا در محاسبه سریع"));
            }
        }

        #endregion
    }
}