using System;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Interface for managing cache invalidation for insurance tariffs
    /// Designed for medical systems with high data consistency requirements
    /// </summary>
    public interface ITariffCacheInvalidator
    {
        /// <summary>
        /// Invalidate all tariff-related cache entries
        /// Used after Create/Edit/Delete/Bulk operations
        /// </summary>
        void InvalidateAll();

        /// <summary>
        /// Invalidate specific tariff cache entries
        /// </summary>
        /// <param name="tariffId">ID of the specific tariff</param>
        void InvalidateTariff(int tariffId);

        /// <summary>
        /// Invalidate cache entries for a specific insurance plan
        /// </summary>
        /// <param name="planId">Insurance plan ID</param>
        void InvalidateByPlan(int planId);

        /// <summary>
        /// Invalidate cache entries for a specific service
        /// </summary>
        /// <param name="serviceId">Service ID</param>
        void InvalidateByService(int serviceId);

        /// <summary>
        /// Invalidate statistics cache
        /// </summary>
        void InvalidateStatistics();

        /// <summary>
        /// Event raised when cache is invalidated
        /// </summary>
        event EventHandler<CacheInvalidatedEventArgs> CacheInvalidated;
    }

    /// <summary>
    /// Event arguments for cache invalidation events
    /// </summary>
    public class CacheInvalidatedEventArgs : EventArgs
    {
        public string CacheType { get; set; }
        public int? TariffId { get; set; }
        public int? PlanId { get; set; }
        public int? ServiceId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Reason { get; set; }
    }
}
