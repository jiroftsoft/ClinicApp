using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.ViewModels.Insurance.InsuranceTariff;
using Serilog;

namespace ClinicApp.Repositories.Insurance
{
    /// <summary>
    /// Repository برای مدیریت تعرفه‌های بیمه
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// </summary>
    public class InsuranceTariffRepository : IInsuranceTariffRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public InsuranceTariffRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region CRUD Operations

        /// <summary>
        /// دریافت تعرفه بیمه بر اساس شناسه
        /// </summary>
        public async Task<InsuranceTariff> GetByIdAsync(int id)
        {
            try
            {
                return await _context.InsuranceTariffs
                    .Include(t => t.Service)
                    .Include(t => t.InsurancePlan)
                    .Include(t => t.InsurancePlan.InsuranceProvider)
                    .FirstOrDefaultAsync(t => t.InsuranceTariffId == id && !t.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه بیمه. Id: {Id}", id);
                throw new InvalidOperationException($"خطا در دریافت تعرفه بیمه {id}", ex);
            }
        }

        /// <summary>
        /// ایجاد تعرفه بیمه جدید
        /// </summary>
        public async Task<InsuranceTariff> CreateAsync(InsuranceTariff tariff)
        {
            try
            {
                _context.InsuranceTariffs.Add(tariff);
                await _context.SaveChangesAsync();
                
                _logger.Information("تعرفه بیمه با موفقیت ایجاد شد. Id: {Id}, ServiceId: {ServiceId}, PlanId: {PlanId}",
                    tariff.InsuranceTariffId, tariff.ServiceId, tariff.InsurancePlanId);
                
                return tariff;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تعرفه بیمه. ServiceId: {ServiceId}, PlanId: {PlanId}",
                    tariff.ServiceId, tariff.InsurancePlanId);
                throw new InvalidOperationException("خطا در ایجاد تعرفه بیمه", ex);
            }
        }

        /// <summary>
        /// دریافت تعرفه بیمه با جزئیات کامل
        /// </summary>
        public async Task<InsuranceTariff> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _context.InsuranceTariffs
                    .Include(t => t.Service)
                    .Include(t => t.Service.ServiceCategory)
                    .Include(t => t.Service.ServiceCategory.Department)
                    .Include(t => t.InsurancePlan)
                    .Include(t => t.InsurancePlan.InsuranceProvider)
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.UpdatedByUser)
                    .Include(t => t.DeletedByUser)
                    .FirstOrDefaultAsync(t => t.InsuranceTariffId == id && !t.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه بیمه با جزئیات. Id: {Id}", id);
                throw new InvalidOperationException($"خطا در دریافت تعرفه بیمه {id}", ex);
            }
        }

        /// <summary>
        /// دریافت تمام تعرفه‌های بیمه فعال
        /// </summary>
        public async Task<List<InsuranceTariff>> GetAllActiveAsync()
        {
            try
            {
                return await _context.InsuranceTariffs
                    .Include(t => t.Service)
                    .Include(t => t.InsurancePlan)
                    .Include(t => t.InsurancePlan.InsuranceProvider)
                    .Where(t => !t.IsDeleted)
                    .OrderBy(t => t.InsurancePlan.InsuranceProvider.Name)
                    .ThenBy(t => t.InsurancePlan.Name)
                    .ThenBy(t => t.Service.Title)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تمام تعرفه‌های بیمه فعال");
                throw new InvalidOperationException("خطا در دریافت تعرفه‌های بیمه فعال", ex);
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های بیمه با صفحه‌بندی - بهینه‌سازی شده با Projection
        /// </summary>
        public async Task<PagedResult<InsuranceTariff>> GetPagedAsync(
            int? planId = null,
            int? serviceId = null,
            int? providerId = null,
            string searchTerm = "",
            InsuranceType? insuranceType = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                _logger.Information("🔍 REPOSITORY: شروع GetPagedAsync - PlanId: {PlanId}, ServiceId: {ServiceId}, ProviderId: {ProviderId}, SearchTerm: {SearchTerm}, InsuranceType: {InsuranceType}", 
                    planId, serviceId, providerId, searchTerm, insuranceType);

                // بهینه‌سازی: استفاده از AsNoTracking برای read-only operations
                var query = _context.InsuranceTariffs
                    .AsNoTracking()
                    .Include(t => t.Service)
                    .Include(t => t.InsurancePlan)
                    .Include(t => t.InsurancePlan.InsuranceProvider)
                    .Where(t => !t.IsDeleted);

                _logger.Information("🔍 REPOSITORY: Query اولیه ساخته شد");

                // فیلتر بر اساس طرح بیمه
                if (planId.HasValue)
                {
                    query = query.Where(t => t.InsurancePlanId == planId.Value);
                    _logger.Information("🔍 REPOSITORY: فیلتر PlanId اضافه شد: {PlanId}", planId.Value);
                }

                // فیلتر بر اساس خدمت
                if (serviceId.HasValue)
                {
                    query = query.Where(t => t.ServiceId == serviceId.Value);
                    _logger.Information("🔍 REPOSITORY: فیلتر ServiceId اضافه شد: {ServiceId}", serviceId.Value);
                }

                // فیلتر بر اساس ارائه‌دهنده بیمه
                if (providerId.HasValue)
                {
                    query = query.Where(t => t.InsurancePlan.InsuranceProviderId == providerId.Value);
                    _logger.Information("🔍 REPOSITORY: فیلتر ProviderId اضافه شد: {ProviderId}", providerId.Value);
                }

                // فیلتر بر اساس نوع بیمه
                if (insuranceType.HasValue)
                {
                    query = query.Where(t => t.InsuranceType == insuranceType.Value);
                    _logger.Information("🔍 REPOSITORY: فیلتر InsuranceType اضافه شد: {InsuranceType}", insuranceType.Value);
                }

                // جستجو
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(t => 
                        t.Service.Title.Contains(searchTerm) ||
                        t.InsurancePlan.Name.Contains(searchTerm) ||
                        t.InsurancePlan.InsuranceProvider.Name.Contains(searchTerm));
                }

                // بهینه‌سازی: محاسبه همزمان totalCount و items
                var totalCountTask = query.CountAsync();
                var itemsTask = query
                    .OrderBy(t => t.InsurancePlan.InsuranceProvider.Name)
                    .ThenBy(t => t.InsurancePlan.Name)
                    .ThenBy(t => t.Service.Title)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                await Task.WhenAll(totalCountTask, itemsTask);

                var totalCount = await totalCountTask;
                var items = await itemsTask;

                _logger.Information("🔍 REPOSITORY: نتایج - TotalCount: {TotalCount}, ItemsCount: {ItemsCount}", totalCount, items.Count);

                // 🔍 DEBUG: بررسی داده‌های موجود در دیتابیس
                var allTariffs = await _context.InsuranceTariffs
                    .AsNoTracking()
                    .Include(t => t.Service)
                    .Include(t => t.InsurancePlan)
                    .Include(t => t.InsurancePlan.InsuranceProvider)
                    .Where(t => !t.IsDeleted)
                    .Take(5)
                    .Select(t => new {
                        Id = t.InsuranceTariffId,
                        t.ServiceId,
                        ServiceTitle = t.Service.Title,
                        t.InsurancePlanId,
                        PlanName = t.InsurancePlan.Name,
                        ProviderId = t.InsurancePlan.InsuranceProviderId,
                        ProviderName = t.InsurancePlan.InsuranceProvider.Name
                    })
                    .ToListAsync();

                _logger.Information("🔍 REPOSITORY: نمونه داده‌های موجود: {@AllTariffs}", allTariffs);

                // 🔍 DEBUG: بررسی فیلترهای اعمال شده
                if (planId.HasValue || serviceId.HasValue || providerId.HasValue)
                {
                    _logger.Information("🔍 REPOSITORY: فیلترهای اعمال شده - PlanId: {PlanId}, ServiceId: {ServiceId}, ProviderId: {ProviderId}", 
                        planId, serviceId, providerId);
                    
                    // بررسی داده‌های موجود با فیلترها
                    var matchingTariffs = await _context.InsuranceTariffs
                        .AsNoTracking()
                        .Include(t => t.Service)
                        .Include(t => t.InsurancePlan)
                        .Include(t => t.InsurancePlan.InsuranceProvider)
                        .Where(t => !t.IsDeleted)
                        .Select(t => new {
                            Id = t.InsuranceTariffId,
                            t.ServiceId,
                            ServiceTitle = t.Service.Title,
                            t.InsurancePlanId,
                            PlanName = t.InsurancePlan.Name,
                            ProviderId = t.InsurancePlan.InsuranceProviderId,
                            ProviderName = t.InsurancePlan.InsuranceProvider.Name
                        })
                        .ToListAsync();

                    _logger.Information("🔍 REPOSITORY: تمام داده‌های موجود: {@MatchingTariffs}", matchingTariffs);
                }

                return new PagedResult<InsuranceTariff>
                {
                    Items = items,
                    TotalItems = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌های بیمه با صفحه‌بندی");
                throw new InvalidOperationException("خطا در دریافت تعرفه‌های بیمه", ex);
            }
        }

        /// <summary>
        /// افزودن تعرفه بیمه جدید
        /// </summary>
        public async Task<InsuranceTariff> AddAsync(InsuranceTariff tariff)
        {
            try
            {
                _context.InsuranceTariffs.Add(tariff);
                await _context.SaveChangesAsync();
                return tariff;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزودن تعرفه بیمه جدید");
                throw new InvalidOperationException("خطا در افزودن تعرفه بیمه", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی تعرفه بیمه
        /// </summary>
        public async Task<InsuranceTariff> UpdateAsync(InsuranceTariff tariff)
        {
            try
            {
                _context.Entry(tariff).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return tariff;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تعرفه بیمه. Id: {Id}", tariff.InsuranceTariffId);
                throw new InvalidOperationException($"خطا در به‌روزرسانی تعرفه بیمه {tariff.InsuranceTariffId}", ex);
            }
        }

        /// <summary>
        /// حذف نرم تعرفه بیمه
        /// </summary>
        public async Task<bool> SoftDeleteAsync(int id, string deletedByUserId)
        {
            try
            {
                var tariff = await _context.InsuranceTariffs
                    .FirstOrDefaultAsync(t => t.InsuranceTariffId == id && !t.IsDeleted);

                if (tariff == null)
                    return false;

                tariff.IsDeleted = true;
                tariff.DeletedAt = DateTime.UtcNow;
                tariff.DeletedByUserId = deletedByUserId;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نرم تعرفه بیمه. Id: {Id}", id);
                throw new InvalidOperationException($"خطا در حذف تعرفه بیمه {id}", ex);
            }
        }

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// دریافت تعرفه بیمه بر اساس طرح و خدمت
        /// </summary>
        public async Task<InsuranceTariff> GetByPlanAndServiceAsync(int planId, int serviceId)
        {
            try
            {
                _logger.Debug("🏥 MEDICAL: شروع GetByPlanAndServiceAsync - PlanId: {PlanId}, ServiceId: {ServiceId}", planId, serviceId);

                var tariff = await _context.InsuranceTariffs
                    .AsNoTracking() // بهینه‌سازی عملکرد برای محیط درمانی
                    .Where(t => t.InsurancePlanId == planId &&
                               t.ServiceId == serviceId &&
                               !t.IsDeleted)
                    .FirstOrDefaultAsync();

                _logger.Information("🏥 MEDICAL: GetByPlanAndServiceAsync تکمیل شد - Found: {Found}, PlanId: {PlanId}, ServiceId: {ServiceId}", 
                    tariff != null, planId, serviceId);

                return tariff;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت تعرفه بر اساس طرح بیمه و خدمت - PlanId: {PlanId}, ServiceId: {ServiceId}", 
                    planId, serviceId);
                throw new InvalidOperationException("خطا در دریافت تعرفه بر اساس طرح بیمه و خدمت", ex);
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های بیمه بر اساس طرح بیمه
        /// </summary>
        public async Task<List<InsuranceTariff>> GetByPlanIdAsync(int planId)
        {
            try
            {
                return await _context.InsuranceTariffs
                    .Include(t => t.Service)
                    .Include(t => t.Service.ServiceCategory)
                    .Where(t => t.InsurancePlanId == planId && !t.IsDeleted)
                    .OrderBy(t => t.Service.Title)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌های بیمه. PlanId: {PlanId}", planId);
                throw new InvalidOperationException($"خطا در دریافت تعرفه‌های بیمه {planId}", ex);
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های بیمه بر اساس خدمت
        /// </summary>
        public async Task<List<InsuranceTariff>> GetByServiceIdAsync(int serviceId)
        {
            try
            {
                return await _context.InsuranceTariffs
                    .Include(t => t.InsurancePlan)
                    .Include(t => t.InsurancePlan.InsuranceProvider)
                    .Where(t => t.ServiceId == serviceId && !t.IsDeleted)
                    .OrderBy(t => t.InsurancePlan.InsuranceProvider.Name)
                    .ThenBy(t => t.InsurancePlan.Name)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌های بیمه. ServiceId: {ServiceId}", serviceId);
                throw new InvalidOperationException($"خطا در دریافت تعرفه‌های بیمه خدمت {serviceId}", ex);
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های بیمه بر اساس ارائه‌دهنده بیمه
        /// </summary>
        public async Task<List<InsuranceTariff>> GetByProviderIdAsync(int providerId)
        {
            try
            {
                return await _context.InsuranceTariffs
                    .Include(t => t.Service)
                    .Include(t => t.InsurancePlan)
                    .Where(t => t.InsurancePlan.InsuranceProviderId == providerId && !t.IsDeleted)
                    .OrderBy(t => t.InsurancePlan.Name)
                    .ThenBy(t => t.Service.Title)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌های بیمه. ProviderId: {ProviderId}", providerId);
                throw new InvalidOperationException($"خطا در دریافت تعرفه‌های بیمه ارائه‌دهنده {providerId}", ex);
            }
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود تعرفه بیمه برای طرح و خدمت
        /// </summary>
        public async Task<bool> DoesTariffExistAsync(int planId, int serviceId, int? excludeId = null)
        {
            try
            {
                var query = _context.InsuranceTariffs
                    .Where(t => t.InsurancePlanId == planId && t.ServiceId == serviceId && !t.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(t => t.InsuranceTariffId != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود تعرفه بیمه. PlanId: {PlanId}, ServiceId: {ServiceId}", planId, serviceId);
                throw new InvalidOperationException($"خطا در بررسی وجود تعرفه بیمه {planId} برای خدمت {serviceId}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود تعرفه‌های بیمه برای طرح
        /// </summary>
        public async Task<bool> HasTariffsAsync(int planId)
        {
            try
            {
                return await _context.InsuranceTariffs
                    .AnyAsync(t => t.InsurancePlanId == planId && !t.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود تعرفه‌های بیمه. PlanId: {PlanId}", planId);
                throw new InvalidOperationException($"خطا در بررسی وجود تعرفه‌های بیمه {planId}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود تعرفه‌های بیمه برای خدمت
        /// </summary>
        public async Task<bool> HasTariffsForServiceAsync(int serviceId)
        {
            try
            {
                return await _context.InsuranceTariffs
                    .AnyAsync(t => t.ServiceId == serviceId && !t.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود تعرفه‌های بیمه. ServiceId: {ServiceId}", serviceId);
                throw new InvalidOperationException($"خطا در بررسی وجود تعرفه‌های بیمه خدمت {serviceId}", ex);
            }
        }

        #endregion

        #region Statistics Operations

        /// <summary>
        /// دریافت تعداد کل تعرفه‌های بیمه
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد کل تعرفه‌های بیمه");
                throw new InvalidOperationException("خطا در دریافت تعداد تعرفه‌های بیمه", ex);
            }
        }

        /// <summary>
        /// دریافت آمار تعرفه‌های بیمه
        /// </summary>
        public async Task<Dictionary<string, int>> GetStatisticsAsync()
        {
            try
            {
                _logger.Debug("🏥 MEDICAL: شروع محاسبه آمار تعرفه‌ها");
                
                // 🚀 P0 FIX: محاسبه آمار کامل برای محیط درمانی
                var totalTariffs = await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted)
                    .CountAsync();
                    
                _logger.Debug("🏥 MEDICAL: تعداد کل تعرفه‌ها: {TotalTariffs}", totalTariffs);
                
                // تست: بررسی وجود تعرفه‌ها بدون فیلتر IsDeleted
                var allTariffsCount = await _context.InsuranceTariffs.CountAsync();
                _logger.Debug("🏥 MEDICAL: تعداد کل تعرفه‌ها (بدون فیلتر): {AllTariffsCount}", allTariffsCount);

                var activeTariffs = await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted && t.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                    .CountAsync();

                var inactiveTariffs = await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted && t.CreatedAt < DateTime.UtcNow.AddDays(-30))
                    .CountAsync();

                var totalServices = await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted)
                    .Select(t => t.ServiceId)
                    .Distinct()
                    .CountAsync();

                var tariffsWithCustomPrice = await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted && t.TariffPrice.HasValue)
                    .CountAsync();

                var tariffsWithCustomPatientShare = await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted && t.PatientShare.HasValue)
                    .CountAsync();

                var tariffsWithCustomInsurerShare = await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted && t.InsurerShare.HasValue)
                    .CountAsync();

                return new Dictionary<string, int>
                {
                    { "TotalTariffs", totalTariffs },
                    { "ActiveTariffs", activeTariffs },
                    { "InactiveTariffs", inactiveTariffs },
                    { "TotalServices", totalServices },
                    { "TariffsWithCustomPrice", tariffsWithCustomPrice },
                    { "TariffsWithCustomPatientShare", tariffsWithCustomPatientShare },
                    { "TariffsWithCustomInsurerShare", tariffsWithCustomInsurerShare }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار تعرفه‌های بیمه");
                throw new InvalidOperationException("خطا در دریافت آمار تعرفه‌های بیمه", ex);
            }
        }

        #endregion

        #region Additional Methods for Service Compatibility

        /// <summary>
        /// حذف تعرفه بیمه
        /// </summary>
        public void Delete(InsuranceTariff tariff)
        {
            try
            {
                _context.InsuranceTariffs.Remove(tariff);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تعرفه بیمه. Id: {Id}", tariff?.InsuranceTariffId);
                throw new InvalidOperationException("خطا در حذف تعرفه بیمه", ex);
            }
        }

        /// <summary>
        /// ذخیره تغییرات
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره تغییرات تعرفه‌های بیمه");
                throw new InvalidOperationException("خطا در ذخیره تغییرات", ex);
            }
        }

        #endregion

        #region Optimized Query Methods

        /// <summary>
        /// دریافت تعرفه‌های بیمه اصلی برای خدمت و طرح
        /// </summary>
        public async Task<List<InsuranceTariff>> GetPrimaryTariffsAsync(int serviceId, int planId)
        {
            try
            {
                _logger.Information("درخواست تعرفه‌های بیمه اصلی. ServiceId: {ServiceId}, PlanId: {PlanId}", serviceId, planId);

                var tariffs = await _context.InsuranceTariffs
                    .AsNoTracking()
                    .Where(t => t.ServiceId == serviceId &&
                                t.InsurancePlanId == planId &&
                                t.InsuranceType == InsuranceType.Primary &&
                                !t.IsDeleted && t.IsActive)
                    .OrderBy(t => t.Priority ?? 0)
                    .ToListAsync();

                _logger.Information("تعرفه‌های بیمه اصلی دریافت شد. ServiceId: {ServiceId}, PlanId: {PlanId}, Count: {Count}", 
                    serviceId, planId, tariffs.Count);

                return tariffs;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌های بیمه اصلی. ServiceId: {ServiceId}, PlanId: {PlanId}", serviceId, planId);
                throw new InvalidOperationException($"خطا در دریافت تعرفه‌های بیمه اصلی", ex);
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های بیمه تکمیلی برای خدمت
        /// </summary>
        public async Task<List<InsuranceTariff>> GetSupplementaryTariffsAsync(int serviceId)
        {
            try
            {
                _logger.Information("درخواست تعرفه‌های بیمه تکمیلی. ServiceId: {ServiceId}", serviceId);

                var tariffs = await _context.InsuranceTariffs
                    .AsNoTracking()
                    .Where(t => t.ServiceId == serviceId &&
                                t.InsuranceType == InsuranceType.Supplementary &&
                                !t.IsDeleted && t.IsActive)
                    .OrderBy(t => t.Priority ?? 0)
                    .ToListAsync();

                _logger.Information("تعرفه‌های بیمه تکمیلی دریافت شد. ServiceId: {ServiceId}, Count: {Count}", 
                    serviceId, tariffs.Count);

                return tariffs;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌های بیمه تکمیلی. ServiceId: {ServiceId}", serviceId);
                throw new InvalidOperationException($"خطا در دریافت تعرفه‌های بیمه تکمیلی", ex);
            }
        }

        /// <summary>
        /// دریافت تعرفه بیمه بر اساس نوع
        /// </summary>
        public async Task<InsuranceTariff> GetTariffByTypeAsync(int serviceId, int planId, InsuranceType insuranceType)
        {
            try
            {
                _logger.Information("درخواست تعرفه بیمه بر اساس نوع. ServiceId: {ServiceId}, PlanId: {PlanId}, Type: {Type}", 
                    serviceId, planId, insuranceType);

                var tariff = await _context.InsuranceTariffs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.ServiceId == serviceId &&
                                            t.InsurancePlanId == planId &&
                                            t.InsuranceType == insuranceType &&
                                            !t.IsDeleted && t.IsActive);

                _logger.Information("تعرفه بیمه بر اساس نوع دریافت شد. ServiceId: {ServiceId}, PlanId: {PlanId}, Type: {Type}, Found: {Found}", 
                    serviceId, planId, insuranceType, tariff != null);

                return tariff;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه بیمه بر اساس نوع. ServiceId: {ServiceId}, PlanId: {PlanId}, Type: {Type}", 
                    serviceId, planId, insuranceType);
                throw new InvalidOperationException($"خطا در دریافت تعرفه بیمه بر اساس نوع", ex);
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های بیمه تکمیلی با فیلترهای بهینه‌سازی شده
        /// </summary>
        public async Task<List<InsuranceTariff>> GetFilteredSupplementaryTariffsAsync(
            string searchTerm = "", 
            int? departmentId = null, 
            bool? isActive = null)
        {
            try
            {
                _logger.Information("🔍 REPOSITORY: شروع GetFilteredSupplementaryTariffsAsync - SearchTerm: {SearchTerm}, DeptId: {DeptId}, IsActive: {IsActive}", 
                    searchTerm, departmentId, isActive);

                var query = _context.InsuranceTariffs
                    .AsNoTracking()
                    .Include(t => t.Service)
                    .Include(t => t.InsurancePlan)
                    .Include(t => t.InsurancePlan.InsuranceProvider)
                    .Where(t => !t.IsDeleted && t.InsuranceType == InsuranceType.Supplementary);

                // فیلتر بر اساس جستجو
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(t => 
                        t.Service.Title.Contains(searchTerm) ||
                        t.Service.ServiceCode.Contains(searchTerm) ||
                        t.InsurancePlan.Name.Contains(searchTerm) ||
                        t.InsurancePlan.InsuranceProvider.Name.Contains(searchTerm));
                }

                // فیلتر بر اساس دپارتمان (فعلاً غیرفعال - نیاز به بررسی ساختار Service)
                // if (departmentId.HasValue)
                // {
                //     query = query.Where(t => t.Service.DepartmentId == departmentId.Value);
                // }

                // فیلتر بر اساس وضعیت فعال
                if (isActive.HasValue)
                {
                    query = query.Where(t => t.IsActive == isActive.Value);
                }

                var result = await query
                    .OrderBy(t => t.InsurancePlan.InsuranceProvider.Name)
                    .ThenBy(t => t.InsurancePlan.Name)
                    .ThenBy(t => t.Service.Title)
                    .ToListAsync();

                _logger.Information("🔍 REPOSITORY: نتایج GetFilteredSupplementaryTariffsAsync - Count: {Count}", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌های بیمه تکمیلی با فیلترها");
                throw new InvalidOperationException("خطا در دریافت تعرفه‌های بیمه تکمیلی با فیلترها", ex);
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های فعال بیمه برای خدمت
        /// </summary>
        public async Task<List<InsuranceTariff>> GetActiveTariffsForServiceAsync(int serviceId, System.DateTime? calculationDate = null)
        {
            try
            {
                var effectiveDate = calculationDate ?? DateTime.Now;
                
                _logger.Information("درخواست تعرفه‌های فعال بیمه. ServiceId: {ServiceId}, Date: {Date}", serviceId, effectiveDate);

                var tariffs = await _context.InsuranceTariffs
                    .AsNoTracking()
                    .Where(t => t.ServiceId == serviceId &&
                                !t.IsDeleted && t.IsActive &&
                                (t.StartDate == null || t.StartDate <= effectiveDate) &&
                                (t.EndDate == null || t.EndDate >= effectiveDate))
                    .OrderBy(t => t.InsuranceType)
                    .ThenBy(t => t.Priority ?? 0)
                    .ToListAsync();

                _logger.Information("تعرفه‌های فعال بیمه دریافت شد. ServiceId: {ServiceId}, Count: {Count}", 
                    serviceId, tariffs.Count);

                return tariffs;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌های فعال بیمه. ServiceId: {ServiceId}, Date: {Date}", 
                    serviceId, calculationDate);
                throw new InvalidOperationException($"خطا در دریافت تعرفه‌های فعال بیمه", ex);
            }
        }

        #endregion

        #region Optimized Projection Methods

        /// <summary>
        /// دریافت تعرفه‌ها با Projection - بهینه‌سازی شده برای performance
        /// </summary>
        public async Task<PagedResult<TariffIndexDto>> GetTariffsProjectionAsync(
            int? planId = null,
            int? serviceId = null,
            int? providerId = null,
            string searchTerm = "",
            InsuranceType? insuranceType = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                _logger.Information("🔍 REPOSITORY: شروع GetTariffsProjectionAsync - PlanId: {PlanId}, ServiceId: {ServiceId}, ProviderId: {ProviderId}, SearchTerm: {SearchTerm}, InsuranceType: {InsuranceType}", 
                    planId, serviceId, providerId, searchTerm, insuranceType);

                // بهینه‌سازی: Projection + AsNoTracking برای read-only operations
                var query = _context.InsuranceTariffs
                    .AsNoTracking()
                    .Where(t => !t.IsDeleted);

                // فیلتر بر اساس طرح بیمه
                if (planId.HasValue)
                {
                    query = query.Where(t => t.InsurancePlanId == planId.Value);
                }

                // فیلتر بر اساس خدمت
                if (serviceId.HasValue)
                {
                    query = query.Where(t => t.ServiceId == serviceId.Value);
                }

                // فیلتر بر اساس ارائه‌دهنده بیمه
                if (providerId.HasValue)
                {
                    query = query.Where(t => t.InsurancePlan.InsuranceProviderId == providerId.Value);
                }

                // فیلتر بر اساس نوع بیمه
                if (insuranceType.HasValue)
                {
                    query = query.Where(t => t.InsuranceType == insuranceType.Value);
                    _logger.Information("🔍 REPOSITORY: فیلتر InsuranceType اضافه شد: {InsuranceType}", insuranceType.Value);
                }

                // جستجو در نام خدمت و طرح بیمه
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(t => 
                        t.Service.Title.Contains(searchTerm) ||
                        t.InsurancePlan.Name.Contains(searchTerm) ||
                        t.InsurancePlan.InsuranceProvider.Name.Contains(searchTerm));
                }

                // شمارش کل
                var totalCount = await query.CountAsync();

                // Projection به DTO
                var items = await query
                    .Select(t => new TariffIndexDto
                    {
                        Id = t.InsuranceTariffId,
                        ServiceId = t.ServiceId,
                        ServiceName = t.Service.Title,
                        InsurancePlanId = t.InsurancePlanId ?? 0,
                        InsurancePlanName = t.InsurancePlan.Name,
                        InsuranceProviderId = t.InsurancePlan.InsuranceProviderId,
                        InsuranceProviderName = t.InsurancePlan.InsuranceProvider.Name,
                        TariffPrice = t.TariffPrice ?? 0,
                        PatientShare = t.PatientShare ?? 0,
                        InsurerShare = t.InsurerShare ?? 0,
                        IsActive = t.IsActive,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .OrderBy(t => t.InsuranceProviderName)
                    .ThenBy(t => t.InsurancePlanName)
                    .ThenBy(t => t.ServiceName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.Information("🔍 REPOSITORY: GetTariffsProjectionAsync تکمیل شد - TotalCount: {TotalCount}, ItemsCount: {ItemsCount}", 
                    totalCount, items.Count);

                return new PagedResult<TariffIndexDto>
                {
                    Items = items,
                    TotalItems = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌ها با Projection");
                throw new InvalidOperationException("خطا در دریافت تعرفه‌ها با Projection", ex);
            }
        }

        /// <summary>
        /// دریافت آمار تعرفه‌ها با Projection - بهینه‌سازی شده
        /// </summary>
        public async Task<TariffStatisticsDto> GetStatisticsProjectionAsync()
        {
            try
            {
                _logger.Information("🔍 REPOSITORY: شروع GetStatisticsProjectionAsync");

                // بهینه‌سازی: Projection + AsNoTracking برای آمار
                var statistics = await _context.InsuranceTariffs
                    .AsNoTracking()
                    .Where(t => !t.IsDeleted)
                    .GroupBy(t => 1)
                    .Select(g => new TariffStatisticsDto
                    {
                        TotalTariffs = g.Count(),
                        ActiveTariffs = g.Count(t => t.IsActive),
                        InactiveTariffs = g.Count(t => !t.IsActive),
                        AverageTariffPrice = g.Average(t => t.TariffPrice ?? 0),
                        TotalTariffValue = g.Sum(t => t.TariffPrice ?? 0),
                        PlansWithTariffs = g.Select(t => t.InsurancePlanId).Distinct().Count(),
                        ServicesWithTariffs = g.Select(t => t.ServiceId).Distinct().Count()
                    })
                    .FirstOrDefaultAsync();

                _logger.Information("🔍 REPOSITORY: GetStatisticsProjectionAsync تکمیل شد - TotalTariffs: {TotalTariffs}", 
                    statistics?.TotalTariffs ?? 0);

                return statistics ?? new TariffStatisticsDto();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار تعرفه‌ها با Projection");
                throw new InvalidOperationException("خطا در دریافت آمار تعرفه‌ها با Projection", ex);
            }
        }

        /// <summary>
        /// دریافت جزئیات تعرفه با Projection - بهینه‌سازی شده
        /// </summary>
        public async Task<TariffDetailsDto> GetTariffDetailsProjectionAsync(int id)
        {
            try
            {
                _logger.Information("🔍 REPOSITORY: شروع GetTariffDetailsProjectionAsync - Id: {Id}", id);

                // بهینه‌سازی: Projection + AsNoTracking برای جزئیات
                var tariff = await _context.InsuranceTariffs
                    .AsNoTracking()
                    .Where(t => t.InsuranceTariffId == id && !t.IsDeleted)
                    .Select(t => new TariffDetailsDto
                    {
                        Id = t.InsuranceTariffId,
                        ServiceId = t.ServiceId,
                        ServiceName = t.Service.Title,
                        ServiceCode = t.Service.ServiceCode ?? "",
                        InsurancePlanId = t.InsurancePlanId ?? 0,
                        InsurancePlanName = t.InsurancePlan.Name,
                        InsuranceProviderId = t.InsurancePlan.InsuranceProviderId,
                        InsuranceProviderName = t.InsurancePlan.InsuranceProvider.Name,
                        TariffPrice = t.TariffPrice ?? 0,
                        PatientShare = t.PatientShare ?? 0,
                        InsurerShare = t.InsurerShare ?? 0,
                        IsActive = t.IsActive,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        CreatedBy = "",
                        UpdatedBy = ""
                    })
                    .FirstOrDefaultAsync();

                _logger.Information("🔍 REPOSITORY: GetTariffDetailsProjectionAsync تکمیل شد - Found: {Found}", tariff != null);

                return tariff;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات تعرفه با Projection - Id: {Id}", id);
                throw new InvalidOperationException($"خطا در دریافت جزئیات تعرفه {id} با Projection", ex);
            }
        }


        #endregion
    }
}
