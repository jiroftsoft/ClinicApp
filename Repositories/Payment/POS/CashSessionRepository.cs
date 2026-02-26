using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp.Repositories.Payment.POS
{
    /// <summary>
    /// پیاده‌سازی مخزن جلسات نقدی
    /// </summary>
    public class CashSessionRepository : ICashSessionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public CashSessionRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        #region CRUD Operations

        public async Task<CashSession> GetByIdAsync(int sessionId)
        {
            try
            {
                return await _context.CashSessions
                    .Include(cs => cs.User)
                    .Include(cs => cs.UpdatedByUser)
                    .Include(cs => cs.Transactions) // ✅ Include Transactions برای محاسبه تعداد
                    .FirstOrDefaultAsync(cs => cs.CashSessionId == sessionId && !cs.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جلسه نقدی. شناسه: {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<IEnumerable<CashSession>> GetAllAsync(int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                return await _context.CashSessions
                    .Include(cs => cs.User)
                    .Include(cs => cs.UpdatedByUser)
                    .Where(cs => !cs.IsDeleted)
                    .OrderByDescending(cs => cs.OpenedAt) // ✅ استفاده از OpenedAt به جای StartTime (computed property)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست جلسات نقدی");
                throw;
            }
        }

        public async Task<CashSession> AddAsync(CashSession session)
        {
            try
            {
                _context.CashSessions.Add(session);
                await _context.SaveChangesAsync();
                return session;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد جلسه نقدی");
                throw;
            }
        }

        public async Task<CashSession> UpdateAsync(CashSession session)
        {
            try
            {
                _context.Entry(session).State = System.Data.Entity.EntityState.Modified;
                await _context.SaveChangesAsync();
                return session;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی جلسه نقدی. شناسه: {SessionId}", session.CashSessionId);
                throw;
            }
        }

        /// <summary>
        /// بستن جلسه با UPDATE شرطی در تراکنش — فقط در صورت باز بودن ردیف به‌روز می‌شود (جلوگیری از race)
        /// </summary>
        public async Task<CashSession> TryCloseSessionConditionalAsync(int sessionId, DateTime closedAt, decimal finalCashBalance, string updatedByUserId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // UPDATE فقط وقتی ClosedAt IS NULL و Status = Open/Active — یک درخواست موفق می‌شود
                    const string sql = @"UPDATE CashSessions SET Status = {1}, ClosedAt = {2}, CashBalance = {3}, UpdatedAt = {4}, UpdatedByUserId = {5} 
WHERE CashSessionId = {0} AND ClosedAt IS NULL AND (Status = 1)";
                    var rows = await _context.Database.ExecuteSqlCommandAsync(sql,
                        sessionId,
                        (int)CashSessionStatus.Closed,
                        closedAt,
                        finalCashBalance,
                        closedAt,
                        (object)updatedByUserId ?? (object)DBNull.Value);
                    if (rows == 0)
                    {
                        transaction.Rollback();
                        _logger.Warning("TryCloseSessionConditional: جلسه قبلاً بسته شده یا وجود ندارد. SessionId: {SessionId}", sessionId);
                        return null;
                    }
                    transaction.Commit();
                    return await GetByIdAsync(sessionId);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.Error(ex, "خطا در بستن شرطی جلسه. SessionId: {SessionId}", sessionId);
                    throw;
                }
            }
        }

        public async Task<ServiceResult> SoftDeleteAsync(int sessionId, string deletedByUserId)
        {
            try
            {
                var session = await _context.CashSessions
                    .FirstOrDefaultAsync(cs => cs.CashSessionId == sessionId && !cs.IsDeleted);

                if (session == null)
                {
                    return ServiceResult.Failed("جلسه نقدی یافت نشد");
                }

                session.IsDeleted = true;
                session.DeletedAt = DateTime.UtcNow;
                session.DeletedByUserId = deletedByUserId;

                await _context.SaveChangesAsync();
                return ServiceResult.Successful("جلسه نقدی با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف جلسه نقدی. شناسه: {SessionId}", sessionId);
                return ServiceResult.Failed("خطا در حذف جلسه نقدی");
            }
        }

        #endregion

        #region Query Operations

        public async Task<IEnumerable<CashSession>> GetActiveSessionsAsync()
        {
            try
            {
                return await _context.CashSessions
                    .Include(cs => cs.User)
                    .Where(cs => !cs.IsDeleted && cs.Status == CashSessionStatus.Active)
                    .OrderBy(cs => cs.OpenedAt) // ✅ استفاده از OpenedAt به جای StartTime (computed property)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جلسات فعال");
                throw;
            }
        }

        public async Task<IEnumerable<CashSession>> GetByUserIdAsync(string userId)
        {
            try
            {
                return await _context.CashSessions
                    .Include(cs => cs.User)
                    .Include(cs => cs.UpdatedByUser)
                    .Where(cs => !cs.IsDeleted && cs.UserId == userId)
                    .OrderByDescending(cs => cs.OpenedAt) // ✅ استفاده از OpenedAt به جای StartTime (computed property)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جلسات کاربر. شناسه کاربر: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// دریافت جلسات کاربر با صفحه‌بندی در سطح DB — بدون بارگذاری همه جلسات در حافظه
        /// </summary>
        public async Task<IEnumerable<CashSession>> GetByUserIdPagedAsync(string userId, int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;
                return await _context.CashSessions
                    .Include(cs => cs.User)
                    .Include(cs => cs.UpdatedByUser)
                    .Where(cs => !cs.IsDeleted && cs.UserId == userId)
                    .OrderByDescending(cs => cs.OpenedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جلسات صفحه‌بندی‌شده کاربر. UserId: {UserId}, Page: {Page}", userId, pageNumber);
                throw;
            }
        }

        public async Task<IEnumerable<CashSession>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.CashSessions
                    .Include(cs => cs.User)
                    .Include(cs => cs.UpdatedByUser)
                    .Where(cs => !cs.IsDeleted && 
                               cs.OpenedAt >= startDate && // ✅ استفاده از OpenedAt به جای StartTime (computed property)
                               cs.OpenedAt <= endDate)
                    .OrderByDescending(cs => cs.OpenedAt) // ✅ استفاده از OpenedAt به جای StartTime (computed property)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جلسات بر اساس تاریخ. از: {StartDate}, تا: {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<CashSession>> GetByStatusAsync(CashSessionStatus status)
        {
            try
            {
                return await _context.CashSessions
                    .Include(cs => cs.User)
                    .Include(cs => cs.UpdatedByUser)
                    .Where(cs => !cs.IsDeleted && cs.Status == status)
                    .OrderByDescending(cs => cs.OpenedAt) // ✅ استفاده از OpenedAt به جای StartTime (computed property)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جلسات بر اساس وضعیت. وضعیت: {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<CashSession>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.CashSessions
                    .Include(cs => cs.User)
                    .Include(cs => cs.UpdatedByUser)
                    .Where(cs => !cs.IsDeleted);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim();
                    // SessionNumber و Description در entity محاسبه‌شده‌اند و ستون DB نیستند — جستجو با CashSessionId و UserName
                    int sessionIdVal;
                    bool searchById = int.TryParse(term, out sessionIdVal)
                        || (term.Length > 2 && term.StartsWith("CS", StringComparison.OrdinalIgnoreCase)
                            && int.TryParse(term.Substring(2).Trim(), out sessionIdVal));
                    if (searchById)
                        query = query.Where(cs => cs.CashSessionId == sessionIdVal || cs.User.UserName.Contains(searchTerm));
                    else
                        query = query.Where(cs => cs.User.UserName.Contains(searchTerm));
                }

                return await query
                    .OrderByDescending(cs => cs.OpenedAt) // ✅ استفاده از OpenedAt به جای StartTime (computed property)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی جلسات نقدی. عبارت: {SearchTerm}", searchTerm);
                throw;
            }
        }

        #endregion

        #region Validation Operations

        public async Task<bool> ExistsAsync(int sessionId)
        {
            try
            {
                return await _context.CashSessions
                    .AnyAsync(cs => cs.CashSessionId == sessionId && !cs.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود جلسه. شناسه: {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<bool> HasActiveSessionAsync(string userId)
        {
            try
            {
                // ✅ چک کردن هم Open و هم Active (هر دو مقدار 1 دارند اما برای وضوح هر دو را چک می‌کنیم)
                return await _context.CashSessions
                    .AnyAsync(cs => !cs.IsDeleted && 
                               cs.UserId == userId && 
                               (cs.Status == CashSessionStatus.Active || cs.Status == CashSessionStatus.Open) &&
                               cs.ClosedAt == null); // ✅ همچنین باید ClosedAt null باشد
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی جلسه فعال کاربر. شناسه کاربر: {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetCountAsync()
        {
            try
            {
                return await _context.CashSessions
                    .CountAsync(cs => !cs.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش جلسات نقدی");
                throw;
            }
        }

        #endregion

        #region Statistics Operations

        /// <summary>
        /// آمار جلسات با تجمیع در SQL — بدون بارگذاری همه ردیف‌ها در حافظه
        /// </summary>
        public async Task<CashSessionStatistics> GetStatisticsAsync()
        {
            try
            {
                var baseQuery = _context.CashSessions.Where(cs => !cs.IsDeleted);
                return await BuildStatisticsFromQueryAsync(baseQuery);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار جلسات نقدی");
                throw;
            }
        }

        public async Task<CashSessionStatistics> GetStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var baseQuery = _context.CashSessions
                    .Where(cs => !cs.IsDeleted && cs.OpenedAt >= startDate && cs.OpenedAt <= endDate);
                return await BuildStatisticsFromQueryAsync(baseQuery);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار جلسات نقدی بر اساس تاریخ");
                throw;
            }
        }

        /// <summary>
        /// ساخت آمار از IQueryable با تجمیع در دیتابیس (Count/Sum در SQL)
        /// </summary>
        private async Task<CashSessionStatistics> BuildStatisticsFromQueryAsync(IQueryable<CashSession> baseQuery)
        {
            var q = baseQuery.GroupBy(cs => 1).Select(g => new
            {
                TotalSessions = g.Count(),
                ActiveSessions = g.Count(cs => cs.Status == CashSessionStatus.Active),
                CompletedSessions = g.Count(cs => cs.Status == CashSessionStatus.Closed),
                CancelledSessions = g.Count(cs => cs.Status == CashSessionStatus.UnderReview),
                TotalInitialCash = g.Sum(cs => cs.OpeningBalance),
                TotalFinalCash = g.Sum(cs => cs.CashBalance),
                TotalIncome = g.Sum(cs => cs.CashBalance + cs.PosBalance),
                TotalDifference = g.Sum(cs => cs.CashBalance - cs.OpeningBalance - cs.PosBalance),
                LastSessionDate = g.Max(cs => cs.OpenedAt)
            });
            var row = await q.FirstOrDefaultAsync();
            if (row == null)
                return new CashSessionStatistics();

            // میانگین مدت جلسه فقط برای جلسات بسته‌شده — کوئری جدا برای سازگاری با EF6
            decimal avgMinutes = 0;
            var closedCount = await baseQuery.CountAsync(cs => cs.ClosedAt != null);
            if (closedCount > 0)
            {
                var avgNullable = await baseQuery
                    .Where(cs => cs.ClosedAt != null)
                    .Select(cs => System.Data.Entity.DbFunctions.DiffMinutes(cs.OpenedAt, cs.ClosedAt))
                    .AverageAsync(); // در EF میانگین روی int? به double? برمی‌گردد
                if (avgNullable.HasValue) avgMinutes = (decimal)avgNullable.Value;
            }

            return new CashSessionStatistics
            {
                TotalSessions = row.TotalSessions,
                ActiveSessions = row.ActiveSessions,
                CompletedSessions = row.CompletedSessions,
                CancelledSessions = row.CancelledSessions,
                TotalInitialCash = row.TotalInitialCash,
                TotalFinalCash = row.TotalFinalCash,
                TotalIncome = row.TotalIncome,
                TotalExpense = 0m,
                TotalDifference = row.TotalDifference,
                AverageSessionDuration = avgMinutes,
                LastSessionDate = row.LastSessionDate
            };
        }

        #endregion
    }
}
