using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش تعرفه خدمت در فرم پذیرش
    /// </summary>
    public class ReceptionServiceTariffViewModel
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
        /// قیمت پایه خدمت
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// قیمت بیمه
        /// </summary>
        public decimal InsurancePrice { get; set; }

        /// <summary>
        /// قیمت بیمار
        /// </summary>
        public decimal PatientPrice { get; set; }

        /// <summary>
        /// قیمت کل
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// آیا بیمه پوشش می‌دهد؟
        /// </summary>
        public bool IsInsuranceCovered { get; set; }

        /// <summary>
        /// درصد پوشش بیمه
        /// </summary>
        public decimal InsuranceCoveragePercent { get; set; }

        /// <summary>
        /// درصد پوشش بیمار
        /// </summary>
        public decimal PatientCoveragePercent { get; set; }

        /// <summary>
        /// نمایش قیمت پایه (فرمات شده)
        /// </summary>
        public string BasePriceDisplay => $"{BasePrice:N0} ریال";

        /// <summary>
        /// نمایش قیمت بیمه (فرمات شده)
        /// </summary>
        public string InsurancePriceDisplay => $"{InsurancePrice:N0} ریال";

        /// <summary>
        /// نمایش قیمت بیمار (فرمات شده)
        /// </summary>
        public string PatientPriceDisplay => $"{PatientPrice:N0} ریال";

        /// <summary>
        /// نمایش قیمت کل (فرمات شده)
        /// </summary>
        public string TotalPriceDisplay => $"{TotalPrice:N0} ریال";

        /// <summary>
        /// نمایش درصد پوشش بیمه
        /// </summary>
        public string InsuranceCoveragePercentDisplay => $"{InsuranceCoveragePercent}%";

        /// <summary>
        /// نمایش درصد پوشش بیمار
        /// </summary>
        public string PatientCoveragePercentDisplay => $"{PatientCoveragePercent}%";

        /// <summary>
        /// نمایش وضعیت پوشش بیمه
        /// </summary>
        public string InsuranceCoverageStatus => IsInsuranceCovered ? "پوشش داده می‌شود" : "پوشش داده نمی‌شود";

        /// <summary>
        /// آیا بیمار سهمی دارد؟
        /// </summary>
        public bool HasPatientShare => PatientPrice > 0;

        /// <summary>
        /// آیا بیمه کامل است؟
        /// </summary>
        public bool IsFullyCovered => InsuranceCoveragePercent >= 100;

        /// <summary>
        /// نمایش اطلاعات تعرفه (فرمات شده)
        /// </summary>
        public string TariffInfoDisplay => $"{ServiceCode} - {BasePrice:N0} ریال - {InsuranceCoveragePercent}% بیمه";
    }
}
