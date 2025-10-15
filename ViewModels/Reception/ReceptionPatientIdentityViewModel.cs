using System;
using System.Collections.Generic;
using System.Linq;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش اطلاعات هویتی و بیمه‌ای بیمار در فرم پذیرش
    /// </summary>
    public class ReceptionPatientIdentityViewModel
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// کد ملی بیمار
        /// </summary>
        public string NationalCode { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی بیمار
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// نام کامل بیمار
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// شماره تلفن بیمار
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// تاریخ تولد (میلادی)
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// تاریخ تولد (شمسی)
        /// </summary>
        public string BirthDateShamsi { get; set; }

        /// <summary>
        /// جنسیت بیمار
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// لیست بیمه‌های بیمار
        /// </summary>
        public List<ReceptionInsuranceViewModel> Insurances { get; set; } = new List<ReceptionInsuranceViewModel>();

        /// <summary>
        /// آیا بیمه اصلی دارد؟
        /// </summary>
        public bool HasPrimaryInsurance { get; set; }

        /// <summary>
        /// آیا بیمه تکمیلی دارد؟
        /// </summary>
        public bool HasSupplementaryInsurance { get; set; }

        /// <summary>
        /// تاریخ جستجو
        /// </summary>
        public DateTime SearchDate { get; set; }

        /// <summary>
        /// بیمه اصلی فعال
        /// </summary>
        public ReceptionInsuranceViewModel PrimaryInsurance => 
            Insurances?.FirstOrDefault(i => i.IsPrimary && i.IsActive);

        /// <summary>
        /// بیمه تکمیلی فعال
        /// </summary>
        public ReceptionInsuranceViewModel SupplementaryInsurance => 
            Insurances?.FirstOrDefault(i => !i.IsPrimary && i.IsActive);

        /// <summary>
        /// آیا بیمار یافت شد؟
        /// </summary>
        public bool IsPatientFound => PatientId > 0;

        /// <summary>
        /// آیا بیمه‌ای دارد؟
        /// </summary>
        public bool HasAnyInsurance => Insurances?.Any() == true;

        /// <summary>
        /// تعداد بیمه‌ها
        /// </summary>
        public int InsuranceCount => Insurances?.Count ?? 0;

        /// <summary>
        /// نمایش وضعیت بیمه
        /// </summary>
        public string InsuranceStatus
        {
            get
            {
                if (!HasAnyInsurance) return "بدون بیمه";
                if (HasPrimaryInsurance && HasSupplementaryInsurance) return "بیمه کامل (اصلی + تکمیلی)";
                if (HasPrimaryInsurance) return "بیمه اصلی";
                if (HasSupplementaryInsurance) return "بیمه تکمیلی";
                return "وضعیت نامشخص";
            }
        }

        /// <summary>
        /// نمایش اطلاعات بیمار (فرمات شده)
        /// </summary>
        public string PatientInfoDisplay => $"{FullName} - {NationalCode}";

        /// <summary>
        /// نمایش اطلاعات بیمه (فرمات شده)
        /// </summary>
        public string InsuranceInfoDisplay
        {
            get
            {
                if (!HasAnyInsurance) return "بدون بیمه";
                
                var primary = PrimaryInsurance;
                var supplementary = SupplementaryInsurance;
                
                var result = "";
                if (primary != null) result += $"اصلی: {primary.InsuranceProviderName}";
                if (supplementary != null) result += $" | تکمیلی: {supplementary.InsuranceProviderName}";
                
                return result;
            }
        }
    }
}
