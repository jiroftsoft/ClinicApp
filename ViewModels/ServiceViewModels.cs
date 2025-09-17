using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Enums;

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

        // فیلدهای TechnicalPart و ProfessionalPart حذف شدند
        // استفاده از ServiceComponents به عنوان روش اصلی محاسبه

        [Display(Name = "آیا هشتگ‌دار است؟")]
        public bool IsHashtagged { get; set; } = false;

        [Display(Name = "نوع محاسبه قیمت")]
        public ServicePriceCalculationType PriceCalculationType { get; set; } = ServicePriceCalculationType.ComponentBased;

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
                
                // فیلدهای جدید
                // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                IsHashtagged = service.IsHashtagged,
                PriceCalculationType = ServicePriceCalculationType.ComponentBased, // پیش‌فرض
                
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
            
            // فیلدهای جدید
            // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
            service.IsHashtagged = this.IsHashtagged;
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

    // ---------------------------------------------------------
    #region FactorSetting ViewModels
    // ---------------------------------------------------------

    /// <summary>
    /// ViewModel برای نمایش کای‌ها
    /// </summary>
    public class FactorSettingViewModel
    {
        public int Id { get; set; }

        [Display(Name = "نام کای")]
        public string Name { get; set; }

        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "نوع کای")]
        public ServiceComponentType FactorType { get; set; }

        [Display(Name = "نوع کای (متن)")]
        public string FactorTypeText
        {
            get
            {
                return FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای";
            }
        }

        [Display(Name = "هشتگ‌دار")]
        public bool IsHashtagged { get; set; }

        [Display(Name = "مقدار")]
        public decimal Value { get; set; }

        [Display(Name = "سال مالی")]
        public int FinancialYear { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        // اطلاعات ردیابی
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        // فیلدهای شمسی برای نمایش
        public string CreatedAtShamsi
        {
            get { return CreatedAt.ToPersianDateTime(); }
        }

        public string UpdatedAtShamsi
        {
            get { return UpdatedAt?.ToPersianDateTime(); }
        }
    }

    /// <summary>
    /// ViewModel برای ایجاد و ویرایش کای‌ها
    /// </summary>
    public class FactorSettingCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام کای الزامی است.")]
        [StringLength(100, ErrorMessage = "نام کای نباید بیش از 100 کاراکتر باشد.")]
        [Display(Name = "نام کای")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "توضیحات نباید بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "نوع کای الزامی است.")]
        [Display(Name = "نوع کای")]
        public ServiceComponentType FactorType { get; set; }

        [Display(Name = "هشتگ‌دار")]
        public bool IsHashtagged { get; set; }

        [Required(ErrorMessage = "مقدار کای الزامی است.")]
        [Range(0.01, 999999.99, ErrorMessage = "مقدار کای باید بین 0.01 تا 999999.99 باشد.")]
        [Display(Name = "مقدار")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "سال مالی الزامی است.")]
        [Range(1400, 1500, ErrorMessage = "سال مالی باید بین 1400 تا 1500 باشد.")]
        [Display(Name = "سال مالی")]
        public int FinancialYear { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تبدیل از Entity به ViewModel
        /// </summary>
        public static FactorSettingCreateEditViewModel FromEntity(FactorSetting factor)
        {
            if (factor == null) return null;

            return new FactorSettingCreateEditViewModel
            {
                Id = factor.FactorSettingId,
                Name = $"کای {(factor.FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای")} {factor.FinancialYear}",
                Description = factor.Description,
                FactorType = factor.FactorType,
                IsHashtagged = factor.IsHashtagged,
                Value = factor.Value,
                FinancialYear = factor.FinancialYear,
                IsActive = factor.IsActive
            };
        }

        /// <summary>
        /// تبدیل از ViewModel به Entity
        /// </summary>
        public FactorSetting MapToEntity(FactorSetting existingFactor = null)
        {
            var factor = existingFactor ?? new FactorSetting();

            factor.FactorSettingId = Id;
            factor.Description = Description;
            factor.FactorType = FactorType;
            factor.IsHashtagged = IsHashtagged;
            factor.Value = Value;
            factor.FinancialYear = FinancialYear;
            factor.IsActive = IsActive;

            return factor;
        }
    }

    #endregion

    #region ServiceComponent ViewModels

    /// <summary>
    /// ViewModel برای نمایش لیست اجزای خدمات
    /// </summary>
    public class ServiceComponentViewModel
    {
        public int ServiceComponentId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }
        public string ServiceCode { get; set; }
        public ServiceComponentType ComponentType { get; set; }
        public string ComponentTypeName { get; set; }
        public decimal Coefficient { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        /// <summary>
        /// تبدیل از Entity به ViewModel
        /// </summary>
        public static ServiceComponentViewModel FromEntity(ServiceComponent entity)
        {
            if (entity == null) return null;

            return new ServiceComponentViewModel
            {
                ServiceComponentId = entity.ServiceComponentId,
                ServiceId = entity.ServiceId,
                ServiceTitle = entity.Service?.Title ?? "",
                ServiceCode = entity.Service?.ServiceCode ?? "",
                ComponentType = entity.ComponentType,
                ComponentTypeName = entity.ComponentType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای",
                Coefficient = entity.Coefficient,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedByUser?.UserName ?? "",
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedByUser?.UserName ?? ""
            };
        }
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات جزء خدمت
    /// </summary>
    public class ServiceComponentDetailsViewModel
    {
        public int ServiceComponentId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }
        public string ServiceCode { get; set; }
        public ServiceComponentType ComponentType { get; set; }
        public string ComponentTypeName { get; set; }
        public decimal Coefficient { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }
    }

    /// <summary>
    /// ViewModel برای ایجاد و ویرایش جزء خدمت
    /// </summary>
    public class ServiceComponentCreateEditViewModel
    {
        public int ServiceComponentId { get; set; }

        [Required(ErrorMessage = "انتخاب خدمت الزامی است.")]
        [Display(Name = "خدمت")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "نوع جزء الزامی است.")]
        [Display(Name = "نوع جزء")]
        public ServiceComponentType ComponentType { get; set; }

        [Required(ErrorMessage = "ضریب الزامی است.")]
        [Range(0.01, 999999.99, ErrorMessage = "ضریب باید بین 0.01 تا 999999.99 باشد.")]
        [Display(Name = "ضریب")]
        public decimal Coefficient { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تبدیل از ViewModel به Entity
        /// </summary>
        public ServiceComponent MapToEntity()
        {
            return new ServiceComponent
            {
                ServiceComponentId = this.ServiceComponentId,
                ServiceId = this.ServiceId,
                ComponentType = this.ComponentType,
                Coefficient = this.Coefficient,
                Description = this.Description,
                IsActive = this.IsActive
            };
        }
    }

    #endregion

    #region ServiceTemplate ViewModels

    /// <summary>
    /// ViewModel برای نمایش لیست قالب‌های خدمات
    /// </summary>
    public class ServiceTemplateViewModel
    {
        public int ServiceTemplateId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public decimal DefaultTechnicalCoefficient { get; set; }
        public decimal DefaultProfessionalCoefficient { get; set; }
        public bool IsHashtagged { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات قالب خدمت
    /// </summary>
    public class ServiceTemplateDetailsViewModel
    {
        public int ServiceTemplateId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public decimal DefaultTechnicalCoefficient { get; set; }
        public decimal DefaultProfessionalCoefficient { get; set; }
        public bool IsHashtagged { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }
    }

    /// <summary>
    /// ViewModel برای ایجاد و ویرایش قالب خدمت
    /// </summary>
    public class ServiceTemplateCreateEditViewModel
    {
        public int ServiceTemplateId { get; set; }

        [Required(ErrorMessage = "کد خدمت الزامی است.")]
        [StringLength(50, ErrorMessage = "کد خدمت نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "کد خدمت")]
        public string ServiceCode { get; set; }

        [Required(ErrorMessage = "نام خدمت الزامی است.")]
        [StringLength(200, ErrorMessage = "نام خدمت نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام خدمت")]
        public string ServiceName { get; set; }

        [Required(ErrorMessage = "ضریب فنی پیش‌فرض الزامی است.")]
        [Range(0.01, 999999.99, ErrorMessage = "ضریب فنی باید بین 0.01 تا 999999.99 باشد.")]
        [Display(Name = "ضریب فنی پیش‌فرض")]
        public decimal DefaultTechnicalCoefficient { get; set; }

        [Required(ErrorMessage = "ضریب حرفه‌ای پیش‌فرض الزامی است.")]
        [Range(0.01, 999999.99, ErrorMessage = "ضریب حرفه‌ای باید بین 0.01 تا 999999.99 باشد.")]
        [Display(Name = "ضریب حرفه‌ای پیش‌فرض")]
        public decimal DefaultProfessionalCoefficient { get; set; }

        [Display(Name = "آیا هشتگ‌دار است؟")]
        public bool IsHashtagged { get; set; }

        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }

    #endregion

    #region ServiceComponents Status ViewModels

    /// <summary>
    /// ViewModel برای نمایش وضعیت اجزای یک خدمت
    /// </summary>
    public class ServiceComponentsStatusViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }
        public string ServiceCode { get; set; }
        public bool HasTechnicalComponent { get; set; }
        public bool HasProfessionalComponent { get; set; }
        public bool IsComplete { get; set; }
        public decimal TechnicalCoefficient { get; set; }
        public decimal ProfessionalCoefficient { get; set; }
        public int ComponentsCount { get; set; }
        public List<ServiceComponentViewModel> Components { get; set; } = new List<ServiceComponentViewModel>();
    }

    #endregion

    #region SharedService ViewModels

    /// <summary>
    /// ViewModel برای نمایش لیست خدمات مشترک
    /// طراحی شده برای محیط‌های درمانی با اطمینان 100%
    /// </summary>
    public class SharedServiceIndexViewModel
    {
        [Display(Name = "شناسه")]
        public int SharedServiceId { get; set; }

        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        [Display(Name = "عنوان خدمت")]
        public string ServiceTitle { get; set; }

        [Display(Name = "کد خدمت")]
        public string ServiceCode { get; set; }

        [Display(Name = "شناسه دپارتمان")]
        public int DepartmentId { get; set; }

        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "وضعیت فعال (متن)")]
        public string IsActiveText => IsActive ? "فعال" : "غیرفعال";

        [Display(Name = "توضیحات خاص دپارتمان")]
        public string DepartmentSpecificNotes { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجادکننده")]
        public string CreatedByUserName { get; set; }

        [Display(Name = "تاریخ به‌روزرسانی")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تاریخ به‌روزرسانی (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "ویرایش‌کننده")]
        public string UpdatedByUserName { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static SharedServiceIndexViewModel FromEntity(SharedService sharedService)
        {
            if (sharedService == null) return null;

            return new SharedServiceIndexViewModel
            {
                SharedServiceId = sharedService.SharedServiceId,
                ServiceId = sharedService.ServiceId,
                ServiceTitle = sharedService.Service?.Title,
                ServiceCode = sharedService.Service?.ServiceCode,
                DepartmentId = sharedService.DepartmentId,
                DepartmentName = sharedService.Department?.Name,
                IsActive = sharedService.IsActive,
                DepartmentSpecificNotes = sharedService.DepartmentSpecificNotes,
                CreatedAt = sharedService.CreatedAt,
                CreatedAtShamsi = sharedService.CreatedAt.ToString("yyyy/MM/dd"),
                CreatedByUserName = sharedService.CreatedByUser?.FullName,
                UpdatedAt = sharedService.UpdatedAt,
                UpdatedAtShamsi = sharedService.UpdatedAt?.ToString("yyyy/MM/dd"),
                UpdatedByUserName = sharedService.UpdatedByUser?.FullName
            };
        }
    }

    /// <summary>
    /// ViewModel برای ایجاد و ویرایش خدمات مشترک
    /// طراحی شده برای محیط‌های درمانی با اطمینان 100%
    /// </summary>
    public class SharedServiceCreateEditViewModel
    {
        [Display(Name = "شناسه خدمت مشترک")]
        public int SharedServiceId { get; set; }

        [Required(ErrorMessage = "انتخاب خدمت الزامی است.")]
        [Display(Name = "خدمت")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "انتخاب دپارتمان الزامی است.")]
        [Display(Name = "دپارتمان")]
        public int DepartmentId { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [StringLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "توضیحات خاص دپارتمان")]
        [DataType(DataType.MultilineText)]
        public string DepartmentSpecificNotes { get; set; }

        [Display(Name = "ضریب فنی Override")]
        public decimal? OverrideTechnicalFactor { get; set; }

        [Display(Name = "ضریب حرفه‌ای Override")]
        public decimal? OverrideProfessionalFactor { get; set; }

        // فیلدهای کمکی برای نمایش
        [Display(Name = "عنوان خدمت")]
        public string ServiceTitle { get; set; }

        [Display(Name = "کد خدمت")]
        public string ServiceCode { get; set; }

        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        // لیست‌های کمکی برای Dropdown
        public List<System.Web.Mvc.SelectListItem> Services { get; set; } = new List<System.Web.Mvc.SelectListItem>();
        public List<System.Web.Mvc.SelectListItem> Departments { get; set; } = new List<System.Web.Mvc.SelectListItem>();

        // اطلاعات ردیابی (فقط برای خواندن)
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "ایجادکننده")]
        public string CreatedByUserName { get; set; }

        [Display(Name = "تاریخ به‌روزرسانی")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "ویرایش‌کننده")]
        public string UpdatedByUserName { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static SharedServiceCreateEditViewModel FromEntity(SharedService sharedService)
        {
            if (sharedService == null) return null;

            return new SharedServiceCreateEditViewModel
            {
                SharedServiceId = sharedService.SharedServiceId,
                ServiceId = sharedService.ServiceId,
                ServiceTitle = sharedService.Service?.Title,
                ServiceCode = sharedService.Service?.ServiceCode,
                DepartmentId = sharedService.DepartmentId,
                DepartmentName = sharedService.Department?.Name,
                IsActive = sharedService.IsActive,
                DepartmentSpecificNotes = sharedService.DepartmentSpecificNotes,
                OverrideTechnicalFactor = sharedService.OverrideTechnicalFactor,
                OverrideProfessionalFactor = sharedService.OverrideProfessionalFactor,
                CreatedAt = sharedService.CreatedAt,
                CreatedByUserName = sharedService.CreatedByUser?.FullName,
                UpdatedAt = sharedService.UpdatedAt,
                UpdatedByUserName = sharedService.UpdatedByUser?.FullName
            };
        }

        /// <summary>
        /// تبدیل ViewModel به Entity
        /// </summary>
        public SharedService ToEntity()
        {
            return new SharedService
            {
                SharedServiceId = SharedServiceId,
                ServiceId = ServiceId,
                DepartmentId = DepartmentId,
                IsActive = IsActive,
                DepartmentSpecificNotes = DepartmentSpecificNotes,
                OverrideTechnicalFactor = OverrideTechnicalFactor,
                OverrideProfessionalFactor = OverrideProfessionalFactor
            };
        }
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات کامل خدمت مشترک
    /// طراحی شده برای محیط‌های درمانی با اطمینان 100%
    /// </summary>
    public class SharedServiceDetailsViewModel
    {
        [Display(Name = "شناسه خدمت مشترک")]
        public int SharedServiceId { get; set; }

        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        [Display(Name = "عنوان خدمت")]
        public string ServiceTitle { get; set; }

        [Display(Name = "کد خدمت")]
        public string ServiceCode { get; set; }

        [Display(Name = "قیمت خدمت")]
        public decimal ServicePrice { get; set; }

        [Display(Name = "قیمت فرمت شده")]
        public string ServicePriceFormatted => ServicePrice.ToString("N0") + " تومان";

        [Display(Name = "توضیحات خدمت")]
        public string ServiceDescription { get; set; }

        [Display(Name = "شناسه دپارتمان")]
        public int DepartmentId { get; set; }

        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        [Display(Name = "توضیحات دپارتمان")]
        public string DepartmentDescription { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "وضعیت فعال (متن)")]
        public string IsActiveText => IsActive ? "فعال" : "غیرفعال";

        [Display(Name = "توضیحات خاص دپارتمان")]
        public string DepartmentSpecificNotes { get; set; }

        [Display(Name = "ضریب فنی Override")]
        public decimal? OverrideTechnicalFactor { get; set; }

        [Display(Name = "ضریب حرفه‌ای Override")]
        public decimal? OverrideProfessionalFactor { get; set; }

        // اطلاعات ردیابی
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجادکننده")]
        public string CreatedByUserName { get; set; }

        [Display(Name = "تاریخ به‌روزرسانی")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تاریخ به‌روزرسانی (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "ویرایش‌کننده")]
        public string UpdatedByUserName { get; set; }

        // اطلاعات حذف (اگر حذف شده باشد)
        [Display(Name = "آیا حذف شده؟")]
        public bool IsDeleted { get; set; }

        [Display(Name = "تاریخ حذف")]
        public DateTime? DeletedAt { get; set; }

        [Display(Name = "تاریخ حذف (شمسی)")]
        public string DeletedAtShamsi { get; set; }

        [Display(Name = "حذف‌کننده")]
        public string DeletedByUserName { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static SharedServiceDetailsViewModel FromEntity(SharedService sharedService)
        {
            if (sharedService == null) return null;

            return new SharedServiceDetailsViewModel
            {
                SharedServiceId = sharedService.SharedServiceId,
                ServiceId = sharedService.ServiceId,
                ServiceTitle = sharedService.Service?.Title,
                ServiceCode = sharedService.Service?.ServiceCode,
                ServicePrice = sharedService.Service?.Price ?? 0,
                ServiceDescription = sharedService.Service?.Description,
                DepartmentId = sharedService.DepartmentId,
                DepartmentName = sharedService.Department?.Name,
                DepartmentDescription = sharedService.Department?.Description,
                IsActive = sharedService.IsActive,
                DepartmentSpecificNotes = sharedService.DepartmentSpecificNotes,
                OverrideTechnicalFactor = sharedService.OverrideTechnicalFactor,
                OverrideProfessionalFactor = sharedService.OverrideProfessionalFactor,
                CreatedAt = sharedService.CreatedAt,
                CreatedAtShamsi = sharedService.CreatedAt.ToString("yyyy/MM/dd"),
                CreatedByUserName = sharedService.CreatedByUser?.FullName,
                UpdatedAt = sharedService.UpdatedAt,
                UpdatedAtShamsi = sharedService.UpdatedAt?.ToString("yyyy/MM/dd"),
                UpdatedByUserName = sharedService.UpdatedByUser?.FullName,
                IsDeleted = sharedService.IsDeleted,
                DeletedAt = sharedService.DeletedAt,
                DeletedAtShamsi = sharedService.DeletedAt?.ToString("yyyy/MM/dd"),
                DeletedByUserName = sharedService.DeletedByUser?.FullName
            };
        }
    }

    /// <summary>
    /// ViewModel برای نمایش آمار خدمات مشترک
    /// طراحی شده برای محیط‌های درمانی با اطمینان 100%
    /// </summary>
    public class SharedServiceStatisticsViewModel
    {
        [Display(Name = "تعداد کل خدمات مشترک")]
        public int TotalSharedServices { get; set; }

        [Display(Name = "تعداد خدمات مشترک فعال")]
        public int ActiveSharedServices { get; set; }

        [Display(Name = "تعداد خدمات مشترک غیرفعال")]
        public int InactiveSharedServices { get; set; }

        [Display(Name = "تعداد کل دپارتمان‌ها")]
        public int TotalDepartments { get; set; }

        [Display(Name = "تعداد کل خدمات")]
        public int TotalServices { get; set; }

        [Display(Name = "میانگین خدمات به ازای هر دپارتمان")]
        public double AverageServicesPerDepartment { get; set; }

        [Display(Name = "درصد خدمات مشترک فعال")]
        public double ActiveServicesPercentage { get; set; }

        [Display(Name = "درصد خدمات مشترک غیرفعال")]
        public double InactiveServicesPercentage { get; set; }

        [Display(Name = "تعداد خدمات با Override")]
        public int ServicesWithOverride { get; set; }

        [Display(Name = "تعداد خدمات بدون Override")]
        public int ServicesWithoutOverride { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش گزارش استفاده از خدمات مشترک
    /// طراحی شده برای محیط‌های درمانی با اطمینان 100%
    /// </summary>
    public class SharedServiceUsageReportViewModel
    {
        [Display(Name = "شناسه خدمت مشترک")]
        public int SharedServiceId { get; set; }

        [Display(Name = "عنوان خدمت")]
        public string ServiceTitle { get; set; }

        [Display(Name = "کد خدمت")]
        public string ServiceCode { get; set; }

        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        [Display(Name = "تعداد استفاده")]
        public int UsageCount { get; set; }

        [Display(Name = "درآمد کل")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "درآمد فرمت شده")]
        public string TotalRevenueFormatted => TotalRevenue.ToString("N0") + " تومان";

        [Display(Name = "آخرین استفاده")]
        public DateTime? LastUsedAt { get; set; }

        [Display(Name = "آخرین استفاده (شمسی)")]
        public string LastUsedAtShamsi { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش دپارتمان در Lookup
    /// </summary>
    public class DepartmentLookupViewModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public bool IsActive { get; set; }

        public static DepartmentLookupViewModel FromEntity(Department department)
        {
            if (department == null) return null;

            return new DepartmentLookupViewModel
            {
                DepartmentId = department.DepartmentId,
                DepartmentName = department.Name,
                DepartmentCode = "", // Department entity doesn't have DepartmentCode field
                IsActive = department.IsActive
            };
        }
    }

    /// <summary>
    /// ViewModel برای فیلترهای صفحه Index خدمات مشترک
    /// </summary>
    public class SharedServiceFilterViewModel
    {
        [Display(Name = "جستجو")]
        public string SearchTerm { get; set; }

        [Display(Name = "دپارتمان")]
        public int? DepartmentId { get; set; }

        [Display(Name = "خدمت")]
        public int? ServiceId { get; set; }

        [Display(Name = "وضعیت")]
        public bool? IsActive { get; set; }

        [Display(Name = "تعداد در صفحه")]
        public int PageSize { get; set; } = 20;

        [Display(Name = "شماره صفحه")]
        public int PageNumber { get; set; } = 1;

        // Lookup lists
        public List<System.Web.Mvc.SelectListItem> Departments { get; set; } = new List<System.Web.Mvc.SelectListItem>();
        public List<System.Web.Mvc.SelectListItem> Services { get; set; } = new List<System.Web.Mvc.SelectListItem>();
        public List<System.Web.Mvc.SelectListItem> StatusOptions { get; set; } = new List<System.Web.Mvc.SelectListItem>();
        public List<System.Web.Mvc.SelectListItem> PageSizeOptions { get; set; } = new List<System.Web.Mvc.SelectListItem>();

        public SharedServiceFilterViewModel()
        {
            // Initialize status options
            StatusOptions = new List<System.Web.Mvc.SelectListItem>
            {
                new System.Web.Mvc.SelectListItem { Text = "همه وضعیت‌ها", Value = "", Selected = !IsActive.HasValue },
                new System.Web.Mvc.SelectListItem { Text = "فعال", Value = "true", Selected = IsActive == true },
                new System.Web.Mvc.SelectListItem { Text = "غیرفعال", Value = "false", Selected = IsActive == false }
            };

            // Initialize page size options
            PageSizeOptions = new List<System.Web.Mvc.SelectListItem>
            {
                new System.Web.Mvc.SelectListItem { Text = "20", Value = "20", Selected = PageSize == 20 },
                new System.Web.Mvc.SelectListItem { Text = "50", Value = "50", Selected = PageSize == 50 },
                new System.Web.Mvc.SelectListItem { Text = "100", Value = "100", Selected = PageSize == 100 }
            };
        }

        public void UpdateSelectedValues()
        {
            // Update status options selection
            foreach (var option in StatusOptions)
            {
                option.Selected = false;
                if (string.IsNullOrEmpty(option.Value) && !IsActive.HasValue)
                    option.Selected = true;
                else if (option.Value == "true" && IsActive == true)
                    option.Selected = true;
                else if (option.Value == "false" && IsActive == false)
                    option.Selected = true;
            }

            // Update page size options selection
            foreach (var option in PageSizeOptions)
            {
                option.Selected = option.Value == PageSize.ToString();
            }
        }
    }

    /// <summary>
    /// ViewModel ترکیبی برای صفحه Index خدمات مشترک
    /// </summary>
    public class SharedServiceIndexPageViewModel
    {
        public ClinicApp.Interfaces.PagedResult<SharedServiceIndexViewModel> SharedServices { get; set; }
        public SharedServiceFilterViewModel Filter { get; set; }
        public SharedServiceStatisticsViewModel Statistics { get; set; }

        public SharedServiceIndexPageViewModel()
        {
            Filter = new SharedServiceFilterViewModel();
            Statistics = new SharedServiceStatisticsViewModel();
        }

        public SharedServiceIndexPageViewModel(ClinicApp.Interfaces.PagedResult<SharedServiceIndexViewModel> sharedServices, SharedServiceFilterViewModel filter, SharedServiceStatisticsViewModel statistics)
        {
            SharedServices = sharedServices;
            Filter = filter;
            Statistics = statistics;
        }
    }


    #endregion
}