using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Models.Statistics;

namespace ClinicApp.Interfaces.Payment
{
    /// <summary>
    /// Repository Interface برای مدیریت تراکنش‌های پرداخت
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت CRUD تراکنش‌های پرداخت
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل تراکنش‌های پرداخت (POS، آنلاین، نقدی)
    /// 2. پشتیبانی از Soft Delete برای حفظ اطلاعات مالی
    /// 3. بهینه‌سازی برای عملکرد بالا (10k+ تراکنش)
    /// 4. پشتیبانی از فیلترهای پیشرفته
    /// 5. ردیابی کامل Audit Trail
    /// </summary>
    public interface IPaymentTransactionRepository
    {
        #region CRUD Operations

        /// <summary>
        /// دریافت تراکنش پرداخت بر اساس شناسه
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>تراکنش پرداخت یا null</returns>
        Task<PaymentTransaction> GetByIdAsync(int transactionId);

        /// <summary>
        /// دریافت تمام تراکنش‌های پرداخت
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<IEnumerable<PaymentTransaction>> GetAllAsync(int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// افزودن تراکنش پرداخت جدید
        /// </summary>
        /// <param name="transaction">تراکنش پرداخت</param>
        /// <returns>تراکنش اضافه شده</returns>
        Task<PaymentTransaction> AddAsync(PaymentTransaction transaction);

        /// <summary>
        /// ایجاد تراکنش پرداخت جدید
        /// </summary>
        /// <param name="transaction">تراکنش پرداخت</param>
        /// <returns>تراکنش ایجاد شده</returns>
        Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction);

        /// <summary>
        /// به‌روزرسانی تراکنش پرداخت
        /// </summary>
        /// <param name="transaction">تراکنش پرداخت</param>
        /// <returns>تراکنش به‌روزرسانی شده</returns>
        Task<PaymentTransaction> UpdateAsync(PaymentTransaction transaction);

        /// <summary>
        /// حذف نرم تراکنش پرداخت
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <param name="deletedByUserId">شناسه کاربر حذف کننده</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> SoftDeleteAsync(int transactionId, string deletedByUserId);

        #endregion

        #region Query Operations

        /// <summary>
        /// دریافت تراکنش‌های پرداخت بر اساس پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<IEnumerable<PaymentTransaction>> GetByReceptionIdAsync(int receptionId);

        /// <summary>
        /// دریافت تراکنش‌های پرداخت بر اساس بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<IEnumerable<PaymentTransaction>> GetByPatientIdAsync(int patientId, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت تراکنش‌های پرداخت بر اساس روش پرداخت
        /// </summary>
        /// <param name="paymentMethod">روش پرداخت</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<IEnumerable<PaymentTransaction>> GetByPaymentMethodAsync(PaymentMethod paymentMethod, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت تراکنش‌های پرداخت بر اساس وضعیت
        /// </summary>
        /// <param name="status">وضعیت تراکنش</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<IEnumerable<PaymentTransaction>> GetByStatusAsync(PaymentStatus status, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت تعداد تراکنش‌های پرداخت بر اساس وضعیت
        /// </summary>
        /// <param name="status">وضعیت تراکنش</param>
        /// <returns>تعداد تراکنش‌ها</returns>
        Task<int> GetTransactionCountByStatusAsync(PaymentStatus status);

        /// <summary>
        /// دریافت تراکنش‌های پرداخت بر اساس تاریخ
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<IEnumerable<PaymentTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت تراکنش‌های پرداخت بر اساس مبلغ
        /// </summary>
        /// <param name="minAmount">حداقل مبلغ</param>
        /// <param name="maxAmount">حداکثر مبلغ</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<IEnumerable<PaymentTransaction>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50);

        #endregion

        #region Search Operations

        /// <summary>
        /// جستجوی تراکنش‌های پرداخت
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<IEnumerable<PaymentTransaction>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// جستجوی پیشرفته تراکنش‌های پرداخت
        /// </summary>
        /// <param name="filters">فیلترهای جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تراکنش‌ها</returns>
        Task<IEnumerable<PaymentTransaction>> AdvancedSearchAsync(PaymentTransactionSearchFilters filters, int pageNumber = 1, int pageSize = 50);

        #endregion

        #region Statistics Operations

        /// <summary>
        /// دریافت آمار تراکنش‌های پرداخت
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار تراکنش‌ها</returns>
        Task<Models.Statistics.PaymentTransactionStatistics> GetStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت آمار تراکنش‌های پرداخت بر اساس روش پرداخت
        /// </summary>
        /// <param name="paymentMethod">روش پرداخت</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار تراکنش‌ها</returns>
        Task<Models.Statistics.PaymentTransactionStatistics> GetStatisticsByPaymentMethodAsync(PaymentMethod paymentMethod, DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت آمار تراکنش‌های پرداخت روزانه
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار روزانه</returns>
        Task<Models.Statistics.DailyPaymentStatistics> GetDailyStatisticsAsync(DateTime date);

        /// <summary>
        /// دریافت آمار تراکنش‌های پرداخت ماهانه
        /// </summary>
        /// <param name="year">سال</param>
        /// <param name="month">ماه</param>
        /// <returns>آمار ماهانه</returns>
        Task<Models.Statistics.MonthlyPaymentStatistics> GetMonthlyStatisticsAsync(int year, int month);

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود تراکنش پرداخت
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>true اگر وجود دارد</returns>
        Task<bool> ExistsAsync(int transactionId);

        /// <summary>
        /// بررسی وجود تراکنش پرداخت بر اساس شماره تراکنش
        /// </summary>
        /// <param name="transactionId">شماره تراکنش</param>
        /// <returns>true اگر وجود دارد</returns>
        Task<bool> ExistsByTransactionIdAsync(string transactionId);

        /// <summary>
        /// دریافت تعداد تراکنش‌های پرداخت
        /// </summary>
        /// <param name="filters">فیلترهای شمارش</param>
        /// <returns>تعداد تراکنش‌ها</returns>
        Task<int> GetCountAsync(PaymentTransactionSearchFilters filters = null);

        #endregion
    }

    /// <summary>
    /// فیلترهای جستجوی تراکنش‌های پرداخت
    /// </summary>
    public class PaymentTransactionSearchFilters
    {
        public int? ReceptionId { get; set; }
        public int? PatientId { get; set; }
        public int? PosTerminalId { get; set; }
        public int? PaymentGatewayId { get; set; }
        public int? OnlinePaymentId { get; set; }
        public int? CashSessionId { get; set; }
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
        public bool? IsDeleted { get; set; }
    }


}
