# 🤖 OTP System - AI Audit Prompt Template

> **هدف:** این Prompt برای بررسی کامل سیستم OTP توسط AI طراحی شده است.  
> **استفاده:** کپی این Prompt و ارسال به AI برای بررسی امنیت، UX، و کیفیت کد

---

## 📋 Prompt برای AI

```
شما یک متخصص امنیت و معماری نرم‌افزار هستید. لطفاً سیستم OTP Login + Signup 
پروژه ClinicApp را با دقت بررسی کنید.

پروژه: سیستم رزرو نوبت کلینیک درمانی
Backend: ASP.NET MVC / C#
نوع: Medical Application (حساس)

لطفاً بررسی خود را بر اساس چک‌لیست زیر انجام دهید:

## 1. بررسی امنیت (Security Audit)

### OTP Generation & Storage
- [ ] آیا OTP با استفاده از Cryptographically Secure Random تولید می‌شود؟
- [ ] آیا OTP به صورت Hash ذخیره می‌شود (نه Plain Text)؟
- [ ] آیا از Salt مناسب استفاده می‌شود؟
- [ ] آیا OTP بعد از Verify حذف می‌شود؟

### Rate Limiting
- [ ] آیا Rate Limiting برای Send OTP پیاده‌سازی شده؟
- [ ] آیا Rate Limiting برای Verify OTP پیاده‌سازی شده؟
- [ ] آیا محدودیت‌ها بر اساس National Code و IP اعمال می‌شوند؟
- [ ] آیا پیاده‌سازی Rate Limiter قابل اعتماد است؟

### Brute Force Protection
- [ ] آیا محدودیت تعداد تلاش‌های ناموفق وجود دارد؟
- [ ] آیا Account Lockout بعد از تلاش‌های زیاد پیاده‌سازی شده؟
- [ ] آیا Delay افزایشی برای تلاش‌های ناموفق وجود دارد؟

### Session & Cookie Security
- [ ] آیا Cookie با HttpOnly flag تنظیم شده؟
- [ ] آیا Cookie با Secure flag تنظیم شده (HTTPS only)?
- [ ] آیا SameSite attribute تنظیم شده؟
- [ ] آیا Session Binding به IP/UserAgent وجود دارد؟

## 2. بررسی UX/UI

### OTP Input Experience
- [ ] آیا Input جداگانه برای هر رقم وجود دارد؟
- [ ] آیا Auto-focus به درستی کار می‌کند؟
- [ ] آیا Auto-submit بعد از تکمیل OTP فعال است؟
- [ ] آیا Paste Support پیاده‌سازی شده؟
- [ ] آیا Navigation Keys (Arrow, Backspace) کار می‌کنند؟

### Feedback & Error Messages
- [ ] آیا پیام‌های خطا واضح و کاربرپسند هستند؟
- [ ] آیا تایمر معکوس برای OTP نمایش داده می‌شود؟
- [ ] آیا Resend OTP با تایمر پیاده‌سازی شده؟

## 3. بررسی Edge Cases

- [ ] آیا OTP منقضی شده به درستی Handle می‌شود؟
- [ ] آیا Refresh صفحه باعث از دست رفتن OTP State نمی‌شود؟
- [ ] آیا Back Button به درستی Handle می‌شود؟
- [ ] آیا Concurrent OTP (چند OTP همزمان) Handle می‌شود؟
- [ ] آیا تغییر شماره موبایل وسط فرآیند Handle می‌شود؟

## 4. بررسی Medical Compliance

- [ ] آیا محتوای SMS فقط شامل OTP است (بدون اطلاعات پزشکی)؟
- [ ] آیا رضایت کاربر برای ارسال SMS ثبت می‌شود؟
- [ ] آیا Audit Trail کامل برای تمام درخواست‌های OTP وجود دارد؟
- [ ] آیا لاگ‌ها شامل اطلاعات حساس نمی‌شوند؟

## 5. بررسی Performance & Reliability

- [ ] آیا Queue برای ارسال SMS پیاده‌سازی شده؟
- [ ] آیا Retry Mechanism با Backoff وجود دارد؟
- [ ] آیا Fallback SMS Provider وجود دارد؟
- [ ] آیا Timeout مناسب برای SMS Service تنظیم شده؟

## 6. بررسی Code Quality

- [ ] آیا Separation of Concerns رعایت شده؟
- [ ] آیا Dependency Injection به درستی استفاده شده؟
- [ ] آیا کد Testable است؟
- [ ] آیا Error Handling مناسب است؟
- [ ] آیا Logging ساختاریافته وجود دارد؟

## 7. بررسی معماری

- [ ] آیا Auto-Detection Login/Signup به درستی کار می‌کند؟
- [ ] آیا Fallback Mechanism برای Session Loss وجود دارد؟
- [ ] آیا State Management (Session + Database) بهینه است؟

---

## فایل‌های کلیدی برای بررسی:

1. `Services/AuthService.cs` - Core OTP Logic
2. `Infrastructure/AuthSettingsFromConfig.cs` - Settings
3. `Models/Core/OtpRequest.cs` - Database Model
4. `Content/js/login-otp-manager.js` - UI Handler
5. `Services/AsanakSmsService.cs` - SMS Service
6. `Controllers/AccountController.cs` - Controllers
7. `App_Start/Startup.Auth.cs` - Cookie Settings

---

## خروجی مورد انتظار:

لطفاً برای هر بخش:
1. ✅ نقاط قوت را مشخص کنید
2. ⚠️ نقاط ضعف و ریسک‌ها را شناسایی کنید
3. 🔧 توصیه‌های بهبود را ارائه دهید
4. 🔴 Critical Issues (اگر وجود دارد) را با اولویت بالا مشخص کنید

همچنین یک JSON Summary با این ساختار ارائه دهید:

```json
{
  "security_score": "X/10",
  "ux_score": "X/10",
  "compliance_score": "X/10",
  "performance_score": "X/10",
  "code_quality_score": "X/10",
  "critical_issues": [],
  "high_priority_issues": [],
  "medium_priority_issues": [],
  "low_priority_issues": [],
  "production_readiness": "ready/needs_work/not_ready"
}
```

لطفاً بررسی را با دقت و جزئیات انجام دهید.
```

---

## 📝 نحوه استفاده

### روش 1: استفاده مستقیم

1. کپی کردن Prompt بالا
2. ارسال به AI (ChatGPT, Claude, Cursor AI, etc.)
3. بررسی نتایج

### روش 2: استفاده با Context Files

```
شما یک متخصص امنیت هستید. لطفاً سیستم OTP را بررسی کنید.

Context Files:
- Docs/OTP_LOGIN_SIGNUP_AI_CHECKLIST.md
- Docs/OTP_LOGIN_SIGNUP_AI_EVALUATION.json
- Docs/OTP_LOGIN_SIGNUP_FLOW_DIAGRAM.md

[کپی Prompt بالا]
```

### روش 3: استفاده برای Code Review

```
بررسی Code Review سیستم OTP:

Files to Review:
- Services/AuthService.cs
- Infrastructure/AuthSettingsFromConfig.cs
- Content/js/login-otp-manager.js

[کپی بخش‌های مربوط به Code Quality از Prompt]
```

---

## 🎯 Checklist Quick Reference

### Security (امنیت)
- [ ] Cryptographically Secure Random
- [ ] Hash Storage (not Plain Text)
- [ ] Salt Usage
- [ ] Rate Limiting
- [ ] Brute Force Protection
- [ ] Cookie Security (HttpOnly, Secure, SameSite)

### UX (تجربه کاربری)
- [ ] Separate Inputs
- [ ] Auto-focus
- [ ] Auto-submit
- [ ] Paste Support
- [ ] Countdown Timer
- [ ] Clear Error Messages

### Compliance (رعایت قوانین)
- [ ] SMS Content Safe
- [ ] User Consent
- [ ] Audit Trail
- [ ] No Sensitive Data in Logs

### Performance (عملکرد)
- [ ] SMS Queue
- [ ] Retry with Backoff
- [ ] Fallback Provider
- [ ] Timeout Configuration

---

## 📊 Expected Output Format

### 1. Security Report
```
✅ Strengths:
- OTP is hashed with HMACSHA256
- Rate limiting implemented
- ...

⚠️ Weaknesses:
- Cookie security flags need verification
- ...

🔧 Recommendations:
- Add HttpOnly flag to cookies
- ...
```

### 2. JSON Summary
```json
{
  "security_score": "8/10",
  "ux_score": "7/10",
  "compliance_score": "9/10",
  "performance_score": "6/10",
  "code_quality_score": "9/10",
  "critical_issues": [
    "Cookie security flags not verified"
  ],
  "high_priority_issues": [
    "SMS queue not implemented",
    "Countdown timer missing in UI"
  ],
  "medium_priority_issues": [
    "Consent checkbox missing in signup"
  ],
  "low_priority_issues": [
    "Delivery rate monitoring not implemented"
  ],
  "production_readiness": "ready_with_recommendations"
}
```

---

## 🔍 Advanced Audit Scenarios

### Scenario 1: Security Penetration Testing
```
بررسی سیستم OTP از نظر حملات:
1. Brute Force Attack
2. Rate Limit Bypass
3. Session Hijacking
4. OTP Replay Attack
5. Timing Attack on Hash Comparison
```

### Scenario 2: Load Testing
```
بررسی سیستم OTP تحت بار:
1. 1000 concurrent OTP requests
2. SMS Provider failure
3. Database connection timeout
4. Session storage overflow
```

### Scenario 3: Compliance Audit
```
بررسی رعایت قوانین:
1. GDPR Compliance (if applicable)
2. Medical Data Protection
3. SMS Consent
4. Audit Trail Completeness
```

---

**تاریخ:** 2025-01-27  
**نسخه:** 1.0  
**استفاده:** کپی و ارسال به AI برای بررسی کامل

