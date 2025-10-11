using System;
using System.Collections.Generic;
using System.Linq;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس نتیجه اعتبارسنجی
    /// </summary>
    public class CustomValidationResult
    {
        /// <summary>
        /// آیا اعتبارسنجی موفق بوده است
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// لیست خطاها
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// لیست هشدارها
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// پیام کلی
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// زمان اعتبارسنجی
        /// </summary>
        public DateTime ValidatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// ایجاد نتیجه موفق
        /// </summary>
        public static CustomValidationResult Success(string message = null)
        {
            return new CustomValidationResult
            {
                IsValid = true,
                Message = message ?? "اعتبارسنجی موفق بود",
                ValidatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// ایجاد نتیجه ناموفق
        /// </summary>
        public static CustomValidationResult Failed(string message, params string[] errors)
        {
            return new CustomValidationResult
            {
                IsValid = false,
                Message = message,
                Errors = errors?.ToList() ?? new List<string>(),
                ValidatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// اضافه کردن خطا
        /// </summary>
        public CustomValidationResult AddError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Errors.Add(error);
                IsValid = false;
            }
            return this;
        }

        /// <summary>
        /// اضافه کردن هشدار
        /// </summary>
        public CustomValidationResult AddWarning(string warning)
        {
            if (!string.IsNullOrEmpty(warning))
            {
                Warnings.Add(warning);
            }
            return this;
        }

        /// <summary>
        /// ترکیب نتایج اعتبارسنجی
        /// </summary>
        public static CustomValidationResult Combine(params CustomValidationResult[] results)
        {
            var combined = new CustomValidationResult
            {
                IsValid = results.All(r => r.IsValid),
                Message = "ترکیب نتایج اعتبارسنجی"
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
                return $"موفق: {Message}";
            }
            else
            {
                var errorText = Errors.Count > 0 ? string.Join(", ", Errors) : "";
                return $"ناموفق: {Message}. خطاها: {errorText}";
            }
        }
    }
}
