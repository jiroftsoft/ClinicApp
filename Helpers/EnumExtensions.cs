using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Extension methods برای enum ها
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// تبدیل enum به SelectList برای استفاده در DropDownList
        /// </summary>
        /// <typeparam name="T">نوع enum</typeparam>
        /// <param name="htmlHelper">HtmlHelper</param>
        /// <returns>SelectList</returns>
        public static SelectList GetEnumSelectList<T>(this HtmlHelper htmlHelper) where T : struct
        {
            var enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new ArgumentException("Type must be an enum");

            var items = Enum.GetValues(enumType)
                .Cast<T>()
                .Select(e => new SelectListItem
                {
                    Value = Convert.ToInt32(e).ToString(),
                    Text = GetEnumDescription(e)
                })
                .ToList();

            return new SelectList(items, "Value", "Text");
        }

        /// <summary>
        /// دریافت توضیحات enum از Description attribute
        /// </summary>
        /// <typeparam name="T">نوع enum</typeparam>
        /// <param name="enumValue">مقدار enum</param>
        /// <returns>توضیحات enum</returns>
        private static string GetEnumDescription<T>(T enumValue) where T : struct
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
                return ((DescriptionAttribute)attributes[0]).Description;

            return enumValue.ToString();
        }

        /// <summary>
        /// تبدیل enum به SelectList با مقدار پیش‌فرض
        /// </summary>
        /// <typeparam name="T">نوع enum</typeparam>
        /// <param name="htmlHelper">HtmlHelper</param>
        /// <param name="defaultText">متن پیش‌فرض</param>
        /// <returns>SelectList</returns>
        public static SelectList GetEnumSelectList<T>(this HtmlHelper htmlHelper, string defaultText) where T : struct
        {
            var selectList = GetEnumSelectList<T>(htmlHelper);
            var items = new List<SelectListItem> { new SelectListItem { Text = defaultText, Value = "" } };
            items.AddRange(selectList);

            return new SelectList(items, "Value", "Text");
        }
    }
}
