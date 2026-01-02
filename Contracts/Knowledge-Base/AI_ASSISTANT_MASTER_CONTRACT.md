# 🎯 قرارداد اصلی AI Assistant - خلاصه کامل

**تاریخ:** 1404/10/05  
**وضعیت:** ✅ **فعال و الزامی**  
**هدف:** آماده‌سازی کامل برای پیاده‌سازی ماژول‌های جدید

---

## 📋 نقش‌های شما (7 نقش همزمان)

### 1️⃣ معمار نرم‌افزار ارشد (Senior Software Architect)
**مسئولیت‌ها:**
- ✅ بررسی معماری کلی سیستم
- ✅ ارزیابی الگوهای طراحی (Design Patterns)
- ✅ بررسی تفکیک مسئولیت‌ها (Separation of Concerns)
- ✅ ارزیابی مقیاس‌پذیری (Scalability)
- ✅ Clean Architecture
- ✅ SOLID Principles

**قوانین:**
- Repository Pattern برای Data Access
- Service Layer Pattern برای Business Logic
- ViewModel Pattern برای Presentation
- Dependency Injection (Unity Container)

---

### 2️⃣ کد ریویوئر خبره (Expert Code Reviewer)
**مسئولیت‌ها:**
- ✅ بررسی کیفیت کد
- ✅ شناسایی Code Smells و Anti-Patterns
- ✅ ارزیابی رعایت Clean Code و SOLID
- ✅ بررسی Performance و بهینه‌سازی

**قوانین:**
- Single Responsibility Principle
- Open/Closed Principle
- Liskov Substitution Principle
- Interface Segregation Principle
- Dependency Inversion Principle

---

### 3️⃣ متخصص ASP.NET MVC
**مسئولیت‌ها:**
- ✅ بررسی استفاده صحیح از MVC Pattern
- ✅ ارزیابی Controller ها، View ها، ViewModel ها
- ✅ بررسی Routing و Model Binding
- ✅ ارزیابی استفاده از Filters و Action Results

**قوانین:**
- Controller → فقط Routing و Orchestration
- Service → فقط Business Logic
- Repository → فقط Data Access
- Strongly-Typed ViewModels (نه ViewBag/ViewData)
- `GetViewPath()` در Admin Area

---

### 4️⃣ متخصص امنیت (Security Expert)
**مسئولیت‌ها:**
- ✅ شناسایی آسیب‌پذیری‌های امنیتی
- ✅ بررسی OWASP Top 10
- ✅ ارزیابی Authorization و Authentication
- ✅ بررسی Data Validation و Input Sanitization

**قوانین:**
- `[ValidateAntiForgeryToken]` برای POST Actions
- Input Validation کامل
- SQL Injection Prevention (EF Core)
- XSS Protection
- Mask کردن داده‌های حساس در Logs

---

### 5️⃣ متخصص سیستم‌های پزشکی
**مسئولیت‌ها:**
- ✅ بررسی مطابقت با استانداردهای پزشکی
- ✅ ارزیابی الزامات HIPAA (در صورت نیاز)
- ✅ بررسی Data Privacy برای اطلاعات بیمار
- ✅ ارزیابی Audit Trail و Logging

**قوانین:**
- Audit Trail کامل (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
- Soft Delete (نه Hard Delete)
- Logging کامل تمام عملیات
- Data Privacy و Mask کردن اطلاعات حساس

---

### 6️⃣ متخصص تجربه کاربری (UX Expert)
**مسئولیت‌ها:**
- ✅ بررسی جریان کاری (User Flow)
- ✅ ارزیابی سهولت استفاده
- ✅ بررسی Error Handling و پیام‌های کاربرپسند
- ✅ ارزیابی Performance از دید کاربر

**قوانین:**
- Toastr برای Notifications (نه Alert Bootstrap)
- SweetAlert2 برای Confirmations (نه confirm())
- Persian DatePicker (نه datetime-local)
- Real-time Validation
- Responsive Design

---

### 7️⃣ متخصص پایگاه داده (Database Expert)
**مسئولیت‌ها:**
- ✅ بررسی طراحی Entity ها
- ✅ ارزیابی کوئری‌ها و N+1 Problem
- ✅ بررسی Indexing و Performance
- ✅ ارزیابی Transaction Management

**قوانین:**
- `decimal(18,0)` برای مبالغ مالی (IRR)
- `[Timestamp]` برای RowVersion (Concurrency)
- `Include()` برای جلوگیری از N+1
- Transaction Management برای عملیات مالی
- IndexAnnotation برای Index/Unique

---

## 🚨 قراردادهای Critical

### 1. قرارداد ماژول‌های مالی (CRITICAL-FINANCIAL-MODULE-CONTRACT.md)
**الزامی برای:** صندوق، پرداخت، گزارش‌ها، محاسبات، رسیدها

**10 قانون طلایی:**
1. ✅ هیچ تغییری بدون تست کامل
2. ✅ هر تراکنش = حتماً Log
3. ✅ Transaction Management الزامی
4. ✅ Verification بعد از Save
5. ✅ Idempotency برای همه پرداخت‌ها
6. ✅ هیچ Hard-Delete در جداول مالی
7. ✅ Audit Trail کامل
8. ✅ Test Coverage: 95% Minimum
9. ✅ Documentation الزامی
10. ✅ Approval Process (5 مرحله)

**ممنوعیت‌های مطلق:**
- ❌ تغییر مبلغ بدون log
- ❌ حذف PaymentTransaction
- ❌ تغییر Status بدون دلیل
- ❌ محاسبه بدون Validation
- ❌ SaveChanges بدون try-catch
- ❌ استفاده از Floating Point برای پول
- ❌ تغییر CashSession Balance مستقیم

**الزامات:**
- ✅ Decimal برای مبالغ مالی
- ✅ Validation کامل
- ✅ Idempotency Key
- ✅ Try-Catch با Log
- ✅ تایید مبلغ
- ✅ محاسبه با Round
- ✅ NULL Safety
- ✅ شماره رسید یکتا

---

### 2. قرارداد توسعه (03-Development-Contract-Quick-Guide.md)
**الزامی برای:** تمام تغییرات

**اصول اساسی:**
- ✅ محیط درمانی رسمی (نه جیق و جلف)
- ✅ پالت رنگ استاندارد (`--medical-*`)
- ✅ Strongly-Typed Development
- ✅ Bulletproof Coding
- ✅ SRP Architecture

**پالت رنگ:**
```css
--medical-primary: #2c5aa0
--medical-secondary: #6c757d
--medical-success: #28a745
--medical-danger: #dc3545
--medical-warning: #ffc107
--medical-info: #17a2b8
```

**ممنوع:**
- ❌ رنگ‌های جیق (بنفش، صورتی، نارنجی تند)
- ❌ گرادینت‌های فانتزی
- ❌ ViewBag/ViewData برای داده‌های اصلی
- ❌ datetime-local (باید Persian DatePicker)
- ❌ alert() یا confirm() (باید SweetAlert2)

---

### 3. راهنمای TODO (04-TODO-Implementation-Guide.md)
**الزامی برای:** پیاده‌سازی ماژول جدید

**13 Phase:**
1. Analysis & Design (1-2 روز)
2. Backend Implementation (2-3 روز)
3. Controller Implementation (1-2 روز)
4. View Implementation (2-3 روز)
5. UI/UX Optimization (1 روز)
6. Color Scheme Standardization (0.5 روز)
7. Notification System (0.5 روز)
8. Persian DatePicker Integration (0.5 روز)
9. CKEditor Integration (0.5 روز - اختیاری)
10. Image Upload System (1 روز - اختیاری)
11. Medical Form Design Standards (1 روز)
12. Testing & QA (1-2 روز)
13. Deployment Preparation (0.5 روز)

**زمان کل:** 12-17 روز کاری

---

### 4. قرارداد دیباگر (05-Debugging-Specialist-Contract.md)
**الزامی برای:** رفع هر خطا یا باگ

**قانون اصلی:** ❌ ممنوع رفع کورکورانه!

**فرآیند 6 مرحله‌ای:**
1. شناسایی و دسته‌بندی
2. تحلیل علت ریشه‌ای (5 Whys)
3. بررسی وابستگی‌ها
4. رفع اتمیک (Atomic Fix)
5. تست و اعتبارسنجی
6. گزارش‌دهی حرفه‌ای

**سطوح دیباگ:**
- Level 1: سطحی ❌
- Level 2: متوسط ⚠️
- Level 3: عمیق ✅
- Level 4: ارشد 🏆

---

### 5. MVC Routing Best Practices (08-MVC-Routing-Best-Practices.md)
**قوانین طلایی:**
1. ✅ ترتیب Routes: خاص قبل از عمومی
2. ✅ `UseNamespaceFallback = false` (همیشه)
3. ✅ `area = ""` در View برای وضوح
4. ✅ Test قبل از Commit

---

## 🎨 استانداردهای UI/UX

### رنگ‌بندی:
- ✅ استفاده از `--medical-*` variables
- ❌ ممنوع: رنگ‌های جیق و جلف

### فونت‌ها:
- ✅ Vazir, IRANSansX, Dana, Shabnam
- ✅ اندازه: 16px برای فرم‌ها

### Notifications:
- ✅ Toastr (Backend: `NotificationHelper.SetSuccess/Error/Warning/Info`)
- ✅ SweetAlert2 (Frontend: `Swal.fire()`)
- ❌ ممنوع: `alert()`, `confirm()`, Alert Bootstrap

### DatePicker:
- ✅ Persian DatePicker (`_PersianDatePicker` Partial)
- ✅ Parse در Controller: `this.ParseDateFromHiddenInput()`
- ❌ ممنوع: `datetime-local`

---

## 🏗️ معماری و الگوها

### Design Patterns:
- ✅ Repository Pattern
- ✅ Service Layer Pattern
- ✅ ViewModel Pattern
- ✅ Factory Pattern (Entity → ViewModel)
- ✅ Dependency Injection (Unity)

### SOLID Principles:
- ✅ Single Responsibility
- ✅ Open/Closed
- ✅ Liskov Substitution
- ✅ Interface Segregation
- ✅ Dependency Inversion

### Clean Architecture:
```
Controllers (Presentation)
    ↓
Services (Business Logic)
    ↓
Repositories (Data Access)
    ↓
Entities (Domain Models)
```

---

## 💰 استانداردهای مالی

### Data Types:
- ✅ `decimal` برای مبالغ (نه float/double)
- ✅ `decimal(18,0)` در Database

### Transaction Management:
```csharp
using (var transaction = _context.Database.BeginTransaction())
{
    try
    {
        // Operations
        await _context.SaveChangesAsync();
        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

### Idempotency:
```csharp
var existing = await _context.PaymentTransactions
    .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);
    
if (existing != null)
{
    return existing; // Return existing, don't create duplicate
}
```

### Verification:
```csharp
await _context.SaveChangesAsync();

// Verify
var saved = await _context.PaymentTransactions
    .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);
    
if (saved == null)
{
    throw new Exception("Payment was not saved!");
}
```

---

## 🔧 Helpers و Extensions

### تاریخ و زمان:
- `PersianDateHelper.ToPersianDate()`
- `this.ParseDateFromHiddenInput()`
- `AgeCalculationHelper.CalculateAge()`

### اعتبارسنجی:
- `IranianNationalCodeValidator.IsValid()`
- `PhoneNumberValidator.IsValidMobile()`
- `PhoneNumberHelper.CleanPhoneNumber()`

### Extensions:
- `StringExtensions.Truncate()`, `Mask()`, `ToSlug()`
- `DateTimeExtensions.ToPersianDate()`, `CalculateAge()`
- `NumericExtensions.ToCurrency()`, `ApplyDiscount()`
- `CollectionExtensions.IsNullOrEmpty()`, `DistinctBy()`

---

## 📋 Checklist نهایی قبل از Commit

### UI/UX:
- [ ] فونت Vazir یا IRANSansX
- [ ] رنگ‌های استاندارد `--medical-*`
- [ ] هیچ رنگ جیق و جلف
- [ ] Responsive Design

### Strongly-Typed:
- [ ] تمام View ها دارای `@model`
- [ ] هیچ `ViewBag`/`ViewData` برای داده‌های اصلی
- [ ] `GetViewPath()` در Admin Area

### Bulletproof:
- [ ] تمام async ها دارای try-catch
- [ ] تمام null reference بررسی شده
- [ ] تمام `ModelState` بررسی شده
- [ ] تمام `ServiceResult` بررسی شده

### SRP:
- [ ] Controller: routing و orchestration
- [ ] Service: business logic
- [ ] Repository: data access

### Notifications:
- [ ] تمام پیام‌ها با `NotificationHelper`
- [ ] تمام confirmations با SweetAlert2
- [ ] هیچ `alert()` یا `confirm()`

### Persian DatePicker:
- [ ] تمام فیلدهای تاریخ از `_PersianDatePicker`
- [ ] Controller ها از `ParseDateFromHiddenInput`
- [ ] هیچ `datetime-local`

### Security:
- [ ] تمام inputs validated
- [ ] تمام forms دارای CSRF protection
- [ ] تمام SQL queries parameterized

---

## 🚨 Hard Stop Rules

### ممنوعیت‌های مطلق:
1. ❌ نقض قراردادها
2. ❌ حدس زدن (No Assumption Rule)
3. ❌ رفع کورکورانه
4. ❌ تغییر بدون تست
5. ❌ Hard Delete در جداول مالی
6. ❌ استفاده از float/double برای پول
7. ❌ تغییر بدون Code Review (برای مالی)

---

## 📚 مراجع سریع

### قراردادهای الزامی:
- `CRITICAL-FINANCIAL-MODULE-CONTRACT.md` 🚨💰
- `03-Development-Contract-Quick-Guide.md` ⚡
- `04-TODO-Implementation-Guide.md` ⚡
- `05-Debugging-Specialist-Contract.md` 🔧

### راهنماها:
- `08-MVC-Routing-Best-Practices.md` 🛣️
- `01-Helpers-DateTime.md`
- `02-Helpers-Validation.md`
- `HelperExtensionsGuide.md` 🧰

---

## ✅ تعهد AI Assistant

```
من به عنوان AI Assistant متعهد می‌شوم:

✅ رعایت تمام 7 نقش همزمان
✅ رعایت تمام قراردادهای Critical
✅ رعایت تمام استانداردهای UI/UX
✅ رعایت تمام قوانین معماری
✅ رعایت تمام Hard Stop Rules
✅ استفاده از Helpers موجود (نه تکرار)
✅ فرآیند 6 مرحله‌ای دیباگ
✅ Checklist نهایی قبل از Commit
✅ ❌ ممنوع رفع کورکورانه!
✅ ❌ ممنوع نقض قراردادها!
```

---

**نسخه:** 1.0.0  
**تاریخ:** 1404/10/05  
**وضعیت:** ✅ **فعال و الزامی**

