using System;
using System.Collections.Generic;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper class for validation operations
    /// کلاس کمکی برای اعتبارسنجی
    /// </summary>
    public static class ValidationHelper
    {
        #region Basic Validation

        /// <summary>
        /// Checks if a value is valid (not null or empty)
        /// بررسی معتبر بودن مقدار
        /// </summary>
        public static bool IsValid(object value)
        {
            if (value == null)
                return false;

            if (value is string str)
                return !string.IsNullOrWhiteSpace(str);

            return true;
        }

        /// <summary>
        /// Validates all conditions
        /// اعتبارسنجی تمام شرایط
        /// </summary>
        public static ValidationResult ValidateAll(params Func<bool>[] validations)
        {
            if (validations == null)
                return ValidationResult.Success;

            var errors = new List<string>();

            foreach (var validation in validations)
            {
                try
                {
                    if (!validation())
                    {
                        errors.Add("Validation failed");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            return errors.Count == 0
                ? ValidationResult.Success
                : ValidationResult.Failure(errors);
        }

        #endregion

        #region Range Validation

        /// <summary>
        /// Checks if value is within range
        /// بررسی قرار گرفتن در محدوده
        /// </summary>
        public static bool IsInRange<T>(T value, T min, T max) where T : IComparable<T>
        {
            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }

        /// <summary>
        /// Checks if number is positive
        /// بررسی مثبت بودن عدد
        /// </summary>
        public static bool IsPositive(decimal value)
        {
            return value > 0;
        }

        /// <summary>
        /// Checks if number is non-negative
        /// بررسی غیرمنفی بودن
        /// </summary>
        public static bool IsNonNegative(decimal value)
        {
            return value >= 0;
        }

        #endregion

        #region List Validation

        /// <summary>
        /// Checks if value is in allowed list
        /// بررسی وجود در لیست مجاز
        /// </summary>
        public static bool IsInList<T>(T value, params T[] allowedValues)
        {
            if (allowedValues == null || allowedValues.Length == 0)
                return false;

            foreach (var allowed in allowedValues)
            {
                if (EqualityComparer<T>.Default.Equals(value, allowed))
                    return true;
            }

            return false;
        }

        #endregion

        #region String Validation

        /// <summary>
        /// Validates string length
        /// اعتبارسنجی طول رشته
        /// </summary>
        public static bool IsLengthValid(string str, int minLength, int maxLength)
        {
            if (string.IsNullOrEmpty(str))
                return minLength == 0;

            return str.Length >= minLength && str.Length <= maxLength;
        }

        /// <summary>
        /// Validates minimum length
        /// بررسی حداقل طول
        /// </summary>
        public static bool IsMinLength(string str, int minLength)
        {
            return !string.IsNullOrEmpty(str) && str.Length >= minLength;
        }

        /// <summary>
        /// Validates maximum length
        /// بررسی حداکثر طول
        /// </summary>
        public static bool IsMaxLength(string str, int maxLength)
        {
            return string.IsNullOrEmpty(str) || str.Length <= maxLength;
        }

        #endregion

        #region Required Field Validation

        /// <summary>
        /// Checks if required field has value
        /// بررسی فیلد الزامی
        /// </summary>
        public static ValidationResult ValidateRequired(string fieldName, object value)
        {
            if (!IsValid(value))
            {
                return ValidationResult.Failure($"{fieldName} الزامی است");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates multiple required fields
        /// اعتبارسنجی چند فیلد الزامی
        /// </summary>
        public static ValidationResult ValidateRequiredFields(params (string fieldName, object value)[] fields)
        {
            var errors = new List<string>();

            foreach (var (fieldName, value) in fields)
            {
                var result = ValidateRequired(fieldName, value);
                if (!result.IsValid)
                {
                    errors.AddRange(result.Errors);
                }
            }

            return errors.Count == 0
                ? ValidationResult.Success
                : ValidationResult.Failure(errors);
        }

        #endregion

        #region Email & URL Validation (using Extensions)

        /// <summary>
        /// Validates email address
        /// اعتبارسنجی ایمیل
        /// </summary>
        public static ValidationResult ValidateEmail(string email, string fieldName = "ایمیل")
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return ValidationResult.Failure($"{fieldName} الزامی است");
            }

            if (!email.Contains("@"))
            {
                return ValidationResult.Failure($"{fieldName} معتبر نیست");
            }

            return ValidationResult.Success;
        }

        #endregion
    }

    #region ValidationResult Class

    /// <summary>
    /// Represents the result of a validation operation
    /// نتیجه عملیات اعتبارسنجی
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; }

        public ValidationResult()
        {
            Errors = new List<string>();
        }

        public static ValidationResult Success => new ValidationResult { IsValid = true };

        public static ValidationResult Failure(string error)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { error }
            };
        }

        public static ValidationResult Failure(List<string> errors)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors ?? new List<string>()
            };
        }

        public string GetErrorMessage()
        {
            return Errors.Count > 0 ? string.Join(", ", Errors) : string.Empty;
        }
    }

    #endregion
}
