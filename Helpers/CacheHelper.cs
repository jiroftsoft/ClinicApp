using System;
using System.Runtime.Caching;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper class for caching operations
    /// کلاس کمکی برای عملیات Cache
    /// </summary>
    public static class CacheHelper
    {
        private static readonly MemoryCache _cache = MemoryCache.Default;

        #region Get/Set Operations

        /// <summary>
        /// Gets or creates a cached value
        /// دریافت یا ایجاد مقدار Cache شده
        /// </summary>
        /// <typeparam name="T">Type of cached value</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="factory">Function to create value if not cached</param>
        /// <param name="expirationMinutes">Cache expiration in minutes (default: 60)</param>
        /// <returns>Cached or newly created value</returns>
        /// <example>
        /// var users = CacheHelper.GetOrCreate("users", () => db.Users.ToList(), 30);
        /// </example>
        public static T GetOrCreate<T>(string key, Func<T> factory, int expirationMinutes = 60)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (_cache.Contains(key))
                return (T)_cache.Get(key);

            var value = factory();
            Set(key, value, expirationMinutes);
            return value;
        }

        /// <summary>
        /// Sets a value in cache
        /// تنظیم مقدار در Cache
        /// </summary>
        public static void Set<T>(string key, T value, int expirationMinutes = 60)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(expirationMinutes)
            };

            _cache.Set(key, value, policy);
        }

        /// <summary>
        /// Gets a value from cache
        /// دریافت مقدار از Cache
        /// </summary>
        public static T Get<T>(string key, T defaultValue = default(T))
        {
            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            if (_cache.Contains(key))
                return (T)_cache.Get(key);

            return defaultValue;
        }

        #endregion

        #region Remove Operations

        /// <summary>
        /// Removes a specific key from cache
        /// حذف کلید مشخص از Cache
        /// </summary>
        public static void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            _cache.Remove(key);
        }

        /// <summary>
        /// Removes all keys matching a pattern
        /// حذف تمام کلیدهای مطابق با الگو
        /// </summary>
        public static void RemoveByPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return;

            var keysToRemove = new System.Collections.Generic.List<string>();
            
            foreach (var item in _cache)
            {
                if (item.Key.Contains(pattern))
                    keysToRemove.Add(item.Key);
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }

        /// <summary>
        /// Clears all cache
        /// پاک کردن تمام Cache
        /// </summary>
        public static void Clear()
        {
            var keys = new System.Collections.Generic.List<string>();
            foreach (var item in _cache)
            {
                keys.Add(item.Key);
            }

            foreach (var key in keys)
            {
                _cache.Remove(key);
            }
        }

        #endregion

        #region Check Operations

        /// <summary>
        /// Checks if a key exists in cache
        /// بررسی وجود کلید در Cache
        /// </summary>
        public static bool Contains(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            return _cache.Contains(key);
        }

        #endregion

        #region Sliding Expiration

        /// <summary>
        /// Sets a value with sliding expiration (resets timer on each access)
        /// تنظیم با Sliding Expiration
        /// </summary>
        public static void SetWithSlidingExpiration<T>(string key, T value, int slidingExpirationMinutes = 20)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var policy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(slidingExpirationMinutes)
            };

            _cache.Set(key, value, policy);
        }

        #endregion
    }
}
