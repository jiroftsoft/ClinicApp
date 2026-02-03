using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.PromotionalEvent;

namespace ClinicApp.Interfaces.PromotionalEvent
{
    /// <summary>
    /// Interface برای Repository مدیریت ایونت‌های تبلیغاتی
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IPromotionalEventRepository
    {
        /// <summary>
        /// دریافت ایونت با شناسه
        /// </summary>
        Task<Models.Entities.PromotionalEvent.PromotionalEvent> GetByIdAsync(int eventId);

        /// <summary>
        /// دریافت تمام ایونت‌ها
        /// </summary>
        Task<List<Models.Entities.PromotionalEvent.PromotionalEvent>> GetAllAsync(bool includeDeleted = false);

        /// <summary>
        /// دریافت ایونت‌های فعال (در بازه زمانی و با تعداد استفاده باقیمانده)
        /// </summary>
        Task<List<Models.Entities.PromotionalEvent.PromotionalEvent>> GetActiveEventsAsync(DateTime? appointmentDate = null);

        /// <summary>
        /// دریافت ایونت‌ها در بازه زمانی مشخص
        /// </summary>
        Task<List<Models.Entities.PromotionalEvent.PromotionalEvent>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate, bool includeDeleted = false);

        /// <summary>
        /// دریافت ایونت‌های مرتبط با یک پزشک خاص
        /// </summary>
        Task<List<Models.Entities.PromotionalEvent.PromotionalEvent>> GetEventsByDoctorAsync(int doctorId, DateTime? appointmentDate = null);

        /// <summary>
        /// جستجوی ایونت‌ها
        /// </summary>
        Task<List<Models.Entities.PromotionalEvent.PromotionalEvent>> SearchAsync(string searchTerm, bool? isActive = null, DateTime? fromDate = null, DateTime? toDate = null, bool includeDeleted = false);

        /// <summary>
        /// افزودن ایونت جدید
        /// </summary>
        void Add(Models.Entities.PromotionalEvent.PromotionalEvent promotionalEvent);

        /// <summary>
        /// به‌روزرسانی ایونت
        /// </summary>
        void Update(Models.Entities.PromotionalEvent.PromotionalEvent promotionalEvent);

        /// <summary>
        /// حذف نرم ایونت
        /// </summary>
        void Delete(Models.Entities.PromotionalEvent.PromotionalEvent promotionalEvent);

        /// <summary>
        /// بررسی وجود ایونت
        /// </summary>
        Task<bool> ExistsAsync(int eventId);

        /// <summary>
        /// افزایش تعداد استفاده شده
        /// </summary>
        Task IncrementUsedSlotsAsync(int eventId);
    }
}

