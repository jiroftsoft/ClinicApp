using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel اصلی برای فرم پذیرش آکاردئون - فوق حرفه‌ای
    /// </summary>
    public class ReceptionAccordionViewModel
    {
        /// <summary>
        /// شناسه پذیرش (در صورت ویرایش)
        /// </summary>
        public int? ReceptionId { get; set; }

        /// <summary>
        /// بخش اطلاعات بیمار
        /// </summary>
        public PatientAccordionViewModel Patient { get; set; } = new PatientAccordionViewModel();

        /// <summary>
        /// بخش اطلاعات بیمه
        /// </summary>
        public InsuranceAccordionViewModel Insurance { get; set; } = new InsuranceAccordionViewModel();

        /// <summary>
        /// بخش دپارتمان و پزشک
        /// </summary>
        public DepartmentAccordionViewModel Department { get; set; } = new DepartmentAccordionViewModel();

        /// <summary>
        /// بخش خدمات
        /// </summary>
        public ServiceAccordionViewModel Service { get; set; } = new ServiceAccordionViewModel();

        /// <summary>
        /// بخش پرداخت
        /// </summary>
        public PaymentAccordionViewModel Payment { get; set; } = new PaymentAccordionViewModel();

        /// <summary>
        /// وضعیت کلی فرم
        /// </summary>
        public FormStatusViewModel FormStatus { get; set; } = new FormStatusViewModel();

        /// <summary>
        /// آیا فرم آماده ثبت است؟
        /// </summary>
        public bool IsReadyForSubmission => 
            Patient.IsValid && 
            Insurance.IsValid && 
            Department.IsValid && 
            Service.IsValid && 
            Payment.IsValid;
    }

    /// <summary>
    /// ViewModel بخش اطلاعات بیمار
    /// </summary>
    public class PatientAccordionViewModel
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int? PatientId { get; set; }

        /// <summary>
        /// کد ملی بیمار
        /// </summary>
        [Required(ErrorMessage = "کد ملی الزامی است")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
        public string NationalCode { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        [Required(ErrorMessage = "نام الزامی است")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "نام باید بین 2 تا 50 کاراکتر باشد")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی بیمار
        /// </summary>
        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "نام خانوادگی باید بین 2 تا 50 کاراکتر باشد")]
        public string LastName { get; set; }

        /// <summary>
        /// نام پدر بیمار
        /// </summary>
        [StringLength(50, ErrorMessage = "نام پدر نمی‌تواند بیش از 50 کاراکتر باشد")]
        public string FatherName { get; set; }

        /// <summary>
        /// تاریخ تولد
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// سن بیمار
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// جنسیت
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// شماره تلفن
        /// </summary>
        [StringLength(13, MinimumLength = 11, ErrorMessage = "شماره تلفن نامعتبر است")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// آدرس
        /// </summary>
        [StringLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string Address { get; set; }

        /// <summary>
        /// آیا بیمار یافت شده؟
        /// </summary>
        public bool IsPatientFound { get; set; }

        /// <summary>
        /// آیا بخش معتبر است؟
        /// </summary>
        public bool IsValid => 
            !string.IsNullOrEmpty(NationalCode) && 
            !string.IsNullOrEmpty(FirstName) && 
            !string.IsNullOrEmpty(LastName);

        /// <summary>
        /// پیام وضعیت
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// کلاس CSS وضعیت
        /// </summary>
        public string StatusCssClass { get; set; }

        /// <summary>
        /// نام کامل بیمار
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    /// <summary>
    /// ViewModel بخش اطلاعات بیمه
    /// </summary>
    public class InsuranceAccordionViewModel
    {
        /// <summary>
        /// بیمه پایه
        /// </summary>
        public PrimaryInsuranceViewModel PrimaryInsurance { get; set; } = new PrimaryInsuranceViewModel();

        /// <summary>
        /// بیمه تکمیلی
        /// </summary>
        public SupplementaryInsuranceViewModel SupplementaryInsurance { get; set; } = new SupplementaryInsuranceViewModel();

        /// <summary>
        /// آیا بخش معتبر است؟
        /// </summary>
        public bool IsValid => PrimaryInsurance.IsValid;

        /// <summary>
        /// آیا بیمه تکمیلی دارد؟
        /// </summary>
        public bool HasSupplementaryInsurance => SupplementaryInsurance.IsValid;

        /// <summary>
        /// پیام وضعیت
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// کلاس CSS وضعیت
        /// </summary>
        public string StatusCssClass { get; set; }
    }

    /// <summary>
    /// ViewModel بیمه پایه
    /// </summary>
    public class PrimaryInsuranceViewModel
    {
        /// <summary>
        /// شناسه بیمه‌گذار
        /// </summary>
        public int? ProviderId { get; set; }

        /// <summary>
        /// نام بیمه‌گذار
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// شناسه طرح بیمه
        /// </summary>
        public int? PlanId { get; set; }

        /// <summary>
        /// نام طرح بیمه
        /// </summary>
        public string PlanName { get; set; }

        /// <summary>
        /// شماره بیمه‌نامه
        /// </summary>
        public string PolicyNumber { get; set; }

        /// <summary>
        /// شماره کارت بیمه
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// آیا معتبر است؟
        /// </summary>
        public bool IsValid => ProviderId.HasValue && PlanId.HasValue;
    }

    /// <summary>
    /// ViewModel بیمه تکمیلی
    /// </summary>
    public class SupplementaryInsuranceViewModel
    {
        /// <summary>
        /// شناسه بیمه‌گذار
        /// </summary>
        public int? ProviderId { get; set; }

        /// <summary>
        /// نام بیمه‌گذار
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// شناسه طرح بیمه
        /// </summary>
        public int? PlanId { get; set; }

        /// <summary>
        /// نام طرح بیمه
        /// </summary>
        public string PlanName { get; set; }

        /// <summary>
        /// شماره بیمه‌نامه
        /// </summary>
        public string PolicyNumber { get; set; }

        /// <summary>
        /// شماره کارت بیمه
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// آیا معتبر است؟
        /// </summary>
        public bool IsValid => ProviderId.HasValue && PlanId.HasValue;
    }

    /// <summary>
    /// ViewModel بخش دپارتمان و پزشک
    /// </summary>
    public class DepartmentAccordionViewModel
    {
        /// <summary>
        /// شناسه کلینیک
        /// </summary>
        public int ClinicId { get; set; } = 1;

        /// <summary>
        /// نام کلینیک
        /// </summary>
        public string ClinicName { get; set; } = "کلینیک شفا";

        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        [Required(ErrorMessage = "انتخاب دپارتمان الزامی است")]
        public int? DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "انتخاب پزشک الزامی است")]
        public int? DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        public string DoctorName { get; set; }

        /// <summary>
        /// تخصص پزشک
        /// </summary>
        public string DoctorSpecialty { get; set; }

        /// <summary>
        /// آیا بخش معتبر است؟
        /// </summary>
        public bool IsValid => DepartmentId.HasValue && DoctorId.HasValue;
    }

    /// <summary>
    /// ViewModel بخش خدمات
    /// </summary>
    public class ServiceAccordionViewModel
    {
        /// <summary>
        /// خدمات انتخاب شده
        /// </summary>
        public List<SelectedServiceViewModel> SelectedServices { get; set; } = new List<SelectedServiceViewModel>();

        /// <summary>
        /// مبلغ کل خدمات
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// سهم بیمه
        /// </summary>
        public decimal InsuranceShare { get; set; }

        /// <summary>
        /// سهم بیمار
        /// </summary>
        public decimal PatientShare { get; set; }

        /// <summary>
        /// آیا بخش معتبر است؟
        /// </summary>
        public bool IsValid => SelectedServices.Count > 0;
    }

    /// <summary>
    /// ViewModel خدمت انتخاب شده
    /// </summary>
    public class SelectedServiceViewModel
    {
        /// <summary>
        /// شناسه خدمت
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// کد خدمت
        /// </summary>
        public string ServiceCode { get; set; }

        /// <summary>
        /// نام خدمت
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// مبلغ خدمت
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// تعداد
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// مبلغ کل
        /// </summary>
        public decimal TotalAmount => Amount * Quantity;
    }

    /// <summary>
    /// ViewModel بخش پرداخت
    /// </summary>
    public class PaymentAccordionViewModel
    {
        /// <summary>
        /// نوع پرداخت
        /// </summary>
        [Required(ErrorMessage = "انتخاب نوع پرداخت الزامی است")]
        public string PaymentType { get; set; }

        /// <summary>
        /// مبلغ قابل پرداخت
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// شماره تراکنش
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// وضعیت پرداخت
        /// </summary>
        public string PaymentStatus { get; set; } = "Pending";

        /// <summary>
        /// آیا بخش معتبر است؟
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(PaymentType) && Amount > 0;
    }

    /// <summary>
    /// ViewModel وضعیت فرم
    /// </summary>
    public class FormStatusViewModel
    {
        /// <summary>
        /// وضعیت کلی فرم
        /// </summary>
        public string Status { get; set; } = "Ready";

        /// <summary>
        /// پیام وضعیت
        /// </summary>
        public string Message { get; set; } = "فرم آماده است";

        /// <summary>
        /// آیا فرم کامل است؟
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// تعداد بخش‌های تکمیل شده
        /// </summary>
        public int CompletedSections { get; set; }

        /// <summary>
        /// تعداد کل بخش‌ها
        /// </summary>
        public int TotalSections { get; set; } = 5;
    }
}
