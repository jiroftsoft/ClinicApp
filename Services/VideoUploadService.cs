using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس آپلود و پردازش ویدیوها
    /// طراحی شده برای محیط Production درمانی با رعایت اصول SRP و Security
    /// </summary>
    public class VideoUploadService : IVideoUploadService
    {
        private readonly ILogger _logger;

        // Production Configuration - برای محیط درمانی
        private const int MaxFileSizeInMB = 100; // حداکثر 100 مگابایت برای ویدیو
        private const int MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;
        private const int MinFileSizeInBytes = 1024; // حداقل 1 کیلوبایت
        
        // فرمت‌های مجاز ویدیو (Production-Ready)
        private static readonly string[] AllowedVideoTypes = 
        {
            "video/mp4",
            "video/webm",
            "video/ogg",
            "video/quicktime", // MOV
            "video/x-msvideo" // AVI
        };
        
        private static readonly string[] AllowedExtensions = 
        {
            ".mp4",
            ".webm",
            ".ogg",
            ".mov",
            ".avi"
        };

        public VideoUploadService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// آپلود ویدیو
        /// </summary>
        public ServiceResult<VideoUploadResult> UploadVideo(
            HttpPostedFileBase file,
            string uploadPath)
        {
            try
            {
                // 1. Validation
                var validationResult = ValidateVideoFile(file);
                if (!validationResult.Success)
                {
                    return ServiceResult<VideoUploadResult>.Failed(validationResult.Message);
                }

                // 2. ایجاد نام فایل امن و منحصر به فرد
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var sanitizedFileName = SanitizeFileName(Path.GetFileNameWithoutExtension(file.FileName));
                var uniqueFileName = $"{sanitizedFileName}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{fileExtension}";

                // 3. مسیر کامل آپلود
                var uploadPhysicalPath = HostingEnvironment.MapPath(uploadPath);
                if (string.IsNullOrEmpty(uploadPhysicalPath))
                {
                    _logger.Error("مسیر آپلود نامعتبر: {UploadPath}", uploadPath);
                    return ServiceResult<VideoUploadResult>.Failed("مسیر آپلود نامعتبر است.");
                }

                // 4. اطمینان از وجود پوشه
                EnsureDirectoryExists(uploadPhysicalPath);

                // 5. مسیر کامل فایل
                var videoFullPath = Path.Combine(uploadPhysicalPath, uniqueFileName);

                // 6. ذخیره ویدیو
                try
                {
                    file.SaveAs(videoFullPath);
                    _logger.Information("ویدیو با موفقیت آپلود شد: {FileName}, Size: {FileSize} bytes", 
                        uniqueFileName, file.ContentLength);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "خطا در ذخیره ویدیو: {FileName}", uniqueFileName);
                    return ServiceResult<VideoUploadResult>.Failed("خطا در ذخیره ویدیو. لطفاً دوباره تلاش کنید.");
                }

                // 7. ساخت URL نسبی
                var videoUrl = uploadPath.TrimStart('~') + "/" + uniqueFileName;

                // 8. فرمت کردن حجم فایل
                var fileSizeFormatted = FormatFileSize(file.ContentLength);

                // 9. نتیجه
                var result = new VideoUploadResult
                {
                    VideoUrl = videoUrl,
                    VideoFileName = uniqueFileName,
                    FileSizeInBytes = file.ContentLength,
                    FileSizeFormatted = fileSizeFormatted,
                    ContentType = file.ContentType
                };

                return ServiceResult<VideoUploadResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در آپلود ویدیو");
                return ServiceResult<VideoUploadResult>.Failed("خطا در آپلود ویدیو. لطفاً دوباره تلاش کنید.");
            }
        }

        /// <summary>
        /// حذف ویدیو
        /// </summary>
        public ServiceResult<bool> DeleteVideo(string videoPath)
        {
            try
            {
                if (string.IsNullOrEmpty(videoPath))
                {
                    return ServiceResult<bool>.Failed("مسیر ویدیو نامعتبر است.");
                }

                var physicalPath = HostingEnvironment.MapPath(videoPath);
                if (string.IsNullOrEmpty(physicalPath) || !File.Exists(physicalPath))
                {
                    _logger.Warning("فایل ویدیو یافت نشد: {VideoPath}", videoPath);
                    return ServiceResult<bool>.Successful(true); // اگر فایل وجود ندارد، موفق در نظر گرفته می‌شود
                }

                File.Delete(physicalPath);
                _logger.Information("ویدیو با موفقیت حذف شد: {VideoPath}", videoPath);
                
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف ویدیو: {VideoPath}", videoPath);
                return ServiceResult<bool>.Failed("خطا در حذف ویدیو.");
            }
        }

        /// <summary>
        /// بررسی اعتبار فایل ویدیو
        /// </summary>
        public ServiceResult<bool> ValidateVideoFile(HttpPostedFileBase file)
        {
            try
            {
                // 1. بررسی وجود فایل
                if (file == null || file.ContentLength == 0)
                {
                    return ServiceResult<bool>.Failed("فایل ویدیو انتخاب نشده است.");
                }

                // 2. بررسی حجم فایل
                if (file.ContentLength > MaxFileSizeInBytes)
                {
                    return ServiceResult<bool>.Failed(
                        $"حجم فایل ویدیو نباید بیشتر از {MaxFileSizeInMB} مگابایت باشد.");
                }

                if (file.ContentLength < MinFileSizeInBytes)
                {
                    return ServiceResult<bool>.Failed("فایل ویدیو خالی یا نامعتبر است.");
                }

                // 3. بررسی پسوند فایل
                var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(fileExtension) || !AllowedExtensions.Contains(fileExtension))
                {
                    return ServiceResult<bool>.Failed(
                        $"فرمت فایل مجاز نیست. فرمت‌های مجاز: {string.Join(", ", AllowedExtensions)}");
                }

                // 4. بررسی Content Type
                if (string.IsNullOrEmpty(file.ContentType) || 
                    !AllowedVideoTypes.Contains(file.ContentType.ToLowerInvariant()))
                {
                    // اگر ContentType نامعتبر بود، فقط بر اساس پسوند بررسی می‌کنیم
                    _logger.Warning("ContentType نامعتبر برای فایل: {FileName}, ContentType: {ContentType}", 
                        file.FileName, file.ContentType);
                }

                // 5. بررسی نام فایل (امنیت)
                var fileName = Path.GetFileName(file.FileName);
                if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                {
                    return ServiceResult<bool>.Failed("نام فایل نامعتبر است.");
                }

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی اعتبار فایل ویدیو");
                return ServiceResult<bool>.Failed("خطا در بررسی فایل ویدیو.");
            }
        }

        #region Helper Methods

        /// <summary>
        /// پاکسازی نام فایل از کاراکترهای خطرناک
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "video";

            // حذف کاراکترهای خطرناک
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName
                .Where(c => !invalidChars.Contains(c))
                .ToArray());

            // محدود کردن طول نام فایل
            if (sanitized.Length > 100)
            {
                sanitized = sanitized.Substring(0, 100);
            }

            // اگر خالی شد، نام پیش‌فرض
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                sanitized = "video";
            }

            return sanitized;
        }

        /// <summary>
        /// اطمینان از وجود پوشه
        /// </summary>
        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                _logger.Information("پوشه ایجاد شد: {Path}", path);
            }
        }

        /// <summary>
        /// فرمت کردن حجم فایل
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        #endregion
    }
}

