using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Core;
using ClinicApp.Services.UserContext;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// تنظیمات ارسال خبرنامه (ایمیل/SMS) — خواندن و ذخیره در DB با fallback به Web.config (پروداکشن).
    /// </summary>
    [Authorize(Roles = AppRoles.Admin)]
    public class NewsletterSettingsController : BaseCMSController
    {
        private readonly IChannelConfigProvider _configProvider;
        private readonly IChannelConfigRepository _configRepo;
        private readonly ICurrentUserService _currentUser;

        public NewsletterSettingsController(
            IChannelConfigProvider configProvider,
            IChannelConfigRepository configRepo,
            ICurrentUserService currentUser)
        {
            _configProvider = configProvider;
            _configRepo = configRepo;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var model = await BuildEditViewModelAsync();
            return View(GetViewPath("Index"), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(NewsletterSettingsEditViewModel model)
        {
            if (model == null)
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(model.EmailFromAddress) || string.IsNullOrWhiteSpace(model.EmailSmtpServer) || string.IsNullOrWhiteSpace(model.EmailPort))
            {
                NotificationHelper.SetError(TempData, "فیلدهای الزامی ایمیل (آدرس فرستنده، سرور SMTP، پورت) را پر کنید.", "اعتبارسنجی");
                return View(GetViewPath("Index"), await BuildEditViewModelAsync(model));
            }

            var userId = _currentUser?.UserId;

            var emailDict = new Dictionary<string, string>
            {
                { "FromAddress", model.EmailFromAddress?.Trim() ?? "" },
                { "NoReplyDisplayName", model.EmailNoReplyDisplayName?.Trim() ?? "" },
                { "SubjectPrefix", model.EmailSubjectPrefix?.Trim() ?? "" },
                { "BccAddresses", model.EmailBccAddresses?.Trim() ?? "" },
                { "SmtpServer", model.EmailSmtpServer?.Trim() ?? "" },
                { "Port", model.EmailPort?.Trim() ?? "" },
                { "Username", model.EmailUsername?.Trim() ?? "" },
                { "Enabled", model.EmailEnabled ? "true" : "false" },
                { "EnableSsl", model.EmailEnableSsl ? "true" : "false" },
                { "MaxRetries", model.EmailMaxRetries.ToString() },
                { "TimeoutMs", model.EmailTimeoutMs.ToString() }
            };
            if (!string.IsNullOrWhiteSpace(model.EmailPassword))
                emailDict["Password"] = model.EmailPassword;

            var smsDict = new Dictionary<string, string>
            {
                { "Username", model.SmsUsername?.Trim() ?? "" },
                { "SourceNumber", model.SmsSourceNumber?.Trim() ?? "" },
                { "Enabled", model.SmsEnabled ? "true" : "false" },
                { "TimeoutMs", model.SmsTimeoutMs.ToString() },
                { "MaxRetries", model.SmsMaxRetries.ToString() }
            };
            if (!string.IsNullOrWhiteSpace(model.SmsPassword))
                smsDict["Password"] = model.SmsPassword;

            try
            {
                await _configRepo.SetBulkAsync(ChannelConfig.Categories.Email, emailDict, userId);
                await _configRepo.SetBulkAsync(ChannelConfig.Categories.Sms, smsDict, userId);
                _configProvider.InvalidateCache();
                NotificationHelper.SetSuccess(TempData, "تنظیمات با موفقیت ذخیره شد. ارسال ایمیل و پیامک از همین مقادیر استفاده می‌کند.", "ذخیره تنظیمات");
            }
            catch (Exception)
            {
                NotificationHelper.SetError(TempData, "خطا در ذخیره تنظیمات. دوباره تلاش کنید.", "خطا");
                return View(GetViewPath("Index"), await BuildEditViewModelAsync(model));
            }

            return RedirectToAction("Index");
        }

        private async Task<NewsletterSettingsEditViewModel> BuildEditViewModelAsync(NewsletterSettingsEditViewModel existing = null)
        {
            if (existing != null)
                return existing;

            var model = new NewsletterSettingsEditViewModel
            {
                EmailFromAddress = _configProvider.GetValue("Email:FromAddress"),
                EmailNoReplyDisplayName = _configProvider.GetValue("Email:NoReplyDisplayName"),
                EmailSubjectPrefix = _configProvider.GetValue("Email:SubjectPrefix"),
                EmailBccAddresses = _configProvider.GetValue("Email:BccAddresses"),
                EmailSmtpServer = _configProvider.GetValue("Email:SmtpServer"),
                EmailPort = _configProvider.GetValue("Email:Port"),
                EmailUsername = _configProvider.GetValue("Email:Username"),
                EmailPassword = "",
                EmailEnabled = "true".Equals(_configProvider.GetValue("Email:Enabled"), StringComparison.OrdinalIgnoreCase),
                EmailEnableSsl = "true".Equals(_configProvider.GetValue("Email:EnableSsl"), StringComparison.OrdinalIgnoreCase),
                EmailMaxRetries = int.TryParse(_configProvider.GetValue("Email:MaxRetries"), out var er) ? er : 3,
                EmailTimeoutMs = int.TryParse(_configProvider.GetValue("Email:TimeoutMs"), out var et) ? et : 15000,
                SmsUsername = _configProvider.GetValue("Asanak:Username"),
                SmsPassword = "",
                SmsSourceNumber = _configProvider.GetValue("Asanak:SourceNumber"),
                SmsEnabled = "true".Equals(_configProvider.GetValue("Asanak:Enabled"), StringComparison.OrdinalIgnoreCase),
                SmsTimeoutMs = int.TryParse(_configProvider.GetValue("Asanak:TimeoutMs"), out var st) ? st : 15000,
                SmsMaxRetries = int.TryParse(_configProvider.GetValue("Asanak:MaxRetries"), out var sr) ? sr : 3
            };
            return await Task.FromResult(model);
        }
    }
}
