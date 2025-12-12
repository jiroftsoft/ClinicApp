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
    /// سرویس آپلود و پردازش فایل‌های مستندات (PDF, Word, Excel)
    /// طراحی شده برای محیط Production درمانی با رعایت اصول SRP و Security
    /// </summary>
    public class DocumentUploadService : IDocumentUploadService
    {
        private readonly ILogger _logger;

        // Production Configuration
        private const int MaxFileSizeInMB = 10; // حداکثر 10 مگابایت
        private const int MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;
        private static readonly string[] AllowedDocumentTypes = 
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // DOCX
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" // XLSX
        };
        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

        public DocumentUploadService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ServiceResult<DocumentUploadResult> UploadDocument(HttpPostedFileBase file, string uploadPath)
        {
            try
            {
                if (file == null || file.ContentLength == 0)
                {
                    return ServiceResult<DocumentUploadResult>.Failed("فایلی انتخاب نشده است.");
                }

                // Validation
                var validationResult = ValidateDocumentFile(file);
                if (!validationResult.Success)
                {
                    return ServiceResult<DocumentUploadResult>.Failed(validationResult.Message);
                }

                // ایجاد مسیر در صورت عدم وجود
                var physicalPath = HostingEnvironment.MapPath(uploadPath);
                if (string.IsNullOrEmpty(physicalPath))
                {
                    _logger.Error("مسیر آپلود نامعتبر: {UploadPath}", uploadPath);
                    return ServiceResult<DocumentUploadResult>.Failed("مسیر آپلود نامعتبر است.");
                }

                if (!Directory.Exists(physicalPath))
                {
                    Directory.CreateDirectory(physicalPath);
                    _logger.Information("پوشه آپلود ایجاد شد: {PhysicalPath}", physicalPath);
                }

                // تولید نام فایل یکتا
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(physicalPath, uniqueFileName);

                // آپلود فایل
                file.SaveAs(filePath);
                _logger.Information("فایل با موفقیت آپلود شد: {FilePath}", filePath);

                // ساخت URL نسبی
                var relativeUrl = uploadPath.Replace("~", "") + "/" + uniqueFileName;

                var result = new DocumentUploadResult
                {
                    FileUrl = relativeUrl,
                    OriginalFileName = file.FileName,
                    SavedFileName = uniqueFileName,
                    FileSize = file.ContentLength,
                    FileType = GetFileType(extension),
                    ContentType = file.ContentType
                };

                return ServiceResult<DocumentUploadResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در آپلود فایل مستندات");
                return ServiceResult<DocumentUploadResult>.Failed("خطا در آپلود فایل. لطفاً دوباره تلاش کنید.");
            }
        }

        public ServiceResult<bool> DeleteDocument(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return ServiceResult<bool>.Failed("مسیر فایل مشخص نشده است.");
                }

                var physicalPath = HostingEnvironment.MapPath(filePath);
                if (string.IsNullOrEmpty(physicalPath))
                {
                    _logger.Warning("مسیر فایل نامعتبر برای حذف: {FilePath}", filePath);
                    return ServiceResult<bool>.Failed("مسیر فایل نامعتبر است.");
                }

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    _logger.Information("فایل با موفقیت حذف شد: {PhysicalPath}", physicalPath);
                    return ServiceResult<bool>.Successful(true);
                }

                _logger.Warning("فایل برای حذف یافت نشد: {PhysicalPath}", physicalPath);
                return ServiceResult<bool>.Successful(true); // اگر فایل وجود نداشته باشد، حذف شده در نظر گرفته می‌شود
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف فایل مستندات: {FilePath}", filePath);
                return ServiceResult<bool>.Failed("خطا در حذف فایل.");
            }
        }

        public ServiceResult<bool> ValidateDocumentFile(HttpPostedFileBase file)
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
            if (!AllowedDocumentTypes.Contains(contentType))
            {
                _logger.Warning("نوع فایل نامعتبر: {ContentType}", contentType);
                return ServiceResult<bool>.Failed("فقط فایل‌های PDF, Word و Excel مجاز هستند.");
            }

            // بررسی extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                _logger.Warning("Extension نامعتبر: {Extension}", extension);
                return ServiceResult<bool>.Failed("فقط فایل‌های PDF, Word و Excel مجاز هستند.");
            }

            return ServiceResult<bool>.Successful(true);
        }

        #region Private Helper Methods

        private string GetFileType(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".pdf":
                    return "PDF";
                case ".doc":
                case ".docx":
                    return "DOC";
                case ".xls":
                case ".xlsx":
                    return "XLS";
                default:
                    return "UNKNOWN";
            }
        }

        #endregion
    }
}

