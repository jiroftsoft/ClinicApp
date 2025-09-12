using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.Payment
{
    /// <summary>
    /// Service Interface برای مدیریت پرداخت‌ها
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت منطق کسب‌وکار پرداخت‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل فرآیند پرداخت (نقدی، POS، آنلاین)
    /// 2. محاسبه خودکار مبالغ و کارمزدها
    /// 3. مدیریت وضعیت‌های مختلف پرداخت
    /// 4. پشتیبانی از برگشت و لغو پرداخت
    /// 5. بهینه‌سازی برای عملکرد بالا
    /// </summary>
    public interface IPaymentService
    {
        #region Payment Processing

        /// <summary>
        /// پردازش پرداخت نقدی
        /// </summary>
        /// <param name="request">درخواست پرداخت نقدی</param>
        /// <returns>نتیجه پردازش</returns>
        Task<ServiceResult<PaymentTransaction>> ProcessCashPaymentAsync(CashPaymentRequest request);

        /// <summary>
        /// پردازش پرداخت POS
        /// </summary>
        /// <param name="request">درخواست پرداخت POS</param>
        /// <returns>نتیجه پردازش</returns>
        Task<ServiceResult<PaymentTransaction>> ProcessPosPaymentAsync(PosPaymentRequest request);

        /// <summary>
        /// پردازش پرداخت آنلاین
        /// </summary>
        /// <param name="request">درخواست پرداخت آنلاین</param>
        /// <returns>نتیجه پردازش</returns>
        Task<ServiceResult<OnlinePayment>> ProcessOnlinePaymentAsync(OnlinePaymentRequest request);

        /// <summary>
        /// تکمیل پرداخت آنلاین
        /// </summary>
        /// <param name="callbackData">داده‌های Callback</param>
        /// <returns>نتیجه تکمیل</returns>
        Task<ServiceResult<PaymentTransaction>> CompleteOnlinePaymentAsync(OnlinePaymentCallback callbackData);

        #endregion

        #region Payment Management

        /// <summary>
        /// لغو پرداخت
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <param name="reason">دلیل لغو</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه لغو</returns>
        Task<ServiceResult> CancelPaymentAsync(int transactionId, string reason, string userId);

        /// <summary>
        /// برگشت پرداخت
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <param name="refundAmount">مبلغ برگشت</param>
        /// <param name="reason">دلیل برگشت</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه برگشت</returns>
        Task<ServiceResult<PaymentTransaction>> RefundPaymentAsync(int transactionId, decimal refundAmount, string reason, string userId);

        /// <summary>
        /// به‌روزرسانی وضعیت پرداخت
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <param name="status">وضعیت جدید</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult> UpdatePaymentStatusAsync(int transactionId, PaymentStatus status, string userId);

        #endregion

        #region Payment Calculation

        /// <summary>
        /// محاسبه مبلغ پرداخت
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="paymentMethod">روش پرداخت</param>
        /// <param name="discountAmount">مبلغ تخفیف</param>
        /// <returns>محاسبه مبلغ</returns>
        Task<ServiceResult<PaymentCalculation>> CalculatePaymentAsync(int receptionId, PaymentMethod paymentMethod, decimal discountAmount = 0);

        /// <summary>
        /// محاسبه کارمزد درگاه
        /// </summary>
        /// <param name="amount">مبلغ</param>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <returns>محاسبه کارمزد</returns>
        Task<ServiceResult<GatewayFeeCalculation>> CalculateGatewayFeeAsync(decimal amount, int gatewayId);

        /// <summary>
        /// محاسبه مبلغ خالص پس از کسر کارمزد
        /// </summary>
        /// <param name="amount">مبلغ</param>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <returns>محاسبه مبلغ خالص</returns>
        Task<ServiceResult<decimal>> CalculateNetAmountAsync(decimal amount, int gatewayId);

        #endregion

        #region Payment Validation

        /// <summary>
        /// اعتبارسنجی پرداخت
        /// </summary>
        /// <param name="request">درخواست پرداخت</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePaymentAsync(PaymentRequest request);

        /// <summary>
        /// بررسی امکان پرداخت
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="paymentMethod">روش پرداخت</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult> CanProcessPaymentAsync(int receptionId, PaymentMethod paymentMethod);

        /// <summary>
        /// بررسی وضعیت پرداخت
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>وضعیت پرداخت</returns>
        Task<ServiceResult<PaymentStatus>> GetPaymentStatusAsync(int transactionId);

        #endregion

        #region Payment Retrieval

        /// <summary>
        /// دریافت تراکنش پرداخت
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>تراکنش پرداخت</returns>
        Task<ServiceResult<PaymentTransaction>> GetPaymentTransactionAsync(int transactionId);

        /// <summary>
        /// دریافت تراکنش‌های پرداخت پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<ServiceResult<IEnumerable<PaymentTransaction>>> GetReceptionPaymentsAsync(int receptionId);

        /// <summary>
        /// دریافت تراکنش‌های پرداخت بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<ServiceResult<IEnumerable<PaymentTransaction>>> GetPatientPaymentsAsync(int patientId, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت تراکنش‌های پرداخت بر اساس تاریخ
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<ServiceResult<IEnumerable<PaymentTransaction>>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 50);

        #endregion

        #region Payment Statistics

        /// <summary>
        /// دریافت آمار پرداخت‌ها
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار پرداخت‌ها</returns>
        Task<ServiceResult<PaymentStatistics>> GetPaymentStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت آمار پرداخت‌ها با فیلترهای اضافی
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="paymentMethod">روش پرداخت</param>
        /// <param name="status">وضعیت پرداخت</param>
        /// <returns>آمار پرداخت‌ها</returns>
        Task<ServiceResult<PaymentStatistics>> GetPaymentStatisticsAsync(DateTime startDate, DateTime endDate, PaymentMethod? paymentMethod, PaymentStatus? status);
        Task<ServiceResult<PaymentStatistics>> GetPaymentStatisticsAsync(DateTime startDate, DateTime endDate, PaymentMethod? paymentMethod, PaymentStatus? status, int? patientId);

        /// <summary>
        /// دریافت آمار پرداخت‌های روزانه
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار روزانه</returns>
        Task<ServiceResult<DailyPaymentStatistics>> GetDailyPaymentStatisticsAsync(DateTime date);

        /// <summary>
        /// دریافت آمار پرداخت‌های ماهانه
        /// </summary>
        /// <param name="year">سال</param>
        /// <param name="month">ماه</param>
        /// <returns>آمار ماهانه</returns>
        Task<ServiceResult<MonthlyPaymentStatistics>> GetMonthlyPaymentStatisticsAsync(int year, int month);

        #endregion

        #region Payment Search

        /// <summary>
        /// جستجوی تراکنش‌های پرداخت
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<ServiceResult<IEnumerable<PaymentTransaction>>> SearchPaymentsAsync(string searchTerm, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// جستجوی پیشرفته تراکنش‌های پرداخت
        /// </summary>
        /// <param name="filters">فیلترهای جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<ServiceResult<IEnumerable<PaymentTransaction>>> AdvancedSearchPaymentsAsync(PaymentSearchFilters filters, int pageNumber = 1, int pageSize = 50);

        #endregion

        #region CRUD Operations

        /// <summary>
        /// دریافت لیست تراکنش‌های پرداخت
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<ServiceResult<PagedResult<PaymentTransaction>>> GetTransactionsAsync(int pageNumber = 1, int pageSize = 50);
        Task<ServiceResult<PagedResult<PaymentTransaction>>> GetTransactionsAsync(int? patientId, int? receptionId, int? appointmentId, PaymentMethod? method, PaymentStatus? status, decimal? minAmount, decimal? maxAmount, DateTime? startDate, DateTime? endDate, string? patientName, string? doctorName, string? transactionId, string? referenceCode, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت تراکنش پرداخت بر اساس شناسه
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>تراکنش پرداخت</returns>
        Task<ServiceResult<PaymentTransaction>> GetTransactionByIdAsync(int transactionId);

        /// <summary>
        /// ایجاد تراکنش پرداخت جدید
        /// </summary>
        /// <param name="transaction">تراکنش پرداخت</param>
        /// <returns>نتیجه ایجاد</returns>
        Task<ServiceResult<PaymentTransaction>> CreateTransactionAsync(PaymentTransaction transaction);

        /// <summary>
        /// به‌روزرسانی تراکنش پرداخت
        /// </summary>
        /// <param name="transaction">تراکنش پرداخت</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult<PaymentTransaction>> UpdateTransactionAsync(PaymentTransaction transaction);

        /// <summary>
        /// حذف تراکنش پرداخت
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه حذف</returns>
        Task<ServiceResult> DeleteTransactionAsync(int transactionId, string userId);

        #endregion
    }

    #region Request/Response Models

    /// <summary>
    /// درخواست پرداخت نقدی
    /// </summary>
    public class CashPaymentRequest
    {
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string CreatedByUserId { get; set; }
        public int CashSessionId { get; set; }
    }

    /// <summary>
    /// درخواست پرداخت POS
    /// </summary>
    public class PosPaymentRequest
    {
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public int PosTerminalId { get; set; }
        public string TransactionId { get; set; }
        public string ReferenceCode { get; set; }
        public string ReceiptNo { get; set; }
        public string Description { get; set; }
        public string CreatedByUserId { get; set; }
        public int CashSessionId { get; set; }
    }

    /// <summary>
    /// درخواست پرداخت آنلاین
    /// </summary>
    public class OnlinePaymentRequest
    {
        public int ReceptionId { get; set; }
        public int? AppointmentId { get; set; }
        public int PatientId { get; set; }
        public OnlinePaymentType PaymentType { get; set; }
        public decimal Amount { get; set; }
        public int PaymentGatewayId { get; set; }
        public string Description { get; set; }
        public string UserIpAddress { get; set; }
        public string UserAgent { get; set; }
        public string CreatedByUserId { get; set; }
    }

    /// <summary>
    /// داده‌های Callback پرداخت آنلاین
    /// </summary>
    public class OnlinePaymentCallback
    {
        public string PaymentToken { get; set; }
        public string GatewayTransactionId { get; set; }
        public string GatewayReferenceCode { get; set; }
        public OnlinePaymentStatus Status { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public decimal? GatewayFee { get; set; }
        public decimal? NetAmount { get; set; }
    }

    /// <summary>
    /// درخواست پرداخت عمومی
    /// </summary>
    public class PaymentRequest
    {
        public int ReceptionId { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string CreatedByUserId { get; set; }
    }

    /// <summary>
    /// محاسبه پرداخت
    /// </summary>
    public class PaymentCalculation
    {
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal GatewayFee { get; set; }
        public decimal NetAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// محاسبه کارمزد درگاه
    /// </summary>
    public class GatewayFeeCalculation
    {
        public decimal Amount { get; set; }
        public decimal FeePercentage { get; set; }
        public decimal FixedFee { get; set; }
        public decimal CalculatedFee { get; set; }
        public decimal NetAmount { get; set; }
        public int PaymentGatewayId { get; set; }
        public string GatewayName { get; set; }
    }

    /// <summary>
    /// فیلترهای جستجوی پرداخت
    /// </summary>
    public class PaymentSearchFilters
    {
        public int? ReceptionId { get; set; }
        public int? PatientId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PaymentStatus? Status { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TransactionId { get; set; }
        public string ReferenceCode { get; set; }
        public string ReceiptNo { get; set; }
        public string CreatedByUserId { get; set; }
    }

    /// <summary>
    /// آمار پرداخت‌ها
    /// </summary>
    public class PaymentStatistics
    {
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public int CanceledTransactions { get; set; }
        public decimal SuccessRate { get; set; }
        public Dictionary<PaymentMethod, int> TransactionsByMethod { get; set; }
        public Dictionary<PaymentStatus, int> TransactionsByStatus { get; set; }
        public decimal CashAmount { get; set; }
        public decimal PosAmount { get; set; }
        public decimal OnlineAmount { get; set; }
        public decimal DebtAmount { get; set; }
    }

    #endregion
}

