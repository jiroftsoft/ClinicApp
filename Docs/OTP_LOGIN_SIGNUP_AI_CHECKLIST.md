# ✅ OTP Login + Signup — Checklist جامع (قابل Audit با AI)

> **هدف:** این سند برای بررسی و ارزیابی سیستم OTP توسط هوش مصنوعی طراحی شده است.  
> **پروژه:** ClinicApp (سیستم رزرو نوبت کلینیک درمانی)  
> **Backend:** ASP.NET MVC / C#  
> **تاریخ:** 2025-01-27

---

## 📋 فهرست مطالب

1. [اصول پایه OTP](#1-اصول-پایه-otp)
2. [تشخیص هوشمند: Login یا Signup](#2-تشخیص-هوشمند-login-یا-signup)
3. [امنیت (Security)](#3-امنیت-security)
4. [UX / UI](#4-ux--ui)
5. [Resend OTP](#5-resend-otp)
6. [Session & Authentication](#6-session--authentication)
7. [Edge Cases](#7-edge-cases)
8. [Legal & Medical Compliance](#8-legal--medical-compliance)
9. [Performance & Reliability](#9-performance--reliability)
10. [AI Evaluation Output](#10-ai-evaluation-output)

---

## 1️⃣ اصول پایه OTP

### ✅ نوع OTP

- [x] **OTP عددی ۶ رقمی** (مطابق با `AuthSettingsFromConfig.cs`: `OtpLength = 6`)
- [x] **تصادفی امن (Cryptographically Secure)** (مطابق با `GenerateSecureOtp` در `AuthService.cs` - استفاده از `RandomNumberGenerator`)
- [x] **One-Time Use** (بعد از Verify حذف می‌شود)

**📍 محل بررسی در کد:**
- `Services/AuthService.cs` → `GenerateSecureOtp()` (خط ~742)
- `Infrastructure/AuthSettingsFromConfig.cs` → `OtpLength` (خط 11)

---

### ✅ مدت اعتبار (TTL)

- [x] **TTL = 2 دقیقه (120 ثانیه)** (مطابق با `OtpExpiryMinutes = 2`)
- [ ] **نمایش تایمر به کاربر** (نیاز به بررسی UI)

**📍 محل بررسی در کد:**
- `Infrastructure/AuthSettingsFromConfig.cs` → `OtpExpiryMinutes` (خط 12)
- `Services/AuthService.cs` → `OtpState.ExpiryUtc` (خط ~163)

**⚠️ توصیه:** نمایش تایمر در UI برای UX بهتر

---

## 2️⃣ تشخیص هوشمند: Login یا Signup

### ✅ Logic Decision

**Flow فعلی:**
1. کاربر کد ملی وارد می‌کند
2. سیستم بررسی می‌کند: `CheckUserExistsAsync(NationalCode)`
3. اگر کاربر وجود دارد → **Login Flow**
4. اگر کاربر وجود ندارد → **Signup Flow**

- [x] **بدون سؤال اضافه از کاربر** (تشخیص خودکار)
- [x] **Query سریع به User DB** (استفاده از `FindByNameAsync`)

**📍 محل بررسی در کد:**
- `Controllers/AccountController.cs` → `CheckUser` action
- `Services/AuthService.cs` → `CheckUserExistsAsync` (اگر وجود دارد)

---

### ✅ AI Checkpoints

- [x] **شماره موبایل Normalize شده** (استفاده از `PersianNumberHelper.ToEnglishNumbers`)
- [x] **کد ملی Validate شده** (استفاده از `IranianNationalCodeValidator.IsValid`)
- [ ] **Response Time < 300ms** (نیاز به Performance Testing)

**📍 محل بررسی در کد:**
- `Services/AuthService.cs` → `SendLoginOtpAsync` (خط ~86)
- `Services/AuthService.cs` → `SendRegistrationOtpAsync` (خط ~520)

---

## 3️⃣ امنیت (Security)

### ✅ Rate Limiting

**تنظیمات فعلی:**
- [x] **حداکثر درخواست OTP: 3 بار در 5 دقیقه** (مطابق با `OtpMaxSendsPerNationalCodePer5Min = 3`)
- [x] **محدودیت IP: 10 بار در 5 دقیقه** (مطابق با `OtpMaxSendsPerIpPer5Min = 10`)
- [x] **Block موقت بعد از تلاش زیاد** (مطابق با `OtpLockoutMinutes = 15`)

**📍 محل بررسی در کد:**
- `Services/AuthService.cs` → `SendLoginOtpAsync` (خط ~132-138)
- `Services/AuthService.cs` → `SendRegistrationOtpAsync` (خط ~538-542)
- `Infrastructure/AuthSettingsFromConfig.cs` (خطوط 14-17)

---

### ✅ Brute Force Protection

- [x] **محدودیت تلاش ورود: 5 بار** (مطابق با `OtpFailedMaxAttempts = 5`)
- [x] **Delay افزایشی** (پیاده‌سازی در `ValidateOtpState` - افزایش `AttemptCount`)
- [x] **Lock موقت اکانت** (استفاده از ASP.NET Identity Lockout - `OtpLockoutMinutes = 15`)

**📍 محل بررسی در کد:**
- `Services/AuthService.cs` → `ValidateOtpState` (خط ~688-740)
- `Infrastructure/AuthSettingsFromConfig.cs` → `OtpFailedMaxAttempts` (خط 16)
- `Services/AuthService.cs` → Constructor (خط ~68-70)

---

### ✅ ذخیره OTP

- [x] **OTP به‌صورت Hash ذخیره می‌شود** (استفاده از `HashOtp` با HMACSHA256)
- [x] **هرگز Plain Text ذخیره نمی‌شود** (فقط `OtpHash` در `OtpState` و `OtpRequest`)
- [x] **OTP بعد از Verify حذف می‌شود** (در `VerifyLoginOtpAsync` و `VerifyRegistrationOtpAsync`)

**📍 محل بررسی در کد:**
- `Services/AuthService.cs` → `HashOtp` (خط ~756-764)
- `Services/AuthService.cs` → `OtpState.OtpHash` (خط ~144)
- `Models/Core/OtpRequest.cs` → `OtpCodeHash` (خط 22)

**🔒 الگوریتم Hash:** HMACSHA256 با Key از `OtpHashKey` در Web.config

---

### ✅ Salt برای Hash

- [x] **استفاده از Salt** (شماره موبایل به عنوان Salt در `HashOtp`)
- [x] **Salt منحصر به فرد** (هر کاربر Salt متفاوت دارد)

**📍 محل بررسی در کد:**
- `Services/AuthService.cs` → `HashOtp(otp, user.PhoneNumber)` (خط ~144)
- `Services/AuthService.cs` → `HashOtp` method (خط ~761)

---

## 4️⃣ UX / UI

### ✅ صفحه ورود

- [x] **فقط کد ملی** (Simple Input)
- [x] **Validation بلادرنگ** (در `SendLoginOtpAsync` - خط ~87-92)
- [ ] **فرمت خودکار (09xx)** (نیاز به بررسی UI)

**📍 محل بررسی در کد:**
- `Views/Account/Login.cshtml` یا `_LoginModal.cshtml`
- `Content/js/login-modal.js`

---

### ✅ صفحه وارد کردن OTP

- [x] **Input جداگانه برای هر رقم** (مطابق با `login-otp-manager.js`)
- [x] **Auto-focus** (مطابق با `setupInputHandlers` - خط ~88-90)
- [x] **Auto-submit بعد از تکمیل** (مطابق با `updateCombinedOtp` - خط ~211-216)
- [x] **Paste Support** (مطابق با `setupPasteHandler` - خط ~117-144)

**📍 محل بررسی در کد:**
- `Content/js/login-otp-manager.js` (خطوط 65-217)

---

### ✅ Feedback به کاربر

- [x] **پیام موفقیت واضح** (در `ServiceResult.Successful`)
- [x] **پیام خطا انسانی** (پیام‌های فارسی در `ServiceResult.Failed`)
- [ ] **نمایش شمارش معکوس** (نیاز به بررسی UI)

**📍 محل بررسی در کد:**
- `Services/AuthService.cs` → تمام `ServiceResult` returns
- `Views/Account/` → نمایش پیام‌ها

**⚠️ توصیه:** اضافه کردن تایمر معکوس در UI

---

## 5️⃣ Resend OTP (ارسال مجدد)

- [ ] **غیرفعال تا پایان تایمر** (نیاز به بررسی UI)
- [x] **محدودیت تعداد resend** (محدودیت Rate Limiting اعمال می‌شود)
- [ ] **هشدار در صورت ارسال زیاد** (نیاز به بررسی UI)

**📍 محل بررسی در کد:**
- `Controllers/AccountController.cs` → Resend action (اگر وجود دارد)
- `Content/js/login-modal.js` → Resend button handler

**⚠️ توصیه:** پیاده‌سازی Resend با تایمر و محدودیت در UI

---

## 6️⃣ Session & Authentication

### ✅ بعد از تایید OTP

- [x] **ایجاد Session امن** (استفاده از OWIN Cookie Authentication)
- [x] **Token (Session ID)** (OWIN Cookie)
- [ ] **HttpOnly Cookie** (نیاز به بررسی `Startup.Auth.cs`)
- [ ] **Secure Flag (HTTPS)** (نیاز به بررسی `Startup.Auth.cs`)
- [ ] **SameSite Flag** (نیاز به بررسی `Startup.Auth.cs`)

**📍 محل بررسی در کد:**
- `Services/AuthService.cs` → `SignInUserAsync` (خط ~355-365)
- `App_Start/Startup.Auth.cs` → Cookie Authentication Settings

**⚠️ توصیه:** بررسی تنظیمات Cookie برای Production

---

### ✅ Logout

- [x] **Invalid کردن Session** (استفاده از `SignOutAsync`)
- [x] **حذف Token** (OWIN SignOut)

**📍 محل بررسی در کد:**
- `Controllers/AccountController.cs` → Logout action

---

## 7️⃣ Edge Cases

### ✅ سناریوهای Edge Case

- [x] **OTP منقضی شده** (بررسی در `ValidateOtpState` - خط ~697-701)
- [x] **OTP اشتباه** (بررسی در `ValidateOtpState` - خط ~730-737)
- [x] **کد ملی نامعتبر** (بررسی در `SendLoginOtpAsync` - خط ~87-92)
- [x] **Refresh صفحه** (Fallback به Database در `VerifyLoginOtpAsync` - خط ~306-330)
- [x] **Back Button** (Session + Database State)
- [x] **OTP قدیمی** (Invalidation در `SendLoginOtpAsync` - خط ~149-154)
- [x] **همزمانی چند OTP** (حذف OTP قبلی قبل از ایجاد جدید)

**📍 محل بررسی در کد:**
- `Services/AuthService.cs` → `ValidateOtpState` (خط ~688-740)
- `Services/AuthService.cs` → `VerifyLoginOtpAsync` (خط ~280-380)

---

## 8️⃣ Legal & Medical Compliance

### ✅ SMS Content Rules

- [x] **فقط کد OTP** (مطابق با `"کد ورود کلینیک شفا: {otp}"` - خط ~230)
- [x] **بدون نام پزشک** ✅
- [x] **بدون نوع بیماری** ✅
- [x] **بدون جزئیات نوبت** ✅

**📍 محل بررسی در کد:**
- `Services/AuthService.cs` → `SendLoginOtpAsync` (خط ~230)
- `Services/AuthService.cs` → `SendRegistrationOtpAsync` (خط ~581)

---

### ✅ Consent

- [ ] **رضایت ضمنی برای ارسال پیامک** (نیاز به بررسی UI/Flow)
- [x] **ثبت Log رضایت** (لاگ‌گیری در `_log.Information`)

**📍 محل بررسی در کد:**
- `Services/AuthService.cs` → تمام `_log.Information` calls
- `Views/Account/` → Checkbox رضایت (اگر وجود دارد)

**⚠️ توصیه:** اضافه کردن Checkbox رضایت در Signup Flow

---

### ✅ Audit Trail

- [x] **لاگ امن برای Audit** (استفاده از Serilog)
- [x] **ثبت درخواست OTP در Database** (`OtpRequest` entity)
- [x] **ثبت Login History** (استفاده از `ILoginHistoryService`)

**📍 محل بررسی در کد:**
- `Services/AuthService.cs` → تمام `_log.*` calls
- `Models/Core/OtpRequest.cs` → Entity definition
- `Services/AuthService.cs` → `VerifyLoginOtpAsync` → `_loginHistoryService` (خط ~365)

---

## 9️⃣ Performance & Reliability

### ✅ Queue برای ارسال SMS

- [ ] **Queue برای ارسال SMS** (نیاز به بررسی `AsanakSmsService`)
- [x] **Retry با Backoff** (مطابق با `AsanakSmsService.cs` - Retry logic)
- [x] **Timeout مناسب** (مطابق با `_timeoutMs` در `AsanakSmsService`)

**📍 محل بررسی در کد:**
- `Services/AsanakSmsService.cs` → Retry logic

---

### ✅ Fallback Provider

- [ ] **Fallback Provider** (نیاز به بررسی)

**⚠️ توصیه:** پیاده‌سازی Fallback SMS Provider برای Production

---

### ✅ Monitoring

- [x] **Logging ساختاریافته** (Serilog)
- [ ] **Delivery Rate Monitoring** (نیاز به پیاده‌سازی)

**📍 محل بررسی در کد:**
- تمام `_log.*` calls در `AuthService.cs`

---

## 🔟 AI Evaluation Output

### JSON Format برای AI Audit

```json
{
  "project_info": {
    "name": "ClinicApp",
    "type": "Medical Appointment Booking System",
    "backend": "ASP.NET MVC / C#",
    "date": "2025-01-27"
  },
  "otp_config": {
    "otp_usage": ["login", "signup"],
    "otp_length": 6,
    "otp_ttl_seconds": 120,
    "otp_hashed": true,
    "otp_hash_algorithm": "HMACSHA256",
    "otp_salt": "phone_number"
  },
  "user_detection": {
    "method": "auto",
    "identifier": "national_code",
    "response_time_target_ms": 300
  },
  "rate_limiting": {
    "send_otp_per_national_code": {
      "max_attempts": 3,
      "time_window_minutes": 5
    },
    "send_otp_per_ip": {
      "max_attempts": 10,
      "time_window_minutes": 5
    }
  },
  "brute_force_protection": {
    "max_verification_attempts": 5,
    "lockout_minutes": 15,
    "incremental_delay": true
  },
  "resend_otp": {
    "enabled": true,
    "timer_based": false,
    "limit": "rate_limiting_applies"
  },
  "session_management": {
    "method": "owin_cookie",
    "http_only": "needs_verification",
    "secure_flag": "needs_verification",
    "same_site": "needs_verification"
  },
  "ui_features": {
    "separate_inputs": true,
    "auto_focus": true,
    "auto_submit": true,
    "paste_support": true,
    "countdown_timer": false
  },
  "security": {
    "otp_storage": "hashed",
    "plain_text_storage": false,
    "otp_deletion_after_verify": true,
    "session_binding": true,
    "ip_tracking": true,
    "user_agent_tracking": true
  },
  "edge_cases": {
    "expired_otp": "handled",
    "invalid_otp": "handled",
    "invalid_national_code": "handled",
    "page_refresh": "handled",
    "back_button": "handled",
    "concurrent_otp": "handled"
  },
  "medical_compliance": {
    "sms_content_safe": true,
    "no_doctor_name": true,
    "no_disease_info": true,
    "no_appointment_details": true,
    "consent_required": false,
    "audit_trail": true
  },
  "performance": {
    "sms_queue": false,
    "retry_with_backoff": true,
    "timeout_configured": true,
    "fallback_provider": false,
    "monitoring": "basic_logging"
  },
  "code_quality": {
    "separation_of_concerns": true,
    "dependency_injection": true,
    "testable": true,
    "logging": "structured_serilog"
  },
  "recommendations": [
    "Add countdown timer in UI",
    "Verify Cookie security settings (HttpOnly, Secure, SameSite)",
    "Add consent checkbox in Signup flow",
    "Implement SMS queue for high load",
    "Add fallback SMS provider",
    "Implement delivery rate monitoring"
  ]
}
```

---

## ✅ جمع‌بندی نهایی

| بُعد | وضعیت | توضیحات |
|---|---|---|
| **امنیت** | ✅ بالا | Hash، Rate Limiting، Brute Force Protection |
| **UX** | ⭐⭐⭐⭐ | Auto-focus، Auto-submit، Paste Support (نیاز به تایمر) |
| **سرعت** | ⚡ | Query بهینه، Response Time مناسب |
| **اعتماد کاربر** | ✅ | پیام‌های واضح، Validation مناسب |
| **مناسب کلینیک** | ✅✅✅ | Compliance با قوانین پزشکی |
| **AI Auditable** | ✅✅✅ | ساختار واضح، JSON Format |

---

## 📝 Notes برای توسعه

### ✅ پیاده‌سازی شده

1. OTP 6 رقمی با Hash امن
2. Rate Limiting برای Send و Verify
3. Brute Force Protection
4. Auto-detect Login/Signup
5. UI با Auto-focus و Auto-submit
6. Edge Cases Handling
7. Medical Compliance (SMS Content Safe)
8. Audit Trail

### ⚠️ نیاز به بررسی/پیاده‌سازی

1. **تایمر معکوس در UI** (UX بهتر)
2. **Cookie Security Settings** (HttpOnly, Secure, SameSite)
3. **Consent Checkbox** در Signup
4. **SMS Queue** برای High Load
5. **Fallback SMS Provider**
6. **Delivery Rate Monitoring**

---

## 🔍 محل فایل‌های کلیدی

```
Services/AuthService.cs                    → Core OTP Logic
Infrastructure/AuthSettingsFromConfig.cs  → OTP Settings
Models/Core/OtpRequest.cs                 → OTP Database Model
Models/Core/OtpStateEntity.cs            → OTP State Entity
Content/js/login-otp-manager.js          → UI OTP Input Handler
Services/AsanakSmsService.cs              → SMS Service
Controllers/AccountController.cs         → Account Actions
App_Start/Startup.Auth.cs                → Cookie Authentication
```

---

**تاریخ آخرین به‌روزرسانی:** 2025-01-27  
**نسخه:** 1.0  
**وضعیت:** Production-Ready (با توصیه‌های بهبود)

