using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization
{
    /// <summary>
    /// Interface برای بهینه‌سازی زمان‌های استراحت پزشکان
    /// 
    /// مسئولیت (SRP):
    /// - محاسبه زمان‌های استراحت بهینه
    /// - توزیع استراحت در طول روز
    /// - در نظر گیری قوانین کار و سلامت
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط بهینه‌سازی استراحت
    /// - Interface Segregation: Interface کوچک و متمرکز
    /// - Dependency Inversion: وابستگی به abstraction
    /// </summary>
    public interface IBreakTimeOptimizer
    {
        /// <summary>
        /// بهینه‌سازی زمان‌های استراحت برای یک روز
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <param name="workStartTime">زمان شروع کار</param>
        /// <param name="workEndTime">زمان پایان کار</param>
        /// <param name="totalWorkMinutes">کل زمان کار (دقیقه)</param>
        /// <returns>لیست زمان‌های استراحت بهینه شده</returns>
        Task<ServiceResult<List<BreakTimeSlot>>> OptimizeBreakTimesAsync(
            int doctorId, 
            DateTime date, 
            TimeSpan workStartTime, 
            TimeSpan workEndTime, 
            int totalWorkMinutes);

        /// <summary>
        /// محاسبه حداقل زمان استراحت مورد نیاز
        /// </summary>
        /// <param name="totalWorkMinutes">کل زمان کار (دقیقه)</param>
        /// <returns>حداقل زمان استراحت (دقیقه)</returns>
        int CalculateMinimumBreakTime(int totalWorkMinutes);

        /// <summary>
        /// بررسی اینکه آیا زمان استراحت کافی است
        /// </summary>
        /// <param name="totalWorkMinutes">کل زمان کار (دقیقه)</param>
        /// <param name="breakTimeMinutes">زمان استراحت (دقیقه)</param>
        /// <returns>true اگر استراحت کافی است</returns>
        bool IsBreakTimeSufficient(int totalWorkMinutes, int breakTimeMinutes);

        /// <summary>
        /// پیشنهاد زمان‌های استراحت بر اساس الگوهای بهینه
        /// </summary>
        /// <param name="workStartTime">زمان شروع کار</param>
        /// <param name="workEndTime">زمان پایان کار</param>
        /// <param name="totalWorkMinutes">کل زمان کار (دقیقه)</param>
        /// <returns>لیست پیشنهادات زمان استراحت</returns>
        List<BreakTimeSuggestion> SuggestBreakTimes(TimeSpan workStartTime, TimeSpan workEndTime, int totalWorkMinutes);
    }

    /// <summary>
    /// پیشنهاد زمان استراحت
    /// </summary>
    public class BreakTimeSuggestion
    {
        /// <summary>
        /// زمان شروع استراحت
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان استراحت
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// مدت زمان استراحت (دقیقه)
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// نوع استراحت
        /// </summary>
        public BreakType Type { get; set; }

        /// <summary>
        /// اولویت (هرچه بالاتر، مهم‌تر)
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// دلیل پیشنهاد
        /// </summary>
        public string Reason { get; set; }
    }
}

