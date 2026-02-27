using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models;
using ClinicApp.Models.DTOs.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Repositories.Insurance
{
    public class InsuranceClaimRepository : IInsuranceClaimRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public InsuranceClaimRepository(ApplicationDbContext context, ILogger logger, ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<InsuranceClaim> GetByIdAsync(int id)
        {
            return await _context.InsuranceClaims
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<InsuranceClaim> GetByIdWithDetailsAsync(int id)
        {
            return await _context.InsuranceClaims
                .Include(c => c.Patient)
                .Include(c => c.InsurancePlan)
                .Include(c => c.InsurancePlan.InsuranceProvider)
                .Include(c => c.Batch)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<List<InsuranceClaim>> GetByDateRangeAsync(DateTime start, DateTime end, int? insuranceProviderId = null, ClaimStatus? status = null)
        {
            var query = _context.InsuranceClaims
                .Where(c => !c.IsDeleted && c.SubmissionDate >= start && c.SubmissionDate <= end);

            if (insuranceProviderId.HasValue)
                query = query.Where(c => c.InsurancePlan.InsuranceProviderId == insuranceProviderId.Value);
            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            return await query
                .OrderByDescending(c => c.SubmissionDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<InsuranceClaim>> GetByPlanIdAsync(int planId)
        {
            return await _context.InsuranceClaims
                .Where(c => !c.IsDeleted && c.InsurancePlanId == planId)
                .OrderByDescending(c => c.SubmissionDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<InsuranceClaim>> GetByBatchIdAsync(int batchId)
        {
            return await _context.InsuranceClaims
                .Where(c => !c.IsDeleted && c.BatchId == batchId)
                .OrderBy(c => c.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<InsuranceClaim> AddAsync(InsuranceClaim entity)
        {
            entity.CreatedAt = DateTime.Now;
            entity.CreatedByUserId = _currentUserService?.GetCurrentUserId();
            _context.InsuranceClaims.Add(entity);
            await _context.SaveChangesAsync();
            _logger.Information("مطالبه بیمه با شناسه {ClaimId} افزوده شد", entity.Id);
            return entity;
        }

        public async Task<InsuranceClaim> UpdateAsync(InsuranceClaim entity)
        {
            entity.UpdatedAt = DateTime.Now;
            entity.UpdatedByUserId = _currentUserService?.GetCurrentUserId();
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _logger.Information("مطالبه بیمه با شناسه {ClaimId} به‌روزرسانی شد", entity.Id);
            return entity;
        }

        public async Task<bool> SoftDeleteAsync(int id, string deletedByUserId)
        {
            var claim = await _context.InsuranceClaims.FindAsync(id);
            if (claim == null) return false;
            claim.IsDeleted = true;
            claim.DeletedAt = DateTime.Now;
            claim.DeletedByUserId = deletedByUserId;
            await _context.SaveChangesAsync();
            _logger.Information("مطالبه بیمه با شناسه {ClaimId} حذف نرم شد", id);
            return true;
        }

        public async Task<List<InsuranceClaimAgingRow>> GetAgingReportAsync(DateTime? asOfDate = null)
        {
            var asOf = (asOfDate ?? DateTime.Now).Date;
            var pendingStatuses = new[] { ClaimStatus.Pending, ClaimStatus.Approved, ClaimStatus.PartiallyPaid };

            // aggregation در DB با DbFunctions.DiffDays (SQL-friendly، بدون بارگذاری کل لیست در حافظه)
            var baseQuery = _context.InsuranceClaims
                .Where(c => !c.IsDeleted && pendingStatuses.Contains(c.Status));

            var bucket0_30 = await baseQuery
                .Where(c => DbFunctions.DiffDays(c.SubmissionDate, asOf) >= 0 && DbFunctions.DiffDays(c.SubmissionDate, asOf) <= 30)
                .GroupBy(c => 1)
                .Select(g => new { TotalClaimed = g.Sum(x => x.ClaimedAmount), TotalApproved = g.Sum(x => x.ApprovedAmount), ClaimCount = g.Count() })
                .FirstOrDefaultAsync();

            var bucket31_60 = await baseQuery
                .Where(c => DbFunctions.DiffDays(c.SubmissionDate, asOf) > 30 && DbFunctions.DiffDays(c.SubmissionDate, asOf) <= 60)
                .GroupBy(c => 1)
                .Select(g => new { TotalClaimed = g.Sum(x => x.ClaimedAmount), TotalApproved = g.Sum(x => x.ApprovedAmount), ClaimCount = g.Count() })
                .FirstOrDefaultAsync();

            var bucket61_90 = await baseQuery
                .Where(c => DbFunctions.DiffDays(c.SubmissionDate, asOf) > 60 && DbFunctions.DiffDays(c.SubmissionDate, asOf) <= 90)
                .GroupBy(c => 1)
                .Select(g => new { TotalClaimed = g.Sum(x => x.ClaimedAmount), TotalApproved = g.Sum(x => x.ApprovedAmount), ClaimCount = g.Count() })
                .FirstOrDefaultAsync();

            var bucket90Plus = await baseQuery
                .Where(c => DbFunctions.DiffDays(c.SubmissionDate, asOf) > 90)
                .GroupBy(c => 1)
                .Select(g => new { TotalClaimed = g.Sum(x => x.ClaimedAmount), TotalApproved = g.Sum(x => x.ApprovedAmount), ClaimCount = g.Count() })
                .FirstOrDefaultAsync();

            return new List<InsuranceClaimAgingRow>
            {
                new InsuranceClaimAgingRow { AgeGroup = "0-30 روز", TotalClaimed = bucket0_30?.TotalClaimed ?? 0, TotalApproved = bucket0_30?.TotalApproved ?? 0, ClaimCount = bucket0_30?.ClaimCount ?? 0 },
                new InsuranceClaimAgingRow { AgeGroup = "31-60 روز", TotalClaimed = bucket31_60?.TotalClaimed ?? 0, TotalApproved = bucket31_60?.TotalApproved ?? 0, ClaimCount = bucket31_60?.ClaimCount ?? 0 },
                new InsuranceClaimAgingRow { AgeGroup = "61-90 روز", TotalClaimed = bucket61_90?.TotalClaimed ?? 0, TotalApproved = bucket61_90?.TotalApproved ?? 0, ClaimCount = bucket61_90?.ClaimCount ?? 0 },
                new InsuranceClaimAgingRow { AgeGroup = "بیش از 90 روز", TotalClaimed = bucket90Plus?.TotalClaimed ?? 0, TotalApproved = bucket90Plus?.TotalApproved ?? 0, ClaimCount = bucket90Plus?.ClaimCount ?? 0 }
            };
        }

        public async Task<List<InsuranceProviderBreakdownRow>> GetProviderBreakdownAsync(DateTime start, DateTime end)
        {
            var query = _context.InsuranceClaims
                .Where(c => !c.IsDeleted && c.SubmissionDate >= start && c.SubmissionDate <= end)
                .GroupBy(c => new { c.InsurancePlan.InsuranceProviderId, c.InsurancePlan.InsuranceProvider.Name })
                .Select(g => new InsuranceProviderBreakdownRow
                {
                    InsuranceProviderId = g.Key.InsuranceProviderId,
                    ProviderName = g.Key.Name ?? "—",
                    TotalClaimed = g.Sum(c => c.ClaimedAmount),
                    TotalPaid = g.Where(c => c.Status == ClaimStatus.Paid).Sum(c => c.FinalSettlement),
                    TotalPending = g.Where(c => c.Status != ClaimStatus.Paid && c.Status != ClaimStatus.Rejected).Sum(c => c.ApprovedAmount - c.FinalSettlement),
                    TotalDeduction = g.Sum(c => c.DeductionAmount),
                    ClaimCount = g.Count()
                });

            var rows = await query.ToListAsync();

            var paidClaims = await _context.InsuranceClaims
                .Where(c => !c.IsDeleted && c.Status == ClaimStatus.Paid && c.PaymentDate != null && c.SubmissionDate >= start && c.SubmissionDate <= end)
                .Select(c => new { c.InsurancePlan.InsuranceProviderId, c.SubmissionDate, c.PaymentDate })
                .ToListAsync();

            var avgByProvider = paidClaims
                .GroupBy(x => x.InsuranceProviderId)
                .ToDictionary(g => g.Key, g => g.Average(x => (x.PaymentDate.Value - x.SubmissionDate).TotalDays));

            foreach (var r in rows)
            {
                r.DeductionRatePercent = r.TotalClaimed > 0 ? (decimal)((double)r.TotalDeduction / (double)r.TotalClaimed * 100) : 0;
                r.AverageSettlementDays = avgByProvider.ContainsKey(r.InsuranceProviderId) ? avgByProvider[r.InsuranceProviderId] : 0;
            }

            return rows.OrderByDescending(x => x.TotalClaimed).ToList();
        }
    }
}
