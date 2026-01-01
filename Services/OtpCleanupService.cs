using ClinicApp.Models;
using Serilog;
using System;
using System.Linq;

namespace ClinicApp.Services
{
    /// <summary>
    /// Background service for cleaning up expired OTP states
    /// 
    /// ✅ Best Practice: Periodic cleanup to prevent table bloat
    /// 
    /// Usage:
    /// - Run as scheduled task (e.g., every hour)
    /// - Or call from Application_Start with Timer
    /// </summary>
    public class OtpCleanupService
    {
        private static readonly ILogger _log = Log.ForContext<OtpCleanupService>();

        /// <summary>
        /// Clean up expired OTP states from database
        /// </summary>
        public static int CleanupExpiredOtpStates()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var now = DateTime.UtcNow;
                    
                    // Delete OTP states older than expiry + 1 hour (grace period)
                    var expiredStates = context.OtpStates
                        .Where(o => o.ExpiryUtc < now.AddHours(-1))
                        .ToList();

                    if (expiredStates.Any())
                    {
                        context.OtpStates.RemoveRange(expiredStates);
                        context.SaveChanges();
                        
                        _log.Information("✅ OTP Cleanup: Removed {Count} expired OTP states", expiredStates.Count);
                        return expiredStates.Count;
                    }

                    _log.Debug("OTP Cleanup: No expired states to remove");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ Error during OTP cleanup");
                return -1;
            }
        }

        /// <summary>
        /// Get statistics about OTP states
        /// </summary>
        public static OtpStatistics GetStatistics()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var now = DateTime.UtcNow;
                    
                    var stats = new OtpStatistics
                    {
                        TotalStates = context.OtpStates.Count(),
                        ActiveStates = context.OtpStates.Count(o => o.ExpiryUtc > now),
                        ExpiredStates = context.OtpStates.Count(o => o.ExpiryUtc <= now),
                        OldestState = context.OtpStates
                            .OrderBy(o => o.CreatedAt)
                            .Select(o => o.CreatedAt)
                            .FirstOrDefault(),
                        NewestState = context.OtpStates
                            .OrderByDescending(o => o.CreatedAt)
                            .Select(o => o.CreatedAt)
                            .FirstOrDefault()
                    };

                    return stats;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ Error getting OTP statistics");
                return null;
            }
        }
    }

    public class OtpStatistics
    {
        public int TotalStates { get; set; }
        public int ActiveStates { get; set; }
        public int ExpiredStates { get; set; }
        public DateTime? OldestState { get; set; }
        public DateTime? NewestState { get; set; }
    }
}

