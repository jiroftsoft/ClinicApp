using System;
using System.Web.Caching;

namespace ClinicApp.Helpers;

public static class RateLimiter
{
    public static bool TryIncrement(string key, int maxAttempts, TimeSpan duration)
    {
        var cache = System.Web.HttpRuntime.Cache;
        int current = cache[key] as int? ?? 0;
        if (current >= maxAttempts)
            return false;

        cache.Insert(key, current + 1, null, DateTime.UtcNow.Add(duration), Cache.NoSlidingExpiration);
        return true;
    }
}