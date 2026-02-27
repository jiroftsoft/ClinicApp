using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Insurance;
using Serilog;

namespace ClinicApp.Repositories.Insurance
{
    public class InsuranceBatchRepository : IInsuranceBatchRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public InsuranceBatchRepository(ApplicationDbContext context, ILogger logger, ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<InsuranceBatch> GetByIdAsync(int id)
        {
            return await _context.InsuranceBatches
                .Include(b => b.InsuranceProvider)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        }

        public async Task<InsuranceBatch> GetByBatchNumberAsync(string batchNumber)
        {
            if (string.IsNullOrWhiteSpace(batchNumber)) return null;
            return await _context.InsuranceBatches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => !b.IsDeleted && b.BatchNumber == batchNumber);
        }

        public async Task<List<InsuranceBatch>> GetByProviderIdAsync(int providerId, int pageSize = 50)
        {
            return await _context.InsuranceBatches
                .Where(b => !b.IsDeleted && b.InsuranceProviderId == providerId)
                .OrderByDescending(b => b.SubmissionDate)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<InsuranceBatch> AddAsync(InsuranceBatch entity)
        {
            entity.CreatedAt = DateTime.Now;
            entity.CreatedByUserId = _currentUserService?.GetCurrentUserId();
            _context.InsuranceBatches.Add(entity);
            await _context.SaveChangesAsync();
            _logger.Information("دسته مطالبه بیمه با شناسه {BatchId} افزوده شد", entity.Id);
            return entity;
        }

        public async Task<InsuranceBatch> UpdateAsync(InsuranceBatch entity)
        {
            entity.UpdatedAt = DateTime.Now;
            entity.UpdatedByUserId = _currentUserService?.GetCurrentUserId();
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _logger.Information("دسته مطالبه بیمه با شناسه {BatchId} به‌روزرسانی شد", entity.Id);
            return entity;
        }

        public async Task<bool> SoftDeleteAsync(int id, string deletedByUserId)
        {
            var batch = await _context.InsuranceBatches.FindAsync(id);
            if (batch == null) return false;
            batch.IsDeleted = true;
            batch.DeletedAt = DateTime.Now;
            batch.DeletedByUserId = deletedByUserId;
            await _context.SaveChangesAsync();
            _logger.Information("دسته مطالبه بیمه با شناسه {BatchId} حذف نرم شد", id);
            return true;
        }
    }
}
