using System;
using System.Drawing;
using System.Drawing.Imaging;
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
    /// سرویس آپلود و پردازش تصاویر
    /// طراحی شده برای محیط Production درمانی با رعایت اصول SRP و Security
    /// </summary>
    public class ImageUploadService : IImageUploadService
    {
        private readonly ILogger _logger;

        // Production Configuration
        private const int MaxFileSizeInMB = 5; // حداکثر 5 مگابایت
        private const int MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;
        private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const int MaxImageWidth = 4000; // حداکثر عرض تصویر
        private const int MaxImageHeight = 4000; // حداکثر ارتفاع تصویر
        private const int MinImageWidth = 100; // حداقل عرض تصویر
        private const int MinImageHeight = 100; // حداقل ارتفاع تصویر

        public ImageUploadService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// آپلود تصویر و ایجاد thumbnail خودکار
        /// </summary>
        public ServiceResult<ImageUploadResult> UploadImageWithThumbnail(
            HttpPostedFileBase file,
            string uploadPath,
            string thumbnailPath,
            int? thumbnailWidth = 300,
            int? thumbnailHeight = 300,
            int? maxWidth = null,
            int? maxHeight = null)
        {
            try
            {
                // 1. Validation
                var validationResult = ValidateImageFile(file);
                if (!validationResult.Success)
                {
                    return ServiceResult<ImageUploadResult>.Failed(validationResult.Message);
                }

                // 2. ایجاد نام فایل امن و منحصر به فرد
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var sanitizedFileName = SanitizeFileName(Path.GetFileNameWithoutExtension(file.FileName));
                var uniqueFileName = $"{sanitizedFileName}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{fileExtension}";

                // 3. مسیرهای کامل
                var uploadPhysicalPath = HostingEnvironment.MapPath(uploadPath);
                var thumbnailPhysicalPath = HostingEnvironment.MapPath(thumbnailPath);

                // 4. اطمینان از وجود پوشه‌ها
                EnsureDirectoryExists(uploadPhysicalPath);
                EnsureDirectoryExists(thumbnailPhysicalPath);

                // 5. مسیرهای کامل فایل‌ها
                var imageFullPath = Path.Combine(uploadPhysicalPath, uniqueFileName);
                var thumbnailFullPath = Path.Combine(thumbnailPhysicalPath, $"thumb_{uniqueFileName}");

                // 6. بارگذاری و پردازش تصویر
                int originalWidth = 0;
                int originalHeight = 0;
                
                using (var originalImage = Image.FromStream(file.InputStream))
                {
                    originalWidth = originalImage.Width;
                    originalHeight = originalImage.Height;

                    // بررسی ابعاد
                    if (originalImage.Width < MinImageWidth || originalImage.Height < MinImageHeight)
                    {
                        return ServiceResult<ImageUploadResult>.Failed(
                            $"ابعاد تصویر باید حداقل {MinImageWidth}x{MinImageHeight} پیکسل باشد.");
                    }

                    if (originalImage.Width > MaxImageWidth || originalImage.Height > MaxImageHeight)
                    {
                        return ServiceResult<ImageUploadResult>.Failed(
                            $"ابعاد تصویر نباید بیشتر از {MaxImageWidth}x{MaxImageHeight} پیکسل باشد.");
                    }

                    // 7. ذخیره تصویر اصلی (با resize در صورت نیاز)
                    Image processedImage = originalImage;
                    bool isProcessed = false;
                    if (maxWidth.HasValue || maxHeight.HasValue)
                    {
                        processedImage = ResizeImage(originalImage, maxWidth ?? originalImage.Width, maxHeight ?? originalImage.Height);
                        isProcessed = true;
                    }

                    SaveImage(processedImage, imageFullPath, GetImageFormat(fileExtension));

                    // 8. ایجاد و ذخیره thumbnail
                    var thumbnail = CreateThumbnail(originalImage, thumbnailWidth ?? 300, thumbnailHeight ?? 300);
                    SaveImage(thumbnail, thumbnailFullPath, GetImageFormat(fileExtension));
                    thumbnail.Dispose();

                    // 9. Dispose در صورت resize
                    if (isProcessed)
                    {
                        processedImage.Dispose();
                    }
                }

                // 10. بازگرداندن نتیجه
                var result = new ImageUploadResult
                {
                    ImageUrl = $"{uploadPath.Replace("~", "")}/{uniqueFileName}",
                    ThumbnailUrl = $"{thumbnailPath.Replace("~", "")}/thumb_{uniqueFileName}",
                    OriginalFileName = file.FileName,
                    SavedFileName = uniqueFileName,
                    FileSize = file.ContentLength,
                    ImageDimensions = $"{originalWidth}x{originalHeight}"
                };

                _logger.Information("تصویر با موفقیت آپلود شد: {ImageUrl}, Thumbnail: {ThumbnailUrl}, Size: {FileSize} bytes",
                    result.ImageUrl, result.ThumbnailUrl, result.FileSize);

                return ServiceResult<ImageUploadResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در آپلود تصویر: {FileName}", file?.FileName);
                return ServiceResult<ImageUploadResult>.Failed("خطا در آپلود تصویر. لطفاً دوباره تلاش کنید.");
            }
        }

        /// <summary>
        /// حذف تصویر و thumbnail مرتبط
        /// </summary>
        public ServiceResult<bool> DeleteImage(string imagePath, string thumbnailPath = null)
        {
            try
            {
                // حذف تصویر اصلی
                var imagePhysicalPath = HostingEnvironment.MapPath(imagePath);
                if (File.Exists(imagePhysicalPath))
                {
                    File.Delete(imagePhysicalPath);
                    _logger.Information("تصویر حذف شد: {ImagePath}", imagePath);
                }

                // حذف thumbnail
                if (!string.IsNullOrEmpty(thumbnailPath))
                {
                    var thumbnailPhysicalPath = HostingEnvironment.MapPath(thumbnailPath);
                    if (File.Exists(thumbnailPhysicalPath))
                    {
                        File.Delete(thumbnailPhysicalPath);
                        _logger.Information("Thumbnail حذف شد: {ThumbnailPath}", thumbnailPath);
                    }
                }

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تصویر: {ImagePath}", imagePath);
                return ServiceResult<bool>.Failed("خطا در حذف تصویر.");
            }
        }

        /// <summary>
        /// بررسی اعتبار فایل تصویر
        /// </summary>
        public ServiceResult<bool> ValidateImageFile(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                return ServiceResult<bool>.Failed("فایلی انتخاب نشده است.");
            }

            // بررسی اندازه فایل
            if (file.ContentLength > MaxFileSizeInBytes)
            {
                return ServiceResult<bool>.Failed(
                    $"حجم فایل نباید بیشتر از {MaxFileSizeInMB} مگابایت باشد.");
            }

            // بررسی نوع فایل (ContentType)
            var contentType = file.ContentType.ToLowerInvariant();
            if (!AllowedImageTypes.Contains(contentType))
            {
                _logger.Warning("نوع فایل نامعتبر: {ContentType}", contentType);
                return ServiceResult<bool>.Failed("فقط فایل‌های تصویری (JPG, PNG, GIF, WEBP) مجاز هستند.");
            }

            // بررسی extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                _logger.Warning("Extension نامعتبر: {Extension}", extension);
                return ServiceResult<bool>.Failed("فقط فایل‌های تصویری (JPG, PNG, GIF, WEBP) مجاز هستند.");
            }

            // بررسی signature فایل (Security)
            if (!IsValidImageFile(file))
            {
                _logger.Warning("فایل تصویر نامعتبر - Signature check failed: {FileName}", file.FileName);
                return ServiceResult<bool>.Failed("فایل تصویر نامعتبر است.");
            }

            return ServiceResult<bool>.Successful(true);
        }

        #region Private Helper Methods

        /// <summary>
        /// بررسی signature فایل برای امنیت
        /// </summary>
        private bool IsValidImageFile(HttpPostedFileBase file)
        {
            try
            {
                var buffer = new byte[12];
                file.InputStream.Read(buffer, 0, 12);
                file.InputStream.Position = 0;

                // JPEG: FF D8 FF
                if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                    return true;

                // PNG: 89 50 4E 47
                if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47)
                    return true;

                // GIF: 47 49 46 38
                if (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38)
                    return true;

                // WEBP: RIFF...WEBP
                if (buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46)
                {
                    // بررسی بیشتر برای WEBP
                    var webpBuffer = new byte[12];
                    file.InputStream.Position = 0;
                    file.InputStream.Read(webpBuffer, 0, 12);
                    file.InputStream.Position = 0;

                    var header = System.Text.Encoding.ASCII.GetString(webpBuffer, 8, 4);
                    if (header == "WEBP")
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// پاکسازی نام فایل برای امنیت
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "image";

            // حذف کاراکترهای خطرناک
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            // حذف فضاهای اضافی
            sanitized = sanitized.Trim().Replace(" ", "_");

            // محدود کردن طول
            if (sanitized.Length > 50)
                sanitized = sanitized.Substring(0, 50);

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
        /// ایجاد thumbnail
        /// </summary>
        private Image CreateThumbnail(Image originalImage, int width, int height)
        {
            // محاسبه ابعاد با حفظ نسبت
            var ratioX = (double)width / originalImage.Width;
            var ratioY = (double)height / originalImage.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(originalImage.Width * ratio);
            var newHeight = (int)(originalImage.Height * ratio);

            // ایجاد thumbnail با کیفیت بالا
            var thumbnail = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(thumbnail))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
            }

            return thumbnail;
        }

        /// <summary>
        /// Resize تصویر
        /// </summary>
        private Image ResizeImage(Image originalImage, int maxWidth, int maxHeight)
        {
            // محاسبه ابعاد جدید با حفظ نسبت
            var ratioX = (double)maxWidth / originalImage.Width;
            var ratioY = (double)maxHeight / originalImage.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(originalImage.Width * ratio);
            var newHeight = (int)(originalImage.Height * ratio);

            // اگر نیازی به resize نیست
            if (newWidth >= originalImage.Width && newHeight >= originalImage.Height)
                return originalImage;

            var resizedImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }

        /// <summary>
        /// ذخیره تصویر
        /// </summary>
        private void SaveImage(Image image, string filePath, ImageFormat format)
        {
            // تنظیمات کیفیت برای JPEG
            if (format == ImageFormat.Jpeg)
            {
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 90L); // کیفیت 90%

                var codec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == format.Guid);
                if (codec != null)
                {
                    image.Save(filePath, codec, encoderParameters);
                    encoderParameters.Dispose();
                    return;
                }
                encoderParameters.Dispose();
            }

            // برای سایر فرمت‌ها
            image.Save(filePath, format);
        }

        /// <summary>
        /// دریافت ImageFormat بر اساس extension
        /// </summary>
        private ImageFormat GetImageFormat(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".gif":
                    return ImageFormat.Gif;
                default:
                    return ImageFormat.Jpeg;
            }
        }

        #endregion
    }
}

