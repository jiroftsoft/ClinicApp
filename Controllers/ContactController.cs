using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر فرم تماس عمومی - Production-Grade و GDPR-Compliant
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// با Rate Limiting و Anti-Spam Protection
    /// </summary>
    public class ContactController : Controller
    {
        private readonly IContactFormService _contactFormService;
        private readonly ILogger _logger;
        private const int MIN_FORM_SUBMIT_TIME_SECONDS = 2; // حداقل 2 ثانیه برای پر کردن فرم

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
                var model = new PublicContactFormViewModel
                {
                    FormStartTime = DateTime.Now // ثبت زمان شروع فرم برای Anti-Spam
                };
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
        [ContactRateLimit(maxRequests: 3, timeWindowMinutes: 15)] // حداکثر 3 درخواست در 15 دقیقه
        public async Task<ActionResult> Submit(PublicContactFormViewModel model)
        {
            try
            {
                // Anti-Spam: بررسی Honeypot Field
                if (!string.IsNullOrWhiteSpace(model.Website))
                {
                    _logger.Warning("Honeypot field filled - Possible bot submission from IP: {IpAddress}", 
                        Request.UserHostAddress);
                    ModelState.AddModelError("", "خطا در ارسال پیام. لطفاً دوباره تلاش کنید.");
                    return View("Index", model);
                }

                // Anti-Spam: بررسی زمان ارسال (اگر کمتر از 2 ثانیه = مشکوک)
                if (model.FormStartTime.HasValue)
                {
                    var submitTime = DateTime.Now;
                    var timeElapsed = (submitTime - model.FormStartTime.Value).TotalSeconds;
                    
                    if (timeElapsed < MIN_FORM_SUBMIT_TIME_SECONDS)
                    {
                        _logger.Warning("Form submitted too quickly - TimeElapsed: {TimeElapsed} seconds, IP: {IpAddress}", 
                            timeElapsed, Request.UserHostAddress);
                        ModelState.AddModelError("", "لطفاً فرم را با دقت بیشتری پر کنید.");
                        return View("Index", model);
                    }
                }

                // GDPR Compliance: بررسی Privacy Policy Acceptance
                if (!model.AcceptPrivacyPolicy)
                {
                    ModelState.AddModelError(nameof(model.AcceptPrivacyPolicy), 
                        "لطفاً سیاست حریم خصوصی را مطالعه و بپذیرید.");
                    return View("Index", model);
                }

                // GDPR Compliance: بررسی عدم ارسال اطلاعات پزشکی در پیام
                if (!string.IsNullOrWhiteSpace(model.Message))
                {
                    var medicalKeywords = new[] { "بیماری", "علائم", "درمان", "دارو", "تشخیص", "آزمایش", "نتیجه" };
                    var messageLower = model.Message.ToLower();
                    var hasMedicalInfo = Array.Exists(medicalKeywords, keyword => messageLower.Contains(keyword));
                    
                    if (hasMedicalInfo)
                    {
                        _logger.Warning("Possible medical information in contact form - Email: {Email}", model.Email);
                        // هشدار می‌دهیم اما فرم را رد نمی‌کنیم
                    }
                }

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

                _logger.Information("فرم تماس با موفقیت ارسال شد - ContactFormId: {ContactFormId}, Email: {Email}, Category: {Category}", 
                    result.Data.ContactFormId, model.Email, model.Category);

                // Redirect به Thank You Page با Tracking ID
                TempData["ContactFormId"] = result.Data.ContactFormId;
                TempData["TrackingId"] = $"CF-{result.Data.ContactFormId:D6}";
                return RedirectToAction("ThankYou", new { id = result.Data.ContactFormId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال فرم تماس");
                ModelState.AddModelError("", "خطا در ارسال پیام. لطفاً دوباره تلاش کنید.");
                return View("Index", model);
            }
        }

        /// <summary>
        /// صفحه تشکر پس از ارسال موفق فرم
        /// شامل Tracking ID و زمان تقریبی پاسخ
        /// </summary>
        [HttpGet]
        public ActionResult ThankYou(int? id)
        {
            try
            {
                var trackingId = TempData["TrackingId"] as string ?? (id.HasValue ? $"CF-{id.Value:D6}" : "نامشخص");
                var contactFormId = TempData["ContactFormId"] as int? ?? id;
                var viewModel = new ContactThankYouViewModel
                {
                    TrackingId = trackingId,
                    ContactFormId = contactFormId,
                    ResponseTime = "در ساعات کاری (شنبه تا پنجشنبه: 8:00 - 20:00)"
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه Thank You");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// صفحه جستجوی وضعیت پیام با Tracking ID
        /// </summary>
        [HttpGet]
        public ActionResult Track()
        {
            return View();
        }

        /// <summary>
        /// جستجوی وضعیت پیام با Tracking ID
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Track(string trackingId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(trackingId))
                {
                    ModelState.AddModelError("trackingId", "لطفاً شماره پیگیری را وارد کنید");
                    return View();
                }

                var result = await _contactFormService.GetContactFormByTrackingIdAsync(trackingId.Trim());
                
                if (!result.Success)
                {
                    ModelState.AddModelError("", result.Message);
                    return View();
                }

                return View("TrackResult", result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی Tracking ID - TrackingId: {TrackingId}", trackingId);
                ModelState.AddModelError("", "خطا در جستجوی پیام. لطفاً دوباره تلاش کنید.");
                return View();
            }
        }
    }
}

