using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;

namespace ClinicApp.ViewModels.CMS
{
    #region Medical Equipment Index & Search

    public class MedicalEquipmentSearchViewModel
    {
        public string SearchTerm { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class MedicalEquipmentCategoryViewModel
    {
        public string Category { get; set; }
        public string DisplayName { get; set; }
    }

    public class MedicalEquipmentStatusViewModel
    {
        public string Status { get; set; }
        public string DisplayName { get; set; }
    }

    public class MedicalEquipmentAdminIndexViewModel
    {
        public PagedResult<MedicalEquipmentIndexViewModel> MedicalEquipments { get; set; }
        public List<MedicalEquipmentCategoryViewModel> Categories { get; set; }
        public List<MedicalEquipmentStatusViewModel> Statuses { get; set; }
    }

    public class MedicalEquipmentIndexViewModel
    {
        public int MedicalEquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Category { get; set; }
        public string CategoryDisplayName { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    #region Medical Equipment Create & Edit

    public class MedicalEquipmentCreateEditPageViewModel
    {
        public MedicalEquipmentCreateEditViewModel Model { get; set; }
        public List<MedicalEquipmentCategoryViewModel> Categories { get; set; }
        public List<MedicalEquipmentStatusViewModel> Statuses { get; set; }
    }

    public class MedicalEquipmentCreateEditViewModel
    {
        public int MedicalEquipmentId { get; set; }

        [Required(ErrorMessage = "نام تجهیز الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام تجهیز نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام تجهیز")]
        public string EquipmentName { get; set; }

        [MaxLength(100, ErrorMessage = "مدل نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "مدل")]
        public string Model { get; set; }

        [MaxLength(200, ErrorMessage = "سازنده نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "سازنده")]
        public string Manufacturer { get; set; }

        [Required(ErrorMessage = "دسته‌بندی الزامی است.")]
        [MaxLength(100, ErrorMessage = "دسته‌بندی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "دسته‌بندی")]
        public string Category { get; set; }

        [MaxLength(2000, ErrorMessage = "توضیحات نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [MaxLength(5000, ErrorMessage = "مشخصات فنی نمی‌تواند بیش از 5000 کاراکتر باشد.")]
        [Display(Name = "مشخصات فنی")]
        [System.Web.Mvc.AllowHtml]
        public string TechnicalSpecifications { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر اصلی نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر اصلی")]
        public string ImageUrl { get; set; }

        [MaxLength(2000, ErrorMessage = "لیست تصاویر نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        [Display(Name = "لیست تصاویر اضافی (JSON Array)")]
        public string ImageUrls { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس ویدیو نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس ویدیو")]
        public string VideoUrl { get; set; }

        [Display(Name = "تاریخ خرید")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }

        [Display(Name = "تاریخ نصب")]
        [DataType(DataType.Date)]
        public DateTime? InstallationDate { get; set; }

        [Display(Name = "تاریخ انقضای گارانتی")]
        [DataType(DataType.Date)]
        public DateTime? WarrantyExpiryDate { get; set; }

        [MaxLength(50, ErrorMessage = "وضعیت نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "ویژه (برای نمایش در صفحه اصلی)")]
        public bool IsFeatured { get; set; }

        [Required(ErrorMessage = "ترتیب نمایش الزامی است.")]
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [MaxLength(2000, ErrorMessage = "لیست ویژگی‌ها نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        [Display(Name = "لیست ویژگی‌ها (JSON Array)")]
        public string Features { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات کوتاه نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات کوتاه")]
        public string ShortDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Slug نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "Slug (آدرس URL)")]
        public string Slug { get; set; }

        [MaxLength(500, ErrorMessage = "عنوان متا نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "عنوان متا (SEO)")]
        public string MetaTitle { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات متا نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "توضیحات متا (SEO)")]
        public string MetaDescription { get; set; }
    }

    #endregion

    #region Medical Equipment Details

    public class MedicalEquipmentDetailsViewModel
    {
        public int MedicalEquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Category { get; set; }
        public string CategoryDisplayName { get; set; }
        public string Description { get; set; }
        public string TechnicalSpecifications { get; set; }
        public string ImageUrl { get; set; }
        public List<string> ImageUrls { get; set; }
        public string VideoUrl { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? InstallationDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
        public List<string> Features { get; set; }
        public string ShortDescription { get; set; }
        public string Slug { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion

    #region Medical Equipment Public (برای نمایش در سایت)

    public class MedicalEquipmentPublicViewModel
    {
        public int MedicalEquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Category { get; set; }
        public string CategoryDisplayName { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string ImageUrl { get; set; }
        public List<string> ImageUrls { get; set; }
        public string VideoUrl { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string Status { get; set; }
        public List<string> Features { get; set; }
        public string Slug { get; set; }
        public int ViewCount { get; set; }
    }

    #endregion
}

