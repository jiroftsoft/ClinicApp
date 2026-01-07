# 📋 راهنمای گام‌به‌گام تست پرداخت

**تاریخ:** 2026-01-06  
**هدف:** تست پرداخت با Merchant ID واقعی `mehranyad.ir` در Development

---

## 🎯 مراحل (5 دقیقه)

### ✅ STEP 1: دریافت Merchant ID Sandbox

1. **وارد پنل ZarinPal شوید:**
   - لینک: **https://next.zarinpal.com/**
   - وارد حساب کاربری خود شوید

2. **به بخش Sandbox بروید:**
   - در منوی سمت راست، **"Sandbox"** را انتخاب کنید
   - یا مستقیماً: **https://next.zarinpal.com/sandbox**

3. **Merchant ID Sandbox را کپی کنید:**
   - فرمت: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
   - این Merchant ID رایگان است و فقط برای تست استفاده می‌شود

---

### ✅ STEP 2: اجرای SQL Script

**فایل:** `Scripts/sql/Create_Test_Gateway_Sandbox.sql`

1. **فایل را باز کنید:**
   ```
   Scripts/sql/Create_Test_Gateway_Sandbox.sql
   ```

2. **Merchant ID Sandbox را جایگزین کنید:**
   - در خط **56** و **57**، `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` را با Merchant ID Sandbox خود جایگزین کنید
   
   ```sql
   -- قبل:
   N'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx',
   
   -- بعد (مثال):
   N'12345678-1234-1234-1234-123456789012',
   ```

3. **Script را در SQL Server Management Studio اجرا کنید:**
   - فایل را باز کنید
   - Merchant ID را جایگزین کنید
   - `F5` بزنید یا دکمه **Execute** را کلیک کنید

4. **بررسی نتیجه:**
   - باید پیام **"✅ Gateway Sandbox ایجاد شد!"** را ببینید
   - اگر پیام **"⚠️ Gateway Sandbox از قبل وجود دارد"** را دیدید، یعنی Gateway قبلاً ایجاد شده است

---

### ✅ STEP 3: فعال‌سازی Gateway Sandbox

**فایل:** `Scripts/sql/Activate_Sandbox_Gateway.sql`

1. **فایل را در SQL Server Management Studio باز کنید**
2. **Execute کنید** (`F5`)
3. **بررسی کنید:**
   - Gateway Sandbox: `IsActive = 1`, `IsDefault = 1`
   - Gateway Production: `IsActive = 0`, `IsDefault = 0`

---

### ✅ STEP 4: Restart Application

1. **Application را Stop کنید** (اگر در حال اجرا است)
2. **Application را Start کنید**
3. **بررسی کنید که Application بدون خطا شروع می‌شود**

---

### ✅ STEP 5: تست پرداخت

1. **ایجاد نوبت:**
   - وارد بخش Patient شوید
   - یک نوبت ایجاد کنید

2. **تلاش برای پرداخت:**
   - روی دکمه پرداخت کلیک کنید
   - باید به صفحه ZarinPal Sandbox هدایت شوید

3. **تست پرداخت:**
   - در صفحه ZarinPal Sandbox، می‌توانید با کارت تست پرداخت کنید
   - یا می‌توانید پرداخت را Cancel کنید

4. **بررسی لاگ‌ها:**
   ```
   ✅ WEB PAYMENT: درگاه پیش‌فرض یافت شد
   ✅ ZarinPal: درخواست پرداخت موفق
   ✅ ZarinPal: Authority دریافت شد
   ```

---

## 🔄 بازگردانی به Production

**بعد از تست، برای بازگردانی به Production:**

**فایل:** `Scripts/sql/Restore_Production_Gateway.sql`

1. **فایل را در SQL Server Management Studio باز کنید**
2. **Execute کنید** (`F5`)
3. **بررسی کنید:**
   - Gateway Production: `IsActive = 1`, `IsDefault = 1`
   - Gateway Sandbox: `IsActive = 0`, `IsDefault = 0`

---

## ⚠️ نکات مهم

### 1. Callback URL در Sandbox

**در پنل ZarinPal Sandbox، Callback URL را ثبت کنید:**

- **Development:** `http://localhost:3560/Patient/AppointmentBooking/PaymentCallback`
- **Production:** `https://mehranyad.ir/Patient/AppointmentBooking/PaymentCallback`

**مراحل:**
1. وارد پنل ZarinPal Sandbox شوید
2. به بخش **Settings** → **Callback URL** بروید
3. Callback URL را وارد کنید
4. **Save** کنید

### 2. PaymentBaseUrl در Web.config

**برای Development:**
```xml
<add key="Payment:BaseUrl" value="http://localhost:3560"/>
```

**برای Production:**
```xml
<add key="Payment:BaseUrl" value="https://mehranyad.ir"/>
```

### 3. قبل از Deploy به Production

**حتماً Script `Restore_Production_Gateway.sql` را اجرا کنید!**

---

## 📋 چک‌لیست

### قبل از تست:

- [ ] ✅ Merchant ID Sandbox دریافت شده است
- [ ] ✅ SQL Script `Create_Test_Gateway_Sandbox.sql` اجرا شده است
- [ ] ✅ SQL Script `Activate_Sandbox_Gateway.sql` اجرا شده است
- [ ] ✅ Gateway Sandbox در Database وجود دارد (`IsActive = 1`, `IsDefault = 1`)
- [ ] ✅ Callback URL در پنل ZarinPal Sandbox ثبت شده است
- [ ] ✅ `Payment:BaseUrl` در `Web.config` تنظیم شده است
- [ ] ✅ Application Restart شده است

### بعد از تست:

- [ ] ✅ تست پرداخت انجام شده است
- [ ] ✅ لاگ‌ها بررسی شده است
- [ ] ✅ SQL Script `Restore_Production_Gateway.sql` اجرا شده است (برای Production)

---

## 🔗 فایل‌های SQL

1. **`Scripts/sql/Create_Test_Gateway_Sandbox.sql`** - ایجاد Gateway Sandbox
2. **`Scripts/sql/Activate_Sandbox_Gateway.sql`** - فعال‌سازی Gateway Sandbox
3. **`Scripts/sql/Restore_Production_Gateway.sql`** - بازگردانی Gateway Production

---

## 🆘 عیب‌یابی

### مشکل: "Gateway Sandbox یافت نشد"

**راه‌حل:**
1. ابتدا `Create_Test_Gateway_Sandbox.sql` را اجرا کنید
2. سپس `Activate_Sandbox_Gateway.sql` را اجرا کنید

### مشکل: "خطا در ایجاد درخواست پرداخت در درگاه"

**راه‌حل:**
1. بررسی کنید که Gateway Sandbox `IsActive = 1` است
2. بررسی کنید که `MerchantId` درست است
3. بررسی کنید که Callback URL در پنل ZarinPal Sandbox ثبت شده است
4. لاگ‌ها را بررسی کنید

---

**نکته:** برای Development، **Sandbox** توصیه می‌شود. برای Production Testing، از Gateway Production با احتیاط استفاده کنید.

