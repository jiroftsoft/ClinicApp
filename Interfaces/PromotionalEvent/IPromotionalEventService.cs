using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.PromotionalEvent;
using ClinicApp.Models.DTOs.PromotionalEvent;

namespace ClinicApp.Interfaces.PromotionalEvent
{
    /// <summary>
    /// Interface برای Service مدیریت ایونت‌های تبلیغاتی
    /// طراحی شده بر اساس اصول SRP و ServiceResult Pattern
    /// </summary>
    public interface IPromotionalEventService
    {
        /// <summary>
        /// ایجاد ایونت جدید
        /// </summary>
        Task<ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>> CreateAsync(Models.Entities.PromotionalEvent.PromotionalEvent promotionalEvent);

        /// <summary>
        /// به‌روزرسانی ایونت
        /// </summary>
        Task<ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>> UpdateAsync(int eventId, Models.Entities.PromotionalEvent.PromotionalEvent promotionalEvent);

        /// <summary>
        /// حذف ایونت
        /// </summary>
        Task<ServiceResult<bool>> DeleteAsync(int eventId);

        /// <summary>
        /// دریافت ایونت با شناسه
        /// </summary>
        Task<ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>> GetByIdAsync(int eventId);

        /// <summary>
        /// دریافت تمام ایونت‌ها
        /// </summary>
        Task<ServiceResult<List<Models.Entities.PromotionalEvent.PromotionalEvent>>> GetAllAsync(bool includeDeleted = false);

        /// <summary>
        /// دریافت ایونت‌های فعال
        /// </summary>
        Task<ServiceResult<List<Models.Entities.PromotionalEvent.PromotionalEvent>>> GetActiveEventsAsync(DateTime? appointmentDate = null);

        /// <summary>
        /// محاسبه تخفیف برای یک پزشک و قیمت پایه
        /// </summary>
        Task<ServiceResult<decimal>> CalculateDiscountAsync(int doctorId, decimal basePrice, DateTime? appointmentDate = null);

        /// <summary>
        /// محاسبه تخفیف با جزئیات (شامل PromotionalEventId)
        /// </summary>
        Task<ServiceResult<DiscountResult>> CalculateDiscountWithDetailsAsync(int doctorId, decimal basePrice, DateTime? appointmentDate = null);

        /// <summary>
        /// افزایش تعداد استفاده شده
        /// </summary>
        Task<ServiceResult<bool>> IncrementUsedSlotsAsync(int eventId);
    }
}

