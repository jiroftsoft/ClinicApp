# 🔍 OTP State Debugging Guide

**تاریخ:** 2025-01-27  
**مشکل:** OTP validation fail → Redirect to Login  
**وضعیت:** در حال بررسی

---

## مشکل گزارش شده

بعد از وارد کردن OTP:
- Redirect به `/Account?returnUrl=...` (نه `/Account/Login?returnUrl=...`)
- باید دوباره login کنید

---

## Debugging Steps

### Step 1: بررسی Logs

بعد از وارد کردن OTP، در Serilog logs دنبال این پیام‌ها بگردید:

```
🔍 VerifyLoginOtp called - NationalCode: ..., OtpCode Length: ...
❌ ModelState invalid - Errors: ...
🔍 OTP State retrieved - IsNull: true/false, ...
❌ CRITICAL: OTP State is NULL - Session may be lost
📊 OTP Validation result - Success: false, Message: ..., Code: ...
```

### Step 2: بررسی Network Request

در DevTools → Network tab:
1. POST request به `/Account/VerifyLoginOtp`
2. Request Body را بررسی کنید:
   - `NationalCode=...` ✅
   - `OtpCode=123456` ✅ (6 رقم)
   - `__RequestVerificationToken=...` ✅

### Step 3: بررسی Session

در DevTools → Application → Session Storage:
- آیا Session موجود است؟
- آیا `OtpState` key موجود است؟

---

## احتمالات

### احتمال 1: OTP State از دست رفته (Session Issue)
**شواهد:**
- Log: `OTP State is NULL`
- Log: `OTP_STATE_NOT_FOUND`

**راه‌حل:**
- بررسی Session configuration
- بررسی Session timeout
- Consider moving to database-backed state

### احتمال 2: ModelState Invalid
**شواهد:**
- Log: `ModelState invalid - Errors: ...`
- Redirect قبل از OTP validation

**راه‌حل:**
- بررسی که OTP در form set شده
- بررسی validation rules

### احتمال 3: OTP Expired
**شواهد:**
- Log: `OTP expired`
- Log: `OTP_EXPIRED`

**راه‌حل:**
- درخواست OTP جدید

---

## Next Steps

1. ✅ Logging اضافه شده
2. ⏳ Test و بررسی logs
3. ⏳ Identify root cause از logs
4. ⏳ Apply targeted fix

---

**لطفاً بعد از تست، logs را بررسی کنید و نتیجه را اطلاع دهید.**

