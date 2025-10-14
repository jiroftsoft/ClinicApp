using System.Collections.Generic;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای بیمه‌های پایه و تکمیلی
    /// </summary>
    public class InsuranceProvidersViewModel
    {
        /// <summary>
        /// لیست بیمه‌های پایه
        /// </summary>
        public List<ReceptionInsuranceLookupViewModel> BaseInsurances { get; set; } = new List<ReceptionInsuranceLookupViewModel>();

        /// <summary>
        /// لیست بیمه‌های تکمیلی
        /// </summary>
        public List<ReceptionInsuranceLookupViewModel> SupplementaryInsurances { get; set; } = new List<ReceptionInsuranceLookupViewModel>();
    }
}
