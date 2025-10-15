using System.Collections.Generic;
using System.Linq;
using ClinicApp.ViewModels.Insurance.PatientInsurance;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش اطلاعات بیمار و بیمه‌هایش در فرم پذیرش
    /// </summary>
    public class ReceptionFormPatientViewModel
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// اطلاعات کامل بیمار
        /// </summary>
        public ReceptionPatientLookupViewModel PatientInfo { get; set; }

        /// <summary>
        /// لیست بیمه‌های بیمار (اصلی و تکمیلی)
        /// </summary>
        public List<PatientInsuranceLookupViewModel> Insurances { get; set; } = new List<PatientInsuranceLookupViewModel>();

        /// <summary>
        /// آیا بیمار بیمه اصلی دارد؟
        /// </summary>
        public bool HasPrimaryInsurance { get; set; }

        /// <summary>
        /// آیا بیمار بیمه تکمیلی دارد؟
        /// </summary>
        public bool HasSupplementaryInsurance { get; set; }

        /// <summary>
        /// بیمه اصلی فعال
        /// </summary>
        public PatientInsuranceLookupViewModel PrimaryInsurance => 
            Insurances?.FirstOrDefault(i => i.IsPrimary);

        /// <summary>
        /// بیمه تکمیلی فعال
        /// </summary>
        public PatientInsuranceLookupViewModel SupplementaryInsurance => 
            Insurances?.FirstOrDefault(i => !i.IsPrimary);

        /// <summary>
        /// نام کامل بیمار
        /// </summary>
        public string FullName => PatientInfo?.FullName ?? "نامشخص";

        /// <summary>
        /// کد ملی بیمار
        /// </summary>
        public string NationalCode => PatientInfo?.NationalCode ?? "";

        /// <summary>
        /// شماره تلفن بیمار
        /// </summary>
        public string PhoneNumber => PatientInfo?.PhoneNumber ?? "";
    }
}
