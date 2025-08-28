using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using ClinicApp.Extensions;

namespace ClinicApp.ViewModels.SpecializationManagementVM
{
    /// <summary>
    /// مدل نمایش جزئیات تخصص
    /// </summary>
    public class SpecializationDetailsViewModel
    {
        /// <summary>
        /// شناسه تخصص
        /// </summary>
        public int SpecializationId { get; set; }

        /// <summary>
        /// نام تخصص
        /// </summary>
        [Display(Name = "نام تخصص")]
        public string Name { get; set; }

        /// <summary>
        /// توضیحات تخصص
        /// </summary>
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن
        /// </summary>
        [Display(Name = "وضعیت")]
        public string StatusText { get; set; }

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// تعداد پزشکان مرتبط
        /// </summary>
        [Display(Name = "تعداد پزشکان")]
        public int DoctorCount { get; set; }

        /// <summary>
        /// لیست پزشکان مرتبط
        /// </summary>
        [Display(Name = "پزشکان مرتبط")]
        public List<DoctorListItemViewModel> Doctors { get; set; } = new List<DoctorListItemViewModel>();

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        public string CreatedAtShamsi { get; set; }

        /// <summary>
        /// نام کاربر ایجاد کننده
        /// </summary>
        [Display(Name = "ایجاد کننده")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش
        /// </summary>
        [Display(Name = "آخرین ویرایش")]
        public string UpdatedAtShamsi { get; set; }

        /// <summary>
        /// نام کاربر آخرین ویرایش کننده
        /// </summary>
        [Display(Name = "ویرایش کننده")]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Factory Method برای تبدیل Entity به ViewModel
        /// </summary>
        public static SpecializationDetailsViewModel FromEntity(Specialization specialization)
        {
            if (specialization == null) return null;

            return new SpecializationDetailsViewModel
            {
                SpecializationId = specialization.SpecializationId,
                Name = specialization.Name,
                Description = specialization.Description,
                StatusText = specialization.IsActive ? "فعال" : "غیرفعال",
                DisplayOrder = specialization.DisplayOrder,
                DoctorCount = specialization.DoctorSpecializations?.Count ?? 0,
                Doctors = specialization.DoctorSpecializations?.Select(ds => new DoctorListItemViewModel
                {
                    DoctorId = ds.Doctor.DoctorId,
                    FullName = $"{ds.Doctor.FirstName} {ds.Doctor.LastName}",
                    IsActive = ds.Doctor.IsActive,
                    StatusText = ds.Doctor.IsActive ? "فعال" : "غیرفعال"
                }).ToList() ?? new List<DoctorListItemViewModel>(),
                CreatedAtShamsi = specialization.CreatedAt.ToPersianDateTime(),
                CreatedBy = specialization.CreatedByUser?.FullName ?? specialization.CreatedByUserId,
                UpdatedAtShamsi = specialization.UpdatedAt?.ToPersianDateTime(),
                UpdatedBy = specialization.UpdatedByUser?.FullName ?? specialization.UpdatedByUserId
            };
        }
    }

    /// <summary>
    /// مدل نمایش خلاصه پزشک در لیست
    /// </summary>
    public class DoctorListItemViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام کامل پزشک
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// متن وضعیت
        /// </summary>
        public string StatusText { get; set; }
    }
}
