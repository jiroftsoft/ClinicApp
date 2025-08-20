using ClinicApp.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Models.Entities;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// مدل نمایشی برای لیست دسته‌بندی‌های خدمات پزشکی
    /// این مدل برای نمایش اطلاعات در صفحه اصلی مدیریت دسته‌بندی‌ها استفاده می‌شود
    /// </summary>
    /// <summary>
    /// مدل ویو برای صفحه لیست دسته‌بندی‌های خدمات پزشکی
    /// این مدل تمام داده‌های مورد نیاز برای نمایش لیست دسته‌بندی‌ها را شامل می‌شود
    /// </summary>
    public class ServiceCategoryIndexViewModel
    {
        /// <summary>
        /// نتایج جستجوی دسته‌بندی‌های خدمات
        /// </summary>
        public PagedResult<ServiceCategoryIndexItemViewModel> ServiceCategories { get; set; }

        /// <summary>
        /// لیست دپارتمان‌ها برای استفاده در فیلتر
        /// </summary>
        public IEnumerable<SelectListItem> Departments { get; set; }

        /// <summary>
        /// شناسه دپارتمان انتخاب شده برای فیلتر
        /// </summary>
        public int? SelectedDepartmentId { get; set; }

        /// <summary>
        /// عبارت جستجو
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// شماره صفحه فعلی
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// تعداد کل دسته‌بندی‌ها
        /// </summary>
        public int TotalCategories { get; set; }

        /// <summary>
        /// تعداد دسته‌بندی‌های فعال
        /// </summary>
        public int ActiveCategories { get; set; }

        public bool IsActive { get; set; }
    }
    /// <summary>
    /// مدل ویو برای آیتم‌های لیست دسته‌بندی‌های خدمات
    /// </summary>
    public class ServiceCategoryIndexItemViewModel
    {
        public int ServiceCategoryId { get; set; }
        public string Title { get; set; }
        public string DepartmentName { get; set; }
        public string ClinicName { get; set; }
        public int ServiceCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int DepartmentId { get; set; }

        // فیلدهای شمسی برای نمایش
        public string CreatedAtShamsi { get; set; }
        public string UpdatedAtShamsi { get; set; }
    }
    /// <summary>
    /// مدل نمایشی برای فرم ایجاد و ویرایش دسته‌بندی‌های خدمات پزشکی
    /// این مدل برای تعامل کاربر با فرم‌های ویرایش استفاده می‌شود
    /// </summary>
    public class ServiceCategoryCreateEditViewModel
    {
        public int ServiceCategoryId { get; set; }

        [Required(ErrorMessage = "عنوان دسته‌بندی الزامی است")]
        [StringLength(200, ErrorMessage = "عنوان دسته‌بندی نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        [Display(Name = "عنوان دسته‌بندی")]
        public string Title { get; set; }

        [Required(ErrorMessage = "دپارتمان الزامی است")]
        [Display(Name = "دپارتمان")]
        public int DepartmentId { get; set; }

        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // اطلاعات ردیابی
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "ایجاد شده توسط")]
        public string CreatedBy { get; set; }

        [Display(Name = "تاریخ ایجاد شمسی")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedBy { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش شمسی")]
        public string UpdatedAtShamsi { get; set; }

        public  ApplicationUser UpdatedByUser { get; set; }
        public ApplicationUser CreatedByUser { get; set; }
    }

    /// <summary>
    /// مدل نمایشی برای جزئیات دسته‌بندی‌های خدمات پزشکی
    /// این مدل برای نمایش اطلاعات کامل در صفحه جزئیات استفاده می‌شود
    /// </summary>
    public class ServiceCategoryDetailsViewModel
    {
        public int ServiceCategoryId { get; set; }

        [Display(Name = "عنوان دسته‌بندی")]
        public string Title { get; set; }

        [Display(Name = "دپارتمان")]
        public string DepartmentName { get; set; }

        [Display(Name = "تعداد خدمات")]
        public int ServiceCount { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        // اطلاعات ردیابی
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "ایجاد شده توسط")]
        public string CreatedBy { get; set; }

        [Display(Name = "تاریخ ایجاد شمسی")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedBy { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش شمسی")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "تاریخ حذف")]
        public DateTime? DeletedAt { get; set; }

        [Display(Name = "حذف شده توسط")]
        public string DeletedBy { get; set; }

        [Display(Name = "تاریخ حذف شمسی")]
        public string DeletedAtShamsi { get; set; }

        public ApplicationUser CreatedByUser { get; set; }
        public ApplicationUser UpdatedByUser { get; set; }
    }

    /// <summary>
    /// مدل نمایشی برای لیست خدمات پزشکی
    /// این مدل برای نمایش اطلاعات در صفحه اصلی مدیریت خدمات استفاده می‌شود
    /// </summary>
    public class ServiceIndexViewModel
    {
        public int ServiceId { get; set; }

        [Display(Name = "عنوان خدمات")]
        public string Title { get; set; }

        [Display(Name = "کد خدمات")]
        public string ServiceCode { get; set; }

        [Display(Name = "دسته‌بندی")]
        public string ServiceCategoryTitle { get; set; }

        [Display(Name = "قیمت")]
        public decimal Price { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد شمسی")]
        public string CreatedAtShamsi { get; set; }
    }

    /// <summary>
    /// مدل نمایشی برای فرم ایجاد و ویرایش خدمات پزشکی
    /// این مدل برای تعامل کاربر با فرم‌های ویرایش استفاده می‌شود
    /// </summary>
    public class ServiceCreateEditViewModel
    {
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "عنوان خدمات الزامی است")]
        [StringLength(250, ErrorMessage = "عنوان خدمات نمی‌تواند بیشتر از 250 کاراکتر باشد")]
        [Display(Name = "عنوان خدمات")]
        public string Title { get; set; }

        [Required(ErrorMessage = "کد خدمات الزامی است")]
        [StringLength(50, ErrorMessage = "کد خدمات نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "کد خدمات فقط می‌تواند شامل حروف، اعداد و زیرخط باشد")]
        [Display(Name = "کد خدمات")]
        public string ServiceCode { get; set; }

        [Required(ErrorMessage = "قیمت الزامی است")]
        [Range(0, 9999999999.99, ErrorMessage = "قیمت باید بین 0 تا 9,999,999,999.99 باشد")]
        [Display(Name = "قیمت")]
        public decimal Price { get; set; }

        [StringLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Required(ErrorMessage = "دسته‌بندی خدمات الزامی است")]
        [Display(Name = "دسته‌بندی خدمات")]
        public int ServiceCategoryId { get; set; }

        [Display(Name = "عنوان دسته‌بندی")]
        public string ServiceCategoryTitle { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // اطلاعات ردیابی
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "ایجاد شده توسط")]
        public string CreatedBy { get; set; }

        [Display(Name = "تاریخ ایجاد شمسی")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedBy { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش شمسی")]
        public string UpdatedAtShamsi { get; set; }
        // فیلدهای جدید برای نمایش آمار استفاده
        public int UsageCount { get; set; }
        public decimal TotalRevenue { get; set; }

    }

    /// <summary>
    /// مدل نمایشی برای جزئیات خدمات پزشکی
    /// این مدل برای نمایش اطلاعات کامل در صفحه جزئیات استفاده می‌شود
    /// </summary>
    public class ServiceDetailsViewModel
    {
        public int ServiceId { get; set; }

        [Display(Name = "عنوان خدمات")]
        public string Title { get; set; }

        [Display(Name = "کد خدمات")]
        public string ServiceCode { get; set; }

        [Display(Name = "دسته‌بندی")]
        public string ServiceCategoryTitle { get; set; }

        [Display(Name = "قیمت")]
        public decimal Price { get; set; }

        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        // اطلاعات ردیابی
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "ایجاد شده توسط")]
        public string CreatedBy { get; set; }

        [Display(Name = "تاریخ ایجاد شمسی")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedBy { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش شمسی")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "تاریخ حذف")]
        public DateTime? DeletedAt { get; set; }

        [Display(Name = "حذف شده توسط")]
        public string DeletedBy { get; set; }

        [Display(Name = "تاریخ حذف شمسی")]
        public string DeletedAtShamsi { get; set; }

        // فیلدهای جدید برای نمایش آمار و اطلاعات آخرین استفاده
        public int UsageCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime? LastUsageDate { get; set; }
        public string LastUsageDateShamsi { get; set; }
        public string DepartmentTitle { get; set; }
        public string ClinicTitle { get; set; }
    }
    public class ServiceQuickCreateViewModel
    {
        [Display(Name = "عنوان خدمت")]
        public string Title { get; set; }

        [Display(Name = "کد خدمت")]
        public string ServiceCode { get; set; }

        [Display(Name = "قیمت (ریال)")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "توضیحات")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "دسته‌بندی خدمت")]
        public int ServiceCategoryId { get; set; }
    }

    /// <summary>
    /// مدل برای ایجاد سریع دسته‌بندی خدمات
    /// </summary>
    public class ServiceCategoryQuickCreateViewModel
    {
        [Display(Name = "عنوان دسته‌بندی")]
        public string Title { get; set; }

        [Display(Name = "بخش مربوطه")]
        public int DepartmentId { get; set; }
    }
}