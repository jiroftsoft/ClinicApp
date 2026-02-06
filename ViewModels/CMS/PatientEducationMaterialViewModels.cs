using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.CMS
{
    #region PatientEducationMaterial Index

    public class PatientEducationMaterialIndexViewModel
    {
        public int PatientEducationMaterialId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public PatientEducationCategory Category { get; set; }
        public string CategoryDisplay { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long? FileSizeInBytes { get; set; }
        public string VideoUrl { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public int DownloadCount { get; set; }
        public int ViewCount { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel صفحه عمومی Index مطالب آموزشی (Strongly-Typed - بدون ViewBag برای داده).
    /// قرارداد: 03-Development-Contract-Quick-Guide
    /// </summary>
    public class PatientEducationMaterialIndexPageViewModel
    {
        public PagedResult<PatientEducationMaterialIndexViewModel> Materials { get; set; }
        public PatientEducationMaterialSearchViewModel SearchModel { get; set; }
        public IEnumerable<PatientEducationCategory> Categories { get; set; } = Array.Empty<PatientEducationCategory>();
        public string ErrorMessage { get; set; }
    }

    #endregion

    #region PatientEducationMaterial Create & Edit

    public class PatientEducationMaterialCreateEditViewModel
    {
        public int PatientEducationMaterialId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(300, ErrorMessage = "عنوان نمی‌تواند بیش از 300 کاراکتر باشد.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [Required(ErrorMessage = "توضیحات الزامی است.")]
        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Required(ErrorMessage = "محتوا الزامی است.")]
        [AllowHtml]
        [Display(Name = "محتوا")]
        public string Content { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس فایل نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس فایل")]
        public string FileUrl { get; set; }

        [MaxLength(100, ErrorMessage = "نام فایل نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "نام فایل")]
        public string FileName { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس ویدیو نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس ویدیو")]
        public string VideoUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر")]
        public string ImageUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر کوچک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر کوچک")]
        public string ThumbnailUrl { get; set; }

        [Required(ErrorMessage = "دسته‌بندی الزامی است.")]
        [Display(Name = "دسته‌بندی")]
        public PatientEducationCategory Category { get; set; }

        [MaxLength(500, ErrorMessage = "برچسب‌ها نمی‌توانند بیش از 500 کاراکتر باشند.")]
        [Display(Name = "برچسب‌ها")]
        public string Tags { get; set; }

        [Display(Name = "تاریخ انتشار")]
        public DateTime? PublishedAt { get; set; }

        [Display(Name = "منتشر شده")]
        public bool IsPublished { get; set; }

        [Display(Name = "ویژه")]
        public bool IsFeatured { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [MaxLength(500, ErrorMessage = "عنوان متا نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "عنوان متا")]
        public string MetaTitle { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات متا نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "توضیحات متا")]
        public string MetaDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Slug نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "Slug")]
        public string Slug { get; set; }
    }

    #endregion

    #region PatientEducationMaterial Details

    public class PatientEducationMaterialDetailsViewModel
    {
        public int PatientEducationMaterialId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long? FileSizeInBytes { get; set; }
        public string VideoUrl { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public PatientEducationCategory Category { get; set; }
        public string CategoryDisplay { get; set; }
        public string Tags { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public int DownloadCount { get; set; }
        public int ViewCount { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int DisplayOrder { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string Slug { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion

    #region PatientEducationMaterial Search

    public class PatientEducationMaterialSearchViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public PatientEducationCategory? Category { get; set; }
        public bool? IsPublished { get; set; }
        public bool? IsFeatured { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    #endregion
}

