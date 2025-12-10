using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;

namespace ClinicApp.ViewModels.CMS
{
    #region Medical Service Info Index & Search

    public class MedicalServiceInfoSearchViewModel
    {
        public string SearchTerm { get; set; }
        public int? ServiceCategoryId { get; set; }
        public int? ServiceId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class MedicalServiceInfoIndexViewModel
    {
        public int MedicalServiceInfoId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceCategoryName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal? Price { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    #region Medical Service Info Create & Edit

    public class MedicalServiceInfoCreateEditViewModel
    {
        public int MedicalServiceInfoId { get; set; }

        [Required(ErrorMessage = "خدمت الزامی است.")]
        [Display(Name = "خدمت")]
        public int ServiceId { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات کوتاه نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات کوتاه")]
        public string Description { get; set; }

        [Display(Name = "توضیحات کامل")]
        public string FullDescription { get; set; }

        [MaxLength(2000, ErrorMessage = "ویژگی‌ها نمی‌توانند بیش از 2000 کاراکتر باشند.")]
        [Display(Name = "ویژگی‌ها (با کاما جدا کنید)")]
        public string Features { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر")]
        public string ImageUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر کوچک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر کوچک")]
        public string ThumbnailUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس ویدیو نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس ویدیو")]
        public string VideoUrl { get; set; }

        [Display(Name = "قیمت نمایشی")]
        public decimal? Price { get; set; }

        [MaxLength(2000, ErrorMessage = "اطلاعات پوشش بیمه نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        [Display(Name = "اطلاعات پوشش بیمه (با کاما جدا کنید)")]
        public string InsuranceCoverage { get; set; }

        [MaxLength(500, ErrorMessage = "مدت زمان تقریبی نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "مدت زمان تقریبی")]
        public string EstimatedDuration { get; set; }

        [MaxLength(500, ErrorMessage = "مدارک مورد نیاز نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "مدارک مورد نیاز (با کاما جدا کنید)")]
        public string RequiredDocuments { get; set; }

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

        [MaxLength(500, ErrorMessage = "لینک مرتبط نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "لینک مرتبط")]
        public string RelatedLinkUrl { get; set; }
    }

    #endregion

    #region Medical Service Info Details

    public class MedicalServiceInfoDetailsViewModel
    {
        public int MedicalServiceInfoId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceCategoryName { get; set; }
        public string Description { get; set; }
        public string FullDescription { get; set; }
        public string Features { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string VideoUrl { get; set; }
        public decimal? Price { get; set; }
        public decimal? ServicePrice { get; set; }
        public string InsuranceCoverage { get; set; }
        public string EstimatedDuration { get; set; }
        public string RequiredDocuments { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
        public int ViewCount { get; set; }
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

    #region Medical Service Info Public (برای نمایش در سایت)

    public class MedicalServiceInfoPublicViewModel
    {
        public int MedicalServiceInfoId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceCategoryName { get; set; }
        public string Description { get; set; }
        public string FullDescription { get; set; }
        public List<string> Features { get; set; } = new List<string>();
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string VideoUrl { get; set; }
        public decimal? Price { get; set; }
        public decimal? ServicePrice { get; set; }
        public List<string> InsuranceCoverage { get; set; } = new List<string>();
        public string EstimatedDuration { get; set; }
        public List<string> RequiredDocuments { get; set; } = new List<string>();
        public int ViewCount { get; set; }
        public string Slug { get; set; }
        public string RelatedLinkUrl { get; set; }
    }

    #endregion
}

