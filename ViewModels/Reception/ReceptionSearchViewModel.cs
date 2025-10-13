using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای صفحه جستجوی پذیرش
    /// </summary>
    public class ReceptionSearchViewModel
    {
        [Display(Name = "از تاریخ")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تا تاریخ")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "از تاریخ (شمسی)")]
        public string StartDateShamsi { get; set; }

        [Display(Name = "تا تاریخ (شمسی)")]
        public string EndDateShamsi { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int? PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        [Display(Name = "کد ملی بیمار")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "شناسه پزشک")]
        public int? DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        [Display(Name = "نوع پذیرش")]
        public string Type { get; set; }

        [Display(Name = "وضعیت (Enum)")]
        public Models.Enums.ReceptionStatus? StatusEnum { get; set; }

        [Display(Name = "نوع پذیرش (Enum)")]
        public Models.Enums.ReceptionType? TypeEnum { get; set; }

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

        [Display(Name = "مبلغ حداقل")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "مبلغ حداکثر")]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "روش پرداخت")]
        public Models.Enums.PaymentMethod? PaymentMethod { get; set; }

        [Display(Name = "شناسه بیمه")]
        public int? InsuranceId { get; set; }

        [Display(Name = "ترتیب مرتب‌سازی")]
        public string SortOrder { get; set; }

        [Display(Name = "پذیرش آنلاین")]
        public bool? IsOnlineReception { get; set; }

        // Lookup Lists
        [Display(Name = "لیست وضعیت‌ها")]
        public List<System.Web.Mvc.SelectListItem> StatusList { get; set; } = new List<System.Web.Mvc.SelectListItem>();

        [Display(Name = "لیست انواع")]
        public List<System.Web.Mvc.SelectListItem> TypeList { get; set; } = new List<System.Web.Mvc.SelectListItem>();

        [Display(Name = "لیست روش‌های پرداخت")]
        public List<System.Web.Mvc.SelectListItem> PaymentMethodList { get; set; } = new List<System.Web.Mvc.SelectListItem>();

        [Display(Name = "لیست بیمه‌ها")]
        public List<System.Web.Mvc.SelectListItem> InsuranceList { get; set; } = new List<System.Web.Mvc.SelectListItem>();

        // Search Results
        [Display(Name = "نتایج جستجو")]
        public List<ReceptionIndexViewModel> SearchResults { get; set; } = new List<ReceptionIndexViewModel>();

        [Display(Name = "تعداد کل نتایج")]
        public int TotalResults { get; set; }

        [Display(Name = "تعداد صفحات")]
        public int TotalPages { get; set; }

        [Display(Name = "آیا نتایجی یافت شد؟")]
        public bool HasResults { get; set; }

        [Display(Name = "پیام جستجو")]
        public string SearchMessage { get; set; }
    }
}
