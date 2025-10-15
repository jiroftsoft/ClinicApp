using System;
using System.Collections.Generic;
using System.Linq;
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
    /// کنترلر تخصصی مدیریت هشدارهای پزشکی
    /// </summary>
    [RoutePrefix("Reception/Alert")]
    public class ReceptionAlertController : BaseController
    {
        private readonly IReceptionSidebarService _receptionSidebarService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionAlertController(
            IReceptionSidebarService receptionSidebarService,
            ILogger logger,
            ICurrentUserService currentUserService) : base(logger)
        {
            _receptionSidebarService = receptionSidebarService ?? throw new ArgumentNullException(nameof(receptionSidebarService));
            _logger = logger.ForContext<ReceptionAlertController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Medical Alerts

        /// <summary>
        /// دریافت هشدارهای پزشکی برای سایدبار
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetMedicalAlerts()
        {
            try
            {
                _logger.Information("🏥 دریافت هشدارهای پزشکی برای سایدبار. کاربر: {UserName}", _currentUserService.UserName);

                var result = await _receptionSidebarService.GetMedicalAlertsAsync();

                if (result.Success)
                {
                    _logger.Information("✅ هشدارهای پزشکی دریافت شد: تعداد={Count}", result.Data.Count);
                    return Json(ServiceResult<List<MedicalAlert>>.Successful(result.Data), JsonRequestBehavior.AllowGet);
                }

                _logger.Warning("⚠️ خطا در دریافت هشدارهای پزشکی: {Message}", result.Message);
                return Json(ServiceResult<List<MedicalAlert>>.Failed(result.Message), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت هشدارهای پزشکی");
                return Json(ServiceResult<List<MedicalAlert>>.Failed("خطا در دریافت هشدارهای پزشکی"), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
