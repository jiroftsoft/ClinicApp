using System;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// اعتبارسنجی امنیتی URL برای جلوگیری از پروتکل‌های خطرناک (javascript:, data:, vbscript: و ...)
    /// مناسب برای ورودی‌های CMS (لینک‌ها) در محیط Production پزشکی
    /// </summary>
    public static class UrlSecurityHelper
    {
        public static bool IsSafeUrl(string url, bool allowRelative = true)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            var trimmed = url.Trim();

            // Prevent control characters / whitespace tricks
            for (int i = 0; i < trimmed.Length; i++)
            {
                if (char.IsControl(trimmed[i]))
                    return false;
            }

            // Relative URLs: /path or #anchor or ?query
            if (allowRelative && (trimmed.StartsWith("/", StringComparison.Ordinal) ||
                                  trimmed.StartsWith("#", StringComparison.Ordinal) ||
                                  trimmed.StartsWith("?", StringComparison.Ordinal)))
            {
                return !ContainsDangerousScheme(trimmed);
            }

            // Absolute URLs: must be http/https
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            {
                var scheme = uri.Scheme?.ToLowerInvariant();
                return scheme == "http" || scheme == "https";
            }

            return false;
        }

        private static bool ContainsDangerousScheme(string value)
        {
            // Normalize for scheme check
            var v = value.Trim().ToLowerInvariant();
            return v.StartsWith("javascript:", StringComparison.Ordinal) ||
                   v.StartsWith("data:", StringComparison.Ordinal) ||
                   v.StartsWith("vbscript:", StringComparison.Ordinal) ||
                   v.StartsWith("file:", StringComparison.Ordinal) ||
                   v.StartsWith("blob:", StringComparison.Ordinal);
        }
    }
}

