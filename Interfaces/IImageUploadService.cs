using System.Web;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Interface برای سرویس آپلود و پردازش تصاویر
    /// طراحی شده برای محیط Production درمانی
    /// </summary>
    public interface IImageUploadService
    {
        /// <summary>
        /// آپلود تصویر و ایجاد thumbnail خودکار
        /// </summary>
        /// <param name="file">فایل آپلود شده</param>
        /// <param name="uploadPath">مسیر آپلود (مثلاً ~/Content/Images/blog)</param>
        /// <param name="thumbnailPath">مسیر thumbnail (مثلاً ~/Content/Images/blog/thumbnails)</param>
        /// <param name="thumbnailWidth">عرض thumbnail (پیش‌فرض: 300)</param>
        /// <param name="thumbnailHeight">ارتفاع thumbnail (پیش‌فرض: 300)</param>
        /// <param name="maxWidth">حداکثر عرض تصویر اصلی (null = بدون resize)</param>
        /// <param name="maxHeight">حداکثر ارتفاع تصویر اصلی (null = بدون resize)</param>
        /// <returns>نتیجه آپلود شامل مسیر تصویر اصلی و thumbnail</returns>
        ServiceResult<ImageUploadResult> UploadImageWithThumbnail(
            HttpPostedFileBase file,
            string uploadPath,
            string thumbnailPath,
            int? thumbnailWidth = 300,
            int? thumbnailHeight = 300,
            int? maxWidth = null,
            int? maxHeight = null);

        /// <summary>
        /// حذف تصویر و thumbnail مرتبط
        /// </summary>
        /// <param name="imagePath">مسیر تصویر اصلی</param>
        /// <param name="thumbnailPath">مسیر thumbnail</param>
        /// <returns>نتیجه حذف</returns>
        ServiceResult<bool> DeleteImage(string imagePath, string thumbnailPath = null);

        /// <summary>
        /// بررسی اعتبار فایل تصویر
        /// </summary>
        /// <param name="file">فایل آپلود شده</param>
        /// <returns>نتیجه بررسی</returns>
        ServiceResult<bool> ValidateImageFile(HttpPostedFileBase file);
    }

    /// <summary>
    /// نتیجه آپلود تصویر
    /// </summary>
    public class ImageUploadResult
    {
        /// <summary>
        /// مسیر نسبی تصویر اصلی (مثلاً /Content/Images/blog/image.jpg)
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// مسیر نسبی thumbnail (مثلاً /Content/Images/blog/thumbnails/image_thumb.jpg)
        /// </summary>
        public string ThumbnailUrl { get; set; }

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
        /// ابعاد تصویر اصلی (Width x Height)
        /// </summary>
        public string ImageDimensions { get; set; }
    }
}

