# 🛡️ فایل پیش‌پرواز - ClinicApp Development

**نسخه:** 1.0.0  
**تاریخ ایجاد:** 2025-01-27  
**وضعیت:** اجباری - قبل از هر پاسخ

---

## ⚠️ قانون طلایی

**این فایل باید قبل از هر پاسخ بررسی شود. در صورت تعارض → Hard Stop**

---

## STEP 0: AI Guard Check (اجباری)

### ✅ بررسی 15 قانون ممنوعه (AI No-Fly Zone)

**مرجع:** `Contracts/04-AI-No-Fly-Zone.md` یا `Contracts/05-AI-Guard-Prompt-Mandatory.md`

**چک‌لیست سریع:**
- [ ] ❌ ممنوعیت حدس، فرض و تولید کد احتمالی
- [ ] ❌ ممنوعیت نقض قراردادها
- [ ] ❌ ممنوعیت دور زدن معماری لایه‌ای
- [ ] ❌ ممنوعیت حذف یا ساده‌سازی ServiceResult
- [ ] ❌ ممنوعیت بی‌توجهی به امنیت داده‌های درمانی
- [ ] ❌ ممنوعیت تولید کد بدون لاگ‌پذیری
- [ ] ❌ ممنوعیت تخطی از استاندارد تاریخ شمسی
- [ ] ❌ ممنوعیت آپلود فایل خارج از سیستم استاندارد
- [ ] ❌ ممنوعیت تغییر Silent (بی‌صدا)
- [ ] ❌ ممنوعیت تولید کد بدون مستندسازی
- [ ] ❌ ممنوعیت پیشنهاد Library یا Pattern ناسازگار
- [ ] ❌ ممنوعیت تغییر رفتار سیستم بدون تست ذهنی
- [ ] ❌ ممنوعیت ساده‌سازی بیش از حد
- [ ] ❌ ممنوعیت تصمیم‌گیری مستقل
- [ ] ❌ شرط توقف فوری (Hard Stop Rule)

**در صورت نقض هر قانون → Hard Stop و اطلاع به کاربر**

---

## STEP 1: بررسی 12 دروازه امنیتی

### دروازه 1: Alignment Check (هم‌راستایی)
- [ ] آیا درخواست با معماری ClinicApp هم‌راستا است؟
- [ ] آیا با Clean Architecture سازگار است؟
- [ ] آیا با Repository Pattern سازگار است؟

**مرجع:** `Contracts/02-Architecture-Guidelines.md`

### دروازه 2: Contract Enforcement (اجرای قرارداد)
- [ ] آیا با `DEVELOPMENT_CONTRACT.md` سازگار است؟
- [ ] آیا با `Bugfix-Master-Contract.md` سازگار است؟
- [ ] آیا با `01-PreFlight-Protocol.md` سازگار است؟

**مراجع:**
- `Docs/DEVELOPMENT_CONTRACT.md`
- `Bugfix-Master-Contract.md`
- `Contracts/01-PreFlight-Protocol.md`

### دروازه 3: Architecture Gate (دروازه معماری)
- [ ] آیا از ServiceResult Pattern استفاده می‌شود؟
- [ ] آیا از Repository Pattern استفاده می‌شود؟
- [ ] آیا از Dependency Injection استفاده می‌شود؟
- [ ] آیا Strongly-Typed است؟

**مرجع:** `Contracts/02-Architecture-Guidelines.md`

### دروازه 4: Security Gate (دروازه امنیت)
- [ ] آیا Anti-Forgery Token استفاده شده؟
- [ ] آیا داده‌های حساس Mask شده‌اند؟
- [ ] آیا HTTPS الزامی است؟
- [ ] آیا Authorization بررسی شده؟

**مرجع:** `Docs/DEVELOPMENT_CONTRACT.md` - بخش Security

### دروازه 5: Standards Gate (دروازه استانداردها)
- [ ] آیا از تاریخ شمسی استفاده شده؟
- [ ] آیا از فونت Vazir استفاده شده؟
- [ ] آیا RTL Support دارد؟
- [ ] آیا از PersianDateHelper استفاده شده؟

**مرجع:** `Docs/DEVELOPMENT_CONTRACT.md` - بخش Standards

### دروازه 6: Data Integrity Gate (دروازه یکپارچگی داده)
- [ ] آیا Decimal Precision درست است؟ (18,0 برای پول)
- [ ] آیا RowVersion برای Concurrency استفاده شده؟
- [ ] آیا SoftDelete پیاده‌سازی شده؟
- [ ] آیا Audit Trail وجود دارد؟

**مرجع:** `Bugfix-Master-Contract.md` - بخش Invariants

### دروازه 7: Code Quality Gate (دروازه کیفیت کد)
- [ ] آیا Code Duplication وجود ندارد؟
- [ ] آیا Error Handling مناسب است؟
- [ ] آیا Logging مناسب است؟
- [ ] آیا Code Comments کافی است؟

**مرجع:** `Contracts/01-PreFlight-Protocol.md`

### دروازه 8: Performance Gate (دروازه عملکرد)
- [ ] آیا N+1 Query وجود ندارد؟
- [ ] آیا از Include/Projection استفاده شده؟
- [ ] آیا Index مناسب است؟
- [ ] آیا Caching مناسب است؟ (در صورت نیاز)

**مرجع:** `Bugfix-Master-Contract.md` - بخش Performance

### دروازه 9: UI/UX Gate (دروازه رابط کاربری)
- [ ] آیا طراحی رسمی و حرفه‌ای است؟
- [ ] آیا از Card Components استفاده شده؟
- [ ] آیا Responsive است؟
- [ ] آیا Accessibility رعایت شده؟

**مرجع:** `Docs/DEVELOPMENT_CONTRACT.md` - بخش UI/UX

### دروازه 10: Testing Gate (دروازه تست)
- [ ] آیا Manual Testing انجام شده؟
- [ ] آیا Edge Cases بررسی شده؟
- [ ] آیا Regression Tests وجود دارد؟
- [ ] آیا Rollback Strategy وجود دارد؟

**مرجع:** `Bugfix-Master-Contract.md` - بخش Testing

### دروازه 11: Documentation Gate (دروازه مستندسازی)
- [ ] آیا Code Comments کافی است؟
- [ ] آیا API Documentation وجود دارد؟
- [ ] آیا User Guide به‌روز است؟
- [ ] آیا Changelog به‌روز است؟

**مرجع:** `Contracts/01-PreFlight-Protocol.md`

### دروازه 12: Backward Compatibility Gate (دروازه سازگاری)
- [ ] آیا Breaking Changes وجود ندارد؟
- [ ] آیا API Contracts حفظ شده؟
- [ ] آیا Migration Path وجود دارد؟
- [ ] آیا Rollback Plan وجود دارد？

**مرجع:** `Bugfix-Master-Contract.md` - بخش Compatibility

---

## STEP 2: چک‌لیست قبل از پاسخ

### ✅ بررسی قراردادها
- [ ] `Bugfix-Master-Contract.md` بررسی شده
- [ ] `Contracts/01-PreFlight-Protocol.md` بررسی شده
- [ ] `Contracts/DEBUGGING_SPECIALIST_CONTRACT.md` بررسی شده (در صورت نیاز)
- [ ] `Contracts/MODULE_ANALYSIS_CONTRACT.md` بررسی شده (در صورت نیاز)
- [ ] `Docs/DEVELOPMENT_CONTRACT.md` بررسی شده
- [ ] `Docs/TODO_TEMPLATE.md` بررسی شده (در صورت نیاز)

### ✅ بررسی Knowledge-Base
- [ ] `Docs/Knowledge-Base/` بررسی شده
- [ ] آیا مستندات مرتبط وجود دارد؟
- [ ] آیا نمونه‌های مشابه وجود دارد؟

### ✅ بررسی پروژه
- [ ] آیا کد مشابه وجود دارد؟
- [ ] آیا Interface/Contract وجود دارد؟
- [ ] آیا Dependency ثبت شده است؟
- [ ] آیا Pattern مشابه استفاده شده؟

### ✅ بررسی امنیت
- [ ] آیا داده‌های حساس Mask می‌شوند؟
- [ ] آیا Anti-Forgery Token استفاده شده؟
- [ ] آیا Authorization بررسی شده؟
- [ ] آیا HTTPS الزامی است؟

### ✅ بررسی کیفیت
- [ ] آیا Code Duplication وجود ندارد؟
- [ ] آیا Error Handling مناسب است؟
- [ ] آیا Logging مناسب است؟
- [ ] آیا Performance مناسب است؟

---

## STEP 3: در صورت تعارض → Hard Stop

### شرایط Hard Stop:
1. **نقض AI No-Fly Zone** → توقف فوری
2. **نقض قراردادها** → توقف و اطلاع
3. **نقض معماری** → توقف و پیشنهاد جایگزین
4. **نقض امنیت** → توقف و پیشنهاد راه‌حل امن
5. **نقض استانداردها** → توقف و پیشنهاد راه‌حل استاندارد

### اقدامات Hard Stop:
```markdown
🚨 HARD STOP - تعارض شناسایی شد

**مشکل:** [توضیح مشکل]
**قرارداد نقض شده:** [نام قرارداد]
**راه‌حل پیشنهادی:** [راه‌حل جایگزین]

لطفاً تأیید کنید یا راه‌حل جایگزین پیشنهاد دهید.
```

---

## STEP 4: ساختار اجباری پاسخ

### برای Bugfix:
```markdown
# 🐛 Bugfix Report - [Module Name]

## 1. Executive Summary
- **مشکل:** [توضیح کوتاه]
- **ریشه:** [علت ریشه‌ای]
- **راه‌حل:** [راه‌حل اعمال شده]

## 2. Evidence (شواهد)
- **File:** [مسیر فایل]
- **Line:** [شماره خط]
- **Error:** [خطای کامل]

## 3. Root Cause Analysis
- **چرا رخ داد:** [توضیح]
- **مدرک:** [شواهد]

## 4. Solution Applied
- **تغییرات:** [تغییرات اعمال شده]
- **دلیل:** [چرا این راه‌حل]

## 5. Testing
- [ ] Build موفق
- [ ] Manual Test موفق
- [ ] Regression Test موفق

## 6. Rollback Strategy
- [گام‌های Rollback]

## 7. TODO for PROD
- [اقدامات لازم برای Production]
```

### برای Module Analysis:
```markdown
# 📊 Module Analysis - [Module Name]

## 1. Module Overview
- **مسئولیت:** [توضیح]
- **وابستگی‌ها:** [لیست]
- **Interfaces:** [لیست]

## 2. Architecture Analysis
- **Patterns:** [الگوهای استفاده شده]
- **Quality:** [ارزیابی کیفیت]
- **Issues:** [مشکلات شناسایی شده]

## 3. Recommendations
- [پیشنهادات بهبود]

## 4. Implementation Plan
- [نقشه راه پیاده‌سازی]
```

### برای Feature Development:
```markdown
# 🚀 Feature Development - [Feature Name]

## 1. Requirements Analysis
- **User Stories:** [لیست]
- **Acceptance Criteria:** [معیارها]

## 2. Design
- **Entities:** [لیست]
- **ViewModels:** [لیست]
- **Interfaces:** [لیست]

## 3. Implementation
- **Phase 1:** [مرحله 1]
- **Phase 2:** [مرحله 2]
- ...

## 4. Testing
- [تست‌های انجام شده]

## 5. Documentation
- [مستندات ایجاد شده]
```

---

## STEP 5: مراجع سریع

### قراردادهای اصلی:
- `Bugfix-Master-Contract.md` - قرارداد اصلی Bugfix
- `Contracts/01-PreFlight-Protocol.md` - پروتکل پیش‌پرواز
- `Contracts/02-Architecture-Guidelines.md` - راهنمای معماری
- `Contracts/DEBUGGING_SPECIALIST_CONTRACT.md` - قرارداد دیباگر
- `Contracts/MODULE_ANALYSIS_CONTRACT.md` - قرارداد تحلیل ماژول
- `Docs/DEVELOPMENT_CONTRACT.md` - قرارداد توسعه
- `Docs/TODO_TEMPLATE.md` - قالب TODO

### Knowledge-Base:
- `Docs/Knowledge-Base/` - بایگاه دانش
- `Docs/Knowledge-Base/AI/` - مستندات AI
- `Docs/Knowledge-Base/Master/` - مستندات اصلی

### Database:
- `Docs/Database-Connection-Guide.md` - راهنمای اتصال دیتابیس

---

## STEP 6: استفاده از Template

### برای TODO List:
از `Docs/TODO_TEMPLATE.md` استفاده کن:
- Phase 1: Analysis & Design
- Phase 2: Backend Implementation
- Phase 3: Controller Implementation
- Phase 4: View Implementation
- Phase 5: UI/UX Optimization
- ...

### برای Bugfix:
از `Bugfix-Master-Contract.md` استفاده کن:
- A) کشف شواهد
- B) تشخیص ریشه‌ای
- C) گزینه‌های رفع
- D) Patch اتمیک
- E) تأیید دستی
- F) گزارش خروجی

### برای Module Analysis:
از `Contracts/MODULE_ANALYSIS_CONTRACT.md` استفاده کن:
- تحلیل ساختاری
- تحلیل وابستگی‌ها
- تحلیل عملکرد
- تحلیل کیفیت
- پیشنهادات بهبود

---

## STEP 7: چک‌لیست نهایی قبل از ارسال

### ✅ بررسی نهایی:
- [ ] STEP 0 (AI Guard Check) انجام شده
- [ ] STEP 1 (12 دروازه امنیتی) بررسی شده
- [ ] STEP 2 (چک‌لیست قبل از پاسخ) بررسی شده
- [ ] STEP 3 (Hard Stop) بررسی شده (در صورت نیاز)
- [ ] STEP 4 (ساختار اجباری) رعایت شده
- [ ] STEP 5 (مراجع) بررسی شده
- [ ] STEP 6 (Template) استفاده شده

### ✅ بررسی کیفیت:
- [ ] پاسخ کامل و جامع است
- [ ] شواهد کافی ارائه شده
- [ ] راه‌حل منطقی و امن است
- [ ] مستندات به‌روز است
- [ ] تست‌ها انجام شده

---

## 🚨 Hard Stop Checklist

**در صورت مشاهده هر یک از موارد زیر → Hard Stop:**

- [ ] نقض AI No-Fly Zone
- [ ] نقض قراردادها
- [ ] نقض معماری
- [ ] نقض امنیت
- [ ] نقض استانداردها
- [ ] Breaking Changes بدون تأیید
- [ ] حذف کد بدون Legacy/Obsolete
- [ ] تغییر Silent (بی‌صدا)
- [ ] تولید کد بدون شواهد
- [ ] پیشنهاد Library ناسازگار

---

## 📝 یادداشت‌های استفاده

### هر بار که کاربر درخواست می‌دهد:
1. **STEP 0 را اجرا کن** - AI Guard Check
2. **STEP 1 را بررسی کن** - 12 دروازه امنیتی
3. **STEP 2 را بررسی کن** - چک‌لیست قبل از پاسخ
4. **در صورت تعارض** - STEP 3 (Hard Stop)
5. **پاسخ را طبق STEP 4 بساز** - ساختار اجباری
6. **از STEP 5 استفاده کن** - مراجع سریع
7. **از STEP 6 استفاده کن** - Template
8. **STEP 7 را بررسی کن** - چک‌لیست نهایی

### نکات مهم:
- **این فایل اجباری است** - قبل از هر پاسخ باید بررسی شود
- **Hard Stop جدی است** - در صورت تعارض توقف کن
- **شواهد مهم است** - هر ادعا باید مدرک داشته باشد
- **قراردادها بالاتر از همه** - قراردادها را رعایت کن

---

## ✅ تأیید و آمادگی

**این فایل پیش‌پرواز آماده استفاده است.**

**هر بار که کاربر درخواست می‌دهد، این فایل را بررسی کن و طبق مراحل عمل کن.**

**در صورت تعارض → Hard Stop و اطلاع به کاربر**

---

**نسخه:** 1.0.0  
**تاریخ:** 2025-01-27  
**وضعیت:** فعال و اجباری

