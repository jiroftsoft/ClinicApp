using ClinicApp.Interfaces.OTP;
using ClinicApp.Models;
using ClinicApp.Models.Core;
using Serilog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using OtpState = ClinicApp.Interfaces.OTP.OtpState;

namespace ClinicApp.Services
{
    /// <summary>
    /// پیاده‌سازی Hybrid برای ذخیره‌سازی OTP با قابلیت Fallback به Database
    /// 
    /// ✅ استراتژی دو لایه:
    /// 1. Primary: Session (سریع - در حافظه سرور)
    /// 2. Fallback: Database (قابل اطمینان - ماندگار)
    /// 
    /// ✅ مزایا:
    /// - سرعت بالا (Session در حافظه است)
    /// - قابلیت اطمینان (Database backup دارد)
    /// - مقاومت در برابر IIS Recycle
    /// - سازگاری با Load Balancer
    /// - بازیابی پس از Server Restart
    /// 
    /// ✅ سناریوهای استفاده:
    /// - کاربر OTP درخواست می‌کند → Session + Database
    /// - Session سالم است → خواندن از Session (سریع)
    /// - Session از بین رفته → خواندن از Database (fallback)
    /// - OTP تایید شد → پاک کردن از Session + Database
    /// 
    /// طبق: BEAST MODE AUDIT - Issue #1 (Session Loss Prevention)
    /// </summary>
    public class HybridOtpStateStore : IOtpStateStore
    {
        private const string OtpStateSessionKey = "OtpState";
        private static readonly ILogger _log = Log.ForContext<HybridOtpStateStore>();
        private readonly ApplicationDbContext _context;

        public HybridOtpStateStore(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// دریافت OTP State از Session یا Database (با Fallback)
        /// 
        /// ✅ الگوریتم:
        /// 1. بررسی Session → اگر موجود بود، برگردان (Fast Path)
        /// 2. اگر Session null بود → جستجو در Database (Fallback)
        /// 3. اگر در Database پیدا شد → بازگردانی به Session (Restore)
        /// 4. اگر هیچکدام نبود → null
        /// </summary>
        public OtpState GetState()
        {
            try
            {
                var httpContext = HttpContext.Current;
                if (httpContext?.Session == null)
                {
                    _log.Warning("⚠️ HttpContext.Session is null - Cannot retrieve OTP state");
                    return null;
                }

                // ✅ STEP 1: سعی در خواندن از Session (Primary - Fast)
                var sessionState = httpContext.Session[OtpStateSessionKey] as OtpState;
                if (sessionState != null)
                {
                    _log.Debug("✅ OTP State found in Session (Fast Path)");
                    return sessionState;
                }

                // ✅ STEP 2: Session خالی است - Fallback به Database
                _log.Information("⚠️ OTP State not found in Session - Attempting Database Fallback");

                var sessionId = httpContext.Session.SessionID;
                if (string.IsNullOrEmpty(sessionId))
                {
                    _log.Warning("⚠️ SessionID is null or empty - Cannot perform Database Fallback");
                    return null;
                }

                // ✅ STEP 3: جستجوی Database برای OTP معتبر (غیرمنقضی)
                var dbState = _context.OtpStates
                    .Where(o => o.SessionId == sessionId && o.ExpiryUtc > DateTime.UtcNow)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefault();

                if (dbState == null)
                {
                    _log.Information("❌ OTP State not found in Database either - OTP may be expired or never created");
                    return null;
                }

                // ✅ STEP 4: Database Fallback موفق - بازگردانی به Session
                _log.Information("✅ OTP State recovered from Database (Fallback successful) - Restoring to Session");

                var restoredState = new OtpState
                {
                    NationalCode = dbState.NationalCode,
                    PhoneNumber = dbState.PhoneNumber,
                    OtpHash = dbState.OtpHash,
                    ExpiryUtc = dbState.ExpiryUtc,
                    IpAddress = dbState.IpAddress,
                    UserAgent = dbState.UserAgent,
                    AttemptCount = dbState.AttemptCount
                };

                // ✅ بازگردانی به Session برای استفاده‌های بعدی
                httpContext.Session[OtpStateSessionKey] = restoredState;

                return restoredState;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "💥 Error in HybridOtpStateStore.GetState - Returning null");
                return null;
            }
        }

        /// <summary>
        /// ذخیره OTP State در Session و Database (هر دو)
        /// 
        /// ✅ استراتژی:
        /// 1. ذخیره در Session (سرعت)
        /// 2. ذخیره در Database (قابلیت اطمینان)
        /// 3. حذف OTP های قدیمی همان Session (Cleanup)
        /// </summary>
        public void SetState(OtpState state)
        {
            try
            {
                var httpContext = HttpContext.Current;
                if (httpContext?.Session == null)
                {
                    _log.Warning("⚠️ HttpContext.Session is null - Cannot save OTP state");
                    return;
                }

                var sessionId = httpContext.Session.SessionID;

                // ✅ STEP 1: ذخیره در Session (Primary)
                httpContext.Session[OtpStateSessionKey] = state;
                _log.Debug("✅ OTP State saved to Session");

                // ✅ STEP 2: حذف OTP های قدیمی این Session از Database (Cleanup)
                var oldStates = _context.OtpStates
                    .Where(o => o.SessionId == sessionId)
                    .ToList();

                if (oldStates.Any())
                {
                    _context.OtpStates.RemoveRange(oldStates);
                    _log.Information("🧹 Cleaned up {Count} old OTP state(s) from Database", oldStates.Count);
                }

                // ✅ STEP 3: ذخیره OTP جدید در Database (Backup)
                var dbState = new OtpStateEntity
                {
                    SessionId = sessionId,
                    NationalCode = state.NationalCode,
                    PhoneNumber = state.PhoneNumber,
                    OtpHash = state.OtpHash,
                    ExpiryUtc = state.ExpiryUtc,
                    IpAddress = state.IpAddress,
                    UserAgent = state.UserAgent,
                    AttemptCount = state.AttemptCount,
                    CreatedAt = DateTime.UtcNow
                };

                _context.OtpStates.Add(dbState);
                _context.SaveChanges();

                _log.Information("✅ OTP State saved to Database (Backup) - ID: {Id}", dbState.Id);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "💥 Error in HybridOtpStateStore.SetState - State may not be persisted");
                // ⚠️ عمداً Exception را نمی‌اندازیم - اگر Database fail شد، Session همچنان کار می‌کند
            }
        }

        /// <summary>
        /// پاک کردن OTP State از Session و Database (هر دو)
        /// 
        /// ✅ استراتژی:
        /// 1. حذف از Session
        /// 2. حذف از Database
        /// </summary>
        public void ClearState()
        {
            try
            {
                var httpContext = HttpContext.Current;
                if (httpContext?.Session == null)
                {
                    _log.Warning("⚠️ HttpContext.Session is null - Cannot clear OTP state");
                    return;
                }

                // ✅ STEP 1: حذف از Session
                httpContext.Session.Remove(OtpStateSessionKey);
                _log.Debug("✅ OTP State removed from Session");

                // ✅ STEP 2: حذف از Database
                var sessionId = httpContext.Session.SessionID;
                if (!string.IsNullOrEmpty(sessionId))
                {
                    var dbStates = _context.OtpStates
                        .Where(o => o.SessionId == sessionId)
                        .ToList();

                    if (dbStates.Any())
                    {
                        _context.OtpStates.RemoveRange(dbStates);
                        _context.SaveChanges();
                        _log.Information("✅ OTP State(s) removed from Database - Count: {Count}", dbStates.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "💥 Error in HybridOtpStateStore.ClearState - State may not be fully cleared");
                // ⚠️ عمداً Exception را نمی‌اندازیم - مشکل در پاکسازی نباید فرآیند را متوقف کند
            }
        }
    }
}

