using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Enums;
using ClinicApp.Interfaces;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس مدیریت تنظیمات کای‌ها
    /// </summary>
    public class FactorSettingService : IFactorSettingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public FactorSettingService(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// دریافت تمام کای‌ها
        /// </summary>
        public async Task<IEnumerable<FactorSetting>> GetAllFactorsAsync()
        {
            try
            {
                return await _context.FactorSettings
                    .Where(f => !f.IsDeleted)
                    .OrderByDescending(f => f.FinancialYear)
                    .ThenBy(f => f.FactorType)
                    .ThenBy(f => f.IsHashtagged)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تمام کای‌ها");
                throw;
            }
        }

        /// <summary>
        /// دریافت کای بر اساس شناسه
        /// </summary>
        public async Task<FactorSetting> GetFactorByIdAsync(int id)
        {
            try
            {
                return await _context.FactorSettings
                    .Where(f => f.FactorSettingId == id && !f.IsDeleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای با شناسه {FactorId}", id);
                throw;
            }
        }

        /// <summary>
        /// دریافت کای‌ها بر اساس نوع و سال مالی
        /// </summary>
        public async Task<IEnumerable<FactorSetting>> GetFactorsByTypeAsync(ServiceComponentType factorType, int financialYear)
        {
            try
            {
                return await _context.FactorSettings
                    .Where(f => f.FactorType == factorType && 
                               f.FinancialYear == financialYear && 
                               !f.IsDeleted)
                    .OrderBy(f => f.IsHashtagged)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای‌ها برای نوع {FactorType} و سال مالی {FinancialYear}", factorType, financialYear);
                throw;
            }
        }

        /// <summary>
        /// دریافت کای فعال بر اساس نوع و سال مالی
        /// </summary>
        public async Task<FactorSetting> GetActiveFactorByTypeAsync(ServiceComponentType factorType, int financialYear, bool isHashtagged = false)
        {
            try
            {
                return await _context.FactorSettings
                    .Where(f => f.FactorType == factorType &&
                               f.FinancialYear == financialYear &&
                               f.IsHashtagged == isHashtagged &&
                               f.IsActive &&
                               !f.IsDeleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای فعال برای نوع {FactorType} و سال مالی {FinancialYear}", factorType, financialYear);
                throw;
            }
        }

        /// <summary>
        /// دریافت کای فعال بر اساس نوع، سال مالی و وضعیت هشتگ‌دار
        /// </summary>
        public async Task<FactorSetting> GetActiveFactorByTypeAndHashtaggedAsync(ServiceComponentType factorType, bool isHashtagged, int financialYear)
        {
            try
            {
                return await _context.FactorSettings
                    .Where(f => f.FactorType == factorType &&
                               f.FinancialYear == financialYear &&
                               f.IsHashtagged == isHashtagged &&
                               f.IsActive &&
                               !f.IsDeleted)
                    .OrderByDescending(f => f.EffectiveFrom) // دریافت آخرین کای فعال
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای فعال برای نوع {FactorType}، سال مالی {FinancialYear} و وضعیت هشتگ {IsHashtagged}", 
                    factorType, financialYear, isHashtagged);
                throw;
            }
        }

        /// <summary>
        /// ایجاد کای جدید
        /// </summary>
        public async Task<FactorSetting> CreateFactorAsync(FactorSetting factor)
        {
            try
            {
                // بررسی تکراری نبودن
                var existingFactor = await _context.FactorSettings
                    .Where(f => f.FactorType == factor.FactorType &&
                               f.FinancialYear == factor.FinancialYear &&
                               f.IsHashtagged == factor.IsHashtagged &&
                               !f.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingFactor != null)
                {
                    throw new InvalidOperationException($"کای {factor.FactorType} برای سال مالی {factor.FinancialYear} و وضعیت هشتگ {factor.IsHashtagged} قبلاً وجود دارد");
                }

                _context.FactorSettings.Add(factor);
                await _context.SaveChangesAsync();

                _logger.Information("کای جدید ایجاد شد: {FactorType} - سال مالی {FinancialYear} - هشتگ {IsHashtagged}", 
                    factor.FactorType, factor.FinancialYear, factor.IsHashtagged);

                return factor;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد کای جدید");
                throw;
            }
        }

        /// <summary>
        /// به‌روزرسانی کای
        /// </summary>
        public async Task<FactorSetting> UpdateFactorAsync(FactorSetting factor)
        {
            try
            {
                var existingFactor = await GetFactorByIdAsync(factor.FactorSettingId);
                if (existingFactor == null)
                {
                    throw new InvalidOperationException("کای مورد نظر یافت نشد");
                }

                // بررسی تکراری نبودن (به جز خود رکورد)
                var duplicateFactor = await _context.FactorSettings
                    .Where(f => f.FactorType == factor.FactorType &&
                               f.FinancialYear == factor.FinancialYear &&
                               f.IsHashtagged == factor.IsHashtagged &&
                               f.FactorSettingId != factor.FactorSettingId &&
                               !f.IsDeleted)
                    .FirstOrDefaultAsync();

                if (duplicateFactor != null)
                {
                    throw new InvalidOperationException($"کای {factor.FactorType} برای سال مالی {factor.FinancialYear} و وضعیت هشتگ {factor.IsHashtagged} قبلاً وجود دارد");
                }

                _context.Entry(existingFactor).CurrentValues.SetValues(factor);
                await _context.SaveChangesAsync();

                _logger.Information("کای به‌روزرسانی شد: {FactorType} - سال مالی {FinancialYear} - هشتگ {IsHashtagged}", 
                    factor.FactorType, factor.FinancialYear, factor.IsHashtagged);

                return existingFactor;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی کای {FactorId}", factor.FactorSettingId);
                throw;
            }
        }

        /// <summary>
        /// حذف کای
        /// </summary>
        public async Task DeleteFactorAsync(int id)
        {
            try
            {
                var factor = await GetFactorByIdAsync(id);
                if (factor == null)
                {
                    throw new InvalidOperationException("کای مورد نظر یافت نشد");
                }

                // Soft Delete
                factor.IsDeleted = true;
                factor.DeletedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.Information("کای حذف شد: {FactorId} - {FactorType}", id, factor.FactorType);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف کای {FactorId}", id);
                throw;
            }
        }

        /// <summary>
        /// بررسی وجود کای برای نوع و سال مالی مشخص
        /// </summary>
        public async Task<bool> ExistsFactorAsync(ServiceComponentType factorType, int financialYear, bool isHashtagged = false)
        {
            try
            {
                return await _context.FactorSettings
                    .AnyAsync(f => f.FactorType == factorType &&
                                  f.FinancialYear == financialYear &&
                                  f.IsHashtagged == isHashtagged &&
                                  !f.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود کای");
                throw;
            }
        }

        /// <summary>
        /// دریافت کای‌های سال مالی جاری
        /// </summary>
        public async Task<IEnumerable<FactorSetting>> GetCurrentYearFactorsAsync()
        {
            try
            {
                var currentYear = GetCurrentPersianYear();
                return await GetFactorsByFinancialYearAsync(currentYear);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای‌های سال جاری");
                throw;
            }
        }

        /// <summary>
        /// دریافت کای‌های سال مالی مشخص
        /// </summary>
        public async Task<IEnumerable<FactorSetting>> GetFactorsByFinancialYearAsync(int financialYear)
        {
            try
            {
                return await _context.FactorSettings
                    .Where(f => f.FinancialYear == financialYear && !f.IsDeleted)
                    .OrderBy(f => f.FactorType)
                    .ThenBy(f => f.IsHashtagged)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای‌های سال مالی {FinancialYear}", financialYear);
                throw;
            }
        }

        /// <summary>
        /// دریافت فیلتر شده کای‌ها با Pagination
        /// </summary>
        public async Task<IEnumerable<FactorSetting>> GetFilteredFactorsAsync(string searchTerm, ServiceComponentType? factorType, int? financialYear, bool? isActive, int page, int pageSize)
        {
            try
            {
                var query = _context.FactorSettings.Where(f => !f.IsDeleted);

                // فیلتر جستجو
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(f => f.Description.Contains(searchTerm));
                }

                // فیلتر نوع کای
                if (factorType.HasValue)
                {
                    query = query.Where(f => f.FactorType == factorType.Value);
                }

                // فیلتر سال مالی
                if (financialYear.HasValue)
                {
                    query = query.Where(f => f.FinancialYear == financialYear.Value);
                }

                // فیلتر وضعیت فعال
                if (isActive.HasValue)
                {
                    query = query.Where(f => f.IsActive == isActive.Value);
                }

                return await query
                    .OrderByDescending(f => f.FinancialYear)
                    .ThenBy(f => f.FactorType)
                    .ThenBy(f => f.IsHashtagged)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت فیلتر شده کای‌ها");
                throw;
            }
        }

        /// <summary>
        /// دریافت تعداد کای‌ها بر اساس فیلتر
        /// </summary>
        public async Task<int> GetFactorsCountAsync(string searchTerm, ServiceComponentType? factorType, int? financialYear, bool? isActive)
        {
            try
            {
                var query = _context.FactorSettings.Where(f => !f.IsDeleted);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(f => f.Description.Contains(searchTerm));
                }

                if (factorType.HasValue)
                {
                    query = query.Where(f => f.FactorType == factorType.Value);
                }

                if (financialYear.HasValue)
                {
                    query = query.Where(f => f.FinancialYear == financialYear.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(f => f.IsActive == isActive.Value);
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش کای‌ها");
                throw;
            }
        }

        /// <summary>
        /// دریافت تعداد کای‌های فعال برای سال مالی
        /// </summary>
        public async Task<int> GetActiveFactorsCountForYearAsync(int financialYear)
        {
            try
            {
                return await _context.FactorSettings
                    .Where(f => f.FinancialYear == financialYear && f.IsActive && !f.IsDeleted)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش کای‌های فعال سال مالی {FinancialYear}", financialYear);
                throw;
            }
        }

        /// <summary>
        /// بررسی استفاده از کای در محاسبات
        /// </summary>
        public async Task<bool> IsFactorUsedInCalculationsAsync(int factorId)
        {
            try
            {
                // بررسی استفاده در ServiceComponent
                var isUsedInServiceComponent = await _context.ServiceComponents
                    .AnyAsync(sc => sc.Coefficient > 0);

                // بررسی استفاده در محاسبات قبلی (در صورت وجود جدول محاسبات)
                // این بخش می‌تواند بعداً پیاده‌سازی شود

                return isUsedInServiceComponent;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی استفاده از کای {FactorId}", factorId);
                throw;
            }
        }

        /// <summary>
        /// فریز کردن کای‌های سال مالی
        /// </summary>
        public async Task<bool> FreezeFinancialYearFactorsAsync(int financialYear, string userId)
        {
            try
            {
                var factorsToFreeze = await _context.FactorSettings
                    .Where(f => f.FinancialYear == financialYear && !f.IsFrozen && !f.IsDeleted)
                    .ToListAsync();

                foreach (var factor in factorsToFreeze)
                {
                    factor.IsFrozen = true;
                    factor.FrozenAt = DateTime.UtcNow;
                    factor.FrozenByUserId = userId;
                    factor.UpdatedAt = DateTime.UtcNow;
                    factor.UpdatedByUserId = userId;
                }

                await _context.SaveChangesAsync();
                
                _logger.Information("کای‌های سال مالی {FinancialYear} فریز شدند - تعداد: {Count}", financialYear, factorsToFreeze.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فریز کردن کای‌های سال مالی {FinancialYear}", financialYear);
                throw;
            }
        }

        /// <summary>
        /// دریافت کای‌های فریز شده
        /// </summary>
        public async Task<IEnumerable<FactorSetting>> GetFrozenFactorsAsync(int? financialYear = null)
        {
            try
            {
                var query = _context.FactorSettings.Where(f => f.IsFrozen && !f.IsDeleted);

                if (financialYear.HasValue)
                {
                    query = query.Where(f => f.FinancialYear == financialYear.Value);
                }

                return await query
                    .OrderByDescending(f => f.FinancialYear)
                    .ThenBy(f => f.FactorType)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای‌های فریز شده");
                throw;
            }
        }

        /// <summary>
        /// دریافت سال مالی جاری شمسی
        /// </summary>
        private int GetCurrentPersianYear()
        {
            var persianCalendar = new System.Globalization.PersianCalendar();
            return persianCalendar.GetYear(DateTime.Now);
        }
    }
}
