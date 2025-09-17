using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Interface برای Repository تعرفه‌های بیمه
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// </summary>
    public interface IInsuranceTariffRepository
    {
        #region CRUD Operations

        /// <summary>
        /// دریافت تعرفه بیمه بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه تعرفه بیمه</param>
        /// <returns>تعرفه بیمه مورد نظر</returns>
        Task<InsuranceTariff> GetByIdAsync(int id);

        /// <summary>
        /// دریافت تعرفه بیمه با جزئیات کامل
        /// </summary>
        /// <param name="id">شناسه تعرفه بیمه</param>
        /// <returns>تعرفه بیمه با جزئیات کامل</returns>
        Task<InsuranceTariff> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// دریافت تمام تعرفه‌های بیمه فعال
        /// </summary>
        /// <returns>لیست تعرفه‌های بیمه فعال</returns>
        Task<List<InsuranceTariff>> GetAllActiveAsync();

        /// <summary>
        /// دریافت تعرفه‌های بیمه با صفحه‌بندی
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه (اختیاری)</param>
        /// <param name="serviceId">شناسه خدمت (اختیاری)</param>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه (اختیاری)</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه صفحه‌بندی شده تعرفه‌های بیمه</returns>
        Task<PagedResult<InsuranceTariff>> GetPagedAsync(
            int? planId = null,
            int? serviceId = null,
            int? providerId = null,
            string searchTerm = "",
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// افزودن تعرفه بیمه جدید
        /// </summary>
        /// <param name="tariff">تعرفه بیمه جدید</param>
        /// <returns>تعرفه بیمه افزوده شده</returns>
        Task<InsuranceTariff> AddAsync(InsuranceTariff tariff);

        /// <summary>
        /// به‌روزرسانی تعرفه بیمه
        /// </summary>
        /// <param name="tariff">تعرفه بیمه برای به‌روزرسانی</param>
        /// <returns>تعرفه بیمه به‌روزرسانی شده</returns>
        Task<InsuranceTariff> UpdateAsync(InsuranceTariff tariff);

        /// <summary>
        /// حذف نرم تعرفه بیمه
        /// </summary>
        /// <param name="id">شناسه تعرفه بیمه</param>
        /// <param name="deletedByUserId">شناسه کاربر حذف‌کننده</param>
        /// <returns>نتیجه حذف</returns>
        Task<bool> SoftDeleteAsync(int id, string deletedByUserId);

        /// <summary>
        /// حذف تعرفه بیمه
        /// </summary>
        /// <param name="tariff">تعرفه بیمه برای حذف</param>
        void Delete(InsuranceTariff tariff);

        /// <summary>
        /// ذخیره تغییرات
        /// </summary>
        /// <returns>تعداد رکوردهای تغییر یافته</returns>
        Task<int> SaveChangesAsync();

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// دریافت تعرفه بیمه بر اساس طرح و خدمت
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>تعرفه بیمه مورد نظر</returns>
        Task<InsuranceTariff> GetByPlanAndServiceAsync(int planId, int serviceId);

        /// <summary>
        /// دریافت تعرفه‌های بیمه بر اساس طرح بیمه
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>لیست تعرفه‌های بیمه</returns>
        Task<List<InsuranceTariff>> GetByPlanIdAsync(int planId);

        /// <summary>
        /// دریافت تعرفه‌های بیمه بر اساس خدمت
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>لیست تعرفه‌های بیمه</returns>
        Task<List<InsuranceTariff>> GetByServiceIdAsync(int serviceId);

        /// <summary>
        /// دریافت تعرفه‌های بیمه بر اساس ارائه‌دهنده بیمه
        /// </summary>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه</param>
        /// <returns>لیست تعرفه‌های بیمه</returns>
        Task<List<InsuranceTariff>> GetByProviderIdAsync(int providerId);

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود تعرفه بیمه برای طرح و خدمت
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="excludeId">شناسه تعرفه برای حذف از جستجو (اختیاری)</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> DoesTariffExistAsync(int planId, int serviceId, int? excludeId = null);

        /// <summary>
        /// بررسی وجود تعرفه‌های بیمه برای طرح
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> HasTariffsAsync(int planId);

        /// <summary>
        /// بررسی وجود تعرفه‌های بیمه برای خدمت
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>نتیجه بررسی</returns>
    

        #endregion

        #region Statistics Operations

        /// <summary>
        /// دریافت تعداد کل تعرفه‌های بیمه
        /// </summary>
        /// <returns>تعداد کل تعرفه‌های بیمه</returns>
        Task<int> GetTotalCountAsync();

        /// <summary>
        /// دریافت آمار تعرفه‌های بیمه
        /// </summary>
        /// <returns>آمار تعرفه‌های بیمه</returns>
        Task<Dictionary<string, int>> GetStatisticsAsync();

        #endregion
    }
}
