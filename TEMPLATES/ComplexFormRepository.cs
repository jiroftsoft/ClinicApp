using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.[Module];

namespace ClinicApp.Repositories
{
    /// <summary>
    /// Interface برای Repository [Module]
    /// </summary>
    public interface I[Module]Repository
    {
        Task<List<[Module]>> GetAllAsync();
        Task<[Module]> GetByIdAsync(int id);
        Task<[Module]> GetByCodeAsync(string code);
        Task<[Module]> AddAsync([Module] entity);
        void Update([Module] entity);
        void Delete([Module] entity);
        Task<int> SaveChangesAsync();
        Task<List<[Module]>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
        Task<bool> IsInUseAsync(int id);
        Task<List<[Module]>> GetActiveAsync();
        Task<List<[Module]>> GetByInsuranceProviderAsync(int insuranceProviderId);
        Task<int> GetTotalCountAsync();
        Task<bool> ExistsAsync(int id);
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
    }

    /// <summary>
    /// Repository [Module] - استاندارد کامل برای فرم‌های پیچیده
    /// </summary>
    public class [Module]Repository : I[Module]Repository
    {
        #region Fields - فیلدها

        private readonly ApplicationDbContext _context;
        private readonly DbSet<[Module]> _dbSet;

        #endregion

        #region Constructor - سازنده

        public [Module]Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<[Module]>();
        }

        #endregion

        #region CRUD Operations - عملیات CRUD

        /// <summary>
        /// دریافت تمام [Module]ها
        /// </summary>
        public async Task<List<[Module]>> GetAllAsync()
        {
            return await _dbSet
                .Where(x => !x.IsDeleted)
                .Include(x => x.InsuranceProvider)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت [Module] بر اساس شناسه
        /// </summary>
        public async Task<[Module]> GetByIdAsync(int id)
        {
            return await _dbSet
                .Where(x => x.Id == id && !x.IsDeleted)
                .Include(x => x.InsuranceProvider)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// دریافت [Module] بر اساس کد
        /// </summary>
        public async Task<[Module]> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            return await _dbSet
                .Where(x => x.Code == code && !x.IsDeleted)
                .Include(x => x.InsuranceProvider)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// افزودن [Module] جدید
        /// </summary>
        public async Task<[Module]> AddAsync([Module] entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// به‌روزرسانی [Module]
        /// </summary>
        public void Update([Module] entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// حذف [Module]
        /// </summary>
        public void Delete([Module] entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
        }

        /// <summary>
        /// ذخیره تغییرات
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        #endregion

        #region Search Operations - عملیات جستجو

        /// <summary>
        /// جستجوی [Module]
        /// </summary>
        public async Task<List<[Module]>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            var query = _dbSet
                .Where(x => !x.IsDeleted)
                .Include(x => x.InsuranceProvider);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x => 
                    x.Name.Contains(searchTerm) ||
                    x.Code.Contains(searchTerm) ||
                    x.Description.Contains(searchTerm) ||
                    x.InsuranceProvider.Name.Contains(searchTerm));
            }

            return await query
                .OrderBy(x => x.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        #endregion

        #region Business Operations - عملیات کسب‌وکار

        /// <summary>
        /// بررسی استفاده از [Module]
        /// </summary>
        public async Task<bool> IsInUseAsync(int id)
        {
            // Check if [Module] is used in any related entities
            // This is a placeholder - implement based on your business logic
            
            // Example: Check if used in PatientInsurance
            var isUsedInPatientInsurance = await _context.PatientInsurances
                .AnyAsync(x => x.InsurancePlanId == id && !x.IsDeleted);

            // Example: Check if used in any other related entities
            // var isUsedInOtherEntity = await _context.OtherEntity
            //     .AnyAsync(x => x.[Module]Id == id && !x.IsDeleted);

            return isUsedInPatientInsurance; // || isUsedInOtherEntity;
        }

        /// <summary>
        /// دریافت [Module]های فعال
        /// </summary>
        public async Task<List<[Module]>> GetActiveAsync()
        {
            return await _dbSet
                .Where(x => !x.IsDeleted && x.IsActive)
                .Include(x => x.InsuranceProvider)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت [Module]های ارائه‌دهنده بیمه
        /// </summary>
        public async Task<List<[Module]>> GetByInsuranceProviderAsync(int insuranceProviderId)
        {
            return await _dbSet
                .Where(x => !x.IsDeleted && x.InsuranceProviderId == insuranceProviderId)
                .Include(x => x.InsuranceProvider)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        #endregion

        #region Count Operations - عملیات شمارش

        /// <summary>
        /// دریافت تعداد کل [Module]ها
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _dbSet
                .Where(x => !x.IsDeleted)
                .CountAsync();
        }

        /// <summary>
        /// بررسی وجود [Module]
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet
                .Where(x => x.Id == id && !x.IsDeleted)
                .AnyAsync();
        }

        /// <summary>
        /// بررسی وجود کد [Module]
        /// </summary>
        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            var query = _dbSet
                .Where(x => x.Code == code && !x.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        #endregion

        #region Advanced Queries - کوئری‌های پیشرفته

        /// <summary>
        /// دریافت [Module]های معتبر در تاریخ مشخص
        /// </summary>
        public async Task<List<[Module]>> GetValidOnDateAsync(DateTime date)
        {
            return await _dbSet
                .Where(x => !x.IsDeleted && 
                           x.IsActive && 
                           x.ValidFrom <= date && 
                           (!x.ValidTo.HasValue || x.ValidTo >= date))
                .Include(x => x.InsuranceProvider)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت [Module]های منقضی شده
        /// </summary>
        public async Task<List<[Module]>> GetExpiredAsync()
        {
            var today = DateTime.Today;
            
            return await _dbSet
                .Where(x => !x.IsDeleted && 
                           x.ValidTo.HasValue && 
                           x.ValidTo < today)
                .Include(x => x.InsuranceProvider)
                .OrderBy(x => x.ValidTo)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت [Module]های در حال انقضا (30 روز آینده)
        /// </summary>
        public async Task<List<[Module]>> GetExpiringSoonAsync(int days = 30)
        {
            var today = DateTime.Today;
            var futureDate = today.AddDays(days);
            
            return await _dbSet
                .Where(x => !x.IsDeleted && 
                           x.ValidTo.HasValue && 
                           x.ValidTo >= today && 
                           x.ValidTo <= futureDate)
                .Include(x => x.InsuranceProvider)
                .OrderBy(x => x.ValidTo)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت آمار [Module]ها
        /// </summary>
        public async Task<[Module]Statistics> GetStatisticsAsync()
        {
            var total = await _dbSet.Where(x => !x.IsDeleted).CountAsync();
            var active = await _dbSet.Where(x => !x.IsDeleted && x.IsActive).CountAsync();
            var inactive = total - active;
            var expired = await _dbSet.Where(x => !x.IsDeleted && x.ValidTo.HasValue && x.ValidTo < DateTime.Today).CountAsync();
            var expiringSoon = await _dbSet.Where(x => !x.IsDeleted && x.ValidTo.HasValue && x.ValidTo >= DateTime.Today && x.ValidTo <= DateTime.Today.AddDays(30)).CountAsync();

            return new [Module]Statistics
            {
                Total = total,
                Active = active,
                Inactive = inactive,
                Expired = expired,
                ExpiringSoon = expiringSoon
            };
        }

        #endregion

        #region Bulk Operations - عملیات دسته‌ای

        /// <summary>
        /// حذف دسته‌ای [Module]ها
        /// </summary>
        public async Task<int> BulkDeleteAsync(List<int> ids)
        {
            var entities = await _dbSet
                .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync();

            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.Now;
                // entity.DeletedBy = currentUserId; // Set from service
            }

            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// تغییر وضعیت دسته‌ای [Module]ها
        /// </summary>
        public async Task<int> BulkToggleStatusAsync(List<int> ids, bool isActive)
        {
            var entities = await _dbSet
                .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync();

            foreach (var entity in entities)
            {
                entity.IsActive = isActive;
                entity.UpdatedAt = DateTime.Now;
                // entity.UpdatedBy = currentUserId; // Set from service
            }

            return await _context.SaveChangesAsync();
        }

        #endregion
    }

    /// <summary>
    /// کلاس آمار [Module]
    /// </summary>
    public class [Module]Statistics
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
        public int Expired { get; set; }
        public int ExpiringSoon { get; set; }
    }
}
