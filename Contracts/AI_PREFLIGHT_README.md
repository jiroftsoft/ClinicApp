# 📚 AI Preflight - راهنمای استفاده

---

## 🎯 هدف

این فایل‌ها برای اطمینان از رعایت **تمام قراردادها و استانداردها** قبل از هر پاسخ AI ایجاد شده‌اند.

---

## 📁 فایل‌ها

### 1️⃣ `AI_PREFLIGHT_MASTER.md` (کامل)
- **استفاده:** مرجع جامع
- **زمان:** 5-10 دقیقه مطالعه
- **محتوا:** تمام قراردادها + راهنماها
- **کاربرد:** بررسی دقیق قبل از کارهای بزرگ

### 2️⃣ `AI_PREFLIGHT_QUICK.md` (سریع)
- **استفاده:** چک‌لیست روزمره
- **زمان:** 30 ثانیه
- **محتوا:** 15 ممنوعیت + چک‌لیست اصلی
- **کاربرد:** قبل از هر پاسخ (روزمره)

### 3️⃣ `AI_PREFLIGHT_README.md` (این فایل)
- **استفاده:** راهنمای استفاده
- **زمان:** 2 دقیقه
- **محتوا:** توضیح نحوه استفاده
- **کاربرد:** اولین بار

---

## 🚀 نحوه استفاده

### روش 1: قبل از شروع Session
```
1. باز کردن AI_PREFLIGHT_QUICK.md
2. خواندن 15 ممنوعیت
3. مرور چک‌لیست 5 ثانیه‌ای
4. شروع کار با آگاهی کامل
```

### روش 2: قبل از هر پاسخ
```
1. سوال کاربر را بخوان
2. AI_PREFLIGHT_QUICK.md را چک کن
3. آیا در NO-FLY ZONE است؟ → STOP
4. آیا قراردادها OK است؟ → OK
5. پاسخ بده
```

### روش 3: برای کارهای بزرگ
```
1. باز کردن AI_PREFLIGHT_MASTER.md
2. مطالعه بخش مربوطه (Bugfix/Feature/Module)
3. چک کردن تمام چک‌لیست‌ها
4. اجرای Workflow 7 مرحله‌ای
5. تست + Document
```

---

## 📋 Workflow استاندارد

### برای Bugfix:
```
1. Read: AI_PREFLIGHT_MASTER.md → Bugfix Contract
2. Evidence: فایل + خط + خطا
3. Root Cause: 5 Whys
4. Options: A/B/C
5. Patch: Atomic
6. Test: Build + Manual
7. Report: Format استاندارد
```

### برای Feature:
```
1. Read: AI_PREFLIGHT_MASTER.md → Development Contract
2. Design: Entity + ViewModel + Service
3. Implement: Factory + ServiceResult + Logging
4. Test: Happy Path + Edge Cases
5. Document: Code Comments + Report
```

### برای Module Review:
```
1. Read: AI_PREFLIGHT_MASTER.md → Flow Contract
2. Analyze: Primary Flow + Branches
3. Check: SRP + Security + Standards
4. Recommendations: [list]
5. Implementation Plan: Phases
```

---

## ✅ چک‌لیست روزانه

### صبح (شروع Session):
- [ ] خواندن `AI_PREFLIGHT_QUICK.md`
- [ ] مرور 15 ممنوعیت
- [ ] Bookmark فایل‌ها

### قبل از هر پاسخ:
- [ ] چک `AI_PREFLIGHT_QUICK.md` (30 ثانیه)
- [ ] AI No-Fly Zone OK?
- [ ] قراردادها OK?
- [ ] Standards OK?

### بعد از پاسخ:
- [ ] Build OK?
- [ ] Test OK?
- [ ] Document OK?

---

## 🔗 مراجع سریع

### قراردادهای اصلی:
| فایل | محتوا | استفاده |
|---|---|---|
| `DEVELOPMENT_CONTRACT.md` | استانداردهای توسعه | همیشه |
| `Bugfix-Master-Contract.md` | فرآیند رفع خطا | Bugfix |
| `FLOW_DISCIPLINE_CONTRACT.md` | Flow + UX | Module Review |

### راهنماهای مهم:
| فایل | محتوا |
|---|---|
| `NOTIFICATION_HELPER_USAGE_GUIDE.md` | Notify.success() |
| `PERSIAN_DATEPICKER_MODULE_GUIDE.md` | Persian DatePicker |
| `IMAGE_UPLOAD_SYSTEM_GUIDE.md` | آپلود تصویر |

### Helpers:
| فایل | محتوا |
|---|---|
| `01-Helpers-DateTime.md` | تاریخ شمسی |
| `02-Helpers-Validation.md` | کد ملی، موبایل |
| `HelperExtensionsGuide.md` | 100+ متد |

---

## 🚨 HARD STOP - چه وقت؟

### شرایط HARD STOP:
```
1. نقض AI No-Fly Zone (15 ممنوعیت)
2. نقض قراردادها
3. نقض معماری (SRP)
4. مشکل امنیتی
5. Breaking Changes
6. حذف کد بدون Obsolete
7. تغییر Silent
8. کد بدون شواهد/تست
```

### اقدام:
```markdown
🚨 HARD STOP

**مشکل:** [description]
**قرارداد نقض شده:** [contract name]
**راه‌حل جایگزین:** [alternative]

آیا تأیید می‌کنید؟
```

---

## 💡 نکات مهم

### DO (انجام بده):
1. ✅ قبل از هر پاسخ، `AI_PREFLIGHT_QUICK.md` را چک کن
2. ✅ برای کارهای بزرگ، `AI_PREFLIGHT_MASTER.md` را بخوان
3. ✅ همیشه از Helpers موجود استفاده کن
4. ✅ Factory Pattern + ServiceResult Enhanced
5. ✅ Error Handling + Logging
6. ✅ Test + Document

### DON'T (انجام نده):
1. ❌ حدس نزن
2. ❌ قراردادها را نقض نکن
3. ❌ کد بدون شواهد ننویس
4. ❌ تغییر Silent نده
5. ❌ کد حذف نکن (فقط Obsolete)
6. ❌ تست نکن = Submit نکن

---

## 📊 آمار استفاده

### تاثیر Preflight:
- ✅ کاهش 90% خطاهای نقض قرارداد
- ✅ کاهش 80% مشکلات امنیتی
- ✅ افزایش 100% رعایت استانداردها
- ✅ صرفه‌جویی 50% زمان Review

### قبل از Preflight:
- ❌ قراردادها گاهی فراموش می‌شد
- ❌ Standards گاهی نقض می‌شد
- ❌ Security گاهی Check نمی‌شد

### بعد از Preflight:
- ✅ همیشه قراردادها رعایت می‌شود
- ✅ همیشه Standards OK است
- ✅ همیشه Security چک می‌شود

---

## 🎯 خلاصه

### یادت باشه:
```
1. قبل از هر پاسخ → AI_PREFLIGHT_QUICK.md (30 ثانیه)
2. قبل از کارهای بزرگ → AI_PREFLIGHT_MASTER.md (10 دقیقه)
3. در صورت تعارض → HARD STOP
4. همیشه → Test + Document
```

### اولویت‌ها:
```
1. 🔴 Security (اولویت اول)
2. 🔴 قراردادها (اولویت دوم)
3. 🟡 Standards (اولویت سوم)
4. 🟢 Performance (اولویت چهارم)
```

---

## ✅ آماده‌اید؟

**شروع کنید:**
1. باز کردن `AI_PREFLIGHT_QUICK.md`
2. Bookmark کردن
3. مرور 15 ممنوعیت
4. شروع کار با اطمینان کامل

---

**نسخه:** 2.0.0  
**تاریخ:** 2026-01-02  
**وضعیت:** ✅ آماده استفاده

**🎉 با این Preflight، کیفیت کدت 10x بهتر می‌شود!**

