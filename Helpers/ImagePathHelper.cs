using System;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper برای تبدیل و نرمال‌سازی مسیرهای تصویر
    /// طراحی شده برای اطمینان از سازگاری مسیرها در تمام بخش‌های سیستم
    /// </summary>
    public static class ImagePathHelper
    {
        /// <summary>
        /// تبدیل مسیر تصویر به فرمت استاندارد برای استفاده در View
        /// تبدیل ~ به / و اطمینان از شروع با /
        /// </summary>
        /// <param name="imagePath">مسیر تصویر (ممکن است با ~ یا / شروع شود)</param>
        /// <returns>مسیر نرمال‌سازی شده با شروع با /</returns>
        public static string NormalizeImagePath(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return null;
            }

            // حذف فاصله‌های اضافی
            imagePath = imagePath.Trim();

            // تبدیل ~ به / برای مسیرهای نسبی
            if (imagePath.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
            {
                imagePath = imagePath.Substring(1); // حذف ~
            }

            // اطمینان از شروع با / برای مسیرهای نسبی
            if (!imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                !imagePath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                imagePath = "/" + imagePath.TrimStart('/', '\\');
            }

            return imagePath;
        }

        /// <summary>
        /// بررسی اینکه آیا مسیر تصویر معتبر است
        /// </summary>
        /// <param name="imagePath">مسیر تصویر</param>
        /// <returns>true اگر مسیر معتبر باشد</returns>
        public static bool IsValidImagePath(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return false;
            }

            // بررسی فرمت فایل
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = System.IO.Path.GetExtension(imagePath)?.ToLowerInvariant();
            
            return !string.IsNullOrEmpty(extension) && 
                   Array.Exists(validExtensions, ext => ext == extension);
        }

        /// <summary>
        /// دریافت مسیر پیش‌فرض برای تصاویر
        /// </summary>
        /// <param name="category">دسته‌بندی تصویر (مثلاً "slider", "blog", "clinic")</param>
        /// <returns>مسیر پیش‌فرض</returns>
        public static string GetDefaultImagePath(string category = "clinic")
        {
            return $"/Content/Images/default-{category}.jpg";
        }
    }
}

