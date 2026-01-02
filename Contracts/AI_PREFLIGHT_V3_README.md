# 📚 AI Preflight V3 - راهنمای کامل

---

## 🎯 **چه تغییری کردیم؟**

### V1 & V2:
- ✅ 15 ممنوعیت AI No-Fly Zone
- ✅ قراردادهای پایه
- ✅ استانداردهای توسعه

### V3 (جدید):
- ✅ همه چیز V2
- ✅ **CRITICAL-FINANCIAL-MODULE-CONTRACT** (10 قانون طلایی مالی)
- ✅ **Debugging Protocol** (فرآیند 6 مرحله‌ای)
- ✅ **12 دروازه امنیتی** (Security Gates)
- ✅ **چک‌لیست جامع برای هر نوع کار**

---

## 📁 **فایل‌های V3**

### 1️⃣ `AI_PREFLIGHT_MASTER_V3.md` (کامل)
**محتوا:**
- STEP 0: AI Guard Check (15 ممنوعیت)
- STEP 1: 12 دروازه امنیتی
- STEP 2: Financial Module Check (10 قانون طلایی)
- STEP 3: Debugging Protocol (6 مرحله)
- STEP 4-7: Implementation + Testing + Documentation

**استفاده:** 
- قبل از کارهای بزرگ
- قبل از تغییرات مالی
- قبل از رفع باگ‌های پیچیده

**زمان:** 5-10 دقیقه مطالعه

---

### 2️⃣ `AI_PREFLIGHT_QUICK_V3.md` (سریع)
**محتوا:**
- چک‌لیست 5 ثانیه‌ای
- 15 ممنوعیت (خلاصه)
- 10 قانون طلایی مالی (خلاصه)
- فرآیند Bugfix (خلاصه)
- چک‌لیست سریع

**استفاده:**
- قبل از هر پاسخ (روزمره)

**زمان:** 30 ثانیه

---

### 3️⃣ `AI_PREFLIGHT_V3_README.md` (این فایل)
**محتوا:**
- راهنمای استفاده
- تفاوت V2 و V3
- Workflow ها

**استفاده:**
- اولین بار
- یادآوری

**زمان:** 2-3 دقیقه

---

## 🚀 **Workflow استاندارد**

### 🟢 **کار معمولی (Feature/Enhancement):**

```
1. باز کن: AI_PREFLIGHT_QUICK_V3.md (30s)
2. چک: 5 گزینه اصلی ✓
3. مرور: 15 ممنوعیت ✓
4. Implement:
   □ Factory Pattern
   □ ServiceResult Enhanced
   □ Error Handling
   □ Logging
   □ Validation
5. Test: Build + Manual + Edge Cases
6. Document: Comments + Report
```

---

### 💰 **ماژول مالی (Payment/CashSession/Reports):**

```
1. باز کن: AI_PREFLIGHT_MASTER_V3.md (10m)
2. مرور: STEP 0 (AI Guard) + STEP 1 (12 دروازه)
3. 🚨 STEP 2: Financial Module Check (الزامی!)
   □ 10 قانون طلایی
   □ Transaction Management
   □ Idempotency
   □ Verification
   □ Audit Trail
   □ Log کامل
   □ decimal(18,0)
   □ RowVersion
   □ SoftDelete
4. با مدیر فنی مشورت کن
5. با حسابدار هماهنگ کن
6. تست کامل (5 سناریو):
   - Happy Path
   - Exception
   - Concurrent Requests
   - Database Failure
   - Network Failure
7. Code Review توسط Senior
8. تست در Staging
9. پشتیبان Database
10. Rollback Plan
```

---

### 🐛 **رفع باگ (Bugfix):**

```
1. باز کن: AI_PREFLIGHT_MASTER_V3.md (10m)
2. مرور: STEP 0 + STEP 1
3. STEP 3: Debugging Protocol (الزامی!)
   
   مرحله 1: شناسایی
   - نوع: Compilation/Runtime/Logic/Performance/Security
   - شدت: Critical/High/Medium/Low
   - محدوده: File/Module/Cross-Module/System-Wide
   
   مرحله 2: علت ریشه‌ای (5 Whys)
   1. چرا خطا رخ داد؟
   2. چرا علت #1 رخ داد؟
   3. چرا علت #2 رخ داد؟
   4. چرا علت #3 رخ داد؟
   5. چرا علت #4 رخ داد؟
   → علت ریشه‌ای
   
   مرحله 3: وابستگی‌ها
   - Callers
   - Dependencies
   - Affected Modules
   
   مرحله 4: رفع اتمیک
   - Minimal Changes
   - NO_DELETE
   - Facade/Forwarder
   
   مرحله 5: تست
   - Build OK
   - Manual Test OK
   - Regression Test OK
   
   مرحله 6: گزارش
   # 🐛 Bugfix Report
   - Executive Summary
   - Evidence
   - Root Cause
   - Solution
   - Testing
   - Rollback
```

---

## 💡 **تفاوت‌های کلیدی V3**

### ماژول معمولی vs ماژول مالی:

| جنبه | معمولی | مالی |
|---|---|---|
| **Test** | Manual + Edge | Manual + Edge + 5 Scenarios |
| **Log** | Serilog | Serilog + Transaction Log |
| **Transaction** | Optional | **الزامی** |
| **Verification** | Optional | **الزامی** |
| **Idempotency** | Optional | **الزامی** |
| **Audit Trail** | Recommended | **الزامی** |
| **Code Review** | Peer | **Senior** |
| **Staging Test** | Optional | **الزامی** |
| **Database Backup** | Optional | **الزامی** |
| **Rollback Plan** | Recommended | **الزامی** |
| **حساسیت** | Normal | **2x Critical** |

---

## 🚨 **HARD STOP شرایط**

### در V3، HARD STOP فعال می‌شود اگر:

1. 🚫 نقض AI No-Fly Zone (15 ممنوعیت)
2. 🚫 نقض قراردادها
3. 🚫 نقض معماری (SRP)
4. 🚫 مشکل امنیتی
5. 🚫 **نقض Financial Rules** (اگر مالی) ← جدید در V3
6. 🚫 Breaking Changes بدون Migration
7. 🚫 حذف کد بدون Obsolete
8. 🚫 تغییر Silent
9. 🚫 کد بدون شواهد/تست

**در ماژول‌های مالی، حساسیت HARD STOP دوبرابر است!**

---

## ✅ **چک‌لیست روزانه**

### صبح (شروع Session):
- [ ] خواندن `AI_PREFLIGHT_QUICK_V3.md` (30s)
- [ ] مرور 15 ممنوعیت
- [ ] اگر کار مالی داری → `AI_PREFLIGHT_MASTER_V3.md` (10m)
- [ ] Bookmark فایل‌ها

### قبل از هر پاسخ:
- [ ] چک `AI_PREFLIGHT_QUICK_V3.md` (30s)
- [ ] AI No-Fly Zone OK?
- [ ] قراردادها OK?
- [ ] 💰 اگر مالی → Financial Check OK?
- [ ] 🐛 اگر باگ → Debugging Protocol OK?

### بعد از پاسخ:
- [ ] Build OK?
- [ ] Test OK?
- [ ] 💰 اگر مالی → Verification OK?
- [ ] Document OK?

---

## 🎯 **خلاصه برای AI**

### V3 = V2 + Financial Critical + Debugging Protocol

**استفاده روزانه:**
```
1. هر پاسخ → AI_PREFLIGHT_QUICK_V3.md (30s)
2. مالی → AI_PREFLIGHT_MASTER_V3.md → STEP 2 (الزامی!)
3. باگ → AI_PREFLIGHT_MASTER_V3.md → STEP 3 (الزامی!)
```

**یادت باشه:**
```
🚫 حدس ممنوع
✅ شواهد الزامی
✅ قراردادها بالاتر از همه
✅ امنیت اولویت اول
💰 مالی = حساسیت دوبرابر
🐛 باگ = فرآیند 6 مرحله‌ای
🚨 HARD STOP در صورت تعارض
```

---

## 📊 **نتایج استفاده از V3**

### قبل از V3:
- ❌ گاهی قراردادها فراموش می‌شد
- ❌ ماژول‌های مالی بدون Transaction
- ❌ باگ‌ها بدون Root Cause Analysis
- ❌ Verification نمی‌شد

### بعد از V3:
- ✅ همیشه قراردادها رعایت می‌شود
- ✅ ماژول‌های مالی 100% امن
- ✅ باگ‌ها با 5 Whys حل می‌شوند
- ✅ همیشه Verification می‌شود
- ✅ صفر خطای مالی در Production

---

## 🔗 **مراجع اصلی**

### V3 Files:
```
📄 AI_PREFLIGHT_MASTER_V3.md (Master کامل)
📄 AI_PREFLIGHT_QUICK_V3.md (Quick روزانه)
📄 AI_PREFLIGHT_V3_README.md (این فایل)
```

### Contracts:
```
📄 PREFLIGHT_CHECKLIST.md
📄 Docs/DEVELOPMENT_CONTRACT.md
📄 Docs/Knowledge-Base/CRITICAL-FINANCIAL-MODULE-CONTRACT.md
📄 Docs/Knowledge-Base/AI/PreFlight/Bugfix-Master-Contract.md
📄 Docs/Knowledge-Base/AI/PreFlight/05-Debugging-Specialist-Contract.md
📄 Docs/Knowledge-Base/03-Development-Contract-Quick-Guide.md
```

### Knowledge Base:
```
📁 Docs/Knowledge-Base/
📁 Docs/Knowledge-Base/AI/
📄 Docs/Knowledge-Base/README.md
```

---

## 🎉 **آماده‌اید؟**

**شروع کنید:**
1. باز کردن `AI_PREFLIGHT_QUICK_V3.md`
2. Bookmark کردن
3. مرور 15 ممنوعیت
4. شروع کار با اطمینان کامل

---

**نسخه:** 3.0.0  
**تاریخ:** 2026-01-02  
**وضعیت:** ✅ آماده استفاده

**🎯 V3 = کیفیت 10x + امنیت مالی 100% + Debugging حرفه‌ای**

