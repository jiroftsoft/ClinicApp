using System;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش سهم بیمه در فرم پذیرش
    /// </summary>
    public class ReceptionInsuranceShareViewModel
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
        /// شناسه بیمه اصلی
        /// </summary>
        public int? PrimaryInsuranceId { get; set; }

        /// <summary>
        /// نام بیمه اصلی
        /// </summary>
        public string PrimaryInsuranceName { get; set; }

        /// <summary>
        /// سهم بیمه اصلی
        /// </summary>
        public decimal PrimaryInsuranceShare { get; set; }

        /// <summary>
        /// درصد سهم بیمه اصلی
        /// </summary>
        public decimal PrimaryInsurancePercent { get; set; }

        /// <summary>
        /// شناسه بیمه تکمیلی
        /// </summary>
        public int? SupplementaryInsuranceId { get; set; }

        /// <summary>
        /// نام بیمه تکمیلی
        /// </summary>
        public string SupplementaryInsuranceName { get; set; }

        /// <summary>
        /// سهم بیمه تکمیلی
        /// </summary>
        public decimal SupplementaryInsuranceShare { get; set; }

        /// <summary>
        /// درصد سهم بیمه تکمیلی
        /// </summary>
        public decimal SupplementaryInsurancePercent { get; set; }

        /// <summary>
        /// سهم کل بیمه
        /// </summary>
        public decimal TotalInsuranceShare { get; set; }

        /// <summary>
        /// سهم بیمار
        /// </summary>
        public decimal PatientShare { get; set; }

        /// <summary>
        /// درصد سهم بیمار
        /// </summary>
        public decimal PatientSharePercent { get; set; }

        /// <summary>
        /// نمایش مبلغ خدمت (فرمات شده)
        /// </summary>
        public string ServiceAmountDisplay => $"{ServiceAmount:N0} ریال";

        /// <summary>
        /// نمایش سهم بیمه اصلی (فرمات شده)
        /// </summary>
        public string PrimaryInsuranceShareDisplay => $"{PrimaryInsuranceShare:N0} ریال ({PrimaryInsurancePercent}%)";

        /// <summary>
        /// نمایش سهم بیمه تکمیلی (فرمات شده)
        /// </summary>
        public string SupplementaryInsuranceShareDisplay => $"{SupplementaryInsuranceShare:N0} ریال ({SupplementaryInsurancePercent}%)";

        /// <summary>
        /// نمایش سهم کل بیمه (فرمات شده)
        /// </summary>
        public string TotalInsuranceShareDisplay => $"{TotalInsuranceShare:N0} ریال";

        /// <summary>
        /// نمایش سهم بیمار (فرمات شده)
        /// </summary>
        public string PatientShareDisplay => $"{PatientShare:N0} ریال ({PatientSharePercent}%)";

        /// <summary>
        /// آیا بیمه اصلی دارد؟
        /// </summary>
        public bool HasPrimaryInsurance => PrimaryInsuranceId.HasValue && PrimaryInsuranceShare > 0;

        /// <summary>
        /// آیا بیمه تکمیلی دارد؟
        /// </summary>
        public bool HasSupplementaryInsurance => SupplementaryInsuranceId.HasValue && SupplementaryInsuranceShare > 0;

        /// <summary>
        /// آیا بیمه کامل است؟
        /// </summary>
        public bool IsFullyInsured => TotalInsuranceShare >= ServiceAmount;

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
                if (IsFullyInsured) return "بیمه کامل";
                if (HasPrimaryInsurance && HasSupplementaryInsurance) return "بیمه ترکیبی";
                if (HasPrimaryInsurance) return "بیمه اصلی";
                if (HasSupplementaryInsurance) return "بیمه تکمیلی";
                return "بدون بیمه";
            }
        }

        /// <summary>
        /// نمایش اطلاعات سهم بیمه (فرمات شده)
        /// </summary>
        public string ShareInfoDisplay
        {
            get
            {
                var parts = new List<string>();
                if (HasPrimaryInsurance) parts.Add($"اصلی: {PrimaryInsuranceShareDisplay}");
                if (HasSupplementaryInsurance) parts.Add($"تکمیلی: {SupplementaryInsuranceShareDisplay}");
                if (HasPatientShare) parts.Add($"بیمار: {PatientShareDisplay}");
                return string.Join(" | ", parts);
            }
        }
    }
}
