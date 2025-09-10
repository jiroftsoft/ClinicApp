using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.Reception;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ClinicApp.ViewModels
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

        // اطلاعات استعلام کمکی
        [Display(Name = "کد ملی برای استعلام")]
        public string NationalCodeForInquiry { get; set; }

        [Display(Name = "تاریخ تولد برای استعلام")]
        public DateTime? BirthDateForInquiry { get; set; }

        [Display(Name = "تاریخ تولد شمسی برای استعلام")]
        public string BirthDateShamsiForInquiry { get; set; }

        [Display(Name = "نتیجه استعلام")]
        public PatientInquiryViewModel InquiryResult { get; set; }

        [Display(Name = "وضعیت استعلام")]
        public bool IsInquiryCompleted { get; set; }

        // اطلاعات پزشک
        [Required(ErrorMessage = "باید یک پزشک انتخاب شود.")]
        [Display(Name = "پزشک معالج")]
        public int DoctorId { get; set; }
        public IEnumerable<SelectListItem> DoctorList { get; set; } // برای منوی انتخاب پزشک

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

        [Display(Name = "شناسه تراکنش POS")]
        public string TransactionId { get; set; }

        public ReceptionCreateViewModel()
        {
            SelectedServiceIds = new List<int>();
            DoctorList = new List<SelectListItem>();
            ServiceList = new List<SelectListItem>();
        }
    }

    /// <summary>
    /// ViewModel سبک برای نمایش هر ردیف در لیست سوابق پذیرش.
    /// </summary>
    public class ReceptionIndexViewModel
    {
        public int ReceptionId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientFullName { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorFullName { get; set; }

        [Display(Name = "تاریخ پذیرش")]
        public string ReceptionDate { get; set; }

        [Display(Name = "مجموع مبلغ")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "وضعیت")]
        public string Status { get; set; }
    }

    /// <summary>
    /// ViewModel دقیق برای نمایش تمام اطلاعات یک پذیرش.
    /// </summary>
    public class ReceptionDetailsViewModel
    {
        public int ReceptionId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientFullName { get; set; }

        [Display(Name = "کد ملی")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorFullName { get; set; }

        [Display(Name = "تاریخ پذیرش")]
        public string ReceptionDate { get; set; }

        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        [Display(Name = "مجموع مبلغ")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "مبلغ پرداختی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal AmountPaid { get; set; }

        public List<ReceptionItemViewModel> Services { get; set; }
        public List<PaymentViewModel> Payments { get; set; }

        public ReceptionDetailsViewModel()
        {
            Services = new List<ReceptionItemViewModel>();
            Payments = new List<PaymentViewModel>();
        }
    }

    /// <summary>
    /// ViewModel فرعی برای نمایش جزئیات یک خدمت ارائه‌شده در پذیرش.
    /// </summary>
    public class ReceptionItemViewModel
    {
        [Display(Name = "خدمت")]
        public string ServiceTitle { get; set; }

        [Display(Name = "تعداد")]
        public int Quantity { get; set; }

        [Display(Name = "قیمت واحد")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }

        [Display(Name = "مجموع")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal LineTotal => Quantity * Price;
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
    }
    public class ReceptionFilterViewModel
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

}