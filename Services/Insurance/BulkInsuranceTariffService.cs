using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Insurance;
using ClinicApp.ViewModels.Insurance.InsuranceTariff;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس عملیات انبوه تعرفه بیمه با تضمین Transaction
    /// </summary>
    public class BulkInsuranceTariffService : IBulkInsuranceTariffService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public BulkInsuranceTariffService(
            ApplicationDbContext context,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// ایجاد تعرفه برای همه خدمات با Transaction و Idempotency
        /// </summary>
        public async Task<ServiceResult<int>> CreateBulkTariffForAllServicesAsync(
            InsuranceTariffCreateEditViewModel model, 
            string idempotencyKey = null)
        {
            var correlationId = Guid.NewGuid().ToString();
            
            try
            {
                _logger.Information("🏥 MEDICAL: شروع Bulk Operation - CorrelationId: {CorrelationId}, IdempotencyKey: {IdempotencyKey}, User: {UserName}",
                    correlationId, idempotencyKey, _currentUserService.UserName);

                // 🔧 CRITICAL FIX: بررسی Idempotency
                if (!string.IsNullOrEmpty(idempotencyKey))
                {
                    var cachedResult = await GetCachedBulkResultAsync(idempotencyKey);
                    if (cachedResult != null)
                    {
                        _logger.Information("🏥 MEDICAL: نتیجه Idempotency یافت شد - IdempotencyKey: {IdempotencyKey}, CachedCount: {Count}",
                            idempotencyKey, cachedResult);
                        return ServiceResult<int>.Successful(cachedResult.Value);
                    }
                }

                // 🔧 CRITICAL FIX: Transaction اتمی
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // دریافت همه خدمات فعال
                        var activeServices = await _context.Services
                            .Where(s => s.IsActive && !s.IsDeleted)
                            .ToListAsync();

                        if (!activeServices.Any())
                        {
                            _logger.Warning("🏥 MEDICAL: هیچ خدمت فعالی یافت نشد - CorrelationId: {CorrelationId}", correlationId);
                            return ServiceResult<int>.Failed("هیچ خدمت فعالی یافت نشد", "NO_ACTIVE_SERVICES");
                        }

                        var createdCount = 0;
                        var tariffEntities = new List<InsuranceTariff>();

                        // ایجاد تعرفه برای هر خدمت
                        foreach (var service in activeServices)
                        {
                            var tariff = new InsuranceTariff
                            {
                                ServiceId = service.ServiceId,
                                InsurancePlanId = model.InsurancePlanId,
                                TariffPrice = model.TariffPrice,
                                PatientShare = model.PatientShare,
                                InsurerShare = model.InsurerShare,
                                // PatientSharePercent = model.PatientSharePercent,
                                // InsurerSharePercent = model.InsurerSharePercent,
                                IsActive = model.IsActive,
                                StartDate = model.StartDate,
                                EndDate = model.EndDate,
                                CreatedAt = DateTime.UtcNow,
                                CreatedByUserId = _currentUserService.UserId,
                                // IdempotencyKey = idempotencyKey
                            };

                            tariffEntities.Add(tariff);
                        }

                        // 🔧 CRITICAL FIX: اضافه کردن همه در یک عملیات
                        _context.InsuranceTariffs.AddRange(tariffEntities);
                        await _context.SaveChangesAsync();

                        // 🔧 CRITICAL FIX: Commit Transaction
                        transaction.Commit();
                        createdCount = tariffEntities.Count;

                        // 🔧 CRITICAL FIX: Cache نتیجه برای Idempotency
                        if (!string.IsNullOrEmpty(idempotencyKey))
                        {
                            await CacheBulkResultAsync(idempotencyKey, createdCount);
                        }

                        _logger.Information("🏥 MEDICAL: Bulk Operation موفق - CorrelationId: {CorrelationId}, CreatedCount: {Count}, User: {UserName}",
                            correlationId, createdCount, _currentUserService.UserName);

                        return ServiceResult<int>.Successful(createdCount);
                    }
                    catch (Exception ex)
                    {
                        // 🔧 CRITICAL FIX: Rollback در صورت خطا
                        transaction.Rollback();
                        _logger.Error(ex, "🏥 MEDICAL: خطا در Bulk Operation - CorrelationId: {CorrelationId}, User: {UserName}",
                            correlationId, _currentUserService.UserName);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای کلی در Bulk Operation - CorrelationId: {CorrelationId}, User: {UserName}",
                    correlationId, _currentUserService.UserName);
                return ServiceResult<int>.Failed("خطا در ایجاد تعرفه‌های انبوه", "BULK_CREATE_ERROR");
            }
        }

        /// <summary>
        /// دریافت نتیجه Cache شده برای Idempotency
        /// </summary>
        private async Task<int?> GetCachedBulkResultAsync(string idempotencyKey)
        {
            try
            {
                // TODO: پیاده‌سازی Cache (Redis/Memory Cache)
                // برای حالا null برمی‌گردانیم
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت Cache - IdempotencyKey: {IdempotencyKey}", idempotencyKey);
                return null;
            }
        }

        /// <summary>
        /// Cache کردن نتیجه برای Idempotency
        /// </summary>
        private async Task CacheBulkResultAsync(string idempotencyKey, int count)
        {
            try
            {
                // TODO: پیاده‌سازی Cache (Redis/Memory Cache)
                // برای حالا هیچ کاری نمی‌کنیم
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در Cache کردن نتیجه - IdempotencyKey: {IdempotencyKey}, Count: {Count}",
                    idempotencyKey, count);
            }
        }
    }

    /// <summary>
    /// Interface برای سرویس Bulk Operations
    /// </summary>
    public interface IBulkInsuranceTariffService
    {
        Task<ServiceResult<int>> CreateBulkTariffForAllServicesAsync(
            InsuranceTariffCreateEditViewModel model, 
            string idempotencyKey = null);
    }
}
