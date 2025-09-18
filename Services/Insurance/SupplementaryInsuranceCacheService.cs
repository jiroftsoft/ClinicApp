using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.ViewModels.Insurance.Supplementary;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس Cache برای بیمه تکمیلی
    /// طراحی شده برای بهبود Performance در سیستم‌های پزشکی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. Cache هوشمند برای تعرفه‌های بیمه تکمیلی
    /// 2. Cache برای تنظیمات بیمه تکمیلی
    /// 3. Cache برای نتایج محاسبات
    /// 4. مدیریت خودکار انقضای Cache
    /// 5. بهینه‌سازی برای محیط‌های با ترافیک بالا
    /// </summary>
    public class SupplementaryInsuranceCacheService : ISupplementaryInsuranceCacheService
    {
        private readonly IInsuranceTariffRepository _tariffRepository;
        private readonly IInsurancePlanRepository _planRepository;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        // Cache های درون‌حافظه‌ای
        private static readonly Dictionary<string, CachedTariffData> _tariffCache = new Dictionary<string, CachedTariffData>();
        private static readonly Dictionary<string, CachedSettingsData> _settingsCache = new Dictionary<string, CachedSettingsData>();
        private static readonly Dictionary<string, CachedCalculationData> _calculationCache = new Dictionary<string, CachedCalculationData>();

        // تنظیمات Cache
        private static readonly TimeSpan TariffCacheExpiry = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan SettingsCacheExpiry = TimeSpan.FromMinutes(60);
        private static readonly TimeSpan CalculationCacheExpiry = TimeSpan.FromMinutes(15);

        public SupplementaryInsuranceCacheService(
            IInsuranceTariffRepository tariffRepository,
            IInsurancePlanRepository planRepository,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _tariffRepository = tariffRepository ?? throw new ArgumentNullException(nameof(tariffRepository));
            _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _log = logger.ForContext<SupplementaryInsuranceCacheService>();
        }

        #region ISupplementaryInsuranceCacheService Implementation

        /// <summary>
        /// دریافت تعرفه‌های بیمه تکمیلی از Cache یا Database
        /// </summary>
        public async Task<ServiceResult<List<SupplementaryTariffViewModel>>> GetCachedSupplementaryTariffsAsync(int planId)
        {
            try
            {
                var cacheKey = $"supplementary_tariffs_{planId}";
                
                // بررسی Cache
                if (_tariffCache.ContainsKey(cacheKey) && !_tariffCache[cacheKey].IsExpired())
                {
                    _log.Information("🏥 MEDICAL: تعرفه‌های بیمه تکمیلی از Cache دریافت شد - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<List<SupplementaryTariffViewModel>>.Successful(_tariffCache[cacheKey].Data);
                }

                // دریافت از Database
                var tariffs = await _tariffRepository.GetByPlanIdAsync(planId);
                var supplementaryTariffs = tariffs
                    .Where(t => t.InsuranceType == InsuranceType.Supplementary)
                    .Select(t => new SupplementaryTariffViewModel
                    {
                        TariffId = t.InsuranceTariffId,
                        PlanId = t.InsurancePlanId ?? 0,
                        ServiceId = t.ServiceId,
                        ServiceName = t.Service?.Title,
                        CoveragePercent = t.SupplementaryCoveragePercent ?? 0,
                        MaxPayment = t.SupplementaryMaxPayment ?? 0,
                        Settings = t.SupplementarySettings
                    })
                    .ToList();

                // ذخیره در Cache
                _tariffCache[cacheKey] = new CachedTariffData
                {
                    Data = supplementaryTariffs,
                    CachedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.Add(TariffCacheExpiry)
                };

                _log.Information("🏥 MEDICAL: تعرفه‌های بیمه تکمیلی در Cache ذخیره شد - PlanId: {PlanId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                    planId, supplementaryTariffs.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<SupplementaryTariffViewModel>>.Successful(supplementaryTariffs);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت تعرفه‌های بیمه تکمیلی از Cache - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<List<SupplementaryTariffViewModel>>.Failed("خطا در دریافت تعرفه‌های بیمه تکمیلی");
            }
        }

        /// <summary>
        /// دریافت تنظیمات بیمه تکمیلی از Cache یا Database
        /// </summary>
        public async Task<ServiceResult<SupplementarySettings>> GetCachedSupplementarySettingsAsync(int planId)
        {
            try
            {
                var cacheKey = $"supplementary_settings_{planId}";
                
                // بررسی Cache
                if (_settingsCache.ContainsKey(cacheKey) && !_settingsCache[cacheKey].IsExpired())
                {
                    _log.Information("🏥 MEDICAL: تنظیمات بیمه تکمیلی از Cache دریافت شد - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<SupplementarySettings>.Successful(_settingsCache[cacheKey].Data);
                }

                // دریافت از Database
                var plan = await _planRepository.GetByIdAsync(planId);
                if (plan == null)
                {
                    return ServiceResult<SupplementarySettings>.Failed("طرح بیمه یافت نشد");
                }

                var tariffs = await _tariffRepository.GetByPlanIdAsync(planId);
                var supplementaryTariff = tariffs.FirstOrDefault(t => t.InsuranceType == InsuranceType.Supplementary);

                if (supplementaryTariff == null)
                {
                    return ServiceResult<SupplementarySettings>.Failed("تعرفه بیمه تکمیلی برای این طرح یافت نشد");
                }

                var settings = new SupplementarySettings
                {
                    PlanId = planId,
                    PlanName = plan.Name,
                    CoveragePercent = supplementaryTariff.SupplementaryCoveragePercent ?? 0,
                    MaxPayment = supplementaryTariff.SupplementaryMaxPayment ?? 0,
                    IsActive = plan.IsActive,
                    SettingsJson = supplementaryTariff.SupplementarySettings
                };

                // ذخیره در Cache
                _settingsCache[cacheKey] = new CachedSettingsData
                {
                    Data = settings,
                    CachedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.Add(SettingsCacheExpiry)
                };

                _log.Information("🏥 MEDICAL: تنظیمات بیمه تکمیلی در Cache ذخیره شد - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<SupplementarySettings>.Successful(settings);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت تنظیمات بیمه تکمیلی از Cache - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<SupplementarySettings>.Failed("خطا در دریافت تنظیمات بیمه تکمیلی");
            }
        }

        /// <summary>
        /// دریافت نتیجه محاسبه از Cache یا محاسبه جدید
        /// </summary>
        public async Task<ServiceResult<SupplementaryCalculationResult>> GetCachedCalculationResultAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            decimal primaryCoverage, 
            DateTime calculationDate,
            ISupplementaryInsuranceService supplementaryService)
        {
            try
            {
                var cacheKey = $"calculation_{patientId}_{serviceId}_{serviceAmount}_{primaryCoverage}_{calculationDate:yyyyMMdd}";
                
                // بررسی Cache
                if (_calculationCache.ContainsKey(cacheKey) && !_calculationCache[cacheKey].IsExpired())
                {
                    _log.Information("🏥 MEDICAL: نتیجه محاسبه از Cache دریافت شد - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<SupplementaryCalculationResult>.Successful(_calculationCache[cacheKey].Data);
                }

                // محاسبه جدید
                var result = await supplementaryService.CalculateSupplementaryInsuranceAsync(
                    patientId, serviceId, serviceAmount, primaryCoverage, calculationDate);

                if (result.Success)
                {
                    // ذخیره در Cache
                    _calculationCache[cacheKey] = new CachedCalculationData
                    {
                        Data = result.Data,
                        CachedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.Add(CalculationCacheExpiry)
                    };

                    _log.Information("🏥 MEDICAL: نتیجه محاسبه در Cache ذخیره شد - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت نتیجه محاسبه از Cache - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<SupplementaryCalculationResult>.Failed("خطا در دریافت نتیجه محاسبه");
            }
        }

        /// <summary>
        /// پاک کردن Cache
        /// </summary>
        public void ClearCache(string cacheType = null)
        {
            try
            {
                if (string.IsNullOrEmpty(cacheType))
                {
                    // پاک کردن تمام Cache ها
                    _tariffCache.Clear();
                    _settingsCache.Clear();
                    _calculationCache.Clear();
                    
                    _log.Information("🏥 MEDICAL: تمام Cache ها پاک شدند. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                }
                else
                {
                    // پاک کردن Cache خاص
                    switch (cacheType.ToLower())
                    {
                        case "tariff":
                            _tariffCache.Clear();
                            break;
                        case "settings":
                            _settingsCache.Clear();
                            break;
                        case "calculation":
                            _calculationCache.Clear();
                            break;
                    }
                    
                    _log.Information("🏥 MEDICAL: Cache {CacheType} پاک شد. User: {UserName} (Id: {UserId})",
                        cacheType, _currentUserService.UserName, _currentUserService.UserId);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در پاک کردن Cache");
            }
        }

        /// <summary>
        /// پاک کردن Cache های منقضی شده
        /// </summary>
        public void CleanExpiredCache()
        {
            try
            {
                var now = DateTime.UtcNow;
                
                // پاک کردن تعرفه‌های منقضی شده
                var expiredTariffKeys = _tariffCache.Where(kvp => kvp.Value.IsExpired()).Select(kvp => kvp.Key).ToList();
                foreach (var key in expiredTariffKeys)
                {
                    _tariffCache.Remove(key);
                }
                
                // پاک کردن تنظیمات منقضی شده
                var expiredSettingsKeys = _settingsCache.Where(kvp => kvp.Value.IsExpired()).Select(kvp => kvp.Key).ToList();
                foreach (var key in expiredSettingsKeys)
                {
                    _settingsCache.Remove(key);
                }
                
                // پاک کردن محاسبات منقضی شده
                var expiredCalculationKeys = _calculationCache.Where(kvp => kvp.Value.IsExpired()).Select(kvp => kvp.Key).ToList();
                foreach (var key in expiredCalculationKeys)
                {
                    _calculationCache.Remove(key);
                }
                
                var totalCleaned = expiredTariffKeys.Count + expiredSettingsKeys.Count + expiredCalculationKeys.Count;
                
                if (totalCleaned > 0)
                {
                    _log.Information("🏥 MEDICAL: {Count} Cache منقضی شده پاک شدند. User: {UserName} (Id: {UserId})",
                        totalCleaned, _currentUserService.UserName, _currentUserService.UserId);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در پاک کردن Cache های منقضی شده");
            }
        }

        /// <summary>
        /// دریافت آمار Cache
        /// </summary>
        public Models.Insurance.CacheStatistics GetCacheStatistics()
        {
            try
            {
                var now = DateTime.UtcNow;
                
                var statistics = new Models.Insurance.CacheStatistics
                {
                    TotalCachedItems = _tariffCache.Count + _settingsCache.Count + _calculationCache.Count,
                    CacheHits = 0, // این مقدار باید در متدهای دیگر به‌روزرسانی شود
                    CacheMisses = 0, // این مقدار باید در متدهای دیگر به‌روزرسانی شود
                    LastCacheClear = now,
                    CacheCreatedAt = now,
                    AverageResponseTimeMs = 0,
                    MaxResponseTimeMs = 0,
                    MinResponseTimeMs = 0,
                    CacheErrors = 0,
                    ExpiredItems = _tariffCache.Count(kvp => kvp.Value.IsExpired()) + 
                                  _settingsCache.Count(kvp => kvp.Value.IsExpired()) + 
                                  _calculationCache.Count(kvp => kvp.Value.IsExpired()),
                    EvictedItems = 0,
                    LastUpdated = now
                };
                
                return statistics;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت آمار Cache");
                return new Models.Insurance.CacheStatistics();
            }
        }

        #endregion

        #region Private Helper Classes

        private class CachedTariffData
        {
            public List<SupplementaryTariffViewModel> Data { get; set; }
            public DateTime CachedAt { get; set; }
            public DateTime ExpiresAt { get; set; }
            
            public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
        }

        private class CachedSettingsData
        {
            public SupplementarySettings Data { get; set; }
            public DateTime CachedAt { get; set; }
            public DateTime ExpiresAt { get; set; }
            
            public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
        }

        private class CachedCalculationData
        {
            public SupplementaryCalculationResult Data { get; set; }
            public DateTime CachedAt { get; set; }
            public DateTime ExpiresAt { get; set; }
            
            public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
        }

        #endregion
    }

}
