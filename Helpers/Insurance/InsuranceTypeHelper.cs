using System;
using System.ComponentModel;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.Helpers.Insurance
{
    /// <summary>
    /// Helper class برای تبدیل InsuranceType enum به string
    /// </summary>
    public static class InsuranceTypeHelper
    {
        /// <summary>
        /// تبدیل InsuranceType به string
        /// </summary>
        /// <param name="insuranceType">نوع بیمه</param>
        /// <returns>نام نوع بیمه</returns>
        public static string ToDisplayString(InsuranceType insuranceType)
        {
            return insuranceType switch
            {
                InsuranceType.Primary => "Primary",
                InsuranceType.Supplementary => "Supplementary",
                _ => throw new ArgumentException($"نوع بیمه نامعتبر: {insuranceType}")
            };
        }

        /// <summary>
        /// تبدیل InsuranceType به string فارسی
        /// </summary>
        /// <param name="insuranceType">نوع بیمه</param>
        /// <returns>نام فارسی نوع بیمه</returns>
        public static string ToPersianString(InsuranceType insuranceType)
        {
            return insuranceType switch
            {
                InsuranceType.Primary => "بیمه پایه",
                InsuranceType.Supplementary => "بیمه تکمیلی",
                _ => throw new ArgumentException($"نوع بیمه نامعتبر: {insuranceType}")
            };
        }

        /// <summary>
        /// تبدیل string به InsuranceType
        /// </summary>
        /// <param name="insuranceTypeString">نام نوع بیمه</param>
        /// <returns>نوع بیمه</returns>
        public static InsuranceType FromString(string insuranceTypeString)
        {
            return insuranceTypeString?.ToLower() switch
            {
                "primary" => InsuranceType.Primary,
                "supplementary" => InsuranceType.Supplementary,
                _ => throw new ArgumentException($"نوع بیمه نامعتبر: {insuranceTypeString}")
            };
        }
    }
}
