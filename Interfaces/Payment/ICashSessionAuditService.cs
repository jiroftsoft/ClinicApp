using ClinicApp.Helpers;
using ClinicApp.Models.DTOs.Payment;
using ClinicApp.Models.Entities.Payment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces.Payment
{
    /// <summary>
    /// سرویس Audit Trail برای جلسات صندوق
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ثبت تمام تغییرات جلسات صندوق
    /// 2. دریافت لاگ‌های یک جلسه
    /// 3. دریافت لاگ‌های یک کاربر
    /// 4. خلاصه Audit Trail
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public interface ICashSessionAuditService
    {
        /// <summary>
        /// ثبت یک اقدام در Audit Log
        /// </summary>
        /// <param name="cashSessionId">شناسه جلسه صندوق</param>
        /// <param name="action">نوع اقدام (Open, Close, Adjust, Cancel, etc.)</param>
        /// <param name="oldValue">مقدار قبلی (object که به JSON تبدیل می‌شود)</param>
        /// <param name="newValue">مقدار جدید (object که به JSON تبدیل می‌شود)</param>
        /// <param name="reason">دلیل تغییر</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> LogActionAsync(int cashSessionId, string action, object oldValue, object newValue, string reason);

        /// <summary>
        /// دریافت لاگ‌های یک جلسه صندوق
        /// </summary>
        /// <param name="cashSessionId">شناسه جلسه صندوق</param>
        /// <returns>لیست لاگ‌ها</returns>
        Task<ServiceResult<List<CashSessionAuditLog>>> GetAuditLogsAsync(int cashSessionId);

        /// <summary>
        /// دریافت لاگ‌های یک کاربر در بازه زمانی
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="fromDate">از تاریخ</param>
        /// <param name="toDate">تا تاریخ</param>
        /// <returns>لیست لاگ‌ها</returns>
        Task<ServiceResult<List<CashSessionAuditLog>>> GetUserAuditLogsAsync(string userId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// دریافت خلاصه Audit Trail یک جلسه
        /// </summary>
        /// <param name="cashSessionId">شناسه جلسه صندوق</param>
        /// <returns>خلاصه Audit Trail</returns>
        Task<ServiceResult<AuditSummary>> GetAuditSummaryAsync(int cashSessionId);
    }
}
