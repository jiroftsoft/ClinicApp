using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.Payment
{
    /// <summary>
    /// Repository Interface برای مدیریت پرداخت‌های آنلاین
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت CRUD پرداخت‌های آنلاین
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل پرداخت‌های آنلاین (نوبت‌ها، پذیرش‌ها، خدمات)
    /// 2. پشتیبانی از Callback و Webhook
    /// 3. مدیریت وضعیت‌های مختلف پرداخت
    /// 4. پشتیبانی از Soft Delete برای حفظ اطلاعات مالی
    /// 5. بهینه‌سازی برای عملکرد بالا
    /// </summary>
    public interface IOnlinePaymentRepository
    {
        #region CRUD Operations

        /// <summary>
        /// دریافت پرداخت آنلاین بر اساس شناسه
        /// </summary>
        /// <param name="onlinePaymentId">شناسه پرداخت آنلاین</param>
        /// <returns>پرداخت آنلاین یا null</returns>
        Task<OnlinePayment> GetByIdAsync(int onlinePaymentId);

        /// <summary>
        /// دریافت تمام پرداخت‌های آنلاین
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پرداخت‌های آنلاین</returns>
        Task<IEnumerable<OnlinePayment>> GetAllAsync(int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// افزودن پرداخت آنلاین جدید
        /// </summary>
        /// <param name="onlinePayment">پرداخت آنلاین</param>
        /// <returns>پرداخت اضافه شده</returns>
        Task<OnlinePayment> AddAsync(OnlinePayment onlinePayment);

        /// <summary>
        /// ایجاد پرداخت آنلاین جدید
        /// </summary>
        /// <param name="onlinePayment">پرداخت آنلاین</param>
        /// <returns>پرداخت ایجاد شده</returns>
        Task<OnlinePayment> CreateAsync(OnlinePayment onlinePayment);

        /// <summary>
        /// به‌روزرسانی پرداخت آنلاین
        /// </summary>
        /// <param name="onlinePayment">پرداخت آنلاین</param>
        /// <returns>پرداخت به‌روزرسانی شده</returns>
        Task<OnlinePayment> UpdateAsync(OnlinePayment onlinePayment);

        /// <summary>
        /// حذف نرم پرداخت آنلاین
        /// </summary>
        /// <param name="onlinePaymentId">شناسه پرداخت آنلاین</param>
        /// <param name="deletedByUserId">شناسه کاربر حذف کننده</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> SoftDeleteAsync(int onlinePaymentId, string deletedByUserId);

        #endregion

        #region Query Operations

        /// <summary>
        /// دریافت پرداخت‌های آنلاین بر اساس درگاه پرداخت
        /// </summary>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پرداخت‌ها</returns>
        Task<IEnumerable<OnlinePayment>> GetByGatewayIdAsync(int gatewayId, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت پرداخت‌های آنلاین بر اساس بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پرداخت‌ها</returns>
        Task<IEnumerable<OnlinePayment>> GetByPatientIdAsync(int patientId, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت پرداخت‌های آنلاین بر اساس پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>لیست پرداخت‌ها</returns>
        Task<IEnumerable<OnlinePayment>> GetByReceptionIdAsync(int receptionId);

        /// <summary>
        /// دریافت پرداخت‌های آنلاین بر اساس نوبت
        /// </summary>
        /// <param name="appointmentId">شناسه نوبت</param>
        /// <returns>لیست پرداخت‌ها</returns>
        Task<IEnumerable<OnlinePayment>> GetByAppointmentIdAsync(int appointmentId);

        /// <summary>
        /// دریافت پرداخت‌های آنلاین بر اساس نوع پرداخت
        /// </summary>
        /// <param name="paymentType">نوع پرداخت</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پرداخت‌ها</returns>
        Task<IEnumerable<OnlinePayment>> GetByPaymentTypeAsync(OnlinePaymentType paymentType, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت پرداخت‌های آنلاین بر اساس وضعیت
        /// </summary>
        /// <param name="status">وضعیت پرداخت</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پرداخت‌ها</returns>
        Task<IEnumerable<OnlinePayment>> GetByStatusAsync(OnlinePaymentStatus status, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت پرداخت‌های آنلاین بر اساس تاریخ
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پرداخت‌ها</returns>
        Task<IEnumerable<OnlinePayment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// دریافت پرداخت‌های آنلاین منقضی شده
        /// </summary>
        /// <param name="currentDate">تاریخ جاری</param>
        /// <returns>لیست پرداخت‌های منقضی</returns>
        Task<IEnumerable<OnlinePayment>> GetExpiredPaymentsAsync(DateTime currentDate);

        /// <summary>
        /// دریافت پرداخت‌های آنلاین در انتظار
        /// </summary>
        /// <param name="olderThan">پرداخت‌های قدیمی‌تر از این تاریخ</param>
        /// <returns>لیست پرداخت‌های در انتظار</returns>
        Task<IEnumerable<OnlinePayment>> GetPendingPaymentsAsync(DateTime olderThan);

        #endregion

        #region Search Operations

        /// <summary>
        /// جستجوی پرداخت‌های آنلاین
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پرداخت‌ها</returns>
        Task<IEnumerable<OnlinePayment>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// جستجوی پیشرفته پرداخت‌های آنلاین
        /// </summary>
        /// <param name="filters">فیلترهای جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پرداخت‌ها</returns>
        Task<IEnumerable<OnlinePayment>> AdvancedSearchAsync(OnlinePaymentSearchFilters filters, int pageNumber = 1, int pageSize = 50);

        #endregion

        #region Transaction Operations

        /// <summary>
        /// دریافت پرداخت آنلاین بر اساس شماره تراکنش درگاه
        /// </summary>
        /// <param name="gatewayTransactionId">شماره تراکنش درگاه</param>
        /// <returns>پرداخت آنلاین یا null</returns>
        Task<OnlinePayment> GetByGatewayTransactionIdAsync(string gatewayTransactionId);

        /// <summary>
        /// دریافت پرداخت آنلاین بر اساس شماره تراکنش داخلی
        /// </summary>
        /// <param name="internalTransactionId">شماره تراکنش داخلی</param>
        /// <returns>پرداخت آنلاین یا null</returns>
        Task<OnlinePayment> GetByInternalTransactionIdAsync(string internalTransactionId);

        /// <summary>
        /// دریافت پرداخت آنلاین بر اساس توکن پرداخت
        /// </summary>
        /// <param name="paymentToken">توکن پرداخت</param>
        /// <returns>پرداخت آنلاین یا null</returns>
        Task<OnlinePayment> GetByPaymentTokenAsync(string paymentToken);

        /// <summary>
        /// به‌روزرسانی وضعیت پرداخت آنلاین
        /// </summary>
        /// <param name="onlinePaymentId">شناسه پرداخت آنلاین</param>
        /// <param name="status">وضعیت جدید</param>
        /// <param name="gatewayTransactionId">شماره تراکنش درگاه</param>
        /// <param name="gatewayReferenceCode">شماره مرجع درگاه</param>
        /// <param name="errorCode">کد خطا</param>
        /// <param name="errorMessage">پیام خطا</param>
        /// <param name="updatedByUserId">شناسه کاربر به‌روزرسانی کننده</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> UpdateStatusAsync(int onlinePaymentId, OnlinePaymentStatus status, string gatewayTransactionId = null, string gatewayReferenceCode = null, string errorCode = null, string errorMessage = null, string updatedByUserId = null);

        /// <summary>
        /// تکمیل پرداخت آنلاین
        /// </summary>
        /// <param name="onlinePaymentId">شناسه پرداخت آنلاین</param>
        /// <param name="gatewayTransactionId">شماره تراکنش درگاه</param>
        /// <param name="gatewayReferenceCode">شماره مرجع درگاه</param>
        /// <param name="gatewayFee">کارمزد درگاه</param>
        /// <param name="netAmount">مبلغ خالص</param>
        /// <param name="updatedByUserId">شناسه کاربر به‌روزرسانی کننده</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> CompletePaymentAsync(int onlinePaymentId, string gatewayTransactionId, string gatewayReferenceCode, decimal? gatewayFee = null, decimal? netAmount = null, string updatedByUserId = null);

        /// <summary>
        /// لغو پرداخت آنلاین
        /// </summary>
        /// <param name="onlinePaymentId">شناسه پرداخت آنلاین</param>
        /// <param name="errorCode">کد خطا</param>
        /// <param name="errorMessage">پیام خطا</param>
        /// <param name="updatedByUserId">شناسه کاربر به‌روزرسانی کننده</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> CancelPaymentAsync(int onlinePaymentId, string errorCode = null, string errorMessage = null, string updatedByUserId = null);

        /// <summary>
        /// برگشت پرداخت آنلاین
        /// </summary>
        /// <param name="onlinePaymentId">شناسه پرداخت آنلاین</param>
        /// <param name="refundAmount">مبلغ برگشت</param>
        /// <param name="refundReason">دلیل برگشت</param>
        /// <param name="updatedByUserId">شناسه کاربر به‌روزرسانی کننده</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> RefundPaymentAsync(int onlinePaymentId, decimal refundAmount, string refundReason, string updatedByUserId);

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود پرداخت آنلاین
        /// </summary>
        /// <param name="onlinePaymentId">شناسه پرداخت آنلاین</param>
        /// <returns>true اگر وجود دارد</returns>
        Task<bool> ExistsAsync(int onlinePaymentId);

        /// <summary>
        /// بررسی وجود پرداخت آنلاین بر اساس شماره تراکنش درگاه
        /// </summary>
        /// <param name="gatewayTransactionId">شماره تراکنش درگاه</param>
        /// <returns>true اگر وجود دارد</returns>
        Task<bool> ExistsByGatewayTransactionIdAsync(string gatewayTransactionId);

        /// <summary>
        /// بررسی وجود پرداخت آنلاین بر اساس شماره تراکنش داخلی
        /// </summary>
        /// <param name="internalTransactionId">شماره تراکنش داخلی</param>
        /// <returns>true اگر وجود دارد</returns>
        Task<bool> ExistsByInternalTransactionIdAsync(string internalTransactionId);

        /// <summary>
        /// دریافت تعداد پرداخت‌های آنلاین
        /// </summary>
        /// <param name="filters">فیلترهای شمارش</param>
        /// <returns>تعداد پرداخت‌ها</returns>
        Task<int> GetCountAsync(OnlinePaymentSearchFilters filters = null);

        #endregion

        #region Statistics Operations

        /// <summary>
        /// دریافت آمار پرداخت‌های آنلاین
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار پرداخت‌ها</returns>
        Task<OnlinePaymentStatistics> GetStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت آمار پرداخت‌های آنلاین بر اساس درگاه
        /// </summary>
        /// <param name="gatewayId">شناسه درگاه</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار پرداخت‌ها</returns>
        Task<OnlinePaymentStatistics> GetStatisticsByGatewayAsync(int gatewayId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت آمار پرداخت‌های آنلاین بر اساس نوع پرداخت
        /// </summary>
        /// <param name="paymentType">نوع پرداخت</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار پرداخت‌ها</returns>
        Task<OnlinePaymentStatistics> GetStatisticsByPaymentTypeAsync(OnlinePaymentType paymentType, DateTime startDate, DateTime endDate);

        #endregion
    }

    /// <summary>
    /// فیلترهای جستجوی پرداخت‌های آنلاین
    /// </summary>
    public class OnlinePaymentSearchFilters
    {
        public int? PaymentGatewayId { get; set; }
        public int? ReceptionId { get; set; }
        public int? AppointmentId { get; set; }
        public int? PatientId { get; set; }
        public OnlinePaymentType? PaymentType { get; set; }
        public OnlinePaymentStatus? Status { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string GatewayTransactionId { get; set; }
        public string InternalTransactionId { get; set; }
        public string PaymentToken { get; set; }
        public string ErrorCode { get; set; }
        public bool? IsRefunded { get; set; }
        public string CreatedByUserId { get; set; }
        public bool? IsDeleted { get; set; }
    }

    /// <summary>
    /// آمار پرداخت‌های آنلاین
    /// </summary>
    public class OnlinePaymentStatistics
    {
        public int TotalPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public int PendingPayments { get; set; }
        public int CanceledPayments { get; set; }
        public int RefundedPayments { get; set; }
        public int ExpiredPayments { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal RefundRate { get; set; }
        public decimal TotalGatewayFees { get; set; }
        public decimal TotalNetAmount { get; set; }
        public Dictionary<OnlinePaymentType, int> PaymentsByType { get; set; }
        public Dictionary<OnlinePaymentStatus, int> PaymentsByStatus { get; set; }
        public Dictionary<int, GatewayPaymentInfo> PaymentsByGateway { get; set; }
    }

    /// <summary>
    /// اطلاعات پرداخت‌های درگاه
    /// </summary>
    public class GatewayPaymentInfo
    {
        public int GatewayId { get; set; }
        public string GatewayName { get; set; }
        public PaymentGatewayType GatewayType { get; set; }
        public int PaymentCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal TotalGatewayFees { get; set; }
    }
}
