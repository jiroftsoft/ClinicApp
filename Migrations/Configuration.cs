namespace ClinicApp.Migrations
{
    using System;
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using ClinicApp.Models.Entities.CMS;

    internal sealed class Configuration : DbMigrationsConfiguration<ClinicApp.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ClinicApp.Models.ApplicationDbContext context)
        {
            // یک‌بار: کپی مقادیر Web.config به ChannelConfigs (تنظیمات ارسال خبرنامه)
            if (!context.ChannelConfigs.Any())
            {
                var now = DateTime.UtcNow;
                string get(string key) => ConfigurationManager.AppSettings[key] ?? string.Empty;

                // ایمیل
                var emailKeys = new[] { "FromAddress", "SmtpServer", "Port", "Username", "Password", "Enabled", "EnableSsl", "MaxRetries", "TimeoutMs", "RetryBaseDelayMs" };
                var emailPrefix = "Email:";
                foreach (var key in emailKeys)
                {
                    var value = get(emailPrefix + key);
                    if (string.IsNullOrEmpty(value) && (key == "Enabled" || key == "EnableSsl")) value = key == "EnableSsl" ? "true" : "false";
                    if (string.IsNullOrEmpty(value) && (key == "MaxRetries" || key == "TimeoutMs" || key == "RetryBaseDelayMs")) value = key == "MaxRetries" ? "3" : key == "TimeoutMs" ? "15000" : "400";
                    context.ChannelConfigs.Add(new ChannelConfig { Category = ChannelConfig.Categories.Email, SettingKey = key, SettingValue = value ?? "", UpdatedAt = now });
                }

                // پیامک (آسانک) — در DB با Category = Sms ذخیره می‌شود
                var smsKeys = new[] { "Username", "Password", "SourceNumber", "Enabled", "TimeoutMs", "MaxRetries", "RetryBaseDelayMs" };
                var smsPrefix = "Asanak:";
                foreach (var key in smsKeys)
                {
                    var value = get(smsPrefix + key);
                    if (string.IsNullOrEmpty(value) && key == "Enabled") value = "true";
                    if (string.IsNullOrEmpty(value) && (key == "TimeoutMs" || key == "MaxRetries" || key == "RetryBaseDelayMs")) value = key == "TimeoutMs" ? "15000" : key == "MaxRetries" ? "3" : "400";
                    context.ChannelConfigs.Add(new ChannelConfig { Category = ChannelConfig.Categories.Sms, SettingKey = key, SettingValue = value ?? "", UpdatedAt = now });
                }

                context.SaveChanges();
            }
        }
    }
}
