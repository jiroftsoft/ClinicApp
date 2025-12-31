# 🚨 ClinicApp – Authentication Cookie Not Persisting (CRITICAL)

**Date:** 2024-12-19  
**Status:** 🔴 **CRITICAL - BLOCKING PRODUCTION**  
**Module:** Authentication / Cookie Persistence

---

## 🎯 Problem Statement

**User Report:**
1. لاگین انجام می‌شود → پیغام "با موفقیت وارد شدید" نمایش داده می‌شود
2. وارد صفحه `/Patient/Appointment/Available` می‌شود
3. روی "رزرو نوبت" کلیک می‌کند → می‌گوید "مجدد لاگین کنید"
4. ایکون پروفایل اصلاً نمایش داده نمی‌شود در صفحه اصلی

**Core Issue:**
Cookie set می‌شود اما در request بعدی validate نمی‌شود یا ارسال نمی‌شود.

---

## 🔬 Root Cause Analysis

### Hypothesis #1: Cookie Not Set in Response
**Evidence:**
- Logs show "Authentication cookie 'ClinicAppAuth' is set in response"
- But cookie may not actually be in Response.Cookies

**Test:**
- Check Response.Cookies after SignIn
- Check browser DevTools → Application → Cookies

---

### Hypothesis #2: Cookie Set But Not Sent in Next Request
**Evidence:**
- Cookie exists in browser
- But not sent in Request Headers

**Possible Causes:**
- `CookieSameSite = Strict` → Cookie not sent in redirects
- `CookieSecure = Always` → Cookie not sent in HTTP
- Domain/Path mismatch

**Test:**
- Check Request Headers → Cookie
- Verify cookie attributes

---

### Hypothesis #3: Cookie Sent But Not Validated by Middleware
**Evidence:**
- Cookie sent in request
- But `Request.IsAuthenticated` is false

**Possible Causes:**
- OWIN middleware not running
- SecurityStamp validation fails
- Cookie expired/invalid

**Test:**
- Check OWIN middleware execution
- Check SecurityStamp validation logs

---

### Hypothesis #4: Timing Issue - Cookie Set After Redirect
**Evidence:**
- Redirect happens before cookie is fully set
- Next request arrives before cookie is available

**Test:**
- Increase redirect delay
- Check cookie existence before redirect

---

## 🔧 Immediate Fixes Applied

### Fix #1: Added Diagnostic Logging to AppointmentController.Available
**File:** `Areas/Patient/Controllers/AppointmentController.cs`
**Action:** Log authentication state and cookie existence

### Fix #2: CookieSameSite Changed to Lax in Development
**File:** `App_Start/Startup.Auth.cs`
**Action:** Changed from Strict to Lax to allow cookie in redirects

### Fix #3: Increased Redirect Delay
**File:** `Views/Account/_LoginModal.cshtml`
**Action:** Increased from 500ms to 1000ms

---

## 📋 Diagnostic Checklist

- [ ] Logs show "cookie is set" after SignIn
- [ ] Browser shows `ClinicAppAuth` cookie after login
- [ ] Cookie attributes are correct (SameSite=Lax in Dev)
- [ ] Next request (Home) includes cookie in headers
- [ ] `Request.IsAuthenticated` is true in HomeController
- [ ] `Request.IsAuthenticated` is true in AppointmentController.Available
- [ ] `_LoginPartial` shows user menu (not login button)

---

**Owner:** ClinicApp Engineering  
**Category:** Critical Bug  
**Priority:** **P0 - BLOCKING**

