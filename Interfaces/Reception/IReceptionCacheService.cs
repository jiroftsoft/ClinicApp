using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// Interface برای مدیریت Cache ماژول پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت Cache پذیرش‌ها
    /// 2. مدیریت Cache Lookup Lists
    /// 3. مدیریت Cache آمار
    /// 4. بهینه‌سازی عملکرد
    /// 5. مدیریت Cache Expiry
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط مدیریت Cache
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    public interface IReceptionCacheService
    {
        #region Reception Cache Methods

        /// <summary>
        /// دریافت پذیرش‌ها از Cache
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست پذیرش‌ها</returns>
        Task<List<ReceptionComponentViewModels.ReceptionIndexViewModel>> GetReceptionsFromCacheAsync(ReceptionSearchCriteria criteria);

        /// <summary>
        /// ذخیره پذیرش‌ها در Cache
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <param name="receptions">لیست پذیرش‌ها</param>
        /// <returns>نتیجه ذخیره</returns>
        Task<bool> CacheReceptionsAsync(ReceptionSearchCriteria criteria, List<ReceptionComponentViewModels.ReceptionIndexViewModel> receptions);

        /// <summary>
        /// دریافت جزئیات پذیرش از Cache
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>جزئیات پذیرش</returns>
        Task<ReceptionComponentViewModels.ReceptionDetailsViewModel> GetReceptionDetailsFromCacheAsync(int id);

        /// <summary>
        /// ذخیره جزئیات پذیرش در Cache
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <param name="reception">جزئیات پذیرش</param>
        /// <returns>نتیجه ذخیره</returns>
        Task<bool> CacheReceptionDetailsAsync(int id, ReceptionComponentViewModels.ReceptionDetailsViewModel reception);

        #endregion

        #region Lookup Lists Cache Methods

        /// <summary>
        /// دریافت لیست پزشکان از Cache
        /// </summary>
        /// <returns>لیست پزشکان</returns>
        Task<List<ReceptionLookupViewModel>> GetDoctorsFromCacheAsync();

        /// <summary>
        /// ذخیره لیست پزشکان در Cache
        /// </summary>
        /// <param name="doctors">لیست پزشکان</param>
        /// <returns>نتیجه ذخیره</returns>
        Task<bool> CacheDoctorsAsync(List<ReceptionLookupViewModel> doctors);

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های خدمات از Cache
        /// </summary>
        /// <returns>لیست دسته‌بندی‌ها</returns>
        Task<List<ReceptionLookupViewModel>> GetServiceCategoriesFromCacheAsync();

        /// <summary>
        /// ذخیره لیست دسته‌بندی‌های خدمات در Cache
        /// </summary>
        /// <param name="categories">لیست دسته‌بندی‌ها</param>
        /// <returns>نتیجه ذخیره</returns>
        Task<bool> CacheServiceCategoriesAsync(List<ReceptionLookupViewModel> categories);

        /// <summary>
        /// دریافت لیست خدمات از Cache
        /// </summary>
        /// <param name="categoryId">شناسه دسته‌بندی</param>
        /// <returns>لیست خدمات</returns>
        Task<List<ReceptionLookupViewModel>> GetServicesFromCacheAsync(int categoryId);

        /// <summary>
        /// ذخیره لیست خدمات در Cache
        /// </summary>
        /// <param name="categoryId">شناسه دسته‌بندی</param>
        /// <param name="services">لیست خدمات</param>
        /// <returns>نتیجه ذخیره</returns>
        Task<bool> CacheServicesAsync(int categoryId, List<ReceptionLookupViewModel> services);

        /// <summary>
        /// دریافت لیست‌های Lookup از Cache
        /// </summary>
        /// <returns>لیست‌های Lookup</returns>
        Task<ReceptionLookupListsViewModel> GetLookupListsFromCacheAsync();

        /// <summary>
        /// ذخیره لیست‌های Lookup در Cache
        /// </summary>
        /// <param name="lookupLists">لیست‌های Lookup</param>
        /// <returns>نتیجه ذخیره</returns>
        Task<bool> CacheLookupListsAsync(ReceptionLookupListsViewModel lookupLists);

        #endregion

        #region Statistics Cache Methods

        /// <summary>
        /// دریافت آمار روزانه از Cache
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار روزانه</returns>
        Task<ReceptionDailyStatsViewModel> GetDailyStatsFromCacheAsync(DateTime date);

        /// <summary>
        /// ذخیره آمار روزانه در Cache
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <param name="stats">آمار روزانه</param>
        /// <returns>نتیجه ذخیره</returns>
        Task<bool> CacheDailyStatsAsync(DateTime date, ReceptionDailyStatsViewModel stats);

        /// <summary>
        /// دریافت آمار ماهانه از Cache
        /// </summary>
        /// <param name="year">سال</param>
        /// <param name="month">ماه</param>
        /// <returns>آمار ماهانه</returns>
        Task<ReceptionMonthlyStatsViewModel> GetMonthlyStatsFromCacheAsync(int year, int month);

        /// <summary>
        /// ذخیره آمار ماهانه در Cache
        /// </summary>
        /// <param name="year">سال</param>
        /// <param name="month">ماه</param>
        /// <param name="stats">آمار ماهانه</param>
        /// <returns>نتیجه ذخیره</returns>
        Task<bool> CacheMonthlyStatsAsync(int year, int month, ReceptionMonthlyStatsViewModel stats);

        #endregion

        #region Cache Management Methods

        /// <summary>
        /// پاک کردن Cache پذیرش‌ها
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>نتیجه پاک کردن</returns>
        Task<bool> ClearReceptionsCacheAsync(ReceptionSearchCriteria criteria);

        /// <summary>
        /// پاک کردن Cache جزئیات پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>نتیجه پاک کردن</returns>
        Task<bool> ClearReceptionDetailsCacheAsync(int id);

        /// <summary>
        /// پاک کردن Cache لیست‌های Lookup
        /// </summary>
        /// <returns>نتیجه پاک کردن</returns>
        Task<bool> ClearLookupListsCacheAsync();

        /// <summary>
        /// پاک کردن Cache آمار
        /// </summary>
        /// <returns>نتیجه پاک کردن</returns>
        Task<bool> ClearStatisticsCacheAsync();

        /// <summary>
        /// پاک کردن تمام Cache
        /// </summary>
        /// <returns>نتیجه پاک کردن</returns>
        Task<bool> ClearAllCacheAsync();

        #endregion

        #region Cache Configuration Methods

        /// <summary>
        /// تنظیم Cache Expiry
        /// </summary>
        /// <param name="key">کلید Cache</param>
        /// <param name="expiry">زمان انقضا</param>
        /// <returns>نتیجه تنظیم</returns>
        Task<bool> SetCacheExpiryAsync(string key, TimeSpan expiry);

        /// <summary>
        /// دریافت Cache Expiry
        /// </summary>
        /// <param name="key">کلید Cache</param>
        /// <returns>زمان انقضا</returns>
        Task<TimeSpan?> GetCacheExpiryAsync(string key);

        /// <summary>
        /// بررسی وجود Cache
        /// </summary>
        /// <param name="key">کلید Cache</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> CacheExistsAsync(string key);

        /// <summary>
        /// دریافت Cache Size
        /// </summary>
        /// <param name="key">کلید Cache</param>
        /// <returns>اندازه Cache</returns>
        Task<long> GetCacheSizeAsync(string key);

        #endregion

        #region Cache Statistics Methods

        /// <summary>
        /// دریافت آمار Cache
        /// </summary>
        /// <returns>آمار Cache</returns>
        Task<CacheStatistics> GetCacheStatisticsAsync();

        /// <summary>
        /// دریافت آمار Cache Hit Rate
        /// </summary>
        /// <returns>نرخ Cache Hit</returns>
        Task<double> GetCacheHitRateAsync();

        /// <summary>
        /// دریافت آمار Cache Miss Rate
        /// </summary>
        /// <returns>نرخ Cache Miss</returns>
        Task<double> GetCacheMissRateAsync();

        /// <summary>
        /// دریافت آمار Cache Memory Usage
        /// </summary>
        /// <returns>استفاده از حافظه Cache</returns>
        Task<long> GetCacheMemoryUsageAsync();

        #endregion

        #region Advanced Cache Methods

        /// <summary>
        /// Cache با Compression
        /// </summary>
        /// <param name="key">کلید Cache</param>
        /// <param name="data">داده</param>
        /// <param name="expiry">زمان انقضا</param>
        /// <returns>نتیجه ذخیره</returns>
        Task<bool> CacheWithCompressionAsync(string key, object data, TimeSpan expiry);

        /// <summary>
        /// دریافت Cache با Decompression
        /// </summary>
        /// <param name="key">کلید Cache</param>
        /// <returns>داده</returns>
        Task<T> GetFromCompressedCacheAsync<T>(string key);

        /// <summary>
        /// Cache با Encryption
        /// </summary>
        /// <param name="key">کلید Cache</param>
        /// <param name="data">داده</param>
        /// <param name="expiry">زمان انقضا</param>
        /// <returns>نتیجه ذخیره</returns>
        Task<bool> CacheWithEncryptionAsync(string key, object data, TimeSpan expiry);

        /// <summary>
        /// دریافت Cache با Decryption
        /// </summary>
        /// <param name="key">کلید Cache</param>
        /// <returns>داده</returns>
        Task<T> GetFromEncryptedCacheAsync<T>(string key);

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// مدل نمایش Lookup
    /// </summary>
    public class ReceptionLookupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// مدل نمایش لیست‌های Lookup
    /// </summary>
    public class ReceptionLookupListsViewModel
    {
        public List<ReceptionLookupViewModel> Doctors { get; set; } = new List<ReceptionLookupViewModel>();
        public List<ReceptionLookupViewModel> Patients { get; set; } = new List<ReceptionLookupViewModel>();
        public List<ReceptionLookupViewModel> Services { get; set; } = new List<ReceptionLookupViewModel>();
        public List<ReceptionLookupViewModel> ServiceCategories { get; set; } = new List<ReceptionLookupViewModel>();
        public List<ReceptionLookupViewModel> PaymentMethods { get; set; } = new List<ReceptionLookupViewModel>();
        public List<ReceptionLookupViewModel> InsuranceProviders { get; set; } = new List<ReceptionLookupViewModel>();
    }

 

    #endregion
}
