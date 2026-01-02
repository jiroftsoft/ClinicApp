# 📚 AI Preflight - فهرست کامل

**آخرین به‌روزرسانی:** 2026-01-02

---

## 🎯 **کدام فایل را استفاده کنم؟**

### ⚡ **استفاده روزانه (توصیه می‌شود):**

| سناریو | فایل | زمان |
|---|---|---|
| 📝 هر پاسخ | `AI_PREFLIGHT_QUICK_V3.md` | 30s |
| 💰 کار مالی | `AI_PREFLIGHT_MASTER_V3.md` → STEP 2 | 10m |
| 🐛 رفع باگ | `AI_PREFLIGHT_MASTER_V3.md` → STEP 3 | بستگی به پیچیدگی |
| 🚀 Feature بزرگ | `AI_PREFLIGHT_MASTER_V3.md` | 10m |
| 📖 راهنما | `AI_PREFLIGHT_V3_README.md` | 3m |

---

## 📁 **فهرست فایل‌ها**

### ✅ **V3 (جدید - توصیه می‌شود)**

#### 1. `AI_PREFLIGHT_MASTER_V3.md` (Master کامل)
**محتوا:**
- STEP 0: AI Guard Check (15 ممنوعیت)
- STEP 1: 12 دروازه امنیتی
- STEP 2: Financial Module Check (10 قانون طلایی) ← **جدید**
- STEP 3: Debugging Protocol (6 مرحله) ← **جدید**
- STEP 4-7: Implementation + Testing + Documentation

**استفاده برای:**
- کارهای بزرگ
- ماژول‌های مالی (الزامی)
- رفع باگ‌های پیچیده (الزامی)

**قراردادهای ادغام شده:**
1. ✅ PREFLIGHT_CHECKLIST.md
2. ✅ CRITICAL-FINANCIAL-MODULE-CONTRACT.md
3. ✅ 03-Development-Contract-Quick-Guide.md
4. ✅ 05-Debugging-Specialist-Contract.md

**زمان:** 5-10 دقیقه

---

#### 2. `AI_PREFLIGHT_QUICK_V3.md` (Quick روزانه)
**محتوا:**
- چک‌لیست 5 ثانیه‌ای
- 15 ممنوعیت (خلاصه)
- 10 قانون طلایی مالی (خلاصه) ← **جدید**
- فرآیند Bugfix 6 مرحله‌ای (خلاصه) ← **جدید**
- چک‌لیست سریع

**استفاده برای:**
- قبل از هر پاسخ (روزمره)
- یادآوری سریع

**زمان:** 30 ثانیه

---

#### 3. `AI_PREFLIGHT_V3_README.md` (راهنما)
**محتوا:**
- راهنمای استفاده از V3
- تفاوت V2 و V3
- Workflow ها
- مثال‌های عملی

**استفاده برای:**
- اولین بار
- یادآوری Workflow ها
- درک تفاوت‌ها

**زمان:** 3 دقیقه

---

#### 4. `AI_PREFLIGHT_V3_SUMMARY.md` (خلاصه)
**محتوا:**
- خلاصه تغییرات V3
- نتایج
- دستاوردها

**استفاده برای:**
- مرور کلی
- گزارش به تیم

**زمان:** 2 دقیقه

---

### 📄 **V2 (قبلی - برای مراجعه)**

#### 1. `AI_PREFLIGHT_MASTER.md` (V2)
- قراردادهای پایه
- بدون Financial Critical
- بدون Debugging Protocol
- **نگه داشته شده برای مراجعه**

#### 2. `AI_PREFLIGHT_QUICK.md` (V2)
- نسخه ساده‌تر
- بدون Financial Check
- **نگه داشته شده برای مراجعه**

#### 3. `AI_PREFLIGHT_README.md` (V2)
- راهنمای V2
- **نگه داشته شده برای مراجعه**

---

### 📋 **فایل‌های مرجع (Contract Files)**

این فایل‌ها منبع اصلی هستند:

```
📄 PREFLIGHT_CHECKLIST.md
   └─ 15 ممنوعیت + 12 دروازه امنیتی

📄 Docs/DEVELOPMENT_CONTRACT.md
   └─ استانداردهای توسعه کامل

📄 Docs/Knowledge-Base/CRITICAL-FINANCIAL-MODULE-CONTRACT.md
   └─ 10 قانون طلایی مالی

📄 Docs/Knowledge-Base/AI/PreFlight/Bugfix-Master-Contract.md
   └─ فرآیند رفع باگ

📄 Docs/Knowledge-Base/AI/PreFlight/05-Debugging-Specialist-Contract.md
   └─ فرآیند دیباگ 6 مرحله‌ای

📄 Docs/Knowledge-Base/03-Development-Contract-Quick-Guide.md
   └─ راهنمای سریع قراردادها
```

---

## 🚀 **Workflow: چطور استفاده کنم؟**

### 🟢 **Scenario 1: کار معمولی**

```
1. صبح → باز کن: AI_PREFLIGHT_QUICK_V3.md (2m)
2. مرور: 15 ممنوعیت + چک‌لیست

3. قبل از هر پاسخ → چک: AI_PREFLIGHT_QUICK_V3.md (30s)
   - AI No-Fly Zone? ✓
   - قراردادها? ✓
   - معماری? ✓
   - Security? ✓
   - Standards? ✓

4. Implement:
   - Factory Pattern
   - ServiceResult Enhanced
   - Error Handling
   - Logging
   - Validation

5. Test + Document
```

---

### 💰 **Scenario 2: ماژول مالی**

```
1. باز کن: AI_PREFLIGHT_MASTER_V3.md (10m)
2. مرور: STEP 0 + STEP 1
3. 🚨 STEP 2 الزامی:
   □ 10 قانون طلایی
   □ Transaction Management
   □ Idempotency
   □ Verification
   □ Audit Trail
   □ Log کامل
   □ decimal(18,0)
   □ RowVersion
   □ SoftDelete

4. مشورت:
   - مدیر فنی
   - حسابدار

5. تست کامل (5 سناریو):
   - Happy Path
   - Exception
   - Concurrent
   - DB Failure
   - Network Failure

6. Code Review Senior
7. Test Staging
8. Backup Database
9. Rollback Plan
10. Deploy
```

---

### 🐛 **Scenario 3: رفع باگ**

```
1. باز کن: AI_PREFLIGHT_MASTER_V3.md (10m)
2. مرور: STEP 0 + STEP 1
3. 🔧 STEP 3 الزامی:
   
   مرحله 1: شناسایی
   - نوع/شدت/محدوده
   
   مرحله 2: 5 Whys
   - علت ریشه‌ای
   
   مرحله 3: وابستگی‌ها
   - Callers/Dependencies
   
   مرحله 4: رفع اتمیک
   - Minimal Changes
   - NO_DELETE
   
   مرحله 5: تست
   - Build + Manual + Regression
   
   مرحله 6: گزارش
   - Evidence + Solution + Rollback
```

---

## 📊 **مقایسه V2 vs V3**

| ویژگی | V2 | V3 |
|---|---|---|
| 15 ممنوعیت | ✅ | ✅ |
| قراردادهای پایه | ✅ | ✅ |
| 12 دروازه امنیتی | ✅ | ✅ |
| **Financial Critical** | ❌ | ✅ **جدید** |
| **Debugging Protocol** | ❌ | ✅ **جدید** |
| **Workflow مشخص** | محدود | ✅ **کامل** |
| تعداد قراردادها | 2 | **4** |
| حساسیت مالی | Normal | **2x** |
| **توصیه برای استفاده** | مراجعه | **روزانه** |

---

## ✅ **توصیه نهایی**

### برای استفاده روزانه:
1. **Bookmark کن:** `AI_PREFLIGHT_QUICK_V3.md`
2. **مطالعه کن:** `AI_PREFLIGHT_V3_README.md` (یک بار)
3. **استفاده روزانه:** `AI_PREFLIGHT_QUICK_V3.md` (30s قبل از هر پاسخ)
4. **مالی/باگ:** `AI_PREFLIGHT_MASTER_V3.md` (10m)

### فایل‌های V2:
- نگه داشته شده برای مراجعه
- V3 جایگزین روزانه است

---

## 🎯 **خلاصه**

```
روزانه → AI_PREFLIGHT_QUICK_V3.md (30s)
مالی → AI_PREFLIGHT_MASTER_V3.md → STEP 2
باگ → AI_PREFLIGHT_MASTER_V3.md → STEP 3
راهنما → AI_PREFLIGHT_V3_README.md
این Index → مرجع سریع
```

---

**نسخه Index:** 1.0.0  
**تاریخ:** 2026-01-02  
**تعداد فایل‌ها:**
- V3: 4 فایل (جدید)
- V2: 3 فایل (مراجعه)
- Contracts: 6 فایل (منبع)
- **جمع:** 13 فایل

**🎉 همه چیز آماده است! از V3 استفاده کن.**

