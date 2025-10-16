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
    /// کنترلر جستجوی بیمار - تخصصی برای ماژول پذیرش
    /// </summary>
    [RoutePrefix("Reception/PatientSearch")]
    public class ReceptionPatientSearchController : BaseController
    {
        private readonly IPatientService _patientService;

        public ReceptionPatientSearchController(
            IPatientService patientService,
            ILogger logger) : base(logger)
        {
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
        }

        /// <summary>
        /// صفحه جستجوی بیمار
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                _logger.Information("🏥 نمایش صفحه جستجوی بیمار");
                return View();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در نمایش صفحه جستجوی بیمار");
                return View("Error");
            }
        }

        /// <summary>
        /// جستجوی بیماران
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> SearchPatients(string nationalCode, string firstName, string lastName, string phoneNumber)
        {
            try
            {
                _logger.Information("🔍 جستجوی بیماران - کد ملی: {NationalCode}, نام: {FirstName} {LastName}", 
                    nationalCode, firstName, lastName);

                // TODO: Implement patient search logic
                var result = ServiceResult<List<object>>.Successful(new List<object>(), "جستجو با موفقیت انجام شد");

                return Json(new { 
                    success = result.Success, 
                    data = result.Data,
                    message = result.Message 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در جستجوی بیماران");
                return Json(new { 
                    success = false, 
                    message = "خطا در جستجوی بیماران" 
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}