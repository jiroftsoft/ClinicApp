using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using ClinicApp.Interfaces;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// Implementation of cache invalidation for insurance tariffs
    /// Provides event-driven cache invalidation for medical systems
    /// </summary>
    public class TariffCacheInvalidator : ITariffCacheInvalidator
    {
        private readonly MemoryCache _cache;
        private readonly ILogger _logger;

        public TariffCacheInvalidator(ILogger logger)
        {
            _cache = MemoryCache.Default;
            _logger = logger;
        }

        public event EventHandler<CacheInvalidatedEventArgs> CacheInvalidated;

        /// <summary>
        /// Invalidate all tariff-related cache entries
        /// </summary>
        public void InvalidateAll()
        {
            var correlationId = Guid.NewGuid().ToString();
            
            _logger.Information("🏥 MEDICAL: Starting cache invalidation - CorrelationId: {CorrelationId}", correlationId);

            try
            {
                // Get all cache keys related to tariffs
                var keysToRemove = _cache
                    .Select(kvp => kvp.Key)
                    .Where(key => key.StartsWith("tariffs:") || 
                                 key.StartsWith("insurance:") || 
                                 key.StartsWith("statistics:"))
                    .ToList();

                // Remove all matching keys
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    _logger.Debug("🏥 MEDICAL: Removed cache key: {Key}", key);
                }

                _logger.Information("🏥 MEDICAL: Cache invalidation completed - CorrelationId: {CorrelationId}, RemovedKeys: {Count}", 
                    correlationId, keysToRemove.Count);

                // Raise event
                OnCacheInvalidated(new CacheInvalidatedEventArgs
                {
                    CacheType = "All",
                    Timestamp = DateTime.UtcNow,
                    Reason = "Full invalidation"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: Error during cache invalidation - CorrelationId: {CorrelationId}", correlationId);
                throw;
            }
        }

        /// <summary>
        /// Invalidate specific tariff cache entries
        /// </summary>
        public void InvalidateTariff(int tariffId)
        {
            var correlationId = Guid.NewGuid().ToString();
            
            _logger.Information("🏥 MEDICAL: Invalidating cache for tariff - CorrelationId: {CorrelationId}, TariffId: {TariffId}", 
                correlationId, tariffId);

            try
            {
                var keysToRemove = _cache
                    .Select(kvp => kvp.Key)
                    .Where(key => key.Contains($"tariff:{tariffId}") || 
                                 key.Contains($"tariffs:details:{tariffId}"))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    _logger.Debug("🏥 MEDICAL: Removed cache key: {Key}", key);
                }

                // Also invalidate related caches
                InvalidateStatistics();

                _logger.Information("🏥 MEDICAL: Tariff cache invalidation completed - CorrelationId: {CorrelationId}, TariffId: {TariffId}, RemovedKeys: {Count}", 
                    correlationId, tariffId, keysToRemove.Count);

                // Raise event
                OnCacheInvalidated(new CacheInvalidatedEventArgs
                {
                    CacheType = "Tariff",
                    TariffId = tariffId,
                    Timestamp = DateTime.UtcNow,
                    Reason = "Tariff-specific invalidation"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: Error during tariff cache invalidation - CorrelationId: {CorrelationId}, TariffId: {TariffId}", 
                    correlationId, tariffId);
                throw;
            }
        }

        /// <summary>
        /// Invalidate cache entries for a specific insurance plan
        /// </summary>
        public void InvalidateByPlan(int planId)
        {
            var correlationId = Guid.NewGuid().ToString();
            
            _logger.Information("🏥 MEDICAL: Invalidating cache for plan - CorrelationId: {CorrelationId}, PlanId: {PlanId}", 
                correlationId, planId);

            try
            {
                var keysToRemove = _cache
                    .Select(kvp => kvp.Key)
                    .Where(key => key.Contains($"plan:{planId}") || 
                                 key.Contains($"tariffs:plan:{planId}"))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    _logger.Debug("🏥 MEDICAL: Removed cache key: {Key}", key);
                }

                // Also invalidate statistics
                InvalidateStatistics();

                _logger.Information("🏥 MEDICAL: Plan cache invalidation completed - CorrelationId: {CorrelationId}, PlanId: {PlanId}, RemovedKeys: {Count}", 
                    correlationId, planId, keysToRemove.Count);

                // Raise event
                OnCacheInvalidated(new CacheInvalidatedEventArgs
                {
                    CacheType = "Plan",
                    PlanId = planId,
                    Timestamp = DateTime.UtcNow,
                    Reason = "Plan-specific invalidation"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: Error during plan cache invalidation - CorrelationId: {CorrelationId}, PlanId: {PlanId}", 
                    correlationId, planId);
                throw;
            }
        }

        /// <summary>
        /// Invalidate cache entries for a specific service
        /// </summary>
        public void InvalidateByService(int serviceId)
        {
            var correlationId = Guid.NewGuid().ToString();
            
            _logger.Information("🏥 MEDICAL: Invalidating cache for service - CorrelationId: {CorrelationId}, ServiceId: {ServiceId}", 
                correlationId, serviceId);

            try
            {
                var keysToRemove = _cache
                    .Select(kvp => kvp.Key)
                    .Where(key => key.Contains($"service:{serviceId}") || 
                                 key.Contains($"tariffs:service:{serviceId}"))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    _logger.Debug("🏥 MEDICAL: Removed cache key: {Key}", key);
                }

                // Also invalidate statistics
                InvalidateStatistics();

                _logger.Information("🏥 MEDICAL: Service cache invalidation completed - CorrelationId: {CorrelationId}, ServiceId: {ServiceId}, RemovedKeys: {Count}", 
                    correlationId, serviceId, keysToRemove.Count);

                // Raise event
                OnCacheInvalidated(new CacheInvalidatedEventArgs
                {
                    CacheType = "Service",
                    ServiceId = serviceId,
                    Timestamp = DateTime.UtcNow,
                    Reason = "Service-specific invalidation"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: Error during service cache invalidation - CorrelationId: {CorrelationId}, ServiceId: {ServiceId}", 
                    correlationId, serviceId);
                throw;
            }
        }

        /// <summary>
        /// Invalidate statistics cache
        /// </summary>
        public void InvalidateStatistics()
        {
            var correlationId = Guid.NewGuid().ToString();
            
            _logger.Information("🏥 MEDICAL: Invalidating statistics cache - CorrelationId: {CorrelationId}", correlationId);

            try
            {
                var keysToRemove = _cache
                    .Select(kvp => kvp.Key)
                    .Where(key => key.StartsWith("statistics:") || 
                                 key.StartsWith("tariffs:stats:"))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    _logger.Debug("🏥 MEDICAL: Removed cache key: {Key}", key);
                }

                _logger.Information("🏥 MEDICAL: Statistics cache invalidation completed - CorrelationId: {CorrelationId}, RemovedKeys: {Count}", 
                    correlationId, keysToRemove.Count);

                // Raise event
                OnCacheInvalidated(new CacheInvalidatedEventArgs
                {
                    CacheType = "Statistics",
                    Timestamp = DateTime.UtcNow,
                    Reason = "Statistics invalidation"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: Error during statistics cache invalidation - CorrelationId: {CorrelationId}", correlationId);
                throw;
            }
        }

        /// <summary>
        /// Raise cache invalidated event
        /// </summary>
        protected virtual void OnCacheInvalidated(CacheInvalidatedEventArgs e)
        {
            CacheInvalidated?.Invoke(this, e);
        }
    }
}
