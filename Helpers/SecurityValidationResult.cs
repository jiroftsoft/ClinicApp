using System;
using System.Collections.Generic;
using System.Linq;
using ClinicApp.Core;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس نتیجه اعتبارسنجی امنیتی
    /// </summary>
    public class SecurityCustomValidationResult
    {
        /// <summary>
        /// آیا اعتبارسنجی موفق بوده است
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// پیام کلی اعتبارسنجی
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// سطح امنیت
        /// </summary>
        public SecurityLevel Level { get; set; }

        /// <summary>
        /// لیست خطاهای امنیتی
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// لیست هشدارهای امنیتی
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// زمان اعتبارسنجی
        /// </summary>
        public DateTime ValidatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// ایجاد نتیجه موفق
        /// </summary>
        public static SecurityCustomValidationResult Success(string message = null, SecurityLevel level = SecurityLevel.Low)
        {
            return new SecurityCustomValidationResult
            {
                IsValid = true,
                Message = message ?? "اعتبارسنجی امنیت موفق بود",
                Level = level,
                ValidatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// ایجاد نتیجه ناموفق
        /// </summary>
        public static SecurityCustomValidationResult Failed(string message, SecurityLevel level = SecurityLevel.High, params string[] errors)
        {
            return new SecurityCustomValidationResult
            {
                IsValid = false,
                Message = message,
                Level = level,
                Errors = errors?.ToList() ?? new List<string>(),
                ValidatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// اضافه کردن خطای امنیتی
        /// </summary>
        public SecurityCustomValidationResult AddSecurityError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Errors.Add(error);
                IsValid = false;
            }
            return this;
        }

        /// <summary>
        /// اضافه کردن هشدار امنیتی
        /// </summary>
        public SecurityCustomValidationResult AddSecurityWarning(string warning)
        {
            if (!string.IsNullOrEmpty(warning))
            {
                Warnings.Add(warning);
            }
            return this;
        }

        /// <summary>
        /// ترکیب نتایج اعتبارسنجی امنیتی
        /// </summary>
        public static SecurityCustomValidationResult Combine(params SecurityCustomValidationResult[] results)
        {
            var combined = new SecurityCustomValidationResult
            {
                IsValid = results.All(r => r.IsValid),
                Message = "ترکیب نتایج اعتبارسنجی امنیتی",
                Level = results.Any() ? results.Max(r => r.Level) : SecurityLevel.Low
            };

            foreach (var result in results)
            {
                combined.Errors.AddRange(result.Errors);
                combined.Warnings.AddRange(result.Warnings);
            }

            return combined;
        }

        /// <summary>
        /// تبدیل به رشته
        /// </summary>
        public override string ToString()
        {
            if (IsValid)
            {
                return $"موفق: {Message} (سطح: {Level})";
            }
            else
            {
                var errorText = Errors.Count > 0 ? string.Join(", ", Errors) : "";
                return $"ناموفق: {Message}. خطاها: {errorText} (سطح: {Level})";
            }
        }
    }
}
