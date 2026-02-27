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
                    return ServiceResult.Failed("Campaign یا Subscription نامعتبر است");

                var phone = NormalizePhone(subscription.PhoneNumber);
                if (string.IsNullOrWhiteSpace(phone))
                    return ServiceResult.Failed("شماره تماس مشترک خالی است");
                if (!IsValidIranianMobile(phone))
                    return ServiceResult.Failed("فرمت شماره موبایل نامعتبر است (مثال: ۰۹۱۲۳۴۵۶۷۸۹)");

                // Render محتوا با Variables پیشرفته
                var variables = SmartTemplateVariableHelper.BuildAdvancedVariables(subscription);

                var renderResult = await RenderSmsContentAsync(campaign.Content, variables);
                if (!renderResult.Success)
                {
                    _logger.Warning("خطا در Render محتوای SMS - CampaignId: {CampaignId}", campaign.NewsletterCampaignId);
                    return ServiceResult.Failed("خطا در Render محتوای SMS");
                }

                var renderedContent = (renderResult.Data ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(renderedContent))
                    return ServiceResult.Failed("متن نهایی SMS خالی است");

                // محدودیت طول SMS (حداکثر 160 کاراکتر برای SMS استاندارد)
                if (renderedContent.Length > 160)
                    renderedContent = renderedContent.Substring(0, 157) + "...";

                var message = new IdentityMessage
                {
                    Destination = phone,
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
                return ServiceResult.Failed("خطا در ارسال SMS. تنظیمات درگاه پیامک و شماره گیرنده را بررسی کنید.");
            }
        }

        private static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.Length == 11 && digits.StartsWith("09")) return digits;
            if (digits.Length == 10 && digits.StartsWith("9")) return "0" + digits;
            return null;
        }

        private static bool IsValidIranianMobile(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            return (digits.Length == 11 && digits.StartsWith("09")) || (digits.Length == 10 && digits.StartsWith("9"));
        }

        public async Task<ServiceResult> SendVerificationSmsAsync(NewsletterSubscription subscription)
        {
            try
            {
                if (subscription == null)
                    return ServiceResult.Failed("Subscription نامعتبر است");
                var phone = NormalizePhone(subscription.PhoneNumber);
                if (string.IsNullOrWhiteSpace(phone) || !IsValidIranianMobile(phone))
                    return ServiceResult.Failed("شماره تماس مشترک نامعتبر است");

                var content = $"سلام {subscription.FullName ?? "کاربر گرامی"}، برای تایید اشتراک خبرنامه کلینیک شفا جیرفت، لطفاً ایمیل خود را بررسی کنید.";

                // محدودیت طول SMS
                if (content.Length > 160)
                {
                    content = content.Substring(0, 157) + "...";
                }

                var message = new IdentityMessage
                {
                    Destination = phone,
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
                var rendered = SmartTemplateRenderer.Render(content, objectVariables);

                // حذف تگ‌های HTML برای SMS
                var textContent = System.Web.HttpUtility.HtmlDecode(rendered);
                textContent = Regex.Replace(textContent, "<.*?>", string.Empty);
                textContent = Regex.Replace(textContent, @"\s+", " ").Trim();
                rendered = textContent;

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

