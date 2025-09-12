using ClinicApp.Data;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

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
                    .FirstOrDefaultAsync(pt => !pt.IsDeleted && pt.Id == transactionId);
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
                _logger.Error(ex, "خطا در به‌روزرسانی تراکنش پرداخت. شناسه: {TransactionId}", transaction.Id);
                throw;
            }
        }

        public async Task<ServiceResult> SoftDeleteAsync(int transactionId, string deletedByUserId)
        {
            try
            {
                var transaction = await _context.PaymentTransactions
                    .FirstOrDefaultAsync(pt => !pt.IsDeleted && pt.Id == transactionId);

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
                    .Where(pt => !pt.IsDeleted && pt.PaymentMethod == paymentMethod)
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
                        (pt.ReferenceNumber.Contains(searchTerm) ||
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
                    .Where(pt => !pt.IsDeleted && pt.PaymentMethod == paymentMethod)
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
    }
}
