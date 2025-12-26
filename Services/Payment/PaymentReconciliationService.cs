using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Models;
using ClinicApp.Models.DTOs.Payment;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicApp.Services.Payment
{
    /// <summary>
    /// سرویس تطبیق و رفع اختلاف‌های مالی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. تطبیق خودکار موجودی جلسه صندوق
    /// 2. شناسایی اختلاف‌ها
    /// 3. رفع اختلاف‌ها
    /// 4. دریافت اختلاف‌های حل نشده
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class PaymentReconciliationService : IPaymentReconciliationService
    {
        #region Fields

        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        #endregion

        #region Constructor

        public PaymentReconciliationService(
            ApplicationDbContext context,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #endregion

        #region ReconcileSessionAsync

        public async Task<ServiceResult<ReconciliationReport>> ReconcileSessionAsync(int cashSessionId)
        {
            try
            {
                _logger.Information("💰 Reconciling session: {CashSessionId}", cashSessionId);

                if (cashSessionId <= 0)
                {
                    return ServiceResult<ReconciliationReport>.Failed("شناسه جلسه صندوق نامعتبر است.", "VALIDATION");
                }

                // دریافت جلسه
                var session = await _context.CashSessions
                    .Include(s => s.Transactions)
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.CashSessionId == cashSessionId && !s.IsDeleted);

                if (session == null)
                {
                    return ServiceResult<ReconciliationReport>.Failed("جلسه صندوق یافت نشد.", "NOT_FOUND");
                }

                // محاسبه موجودی مورد انتظار از تراکنش‌ها
                var transactions = session.Transactions?.Where(t => !t.IsDeleted).ToList() ?? new List<PaymentTransaction>();

                var expectedCashBalance = session.OpeningBalance + 
                    transactions.Where(t => t.Method == PaymentMethod.Cash && t.Status == PaymentStatus.Success)
                        .Sum(t => t.Amount);

                var expectedPosBalance = 
                    transactions.Where(t => t.Method == PaymentMethod.POS && t.Status == PaymentStatus.Success)
                        .Sum(t => t.Amount);

                // محاسبه تفاوت‌ها
                var cashDifference = session.CashBalance - expectedCashBalance;
                var posDifference = session.PosBalance - expectedPosBalance;

                // دریافت اختلاف‌های موجود
                var discrepancies = await _context.PaymentDiscrepancies
                    .Include(d => d.ReportedByUser)
                    .Include(d => d.ResolvedByUser)
                    .Where(d => d.CashSessionId == cashSessionId)
                    .ToListAsync();

                var report = new ReconciliationReport
                {
                    CashSessionId = cashSessionId,
                    ExpectedCashBalance = expectedCashBalance,
                    ActualCashBalance = session.CashBalance,
                    CashDifference = cashDifference,
                    ExpectedPosBalance = expectedPosBalance,
                    ActualPosBalance = session.PosBalance,
                    PosDifference = posDifference,
                    IsReconciled = Math.Abs(cashDifference) < 0.01m && Math.Abs(posDifference) < 0.01m && 
                                   discrepancies.All(d => d.Status == DiscrepancyStatus.Resolved),
                    Discrepancies = discrepancies.Select(d => new DiscrepancyDetail
                    {
                        Id = d.Id,
                        PaymentTransactionId = d.PaymentTransactionId,
                        Type = d.Type.ToString(),
                        ExpectedAmount = d.ExpectedAmount,
                        ActualAmount = d.ActualAmount,
                        Difference = d.Difference,
                        Reason = d.Reason,
                        Resolution = d.Resolution,
                        Status = d.Status.ToString(),
                        ReportedAt = d.ReportedAt,
                        ReportedBy = d.ReportedByUser?.UserName ?? d.ReportedByUser?.Email ?? "نامشخص",
                        ResolvedAt = d.ResolvedAt,
                        ResolvedBy = d.ResolvedByUser?.UserName ?? d.ResolvedByUser?.Email ?? "نامشخص"
                    }).ToList()
                };

                // اگر اختلافی وجود دارد، ثبت آن
                if (Math.Abs(cashDifference) >= 0.01m || Math.Abs(posDifference) >= 0.01m)
                {
                    var discrepancyType = cashDifference < 0 || posDifference < 0 
                        ? DiscrepancyType.Shortage 
                        : DiscrepancyType.Overage;

                    // بررسی وجود اختلاف مشابه
                    var existingDiscrepancy = discrepancies
                        .FirstOrDefault(d => d.Type == discrepancyType && 
                                            d.Status == DiscrepancyStatus.Pending);

                    if (existingDiscrepancy == null)
                    {
                        var newDiscrepancy = new PaymentDiscrepancy
                        {
                            CashSessionId = cashSessionId,
                            Type = discrepancyType,
                            ExpectedAmount = expectedCashBalance + expectedPosBalance,
                            ActualAmount = session.CashBalance + session.PosBalance,
                            Difference = cashDifference + posDifference,
                            Reason = $"تطبیق خودکار: تفاوت نقدی: {cashDifference:N0} ریال، تفاوت POS: {posDifference:N0} ریال",
                            Status = DiscrepancyStatus.Pending,
                            ReportedByUserId = _currentUserService?.UserId ?? "SYSTEM",
                            ReportedAt = DateTime.Now
                        };

                        _context.PaymentDiscrepancies.Add(newDiscrepancy);
                        await _context.SaveChangesAsync();

                        _logger.Warning("⚠️ Discrepancy detected and created. Id: {Id}, Difference: {Difference}", 
                            newDiscrepancy.Id, newDiscrepancy.Difference);
                    }
                }

                _logger.Information("✅ Reconciliation completed. IsReconciled: {IsReconciled}, CashDiff: {CashDiff}, PosDiff: {PosDiff}", 
                    report.IsReconciled, cashDifference, posDifference);

                return ServiceResult<ReconciliationReport>.Successful(report);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error reconciling session: {CashSessionId}", cashSessionId);
                return ServiceResult<ReconciliationReport>.Failed("خطا در تطبیق جلسه", "EXCEPTION");
            }
        }

        #endregion

        #region DetectDiscrepanciesAsync

        public async Task<ServiceResult<DiscrepancyReport>> DetectDiscrepanciesAsync(int cashSessionId)
        {
            try
            {
                _logger.Information("🔍 Detecting discrepancies for session: {CashSessionId}", cashSessionId);

                if (cashSessionId <= 0)
                {
                    return ServiceResult<DiscrepancyReport>.Failed("شناسه جلسه صندوق نامعتبر است.", "VALIDATION");
                }

                // دریافت جلسه
                var session = await _context.CashSessions
                    .Include(s => s.Transactions)
                    .FirstOrDefaultAsync(s => s.CashSessionId == cashSessionId && !s.IsDeleted);

                if (session == null)
                {
                    return ServiceResult<DiscrepancyReport>.Failed("جلسه صندوق یافت نشد.", "NOT_FOUND");
                }

                // دریافت اختلاف‌ها
                var discrepancies = await _context.PaymentDiscrepancies
                    .Include(d => d.ReportedByUser)
                    .Include(d => d.ResolvedByUser)
                    .Include(d => d.PaymentTransaction)
                    .Where(d => d.CashSessionId == cashSessionId)
                    .ToListAsync();

                var report = new DiscrepancyReport
                {
                    CashSessionId = cashSessionId,
                    TotalDiscrepancies = discrepancies.Count,
                    UnresolvedCount = discrepancies.Count(d => d.Status == DiscrepancyStatus.Pending),
                    TotalDiscrepancyAmount = discrepancies.Sum(d => d.Difference),
                    Discrepancies = discrepancies.Select(d => new DiscrepancyDetail
                    {
                        Id = d.Id,
                        PaymentTransactionId = d.PaymentTransactionId,
                        Type = d.Type.ToString(),
                        ExpectedAmount = d.ExpectedAmount,
                        ActualAmount = d.ActualAmount,
                        Difference = d.Difference,
                        Reason = d.Reason,
                        Resolution = d.Resolution,
                        Status = d.Status.ToString(),
                        ReportedAt = d.ReportedAt,
                        ReportedBy = d.ReportedByUser?.UserName ?? d.ReportedByUser?.Email ?? "نامشخص",
                        ResolvedAt = d.ResolvedAt,
                        ResolvedBy = d.ResolvedByUser?.UserName ?? d.ResolvedByUser?.Email ?? "نامشخص"
                    }).ToList()
                };

                _logger.Information("✅ Discrepancies detected. Total: {Total}, Unresolved: {Unresolved}", 
                    report.TotalDiscrepancies, report.UnresolvedCount);

                return ServiceResult<DiscrepancyReport>.Successful(report);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error detecting discrepancies for session: {CashSessionId}", cashSessionId);
                return ServiceResult<DiscrepancyReport>.Failed("خطا در شناسایی اختلاف‌ها", "EXCEPTION");
            }
        }

        #endregion

        #region ResolveDiscrepancyAsync

        public async Task<ServiceResult<bool>> ResolveDiscrepancyAsync(int discrepancyId, string resolution)
        {
            try
            {
                _logger.Information("✅ Resolving discrepancy: {DiscrepancyId}", discrepancyId);

                if (discrepancyId <= 0)
                {
                    return ServiceResult<bool>.Failed("شناسه اختلاف نامعتبر است.", "VALIDATION");
                }

                if (string.IsNullOrWhiteSpace(resolution))
                {
                    return ServiceResult<bool>.Failed("راه‌حل الزامی است.", "VALIDATION");
                }

                var discrepancy = await _context.PaymentDiscrepancies.FindAsync(discrepancyId);
                if (discrepancy == null)
                {
                    return ServiceResult<bool>.Failed("اختلاف یافت نشد.", "NOT_FOUND");
                }

                if (discrepancy.Status == DiscrepancyStatus.Resolved)
                {
                    return ServiceResult<bool>.Failed("این اختلاف قبلاً حل شده است.", "ALREADY_RESOLVED");
                }

                // رفع اختلاف
                discrepancy.Status = DiscrepancyStatus.Resolved;
                discrepancy.Resolution = resolution;
                discrepancy.ResolvedByUserId = _currentUserService?.UserId ?? "SYSTEM";
                discrepancy.ResolvedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.Information("✅ Discrepancy resolved successfully. Id: {Id}", discrepancyId);

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error resolving discrepancy: {DiscrepancyId}", discrepancyId);
                return ServiceResult<bool>.Failed("خطا در رفع اختلاف", "EXCEPTION");
            }
        }

        #endregion

        #region GetUnresolvedDiscrepanciesAsync

        public async Task<ServiceResult<List<PaymentDiscrepancy>>> GetUnresolvedDiscrepanciesAsync()
        {
            try
            {
                _logger.Information("📋 Getting unresolved discrepancies");

                var discrepancies = await _context.PaymentDiscrepancies
                    .Include(d => d.CashSession)
                    .Include(d => d.ReportedByUser)
                    .Include(d => d.PaymentTransaction)
                    .Where(d => d.Status == DiscrepancyStatus.Pending)
                    .OrderByDescending(d => d.ReportedAt)
                    .ToListAsync();

                _logger.Information("✅ Retrieved {Count} unresolved discrepancies", discrepancies.Count);

                return ServiceResult<List<PaymentDiscrepancy>>.Successful(discrepancies);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting unresolved discrepancies");
                return ServiceResult<List<PaymentDiscrepancy>>.Failed("خطا در دریافت اختلاف‌های حل نشده", "EXCEPTION");
            }
        }

        #endregion
    }
}

