# 🔧 راهنمای کامل تنظیم درگاه پرداخت (Gateway Setup)

**تاریخ:** 2026-01-06  
**هدف:** راهنمای گام‌به‌گام تنظیم درگاه پرداخت زرین‌پال و سایر درگاه‌ها

---

## 📋 فهرست مطالب

1. [روش‌های تنظیم Gateway](#روش‌های-تنظیم-gateway)
2. [روش 1: تنظیم از طریق UI (Admin Panel)](#روش-1-تنظیم-از-طریق-ui-admin-panel)
3. [روش 2: تنظیم از طریق Web.config](#روش-2-تنظیم-از-طریق-webconfig)
4. [روش 3: تنظیم مستقیم در دیتابیس](#روش-3-تنظیم-مستقیم-در-دیتابیس)
5. [تنظیمات مورد نیاز](#تنظیمات-مورد-نیاز)
6. [تست و بررسی](#تست-و-بررسی)

---

## 🎯 روش‌های تنظیم Gateway

### سه روش برای تنظیم Gateway:

1. **از طریق UI (Admin Panel)** - ✅ توصیه می‌شود
2. **از طریق Web.config** - برای تنظیمات اولیه
3. **مستقیم در دیتابیس** - برای موارد خاص

---

## 📱 روش 1: تنظیم از طریق UI (Admin Panel)

### مسیر دسترسی:

```
Admin Panel → Payment → Gateway Management → Create Gateway
```

**URL:** `/Payment/Gateway/CreateGateway`

### مراحل:

#### 1. ورود به Admin Panel

- لاگین با نقش `Admin` یا `Accountant`
- رفتن به بخش `Payment` → `Gateway Management`

#### 2. ایجاد Gateway جدید

**فیلدهای مورد نیاز:**

| فیلد | توضیحات | مثال |
|------|---------|------|
| **Name** | نام درگاه | "زرین‌پال اصلی" |
| **GatewayType** | نوع درگاه | `ZarinPal` |
| **MerchantId** | شناسه مرچنت | `156be6cd-e0a4-4af8-9113-83647771376f` |
| **ApiKey** | کلید API | (معمولاً همان MerchantId است) |
| **ApiSecret** | کلید مخفی | (اختیاری) |
| **CallbackUrl** | URL بازگشت | `/Patient/AppointmentBooking/PaymentCallback` |
| **WebhookUrl** | URL Webhook | (اختیاری) |
| **IsActive** | فعال بودن | ✅ `true` |
| **IsDefault** | پیش‌فرض بودن | ✅ `true` (برای اولین Gateway) |
| **IsTestMode** | حالت تست | ❌ `false` (برای Production) |

#### 3. مثال کامل برای زرین‌پال:

```
Name: زرین‌پال Production
GatewayType: ZarinPal
MerchantId: 156be6cd-e0a4-4af8-9113-83647771376f
ApiKey: 156be6cd-e0a4-4af8-9113-83647771376f
ApiSecret: (خالی)
CallbackUrl: /Patient/AppointmentBooking/PaymentCallback
WebhookUrl: (خالی)
IsActive: ✅ true
IsDefault: ✅ true
IsTestMode: ❌ false
```

#### 4. ذخیره و بررسی

- بعد از ذخیره، Gateway در دیتابیس ثبت می‌شود
- می‌توانید Gateway را در لیست مشاهده کنید
- می‌توانید Gateway را به عنوان پیش‌فرض تنظیم کنید

---

## ⚙️ روش 2: تنظیم از طریق Web.config

### مراحل:

#### 1. تنظیم Merchant ID در Web.config

**فایل:** `Web.config`

```xml
<appSettings>
    <!-- ZarinPal Payment Gateway Configuration -->
    <add key="ZarinpalMerchantId" value="156be6cd-e0a4-4af8-9113-83647771376f"/>
    <add key="Zarinpal:IsSandbox" value="false"/>
    
    <!-- Production URLs -->
    <add key="Zarinpal:RequestUrl" value="https://api.zarinpal.com/pg/v4/payment/request.json"/>
    <add key="Zarinpal:VerifyUrl" value="https://api.zarinpal.com/pg/v4/payment/verify.json"/>
    <add key="Zarinpal:StartPayUrl" value="https://www.zarinpal.com/pg/StartPay/"/>
    <add key="Zarinpal:StatusUrl" value="https://api.zarinpal.com/pg/v4/payment/status.json"/>
</appSettings>
```

#### 2. ایجاد خودکار Gateway

**سیستم به صورت خودکار Gateway را ایجاد می‌کند اگر:**
- هیچ Gateway فعالی در دیتابیس وجود نداشته باشد
- `ZarinpalMerchantId` در `Web.config` تنظیم شده باشد

**منطق ایجاد خودکار:**
```csharp
// در GetDefaultPaymentGatewayAsync:
// STEP 4: اگر هیچ درگاهی یافت نشد، تلاش برای ایجاد خودکار از Web.config
```

**Gateway ایجاد شده:**
- `Name`: "زرین‌پال (خودکار)"
- `GatewayType`: `ZarinPal`
- `MerchantId`: از `Web.config`
- `IsActive`: `true`
- `IsDefault`: `true`
- `CallbackUrl`: `/Patient/AppointmentBooking/PaymentCallback`

#### 3. محدودیت‌ها

- ❌ فقط برای اولین Gateway کار می‌کند
- ❌ اگر Gateway موجود باشد، ایجاد نمی‌شود
- ❌ تنظیمات پیشرفته (Fee, MinAmount, MaxAmount) تنظیم نمی‌شود

**توصیه:** برای تنظیمات کامل، از UI استفاده کنید.

---

## 💾 روش 3: تنظیم مستقیم در دیتابیس

### ⚠️ هشدار:

**این روش فقط برای موارد خاص توصیه می‌شود!**

### مراحل:

#### 1. اتصال به دیتابیس

```sql
USE ClinicDb;
GO
```

#### 2. بررسی Gateway های موجود

```sql
SELECT 
    PaymentGatewayId,
    Name,
    GatewayType,
    MerchantId,
    IsActive,
    IsDefault,
    IsDeleted,
    CreatedAt
FROM PaymentGateways
WHERE IsDeleted = 0;
```

#### 3. ایجاد Gateway جدید

```sql
INSERT INTO PaymentGateways (
    Name,
    GatewayType,
    MerchantId,
    ApiKey,
    CallbackUrl,
    GatewayUrl,
    IsActive,
    IsDefault,
    IsTestMode,
    IsDeleted,
    CreatedAt
)
VALUES (
    N'زرین‌پال Production',           -- Name
    1,                                 -- GatewayType (1 = ZarinPal)
    N'156be6cd-e0a4-4af8-9113-83647771376f', -- MerchantId
    N'156be6cd-e0a4-4af8-9113-83647771376f', -- ApiKey (معمولاً همان MerchantId)
    N'/Patient/AppointmentBooking/PaymentCallback', -- CallbackUrl
    N'https://api.zarinpal.com/pg/v4/payment/request.json', -- GatewayUrl
    1,                                 -- IsActive (true)
    1,                                 -- IsDefault (true)
    0,                                 -- IsTestMode (false)
    0,                                 -- IsDeleted (false)
    GETUTCDATE()                       -- CreatedAt
);
```

#### 4. بررسی Gateway ایجاد شده

```sql
SELECT * FROM PaymentGateways WHERE MerchantId = '156be6cd-e0a4-4af8-9113-83647771376f';
```

---

## 🔑 تنظیمات مورد نیاز

### 1. Merchant ID (شناسه مرچنت)

**منبع:**
- پنل زرین‌پال → تنظیمات → Merchant ID

**فرمت:**
- UUID Format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- مثال: `156be6cd-e0a4-4af8-9113-83647771376f`

**نکات:**
- ✅ باید از پنل زرین‌پال دریافت شود
- ✅ باید فعال باشد
- ✅ باید برای Production باشد (نه Sandbox)

---

### 2. CallbackUrl (URL بازگشت)

**فرمت:**
- نسبی: `/Patient/AppointmentBooking/PaymentCallback`
- کامل: `https://yourdomain.com/Patient/AppointmentBooking/PaymentCallback`

**نکات:**
- ✅ باید قابل دسترسی از اینترنت باشد
- ✅ باید به `PaymentCallback` action اشاره کند
- ✅ سیستم به صورت خودکار URL کامل می‌سازد

---

### 3. Gateway URLs

**Production:**
```
RequestUrl: https://api.zarinpal.com/pg/v4/payment/request.json
VerifyUrl: https://api.zarinpal.com/pg/v4/payment/verify.json
StartPayUrl: https://www.zarinpal.com/pg/StartPay/
StatusUrl: https://api.zarinpal.com/pg/v4/payment/status.json
```

**Sandbox (برای تست):**
```
RequestUrl: https://sandbox.zarinpal.com/pg/v4/payment/request.json
VerifyUrl: https://sandbox.zarinpal.com/pg/v4/payment/verify.json
StartPayUrl: https://sandbox.zarinpal.com/pg/StartPay/
StatusUrl: https://sandbox.zarinpal.com/pg/v4/payment/status.json
```

---

### 4. تنظیمات پیشرفته (اختیاری)

| فیلد | توضیحات | مقدار پیش‌فرض |
|------|---------|---------------|
| **MinAmount** | حداقل مبلغ (ریال) | `1000` |
| **MaxAmount** | حداکثر مبلغ (ریال) | `null` (بدون محدودیت) |
| **FeePercentage** | درصد کارمزد | `null` |
| **FixedFee** | کارمزد ثابت (ریال) | `null` |
| **Description** | توضیحات | (اختیاری) |

---

## ✅ تست و بررسی

### 1. بررسی Gateway در دیتابیس

```sql
SELECT 
    PaymentGatewayId,
    Name,
    GatewayType,
    MerchantId,
    IsActive,
    IsDefault,
    IsTestMode,
    CallbackUrl,
    CreatedAt
FROM PaymentGateways
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;
```

---

### 2. تست از طریق UI

**مراحل:**
1. رفتن به `Admin Panel` → `Payment` → `Gateway Management`
2. بررسی لیست Gateway ها
3. کلیک روی Gateway برای مشاهده جزئیات
4. تست اتصال (اگر قابلیت موجود باشد)

---

### 3. تست از طریق API

**درخواست پرداخت:**
```
POST /Patient/AppointmentBooking/ProcessPayment
{
    "appointmentId": 1
}
```

**بررسی لاگ‌ها:**
```
✅ WEB PAYMENT: درگاه پیش‌فرض یافت شد - GatewayId: 1, Name: زرین‌پال Production
✅ ZarinPal: درخواست پرداخت موفق - Authority: A00000000000000000000000000000000000000
```

---

## 🔍 عیب‌یابی (Troubleshooting)

### مشکل 1: "درگاه پرداخت پیش‌فرض یافت نشد"

**علت:**
- هیچ Gateway فعالی در دیتابیس وجود ندارد
- `IsDefault = true` تنظیم نشده است

**راه‌حل:**
1. بررسی Gateway ها در دیتابیس
2. تنظیم `IsDefault = true` برای یک Gateway
3. یا تنظیم `IsActive = true` برای Gateway

---

### مشکل 2: "Merchant ID نامعتبر است"

**علت:**
- Merchant ID در `Web.config` تنظیم نشده است
- Merchant ID اشتباه است

**راه‌حل:**
1. بررسی `Web.config` → `ZarinpalMerchantId`
2. بررسی Merchant ID در پنل زرین‌پال
3. اطمینان از فعال بودن Merchant ID

---

### مشکل 3: "CallbackUrl نامعتبر است"

**علت:**
- `CallbackUrl` خالی است
- `CallbackUrl` فرمت نامعتبر دارد

**راه‌حل:**
1. تنظیم `CallbackUrl = "/Patient/AppointmentBooking/PaymentCallback"`
2. بررسی فرمت URL

---

## 📊 خلاصه

### ✅ روش توصیه شده:

**برای Production:**
1. ✅ تنظیم از طریق UI (Admin Panel)
2. ✅ تنظیم `IsDefault = true`
3. ✅ تنظیم `IsTestMode = false`
4. ✅ تست کامل

**برای Development:**
1. ✅ تنظیم از طریق `Web.config`
2. ✅ سیستم به صورت خودکار Gateway ایجاد می‌کند
3. ✅ تنظیم `IsSandbox = true` در `Web.config`

---

## 🔗 مراجع

- **مستندات زرین‌پال:** https://www.zarinpal.com/lab/
- **پنل زرین‌پال:** https://next.zarinpal.com/
- **راهنمای API:** `Docs/ZARINPAL_PRODUCTION_SETUP.md`

---

**📌 نکته مهم:**
> بعد از تنظیم Gateway، حتماً Application را Restart کنید تا تغییرات اعمال شود.

