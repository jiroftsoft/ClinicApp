using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس پیشنهاد پلن‌های پیش‌فرض بیمه
    /// طبق قرارداد: اگر پلن IsDefault دارد، همان را پیشنهاد می‌دهد؛ در غیر اینصورت اولین پلن فعال
    /// </summary>
    public class InsurancePlanSuggestionService
    {
        private readonly ApplicationDbContext _context;

        public InsurancePlanSuggestionService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// پیشنهاد پلن‌های پیش‌فرض برای بیمه‌های پایه و تکمیلی
        /// </summary>
        /// <param name="baseInsuranceId">شناسه بیمه‌گذار پایه (اختیاری)</param>
        /// <param name="suppInsuranceId">شناسه بیمه‌گذار تکمیلی (اختیاری)</param>
        /// <returns>Tuple شامل (baseDefaultPlanId, suppDefaultPlanId)</returns>
        public async Task<(int? baseDefaultPlanId, int? suppDefaultPlanId)> SuggestDefaultsAsync(int? baseInsuranceId, int? suppInsuranceId)
        {
            int? basePlan = null;
            int? suppPlan = null;

            // پیشنهاد پلن پایه
            if (baseInsuranceId.HasValue)
            {
                var basePlansQuery = _context.InsurancePlans
                    .Where(x => x.InsuranceProviderId == baseInsuranceId.Value && 
                               x.IsActive && 
                               !x.IsDeleted &&
                               x.InsuranceType == InsuranceType.Primary);

                // انتخاب اولین پلن فعال (چون InsurancePlan فیلد IsDefault ندارد)
                basePlan = await basePlansQuery
                    .OrderBy(x => x.InsurancePlanId)
                    .Select(x => (int?)x.InsurancePlanId)
                    .FirstOrDefaultAsync();
            }

            // پیشنهاد پلن تکمیلی
            if (suppInsuranceId.HasValue)
            {
                var suppPlansQuery = _context.InsurancePlans
                    .Where(x => x.InsuranceProviderId == suppInsuranceId.Value && 
                               x.IsActive && 
                               !x.IsDeleted &&
                               x.InsuranceType == InsuranceType.Supplementary);

                // انتخاب اولین پلن فعال (چون InsurancePlan فیلد IsDefault ندارد)
                suppPlan = await suppPlansQuery
                    .OrderBy(x => x.InsurancePlanId)
                    .Select(x => (int?)x.InsurancePlanId)
                    .FirstOrDefaultAsync();
            }

            return (basePlan, suppPlan);
        }
    }
}

