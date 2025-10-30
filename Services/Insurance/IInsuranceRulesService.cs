using System.Collections.Generic;

namespace ClinicApp.Services.Insurance
{
    public class InsuranceRuleIssue 
    { 
        public string Field { get; set; } 
        public string Message { get; set; } 
        public string Code { get; set; } 
    }
    
    public interface IInsuranceRulesService
    {
        // بررسی سقف/محدودیت خدمت برای بیمار و پلن‌ها
        List<InsuranceRuleIssue> ValidateServiceCoverage(int patientId, int serviceId, int qty, int? basePlanId, int? suppPlanId, decimal gross);
    }
}
