# 🎉 قرارداد جدید اضافه شد!

**تاریخ:** 1404/10/13  
**نام قرارداد:** `CRITICAL_MODULE_SAFETY_CONTRACT.md`

---

## ✅ **چه چیزی اضافه شد؟**

### 📄 `Contracts/CRITICAL_MODULE_SAFETY_CONTRACT.md`

یک قرارداد **فوق‌العاده مهم** برای جلوگیری از تغییرات خطرناک روی ماژول‌های حیاتی.

---

## 🎯 **هدف:**

```
🚫 هیچ تغییری روی ماژول‌های حیاتی بدون:
   1. درک کامل منطق سیستم
   2. سوال و تأیید از کاربر
   3. بررسی تأثیرات جانبی
```

---

## 🔴 **ماژول‌های حیاتی (CRITICAL):**

| ماژول | مثال |
|-------|------|
| **Authentication** | `AspNetUsers`, Identity System |
| **Patient Management** | `Patients`, `Patient` Entity, رابطه با User |
| **Financial** | `Payments`, `Factor`, محاسبات مالی |
| **Database Relations** | Foreign Keys, EF Configurations, Migrations |

---

## ✅ **قوانین اصلی:**

### 1️⃣ **STOP & THINK**
قبل از هر تغییر حیاتی:
- آیا منطق فعلی را درک کرده‌ام؟
- آیا راه‌حل بهتری وجود دارد؟

### 2️⃣ **ASK USER**
همیشه از کاربر بپرس:
- مشکل دقیقاً چیست؟ (نمونه بخواه)
- رفتار مورد انتظار چیست؟
- چه تستی لازم است؟

### 3️⃣ **ANALYZE IMPACT**
تأثیرات را بررسی کن:
- grep/codebase_search
- Database Dependencies
- ماژول‌های تحت تأثیر

### 4️⃣ **PROPOSE SOLUTION**
راه‌حل را توضیح بده و منتظر تأیید بمان

### 5️⃣ **IMPLEMENT**
بعد از تأیید، تدریجی پیش برو

### 6️⃣ **VERIFY**
تست کامل و گزارش

---

## 🚫 **ممنوعیت‌های مطلق:**

```
❌ تغییر EF Relationships بدون درک کامل
   (مثلاً: WithRequired → WithOptional)

❌ Nullable کردن Foreign Keys بدون دلیل مشخص
   (مثلاً: ALTER COLUMN ... NULL)

❌ تغییر منطق Authentication/Authorization
   (مثلاً: GetCurrentPatientIdAsync)

❌ تغییر محاسبات مالی بدون تست کامل

❌ Bulk Update/Delete روی جداول حیاتی
```

---

## 📖 **مثال واقعی از اشتباه قبلی:**

### ❌ **اشتباه:**
```
گزارش: "CreatePatientAsync همیشه User می‌سازد - اشتباه است"

AI بدون سوال پرسیدن:
→ تغییر WithRequired → WithOptional
→ ایجاد Migration برای Nullable
→ تغییر CreatePatientAsync
→ شکستن منطق سیستم ❌
```

### ✅ **صحیح:**
```
AI باید می‌پرسید:
"چرا ایجاد User برای بیمار اشتباه است?
آیا سناریوی خاصی وجود دارد که بیمار نباید User داشته باشد?
در حال حاضر منطق این است:
- منشی بیمار را پذیرش می‌کند
- سیستم Patient + User می‌سازد
- پیامک با credentials ارسال می‌شود
- بیمار می‌تواند لاگین کند
آیا این منطق اشتباه است؟"

→ سپس متوجه می‌شد منطق فعلی درست است! ✅
```

---

## 📋 **چک‌لیست سریع:**

```
□ آیا منطق فعلی را کاملاً درک کرده‌ام?
□ آیا از کاربر سوال پرسیده‌ام?
□ آیا نمونه واقعی از مشکل دیده‌ام?
□ آیا تأثیرات جانبی را بررسی کرده‌ام?
□ آیا راه‌حل را با کاربر تأیید کرده‌ام?
□ آیا تغییرات قابل Rollback هستند?
□ آیا تست‌های لازم را شناسایی کرده‌ام?
```

**اگر جواب هر کدام "خیر" است → STOP!**

---

## 🔗 **ارتباط با سایر قراردادها:**

```
1. AI_EXECUTION_CONTRACT.md
2. CRITICAL_MODULE_SAFETY_CONTRACT.md ← ⭐ شما اینجا هستید (بالاترین اولویت)
3. AI_PREFLIGHT_MASTER_V3.md
4. CRITICAL-FINANCIAL-MODULE-CONTRACT.md
5. 05-Debugging-Specialist-Contract.md
```

---

## 📝 **فایل‌های آپدیت شده:**

| فایل | تغییر |
|------|-------|
| ✅ `Contracts/CRITICAL_MODULE_SAFETY_CONTRACT.md` | ایجاد شد |
| ✅ `Contracts/PASTE_THIS_EVERY_CHAT.md` | آپدیت شد (اضافه شدن قرارداد جدید) |
| ✅ `Contracts/AI_PREFLIGHT_INDEX.md` | آپدیت شد (اضافه شدن به لیست) |

---

## 🚀 **چگونه استفاده کنیم؟**

### برای Chat جدید:

```
🤖 AI: رعایت Contracts الزامی است:
- AI_EXECUTION_CONTRACT.md
- CRITICAL_MODULE_SAFETY_CONTRACT.md ← ⭐ جدید: NO BLIND CHANGES
- AI_PREFLIGHT_QUICK_V3.md (30s)

مالی = STEP 2 | باگ = STEP 3 | تغییر حیاتی = ASK FIRST | تعارض = HARD STOP
```

این متن را از `Contracts/PASTE_THIS_EVERY_CHAT.md` کپی کن.

---

## 💡 **یادآوری:**

```
🚨 "بهتر است 10 بار سوال بپرسی
    تا یک بار سیستم را خراب کنی"
    
✅ همیشه:
   - بپرس
   - بررسی کن
   - تأیید بگیر
   - تدریجی پیش برو
   
❌ هرگز:
   - حدس نزن
   - فرض نکن
   - عجله نکن
   - تغییرات گسترده ندهی
```

---

## ✅ **خلاصه:**

1. ✅ قرارداد جدید `CRITICAL_MODULE_SAFETY_CONTRACT.md` اضافه شد
2. ✅ تمام فایل‌های مرتبط آپدیت شدند
3. ✅ از این به بعد، AI قبل از هر تغییر حیاتی **سوال می‌پرسد**
4. ✅ این قرارداد **بالاترین اولویت** را دارد

---

**🎉 حالا سیستم ایمن‌تر از قبل است!**

**📖 برای جزئیات کامل:** `Contracts/CRITICAL_MODULE_SAFETY_CONTRACT.md`

---

> "Better safe than sorry - بهتر است محتاط باشیم تا پشیمان"

