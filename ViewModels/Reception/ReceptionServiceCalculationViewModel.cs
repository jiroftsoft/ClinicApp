namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش محاسبه یک خدمت در فرم پذیرش
    /// </summary>
    public class ReceptionServiceCalculationViewModel
    {
        /// <summary>
        /// شناسه خدمت
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// نام خدمت
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// کد خدمت
        /// </summary>
        public string ServiceCode { get; set; }

        /// <summary>
        /// مبلغ خدمت
        /// </summary>
        public decimal ServiceAmount { get; set; }

        /// <summary>
        /// سهم بیمه
        /// </summary>
        public decimal InsuranceShare { get; set; }

        /// <summary>
        /// سهم بیمار
        /// </summary>
        public decimal PatientShare { get; set; }

        /// <summary>
        /// درصد پوشش بیمه
        /// </summary>
        public decimal CoveragePercent => ServiceAmount > 0 ? (InsuranceShare / ServiceAmount) * 100 : 0;

        /// <summary>
        /// درصد سهم بیمار
        /// </summary>
        public decimal PatientSharePercent => ServiceAmount > 0 ? (PatientShare / ServiceAmount) * 100 : 0;

        /// <summary>
        /// آیا سهم بیمار صفر است؟
        /// </summary>
        public bool IsFullyCovered => PatientShare <= 0;

        /// <summary>
        /// مبلغ خدمت (فرمات شده)
        /// </summary>
        public string FormattedServiceAmount => ServiceAmount.ToString("N0") + " ریال";

        /// <summary>
        /// سهم بیمه (فرمات شده)
        /// </summary>
        public string FormattedInsuranceShare => InsuranceShare.ToString("N0") + " ریال";

        /// <summary>
        /// سهم بیمار (فرمات شده)
        /// </summary>
        public string FormattedPatientShare => PatientShare.ToString("N0") + " ریال";
    }
}
