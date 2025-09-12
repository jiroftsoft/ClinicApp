using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces.Payment.Gateway
{
    /// <summary>
    /// رابط مخزن درگاه‌های پرداخت
    /// </summary>
    public interface IPaymentGatewayRepository
    {
        #region CRUD Operations
        /// <summary>
        /// دریافت درگاه پرداخت بر اساس شناسه
        /// </summary>
        Task<PaymentGateway> GetByIdAsync(int gatewayId);

        /// <summary>
        /// دریافت تمام درگاه‌های پرداخت با صفحه‌بندی
        /// </summary>
        Task<IEnumerable<PaymentGateway>> GetAllAsync(int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// ایجاد درگاه پرداخت جدید
        /// </summary>
        Task<PaymentGateway> CreateAsync(PaymentGateway gateway);

        /// <summary>
        /// ایجاد درگاه پرداخت جدید
        /// </summary>
        Task<PaymentGateway> AddAsync(PaymentGateway gateway);

        /// <summary>
        /// به‌روزرسانی درگاه پرداخت
        /// </summary>
        Task<PaymentGateway> UpdateAsync(PaymentGateway gateway);

        /// <summary>
        /// حذف نرم درگاه پرداخت
        /// </summary>
        Task<ServiceResult> SoftDeleteAsync(int gatewayId, string deletedByUserId);
        #endregion

        #region Query Operations
        /// <summary>
        /// دریافت درگاه‌های فعال
        /// </summary>
        Task<IEnumerable<PaymentGateway>> GetActiveGatewaysAsync();

        /// <summary>
        /// دریافت درگاه‌های پیش‌فرض
        /// </summary>
        Task<IEnumerable<PaymentGateway>> GetDefaultGatewaysAsync();

        /// <summary>
        /// دریافت درگاه‌ها بر اساس نوع
        /// </summary>
        Task<IEnumerable<PaymentGateway>> GetByTypeAsync(PaymentGatewayType gatewayType);

        /// <summary>
        /// دریافت درگاه پرداخت بر اساس MerchantId
        /// </summary>
        Task<PaymentGateway> GetByMerchantIdAsync(string merchantId);

        /// <summary>
        /// جستجو در درگاه‌های پرداخت
        /// </summary>
        Task<IEnumerable<PaymentGateway>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 50);
        #endregion

        #region Management Operations
        /// <summary>
        /// پاک کردن تمام درگاه‌های پیش‌فرض
        /// </summary>
        Task ClearDefaultGatewaysAsync();

        /// <summary>
        /// تنظیم درگاه به عنوان پیش‌فرض
        /// </summary>
        Task SetAsDefaultAsync(int gatewayId);

        /// <summary>
        /// فعال/غیرفعال کردن درگاه
        /// </summary>
        Task ToggleStatusAsync(int gatewayId, bool isActive);
        #endregion

        #region Validation Operations
        /// <summary>
        /// بررسی وجود درگاه
        /// </summary>
        Task<bool> ExistsAsync(int gatewayId);

        /// <summary>
        /// بررسی یکتا بودن نام درگاه
        /// </summary>
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);

        /// <summary>
        /// شمارش کل درگاه‌ها
        /// </summary>
        Task<int> GetCountAsync();
        #endregion
    }
}
