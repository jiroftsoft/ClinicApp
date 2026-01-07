# 🔧 راهنمای تنظیم درگاه پرداخت برای Production

**تاریخ:** 2026-01-06  
**هدف:** راهنمای تغییر از Sandbox به Production برای درگاه پرداخت زرین‌پال

---

## ⚠️ مشکل فعلی

خطای "درگاه پرداخت پیش‌فرض یافت نشد" به این دلایل ممکن است رخ دهد:

1. **درگاه در دیتابیس وجود ندارد** → سیستم به صورت خودکار از `Web.config` ایجاد می‌کند
2. **درگاه موجود است اما `IsActive = false`** → سیستم آن را فعال می‌کند
3. **درگاه موجود است اما `CallbackUrl` خالی است** → سیستم آن را تنظیم می‌کند
4. **خطای Validation هنگام ایجاد درگاه** → باید لاگ‌ها را بررسی کنید

---

## 🔧 تنظیمات Web.config برای Production

### 1. تغییر `IsSandbox` به `false`

```xml
<!-- ❌ قبل (Sandbox) -->
<add key="Zarinpal:IsSandbox" value="true"/>

<!-- ✅ بعد (Production) -->
<add key="Zarinpal:IsSandbox" value="false"/>
```

### 2. تنظیم URL های Production (اختیاری - اگر خالی باشند، به صورت خودکار تنظیم می‌شوند)

```xml
<!-- ✅ URL های Production -->
<add key="Zarinpal:RequestUrl" value="https://api.zarinpal.com/pg/v4/payment/request.json"/>
<add key="Zarinpal:VerifyUrl" value="https://api.zarinpal.com/pg/v4/payment/verify.json"/>
<add key="Zarinpal:StartPayUrl" value="https://www.zarinpal.com/pg/StartPay/"/>
<add key="Zarinpal:StatusUrl" value="https://api.zarinpal.com/pg/v4/payment/status.json"/>
```

### 3. تنظیم Merchant ID واقعی

```xml
<!-- ✅ Merchant ID واقعی از پنل زرین‌پال -->
<add key="ZarinpalMerchantId" value="YOUR_REAL_MERCHANT_ID"/>
```

---

## 📋 مراحل تنظیم

### مرحله 1: تغییر Web.config

1. فایل `Web.config` را باز کنید
2. `Zarinpal:IsSandbox` را از `true` به `false` تغییر دهید
3. `ZarinpalMerchantId` را به Merchant ID واقعی تغییر دهید (اگر تغییر نکرده‌اید)
4. Application را Restart کنید

### مرحله 2: بررسی دیتابیس

```sql
-- بررسی درگاه‌های موجود
SELECT 
    PaymentGatewayId,
    Name,
    GatewayType,
    MerchantId,
    IsActive,
    IsDefault,
    CallbackUrl,
    IsDeleted,
    CreatedAt
FROM PaymentGateways
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;
```

### مرحله 3: بررسی لاگ‌ها

بعد از Restart، لاگ‌های زیر را بررسی کنید:

```
🔍 WEB PAYMENT: شروع جستجوی درگاه پرداخت پیش‌فرض...
🔍 WEB PAYMENT: STEP 1 - تعداد درگاه‌های پیش‌فرض: ...
🔍 WEB PAYMENT: STEP 2 - تعداد درگاه‌های ZarinPal: ...
🔍 WEB PAYMENT: STEP 3 - تعداد درگاه‌های فعال: ...
🔍 WEB PAYMENT: STEP 4 - MerchantId از Web.config: ...
```

---

## 🔍 عیب‌یابی

### مشکل 1: درگاه پیدا نمی‌شود

**علت:** درگاه در دیتابیس وجود ندارد یا `IsDeleted = true`

**راه‌حل:**
1. بررسی کنید که `ZarinpalMerchantId` در `Web.config` تنظیم شده است
2. Application را Restart کنید
3. سیستم به صورت خودکار درگاه را ایجاد می‌کند

### مشکل 2: خطای Validation

**علت:** یکی از فیلدهای الزامی خالی است

**راه‌حل:**
1. لاگ‌های Validation را بررسی کنید:
   ```
   ❌ PAYMENT GATEWAY REPO: Validation Error - Property: ..., Error: ...
   ```
2. مطمئن شوید که `CallbackUrl` تنظیم شده است (به صورت خودکار تنظیم می‌شود)
3. اگر مشکل ادامه داشت، درگاه را به صورت دستی از پنل مدیریت ایجاد کنید

### مشکل 3: درگاه موجود است اما فعال نیست

**علت:** `IsActive = false` یا `IsDefault = false`

**راه‌حل:**
1. سیستم به صورت خودکار درگاه را فعال می‌کند
2. یا به صورت دستی:
   ```sql
   UPDATE PaymentGateways
   SET IsActive = 1, IsDefault = 1
   WHERE MerchantId = 'YOUR_MERCHANT_ID' AND IsDeleted = 0;
   ```

### مشکل 4: CallbackUrl خالی است

**علت:** درگاه موجود است اما `CallbackUrl` تنظیم نشده است

**راه‌حل:**
1. سیستم به صورت خودکار `CallbackUrl` را تنظیم می‌کند
2. یا به صورت دستی:
   ```sql
   UPDATE PaymentGateways
   SET CallbackUrl = '/Patient/AppointmentBooking/PaymentCallback'
   WHERE MerchantId = 'YOUR_MERCHANT_ID' AND IsDeleted = 0;
   ```

---

## ✅ چک‌لیست نهایی

- [ ] `Zarinpal:IsSandbox` = `false` در `Web.config`
- [ ] `ZarinpalMerchantId` = Merchant ID واقعی در `Web.config`
- [ ] Application Restart شده است
- [ ] درگاه در دیتابیس وجود دارد (`IsDeleted = 0`)
- [ ] `IsActive = 1` و `IsDefault = 1`
- [ ] `CallbackUrl` تنظیم شده است
- [ ] لاگ‌ها خطایی نشان نمی‌دهند

---

## 📊 بررسی درگاه در دیتابیس

```sql
-- بررسی کامل درگاه
SELECT 
    PaymentGatewayId,
    Name,
    GatewayType,
    MerchantId,
    IsActive,
    IsDefault,
    CallbackUrl,
    GatewayUrl,
    IsDeleted,
    CreatedAt,
    UpdatedAt
FROM PaymentGateways
WHERE MerchantId = 'YOUR_MERCHANT_ID'
ORDER BY CreatedAt DESC;
```

---

## 🎯 نتیجه

بعد از انجام این مراحل:

1. ✅ درگاه به صورت خودکار از `Web.config` ایجاد می‌شود (اگر وجود ندارد)
2. ✅ درگاه موجود فعال می‌شود (اگر غیرفعال است)
3. ✅ `CallbackUrl` تنظیم می‌شود (اگر خالی است)
4. ✅ درگاه به عنوان پیش‌فرض تنظیم می‌شود

**اگر هنوز خطا دارید:**
- لاگ‌های سرور را بررسی کنید
- دیتابیس را بررسی کنید
- مطمئن شوید که `Web.config` درست تنظیم شده است

---

**وضعیت:** ✅ آماده برای استفاده

