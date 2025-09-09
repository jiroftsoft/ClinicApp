using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using Serilog;

namespace ClinicApp.Repositories.Insurance
{
    /// <summary>
    /// پیاده‌سازی Repository برای مدیریت طرح‌های بیمه در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت طرح‌های بیمه (Basic, Standard, Premium, Supplementary)
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت بیمه‌ها
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات بیمه‌ای
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. بهینه‌سازی عملکرد با AsNoTracking و Include
    /// 6. مدیریت روابط با InsuranceProvider و PlanService
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class InsurancePlanRepository : IInsurancePlanRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public InsurancePlanRepository(ApplicationDbContext context, ILogger logger, ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Core CRUD Operations

        /// <summary>
        /// دریافت طرح بیمه بر اساس شناسه
        /// </summary>
        public async Task<InsurancePlan> GetByIdAsync(int id)
        {
            try
            {
                return await _context.InsurancePlans
                    .Where(ip => ip.InsurancePlanId == id)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت طرح بیمه. InsurancePlanId: {InsurancePlanId}", id);
                throw new InvalidOperationException($"خطا در دریافت طرح بیمه {id}", ex);
            }
        }

        /// <summary>
        /// دریافت طرح بیمه بر اساس شناسه همراه با جزئیات کامل
        /// </summary>
        public async Task<InsurancePlan> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _context.InsurancePlans
                    .Where(ip => ip.InsurancePlanId == id)
                    .Include(ip => ip.InsuranceProvider)
                    .Include(ip => ip.PlanServices)
                    .Include(ip => ip.CreatedByUser)
                    .Include(ip => ip.UpdatedByUser)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت طرح بیمه با جزئیات. InsurancePlanId: {InsurancePlanId}", id);
                throw new InvalidOperationException($"خطا در دریافت طرح بیمه {id} با جزئیات", ex);
            }
        }

        /// <summary>
        /// دریافت تمام طرح‌های بیمه
        /// </summary>
        public async Task<List<InsurancePlan>> GetAllAsync()
        {
            try
            {
                return await _context.InsurancePlans
                    .Include(ip => ip.InsuranceProvider)
                    .OrderBy(ip => ip.InsuranceProvider.Name)
                    .ThenBy(ip => ip.Name)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تمام طرح‌های بیمه");
                throw new InvalidOperationException("خطا در دریافت تمام طرح‌های بیمه", ex);
            }
        }

        /// <summary>
        /// دریافت طرح‌های بیمه فعال
        /// </summary>
        public async Task<List<InsurancePlan>> GetActiveAsync()
        {
            try
            {
                return await _context.InsurancePlans
                    .Where(ip => ip.IsActive)
                    .Include(ip => ip.InsuranceProvider)
                    .OrderBy(ip => ip.InsuranceProvider.Name)
                    .ThenBy(ip => ip.Name)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت طرح‌های بیمه فعال");
                throw new InvalidOperationException("خطا در دریافت طرح‌های بیمه فعال", ex);
            }
        }

        /// <summary>
        /// دریافت طرح‌های بیمه بر اساس ارائه‌دهنده
        /// </summary>
        public async Task<List<InsurancePlan>> GetByProviderIdAsync(int providerId)
        {
            try
            {
                return await _context.InsurancePlans
                    .Where(ip => ip.InsuranceProviderId == providerId)
                    .Include(ip => ip.InsuranceProvider)
                    .OrderBy(ip => ip.Name)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت طرح‌های بیمه بر اساس ارائه‌دهنده. ProviderId: {ProviderId}", providerId);
                throw new InvalidOperationException($"خطا در دریافت طرح‌های بیمه ارائه‌دهنده {providerId}", ex);
            }
        }

        /// <summary>
        /// دریافت طرح‌های بیمه فعال بر اساس ارائه‌دهنده
        /// </summary>
        public async Task<List<InsurancePlan>> GetActiveByProviderIdAsync(int providerId)
        {
            try
            {
                return await _context.InsurancePlans
                    .Where(ip => ip.InsuranceProviderId == providerId && ip.IsActive)
                    .Include(ip => ip.InsuranceProvider)
                    .OrderBy(ip => ip.Name)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت طرح‌های بیمه فعال بر اساس ارائه‌دهنده. ProviderId: {ProviderId}", providerId);
                throw new InvalidOperationException($"خطا در دریافت طرح‌های بیمه فعال ارائه‌دهنده {providerId}", ex);
            }
        }

        /// <summary>
        /// دریافت طرح بیمه بر اساس کد طرح
        /// </summary>
        public async Task<InsurancePlan> GetByPlanCodeAsync(string planCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(planCode))
                    return null;

                return await _context.InsurancePlans
                    .Where(ip => ip.PlanCode == planCode)
                    .Include(ip => ip.InsuranceProvider)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت طرح بیمه بر اساس کد طرح. PlanCode: {PlanCode}", planCode);
                throw new InvalidOperationException($"خطا در دریافت طرح بیمه با کد {planCode}", ex);
            }
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود کد طرح بیمه
        /// </summary>
        public async Task<bool> DoesPlanCodeExistAsync(string planCode, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(planCode))
                    return false;

                var query = _context.InsurancePlans
                    .Where(ip => ip.PlanCode == planCode);

                if (excludeId.HasValue)
                    query = query.Where(ip => ip.InsurancePlanId != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود کد طرح بیمه. PlanCode: {PlanCode}, ExcludeId: {ExcludeId}", planCode, excludeId);
                throw new InvalidOperationException($"خطا در بررسی وجود کد طرح بیمه {planCode}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود نام طرح بیمه در ارائه‌دهنده
        /// </summary>
        public async Task<bool> DoesNameExistInProviderAsync(string name, int providerId, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return false;

                var query = _context.InsurancePlans
                    .Where(ip => ip.Name == name && ip.InsuranceProviderId == providerId);

                if (excludeId.HasValue)
                    query = query.Where(ip => ip.InsurancePlanId != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود نام طرح بیمه در ارائه‌دهنده. Name: {Name}, ProviderId: {ProviderId}, ExcludeId: {ExcludeId}", name, providerId, excludeId);
                throw new InvalidOperationException($"خطا در بررسی وجود نام طرح بیمه {name} در ارائه‌دهنده {providerId}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود طرح بیمه
        /// </summary>
        public async Task<bool> DoesExistAsync(int id)
        {
            try
            {
                return await _context.InsurancePlans
                    .Where(ip => ip.InsurancePlanId == id)
                    .AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود طرح بیمه. InsurancePlanId: {InsurancePlanId}", id);
                throw new InvalidOperationException($"خطا در بررسی وجود طرح بیمه {id}", ex);
            }
        }

        #endregion

        #region Search Operations

        /// <summary>
        /// جستجوی طرح‌های بیمه بر اساس عبارت جستجو
        /// </summary>
        public async Task<List<InsurancePlan>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var term = searchTerm.Trim();
                return await _context.InsurancePlans
                    .Where(ip => ip.Name.Contains(term) || 
                                ip.PlanCode.Contains(term) || 
                                ip.InsuranceProvider.Name.Contains(term))
                    .Include(ip => ip.InsuranceProvider)
                    .OrderBy(ip => ip.InsuranceProvider.Name)
                    .ThenBy(ip => ip.Name)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی طرح‌های بیمه. SearchTerm: {SearchTerm}", searchTerm);
                throw new InvalidOperationException($"خطا در جستجوی طرح‌های بیمه با عبارت {searchTerm}", ex);
            }
        }

        /// <summary>
        /// جستجوی طرح‌های بیمه فعال بر اساس عبارت جستجو
        /// </summary>
        public async Task<List<InsurancePlan>> SearchActiveAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetActiveAsync();

                var term = searchTerm.Trim();
                return await _context.InsurancePlans
                    .Where(ip => ip.IsActive && 
                                (ip.Name.Contains(term) || 
                                 ip.PlanCode.Contains(term) || 
                                 ip.InsuranceProvider.Name.Contains(term)))
                    .Include(ip => ip.InsuranceProvider)
                    .OrderBy(ip => ip.InsuranceProvider.Name)
                    .ThenBy(ip => ip.Name)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی طرح‌های بیمه فعال. SearchTerm: {SearchTerm}", searchTerm);
                throw new InvalidOperationException($"خطا در جستجوی طرح‌های بیمه فعال با عبارت {searchTerm}", ex);
            }
        }

        /// <summary>
        /// جستجوی طرح‌های بیمه بر اساس ارائه‌دهنده و عبارت جستجو
        /// </summary>
        public async Task<List<InsurancePlan>> SearchByProviderAsync(int providerId, string searchTerm)
        {
            try
            {
                var query = _context.InsurancePlans
                    .Where(ip => ip.InsuranceProviderId == providerId);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim();
                    query = query.Where(ip => ip.Name.Contains(term) || ip.PlanCode.Contains(term));
                }

                return await query
                    .Include(ip => ip.InsuranceProvider)
                    .OrderBy(ip => ip.Name)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی طرح‌های بیمه بر اساس ارائه‌دهنده. ProviderId: {ProviderId}, SearchTerm: {SearchTerm}", providerId, searchTerm);
                throw new InvalidOperationException($"خطا در جستجوی طرح‌های بیمه ارائه‌دهنده {providerId} با عبارت {searchTerm}", ex);
            }
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// افزودن طرح بیمه جدید
        /// </summary>
        public void Add(InsurancePlan plan)
        {
            try
            {
                if (plan == null)
                    throw new ArgumentNullException(nameof(plan));

                // تنظیم اطلاعات Audit
                var currentUser = _currentUserService.GetCurrentUserId();
                plan.CreatedAt = DateTime.Now;
                plan.CreatedByUserId = currentUser;
                plan.UpdatedAt = DateTime.Now;
                plan.CreatedByUserId = currentUser;

                _context.InsurancePlans.Add(plan);
                _logger.Information("طرح بیمه جدید اضافه شد. Name: {Name}, PlanCode: {PlanCode}, ProviderId: {ProviderId}", 
                    plan.Name, plan.PlanCode, plan.InsuranceProviderId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزودن طرح بیمه. Name: {Name}, PlanCode: {PlanCode}, ProviderId: {ProviderId}", 
                    plan?.Name, plan?.PlanCode, plan?.InsuranceProviderId);
                throw new InvalidOperationException("خطا در افزودن طرح بیمه", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی طرح بیمه
        /// </summary>
        public void Update(InsurancePlan plan)
        {
            try
            {
                if (plan == null)
                    throw new ArgumentNullException(nameof(plan));

                // تنظیم اطلاعات Audit
                var currentUser = _currentUserService.GetCurrentUserId();
                plan.UpdatedAt = DateTime.Now;
                plan.CreatedByUserId = currentUser;

                _context.Entry(plan).State = EntityState.Modified;
                _logger.Information("طرح بیمه به‌روزرسانی شد. InsurancePlanId: {InsurancePlanId}, Name: {Name}", 
                    plan.InsurancePlanId, plan.Name);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی طرح بیمه. InsurancePlanId: {InsurancePlanId}", plan?.InsurancePlanId);
                throw new InvalidOperationException("خطا در به‌روزرسانی طرح بیمه", ex);
            }
        }

        /// <summary>
        /// حذف نرم طرح بیمه
        /// </summary>
        public void Delete(InsurancePlan plan)
        {
            try
            {
                if (plan == null)
                    throw new ArgumentNullException(nameof(plan));

                // تنظیم اطلاعات Audit برای حذف نرم
                var currentUser = _currentUserService.GetCurrentUserId();
                plan.IsDeleted = true;
                plan.UpdatedAt = DateTime.Now;
                plan.UpdatedByUserId = currentUser;

                _context.Entry(plan).State = EntityState.Modified;
                _logger.Information("طرح بیمه حذف شد. InsurancePlanId: {InsurancePlanId}, Name: {Name}", 
                    plan.InsurancePlanId, plan.Name);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف طرح بیمه. InsurancePlanId: {InsurancePlanId}", plan?.InsurancePlanId);
                throw new InvalidOperationException("خطا در حذف طرح بیمه", ex);
            }
        }

        #endregion

        #region Database Operations

        /// <summary>
        /// ذخیره تغییرات در پایگاه داده
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                var result = await _context.SaveChangesAsync();
                _logger.Information("تغییرات طرح‌های بیمه ذخیره شد. تعداد رکوردهای تأثیرپذیرفته: {Count}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره تغییرات طرح‌های بیمه");
                throw new InvalidOperationException("خطا در ذخیره تغییرات طرح‌های بیمه", ex);
            }
        }

        #endregion
    }
}
