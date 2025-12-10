using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;

namespace ClinicApp.ViewModels.CMS
{
    #region Health Tip Index & Search

    public class HealthTipSearchViewModel
    {
        public string SearchTerm { get; set; }
        public string Category { get; set; }
        public bool? IsPublished { get; set; }
        public bool? IsFeatured { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class HealthTipIndexViewModel
    {
        public int HealthTipId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int ViewCount { get; set; }
        public int ShareCount { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    #region Health Tip Create & Edit

    public class HealthTipCreateEditViewModel
    {
        public int HealthTipId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(300, ErrorMessage = "عنوان نمی‌تواند بیش از 300 کاراکتر باشد.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "خلاصه نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "خلاصه")]
        public string Summary { get; set; }

        [Required(ErrorMessage = "محتوا الزامی است.")]
        [Display(Name = "محتوا")]
        public string Content { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر")]
        public string ImageUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر کوچک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر کوچک")]
        public string ThumbnailUrl { get; set; }

        [MaxLength(100, ErrorMessage = "دسته‌بندی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "دسته‌بندی")]
        public string Category { get; set; } // "prevention", "nutrition", "exercise", "diseases", "general"

        [MaxLength(500, ErrorMessage = "برچسب‌ها نمی‌توانند بیش از 500 کاراکتر باشند.")]
        [Display(Name = "برچسب‌ها (با کاما جدا کنید)")]
        public string Tags { get; set; }

        [Display(Name = "تاریخ انتشار")]
        public DateTime? PublishedAt { get; set; }

        [Display(Name = "تاریخ انقضا")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "منتشر شده")]
        public bool IsPublished { get; set; }

        [Display(Name = "ویژه (نمایش در صفحه اصلی)")]
        public bool IsFeatured { get; set; }

        [Required(ErrorMessage = "ترتیب نمایش الزامی است.")]
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [MaxLength(500, ErrorMessage = "عنوان متا نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "عنوان متا (SEO)")]
        public string MetaTitle { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات متا نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "توضیحات متا (SEO)")]
        public string MetaDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Slug نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "Slug (آدرس URL)")]
        public string Slug { get; set; }

        [MaxLength(500, ErrorMessage = "لینک مرتبط نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "لینک مرتبط")]
        public string RelatedLinkUrl { get; set; }
    }

    #endregion

    #region Health Tip Details

    public class HealthTipDetailsViewModel
    {
        public int HealthTipId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public int ViewCount { get; set; }
        public int ShareCount { get; set; }
        public int DisplayOrder { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string Slug { get; set; }
        public string RelatedLinkUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion

    #region Health Tip Public (برای نمایش در سایت)

    public class HealthTipPublicViewModel
    {
        public int HealthTipId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Category { get; set; }
        public string CategoryDisplayName { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime? PublishedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int ViewCount { get; set; }
        public int ShareCount { get; set; }
        public string Slug { get; set; }
        public string RelatedLinkUrl { get; set; }
    }

    public class HealthTipCategoryViewModel
    {
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public int Count { get; set; }
        public string IconClass { get; set; }
    }

    #endregion
}

