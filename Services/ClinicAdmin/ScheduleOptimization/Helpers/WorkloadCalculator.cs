using System;
using System.Linq;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;

namespace ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Helpers
{
    /// <summary>
    /// Helper Class برای محاسبه بار کاری
    /// 
    /// مسئولیت (SRP):
    /// - محاسبه بار کاری
    /// - محاسبه تعداد نوبت‌ها
    /// - محاسبه درصد استفاده از ظرفیت
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط محاسبه بار کاری
    /// - Static Methods: بدون state، thread-safe
    /// </summary>
    public static class WorkloadCalculator
    {
        /// <summary>
        /// محاسبه تعداد نوبت‌های قابل رزرو برای یک بازه زمانی
        /// </summary>
        /// <param name="startTime">زمان شروع</param>
        /// <param name="endTime">زمان پایان</param>
        /// <param name="appointmentDuration">مدت زمان هر نوبت (دقیقه)</param>
        /// <param name="breakTimeMinutes">زمان استراحت (دقیقه)</param>
        /// <returns>تعداد نوبت‌های قابل رزرو</returns>
        public static int CalculateAvailableAppointments(TimeSpan startTime, TimeSpan endTime, int appointmentDuration, int breakTimeMinutes = 0)
        {
            if (startTime >= endTime)
            {
                return 0;
            }

            if (appointmentDuration <= 0)
            {
                return 0;
            }

            var totalMinutes = (endTime - startTime).TotalMinutes;
            var availableMinutes = totalMinutes - breakTimeMinutes;

            if (availableMinutes <= 0)
            {
                return 0;
            }

            return (int)Math.Floor(availableMinutes / appointmentDuration);
        }

        /// <summary>
        /// محاسبه درصد استفاده از ظرفیت
        /// </summary>
        /// <param name="currentAppointments">تعداد نوبت‌های فعلی</param>
        /// <param name="maxCapacity">حداکثر ظرفیت</param>
        /// <returns>درصد استفاده (0-100)</returns>
        public static decimal CalculateUtilizationPercentage(int currentAppointments, int maxCapacity)
        {
            if (maxCapacity <= 0)
            {
                return 0;
            }

            var percentage = (decimal)currentAppointments / maxCapacity * 100;
            return Math.Min(100, Math.Max(0, percentage));
        }

        /// <summary>
        /// تشخیص وضعیت بار کاری
        /// </summary>
        /// <param name="appointmentCount">تعداد نوبت‌ها</param>
        /// <param name="maxCapacity">حداکثر ظرفیت</param>
        /// <returns>وضعیت بار کاری</returns>
        public static WorkloadBalanceStatus DetermineWorkloadStatus(int appointmentCount, int maxCapacity)
        {
            if (maxCapacity <= 0)
            {
                return WorkloadBalanceStatus.NoWorkDay;
            }

            var utilizationPercentage = CalculateUtilizationPercentage(appointmentCount, maxCapacity);

            if (utilizationPercentage <= 50)
            {
                return WorkloadBalanceStatus.Light;
            }
            else if (utilizationPercentage <= 75)
            {
                return WorkloadBalanceStatus.Balanced;
            }
            else if (utilizationPercentage <= 90)
            {
                return WorkloadBalanceStatus.Heavy;
            }
            else
            {
                return WorkloadBalanceStatus.Overloaded;
            }
        }

        /// <summary>
        /// محاسبه کل زمان کار (دقیقه)
        /// </summary>
        /// <param name="startTime">زمان شروع</param>
        /// <param name="endTime">زمان پایان</param>
        /// <returns>کل زمان کار (دقیقه)</returns>
        public static int CalculateTotalWorkMinutes(TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime >= endTime)
            {
                return 0;
            }

            return (int)(endTime - startTime).TotalMinutes;
        }

        /// <summary>
        /// محاسبه زمان خالی (دقیقه)
        /// </summary>
        /// <param name="totalWorkMinutes">کل زمان کار (دقیقه)</param>
        /// <param name="appointmentCount">تعداد نوبت‌ها</param>
        /// <param name="appointmentDuration">مدت زمان هر نوبت (دقیقه)</param>
        /// <param name="breakTimeMinutes">زمان استراحت (دقیقه)</param>
        /// <returns>زمان خالی (دقیقه)</returns>
        public static int CalculateFreeTimeMinutes(int totalWorkMinutes, int appointmentCount, int appointmentDuration, int breakTimeMinutes)
        {
            var usedMinutes = (appointmentCount * appointmentDuration) + breakTimeMinutes;
            var freeMinutes = totalWorkMinutes - usedMinutes;
            return Math.Max(0, freeMinutes);
        }

        /// <summary>
        /// محاسبه حداقل زمان استراحت مورد نیاز
        /// </summary>
        /// <param name="totalWorkMinutes">کل زمان کار (دقیقه)</param>
        /// <returns>حداقل زمان استراحت (دقیقه)</returns>
        public static int CalculateMinimumBreakTime(int totalWorkMinutes)
        {
            // قانون: حداقل 15 دقیقه استراحت برای هر 4 ساعت کار
            var hours = totalWorkMinutes / 60.0;
            var requiredBreaks = (int)Math.Ceiling(hours / 4.0);
            return requiredBreaks * 15; // 15 دقیقه برای هر 4 ساعت
        }
    }
}

