using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// ریپازیتوری قوانین کسب‌وکار
    /// </summary>
    public interface IBusinessRuleRepository
    {
        /// <summary>
        /// دریافت قوانین فعال بر اساس نوع
        /// </summary>
        Task<List<BusinessRule>> GetActiveRulesByTypeAsync(
            BusinessRuleType ruleType, 
            int? insurancePlanId = null, 
            int? serviceCategoryId = null);

        /// <summary>
        /// دریافت قانون بر اساس شناسه
        /// </summary>
        Task<BusinessRule> GetByIdAsync(int businessRuleId);

        /// <summary>
        /// دریافت تمام قوانین فعال
        /// </summary>
        Task<List<BusinessRule>> GetAllActiveRulesAsync();

        /// <summary>
        /// دریافت تمام قوانین
        /// </summary>
        Task<List<BusinessRule>> GetAllAsync();

        /// <summary>
        /// افزودن قانون جدید
        /// </summary>
        Task<BusinessRule> AddAsync(BusinessRule businessRule);

        /// <summary>
        /// به‌روزرسانی قانون
        /// </summary>
        Task<BusinessRule> UpdateAsync(BusinessRule businessRule);

        /// <summary>
        /// حذف نرم قانون
        /// </summary>
        Task<bool> DeleteAsync(int businessRuleId, string deletedByUserId);

        /// <summary>
        /// ذخیره تغییرات
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
