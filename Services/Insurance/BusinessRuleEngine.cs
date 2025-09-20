using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Clinic;
using Serilog;
using System.Text.Json;
using ClinicApp.Helpers;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// موتور قوانین کسب‌وکار برای سیستم بیمه
    /// پیاده‌سازی حرفه‌ای و انعطاف‌پذیر
    /// </summary>
    public class BusinessRuleEngine : IBusinessRuleEngine
    {
        private readonly IBusinessRuleRepository _businessRuleRepository;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public BusinessRuleEngine(
            IBusinessRuleRepository businessRuleRepository,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _businessRuleRepository = businessRuleRepository ?? throw new ArgumentNullException(nameof(businessRuleRepository));
            _log = logger.ForContext<BusinessRuleEngine>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// محاسبه درصد پوشش بر اساس قوانین پیچیده
        /// </summary>
        public async Task<ServiceResult<decimal>> CalculateCoveragePercentAsync(InsuranceCalculationContext context)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه درصد پوشش با Rule Engine - PatientId: {PatientId}, ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    context.PatientId, context.ServiceId, context.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت قوانین مربوط به درصد پوشش
                var coverageRules = await _businessRuleRepository.GetActiveRulesByTypeAsync(
                    BusinessRuleType.CoveragePercent, context.InsurancePlanId, context.ServiceCategoryId);

                decimal finalCoveragePercent = 0;

                // اعمال قوانین به ترتیب اولویت
                foreach (var rule in coverageRules.OrderByDescending(r => r.Priority))
                {
                    if (await EvaluateRuleConditionsAsync(rule, context))
                    {
                        var ruleResult = await ApplyRuleActionsAsync(rule, context);
                        if (ruleResult.Success && ruleResult.Data.ContainsKey("CoveragePercent"))
                        {
                            finalCoveragePercent = Convert.ToDecimal(ruleResult.Data["CoveragePercent"]);
                            _log.Information("🏥 MEDICAL: قانون اعمال شد - RuleName: {RuleName}, CoveragePercent: {CoveragePercent}. User: {UserName} (Id: {UserId})",
                                rule.RuleName, finalCoveragePercent, _currentUserService.UserName, _currentUserService.UserId);
                            break; // اولین قانون معتبر اعمال می‌شود
                        }
                    }
                }

                // اگر هیچ قانونی اعمال نشد، از مقدار پیش‌فرض استفاده کن
                if (finalCoveragePercent == 0)
                {
                    finalCoveragePercent = context.InsurancePlan?.CoveragePercent ?? 
                                         context.PlanService?.CoverageOverride ?? 0;
                }

                // محدود کردن به محدوده مجاز
                finalCoveragePercent = Math.Max(0, Math.Min(100, finalCoveragePercent));

                _log.Information("🏥 MEDICAL: محاسبه درصد پوشش تکمیل شد - FinalCoveragePercent: {CoveragePercent}%. User: {UserName} (Id: {UserId})",
                    finalCoveragePercent, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<decimal>.Successful(finalCoveragePercent);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه درصد پوشش با Rule Engine - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    context.PatientId, context.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<decimal>.Failed("خطا در محاسبه درصد پوشش");
            }
        }

        /// <summary>
        /// محاسبه فرانشیز بر اساس قوانین
        /// </summary>
        public async Task<ServiceResult<decimal>> CalculateDeductibleAsync(InsuranceCalculationContext context)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع محاسبه فرانشیز با Rule Engine - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    context.PatientId, context.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                var deductibleRules = await _businessRuleRepository.GetActiveRulesByTypeAsync(
                    BusinessRuleType.Deductible, context.InsurancePlanId, context.ServiceCategoryId);

                decimal finalDeductible = context.InsurancePlan?.Deductible ?? 0;

                foreach (var rule in deductibleRules.OrderByDescending(r => r.Priority))
                {
                    if (await EvaluateRuleConditionsAsync(rule, context))
                    {
                        var ruleResult = await ApplyRuleActionsAsync(rule, context);
                        if (ruleResult.Success && ruleResult.Data.ContainsKey("Deductible"))
                        {
                            finalDeductible = Convert.ToDecimal(ruleResult.Data["Deductible"]);
                            break;
                        }
                    }
                }

                _log.Information("🏥 MEDICAL: محاسبه فرانشیز تکمیل شد - FinalDeductible: {Deductible}. User: {UserName} (Id: {UserId})",
                    finalDeductible, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<decimal>.Successful(finalDeductible);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه فرانشیز با Rule Engine - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    context.PatientId, context.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<decimal>.Failed("خطا در محاسبه فرانشیز");
            }
        }

        /// <summary>
        /// بررسی سقف‌های پرداخت
        /// </summary>
        public async Task<ServiceResult<bool>> ValidatePaymentLimitsAsync(InsuranceCalculationContext context)
        {
            try
            {
                var limitRules = await _businessRuleRepository.GetActiveRulesByTypeAsync(
                    BusinessRuleType.PaymentLimit, context.InsurancePlanId, context.ServiceCategoryId);

                foreach (var rule in limitRules.OrderByDescending(r => r.Priority))
                {
                    if (await EvaluateRuleConditionsAsync(rule, context))
                    {
                        var ruleResult = await ApplyRuleActionsAsync(rule, context);
                        if (ruleResult.Success && ruleResult.Data.ContainsKey("IsValid"))
                        {
                            bool isValid = Convert.ToBoolean(ruleResult.Data["IsValid"]);
                            if (!isValid)
                            {
                                return ServiceResult<bool>.Failed("سقف پرداخت تجاوز شده است");
                            }
                        }
                    }
                }

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در بررسی سقف‌های پرداخت - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    context.PatientId, context.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<bool>.Failed("خطا در بررسی سقف‌های پرداخت");
            }
        }

        /// <summary>
        /// اعمال قوانین خاص بیمه تکمیلی
        /// </summary>
        public async Task<ServiceResult<SupplementaryInsuranceRule>> ApplySupplementaryRulesAsync(InsuranceCalculationContext context)
        {
            try
            {
                var supplementaryRules = await _businessRuleRepository.GetActiveRulesByTypeAsync(
                    BusinessRuleType.SupplementaryInsurance, context.InsurancePlanId, context.ServiceCategoryId);

                var result = new SupplementaryInsuranceRule
                {
                    CoveragePercent = 0,
                    MaxPayment = 0,
                    IsApplicable = false
                };

                foreach (var rule in supplementaryRules.OrderByDescending(r => r.Priority))
                {
                    if (await EvaluateRuleConditionsAsync(rule, context))
                    {
                        var ruleResult = await ApplyRuleActionsAsync(rule, context);
                        if (ruleResult.Success)
                        {
                            result.CoveragePercent = ruleResult.Data.ContainsKey("CoveragePercent") ? 
                                Convert.ToDecimal(ruleResult.Data["CoveragePercent"]) : 0;
                            result.MaxPayment = ruleResult.Data.ContainsKey("MaxPayment") ? 
                                Convert.ToDecimal(ruleResult.Data["MaxPayment"]) : 0;
                            result.IsApplicable = ruleResult.Data.ContainsKey("IsApplicable") ? 
                                Convert.ToBoolean(ruleResult.Data["IsApplicable"]) : false;
                            result.RuleName = rule.RuleName;
                            result.Description = rule.Description;
                            break;
                        }
                    }
                }

                return ServiceResult<SupplementaryInsuranceRule>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اعمال قوانین بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    context.PatientId, context.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<SupplementaryInsuranceRule>.Failed("خطا در اعمال قوانین بیمه تکمیلی");
            }
        }

        /// <summary>
        /// اعتبارسنجی قوانین کسب‌وکار
        /// </summary>
        public async Task<ServiceResult<BusinessRuleValidationResult>> ValidateBusinessRulesAsync(InsuranceCalculationContext context)
        {
            try
            {
                var validationRules = await _businessRuleRepository.GetActiveRulesByTypeAsync(
                    BusinessRuleType.Validation, context.InsurancePlanId, context.ServiceCategoryId);

                var result = new BusinessRuleValidationResult
                {
                    IsValid = true,
                    Errors = new List<string>(),
                    Warnings = new List<string>(),
                    AppliedRules = new Dictionary<string, object>()
                };

                foreach (var rule in validationRules.OrderByDescending(r => r.Priority))
                {
                    if (await EvaluateRuleConditionsAsync(rule, context))
                    {
                        var ruleResult = await ApplyRuleActionsAsync(rule, context);
                        if (ruleResult.Success)
                        {
                            if (ruleResult.Data.ContainsKey("IsValid"))
                            {
                                bool isValid = Convert.ToBoolean(ruleResult.Data["IsValid"]);
                                if (!isValid)
                                {
                                    result.IsValid = false;
                                    if (ruleResult.Data.ContainsKey("ErrorMessage"))
                                    {
                                        result.Errors.Add(ruleResult.Data["ErrorMessage"].ToString());
                                    }
                                }
                            }

                            if (ruleResult.Data.ContainsKey("WarningMessage"))
                            {
                                result.Warnings.Add(ruleResult.Data["WarningMessage"].ToString());
                            }

                            result.AppliedRules[rule.RuleName] = ruleResult.Data;
                        }
                    }
                }

                return ServiceResult<BusinessRuleValidationResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی قوانین کسب‌وکار - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    context.PatientId, context.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<BusinessRuleValidationResult>.Failed("خطا در اعتبارسنجی قوانین کسب‌وکار");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// ارزیابی شرایط قانون
        /// </summary>
        private async Task<bool> EvaluateRuleConditionsAsync(BusinessRule rule, InsuranceCalculationContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(rule.Conditions))
                    return true;

                var conditions = JsonSerializer.Deserialize<Dictionary<string, object>>(rule.Conditions);
                
                // بررسی تاریخ اعتبار
                if (rule.StartDate.HasValue && context.CalculationDate < rule.StartDate.Value)
                    return false;
                
                if (rule.EndDate.HasValue && context.CalculationDate > rule.EndDate.Value)
                    return false;

                // ارزیابی شرایط پیچیده
                foreach (var condition in conditions)
                {
                    if (!await EvaluateConditionAsync(condition.Key, condition.Value, context))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ارزیابی شرایط قانون - RuleName: {RuleName}", rule.RuleName);
                return false;
            }
        }

        /// <summary>
        /// ارزیابی یک شرط خاص
        /// </summary>
        private async Task<bool> EvaluateConditionAsync(string conditionKey, object conditionValue, InsuranceCalculationContext context)
        {
            try
            {
                switch (conditionKey.ToLower())
                {
                    case "patient_age":
                        if (context.Patient?.BirthDate.HasValue == true)
                        {
                            var age = DateTime.Now.Year - context.Patient.BirthDate.Value.Year;
                            return EvaluateNumericCondition(age, conditionValue);
                        }
                        break;

                    case "service_amount":
                        return EvaluateNumericCondition(context.ServiceAmount, conditionValue);

                    case "patient_gender":
                        return context.Patient?.Gender.ToString() == conditionValue.ToString();

                    case "service_category":
                        return context.Service?.ServiceCategoryId.ToString() == conditionValue.ToString();

                    case "insurance_plan":
                        return context.InsurancePlanId.ToString() == conditionValue.ToString();

                    default:
                        _log.Warning("شرط ناشناخته: {ConditionKey}", conditionKey);
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ارزیابی شرط - ConditionKey: {ConditionKey}", conditionKey);
                return false;
            }
        }

        /// <summary>
        /// ارزیابی شرایط عددی
        /// </summary>
        private bool EvaluateNumericCondition(decimal value, object condition)
        {
            try
            {
                if (condition is JsonElement element)
                {
                    if (element.TryGetProperty("min", out var minElement))
                    {
                        if (value < minElement.GetDecimal())
                            return false;
                    }
                    
                    if (element.TryGetProperty("max", out var maxElement))
                    {
                        if (value > maxElement.GetDecimal())
                            return false;
                    }
                    
                    if (element.TryGetProperty("equals", out var equalsElement))
                    {
                        if (value != equalsElement.GetDecimal())
                            return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ارزیابی شرط عددی - Value: {Value}, Condition: {Condition}", value, condition);
                return false;
            }
        }

        /// <summary>
        /// اعمال عملیات قانون
        /// </summary>
        private async Task<ServiceResult<Dictionary<string, object>>> ApplyRuleActionsAsync(BusinessRule rule, InsuranceCalculationContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(rule.Actions))
                    return ServiceResult<Dictionary<string, object>>.Successful(new Dictionary<string, object>());

                var actions = JsonSerializer.Deserialize<Dictionary<string, object>>(rule.Actions);
                var result = new Dictionary<string, object>();

                foreach (var action in actions)
                {
                    var actionResult = await ApplyActionAsync(action.Key, action.Value, context);
                    if (actionResult.Success)
                    {
                        foreach (var item in actionResult.Data)
                        {
                            result[item.Key] = item.Value;
                        }
                    }
                }

                return ServiceResult<Dictionary<string, object>>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در اعمال عملیات قانون - RuleName: {RuleName}", rule.RuleName);
                return ServiceResult<Dictionary<string, object>>.Failed("خطا در اعمال عملیات قانون");
            }
        }

        /// <summary>
        /// اعمال یک عمل خاص
        /// </summary>
        private async Task<ServiceResult<Dictionary<string, object>>> ApplyActionAsync(string actionKey, object actionValue, InsuranceCalculationContext context)
        {
            try
            {
                var result = new Dictionary<string, object>();

                switch (actionKey.ToLower())
                {
                    case "set_coverage_percent":
                        result["CoveragePercent"] = Convert.ToDecimal(actionValue);
                        break;

                    case "set_deductible":
                        result["Deductible"] = Convert.ToDecimal(actionValue);
                        break;

                    case "set_max_payment":
                        result["MaxPayment"] = Convert.ToDecimal(actionValue);
                        break;

                    case "validate_payment_limit":
                        var limit = Convert.ToDecimal(actionValue);
                        result["IsValid"] = context.ServiceAmount <= limit;
                        if (!result["IsValid"].Equals(true))
                        {
                            result["ErrorMessage"] = $"مبلغ خدمت ({context.ServiceAmount:N0}) از سقف مجاز ({limit:N0}) بیشتر است";
                        }
                        break;

                    case "set_supplementary_applicable":
                        result["IsApplicable"] = Convert.ToBoolean(actionValue);
                        break;

                    default:
                        _log.Warning("عمل ناشناخته: {ActionKey}", actionKey);
                        break;
                }

                return ServiceResult<Dictionary<string, object>>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در اعمال عمل - ActionKey: {ActionKey}", actionKey);
                return ServiceResult<Dictionary<string, object>>.Failed("خطا در اعمال عمل");
            }
        }

        #endregion

        /// <summary>
        /// ارزیابی قانون خاص
        /// </summary>
        public async Task<bool> EvaluateRuleAsync(BusinessRule rule, InsuranceCalculationContext context)
        {
            try
            {
                _log.Information("🏥 MEDICAL: ارزیابی قانون. RuleId: {RuleId}, RuleName: {RuleName}, RuleType: {RuleType}. User: {UserName} (Id: {UserId})",
                    rule.BusinessRuleId, rule.RuleName, rule.RuleType, _currentUserService.UserName, _currentUserService.UserId);

                // بررسی فعال بودن قانون
                if (!rule.IsActive)
                {
                    _log.Debug("قانون غیرفعال است. RuleId: {RuleId}", rule.BusinessRuleId);
                    return false;
                }

                // بررسی تاریخ اعتبار
                if (rule.StartDate.HasValue && rule.StartDate.Value > context.CalculationDate)
                {
                    _log.Debug("قانون هنوز فعال نشده. RuleId: {RuleId}, StartDate: {StartDate}", rule.BusinessRuleId, rule.StartDate);
                    return false;
                }

                if (rule.EndDate.HasValue && rule.EndDate.Value < context.CalculationDate)
                {
                    _log.Debug("قانون منقضی شده. RuleId: {RuleId}, EndDate: {EndDate}", rule.BusinessRuleId, rule.EndDate);
                    return false;
                }

                // ارزیابی شرط
                var conditionResult = await EvaluateRuleConditionsAsync(rule, context);
                _log.Information("🏥 MEDICAL: قانون ارزیابی شد. RuleId: {RuleId}, Result: {Result}. User: {UserName} (Id: {UserId})",
                    rule.BusinessRuleId, conditionResult, _currentUserService.UserName, _currentUserService.UserId);

                return conditionResult;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ارزیابی قانون. RuleId: {RuleId}. User: {UserName} (Id: {UserId})",
                    rule.BusinessRuleId, _currentUserService.UserName, _currentUserService.UserId);
                return false;
            }
        }
    }
}
