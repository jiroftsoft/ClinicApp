using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.ViewModels.Insurance.BusinessRule
{
    /// <summary>
    /// ViewModel برای ایجاد و ویرایش قانون کسب‌وکار
    /// </summary>
    public class BusinessRuleCreateEditViewModel
    {
        /// <summary>
        /// شناسه قانون (برای ویرایش)
        /// </summary>
        public int? BusinessRuleId { get; set; }

        /// <summary>
        /// نام قانون
        /// </summary>
        [Required(ErrorMessage = "نام قانون الزامی است.")]
        [StringLength(200, ErrorMessage = "نام قانون نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام قانون")]
        public string RuleName { get; set; }

        /// <summary>
        /// توضیحات قانون
        /// </summary>
        [StringLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        /// <summary>
        /// نوع قانون
        /// </summary>
        [Required(ErrorMessage = "نوع قانون الزامی است.")]
        [Display(Name = "نوع قانون")]
        public BusinessRuleType RuleType { get; set; }

        /// <summary>
        /// شرط قانون (JSON)
        /// </summary>
        [Required(ErrorMessage = "شرط قانون الزامی است.")]
        [Display(Name = "شرط قانون")]
        public string Conditions { get; set; }

        /// <summary>
        /// عمل قانون (JSON)
        /// </summary>
        [Required(ErrorMessage = "عمل قانون الزامی است.")]
        [Display(Name = "عمل قانون")]
        public string Actions { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// اولویت قانون
        /// </summary>
        [Required(ErrorMessage = "اولویت الزامی است.")]
        [Range(1, 100, ErrorMessage = "اولویت باید بین 1 تا 100 باشد.")]
        [Display(Name = "اولویت")]
        public int Priority { get; set; } = 1;

        /// <summary>
        /// نمونه شرط برای راهنمایی
        /// </summary>
        public string ConditionExample => GetConditionExample(RuleType);

        /// <summary>
        /// نمونه عمل برای راهنمایی
        /// </summary>
        public string ActionExample => GetActionExample(RuleType);

        /// <summary>
        /// دریافت نمونه شرط بر اساس نوع قانون
        /// </summary>
        private string GetConditionExample(BusinessRuleType ruleType)
        {
            return ruleType switch
            {
                BusinessRuleType.CoveragePercent => @"{
  ""field"": ""service_amount"",
  ""operator"": ""greater_than"",
  ""value"": ""1000000""
}",
                BusinessRuleType.Deductible => @"{
  ""field"": ""patient_age"",
  ""operator"": ""less_than"",
  ""value"": ""65""
}",
                BusinessRuleType.PaymentLimit => @"{
  ""field"": ""service_category"",
  ""operator"": ""equals"",
  ""value"": ""1""
}",
                BusinessRuleType.AgeBasedDiscount => @"{
  ""field"": ""patient_age"",
  ""operator"": ""between"",
  ""value"": ""60,80""
}",
                BusinessRuleType.GenderBasedDiscount => @"{
  ""field"": ""patient_gender"",
  ""operator"": ""equals"",
  ""value"": ""Female""
}",
                BusinessRuleType.ServiceBasedDiscount => @"{
  ""field"": ""service_amount"",
  ""operator"": ""greater_than"",
  ""value"": ""500000""
}",
                BusinessRuleType.InsuranceBasedDiscount => @"{
  ""field"": ""insurance_plan"",
  ""operator"": ""equals"",
  ""value"": ""1""
}",
                BusinessRuleType.CustomRule => @"{
  ""field"": ""custom_field"",
  ""operator"": ""custom_operator"",
  ""value"": ""custom_value""
}",
                _ => "{}"
            };
        }

        /// <summary>
        /// دریافت نمونه عمل بر اساس نوع قانون
        /// </summary>
        private string GetActionExample(BusinessRuleType ruleType)
        {
            return ruleType switch
            {
                BusinessRuleType.CoveragePercent => @"{
  ""type"": ""set_coverage_percent"",
  ""value"": ""90""
}",
                BusinessRuleType.Deductible => @"{
  ""type"": ""set_deductible"",
  ""value"": ""50000""
}",
                BusinessRuleType.PaymentLimit => @"{
  ""type"": ""set_payment_limit"",
  ""value"": ""2000000""
}",
                BusinessRuleType.AgeBasedDiscount => @"{
  ""type"": ""apply_discount"",
  ""value"": ""10""
}",
                BusinessRuleType.GenderBasedDiscount => @"{
  ""type"": ""apply_discount"",
  ""value"": ""5""
}",
                BusinessRuleType.ServiceBasedDiscount => @"{
  ""type"": ""apply_discount"",
  ""value"": ""15""
}",
                BusinessRuleType.InsuranceBasedDiscount => @"{
  ""type"": ""apply_discount"",
  ""value"": ""20""
}",
                BusinessRuleType.CustomRule => @"{
  ""type"": ""custom_action"",
  ""value"": ""custom_value""
}",
                _ => "{}"
            };
        }
    }
}
