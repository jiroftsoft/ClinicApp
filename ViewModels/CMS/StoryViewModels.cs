using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ClinicApp.ViewModels.CMS
{
    #region Story Index

    public class StoryIndexViewModel
    {
        public int StoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public string VideoUrl { get; set; }
        public string VideoType { get; set; }
        public string LinkUrl { get; set; }
        public string ButtonText { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ViewCount { get; set; }
        public int? Duration { get; set; }
    }

    #endregion

    #region Story Create & Edit

    public class StoryCreateEditViewModel
    {
        public int StoryId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [MaxLength(1000, ErrorMessage = "آدرس ویدیو نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "آدرس ویدیو")]
        public string VideoUrl { get; set; }

        [MaxLength(50, ErrorMessage = "نوع ویدیو نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "نوع ویدیو")]
        public string VideoType { get; set; } // "YouTube", "Vimeo", "DirectUpload"

        [Required(ErrorMessage = "تصویر Thumbnail الزامی است.")]
        [MaxLength(500, ErrorMessage = "آدرس تصویر Thumbnail نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "تصویر Thumbnail")]
        public string ThumbnailUrl { get; set; }

        [MaxLength(500, ErrorMessage = "لینک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "لینک (اختیاری)")]
        public string LinkUrl { get; set; }

        [MaxLength(100, ErrorMessage = "متن دکمه نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "متن دکمه (اختیاری)")]
        public string ButtonText { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "ترتیب نمایش الزامی است.")]
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "تاریخ شروع")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "مدت زمان ویدیو (ثانیه)")]
        public int? Duration { get; set; }
    }

    #endregion

    #region Story Details

    public class StoryDetailsViewModel
    {
        public int StoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public string VideoUrl { get; set; }
        public string VideoType { get; set; }
        public string LinkUrl { get; set; }
        public string ButtonText { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ViewCount { get; set; }
        public int? Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion

    #region Story Public (برای نمایش در سایت)

    public class StoryPublicViewModel
    {
        public int StoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public string VideoUrl { get; set; }
        public string VideoType { get; set; }
        public string LinkUrl { get; set; }
        public string ButtonText { get; set; }
        public int? Duration { get; set; }
    }

    #endregion
}
