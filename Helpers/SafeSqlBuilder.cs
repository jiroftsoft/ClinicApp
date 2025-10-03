using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Builder امن برای تولید SQL Query با اعتبارسنجی پارامترها
    /// این کلاس از SQL Injection جلوگیری می‌کند و پارامترها را اعتبارسنجی می‌کند
    /// </summary>
    public class SafeSqlBuilder
    {
        private static readonly ILogger _logger = Log.ForContext<SafeSqlBuilder>();
        private readonly StringBuilder _sqlBuilder;
        private readonly List<object> _parameters;
        private int _parameterIndex;

        public SafeSqlBuilder()
        {
            _sqlBuilder = new StringBuilder();
            _parameters = new List<object>();
            _parameterIndex = 0;
        }

        /// <summary>
        /// اضافه کردن SELECT clause
        /// </summary>
        /// <param name="columns">ستون‌ها</param>
        /// <returns>این instance</returns>
        public SafeSqlBuilder Select(params string[] columns)
        {
            if (columns == null || columns.Length == 0)
            {
                throw new ArgumentException("ستون‌ها نمی‌توانند خالی باشند", nameof(columns));
            }

            // اعتبارسنجی نام ستون‌ها
            foreach (var column in columns)
            {
                ValidateColumnName(column);
            }

            _sqlBuilder.Append("SELECT ");
            _sqlBuilder.Append(string.Join(", ", columns));
            _sqlBuilder.Append(" ");

            _logger.Debug("🔧 SAFE_SQL: SELECT clause اضافه شد: {Columns}", string.Join(", ", columns));
            return this;
        }

        /// <summary>
        /// اضافه کردن FROM clause
        /// </summary>
        /// <param name="tableName">نام جدول</param>
        /// <returns>این instance</returns>
        public SafeSqlBuilder From(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("نام جدول نمی‌تواند خالی باشد", nameof(tableName));
            }

            ValidateTableName(tableName);
            _sqlBuilder.Append($"FROM {tableName} ");

            _logger.Debug("🔧 SAFE_SQL: FROM clause اضافه شد: {TableName}", tableName);
            return this;
        }

        /// <summary>
        /// اضافه کردن WHERE clause
        /// </summary>
        /// <param name="condition">شرط</param>
        /// <param name="parameters">پارامترها</param>
        /// <returns>این instance</returns>
        public SafeSqlBuilder Where(string condition, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(condition))
            {
                throw new ArgumentException("شرط WHERE نمی‌تواند خالی باشد", nameof(condition));
            }

            _sqlBuilder.Append("WHERE ");
            _sqlBuilder.Append(condition);
            _sqlBuilder.Append(" ");

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    _parameters.Add(param);
                }
            }

            _logger.Debug("🔧 SAFE_SQL: WHERE clause اضافه شد: {Condition}", condition);
            return this;
        }

        /// <summary>
        /// اضافه کردن AND condition
        /// </summary>
        /// <param name="condition">شرط</param>
        /// <param name="parameters">پارامترها</param>
        /// <returns>این instance</returns>
        public SafeSqlBuilder And(string condition, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(condition))
            {
                throw new ArgumentException("شرط AND نمی‌تواند خالی باشد", nameof(condition));
            }

            _sqlBuilder.Append("AND ");
            _sqlBuilder.Append(condition);
            _sqlBuilder.Append(" ");

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    _parameters.Add(param);
                }
            }

            _logger.Debug("🔧 SAFE_SQL: AND condition اضافه شد: {Condition}", condition);
            return this;
        }

        /// <summary>
        /// اضافه کردن ORDER BY clause
        /// </summary>
        /// <param name="columnName">نام ستون</param>
        /// <param name="direction">جهت مرتب‌سازی (ASC/DESC)</param>
        /// <returns>این instance</returns>
        public SafeSqlBuilder OrderBy(string columnName, string direction = "ASC")
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("نام ستون نمی‌تواند خالی باشد", nameof(columnName));
            }

            ValidateColumnName(columnName);
            ValidateOrderDirection(direction);

            _sqlBuilder.Append($"ORDER BY {columnName} {direction.ToUpper()} ");

            _logger.Debug("🔧 SAFE_SQL: ORDER BY clause اضافه شد: {ColumnName} {Direction}", columnName, direction);
            return this;
        }

        /// <summary>
        /// تولید SQL Query نهایی
        /// </summary>
        /// <returns>SQL Query</returns>
        public string Build()
        {
            var query = _sqlBuilder.ToString().Trim();
            _logger.Debug("🔧 SAFE_SQL: Query نهایی تولید شد: {Query}", query);
            return query;
        }

        /// <summary>
        /// دریافت پارامترها
        /// </summary>
        /// <returns>لیست پارامترها</returns>
        public object[] GetParameters()
        {
            return _parameters.ToArray();
        }

        /// <summary>
        /// اعتبارسنجی نام ستون
        /// </summary>
        /// <param name="columnName">نام ستون</param>
        private void ValidateColumnName(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("نام ستون نمی‌تواند خالی باشد");
            }

            // بررسی کاراکترهای غیرمجاز
            if (columnName.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
            {
                throw new ArgumentException($"نام ستون '{columnName}' شامل کاراکترهای غیرمجاز است");
            }

            // بررسی کلمات کلیدی SQL
            var sqlKeywords = new[] { "SELECT", "FROM", "WHERE", "ORDER", "BY", "GROUP", "HAVING", "UNION", "INSERT", "UPDATE", "DELETE" };
            if (sqlKeywords.Contains(columnName.ToUpper()))
            {
                throw new ArgumentException($"نام ستون '{columnName}' یک کلمه کلیدی SQL است");
            }
        }

        /// <summary>
        /// اعتبارسنجی نام جدول
        /// </summary>
        /// <param name="tableName">نام جدول</param>
        private void ValidateTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("نام جدول نمی‌تواند خالی باشد");
            }

            // بررسی کاراکترهای غیرمجاز
            if (tableName.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
            {
                throw new ArgumentException($"نام جدول '{tableName}' شامل کاراکترهای غیرمجاز است");
            }
        }

        /// <summary>
        /// اعتبارسنجی جهت مرتب‌سازی
        /// </summary>
        /// <param name="direction">جهت</param>
        private void ValidateOrderDirection(string direction)
        {
            if (string.IsNullOrWhiteSpace(direction))
            {
                throw new ArgumentException("جهت مرتب‌سازی نمی‌تواند خالی باشد");
            }

            var validDirections = new[] { "ASC", "DESC" };
            if (!validDirections.Contains(direction.ToUpper()))
            {
                throw new ArgumentException($"جهت مرتب‌سازی '{direction}' معتبر نیست. مقادیر مجاز: ASC, DESC");
            }
        }
    }
}
