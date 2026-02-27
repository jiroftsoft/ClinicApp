using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// پروداکشن: اول از DB، در صورت نبود از Web.config. کش به‌ازای هر دسته.
    /// </summary>
    public class ChannelConfigProviderService : IChannelConfigProvider
    {
        private readonly IChannelConfigRepository _repo;
        private static readonly object CacheLock = new object();
        private static readonly Dictionary<string, Dictionary<string, string>> CategoryCache = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public ChannelConfigProviderService(IChannelConfigRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        /// <summary>
        /// همگام — فقط از کش یا Web.config می‌خواند تا در سازنده‌ها deadlock نشود.
        /// اگر کش خالی باشد مستقیم از Web.config برمی‌گرداند (بدون فراخوانی DB).
        /// </summary>
        public string GetValue(string fullKey)
        {
            if (string.IsNullOrWhiteSpace(fullKey)) return null;
            var (category, key) = ParseFullKey(fullKey);
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(key)) return FallbackConfig(fullKey);

            lock (CacheLock)
            {
                if (CategoryCache.TryGetValue(category, out var cached) && cached.TryGetValue(key, out var val))
                    return val;
            }
            return FallbackConfig(fullKey);
        }

        public async Task<string> GetValueAsync(string fullKey)
        {
            if (string.IsNullOrWhiteSpace(fullKey)) return null;
            var (category, key) = ParseFullKey(fullKey);
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(key)) return FallbackConfig(fullKey);

            var dict = await GetCategoryMergedAsync(category).ConfigureAwait(false);
            return dict.TryGetValue(key, out var val) ? val : FallbackConfig(fullKey);
        }

        public void InvalidateCache()
        {
            lock (CacheLock)
            {
                CategoryCache.Clear();
            }
        }

        private static (string category, string key) ParseFullKey(string fullKey)
        {
            var colon = fullKey.IndexOf(':');
            if (colon <= 0 || colon == fullKey.Length - 1)
                return (null, null);
            var category = fullKey.Substring(0, colon).Trim();
            var key = fullKey.Substring(colon + 1).Trim();
            if (string.Equals(category, "Asanak", StringComparison.OrdinalIgnoreCase))
                category = ChannelConfig.Categories.Sms;
            return (category, key);
        }

        private async Task<Dictionary<string, string>> GetCategoryMergedAsync(string category)
        {
            lock (CacheLock)
            {
                if (CategoryCache.TryGetValue(category, out var cached))
                    return cached;
            }

            var fromDb = await _repo.GetByCategoryAsync(category).ConfigureAwait(false);
            var prefix = category == ChannelConfig.Categories.Sms ? "Asanak:" : "Email:";
            var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var configKeys = new[] { "FromAddress", "SmtpServer", "Port", "Username", "Password", "Enabled", "EnableSsl", "MaxRetries", "TimeoutMs", "RetryBaseDelayMs" };
            var smsKeys = new[] { "Username", "Password", "SourceNumber", "Enabled", "TimeoutMs", "MaxRetries", "RetryBaseDelayMs" };
            var keys = category == ChannelConfig.Categories.Sms ? smsKeys : configKeys;

            foreach (var k in keys)
            {
                var fullKey = prefix + k;
                if (fromDb.TryGetValue(k, out var dbVal) && !string.IsNullOrEmpty(dbVal))
                    merged[k] = dbVal;
                else
                    merged[k] = ConfigurationManager.AppSettings[fullKey] ?? string.Empty;
            }

            lock (CacheLock)
            {
                CategoryCache[category] = merged;
            }

            return merged;
        }

        private static string FallbackConfig(string fullKey)
        {
            return ConfigurationManager.AppSettings[fullKey] ?? string.Empty;
        }
    }
}
