using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.Reception;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicApp.ViewModels.Reception
{
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