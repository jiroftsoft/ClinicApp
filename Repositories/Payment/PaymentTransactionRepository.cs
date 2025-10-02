using ClinicApp.Interfaces.Payment;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Statistics;
using Serilog;
using PaymentTransactionStatistics = ClinicApp.Models.Statistics.PaymentTransactionStatistics;

namespace ClinicApp.Repositories.Payment
{
    /// <summary>
    /// Repository برای مدیریت تراکنش‌های پرداخت
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت CRUD تراکنش‌های پرداخت
    /// </summary>
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public PaymentTransactionRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        #region CRUD Operations

        public async Task<PaymentTransaction> GetByIdAsync(int transactionId)
        {
            try
            {
                return await _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .FirstOrDefaultAsync(pt => !pt.IsDeleted && pt.PaymentTransactionId == transactionId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش پرداخت. شناسه: {TransactionId}", transactionId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentTransaction>> GetAllAsync(int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                return await _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .Where(pt => !pt.IsDeleted)
                    .OrderByDescending(pt => pt.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌های پرداخت");
                throw;
            }
        }

        public async Task<PaymentTransaction> AddAsync(PaymentTransaction transaction)
        {
            try
            {
                _context.PaymentTransactions.Add(transaction);
                await _context.SaveChangesAsync();
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزودن تراکنش پرداخت");
                throw;
            }
        }

        public async Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction)
        {
            try
            {
                _context.PaymentTransactions.Add(transaction);
                await _context.SaveChangesAsync();
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تراکنش پرداخت");
                throw;
            }
        }

        public async Task<PaymentTransaction> UpdateAsync(PaymentTransaction transaction)
        {
            try
            {
                _context.Entry(transaction).State = System.Data.Entity.EntityState.Modified;
                await _context.SaveChangesAsync();
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تراکنش پرداخت. شناسه: {TransactionId}", transaction.PaymentGatewayId);
                throw;
            }
        }

        public async Task<ServiceResult> SoftDeleteAsync(int transactionId, string deletedByUserId)
        {
            try
            {
                var transaction = await _context.PaymentTransactions
                    .FirstOrDefaultAsync(pt => !pt.IsDeleted && pt.PaymentGatewayId == transactionId);

                if (transaction == null)
                {
                    return ServiceResult.Failed("تراکنش پرداخت یافت نشد");
                }

                transaction.IsDeleted = true;
                transaction.DeletedAt = DateTime.UtcNow;
                transaction.DeletedByUserId = deletedByUserId;

                await _context.SaveChangesAsync();
                return ServiceResult.Successful("تراکنش پرداخت با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نرم تراکنش پرداخت. شناسه: {TransactionId}", transactionId);
                return ServiceResult.Failed("خطا در حذف تراکنش پرداخت");
            }
        }

        #endregion

        #region Query Operations

        public async Task<IEnumerable<PaymentTransaction>> GetByReceptionIdAsync(int receptionId)
        {
            try
            {
                return await _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .Where(pt => !pt.IsDeleted && pt.ReceptionId == receptionId)
                    .OrderByDescending(pt => pt.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌های پرداخت بر اساس ReceptionId. شناسه: {ReceptionId}", receptionId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByPaymentMethodAsync(PaymentMethod paymentMethod)
        {
            try
            {
                return await _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .Where(pt => !pt.IsDeleted && pt.Method == paymentMethod)
                    .OrderByDescending(pt => pt.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌های پرداخت بر اساس روش پرداخت. روش: {PaymentMethod}", paymentMethod);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByStatusAsync(PaymentStatus status)
        {
            try
            {
                return await _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .Where(pt => !pt.IsDeleted && pt.Status == status)
                    .OrderByDescending(pt => pt.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌های پرداخت بر اساس وضعیت. وضعیت: {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .Where(pt => !pt.IsDeleted && pt.CreatedAt >= startDate && pt.CreatedAt <= endDate)
                    .OrderByDescending(pt => pt.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌های پرداخت بر اساس بازه تاریخ. از: {StartDate} تا: {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            try
            {
                return await _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .Where(pt => !pt.IsDeleted && pt.Amount >= minAmount && pt.Amount <= maxAmount)
                    .OrderByDescending(pt => pt.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌های پرداخت بر اساس بازه مبلغ. از: {MinAmount} تا: {MaxAmount}", minAmount, maxAmount);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentTransaction>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                return await _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .Where(pt => !pt.IsDeleted && 
                        (pt.ReferenceCode.Contains(searchTerm) ||
                         pt.Description.Contains(searchTerm) ||
                         pt.CreatedByUser.UserName.Contains(searchTerm)))
                    .OrderByDescending(pt => pt.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی تراکنش‌های پرداخت. عبارت: {SearchTerm}", searchTerm);
                throw;
            }
        }

        #endregion

        #region Statistics Operations

        public async Task<decimal> GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.PaymentTransactions
                    .Where(pt => !pt.IsDeleted && pt.CreatedAt >= startDate && pt.CreatedAt <= endDate)
                    .SumAsync(pt => pt.Amount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه مجموع مبلغ تراکنش‌ها. از: {StartDate} تا: {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<decimal> GetTotalAmountByPaymentMethodAsync(PaymentMethod paymentMethod)
        {
            try
            {
                return await _context.PaymentTransactions
                    .Where(pt => !pt.IsDeleted && pt.Method == paymentMethod)
                    .SumAsync(pt => pt.Amount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه مجموع مبلغ تراکنش‌ها بر اساس روش پرداخت. روش: {PaymentMethod}", paymentMethod);
                throw;
            }
        }

        public async Task<int> GetTransactionCountByStatusAsync(PaymentStatus status)
        {
            try
            {
                return await _context.PaymentTransactions
                    .CountAsync(pt => !pt.IsDeleted && pt.Status == status);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش تراکنش‌ها بر اساس وضعیت. وضعیت: {Status}", status);
                throw;
            }
        }

        #endregion

        #region Missing Methods Implementation

        public async Task<IEnumerable<PaymentTransaction>> GetByPatientIdAsync(int patientId, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .Where(pt => !pt.IsDeleted && pt.Reception.PatientId == patientId)
                    .OrderByDescending(pt => pt.CreatedAt);

                return await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌های بیمار. شناسه بیمار: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByPaymentMethodAsync(PaymentMethod paymentMethod, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .Where(pt => !pt.IsDeleted && pt.Method == paymentMethod);

                if (startDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt <= endDate.Value);

                return await query
                    .OrderByDescending(pt => pt.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌ها بر اساس روش پرداخت. روش: {PaymentMethod}", paymentMethod);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByStatusAsync(PaymentStatus status, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .Where(pt => !pt.IsDeleted && pt.Status == status);

                if (startDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt <= endDate.Value);

                return await query
                    .OrderByDescending(pt => pt.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌ها بر اساس وضعیت. وضعیت: {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                return await _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .Where(pt => !pt.IsDeleted && pt.CreatedAt >= startDate && pt.CreatedAt <= endDate)
                    .OrderByDescending(pt => pt.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌ها بر اساس بازه تاریخ. از: {StartDate}, تا: {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .Where(pt => !pt.IsDeleted && pt.Amount >= minAmount && pt.Amount <= maxAmount);

                if (startDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(pt => pt.CreatedAt <= endDate.Value);

                return await query
                    .OrderByDescending(pt => pt.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌ها بر اساس بازه مبلغ. از: {MinAmount}, تا: {MaxAmount}", minAmount, maxAmount);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentTransaction>> AdvancedSearchAsync(PaymentTransactionSearchFilters filters, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.PaymentTransactions
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Include(pt => pt.Reception)
                    .Include(pt => pt.PosTerminal)
                    .Include(pt => pt.CashSession)
                    .Where(pt => !pt.IsDeleted);

                if (filters != null)
                {
                    if (filters.PatientId.HasValue)
                        query = query.Where(pt => pt.Reception.PatientId == filters.PatientId.Value);

                    if (filters.ReceptionId.HasValue)
                        query = query.Where(pt => pt.ReceptionId == filters.ReceptionId.Value);

                    if (filters.PaymentMethod.HasValue)
                        query = query.Where(pt => pt.Method == filters.PaymentMethod.Value);

                    if (filters.Status.HasValue)
                        query = query.Where(pt => pt.Status == filters.Status.Value);

                    if (filters.MinAmount.HasValue)
                        query = query.Where(pt => pt.Amount >= filters.MinAmount.Value);

                    if (filters.MaxAmount.HasValue)
                        query = query.Where(pt => pt.Amount <= filters.MaxAmount.Value);

                    if (filters.StartDate.HasValue)
                        query = query.Where(pt => pt.CreatedAt >= filters.StartDate.Value);

                    if (filters.EndDate.HasValue)
                        query = query.Where(pt => pt.CreatedAt <= filters.EndDate.Value);

                    if (!string.IsNullOrEmpty(filters.TransactionId))
                        query = query.Where(pt => pt.TransactionId.Contains(filters.TransactionId));

                    if (!string.IsNullOrEmpty(filters.ReferenceCode))
                        query = query.Where(pt => pt.ReferenceCode.Contains(filters.ReferenceCode));
                }

                return await query
                    .OrderByDescending(pt => pt.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پیشرفته تراکنش‌ها");
                throw;
            }
        }

        public async Task<Models.Statistics.PaymentTransactionStatistics> GetStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var transactions = await _context.PaymentTransactions
                    .Where(pt => !pt.IsDeleted && pt.CreatedAt >= startDate && pt.CreatedAt <= endDate)
                    .ToListAsync();

                return new PaymentTransactionStatistics
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalTransactions = transactions.Count,
                    TotalAmount = transactions.Sum(t => t.Amount),
                    SuccessfulTransactions = transactions.Count(t => t.Status == PaymentStatus.Success),
                    SuccessfulAmount = transactions.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount),
                    FailedTransactions = transactions.Count(t => t.Status == PaymentStatus.Failed),
                    FailedAmount = transactions.Where(t => t.Status == PaymentStatus.Failed).Sum(t => t.Amount),
                    PendingTransactions = transactions.Count(t => t.Status == PaymentStatus.Pending),
                    PendingAmount = transactions.Where(t => t.Status == PaymentStatus.Pending).Sum(t => t.Amount),
                    CanceledTransactions = transactions.Count(t => t.Status == PaymentStatus.Canceled),
                    CanceledAmount = transactions.Where(t => t.Status == PaymentStatus.Canceled).Sum(t => t.Amount),
                    CalculatedAt = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه آمار تراکنش‌ها. از: {StartDate}, تا: {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<Models.Statistics.PaymentTransactionStatistics> GetStatisticsByPaymentMethodAsync(PaymentMethod paymentMethod, DateTime startDate, DateTime endDate)
        {
            try
            {
                var transactions = await _context.PaymentTransactions
                    .Where(pt => !pt.IsDeleted && pt.Method == paymentMethod && pt.CreatedAt >= startDate && pt.CreatedAt <= endDate)
                    .ToListAsync();

                return new PaymentTransactionStatistics
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalTransactions = transactions.Count,
                    TotalAmount = transactions.Sum(t => t.Amount),
                    SuccessfulTransactions = transactions.Count(t => t.Status == PaymentStatus.Success),
                    SuccessfulAmount = transactions.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount),
                    FailedTransactions = transactions.Count(t => t.Status == PaymentStatus.Failed),
                    FailedAmount = transactions.Where(t => t.Status == PaymentStatus.Failed).Sum(t => t.Amount),
                    PendingTransactions = transactions.Count(t => t.Status == PaymentStatus.Pending),
                    PendingAmount = transactions.Where(t => t.Status == PaymentStatus.Pending).Sum(t => t.Amount),
                    CanceledTransactions = transactions.Count(t => t.Status == PaymentStatus.Canceled),
                    CanceledAmount = transactions.Where(t => t.Status == PaymentStatus.Canceled).Sum(t => t.Amount),
                    CalculatedAt = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه آمار تراکنش‌ها بر اساس روش پرداخت. روش: {PaymentMethod}", paymentMethod);
                throw;
            }
        }

        public async Task<Models.Statistics.DailyPaymentStatistics> GetDailyStatisticsAsync(DateTime date)
        {
            try
            {
                var startOfDay = date.Date;
                var endOfDay = startOfDay.AddDays(1);

                var transactions = await _context.PaymentTransactions
                    .Where(pt => !pt.IsDeleted && pt.CreatedAt >= startOfDay && pt.CreatedAt < endOfDay)
                    .ToListAsync();

                return new Models.Statistics.DailyPaymentStatistics
                {
                    Date = date,
                    TotalTransactions = transactions.Count,
                    TotalAmount = transactions.Sum(t => t.Amount),
                    SuccessfulTransactions = transactions.Count(t => t.Status == PaymentStatus.Success),
                    SuccessfulAmount = transactions.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount),
                    FailedTransactions = transactions.Count(t => t.Status == PaymentStatus.Failed),
                    FailedAmount = transactions.Where(t => t.Status == PaymentStatus.Failed).Sum(t => t.Amount),
                    PendingTransactions = transactions.Count(t => t.Status == PaymentStatus.Pending),
                    PendingAmount = transactions.Where(t => t.Status == PaymentStatus.Pending).Sum(t => t.Amount),
                    CanceledTransactions = transactions.Count(t => t.Status == PaymentStatus.Canceled),
                    CanceledAmount = transactions.Where(t => t.Status == PaymentStatus.Canceled).Sum(t => t.Amount),
                    CalculatedAt = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه آمار روزانه تراکنش‌ها. تاریخ: {Date}", date);
                throw;
            }
        }

        public async Task<Models.Statistics.MonthlyPaymentStatistics> GetMonthlyStatisticsAsync(int year, int month)
        {
            try
            {
                var startOfMonth = new DateTime(year, month, 1);
                var endOfMonth = startOfMonth.AddMonths(1);

                var transactions = await _context.PaymentTransactions
                    .Where(pt => !pt.IsDeleted && pt.CreatedAt >= startOfMonth && pt.CreatedAt < endOfMonth)
                    .ToListAsync();

                return new Models.Statistics.MonthlyPaymentStatistics
                {
                    Year = year,
                    Month = month,
                    TotalTransactions = transactions.Count,
                    TotalAmount = transactions.Sum(t => t.Amount),
                    SuccessfulTransactions = transactions.Count(t => t.Status == PaymentStatus.Success),
                    SuccessfulAmount = transactions.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount),
                    FailedTransactions = transactions.Count(t => t.Status == PaymentStatus.Failed),
                    FailedAmount = transactions.Where(t => t.Status == PaymentStatus.Failed).Sum(t => t.Amount),
                    PendingTransactions = transactions.Count(t => t.Status == PaymentStatus.Pending),
                    PendingAmount = transactions.Where(t => t.Status == PaymentStatus.Pending).Sum(t => t.Amount),
                    CanceledTransactions = transactions.Count(t => t.Status == PaymentStatus.Canceled),
                    CanceledAmount = transactions.Where(t => t.Status == PaymentStatus.Canceled).Sum(t => t.Amount),
                    CalculatedAt = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه آمار ماهانه تراکنش‌ها. سال: {Year}, ماه: {Month}", year, month);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int transactionId)
        {
            try
            {
                return await _context.PaymentTransactions
                    .AnyAsync(pt => !pt.IsDeleted && pt.PaymentTransactionId == transactionId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود تراکنش. شناسه: {TransactionId}", transactionId);
                throw;
            }
        }

        public async Task<bool> ExistsByTransactionIdAsync(string transactionId)
        {
            try
            {
                return await _context.PaymentTransactions
                    .AnyAsync(pt => !pt.IsDeleted && pt.TransactionId == transactionId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود تراکنش بر اساس شناسه تراکنش. شناسه: {TransactionId}", transactionId);
                throw;
            }
        }

        public async Task<int> GetCountAsync(PaymentTransactionSearchFilters filters = null)
        {
            try
            {
                var query = _context.PaymentTransactions.Where(pt => !pt.IsDeleted);

                if (filters != null)
                {
                    if (filters.PatientId.HasValue)
                        query = query.Where(pt => pt.Reception.PatientId == filters.PatientId.Value);

                    if (filters.ReceptionId.HasValue)
                        query = query.Where(pt => pt.ReceptionId == filters.ReceptionId.Value);

                    if (filters.PaymentMethod.HasValue)
                        query = query.Where(pt => pt.Method == filters.PaymentMethod.Value);

                    if (filters.Status.HasValue)
                        query = query.Where(pt => pt.Status == filters.Status.Value);

                    if (filters.MinAmount.HasValue)
                        query = query.Where(pt => pt.Amount >= filters.MinAmount.Value);

                    if (filters.MaxAmount.HasValue)
                        query = query.Where(pt => pt.Amount <= filters.MaxAmount.Value);

                    if (filters.StartDate.HasValue)
                        query = query.Where(pt => pt.CreatedAt >= filters.StartDate.Value);

                    if (filters.EndDate.HasValue)
                        query = query.Where(pt => pt.CreatedAt <= filters.EndDate.Value);

                    if (!string.IsNullOrEmpty(filters.TransactionId))
                        query = query.Where(pt => pt.TransactionId.Contains(filters.TransactionId));

                    if (!string.IsNullOrEmpty(filters.ReferenceCode))
                        query = query.Where(pt => pt.ReferenceCode.Contains(filters.ReferenceCode));
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش تراکنش‌ها");
                throw;
            }
        }

        #endregion
    }
}
