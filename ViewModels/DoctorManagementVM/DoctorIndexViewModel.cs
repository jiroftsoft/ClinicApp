using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Extensions;
using ClinicApp.Models.Entities;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// ViewModel برای نمایش اطلاعات پزشک در لیست
    /// </summary>
    public class DoctorIndexViewModel
    {
        public int DoctorId { get; set; }

        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Display(Name = "نام کامل")]
        public string FullName { get; set; }

        [Display(Name = "تخصص")]
        public string Specialization { get; set; }

        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "آخرین به‌روزرسانی")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "آخرین به‌روزرسانی (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "تعداد دپارتمان‌ها")]
        public int DepartmentCount { get; set; }

        [Display(Name = "تعداد دسته‌بندی‌های خدماتی")]
        public int ServiceCategoryCount { get; set; }

        /// <summary>
        /// تبدیل Entity به ViewModel
        /// </summary>
        public static DoctorIndexViewModel FromEntity(Doctor doctor)
        {
            if (doctor == null) return null;

            return new DoctorIndexViewModel
            {
                DoctorId = doctor.DoctorId,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                FullName = doctor.FullName,
                Specialization = doctor.Specialization,
                PhoneNumber = doctor.PhoneNumber,
                IsActive = doctor.IsActive,
                CreatedAt = doctor.CreatedAt,
                CreatedAtShamsi = doctor.CreatedAt.ToPersianDate(),
                UpdatedAt = doctor.UpdatedAt,
                UpdatedAtShamsi = doctor.UpdatedAt?.ToPersianDate(),
                DepartmentCount = doctor.DoctorDepartments?.Count(dd => !dd.IsDeleted) ?? 0,
                ServiceCategoryCount = doctor.DoctorServiceCategories?.Count(dsc => !dsc.IsDeleted) ?? 0
            };
        }
    }
}
    