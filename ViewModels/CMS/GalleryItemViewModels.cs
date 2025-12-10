using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;

namespace ClinicApp.ViewModels.CMS
{
    #region GalleryItem Index

    public class GalleryItemIndexViewModel
    {
        public int GalleryItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    #endregion

    #region GalleryItem Create & Edit

    public class GalleryItemCreateEditViewModel
    {
        public int GalleryItemId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Required(ErrorMessage = "تصویر الزامی است.")]
        [MaxLength(500, ErrorMessage = "آدرس تصویر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر")]
        public string ImageUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر کوچک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر کوچک")]
        public string ThumbnailUrl { get; set; }

        [MaxLength(100, ErrorMessage = "دسته‌بندی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "دسته‌بندی")]
        public string Category { get; set; } // "clinic", "doctors", "equipment", "events"

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "ترتیب نمایش الزامی است.")]
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }
    }

    #endregion

    #region GalleryItem Details

    public class GalleryItemDetailsViewModel
    {
        public int GalleryItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion
}

