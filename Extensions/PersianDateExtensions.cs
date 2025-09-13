using System;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Base;

namespace ClinicApp.Extensions
{
    /// <summary>
    /// Extension Methods برای Persian DatePicker
    /// طبق اصول DRY و SRP طراحی شده است
    /// </summary>
    public static class PersianDateExtensions
    {
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
    }
}
