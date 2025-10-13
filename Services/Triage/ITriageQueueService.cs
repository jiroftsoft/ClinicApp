using ClinicApp.Models.Entities.Triage;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Triage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Services.Triage
{
    /// <summary>
    /// رابط سرویس مدیریت صف تریاژ - SRP محور
    /// </summary>
    public interface ITriageQueueService
    {
        /// <summary>
        /// اضافه کردن ارزیابی به صف تریاژ
        /// </summary>
        /// <param name="assessmentId">شناسه ارزیابی</param>
        /// <param name="priority">اولویت</param>
        /// <param name="queueTimeUtc">زمان صف (اختیاری)</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult<TriageQueue>> AddToTriageQueueAsync(int assessmentId, int priority, DateTime? queueTimeUtc = null);

        /// <summary>
        /// دریافت لیست صف انتظار
        /// </summary>
        /// <param name="departmentId">شناسه بخش (اختیاری)</param>
        /// <returns>لیست صف انتظار</returns>
        Task<ServiceResult<List<TriageQueueItemDto>>> GetWaitingAsync(int? departmentId = null);

        /// <summary>
        /// فراخوانی بیمار بعدی
        /// </summary>
        /// <param name="departmentId">شناسه بخش (اختیاری)</param>
        /// <returns>نتیجه فراخوانی</returns>
        Task<ServiceResult<TriageQueue>> CallNextAsync(int? departmentId = null);

        /// <summary>
        /// تکمیل صف
        /// </summary>
        /// <param name="queueId">شناسه صف</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> CompleteAsync(int queueId);

        /// <summary>
        /// بستن صف‌های فعال برای ارزیابی
        /// </summary>
        /// <param name="assessmentId">شناسه ارزیابی</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> CloseActiveQueueIfAnyAsync(int assessmentId);

        /// <summary>
        /// دریافت لیست آماده پذیرش
        /// </summary>
        /// <param name="departmentId">شناسه بخش (اختیاری)</param>
        /// <returns>لیست آماده پذیرش</returns>
        Task<ServiceResult<List<TriageAssessment>>> GetCompletedForAdmissionAsync(int? departmentId = null);

        /// <summary>
        /// مرتب‌سازی صف بر اساس اولویت
        /// </summary>
        /// <param name="departmentId">شناسه بخش (اختیاری)</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> ReorderQueueByPriorityAsync(int? departmentId = null);

        /// <summary>
        /// دریافت آمار صف
        /// </summary>
        /// <param name="departmentId">شناسه بخش (اختیاری)</param>
        /// <returns>آمار صف</returns>
        Task<ServiceResult<TriageQueueStats>> GetQueueStatsAsync(int? departmentId = null);

        /// <summary>
        /// دریافت صف فوری
        /// </summary>
        /// <returns>لیست بیماران فوری</returns>
        Task<ServiceResult<List<TriageQueue>>> GetUrgentQueueAsync();

        /// <summary>
        /// دریافت صف تاخیردار
        /// </summary>
        /// <returns>لیست بیماران تاخیردار</returns>
        Task<ServiceResult<List<TriageQueue>>> GetOverdueQueueAsync();
    }

    /// <summary>
    /// آمار صف تریاژ
    /// </summary>
    public class TriageQueueStats
    {
        public int TotalWaiting { get; set; }
        public int TotalInProgress { get; set; }
        public int TotalCompleted { get; set; }
        public int CriticalWaiting { get; set; }
        public int HighPriorityWaiting { get; set; }
        public int OverdueCount { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
        public int CompletedToday { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
