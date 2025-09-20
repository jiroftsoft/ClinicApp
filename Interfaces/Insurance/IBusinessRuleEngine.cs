using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// موتور قوانین کسب‌وکار برای سیستم بیمه
    /// طراحی شده برای قوانین پیچیده و انعطاف‌پذیر
    /// </summary>
    public interface IBusinessRuleEngine
    {
        /// <summary>
        /// محاسبه درصد پوشش بر اساس قوانین پیچیده
        /// </summary>
        Task<ServiceResult<decimal>> CalculateCoveragePercentAsync(
            InsuranceCalculationContext context);

        /// <summary>
        /// محاسبه فرانشیز بر اساس قوانین
        /// </summary>
        Task<ServiceResult<decimal>> CalculateDeductibleAsync(
            InsuranceCalculationContext context);

        /// <summary>
        /// بررسی سقف‌های پرداخت
        /// </summary>
        Task<ServiceResult<bool>> ValidatePaymentLimitsAsync(
            InsuranceCalculationContext context);

        /// <summary>
        /// اعمال قوانین خاص بیمه تکمیلی
        /// </summary>
        Task<ServiceResult<SupplementaryInsuranceRule>> ApplySupplementaryRulesAsync(
            InsuranceCalculationContext context);

        /// <summary>
        /// اعتبارسنجی قوانین کسب‌وکار
        /// </summary>
        Task<ServiceResult<BusinessRuleValidationResult>> ValidateBusinessRulesAsync(
            InsuranceCalculationContext context);

        /// <summary>
        /// ارزیابی قانون خاص
        /// </summary>
        Task<bool> EvaluateRuleAsync(BusinessRule rule, InsuranceCalculationContext context);
    }

    /// <summary>
    /// زمینه محاسبه بیمه برای Rule Engine
    /// </summary>
    public class InsuranceCalculationContext
    {
        public int PatientId { get; set; }
        public int ServiceId { get; set; }
        public int InsurancePlanId { get; set; }
        public int? ServiceCategoryId { get; set; }
        public decimal ServiceAmount { get; set; }
        public DateTime CalculationDate { get; set; }
        public PatientInsurance PatientInsurance { get; set; }
        public InsurancePlan InsurancePlan { get; set; }
        public PlanService PlanService { get; set; }
        public Service Service { get; set; }
        public Patient Patient { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// نتیجه اعتبارسنجی قوانین کسب‌وکار
    /// </summary>
    public class BusinessRuleValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public Dictionary<string, object> AppliedRules { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// قوانین بیمه تکمیلی
    /// </summary>
    public class SupplementaryInsuranceRule
    {
        public decimal CoveragePercent { get; set; }
        public decimal MaxPayment { get; set; }
        public bool IsApplicable { get; set; }
        public string RuleName { get; set; }
        public string Description { get; set; }
    }
}
