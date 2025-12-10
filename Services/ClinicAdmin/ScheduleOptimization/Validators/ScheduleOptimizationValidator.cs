using System;
using ClinicApp.Core;

namespace ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Validators
{
    /// <summary>
    /// Validator برای اعتبارسنجی درخواست‌های بهینه‌سازی برنامه کاری
    /// 
    /// مسئولیت (SRP):
    /// - اعتبارسنجی پارامترهای ورودی
    /// - بررسی قوانین کسب و کار
    /// - تولید پیام‌های خطا
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط اعتبارسنجی
    /// - Open/Closed: قابل توسعه برای قوانین جدید
    /// </summary>
    public static class ScheduleOptimizationValidator
    {
        /// <summary>
        /// اعتبارسنجی شناسه پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public static ValidationResult ValidateDoctorId(int doctorId)
        {
            if (doctorId <= 0)
            {
                return ValidationResult.Failed("شناسه پزشک باید بزرگتر از صفر باشد.");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// اعتبارسنجی تاریخ
        /// </summary>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <param name="allowPastDates">آیا تاریخ‌های گذشته مجاز هستند؟</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public static ValidationResult ValidateDate(DateTime date, bool allowPastDates = false)
        {
            if (date == default(DateTime))
            {
                return ValidationResult.Failed("تاریخ نامعتبر است.");
            }

            if (!allowPastDates && date.Date < DateTime.Today)
            {
                return ValidationResult.Failed("تاریخ مورد نظر نمی‌تواند در گذشته باشد.");
            }

            // بررسی محدوده منطقی (مثلاً بیش از 1 سال آینده)
            if (date.Date > DateTime.Today.AddYears(1))
            {
                return ValidationResult.Failed("تاریخ مورد نظر نمی‌تواند بیش از یک سال آینده باشد.");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// اعتبارسنجی بازه زمانی
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public static ValidationResult ValidateDateRange(DateTime startDate, DateTime endDate)
        {
            var startValidation = ValidateDate(startDate);
            if (!startValidation.IsValid)
            {
                return startValidation;
            }

            var endValidation = ValidateDate(endDate);
            if (!endValidation.IsValid)
            {
                return endValidation;
            }

            if (startDate >= endDate)
            {
                return ValidationResult.Failed("تاریخ شروع باید قبل از تاریخ پایان باشد.");
            }

            // بررسی محدوده منطقی (مثلاً بیش از 3 ماه)
            var daysDifference = (endDate - startDate).Days;
            if (daysDifference > 90)
            {
                return ValidationResult.Failed("بازه زمانی نمی‌تواند بیش از 90 روز باشد.");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// اعتبارسنجی مدت زمان نوبت
        /// </summary>
        /// <param name="duration">مدت زمان (دقیقه)</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public static ValidationResult ValidateAppointmentDuration(int duration)
        {
            if (duration <= 0)
            {
                return ValidationResult.Failed("مدت زمان نوبت باید بزرگتر از صفر باشد.");
            }

            if (duration < 5)
            {
                return ValidationResult.Failed("مدت زمان نوبت نمی‌تواند کمتر از 5 دقیقه باشد.");
            }

            if (duration > 480) // 8 ساعت
            {
                return ValidationResult.Failed("مدت زمان نوبت نمی‌تواند بیش از 480 دقیقه باشد.");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// اعتبارسنجی زمان کار
        /// </summary>
        /// <param name="startTime">زمان شروع</param>
        /// <param name="endTime">زمان پایان</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public static ValidationResult ValidateWorkTime(TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime >= endTime)
            {
                return ValidationResult.Failed("زمان شروع باید قبل از زمان پایان باشد.");
            }

            var duration = (endTime - startTime).TotalMinutes;
            if (duration < 30)
            {
                return ValidationResult.Failed("مدت زمان کار نمی‌تواند کمتر از 30 دقیقه باشد.");
            }

            if (duration > 1440) // 24 ساعت
            {
                return ValidationResult.Failed("مدت زمان کار نمی‌تواند بیش از 24 ساعت باشد.");
            }

            return ValidationResult.Success();
        }
    }

    /// <summary>
    /// نتیجه اعتبارسنجی
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// آیا اعتبارسنجی موفق بود؟
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// پیام خطا (در صورت وجود)
        /// </summary>
        public string ErrorMessage { get; private set; }

        private ValidationResult(bool isValid, string errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// ایجاد نتیجه موفق
        /// </summary>
        public static ValidationResult Success()
        {
            return new ValidationResult(true);
        }

        /// <summary>
        /// ایجاد نتیجه ناموفق
        /// </summary>
        public static ValidationResult Failed(string errorMessage)
        {
            return new ValidationResult(false, errorMessage);
        }
    }
}

