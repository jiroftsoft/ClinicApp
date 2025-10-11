using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای معیارهای جستجوی پذیرش
    /// </summary>
    public class ReceptionSearchCriteria
    {
        [Display(Name = "از تاریخ")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تا تاریخ")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "از تاریخ (سازگاری)")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "تا تاریخ (سازگاری)")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int? PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        [Display(Name = "شناسه پزشک")]
        public int? DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        [Display(Name = "نوع پذیرش")]
        public string Type { get; set; }

        [Display(Name = "اورژانس")]
        public bool? IsEmergency { get; set; }

        [Display(Name = "عبارت جستجو")]
        public string SearchTerm { get; set; }

        [Display(Name = "مرتب‌سازی بر اساس")]
        public string SortBy { get; set; }

        [Display(Name = "جهت مرتب‌سازی")]
        public string SortDirection { get; set; }

        [Display(Name = "شماره صفحه")]
        public int PageNumber { get; set; } = 1;

        [Display(Name = "اندازه صفحه")]
        public int PageSize { get; set; } = 10;
    }
}