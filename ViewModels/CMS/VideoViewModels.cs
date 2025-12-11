using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using ClinicApp.Helpers;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.CMS
{
    #region Video Index & Search

    public class VideoSearchViewModel
    {
        public string SearchTerm { get; set; }
        public string Category { get; set; }
        public VideoType? VideoType { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class VideoIndexViewModel
    {
        public int VideoId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public VideoType VideoType { get; set; }
        public string VideoTypeName { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Category { get; set; }
        public int? Duration { get; set; }
        public string DurationFormatted { get; set; }
        public int ViewCount { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    #region Video Create & Edit

    public class VideoCreateEditViewModel
    {
        public int VideoId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(500, ErrorMessage = "عنوان نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [MaxLength(2000, ErrorMessage = "توضیحات نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [MaxLength(1000, ErrorMessage = "آدرس ویدیو نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "آدرس ویدیو (برای YouTube, Vimeo, Aparat)")]
        public string VideoUrl { get; set; }

        /// <summary>
        /// فایل ویدیو برای آپلود مستقیم (فقط برای DirectUpload)
        /// </summary>
        [Display(Name = "فایل ویدیو (فقط برای آپلود مستقیم)")]
        public HttpPostedFileBase VideoFile { get; set; }

        [Required(ErrorMessage = "نوع ویدیو الزامی است.")]
        [Display(Name = "نوع ویدیو")]
        public VideoType VideoType { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر کوچک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر کوچک")]
        public string ThumbnailUrl { get; set; }

        [MaxLength(100, ErrorMessage = "دسته‌بندی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "دسته‌بندی")]
        public string Category { get; set; }

        [Display(Name = "مدت زمان (ثانیه)")]
        public int? Duration { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "ترتیب نمایش الزامی است.")]
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }
    }

    #endregion

    #region Video Details

    public class VideoDetailsViewModel
    {
        public int VideoId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public VideoType VideoType { get; set; }
        public string VideoTypeName { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Category { get; set; }
        public int? Duration { get; set; }
        public string DurationFormatted { get; set; }
        public int ViewCount { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion

    #region Video HomePage

    /// <summary>
    /// ViewModel برای نمایش ویدیوها در صفحه اصلی
    /// طراحی شده برای محیط Production درمانی
    /// </summary>
    public class VideoHomePageViewModel
    {
        public int VideoId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public VideoType VideoType { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Category { get; set; }
        public int? Duration { get; set; }
        public string DurationFormatted { get; set; }
        public int ViewCount { get; set; }
        public string EmbedUrl { get; set; } // URL برای embed در iframe
        public string VideoIdFromUrl { get; set; } // Video ID از YouTube/Vimeo
    }

    #endregion
}

