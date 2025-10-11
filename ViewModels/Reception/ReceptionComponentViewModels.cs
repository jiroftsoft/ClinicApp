using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel یکپارچه برای کامپوننت‌های پذیرش
    /// </summary>
    public class ReceptionComponentViewModels
    {
        #region Patient Search Component

        /// <summary>
        /// ViewModel برای کامپوننت جستجوی بیمار
        /// </summary>
        public class PatientSearchComponentViewModel
        {
            #region Search Parameters

            /// <summary>
            /// عبارت جستجو
            /// </summary>
            [Display(Name = "عبارت جستجو")]
            [StringLength(100, ErrorMessage = "عبارت جستجو نمی‌تواند بیش از 100 کاراکتر باشد")]
            public string SearchTerm { get; set; }

            /// <summary>
            /// کد ملی
            /// </summary>
            [Display(Name = "کد ملی")]
            [StringLength(10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
            public string NationalCode { get; set; }

            /// <summary>
            /// شماره صفحه
            /// </summary>
            [Display(Name = "شماره صفحه")]
            [Range(1, int.MaxValue, ErrorMessage = "شماره صفحه باید مثبت باشد")]
            public int PageNumber { get; set; } = 1;

            /// <summary>
            /// اندازه صفحه
            /// </summary>
            [Display(Name = "اندازه صفحه")]
            [Range(1, 50, ErrorMessage = "اندازه صفحه باید بین 1 تا 50 باشد")]
            public int PageSize { get; set; } = 10;

            #endregion

            #region Search Results

            /// <summary>
            /// نتایج جستجو
            /// </summary>
            public List<ReceptionPatientLookupViewModel> SearchResults { get; set; } = new List<ReceptionPatientLookupViewModel>();

            /// <summary>
            /// تعداد کل نتایج
            /// </summary>
            public int TotalCount { get; set; }

            /// <summary>
            /// آیا جستجو انجام شده است؟
            /// </summary>
            public bool HasSearched { get; set; }

            /// <summary>
            /// آیا در حال جستجو است؟
            /// </summary>
            public bool IsSearching { get; set; }

            #endregion

            #region Selected Patient

            /// <summary>
            /// بیمار انتخاب شده
            /// </summary>
            public ReceptionPatientLookupViewModel SelectedPatient { get; set; }

            /// <summary>
            /// آیا بیمار انتخاب شده است؟
            /// </summary>
            public bool HasSelectedPatient => SelectedPatient != null;

            #endregion

            #region UI Helper Properties

            /// <summary>
            /// پیام وضعیت
            /// </summary>
            public string StatusMessage { get; set; }

            /// <summary>
            /// نوع پیام (success, error, warning, info)
            /// </summary>
            public string MessageType { get; set; } = "info";

            /// <summary>
            /// آیا نمایش نتایج فعال است؟
            /// </summary>
            public bool ShowResults { get; set; }

            /// <summary>
            /// آیا نمایش فرم ایجاد بیمار فعال است؟
            /// </summary>
            public bool ShowCreateForm { get; set; }

            #endregion

            #region Constructor

            public PatientSearchComponentViewModel()
            {
                PageNumber = 1;
                PageSize = 10;
                HasSearched = false;
                IsSearching = false;
                ShowResults = false;
                ShowCreateForm = false;
                StatusMessage = "آماده برای جستجو";
                MessageType = "info";
            }

            #endregion
        }

        #endregion

        #region Reception Form Component

        /// <summary>
        /// ViewModel برای کامپوننت فرم پذیرش
        /// </summary>
        public class ReceptionFormComponentViewModel
        {
            #region Reception Information

            /// <summary>
            /// شناسه پذیرش (برای ویرایش)
            /// </summary>
            public int? ReceptionId { get; set; }

            /// <summary>
            /// تاریخ پذیرش
            /// </summary>
            [Required(ErrorMessage = "تاریخ پذیرش الزامی است")]
            [Display(Name = "تاریخ پذیرش")]
            public DateTime ReceptionDate { get; set; }

            /// <summary>
            /// ساعت پذیرش
            /// </summary>
            [Display(Name = "ساعت پذیرش")]
            public TimeSpan? ReceptionTime { get; set; }

            /// <summary>
            /// نوع پذیرش
            /// </summary>
            [Required(ErrorMessage = "نوع پذیرش الزامی است")]
            [Display(Name = "نوع پذیرش")]
            public string ReceptionType { get; set; }

            /// <summary>
            /// اولویت پذیرش
            /// </summary>
            [Display(Name = "اولویت")]
            public string Priority { get; set; }

            /// <summary>
            /// آیا اورژانس است؟
            /// </summary>
            [Display(Name = "اورژانس")]
            public bool IsEmergency { get; set; }

            /// <summary>
            /// یادداشت‌ها
            /// </summary>
            [Display(Name = "یادداشت‌ها")]
            [StringLength(1000, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 1000 کاراکتر باشد")]
            public string Notes { get; set; }

            #endregion

            #region Patient Information

            /// <summary>
            /// شناسه بیمار
            /// </summary>
            [Required(ErrorMessage = "انتخاب بیمار الزامی است")]
            [Display(Name = "بیمار")]
            public int PatientId { get; set; }

            /// <summary>
            /// نام بیمار (فقط برای نمایش)
            /// </summary>
            [Display(Name = "نام بیمار")]
            public string PatientName { get; set; }

            /// <summary>
            /// کد ملی بیمار (فقط برای نمایش)
            /// </summary>
            [Display(Name = "کد ملی")]
            public string PatientNationalCode { get; set; }

            #endregion

            #region Doctor Information

            /// <summary>
            /// شناسه پزشک
            /// </summary>
            [Required(ErrorMessage = "انتخاب پزشک الزامی است")]
            [Display(Name = "پزشک")]
            public int DoctorId { get; set; }

            /// <summary>
            /// نام پزشک (فقط برای نمایش)
            /// </summary>
            [Display(Name = "نام پزشک")]
            public string DoctorName { get; set; }

            /// <summary>
            /// شناسه دپارتمان
            /// </summary>
            [Display(Name = "دپارتمان")]
            public int? DepartmentId { get; set; }

            /// <summary>
            /// نام دپارتمان (فقط برای نمایش)
            /// </summary>
            [Display(Name = "نام دپارتمان")]
            public string DepartmentName { get; set; }

            #endregion

            #region Service Information

            /// <summary>
            /// شناسه خدمت
            /// </summary>
            [Required(ErrorMessage = "انتخاب خدمت الزامی است")]
            [Display(Name = "خدمت")]
            public int ServiceId { get; set; }

            /// <summary>
            /// نام خدمت (فقط برای نمایش)
            /// </summary>
            [Display(Name = "نام خدمت")]
            public string ServiceName { get; set; }

            /// <summary>
            /// شناسه دسته‌بندی خدمت
            /// </summary>
            [Display(Name = "دسته‌بندی خدمت")]
            public int? ServiceCategoryId { get; set; }

            /// <summary>
            /// نام دسته‌بندی خدمت (فقط برای نمایش)
            /// </summary>
            [Display(Name = "نام دسته‌بندی")]
            public string ServiceCategoryName { get; set; }

            #endregion

            #region Insurance Information

            /// <summary>
            /// شناسه بیمه
            /// </summary>
            [Display(Name = "بیمه")]
            public int? InsuranceId { get; set; }

            /// <summary>
            /// نام بیمه (فقط برای نمایش)
            /// </summary>
            [Display(Name = "نام بیمه")]
            public string InsuranceName { get; set; }

            /// <summary>
            /// شماره بیمه
            /// </summary>
            [Display(Name = "شماره بیمه")]
            [StringLength(50, ErrorMessage = "شماره بیمه نمی‌تواند بیش از 50 کاراکتر باشد")]
            public string InsuranceNumber { get; set; }

            /// <summary>
            /// تاریخ انقضای بیمه
            /// </summary>
            [Display(Name = "تاریخ انقضای بیمه")]
            public DateTime? InsuranceExpiryDate { get; set; }

            /// <summary>
            /// آیا بیمه معتبر است؟
            /// </summary>
            public bool IsInsuranceValid { get; set; }

            /// <summary>
            /// پیام وضعیت بیمه
            /// </summary>
            public string InsuranceStatusMessage { get; set; }

            #endregion

            #region Financial Information

            /// <summary>
            /// مبلغ کل خدمت
            /// </summary>
            [Display(Name = "مبلغ کل")]
            [Range(0, double.MaxValue, ErrorMessage = "مبلغ باید مثبت باشد")]
            public decimal TotalAmount { get; set; }

            /// <summary>
            /// سهم بیمه
            /// </summary>
            [Display(Name = "سهم بیمه")]
            [Range(0, double.MaxValue, ErrorMessage = "سهم بیمه باید مثبت باشد")]
            public decimal InsuranceShare { get; set; }

            /// <summary>
            /// مبلغ قابل پرداخت
            /// </summary>
            [Display(Name = "مبلغ قابل پرداخت")]
            [Range(0, double.MaxValue, ErrorMessage = "مبلغ قابل پرداخت باید مثبت باشد")]
            public decimal PayableAmount { get; set; }

            #endregion

            #region UI Helper Properties

            /// <summary>
            /// لیست پزشکان برای Dropdown
            /// </summary>
            public List<SelectListItem> DoctorList { get; set; } = new List<SelectListItem>();

            /// <summary>
            /// لیست خدمات برای Dropdown
            /// </summary>
            public List<SelectListItem> ServiceList { get; set; } = new List<SelectListItem>();

            /// <summary>
            /// لیست دسته‌بندی‌های خدمات برای Dropdown
            /// </summary>
            public List<SelectListItem> ServiceCategoryList { get; set; } = new List<SelectListItem>();

            /// <summary>
            /// لیست بیمه‌ها برای Dropdown
            /// </summary>
            public List<SelectListItem> InsuranceList { get; set; } = new List<SelectListItem>();

            /// <summary>
            /// لیست دپارتمان‌ها برای Dropdown
            /// </summary>
            public List<SelectListItem> DepartmentList { get; set; } = new List<SelectListItem>();

            /// <summary>
            /// لیست انواع پذیرش برای Dropdown
            /// </summary>
            public List<SelectListItem> ReceptionTypeList { get; set; } = new List<SelectListItem>
            {
                new SelectListItem { Value = "Normal", Text = "عادی" },
                new SelectListItem { Value = "Emergency", Text = "اورژانس" },
                new SelectListItem { Value = "Special", Text = "ویژه" },
                new SelectListItem { Value = "Online", Text = "آنلاین" }
            };

            /// <summary>
            /// لیست اولویت‌ها برای Dropdown
            /// </summary>
            public List<SelectListItem> PriorityList { get; set; } = new List<SelectListItem>
            {
                new SelectListItem { Value = "Normal", Text = "عادی" },
                new SelectListItem { Value = "High", Text = "بالا" },
                new SelectListItem { Value = "Urgent", Text = "فوری" }
            };

            #endregion

            #region Component State

            /// <summary>
            /// آیا فرم در حال بارگذاری است؟
            /// </summary>
            public bool IsLoading { get; set; }

            /// <summary>
            /// آیا فرم معتبر است؟
            /// </summary>
            public bool IsValid { get; set; }

            /// <summary>
            /// آیا فرم ذخیره شده است؟
            /// </summary>
            public bool IsSaved { get; set; }

            /// <summary>
            /// پیام وضعیت
            /// </summary>
            public string StatusMessage { get; set; }

            /// <summary>
            /// نوع پیام (success, error, warning, info)
            /// </summary>
            public string MessageType { get; set; } = "info";

            /// <summary>
            /// آیا نمایش جزئیات خدمت فعال است؟
            /// </summary>
            public bool ShowServiceDetails { get; set; }

            /// <summary>
            /// آیا نمایش جزئیات بیمه فعال است؟
            /// </summary>
            public bool ShowInsuranceDetails { get; set; }

            #endregion

            #region Constructor

            public ReceptionFormComponentViewModel()
            {
                ReceptionDate = DateTime.Now;
                ReceptionTime = DateTime.Now.TimeOfDay;
                ReceptionType = "Normal";
                Priority = "Normal";
                IsEmergency = false;
                IsLoading = false;
                IsValid = false;
                IsSaved = false;
                ShowServiceDetails = false;
                ShowInsuranceDetails = false;
                StatusMessage = "آماده";
                MessageType = "info";
            }

            #endregion
        }

        #endregion

        #region Reception History Component

        /// <summary>
        /// ViewModel برای کامپوننت تاریخچه پذیرش‌ها
        /// </summary>
        public class ReceptionHistoryComponentViewModel
        {
            #region Search Parameters

            /// <summary>
            /// شناسه بیمار
            /// </summary>
            [Display(Name = "بیمار")]
            public int? PatientId { get; set; }

            /// <summary>
            /// تاریخ شروع
            /// </summary>
            [Display(Name = "از تاریخ")]
            public DateTime? StartDate { get; set; }

            /// <summary>
            /// تاریخ پایان
            /// </summary>
            [Display(Name = "تا تاریخ")]
            public DateTime? EndDate { get; set; }

            /// <summary>
            /// وضعیت پذیرش
            /// </summary>
            [Display(Name = "وضعیت")]
            public string Status { get; set; }

            /// <summary>
            /// نوع پذیرش
            /// </summary>
            [Display(Name = "نوع پذیرش")]
            public string ReceptionType { get; set; }

            /// <summary>
            /// اولویت
            /// </summary>
            [Display(Name = "اولویت")]
            public string Priority { get; set; }

            /// <summary>
            /// آیا اورژانس است؟
            /// </summary>
            [Display(Name = "اورژانس")]
            public bool? IsEmergency { get; set; }

            /// <summary>
            /// شماره صفحه
            /// </summary>
            [Display(Name = "شماره صفحه")]
            [Range(1, int.MaxValue, ErrorMessage = "شماره صفحه باید مثبت باشد")]
            public int PageNumber { get; set; } = 1;

            /// <summary>
            /// اندازه صفحه
            /// </summary>
            [Display(Name = "اندازه صفحه")]
            [Range(1, 50, ErrorMessage = "اندازه صفحه باید بین 1 تا 50 باشد")]
            public int PageSize { get; set; } = 10;

            #endregion

            #region Search Results

            /// <summary>
            /// نتایج جستجو
            /// </summary>
            public List<ReceptionHistoryItemViewModel> HistoryItems { get; set; } = new List<ReceptionHistoryItemViewModel>();

            /// <summary>
            /// تعداد کل نتایج
            /// </summary>
            public int TotalCount { get; set; }

            /// <summary>
            /// آیا جستجو انجام شده است؟
            /// </summary>
            public bool HasSearched { get; set; }

            /// <summary>
            /// آیا در حال جستجو است؟
            /// </summary>
            public bool IsSearching { get; set; }

            #endregion

            #region UI Helper Properties

            /// <summary>
            /// لیست وضعیت‌ها برای Dropdown
            /// </summary>
            public List<SelectListItem> StatusList { get; set; } = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "همه", Selected = true },
                new SelectListItem { Value = "Pending", Text = "در انتظار" },
                new SelectListItem { Value = "Completed", Text = "تکمیل شده" },
                new SelectListItem { Value = "Cancelled", Text = "لغو شده" }
            };

            /// <summary>
            /// لیست انواع پذیرش برای Dropdown
            /// </summary>
            public List<SelectListItem> ReceptionTypeList { get; set; } = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "همه", Selected = true },
                new SelectListItem { Value = "Normal", Text = "عادی" },
                new SelectListItem { Value = "Emergency", Text = "اورژانس" },
                new SelectListItem { Value = "Special", Text = "ویژه" },
                new SelectListItem { Value = "Online", Text = "آنلاین" }
            };

            /// <summary>
            /// لیست اولویت‌ها برای Dropdown
            /// </summary>
            public List<SelectListItem> PriorityList { get; set; } = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "همه", Selected = true },
                new SelectListItem { Value = "Normal", Text = "عادی" },
                new SelectListItem { Value = "High", Text = "بالا" },
                new SelectListItem { Value = "Urgent", Text = "فوری" }
            };

            #endregion

            #region Component State

            /// <summary>
            /// آیا نمایش فیلترها فعال است؟
            /// </summary>
            public bool ShowFilters { get; set; }

            /// <summary>
            /// آیا نمایش جزئیات فعال است؟
            /// </summary>
            public bool ShowDetails { get; set; }

            /// <summary>
            /// شناسه پذیرش انتخاب شده برای جزئیات
            /// </summary>
            public int? SelectedReceptionId { get; set; }

            /// <summary>
            /// پیام وضعیت
            /// </summary>
            public string StatusMessage { get; set; }

            /// <summary>
            /// نوع پیام (success, error, warning, info)
            /// </summary>
            public string MessageType { get; set; } = "info";

            #endregion

            #region Constructor

            public ReceptionHistoryComponentViewModel()
            {
                StartDate = DateTime.Today.AddDays(-30);
                EndDate = DateTime.Today;
                PageNumber = 1;
                PageSize = 10;
                HasSearched = false;
                IsSearching = false;
                ShowFilters = false;
                ShowDetails = false;
                StatusMessage = "آماده";
                MessageType = "info";
            }

            #endregion
        }

        #endregion

        #region Reception History Item

        /// <summary>
        /// ViewModel برای آیتم تاریخچه پذیرش
        /// </summary>
        public class ReceptionHistoryItemViewModel
        {
            /// <summary>
            /// شناسه پذیرش
            /// </summary>
            public int ReceptionId { get; set; }

            /// <summary>
            /// تاریخ پذیرش
            /// </summary>
            public DateTime ReceptionDate { get; set; }

            /// <summary>
            /// ساعت پذیرش
            /// </summary>
            public TimeSpan ReceptionTime { get; set; }

            /// <summary>
            /// نوع پذیرش
            /// </summary>
            public string ReceptionType { get; set; }

            /// <summary>
            /// متن نوع پذیرش
            /// </summary>
            public string ReceptionTypeText { get; set; }

            /// <summary>
            /// اولویت
            /// </summary>
            public string Priority { get; set; }

            /// <summary>
            /// متن اولویت
            /// </summary>
            public string PriorityText { get; set; }

            /// <summary>
            /// آیا اورژانس است؟
            /// </summary>
            public bool IsEmergency { get; set; }

            /// <summary>
            /// وضعیت پذیرش
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// متن وضعیت پذیرش
            /// </summary>
            public string StatusText { get; set; }

            /// <summary>
            /// یادداشت‌ها
            /// </summary>
            public string Notes { get; set; }

            /// <summary>
            /// شناسه بیمار
            /// </summary>
            public int PatientId { get; set; }

            /// <summary>
            /// نام بیمار
            /// </summary>
            public string PatientName { get; set; }

            /// <summary>
            /// کد ملی بیمار
            /// </summary>
            public string PatientNationalCode { get; set; }

            /// <summary>
            /// شناسه پزشک
            /// </summary>
            public int DoctorId { get; set; }

            /// <summary>
            /// نام پزشک
            /// </summary>
            public string DoctorName { get; set; }

            /// <summary>
            /// تخصص پزشک
            /// </summary>
            public string DoctorSpecialization { get; set; }

            /// <summary>
            /// شناسه خدمت
            /// </summary>
            public int ServiceId { get; set; }

            /// <summary>
            /// نام خدمت
            /// </summary>
            public string ServiceName { get; set; }

            /// <summary>
            /// مبلغ کل
            /// </summary>
            public decimal TotalAmount { get; set; }

            /// <summary>
            /// سهم بیمه
            /// </summary>
            public decimal InsuranceShare { get; set; }

            /// <summary>
            /// مبلغ قابل پرداخت
            /// </summary>
            public decimal PayableAmount { get; set; }

            /// <summary>
            /// شناسه بیمه
            /// </summary>
            public int? InsuranceId { get; set; }

            /// <summary>
            /// نام بیمه
            /// </summary>
            public string InsuranceName { get; set; }

            /// <summary>
            /// شماره بیمه
            /// </summary>
            public string InsuranceNumber { get; set; }

            /// <summary>
            /// تاریخ انقضای بیمه
            /// </summary>
            public DateTime? InsuranceExpiryDate { get; set; }

            /// <summary>
            /// تاریخ ایجاد
            /// </summary>
            public DateTime CreatedAt { get; set; }

            /// <summary>
            /// تاریخ آخرین ویرایش
            /// </summary>
            public DateTime? UpdatedAt { get; set; }

            /// <summary>
            /// کاربر ایجادکننده
            /// </summary>
            public string CreatedBy { get; set; }

            /// <summary>
            /// کاربر ویرایش‌کننده
            /// </summary>
            public string UpdatedBy { get; set; }
        }

        #endregion





        #region Reception Statistics ViewModel

        /// <summary>
        /// ViewModel برای آمار پذیرش‌ها
        /// </summary>
        public class ReceptionStatisticsViewModel
        {
            /// <summary>
            /// تعداد پذیرش‌های امروز
            /// </summary>
            public int TodayCount { get; set; }

            /// <summary>
            /// تعداد پذیرش‌های این هفته
            /// </summary>
            public int ThisWeekCount { get; set; }

            /// <summary>
            /// تعداد پذیرش‌های این ماه
            /// </summary>
            public int ThisMonthCount { get; set; }

            /// <summary>
            /// تعداد پذیرش‌های اورژانس
            /// </summary>
            public int EmergencyCount { get; set; }

            /// <summary>
            /// تعداد پذیرش‌های عادی
            /// </summary>
            public int NormalCount { get; set; }

            /// <summary>
            /// میانگین زمان انتظار
            /// </summary>
            public TimeSpan AverageWaitingTime { get; set; }

            /// <summary>
            /// تعداد پزشکان فعال
            /// </summary>
            public int ActiveDoctorsCount { get; set; }

            /// <summary>
            /// تعداد دپارتمان‌های فعال
            /// </summary>
            public int ActiveDepartmentsCount { get; set; }
        }

        #endregion
    }
}
