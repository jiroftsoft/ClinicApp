# 🎯 پرامپت جامع بررسی ماژول رزرو نوبت و پرداخت آنلاین

**تاریخ:** ۱۴۰۳/۱۰/۰۹  
**ورژن:** 1.0  
**ماژول:** Appointment Booking + Online Payment (ZarinPal Integration)

---

## 📋 مقدمه: نقش و

 مسئولیت شما

شما به عنوان **متخصص ارشد بررسی سیستم‌های مالی و پزشکی** موظف به انجام یک **Comprehensive Code Review** والی بر روی ماژول **"رزرو نوبت و پرداخت آنلاین"** هستید. این ماژول شامل **ZarinPal Payment Gateway Integration** و **Online Appointment Booking System** است.

### ⚠️ اهمیت حیاتی:
این ماژول دارای **جنبه‌های مالی Critical** است. طبق `CRITICAL-FINANCIAL-MODULE-CONTRACT.md`:
> **کوچکترین اشتباه در ماژول‌های مالی = مسئولیت قانونی برای تیم برنامه‌نویسی**

---

## 🎭 نقش‌های شما (7 نقش همزمان)

برای این بررسی، شما باید **همزمان** 7 نقش زیر را ایفا کنید:

| # | نقش | مسئولیت کلیدی |
|---|-----|----------------|
| 1️⃣ | **معمار نرم‌افزار ارشد** | بررسی معماری، Design Patterns، Scalability، Clean Architecture |
| 2️⃣ | **کد ریویوئر خبره** | Code Quality، SOLID، Clean Code، Performance |
| 3️⃣ | **متخصص ASP.NET MVC** | MVC Pattern، Strongly-Typed، Routing، Model Binding |
| 4️⃣ | **متخصص امنیت** | OWASP Top 10، SQL Injection، XSS، Authorization |
| 5️⃣ | **متخصص سیستم‌های پزشکی** | HIPAA Compliance، Data Privacy، Audit Trail |
| 6️⃣ | **متخصص تجربه کاربری** | User Flow، Error Handling، Notifications، Performance |
| 7️⃣ | **متخصص پایگاه داده** | Entity Design، Query Optimization، Transaction Management |

---

## 📚 مرحله 0: مطالعه الزامی قبل از بررسی

### 🚨 قراردادهای Critical (الزامی):

1. **`Docs/Knowledge-Base/CRITICAL-FINANCIAL-MODULE-CONTRACT.md`** 💰🚨  
   > **10 قانون طلایی ماژول‌های مالی**
   - Logging الزامی برای تمام تراکنش‌ها
   - Transaction Management برای عملیات مالی
   - Idempotency برای پرداخت‌ها
   - **Soft Delete فقط** (نه Hard Delete)
   - Test Coverage: 95% Minimum
   - `decimal` برای مبالغ (نه float/double)

2. **`Docs/Knowledge-Base/AI_ASSISTANT_MASTER_CONTRACT.md`** 🎯  
   > خلاصه کامل تمام نقش‌ها، قراردادها و استانداردها

3. **`Docs/DEVELOPMENT_CONTRACT.md`** ⚡  
   > استانداردهای UI/UX، Strongly-Typed، Bulletproof Coding، SRP

4. **`Docs/TODO_TEMPLATE.md`** 📋  
   > 13 Phase پیاده‌سازی استاندارد

5. **`Docs/Knowledge-Base/03-Development-Contract-Quick-Guide.md`**  
   > راهنمای سریع قرارداد توسعه

---

### 📖 مستندات فنی (مطالعه توصیه‌شده):

- `Docs/Knowledge-Base/01-Helpers-DateTime.md` - Helper های تاریخ و زمان
- `Docs/Knowledge-Base/02-Helpers-Validation.md` - Helper های اعتبارسنجی
- `Docs/Knowledge-Base/HelperExtensionsGuide.md` - راهنمای Extensions
- `Docs/Knowledge-Base/08-MVC-Routing-Best-Practices.md` - بهترین روش‌های Routing

---

## 🔍 مرحله 1: شناسایی و کشف (Discovery Phase)

### 1.1 شناسایی ماژول رزرو نوبت (Appointment Module):

**فایل‌های کلیدی:**
```
Services/Appointment/
├── AppointmentBookingService.cs
├── AppointmentNotificationService.cs
├── AppointmentPricingService.cs
└── AppointmentValidationService.cs

Models/Entities/
├── Appointment.cs
└── AppointmentSlot.cs

ViewModels/Appointment/
├── AppointmentBookingViewModel.cs
└── AppointmentListViewModel.cs

Controllers/
└── Areas/Patient/Controllers/AppointmentBookingController.cs
```

**بررسی‌های لازم:**
- [ ] Entity Design (Fields، Properties، Relationships)
- [ ] Repository Pattern Implementation
- [ ] Service Layer Architecture
- [ ] Business Logic Validation
- [ ] Error Handling & Logging
- [ ] Integration با سیستم پذیرش

---

### 1.2 شناسایی ماژول پرداخت آنلاین (Payment Module):

**فایل‌های کلیدی:**
```
Services/Payment/
├── PaymentService.cs
├── Web/WebPaymentService.cs
└── Gateway/
    ├── PaymentGatewayService.cs
    └── Drivers/ZarinPalDriver.cs

Models/Entities/Payment/
├── OnlinePayment.cs
└── PaymentGateway.cs

ViewModels/Payment/Gateway/
├── PaymentGatewayCreateViewModel.cs
├── OnlinePaymentCreateViewModel.cs
└── PaymentCallbackViewModel.cs

Controllers/Payment/Gateway/
└── PaymentGatewayController.cs
```

**بررسی‌های کلیدی:**
- [ ] ✅ **WebPaymentService فعال است؟** (آیا کامل commented out نیست؟)
- [ ] ✅ **ZarinPalDriver کامل پیاده‌سازی شده؟**
- [ ] ✅ **IdempotencyService موجود و فعال است؟**
- [ ] ✅ **Transaction Management درست است؟**
- [ ] ❌ **Test Coverage چقدر است؟** (باید 95%+ باشد)

---

### 1.3 شناسایی فایل‌های مرتبط:

```
Interfaces/
├── Payment/Gateway/IPaymentGatewayService.cs
├── Payment/Web/IWebPaymentService.cs
└── Appointment/IAppointmentBookingService.cs

Repositories/
├── Payment/OnlinePaymentRepository.cs
├── Payment/PaymentGatewayRepository.cs
└── Appointment/AppointmentRepository.cs

Helpers/
├── NotificationHelper.cs
├── PersianDateHelper.cs
└── ValidationHelper.cs
```

---

## ⚙️ مرحله 2: بررسی معماری کلی (Architecture Review)

### 2.1 بررسی Layered Architecture:

```
درست یا غلط؟

Controller → Service → Repository → Entity

✅ Controller فقط Routing و Orchestration
✅ Service فقط Business Logic
✅ Repository فقط Data Access
✅ هیچ Business Logic در Controller نیست
✅ هیچ کوئری EF در Service نیست
```

### 2.2 بررسی Design Patterns:

- [ ] **Repository Pattern** - استفاده صحیح؟
- [ ] **Service Layer Pattern** - جدا از Controller؟
- [ ] **ViewModel Pattern** - Strongly-Typed؟
- [ ] **Factory Pattern** - برای Entity → ViewModel؟
- [ ] **Strategy Pattern** - برای Payment Gateways؟

### 2.3 بررسی SOLID Principles:

| اصل | بررسی | پاس/فیل |
|-----|-------|---------|
| **S**RP | هر کلاس فقط یک مسئولیت دارد؟ | [ ] |
| **O**CP | Open for extension, Closed for modification? | [ ] |
| **L**SP | Subclasses قابل جایگزینی با Base classes? | [ ] |
| **I**SP | Interfaces تخصصی (نه Fat Interface)? | [ ] |
| **D**IP | Dependency روی Interfaces (نه Concrete classes)? | [ ] |

---

## 💰 مرحله 3: بررسی ملزومات مالی (Financial Contract Compliance)

### 3.1 چک‌لیست 10 قانون طلایی:

#### 1. ✅ **Logging کامل:**
```csharp
// ❌ اشتباه:
_context.PaymentTransactions.Add(payment);
await _context.SaveChangesAsync();

// ✅ درست:
_logger.Information("💰 PAYMENT: شروع ثبت - Amount: {Amount}, Gateway: {Gateway}", 
    payment.Amount, payment.PaymentGatewayId);
await _context.SaveChangesAsync();
_logger.Information("✅ PAYMENT: ثبت موفق - PaymentId: {Id}", payment.OnlinePaymentId);
```

**بررسی:**
- [ ] تمام PaymentService methods دارای Logging
- [ ] تمام WebPaymentService methods دارای Logging
- [ ] تمام ZarinPalDriver methods دارای Logging
- [ ] تمام Create/Update/Delete دارای Log قبل و بعد

---

#### 2. ✅ **Transaction Management:**
```csharp
// ✅ الزامی برای:
// - ایجاد OnlinePayment + Update CashSession
// - ProcessPaymentCallback + Create PaymentTransaction
// - Update Reception Status + Create Payment

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

**بررسی:**
- [ ] `WebPaymentService.ProcessWebPaymentAsync` دارای Transaction
- [ ] `PaymentService.ProcessOnlinePaymentAsync` دارای Transaction
- [ ] `PaymentService.CompleteOnlinePaymentAsync` دارای Transaction
- [ ] تمام متدهایی که چند Entity را Update می‌کنند

---

#### 3. ✅ **Verification بعد از Save:**
```csharp
await _context.SaveChangesAsync();

// ✅ Verify
var saved = await _context.OnlinePayments
    .FirstOrDefaultAsync(p => p.IdempotencyKey == key);
    
if (saved == null)
{
    _logger.Error("❌ VERIFY: OnlinePayment ذخیره نشد!");
    throw new Exception("Payment was not saved!");
}
```

**بررسی:**
- [ ] تمام ایجادهای OnlinePayment دارای Verification
- [ ] تمام ایجادهای PaymentTransaction دارای Verification

---

#### 4. ✅ **Idempotency:**
```csharp
// ✅ چک کردن تکراری بودن
var existing = await _context.OnlinePayments
    .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);
    
if (existing != null)
{
    return existing; // Return existing
}
```

**بررسی:**
- [ ] `WebPaymentService.CreatePaymentRequestAsync` دارای Idempotency Check
- [ ] `WebPaymentService.ProcessPaymentCallbackAsync` دارای Idempotency Check
- [ ] `ZarinPalDriver.RequestPaymentAsync` دارای Idempotency Support
- [ ] OnlinePayment Entity دارای `IdempotencyKey` field

---

#### 5. ✅ **Soft Delete (NO Hard Delete):**
```csharp
// ❌ ممنوع:
_context.OnlinePayments.Remove(payment);

// ✅ درست:
payment.IsDeleted = true;
payment.DeletedAt = DateTime.Now;
payment.DeletedByUserId = currentUserId;
```

**بررسی:**
- [ ] هیچ `.Remove()` در Payment Services وجود ندارد
- [ ] `OnlinePayment` implements `ISoftDelete`
- [ ] `PaymentTransaction` implements `ISoftDelete`

---

#### 6. ✅ **Audit Trail:**
```csharp
public class OnlinePayment : ISoftDelete, ITrackable
{
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedByUserId { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
}
```

**بررسی:**
- [ ] `OnlinePayment` implements `ITrackable`
- [ ] `PaymentTransaction` implements `ITrackable`
- [ ] تمام Create Methods تنظیم `CreatedAt` و `CreatedByUserId`
- [ ] تمام Update Methods تنظیم `UpdatedAt` و `UpdatedByUserId`

---

#### 7. ✅ **Test Coverage: 95% Minimum:**
```
Unit Tests:
□ ZarinPalDriver
  □ RequestPaymentAsync - Happy Path
  □ RequestPaymentAsync - Invalid Amount
  □ VerifyPaymentAsync - Success
  □ VerifyPaymentAsync - Failed
  □ Idempotency Handling
  
□ WebPaymentService
  □ CreatePaymentRequestAsync - Success
  □ ProcessPaymentCallbackAsync - Success
  □ ProcessPaymentCallbackAsync - Duplicate
  
□ PaymentService
  □ ProcessOnlinePaymentAsync - Success
  □ CompleteOnlinePaymentAsync - Success
  □ Transaction Rollback Scenarios
```

**بررسی:**
- [ ] وجود فایل‌های `*Test.cs` برای Payment Module
- [ ] Code Coverage گزارش شده چقدر است؟
- [ ] Integration Tests موجود است؟

---

#### 8. ✅ **Decimal برای مبالغ:**
```csharp
// ✅ درست:
public decimal Amount { get; set; }
public decimal GatewayFee { get; set; }

// ❌ اشتباه - ممنوع!:
public float Amount { get; set; }
public double Price { get; set; }
```

**بررسی:**
- [ ] تمام فیلدهای مالی `decimal` هستند (نه float/double)
- [ ] `OnlinePayment.Amount` از نوع `decimal`
- [ ] `OnlinePayment.GatewayFee` از نوع `decimal`
- [ ] `PaymentTransaction.Amount` از نوع `decimal`

---

#### 9. ✅ **Documentation:**
```csharp
/// <summary>
/// پردازش درخواست پرداخت آنلاین از طریق درگاه
/// </summary>
/// <param name="request">اطلاعات درخواست پرداخت</param>
/// <returns>نتیجه عملیات شامل URL پرداخت</returns>
/// <remarks>
/// این متد:
/// 1. اعتبارسنجی درخواست
/// 2. چک Idempotency
/// 3. ایجاد OnlinePayment Entity
/// 4. فراخوانی ZarinPal API
/// 5. ذخیره و Verification
/// </remarks>
```

**بررسی:**
- [ ] تمام Public Methods دارای XML Documentation
- [ ] توضیح واضح Business Logic
- [ ] ذکر Side Effects و Exceptions

---

#### 10. ✅ **Approval Process:**
```
□ Developer: Self-review
□ Senior Developer: Code Review
□ Tech Lead: Architecture Review
□ QA: Testing
□ Manager: Business Logic Review
```

---

## 🔒 مرحله 4: بررسی امنیت (Security Review)

### 4.1 OWASP Top 10:

| # | تهدید | بررسی | پاس/فیل |
|---|--------|-------|---------|
| A1 | **Injection** | استفاده از Parameterized Queries | [ ] |
| A2 | **Broken Authentication** | [Authorize] Attributes | [ ] |
| A3 | **Sensitive Data Exposure** | Mask کردن در Logs | [ ] |
| A4 | **XML External Entities** | N/A | [ ] |
| A5 | **Broken Access Control** | Role-based Authorization | [ ] |
| A6 | **Security Misconfiguration** | HTTPS، Error Handling | [ ] |
| A7 | **Cross-Site Scripting** | Input Validation، `Html.Raw` usage | [ ] |
| A8 | **Insecure Deserialization** | JSON Deserialization Safety | [ ] |
| A9 | **Using Components with Known Vulnerabilities** | NuGet Packages به‌روز | [ ] |
| A10 | **Insufficient Logging** | Comprehensive Logging | [ ] |

---

### 4.2 بررسی‌های امنیتی خاص:

#### Input Validation:
```csharp
// ✅ Validation لازم:
if (amount <= 0 || amount > 1_000_000_000)
{
    return ServiceResult.Failed("مبلغ نامعتبر است");
}

if (string.IsNullOrWhiteSpace(callbackUrl))
{
    return ServiceResult.Failed("آدرس بازگشت الزامی است");
}
```

**بررسی:**
- [ ] Validation برای Amount (>0، حداکثر مجاز)
- [ ] Validation برای CallbackUrl (Valid URL)
- [ ] Validation برای Gateway Type
- [ ] Validation برای Patient/Reception ID

---

#### CSRF Protection:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> ProcessPayment(...)
```

**بررسی:**
- [ ] تمام POST Actions دارای `[ValidateAntiForgeryToken]`

---

#### Authorization:
```csharp
[Authorize(Roles = "Patient,Admin")]
public class AppointmentBookingController : Controller
```

**بررسی:**
- [ ] Controller ها دارای `[Authorize]`
- [ ] Roles مناسب تعریف شده
- [ ] بررسی User.Identity در Actions

---

#### Sensitive Data Masking:
```csharp
_logger.Information("💳 PAYMENT: کارت: {MaskedCard}", 
    payment.CardNumber?.Mask()); // 1234-****-****-5678
```

**بررسی:**
- [ ] شماره کارت Mask شده در Logs
- [ ] اطلاعات بیمار Mask شده در Logs
- [ ] Password ها هرگز Log نمی‌شوند

---

## 🎨 مرحله 5: بررسی UI/UX (User Experience Review)

### 5.1 Notification System:

**✅ الزامی:**
```csharp
// ✅ درست:
NotificationHelper.SetSuccess(TempData, "پرداخت با موفقیت انجام شد");
NotificationHelper.SetError(TempData, "خطا در اتصال به درگاه");

// ❌ اشتباه:
TempData["Success"] = "پرداخت موفق بود";
ViewBag.ErrorMessage = "خطا";
```

**بررسی:**
- [ ] استفاده از `NotificationHelper` (نه TempData/ViewBag مستقیم)
- [ ] SweetAlert2 برای Confirmations (نه `confirm()`)
- [ ] هیچ `alert()` JavaScript

---

### 5.2 Persian DatePicker:

**✅ الزامی:**
```razor
@{
    ViewBag.PersianDatePickerId = "appointmentDatePicker";
    ViewBag.PersianDatePickerName = "AppointmentDate";
    ViewBag.PersianDatePickerValue = Model.AppointmentDate;
}
@Html.Partial("_PersianDatePicker")
```

**Controller:**
```csharp
model.AppointmentDate = this.ParseDateFromHiddenInput("AppointmentDate", _logger);
```

**بررسی:**
- [ ] استفاده از `_PersianDatePicker` (نه `datetime-local`)
- [ ] Parse با `ParseDateFromHiddenInput` در Controller
- [ ] نمایش با `PersianDateHelper.ToPersianDate`

---

### 5.3 Strongly-Typed ViewModels:

```csharp
// ✅ درست:
@model ClinicApp.ViewModels.Appointment.AppointmentBookingViewModel

// ❌ اشتباه:
@model dynamic
@ViewBag.Appointments
```

**بررسی:**
- [ ] تمام Views دارای `@model`
- [ ] هیچ `ViewBag`/`ViewData` برای داده‌های اصلی
- [ ] استفاده از `GetViewPath()` در Admin Area (اگر Area است)

---

### 5.4 Color Scheme (محیط درمانی رسمی):

**✅ مجاز:**
```css
--medical-primary: #2c5aa0;
--medical-success: #28a745;
--medical-danger: #dc3545;
```

**❌ ممنوع:**
```css
background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); /* جیق! */
color: #ff00ff; /* بنفش جیق! */
```

**بررسی:**
- [ ] استفاده از `--medical-*` variables
- [ ] هیچ رنگ جیق و جلف (بنفش، صورتی، نارنجی تند)
- [ ] هیچ گرادینت فانتزی
- [ ] فونت Vazir یا IRANSansX

---

## 📊 مرحله 6: بررسی Performance (عملکرد)

### 6.1 N+1 Query Problem:

```csharp
// ❌ N+1 Problem:
var payments = await _context.OnlinePayments.ToListAsync();
foreach (var payment in payments)
{
    var gateway = await _context.PaymentGateways
        .FirstOrDefaultAsync(g => g.PaymentGatewayId == payment.PaymentGatewayId);
}

// ✅ حل شده با Include:
var payments = await _context.OnlinePayments
    .Include(p => p.PaymentGateway)
    .ToListAsync();
```

**بررسی:**
- [ ] استفاده از `.Include()` برای Navigation Properties
- [ ] استفاده از `.AsNoTracking()` برای ReadOnly queries
- [ ] Pagination برای لیست‌ها (نه `.ToList()` بدون limit)

---

### 6.2 Async/Await:

```csharp
// ✅ درست:
public async Task<ServiceResult> ProcessPaymentAsync(...)
{
    await _context.SaveChangesAsync();
}

// ❌ اشتباه:
public ServiceResult ProcessPayment(...)
{
    _context.SaveChanges();
}
```

**بررسی:**
- [ ] تمام Database Operations async
- [ ] تمام HTTP Calls async (ZarinPal API)
- [ ] استفاده صحیح از `await`

---

### 6.3 Caching Strategy:

```csharp
// برای داده‌هایی که کم تغییر می‌کنند:
var gateways = await CacheHelper.GetOrCreate("ActiveGateways", async () =>
{
    return await _context.PaymentGateways
        .Where(g => g.IsActive)
        .ToListAsync();
}, TimeSpan.FromMinutes(30));
```

**بررسی:**
- [ ] لیست درگاه‌های فعال Cache شده
- [ ] تنظیمات سیستم Cache شده
- [ ] Invalidation Strategy مشخص

---

## 🧪 مرحله 7: بررسی Testing (تست)

### 7.1 Test Coverage Report:

```
📊 Target: 95% Code Coverage (Financial Modules)

Current Coverage:
□ ZarinPalDriver: ___%
□ WebPaymentService: ___%
□ PaymentService: ___%
□ AppointmentBookingService: ___%
□ Overall: ___%
```

**بررسی:**
- [ ] آیا فایل‌های `*Test.cs` موجود است؟
- [ ] Coverage% چقدر است؟
- [ ] Test برای Happy Path
- [ ] Test برای Exception Scenarios
- [ ] Test برای Edge Cases

---

### 7.2 Integration Tests:

```csharp
[TestClass]
public class ZarinPalIntegrationTests
{
    [TestMethod]
    public async Task RequestPayment_ValidRequest_ReturnsAuthority()
    {
        // Arrange
        var driver = new ZarinPalDriver(...);
        var request = new PaymentRequest { Amount = 10000, ... };
        
        // Act
        var result = await driver.RequestPaymentAsync(request);
        
        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data.Authority);
    }
}
```

**بررسی:**
- [ ] Integration Test برای ZarinPal API
- [ ] Mock Testing برای Failure Scenarios
- [ ] End-to-End Test برای Payment Flow

---

## 📝 مرحله 8: فرمت گزارش نهایی

**گزارش خود را دقیقاً به این فرمت تحویل دهید:**

```markdown
# 🔍 گزارش بررسی جامع: ماژول رزرو نوبت و پرداخت آنلاین

**تاریخ بررسی:** [تاریخ]  
**نسخه:** 1.0  
**بررسی‌کننده:** AI Expert Reviewer

---

## 📊 خلاصه اجرایی (Executive Summary)

**امتیاز کلی:** __/100

| بخش | امتیاز | وضعیت |
|-----|--------|-------|
| معماری | __/10 | 🟢/🟡/🔴 |
| پیاده‌سازی | __/20 | 🟢/🟡/🔴 |
| امنیت | __/20 | 🟢/🟡/🔴 |
| ملزومات مالی | __/25 | 🟢/🟡/🔴 |
| Performance | __/10 | 🟢/🟡/🔴 |
| Testing | __/10 | 🟢/🟡/🔴 |
| Documentation | __/5 | 🟢/🟡/🔴 |

**وضعیت Production:** ✅ آماده / ⚠️ نیاز به بهبود / ❌ غیرقابل استفاده

---

## 🚨 مشکلات Critical (اولویت P0)

### 1. [عنوان مشکل]
**شدت:** 🔴 Critical  
**فایل:** `[مسیر فایل]`  
**خط:** [شماره خط]

**توضیح:**
[توضیح کامل مشکل]

**کد فعلی:**
\`\`\`csharp
[کد اشتباه]
\`\`\`

**پیشنهاد:**
\`\`\`csharp
[کد صحیح]
\`\`\`

**تاثیر:** [تاثیر روی سیستم]  
**زمان رفع:** [تخمین زمان]

---

## ⚠️ مشکلات High Priority (اولویت P1)

[مشابه فرمت بالا]

---

## 🟡 مشکلات Medium Priority (اولویت P2)

[مشابه فرمت بالا]

---

## ✅ نقاط قوت (Strengths)

1. [نقطه قوت 1]
2. [نقطه قوت 2]
...

---

## 📋 چک‌لیست 10 قانون طلایی مالی

- [ ] 1. Logging کامل
- [ ] 2. Transaction Management
- [ ] 3. Verification بعد از Save
- [ ] 4. Idempotency
- [ ] 5. Soft Delete (NO Hard Delete)
- [ ] 6. Audit Trail
- [ ] 7. Test Coverage (95%+)
- [ ] 8. Decimal برای مبالغ
- [ ] 9. Documentation
- [ ] 10. Approval Process

---

## 🎯 نقشه راه پیشنهادی (Roadmap)

### فاز 1: Critical Fixes (هفته 1-2)
- [ ] [مشکل Critical 1]
- [ ] [مشکل Critical 2]

### فاز 2: High Priority (هفته 3-4)
- [ ] [مشکل High 1]
- [ ] [مشکل High 2]

### فاز 3: Testing & Documentation (هفته 5-6)
- [ ] Unit Testing → 95% Coverage
- [ ] Integration Testing
- [ ] Documentation Update

**تخمین زمان کل:** [X] هفته  
**منابع لازم:** [تعداد] Developer + [تعداد] QA

---

## 💡 توصیه نهایی

[جمع‌بندی کلی و توصیه]

---

**تهیه‌کننده:** AI Expert Reviewer  
**تاریخ:** [تاریخ]  
**نوع بررسی:** Comprehensive Code Review (Financial Module)
```

---

## ✅ تعهد شما به عنوان AI Reviewer

```
من به عنوان AI Expert Reviewer متعهد می‌شوم:

✅ رعایت تمام 7 نقش همزمان
✅ مطالعه کامل قراردادهای Critical قبل از بررسی
✅ بررسی 100% تمام فایل‌های مرتبط
✅ شناسایی تمام مشکلات Critical، High، Medium
✅ ارائه راه‌حل‌های عملی و کاربردی
✅ اولویت‌بندی مسائل براساس تاثیر
✅ گزارش‌دهی حرفه‌ای و جامع
✅ رعایت قانون "هیچ حدس و گمان نکنید"
✅ ❌ ممنوع رفع کورکورانه!
✅ ❌ ممنوع نقض قراردادها!
```

---

**تاریخ:** ۱۴۰۳/۱۰/۰۹  
**نسخه:** 1.0  
**وضعیت:** ✅ فعال و الزامی
