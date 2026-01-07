# 📚 گزارش مهندسی کامل فولدر Contracts - ClinicApp

**تاریخ بررسی:** 2026-01-06  
**نوع بررسی:** مهندسی سیستماتیک و جامع  
**هدف:** درک کامل قراردادها، استانداردها و Knowledge-Base پروژه کلینیک شفا  
**نسخه:** 2.0.0 (به‌روزرسانی شده)

---

## 📋 خلاصه اجرایی

فولدر `Contracts` شامل **سیستم جامع مدیریت قراردادها و استانداردهای پروژه** است که شامل:

- **قراردادهای AI Preflight** (7 فایل) - الزامات قبل از هر پاسخ AI
- **قراردادهای اجرایی** (2 فایل) - قواعد کلی توسعه
- **قراردادهای ایمنی** (2 فایل) - حفاظت از ماژول‌های حیاتی
- **Knowledge-Base** (100+ فایل) - پایگاه دانش کامل پروژه

**کل فایل‌ها:** 129+ فایل Markdown  
**ساختار:** سلسله‌مراتبی و سازمان‌یافته  
**وضعیت:** ✅ فعال و به‌روز

---

## 🏗️ ساختار کلی فولدر Contracts

```
Contracts/
├── 📄 قراردادهای اصلی (Root Level)
│   ├── AI_EXECUTION_CONTRACT.md ⭐ (الزامی)
│   ├── AI_PREFLIGHT_MASTER_V3.md ⭐ (الزامی)
│   ├── AI_PREFLIGHT_QUICK_V3.md ⭐ (الزامی)
│   ├── CRITICAL_MODULE_SAFETY_CONTRACT.md ⭐ (الزامی)
│   ├── PASTE_THIS_EVERY_CHAT.md (یادآوری)
│   ├── CONTRACTS_ENGINEERING_REPORT.md (این فایل)
│   └── ...
│
├── 📁 1015/ (گزارش‌های تاریخی)
│   ├── CLINICAPP_APPOINTMENT_BOOKING_MODULE_PROMPT.md
│   ├── CLINICAPP_CURSOR_7ROLES_CONTRACT_PROMPT_OPTIMIZED.md
│   └── CLINICAPP_RULES_SNAPSHOT_ONE_PAGE.md
│
└── 📚 Knowledge-Base/ (پایگاه دانش)
    ├── AI/ (قراردادهای AI)
    │   ├── Master/ (راهنماهای اصلی)
    │   ├── PreFlight/ (چک‌لیست‌های پیش‌پرواز)
    │   ├── REVIEWS/ (قراردادهای بررسی)
    │   ├── CURSOR/ (Cursor-specific prompts)
    │   ├── CHECKLISTS/ (چک‌لیست‌های تخصصی)
    │   ├── PROMPTS/ (Prompts آماده)
    │   ├── QUALITY/ (استانداردهای کیفیت)
    │   ├── RELEASE/ (چک‌لیست‌های Release)
    │   ├── ARCH/ (معماری)
    │   ├── View/ (استانداردهای View)
    │   └── DB/ (راهنمای Database)
    │
    └── [Historical Reports] (گزارش‌های تاریخی - در صورت وجود)
```

---

## 1️⃣ قراردادهای اصلی (Root Level)

### ⚡ **AI_EXECUTION_CONTRACT.md** - قرارداد اجرایی AI

**اولویت:** 🔴 **الزامی قبل از هر پاسخ**

**محتوا:**
- چک‌لیست 30 ثانیه‌ای قبل از هر پاسخ
- 15 ممنوعیت AI No-Fly Zone
- الزامات هر پاسخ (Code, Security, Standards)
- Workflow برای کارهای معمولی، مالی و باگ
- HARD STOP در صورت تعارض

**قوانین کلیدی:**
```
1. حدس زدن ❌
2. نقض قرارداد ❌
3. Controller→DB مستقیم ❌
4. حذف ServiceResult ❌
5. کد بدون Log ❌
... (15 قانون)
```

**استفاده:**
- قبل از هر پاسخ AI باید این قرارداد بررسی شود
- در صورت تعارض → HARD STOP

**مسیر:** `Contracts/AI_EXECUTION_CONTRACT.md`

---

### 🛡️ **AI_PREFLIGHT_MASTER_V3.md** - قرارداد پیش‌پرواز اصلی

**اولویت:** 🔴 **الزامی برای کارهای پیچیده**

**محتوا:**
- STEP 0: AI Guard Check (15 ممنوعیت)
- STEP 1: 12 دروازه امنیتی
- STEP 2: Financial Module Check (10 قانون طلایی)
- STEP 3: Debugging Protocol (6 مرحله)
- STEP 4: Standards & Contracts
- STEP 5: Implementation
- STEP 6: Testing & Verification
- STEP 7: Documentation & Report

**12 دروازه امنیتی:**
1. Alignment Check
2. Contract Enforcement
3. Architecture Gate
4. Security Gate
5. Standards Gate
6. Data Integrity Gate
7. Code Quality Gate
8. Performance Gate
9. UI/UX Gate
10. Testing Gate
11. Documentation Gate
12. Backward Compatibility Gate

**استفاده:**
- برای کارهای پیچیده (مالی، باگ، تغییرات بزرگ)
- قبل از هر Implementation

**مسیر:** `Contracts/AI_PREFLIGHT_MASTER_V3.md`

---

### ⚡ **AI_PREFLIGHT_QUICK_V3.md** - قرارداد پیش‌پرواز سریع

**اولویت:** 🔴 **الزامی برای کارهای معمولی**

**محتوا:**
- نسخه خلاصه‌شده AI_PREFLIGHT_MASTER_V3
- چک‌لیست سریع 30 ثانیه‌ای
- 15 ممنوعیت
- Quick Reference

**استفاده:**
- برای کارهای معمولی (نه مالی، نه باگ)
- قبل از هر پاسخ AI

**مسیر:** `Contracts/AI_PREFLIGHT_QUICK_V3.md`

---

### 🚨 **CRITICAL_MODULE_SAFETY_CONTRACT.md** - قرارداد ایمنی ماژول‌های حیاتی

**اولویت:** 🔴 **الزامی برای تغییرات حیاتی**

**قانون طلایی:**
```
🚫 NO BLIND CHANGES
هیچ تغییری روی ماژول‌های حیاتی بدون درک کامل منطق سیستم
```

**ماژول‌های حیاتی (سطح 1 - CRITICAL):**
1. Authentication & Authorization
2. Patient Management
3. Financial & Payment
4. Database Relationships

**الزامات قبل از تغییر:**
1. درک منطق سیستم (System Logic Understanding)
2. سوال از کاربر (User Confirmation)
3. تحلیل تأثیرات (Impact Analysis)
4. تغییرات تدریجی (Incremental Changes)

**فرآیند صحیح:**
```
1️⃣ STOP & THINK
2️⃣ ASK USER
3️⃣ ANALYZE IMPACT
4️⃣ PROPOSE SOLUTION
5️⃣ IMPLEMENT (بعد از تأیید)
6️⃣ VERIFY
```

**استفاده:**
- قبل از هر تغییر روی ماژول‌های حیاتی
- بالاترین اولویت در صورت تعارض

**مسیر:** `Contracts/CRITICAL_MODULE_SAFETY_CONTRACT.md`

---

### 💰 **CRITICAL-FINANCIAL-MODULE-CONTRACT.md** - قرارداد ماژول‌های مالی

**اولویت:** 🔴 **الزامی برای تغییرات مالی**

**10 قانون طلایی مالی:**
1. هیچ تغییری بدون تست کامل (5 سناریو)
2. هر تراکنش = حتماً Log کامل
3. Transaction Management الزامی
4. Verification بعد از Save
5. Idempotency برای همه پرداخت‌ها
6. هیچ Hard-Delete در جداول مالی
7. Audit Trail کامل
8. decimal(18,0) برای تمام مبالغ IRR
9. Concurrency Control (RowVersion)
10. Code Review توسط Senior قبل از Merge

**ممنوعیت‌های مطلق:**
```csharp
// ❌ تغییر مبلغ بدون log
payment.Amount = newAmount;

// ❌ Hard Delete
_context.PaymentTransactions.Remove(payment);

// ❌ بدون Transaction
_context.PaymentTransactions.Add(payment);
await _context.SaveChangesAsync();
```

**استفاده:**
- برای هر تغییر در: CashSession, PaymentTransaction, Reports, محاسبات مالی
- کوچکترین اشتباه = مشکل حقوقی!

**مسیر:** `Contracts/CRITICAL-FINANCIAL-MODULE-CONTRACT.md` (در صورت وجود)

---

## 2️⃣ Knowledge-Base - پایگاه دانش

### 📚 **AI/Master/** - راهنماهای اصلی

#### **README.md** - راهنمای اصلی Knowledge-Base

**محتوا:**
- هدف پایگاه دانش
- نحوه استفاده
- فهرست کامل Helpers
- مسیر یادگیری پیشنهادی
- FAQ

**فایل‌های الزامی:**
1. `03-Development-Contract-Quick-Guide.md` ⚡
2. `04-TODO-Implementation-Guide.md` ⚡
3. `05-Debugging-Specialist-Contract.md` 🔧

**مسیر:** `Contracts/Knowledge-Base/AI/Master/README.md`

---

#### **03-Development-Contract-Quick-Guide.md** - قرارداد توسعه

**محتوا:**
- اصول اساسی (Non-Negotiable)
- پالت رنگ استاندارد (`--medical-*`)
- Strongly-Typed Development
- Bulletproof Coding (try-catch, null check, validation)
- معماری SRP (Controller, Service, Repository)
- سیستم پیام‌ها (Toastr, SweetAlert2)
- تقویم شمسی (Persian DatePicker)
- سیستم آپلود تصویر
- CKEditor (ویرایشگر متن)
- فرم‌های درمانی (Medical Forms)
- Checklist نهایی قبل از Commit

**رنگ‌های مجاز:**
```css
--medical-primary: #2c5aa0;      /* آبی درمانی */
--medical-secondary: #6c757d;    /* خاکستری */
--medical-success: #28a745;      /* سبز */
--medical-danger: #dc3545;       /* قرمز */
--medical-warning: #ffc107;      /* زرد */
```

**رنگ‌های ممنوع:**
- بنفش جیغ (#9b59b6, #8e44ad)
- صورتی (#e91e63, #f06292)
- نارنجی تند (#ff5722, #ff9800)
- گرادینت‌های فانتزی

**استفاده:**
- قبل از هر کد زدن
- Checklist نهایی قبل از Commit

**مسیر:** `Contracts/Knowledge-Base/AI/Master/03-Development-Contract-Quick-Guide.md`

---

#### **04-TODO-Implementation-Guide.md** - راهنمای پیاده‌سازی

**محتوا:**
- Quick Start Checklist
- 13 Phase پیاده‌سازی:
  1. Analysis & Design
  2. Backend Implementation
  3. Controller Implementation
  4. View Implementation
  5. UI/UX Optimization
  6. Color Scheme Standardization
  7. Notification System
  8. Persian DatePicker Integration
  9. CKEditor Integration
  10. Image Upload System
  11. Medical Form Design Standards
  12. Testing & QA
  13. Deployment Preparation
- زمان‌بندی کلی (12-17 روز کاری)
- Template TODO آماده کپی
- Checklist نهایی قبل از Commit

**استفاده:**
- برای پیاده‌سازی ماژول جدید
- Phase به Phase پیش رفتن

**مسیر:** `Contracts/Knowledge-Base/AI/Master/04-TODO-Implementation-Guide.md`

---

#### **05-Debugging-Specialist-Contract.md** - متخصص دیباگر

**محتوا:**
- فرآیند استاندارد دیباگ (6 مرحله)
- تحلیل علت ریشه‌ای (5 Whys)
- بررسی وابستگی‌ها
- رفع اتمیک (Atomic Fix)
- تست و اعتبارسنجی
- گزارش‌دهی حرفه‌ای
- ابزارهای دیباگ (Static Analysis, Runtime, Database)
- نمونه‌های کاربردی (Compilation Error, N+1 Query, Memory Leak)
- چک‌لیست کامل
- سطوح دیباگ (Level 1-4)
- الگوهای رایج خطا
- نکات طلایی

**قانون اصلی:**
```
❌ ممنوع رفع کورکورانه!
✅ همیشه: شناسایی → تحلیل → رفع → تست → گزارش
```

**6 مرحله الزامی:**
1. شناسایی و دسته‌بندی
2. تحلیل علت ریشه‌ای (5 Whys)
3. بررسی وابستگی‌ها
4. رفع اتمیک
5. تست و اعتبارسنجی
6. گزارش‌دهی حرفه‌ای

**استفاده:**
- برای رفع هر خطا یا باگ
- قبل از هر تغییر در کد

**مسیر:** `Contracts/Knowledge-Base/AI/PreFlight/05-Debugging-Specialist-Contract.md`

---

#### **01-Helpers-DateTime.md** - Helpers تاریخ و زمان

**محتوا:**
- `PersianDateHelper.cs` - تبدیل میلادی ↔ شمسی
- `PersianDatePickerHelper.cs` - DatePicker در View
- `DateTimeExtensions.cs` - Extension برای DateTime
- `PersianDateExtensions.cs` - Extension تاریخ شمسی
- `TimeFormatHelper.cs` - فرمت زمان
- `AgeCalculationHelper.cs` - محاسبه سن
- `ControllerExtensions.ParseDateFromHiddenInput` - Parse تاریخ در Controller

**مثال:**
```csharp
// تبدیل میلادی به شمسی
var persianDate = PersianDateHelper.ToPersianDate(DateTime.Now);

// Parse تاریخ در Controller
model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
```

**مسیر:** `Contracts/Knowledge-Base/AI/Master/01-Helpers-DateTime.md`

---

#### **02-Helpers-Validation.md** - Helpers اعتبارسنجی

**محتوا:**
- `IranianNationalCodeValidator.cs` - کد ملی
- `PhoneNumberValidator.cs` - شماره تلفن
- `PhoneNumberHelper.cs` - نرمال‌سازی تلفن
- `IdentityValidators.cs` - Identity
- `ValidationResult.cs` - نتیجه Validation
- `SecurityValidationResult.cs` - Validation امنیتی

**مثال:**
```csharp
// Validation کد ملی
if (!IranianNationalCodeValidator.IsValid(model.NationalCode)) { ... }

// Validation شماره موبایل
if (!PhoneNumberValidator.IsValidMobile(model.PhoneNumber)) { ... }
```

**مسیر:** `Contracts/Knowledge-Base/AI/Master/02-Helpers-Validation.md`

---

#### **HelperExtensionsGuide.md** - جعبه ابزار کامل

**محتوا:**
- **5 Extensions:** StringExtensions, DateTimeExtensions, NumericExtensions, CollectionExtensions, ObjectExtensions
- **8 Helpers:** CacheHelper, RetryHelper, SecurityHelper, ValidationHelper, FileHelper, HtmlHelper, UrlHelper, ImageHelper
- **100+ متد** کاربردی

**ویژگی‌ها:**
- ✅ رعایت SRP
- ✅ XML Documentation کامل
- ✅ Null Safety
- ✅ Error Handling
- ✅ Performance Optimized

**مسیر:** `Contracts/Knowledge-Base/AI/Master/HelperExtensionsGuide.md`

---

#### **08-MVC-Routing-Best-Practices.md** - بهترین روش‌های Routing

**محتوا:**
- Route Registration
- Area Routes
- Route Constraints
- Route Naming
- Route Resolution
- Best Practices

**درس‌های گرانبها:**
- استفاده از RouteUrl با route name مشخص
- استفاده از UseNamespaceFallback = false
- Route های خاص قبل از default route
- Constraint برای route parameters

**مسیر:** `Contracts/Knowledge-Base/AI/Master/08-MVC-Routing-Best-Practices.md`

---

### 📊 **AI/PreFlight/ClinicApp_Knowledge_Base.md** - پایگاه دانش پروژه

**محتوا:**
- اطلاعات کلی پروژه
- ماژول‌های بررسی شده
- ساختار پروژه
- فولدرهای Helper و Extensions
- کلاس‌های Seed
- مدل‌های کلیدی
- روابط کلیدی بین موجودیت‌ها
- ویژگی‌های فنی کلیدی

**اطلاعات پروژه:**
- نام: ClinicApp - سیستم مدیریت کلینیک شفا
- فناوری: .NET MVC 5 + Entity Framework Code First
- معماری: Clean Architecture + Repository Pattern + Service Layer
- پایگاه داده: SQL Server
- لاگ‌گیری: Serilog
- تزریق وابستگی: Unity Container

**مسیر:** `Contracts/Knowledge-Base/AI/PreFlight/ClinicApp_Knowledge_Base.md`

---

### 📁 **سایر فولدرهای Knowledge-Base**

#### **AI/CURSOR/** - Cursor-Specific Prompts
- `CLINICAPP_CURSOR_MASTER_CONTEX.md` - Master Context
- `CLINICAPP_MAIN_MENU_EXECUTION_PROMPT.md` - Main Menu
- `CLINICAPP_VIEW_UI_BEAST_MODE_PROMPT_FINAL.md` - View UI
- و سایر Prompts تخصصی

#### **AI/REVIEWS/** - قراردادهای بررسی
- `Bugfix-Master-Contract.md` - قرارداد رفع باگ
- `CLINICAPP_VIEW_REVIEW_CHECKLIST.md` - چک‌لیست View

#### **AI/CHECKLISTS/** - چک‌لیست‌های تخصصی
- `CLINICAPP_OTP_LOGIN_REGISTRATION_CHECKLIST.md` - OTP Login

#### **AI/RELEASE/** - چک‌لیست‌های Release
- `CLINICAPP_PRODUCTION_READINESS_CHECKLIST.md` - آمادگی Production

---

## 3️⃣ سلسله‌مراتب اولویت قراردادها

```
1. CRITICAL_MODULE_SAFETY_CONTRACT.md ← ⭐ بالاترین اولویت
2. CRITICAL-FINANCIAL-MODULE-CONTRACT.md (اگر مالی)
3. AI_EXECUTION_CONTRACT.md
4. AI_PREFLIGHT_MASTER_V3.md (اگر پیچیده)
5. AI_PREFLIGHT_QUICK_V3.md (اگر معمولی)
6. Knowledge-Base (اگر نیاز به Helper/Standard)
```

**در صورت تعارض:**
- CRITICAL_MODULE_SAFETY_CONTRACT.md اولویت دارد
- سپس CRITICAL-FINANCIAL-MODULE-CONTRACT.md (اگر مالی)
- سپس AI_EXECUTION_CONTRACT.md

---

## 4️⃣ Workflow پیشنهادی برای استفاده

### ⚡ **برای کارهای معمولی:**

```
1. AI_EXECUTION_CONTRACT.md (10s)
2. AI_PREFLIGHT_QUICK_V3.md (30s)
3. Knowledge-Base (اگر نیاز به Helper)
4. Implementation
5. Checklist نهایی
```

### 💰 **برای کارهای مالی:**

```
1. AI_EXECUTION_CONTRACT.md (10s)
2. CRITICAL-FINANCIAL-MODULE-CONTRACT.md (5m)
3. AI_PREFLIGHT_MASTER_V3.md → STEP 2 (10m)
4. مشورت مدیر + حسابدار
5. تست 5 سناریو
6. Code Review Senior
7. Implementation
```

### 🐛 **برای رفع باگ:**

```
1. AI_EXECUTION_CONTRACT.md (10s)
2. CRITICAL_MODULE_SAFETY_CONTRACT.md (اگر حیاتی)
3. AI_PREFLIGHT_MASTER_V3.md → STEP 3 (بستگی به پیچیدگی)
4. 05-Debugging-Specialist-Contract.md (6 مرحله)
5. تحلیل ریشه‌ای (5 Whys)
6. رفع اتمیک
7. تست و گزارش
```

### 🔴 **برای تغییرات حیاتی:**

```
1. CRITICAL_MODULE_SAFETY_CONTRACT.md (20s)
2. STOP & THINK
3. ASK USER (سوالات الزامی)
4. ANALYZE IMPACT
5. PROPOSE SOLUTION
6. منتظر تأیید کاربر
7. IMPLEMENT (بعد از تأیید)
8. VERIFY
```

---

## 5️⃣ نکات مهم و Best Practices

### ✅ **قبل از هر کاری:**

1. **قراردادها را بخوان** - حداقل AI_EXECUTION_CONTRACT.md
2. **Knowledge-Base را بررسی کن** - قبل از ایجاد Helper جدید
3. **ماژول‌های موجود را چک کن** - قبل از ایجاد ماژول جدید
4. **Helper های موجود را استفاده کن** - تکرار ننویس

### ✅ **در حین کار:**

1. **قوانین را رعایت کن** - 15 ممنوعیت
2. **Standards را دنبال کن** - رنگ، فونت، تاریخ
3. **Log بنویس** - Serilog با Mask PII
4. **Error Handling** - try-catch + ServiceResult

### ✅ **بعد از کار:**

1. **Checklist نهایی** - قبل از Commit
2. **Linter Errors** - برطرف کن
3. **Test** - Manual Testing
4. **Document** - Code Comments + Summary Report

---

## 6️⃣ آمار و خلاصه

### 📊 **آمار فایل‌ها:**

| دسته‌بندی | تعداد | وضعیت |
|---------|------|------|
| **قراردادهای اصلی** | 7+ | ✅ فعال |
| **قراردادهای ایمنی** | 2 | ✅ فعال |
| **Knowledge-Base/Master** | 15+ | ✅ فعال |
| **Knowledge-Base/CURSOR** | 10+ | ✅ فعال |
| **Knowledge-Base/REVIEWS** | 5+ | ✅ فعال |
| **Knowledge-Base/CHECKLISTS** | 5+ | ✅ فعال |
| **Knowledge-Base/RELEASE** | 3+ | ✅ فعال |
| **گزارش‌های تاریخی** | 10+ | ✅ فعال |
| **جمع کل** | **129+** | ✅ فعال |

### 📊 **آمار Helpers (بر اساس Knowledge-Base):**

| دسته‌بندی | تعداد |
|---------|------|
| **Helpers** | 50+ |
| **Extensions** | 6 |
| **Services** | 150+ |
| **CSS Files** | 50+ |
| **JavaScript Files** | 18 |
| **Partial Views** | 15+ |

---

## 7️⃣ فایل‌های کلیدی برای Quick Reference

### 🔴 **الزامی (باید همیشه در دسترس باشد):**

1. `Contracts/AI_EXECUTION_CONTRACT.md` - قبل از هر پاسخ
2. `Contracts/AI_PREFLIGHT_QUICK_V3.md` - برای کارهای معمولی
3. `Contracts/CRITICAL_MODULE_SAFETY_CONTRACT.md` - برای تغییرات حیاتی

### 🟡 **مهم (برای کارهای خاص):**

4. `Contracts/AI_PREFLIGHT_MASTER_V3.md` - برای کارهای پیچیده
5. `Contracts/Knowledge-Base/AI/Master/03-Development-Contract-Quick-Guide.md` - قبل از کد زدن
6. `Contracts/Knowledge-Base/AI/PreFlight/05-Debugging-Specialist-Contract.md` - برای رفع باگ

### 🟢 **مرجع (در صورت نیاز):**

7. `Contracts/Knowledge-Base/AI/Master/01-Helpers-DateTime.md` - Helpers تاریخ
8. `Contracts/Knowledge-Base/AI/Master/02-Helpers-Validation.md` - Helpers Validation
9. `Contracts/Knowledge-Base/AI/Master/HelperExtensionsGuide.md` - Extensions
10. `Contracts/Knowledge-Base/AI/PreFlight/ClinicApp_Knowledge_Base.md` - پایگاه دانش

---

## 8️⃣ نتیجه‌گیری

فولدر `Contracts` یک **سیستم جامع و حرفه‌ای** برای مدیریت قراردادها و استانداردهای پروژه است که:

✅ **سازمان‌یافته** - ساختار سلسله‌مراتبی و منطقی  
✅ **جامع** - پوشش کامل تمام جنبه‌های توسعه  
✅ **به‌روز** - مستندات فعال و به‌روز  
✅ **قابل استفاده** - مثال‌های عملی و راهنماهای گام‌به‌گام  
✅ **امن** - حفاظت از ماژول‌های حیاتی و مالی  

**نکته نهایی:**
> این قراردادها برای **حفاظت از کیفیت، امنیت و یکپارچگی** پروژه طراحی شده‌اند.  
> **همیشه قبل از هر کاری، قراردادهای مربوطه را مطالعه کنید.**

---

## 📌 **Quick Start Guide**

### برای شروع سریع:

1. **قبل از هر پاسخ:** `Contracts/AI_EXECUTION_CONTRACT.md`
2. **برای کار معمولی:** `Contracts/AI_PREFLIGHT_QUICK_V3.md`
3. **برای کار پیچیده:** `Contracts/AI_PREFLIGHT_MASTER_V3.md`
4. **برای تغییر حیاتی:** `Contracts/CRITICAL_MODULE_SAFETY_CONTRACT.md`
5. **برای کار مالی:** `Contracts/CRITICAL-FINANCIAL-MODULE-CONTRACT.md` (در صورت وجود)

---

**📅 آخرین به‌روزرسانی:** 2026-01-06  
**✍️ نگهدارنده:** Development Team  
**📧 پشتیبانی:** مراجعه به Knowledge-Base

