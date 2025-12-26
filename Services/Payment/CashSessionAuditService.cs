using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Models;
using ClinicApp.Models.DTOs.Payment;
using ClinicApp.Models.Entities.Payment;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ClinicApp.Services.Payment
{
    /// <summary>
    /// سرویس Audit Trail برای جلسات صندوق
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ثبت تمام تغییرات جلسات صندوق
    /// 2. دریافت لاگ‌های یک جلسه
    /// 3. دریافت لاگ‌های یک کاربر
    /// 4. خلاصه Audit Trail
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class CashSessionAuditService : ICashSessionAuditService
    {
        #region Fields

        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        #endregion

        #region Constructor

        public CashSessionAuditService(
            ApplicationDbContext context,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #endregion

        #region LogActionAsync

        public async Task<ServiceResult> LogActionAsync(int cashSessionId, string action, object oldValue, object newValue, string reason)
        {
            try
            {
                _logger.Information("📝 Logging action: {Action} for CashSession: {CashSessionId}", action, cashSessionId);

                if (cashSessionId <= 0)
                {
                    return ServiceResult.Failed("شناسه جلسه صندوق نامعتبر است.", "VALIDATION");
                }

                if (string.IsNullOrWhiteSpace(action))
                {
                    return ServiceResult.Failed("نوع اقدام الزامی است.", "VALIDATION");
                }

                // بررسی وجود جلسه
                var session = await _context.CashSessions.FindAsync(cashSessionId);
                if (session == null)
                {
                    return ServiceResult.Failed("جلسه صندوق یافت نشد.", "NOT_FOUND");
                }

                // دریافت اطلاعات کاربر و IP
                var userId = _currentUserService?.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("⚠️ UserId is null, using system user");
                    userId = "SYSTEM";
                }

                var ipAddress = GetClientIpAddress();
                var userAgent = GetUserAgent();

                // تبدیل مقادیر به JSON
                var oldValueJson = oldValue != null ? JsonConvert.SerializeObject(oldValue) : null;
                var newValueJson = newValue != null ? JsonConvert.SerializeObject(newValue) : null;

                // ایجاد لاگ
                var auditLog = new CashSessionAuditLog
                {
                    CashSessionId = cashSessionId,
                    Action = action,
                    OldValue = oldValueJson,
                    NewValue = newValueJson,
                    Reason = reason,
                    PerformedByUserId = userId,
                    PerformedAt = DateTime.Now,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                _context.CashSessionAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.Information("✅ Audit log created successfully. Id: {Id}, Action: {Action}, CashSessionId: {CashSessionId}", 
                    auditLog.Id, action, cashSessionId);

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error logging action: {Action} for CashSession: {CashSessionId}", action, cashSessionId);
                return ServiceResult.Failed("خطا در ثبت لاگ", "EXCEPTION");
            }
        }

        #endregion

        #region GetAuditLogsAsync

        public async Task<ServiceResult<List<CashSessionAuditLog>>> GetAuditLogsAsync(int cashSessionId)
        {
            try
            {
                _logger.Information("📋 Getting audit logs for CashSession: {CashSessionId}", cashSessionId);

                if (cashSessionId <= 0)
                {
                    return ServiceResult<List<CashSessionAuditLog>>.Failed("شناسه جلسه صندوق نامعتبر است.", "VALIDATION");
                }

                var logs = await _context.CashSessionAuditLogs
                    .Include(l => l.PerformedByUser)
                    .Include(l => l.CashSession)
                    .Where(l => l.CashSessionId == cashSessionId)
                    .OrderByDescending(l => l.PerformedAt)
                    .ToListAsync();

                _logger.Information("✅ Retrieved {Count} audit logs for CashSession: {CashSessionId}", logs.Count, cashSessionId);

                return ServiceResult<List<CashSessionAuditLog>>.Successful(logs);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting audit logs for CashSession: {CashSessionId}", cashSessionId);
                return ServiceResult<List<CashSessionAuditLog>>.Failed("خطا در دریافت لاگ‌ها", "EXCEPTION");
            }
        }

        #endregion

        #region GetUserAuditLogsAsync

        public async Task<ServiceResult<List<CashSessionAuditLog>>> GetUserAuditLogsAsync(string userId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.Information("📋 Getting audit logs for User: {UserId} from {FromDate} to {ToDate}", userId, fromDate, toDate);

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ServiceResult<List<CashSessionAuditLog>>.Failed("شناسه کاربر الزامی است.", "VALIDATION");
                }

                if (fromDate > toDate)
                {
                    return ServiceResult<List<CashSessionAuditLog>>.Failed("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.", "VALIDATION");
                }

                var logs = await _context.CashSessionAuditLogs
                    .Include(l => l.PerformedByUser)
                    .Include(l => l.CashSession)
                    .Where(l => l.PerformedByUserId == userId &&
                                l.PerformedAt >= fromDate &&
                                l.PerformedAt <= toDate)
                    .OrderByDescending(l => l.PerformedAt)
                    .ToListAsync();

                _logger.Information("✅ Retrieved {Count} audit logs for User: {UserId}", logs.Count, userId);

                return ServiceResult<List<CashSessionAuditLog>>.Successful(logs);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting audit logs for User: {UserId}", userId);
                return ServiceResult<List<CashSessionAuditLog>>.Failed("خطا در دریافت لاگ‌ها", "EXCEPTION");
            }
        }

        #endregion

        #region GetAuditSummaryAsync

        public async Task<ServiceResult<AuditSummary>> GetAuditSummaryAsync(int cashSessionId)
        {
            try
            {
                _logger.Information("📊 Getting audit summary for CashSession: {CashSessionId}", cashSessionId);

                if (cashSessionId <= 0)
                {
                    return ServiceResult<AuditSummary>.Failed("شناسه جلسه صندوق نامعتبر است.", "VALIDATION");
                }

                var logs = await _context.CashSessionAuditLogs
                    .Include(l => l.PerformedByUser)
                    .Where(l => l.CashSessionId == cashSessionId)
                    .ToListAsync();

                if (logs.Count == 0)
                {
                    return ServiceResult<AuditSummary>.Successful(new AuditSummary
                    {
                        CashSessionId = cashSessionId,
                        TotalLogs = 0
                    });
                }

                var summary = new AuditSummary
                {
                    CashSessionId = cashSessionId,
                    TotalLogs = logs.Count,
                    FirstLogDate = logs.Min(l => l.PerformedAt),
                    LastLogDate = logs.Max(l => l.PerformedAt),
                    UniqueUserCount = logs.Select(l => l.PerformedByUserId).Distinct().Count(),
                    UserNames = logs
                        .Where(l => l.PerformedByUser != null)
                        .Select(l => l.PerformedByUser.UserName ?? l.PerformedByUser.Email ?? "نامشخص")
                        .Distinct()
                        .ToList()
                };

                // شمارش اقدامات
                summary.ActionCounts = logs
                    .GroupBy(l => l.Action)
                    .ToDictionary(g => g.Key, g => g.Count());

                _logger.Information("✅ Audit summary generated. TotalLogs: {Count}, UniqueUsers: {UserCount}", 
                    summary.TotalLogs, summary.UniqueUserCount);

                return ServiceResult<AuditSummary>.Successful(summary);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting audit summary for CashSession: {CashSessionId}", cashSessionId);
                return ServiceResult<AuditSummary>.Failed("خطا در دریافت خلاصه لاگ‌ها", "EXCEPTION");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت IP Address کلاینت
        /// </summary>
        private string GetClientIpAddress()
        {
            try
            {
                if (HttpContext.Current?.Request != null)
                {
                    var ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(ipAddress))
                    {
                        ipAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    }
                    return ipAddress ?? "Unknown";
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// دریافت User Agent
        /// </summary>
        private string GetUserAgent()
        {
            try
            {
                if (HttpContext.Current?.Request != null)
                {
                    return HttpContext.Current.Request.UserAgent ?? "Unknown";
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        #endregion
    }
}
