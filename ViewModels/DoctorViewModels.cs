using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc; // Required for SelectListItem

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// **For the main list (Index view).**
    /// Lightweight and optimized for fast loading of doctor tables.
    /// </summary>
    public class DoctorIndexViewModel
    {
        public int DoctorId { get; set; }

        [Display(Name = "نام کامل")]
        public string FullName { get; set; }

        [Display(Name = "تخصص")]
        public string Specialization { get; set; }

        [Display(Name = "بخش")]
        public string DepartmentName { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string ClinicName { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public long PatientCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }

    // ---

    /// <summary>
    /// **For the Create and Edit forms.**
    /// Contains all editable fields and validation rules. It also includes a
    /// property to hold the list of departments for a dropdown menu.
    /// </summary>
    public class DoctorCreateEditViewModel
    {
        public int DoctorId { get; set; }

        [Display(Name = "نام")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [MaxLength(150)]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [MaxLength(150)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "تخصص الزامی است")]
        [StringLength(250, ErrorMessage = "تخصص نمی‌تواند بیشتر از 250 کاراکتر باشد")]
        [Display(Name = "تخصص")]
        public string Specialization { get; set; }

        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        [StringLength(20, ErrorMessage = "شماره تلفن نمی‌تواند بیشتر از 20 کاراکتر باشد")]
        [Display(Name = "شماره تلفن")]
        [RegularExpression(@"^\+[1-9]\d{1,14}$",
            ErrorMessage = "شماره تلفن نامعتبر است. لطفاً از فرمت بین‌المللی استفاده کنید (مثال: +989123456789)")]
        public string PhoneNumber { get; set; }

        // Foreign Key for the Department relationship
        [Display(Name = "بخش")]
        [Required(ErrorMessage = "انتخاب {0} الزامی است.")]
        public int? DepartmentId { get; set; }
        [Display(Name = "کلینیک")]
        public int? ClinicId { get; set; }  // ویژگی اضافه شده

     
        public IEnumerable<SelectListItem> DepartmentList { get; set; }

        // This will be used to link or create an ASP.NET Identity user for the doctor.
        // We can extend this later to include password fields if creating a new user.
        [Display(Name = "ایمیل")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست.")]
        public string Email { get; set; }

        [Display(Name = "کدملی (نام کاربری)")]
        [Required(ErrorMessage = "لطفاً {0} را وارد کنید.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "{0} باید ۱۰ رقم باشد.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "در فیلد {0} فقط عدد مجاز است.")]
        public string NationalCode { get; set; }

        [StringLength(1000, ErrorMessage = "بیوگرافی نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
        [Display(Name = "بیوگرافی و سوابق")]
        [DataType(DataType.MultilineText)]
        public string Bio { get; set; }
        // ویژگی‌های محاسبه‌شده
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    // ---

    /// <summary>
    /// **For the Details view.**
    /// Provides a complete, read-only profile of the doctor, including related information
    /// like their department and upcoming appointments.
    /// </summary>
    public class DoctorDetailsViewModel
    {
        public int DoctorId { get; set; }

        [Display(Name = "نام کامل")]
        public string FullName { get; set; }

        [Display(Name = "تخصص")]
        public string Specialization { get; set; }

        [Display(Name = "شماره موبایل")]
        public string PhoneNumber { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "بخش")]
        public string DepartmentName { get; set; }

        [Display(Name = "کلینیک")]
        public string ClinicName { get; set; }

        [Display(Name = "تاریخ ثبت")]
        public string CreatedAt { get; set; }

        // --- Related Data ---

        // We can populate this list to show the doctor's schedule.
        [Display(Name = "نوبت‌های پیش رو")]
        public List<AppointmentSummaryViewModel> UpcomingAppointments { get; set; }

        public string FirstName { get; }
        public string LastName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string DeletedBy { get; set; }

        public DoctorDetailsViewModel()
        {
            // Initialize the list to prevent null reference errors.
            UpcomingAppointments = new List<AppointmentSummaryViewModel>();
        }
    }

    /// <summary>
    /// A small, auxiliary ViewModel to show appointment info on the doctor's details page.
    /// </summary>
    public class AppointmentSummaryViewModel
    {
        public int AppointmentId { get; set; }
        public string PatientFullName { get; set; }
        public string AppointmentTime { get; set; }
    }
}