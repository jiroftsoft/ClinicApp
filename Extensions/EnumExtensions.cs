using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace ClinicApp.Extensions
{
    /// <summary>
    /// Extension Methods برای Enum ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. دریافت Description از Enum values
    /// 2. تبدیل Enum به SelectList برای DropDown
    /// 3. بررسی Flags برای [Flags] enums
    /// 4. دریافت Custom Attributes
    /// </summary>
    public static class EnumExtensions
    {
        #region Description Extensions

        /// <summary>
        /// دریافت Description از Enum value
        /// </summary>
        public static string GetDescription(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field == null) return enumValue.ToString();

            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? enumValue.ToString();
        }

        /// <summary>
        /// دریافت DisplayName از Enum value
        /// پشتیبانی از DisplayAttribute و DisplayNameAttribute
        /// </summary>
        public static string GetDisplayName(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field == null) return enumValue.ToString();

            // ابتدا DisplayAttribute را بررسی می‌کنیم (System.ComponentModel.DataAnnotations)
            var displayAttribute = field.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
            if (displayAttribute != null)
                return displayAttribute.Name ?? enumValue.ToString();

            // سپس DisplayNameAttribute را بررسی می‌کنیم (System.ComponentModel)
            var displayNameAttribute = field.GetCustomAttribute<DisplayNameAttribute>();
            return displayNameAttribute?.DisplayName ?? enumValue.ToString();
        }

        /// <summary>
        /// دریافت Custom Attribute از Enum value
        /// </summary>
        public static T GetAttribute<T>(this Enum enumValue) where T : Attribute
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            return field?.GetCustomAttribute<T>();
        }

        #endregion

        #region SelectList Extensions

        /// <summary>
        /// تبدیل Enum به SelectList
        /// </summary>
        public static SelectList ToSelectList<T>(this T enumValue, string valueField = "Value", string textField = "Text", object selectedValue = null) where T : struct, Enum
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            var values = Enum.GetValues(typeof(T)).Cast<T>();
            var items = values.Select(v => new SelectListItem
            {
                Value = Convert.ToInt32(v).ToString(),
                Text = v.GetDescription(),
                Selected = selectedValue != null && Convert.ToInt32(v).Equals(Convert.ToInt32(selectedValue))
            }).ToList();

            return new SelectList(items, valueField, textField, selectedValue);
        }

        /// <summary>
        /// تبدیل Enum به SelectList با فیلتر
        /// </summary>
        public static SelectList ToSelectList<T>(this T enumValue, Func<T, bool> filter, string valueField = "Value", string textField = "Text", object selectedValue = null) where T : struct, Enum
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            var values = Enum.GetValues(typeof(T)).Cast<T>().Where(filter);
            var items = values.Select(v => new SelectListItem
            {
                Value = Convert.ToInt32(v).ToString(),
                Text = v.GetDescription(),
                Selected = selectedValue != null && Convert.ToInt32(v).Equals(Convert.ToInt32(selectedValue))
            }).ToList();

            return new SelectList(items, valueField, textField, selectedValue);
        }

        /// <summary>
        /// تبدیل Enum به SelectList با مقدار پیش‌فرض
        /// </summary>
        public static SelectList ToSelectListWithDefault<T>(this T enumValue, string defaultText = "انتخاب کنید", string defaultValue = "", object selectedValue = null) where T : struct, Enum
        {
            var selectList = enumValue.ToSelectList(selectedValue: selectedValue);
            var items = selectList.Items.Cast<SelectListItem>().ToList();
            
            items.Insert(0, new SelectListItem
            {
                Value = defaultValue,
                Text = defaultText,
                Selected = selectedValue != null && selectedValue.ToString() == defaultValue
            });

            return new SelectList(items, "Value", "Text", selectedValue);
        }

        #endregion

        #region Flags Extensions

        /// <summary>
        /// بررسی وجود هر یک از Flags
        /// </summary>
        public static bool HasAnyFlag<T>(this T enumValue, T flags) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            var enumValueInt = Convert.ToInt32(enumValue);
            var flagsInt = Convert.ToInt32(flags);
            
            return (enumValueInt & flagsInt) != 0;
        }

        /// <summary>
        /// بررسی وجود تمام Flags
        /// </summary>
        public static bool HasAllFlags<T>(this T enumValue, T flags) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            var enumValueInt = Convert.ToInt32(enumValue);
            var flagsInt = Convert.ToInt32(flags);
            
            return (enumValueInt & flagsInt) == flagsInt;
        }

        /// <summary>
        /// اضافه کردن Flag
        /// </summary>
        public static T AddFlag<T>(this T enumValue, T flag) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            var enumValueInt = Convert.ToInt32(enumValue);
            var flagInt = Convert.ToInt32(flag);
            
            return (T)Enum.ToObject(typeof(T), enumValueInt | flagInt);
        }

        /// <summary>
        /// حذف Flag
        /// </summary>
        public static T RemoveFlag<T>(this T enumValue, T flag) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            var enumValueInt = Convert.ToInt32(enumValue);
            var flagInt = Convert.ToInt32(flag);
            
            return (T)Enum.ToObject(typeof(T), enumValueInt & ~flagInt);
        }

        /// <summary>
        /// دریافت تمام Flags فعال
        /// </summary>
        public static IEnumerable<T> GetActiveFlags<T>(this T enumValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            var enumValueInt = Convert.ToInt32(enumValue);
            return Enum.GetValues(typeof(T)).Cast<T>().Where(flag => enumValue.HasAnyFlag(flag));
        }

        #endregion

        #region Parsing Extensions

        /// <summary>
        /// تبدیل رشته به Enum با مقدار پیش‌فرض
        /// </summary>
        public static T ParseEnum<T>(this string value, T defaultValue = default(T)) where T : struct, IConvertible
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (Enum.TryParse<T>(value, true, out T result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// تبدیل عدد به Enum با مقدار پیش‌فرض
        /// </summary>
        public static T ParseEnum<T>(this int value, T defaultValue = default(T)) where T : struct, IConvertible
        {
            if (Enum.IsDefined(typeof(T), value))
                return (T)Enum.ToObject(typeof(T), value);

            return defaultValue;
        }

        #endregion

        #region Medical Extensions

        /// <summary>
        /// دریافت نام فارسی برای Enum های پزشکی
        /// </summary>
        public static string GetPersianName(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field == null) return enumValue.ToString();

            // بررسی برای PersianName attribute
            var persianNameAttr = field.GetCustomAttribute<PersianNameAttribute>();
            if (persianNameAttr != null)
                return persianNameAttr.Name;

            // بررسی برای Description
            var descriptionAttr = field.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttr != null)
                return descriptionAttr.Description;

            return enumValue.ToString();
        }

        /// <summary>
        /// دریافت رنگ CSS برای Enum های وضعیت
        /// </summary>
        public static string GetStatusColor(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field == null) return "default";

            var colorAttr = field.GetCustomAttribute<StatusColorAttribute>();
            return colorAttr?.Color ?? "default";
        }

        /// <summary>
        /// دریافت آیکون برای Enum های پزشکی
        /// </summary>
        public static string GetIcon(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field == null) return "";

            var iconAttr = field.GetCustomAttribute<IconAttribute>();
            return iconAttr?.Icon ?? "";
        }

        #endregion
    }

    #region Custom Attributes

    /// <summary>
    /// Attribute برای نام فارسی
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PersianNameAttribute : Attribute
    {
        public string Name { get; }

        public PersianNameAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Attribute برای رنگ وضعیت
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class StatusColorAttribute : Attribute
    {
        public string Color { get; }

        public StatusColorAttribute(string color)
        {
            Color = color;
        }
    }

    /// <summary>
    /// Attribute برای آیکون
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class IconAttribute : Attribute
    {
        public string Icon { get; }

        public IconAttribute(string icon)
        {
            Icon = icon;
        }
    }

    #endregion
}
