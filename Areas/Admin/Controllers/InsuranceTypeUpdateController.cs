using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Models;
using ClinicApp.Services.DataSeeding;
using ClinicApp.ViewModels.InsuranceTypeUpdate;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت به‌روزرسانی مقادیر InsuranceType
    /// </summary>
    public class InsuranceTypeUpdateController : Controller
    {
        private readonly InsuranceTypeUpdateService _insuranceTypeUpdateService;
        private readonly ILogger _logger;

        public InsuranceTypeUpdateController(InsuranceTypeUpdateService insuranceTypeUpdateService)
        {
            _insuranceTypeUpdateService = insuranceTypeUpdateService;
            _logger = Log.ForContext<InsuranceTypeUpdateController>();
        }

        /// <summary>
        /// صفحه اصلی مدیریت به‌روزرسانی InsuranceType
        /// </summary>
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.Information("🔧 INSURANCE_TYPE_UPDATE: نمایش صفحه مدیریت به‌روزرسانی InsuranceType");

                var viewModel = new InsuranceTypeUpdateIndexViewModel
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    SuccessMessage = TempData["SuccessMessage"] as string,
                    ErrorMessage = TempData["ErrorMessage"] as string,
                    WarningMessage = TempData["WarningMessage"] as string
                };

                var statisticsResult = await _insuranceTypeUpdateService.GetUpdateStatisticsAsync();
                
                if (statisticsResult.Success && statisticsResult.Data != null)
                {
                    viewModel.Statistics = new InsuranceTypeStatistics
                    {
                        TotalPlans = GetIntValue(statisticsResult.Data, "TotalPlans"),
                        PrimaryPlans = GetIntValue(statisticsResult.Data, "PrimaryPlans"),
                        SupplementaryPlans = GetIntValue(statisticsResult.Data, "SupplementaryPlans"),
                        InvalidPlans = GetIntValue(statisticsResult.Data, "InvalidPlans")
                    };
                    viewModel.NeedsUpdate = GetBoolValue(statisticsResult.Data, "NeedsUpdate");
                    _logger.Information("🔧 INSURANCE_TYPE_UPDATE: آمار با موفقیت بارگیری شد");
                }
                else
                {
                    viewModel.Statistics = new InsuranceTypeStatistics();
                    viewModel.NeedsUpdate = true;
                    _logger.Warning("🔧 INSURANCE_TYPE_UPDATE: خطا در بارگیری آمار");
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🔧 INSURANCE_TYPE_UPDATE: خطا در نمایش صفحه مدیریت");
                
                var errorViewModel = new InsuranceTypeUpdateIndexViewModel
                {
                    ErrorMessage = "خطا در بارگیری اطلاعات. لطفاً دوباره تلاش کنید.",
                    Statistics = new InsuranceTypeStatistics()
                };
                
                return View(errorViewModel);
            }
        }

        /// <summary>
        /// Helper method برای دریافت مقدار int از Dictionary
        /// </summary>
        private int GetIntValue(Dictionary<string, object> data, string key)
        {
            if (data != null && data.ContainsKey(key) && data[key] is int value)
                return value;
            return 0;
        }

        /// <summary>
        /// Helper method برای دریافت مقدار bool از Dictionary
        /// </summary>
        private bool GetBoolValue(Dictionary<string, object> data, string key)
        {
            if (data != null && data.ContainsKey(key) && data[key] is bool value)
                return value;
            return false;
        }

        /// <summary>
        /// اجرای به‌روزرسانی مقادیر InsuranceType
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateInsuranceTypeValues()
        {
            try
            {
                _logger.Information("🔧 INSURANCE_TYPE_UPDATE: شروع به‌روزرسانی مقادیر InsuranceType");

                var result = await _insuranceTypeUpdateService.UpdateInsuranceTypeValuesAsync();

                if (result.Success)
                {
                    _logger.Information("🔧 INSURANCE_TYPE_UPDATE: به‌روزرسانی با موفقیت انجام شد");
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    _logger.Warning("🔧 INSURANCE_TYPE_UPDATE: خطا در به‌روزرسانی - {Error}", result.Message);
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🔧 INSURANCE_TYPE_UPDATE: خطای کلی در به‌روزرسانی");
                TempData["ErrorMessage"] = "خطای سیستمی در به‌روزرسانی. لطفاً با مدیر سیستم تماس بگیرید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// دریافت آمار به‌روزرسانی (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetStatistics()
        {
            try
            {
                _logger.Debug("🔧 INSURANCE_TYPE_UPDATE: درخواست آمار به‌روزرسانی");

                var result = await _insuranceTypeUpdateService.GetUpdateStatisticsAsync();

                if (result.Success)
                {
                    _logger.Debug("🔧 INSURANCE_TYPE_UPDATE: آمار با موفقیت دریافت شد");
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning("🔧 INSURANCE_TYPE_UPDATE: خطا در دریافت آمار - {Error}", result.Message);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🔧 INSURANCE_TYPE_UPDATE: خطای کلی در دریافت آمار");
                return Json(new { success = false, message = "خطای سیستمی در دریافت آمار" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
