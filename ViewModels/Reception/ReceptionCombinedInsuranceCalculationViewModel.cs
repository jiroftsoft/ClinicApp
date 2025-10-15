using System;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش محاسبه بیمه ترکیبی در فرم پذیرش
    /// </summary>
    public class ReceptionCombinedInsuranceCalculationViewModel
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// شناسه خدمت
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// مبلغ خدمت
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
        /// سهم کل بیمه
        /// </summary>
        public decimal TotalInsuranceShare { get; set; }

        /// <summary>
        /// سهم بیمار
        /// </summary>
        public decimal PatientShare { get; set; }

        /// <summary>
        /// آیا بیمه کامل است؟
        /// </summary>
        public bool IsFullyCovered { get; set; }

        /// <summary>
        /// تاریخ محاسبه
        /// </summary>
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// نمایش مبلغ خدمت (فرمات شده)
        /// </summary>
        public string ServiceAmountDisplay => $"{ServiceAmount:N0} ریال";

        /// <summary>
        /// نمایش سهم بیمه اصلی (فرمات شده)
        /// </summary>
        public string PrimaryInsuranceShareDisplay => $"{PrimaryInsuranceShare:N0} ریال";

        /// <summary>
        /// نمایش سهم بیمه تکمیلی (فرمات شده)
        /// </summary>
        public string SupplementaryInsuranceShareDisplay => $"{SupplementaryInsuranceShare:N0} ریال";

        /// <summary>
        /// نمایش سهم کل بیمه (فرمات شده)
        /// </summary>
        public string TotalInsuranceShareDisplay => $"{TotalInsuranceShare:N0} ریال";

        /// <summary>
        /// نمایش سهم بیمار (فرمات شده)
        /// </summary>
        public string PatientShareDisplay => $"{PatientShare:N0} ریال";

        /// <summary>
        /// درصد سهم بیمه اصلی
        /// </summary>
        public decimal PrimaryInsurancePercent => ServiceAmount > 0 ? (PrimaryInsuranceShare / ServiceAmount) * 100 : 0;

        /// <summary>
        /// درصد سهم بیمه تکمیلی
        /// </summary>
        public decimal SupplementaryInsurancePercent => ServiceAmount > 0 ? (SupplementaryInsuranceShare / ServiceAmount) * 100 : 0;

        /// <summary>
        /// درصد سهم کل بیمه
        /// </summary>
        public decimal TotalInsurancePercent => ServiceAmount > 0 ? (TotalInsuranceShare / ServiceAmount) * 100 : 0;

        /// <summary>
        /// درصد سهم بیمار
        /// </summary>
        public decimal PatientSharePercent => ServiceAmount > 0 ? (PatientShare / ServiceAmount) * 100 : 0;

        /// <summary>
        /// نمایش درصد سهم بیمه اصلی
        /// </summary>
        public string PrimaryInsurancePercentDisplay => $"{PrimaryInsurancePercent:F1}%";

        /// <summary>
        /// نمایش درصد سهم بیمه تکمیلی
        /// </summary>
        public string SupplementaryInsurancePercentDisplay => $"{SupplementaryInsurancePercent:F1}%";

        /// <summary>
        /// نمایش درصد سهم کل بیمه
        /// </summary>
        public string TotalInsurancePercentDisplay => $"{TotalInsurancePercent:F1}%";

        /// <summary>
        /// نمایش درصد سهم بیمار
        /// </summary>
        public string PatientSharePercentDisplay => $"{PatientSharePercent:F1}%";

        /// <summary>
        /// آیا بیمه اصلی دارد؟
        /// </summary>
        public bool HasPrimaryInsurance => PrimaryInsuranceShare > 0;

        /// <summary>
        /// آیا بیمه تکمیلی دارد؟
        /// </summary>
        public bool HasSupplementaryInsurance => SupplementaryInsuranceShare > 0;

        /// <summary>
        /// آیا بیمار سهمی دارد؟
        /// </summary>
        public bool HasPatientShare => PatientShare > 0;

        /// <summary>
        /// نمایش وضعیت بیمه
        /// </summary>
        public string InsuranceStatus
        {
            get
            {
                if (IsFullyCovered) return "بیمه کامل";
                if (HasPrimaryInsurance && HasSupplementaryInsurance) return "بیمه ترکیبی";
                if (HasPrimaryInsurance) return "بیمه اصلی";
                if (HasSupplementaryInsurance) return "بیمه تکمیلی";
                return "بدون بیمه";
            }
        }

        /// <summary>
        /// نمایش اطلاعات محاسبه (فرمات شده)
        /// </summary>
        public string CalculationInfoDisplay
        {
            get
            {
                var parts = new List<string>();
                parts.Add($"خدمت: {ServiceAmountDisplay}");
                if (HasPrimaryInsurance) parts.Add($"اصلی: {PrimaryInsuranceShareDisplay} ({PrimaryInsurancePercentDisplay})");
                if (HasSupplementaryInsurance) parts.Add($"تکمیلی: {SupplementaryInsuranceShareDisplay} ({SupplementaryInsurancePercentDisplay})");
                if (HasPatientShare) parts.Add($"بیمار: {PatientShareDisplay} ({PatientSharePercentDisplay})");
                return string.Join(" | ", parts);
            }
        }
    }
}
