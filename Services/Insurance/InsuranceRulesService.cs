using System.Collections.Generic;
using ClinicApp.Models;

namespace ClinicApp.Services.Insurance
{
    public class InsuranceRulesService : IInsuranceRulesService
    {
        private readonly ApplicationDbContext _ctx;
        public InsuranceRulesService(ApplicationDbContext ctx) { _ctx = ctx; }

        public List<InsuranceRuleIssue> ValidateServiceCoverage(int patientId, int serviceId, int qty, int? basePlanId, int? suppPlanId, decimal gross)
        {
            var issues = new List<InsuranceRuleIssue>();

            // TODO: از جدول‌های Limit/Rules بخوان:
            // - محدودیت تعداد/بازه زمانی
            // - پوشش/عدم پوشش خدمت در پلن
            // - سقف مبلغ پوشش

            // نمونه: اگر خدمت در پلن پایه پوشش ندارد
            // bool covered = _ctx.PlanServiceCovers.Any(x => x.PlanId==basePlanId && x.ServiceId==serviceId);
            // if (!covered) issues.Add(new InsuranceRuleIssue{ Field="BasePlanId", Code="BASE_NOT_COVERED", Message="این خدمت در بیمه پایه پوشش ندارد." });

            return issues; // خالی یعنی مجاز
        }
    }
}
