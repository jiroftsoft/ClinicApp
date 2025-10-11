using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای کامپوننت تاریخچه پذیرش‌ها
    /// </summary>
    public class ReceptionHistoryComponentViewModel
    {
        [Display(Name = "تاریخ شروع")]
        public string StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public string EndDate { get; set; }

        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        [Display(Name = "نوع پذیرش")]
        public string Type { get; set; }

        [Display(Name = "پزشک")]
        public string Doctor { get; set; }

        [Display(Name = "بیمار")]
        public string Patient { get; set; }

        [Display(Name = "لیست پذیرش‌ها")]
        public List<ReceptionHistoryItemViewModel> Receptions { get; set; } = new List<ReceptionHistoryItemViewModel>();

        [Display(Name = "تعداد کل")]
        public int TotalCount { get; set; }

        [Display(Name = "صفحه فعلی")]
        public int CurrentPage { get; set; } = 1;

        [Display(Name = "تعداد در صفحه")]
        public int PageSize { get; set; } = 10;

        [Display(Name = "تعداد صفحات")]
        public int TotalPages { get; set; }

        [Display(Name = "وضعیت بارگذاری")]
        public string LoadingStatus { get; set; } = "آماده";

        [Display(Name = "آیا در حال بارگذاری")]
        public bool IsLoading { get; set; } = false;
    }

    /// <summary>
    /// ViewModel برای نمایش آیتم تاریخچه پذیرش
    /// </summary>
    public class ReceptionHistoryItemViewModel
    {
        [Display(Name = "شناسه")]
        public int ReceptionId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "کد ملی")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "تاریخ پذیرش")]
        public string ReceptionDate { get; set; }

        [Display(Name = "ساعت پذیرش")]
        public string ReceptionTime { get; set; }

        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        [Display(Name = "نوع")]
        public string Type { get; set; }

        [Display(Name = "اولویت")]
        public string Priority { get; set; }

        [Display(Name = "مجموع مبلغ")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "مبلغ پرداخت شده")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "مبلغ باقی‌مانده")]
        public decimal RemainingAmount { get; set; }

        [Display(Name = "روش پرداخت")]
        public string PaymentMethod { get; set; }

        [Display(Name = "بیمه اولیه")]
        public string PrimaryInsurance { get; set; }

        [Display(Name = "بیمه تکمیلی")]
        public string SecondaryInsurance { get; set; }

        [Display(Name = "یادداشت‌ها")]
        public string Notes { get; set; }

        [Display(Name = "خدمات")]
        public List<ReceptionServiceItemViewModel> Services { get; set; } = new List<ReceptionServiceItemViewModel>();
    }

    /// <summary>
    /// ViewModel برای نمایش خدمت در پذیرش
    /// </summary>
    public class ReceptionServiceItemViewModel
    {
        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        [Display(Name = "نام خدمت")]
        public string ServiceName { get; set; }

        [Display(Name = "دسته‌بندی")]
        public string Category { get; set; }

        [Display(Name = "تعداد")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "قیمت واحد")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "مجموع")]
        public decimal TotalPrice { get; set; }
    }
}
