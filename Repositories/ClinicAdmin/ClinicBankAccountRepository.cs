using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Clinic;
using Serilog;

namespace ClinicApp.Repositories.ClinicAdmin
{
    /// <summary>
    /// Repository Implementation برای مدیریت حساب بانکی کلینیک
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت حساب بانکی هر کلینیک (شماره شبا)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete)
    /// 3. بهینه‌سازی عملکرد با AsNoTracking
    /// 4. مدیریت رابطه One-to-One با Clinic
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط دسترسی به داده‌های ClinicBankAccount
    /// ✅ Separation of Concerns: منطق کسب‌وکار در Service Layer
    /// ✅ Performance Optimization: استفاده از AsNoTracking برای خواندن
    /// ✅ Clean Architecture: Repository layer فقط دسترسی به داده
    /// </summary>
    public class ClinicBankAccountRepository : IClinicBankAccountRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public ClinicBankAccountRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// دریافت حساب بانکی بر اساس شناسه
        /// </summary>
        public async Task<ClinicBankAccount> GetByIdAsync(int clinicBankAccountId)
        {
            _logger.Debug("🏥 MEDICAL: دریافت حساب بانکی با شناسه: {ClinicBankAccountId}", clinicBankAccountId);

            // عدم استفاده از AsNoTracking چون ممکن است Entity را Update کنیم
            return await _context.ClinicBankAccounts
                .Include(c => c.Clinic)
                .Include(c => c.CreatedByUser)
                .Include(c => c.UpdatedByUser)
                .Where(c => c.ClinicBankAccountId == clinicBankAccountId && !c.IsDeleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// دریافت حساب بانکی بر اساس شناسه کلینیک
        /// </summary>
        public async Task<ClinicBankAccount> GetByClinicIdAsync(int clinicId)
        {
            _logger.Debug("🏥 MEDICAL: دریافت حساب بانکی برای کلینیک: {ClinicId}", clinicId);

            return await _context.ClinicBankAccounts
                .AsNoTracking()
                .Include(c => c.Clinic)
                .Where(c => c.ClinicId == clinicId && !c.IsDeleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// دریافت تمام حساب‌های بانکی فعال
        /// </summary>
        public async Task<List<ClinicBankAccount>> GetAllAsync()
        {
            _logger.Debug("🏥 MEDICAL: دریافت تمام حساب‌های بانکی فعال");

            return await _context.ClinicBankAccounts
                .AsNoTracking()
                .Include(c => c.Clinic)
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.Clinic.Name)
                .ToListAsync();
        }

        /// <summary>
        /// بررسی وجود حساب بانکی برای کلینیک
        /// </summary>
        public async Task<bool> ExistsForClinicAsync(int clinicId)
        {
            _logger.Debug("🏥 MEDICAL: بررسی وجود حساب بانکی برای کلینیک: {ClinicId}", clinicId);

            return await _context.ClinicBankAccounts
                .AsNoTracking()
                .AnyAsync(c => c.ClinicId == clinicId && !c.IsDeleted);
        }

        /// <summary>
        /// بررسی وجود شماره شبا (برای جلوگیری از تکراری)
        /// </summary>
        public async Task<bool> IbanNumberExistsAsync(string ibanNumber, int? excludeId = null)
        {
            _logger.Debug("🏥 MEDICAL: بررسی وجود شماره شبا: {IbanNumber}, ExcludeId: {ExcludeId}", ibanNumber, excludeId);

            var query = _context.ClinicBankAccounts
                .AsNoTracking()
                .Where(c => c.IbanNumber == ibanNumber && !c.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.ClinicBankAccountId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// افزودن حساب بانکی جدید
        /// </summary>
        public void Add(ClinicBankAccount clinicBankAccount)
        {
            _logger.Information("🏥 MEDICAL: افزودن حساب بانکی جدید برای کلینیک: {ClinicId}", clinicBankAccount?.ClinicId);

            _context.ClinicBankAccounts.Add(clinicBankAccount);
        }

        /// <summary>
        /// به‌روزرسانی حساب بانکی
        /// </summary>
        public void Update(ClinicBankAccount clinicBankAccount)
        {
            _logger.Information("🏥 MEDICAL: به‌روزرسانی حساب بانکی: {ClinicBankAccountId}", clinicBankAccount?.ClinicBankAccountId);

            _context.Entry(clinicBankAccount).State = EntityState.Modified;
        }

        /// <summary>
        /// حذف حساب بانکی (Soft Delete)
        /// </summary>
        public void Delete(ClinicBankAccount clinicBankAccount)
        {
            _logger.Information("🏥 MEDICAL: حذف حساب بانکی: {ClinicBankAccountId}", clinicBankAccount?.ClinicBankAccountId);

            _context.ClinicBankAccounts.Remove(clinicBankAccount);
        }

        /// <summary>
        /// ذخیره تغییرات در پایگاه داده
        /// </summary>
        public async Task SaveChangesAsync()
        {
            _logger.Debug("🏥 MEDICAL: ذخیره تغییرات در پایگاه داده");

            await _context.SaveChangesAsync();
        }
    }
}

