using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;
using System.Web;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر عمومی خبرنامه (برای کاربران سایت)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class NewsletterController : Controller
    {
        private readonly INewsletterSubscriptionService _subscriptionService;
        private readonly INewsletterEmailService _emailService;
        private readonly INewsletterCampaignService _campaignService;
        private readonly ILogger _logger;

        public NewsletterController(
            INewsletterSubscriptionService subscriptionService,
            INewsletterEmailService emailService,
            INewsletterCampaignService campaignService)
        {
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _campaignService = campaignService ?? throw new ArgumentNullException(nameof(campaignService));
            _logger = Log.ForContext<NewsletterController>();
        }

        #region Subscribe

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Subscribe(PublicNewsletterSubscriptionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید.");
                    return RedirectToAction("Index", "Home");
                }

                var ipAddress = Request.UserHostAddress;
                var userAgent = Request.UserAgent;

                var result = await _subscriptionService.CreateSubscriptionAsync(model, ipAddress, userAgent);

                if (result.Success)
                {
                    // ارسال ایمیل تایید (Double Opt-in)
                    var emailResult = await _emailService.SendVerificationEmailAsync(result.Data);
                    if (!emailResult.Success)
                    {
                        _logger.Warning("ایمیل تایید ارسال نشد - Email: {Email}", model.Email);
                    }

                    NotificationHelper.SetSuccess(TempData, "اشتراک شما با موفقیت ثبت شد. لطفاً ایمیل خود را برای تایید بررسی کنید.");
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ثبت اشتراک - Email: {Email}", model?.Email);
                NotificationHelper.SetError(TempData, "خطا در ثبت اشتراک. لطفاً دوباره تلاش کنید.");
                return RedirectToAction("Index", "Home");
            }
        }

        #endregion

        #region Verify

        [HttpGet]
        public async Task<ActionResult> Verify(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    NotificationHelper.SetError(TempData, "Token تایید نامعتبر است");
                    return View();
                }

                var result = await _subscriptionService.VerifySubscriptionAsync(token);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تایید اشتراک - Token: {Token}", token);
                NotificationHelper.SetError(TempData, "خطا در تایید اشتراک");
                return View();
            }
        }

        #endregion

        #region Unsubscribe

        [HttpGet]
        public async Task<ActionResult> Unsubscribe(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    NotificationHelper.SetError(TempData, "Token لغو اشتراک نامعتبر است");
                    return View();
                }

                // نمایش صفحه تایید لغو اشتراک
                ViewBag.Token = token;
                return View();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه لغو اشتراک - Token: {Token}", token);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری صفحه");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Unsubscribe(string token, bool confirm)
        {
            try
            {
                if (!confirm)
                {
                    NotificationHelper.SetInfo(TempData, "لغو اشتراک لغو شد");
                    return RedirectToAction("Index", "Home");
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    NotificationHelper.SetError(TempData, "Token لغو اشتراک نامعتبر است");
                    return View();
                }

                var result = await _subscriptionService.UnsubscribeAsync(token);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو اشتراک - Token: {Token}", token);
                NotificationHelper.SetError(TempData, "خطا در لغو اشتراک");
                return View();
            }
        }

        #endregion

        #region Tracking

        [HttpGet]
        public async Task<ActionResult> TrackOpen(int campaignId, int recipientId)
        {
            try
            {
                // Tracking باز شدن ایمیل
                var result = await _campaignService.TrackEmailOpenAsync(campaignId, recipientId);

                // برگرداندن تصویر 1x1 شفاف
                var pixel = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x21, 0xF9, 0x04, 0x01, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x04, 0x01, 0x00, 0x3B };
                return File(pixel, "image/gif");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Tracking باز شدن ایمیل - CampaignId: {CampaignId}, RecipientId: {RecipientId}", 
                    campaignId, recipientId);
                // حتی در صورت خطا، تصویر را برمی‌گردانیم
                var pixel = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x21, 0xF9, 0x04, 0x01, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x04, 0x01, 0x00, 0x3B };
                return File(pixel, "image/gif");
            }
        }

        [HttpGet]
        public async Task<ActionResult> TrackClick(int campaignId, int recipientId, string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    return RedirectToAction("Index", "Home");
                }

                var decodedUrl = HttpUtility.UrlDecode(url);
                var result = await _campaignService.TrackEmailClickAsync(campaignId, recipientId, decodedUrl);

                // امنیت: فقط ریدایرکت به همان دامنه یا مسیر نسبی (جلوگیری از Open Redirect)
                if (IsSafeRedirectUrl(decodedUrl))
                {
                    return Redirect(decodedUrl);
                }
                _logger.Warning("ریدایرکت نامعتبر در TrackClick - Url: {Url}, CampaignId: {CampaignId}", decodedUrl, campaignId);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Tracking کلیک - CampaignId: {CampaignId}, RecipientId: {RecipientId}, Url: {Url}",
                    campaignId, recipientId, url);
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// فقط مسیر نسبی (/) یا همان دامنه فعلی را برای ریدایرکت مجاز می‌داند.
        /// </summary>
        private bool IsSafeRedirectUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            url = url.Trim();
            if (url.StartsWith("/", StringComparison.Ordinal) && !url.StartsWith("//", StringComparison.Ordinal))
                return true;
            try
            {
                var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri) return true;
                var requestHost = Request?.Url?.Host ?? "";
                return string.Equals(uri.Host, requestHost, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}

