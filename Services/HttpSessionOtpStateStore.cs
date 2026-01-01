using System;
using ClinicApp.Interfaces.OTP;
using ClinicApp.Helpers;
using Serilog;
using System.Web;
using OtpState = ClinicApp.Interfaces.OTP.OtpState;

namespace ClinicApp.Services
{
    /// <summary>
    /// Session-only OTP State Store (Simple & Reliable)
    /// 
    /// ✅ استراتژی:
    /// - فقط Session (سریع و ساده)
    /// - Database persistence توسط AuthService مدیریت می‌شود
    /// 
    /// ✅ مزایا:
    /// - No DbContext dependency issues
    /// - No transaction conflicts
    /// - Simple and fast
    /// 
    /// طبق: Bugfix-Master-Contract.md
    /// </summary>
    public class HttpSessionOtpStateStore : IOtpStateStore
    {
        private const string OtpStateSessionKey = "OtpState";
        private static readonly ILogger _log = Log.ForContext<HttpSessionOtpStateStore>();

        public OtpState GetState()
        {
            var httpContext = HttpContext.Current;
            if (httpContext?.Session == null)
            {
                _log.Warning("⚠️ [GetState] HttpContext.Session is NULL");
                return null;
            }

            var sessionId = httpContext.Session.SessionID;
            _log.Debug("[GetState] SessionID: {SessionId}", sessionId?.Substring(0, Math.Min(8, sessionId?.Length ?? 0)) + "...");

            var state = httpContext.Session[OtpStateSessionKey] as OtpState;
            if (state != null)
            {
                _log.Information("[GetState] ✅ Found in Session - SessionID: {SessionId}, MaskedNC: {MaskedNC}", 
                    sessionId?.Substring(0, Math.Min(8, sessionId?.Length ?? 0)) + "...",
                    MaskHelper.MaskNationalCode(state.NationalCode));
            }
            else
            {
                _log.Warning("[GetState] ❌ NOT FOUND in Session - SessionID: {SessionId}, Keys in Session: {Keys}", 
                    sessionId?.Substring(0, Math.Min(8, sessionId?.Length ?? 0)) + "...",
                    httpContext.Session.Keys.Count);
            }
            return state;
        }

        public OtpState GetState(string nationalCode)
        {
            // For session-only store, this is the same as GetState()
            // Database fallback is handled by AuthService
            _log.Debug("[GetState(NC)] Delegating to GetState() - Session-only store");
            return GetState();
        }

        public void SetState(OtpState state)
        {
            var httpContext = HttpContext.Current;
            if (httpContext?.Session == null)
            {
                _log.Warning("⚠️ [SetState] HttpContext.Session is NULL");
                return;
            }

            var sessionId = httpContext.Session.SessionID;
            httpContext.Session[OtpStateSessionKey] = state;
            
            // Verify immediately
            var verify = httpContext.Session[OtpStateSessionKey] as OtpState;
            if (verify == null)
            {
                _log.Error("❌ [SetState] CRITICAL: State NOT saved! SessionID: {SessionId}", 
                    sessionId?.Substring(0, Math.Min(8, sessionId?.Length ?? 0)) + "...");
            }
            
            _log.Information("✅ [SetState] SUCCESS - SessionID: {SessionId}, MaskedNC: {MaskedNC}, Expiry: {Expiry}",
                sessionId?.Substring(0, Math.Min(8, sessionId?.Length ?? 0)) + "...",
                MaskHelper.MaskNationalCode(state.NationalCode),
                state.ExpiryUtc);
        }

        public void ClearState()
        {
            var httpContext = HttpContext.Current;
            if (httpContext?.Session == null)
            {
                _log.Warning("⚠️ [ClearState] HttpContext.Session is NULL");
                return;
            }

            httpContext.Session.Remove(OtpStateSessionKey);
            _log.Debug("✅ [ClearState] SUCCESS - Removed from Session");
        }
    }
}

