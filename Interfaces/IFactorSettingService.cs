using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// سرویس مدیریت تنظیمات کای‌ها
    /// </summary>
    public interface IFactorSettingService
    {
        /// <summary>
        /// دریافت تمام کای‌ها
        /// </summary>
        Task<IEnumerable<FactorSetting>> GetAllFactorsAsync();

        /// <summary>
        /// دریافت کای بر اساس شناسه
        /// </summary>
        Task<FactorSetting> GetFactorByIdAsync(int id);

        /// <summary>
        /// دریافت کای‌ها بر اساس نوع و سال مالی
        /// </summary>
        Task<IEnumerable<FactorSetting>> GetFactorsByTypeAsync(ServiceComponentType factorType, int financialYear);

        /// <summary>
        /// دریافت کای فعال بر اساس نوع و سال مالی
        /// </summary>
        Task<FactorSetting> GetActiveFactorByTypeAsync(ServiceComponentType factorType, int financialYear, bool isHashtagged = false);

        /// <summary>
        /// دریافت کای فعال بر اساس نوع، سال مالی و وضعیت هشتگ‌دار
        /// </summary>
        Task<FactorSetting> GetActiveFactorByTypeAndHashtaggedAsync(ServiceComponentType factorType, bool isHashtagged, int financialYear);

        /// <summary>
        /// ایجاد کای جدید
        /// </summary>
        Task<FactorSetting> CreateFactorAsync(FactorSetting factor);

        /// <summary>
        /// به‌روزرسانی کای
        /// </summary>
        Task<FactorSetting> UpdateFactorAsync(FactorSetting factor);

        /// <summary>
        /// حذف کای
        /// </summary>
        Task DeleteFactorAsync(int id);

        /// <summary>
        /// بررسی وجود کای برای نوع و سال مالی مشخص
        /// </summary>
        Task<bool> ExistsFactorAsync(ServiceComponentType factorType, int financialYear, bool isHashtagged = false);

        /// <summary>
        /// دریافت کای‌های سال مالی جاری
        /// </summary>
        Task<IEnumerable<FactorSetting>> GetCurrentYearFactorsAsync();

        /// <summary>
        /// دریافت کای‌های سال مالی مشخص
        /// </summary>
        Task<IEnumerable<FactorSetting>> GetFactorsByFinancialYearAsync(int financialYear);

        /// <summary>
        /// دریافت فیلتر شده کای‌ها با Pagination
        /// </summary>
        Task<IEnumerable<FactorSetting>> GetFilteredFactorsAsync(string searchTerm, ServiceComponentType? factorType, int? financialYear, bool? isActive, int page, int pageSize);

        /// <summary>
        /// دریافت تعداد کای‌ها بر اساس فیلتر
        /// </summary>
        Task<int> GetFactorsCountAsync(string searchTerm, ServiceComponentType? factorType, int? financialYear, bool? isActive);

        /// <summary>
        /// دریافت تعداد کای‌های فعال برای سال مالی
        /// </summary>
        Task<int> GetActiveFactorsCountForYearAsync(int financialYear);

        /// <summary>
        /// بررسی استفاده از کای در محاسبات
        /// </summary>
        Task<bool> IsFactorUsedInCalculationsAsync(int factorId);

        /// <summary>
        /// فریز کردن کای‌های سال مالی
        /// </summary>
        Task<bool> FreezeFinancialYearFactorsAsync(int financialYear, string userId);

        /// <summary>
        /// دریافت کای‌های فریز شده
        /// </summary>
        Task<IEnumerable<FactorSetting>> GetFrozenFactorsAsync(int? financialYear = null);
    }
}
