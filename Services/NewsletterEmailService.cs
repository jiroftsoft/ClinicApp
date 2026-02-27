using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Configuration;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models.Entities.CMS;
using Microsoft.AspNet.Identity;
using Serilog;
using System.Text.RegularExpressions;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس ارسال ایمیل خبرنامه
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class NewsletterEmailService : INewsletterEmailService
    {
        private readonly EmailService _emailService;
        private readonly ILogger _logger;
        private readonly string _baseUrl;

        public NewsletterEmailService(ILogger logger, IChannelConfigProvider configProvider = null)
        {
            _emailService = new EmailService(configProvider);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baseUrl = ConfigurationManager.AppSettings["SiteUrl"] ?? "http://localhost";
        }

        public async Task<ServiceResult> SendNewsletterAsync(NewsletterCampaign campaign, NewsletterSubscription subscription)
        {
            try
            {
                if (campaign == null || subscription == null)
                    return ServiceResult.Failed("Campaign یا Subscription نامعتبر است");

                var email = subscription.Email?.Trim();
                if (string.IsNullOrWhiteSpace(email))
                    return ServiceResult.Failed("آدرس ایمیل مشترک خالی است");
                if (!IsValidEmail(email))
                    return ServiceResult.Failed("فرمت آدرس ایمیل نامعتبر است");

                if (string.IsNullOrWhiteSpace(campaign.Subject))
                    return ServiceResult.Failed("عنوان کمپین خالی است");

                // Render محتوا با Variables پیشرفته
                var unsubscribeUrl = GenerateUnsubscribeUrl(subscription.UnsubscribeToken ?? string.Empty);
                var variables = SmartTemplateVariableHelper.BuildAdvancedVariables(subscription, unsubscribeUrl);

                var renderResult = await RenderContentAsync(campaign.Content, variables);
                if (!renderResult.Success)
                {
                    _logger.Warning("خطا در Render محتوا - CampaignId: {CampaignId}", campaign.NewsletterCampaignId);
                    return ServiceResult.Failed("خطا در Render محتوا");
                }

                var renderedContent = renderResult.Data;
                
                // اضافه کردن Tracking Pixel
                renderedContent = AddTrackingPixel(renderedContent, campaign.NewsletterCampaignId, subscription.NewsletterSubscriptionId);

                // Rewrite لینک‌ها برای Click Tracking
                renderedContent = RewriteLinksForTracking(renderedContent, campaign.NewsletterCampaignId, subscription.NewsletterSubscriptionId);

                // ارسال ایمیل
                var message = new IdentityMessage
                {
                    Destination = email,
                    Subject = campaign.Subject,
                    Body = renderedContent ?? string.Empty
                };

                await _emailService.SendAsync(message);

                _logger.Information("ایمیل خبرنامه ارسال شد - CampaignId: {CampaignId}, Email: {Email}", 
                    campaign.NewsletterCampaignId, subscription.Email);

                return ServiceResult.Successful("ایمیل با موفقیت ارسال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال ایمیل خبرنامه - CampaignId: {CampaignId}, Email: {Email}", 
                    campaign?.NewsletterCampaignId, subscription?.Email);
                return ServiceResult.Failed("خطا در ارسال ایمیل. لطفاً تنظیمات SMTP و آدرس گیرنده را بررسی کنید.");
            }
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length > 254) return false;
            try
            {
                return Regex.IsMatch(email,
                    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
                    RegexOptions.Compiled | RegexOptions.Singleline);
            }
            catch { return false; }
        }

        public async Task<ServiceResult> SendVerificationEmailAsync(NewsletterSubscription subscription)
        {
            try
            {
                if (subscription == null)
                    return ServiceResult.Failed("Subscription نامعتبر است");
                var email = subscription.Email?.Trim();
                if (string.IsNullOrWhiteSpace(email))
                    return ServiceResult.Failed("آدرس ایمیل مشترک خالی است");
                if (!IsValidEmail(email))
                    return ServiceResult.Failed("فرمت آدرس ایمیل نامعتبر است");
                if (string.IsNullOrWhiteSpace(subscription.VerificationToken))
                    return ServiceResult.Failed("توکن تایید اشتراک موجود نیست");

                var verificationUrl = $"{_baseUrl}/Newsletter/Verify?token={HttpUtility.UrlEncode(subscription.VerificationToken)}";

                var content = $@"
<div dir=""rtl"" style=""font-family: Vazir, Arial, sans-serif; padding: 20px; background-color: #f5f5f5;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px;"">
        <h2 style=""color: #007bff; text-align: center;"">تایید اشتراک خبرنامه</h2>
        <p style=""font-size: 16px; line-height: 1.8;"">
            سلام {subscription.FullName ?? "کاربر گرامی"}،
        </p>
        <p style=""font-size: 16px; line-height: 1.8;"">
            از اینکه در خبرنامه کلینیک شفا جیرفت ثبت‌نام کردید، متشکریم.
        </p>
        <p style=""font-size: 16px; line-height: 1.8;"">
            لطفاً برای تایید اشتراک خود، روی لینک زیر کلیک کنید:
        </p>
        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{verificationUrl}"" 
               style=""display: inline-block; padding: 12px 30px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; font-size: 16px;"">
                تایید اشتراک
            </a>
        </div>
        <p style=""font-size: 14px; color: #666; line-height: 1.8;"">
            اگر شما در خبرنامه ثبت‌نام نکرده‌اید، لطفاً این ایمیل را نادیده بگیرید.
        </p>
        <hr style=""border: none; border-top: 1px solid #eee; margin: 30px 0;"">
        <p style=""font-size: 12px; color: #999; text-align: center;"">
            کلینیک شفا جیرفت
        </p>
    </div>
</div>";

                var message = new IdentityMessage
                {
                    Destination = email,
                    Subject = "تایید اشتراک خبرنامه - کلینیک شفا جیرفت",
                    Body = content
                };

                await _emailService.SendAsync(message);

                _logger.Information("ایمیل تایید ارسال شد - Email: {Email}, SubscriptionId: {SubscriptionId}", 
                    email, subscription.NewsletterSubscriptionId);

                return ServiceResult.Successful("ایمیل تایید با موفقیت ارسال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال ایمیل تایید - Email: {Email}", subscription?.Email);
                return ServiceResult.Failed("خطا در ارسال ایمیل تایید");
            }
        }

        public async Task<ServiceResult> SendUnsubscribeConfirmationAsync(NewsletterSubscription subscription)
        {
            try
            {
                if (subscription == null)
                    return ServiceResult.Failed("Subscription نامعتبر است");
                var email = subscription.Email?.Trim();
                if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
                    return ServiceResult.Failed("آدرس ایمیل مشترک نامعتبر است");

                var content = $@"
<div dir=""rtl"" style=""font-family: Vazir, Arial, sans-serif; padding: 20px; background-color: #f5f5f5;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px;"">
        <h2 style=""color: #28a745; text-align: center;"">لغو اشتراک با موفقیت انجام شد</h2>
        <p style=""font-size: 16px; line-height: 1.8;"">
            سلام {subscription.FullName ?? "کاربر گرامی"}،
        </p>
        <p style=""font-size: 16px; line-height: 1.8;"">
            اشتراک شما در خبرنامه کلینیک شفا جیرفت با موفقیت لغو شد.
        </p>
        <p style=""font-size: 16px; line-height: 1.8;"">
            دیگر خبرنامه‌ای از ما دریافت نخواهید کرد.
        </p>
        <p style=""font-size: 14px; color: #666; line-height: 1.8;"">
            اگر این کار را شما انجام نداده‌اید، لطفاً با ما تماس بگیرید.
        </p>
        <hr style=""border: none; border-top: 1px solid #eee; margin: 30px 0;"">
        <p style=""font-size: 12px; color: #999; text-align: center;"">
            کلینیک شفا جیرفت
        </p>
    </div>
</div>";

                var message = new IdentityMessage
                {
                    Destination = email,
                    Subject = "لغو اشتراک خبرنامه - کلینیک شفا جیرفت",
                    Body = content
                };

                await _emailService.SendAsync(message);

                _logger.Information("ایمیل تایید لغو اشتراک ارسال شد - Email: {Email}", email);

                return ServiceResult.Successful("ایمیل تایید لغو اشتراک با موفقیت ارسال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال ایمیل تایید لغو اشتراک - Email: {Email}", subscription?.Email);
                return ServiceResult.Failed("خطا در ارسال ایمیل تایید لغو اشتراک");
            }
        }

        public async Task<ServiceResult<string>> RenderContentAsync(string content, Dictionary<string, string> variables)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return ServiceResult<string>.Successful(string.Empty);
                }

                // تبدیل Dictionary<string, string> به Dictionary<string, object> برای SmartTemplateRenderer
                var objectVariables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                if (variables != null)
                {
                    foreach (var kvp in variables)
                    {
                        objectVariables[kvp.Key] = kvp.Value;
                    }
                }

                // استفاده از SmartTemplateRenderer با Cache و Error Handling
                // برای Email، HTML خود Template نباید Encode شود، فقط متغیرها Encode می‌شوند
                var rendered = SmartTemplateRenderer.Render(content, objectVariables);

                return ServiceResult<string>.Successful(rendered);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Render محتوای Template");
                return ServiceResult<string>.Failed("خطا در Render محتوا");
            }
        }

        public string GenerateTrackingPixelUrl(int campaignId, int recipientId)
        {
            return $"{_baseUrl}/Newsletter/TrackOpen?campaignId={campaignId}&recipientId={recipientId}";
        }

        public string GenerateClickTrackingUrl(int campaignId, int recipientId, string originalUrl)
        {
            var encodedUrl = HttpUtility.UrlEncode(originalUrl);
            return $"{_baseUrl}/Newsletter/TrackClick?campaignId={campaignId}&recipientId={recipientId}&url={encodedUrl}";
        }

        #region Private Helper Methods

        private string AddTrackingPixel(string content, int campaignId, int recipientId)
        {
            var trackingUrl = GenerateTrackingPixelUrl(campaignId, recipientId);
            var trackingPixel = $"<img src=\"{trackingUrl}\" width=\"1\" height=\"1\" style=\"display:none;\" alt=\"\" />";
            
            // اضافه کردن Tracking Pixel قبل از </body>
            if (content.IndexOf("</body>", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                content = content.Replace("</body>", trackingPixel + "</body>");
            }
            else
            {
                // اگر تگ body وجود نداشت، در انتهای محتوا اضافه می‌کنیم
                content += trackingPixel;
            }

            return content;
        }

        private string RewriteLinksForTracking(string content, int campaignId, int recipientId)
        {
            // پیدا کردن تمام لینک‌های <a href="...">
            var linkPattern = @"<a\s+[^>]*href\s*=\s*[""']([^""']+)[""'][^>]*>";
            var matches = Regex.Matches(content, linkPattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var originalUrl = match.Groups[1].Value;
                
                // Skip کردن لینک‌های داخلی (Unsubscribe, Tracking)
                if (originalUrl.Contains("/Newsletter/") || originalUrl.StartsWith("#") || originalUrl.StartsWith("mailto:"))
                    continue;

                var trackingUrl = GenerateClickTrackingUrl(campaignId, recipientId, originalUrl);
                content = content.Replace(match.Groups[0].Value, match.Groups[0].Value.Replace(originalUrl, trackingUrl));
            }

            return content;
        }

        private string GenerateUnsubscribeUrl(string unsubscribeToken)
        {
            return $"{_baseUrl}/Newsletter/Unsubscribe?token={HttpUtility.UrlEncode(unsubscribeToken)}";
        }

        #endregion
    }
}

