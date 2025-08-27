using System;
using System.Collections.Generic;
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

        [Display(Name = "مدرک تحصیلی")]
        public Degree Degree { get; set; }

        [Display(Name = "نام مدرک تحصیلی")]
        public string DegreeName => Degree.ToString();

        [Display(Name = "سال فارغ‌التحصیلی")]
        public int? GraduationYear { get; set; }

        [Display(Name = "دانشگاه")]
        public string University { get; set; }

        [Display(Name = "جنسیت")]
        public Gender Gender { get; set; }

        [Display(Name = "نام جنسیت")]
        public string GenderName => Gender.ToString();

        [Display(Name = "تعرفه ویزیت")]
        public decimal? ConsultationFee { get; set; }

        [Display(Name = "سابقه کاری")]
        public int? ExperienceYears { get; set; }

        [Display(Name = "تخصص‌ها")]
        public List<string> SpecializationNames { get; set; } = new List<string>();

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

        // Properties جدید مورد نیاز View
        [Display(Name = "شناسه")]
        public int Id => DoctorId; // Alias برای View

        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        [Display(Name = "کد نظام پزشکی")]
        public string MedicalCouncilCode { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "تصویر پروفایل")]
        public string ProfileImageUrl { get; set; }

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
                Degree = doctor.Degree,
                GraduationYear = doctor.GraduationYear,
                University = doctor.University,
                Gender = doctor.Gender,
                ConsultationFee = doctor.ConsultationFee,
                ExperienceYears = doctor.ExperienceYears,
                SpecializationNames = doctor.Specializations?.Select(s => s.Name).ToList() ?? new List<string>(),
                PhoneNumber = doctor.PhoneNumber,
                IsActive = doctor.IsActive,
                CreatedAt = doctor.CreatedAt,
                CreatedAtShamsi = doctor.CreatedAt.ToPersianDate(),
                UpdatedAt = doctor.UpdatedAt,
                UpdatedAtShamsi = doctor.UpdatedAt?.ToPersianDate(),
                DepartmentCount = doctor.DoctorDepartments?.Count(dd => !dd.IsDeleted) ?? 0,
                ServiceCategoryCount = doctor.DoctorServiceCategories?.Count(dsc => !dsc.IsDeleted) ?? 0,
                // Properties جدید
                NationalCode = doctor.NationalCode,
                MedicalCouncilCode = doctor.MedicalCouncilCode,
                Email = doctor.Email,
                ProfileImageUrl = doctor.ProfileImageUrl ?? "/Content/Images/default-avatar.png"
            };
        }
    }
}
    