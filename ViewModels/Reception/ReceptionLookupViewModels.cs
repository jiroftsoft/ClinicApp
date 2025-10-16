using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using ClinicApp.ViewModels.Payment;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای جستجوی بیمار در پذیرش - بهینه‌سازی شده
    /// </summary>
    public class ReceptionPatientLookupViewModel
    {
        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Display(Name = "نام کامل")]
        public string FullName { get; set; }

        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        [Display(Name = "شماره موبایل")]
        public string MobileNumber { get; set; }

        [Display(Name = "تاریخ تولد")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "تاریخ تولد (شمسی)")]
        public string BirthDateShamsi { get; set; }

        [Display(Name = "سن")]
        public int Age { get; set; }

        [Display(Name = "جنسیت")]
        public Gender Gender { get; set; }

        [Display(Name = "جنسیت (متن)")]
        public string GenderText { get; set; }

        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "گروه خونی")]
        public string BloodType { get; set; }

        [Display(Name = "آیا فعال است")]
        public bool IsActive { get; set; }

        [Display(Name = "آیا بیمه فعال دارد")]
        public bool HasActiveInsurance { get; set; }

        [Display(Name = "نام بیمه فعال")]
        public string ActiveInsuranceName { get; set; }

        [Display(Name = "شماره بیمه فعال")]
        public string ActiveInsuranceNumber { get; set; }

        [Display(Name = "تاریخ انقضای بیمه")]
        public DateTime? InsuranceExpiryDate { get; set; }

        [Display(Name = "تاریخ آخرین پذیرش")]
        public DateTime? LastReceptionDate { get; set; }

        [Display(Name = "تاریخ آخرین پذیرش (شمسی)")]
        public string LastReceptionDateShamsi { get; set; }

        [Display(Name = "تعداد پذیرش‌ها")]
        public int ReceptionCount { get; set; }

        [Display(Name = "بیمه تکمیلی")]
        public string SupplementaryInsurance { get; set; }

        [Display(Name = "آلرژی‌ها")]
        public string Allergies { get; set; }

        [Display(Name = "بیماری‌های مزمن")]
        public string ChronicDiseases { get; set; }
    }

    /// <summary>
    /// ViewModel برای جستجوی دسته‌بندی خدمات در پذیرش
    /// </summary>
    public class ReceptionServiceCategoryLookupViewModel
    {
        [Display(Name = "شناسه دسته‌بندی")]
        public int ServiceCategoryId { get; set; }

        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "شناسه دپارتمان")]
        public int DepartmentId { get; set; }

        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        [Display(Name = "آیا فعال است")]
        public bool IsActive { get; set; }

        [Display(Name = "تعداد خدمات")]
        public int ServicesCount { get; set; }

        [Display(Name = "نام نمایشی")]
        public string DisplayName { get; set; }
    }

    /// <summary>
    /// ViewModel برای جستجوی خدمات در پذیرش
    /// </summary>
    public class ReceptionServiceLookupViewModel
    {
        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "قیمت")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }

        [Display(Name = "قیمت پایه")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal BasePrice { get; set; }

        [Display(Name = "دسته‌بندی")]
        public string CategoryName { get; set; }

        [Display(Name = "نام دسته‌بندی")]
        public string ServiceCategoryName { get; set; }

        [Display(Name = "شناسه دسته‌بندی")]
        public int ServiceCategoryId { get; set; }

        [Display(Name = "آیا فعال است")]
        public bool IsActive { get; set; }

        [Display(Name = "مدت زمان (دقیقه)")]
        public int? Duration { get; set; }

        [Display(Name = "نوع خدمت")]
        public string ServiceType { get; set; }

        [Display(Name = "نام نمایشی")]
        public string DisplayName { get; set; }

        [Display(Name = "نمایش قیمت")]
        public string PriceDisplay { get; set; }
    }

    /// <summary>
    /// ViewModel برای جستجوی پزشکان در پذیرش (Legacy - استفاده از فایل جداگانه)
    /// </summary>
    public class LegacyReceptionDoctorLookupViewModel
    {
        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Display(Name = "نام کامل")]
        public string FullName { get; set; }

        [Display(Name = "شماره نظام پزشکی")]
        public string MedicalLicenseNumber { get; set; }

        [Display(Name = "تخصص")]
        public string SpecializationName { get; set; }

        [Display(Name = "شناسه تخصص")]
        public int? SpecializationId { get; set; }

        [Display(Name = "شناسه دپارتمان")]
        public int? DepartmentId { get; set; }

        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "آیا فعال است")]
        public bool IsActive { get; set; }

        [Display(Name = "نام نمایشی")]
        public string DisplayName { get; set; }

        [Display(Name = "آیا در دسترس است")]
        public bool IsAvailable { get; set; }

        [Display(Name = "تعداد پذیرش‌های امروز")]
        public int TodayReceptionsCount { get; set; }

        [Display(Name = "حداکثر پذیرش روزانه")]
        public int? MaxDailyReceptions { get; set; }
    }

    /// <summary>
    /// ViewModel برای جستجوی بیمه‌های بیمار در پذیرش
    /// </summary>
    public class ReceptionPatientInsuranceLookupViewModel
    {
        [Display(Name = "شناسه بیمه بیمار")]
        public int PatientInsuranceId { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام طرح بیمه")]
        public string InsurancePlanName { get; set; }

        [Display(Name = "نام ارائه‌دهنده بیمه")]
        public string InsuranceProviderName { get; set; }

        [Display(Name = "شماره بیمه")]
        public string PolicyNumber { get; set; }

        [Display(Name = "آیا بیمه اصلی است")]
        public bool IsPrimary { get; set; }

        [Display(Name = "اولویت")]
        public int Priority { get; set; }

        [Display(Name = "تاریخ شروع")]
        public DateTime StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "تاریخ شروع (شمسی)")]
        public string StartDateShamsi { get; set; }

        [Display(Name = "تاریخ پایان (شمسی)")]
        public string EndDateShamsi { get; set; }

        [Display(Name = "آیا فعال است")]
        public bool IsActive { get; set; }

        [Display(Name = "درصد پوشش")]
        public decimal? CoveragePercentage { get; set; }

        [Display(Name = "فرانشیز")]
        public decimal? Deductible { get; set; }
    }

    /// <summary>
    /// ViewModel برای محاسبه هزینه‌های پذیرش
    /// </summary>
    public class ReceptionCostCalculationViewModel
    {
        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "تاریخ محاسبه")]
        public DateTime CalculationDate { get; set; }

        [Display(Name = "مجموع هزینه خدمات")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalServiceCost { get; set; }

        [Display(Name = "سهم بیمه")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal InsuranceShare { get; set; }

        [Display(Name = "سهم بیمار")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PatientShare { get; set; }

        [Display(Name = "فرانشیز")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Deductible { get; set; }

        [Display(Name = "درصد پوشش بیمه")]
        public decimal CoveragePercentage { get; set; }

        [Display(Name = "نام بیمه")]
        public string InsuranceName { get; set; }

        [Display(Name = "آیا بیمه فعال است")]
        public bool IsInsuranceActive { get; set; }

        [Display(Name = "جزئیات محاسبه")]
        public List<ServiceCostDetailViewModel> ServiceDetails { get; set; }

        public ReceptionCostCalculationViewModel()
        {
            ServiceDetails = new List<ServiceCostDetailViewModel>();
        }
    }

    /// <summary>
    /// ViewModel برای جزئیات هزینه هر خدمت
    /// </summary>
    public class ServiceCostDetailViewModel
    {
        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        [Display(Name = "نام خدمت")]
        public string ServiceName { get; set; }

        [Display(Name = "قیمت پایه")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal BasePrice { get; set; }

        [Display(Name = "سهم بیمه")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal InsuranceShare { get; set; }

        [Display(Name = "سهم بیمار")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PatientShare { get; set; }

        [Display(Name = "درصد پوشش")]
        public decimal CoveragePercentage { get; set; }
    }

    /// <summary>
    /// ViewModel برای اعتبارسنجی پذیرش
    /// </summary>
    public class ReceptionValidationViewModel
    {
        [Display(Name = "آیا معتبر است")]
        public bool IsValid { get; set; }

        [Display(Name = "پیام اعتبارسنجی")]
        public string ValidationMessage { get; set; }

        [Display(Name = "خطاهای اعتبارسنجی")]
        public List<string> ValidationErrors { get; set; }

        [Display(Name = "هشدارها")]
        public List<string> Warnings { get; set; }

        [Display(Name = "آیا بیمار قبلاً پذیرش شده")]
        public bool HasExistingReception { get; set; }

        [Display(Name = "تاریخ آخرین پذیرش")]
        public DateTime? LastReceptionDate { get; set; }

        [Display(Name = "آیا پزشک در دسترس است")]
        public bool IsDoctorAvailable { get; set; }

        [Display(Name = "تعداد پذیرش‌های پزشک امروز")]
        public int DoctorTodayReceptionsCount { get; set; }

        [Display(Name = "حداکثر پذیرش‌های پزشک")]
        public int? DoctorMaxDailyReceptions { get; set; }

        public ReceptionValidationViewModel()
        {
            ValidationErrors = new List<string>();
            Warnings = new List<string>();
        }
    }

    /// <summary>
    /// ViewModel برای آمار روزانه پذیرش‌ها
    /// </summary>
    public class ReceptionDailyStatsViewModel
    {
        [Display(Name = "تاریخ")]
        public DateTime Date { get; set; }

        [Display(Name = "تاریخ (شمسی)")]
        public string DateShamsi { get; set; }

        [Display(Name = "تعداد کل پذیرش‌ها")]
        public int TotalReceptions { get; set; }

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
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "میانگین درآمد هر پذیرش")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal AverageRevenuePerReception { get; set; }

        [Display(Name = "درآمد نقدی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal CashPayments { get; set; }

        [Display(Name = "درآمد کارتی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal CardPayments { get; set; }

        [Display(Name = "درآمد آنلاین")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal OnlinePayments { get; set; }

        [Display(Name = "درآمد بیمه")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal InsurancePayments { get; set; }

        [Display(Name = "آمار پزشکان")]
        public List<ReceptionDoctorStatsViewModel> DoctorStats { get; set; }

        public ReceptionDailyStatsViewModel()
        {
            DoctorStats = new List<ReceptionDoctorStatsViewModel>();
        }
    }



    /// <summary>
    /// ViewModel برای لیست‌های مورد نیاز UI
    /// </summary>
    public class ReceptionLookupListsViewModel
    {
        [Display(Name = "لیست بیماران")]
        public List<ReceptionPatientLookupViewModel> Patients { get; set; }
        
        [Display(Name = "لیست پزشکان")]
        public List<ReceptionDoctorLookupViewModel> Doctors { get; set; }

        [Display(Name = "لیست دسته‌بندی خدمات")]
        public List<ReceptionServiceCategoryLookupViewModel> ServiceCategories { get; set; }

        [Display(Name = "لیست خدمات")]
        public List<ReceptionServiceLookupViewModel> Services { get; set; }

        [Display(Name = "لیست تخصص‌ها")]
        public List<SpecializationLookupViewModel> Specializations { get; set; }

        [Display(Name = "لیست روش‌های پرداخت")]
        public List<ClinicApp.ViewModels.Payment.PaymentMethodLookupViewModel> PaymentMethods { get; set; }

        public ReceptionLookupListsViewModel()
        {
            Doctors = new List<ReceptionDoctorLookupViewModel>();
            ServiceCategories = new List<ReceptionServiceCategoryLookupViewModel>();
            Services = new List<ReceptionServiceLookupViewModel>();
            Specializations = new List<SpecializationLookupViewModel>();
            PaymentMethods = new List<PaymentMethodLookupViewModel>();
        }
    }

    /// <summary>
    /// ViewModel برای جستجوی تخصص‌ها
    /// </summary>
   

    /// <summary>
    /// ViewModel برای دپارتمان‌های پزشک در پذیرش
    /// </summary>
    public class ReceptionDoctorDepartmentLookupViewModel
    {
        [Display(Name = "شناسه دپارتمان")]
        public int DepartmentId { get; set; }

        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        [Display(Name = "توضیحات دپارتمان")]
        public string DepartmentDescription { get; set; }

        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "آیا فعال است")]
        public bool IsActive { get; set; }

        [Display(Name = "تاریخ عضویت")]
        public DateTime? JoinDate { get; set; }

        [Display(Name = "تاریخ عضویت (شمسی)")]
        public string JoinDateShamsi { get; set; }

        [Display(Name = "نقش در دپارتمان")]
        public string Role { get; set; }

        [Display(Name = "نام نمایشی")]
        public string DisplayName { get; set; }
    }

}
