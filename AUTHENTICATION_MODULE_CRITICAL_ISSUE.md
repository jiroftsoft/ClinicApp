# 🚨 CRITICAL ISSUE: Session Loss in Authentication Flow

## Problem Summary

**Date:** 2026-01-02  
**Severity:** 🔴 **CRITICAL - BLOCKS PRODUCTION**  
**Status:** ❌ **UNRESOLVED**

---

## Issue Description

Session state is lost between `SendLoginOtp` and `VerifyLoginOtp` requests, causing:

1. ✅ **SendLoginOtp** succeeds → OTP sent via SMS
2. ❌ **VerifyLoginOtp** fails → `OTP_STATE_NOT_FOUND`
3. ❌ User redirected back to login page
4. ❌ **Cannot login to the system**

---

## Evidence from Logs

```
2026-01-02 00:09:09.873 [INF] ✅ [SendLoginOtp] SUCCESS - UserId: 1f8ee9f1-27fb-4ff2-8164-5ed3ef289780, Duration: 1149.5123ms
2026-01-02 00:09:09.873 [INF] SMS sent via Asanak. To: +989136381995, Status: "OK"

... (20 seconds later) ...

2026-01-02 00:20:09.445 [INF] OTP State retrieved - IsNull: true, NationalCode: NULL
2026-01-02 00:20:09.446 [ERR] CRITICAL: OTP State is NULL - Session may be lost. NationalCode: 5369873054
2026-01-02 00:20:09.446 [WRN] OTP state not found during validation | NationalCode: 5369873054
2026-01-02 00:20:09.447 [ERR] Code: OTP_STATE_NOT_FOUND
```

---

## Root Cause Analysis

### Hypothesis #1: IIS Application Pool Recycle ⚠️
- **Likelihood:** HIGH
- **Evidence:** 11-minute gap between SendOtp (00:09) and VerifyOtp (00:20)
- **Impact:** In-memory session lost, but database fallback should work

### Hypothesis #2: Database Fallback Not Working ⚠️
- **Likelihood:** HIGH
- **Evidence:** `HybridOtpStateStore` should fallback to database, but returns NULL
- **Possible Causes:**
  - SessionID mismatch between requests
  - Database record not saved properly
  - Query not finding the record

### Hypothesis #3: SessionID Changed ⚠️
- **Likelihood:** MEDIUM
- **Evidence:** New session created after app restart
- **Impact:** Cannot retrieve OTP state from database (SessionID mismatch)

---

## Components Involved

1. **HybridOtpStateStore.cs** (Lines 55-129)
   - `GetState()`: Checks Session → Database fallback
   - `SetState()`: Saves to Session → Database

2. **AuthService.cs** (Lines 76-197, 201-318)
   - `SendLoginOtpAsync()`: Calls `SetState()`
   - `VerifyLoginOtpAndSignInAsync()`: Calls `GetState()`

3. **Database Table:** `OtpStates`
   - Stores OTP state for fallback
   - Indexed by `SessionID` and `ExpiryUtc`

---

## Impact

- ❌ **Users cannot login**
- ❌ **OTP system completely broken**
- ❌ **BLOCKS PRODUCTION DEPLOYMENT**

---

## Required Actions

### Immediate (P0):
1. ✅ Verify `OtpStates` table has records after SendOtp
2. ✅ Check SessionID consistency between requests
3. ✅ Add detailed logging to `HybridOtpStateStore.GetState()`
4. ✅ Test database fallback mechanism

### Short-term (P1):
1. Consider using alternative to SessionID for database lookup (e.g., NationalCode + IP)
2. Increase OTP expiry time to account for delays
3. Add session persistence configuration in Web.config

### Long-term (P2):
1. Implement Redis/distributed cache for session state
2. Add monitoring/alerting for session loss
3. Consider stateless OTP validation (JWT-based)

---

## Test Plan

1. Send OTP → Wait 2 minutes → Verify OTP
2. Send OTP → Restart IIS → Verify OTP
3. Send OTP → Clear cookies → Verify OTP
4. Check database for OtpStates records after each SendOtp

---

## Status

**CRITICAL:** Module cannot be deployed until this is resolved.

**Next Steps:**
1. Run diagnostic queries on OtpStates table
2. Add comprehensive logging
3. Test session persistence
4. Implement fix based on findings

