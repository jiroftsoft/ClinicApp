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
    /// کنترلر تخصصی مدیریت آمار پذیرش
    /// </summary>
    [RoutePrefix("Reception/Statistics")]
    public class ReceptionStatisticsController : BaseController
    {
        private readonly IReceptionSidebarService _receptionSidebarService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionStatisticsController(
            IReceptionSidebarService receptionSidebarService,
            ILogger logger,
            ICurrentUserService currentUserService) : base(logger)
        {
            _receptionSidebarService = receptionSidebarService ?? throw new ArgumentNullException(nameof(receptionSidebarService));
            _logger = logger.ForContext<ReceptionStatisticsController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Statistics

        /// <summary>
        /// دریافت آمار پذیرش برای سایدبار
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetStatistics()
        {
            try
            {
                _logger.Information("🏥 دریافت آمار پذیرش برای سایدبار. کاربر: {UserName}", _currentUserService.UserName);

                var result = await _receptionSidebarService.GetTodayStatisticsAsync();

                if (result.Success)
                {
                    _logger.Information("✅ آمار پذیرش دریافت شد: امروز={Today}, در انتظار={Pending}, تکمیل شده={Completed}", 
                        result.Data.TodayReceptions, result.Data.PendingReceptions, result.Data.CompletedReceptions);
                    return Json(ServiceResult<SidebarStatistics>.Successful(result.Data), JsonRequestBehavior.AllowGet);
                }

                _logger.Warning("⚠️ خطا در دریافت آمار پذیرش: {Message}", result.Message);
                return Json(ServiceResult<SidebarStatistics>.Failed(result.Message), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت آمار پذیرش");
                return Json(ServiceResult<SidebarStatistics>.Failed("خطا در دریافت آمار پذیرش"), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
