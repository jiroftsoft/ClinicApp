using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای کامپوننت جستجوی بیمار
    /// </summary>
    public class PatientSearchComponentViewModel
    {
        [Display(Name = "کد ملی")]
        [StringLength(10, ErrorMessage = "کد ملی باید ۱۰ رقم باشد")]
        public string NationalCode { get; set; }

        [Display(Name = "نام بیمار")]
        [StringLength(100, ErrorMessage = "نام نمی‌تواند بیش از ۱۰۰ کاراکتر باشد")]
        public string PatientName { get; set; }

        [Display(Name = "نتیجه جستجو")]
        public List<PatientSearchResultViewModel> SearchResults { get; set; } = new List<PatientSearchResultViewModel>();

        [Display(Name = "بیمار انتخاب شده")]
        public PatientDetailsViewModel SelectedPatient { get; set; }

        [Display(Name = "وضعیت جستجو")]
        public string SearchStatus { get; set; } = "آماده";

        [Display(Name = "آیا در حال جستجو")]
        public bool IsSearching { get; set; } = false;
    }

    /// <summary>
    /// ViewModel برای نتیجه جستجوی بیمار
    /// </summary>
    public class PatientSearchResultViewModel
    {
        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        [Display(Name = "تاریخ تولد")]
        public string BirthDate { get; set; }

        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [Display(Name = "نام کامل")]
        public string FullName => $"{FirstName} {LastName}";
    }
}
