using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities.CMS;
using Microsoft.AspNet.Identity;
using Serilog;
using System.Text.RegularExpressions;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس ارسال SMS خبرنامه
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class NewsletterSmsService : INewsletterSmsService
    {
        private readonly AsanakSmsService _smsService;
        private readonly ILogger _logger;

        public NewsletterSmsService(ILogger logger)
        {
            _smsService = new AsanakSmsService();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult> SendNewsletterSmsAsync(NewsletterCampaign campaign, NewsletterSubscription subscription)
        {
            try
            {
                if (campaign == null || subscription == null)
                {
                    return ServiceResult.Failed("Campaign یا Subscription نامعتبر است");
                }

                if (string.IsNullOrWhiteSpace(subscription.PhoneNumber))
                {
                    return ServiceResult.Failed("شماره تماس مشترک موجود نیست");
                }

                // Render محتوا با Variables
                var variables = new Dictionary<string, string>
                {
                    { "FullName", subscription.FullName ?? "کاربر گرامی" },
                    { "Email", subscription.Email }
                };

                var renderResult = await RenderSmsContentAsync(campaign.Content, variables);
                if (!renderResult.Success)
                {
                    _logger.Warning("خطا در Render محتوای SMS - CampaignId: {CampaignId}", campaign.NewsletterCampaignId);
                    return ServiceResult.Failed("خطا در Render محتوای SMS");
                }

                var renderedContent = renderResult.Data;

                // محدودیت طول SMS (حداکثر 160 کاراکتر برای SMS استاندارد)
                if (renderedContent.Length > 160)
                {
                    renderedContent = renderedContent.Substring(0, 157) + "...";
                }

                // ارسال SMS
                var message = new IdentityMessage
                {
                    Destination = subscription.PhoneNumber,
                    Body = renderedContent
                };

                await _smsService.SendAsync(message);

                _logger.Information("SMS خبرنامه ارسال شد - CampaignId: {CampaignId}, PhoneNumber: {PhoneNumber}", 
                    campaign.NewsletterCampaignId, subscription.PhoneNumber);

                return ServiceResult.Successful("SMS با موفقیت ارسال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال SMS خبرنامه - CampaignId: {CampaignId}, PhoneNumber: {PhoneNumber}", 
                    campaign?.NewsletterCampaignId, subscription?.PhoneNumber);
                return ServiceResult.Failed("خطا در ارسال SMS");
            }
        }

        public async Task<ServiceResult> SendVerificationSmsAsync(NewsletterSubscription subscription)
        {
            try
            {
                if (subscription == null)
                {
                    return ServiceResult.Failed("Subscription نامعتبر است");
                }

                if (string.IsNullOrWhiteSpace(subscription.PhoneNumber))
                {
                    return ServiceResult.Failed("شماره تماس مشترک موجود نیست");
                }

                var content = $"سلام {subscription.FullName ?? "کاربر گرامی"}، برای تایید اشتراک خبرنامه کلینیک شفا جیرفت، لطفاً ایمیل خود را بررسی کنید.";

                // محدودیت طول SMS
                if (content.Length > 160)
                {
                    content = content.Substring(0, 157) + "...";
                }

                var message = new IdentityMessage
                {
                    Destination = subscription.PhoneNumber,
                    Body = content
                };

                await _smsService.SendAsync(message);

                _logger.Information("SMS تایید ارسال شد - PhoneNumber: {PhoneNumber}, SubscriptionId: {SubscriptionId}", 
                    subscription.PhoneNumber, subscription.NewsletterSubscriptionId);

                return ServiceResult.Successful("SMS تایید با موفقیت ارسال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال SMS تایید - PhoneNumber: {PhoneNumber}", subscription?.PhoneNumber);
                return ServiceResult.Failed("خطا در ارسال SMS تایید");
            }
        }

        public async Task<ServiceResult<string>> RenderSmsContentAsync(string content, Dictionary<string, string> variables)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return ServiceResult<string>.Successful(string.Empty);
                }

                if (variables == null || !variables.Any())
                {
                    return ServiceResult<string>.Successful(content);
                }

                // حذف تگ‌های HTML برای SMS
                var textContent = System.Web.HttpUtility.HtmlDecode(content);
                textContent = Regex.Replace(textContent, "<.*?>", string.Empty);
                textContent = Regex.Replace(textContent, @"\s+", " ").Trim();

                var rendered = textContent;

                // جایگزینی Variables با الگوی {{VariableName}}
                foreach (var variable in variables)
                {
                    var pattern = $"\\{{\\{{{variable.Key}\\}}\\}}";
                    rendered = Regex.Replace(rendered, pattern, variable.Value ?? string.Empty, RegexOptions.IgnoreCase);
                }

                return ServiceResult<string>.Successful(rendered);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Render محتوای SMS");
                return ServiceResult<string>.Failed("خطا در Render محتوای SMS");
            }
        }
    }
}

