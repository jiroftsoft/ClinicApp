using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Interfaces;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// مدل داده‌های دپارتمان برای ایجاد و ویرایش
    /// </summary>
    public class DepartmentCreateEditViewModel
    {
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "نام دپارتمان الزامی است")]
        [StringLength(200, ErrorMessage = "نام دپارتمان نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        [Display(Name = "نام دپارتمان")]
        public string Name { get; set; }

        [Required(ErrorMessage = "کلینیک مربوطه الزامی است")]
        [Display(Name = "کلینیک")]
        public int ClinicId { get; set; }

        public bool IsActive { get; set; }
        public string ClinicName { get; set; }
      public int DoctorCount { get; set; }
        public int ServiceCount { get; set; }
        public DateTime DeletedAt { get; set; }
     
        // فیلدهای ردیابی (Audit Trail)
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } // این فیلد باید از نوع string باشد، نه DateTime

        // فیلدهای تاریخ شمسی برای نمایش
        public string CreatedAtShamsi { get; set; }
        public string UpdatedAtShamsi { get; set; }
        public string DeletedBy { get; set; }
        public string DeletedAtShamsi { get; set; }
    }


    /// <summary>
    /// مدل داده‌های دپارتمان برای نمایش در لیست
    /// </summary>
    public class DepartmentIndexViewModel
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string ClinicName { get; set; }
        public int DoctorCount { get; set; }
        public int ServiceCount { get; set; }
        public bool IsActive { get; set; }
        public int? ClinicId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }

    /// <summary>
    /// مدل داده‌های جزئیات دپارتمان
    /// </summary>
    /// <summary>
    /// مدل جزئیات دپارتمان برای سیستم‌های پزشکی
    /// این مدل تمام اطلاعات لازم برای نمایش جزئیات دپارتمان را شامل می‌شود
    /// </summary>
    public class DepartmentDetailsViewModel
    {
        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان
        /// </summary>
        [Required(ErrorMessage = "نام دپارتمان الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام دپارتمان نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Name { get; set; }

        /// <summary>
        /// شناسه کلینیک مرتبط
        /// </summary>
        public int ClinicId { get; set; }

        /// <summary>
        /// نام کلینیک مرتبط
        /// </summary>
        public string ClinicName { get; set; }

        /// <summary>
        /// تعداد پزشکان فعال در این دپارتمان
        /// </summary>
        public int DoctorCount { get; set; }

        /// <summary>
        /// تعداد خدمات فعال در این دپارتمان
        /// </summary>
        public int ServiceCount { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن دپارتمان
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// تاریخ ایجاد دپارتمان
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// تاریخ حذف (در صورت حذف)
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// نام کاربر ایجاد کننده
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// نام کاربر آخرین بروزرسانی
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// نام کاربر حذف کننده (در صورت حذف)
        /// </summary>
        public string DeletedBy { get; set; }

        /// <summary>
        /// لیست دسته‌بندی‌های خدمات مرتبط با این دپارتمان
        /// </summary>
        public List<ServiceCategoryViewModel> ServiceCategories { get; set; } = new List<ServiceCategoryViewModel>();

        public string CreatedAtShamsi { get; set; }
        public string UpdatedAtShamsi { get; set; }
        public string DeletedAtShamsi { get; set; }
    }



    /// <summary>
    /// مدل داده‌های صفحه لیست دپارتمان‌ها برای سیستم‌های پزشکی
    /// این مدل تمام اطلاعات لازم برای نمایش لیست دپارتمان‌ها را شامل می‌شود
    /// </summary>
    public class DepartmentIndexPageViewModel
    {
        /// <summary>
        /// لیست صفحه‌بندی‌شده دپارتمان‌ها
        /// </summary>
        public PagedResult<DepartmentIndexViewModel> Departments { get; set; }

        /// <summary>
        /// لیست کلینیک‌های فعال برای فیلتر
        /// </summary>
        public List<ClinicIndexViewModel> Clinics { get; set; }

        /// <summary>
        /// شماره صفحه فعلی
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// اندازه صفحه
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// عبارت جستجو
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// شناسه کلینیک جاری برای فیلتر
        /// </summary>
        public int? ClinicId { get; set; }

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        public DepartmentIndexPageViewModel()
        {
            Departments = new PagedResult<DepartmentIndexViewModel>();
            Clinics = new List<ClinicIndexViewModel>();
            PageNumber = 1;
            PageSize = 10;
        }
    }
    /// <summary>
    /// مدل دسته‌بندی خدمات برای سیستم‌های پزشکی
    /// </summary>
    public class ServiceCategoryViewModel
    {
        /// <summary>
        /// شناسه دسته‌بندی خدمات
        /// </summary>
        public int ServiceCategoryId { get; set; }

        /// <summary>
        /// عنوان دسته‌بندی خدمات
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// لیست خدمات مرتبط با این دسته‌بندی
        /// </summary>
        public List<ServiceViewModel> Services { get; set; } = new List<ServiceViewModel>();
    }

    /// <summary>
    /// مدل خدمات پزشکی
    /// </summary>
    public class ServiceViewModel
    {
        /// <summary>
        /// شناسه خدمات
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// عنوان خدمات
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// کد خدمات
        /// </summary>
        public string ServiceCode { get; set; }

        /// <summary>
        /// قیمت خدمات
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// توضیحات خدمات
        /// </summary>
        public string Description { get; set; }
    }
}