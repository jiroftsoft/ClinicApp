# 🛡️ AI Preflight Master V3 - ClinicApp
**نسخه:** 3.0.0 | **تاریخ:** 2026-01-02 | **وضعیت:** 🔴 MANDATORY

---

## ⚡ **قانون طلایی:**
**این فایل باید قبل از هر پاسخ بررسی شود. در صورت تعارض → HARD STOP**

---

## 📋 **فهرست سریع (Quick TOC)**

```
STEP 0: AI Guard Check (15 ممنوعیت) ← 30 ثانیه
STEP 1: 12 دروازه امنیتی ← 2 دقیقه
STEP 2: Financial Module Check (اگر مالی) ← 5 دقیقه
STEP 3: Debugging Protocol (اگر باگ) ← بستگی به پیچیدگی
STEP 4: Standards & Contracts ← 2 دقیقه
STEP 5: Implementation ← بستگی به کار
STEP 6: Testing & Verification ← 3 دقیقه
STEP 7: Documentation & Report ← 2 دقیقه
```

---

## 🚫 **STEP 0: AI Guard Check (15 ممنوعیت)**

### الزامی برای هر پاسخ:

1. ❌ **حدس، فرض و تولید کد احتمالی**
2. ❌ **نقض قراردادها** (DEVELOPMENT_CONTRACT.md)
3. ❌ **دور زدن معماری لایه‌ای** (Controller→DB مستقیم)
4. ❌ **حذف یا ساده‌سازی ServiceResult Enhanced**
5. ❌ **بی‌توجهی به امنیت داده‌های درمانی**
6. ❌ **تولید کد بدون لاگ‌پذیری** (Serilog + Mask PII)
7. ❌ **تخطی از استاندارد تاریخ شمسی**
8. ❌ **آپلود فایل خارج از سیستم استاندارد** (IImageUploadService)
9. ❌ **تغییر Silent (بی‌صدا)**
10. ❌ **تولید کد بدون مستندسازی**
11. ❌ **پیشنهاد Library یا Pattern ناسازگار**
12. ❌ **تغییر رفتار سیستم بدون تست ذهنی**
13. ❌ **ساده‌سازی بیش از حد**
14. ❌ **تصمیم‌گیری مستقل بدون تأیید**
15. ❌ **شرط توقف فوری** (Hard Stop Rule)

### 🚨 در صورت نقض هر قانون → HARD STOP

---

## 🚪 **STEP 1: 12 دروازه امنیتی (Security Gates)**

### دروازه 1: Alignment Check (هم‌راستایی)
- [ ] با معماری ClinicApp هم‌راستا است؟
- [ ] با Clean Architecture سازگار است؟
- [ ] با Repository Pattern سازگار است؟

### دروازه 2: Contract Enforcement (اجرای قرارداد)
- [ ] `DEVELOPMENT_CONTRACT.md` رعایت شده؟
- [ ] `Bugfix-Master-Contract.md` رعایت شده؟ (در صورت باگ)
- [ ] `CRITICAL-FINANCIAL-MODULE-CONTRACT.md` رعایت شده؟ (اگر مالی)

### دروازه 3: Architecture Gate (معماری)
- [ ] ServiceResult Pattern استفاده شده؟
- [ ] Repository Pattern استفاده شده؟
- [ ] Dependency Injection استفاده شده؟
- [ ] Strongly-Typed است؟
- [ ] Factory Pattern برای ViewModels؟

### دروازه 4: Security Gate (امنیت)
- [ ] `[Authorize]` attribute دارد؟
- [ ] `[ValidateAntiForgeryToken]` در POST دارد؟
- [ ] `[NoCache]` برای صفحات حساس؟
- [ ] HTTPS الزامی است؟
- [ ] PII در Logs Mask شده؟

### دروازه 5: Standards Gate (استانداردها)
- [ ] Persian DatePicker (PersianDateHelper)؟
- [ ] فونت Vazir استفاده شده؟
- [ ] RTL Support دارد؟
- [ ] رنگ‌های استاندارد (--medical-primary)؟
- [ ] بدون Gradient؟

### دروازه 6: Data Integrity Gate (یکپارچگی داده)
- [ ] Decimal Precision: decimal(18,0) برای پول؟
- [ ] RowVersion برای Concurrency؟
- [ ] SoftDelete پیاده‌سازی شده؟
- [ ] Audit Trail (CreatedAt, UpdatedAt, DeletedAt)؟

### دروازه 7: Code Quality Gate (کیفیت کد)
- [ ] Code Duplication وجود ندارد؟
- [ ] Error Handling (try-catch) مناسب؟
- [ ] Logging (Serilog) مناسب؟
- [ ] Code Comments کافی؟

### دروازه 8: Performance Gate (عملکرد)
- [ ] N+1 Query وجود ندارد؟
- [ ] Include/Projection استفاده شده؟
- [ ] Index مناسب است؟
- [ ] Caching (در صورت نیاز، نه برای Clinical)؟

### دروازه 9: UI/UX Gate (رابط کاربری)
- [ ] طراحی رسمی و حرفه‌ای (نه جیغ)؟
- [ ] Card Components استفاده شده؟
- [ ] Responsive است؟
- [ ] Accessibility رعایت شده؟

### دروازه 10: Testing Gate (تست)
- [ ] Manual Testing انجام شده؟
- [ ] Edge Cases بررسی شده؟
- [ ] Regression Tests؟
- [ ] Rollback Strategy وجود دارد؟

### دروازه 11: Documentation Gate (مستندسازی)
- [ ] Code Comments کافی؟
- [ ] API Documentation؟
- [ ] Summary Report؟
- [ ] Changelog به‌روز؟

### دروازه 12: Backward Compatibility Gate (سازگاری)
- [ ] Breaking Changes وجود ندارد؟
- [ ] API Contracts حفظ شده؟
- [ ] Migration Path وجود دارد؟
- [ ] Rollback Plan وجود دارد؟

---

## 💰 **STEP 2: Financial Module Check (اگر مالی)**

### 🚨 اگر ماژول مالی است → این بخش الزامی است:

#### 10 قانون طلایی مالی:

1. ✅ **هیچ تغییری بدون تست کامل** (5 سناریو)
2. ✅ **هر تراکنش = حتماً Log کامل**
   ```csharp
   _logger.Information("💰 PAYMENT: شروع ثبت پرداخت - ReceptionId: {ReceptionId}, Amount: {Amount}", id, amount);
   ```
3. ✅ **Transaction Management الزامی**
   ```csharp
   using (var transaction = _context.Database.BeginTransaction()) {
       // ... code ...
       transaction.Commit();
   }
   ```
4. ✅ **Verification بعد از Save**
   ```csharp
   var saved = await _context.PaymentTransactions.FirstOrDefaultAsync(...);
   if (saved == null) throw new Exception("Payment not saved!");
   ```
5. ✅ **Idempotency برای همه پرداخت‌ها**
   ```csharp
   var existing = await _context.PaymentTransactions.FirstOrDefaultAsync(p => p.IdempotencyKey == key);
   if (existing != null) return existing;
   ```
6. ✅ **هیچ Hard-Delete در جداول مالی**
   ```csharp
   payment.IsDeleted = true;
   payment.DeletedAt = DateTime.Now;
   ```
7. ✅ **Audit Trail کامل**
   ```csharp
   public DateTime CreatedAt { get; set; }
   public string CreatedByUserId { get; set; }
   public DateTime? UpdatedAt { get; set; }
   [Timestamp] public byte[] RowVersion { get; set; }
   ```
8. ✅ **decimal(18,0) برای تمام مبالغ IRR**
9. ✅ **Concurrency Control (RowVersion)**
10. ✅ **Code Review توسط Senior قبل از Merge**

#### ممنوعیت‌های مطلق در ماژول‌های مالی:

```csharp
// ❌ 1. تغییر مبلغ بدون log
payment.Amount = newAmount; // خطرناک!

// ❌ 2. Hard Delete
_context.PaymentTransactions.Remove(payment);

// ❌ 3. بدون Transaction
_context.PaymentTransactions.Add(payment);
await _context.SaveChangesAsync(); // بدون transaction

// ❌ 4. بدون Idempotency
// هیچ چک تکراری نداریم

// ❌ 5. بدون Verification
await _context.SaveChangesAsync();
// هیچ بررسی نداریم که واقعاً ذخیره شده؟
```

#### Checklist قبل از تغییر مالی:

```
قبل از تغییر:
□ با مدیر فنی مشورت کن
□ با حسابدار مشورت کن
□ تست کامل بنویس (5 سناریو)
□ Document بنویس
□ Log کامل اضافه کن
□ Transaction management اضافه کن
□ Verification اضافه کن
□ Idempotency اضافه کن
□ Code Review دریافت کن
□ تست در Staging
□ پشتیبان Database بگیر
□ Rollback Plan داشته باش
```

---

## 🔧 **STEP 3: Debugging Protocol (اگر باگ)**

### فرآیند 6 مرحله‌ای (الزامی):

#### مرحله 1: شناسایی و دسته‌بندی
```
1. نوع خطا: Compilation / Runtime / Logic / Performance / Security
2. شدت: Critical / High / Medium / Low
3. محدوده: File / Module / Cross-Module / System-Wide
```

#### مرحله 2: تحلیل علت ریشه‌ای (5 Whys)
```
1. چرا خطا رخ داد؟
2. چرا علت #1 رخ داد؟
3. چرا علت #2 رخ داد؟
4. چرا علت #3 رخ داد؟
5. چرا علت #4 رخ داد؟

→ علت ریشه‌ای شناسایی شد
```

#### مرحله 3: بررسی وابستگی‌ها
```
- این کد از کجا فراخوانی می‌شود؟
- به چه سرویس‌هایی وابسته است؟
- کدام ماژول‌ها تحت تأثیر قرار می‌گیرند؟
```

#### مرحله 4: رفع اتمیک (Atomic Fix)
```
- کوچکترین تغییر ممکن
- Patch اتمیک (فقط فایل‌های لازم)
- NO_DELETE: حذف کد ممنوع (فقط Obsolete)
- Facade/Forwarder برای سازگاری
```

#### مرحله 5: تست و اعتبارسنجی
```
□ Build موفق
□ Manual Test موفق
□ Edge Cases تست شد
□ Regression Test انجام شد
```

#### مرحله 6: گزارش‌دهی
```
# 🐛 Bugfix Report

## 1. Executive Summary
- مشکل: [1 خط]
- ریشه: [1 خط]
- راه‌حل: [1 خط]

## 2. Evidence
- File: [path:line]
- Error: [message]

## 3. Root Cause
- [5 Whys]

## 4. Solution
- [changes]

## 5. Testing
- [tests]

## 6. Rollback
- [steps]
```

---

## 📋 **STEP 4: Standards & Contracts**

### Development Contract:

#### Strongly-Typed ViewModels:
```csharp
// ✅ GOOD
@model ClinicApp.ViewModels.Patient.DashboardViewModel

public ActionResult Index() {
    var viewModel = DashboardViewModelFactory.CreateEmpty();
    return View(viewModel);
}

// ❌ BAD
ViewBag.Data = data; // ممنوع
```

#### رنگ‌بندی استاندارد:
```css
:root {
    --medical-primary: #2c5aa0;   /* آبی درمانی */
    --medical-success: #28a745;   /* سبز */
    --medical-danger: #dc3545;    /* قرمز */
}
```
❌ ممنوع: Gradient، بنفش، صورتی، رنگ‌های جیغ

#### NotificationHelper:
**Server-Side:**
```csharp
NotificationHelper.SetSuccess(TempData, "پیام موفقیت");
NotificationHelper.SetError(TempData, "پیام خطا");
```

**Client-Side:**
```javascript
Notify.success('پیام موفقیت');
Notify.error('پیام خطا');
Notify.confirm('تأیید؟', 'عنوان', onConfirm, onCancel);
```

#### Persian DatePicker:
```csharp
// ✅ GOOD
PersianDateHelper.ToPersianDate(DateTime.Now)

// ❌ BAD
<input type="datetime-local"> // ممنوع
```

#### Factory Pattern:
```csharp
// ✅ GOOD
var viewModel = DashboardViewModelFactory.CreateEmpty();

// ❌ BAD
var viewModel = new DashboardViewModel { ... }; // inline
```

#### ServiceResult Enhanced:
```csharp
// ✅ GOOD
public async Task<ServiceResult<T>> GetDataAsync() {
    try {
        var data = await _repo.GetAsync();
        return ServiceResult<T>.Success(data);
    }
    catch (Exception ex) {
        _logger.Error(ex, "خطا");
        return ServiceResult<T>.Failed("خطا در دریافت داده");
    }
}
```

---

## 🎯 **STEP 5: Implementation Workflow**

### 1️⃣ Analyze (تحلیل):
```
- درخواست کاربر چیست؟
- کدام ماژول؟
- آیا مالی است؟ → STEP 2
- آیا باگ است؟ → STEP 3
```

### 2️⃣ Search (جستجو):
```
- آیا کد مشابه وجود دارد؟
- آیا Helper موجود است؟
- آیا Pattern استفاده شده؟
- مراجعه به: Docs/Knowledge-Base/
```

### 3️⃣ Design (طراحی):
```
- معماری: Controller → Service → Repository
- SRP رعایت می‌شود؟
- Factory Pattern برای ViewModels
- ServiceResult Enhanced
```

### 4️⃣ Implement (پیاده‌سازی):
```csharp
// ✅ Checklist:
□ Interface تعریف شد
□ Service پیاده‌سازی شد (با ServiceResult)
□ Factory ایجاد شد
□ Controller orchestration فقط
□ ViewModel Strongly-Typed
□ Error Handling (try-catch)
□ Logging (Serilog + Mask PII)
□ Validation (Server + Client)
□ Authorization ([Authorize])
□ AntiForgeryToken ([ValidateAntiForgeryToken])
□ DI Registration (UnityConfig)
```

### 5️⃣ Test (تست):
```
□ Build OK
□ Happy Path OK
□ Edge Cases OK
□ Error Handling OK
□ Responsive (Desktop/Tablet/Mobile) OK
□ Browser (Chrome/Edge/Firefox) OK
□ Console Clean (no errors)
```

### 6️⃣ Document (مستندسازی):
```
□ Code Comments (XML Documentation)
□ Summary Report
□ TODO items (اگر لازم)
□ Changelog به‌روز
```

---

## 🚨 **STEP 6: HARD STOP Conditions**

در صورت مشاهده → STOP:

1. 🚫 نقض AI No-Fly Zone (15 ممنوعیت)
2. 🚫 نقض قراردادها
3. 🚫 نقض معماری (SRP)
4. 🚫 مشکل امنیتی
5. 🚫 نقض Financial Contract (اگر مالی)
6. 🚫 Breaking Changes بدون Migration
7. 🚫 حذف کد بدون Obsolete
8. 🚫 تغییر Silent
9. 🚫 کد بدون شواهد/تست

### Format HARD STOP:
```markdown
🚨 HARD STOP - تعارض شناسایی شد

**مشکل:** [description]
**قرارداد نقض شده:** [contract name]
**راه‌حل جایگزین:** [alternative solution]

آیا تأیید می‌کنید یا راه‌حل دیگری پیشنهاد می‌دهید؟
```

---

## 📁 **STEP 7: Quick References**

### قراردادهای اصلی:
```
📄 PREFLIGHT_CHECKLIST.md (این فایل)
📄 Docs/DEVELOPMENT_CONTRACT.md
📄 Docs/Knowledge-Base/CRITICAL-FINANCIAL-MODULE-CONTRACT.md
📄 Docs/Knowledge-Base/AI/PreFlight/Bugfix-Master-Contract.md
📄 Docs/Knowledge-Base/AI/PreFlight/05-Debugging-Specialist-Contract.md
📄 Docs/Knowledge-Base/03-Development-Contract-Quick-Guide.md
```

### راهنماها:
```
📄 Docs/Knowledge-Base/README.md
📄 Docs/Knowledge-Base/14041004/NOTIFICATION_HELPER_USAGE_GUIDE.md
📄 Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md
📄 Docs/IMAGE_UPLOAD_SYSTEM_GUIDE.md
```

### Helpers:
```
📄 Docs/Knowledge-Base/01-Helpers-DateTime.md
📄 Docs/Knowledge-Base/02-Helpers-Validation.md
📄 Docs/Knowledge-Base/HelperExtensionsGuide.md (100+ متد)
```

---

## ✅ **چک‌لیست نهایی**

### قبل از Submit:

#### Code Quality:
- [ ] ✅ قراردادها رعایت شد
- [ ] ✅ معماری صحیح (SRP)
- [ ] ✅ Factory Pattern
- [ ] ✅ ServiceResult Enhanced
- [ ] ✅ Error Handling
- [ ] ✅ Logging (Serilog + Mask PII)
- [ ] ✅ Validation

#### Security:
- [ ] ✅ [Authorize]
- [ ] ✅ [ValidateAntiForgeryToken]
- [ ] ✅ [NoCache]
- [ ] ✅ PII Masked

#### Standards:
- [ ] ✅ رنگ‌های استاندارد
- [ ] ✅ فونت Vazir
- [ ] ✅ Persian Date
- [ ] ✅ NotificationHelper (Server + Client)
- [ ] ✅ بدون Gradient

#### Financial (اگر مالی):
- [ ] ✅ Transaction Management
- [ ] ✅ Idempotency
- [ ] ✅ Verification
- [ ] ✅ Audit Trail
- [ ] ✅ Log کامل
- [ ] ✅ decimal(18,0)
- [ ] ✅ RowVersion
- [ ] ✅ SoftDelete

#### Testing:
- [ ] ✅ Build OK
- [ ] ✅ Manual Test OK
- [ ] ✅ Edge Cases OK
- [ ] ✅ Responsive OK
- [ ] ✅ Console Clean

---

## 🎯 **خلاصه برای AI**

### قبل از هر پاسخ:
```
1. این Preflight را بخوان (2-5 دقیقه)
2. STEP 0: AI Guard Check (15 ممنوعیت)
3. STEP 1: 12 دروازه امنیتی
4. STEP 2: Financial Check (اگر مالی)
5. STEP 3: Debugging Protocol (اگر باگ)
6. STEP 4-7: Implementation + Test + Document
7. HARD STOP (در صورت تعارض)
```

### یادت باشه:
```
🚫 حدس ممنوع
✅ شواهد الزامی
✅ قراردادها بالاتر از همه
✅ امنیت اولویت اول
✅ مالی = حساسیت دوبرابر
✅ تست الزامی
🚨 HARD STOP در صورت تعارض
```

---

**نسخه:** 3.0.0  
**تاریخ:** 2026-01-02  
**وضعیت:** 🔴 MANDATORY - قبل از هر پاسخ اجباری است

**📌 این فایل شامل تمام 4 قرارداد حیاتی است:**
1. ✅ PREFLIGHT_CHECKLIST.md (15 ممنوعیت + 12 دروازه)
2. ✅ CRITICAL-FINANCIAL-MODULE-CONTRACT.md (10 قانون طلایی)
3. ✅ 03-Development-Contract-Quick-Guide.md (استانداردها)
4. ✅ 05-Debugging-Specialist-Contract.md (فرآیند دیباگ)

**🎉 با این Preflight، کیفیت کدت 10x بهتر + امنیت مالی 100% تضمین شده!**

