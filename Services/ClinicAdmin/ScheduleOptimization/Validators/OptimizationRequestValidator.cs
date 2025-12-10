using System;
using System.Collections.Generic;
using ClinicApp.Core;

namespace ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Validators
{
    /// <summary>
    /// Validator برای اعتبارسنجی درخواست‌های بهینه‌سازی
    /// 
    /// مسئولیت (SRP):
    /// - اعتبارسنجی درخواست‌های بهینه‌سازی
    /// - بررسی قوانین کسب و کار خاص
    /// - تولید پیام‌های خطا
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط اعتبارسنجی درخواست‌ها
    /// - Open/Closed: قابل توسعه برای قوانین جدید
    /// </summary>
    public static class OptimizationRequestValidator
    {
        /// <summary>
        /// اعتبارسنجی درخواست بهینه‌سازی روزانه
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public static ValidationResult ValidateDailyOptimizationRequest(int doctorId, DateTime date)
        {
            var results = new List<ValidationResult>();

            results.Add(ScheduleOptimizationValidator.ValidateDoctorId(doctorId));
            results.Add(ScheduleOptimizationValidator.ValidateDate(date, allowPastDates: false));

            return CombineResults(results);
        }

        /// <summary>
        /// اعتبارسنجی درخواست بهینه‌سازی هفتگی
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="weekStart">شروع هفته</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public static ValidationResult ValidateWeeklyOptimizationRequest(int doctorId, DateTime weekStart)
        {
            var results = new List<ValidationResult>();

            results.Add(ScheduleOptimizationValidator.ValidateDoctorId(doctorId));
            results.Add(ScheduleOptimizationValidator.ValidateDate(weekStart, allowPastDates: false));

            return CombineResults(results);
        }

        /// <summary>
        /// اعتبارسنجی درخواست بهینه‌سازی ماهانه
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="monthStart">شروع ماه</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public static ValidationResult ValidateMonthlyOptimizationRequest(int doctorId, DateTime monthStart)
        {
            var results = new List<ValidationResult>();

            results.Add(ScheduleOptimizationValidator.ValidateDoctorId(doctorId));
            results.Add(ScheduleOptimizationValidator.ValidateDate(monthStart, allowPastDates: false));

            return CombineResults(results);
        }

        /// <summary>
        /// اعتبارسنجی درخواست متعادل‌سازی بار کاری
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public static ValidationResult ValidateWorkloadBalanceRequest(int doctorId, DateTime startDate, DateTime endDate)
        {
            var results = new List<ValidationResult>();

            results.Add(ScheduleOptimizationValidator.ValidateDoctorId(doctorId));
            results.Add(ScheduleOptimizationValidator.ValidateDateRange(startDate, endDate));

            return CombineResults(results);
        }

        /// <summary>
        /// اعتبارسنجی درخواست بهینه‌سازی هزینه
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public static ValidationResult ValidateCostOptimizationRequest(int doctorId, DateTime startDate, DateTime endDate)
        {
            var results = new List<ValidationResult>();

            results.Add(ScheduleOptimizationValidator.ValidateDoctorId(doctorId));
            results.Add(ScheduleOptimizationValidator.ValidateDateRange(startDate, endDate));

            return CombineResults(results);
        }

        /// <summary>
        /// ترکیب نتایج اعتبارسنجی
        /// </summary>
        private static ValidationResult CombineResults(List<ValidationResult> results)
        {
            foreach (var result in results)
            {
                if (!result.IsValid)
                {
                    return result;
                }
            }

            return ValidationResult.Success();
        }
    }
}

