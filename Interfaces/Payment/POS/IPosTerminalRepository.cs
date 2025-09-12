using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.Payment.POS
{
    /// <summary>
    /// Repository Interface برای مدیریت دستگاه‌های پوز
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت CRUD دستگاه‌های پوز
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل دستگاه‌های پوز بانکی
    /// 2. پشتیبانی از ارائه‌دهندگان مختلف (سامان کیش، آسان پرداخت و...)
    /// 3. مدیریت تنظیمات شبکه و پروتکل‌ها
    /// 4. پشتیبانی از Soft Delete برای حفظ اطلاعات
    /// 5. بهینه‌سازی برای عملکرد بالا
    /// </summary>
    public interface IPosTerminalRepository
    {
        #region CRUD Operations

        /// <summary>
        /// دریافت دستگاه پوز بر اساس شناسه
        /// </summary>
        /// <param name="terminalId">شناسه دستگاه</param>
        /// <returns>دستگاه پوز یا null</returns>
        Task<PosTerminal> GetByIdAsync(int terminalId);

        /// <summary>
        /// دریافت تمام دستگاه‌های پوز
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست دستگاه‌ها</returns>
        Task<IEnumerable<PosTerminal>> GetAllAsync(int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// افزودن دستگاه پوز جدید
        /// </summary>
        /// <param name="terminal">دستگاه پوز</param>
        /// <returns>دستگاه اضافه شده</returns>
        Task<PosTerminal> AddAsync(PosTerminal terminal);

        /// <summary>
        /// ایجاد دستگاه پوز جدید
        /// </summary>
        /// <param name="terminal">دستگاه پوز</param>
        /// <returns>دستگاه ایجاد شده</returns>
        Task<PosTerminal> CreateAsync(PosTerminal terminal);

        /// <summary>
        /// به‌روزرسانی دستگاه پوز
        /// </summary>
        /// <param name="terminal">دستگاه پوز</param>
        /// <returns>دستگاه به‌روزرسانی شده</returns>
        Task<PosTerminal> UpdateAsync(PosTerminal terminal);

        /// <summary>
        /// حذف نرم دستگاه پوز
        /// </summary>
        /// <param name="terminalId">شناسه دستگاه</param>
        /// <param name="deletedByUserId">شناسه کاربر حذف کننده</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> SoftDeleteAsync(int terminalId, string deletedByUserId);

        #endregion

        #region Query Operations

        /// <summary>
        /// دریافت دستگاه‌های پوز فعال
        /// </summary>
        /// <returns>لیست دستگاه‌های فعال</returns>
        Task<IEnumerable<PosTerminal>> GetActiveTerminalsAsync();

        /// <summary>
        /// دریافت دستگاه پوز پیش‌فرض
        /// </summary>
        /// <returns>دستگاه پیش‌فرض یا null</returns>
        Task<PosTerminal> GetDefaultTerminalAsync();

        /// <summary>
        /// دریافت دستگاه‌های پوز بر اساس ارائه‌دهنده
        /// </summary>
        /// <param name="provider">ارائه‌دهنده پوز</param>
        /// <returns>لیست دستگاه‌ها</returns>
        Task<IEnumerable<PosTerminal>> GetByProviderAsync(PosProviderType provider);

        /// <summary>
        /// دریافت دستگاه‌های پوز بر اساس پروتکل
        /// </summary>
        /// <param name="protocol">پروتکل ارتباطی</param>
        /// <returns>لیست دستگاه‌ها</returns>
        Task<IEnumerable<PosTerminal>> GetByProtocolAsync(PosProtocol protocol);

        /// <summary>
        /// دریافت دستگاه پوز بر اساس شماره ترمینال
        /// </summary>
        /// <param name="terminalId">شماره ترمینال</param>
        /// <returns>دستگاه پوز یا null</returns>
        Task<PosTerminal> GetByTerminalIdAsync(string terminalId);

        /// <summary>
        /// دریافت دستگاه پوز بر اساس شماره سریال
        /// </summary>
        /// <param name="serialNumber">شماره سریال</param>
        /// <returns>دستگاه پوز یا null</returns>
        Task<PosTerminal> GetBySerialNumberAsync(string serialNumber);

        /// <summary>
        /// دریافت دستگاه پوز بر اساس شماره پذیرنده
        /// </summary>
        /// <param name="merchantId">شماره پذیرنده</param>
        /// <returns>دستگاه پوز یا null</returns>
        Task<PosTerminal> GetByMerchantIdAsync(string merchantId);

        /// <summary>
        /// دریافت دستگاه پوز بر اساس آدرس IP
        /// </summary>
        /// <param name="ipAddress">آدرس IP</param>
        /// <returns>دستگاه پوز یا null</returns>
        Task<PosTerminal> GetByIpAddressAsync(string ipAddress);

        /// <summary>
        /// دریافت دستگاه پوز بر اساس آدرس MAC
        /// </summary>
        /// <param name="macAddress">آدرس MAC</param>
        /// <returns>دستگاه پوز یا null</returns>
        Task<PosTerminal> GetByMacAddressAsync(string macAddress);

        #endregion

        #region Search Operations

        /// <summary>
        /// جستجوی دستگاه‌های پوز
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست دستگاه‌ها</returns>
        Task<IEnumerable<PosTerminal>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// جستجوی پیشرفته دستگاه‌های پوز
        /// </summary>
        /// <param name="filters">فیلترهای جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست دستگاه‌ها</returns>
        Task<IEnumerable<PosTerminal>> AdvancedSearchAsync(PosTerminalSearchFilters filters, int pageNumber = 1, int pageSize = 50);

        #endregion

        #region Configuration Operations

        /// <summary>
        /// تنظیم دستگاه پوز به عنوان پیش‌فرض
        /// </summary>
        /// <param name="terminalId">شناسه دستگاه</param>
        /// <param name="updatedByUserId">شناسه کاربر به‌روزرسانی کننده</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> SetAsDefaultAsync(int terminalId, string updatedByUserId);

        /// <summary>
        /// پاک کردن وضعیت پیش‌فرض از تمام دستگاه‌های پوز
        /// </summary>
        /// <param name="updatedByUserId">شناسه کاربر به‌روزرسانی کننده</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> ClearDefaultTerminalsAsync(string updatedByUserId);

        /// <summary>
        /// فعال/غیرفعال کردن دستگاه پوز
        /// </summary>
        /// <param name="terminalId">شناسه دستگاه</param>
        /// <param name="isActive">وضعیت فعال</param>
        /// <param name="updatedByUserId">شناسه کاربر به‌روزرسانی کننده</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> SetActiveStatusAsync(int terminalId, bool isActive, string updatedByUserId);

        /// <summary>
        /// به‌روزرسانی تنظیمات شبکه دستگاه پوز
        /// </summary>
        /// <param name="terminalId">شناسه دستگاه</param>
        /// <param name="ipAddress">آدرس IP جدید</param>
        /// <param name="macAddress">آدرس MAC جدید</param>
        /// <param name="port">پورت جدید</param>
        /// <param name="updatedByUserId">شناسه کاربر به‌روزرسانی کننده</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> UpdateNetworkSettingsAsync(int terminalId, string ipAddress, string macAddress, int? port, string updatedByUserId);

        /// <summary>
        /// به‌روزرسانی تنظیمات پروتکل دستگاه پوز
        /// </summary>
        /// <param name="terminalId">شناسه دستگاه</param>
        /// <param name="protocol">پروتکل جدید</param>
        /// <param name="port">پورت جدید</param>
        /// <param name="updatedByUserId">شناسه کاربر به‌روزرسانی کننده</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> UpdateProtocolSettingsAsync(int terminalId, PosProtocol protocol, int? port, string updatedByUserId);

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود دستگاه پوز
        /// </summary>
        /// <param name="terminalId">شناسه دستگاه</param>
        /// <returns>true اگر وجود دارد</returns>
        Task<bool> ExistsAsync(int terminalId);

        /// <summary>
        /// بررسی وجود دستگاه پوز بر اساس شماره ترمینال
        /// </summary>
        /// <param name="terminalId">شماره ترمینال</param>
        /// <returns>true اگر وجود دارد</returns>
        Task<bool> ExistsByTerminalIdAsync(string terminalId);

        /// <summary>
        /// بررسی وجود دستگاه پوز بر اساس شماره پذیرنده
        /// </summary>
        /// <param name="merchantId">شماره پذیرنده</param>
        /// <returns>true اگر وجود دارد</returns>
        Task<bool> ExistsByMerchantIdAsync(string merchantId);

        /// <summary>
        /// بررسی یکتایی شماره ترمینال
        /// </summary>
        /// <param name="terminalId">شماره ترمینال</param>
        /// <param name="excludeId">شناسه دستگاه برای حذف از بررسی</param>
        /// <returns>true اگر یکتا است</returns>
        Task<bool> IsTerminalIdUniqueAsync(string terminalId, int? excludeId = null);

        /// <summary>
        /// بررسی یکتایی شماره پذیرنده
        /// </summary>
        /// <param name="merchantId">شماره پذیرنده</param>
        /// <param name="excludeId">شناسه دستگاه برای حذف از بررسی</param>
        /// <returns>true اگر یکتا است</returns>
        Task<bool> IsMerchantIdUniqueAsync(string merchantId, int? excludeId = null);

        /// <summary>
        /// بررسی یکتایی آدرس IP
        /// </summary>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="excludeId">شناسه دستگاه برای حذف از بررسی</param>
        /// <returns>true اگر یکتا است</returns>
        Task<bool> IsIpAddressUniqueAsync(string ipAddress, int? excludeId = null);

        /// <summary>
        /// بررسی یکتایی آدرس MAC
        /// </summary>
        /// <param name="macAddress">آدرس MAC</param>
        /// <param name="excludeId">شناسه دستگاه برای حذف از بررسی</param>
        /// <returns>true اگر یکتا است</returns>
        Task<bool> IsMacAddressUniqueAsync(string macAddress, int? excludeId = null);

        /// <summary>
        /// دریافت تعداد دستگاه‌های پوز
        /// </summary>
        /// <param name="filters">فیلترهای شمارش</param>
        /// <returns>تعداد دستگاه‌ها</returns>
        Task<int> GetCountAsync(PosTerminalSearchFilters filters = null);

        #endregion

        #region Statistics Operations

        /// <summary>
        /// دریافت آمار دستگاه‌های پوز
        /// </summary>
        /// <returns>آمار دستگاه‌ها</returns>
        Task<PosTerminalStatistics> GetStatisticsAsync();

        /// <summary>
        /// دریافت آمار استفاده از دستگاه‌های پوز
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار استفاده</returns>
        Task<PosTerminalUsageStatistics> GetUsageStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت آمار استفاده از دستگاه پوز
        /// </summary>
        /// <param name="terminalId">شناسه دستگاه</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار استفاده</returns>
        Task<PosTerminalUsageInfo> GetTerminalUsageAsync(int terminalId, DateTime startDate, DateTime endDate);

        #endregion
    }

    /// <summary>
    /// فیلترهای جستجوی دستگاه‌های پوز
    /// </summary>
    public class PosTerminalSearchFilters
    {
        public PosProviderType? Provider { get; set; }
        public PosProtocol? Protocol { get; set; }
        public bool? IsActive { get; set; }
        public string Title { get; set; }
        public string TerminalId { get; set; }
        public string MerchantId { get; set; }
        public string SerialNumber { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public int? Port { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public string CreatedByUserId { get; set; }
        public bool? IsDeleted { get; set; }
    }

    /// <summary>
    /// آمار دستگاه‌های پوز
    /// </summary>
 

    /// <summary>
    /// آمار استفاده از دستگاه‌های پوز
    /// </summary>
    public class PosTerminalUsageStatistics
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public Dictionary<int, PosTerminalUsageInfo> TerminalUsage { get; set; }
    }

    /// <summary>
    /// اطلاعات استفاده از دستگاه پوز
    /// </summary>
    public class PosTerminalUsageInfo
    {
        public int TerminalId { get; set; }
        public string TerminalTitle { get; set; }
        public PosProviderType Provider { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageAmount { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public DateTime? LastTransactionDate { get; set; }
    }
}
