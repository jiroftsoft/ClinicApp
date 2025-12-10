using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;

namespace ClinicApp.ViewModels.CMS
{
    #region Slider Index

    public class SliderIndexViewModel
    {
        public int SliderId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string LinkUrl { get; set; }
        public string ButtonText { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public string Position { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    #endregion

    #region Slider Create & Edit

    public class SliderCreateEditViewModel
    {
        public int SliderId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Required(ErrorMessage = "تصویر الزامی است.")]
        [MaxLength(500, ErrorMessage = "آدرس تصویر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر")]
        public string ImageUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر کوچک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر کوچک")]
        public string ThumbnailUrl { get; set; }

        [MaxLength(500, ErrorMessage = "لینک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "لینک")]
        public string LinkUrl { get; set; }

        [MaxLength(100, ErrorMessage = "متن دکمه نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "متن دکمه")]
        public string ButtonText { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "ترتیب نمایش الزامی است.")]
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [Display(Name = "تاریخ شروع")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        [MaxLength(50, ErrorMessage = "موقعیت نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "موقعیت")]
        public string Position { get; set; } // "hero", "sidebar", "footer"
    }

    #endregion

    #region Slider Details

    public class SliderDetailsViewModel
    {
        public int SliderId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string LinkUrl { get; set; }
        public string ButtonText { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Position { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion
}

