using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Insurance;
using Serilog;

namespace ClinicApp.Repositories.Insurance
{
    /// <summary>
    /// ریپازیتوری قوانین کسب‌وکار
    /// </summary>
    public class BusinessRuleRepository : IBusinessRuleRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public BusinessRuleRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger.ForContext<BusinessRuleRepository>();
        }

        /// <summary>
        /// دریافت قوانین فعال بر اساس نوع
        /// </summary>
        public async Task<List<BusinessRule>> GetActiveRulesByTypeAsync(
            BusinessRuleType ruleType, 
            int? insurancePlanId = null, 
            int? serviceCategoryId = null)
        {
            try
            {
                _logger.Information("Getting active business rules - RuleType: {RuleType}, InsurancePlanId: {InsurancePlanId}, ServiceCategoryId: {ServiceCategoryId}",
                    ruleType, insurancePlanId, serviceCategoryId);

                var query = _context.BusinessRules
                    .Where(br => br.RuleType == ruleType && 
                                 br.IsActive && 
                                 !br.IsDeleted);

                // فیلتر بر اساس طرح بیمه
                if (insurancePlanId.HasValue)
                {
                    query = query.Where(br => br.InsurancePlanId == null || br.InsurancePlanId == insurancePlanId.Value);
                }

                // فیلتر بر اساس دسته‌بندی خدمت
                if (serviceCategoryId.HasValue)
                {
                    query = query.Where(br => br.ServiceCategoryId == null || br.ServiceCategoryId == serviceCategoryId.Value);
                }

                // فیلتر بر اساس تاریخ اعتبار
                var now = DateTime.Now;
                query = query.Where(br => 
                    (!br.StartDate.HasValue || br.StartDate.Value <= now) &&
                    (!br.EndDate.HasValue || br.EndDate.Value >= now));

                var rules = await query
                    .OrderByDescending(br => br.Priority)
                    .ThenBy(br => br.BusinessRuleId)
                    .ToListAsync();

                _logger.Information("Found {Count} active business rules for RuleType: {RuleType}",
                    rules.Count, ruleType);

                return rules;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting active business rules - RuleType: {RuleType}, InsurancePlanId: {InsurancePlanId}, ServiceCategoryId: {ServiceCategoryId}",
                    ruleType, insurancePlanId, serviceCategoryId);
                throw;
            }
        }

        /// <summary>
        /// دریافت قانون بر اساس شناسه
        /// </summary>
        public async Task<BusinessRule> GetByIdAsync(int businessRuleId)
        {
            try
            {
                _logger.Information("Getting business rule by ID: {BusinessRuleId}", businessRuleId);

                var rule = await _context.BusinessRules
                    .FirstOrDefaultAsync(br => br.BusinessRuleId == businessRuleId && !br.IsDeleted);

                if (rule == null)
                {
                    _logger.Warning("Business rule not found - ID: {BusinessRuleId}", businessRuleId);
                }

                return rule;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting business rule by ID: {BusinessRuleId}", businessRuleId);
                throw;
            }
        }

        /// <summary>
        /// دریافت تمام قوانین فعال
        /// </summary>
        public async Task<List<BusinessRule>> GetAllActiveRulesAsync()
        {
            try
            {
                _logger.Information("Getting all active business rules");

                var rules = await _context.BusinessRules
                    .Where(br => br.IsActive && !br.IsDeleted)
                    .OrderByDescending(br => br.Priority)
                    .ThenBy(br => br.BusinessRuleId)
                    .ToListAsync();

                _logger.Information("Found {Count} active business rules", rules.Count);

                return rules;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting all active business rules");
                throw;
            }
        }

        /// <summary>
        /// دریافت تمام قوانین
        /// </summary>
        public async Task<List<BusinessRule>> GetAllAsync()
        {
            try
            {
                _logger.Information("Getting all business rules");

                var rules = await _context.BusinessRules
                    .Where(br => !br.IsDeleted)
                    .OrderByDescending(br => br.Priority)
                    .ThenBy(br => br.BusinessRuleId)
                    .ToListAsync();

                _logger.Information("Found {Count} business rules", rules.Count);

                return rules;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting all business rules");
                throw;
            }
        }

        /// <summary>
        /// افزودن قانون جدید
        /// </summary>
        public async Task<BusinessRule> AddAsync(BusinessRule businessRule)
        {
            try
            {
                _logger.Information("Adding new business rule - RuleName: {RuleName}, RuleType: {RuleType}",
                    businessRule.RuleName, businessRule.RuleType);

                businessRule.CreatedAt = DateTime.UtcNow;
                businessRule.UpdatedAt = DateTime.UtcNow;

                _context.BusinessRules.Add(businessRule);
                await _context.SaveChangesAsync();

                _logger.Information("Business rule added successfully - ID: {BusinessRuleId}, RuleName: {RuleName}",
                    businessRule.BusinessRuleId, businessRule.RuleName);

                return businessRule;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding business rule - RuleName: {RuleName}, RuleType: {RuleType}",
                    businessRule.RuleName, businessRule.RuleType);
                throw;
            }
        }

        /// <summary>
        /// به‌روزرسانی قانون
        /// </summary>
        public async Task<BusinessRule> UpdateAsync(BusinessRule businessRule)
        {
            try
            {
                _logger.Information("Updating business rule - ID: {BusinessRuleId}, RuleName: {RuleName}",
                    businessRule.BusinessRuleId, businessRule.RuleName);

                businessRule.UpdatedAt = DateTime.UtcNow;

                _context.Entry(businessRule).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.Information("Business rule updated successfully - ID: {BusinessRuleId}, RuleName: {RuleName}",
                    businessRule.BusinessRuleId, businessRule.RuleName);

                return businessRule;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating business rule - ID: {BusinessRuleId}, RuleName: {RuleName}",
                    businessRule.BusinessRuleId, businessRule.RuleName);
                throw;
            }
        }

        /// <summary>
        /// حذف نرم قانون
        /// </summary>
        public async Task<bool> DeleteAsync(int businessRuleId, string deletedByUserId)
        {
            try
            {
                _logger.Information("Soft deleting business rule - ID: {BusinessRuleId}, DeletedBy: {DeletedByUserId}",
                    businessRuleId, deletedByUserId);

                var rule = await _context.BusinessRules
                    .FirstOrDefaultAsync(br => br.BusinessRuleId == businessRuleId && !br.IsDeleted);

                if (rule == null)
                {
                    _logger.Warning("Business rule not found for deletion - ID: {BusinessRuleId}", businessRuleId);
                    return false;
                }

                rule.IsDeleted = true;
                rule.DeletedAt = DateTime.UtcNow;
                rule.DeletedByUserId = deletedByUserId;
                rule.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.Information("Business rule soft deleted successfully - ID: {BusinessRuleId}",
                    businessRuleId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error soft deleting business rule - ID: {BusinessRuleId}, DeletedBy: {DeletedByUserId}",
                    businessRuleId, deletedByUserId);
                throw;
            }
        }

        /// <summary>
        /// ذخیره تغییرات
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                var changes = await _context.SaveChangesAsync();
                _logger.Debug("Saved {Changes} changes to database", changes);
                return changes;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error saving changes to database");
                throw;
            }
        }
    }
}
