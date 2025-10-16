using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر لیست پذیرش‌ها - تخصصی برای ماژول پذیرش
    /// </summary>
    [RoutePrefix("Reception/ReceptionList")]
    public class ReceptionListController : BaseController
    {
        private readonly IReceptionService _receptionService;

        public ReceptionListController(
            IReceptionService receptionService,
            ILogger logger) : base(logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
        }

        /// <summary>
        /// صفحه لیست پذیرش‌ها
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                _logger.Information("🏥 نمایش صفحه لیست پذیرش‌ها");
                return View();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در نمایش صفحه لیست پذیرش‌ها");
                return View("Error");
            }
        }

        /// <summary>
        /// دریافت لیست پذیرش‌ها
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetReceptionList()
        {
            try
            {
                _logger.Information("📋 دریافت لیست پذیرش‌ها");

                // TODO: Implement reception list logic
                var result = ServiceResult<List<object>>.Successful(new List<object>(), "لیست پذیرش‌ها با موفقیت دریافت شد");

                return Json(new { 
                    success = result.Success, 
                    data = result.Data,
                    message = result.Message 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت لیست پذیرش‌ها");
                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت لیست پذیرش‌ها" 
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}