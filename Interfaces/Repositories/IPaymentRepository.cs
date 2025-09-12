using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Models.Statistics;

namespace ClinicApp.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Payment entity operations
    /// Production-ready with comprehensive payment management
    /// </summary>
    public interface IPaymentRepository
    {
        /// <summary>
        /// Add new payment transaction
        /// </summary>
        /// <param name="payment">Payment transaction entity</param>
        /// <returns>Created payment transaction with ID</returns>
        Task<PaymentTransaction> AddPaymentTransactionAsync(PaymentTransaction payment);

        /// <summary>
        /// Get all payments for a reception
        /// </summary>
        /// <param name="receptionId">Reception ID</param>
        /// <returns>List of payment transactions</returns>
        Task<List<PaymentTransaction>> GetReceptionPaymentsAsync(int receptionId);

        /// <summary>
        /// Get payment transaction by ID
        /// </summary>
        /// <param name="paymentId">Payment ID</param>
        /// <returns>Payment transaction or null if not found</returns>
        Task<PaymentTransaction> GetPaymentByIdAsync(int paymentId);

        /// <summary>
        /// Update payment transaction
        /// </summary>
        /// <param name="payment">Payment transaction entity</param>
        /// <returns>Updated payment transaction</returns>
        Task<PaymentTransaction> UpdatePaymentTransactionAsync(PaymentTransaction payment);

        /// <summary>
        /// Get total paid amount for reception
        /// </summary>
        /// <param name="receptionId">Reception ID</param>
        /// <returns>Total paid amount</returns>
        Task<decimal> GetReceptionTotalPaidAmountAsync(int receptionId);

        /// <summary>
        /// Get remaining amount for reception
        /// </summary>
        /// <param name="receptionId">Reception ID</param>
        /// <param name="totalAmount">Total reception amount</param>
        /// <returns>Remaining amount</returns>
        Task<decimal> GetReceptionRemainingAmountAsync(int receptionId, decimal totalAmount);

        /// <summary>
        /// Check if reception is fully paid
        /// </summary>
        /// <param name="receptionId">Reception ID</param>
        /// <param name="totalAmount">Total reception amount</param>
        /// <returns>True if fully paid</returns>
        Task<bool> IsReceptionFullyPaidAsync(int receptionId, decimal totalAmount);

        /// <summary>
        /// Get payment statistics for date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Payment statistics</returns>
        Task<PaymentStatistics> GetPaymentStatisticsAsync(DateTime startDate, DateTime endDate);
    }

    // PaymentStatistics به Models/Statistics منتقل شد
}
