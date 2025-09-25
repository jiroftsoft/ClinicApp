using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.SystemSettings;

namespace ClinicApp.Services.SystemSettings
{
    /// <summary>
    /// سرویس مدیریت تنظیمات سیستم
    /// </summary>
    public interface ISystemSettingService
    {
        /// <summary>
        /// دریافت تنظیمات بر اساس کلید
        /// </summary>
        /// <param name="settingKey">کلید تنظیمات</param>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <returns>مقدار تنظیمات</returns>
        Task<string> GetSettingValueAsync(string settingKey, int? clinicId = null);

        /// <summary>
        /// دریافت تنظیمات به صورت تایپ شده
        /// </summary>
        /// <typeparam name="T">نوع داده</typeparam>
        /// <param name="settingKey">کلید تنظیمات</param>
        /// <param name="defaultValue">مقدار پیش‌فرض</param>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <returns>مقدار تنظیمات</returns>
        Task<T> GetSettingValueAsync<T>(string settingKey, T defaultValue = default, int? clinicId = null);

        /// <summary>
        /// تنظیم مقدار تنظیمات
        /// </summary>
        /// <param name="settingKey">کلید تنظیمات</param>
        /// <param name="settingValue">مقدار تنظیمات</param>
        /// <param name="dataType">نوع داده</param>
        /// <param name="category">دسته‌بندی</param>
        /// <param name="description">توضیحات</param>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <returns>نتیجه عملیات</returns>
        Task<bool> SetSettingValueAsync(string settingKey, string settingValue, string dataType, string category, string description = null, int? clinicId = null);

        /// <summary>
        /// دریافت تمام تنظیمات یک دسته‌بندی
        /// </summary>
        /// <param name="category">دسته‌بندی</param>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <returns>لیست تنظیمات</returns>
        Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category, int? clinicId = null);

        /// <summary>
        /// دریافت سال مالی جاری
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <returns>سال مالی</returns>
        Task<int> GetCurrentFinancialYearAsync(int? clinicId = null);

        /// <summary>
        /// دریافت تنظیمات محاسبه بیمه
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <returns>تنظیمات محاسبه</returns>
        Task<InsuranceCalculationSettings> GetInsuranceCalculationSettingsAsync(int? clinicId = null);

        /// <summary>
        /// دریافت تنظیمات سیستم
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <returns>تنظیمات سیستم</returns>
        Task<SystemConfigurationSettings> GetSystemConfigurationSettingsAsync(int? clinicId = null);
    }

    /// <summary>
    /// تنظیمات محاسبه بیمه
    /// </summary>
    public class InsuranceCalculationSettings
    {
        public decimal DefaultCoveragePercent { get; set; } = 80m;
        public decimal MaxCoveragePercent { get; set; } = 100m;
        public decimal MinCoveragePercent { get; set; } = 0m;
        public decimal DefaultTechnicalFactor { get; set; } = 1.0m;
        public decimal DefaultProfessionalFactor { get; set; } = 1.0m;
        public int CalculationPrecision { get; set; } = 2;
    }

    /// <summary>
    /// تنظیمات پیکربندی سیستم
    /// </summary>
    public class SystemConfigurationSettings
    {
        public string SystemName { get; set; } = "سیستم مدیریت کلینیک";
        public string SystemVersion { get; set; } = "1.0.0";
        public bool MaintenanceMode { get; set; } = false;
        public int SessionTimeout { get; set; } = 30; // دقیقه
        public int MaxLoginAttempts { get; set; } = 5;
        public int PasswordExpiryDays { get; set; } = 90;
    }
}
