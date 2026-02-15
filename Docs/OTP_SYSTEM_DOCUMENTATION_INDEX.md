# 📚 OTP System Documentation Index

> **هدف:** فهرست کامل مستندات سیستم OTP برای دسترسی سریع  
> **پروژه:** ClinicApp  
> **تاریخ:** 2025-01-27

---

## 📋 فهرست مستندات

### 1. ✅ [OTP_LOGIN_SIGNUP_AI_CHECKLIST.md](./OTP_LOGIN_SIGNUP_AI_CHECKLIST.md)
**چک‌لیست جامع برای بررسی OTP**

- ✅ تمام موارد امنیتی، UX، و Compliance
- ✅ محل بررسی در کد برای هر مورد
- ✅ وضعیت پیاده‌سازی (✅ انجام شده / ⚠️ نیاز به بررسی)
- ✅ توصیه‌های بهبود

**استفاده:** برای بررسی کامل سیستم OTP توسط تیم یا AI

---

### 2. 📊 [OTP_LOGIN_SIGNUP_AI_EVALUATION.json](./OTP_LOGIN_SIGNUP_AI_EVALUATION.json)
**خروجی JSON برای ارزیابی ماشینی**

- 📊 ساختار JSON کامل
- 📊 وضعیت هر بخش (passed/needs_implementation)
- 📊 توصیه‌ها با اولویت
- 📊 Production Readiness Score

**استفاده:** برای ارزیابی خودکار توسط AI یا Scripts

---

### 3. 🔄 [OTP_LOGIN_SIGNUP_FLOW_DIAGRAM.md](./OTP_LOGIN_SIGNUP_FLOW_DIAGRAM.md)
**نمودار Flow کامل Login و Signup**

- 🔄 Flow Diagram برای Login
- 🔄 Flow Diagram برای Signup
- 🔄 Unified Flow (Auto-Detection)
- 🔄 Security Checkpoints
- 🔄 UI Flow States
- 🔄 Error Handling Flow

**استفاده:** برای درک کامل Flow و Debugging

---

### 4. 🤖 [OTP_AI_AUDIT_PROMPT_TEMPLATE.md](./OTP_AI_AUDIT_PROMPT_TEMPLATE.md)
**Prompt Template برای بررسی توسط AI**

- 🤖 Prompt آماده برای ارسال به AI
- 🤖 Checklist Quick Reference
- 🤖 Expected Output Format
- 🤖 Advanced Audit Scenarios

**استفاده:** کپی و ارسال به AI برای Code Review

---

## 🚀 راهنمای استفاده سریع

### برای بررسی کامل سیستم:

1. **شروع با Checklist:**
   ```
   Docs/OTP_LOGIN_SIGNUP_AI_CHECKLIST.md
   ```
   - بررسی تمام موارد
   - شناسایی نقاط ضعف

2. **دریافت JSON Evaluation:**
   ```
   Docs/OTP_LOGIN_SIGNUP_AI_EVALUATION.json
   ```
   - برای ارزیابی خودکار
   - برای Dashboard/Monitoring

3. **درک Flow:**
   ```
   Docs/OTP_LOGIN_SIGNUP_FLOW_DIAGRAM.md
   ```
   - برای Debugging
   - برای Onboarding تیم جدید

4. **بررسی توسط AI:**
   ```
   Docs/OTP_AI_AUDIT_PROMPT_TEMPLATE.md
   ```
   - کپی Prompt
   - ارسال به AI
   - دریافت Report

---

## 📁 فایل‌های کلیدی در کد

### Backend (C#)

| فایل | توضیح |
|------|-------|
| `Services/AuthService.cs` | Core OTP Logic (Send, Verify, SignIn) |
| `Infrastructure/AuthSettingsFromConfig.cs` | OTP Settings (Length, TTL, Rate Limits) |
| `Models/Core/OtpRequest.cs` | Database Model برای Audit |
| `Models/Core/OtpStateEntity.cs` | Database Model برای OTP State |
| `Services/AsanakSmsService.cs` | SMS Service با Retry |
| `Controllers/AccountController.cs` | Account Actions (Login, Signup) |
| `App_Start/Startup.Auth.cs` | Cookie Authentication Settings |
| `Interfaces/OTP/OTPSystem.cs` | Interfaces (IOtpStateStore, IRateLimiter, etc.) |

### Frontend (JavaScript)

| فایل | توضیح |
|------|-------|
| `Content/js/login-otp-manager.js` | OTP Input Handler (Auto-focus, Auto-submit, Paste) |
| `Content/js/login-modal.js` | Login Modal Logic |
| `Views/Account/Login.cshtml` | Login Page |
| `Views/Account/_LoginModal.cshtml` | Login Modal |

---

## 🔍 Quick Reference

### تنظیمات OTP (Web.config)

```xml
<appSettings>
  <add key="Otp.Length" value="6" />
  <add key="Otp.ExpiryMinutes" value="2" />
  <add key="Otp.HashKey" value="..." />
  <add key="Otp.MaxSendsPerNationalCodePer5Min" value="3" />
  <add key="Otp.MaxSendsPerIpPer5Min" value="10" />
  <add key="Otp.FailedMaxAttempts" value="5" />
  <add key="Otp.LockoutMinutes" value="15" />
  <add key="Otp.MaxVerificationAttempts" value="5" />
</appSettings>
```

### API Endpoints

| Endpoint | Method | توضیح |
|----------|--------|-------|
| `/Account/CheckUser` | POST | بررسی وجود کاربر |
| `/Account/SendLoginOtp` | POST | ارسال OTP برای Login |
| `/Account/VerifyLoginOtp` | POST | تایید OTP برای Login |
| `/Account/SendRegistrationOtp` | POST | ارسال OTP برای Signup |
| `/Account/VerifyRegistrationOtp` | POST | تایید OTP برای Signup |

---

## ✅ وضعیت Production Readiness

| بخش | وضعیت | توضیح |
|-----|-------|-------|
| **Security** | ✅ Ready | Hash، Rate Limiting، Brute Force Protection |
| **Functionality** | ✅ Ready | Login + Signup با Auto-Detection |
| **UX** | ⚠️ Mostly Ready | نیاز به تایمر معکوس |
| **Compliance** | ⚠️ Mostly Ready | نیاز به Consent Checkbox |
| **Performance** | ✅ Ready | Retry، Timeout (نیاز به Queue) |
| **Overall** | ✅ **Ready with Recommendations** | آماده Production با توصیه‌های بهبود |

---

## 🔧 توصیه‌های اولویت بالا

### High Priority

1. **✅ اضافه کردن تایمر معکوس در UI**
   - بهبود UX
   - کاهش سوالات کاربران

2. **✅ بررسی Cookie Security Settings**
   - HttpOnly
   - Secure (HTTPS only)
   - SameSite

### Medium Priority

3. **✅ اضافه کردن Consent Checkbox در Signup**
   - رعایت قوانین
   - کاهش ریسک قانونی

4. **✅ پیاده‌سازی SMS Queue**
   - بهبود Performance
   - Handle High Load

5. **✅ اضافه کردن Fallback SMS Provider**
   - افزایش Reliability
   - کاهش Single Point of Failure

---

## 📊 Metrics & Monitoring

### Metrics پیشنهادی

- OTP Send Success Rate
- OTP Verify Success Rate
- Average OTP Delivery Time
- Rate Limit Hit Count
- Account Lockout Count
- Failed Verification Attempts

### Logging

- تمام درخواست‌های OTP در `OtpRequest` table
- Login History در `LoginHistory` table
- Structured Logging با Serilog

---

## 🆘 Troubleshooting

### مشکل: OTP State Not Found

**راه‌حل:**
1. بررسی Session State
2. Fallback به Database (`OtpStateEntity`)
3. اگر پیدا نشد، درخواست OTP جدید

**کد:** `Services/AuthService.cs` → `VerifyLoginOtpAsync` (خط ~306-330)

---

### مشکل: Rate Limit Exceeded

**راه‌حل:**
1. بررسی `MemoryCacheRateLimiter`
2. بررسی تنظیمات در `Web.config`
3. Wait برای پایان Time Window

**کد:** `Services/AuthService.cs` → `SendLoginOtpAsync` (خط ~132-138)

---

### مشکل: SMS Not Delivered

**راه‌حل:**
1. بررسی `AsanakSmsService` Logs
2. بررسی Retry Mechanism
3. بررسی SMS Provider Status

**کد:** `Services/AsanakSmsService.cs`

---

## 📞 Support & Contact

برای سوالات یا مشکلات:
1. بررسی مستندات بالا
2. بررسی Logs (Serilog)
3. بررسی Database (`OtpRequest`, `OtpStateEntity`)

---

## 🔄 Changelog

### Version 1.0 (2025-01-27)
- ✅ ایجاد چک‌لیست جامع
- ✅ ایجاد JSON Evaluation
- ✅ ایجاد Flow Diagrams
- ✅ ایجاد AI Audit Prompt Template
- ✅ ایجاد Documentation Index

---

**آخرین به‌روزرسانی:** 2025-01-27  
**نسخه:** 1.0  
**وضعیت:** Complete

