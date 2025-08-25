using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// ViewModel برای ایجاد و ویرایش یک کلینیک.
    /// </summary>
    public class ClinicCreateEditViewModel
    {
        public int ClinicId { get; set; }

        [Required(ErrorMessage = "نام کلینیک الزامی است")]
        [StringLength(200, ErrorMessage = "نام کلینیک نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        [Display(Name = "نام کلینیک")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "آدرس نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [StringLength(50, ErrorMessage = "شماره تلفن نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static ClinicCreateEditViewModel FromEntity(Clinic clinic)
        {
            if (clinic == null) return null;
            return new ClinicCreateEditViewModel
            {
                ClinicId = clinic.ClinicId,
                Name = clinic.Name,
                Address = clinic.Address,
                PhoneNumber = clinic.PhoneNumber,
                IsActive = clinic.IsActive
            };
        }

        /// <summary>
        /// ✅ یک Entity موجود را بر اساس داده‌های این ViewModel به‌روزرسانی می‌کند.
        /// </summary>
        public void MapToEntity(Clinic clinic)
        {
            if (clinic == null) return;
            clinic.Name = this.Name?.Trim();
            clinic.Address = this.Address?.Trim();
            clinic.PhoneNumber = PersianNumberHelper.ToEnglishNumbers(this.PhoneNumber?.Trim());
            clinic.IsActive = this.IsActive;
        }
    }

    /// <summary>
    /// ViewModel برای نمایش یک کلینیک در لیست (جدول).
    /// </summary>
    public class ClinicIndexViewModel
    {
        public int ClinicId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public int DepartmentCount { get; set; }
        public bool IsActive { get; set; }
        public string CreatedAtShamsi { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static ClinicIndexViewModel FromEntity(Clinic clinic)
        {
            if (clinic == null) return null;
            return new ClinicIndexViewModel
            {
                ClinicId = clinic.ClinicId,
                Name = clinic.Name,
                PhoneNumber = clinic.PhoneNumber,
                // برای جلوگیری از خطای Null Reference، ابتدا collection را چک می‌کنیم
                DepartmentCount = clinic.Departments?.Count(d => !d.IsDeleted) ?? 0,
                IsActive = clinic.IsActive,
                CreatedAtShamsi = clinic.CreatedAt.ToPersianDate()
            };
        }
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات کامل یک کلینیک.
    /// </summary>
    public class ClinicDetailsViewModel
    {
        public int ClinicId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string CreatedAtShamsi { get; set; }
        public string UpdatedAtShamsi { get; set; }
        public string CreatedByUser { get; set; }
        public string UpdatedByUser { get; set; }
        public int DepartmentCount { get; set; }
        public int DoctorCount { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static ClinicDetailsViewModel FromEntity(Clinic clinic)
        {
            if (clinic == null) return null;
            return new ClinicDetailsViewModel
            {
                ClinicId = clinic.ClinicId,
                Name = clinic.Name,
                Address = clinic.Address,
                PhoneNumber = clinic.PhoneNumber,
                IsActive = clinic.IsActive,
                CreatedAtShamsi = clinic.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = clinic.UpdatedAt?.ToPersianDateTime(),
                CreatedByUser = clinic.CreatedByUser?.FullName,
                UpdatedByUser = clinic.UpdatedByUser?.FullName,
                DepartmentCount = clinic.Departments?.Count(d => !d.IsDeleted) ?? 0,
                DoctorCount = clinic.Doctors?.Count(d => !d.IsDeleted) ?? 0
            };
        }
    }
}