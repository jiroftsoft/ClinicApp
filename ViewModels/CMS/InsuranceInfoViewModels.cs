using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;

namespace ClinicApp.ViewModels.CMS
{
    #region Insurance Info Index & Search

    public class InsuranceInfoSearchViewModel
    {
        public string SearchTerm { get; set; }
        public string InsuranceType { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class InsuranceInfoAdminIndexViewModel
    {
        public PagedResult<InsuranceInfoIndexViewModel> InsuranceInfos { get; set; }
        public List<InsuranceInfoTypeViewModel> InsuranceTypes { get; set; }
    }

    public class InsuranceInfoIndexViewModel
    {
        public int InsuranceInfoId { get; set; }
        public string InsuranceName { get; set; }
        public string InsuranceType { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ContactPhone { get; set; }
        public string WebsiteUrl { get; set; }
        public decimal? CoveragePercentage { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    #region Insurance Info Create & Edit

    public class InsuranceInfoCreateEditViewModel
    {
        public int InsuranceInfoId { get; set; }

        [Required(ErrorMessage = "نام بیمه الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام بیمه نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام بیمه")]
        public string InsuranceName { get; set; }

        [MaxLength(100, ErrorMessage = "نوع بیمه نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "نوع بیمه")]
        public string InsuranceType { get; set; } // "basic", "supplementary", "private", "government"

        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات کوتاه")]
        public string Description { get; set; }

        [Display(Name = "توضیحات کامل")]
        public string FullDescription { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس لوگو نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس لوگو")]
        public string LogoUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر کوچک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر کوچک")]
        public string ThumbnailUrl { get; set; }

        [MaxLength(200, ErrorMessage = "شماره تماس نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "شماره تماس")]
        public string ContactPhone { get; set; }

        [MaxLength(200, ErrorMessage = "وب‌سایت نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "وب‌سایت")]
        public string WebsiteUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [Display(Name = "درصد پوشش")]
        public decimal? CoveragePercentage { get; set; }

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

    #region Insurance Info Details

    public class InsuranceInfoDetailsViewModel
    {
        public int InsuranceInfoId { get; set; }
        public string InsuranceName { get; set; }
        public string InsuranceType { get; set; }
        public string Description { get; set; }
        public string FullDescription { get; set; }
        public string LogoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ContactPhone { get; set; }
        public string WebsiteUrl { get; set; }
        public string Address { get; set; }
        public decimal? CoveragePercentage { get; set; }
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

    #region Insurance Info Public (برای نمایش در سایت)

    public class InsuranceInfoIndexPageViewModel
    {
        public List<InsuranceInfoPublicViewModel> InsuranceInfos { get; set; }
        public List<InsuranceInfoTypeViewModel> InsuranceTypes { get; set; }
        public string SelectedType { get; set; }
    }

    public class InsuranceInfoPublicViewModel
    {
        public int InsuranceInfoId { get; set; }
        public string InsuranceName { get; set; }
        public string InsuranceType { get; set; }
        public string TypeDisplayName { get; set; }
        public string Description { get; set; }
        public string FullDescription { get; set; }
        public string LogoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ContactPhone { get; set; }
        public string WebsiteUrl { get; set; }
        public string Address { get; set; }
        public decimal? CoveragePercentage { get; set; }
        public int ViewCount { get; set; }
        public string Slug { get; set; }
    }

    public class InsuranceInfoTypeViewModel
    {
        public string InsuranceType { get; set; }
        public string DisplayName { get; set; }
        public int Count { get; set; }
        public string IconClass { get; set; }
    }

    #endregion
}

