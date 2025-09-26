using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace ClinicApp.Services.Idempotency
{
    /// <summary>
    /// پیاده‌سازی In-Memory برای سرویس Idempotency
    /// </summary>
    public class InMemoryIdempotencyService : IIdempotencyService
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, DateTime> _keyStore = new ConcurrentDictionary<string, DateTime>();
        private readonly object _cleanupLock = new object();

        public InMemoryIdempotencyService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> TryUseKeyAsync(string key, int ttlMinutes = 30, string scope = "default")
        {
            if (string.IsNullOrEmpty(key))
            {
                _logger.Warning("🔒 IDEMPOTENCY: کلید خالی دریافت شد");
                return false;
            }

            var scopedKey = $"{scope}:{key}";
            var now = DateTime.UtcNow;

            try
            {
                // بررسی وجود کلید
                if (_keyStore.TryGetValue(scopedKey, out var storedTime))
                {
                    var timeDiff = now - storedTime;
                    
                    // اگر کلید منقضی شده، حذف کن و اجازه استفاده مجدد
                    if (timeDiff.TotalMinutes > ttlMinutes)
                    {
                        _keyStore.TryRemove(scopedKey, out _);
                        _keyStore.TryAdd(scopedKey, now);
                        
                        _logger.Debug("🔒 IDEMPOTENCY: کلید منقضی شده مجدداً استفاده شد - Key: {Key}, Scope: {Scope}", key, scope);
                        
                        // پاکسازی کلیدهای منقضی شده
                        await CleanupExpiredKeysAsync(scope);
                        
                        return true;
                    }
                    
                    // کلید تکراری است (هنوز منقضی نشده)
                    _logger.Debug("🔒 IDEMPOTENCY: کلید تکراری شناسایی شد - Key: {Key}, Scope: {Scope}, Age: {Age}min", 
                        key, scope, timeDiff.TotalMinutes);
                    return false;
                }
                
                // ثبت کلید جدید
                _keyStore.TryAdd(scopedKey, now);
                
                _logger.Debug("🔒 IDEMPOTENCY: کلید جدید ثبت شد - Key: {Key}, Scope: {Scope}", key, scope);
                
                // پاکسازی کلیدهای منقضی شده
                await CleanupExpiredKeysAsync(scope);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🔒 IDEMPOTENCY: خطا در بررسی کلید - Key: {Key}, Scope: {Scope}", key, scope);
                return false; // در صورت خطا، اجازه ادامه
            }
        }

        public Task RemoveKeyAsync(string key, string scope = "default")
        {
            if (string.IsNullOrEmpty(key))
                return Task.CompletedTask;

            var scopedKey = $"{scope}:{key}";
            _keyStore.TryRemove(scopedKey, out _);
            
            _logger.Debug("🔒 IDEMPOTENCY: کلید حذف شد - Key: {Key}, Scope: {Scope}", key, scope);
            
            return Task.CompletedTask;
        }

        public Task CleanupExpiredKeysAsync(string scope = "default")
        {
            try
            {
                lock (_cleanupLock)
                {
                    var now = DateTime.UtcNow;
                    var expiredKeys = _keyStore
                        .Where(kvp => kvp.Key.StartsWith($"{scope}:") && (now - kvp.Value).TotalMinutes > 60) // 1 ساعت
                        .Select(kvp => kvp.Key)
                        .ToList();
                        
                    foreach (var key in expiredKeys)
                    {
                        _keyStore.TryRemove(key, out _);
                    }
                    
                    if (expiredKeys.Count > 0)
                    {
                        _logger.Debug("🔒 IDEMPOTENCY: {Count} کلید منقضی شده پاکسازی شد - Scope: {Scope}", expiredKeys.Count, scope);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🔒 IDEMPOTENCY: خطا در پاکسازی کلیدهای منقضی شده - Scope: {Scope}", scope);
            }
            
            return Task.CompletedTask;
        }
    }
}
