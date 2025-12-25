using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace ClinicApp.Extensions
{
    /// <summary>
    /// Extension methods for Object operations
    /// متدهای کمکی برای عملیات روی اشیا
    /// </summary>
    public static class ObjectExtensions
    {
        #region DeepClone

        /// <summary>
        /// Creates a deep copy of an object using JSON serialization
        /// ایجاد کپی عمیق از یک شیء
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="obj">Object to clone</param>
        /// <returns>Deep cloned object</returns>
        /// <example>
        /// var original = new User { Name = "Ali", Age = 30 };
        /// var clone = original.DeepClone();
        /// clone.Name = "Reza"; // original.Name is still "Ali"
        /// </example>
        public static T DeepClone<T>(this T obj) where T : class
        {
            if (obj == null)
                return null;

            var json = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(json);
        }

        #endregion

        #region ToDictionary

        /// <summary>
        /// Converts an object's properties to a dictionary
        /// تبدیل Properties یک شیء به Dictionary
        /// </summary>
        /// <param name="obj">Object to convert</param>
        /// <returns>Dictionary of property names and values</returns>
        /// <example>
        /// var user = new { Name = "Ali", Age = 30 };
        /// var dict = user.ToDictionary();
        /// // { "Name": "Ali", "Age": 30 }
        /// </example>
        public static Dictionary<string, object> ToDictionary(this object obj)
        {
            if (obj == null)
                return new Dictionary<string, object>();

            return obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, p => p.GetValue(obj));
        }

        #endregion

        #region Null Checking

        /// <summary>
        /// Checks if an object is null
        /// بررسی null بودن
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>True if null</returns>
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        /// <summary>
        /// Checks if an object is not null
        /// بررسی null نبودن
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>True if not null</returns>
        public static bool IsNotNull(this object obj)
        {
            return obj != null;
        }

        #endregion

        #region Safe Casting

        /// <summary>
        /// Safely casts object to specified type
        /// تبدیل امن به نوع مشخص
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="obj">Object to cast</param>
        /// <returns>Casted object or null</returns>
        public static T As<T>(this object obj) where T : class
        {
            return obj as T;
        }

        #endregion

        #region GetPropertyValue

        /// <summary>
        /// Gets the value of a property by name
        /// دریافت مقدار Property به وسیله نام
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>Property value or null</returns>
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            if (obj == null)
                return null;

            var property = obj.GetType().GetProperty(propertyName);
            return property?.GetValue(obj);
        }

        /// <summary>
        /// Gets the value of a property by name with type casting
        /// دریافت مقدار Property با تبدیل نوع
        /// </summary>
        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            var value = obj.GetPropertyValue(propertyName);
            
            if (value == null)
                return default(T);

            try
            {
                return (T)value;
            }
            catch
            {
                return default(T);
            }
        }

        #endregion

        #region SetPropertyValue

        /// <summary>
        /// Sets the value of a property by name
        /// تنظیم مقدار Property به وسیله نام
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="value">Value to set</param>
        /// <returns>True if successful</returns>
        public static bool SetPropertyValue(this object obj, string propertyName, object value)
        {
            if (obj == null)
                return false;

            try
            {
                var property = obj.GetType().GetProperty(propertyName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(obj, value);
                    return true;
                }
            }
            catch
            {
                // Failed to set property
            }

            return false;
        }

        #endregion

        #region ToJson

        /// <summary>
        /// Converts object to JSON string
        /// تبدیل به JSON
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="formatting">JSON formatting</param>
        /// <returns>JSON string</returns>
        public static string ToJson(this object obj, Formatting formatting = Formatting.None)
        {
            if (obj == null)
                return null;

            return JsonConvert.SerializeObject(obj, formatting);
        }

        #endregion
    }
}
