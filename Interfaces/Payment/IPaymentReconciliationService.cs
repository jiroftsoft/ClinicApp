using ClinicApp.Helpers;
using ClinicApp.Models.DTOs.Payment;
using ClinicApp.Models.Entities.Payment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces.Payment
{
    /// <summary>
    /// سرویس تطبیق و رفع اختلاف‌های مالی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. تطبیق خودکار موجودی جلسه صندوق
    /// 2. شناسایی اختلاف‌ها
    /// 3. رفع اختلاف‌ها
    /// 4. دریافت اختلاف‌های حل نشده
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public interface IPaymentReconciliationService
    {
        /// <summary>
        /// تطبیق موجودی یک جلسه صندوق
        /// </summary>
        /// <param name="cashSessionId">شناسه جلسه صندوق</param>
        /// <returns>گزارش تطبیق</returns>
        Task<ServiceResult<ReconciliationReport>> ReconcileSessionAsync(int cashSessionId);

        /// <summary>
        /// شناسایی اختلاف‌های مالی در یک جلسه
        /// </summary>
        /// <param name="cashSessionId">شناسه جلسه صندوق</param>
        /// <returns>گزارش اختلاف‌ها</returns>
        Task<ServiceResult<DiscrepancyReport>> DetectDiscrepanciesAsync(int cashSessionId);

        /// <summary>
        /// رفع یک اختلاف
        /// </summary>
        /// <param name="discrepancyId">شناسه اختلاف</param>
        /// <param name="resolution">راه‌حل و توضیحات</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult<bool>> ResolveDiscrepancyAsync(int discrepancyId, string resolution);

        /// <summary>
        /// دریافت اختلاف‌های حل نشده
        /// </summary>
        /// <returns>لیست اختلاف‌های حل نشده</returns>
        Task<ServiceResult<List<PaymentDiscrepancy>>> GetUnresolvedDiscrepanciesAsync();
    }
}

