using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Filters;
using ClinicApp.Extensions;

namespace ClinicApp.ViewModels.Base
{
    /// <summary>
    /// Base ViewModel برای تاریخ‌های شمسی
    /// طبق اصول DRY و SRP طراحی شده است
    /// </summary>
    public abstract class PersianDateViewModel
    {
        /// <summary>
        /// تبدیل تاریخ‌های شمسی به میلادی
        /// </summary>
        public virtual void ConvertPersianDatesToGregorian()
        {
            // این متد باید در کلاس‌های فرزند override شود
        }

        /// <summary>
        /// تبدیل تاریخ‌های میلادی به شمسی
        /// </summary>
        public virtual void ConvertGregorianDatesToPersian()
        {
            // این متد باید در کلاس‌های فرزند override شود
        }
    }

    /// <summary>
    /// Base ViewModel برای تاریخ‌های شمسی با validation
    /// </summary>
    public abstract class PersianDateViewModelWithValidation : PersianDateViewModel
    {
        /// <summary>
        /// اعتبارسنجی تاریخ‌های شمسی
        /// </summary>
        /// <returns>true اگر معتبر باشد</returns>
        public virtual bool ValidatePersianDates()
        {
            try
            {
                ConvertPersianDatesToGregorian();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Helper class برای ایجاد properties تاریخ شمسی
    /// </summary>
    public static class PersianDatePropertyHelper
    {
        /// <summary>
        /// ایجاد property برای تاریخ شروع
        /// </summary>
        /// <param name="propertyName">نام property</param>
        /// <param name="isRequired">آیا اجباری است</param>
        /// <param name="minYear">حداقل سال</param>
        /// <param name="maxYear">حداکثر سال</param>
        /// <returns>Property definition</returns>
        public static string CreateStartDateProperty(string propertyName = "ValidFromShamsi", bool isRequired = true, int minYear = 700, int maxYear = 1500)
        {
            return $@"
        [Required(ErrorMessage = ""تاریخ شروع الزامی است"")]
        [PersianDate(IsRequired = {isRequired.ToString().ToLower()}, MustBeFutureDate = false, MinYear = {minYear}, MaxYear = {maxYear},
            InvalidFormatMessage = ""فرمت تاریخ شروع نامعتبر است. (مثال: 1404/06/23)"",
            YearRangeMessage = ""سال تاریخ شروع باید بین {minYear} تا {maxYear} باشد."")]
        [Display(Name = ""تاریخ شروع"")]
        public string {propertyName} {{ get; set; }}";
        }

        /// <summary>
        /// ایجاد property برای تاریخ پایان
        /// </summary>
        /// <param name="propertyName">نام property</param>
        /// <param name="isRequired">آیا اجباری است</param>
        /// <param name="minYear">حداقل سال</param>
        /// <param name="maxYear">حداکثر سال</param>
        /// <returns>Property definition</returns>
        public static string CreateEndDateProperty(string propertyName = "ValidToShamsi", bool isRequired = false, int minYear = 700, int maxYear = 1500)
        {
            return $@"
        [PersianDate(IsRequired = {isRequired.ToString().ToLower()}, MustBeFutureDate = false, MinYear = {minYear}, MaxYear = {maxYear},
            InvalidFormatMessage = ""فرمت تاریخ پایان نامعتبر است. (مثال: 1404/06/23)"",
            YearRangeMessage = ""سال تاریخ پایان باید بین {minYear} تا {maxYear} باشد."")]
        [Display(Name = ""تاریخ پایان"")]
        public string {propertyName} {{ get; set; }}";
        }

        /// <summary>
        /// ایجاد property برای تاریخ میلادی
        /// </summary>
        /// <param name="propertyName">نام property</param>
        /// <param name="isNullable">آیا nullable است</param>
        /// <returns>Property definition</returns>
        public static string CreateGregorianDateProperty(string propertyName = "ValidFrom", bool isNullable = false)
        {
            var type = isNullable ? "DateTime?" : "DateTime";
            return $@"
        [HiddenInput(DisplayValue = false)]
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public {type} {propertyName} {{ get; set; }}";
        }
    }
}
