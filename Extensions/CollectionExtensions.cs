using System;
using System.Collections.Generic;
using System.Linq;

namespace ClinicApp.Extensions
{
    /// <summary>
    /// Extension methods for Collection operations that are not built-in to C#
    /// متدهای کمکی برای عملیات روی Collection ها که در C# وجود ندارند
    /// </summary>
    public static class CollectionExtensions
    {
        #region IsNullOrEmpty & HasAny

        /// <summary>
        /// Checks if a collection is null or empty
        /// بررسی خالی بودن Collection
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="source">The collection to check</param>
        /// <returns>True if null or empty</returns>
        /// <example>
        /// List&lt;string&gt; items = null;
        /// if (items.IsNullOrEmpty()) // True - no NullReferenceException
        /// {
        ///     // Handle empty case
        /// }
        /// </example>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }

        /// <summary>
        /// Checks if a collection has any elements
        /// بررسی داشتن مقدار
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="source">The collection to check</param>
        /// <returns>True if has elements</returns>
        public static bool HasAny<T>(this IEnumerable<T> source)
        {
            return source != null && source.Any();
        }

        #endregion

        #region ForEach

        /// <summary>
        /// Executes an action on each element in the collection
        /// اجرای عملیات روی هر عضو Collection
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="source">The collection</param>
        /// <param name="action">Action to execute</param>
        /// <example>
        /// var numbers = new[] { 1, 2, 3, 4, 5 };
        /// numbers.ForEach(n => Console.WriteLine(n));
        /// </example>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var item in source)
            {
                action(item);
            }
        }

        #endregion

        #region DistinctBy

        /// <summary>
        /// Returns distinct elements based on a key selector
        /// دریافت موارد یکتا بر اساس Property
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <typeparam name="TKey">Type of key</typeparam>
        /// <param name="source">The collection</param>
        /// <param name="keySelector">Key selector function</param>
        /// <returns>Distinct elements</returns>
        /// <example>
        /// var users = new[] 
        /// {
        ///     new { Id = 1, Name = "Ali" },
        ///     new { Id = 2, Name = "Reza" },
        ///     new { Id = 1, Name = "Ali2" }
        /// };
        /// var distinct = users.DistinctBy(u => u.Id); // Only unique Ids
        /// </example>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return source.GroupBy(keySelector).Select(g => g.First());
        }

        #endregion

        #region Chunk

        /// <summary>
        /// Splits a collection into chunks of specified size
        /// تقسیم Collection به بخش‌های کوچکتر
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="source">The collection</param>
        /// <param name="chunkSize">Size of each chunk</param>
        /// <returns>Collection of chunks</returns>
        /// <example>
        /// var numbers = Enumerable.Range(1, 100);
        /// var chunks = numbers.Chunk(10); // 10 chunks of 10 numbers each
        /// </example>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (chunkSize <= 0)
                throw new ArgumentException("Chunk size must be greater than 0", nameof(chunkSize));

            var list = source.ToList();
            for (int i = 0; i < list.Count; i += chunkSize)
            {
                yield return list.Skip(i).Take(chunkSize);
            }
        }

        #endregion

        #region Shuffle

        /// <summary>
        /// Shuffles a collection randomly
        /// به هم زدن ترتیب به صورت تصادفی
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="source">The collection</param>
        /// <returns>Shuffled collection</returns>
        /// <example>
        /// var cards = new[] { "A", "K", "Q", "J" };
        /// var shuffled = cards.Shuffle();
        /// </example>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.OrderBy(x => Guid.NewGuid());
        }

        #endregion

        #region SafeGet

        /// <summary>
        /// Safely gets an element at index, returns default value if index is out of range
        /// دریافت امن عنصر بدون خطا
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="list">The list</param>
        /// <param name="index">Index to get</param>
        /// <param name="defaultValue">Default value if index is invalid</param>
        /// <returns>Element at index or default value</returns>
        public static T SafeGet<T>(this IList<T> list, int index, T defaultValue = default(T))
        {
            if (list == null)
                return defaultValue;

            if (index < 0 || index >= list.Count)
                return defaultValue;

            return list[index];
        }

        #endregion

        #region ToCsv

        /// <summary>
        /// Converts a collection to CSV string
        /// تبدیل به رشته CSV
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="source">The collection</param>
        /// <param name="separator">Separator (default: comma)</param>
        /// <returns>CSV string</returns>
        public static string ToCsv<T>(this IEnumerable<T> source, string separator = ",")
        {
            if (source == null)
                return string.Empty;

            return string.Join(separator, source);
        }

        #endregion

        #region Batch (Alias for Chunk)

        /// <summary>
        /// Alias for Chunk - Batches items into groups
        /// نام دیگر برای Chunk
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            return source.Chunk(batchSize);
        }

        #endregion

        #region EmptyIfNull

        /// <summary>
        /// Returns empty collection if source is null
        /// بازگشت Collection خالی در صورت null بودن
        /// </summary>
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        #endregion
    }
}
