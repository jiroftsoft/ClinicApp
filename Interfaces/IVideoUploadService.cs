using System.Web;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Interface برای سرویس آپلود و پردازش ویدیوها
    /// طراحی شده برای محیط Production درمانی
    /// </summary>
    public interface IVideoUploadService
    {
        /// <summary>
        /// آپلود ویدیو
        /// </summary>
        /// <param name="file">فایل ویدیو آپلود شده</param>
        /// <param name="uploadPath">مسیر آپلود (مثلاً ~/Content/Videos)</param>
        /// <returns>نتیجه آپلود شامل مسیر ویدیو</returns>
        ServiceResult<VideoUploadResult> UploadVideo(
            HttpPostedFileBase file,
            string uploadPath);

        /// <summary>
        /// حذف ویدیو
        /// </summary>
        /// <param name="videoPath">مسیر ویدیو</param>
        /// <returns>نتیجه حذف</returns>
        ServiceResult<bool> DeleteVideo(string videoPath);

        /// <summary>
        /// بررسی اعتبار فایل ویدیو
        /// </summary>
        /// <param name="file">فایل آپلود شده</param>
        /// <returns>نتیجه بررسی</returns>
        ServiceResult<bool> ValidateVideoFile(HttpPostedFileBase file);
    }

    /// <summary>
    /// نتیجه آپلود ویدیو
    /// </summary>
    public class VideoUploadResult
    {
        public string VideoUrl { get; set; }
        public string VideoFileName { get; set; }
        public long FileSizeInBytes { get; set; }
        public string FileSizeFormatted { get; set; }
        public string ContentType { get; set; }
    }
}

