using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Repositories.Insurance
{
    /// <summary>
    /// پیاده‌سازی Repository برای مدیریت ارائه‌دهندگان بیمه در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت ارائه‌دهندگان بیمه (SSO, FREE, MILITARY, HEALTH, SUPPLEMENTARY)
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت بیمه‌ها
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات بیمه‌ای
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. بهینه‌سازی عملکرد با AsNoTracking و Include
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class InsuranceProviderRepository : IInsuranceProviderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public InsuranceProviderRepository(ApplicationDbContext context, ILogger logger, ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Core CRUD Operations

        /// <summary>
        /// دریافت ارائه‌دهنده بیمه بر اساس شناسه
        /// </summary>
        public async Task<InsuranceProvider> GetByIdAsync(int id)
        {
            try
            {
                return await _context.InsuranceProviders
                    .Where(ip => ip.InsuranceProviderId == id)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ارائه‌دهنده بیمه. InsuranceProviderId: {InsuranceProviderId}", id);
                throw new InvalidOperationException($"خطا در دریافت ارائه‌دهنده بیمه {id}", ex);
            }
        }

        /// <summary>
        /// دریافت ارائه‌دهنده بیمه بر اساس شناسه همراه با جزئیات کامل
        /// </summary>
        public async Task<InsuranceProvider> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _context.InsuranceProviders
                    .Where(ip => ip.InsuranceProviderId == id)
                    .Include(ip => ip.InsurancePlans)
                    .Include(ip => ip.CreatedByUser)
                    .Include(ip => ip.UpdatedByUser)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ارائه‌دهنده بیمه با جزئیات. InsuranceProviderId: {InsuranceProviderId}", id);
                throw new InvalidOperationException($"خطا در دریافت ارائه‌دهنده بیمه {id} با جزئیات", ex);
            }
        }

        /// <summary>
        /// دریافت تمام ارائه‌دهندگان بیمه
        /// </summary>
        public async Task<List<InsuranceProvider>> GetAllAsync()
        {
            try
            {
                return await _context.InsuranceProviders
                    .OrderBy(ip => ip.Name)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تمام ارائه‌دهندگان بیمه");
                throw new InvalidOperationException("خطا در دریافت تمام ارائه‌دهندگان بیمه", ex);
            }
        }

        /// <summary>
        /// دریافت ارائه‌دهندگان بیمه فعال
        /// </summary>
        public async Task<List<InsuranceProvider>> GetActiveAsync()
        {
            try
            {
                _logger.Debug("🔍 ANTI-BULLET: شروع دریافت ارائه‌دهندگان بیمه فعال از دیتابیس");
                
                // 🔧 DYNAMIC_SQL: استفاده از Dynamic SQL Helper
                var dynamicQuery = DynamicSqlHelper.GenerateSelectQuery<InsuranceProvider>(
                    "InsuranceProviders",
                    DynamicSqlHelper.GetActiveFilterWhereClause(),
                    DynamicSqlHelper.GetNameOrderByClause()
                );
                
                _logger.Debug("🔧 DYNAMIC_SQL: Query تولید شده: {Query}", dynamicQuery);
                
                // 🔍 ANTI-BULLET: تست مستقیم دیتابیس با Dynamic SQL
                var directQuery = await _context.Database.SqlQuery<InsuranceProvider>(dynamicQuery)
                    .ToListAsync();
                
                _logger.Debug("🔍 ANTI-BULLET: Direct Query موفق - تعداد: {Count}", directQuery.Count);
                
                // 🔍 ANTI-BULLET: تست Entity Framework
                var efQuery = await _context.InsuranceProviders
                    .Where(ip => ip.IsActive)
                    .OrderBy(ip => ip.Name)
                    .AsNoTracking()
                    .ToListAsync();
                
                _logger.Debug("🔍 ANTI-BULLET: Entity Framework Query موفق - تعداد: {Count}", efQuery.Count);
                
                return efQuery;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🔍 ANTI-BULLET: خطا در دریافت ارائه‌دهندگان بیمه فعال - Type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                    ex.GetType().Name, ex.Message, ex.StackTrace);
                throw new InvalidOperationException("خطا در دریافت ارائه‌دهندگان بیمه فعال", ex);
            }
        }

        /// <summary>
        /// دریافت ارائه‌دهنده بیمه بر اساس کد
        /// </summary>
        public async Task<InsuranceProvider> GetByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return null;

                return await _context.InsuranceProviders
                    .Where(ip => ip.Code == code)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ارائه‌دهنده بیمه بر اساس کد. Code: {Code}", code);
                throw new InvalidOperationException($"خطا در دریافت ارائه‌دهنده بیمه با کد {code}", ex);
            }
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود کد ارائه‌دهنده بیمه
        /// </summary>
        public async Task<bool> DoesCodeExistAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                var query = _context.InsuranceProviders
                    .Where(ip => ip.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(ip => ip.InsuranceProviderId != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود کد ارائه‌دهنده بیمه. Code: {Code}, ExcludeId: {ExcludeId}", code, excludeId);
                throw new InvalidOperationException($"خطا در بررسی وجود کد ارائه‌دهنده بیمه {code}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود نام ارائه‌دهنده بیمه
        /// </summary>
        public async Task<bool> DoesNameExistAsync(string name, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return false;

                var query = _context.InsuranceProviders
                    .Where(ip => ip.Name == name);

                if (excludeId.HasValue)
                    query = query.Where(ip => ip.InsuranceProviderId != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود نام ارائه‌دهنده بیمه. Name: {Name}, ExcludeId: {ExcludeId}", name, excludeId);
                throw new InvalidOperationException($"خطا در بررسی وجود نام ارائه‌دهنده بیمه {name}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود ارائه‌دهنده بیمه
        /// </summary>
        public async Task<bool> DoesExistAsync(int id)
        {
            try
            {
                return await _context.InsuranceProviders
                    .Where(ip => ip.InsuranceProviderId == id)
                    .AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود ارائه‌دهنده بیمه. InsuranceProviderId: {InsuranceProviderId}", id);
                throw new InvalidOperationException($"خطا در بررسی وجود ارائه‌دهنده بیمه {id}", ex);
            }
        }

        #endregion

        #region Search Operations

        /// <summary>
        /// جستجوی ارائه‌دهندگان بیمه بر اساس عبارت جستجو
        /// </summary>
        public async Task<List<InsuranceProvider>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var term = searchTerm.Trim();
                return await _context.InsuranceProviders
                    .Where(ip => ip.Name.Contains(term) || 
                                ip.Code.Contains(term) || 
                                ip.ContactInfo.Contains(term))
                    .OrderBy(ip => ip.Name)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی ارائه‌دهندگان بیمه. SearchTerm: {SearchTerm}", searchTerm);
                throw new InvalidOperationException($"خطا در جستجوی ارائه‌دهندگان بیمه با عبارت {searchTerm}", ex);
            }
        }

        /// <summary>
        /// جستجوی ارائه‌دهندگان بیمه فعال بر اساس عبارت جستجو
        /// </summary>
        public async Task<List<InsuranceProvider>> SearchActiveAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetActiveAsync();

                var term = searchTerm.Trim();
                return await _context.InsuranceProviders
                    .Where(ip => ip.IsActive && 
                                (ip.Name.Contains(term) || 
                                 ip.Code.Contains(term) || 
                                 ip.ContactInfo.Contains(term)))
                    .OrderBy(ip => ip.Name)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی ارائه‌دهندگان بیمه فعال. SearchTerm: {SearchTerm}", searchTerm);
                throw new InvalidOperationException($"خطا در جستجوی ارائه‌دهندگان بیمه فعال با عبارت {searchTerm}", ex);
            }
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// افزودن ارائه‌دهنده بیمه جدید
        /// </summary>
        public void Add(InsuranceProvider provider)
        {
            try
            {
                if (provider == null)
                    throw new ArgumentNullException(nameof(provider));

                // تنظیم اطلاعات Audit
                var currentUser = _currentUserService.GetCurrentUserId();
                provider.CreatedAt = DateTime.Now;
                provider.CreatedByUserId = currentUser;
                provider.UpdatedAt = DateTime.Now;
                provider.UpdatedByUserId = currentUser;

                _context.InsuranceProviders.Add(provider);
                _logger.Information("ارائه‌دهنده بیمه جدید اضافه شد. Name: {Name}, Code: {Code}", provider.Name, provider.Code);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزودن ارائه‌دهنده بیمه. Name: {Name}, Code: {Code}", provider?.Name, provider?.Code);
                throw new InvalidOperationException("خطا در افزودن ارائه‌دهنده بیمه", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی ارائه‌دهنده بیمه
        /// </summary>
        public void Update(InsuranceProvider provider)
        {
            try
            {
                if (provider == null)
                    throw new ArgumentNullException(nameof(provider));

                // تنظیم اطلاعات Audit
                var currentUser = _currentUserService.GetCurrentUserId();
                provider.UpdatedAt = DateTime.Now;
                provider.CreatedByUserId = currentUser;

                _context.Entry(provider).State = EntityState.Modified;
                _logger.Information("ارائه‌دهنده بیمه به‌روزرسانی شد. InsuranceProviderId: {InsuranceProviderId}, Name: {Name}", 
                    provider.InsuranceProviderId, provider.Name);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی ارائه‌دهنده بیمه. InsuranceProviderId: {InsuranceProviderId}", provider?.InsuranceProviderId);
                throw new InvalidOperationException("خطا در به‌روزرسانی ارائه‌دهنده بیمه", ex);
            }
        }

        /// <summary>
        /// حذف نرم ارائه‌دهنده بیمه
        /// </summary>
        public void Delete(InsuranceProvider provider)
        {
            try
            {
                if (provider == null)
                    throw new ArgumentNullException(nameof(provider));

                // تنظیم اطلاعات Audit برای حذف نرم
                var currentUser = _currentUserService.GetCurrentUserId();
                provider.IsDeleted = true;
                provider.UpdatedAt = DateTime.Now;
                provider.CreatedByUserId = currentUser;

                _context.Entry(provider).State = EntityState.Modified;
                _logger.Information("ارائه‌دهنده بیمه حذف شد. InsuranceProviderId: {InsuranceProviderId}, Name: {Name}", 
                    provider.InsuranceProviderId, provider.Name);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف ارائه‌دهنده بیمه. InsuranceProviderId: {InsuranceProviderId}", provider?.InsuranceProviderId);
                throw new InvalidOperationException("خطا در حذف ارائه‌دهنده بیمه", ex);
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
                _logger.Information("تغییرات ارائه‌دهندگان بیمه ذخیره شد. تعداد رکوردهای تأثیرپذیرفته: {Count}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره تغییرات ارائه‌دهندگان بیمه");
                throw new InvalidOperationException("خطا در ذخیره تغییرات ارائه‌دهندگان بیمه", ex);
            }
        }

        /// <summary>
        /// دریافت بیمه‌گذاران پایه
        /// </summary>
        public async Task<List<InsuranceProvider>> GetPrimaryInsuranceProvidersAsync()
        {
            try
            {
                _logger.Information("درخواست دریافت بیمه‌گذاران پایه");

                var providers = await _context.InsuranceProviders
                    .Where(p => p.IsActive && 
                               !p.IsDeleted &&
                               p.InsurancePlans.Any(plan => plan.InsuranceType == InsuranceType.Primary && 
                                                           plan.IsActive && 
                                                           !plan.IsDeleted))
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                _logger.Information("بیمه‌گذاران پایه دریافت شد. تعداد: {Count}", providers.Count);

                return providers;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه‌گذاران پایه");
                throw new InvalidOperationException("خطا در دریافت بیمه‌گذاران پایه", ex);
            }
        }

        /// <summary>
        /// دریافت بیمه‌گذاران تکمیلی
        /// </summary>
        public async Task<List<InsuranceProvider>> GetSupplementaryInsuranceProvidersAsync()
        {
            try
            {
                _logger.Information("درخواست دریافت بیمه‌گذاران تکمیلی");

                var providers = await _context.InsuranceProviders
                    .Where(p => p.IsActive && 
                               !p.IsDeleted &&
                               p.InsurancePlans.Any(plan => plan.InsuranceType == InsuranceType.Supplementary && 
                                                           plan.IsActive && 
                                                           !plan.IsDeleted))
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                _logger.Information("بیمه‌گذاران تکمیلی دریافت شد. تعداد: {Count}", providers.Count);

                return providers;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه‌گذاران تکمیلی");
                throw new InvalidOperationException("خطا در دریافت بیمه‌گذاران تکمیلی", ex);
            }
        }

        #endregion
    }
}
