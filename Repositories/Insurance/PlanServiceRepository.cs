using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Insurance;
using Serilog;

namespace ClinicApp.Repositories.Insurance
{
    /// <summary>
    /// پیاده‌سازی Repository برای مدیریت خدمات طرح‌های بیمه در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت خدمات طرح‌های بیمه (Copay, CoverageOverride)
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت بیمه‌ها
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات بیمه‌ای
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. بهینه‌سازی عملکرد با AsNoTracking و Include
    /// 6. مدیریت روابط با InsurancePlan و ServiceCategory
    /// 7. محاسبه پوشش بیمه برای خدمات مختلف
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class PlanServiceRepository : IPlanServiceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public PlanServiceRepository(ApplicationDbContext context, ILogger logger, ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Core CRUD Operations

        /// <summary>
        /// دریافت خدمات طرح بیمه بر اساس شناسه
        /// </summary>
        public async Task<PlanService> GetByIdAsync(int id)
        {
            try
            {
                return await _context.PlanServices
                    .Where(ps => ps.PlanServiceId == id)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات طرح بیمه. PlanServiceId: {PlanServiceId}", id);
                throw new InvalidOperationException($"خطا در دریافت خدمات طرح بیمه {id}", ex);
            }
        }

        /// <summary>
        /// دریافت خدمات طرح بیمه بر اساس شناسه همراه با جزئیات کامل
        /// </summary>
        public async Task<PlanService> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _context.PlanServices
                    .Where(ps => ps.PlanServiceId == id)
                    .Include(ps => ps.InsurancePlan.InsuranceProvider)
                    .Include(ps => ps.ServiceCategory.Department)
                    .Include(ps => ps.CreatedByUser)
                    .Include(ps => ps.UpdatedByUser)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات طرح بیمه با جزئیات. PlanServiceId: {PlanServiceId}", id);
                throw new InvalidOperationException($"خطا در دریافت خدمات طرح بیمه {id} با جزئیات", ex);
            }
        }

        /// <summary>
        /// دریافت تمام خدمات طرح‌های بیمه
        /// </summary>
        public async Task<List<PlanService>> GetAllAsync()
        {
            try
            {
                return await _context.PlanServices
                    .Include(ps => ps.InsurancePlan.InsuranceProvider)
                    .Include(ps => ps.ServiceCategory.Department)
                    .OrderBy(ps => ps.InsurancePlan.InsuranceProvider.Name)
                    .ThenBy(ps => ps.InsurancePlan.Name)
                    .ThenBy(ps => ps.ServiceCategory.Title)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تمام خدمات طرح‌های بیمه");
                throw new InvalidOperationException("خطا در دریافت تمام خدمات طرح‌های بیمه", ex);
            }
        }

        /// <summary>
        /// دریافت خدمات طرح‌های بیمه فعال
        /// </summary>
        public async Task<List<PlanService>> GetActiveAsync()
        {
            try
            {
                return await _context.PlanServices
                    .Where(ps => ps.IsCovered)
                    .Include(ps => ps.InsurancePlan.InsuranceProvider)
                    .Include(ps => ps.ServiceCategory.Department)
                    .OrderBy(ps => ps.InsurancePlan.InsuranceProvider.Name)
                    .ThenBy(ps => ps.InsurancePlan.Name)
                    .ThenBy(ps => ps.ServiceCategory.Title)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات طرح‌های بیمه فعال");
                throw new InvalidOperationException("خطا در دریافت خدمات طرح‌های بیمه فعال", ex);
            }
        }

        /// <summary>
        /// دریافت خدمات طرح بیمه بر اساس شناسه طرح
        /// </summary>
        public async Task<List<PlanService>> GetByPlanIdAsync(int planId)
        {
            try
            {
                return await _context.PlanServices
                    .Where(ps => ps.InsurancePlanId == planId)
                    .Include(ps => ps.ServiceCategory.Department)
                    .OrderBy(ps => ps.ServiceCategory.Title)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات طرح بیمه. PlanId: {PlanId}", planId);
                throw new InvalidOperationException($"خطا در دریافت خدمات طرح بیمه {planId}", ex);
            }
        }

        /// <summary>
        /// دریافت خدمات طرح بیمه فعال بر اساس شناسه طرح
        /// </summary>
        public async Task<List<PlanService>> GetActiveByPlanIdAsync(int planId)
        {
            try
            {
                return await _context.PlanServices
                    .Where(ps => ps.InsurancePlanId == planId && ps.IsCovered)
                    .Include(ps => ps.ServiceCategory.Department)
                    .OrderBy(ps => ps.ServiceCategory.Title)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات طرح بیمه فعال. PlanId: {PlanId}", planId);
                throw new InvalidOperationException($"خطا در دریافت خدمات طرح بیمه فعال {planId}", ex);
            }
        }

        /// <summary>
        /// دریافت خدمات طرح‌های بیمه بر اساس شناسه دسته‌بندی خدمات
        /// </summary>
        public async Task<List<PlanService>> GetByServiceCategoryIdAsync(int serviceCategoryId)
        {
            try
            {
                return await _context.PlanServices
                    .Where(ps => ps.ServiceCategoryId == serviceCategoryId)
                    .Include(ps => ps.InsurancePlan.InsuranceProvider)
                    .OrderBy(ps => ps.InsurancePlan.InsuranceProvider.Name)
                    .ThenBy(ps => ps.InsurancePlan.Name)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات طرح‌های بیمه بر اساس دسته‌بندی. ServiceCategoryId: {ServiceCategoryId}", serviceCategoryId);
                throw new InvalidOperationException($"خطا در دریافت خدمات طرح‌های بیمه دسته‌بندی {serviceCategoryId}", ex);
            }
        }

        /// <summary>
        /// دریافت خدمات طرح بیمه بر اساس طرح و دسته‌بندی خدمات
        /// </summary>
        public async Task<PlanService> GetByPlanAndCategoryAsync(int planId, int serviceCategoryId)
        {
            try
            {
                return await _context.PlanServices
                    .Where(ps => ps.InsurancePlanId == planId && ps.ServiceCategoryId == serviceCategoryId)
                    .Include(ps => ps.InsurancePlan.InsuranceProvider)
                    .Include(ps => ps.ServiceCategory.Department)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات طرح بیمه. PlanId: {PlanId}, ServiceCategoryId: {ServiceCategoryId}", planId, serviceCategoryId);
                throw new InvalidOperationException($"خطا در دریافت خدمات طرح بیمه {planId} برای دسته‌بندی {serviceCategoryId}", ex);
            }
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود خدمات طرح بیمه برای طرح و دسته‌بندی
        /// </summary>
        public async Task<bool> DoesPlanServiceExistAsync(int planId, int serviceCategoryId, int? excludeId = null)
        {
            try
            {
                var query = _context.PlanServices
                    .Where(ps => ps.InsurancePlanId == planId && ps.ServiceCategoryId == serviceCategoryId);

                if (excludeId.HasValue)
                    query = query.Where(ps => ps.PlanServiceId != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود خدمات طرح بیمه. PlanId: {PlanId}, ServiceCategoryId: {ServiceCategoryId}, ExcludeId: {ExcludeId}", 
                    planId, serviceCategoryId, excludeId);
                throw new InvalidOperationException($"خطا در بررسی وجود خدمات طرح بیمه {planId} برای دسته‌بندی {serviceCategoryId}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود خدمات طرح بیمه
        /// </summary>
        public async Task<bool> DoesExistAsync(int id)
        {
            try
            {
                return await _context.PlanServices
                    .Where(ps => ps.PlanServiceId == id)
                    .AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود خدمات طرح بیمه. PlanServiceId: {PlanServiceId}", id);
                throw new InvalidOperationException($"خطا در بررسی وجود خدمات طرح بیمه {id}", ex);
            }
        }

        #endregion

        #region Search Operations

        /// <summary>
        /// جستجوی خدمات طرح‌های بیمه بر اساس عبارت جستجو
        /// </summary>
        public async Task<List<PlanService>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var term = searchTerm.Trim();
                return await _context.PlanServices
                    .Where(ps => ps.InsurancePlan.Name.Contains(term) || 
                                ps.InsurancePlan.PlanCode.Contains(term) ||
                                ps.InsurancePlan.InsuranceProvider.Name.Contains(term) ||
                                ps.ServiceCategory.Title.Contains(term) ||
                                ps.ServiceCategory.Department.Name.Contains(term))
                    .Include(ps => ps.InsurancePlan.InsuranceProvider)
                    .Include(ps => ps.ServiceCategory.Department)
                    .OrderBy(ps => ps.InsurancePlan.InsuranceProvider.Name)
                    .ThenBy(ps => ps.InsurancePlan.Name)
                    .ThenBy(ps => ps.ServiceCategory.Title)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی خدمات طرح‌های بیمه. SearchTerm: {SearchTerm}", searchTerm);
                throw new InvalidOperationException($"خطا در جستجوی خدمات طرح‌های بیمه با عبارت {searchTerm}", ex);
            }
        }

        /// <summary>
        /// جستجوی خدمات طرح‌های بیمه فعال بر اساس عبارت جستجو
        /// </summary>
        public async Task<List<PlanService>> SearchActiveAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetActiveAsync();

                var term = searchTerm.Trim();
                return await _context.PlanServices
                    .Where(ps => ps.IsCovered && 
                                (ps.InsurancePlan.Name.Contains(term) || 
                                 ps.InsurancePlan.PlanCode.Contains(term) ||
                                 ps.InsurancePlan.InsuranceProvider.Name.Contains(term) ||
                                 ps.ServiceCategory.Title.Contains(term) ||
                                 ps.ServiceCategory.Department.Name.Contains(term)))
                    .Include(ps => ps.InsurancePlan.InsuranceProvider)
                    .Include(ps => ps.ServiceCategory.Department)
                    .OrderBy(ps => ps.InsurancePlan.InsuranceProvider.Name)
                    .ThenBy(ps => ps.InsurancePlan.Name)
                    .ThenBy(ps => ps.ServiceCategory.Title)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی خدمات طرح‌های بیمه فعال. SearchTerm: {SearchTerm}", searchTerm);
                throw new InvalidOperationException($"خطا در جستجوی خدمات طرح‌های بیمه فعال با عبارت {searchTerm}", ex);
            }
        }

        /// <summary>
        /// جستجوی خدمات طرح بیمه بر اساس شناسه طرح و عبارت جستجو
        /// </summary>
        public async Task<List<PlanService>> SearchByPlanAsync(int planId, string searchTerm)
        {
            try
            {
                var query = _context.PlanServices
                    .Where(ps => ps.InsurancePlanId == planId);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim();
                    query = query.Where(ps => ps.ServiceCategory.Title.Contains(term) ||
                                            ps.ServiceCategory.Department.Name.Contains(term));
                }

                return await query
                    .Include(ps => ps.ServiceCategory.Department)
                    .OrderBy(ps => ps.ServiceCategory.Title)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی خدمات طرح بیمه. PlanId: {PlanId}, SearchTerm: {SearchTerm}", planId, searchTerm);
                throw new InvalidOperationException($"خطا در جستجوی خدمات طرح بیمه {planId} با عبارت {searchTerm}", ex);
            }
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// افزودن خدمات طرح بیمه جدید
        /// </summary>
        public void Add(PlanService planService)
        {
            try
            {
                if (planService == null)
                    throw new ArgumentNullException(nameof(planService));

                // تنظیم اطلاعات Audit
                var currentUser = _currentUserService.GetCurrentUserId();
                planService.CreatedAt = DateTime.Now;
                planService.CreatedByUserId = currentUser;
                planService.UpdatedAt = DateTime.Now;
                planService.CreatedByUserId = currentUser;

                _context.PlanServices.Add(planService);
                _logger.Information("خدمات طرح بیمه جدید اضافه شد. PlanId: {PlanId}, ServiceCategoryId: {ServiceCategoryId}, Copay: {Copay}", 
                    planService.InsurancePlanId, planService.ServiceCategoryId, planService.Copay);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزودن خدمات طرح بیمه. PlanId: {PlanId}, ServiceCategoryId: {ServiceCategoryId}, Copay: {Copay}", 
                    planService?.InsurancePlanId, planService?.ServiceCategoryId, planService?.Copay);
                throw new InvalidOperationException("خطا در افزودن خدمات طرح بیمه", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی خدمات طرح بیمه
        /// </summary>
        public void Update(PlanService planService)
        {
            try
            {
                if (planService == null)
                    throw new ArgumentNullException(nameof(planService));

                // تنظیم اطلاعات Audit
                var currentUser = _currentUserService.GetCurrentUserId();
                planService.UpdatedAt = DateTime.Now;
                planService.UpdatedByUserId = currentUser;

                _context.Entry(planService).State = EntityState.Modified;
                _logger.Information("خدمات طرح بیمه به‌روزرسانی شد. PlanServiceId: {PlanServiceId}, PlanId: {PlanId}, ServiceCategoryId: {ServiceCategoryId}", 
                    planService.PlanServiceId, planService.InsurancePlanId, planService.ServiceCategoryId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی خدمات طرح بیمه. PlanServiceId: {PlanServiceId}", planService?.PlanServiceId);
                throw new InvalidOperationException("خطا در به‌روزرسانی خدمات طرح بیمه", ex);
            }
        }

        /// <summary>
        /// حذف نرم خدمات طرح بیمه
        /// </summary>
        public void Delete(PlanService planService)
        {
            try
            {
                if (planService == null)
                    throw new ArgumentNullException(nameof(planService));

                // تنظیم اطلاعات Audit برای حذف نرم
                var currentUser = _currentUserService.GetCurrentUserId();
                planService.IsDeleted = true;
                planService.UpdatedAt = DateTime.Now;
                planService.UpdatedByUserId = currentUser;

                _context.Entry(planService).State = EntityState.Modified;
                _logger.Information("خدمات طرح بیمه حذف شد. PlanServiceId: {PlanServiceId}, PlanId: {PlanId}, ServiceCategoryId: {ServiceCategoryId}", 
                    planService.PlanServiceId, planService.InsurancePlanId, planService.ServiceCategoryId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف خدمات طرح بیمه. PlanServiceId: {PlanServiceId}", planService?.PlanServiceId);
                throw new InvalidOperationException("خطا در حذف خدمات طرح بیمه", ex);
            }
        }

        #endregion

        #region IPlanServiceRepository Implementation

        public async Task<ServiceResult<PlanService>> GetByPlanAndServiceCategoryAsync(int planId, int serviceCategoryId)
        {
            try
            {
                _logger.Information("Getting plan service by PlanId: {PlanId}, ServiceCategoryId: {ServiceCategoryId}", planId, serviceCategoryId);

                var planService = await _context.PlanServices
                    .Include(ps => ps.InsurancePlan)
                    .Include(ps => ps.ServiceCategory)
                    .FirstOrDefaultAsync(ps => ps.InsurancePlanId == planId && 
                                              ps.ServiceCategoryId == serviceCategoryId && 
                                              !ps.IsDeleted);

                if (planService == null)
                {
                    return ServiceResult<PlanService>.Failed("خدمات طرح بیمه یافت نشد");
                }

                return ServiceResult<PlanService>.Successful(planService);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting plan service by PlanId: {PlanId}, ServiceCategoryId: {ServiceCategoryId}", planId, serviceCategoryId);
                return ServiceResult<PlanService>.Failed("خطا در دریافت خدمات طرح بیمه");
            }
        }

        /// <summary>
        /// دریافت پیکربندی بیمه بر اساس ServiceId
        /// </summary>
        public async Task<ServiceResult<PlanService>> GetByPlanAndServiceAsync(int planId, int serviceId)
        {
            try
            {
                _logger.Information("Getting plan service by PlanId: {PlanId}, ServiceId: {ServiceId}", planId, serviceId);

                // ابتدا ServiceCategoryId را از Service دریافت کن
                var service = await _context.Services
                    .Where(s => s.ServiceId == serviceId && !s.IsDeleted)
                    .Select(s => new { s.ServiceCategoryId })
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    return ServiceResult<PlanService>.Failed("خدمت یافت نشد");
                }

                // سپس PlanService را بر اساس ServiceCategoryId جستجو کن
                var planService = await _context.PlanServices
                    .Include(ps => ps.InsurancePlan)
                    .Include(ps => ps.ServiceCategory)
                    .FirstOrDefaultAsync(ps => ps.InsurancePlanId == planId && 
                                              ps.ServiceCategoryId == service.ServiceCategoryId && 
                                              !ps.IsDeleted);

                if (planService == null)
                {
                    return ServiceResult<PlanService>.Failed("پیکربندی بیمه برای این خدمت یافت نشد");
                }

                return ServiceResult<PlanService>.Successful(planService);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting plan service by PlanId: {PlanId}, ServiceId: {ServiceId}", planId, serviceId);
                return ServiceResult<PlanService>.Failed("خطا در دریافت پیکربندی بیمه");
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
                _logger.Information("تغییرات خدمات طرح‌های بیمه ذخیره شد. تعداد رکوردهای تأثیرپذیرفته: {Count}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره تغییرات خدمات طرح‌های بیمه");
                throw new InvalidOperationException("خطا در ذخیره تغییرات خدمات طرح‌های بیمه", ex);
            }
        }

        #endregion
    }
}
