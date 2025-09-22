using System;
using System.Resources;
using System.Reflection;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper class for managing error messages across the application
    /// کلاس کمکی برای مدیریت پیام‌های خطا در سراسر برنامه
    /// </summary>
    public static class ErrorMessageHelper
    {
        private static readonly ResourceManager _resourceManager;

        static ErrorMessageHelper()
        {
            _resourceManager = new ResourceManager("ClinicApp.Resources.ErrorMessages", Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Get error message by key
        /// دریافت پیام خطا با کلید
        /// </summary>
        /// <param name="key">Error message key</param>
        /// <returns>Error message</returns>
        public static string GetErrorMessage(string key)
        {
            try
            {
                return _resourceManager.GetString(key) ?? "خطای نامشخص";
            }
            catch (Exception)
            {
                return "خطای نامشخص";
            }
        }

        /// <summary>
        /// Get error message with parameters
        /// دریافت پیام خطا با پارامترها
        /// </summary>
        /// <param name="key">Error message key</param>
        /// <param name="parameters">Parameters to format the message</param>
        /// <returns>Formatted error message</returns>
        public static string GetErrorMessage(string key, params object[] parameters)
        {
            try
            {
                var message = _resourceManager.GetString(key) ?? "خطای نامشخص";
                return string.Format(message, parameters);
            }
            catch (Exception)
            {
                return "خطای نامشخص";
            }
        }

        #region Supplementary Tariff Error Messages

        /// <summary>
        /// General system error message
        /// پیام خطای عمومی سیستم
        /// </summary>
        public static string SystemError => GetErrorMessage("SupplementaryTariff_SystemError");

        /// <summary>
        /// Validation error message
        /// پیام خطای اعتبارسنجی
        /// </summary>
        public static string ValidationError => GetErrorMessage("SupplementaryTariff_ValidationError");

        /// <summary>
        /// Invalid tariff ID error message
        /// پیام خطای شناسه تعرفه نامعتبر
        /// </summary>
        public static string InvalidTariffId => GetErrorMessage("SupplementaryTariff_InvalidTariffId");

        /// <summary>
        /// Create error message
        /// پیام خطای ایجاد
        /// </summary>
        public static string CreateError => GetErrorMessage("SupplementaryTariff_CreateError");

        /// <summary>
        /// Edit error message
        /// پیام خطای ویرایش
        /// </summary>
        public static string EditError => GetErrorMessage("SupplementaryTariff_EditError");

        /// <summary>
        /// Delete error message
        /// پیام خطای حذف
        /// </summary>
        public static string DeleteError => GetErrorMessage("SupplementaryTariff_DeleteError");

        /// <summary>
        /// Load data error message
        /// پیام خطای بارگذاری داده
        /// </summary>
        public static string LoadDataError => GetErrorMessage("SupplementaryTariff_LoadDataError");

        /// <summary>
        /// Filter error message
        /// پیام خطای فیلتر
        /// </summary>
        public static string FilterError => GetErrorMessage("SupplementaryTariff_FilterError");

        /// <summary>
        /// Initialization error message
        /// پیام خطای راه‌اندازی
        /// </summary>
        public static string InitError => GetErrorMessage("SupplementaryTariff_InitError");

        /// <summary>
        /// Save error message
        /// پیام خطای ذخیره
        /// </summary>
        public static string SaveError => GetErrorMessage("SupplementaryTariff_SaveError");

        /// <summary>
        /// Network error message
        /// پیام خطای شبکه
        /// </summary>
        public static string NetworkError => GetErrorMessage("SupplementaryTariff_NetworkError");

        /// <summary>
        /// Permission error message
        /// پیام خطای مجوز
        /// </summary>
        public static string PermissionError => GetErrorMessage("SupplementaryTariff_PermissionError");

        /// <summary>
        /// Timeout error message
        /// پیام خطای زمان
        /// </summary>
        public static string TimeoutError => GetErrorMessage("SupplementaryTariff_TimeoutError");

        /// <summary>
        /// Data not found error message
        /// پیام خطای داده یافت نشد
        /// </summary>
        public static string DataNotFound => GetErrorMessage("SupplementaryTariff_DataNotFound");

        /// <summary>
        /// Invalid data error message
        /// پیام خطای داده نامعتبر
        /// </summary>
        public static string InvalidData => GetErrorMessage("SupplementaryTariff_InvalidData");

        /// <summary>
        /// Duplicate data error message
        /// پیام خطای داده تکراری
        /// </summary>
        public static string DuplicateData => GetErrorMessage("SupplementaryTariff_DuplicateData");

        /// <summary>
        /// Calculation error message
        /// پیام خطای محاسبه
        /// </summary>
        public static string CalculationError => GetErrorMessage("SupplementaryTariff_CalculationError");

        /// <summary>
        /// Validation not available error message
        /// پیام خطای عدم دسترسی به اعتبارسنجی
        /// </summary>
        public static string ValidationNotAvailable => GetErrorMessage("SupplementaryTariff_ValidationNotAvailable");

        /// <summary>
        /// API not available error message
        /// پیام خطای عدم دسترسی به API
        /// </summary>
        public static string APINotAvailable => GetErrorMessage("SupplementaryTariff_APINotAvailable");

        /// <summary>
        /// UI not available error message
        /// پیام خطای عدم دسترسی به UI
        /// </summary>
        public static string UINotAvailable => GetErrorMessage("SupplementaryTariff_UINotAvailable");

        #endregion
    }
}
