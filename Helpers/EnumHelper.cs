using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// ✅ Enterprise-Grade Enum Helper
    /// 
    /// Single Responsibility: Convert enums to SelectList with Display attributes support
    /// 
    /// Usage:
    ///   var genderList = EnumHelper.GetSelectList<Gender>(selectedValue);
    /// 
    /// طبق قرارداد: DEVELOPMENT_CONTRACT.md - "Reuse existing abstractions"
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Convert enum to SelectList with Display attribute support
        /// </summary>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <param name="selectedValue">Currently selected value (optional)</param>
        /// <returns>SelectList for use in DropDownListFor</returns>
        public static SelectList GetSelectList<TEnum>(object selectedValue = null) where TEnum : struct, Enum
        {
            var items = GetSelectListItems<TEnum>();
            return new SelectList(items, "Value", "Text", selectedValue);
        }

        /// <summary>
        /// Convert enum to list of SelectListItem
        /// </summary>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <returns>List of SelectListItem</returns>
        public static List<SelectListItem> GetSelectListItems<TEnum>() where TEnum : struct, Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(value => new SelectListItem
                {
                    Text = GetDisplayName(value),
                    Value = Convert.ToInt32(value).ToString()
                })
                .ToList();
        }

        /// <summary>
        /// Get display name from Display attribute or enum name
        /// </summary>
        /// <param name="enumValue">Enum value</param>
        /// <returns>Display name</returns>
        public static string GetDisplayName<TEnum>(TEnum enumValue) where TEnum : struct, Enum
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            if (fieldInfo == null) return enumValue.ToString();

            var displayAttribute = fieldInfo.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.Name ?? enumValue.ToString();
        }

        /// <summary>
        /// Get all enum values with their display names
        /// </summary>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <returns>Dictionary of enum values and display names</returns>
        public static Dictionary<TEnum, string> GetDisplayNames<TEnum>() where TEnum : struct, Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .ToDictionary(value => value, value => GetDisplayName(value));
        }
    }
}

