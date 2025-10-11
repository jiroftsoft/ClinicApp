using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Reception;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace ClinicApp.ViewModels.Reception
{
  

    /// <summary>
    /// ViewModel برای فرم پذیرش اصلی در محل.
    /// این ViewModel پیچیده‌ترین ViewModel است که تمام داده‌ها را برای پذیرش جدید مدیریت می‌کند.
    /// </summary>
    public class ReceptionCreateViewModel
    {
        // اطلاعات بیمار
        [Required(ErrorMessage = "باید یک بیمار انتخاب شود.")]
        [Display(Name = "بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientFullName { get; set; } // برای نمایش نام بعد از جستجو

        // فیلدهای اطلاعات بیمار برای نمایش و ویرایش
        [Display(Name = "نام")]
        [StringLength(50, ErrorMessage = "نام نمی‌تواند بیش از ۵۰ کاراکتر باشد.")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [StringLength(50, ErrorMessage = "نام خانوادگی نمی‌تواند بیش از ۵۰ کاراکتر باشد.")]
        public string LastName { get; set; }

        [Display(Name = "کد ملی")]
        [StringLength(10, ErrorMessage = "کد ملی باید ۱۰ رقم باشد.")]
        public string NationalCode { get; set; }

        [Display(Name = "شماره تلفن")]
        [StringLength(15, ErrorMessage = "شماره تلفن نمی‌تواند بیش از ۱۵ کاراکتر باشد.")]
        public string PhoneNumber { get; set; }

        [Display(Name = "جنسیت")]
        public int? Gender { get; set; }

        [Display(Name = "تاریخ تولد")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        //[Display(Name = "تاریخ تولد شمسی")]
        //public string BirthDateShamsi { get; set; }

        [Display(Name = "تاریخ تولد شمسی")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public string BirthDateShamsi { get; set; }

        [Display(Name = "آدرس")]
        [StringLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از ۵۰۰ کاراکتر باشد.")]
        public string Address { get; set; }

        // اطلاعات استعلام کمکی
        [Display(Name = "کد ملی برای استعلام")]
        public string NationalCodeForInquiry { get; set; }

        [Display(Name = "تاریخ تولد برای استعلام")]
        public DateTime? BirthDateForInquiry { get; set; }

        [Display(Name = "تاریخ تولد شمسی برای استعلام")]
        public string BirthDateShamsiForInquiry { get; set; }

        [Display(Name = "نتیجه استعلام")]
        public Reception.PatientInquiryViewModel InquiryResult { get; set; }

        [Display(Name = "وضعیت استعلام")]
        public bool IsInquiryCompleted { get; set; }

        // Properties for Controller
        [Display(Name = "تاریخ پذیرش")]
        public DateTime ReceptionDate { get; set; }

        [Display(Name = "تاریخ پذیرش (شمسی)")]
        public string ReceptionDateShamsi { get; set; }

        [Display(Name = "آیا اورژانس است")]
        public bool IsEmergency { get; set; }

        [Display(Name = "آیا پذیرش آنلاین است")]
        public bool IsOnlineReception { get; set; }

        [Display(Name = "نوع پذیرش")]
        public ReceptionType Type { get; set; }

        [Display(Name = "وضعیت پذیرش")]
        public ReceptionStatus Status { get; set; }

        [Display(Name = "اولویت")]
        public AppointmentPriority Priority { get; set; }

        // اطلاعات پزشک
        [Required(ErrorMessage = "باید یک پزشک انتخاب شود.")]
        [Display(Name = "پزشک معالج")]
        public int DoctorId { get; set; }
        public IEnumerable<SelectListItem> DoctorList { get; set; } // برای منوی انتخاب پزشک

        // اطلاعات خدمت (برای Controller)
        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        // اطلاعات خدمات (پشتیبانی از افزودن چندین خدمت)
        [Required(ErrorMessage = "باید حداقل یک خدمت انتخاب شود.")]
        [Display(Name = "خدمات")]
        public List<int> SelectedServiceIds { get; set; }
        public IEnumerable<SelectListItem> ServiceList { get; set; } // برای انتخاب خدمات

        // اطلاعات پرداخت
        [Required]
        [Display(Name = "مجموع مبلغ")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "مجموع مبلغ نمی‌تواند منفی باشد.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "لطفاً یک روش پرداخت انتخاب کنید.")]
        [Display(Name = "روش پرداخت")]
        public PaymentMethod PaymentMethod { get; set; }
        
        // برای منوی انتخاب روش پرداخت
        public IEnumerable<SelectListItem> PaymentMethodList { get; set; }

        [Display(Name = "شناسه تراکنش POS")]
        public string PosTransactionId { get; set; }

        [Display(Name = "مبلغ پرداخت شده")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ پرداخت شده نمی‌تواند منفی باشد.")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "مبلغ باقی‌مانده")]
        [DataType(DataType.Currency)]
        public decimal RemainingAmount { get; set; }

        // اطلاعات بیمه
        [Display(Name = "بیمه اولیه")]
        public int? PrimaryInsuranceId { get; set; }

        [Display(Name = "بیمه تکمیلی")]
        public int? SecondaryInsuranceId { get; set; }

        [Display(Name = "نام بیمه اولیه")]
        public string PrimaryInsuranceName { get; set; }

        [Display(Name = "نام بیمه تکمیلی")]
        public string SecondaryInsuranceName { get; set; }

        [Display(Name = "شماره بیمه")]
        [StringLength(20, ErrorMessage = "شماره بیمه نمی‌تواند بیش از ۲۰ کاراکتر باشد.")]
        public string InsuranceNumber { get; set; }

        [Display(Name = "سهم بیمه")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "سهم بیمه نمی‌تواند منفی باشد.")]
        public decimal InsuranceShare { get; set; }

        [Display(Name = "سهم بیمار")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "سهم بیمار نمی‌تواند منفی باشد.")]
        public decimal PatientShare { get; set; }

        [Display(Name = "یادداشت‌ها")]
        [StringLength(1000, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از ۱۰۰۰ کاراکتر باشد.")]
        public string Notes { get; set; }

        // Properties for lookup lists
        public List<SelectListItem> Doctors { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Patients { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Services { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ServiceCategories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentMethods { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> InsuranceProviders { get; set; } = new List<SelectListItem>();

        public ReceptionCreateViewModel()
        {
            SelectedServiceIds = new List<int>();
            DoctorList = new List<SelectListItem>();
            ServiceList = new List<SelectListItem>();
            PaymentMethodList = new List<SelectListItem>();
            ReceptionDate = DateTime.Now;
            IsInquiryCompleted = false;
            TotalAmount = 0;
            PaidAmount = 0;
            InsuranceShare = 0;
            PatientShare = 0;
            Type = ReceptionType.Normal;
            InquiryResult = new Reception.PatientInquiryViewModel();
        }
    }

    /// <summary>
    /// ViewModel برای ویرایش پذیرش موجود
    /// </summary>
    public class ReceptionEditViewModel
    {
        public int ReceptionId { get; set; }

        // اطلاعات بیمار
        [Required(ErrorMessage = "باید یک بیمار انتخاب شود.")]
        [Display(Name = "بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientFullName { get; set; }

        // اطلاعات پزشک
        [Required(ErrorMessage = "باید یک پزشک انتخاب شود.")]
        [Display(Name = "پزشک معالج")]
        public int DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorFullName { get; set; }

        // اطلاعات خدمات
        [Required(ErrorMessage = "باید حداقل یک خدمت انتخاب شود.")]
        [Display(Name = "خدمات")]
        public List<int> SelectedServiceIds { get; set; }

        [Display(Name = "خدمات انتخاب شده")]
        public string SelectedServicesNames { get; set; }

        // اطلاعات پذیرش
        [Required(ErrorMessage = "تاریخ پذیرش الزامی است.")]
        [Display(Name = "تاریخ پذیرش")]
        [DataType(DataType.DateTime)]
        public DateTime ReceptionDate { get; set; }

        [Display(Name = "تاریخ پذیرش (شمسی)")]
        public string ReceptionDateShamsi { get; set; }

        [Display(Name = "نوع پذیرش")]
        public ReceptionType Type { get; set; }

        [Display(Name = "اولویت")]
        public AppointmentPriority Priority { get; set; }

        [Display(Name = "آیا اورژانس است")]
        public bool IsEmergency { get; set; }

        [Display(Name = "آیا پذیرش آنلاین است")]
        public bool IsOnlineReception { get; set; }

        [Display(Name = "یادداشت‌ها")]
        [StringLength(1000, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از ۱۰۰۰ کاراکتر باشد.")]
        public string Notes { get; set; }

        // اطلاعات پرداخت
        [Required]
        [Display(Name = "مجموع مبلغ")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "مجموع مبلغ نمی‌تواند منفی باشد.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "لطفاً یک روش پرداخت انتخاب کنید.")]
        [Display(Name = "روش پرداخت")]
        public PaymentMethod PaymentMethod { get; set; }

        [Display(Name = "شناسه تراکنش POS")]
        [StringLength(50, ErrorMessage = "شناسه تراکنش نمی‌تواند بیش از ۵۰ کاراکتر باشد.")]
        public string PosTransactionId { get; set; }

        [Display(Name = "مبلغ پرداخت شده")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ پرداخت شده نمی‌تواند منفی باشد.")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "مبلغ باقی‌مانده")]
        [DataType(DataType.Currency)]
        public decimal RemainingAmount { get; set; }

        // اطلاعات بیمه
        [Display(Name = "بیمه اولیه")]
        public int? PrimaryInsuranceId { get; set; }

        [Display(Name = "بیمه تکمیلی")]
        public int? SecondaryInsuranceId { get; set; }

        [Display(Name = "سهم بیمه")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "سهم بیمه نمی‌تواند منفی باشد.")]
        public decimal InsuranceShare { get; set; }

        [Display(Name = "سهم بیمار")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "سهم بیمار نمی‌تواند منفی باشد.")]
        public decimal PatientShare { get; set; }

        // اطلاعات وضعیت
        [Display(Name = "وضعیت پذیرش")]
        public ReceptionStatus Status { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        // لیست‌های کمکی
        public IEnumerable<SelectListItem> PatientList { get; set; }
        public IEnumerable<SelectListItem> DoctorList { get; set; }
        public IEnumerable<SelectListItem> ServiceList { get; set; }
        public IEnumerable<SelectListItem> InsuranceList { get; set; }
        public IEnumerable<SelectListItem> PaymentMethodList { get; set; }

        // Properties for lookup lists (for compatibility)
        public List<SelectListItem> Doctors { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Patients { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Services { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ServiceCategories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentMethods { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> InsuranceProviders { get; set; } = new List<SelectListItem>();

        public ReceptionEditViewModel()
        {
            SelectedServiceIds = new List<int>();
            PatientList = new List<SelectListItem>();
            DoctorList = new List<SelectListItem>();
            ServiceList = new List<SelectListItem>();
            InsuranceList = new List<SelectListItem>();
            PaymentMethodList = new List<SelectListItem>();
            CreatedAt = DateTime.Now;
            TotalAmount = 0;
            PaidAmount = 0;
            InsuranceShare = 0;
            PatientShare = 0;
            Status = ReceptionStatus.Pending;
            Type = ReceptionType.Normal;
        }
    }

    /// <summary>
    /// ViewModel برای جستجو و فیلتر پذیرش‌ها
    /// </summary>
    public class ReceptionSearchViewModel
    {
        [Display(Name = "کد ملی بیمار")]
        [StringLength(10, ErrorMessage = "کد ملی باید ۱۰ رقم باشد.")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "نام بیمار")]
        [StringLength(100, ErrorMessage = "نام بیمار نمی‌تواند بیش از ۱۰۰ کاراکتر باشد.")]
        public string PatientName { get; set; }

        [Display(Name = "نام پزشک")]
        [StringLength(100, ErrorMessage = "نام پزشک نمی‌تواند بیش از ۱۰۰ کاراکتر باشد.")]
        public string DoctorName { get; set; }

        [Display(Name = "تاریخ شروع")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ شروع (شمسی)")]
        public string StartDateShamsi { get; set; }

        [Display(Name = "تاریخ پایان")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "تاریخ پایان (شمسی)")]
        public string EndDateShamsi { get; set; }

        [Display(Name = "وضعیت پذیرش")]
        public ReceptionStatus? Status { get; set; }

        [Display(Name = "نوع پذیرش")]
        public ReceptionType? Type { get; set; }

        [Display(Name = "آیا اورژانس است")]
        public bool? IsEmergency { get; set; }

        [Display(Name = "آیا پذیرش آنلاین است")]
        public bool? IsOnlineReception { get; set; }

        [Display(Name = "عبارت جستجو")]
        public string SearchTerm { get; set; }

        [Display(Name = "مبلغ حداقل")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ حداقل نمی‌تواند منفی باشد.")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "مبلغ حداکثر")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ حداکثر نمی‌تواند منفی باشد.")]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "روش پرداخت")]
        public PaymentMethod? PaymentMethod { get; set; }

        [Display(Name = "بیمه")]
        public int? InsuranceId { get; set; }

        // اطلاعات صفحه‌بندی
        [Display(Name = "شماره صفحه")]
        [Range(1, int.MaxValue, ErrorMessage = "شماره صفحه باید بزرگتر از صفر باشد.")]
        public int PageNumber { get; set; } = 1;

        [Display(Name = "اندازه صفحه")]
        [Range(1, 100, ErrorMessage = "اندازه صفحه باید بین ۱ تا ۱۰۰ باشد.")]
        public int PageSize { get; set; } = 10;

        [Display(Name = "مرتب‌سازی بر اساس")]
        public string SortBy { get; set; } = "ReceptionDate";

        [Display(Name = "ترتیب مرتب‌سازی")]
        public string SortOrder { get; set; } = "desc";

        // لیست‌های کمکی
        public IEnumerable<SelectListItem> StatusList { get; set; }
        public IEnumerable<SelectListItem> TypeList { get; set; }
        public IEnumerable<SelectListItem> PaymentMethodList { get; set; }
        public IEnumerable<SelectListItem> InsuranceList { get; set; }

        public ReceptionSearchViewModel()
        {
            StatusList = new List<SelectListItem>();
            TypeList = new List<SelectListItem>();
            PaymentMethodList = new List<SelectListItem>();
            InsuranceList = new List<SelectListItem>();
            PageNumber = 1;
            PageSize = 10;
            SortBy = "ReceptionDate";
            SortOrder = "desc";
            StartDate = DateTime.Now.AddDays(-30);
            EndDate = DateTime.Now;
        }
    }

    /// <summary>
    /// ViewModel برای آمار پزشکان در پذیرش
    /// </summary>
    public class ReceptionDoctorStatsViewModel
    {
        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "تخصص")]
        public string Specialty { get; set; }

        [Display(Name = "تاریخ")]
        public DateTime Date { get; set; }

        [Display(Name = "تعداد پذیرش‌ها")]
        public int ReceptionCount { get; set; }

        [Display(Name = "تعداد پذیرش‌ها (سازگاری)")]
        public int ReceptionsCount { get; set; }

        [Display(Name = "تعداد پذیرش‌ها (سازگاری)")]
        public int Count => ReceptionCount;

        [Display(Name = "تعداد پذیرش‌های تکمیل شده")]
        public int CompletedReceptions { get; set; }

        [Display(Name = "تعداد پذیرش‌های در انتظار")]
        public int PendingReceptions { get; set; }

        [Display(Name = "تعداد پذیرش‌های لغو شده")]
        public int CancelledReceptions { get; set; }

        [Display(Name = "تعداد پذیرش‌های در حال انجام")]
        public int InProgressReceptions { get; set; }

        [Display(Name = "تعداد پذیرش‌های اورژانس")]
        public int EmergencyReceptions { get; set; }

        [Display(Name = "تعداد پذیرش‌های آنلاین")]
        public int OnlineReceptions { get; set; }

        [Display(Name = "تعداد پذیرش‌های عادی")]
        public int NormalReceptions { get; set; }

        [Display(Name = "مجموع درآمد")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "میانگین درآمد")]
        public decimal AverageRevenue { get; set; }

        [Display(Name = "میانگین درآمد هر پذیرش")]
        public decimal AverageRevenuePerReception { get; set; }

        [Display(Name = "درآمد نقدی")]
        public decimal CashPayments { get; set; }

        [Display(Name = "درآمد کارتی")]
        public decimal CardPayments { get; set; }

        [Display(Name = "درآمد آنلاین")]
        public decimal OnlinePayments { get; set; }

        [Display(Name = "درآمد بیمه")]
        public decimal InsurancePayments { get; set; }

        [Display(Name = "درصد تکمیل")]
        public decimal CompletionRate { get; set; }

        [Display(Name = "درصد لغو")]
        public decimal CancellationRate { get; set; }

        [Display(Name = "درصد اورژانس")]
        public decimal EmergencyRate { get; set; }

        [Display(Name = "میانگین زمان انتظار")]
        public string AverageWaitingTime { get; set; }

        [Display(Name = "نرخ رضایت")]
        public decimal SatisfactionRate { get; set; }
    }

    /// <summary>
    /// ViewModel برای گزارش‌گیری از پذیرش‌ها
    /// </summary>
    public class ReceptionReportViewModel
    {
        [Display(Name = "عنوان گزارش")]
        [Required(ErrorMessage = "عنوان گزارش الزامی است.")]
        [StringLength(200, ErrorMessage = "عنوان گزارش نمی‌تواند بیش از ۲۰۰ کاراکتر باشد.")]
        public string ReportTitle { get; set; }

        [Display(Name = "نوع گزارش")]
        [Required(ErrorMessage = "نوع گزارش الزامی است.")]
        public ReportType ReportType { get; set; }

        [Display(Name = "تاریخ شروع")]
        [Required(ErrorMessage = "تاریخ شروع الزامی است.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        [Required(ErrorMessage = "تاریخ پایان الزامی است.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "گروه‌بندی بر اساس")]
        public string GroupBy { get; set; } = "Date";

        [Display(Name = "فیلتر وضعیت")]
        public ReceptionStatus? StatusFilter { get; set; }

        [Display(Name = "فیلتر نوع")]
        public ReceptionType? TypeFilter { get; set; }

        [Display(Name = "فیلتر پزشک")]
        public int? DoctorFilter { get; set; }

        [Display(Name = "فیلتر بیمه")]
        public int? InsuranceFilter { get; set; }

        [Display(Name = "شامل اورژانس")]
        public bool IncludeEmergency { get; set; } = true;

        [Display(Name = "شامل پذیرش آنلاین")]
        public bool IncludeOnline { get; set; } = true;

        [Display(Name = "فرمت خروجی")]
        public string OutputFormat { get; set; } = "PDF";

        // نتایج گزارش
        public List<ReceptionReportDataViewModel> ReportData { get; set; }
        public ReceptionReportSummaryViewModel Summary { get; set; }

        // لیست‌های کمکی
        public IEnumerable<SelectListItem> ReportTypeList { get; set; }
        public IEnumerable<SelectListItem> GroupByList { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
        public IEnumerable<SelectListItem> TypeList { get; set; }
        public IEnumerable<SelectListItem> DoctorList { get; set; }
        public IEnumerable<SelectListItem> InsuranceList { get; set; }
        public IEnumerable<SelectListItem> OutputFormatList { get; set; }

        public ReceptionReportViewModel()
        {
            ReportData = new List<ReceptionReportDataViewModel>();
            Summary = new ReceptionReportSummaryViewModel();
            ReportTypeList = new List<SelectListItem>();
            GroupByList = new List<SelectListItem>();
            StatusList = new List<SelectListItem>();
            TypeList = new List<SelectListItem>();
            DoctorList = new List<SelectListItem>();
            InsuranceList = new List<SelectListItem>();
            OutputFormatList = new List<SelectListItem>();
            StartDate = DateTime.Now.AddDays(-30);
            EndDate = DateTime.Now;
            GroupBy = "Date";
            IncludeEmergency = true;
            IncludeOnline = true;
            OutputFormat = "PDF";
            ReportType = ReportType.Daily;
        }
    }

    /// <summary>
    /// ViewModel برای داده‌های گزارش پذیرش
    /// </summary>
    public class ReceptionReportDataViewModel
    {
        [Display(Name = "تاریخ")]
        public string Date { get; set; }

        [Display(Name = "تعداد پذیرش")]
        public int ReceptionCount { get; set; }

        [Display(Name = "مجموع مبلغ")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "میانگین مبلغ")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal AverageAmount { get; set; }

        [Display(Name = "تعداد اورژانس")]
        public int EmergencyCount { get; set; }

        [Display(Name = "تعداد آنلاین")]
        public int OnlineCount { get; set; }

        [Display(Name = "تعداد پرداخت نقدی")]
        public int CashPaymentCount { get; set; }

        [Display(Name = "تعداد پرداخت کارتی")]
        public int CardPaymentCount { get; set; }

        [Display(Name = "تعداد پرداخت بیمه")]
        public int InsurancePaymentCount { get; set; }

        public ReceptionReportDataViewModel()
        {
            // Constructor for initialization if needed
            Date = DateTime.Now.ToString("yyyy-MM-dd");
            ReceptionCount = 0;
            TotalAmount = 0;
            AverageAmount = 0;
            EmergencyCount = 0;
            OnlineCount = 0;
            CashPaymentCount = 0;
            CardPaymentCount = 0;
            InsurancePaymentCount = 0;
        }
    }

    /// <summary>
    /// ViewModel برای خلاصه گزارش پذیرش
    /// </summary>
    public class ReceptionReportSummaryViewModel
    {
        [Display(Name = "کل پذیرش‌ها")]
        public int TotalReceptions { get; set; }

        [Display(Name = "کل مبلغ")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "میانگین مبلغ")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal AverageAmount { get; set; }

        [Display(Name = "بیشترین مبلغ")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal MaxAmount { get; set; }

        [Display(Name = "کمترین مبلغ")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal MinAmount { get; set; }

        [Display(Name = "کل اورژانس")]
        public int TotalEmergency { get; set; }

        [Display(Name = "کل آنلاین")]
        public int TotalOnline { get; set; }

        [Display(Name = "درصد اورژانس")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public decimal EmergencyPercentage { get; set; }

        [Display(Name = "درصد آنلاین")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public decimal OnlinePercentage { get; set; }

        public ReceptionReportSummaryViewModel()
        {
            // Constructor for initialization if needed
            TotalReceptions = 0;
            TotalAmount = 0;
            AverageAmount = 0;
            MaxAmount = 0;
            MinAmount = 0;
            TotalEmergency = 0;
            TotalOnline = 0;
            EmergencyPercentage = 0;
            OnlinePercentage = 0;
        }
    }


    /// <summary>
    /// ViewModel دقیق برای نمایش تمام اطلاعات یک پذیرش.
    /// </summary>
    public class ReceptionDetailsViewModel
    {
        public int ReceptionId { get; set; }
        
        public int PatientId { get; set; }
        public int DoctorId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientFullName { get; set; }

        [Display(Name = "کد ملی")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "شماره تماس بیمار")]
        public string PatientPhoneNumber { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorFullName { get; set; }

        [Display(Name = "تخصص پزشک")]
        public string DoctorSpecialization { get; set; }

        [Display(Name = "تاریخ پذیرش")]
        public string ReceptionDate { get; set; }

        [Display(Name = "نوع پذیرش")]
        public string Type { get; set; }

        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        [Display(Name = "مجموع مبلغ")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "مبلغ پرداخت شده")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "مبلغ پرداخت شده")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal AmountPaid => PaidAmount;

        [Display(Name = "مبلغ باقی‌مانده")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal RemainingAmount => TotalAmount - PaidAmount;

        [Display(Name = "روش پرداخت")]
        public string PaymentMethod { get; set; }

        [Display(Name = "شناسه تراکنش POS")]
        public string PosTransactionId { get; set; }

        [Display(Name = "بیمه اولیه")]
        public string PrimaryInsurance { get; set; }

        [Display(Name = "بیمه تکمیلی")]
        public string SecondaryInsurance { get; set; }

        [Display(Name = "سهم بیمه")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal InsuranceShare { get; set; }

        [Display(Name = "سهم بیمار")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PatientShare { get; set; }

        [Display(Name = "یادداشت‌ها")]
        public string Notes { get; set; }

        // Additional properties for compatibility
        [Display(Name = "شماره پذیرش")]
        public string ReceptionNumber { get; set; }

        [Display(Name = "اولویت")]
        public AppointmentPriority Priority { get; set; }

        [Display(Name = "آیا اورژانس است")]
        public bool IsEmergency { get; set; }

        [Display(Name = "آیا پذیرش آنلاین است")]
        public bool IsOnlineReception { get; set; }

        [Display(Name = "آیتم‌های پذیرش")]
        public List<ReceptionItemViewModel> ReceptionItems { get; set; } = new List<ReceptionItemViewModel>();

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "ایجاد کننده")]
        public string CreatedBy { get; set; }

        public List<ReceptionItemViewModel> Services { get; set; }
        public List<ClinicApp.ViewModels.Payment.ReceptionPaymentViewModel> Payments { get; set; }

        public ReceptionDetailsViewModel()
        {
            Services = new List<ReceptionItemViewModel>();
            Payments = new List<ClinicApp.ViewModels.Payment.ReceptionPaymentViewModel>();
            CreatedAt = DateTime.Now;
            TotalAmount = 0;
            PaidAmount = 0;
            InsuranceShare = 0;
            PatientShare = 0;
            Status = "در انتظار";
            Type = "عادی";
        }
    }

    /// <summary>
    /// ViewModel فرعی برای نمایش جزئیات یک خدمت ارائه‌شده در پذیرش.
    /// </summary>
    public class ReceptionItemViewModel
    {
        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        [Display(Name = "نام خدمت")]
        public string ServiceName { get; set; }

        [Display(Name = "دسته‌بندی خدمت")]
        public string ServiceCategory { get; set; }

        [Display(Name = "خدمت")]
        public string ServiceTitle { get; set; }

        [Display(Name = "تعداد")]
        public int Quantity { get; set; }

        [Display(Name = "قیمت واحد")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }

        [Display(Name = "مجموع")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalPrice => Quantity * Price;

        [Display(Name = "مجموع")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal LineTotal => Quantity * Price;

        public ReceptionItemViewModel()
        {
            // Constructor for initialization if needed
        }
    }


    /// <summary>
    /// ViewModel فرعی برای نمایش جزئیات یک پرداخت انجام‌شده برای پذیرش.
    /// </summary>
    public class PaymentViewModel
    {
        [Display(Name = "روش پرداخت")]
        public string PaymentMethod { get; set; }

        [Display(Name = "مبلغ")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Amount { get; set; }

        [Display(Name = "تاریخ پرداخت")]
        public string PaymentDate { get; set; }

        [Display(Name = "شناسه تراکنش")]
        public string TransactionId { get; set; }

        public PaymentViewModel()
        {
            // Constructor for initialization if needed
        }
    }

    /// <summary>
    /// ViewModel برای نمایش و ذخیره اطلاعات قبض چاپی پذیرش.
    /// </summary>
    public class ReceiptPrintViewModel
    {
        public int ReceiptPrintId { get; set; }

        [Required]
        public int ReceptionId { get; set; }

        [Display(Name = "محتوای قبض")]
        [Required(ErrorMessage = "محتوای قبض نمی‌تواند خالی باشد.")]
        public string ReceiptContent { get; set; }

        [Display(Name = "تاریخ چاپ")]
        public DateTime PrintDate { get; set; } = DateTime.Now;

        [Display(Name = "چاپ شده توسط")]
        [MaxLength(250)]
        public string PrintedBy { get; set; }

        // اطلاعات اضافی برای نمایش
        [Display(Name = "نام بیمار")]
        public string PatientFullName { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorFullName { get; set; }

        [Display(Name = "مبلغ کل")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "وضعیت پذیرش")]
        public string ReceptionStatus { get; set; }
        // ✅ افزودن لیست خدمات
        public List<ReceptionItemViewModel> Services { get; set; } = new List<ReceptionItemViewModel>();

        // ✅ افزودن روش پرداخت برای نمایش
        public string PaymentMethod { get; set; }

        public ReceiptPrintViewModel()
        {
            Services = new List<ReceptionItemViewModel>();
        }
    }
    public class ReceptionFilterViewModel
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public ReceptionFilterViewModel()
        {
            // Constructor for initialization if needed
        }
    }

}