using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.ViewModels.Insurance.InsuranceTariff;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Interface برای سرویس تعرفه‌های بیمه
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// </summary>
    public interface IInsuranceTariffService
    {
        #region CRUD Operations

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
        Task<ServiceResult<PagedResult<InsuranceTariffIndexViewModel>>> GetTariffsAsync(
            int? planId = null,
            int? serviceId = null,
            int? providerId = null,
            string searchTerm = "",
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// دریافت جزئیات تعرفه بیمه
        /// </summary>
        /// <param name="id">شناسه تعرفه بیمه</param>
        /// <returns>جزئیات تعرفه بیمه</returns>
        Task<ServiceResult<InsuranceTariffDetailsViewModel>> GetTariffDetailsAsync(int id);

        /// <summary>
        /// دریافت تعرفه بیمه بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه تعرفه بیمه</param>
        /// <returns>تعرفه بیمه</returns>
        Task<ServiceResult<InsuranceTariffDetailsViewModel>> GetTariffByIdAsync(int id);

        /// <summary>
        /// دریافت تعرفه بیمه برای ویرایش
        /// </summary>
        /// <param name="id">شناسه تعرفه بیمه</param>
        /// <returns>تعرفه بیمه برای ویرایش</returns>
        Task<ServiceResult<InsuranceTariffCreateEditViewModel>> GetTariffForEditAsync(int id);

        /// <summary>
        /// ایجاد تعرفه بیمه جدید
        /// </summary>
        /// <param name="model">مدل ایجاد تعرفه بیمه</param>
        /// <returns>نتیجه ایجاد</returns>
        Task<ServiceResult<int>> CreateTariffAsync(InsuranceTariffCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی تعرفه بیمه
        /// </summary>
        /// <param name="model">مدل به‌روزرسانی تعرفه بیمه</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult> UpdateTariffAsync(InsuranceTariffCreateEditViewModel model);

        /// <summary>
        /// حذف نرم تعرفه بیمه
        /// </summary>
        /// <param name="id">شناسه تعرفه بیمه</param>
        /// <returns>نتیجه حذف</returns>
        Task<ServiceResult> SoftDeleteTariffAsync(int id);

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// دریافت تعرفه بیمه بر اساس طرح و خدمت
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>تعرفه بیمه مورد نظر</returns>
        Task<ServiceResult<InsuranceTariff>> GetTariffByPlanAndServiceAsync(int planId, int serviceId);

        /// <summary>
        /// دریافت تعرفه‌های بیمه بر اساس طرح بیمه
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>لیست تعرفه‌های بیمه</returns>
        Task<ServiceResult<List<InsuranceTariff>>> GetTariffsByPlanIdAsync(int planId);

        /// <summary>
        /// دریافت تعرفه‌های بیمه بر اساس خدمت
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>لیست تعرفه‌های بیمه</returns>
        Task<ServiceResult<List<InsuranceTariff>>> GetTariffsByServiceIdAsync(int serviceId);

        #endregion

        #region Validation Operations

        /// <summary>
        /// اعتبارسنجی تعرفه بیمه
        /// </summary>
        /// <param name="model">مدل تعرفه بیمه</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult<Dictionary<string, string>>> ValidateTariffAsync(InsuranceTariffCreateEditViewModel model);

        /// <summary>
        /// بررسی وجود تعرفه بیمه
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="excludeId">شناسه تعرفه برای حذف از جستجو (اختیاری)</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult<bool>> DoesTariffExistAsync(int planId, int serviceId, int? excludeId = null);

        /// <summary>
        /// بررسی وجود تعرفه بیمه (نام متد جایگزین)
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="excludeId">شناسه تعرفه برای حذف از جستجو (اختیاری)</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult<bool>> CheckTariffExistsAsync(int planId, int serviceId, int? excludeId = null);

        /// <summary>
        /// حذف تعرفه بیمه
        /// </summary>
        /// <param name="id">شناسه تعرفه بیمه</param>
        /// <returns>نتیجه حذف</returns>
        Task<ServiceResult> DeleteTariffAsync(int id);

        #endregion

        #region Bulk Operations

        /// <summary>
        /// تغییر وضعیت گروهی تعرفه‌ها
        /// </summary>
        /// <param name="tariffIds">لیست شناسه‌های تعرفه‌ها</param>
        /// <param name="isActive">وضعیت جدید</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> BulkToggleStatusAsync(List<int> tariffIds, bool isActive);

        #endregion

        #region Bulk Operations

        /// <summary>
        /// ایجاد تعرفه برای همه خدمات (Bulk Operation)
        /// </summary>
        /// <param name="model">مدل تعرفه بیمه</param>
        /// <returns>تعداد تعرفه‌های ایجاد شده</returns>
        Task<ServiceResult<int>> CreateBulkTariffForAllServicesAsync(InsuranceTariffCreateEditViewModel model);

        #endregion

        #region Supplementary Insurance Methods

        /// <summary>
        /// دریافت تعرفه‌های بیمه تکمیلی
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>لیست تعرفه‌های بیمه تکمیلی</returns>
        Task<ServiceResult<List<InsuranceTariff>>> GetSupplementaryTariffsAsync(int planId, DateTime? calculationDate = null);

        /// <summary>
        /// محاسبه تعرفه بیمه تکمیلی
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="baseAmount">مبلغ پایه</param>
        /// <returns>مبلغ محاسبه شده تعرفه بیمه تکمیلی</returns>
        Task<ServiceResult<decimal>> CalculateSupplementaryTariffAsync(int serviceId, int planId, decimal baseAmount, DateTime? calculationDate = null);

        /// <summary>
        /// دریافت تنظیمات بیمه تکمیلی
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>تنظیمات بیمه تکمیلی</returns>
        Task<ServiceResult<Dictionary<string, object>>> GetSupplementarySettingsAsync(int planId);

        /// <summary>
        /// به‌روزرسانی تنظیمات بیمه تکمیلی
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="settings">تنظیمات جدید</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult> UpdateSupplementarySettingsAsync(int planId, Dictionary<string, object> settings);

        #endregion

        #region Statistics Operations

        /// <summary>
        /// دریافت آمار تعرفه‌های بیمه
        /// </summary>
        /// <returns>آمار تعرفه‌های بیمه</returns>
        Task<ServiceResult<InsuranceTariffStatisticsViewModel>> GetStatisticsAsync();

        #endregion
    }
}
