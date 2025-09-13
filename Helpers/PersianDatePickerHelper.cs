using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// HtmlHelper مرکزی برای Persian DatePicker
    /// طبق اصول DRY و SRP طراحی شده است
    /// </summary>
    public static class PersianDatePickerHelper
    {
        /// <summary>
        /// ایجاد Persian DatePicker با تنظیمات کامل
        /// </summary>
        /// <typeparam name="TModel">نوع مدل</typeparam>
        /// <param name="htmlHelper">HtmlHelper</param>
        /// <param name="expression">Expression برای property</param>
        /// <param name="htmlAttributes">ویژگی‌های HTML</param>
        /// <param name="options">تنظیمات DatePicker</param>
        /// <returns>MvcHtmlString</returns>
        public static MvcHtmlString PersianDatePickerFor<TModel>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, string>> expression,
            object htmlAttributes = null,
            PersianDatePickerOptions options = null)
        {
            // تنظیمات پیش‌فرض
            options = options ?? new PersianDatePickerOptions();
            
            // دریافت metadata
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            string propertyName = ExpressionHelper.GetExpressionText(expression);
            string fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(propertyName);
            string id = TagBuilder.CreateSanitizedId(fullName);
            
            // ایجاد HTML attributes
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            attributes["class"] = (attributes.ContainsKey("class") ? attributes["class"] + " " : "") + "form-control persian-datepicker";
            attributes["data-datepicker-id"] = id;
            
            // ایجاد TextBox
            var textBox = htmlHelper.TextBoxFor(expression, attributes);
            
            // ایجاد JavaScript
            var script = GenerateDatePickerScript(id, options);
            
            return MvcHtmlString.Create(textBox.ToHtmlString() + script);
        }

        /// <summary>
        /// ایجاد Persian DatePicker با مقایسه تاریخ
        /// </summary>
        /// <typeparam name="TModel">نوع مدل</typeparam>
        /// <param name="htmlHelper">HtmlHelper</param>
        /// <param name="expression">Expression برای property</param>
        /// <param name="compareWithExpression">Expression برای مقایسه</param>
        /// <param name="htmlAttributes">ویژگی‌های HTML</param>
        /// <param name="options">تنظیمات DatePicker</param>
        /// <returns>MvcHtmlString</returns>
        public static MvcHtmlString PersianDatePickerFor<TModel>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, string>> expression,
            Expression<Func<TModel, string>> compareWithExpression,
            object htmlAttributes = null,
            PersianDatePickerOptions options = null)
        {
            // تنظیمات پیش‌فرض
            options = options ?? new PersianDatePickerOptions();
            
            // دریافت metadata
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            string propertyName = ExpressionHelper.GetExpressionText(expression);
            string fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(propertyName);
            string id = TagBuilder.CreateSanitizedId(fullName);
            
            // دریافت metadata برای مقایسه
            var compareMetadata = ModelMetadata.FromLambdaExpression(compareWithExpression, htmlHelper.ViewData);
            string comparePropertyName = ExpressionHelper.GetExpressionText(compareWithExpression);
            string compareFullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(comparePropertyName);
            string compareId = TagBuilder.CreateSanitizedId(compareFullName);
            
            // ایجاد HTML attributes
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            attributes["class"] = (attributes.ContainsKey("class") ? attributes["class"] + " " : "") + "form-control persian-datepicker";
            attributes["data-datepicker-id"] = id;
            attributes["data-compare-with"] = compareId;
            
            // ایجاد TextBox
            var textBox = htmlHelper.TextBoxFor(expression, attributes);
            
            // ایجاد JavaScript با مقایسه
            var script = GenerateDatePickerScriptWithComparison(id, compareId, options);
            
            return MvcHtmlString.Create(textBox.ToHtmlString() + script);
        }

        /// <summary>
        /// ایجاد JavaScript برای DatePicker
        /// </summary>
        /// <param name="id">شناسه element</param>
        /// <param name="options">تنظیمات</param>
        /// <returns>JavaScript string</returns>
        private static string GenerateDatePickerScript(string id, PersianDatePickerOptions options)
        {
            return $@"
                <script>
                    $(document).ready(function() {{
                        // Initialize Persian DatePicker
                        $('#{id}').persianDatepicker({{
                            format: '{options.Format}',
                            altField: '.observer-example-alt',
                            altFormat: '{options.Format}',
                            observer: true,
                            timePicker: {{
                                enabled: false
                            }},
                            minDate: new persianDate([{options.MinYear}, 1, 1]),
                            maxDate: new persianDate([{options.MaxYear}, 12, 29]),
                            onSelect: function(unix) {{
                                $('#{id}').removeClass('is-invalid');
                                $('#{id}').trigger('datepicker:change');
                            }}
                        }});

                        // Real-time validation
                        $('#{id}').on('blur', function() {{
                            if ($(this).val().trim() === '' && $(this).prop('required')) {{
                                $(this).addClass('is-invalid');
                            }} else {{
                                $(this).removeClass('is-invalid');
                            }}
                        }});
                    }});
                </script>";
        }

        /// <summary>
        /// ایجاد JavaScript برای DatePicker با مقایسه
        /// </summary>
        /// <param name="id">شناسه element</param>
        /// <param name="compareId">شناسه element مقایسه</param>
        /// <param name="options">تنظیمات</param>
        /// <returns>JavaScript string</returns>
        private static string GenerateDatePickerScriptWithComparison(string id, string compareId, PersianDatePickerOptions options)
        {
            return $@"
                <script>
                    $(document).ready(function() {{
                        // Initialize Persian DatePicker
                        $('#{id}').persianDatepicker({{
                            format: '{options.Format}',
                            altField: '.observer-example-alt',
                            altFormat: '{options.Format}',
                            observer: true,
                            timePicker: {{
                                enabled: false
                            }},
                            minDate: new persianDate([{options.MinYear}, 1, 1]),
                            maxDate: new persianDate([{options.MaxYear}, 12, 29]),
                            onSelect: function(unix) {{
                                $('#{id}').removeClass('is-invalid');
                                validateDateComparison();
                                $('#{id}').trigger('datepicker:change');
                            }}
                        }});

                        // Date comparison validation
                        function validateDateComparison() {{
                            var currentDate = $('#{id}').val();
                            var compareDate = $('#{compareId}').val();
                            
                            if (currentDate && compareDate) {{
                                try {{
                                    var current = new persianDate(currentDate.split('/')).toDate();
                                    var compare = new persianDate(compareDate.split('/')).toDate();
                                    
                                    if (current >= compare) {{
                                        $('#{id}').addClass('is-invalid');
                                        $('#{id}').next('.invalid-feedback').remove();
                                        $('#{id}').after('<div class=""invalid-feedback"">{options.ComparisonErrorMessage}</div>');
                                    }} else {{
                                        $('#{id}').removeClass('is-invalid');
                                        $('#{id}').next('.invalid-feedback').remove();
                                    }}
                                }} catch (e) {{
                                    console.log('Date comparison error:', e);
                                }}
                            }}
                        }}

                        // Real-time validation
                        $('#{id}').on('blur', function() {{
                            if ($(this).val().trim() === '' && $(this).prop('required')) {{
                                $(this).addClass('is-invalid');
                            }} else {{
                                $(this).removeClass('is-invalid');
                                validateDateComparison();
                            }}
                        }});

                        // Listen for changes in compare field
                        $('#{compareId}').on('datepicker:change', function() {{
                            validateDateComparison();
                        }});
                    }});
                </script>";
        }
    }

    /// <summary>
    /// تنظیمات Persian DatePicker
    /// </summary>
    public class PersianDatePickerOptions
    {
        /// <summary>
        /// فرمت تاریخ
        /// </summary>
        public string Format { get; set; } = "YYYY/MM/DD";

        /// <summary>
        /// حداقل سال
        /// </summary>
        public int MinYear { get; set; } = 700;

        /// <summary>
        /// حداکثر سال
        /// </summary>
        public int MaxYear { get; set; } = 1500;

        /// <summary>
        /// پیام خطای مقایسه
        /// </summary>
        public string ComparisonErrorMessage { get; set; } = "تاریخ نمی‌تواند قبل از تاریخ مقایسه باشد.";

        /// <summary>
        /// فعال بودن time picker
        /// </summary>
        public bool EnableTimePicker { get; set; } = false;
    }
}