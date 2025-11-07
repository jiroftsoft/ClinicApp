using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای لیست پذیرش‌ها (مراجعات قبلی)
    /// </summary>
    public class ReceptionListViewModel
    {
        [Display(Name = "فیلترها")]
        public ReceptionListFilterViewModel Filters { get; set; } = new ReceptionListFilterViewModel();

        [Display(Name = "لیست پذیرش‌ها")]
        public List<ReceptionListItemViewModel> Items { get; set; } = new List<ReceptionListItemViewModel>();

        [Display(Name = "تعداد کل")]
        public int TotalCount { get; set; }

        [Display(Name = "صفحه فعلی")]
        public int CurrentPage { get; set; } = 1;

        [Display(Name = "تعداد در هر صفحه")]
        public int PageSize { get; set; } = 20;

        [Display(Name = "تعداد صفحات")]
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// ViewModel برای فیلترهای لیست پذیرش‌ها
    /// </summary>
    public class ReceptionListFilterViewModel
    {
        [Display(Name = "کد ملی بیمار")]
        public string NationalCode { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "تاریخ از")]
        public string DateFrom { get; set; }

        [Display(Name = "تاریخ تا")]
        public string DateTo { get; set; }

        [Display(Name = "وضعیت")]
        public ReceptionStatus? Status { get; set; }

        [Display(Name = "پزشک")]
        public int? DoctorId { get; set; }

        [Display(Name = "دپارتمان")]
        public int? DepartmentId { get; set; }

        [Display(Name = "دارای بدهی")]
        public bool? HasDebt { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش آیتم پذیرش در لیست
    /// </summary>
    public class ReceptionListItemViewModel
    {
        [Display(Name = "شناسه پذیرش")]
        public int ReceptionId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "کد ملی")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        [Display(Name = "تاریخ پذیرش")]
        public DateTime ReceptionDate { get; set; }

        [Display(Name = "تاریخ پذیرش (شمسی)")]
        public string ReceptionDateShamsi { get; set; }

        [Display(Name = "وضعیت")]
        public ReceptionStatus Status { get; set; }

        [Display(Name = "وضعیت (متن)")]
        public string StatusText { get; set; }

        [Display(Name = "مبلغ کل")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "مبلغ پرداخت شده")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "مبلغ باقی‌مانده")]
        public decimal RemainingAmount { get; set; }

        [Display(Name = "دارای بدهی")]
        public bool HasDebt => RemainingAmount > 0;

        [Display(Name = "روش پرداخت")]
        public PaymentMethod? PaymentMethod { get; set; }

        [Display(Name = "تعداد خدمات")]
        public int ServiceCount { get; set; }

        [Display(Name = "شماره رسید")]
        public string ReceiptNo { get; set; }

        [Display(Name = "یادداشت")]
        public string Notes { get; set; }
    }

    /// <summary>
    /// ViewModel برای اطلاعات دپارتمان در ویرایش
    /// </summary>
    public class DepartmentInfoViewModel
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// ViewModel برای آیتم پذیرش در ویرایش
    /// </summary>
    public class ReceptionItemEditViewModel
    {
        public int ReceptionItemId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal PatientShare { get; set; }
        public decimal InsuranceShare { get; set; }
    }

    /// <summary>
    /// ViewModel برای بیمه در ویرایش
    /// </summary>
    public class ReceptionInsuranceEditViewModel
    {
        public int? BaseInsurancePlanId { get; set; }
        public string BaseInsurancePlanName { get; set; }
        public int? SupplementaryInsurancePlanId { get; set; }
        public string SupplementaryInsurancePlanName { get; set; }
    }

    /// <summary>
    /// ViewModel برای جمع‌ها در ویرایش
    /// </summary>
    public class ReceptionTotalsEditViewModel
    {
        public decimal GrossAmount { get; set; }
        public decimal BaseInsuranceAmount { get; set; }
        public decimal SupplementaryInsuranceAmount { get; set; }
        public decimal PatientAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}

