using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر فرم تماس عمومی
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class ContactController : Controller
    {
        private readonly IContactFormService _contactFormService;
        private readonly ILogger _logger;

        public ContactController(IContactFormService contactFormService)
        {
            _contactFormService = contactFormService ?? throw new ArgumentNullException(nameof(contactFormService));
            _logger = Log.ForContext<ContactController>();
        }

        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                var model = new PublicContactFormViewModel();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم تماس");
                return View(new PublicContactFormViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(PublicContactFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Index", model);
                }

                // دریافت IP Address و User Agent
                string ipAddress = Request.UserHostAddress;
                if (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                {
                    ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split(',')[0].Trim();
                }

                string userAgent = Request.UserAgent;

                var result = await _contactFormService.CreateContactFormAsync(model, ipAddress, userAgent);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ارسال فرم تماس: {ErrorMessage}", result.Message);
                    ModelState.AddModelError("", result.Message);
                    return View("Index", model);
                }

                _logger.Information("فرم تماس با موفقیت ارسال شد - ContactFormId: {ContactFormId}, Email: {Email}", 
                    result.Data.ContactFormId, model.Email);

                TempData["SuccessMessage"] = "پیام شما با موفقیت ارسال شد. در اسرع وقت با شما تماس خواهیم گرفت.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال فرم تماس");
                ModelState.AddModelError("", "خطا در ارسال پیام. لطفاً دوباره تلاش کنید.");
                return View("Index", model);
            }
        }
    }
}

