using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ClinicApp.ViewModels
{
    // ---------------------------------------------------------
    #region Service Category ViewModels
    // ---------------------------------------------------------

    public class ServiceCategoryCreateEditViewModel
    {
        public int ServiceCategoryId { get; set; }

        [Required(ErrorMessage = "وارد کردن عنوان الزامی است.")]
        [StringLength(200)]
        [Display(Name = "عنوان دسته‌بندی")]
        public string Title { get; set; }

        [StringLength(1000)]
        [Display(Name = "توضیحات")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "انتخاب دپارتمان الزامی است.")]
        [Display(Name = "دپارتمان")]
        public int DepartmentId { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // For context in the view
        public string DepartmentName { get; set; }
        public int ClinicId { get; set; }
        
        // اطلاعات ردیابی (فقط برای خواندن)
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        
        // فیلدهای شمسی برای نمایش
        public string CreatedAtShamsi { get; set; }
        public string UpdatedAtShamsi { get; set; }

        public static ServiceCategoryCreateEditViewModel FromEntity(ServiceCategory sc)
        {
            if (sc == null) return null;
            return new ServiceCategoryCreateEditViewModel
            {
                ServiceCategoryId = sc.ServiceCategoryId,
                Title = sc.Title,
                Description = sc.Description,
                DepartmentId = sc.DepartmentId,
                IsActive = sc.IsActive,
                DepartmentName = sc.Department?.Name,
                ClinicId = sc.Department?.ClinicId ?? 0,
                
                // اطلاعات ردیابی
                CreatedAt = sc.CreatedAt,
                CreatedBy = sc.CreatedByUser?.FullName,
                UpdatedAt = sc.UpdatedAt,
                UpdatedBy = sc.UpdatedByUser?.FullName,
                CreatedAtShamsi = sc.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = sc.UpdatedAt?.ToPersianDateTime()
            };
        }

        public void MapToEntity(ServiceCategory sc)
        {
            if (sc == null) return;
            sc.Title = this.Title?.Trim();
            sc.Description = this.Description?.Trim();
            sc.DepartmentId = this.DepartmentId;
            sc.IsActive = this.IsActive;
        }
    }

    public class ServiceCategoryIndexViewModel
    {
        public int ServiceCategoryId { get; set; }
        public string Title { get; set; }
        public int ServiceCount { get; set; }
        public bool IsActive { get; set; }
        public string DepartmentName { get; set; }
        public string ClinicName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int DepartmentId { get; set; }

        // فیلدهای شمسی برای نمایش
        public string CreatedAtShamsi => CreatedAt.ToPersianDateTime();
        public string UpdatedAtShamsi => UpdatedAt?.ToPersianDateTime();
        public string Description { get; set; }

        public static ServiceCategoryIndexViewModel FromEntity(ServiceCategory sc)
        {
            if (sc == null) return null;
            return new ServiceCategoryIndexViewModel
            {
                ServiceCategoryId = sc.ServiceCategoryId,
                Title = sc.Title,
                ServiceCount = sc.Services?.Count(s => !s.IsDeleted) ?? 0,
                IsActive = sc.IsActive,
                DepartmentName = sc.Department?.Name,
                ClinicName = sc.Department?.Clinic?.Name,
                CreatedAt = sc.CreatedAt,
                CreatedBy = sc.CreatedByUser?.FullName,
                UpdatedAt = sc.UpdatedAt,
                DepartmentId = sc.DepartmentId
            };
        }
    }

    /// <summary>
    /// ViewModel برای نمایش آیتم‌های فهرست دسته‌بندی خدمات در جداول و لیست‌ها
    /// این ViewModel شامل تمام اطلاعات لازم برای نمایش در فهرست‌ها می‌باشد
    /// </summary>
    public class ServiceCategoryIndexItemViewModel
    {
        public int ServiceCategoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ServiceCount { get; set; }
        public bool IsActive { get; set; }
        public string DepartmentName { get; set; }
        public string ClinicName { get; set; }
        public int DepartmentId { get; set; }
        public int ClinicId { get; set; }
        
        // اطلاعات ردیابی
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedBy { get; set; }
        
        // فیلدهای شمسی برای نمایش
        public string CreatedAtShamsi { get; set; }
        public string UpdatedAtShamsi { get; set; }
        public string DeletedAtShamsi { get; set; }

        public static ServiceCategoryIndexItemViewModel FromEntity(ServiceCategory sc)
        {
            if (sc == null) return null;
            return new ServiceCategoryIndexItemViewModel
            {
                ServiceCategoryId = sc.ServiceCategoryId,
                Title = sc.Title,
                Description = sc.Description,
                ServiceCount = sc.Services?.Count(s => !s.IsDeleted) ?? 0,
                IsActive = sc.IsActive,
                DepartmentName = sc.Department?.Name,
                ClinicName = sc.Department?.Clinic?.Name,
                DepartmentId = sc.DepartmentId,
                ClinicId = sc.Department?.ClinicId ?? 0,
                CreatedAt = sc.CreatedAt,
                CreatedBy = sc.CreatedByUser?.FullName,
                UpdatedAt = sc.UpdatedAt,
                UpdatedBy = sc.UpdatedByUser?.FullName,
                DeletedAt = sc.DeletedAt,
                DeletedBy = sc.DeletedByUser?.FullName,
                CreatedAtShamsi = sc.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = sc.UpdatedAt?.ToPersianDateTime(),
                DeletedAtShamsi = sc.DeletedAt?.ToPersianDateTime()
            };
        }
    }

    public class ServiceCategoryDetailsViewModel
    {
        public int ServiceCategoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string ClinicName { get; set; }
        public int ServiceCount { get; set; }
        public bool IsActive { get; set; }
        
        // اطلاعات ردیابی
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedBy { get; set; }
        
        // فیلدهای شمسی برای نمایش
        public string CreatedAtShamsi { get; set; }
        public string CreatedByUser { get; set; }
        public string UpdatedAtShamsi { get; set; }
        public string UpdatedByUser { get; set; }
        public string DeletedAtShamsi { get; set; }

        public static ServiceCategoryDetailsViewModel FromEntity(ServiceCategory sc)
        {
            if (sc == null) return null;
            return new ServiceCategoryDetailsViewModel
            {
                ServiceCategoryId = sc.ServiceCategoryId,
                Title = sc.Title,
                Description = sc.Description,
                DepartmentId = sc.DepartmentId,
                DepartmentName = sc.Department?.Name,
                ClinicName = sc.Department?.Clinic?.Name,
                ServiceCount = sc.Services?.Count(s => !s.IsDeleted) ?? 0,
                IsActive = sc.IsActive,
                
                // اطلاعات ردیابی
                CreatedAt = sc.CreatedAt,
                CreatedBy = sc.CreatedByUser?.FullName,
                UpdatedAt = sc.UpdatedAt,
                UpdatedBy = sc.UpdatedByUser?.FullName,
                DeletedAt = sc.DeletedAt,
                DeletedBy = sc.DeletedByUser?.FullName,
                
                // فیلدهای شمسی
                CreatedAtShamsi = sc.CreatedAt.ToPersianDateTime(),
                CreatedByUser = sc.CreatedByUser?.FullName,
                UpdatedAtShamsi = sc.UpdatedAt?.ToPersianDateTime(),
                UpdatedByUser = sc.UpdatedByUser?.FullName,
                DeletedAtShamsi = sc.DeletedAt?.ToPersianDateTime()
            };
        }
    }

    #endregion

    // ---------------------------------------------------------
    #region Service ViewModels
    // ---------------------------------------------------------

    public class ServiceCreateEditViewModel
    {
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "وارد کردن عنوان الزامی است.")]
        [StringLength(200)]
        [Display(Name = "عنوان خدمت")]
        public string Title { get; set; }

        [Required(ErrorMessage = "وارد کردن کد خدمت الزامی است.")]
        [StringLength(50)]
        [Display(Name = "کد خدمت")]
        public string ServiceCode { get; set; }

        [Required(ErrorMessage = "وارد کردن قیمت الزامی است.")]
        [Range(0, double.MaxValue, ErrorMessage = "مقدار قیمت نمی‌تواند منفی باشد.")]
        [Display(Name = "قیمت پایه (تومان)")]
        public decimal Price { get; set; }

        [StringLength(1000)]
        [Display(Name = "توضیحات")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "انتخاب دسته‌بندی الزامی است.")]
        public int ServiceCategoryId { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "یادداشت‌ها")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        // For context in the view
        public string ServiceCategoryTitle { get; set; }
        public int DepartmentId { get; set; }
        
        // اطلاعات ردیابی (فقط برای خواندن)
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public int UsageCount { get; set; }
        public decimal TotalRevenue { get; set; }
        
        // فیلدهای شمسی برای نمایش
        public string CreatedAtShamsi { get; set; }
        public string UpdatedAtShamsi { get; set; }

        public static ServiceCreateEditViewModel FromEntity(Service service)
        {
            if (service == null) return null;
            return new ServiceCreateEditViewModel
            {
                ServiceId = service.ServiceId,
                Title = service.Title,
                ServiceCode = service.ServiceCode,
                Price = service.Price,
                Description = service.Description,
                ServiceCategoryId = service.ServiceCategoryId,
                IsActive = service.IsActive,
                Notes = service.Notes,
                ServiceCategoryTitle = service.ServiceCategory?.Title,
                DepartmentId = service.ServiceCategory?.DepartmentId ?? 0,
                
                // اطلاعات ردیابی
                CreatedAt = service.CreatedAt,
                CreatedBy = service.CreatedByUser?.FullName,
                UpdatedAt = service.UpdatedAt,
                UpdatedBy = service.UpdatedByUser?.FullName,
                UsageCount = 0, // می‌تواند از ReceptionDetails محاسبه شود
                TotalRevenue = 0, // می‌تواند از ReceptionDetails محاسبه شود
                CreatedAtShamsi = service.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = service.UpdatedAt?.ToPersianDateTime()
            };
        }

        public void MapToEntity(Service service)
        {
            if (service == null) return;
            service.Title = this.Title?.Trim();
            service.ServiceCode = this.ServiceCode?.Trim();
            service.Price = this.Price;
            service.Description = this.Description?.Trim();
            service.ServiceCategoryId = this.ServiceCategoryId;
            service.IsActive = this.IsActive;
            service.Notes = this.Notes?.Trim();
        }
    }

    public class ServiceIndexViewModel
    {
        public int ServiceId { get; set; }
        public string Title { get; set; }
        public string ServiceCode { get; set; }
        public decimal Price { get; set; }
        public string PriceFormatted { get; set; }
        public bool IsActive { get; set; }
        public string ServiceCategoryTitle { get; set; }
        public int ServiceCategoryId { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; }
        
        // اطلاعات ردیابی
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        
        // فیلدهای شمسی برای نمایش
        public string CreatedAtShamsi { get; set; }
        public string UpdatedAtShamsi { get; set; }

        public static ServiceIndexViewModel FromEntity(Service service)
        {
            if (service == null) return null;
            return new ServiceIndexViewModel
            {
                ServiceId = service.ServiceId,
                Title = service.Title,
                ServiceCode = service.ServiceCode,
                Price = service.Price,
                PriceFormatted = service.Price.ToString("N0"),
                IsActive = service.IsActive,
                ServiceCategoryTitle = service.ServiceCategory?.Title,
                ServiceCategoryId = service.ServiceCategoryId,
                DepartmentName = service.ServiceCategory?.Department?.Name,
                DepartmentId = service.ServiceCategory?.DepartmentId ?? 0,
                
                // اطلاعات ردیابی
                CreatedAt = service.CreatedAt,
                CreatedBy = service.CreatedByUser?.FullName,
                UpdatedAt = service.UpdatedAt,
                UpdatedBy = service.UpdatedByUser?.FullName,
                CreatedAtShamsi = service.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = service.UpdatedAt?.ToPersianDateTime()
            };
        }
    }

    /// <summary>
    /// مدل ویو برای نمایش جزئیات کامل یک خدمت پزشکی
    /// شامل تمام اطلاعات مرتبط با خدمت، دسته‌بندی، دپارتمان و تاریخچه
    /// </summary>
    public class ServiceDetailsViewModel
    {
        /// <summary>
        /// شناسه خدمت
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// عنوان خدمت
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// کد خدمت
        /// </summary>
        public string ServiceCode { get; set; }

        /// <summary>
        /// قیمت پایه خدمت
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// قیمت فرمت شده با جداکننده
        /// </summary>
        public string PriceFormatted => Price.ToString("N0") + " تومان";

        /// <summary>
        /// توضیحات خدمت
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// نام دسته‌بندی خدمات
        /// </summary>
        public string ServiceCategoryTitle { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی خدمات
        /// </summary>
        public int ServiceCategoryId { get; set; }

        /// <summary>
        /// نام دپارتمان
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// نام کلینیک
        /// </summary>
        public string ClinicName { get; set; }

        /// <summary>
        /// شناسه کلینیک
        /// </summary>
        public int ClinicId { get; set; }
        
        /// <summary>
        /// عنوان کلینیک (alias برای ClinicName)
        /// </summary>
        public string ClinicTitle { get; set; }

        /// <summary>
        /// عنوان دپارتمان (alias برای DepartmentName)
        /// </summary>
        public string DepartmentTitle { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// متن وضعیت
        /// </summary>
        public string StatusText => IsActive ? "فعال" : "غیرفعال";

        /// <summary>
        /// کلاس CSS بر اساس وضعیت
        /// </summary>
        public string StatusCssClass => IsActive ? "badge-success" : "badge-secondary";

        /// <summary>
        /// تاریخ ایجاد (شمسی)
        /// </summary>
        public string CreatedAtShamsi { get; set; }

        /// <summary>
        /// نام کاربر ایجادکننده
        /// </summary>
        public string CreatedByUser { get; set; }

        /// <summary>
        /// شناسه کاربر ایجادکننده
        /// </summary>
        public string CreatedByUserId { get; set; }
        
        /// <summary>
        /// تاریخ ایجاد (DateTime)
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// نام کاربر ایجادکننده (alias)
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی (شمسی)
        /// </summary>
        public string UpdatedAtShamsi { get; set; }

        /// <summary>
        /// نام کاربر به‌روزرسان
        /// </summary>
        public string UpdatedByUser { get; set; }

        /// <summary>
        /// شناسه کاربر به‌روزرسان
        /// </summary>
        public string UpdatedByUserId { get; set; }
        
        /// <summary>
        /// تاریخ آخرین به‌روزرسانی (DateTime)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// نام کاربر به‌روزرسان (alias)
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// یادداشت‌ها
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// آیا خدمت حذف شده است؟
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ حذف (شمسی) - در صورت وجود
        /// </summary>
        public string DeletedAtShamsi { get; set; }

        /// <summary>
        /// نام کاربر حذف‌کننده - در صورت وجود
        /// </summary>
        public string DeletedByUser { get; set; }
        
        /// <summary>
        /// تاریخ حذف (DateTime) - در صورت وجود
        /// </summary>
        public DateTime? DeletedAt { get; set; }
        
        /// <summary>
        /// نام کاربر حذف‌کننده (alias)
        /// </summary>
        public string DeletedBy { get; set; }

        /// <summary>
        /// تعداد استفاده از این خدمت (اختیاری - برای آمار)
        /// </summary>
        public int UsageCount { get; set; }

        /// <summary>
        /// آخرین تاریخ استفاده (شمسی) - اختیاری
        /// </summary>
        public string LastUsedAtShamsi { get; set; }
        
        /// <summary>
        /// آخرین تاریخ استفاده (DateTime) - اختیاری
        /// </summary>
        public DateTime? LastUsageDate { get; set; }
        
        /// <summary>
        /// آخرین تاریخ استفاده (شمسی) - alias
        /// </summary>
        public string LastUsageDateShamsi { get; set; }
        
        /// <summary>
        /// مجموع درآمد از این خدمت
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// تبدیل Entity به ViewModel
        /// </summary>
        public static ServiceDetailsViewModel FromEntity(Service service)
        {
            if (service == null) return null;

            return new ServiceDetailsViewModel
            {
                ServiceId = service.ServiceId,
                Title = service.Title,
                ServiceCode = service.ServiceCode,
                Price = service.Price,
                Description = service.Description,
                ServiceCategoryTitle = service.ServiceCategory?.Title,
                ServiceCategoryId = service.ServiceCategoryId,
                DepartmentName = service.ServiceCategory?.Department?.Name,
                DepartmentId = service.ServiceCategory?.DepartmentId ?? 0,
                ClinicName = service.ServiceCategory?.Department?.Clinic?.Name,
                ClinicId = service.ServiceCategory?.Department?.ClinicId ?? 0,
                ClinicTitle = service.ServiceCategory?.Department?.Clinic?.Name,
                DepartmentTitle = service.ServiceCategory?.Department?.Name,
                IsActive = service.IsActive,
                // اطلاعات ردیابی
                CreatedAt = service.CreatedAt,
                CreatedAtShamsi = service.CreatedAt.ToPersianDateTime(),
                CreatedBy = service.CreatedByUser?.FullName,
                CreatedByUser = service.CreatedByUser?.FullName,
                CreatedByUserId = service.CreatedByUserId,
                UpdatedAt = service.UpdatedAt,
                UpdatedAtShamsi = service.UpdatedAt?.ToPersianDateTime(),
                UpdatedBy = service.UpdatedByUser?.FullName,
                UpdatedByUser = service.UpdatedByUser?.FullName,
                UpdatedByUserId = service.UpdatedByUserId,
                DeletedAt = service.DeletedAt,
                DeletedAtShamsi = service.DeletedAt?.ToPersianDateTime(),
                DeletedBy = service.DeletedByUser?.FullName,
                DeletedByUser = service.DeletedByUser?.FullName,
                
                // اطلاعات اضافی
                Notes = service.Notes,
                IsDeleted = service.IsDeleted,
                UsageCount = 0, // محاسبه از ReceptionDetails
                TotalRevenue = 0, // محاسبه از ReceptionDetails
                LastUsageDate = null, // محاسبه از ReceptionDetails
                LastUsageDateShamsi = null
            };
        }
    }

    #endregion
}