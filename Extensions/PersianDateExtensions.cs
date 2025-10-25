using System;
using System.Globalization;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Base;

namespace ClinicApp.Extensions
{
    /// <summary>
    /// Extension Methods برای Persian DatePicker و DateTime
    /// طبق اصول DRY و SRP طراحی شده است
    /// </summary>
    public static class PersianDateExtensions
    {
        #region DateTime Extensions

        /// <summary>
        /// تبدیل DateTime به تاریخ شمسی
        /// </summary>
        public static string ToFaDate(this DateTime dateTime, string format = "yyyy/MM/dd")
        {
            var persianCalendar = new PersianCalendar();
            var year = persianCalendar.GetYear(dateTime);
            var month = persianCalendar.GetMonth(dateTime);
            var day = persianCalendar.GetDayOfMonth(dateTime);

            return format
                .Replace("yyyy", year.ToString("D4"))
                .Replace("MM", month.ToString("D2"))
                .Replace("dd", day.ToString("D2"))
                .Replace("M", month.ToString())
                .Replace("d", day.ToString());
        }

        /// <summary>
        /// تبدیل DateTime به تاریخ و زمان شمسی
        /// </summary>
        public static string ToFaDateTime(this DateTime dateTime, string format = "yyyy/MM/dd HH:mm")
        {
            var persianCalendar = new PersianCalendar();
            var year = persianCalendar.GetYear(dateTime);
            var month = persianCalendar.GetMonth(dateTime);
            var day = persianCalendar.GetDayOfMonth(dateTime);
            var hour = dateTime.Hour;
            var minute = dateTime.Minute;

            return format
                .Replace("yyyy", year.ToString("D4"))
                .Replace("MM", month.ToString("D2"))
                .Replace("dd", day.ToString("D2"))
                .Replace("HH", hour.ToString("D2"))
                .Replace("mm", minute.ToString("D2"))
                .Replace("M", month.ToString())
                .Replace("d", day.ToString())
                .Replace("H", hour.ToString())
                .Replace("m", minute.ToString());
        }

        /// <summary>
        /// تبدیل DateTime به زمان شمسی
        /// </summary>
        public static string ToFaTime(this DateTime dateTime, string format = "HH:mm")
        {
            return dateTime.ToString(format);
        }

        /// <summary>
        /// تبدیل DateTime? به تاریخ شمسی (nullable)
        /// </summary>
        public static string ToFaDate(this DateTime? dateTime, string format = "yyyy/MM/dd", string nullValue = "")
        {
            return dateTime?.ToFaDate(format) ?? nullValue;
        }

        /// <summary>
        /// تبدیل DateTime? به تاریخ و زمان شمسی (nullable)
        /// </summary>
        public static string ToFaDateTime(this DateTime? dateTime, string format = "yyyy/MM/dd HH:mm", string nullValue = "")
        {
            return dateTime?.ToFaDateTime(format) ?? nullValue;
        }

        #endregion

        #region String Extensions

        /// <summary>
        /// تبدیل رشته تاریخ شمسی به DateTime
        /// </summary>
        public static DateTime? FromFaDate(this string persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate))
                return null;

            try
            {
                // نرمال‌سازی ارقام
                var normalized = RegexHelper.ToEnglishDigits(persianDate);
                
                // فرمت‌های مختلف
                var formats = new[]
                {
                    "yyyy/MM/dd",
                    "yyyy/M/d",
                    "yyyy-MM-dd",
                    "yyyy-M-d",
                    "yyyyMMdd"
                };

                foreach (var format in formats)
                {
                    if (DateTime.TryParseExact(normalized, format, null, DateTimeStyles.None, out var result))
                    {
                        // تبدیل از شمسی به میلادی
                        var persianCalendar = new PersianCalendar();
                        return persianCalendar.ToDateTime(
                            int.Parse(normalized.Substring(0, 4)),
                            int.Parse(normalized.Substring(5, 2)),
                            int.Parse(normalized.Substring(8, 2)),
                            0, 0, 0, 0);
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// تبدیل رشته تاریخ شمسی به DateTime با TryParse
        /// </summary>
        public static bool TryParseFaDate(this string persianDate, out DateTime result)
        {
            result = default;
            var parsed = persianDate.FromFaDate();
            if (parsed.HasValue)
            {
                result = parsed.Value;
                return true;
            }
            return false;
        }

        #endregion

        #region HtmlHelper Extensions

        /// <summary>
        /// ایجاد Persian DatePicker با تنظیمات پیش‌فرض
        /// </summary>
        /// <typeparam name="TModel">نوع مدل</typeparam>
        /// <param name="htmlHelper">HtmlHelper</param>
        /// <param name="expression">Expression</param>
        /// <param name="htmlAttributes">ویژگی‌های HTML</param>
        /// <returns>MvcHtmlString</returns>
        public static MvcHtmlString PersianDatePicker<TModel>(
            this HtmlHelper<TModel> htmlHelper,
            string propertyName,
            object htmlAttributes = null)
        {
            var expression = System.Linq.Expressions.Expression.Parameter(typeof(TModel), "m");
            var property = System.Linq.Expressions.Expression.Property(expression, propertyName);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<TModel, string>>(property, expression);

            return htmlHelper.PersianDatePickerFor(lambda, htmlAttributes);
        }

        /// <summary>
        /// ایجاد Persian DatePicker با مقایسه
        /// </summary>
        /// <typeparam name="TModel">نوع مدل</typeparam>
        /// <param name="htmlHelper">HtmlHelper</param>
        /// <param name="propertyName">نام property</param>
        /// <param name="compareWithPropertyName">نام property مقایسه</param>
        /// <param name="htmlAttributes">ویژگی‌های HTML</param>
        /// <returns>MvcHtmlString</returns>
        public static MvcHtmlString PersianDatePickerWithComparison<TModel>(
            this HtmlHelper<TModel> htmlHelper,
            string propertyName,
            string compareWithPropertyName,
            object htmlAttributes = null)
        {
            var expression = System.Linq.Expressions.Expression.Parameter(typeof(TModel), "m");
            var property = System.Linq.Expressions.Expression.Property(expression, propertyName);
            var compareProperty = System.Linq.Expressions.Expression.Property(expression, compareWithPropertyName);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<TModel, string>>(property, expression);
            var compareLambda = System.Linq.Expressions.Expression.Lambda<Func<TModel, string>>(compareProperty, expression);

            return htmlHelper.PersianDatePickerFor(lambda, compareLambda, htmlAttributes);
        }

        /// <summary>
        /// ایجاد Persian DatePicker با تنظیمات سفارشی
        /// </summary>
        /// <typeparam name="TModel">نوع مدل</typeparam>
        /// <param name="htmlHelper">HtmlHelper</param>
        /// <param name="propertyName">نام property</param>
        /// <param name="options">تنظیمات</param>
        /// <param name="htmlAttributes">ویژگی‌های HTML</param>
        /// <returns>MvcHtmlString</returns>
        public static MvcHtmlString PersianDatePickerWithOptions<TModel>(
            this HtmlHelper<TModel> htmlHelper,
            string propertyName,
            PersianDatePickerOptions options,
            object htmlAttributes = null)
        {
            var expression = System.Linq.Expressions.Expression.Parameter(typeof(TModel), "m");
            var property = System.Linq.Expressions.Expression.Property(expression, propertyName);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<TModel, string>>(property, expression);

            return htmlHelper.PersianDatePickerFor(lambda, htmlAttributes, options);
        }

        /// <summary>
        /// ایجاد Persian DatePicker برای تاریخ شروع
        /// </summary>
        /// <typeparam name="TModel">نوع مدل</typeparam>
        /// <param name="htmlHelper">HtmlHelper</param>
        /// <param name="htmlAttributes">ویژگی‌های HTML</param>
        /// <returns>MvcHtmlString</returns>
        public static MvcHtmlString PersianStartDatePicker<TModel>(
            this HtmlHelper<TModel> htmlHelper,
            object htmlAttributes = null)
        {
            return htmlHelper.PersianDatePicker("ValidFromShamsi", htmlAttributes);
        }

        /// <summary>
        /// ایجاد Persian DatePicker برای تاریخ پایان
        /// </summary>
        /// <typeparam name="TModel">نوع مدل</typeparam>
        /// <param name="htmlHelper">HtmlHelper</param>
        /// <param name="htmlAttributes">ویژگی‌های HTML</param>
        /// <returns>MvcHtmlString</returns>
        public static MvcHtmlString PersianEndDatePicker<TModel>(
            this HtmlHelper<TModel> htmlHelper,
            object htmlAttributes = null)
        {
            return htmlHelper.PersianDatePickerWithComparison("ValidToShamsi", "ValidFromShamsi", htmlAttributes);
        }

        /// <summary>
        /// ایجاد Persian DatePicker برای محدوده تاریخ
        /// </summary>
        /// <typeparam name="TModel">نوع مدل</typeparam>
        /// <param name="htmlHelper">HtmlHelper</param>
        /// <param name="htmlAttributes">ویژگی‌های HTML</param>
        /// <returns>MvcHtmlString</returns>
        public static MvcHtmlString PersianDateRangePicker<TModel>(
            this HtmlHelper<TModel> htmlHelper,
            object htmlAttributes = null)
        {
            var startDate = htmlHelper.PersianStartDatePicker(htmlAttributes);
            var endDate = htmlHelper.PersianEndDatePicker(htmlAttributes);
            
            return MvcHtmlString.Create(startDate.ToHtmlString() + endDate.ToHtmlString());
        }

        #endregion

        #region Decimal Extensions

        /// <summary>
        /// تبدیل decimal به رشته IRR با فرمت سه‌رقمی
        /// </summary>
        public static string ToIrrString(this decimal value, string format = "N0")
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// تبدیل decimal به رشته IRR با جداکننده هزارگان
        /// </summary>
        public static string ToIrrStringWithSeparator(this decimal value)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// تبدیل decimal به رشته IRR با ارقام فارسی
        /// </summary>
        public static string ToIrrStringPersian(this decimal value)
        {
            var formatted = value.ToString("N0", CultureInfo.InvariantCulture);
            return RegexHelper.ToPersianDigits(formatted);
        }

        #endregion
    }
}
