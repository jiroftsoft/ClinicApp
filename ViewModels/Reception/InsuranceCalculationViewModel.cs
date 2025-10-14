namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای محاسبه بیمه
    /// </summary>
    public class InsuranceCalculationViewModel
    {
        /// <summary>
        /// قیمت کل خدمت
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// سهم بیمه پایه
        /// </summary>
        public decimal BaseInsuranceShare { get; set; }

        /// <summary>
        /// سهم بیمه تکمیلی
        /// </summary>
        public decimal SupplementaryInsuranceShare { get; set; }

        /// <summary>
        /// سهم بیمار
        /// </summary>
        public decimal PatientShare { get; set; }

        /// <summary>
        /// درصد پوشش بیمه پایه
        /// </summary>
        public decimal BaseInsuranceCoveragePercent { get; set; }

        /// <summary>
        /// درصد پوشش بیمه تکمیلی
        /// </summary>
        public decimal SupplementaryInsuranceCoveragePercent { get; set; }

        /// <summary>
        /// مجموع درصد پوشش
        /// </summary>
        public decimal TotalCoveragePercent { get; set; }
    }
}
