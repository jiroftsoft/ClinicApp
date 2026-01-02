# ⚡ AI Preflight Quick V3 - ClinicApp
**30 ثانیه قبل از هر پاسخ**

---

## 🚦 چک‌لیست 5 ثانیه‌ای

```
1. ❌ AI No-Fly Zone? → ✓
2. ✅ قراردادها? → ✓  
3. 🏗️ معماری SRP? → ✓
4. 🔒 Security? → ✓
5. 📋 Standards? → ✓
6. 💰 Financial? (اگر مالی) → ✓
```

---

## 🚫 **15 ممنوعیت (NEVER DO)**

1. ❌ حدس زدن بدون شواهد
2. ❌ نقض قرارداد (DEVELOPMENT_CONTRACT)
3. ❌ Controller→DB مستقیم
4. ❌ حذف ServiceResult Enhanced
5. ❌ تغییر Silent
6. ❌ کد بدون Log
7. ❌ نقض Persian Date Standards
8. ❌ آپلود خارج از IImageUploadService
9. ❌ کد بدون Documentation
10. ❌ Library ناسازگار
11. ❌ تغییر بدون Test
12. ❌ ساده‌سازی بیش از حد
13. ❌ تصمیم مستقل
14. ❌ Breaking Changes بدون Migration
15. ❌ حذف کد بدون Obsolete

---

## 💰 **Financial Module? (اگر مالی)**

### 🚨 10 قانون طلایی:

1. ✅ هیچ تغییر بدون تست کامل (5 سناریو)
2. ✅ هر تراکنش = Log کامل
3. ✅ Transaction Management الزامی
4. ✅ Verification بعد از Save
5. ✅ Idempotency Key برای پرداخت‌ها
6. ✅ هیچ Hard-Delete نداریم
7. ✅ Audit Trail کامل (Created/Updated/Deleted)
8. ✅ decimal(18,0) برای مبالغ
9. ✅ RowVersion (Concurrency)
10. ✅ Code Review قبل از Merge

### ممنوع در مالی:
```csharp
❌ payment.Amount = newAmount; // بدون log
❌ _context.Remove(payment); // hard delete
❌ SaveChanges() بدون transaction
❌ SaveChanges() بدون verification
```

---

## ✅ **چک‌لیست سریع**

### Code:
- [ ] Factory Pattern (ViewModels)
- [ ] ServiceResult Enhanced
- [ ] try-catch (Error Handling)
- [ ] Serilog + Mask PII
- [ ] Strongly-Typed ViewModels

### Security:
- [ ] [Authorize]
- [ ] [ValidateAntiForgeryToken]
- [ ] [NoCache]
- [ ] Input Validation

### Standards:
- [ ] رنگ: --medical-primary: #2c5aa0
- [ ] فونت: Vazir
- [ ] تاریخ: PersianDateHelper
- [ ] پیام: Notify.success() / NotificationHelper
- [ ] بدون Gradient

### Test:
- [ ] Build OK
- [ ] Manual Test OK
- [ ] Edge Cases OK
- [ ] Responsive OK
- [ ] Console Clean

---

## 🔧 **Bugfix? (اگر باگ)**

### فرآیند 6 مرحله‌ای:

1. **شناسایی:** نوع/شدت/محدوده
2. **علت ریشه‌ای:** 5 Whys
3. **وابستگی‌ها:** Callers/Dependencies
4. **رفع اتمیک:** Minimal Changes + NO_DELETE
5. **تست:** Build + Manual + Regression
6. **گزارش:** Evidence + Solution + Rollback

---

## 🚨 **HARD STOP**

اگر دیدی → STOP:
- نقض AI No-Fly Zone
- نقض قرارداد
- نقض معماری  
- مشکل امنیتی
- نقض Financial Rules (اگر مالی)
- Breaking Changes
- حذف کد

---

## 📁 **مراجع سریع**

**Full:** `AI_PREFLIGHT_MASTER_V3.md`  
**Contracts:**
- `Docs/DEVELOPMENT_CONTRACT.md`
- `Docs/Knowledge-Base/CRITICAL-FINANCIAL-MODULE-CONTRACT.md` (مالی)
- `PREFLIGHT_CHECKLIST.md`

**Helpers:** `Docs/Knowledge-Base/`

---

**⏱ 30 ثانیه → GO!**

**🎯 یادت باشه: مالی = حساسیت دوبرابر!**

