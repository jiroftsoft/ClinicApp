# 🛡️ AI Preflight Master - ClinicApp
**نسخه:** 2.0.0 | **تاریخ:** 2026-01-02 | **وضعیت:** 🔴 MANDATORY

---

## ⚡ **استفاده سریع:**
قبل از هر پاسخ، این 5 مرحله را چک کن:

```
1. ❌ AI No-Fly Zone بررسی شد؟
2. ✅ قراردادها رعایت می‌شود؟
3. 🏗️ معماری درست است؟
4. 🔒 امنیت چک شد؟
5. 📋 استانداردها OK است؟
```

---

## 🚫 **AI No-Fly Zone (15 ممنوعیت)**

### NEVER DO:
1. ❌ حدس و فرض زدن بدون شواهد
2. ❌ نقض قراردادها (DEVELOPMENT_CONTRACT.md)
3. ❌ دور زدن معماری (Controller → DB مستقیم)
4. ❌ حذف ServiceResult Enhanced
5. ❌ تغییر Silent بدون اطلاع
6. ❌ کد بدون Logging
7. ❌ نقض Persian Date Standards
8. ❌ آپلود فایل خارج از IImageUploadService
9. ❌ کد بدون Documentation
10. ❌ پیشنهاد Library ناسازگار
11. ❌ تغییر بدون Test Plan
12. ❌ ساده‌سازی بیش از حد
13. ❌ تصمیم‌گیری مستقل بدون تأیید
14. ❌ Breaking Changes بدون Migration
15. ❌ حذف کد بدون Obsolete/Legacy mark

### 🚨 در صورت نقض → HARD STOP

---

## ✅ **قراردادهای اصلی (Must Read)**

### 1️⃣ Development Contract:
```
📁 Docs/DEVELOPMENT_CONTRACT.md
📁 Docs/Knowledge-Base/03-Development-Contract-Quick-Guide.md
```

**چک‌لیست:**
- [ ] Strongly-Typed ViewModels (نه ViewBag/ViewData)
- [ ] رنگ‌بندی استاندارد (--medical-primary: #2c5aa0)
- [ ] بدون Gradient
- [ ] Factory Pattern برای ViewModels
- [ ] ServiceResult Enhanced
- [ ] Error Handling (try-catch)
- [ ] Logging (Serilog)
- [ ] NotificationHelper (Server + Client)
- [ ] Persian DatePicker
- [ ] Authorization & Security

### 2️⃣ Bugfix Master Contract:
```
📁 Docs/Knowledge-Base/AI/PreFlight/Bugfix-Master-Contract.md
```

**فرآیند 6 مرحله‌ای:**
1. کشف شواهد (Evidence Discovery)
2. تحلیل ریشه‌ای (Root Cause Analysis)
3. گزینه‌های رفع (Options A/B/C)
4. Patch اتمیک (Minimal Changes)
5. تأیید دستی (Manual Sanity)
6. گزارش خروجی (Report)

**قواعد:**
- ❌ NO_DELETE: حذف کد ممنوع (فقط Obsolete)
- ✅ Evidence-Based: هر ادعا = مدرک
- ✅ Atomic: تغییرات کوچک و متمرکز

### 3️⃣ Flow Discipline Contract:
```
📁 Docs/Knowledge-Base/AI/CURSOR/CLINICAPP_CURSOR_FLOW_DISCIPLINE_CONTRACT.md
```

**الزامات:**
- [ ] Primary Flow شناسایی شد
- [ ] تمام Branch ها پیش‌بینی شد
- [ ] Return Destination مشخص است
- [ ] Safe Failure تعریف شد
- [ ] SRP رعایت شد
- [ ] UI/UX Healthcare Standards
- [ ] Security Baseline چک شد

---

## 🏗️ **معماری (Architecture)**

### SRP (Single Responsibility):
```
Controller → فقط Orchestration
Service → Business Logic
Repository → Data Access
Factory → Entity → ViewModel
Helper → Utility Functions
```

### Patterns الزامی:
- ✅ ServiceResult Enhanced
- ✅ Factory Method (Entity→ViewModel)
- ✅ Repository Pattern
- ✅ Dependency Injection (Unity)
- ✅ Strongly-Typed ViewModels

### ممنوع:
- ❌ Controller → DbContext مستقیم
- ❌ View → Business Logic
- ❌ Service → View منطق
- ❌ Circular Dependencies

---

## 🔒 **امنیت (Security)**

### الزامات:
- [ ] `[Authorize]` attribute
- [ ] `[ValidateAntiForgeryToken]` در POST
- [ ] `[NoCache]` برای صفحات حساس
- [ ] HTTPS الزامی
- [ ] Mask کردن PII در Logs (کد ملی، موبایل، توکن)
- [ ] Rate Limiting (در صورت نیاز)
- [ ] Input Validation (Server + Client)

### ممنوع:
- ❌ PII در Logs بدون Mask
- ❌ SQL Injection (استفاده از Parameterized Queries)
- ❌ XSS (استفاده از @Html.Raw بدون Sanitize)
- ❌ CSRF (POST بدون AntiForgeryToken)

---

## 📋 **استانداردها (Standards)**

### رنگ‌بندی:
```css
:root {
    --medical-primary: #2c5aa0;   /* ✅ آبی درمانی */
    --medical-success: #28a745;   /* ✅ سبز */
    --medical-danger: #dc3545;    /* ✅ قرمز */
    --medical-warning: #ffc107;   /* ✅ زرد */
    --medical-info: #17a2b8;      /* ✅ آبی روشن */
}
```
❌ ممنوع: Gradient، رنگ‌های جیغ، بنفش، صورتی

### فونت:
```css
font-family: 'Vazir', 'Shabnam', 'Yekan', 'Tahoma', sans-serif;
```

### تاریخ:
- ✅ Persian DatePicker (از Helper استفاده کن)
- ✅ PersianDateHelper.ToPersianDate()
- ❌ ممنوع: `<input type="datetime-local">`

### پیام‌ها:
**Server-Side:**
```csharp
NotificationHelper.SetSuccess(TempData, "پیام");
NotificationHelper.SetError(TempData, "خطا");
```

**Client-Side:**
```javascript
Notify.success('پیام');  // نه toastr مستقیم
Notify.error('خطا');
Notify.confirm('تأیید?', 'عنوان', onConfirm, onCancel);
```

### آپلود تصویر:
```csharp
// ✅ استفاده از Service
await _imageUploadService.SaveImageAsync(file, folder);

// ❌ ممنوع: ذخیره مستقیم
```

---

## 🧪 **تست (Testing)**

### قبل از Submit:
- [ ] Build موفق (بدون Error)
- [ ] Happy Path تست شد
- [ ] Edge Cases تست شد
- [ ] Failure Recovery تست شد
- [ ] Responsive (Desktop/Tablet/Mobile)
- [ ] Browser Compatibility (Chrome, Edge, Firefox)
- [ ] Linter Errors رفع شد
- [ ] Console Errors وجود ندارد

---

## 📁 **مراجع سریع**

### قراردادها:
```
📁 Docs/DEVELOPMENT_CONTRACT.md
📁 Docs/Knowledge-Base/03-Development-Contract-Quick-Guide.md
📁 Docs/Knowledge-Base/AI/PreFlight/Bugfix-Master-Contract.md
📁 Docs/Knowledge-Base/AI/CURSOR/CLINICAPP_CURSOR_FLOW_DISCIPLINE_CONTRACT.md
```

### راهنماها:
```
📁 Docs/Knowledge-Base/README.md
📁 Docs/Knowledge-Base/14041004/NOTIFICATION_HELPER_USAGE_GUIDE.md
📁 Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md
📁 Docs/IMAGE_UPLOAD_SYSTEM_GUIDE.md
📁 Docs/CKEDITOR_USAGE_GUIDE.md
```

### Helpers:
```
📁 Docs/Knowledge-Base/01-Helpers-DateTime.md
📁 Docs/Knowledge-Base/02-Helpers-Validation.md
📁 Docs/Knowledge-Base/HelperExtensionsGuide.md
```

---

## 🎯 **Workflow: قبل از هر کاری**

### 1️⃣ Analyze (تحلیل):
```
- درخواست کاربر چیست؟
- کدام ماژول؟
- کدام فایل‌ها؟
- وابستگی‌ها؟
```

### 2️⃣ Search (جستجو):
```
- آیا کد مشابه وجود دارد؟
- آیا Helper موجود است؟
- آیا Pattern استفاده شده؟
- آیا مستندات وجود دارد؟
```

### 3️⃣ Check Contracts (بررسی قراردادها):
```
✅ این Preflight را چک کن
✅ DEVELOPMENT_CONTRACT را مرور کن
✅ Bugfix Contract (در صورت رفع خطا)
✅ Flow Contract (در صورت تغییر Flow)
```

### 4️⃣ Design (طراحی):
```
- معماری چطور است؟
- SRP رعایت می‌شود؟
- Security OK است؟
- Standards رعایت می‌شود؟
```

### 5️⃣ Implement (پیاده‌سازی):
```
✅ Factory Pattern
✅ ServiceResult Enhanced
✅ Error Handling
✅ Logging
✅ Notification (Server + Client)
✅ Validation
```

### 6️⃣ Test (تست):
```
✅ Build
✅ Manual Test
✅ Edge Cases
✅ Responsive
```

### 7️⃣ Document (مستندسازی):
```
✅ Code Comments
✅ Summary Report
✅ TODO items (اگر لازم)
```

---

## 🚨 **HARD STOP شرایط**

در صورت مشاهده هر یک → STOP و اطلاع به کاربر:

1. 🚫 نقض AI No-Fly Zone
2. 🚫 نقض قراردادها
3. 🚫 نقض معماری (SRP)
4. 🚫 مشکل امنیتی
5. 🚫 Breaking Changes بدون Migration
6. 🚫 حذف کد بدون Obsolete
7. 🚫 تغییر Silent
8. 🚫 کد بدون شواهد/تست

**Format:**
```markdown
🚨 HARD STOP - تعارض شناسایی شد

**مشکل:** [توضیح]
**قرارداد:** [نام قرارداد نقض شده]
**راه‌حل:** [پیشنهاد جایگزین]

آیا تأیید می‌کنید یا راه‌حل دیگری پیشنهاد دارید؟
```

---

## 📊 **ساختار خروجی**

### برای Bugfix:
```markdown
# 🐛 Bugfix: [Module]

## 1. خلاصه
- مشکل: [1 خط]
- ریشه: [1 خط]
- راه‌حل: [1 خط]

## 2. شواهد
- File: [path]
- Line: [number]
- Error: [message]

## 3. تحلیل ریشه‌ای
- چرا: [توضیح]
- مدرک: [evidence]

## 4. راه‌حل
- تغییرات: [changes]
- دلیل: [reason]

## 5. تست
- [ ] Build OK
- [ ] Manual Test OK
- [ ] Regression OK

## 6. Rollback
- [steps]
```

### برای Feature:
```markdown
# 🚀 Feature: [Name]

## 1. Requirements
- [list]

## 2. Design
- Entities: [list]
- ViewModels: [list]
- Services: [list]

## 3. Implementation
- [changes]

## 4. Testing
- [tests]

## 5. Documentation
- [docs]
```

---

## ✅ **چک‌لیست نهایی**

قبل از Submit، این را چک کن:

### Code Quality:
- [ ] ✅ قراردادها رعایت شد
- [ ] ✅ معماری صحیح است
- [ ] ✅ SRP رعایت شد
- [ ] ✅ Factory Pattern استفاده شد
- [ ] ✅ ServiceResult Enhanced استفاده شد
- [ ] ✅ Error Handling دارد
- [ ] ✅ Logging دارد
- [ ] ✅ Validation دارد

### Security:
- [ ] ✅ Authorization چک شد
- [ ] ✅ AntiForgeryToken دارد
- [ ] ✅ PII Masked است
- [ ] ✅ Input Validation دارد

### Standards:
- [ ] ✅ رنگ‌های استاندارد
- [ ] ✅ فونت Vazir
- [ ] ✅ Persian Date
- [ ] ✅ NotificationHelper
- [ ] ✅ بدون Gradient

### Testing:
- [ ] ✅ Build موفق
- [ ] ✅ Manual Test OK
- [ ] ✅ Responsive OK
- [ ] ✅ Console Errors None

---

## 🎯 **خلاصه برای AI:**

### قبل از هر پاسخ:
```
1. این Preflight را بخوان
2. AI No-Fly Zone را چک کن
3. قراردادها را مرور کن
4. معماری را بررسی کن
5. امنیت را چک کن
6. استانداردها را رعایت کن
7. تست کن
8. مستند کن
9. HARD STOP (در صورت تعارض)
```

### یادت باشه:
- 🚫 حدس ممنوع
- ✅ شواهد الزامی
- ✅ قراردادها بالاتر از همه
- ✅ امنیت اولویت اول
- ✅ تست الزامی
- 🚨 HARD STOP در صورت تعارض

---

**نسخه:** 2.0.0  
**تاریخ:** 2026-01-02  
**وضعیت:** 🔴 MANDATORY - قبل از هر پاسخ اجباری است

**📌 این فایل را Bookmark کن و همیشه قبل از پاسخ مراجعه کن!**

