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
                    .OrderByDescending(cs => cs.StartTime)
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
                    .OrderBy(cs => cs.StartTime)
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
                    .OrderByDescending(cs => cs.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جلسات کاربر. شناسه کاربر: {UserId}", userId);
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
                               cs.StartTime >= startDate && 
                               cs.StartTime <= endDate)
                    .OrderByDescending(cs => cs.StartTime)
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
                    .OrderByDescending(cs => cs.StartTime)
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

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(cs => 
                        cs.SessionNumber.Contains(searchTerm) ||
                        cs.Description.Contains(searchTerm) ||
                        cs.User.UserName.Contains(searchTerm));
                }

                return await query
                    .OrderByDescending(cs => cs.StartTime)
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
                return await _context.CashSessions
                    .AnyAsync(cs => !cs.IsDeleted && 
                               cs.UserId == userId && 
                               cs.Status == CashSessionStatus.Active);
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

        public async Task<CashSessionStatistics> GetStatisticsAsync()
        {
            try
            {
                var sessions = await _context.CashSessions
                    .Where(cs => !cs.IsDeleted)
                    .ToListAsync();

                return new CashSessionStatistics
                {
                    TotalSessions = sessions.Count,
                    ActiveSessions = sessions.Count(s => s.Status == CashSessionStatus.Active),
                    CompletedSessions = sessions.Count(s => s.Status == CashSessionStatus.Closed),
                    CancelledSessions = sessions.Count(s => s.Status == CashSessionStatus.UnderReview),
                    TotalInitialCash = sessions.Sum(s => s.InitialCashAmount),
                    TotalFinalCash = sessions.Sum(s => s.FinalCashAmount),
                    TotalIncome = sessions.Sum(s => s.TotalIncome),
                    TotalExpense = sessions.Sum(s => s.TotalExpense),
                    TotalDifference = sessions.Sum(s => s.Difference),
                    AverageSessionDuration = (decimal)sessions.Where(s => s.EndTime.HasValue).Average(s => (s.EndTime.Value - s.StartTime).TotalMinutes),
                    LastSessionDate = sessions.OrderByDescending(s => s.StartTime).FirstOrDefault()?.StartTime
                };
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
                var sessions = await _context.CashSessions
                    .Where(cs => !cs.IsDeleted && 
                               cs.StartTime >= startDate && 
                               cs.StartTime <= endDate)
                    .ToListAsync();

                return new CashSessionStatistics
                {
                    TotalSessions = sessions.Count,
                    ActiveSessions = sessions.Count(s => s.Status == CashSessionStatus.Active),
                    CompletedSessions = sessions.Count(s => s.Status == CashSessionStatus.Closed),
                    CancelledSessions = sessions.Count(s => s.Status == CashSessionStatus.UnderReview),
                    TotalInitialCash = sessions.Sum(s => s.InitialCashAmount),
                    TotalFinalCash = sessions.Sum(s => s.FinalCashAmount),
                    TotalIncome = sessions.Sum(s => s.TotalIncome),
                    TotalExpense = sessions.Sum(s => s.TotalExpense),
                    TotalDifference = sessions.Sum(s => s.Difference),
                    AverageSessionDuration = (decimal)sessions.Where(s => s.EndTime.HasValue).Average(s => (s.EndTime.Value - s.StartTime).TotalMinutes),
                    LastSessionDate = sessions.OrderByDescending(s => s.StartTime).FirstOrDefault()?.StartTime
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار جلسات نقدی بر اساس تاریخ");
                throw;
            }
        }

        #endregion
    }
}
