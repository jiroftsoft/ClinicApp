using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Serilog;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper برای تولید Dynamic SQL Query با استفاده از Reflection
    /// این کلاس به صورت خودکار ستون‌های مدل را شناسایی کرده و SQL Query تولید می‌کند
    /// </summary>
    public static class DynamicSqlHelper
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(DynamicSqlHelper));

        /// <summary>
        /// تولید Dynamic SELECT Query برای یک مدل
        /// </summary>
        /// <typeparam name="T">نوع مدل</typeparam>
        /// <param name="tableName">نام جدول</param>
        /// <param name="whereClause">شرط WHERE (اختیاری)</param>
        /// <param name="orderByClause">شرط ORDER BY (اختیاری)</param>
        /// <returns>SQL Query تولید شده</returns>
        public static string GenerateSelectQuery<T>(string tableName, string whereClause = null, string orderByClause = null)
        {
            try
            {
                _logger.Debug("🔧 DYNAMIC_SQL: شروع تولید Query برای مدل {ModelType} در جدول {TableName}", 
                    typeof(T).Name, tableName);

                var columns = GetModelColumns<T>();
                var sqlBuilder = new StringBuilder();

                // SELECT clause
                sqlBuilder.Append("SELECT ");
                sqlBuilder.Append(string.Join(", ", columns));
                sqlBuilder.Append($" FROM {tableName}");

                // WHERE clause
                if (!string.IsNullOrWhiteSpace(whereClause))
                {
                    sqlBuilder.Append($" WHERE {whereClause}");
                }

                // ORDER BY clause
                if (!string.IsNullOrWhiteSpace(orderByClause))
                {
                    sqlBuilder.Append($" ORDER BY {orderByClause}");
                }

                var finalQuery = sqlBuilder.ToString();
                _logger.Debug("🔧 DYNAMIC_SQL: Query تولید شده: {Query}", finalQuery);

                return finalQuery;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ DYNAMIC_SQL: خطا در تولید Query برای مدل {ModelType}", typeof(T).Name);
                throw new InvalidOperationException($"خطا در تولید Dynamic SQL Query برای مدل {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// دریافت ستون‌های مدل با استفاده از Reflection
        /// </summary>
        /// <typeparam name="T">نوع مدل</typeparam>
        /// <returns>لیست ستون‌ها</returns>
        private static List<string> GetModelColumns<T>()
        {
            try
            {
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var columns = new List<string>();

                foreach (var property in properties)
                {
                    // بررسی اینکه آیا property باید در SQL استفاده شود یا نه
                    if (ShouldIncludeProperty(property))
                    {
                        var columnName = GetColumnName(property);
                        columns.Add(columnName);
                    }
                }

                _logger.Debug("🔧 DYNAMIC_SQL: شناسایی {Count} ستون برای مدل {ModelType}", 
                    columns.Count, typeof(T).Name);

                return columns;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ DYNAMIC_SQL: خطا در شناسایی ستون‌های مدل {ModelType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// بررسی اینکه آیا property باید در SQL استفاده شود یا نه
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns>true اگر باید استفاده شود</returns>
        private static bool ShouldIncludeProperty(PropertyInfo property)
        {
            // بررسی NotMapped attribute
            if (property.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>() != null)
            {
                return false;
            }

            // بررسی Navigation Properties (Collection)
            if (property.PropertyType.IsGenericType && 
                typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                return false;
            }

            // بررسی Navigation Properties (Single)
            if (property.PropertyType.IsClass && 
                property.PropertyType != typeof(string) && 
                property.PropertyType != typeof(byte[]))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// دریافت نام ستون از Property
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns>نام ستون</returns>
        private static string GetColumnName(PropertyInfo property)
        {
            // بررسی Column attribute
            var columnAttribute = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
            if (columnAttribute != null && !string.IsNullOrWhiteSpace(columnAttribute.Name))
            {
                return columnAttribute.Name;
            }

            // استفاده از نام Property
            return property.Name;
        }

        /// <summary>
        /// تولید WHERE clause برای فیلترهای فعال
        /// </summary>
        /// <returns>WHERE clause برای فیلترهای فعال</returns>
        public static string GetActiveFilterWhereClause()
        {
            return "IsActive = 1 AND IsDeleted = 0";
        }

        /// <summary>
        /// تولید WHERE clause برای فیلترهای حذف نشده
        /// </summary>
        /// <returns>WHERE clause برای فیلترهای حذف نشده</returns>
        public static string GetNotDeletedFilterWhereClause()
        {
            return "IsDeleted = 0";
        }

        /// <summary>
        /// تولید ORDER BY clause برای مرتب‌سازی بر اساس نام
        /// </summary>
        /// <param name="columnName">نام ستون برای مرتب‌سازی (پیش‌فرض: Name)</param>
        /// <returns>ORDER BY clause</returns>
        public static string GetNameOrderByClause(string columnName = "Name")
        {
            return $"{columnName} ASC";
        }
    }
}
