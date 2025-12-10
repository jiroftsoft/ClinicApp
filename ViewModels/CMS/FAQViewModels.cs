using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;

namespace ClinicApp.ViewModels.CMS
{
    #region FAQ Index & Search

    public class FAQSearchViewModel
    {
        public string SearchTerm { get; set; }
        public string Category { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class FAQIndexViewModel
    {
        public int FAQId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    #region FAQ Create & Edit

    public class FAQCreateEditViewModel
    {
        public int FAQId { get; set; }

        [Required(ErrorMessage = "سوال الزامی است.")]
        [MaxLength(500, ErrorMessage = "سوال نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "سوال")]
        public string Question { get; set; }

        [Required(ErrorMessage = "پاسخ الزامی است.")]
        [Display(Name = "پاسخ")]
        public string Answer { get; set; }

        [MaxLength(100, ErrorMessage = "دسته‌بندی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "دسته‌بندی")]
        public string Category { get; set; } // "appointment", "insurance", "services", "costs", "general"

        [MaxLength(500, ErrorMessage = "برچسب‌ها نمی‌توانند بیش از 500 کاراکتر باشند.")]
        [Display(Name = "برچسب‌ها (با کاما جدا کنید)")]
        public string Tags { get; set; }

        [MaxLength(500, ErrorMessage = "لینک مرتبط نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "لینک مرتبط")]
        public string RelatedLinkUrl { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

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
    }

    #endregion

    #region FAQ Details

    public class FAQDetailsViewModel
    {
        public int FAQId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public string RelatedLinkUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
        public int ViewCount { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string Slug { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion

    #region FAQ Public (برای نمایش در سایت)

    public class FAQPublicViewModel
    {
        public int FAQId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Category { get; set; }
        public string CategoryDisplayName { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public string RelatedLinkUrl { get; set; }
        public int ViewCount { get; set; }
        public string Slug { get; set; }
    }

    public class FAQCategoryViewModel
    {
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public int Count { get; set; }
    }

    #endregion
}

