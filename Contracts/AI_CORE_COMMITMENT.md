# 🤖 تعهدات اصلی AI - ClinicApp
**نسخه:** 1.0.0 | **تاریخ:** 2026-01-06 | **وضعیت:** 🔴 **الزامی - نقض‌ناپذیر**

---

## ⚡ **قانون طلایی:**
**این فایل = تعهد من به شما. هرگز نقض نمی‌کنم.**

---

## 🚫 **15 ممنوعیت مطلق (AI No-Fly Zone)**

```
1. ❌ حدس زدن بدون شواهد
2. ❌ نقض قرارداد (DEVELOPMENT_CONTRACT)
3. ❌ Controller→DB مستقیم (باید: Controller→Service→Repository)
4. ❌ حذف/ساده‌سازی ServiceResult Enhanced
5. ❌ تغییر Silent (بی‌صدا)
6. ❌ کد بدون Log (Serilog + Mask PII)
7. ❌ نقض Persian Date Standards (ENTERPRISE_DATE_MIGRATION_GUIDE.md)
8. ❌ آپلود خارج از IImageUploadService
9. ❌ کد بدون Documentation
10. ❌ Library/Pattern ناسازگار
11. ❌ تغییر بدون Test
12. ❌ ساده‌سازی بیش از حد
13. ❌ تصمیم مستقل بدون تأیید
14. ❌ Breaking Changes بدون Migration
15. ❌ حذف کد بدون Obsolete
```

**در صورت نقض → HARD STOP**

---

## 🚨 **NO BLIND CHANGES (بالاترین اولویت)**

### ماژول‌های حیاتی (CRITICAL):
- Authentication & Authorization
- Patient Management
- Financial & Payment
- Database Relationships

### فرآیند الزامی:
```
1️⃣ STOP & THINK
   - آیا منطق فعلی را درک کرده‌ام؟
   - آیا راه‌حل بهتری وجود دارد؟

2️⃣ ASK USER
   - مشکل دقیقاً چیست؟ (نمونه بخواه)
   - رفتار مورد انتظار چیست؟
   - چه تستی لازم است؟

3️⃣ ANALYZE IMPACT
   - grep/codebase_search برای استفاده‌ها
   - Database Dependencies
   - ماژول‌های تحت تأثیر

4️⃣ PROPOSE SOLUTION
   - توضیح کامل تغییرات
   - منتظر تأیید کاربر

5️⃣ IMPLEMENT (بعد از تأیید)
   - تغییرات تدریجی
   - تست بعد از هر مرحله

6️⃣ VERIFY
   - Build OK
   - Test OK
   - Report
```

**هرگز بدون درک کامل منطق + تأیید کاربر تغییر نده!**

---

## 💰 **10 قانون طلایی مالی (اگر مالی)**

```
1. ✅ تست کامل (5 سناریو) قبل از تغییر
2. ✅ هر تراکنش = Log کامل
3. ✅ Transaction Management الزامی
4. ✅ Verification بعد از Save
5. ✅ Idempotency Key برای پرداخت‌ها
6. ✅ هیچ Hard-Delete (فقط SoftDelete)
7. ✅ Audit Trail کامل (Created/Updated/Deleted)
8. ✅ decimal(18,0) برای مبالغ IRR
9. ✅ RowVersion (Concurrency Control)
10. ✅ Code Review Senior قبل از Merge
```

**ممنوع در مالی:**
```csharp
❌ payment.Amount = newAmount; // بدون log
❌ _context.Remove(payment); // hard delete
❌ SaveChanges() بدون transaction
❌ SaveChanges() بدون verification
```

---

## 🔧 **فرآیند 6 مرحله‌ای دیباگ (اگر باگ)**

```
1. شناسایی: نوع/شدت/محدوده
2. 5 Whys: علت ریشه‌ای (نه علائم)
3. وابستگی‌ها: Callers/Dependencies
4. رفع اتمیک: Minimal Changes + NO_DELETE
5. تست: Build + Manual + Regression
6. گزارش: Evidence + Solution + Rollback
```

**قانون:** ❌ ممنوع رفع کورکورانه!

---

## ✅ **الزامات هر پاسخ**

### Code:
```csharp
✓ Factory Pattern (ViewModels)
✓ ServiceResult Enhanced (همه جا)
✓ try-catch (Error Handling)
✓ Serilog + Mask PII (Logging)
✓ Strongly-Typed ViewModels
```

### Security:
```csharp
✓ [Authorize]
✓ [ValidateAntiForgeryToken] (POST)
✓ [NoCache] (صفحات حساس)
✓ Input Validation
✓ PII Masked در Logs
```

### Standards:
```css
✓ رنگ: --medical-primary: #2c5aa0
✓ فونت: Vazir / IRANSansX
✓ تاریخ: PersianDateHelper + Enterprise-Grade (ENTERPRISE_DATE_MIGRATION_GUIDE.md)
✓ DatePicker: @Html.Partial("_PersianDatePicker") (الزامی - ممنوع datetime-local)
✓ پیام: NotificationHelper / Notify.success()
✓ بدون Gradient / رنگ جیغ
✓ UTC در دیتابیس، تبدیل به timezone محلی فقط برای نمایش
```

---

## 🏗️ **معماری (SRP سختگیرانه)**

```
View = Passive (فقط نمایش)
Controller = Orchestration (فقط هماهنگی)
Service = Business Logic (منطق کسب‌وکار)
Repository = Data Access (دسترسی به داده)
```

**Entity → ViewModel:**
```csharp
// ✅ فقط با Factory Method
var viewModel = DoctorCardViewModel.FromEntity(doctor);

// ❌ ممنوع: inline mapping
var viewModel = new DoctorCardViewModel { ... };
```

**Service Output:**
```csharp
// ✅ همیشه ServiceResult
return ServiceResult<T>.Successful(data, "پیام", "Operation", userId);

// ❌ ممنوع: raw object
return Json(new { success = true });
```

---

## 🧰 **Helpers موجود (Reuse-First)**

**قبل از نوشتن کد جدید → جستجو کن:**

### تاریخ (الزامی - Enterprise-Grade):
- `PersianDateHelper.ToPersianDate()` - تبدیل به شمسی
- `this.ParseDateFromHiddenInput("Date", _logger)` - Parse در Controller
- `@Html.Partial("_PersianDatePicker")` - DatePicker در View (الزامی)
- `ITimeProvider` - برای Services (UTC management)
- **مرجع:** `Docs/ENTERPRISE_DATE_MIGRATION_GUIDE.md` (الزامی)

### Validation:
- `IranianNationalCodeValidator.IsValid()`
- `PhoneNumberValidator.IsValidMobile()`
- `PhoneNumberHelper.CleanPhoneNumber()`

### Notification:
- `NotificationHelper.SetSuccess(TempData, "...")`
- `Notify.success('...')` (Frontend)
- `AdminNotification.success('...')` (Admin)

### Age:
- `AgeCalculationHelper.CalculateAge()`
- `AgeCalculationHelper.CalculateAgeString()`

**❌ اگر چیزی موجود است، دوباره نساز!**

---

## 📋 **چک‌لیست قبل از هر پاسخ (30s)**

```
□ AI_EXECUTION_CONTRACT.md چک شد (10s)
□ CRITICAL_MODULE_SAFETY_CONTRACT.md چک شد (20s) ← اگر حیاتی
□ AI_PREFLIGHT_QUICK_V3.md چک شد (30s)
□ Knowledge-Base چک شد (اگر نیاز به Helper)
□ 15 ممنوعیت بررسی شد
□ نوع کار شناسایی شد:
   - معمولی → Quick
   - مالی → STEP 2 (10 قانون)
   - باگ → STEP 3 (6 مرحله)
   - حیاتی → ASK USER FIRST
□ HARD STOP بررسی شد (در صورت تعارض)
```

---

## 🚨 **HARD STOP Conditions**

**در صورت مشاهده → STOP و اطلاع:**

1. 🚫 نقض 15 ممنوعیت
2. 🚫 نقض قراردادها
3. 🚫 نقض معماری (SRP)
4. 🚫 مشکل امنیتی
5. 🚫 نقض Financial Rules (اگر مالی)
6. 🚫 Breaking Changes بدون Migration
7. 🚫 حذف کد بدون Obsolete
8. 🚫 تغییر Silent
9. 🚫 کد بدون شواهد/تست
10. 🚫 تغییر حیاتی بدون تأیید کاربر

**Format HARD STOP:**
```markdown
🚨 HARD STOP - تعارض شناسایی شد

**مشکل:** [description]
**قرارداد نقض شده:** [contract name]
**راه‌حل جایگزین:** [alternative solution]

آیا تأیید می‌کنید یا راه‌حل دیگری پیشنهاد می‌دهید؟
```

---

## 🎯 **Workflow برای هر نوع تسک**

### 🔍 **بررسی و بهینه‌سازی:**
```
1. Scope + Risk Assessment
2. Module Map (Controller→Service→Repo)
3. Critical Issues (Max 7) با Evidence
4. Root Cause Analysis (5 Whys)
5. Fix Plan (Ranked, Minimal)
6. Implementation Diffs
7. Tests + Verify + Rollback
```

### 🚀 **پیاده‌سازی ماژول جدید:**
```
1. Analysis & Design (Entity/ViewModel/Interface)
2. Backend (Repository + Service)
3. Controller (Orchestration فقط)
4. View (Strongly-Typed ViewModel)
5. UI/UX (رنگ/فونت/تاریخ استاندارد)
6. Test (Build + Manual + Edge Cases)
7. Document (Comments + Summary)
```

### 🐛 **رفع خطا:**
```
1. شناسایی (نوع/شدت/محدوده)
2. 5 Whys (Root Cause)
3. وابستگی‌ها (grep/codebase_search)
4. رفع اتمیک (Minimal + NO_DELETE)
5. تست (Build + Manual + Regression)
6. گزارش (Evidence + Solution + Rollback)
```

### 🗄️ **اتصال به دیتابیس:**
```
1. بررسی Connection String (Web.config)
2. بررسی DbContext Configuration
3. بررسی Entity Configurations
4. بررسی Migrations (اگر لازم)
5. تست Connection (Manual)
6. Log Connection Issues
```

---

## 📊 **Output Format (کوتاه و کاربردی)**

### برای Bugfix:
```markdown
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

### برای Module Review:
```markdown
# 📊 Module Review

## 1. Preflight
- Scope: [files]
- Risk: [Critical/High/Medium/Low]

## 2. Critical Issues (≤7)
1. [Issue] (Evidence: [file:line])

## 3. Root Cause
- [analysis]

## 4. Fix Plan
- [minimal changes]

## 5. Tests
- [unit/integration]

## 6. Verdict
- ✅ Go | ⚠️ Go with risk | ❌ No-Go
```

---

## 🎯 **اصول تفکر و عمل**

### ✅ **همیشه:**
- منطقی و عمیق فکر کن
- شواهد قبل از ادعا
- Root Cause قبل از Fix
- Minimal Changes
- Test قبل از Commit
- Document بعد از Change

### ❌ **هرگز:**
- حدس نزن
- فرض نکن
- عجله نکن
- تغییرات گسترده نده
- بدون Test تغییر نده
- بدون Document تغییر نده

---

## 📝 **نکات مهم پروژه (User Preferences)**

### **Migration Management:**
```
✅ کاربر خودش Migration ها را ایجاد و اجرا می‌کند
❌ من نباید Migration ایجاد کنم (فقط در صورت درخواست صریح)
❌ من نباید Update-Database اجرا کنم
```

**قانون:** 
- اگر نیاز به Migration بود، فقط فایل Migration را ایجاد می‌کنم
- اجرای Migration بر عهده کاربر است

---

## 📁 **مراجع سریع (Read-by-Reference)**

```
📄 Contracts/AI_EXECUTION_CONTRACT.md (10s)
📄 Contracts/CRITICAL_MODULE_SAFETY_CONTRACT.md (20s) ← بالاترین اولویت
📄 Contracts/AI_PREFLIGHT_QUICK_V3.md (30s)
📄 Contracts/AI_PREFLIGHT_MASTER_V3.md (10m) ← اگر پیچیده

📚 Knowledge-Base (اگر نیاز به Helper):
   📖 Contracts/Knowledge-Base/AI/Master/README.md
   📋 Contracts/Knowledge-Base/AI/Master/INDEX.md
   📅 Contracts/Knowledge-Base/AI/Master/01-Helpers-DateTime.md
   📋 Contracts/Knowledge-Base/AI/Master/02-Helpers-Validation.md
   ⚡ Contracts/Knowledge-Base/AI/Master/03-Development-Contract-Quick-Guide.md
   🔧 Contracts/Knowledge-Base/AI/PreFlight/05-Debugging-Specialist-Contract.md
```

---

## ✅ **تعهد نهایی**

```
من متعهد می‌شوم:

✅ قبل از هر پاسخ، این فایل را می‌خوانم
✅ 15 ممنوعیت را رعایت می‌کنم
✅ NO BLIND CHANGES برای تغییرات حیاتی
✅ مالی = حساسیت 2x (10 قانون طلایی)
✅ باگ = فرآیند 6 مرحله‌ای (5 Whys)
✅ همیشه منطقی و عمیق فکر می‌کنم
✅ بدون حاشیه و داکیومنت‌های طولانی
✅ شواهد + تست + مستند
✅ HARD STOP در صورت تعارض
✅ استفاده از Helpers موجود (Reuse-First)
```

---

## 🎯 **خلاصه برای Quick Reference**

```
قبل از هر پاسخ:
1. AI_EXECUTION_CONTRACT.md (10s)
2. CRITICAL_MODULE_SAFETY_CONTRACT.md (20s) ← اگر حیاتی
3. AI_PREFLIGHT_QUICK_V3.md (30s)
4. Knowledge-Base (اگر نیاز به Helper)

الزامات:
✓ Factory Pattern
✓ ServiceResult Enhanced
✓ try-catch + Serilog
✓ [Authorize] + [ValidateAntiForgeryToken]
✓ Standards (رنگ/فونت/تاریخ)

مالی = STEP 2 (10 قانون)
باگ = STEP 3 (6 مرحله)
حیاتی = ASK USER FIRST
تعارض = HARD STOP
```

---

**نسخه:** 1.0.0  
**تاریخ:** 2026-01-06  
**وضعیت:** 🔴 **الزامی - نقض‌ناپذیر**

**📌 این فایل = تعهد من به شما. هرگز نقض نمی‌کنم.**

