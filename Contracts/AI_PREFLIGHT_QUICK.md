# ⚡ AI Preflight Quick - ClinicApp
**30 ثانیه قبل از هر پاسخ**

---

## 🚦 چک‌لیست 5 ثانیه‌ای

```
1. ❌ AI No-Fly Zone? → OK
2. ✅ قراردادها? → OK  
3. 🏗️ معماری SRP? → OK
4. 🔒 Security? → OK
5. 📋 Standards? → OK
```

---

## 🚫 15 ممنوعیت

1. ❌ حدس زدن
2. ❌ نقض قرارداد
3. ❌ Controller→DB مستقیم
4. ❌ حذف ServiceResult
5. ❌ تغییر Silent
6. ❌ کد بدون Log
7. ❌ نقض Persian Date
8. ❌ آپلود خارج از Service
9. ❌ کد بدون Doc
10. ❌ Library ناسازگار
11. ❌ تغییر بدون Test
12. ❌ ساده‌سازی بیش از حد
13. ❌ تصمیم مستقل
14. ❌ Breaking Changes
15. ❌ حذف کد

---

## ✅ چک‌لیست سریع

### Code:
- [ ] Factory Pattern
- [ ] ServiceResult Enhanced
- [ ] Error Handling (try-catch)
- [ ] Logging (Serilog + Mask PII)
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
- [ ] Responsive OK
- [ ] Console Clean

---

## 🚨 HARD STOP

اگر دیدی → STOP:
- نقض قرارداد
- نقض معماری  
- مشکل امنیتی
- Breaking Changes
- حذف کد

---

## 📁 مراجع

**Full:** `AI_PREFLIGHT_MASTER.md`  
**Contracts:** `Docs/DEVELOPMENT_CONTRACT.md`  
**Helpers:** `Docs/Knowledge-Base/`

---

**⏱ 30 ثانیه → GO!**

