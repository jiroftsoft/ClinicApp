using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Interfaces;
using ClinicApp.Services;
using ClinicApp.Services.Insurance;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.Insurance
{
    /// <summary>
    /// کنترلر تست محاسبه بیمه ترکیبی
    /// طراحی شده برای محیط درمانی کلینیک شفا
    /// </summary>
    [RouteArea("Admin")]
    [RoutePrefix("Insurance/CombinedInsuranceCalculationTest")]
    public class CombinedInsuranceCalculationTestController : BaseController
    {
        private readonly CombinedInsuranceCalculationTestService _testService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public CombinedInsuranceCalculationTestController(
            CombinedInsuranceCalculationTestService testService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IMessageNotificationService messageNotificationService)
            : base(messageNotificationService)
        {
            _testService = testService ?? throw new ArgumentNullException(nameof(testService));
            _log = logger.ForContext<CombinedInsuranceCalculationTestController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// صفحه اصلی تست محاسبه بیمه ترکیبی
        /// </summary>
        [HttpGet]
        [Route("")]
        public ActionResult Index()
        {
            try
            {
                _log.Information("🏥 MEDICAL: دسترسی به صفحه تست محاسبه بیمه ترکیبی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return View();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دسترسی به صفحه تست محاسبه بیمه ترکیبی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return View();
            }
        }

        /// <summary>
        /// اجرای تست‌های جامع محاسبه بیمه ترکیبی
        /// </summary>
        [HttpPost]
        [Route("RunComprehensiveTests")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RunComprehensiveTests()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست اجرای تست‌های جامع محاسبه بیمه ترکیبی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var result = await _testService.RunComprehensiveTestsAsync();
                
                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: تست‌های جامع محاسبه بیمه ترکیبی با موفقیت اجرا شدند - SuccessRate: {SuccessRate}%, TotalScenarios: {TotalScenarios}. User: {UserName} (Id: {UserId})",
                        result.Data.SuccessRate, result.Data.TestScenarios.Count, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { 
                        success = true, 
                        data = result.Data,
                        message = $"تست‌های جامع با موفقیت اجرا شدند. نرخ موفقیت: {result.Data.SuccessRate:F2}%"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در اجرای تست‌های جامع محاسبه بیمه ترکیبی - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اجرای تست‌های جامع محاسبه بیمه ترکیبی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در اجرای تست‌های جامع محاسبه بیمه ترکیبی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت گزارش تست‌های محاسبه بیمه ترکیبی
        /// </summary>
        [HttpGet]
        [Route("GetTestReport")]
        public async Task<JsonResult> GetTestReport()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست گزارش تست‌های محاسبه بیمه ترکیبی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var result = await _testService.RunComprehensiveTestsAsync();
                
                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: گزارش تست‌های محاسبه بیمه ترکیبی با موفقیت دریافت شد. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت گزارش تست‌های محاسبه بیمه ترکیبی - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت گزارش تست‌های محاسبه بیمه ترکیبی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت گزارش تست‌های محاسبه بیمه ترکیبی" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
