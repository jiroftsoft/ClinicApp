using System.Web;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Interface برای سرویس آپلود فایل‌های مستندات (PDF, Word, Excel)
    /// طراحی شده برای محیط Production درمانی
    /// </summary>
    public interface IDocumentUploadService
    {
        /// <summary>
        /// آپلود فایل مستندات (PDF, Word, Excel)
        /// </summary>
        /// <param name="file">فایل آپلود شده</param>
        /// <param name="uploadPath">مسیر آپلود (مثلاً ~/Content/Documents/patient-education)</param>
        /// <returns>نتیجه آپلود شامل مسیر فایل</returns>
        ServiceResult<DocumentUploadResult> UploadDocument(
            HttpPostedFileBase file,
            string uploadPath);

        /// <summary>
        /// حذف فایل مستندات
        /// </summary>
        /// <param name="filePath">مسیر فایل</param>
        /// <returns>نتیجه حذف</returns>
        ServiceResult<bool> DeleteDocument(string filePath);

        /// <summary>
        /// بررسی اعتبار فایل مستندات
        /// </summary>
        /// <param name="file">فایل آپلود شده</param>
        /// <returns>نتیجه بررسی</returns>
        ServiceResult<bool> ValidateDocumentFile(HttpPostedFileBase file);
    }

    /// <summary>
    /// نتیجه آپلود فایل مستندات
    /// </summary>
    public class DocumentUploadResult
    {
        /// <summary>
        /// مسیر نسبی فایل (مثلاً /Content/Documents/patient-education/file.pdf)
        /// </summary>
        public string FileUrl { get; set; }

        /// <summary>
        /// نام فایل اصلی
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// نام فایل ذخیره شده
        /// </summary>
        public string SavedFileName { get; set; }

        /// <summary>
        /// اندازه فایل (bytes)
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// نوع فایل (PDF, DOC, DOCX, XLS, XLSX)
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// Content Type
        /// </summary>
        public string ContentType { get; set; }
    }
}

