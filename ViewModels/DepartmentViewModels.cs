using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using ClinicApp.Interfaces;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// ViewModel برای ایجاد و ویرایش یک دپارتمان.
    /// </summary>
    public class DepartmentCreateEditViewModel
    {
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "نام دپارتمان الزامی است.")]
        [StringLength(200, ErrorMessage = "نام دپارتمان نمی‌تواند بیش از ۲۰۰ کاراکتر باشد.")]
        [Display(Name = "نام دپارتمان")]
        public string Name { get; set; }

        [Required(ErrorMessage = "انتخاب کلینیک الزامی است.")]
        [Display(Name = "کلینیک")]
        public int ClinicId { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // این فیلد برای نمایش نام کلینیک در هدر صفحات Create/Edit استفاده می‌شود
        public string ClinicName { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static DepartmentCreateEditViewModel FromEntity(Department department)
        {
            if (department == null) return null;
            return new DepartmentCreateEditViewModel
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name,
                ClinicId = department.ClinicId,
                IsActive = department.IsActive,
                ClinicName = department.Clinic?.Name // دسترسی امن به رابطه
            };
        }

        /// <summary>
        /// ✅ یک Entity موجود را بر اساس داده‌های این ViewModel به‌روزرسانی می‌کند.
        /// </summary>
        public void MapToEntity(Department department)
        {
            if (department == null) return;
            department.Name = this.Name?.Trim();
            department.ClinicId = this.ClinicId;
            department.IsActive = this.IsActive;
        }
    }

    /// <summary>
    /// ViewModel برای نمایش یک دپارتمان در لیست (جدول).
    /// </summary>
    public class DepartmentIndexViewModel
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string ClinicName { get; set; }
        public int DoctorCount { get; set; }
        public int ServiceCategoryCount { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static DepartmentIndexViewModel FromEntity(Department department)
        {
            if (department == null) return null;
            return new DepartmentIndexViewModel
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name,
                ClinicName = department.Clinic?.Name,
                // شمارش امن روابط برای جلوگیری از خطا
                DoctorCount = department.DoctorDepartments?.Count(dd => dd.Doctor.IsActive) ?? 0,
                ServiceCategoryCount = department.ServiceCategories?.Count(sc => sc.IsActive) ?? 0,
                IsActive = department.IsActive
            };
        }
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات کامل یک دپارتمان.
    /// </summary>
    public class DepartmentDetailsViewModel
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public int ClinicId { get; set; }
        public string ClinicName { get; set; }
        public int DoctorCount { get; set; }
        public int ServiceCategoryCount { get; set; }
        public bool IsActive { get; set; }
        public string CreatedAtShamsi { get; set; }
        public string UpdatedAtShamsi { get; set; }
        public string CreatedByUser { get; set; }
        public string UpdatedByUser { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static DepartmentDetailsViewModel FromEntity(Department department)
        {
            if (department == null) return null;
            return new DepartmentDetailsViewModel
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name,
                ClinicId = department.ClinicId,
                ClinicName = department.Clinic?.Name,
                IsActive = department.IsActive,
                DoctorCount = department.DoctorDepartments?.Count(dd => dd.Doctor.IsActive) ?? 0,
                ServiceCategoryCount = department.ServiceCategories?.Count(sc => sc.IsActive) ?? 0,
                CreatedAtShamsi = department.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = department.UpdatedAt?.ToPersianDateTime(),
                CreatedByUser = department.CreatedByUser?.FullName,
                UpdatedByUser = department.UpdatedByUser?.FullName
            };
        }

       
    }

    /// <summary>
    /// ViewModel برای صفحه لیست دپارتمان‌ها.
    /// این کلاس تمام داده‌های لازم برای نمایش صفحه، از جمله لیست دپارتمان‌ها و فیلترها را نگهداری می‌کند.
    /// </summary>
    public class DepartmentIndexPageViewModel
    {
        /// <summary>
        /// نتیجه صفحه‌بندی شده دپارتمان‌ها برای نمایش در جدول.
        /// </summary>
        public ClinicApp.Interfaces.PagedResult<DepartmentIndexViewModel> Departments { get; set; }

        /// <summary>
        /// لیست کلینیک‌ها برای نمایش در dropdown فیلتر.
        /// </summary>
        public SelectList Clinics { get; set; }

        /// <summary>
        /// عبارت جستجوی فعلی کاربر.
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// شناسه کلینیک انتخاب شده برای فیلتر.
        /// </summary>
        public int? SelectedClinicId { get; set; }

        public string SelectedClinicName { get; set; }

        public DepartmentIndexPageViewModel()
        {
            Departments = new ClinicApp.Interfaces.PagedResult<DepartmentIndexViewModel>();
            Clinics = new SelectList(new List<LookupItemViewModel>());
        }
    }
}