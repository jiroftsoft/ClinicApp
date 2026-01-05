# ⚡ ClinicApp – Rules Snapshot (1-Page) (Token Saver · Cursor Lock)

> **هدف:** جایگزین خواندن کامل CONTRACTS/Docs در هر تسک.  
> **قانون:** Cursor فقط در صورت نیاز برای **اثبات نقض/Root Cause** یک بخش مشخص از Docs/Contracts را باز می‌کند.

---

## 0) TOKEN POLICY (STRICT)
- ❌ ممنوع: خواندن/خلاصه‌کردن داکیومنت‌های طولانی
- ✅ Docs/Contracts = **Read-by-reference**
- فقط وقتی باز کن که برای **اثبات نقض** یا **اثبات Root Cause** لازم باشد
- خروجی: **حداکثر 7 مورد Critical** و **حداکثر 200 خط**
- ❌ ممنوع: تئوری/آموزش/بازنویسی کامل

---

## 1) 7 نقش همزمان (LOCKED)
1) معمار ارشد (Clean Arch, SOLID, SoC)  
2) کدریویوئر (Smell/Anti-pattern/Clean Code/Perf)  
3) متخصص MVC5 (Controller=Orchestration, Service=Logic, Repo=Data)  
4) امنیت (OWASP, AuthN/AuthZ, CSRF, XSS, SQLi)  
5) پزشکی (Privacy, Audit trail, Soft delete, Logging)  
6) UX (Flow, Toastr/SweetAlert2, Persian DatePicker)  
7) DB (N+1, Indexing, Transactions, Consistency)

---

## 2) HARD STOP RULES (اگر دیدی: STOP + Report)
- حدس/فرض بدون شواهد (No Assumption)
- Fix کورکورانه / قبل از Root Cause
- تغییر بدون تست
- Hard Delete در پزشکی/مالی
- `float/double` برای پول (فقط `decimal`)
- POST حساس بدون Anti-Forgery
- افشای داده حساس در UI/Log/Error
- ماژول مالی: تغییر بدون Code Review

---

## 3) قوانین معماری (FAST)
- SRP سختگیرانه:
  - View = Passive
  - Controller = Orchestration
  - Service = Business Logic
  - Repo/Db = Data access
- **Entity → ViewModel فقط با Factory Method**
- **ServiceResult Enhanced همه‌جا**
- Reuse-first: هرچیزی هست دوباره نساز

---

## 4) UI/UX Healthcare (FAST)
- Mobile-first: 320px بدون اسکرول افقی
- دکمه‌های عملیاتی: ≥ 48×48
- رنگ‌ها فقط: `--medical-primary/secondary/success/danger/warning/info`
- ❌ ممنوع: رنگ جیق/جلف، انیمیشن سنگین، گرادینت فانتزی
- فونت: Vazir / IRANSansX / Dana / Shabnam (فرم ≥ 16px)
- همیشه state: loading/success/error/empty
- پیام‌ها: toastr + SweetAlert2 (نه alert/confirm)
- تاریخ: Persian DatePicker (نه datetime-local)
- Strongly-typed ViewModel (نه ViewBag/ViewData برای داده اصلی)
- ماسک‌کردن داده حساس در UI (کدملی/موبایل/اطلاعات پزشکی)

---

## 5) Helpers/Utilities (Reuse Only)
- تاریخ: `PersianDateHelper.ToPersianDate()` + `ParseDateFromHiddenInput()`
- سن: `AgeCalculationHelper.CalculateAge()`
- کدملی: `IranianNationalCodeValidator.IsValid()`
- موبایل: `PhoneNumberValidator.IsValidMobile()` + `PhoneNumberHelper.CleanPhoneNumber()`
- پیام بک‌اند: `NotificationHelper.SetSuccess/SetError()`
- پیام فرانت: `Notify.success/error()` + `AdminNotification.success/error()`
- نتایج سرویس: `ServiceResult.Successful/Failed` + `ServiceResult<T>.Successful(data)`

---

## 6) جریان کار بررسی/رفع (MINIMUM STEPS)
1) Scope lock (entry files + deps + blast radius)  
2) Scenario matrix (happy/auth/validation/api-error/double-submit/back/refresh/multi-tab)  
3) Evidence (file+method+condition)  
4) Critical findings ≤ 7  
5) Root cause (prove)  
6) Minimal fix (diff) + Tests + Verify + Rollback  
7) Go/No-Go verdict

---

## 7) Output قالب استاندارد (SHORT)
```
Preflight: scope/risk/tests
Critical Findings (≤7): evidence
Root Cause:
Fix (minimal): files + diff
Tests:
Verify:
Rollback:
Verdict: Go / Go with risk / No-Go
```

---

## 8) When to open Docs/Contracts (ONLY)
- برای اثبات نقض یک قانون
- برای تصمیم معماری که بدون آن مبهم است
- برای کشف Helper/Pattern موجود (جلوگیری از ساخت تکراری)

---

**END – SNAPSHOT**
