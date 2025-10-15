using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش نتایج محاسبه سهم بیمه در فرم پذیرش
    /// </summary>
    public class ReceptionInsuranceCalculationViewModel
    {
        /// <summary>
        /// شناسه خدمت
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// مبلغ کل خدمت
        /// </summary>
        public decimal ServiceAmount { get; set; }

        /// <summary>
        /// سهم بیمه اصلی
        /// </summary>
        public decimal PrimaryInsuranceShare { get; set; }

        /// <summary>
        /// سهم بیمه تکمیلی
        /// </summary>
        public decimal SupplementaryInsuranceShare { get; set; }

        /// <summary>
        /// سهم کل بیمه (اصلی + تکمیلی)
        /// </summary>
        public decimal TotalInsuranceShare { get; set; }

        /// <summary>
        /// سهم نهایی بیمار
        /// </summary>
        public decimal PatientShare { get; set; }

        /// <summary>
        /// تاریخ محاسبه
        /// </summary>
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// آیا بیمه اصلی دارد؟
        /// </summary>
        public bool HasPrimaryInsurance { get; set; }

        /// <summary>
        /// آیا بیمه تکمیلی دارد؟
        /// </summary>
        public bool HasSupplementaryInsurance { get; set; }

        /// <summary>
        /// درصد پوشش بیمه اصلی
        /// </summary>
        public decimal PrimaryCoveragePercent => ServiceAmount > 0 ? (PrimaryInsuranceShare / ServiceAmount) * 100 : 0;

        /// <summary>
        /// درصد پوشش بیمه تکمیلی
        /// </summary>
        public decimal SupplementaryCoveragePercent => ServiceAmount > 0 ? (SupplementaryInsuranceShare / ServiceAmount) * 100 : 0;

        /// <summary>
        /// درصد کل پوشش بیمه
        /// </summary>
        public decimal TotalCoveragePercent => ServiceAmount > 0 ? (TotalInsuranceShare / ServiceAmount) * 100 : 0;

        /// <summary>
        /// درصد سهم بیمار
        /// </summary>
        public decimal PatientSharePercent => ServiceAmount > 0 ? (PatientShare / ServiceAmount) * 100 : 0;

        /// <summary>
        /// آیا سهم بیمار صفر است؟ (بیمه کامل)
        /// </summary>
        public bool IsFullyCovered => PatientShare <= 0;

        /// <summary>
        /// مبلغ قابل پرداخت بیمار (فرمات شده)
        /// </summary>
        public string FormattedPatientShare => PatientShare.ToString("N0") + " ریال";

        /// <summary>
        /// مبلغ کل خدمت (فرمات شده)
        /// </summary>
        public string FormattedServiceAmount => ServiceAmount.ToString("N0") + " ریال";

        /// <summary>
        /// سهم کل بیمه (فرمات شده)
        /// </summary>
        public string FormattedTotalInsuranceShare => TotalInsuranceShare.ToString("N0") + " ریال";
    }
}
