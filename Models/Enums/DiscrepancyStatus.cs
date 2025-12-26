using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// وضعیت اختلاف مالی
    /// 
    /// استفاده در:
    /// - PaymentDiscrepancy Entity
    /// - گزارش‌های اختلاف
    /// - Dashboard صندوق
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public enum DiscrepancyStatus
    {
        /// <summary>
        /// در انتظار - اختلاف شناسایی شده اما هنوز بررسی نشده
        /// </summary>
        [Display(Name = "در انتظار")]
        Pending = 1,

        /// <summary>
        /// حل شده - اختلاف بررسی و رفع شده است
        /// </summary>
        [Display(Name = "حل شده")]
        Resolved = 2,

        /// <summary>
        /// ارجاع شده - اختلاف به مدیر یا واحد بالاتر ارجاع شده
        /// </summary>
        [Display(Name = "ارجاع شده")]
        Escalated = 3
    }
}

