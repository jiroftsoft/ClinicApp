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
    /// مدل نمایش لیست تخصص‌ها در صفحه Index
    /// </summary>
    public class SpecializationIndexViewModel
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
        [Display(Name = "فعال است")]
        public bool IsActive { get; set; }

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
        /// تاریخ ایجاد
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ ایجاد به فرمت شمسی
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
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش به فرمت شمسی
        /// </summary>
        [Display(Name = "آخرین ویرایش")]
        public string UpdatedAtShamsi { get; set; }

        /// <summary>
        /// نام کاربر آخرین ویرایش کننده
        /// </summary>
        [Display(Name = "ویرایش کننده")]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// وضعیت حذف
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Factory Method برای تبدیل Entity به ViewModel
        /// </summary>
        public static SpecializationIndexViewModel FromEntity(Specialization specialization)
        {
            if (specialization == null) return null;

            return new SpecializationIndexViewModel
            {
                SpecializationId = specialization.SpecializationId,
                Name = specialization.Name,
                Description = specialization.Description,
                IsActive = specialization.IsActive,
                DisplayOrder = specialization.DisplayOrder,
                DoctorCount = specialization.DoctorSpecializations?.Count ?? 0,
                CreatedAt = specialization.CreatedAt,
                CreatedAtShamsi = specialization.CreatedAt.ToPersianDateTime(),
                CreatedBy = specialization.CreatedByUser?.FullName ?? specialization.CreatedByUserId,
                UpdatedAt = specialization.UpdatedAt,
                UpdatedAtShamsi = specialization.UpdatedAt?.ToPersianDateTime(),
                UpdatedBy = specialization.UpdatedByUser?.FullName ?? specialization.UpdatedByUserId,
                IsDeleted = specialization.IsDeleted
            };
        }
    }
}
