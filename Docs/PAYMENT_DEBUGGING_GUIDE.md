# 🔍 راهنمای دیباگ: خطای "خطا در ایجاد درخواست پرداخت در درگاه"

**تاریخ:** 2026-01-06  
**هدف:** شناسایی و رفع خطای "خطا در ایجاد درخواست پرداخت در درگاه"

---

## ⚠️ مشکل

خطای "خطا در ایجاد درخواست پرداخت در درگاه" هنگام کلیک روی "تائید و پرداخت" نمایش داده می‌شود.

---

## 🔍 مراحل دیباگ

### مرحله 1: بررسی لاگ‌های سرور

لاگ‌های زیر را در فایل لاگ (مثلاً `clinicapp-*.log`) جستجو کنید:

#### 1.1. لاگ‌های `ProcessPayment`:
```
💰 PAYMENT REQUEST: درخواست پردازش پرداخت - AppointmentId: ...
🔗 PAYMENT REQUEST: CallbackUrl تنظیم شد - ...
```

#### 1.2. لاگ‌های `CreatePaymentRequestAsync`:
```
💰 WEB PAYMENT: شروع ایجاد درخواست پرداخت در درگاه ...
🔧 WEB PAYMENT: فراخوانی Driver - Amount: ..., CallbackUrl: ..., Description: ...
```

#### 1.3. لاگ‌های `ZarinPalDriver`:
```
💰 ZarinPal: شروع درخواست پرداخت - Amount: ..., Description: ...
📤 ZarinPal: ارسال درخواست به ...
📥 ZarinPal: پاسخ دریافت شد - StatusCode: ..., Content: ...
```

---

### مرحله 2: بررسی Validation Errors

#### 2.1. Validation در `ZarinPalDriver`:

خطاهای احتمالی:
- ❌ "مبلغ پرداخت باید بیشتر از صفر باشد" → `Amount <= 0`
- ❌ "حداقل مبلغ پرداخت 1000 ریال است" → `Amount < 1000`
- ❌ "آدرس Callback الزامی است" → `CallbackUrl` خالی است

**راه‌حل:**
```sql
-- بررسی مبلغ نوبت
SELECT AppointmentId, Price, Status
FROM Appointments
WHERE AppointmentId = 26; -- شناسه نوبت
```

#### 2.2. Validation در `WebPaymentService`:

خطاهای احتمالی:
- ❌ "شناسه پرداخت آنلاین نامعتبر است" → `OnlinePaymentId <= 0`
- ❌ "مبلغ پرداخت باید بیشتر از صفر باشد" → `Amount <= 0`
- ❌ "آدرس Callback الزامی است" → `CallbackUrl` خالی است

---

### مرحله 3: بررسی خطاهای API زرین‌پال

#### 3.1. کدهای خطای زرین‌پال:

| کد | پیام | علت |
|---|---|---|
| -9 | خطای اعتبارسنجی | داده‌های ارسالی نامعتبر است |
| -10 | IP یا مرچنت کد صحیح نیست | MerchantId یا IP نامعتبر |
| -11 | مرچنت کد فعال نیست | MerchantId غیرفعال است |
| -12 | تلاش بیش از حد درخواست | Rate Limiting |
| -15 | درگاه پرداخت فعال نیست | درگاه غیرفعال است |
| -32 | مبلغ از حد مجاز بیشتر است | Amount > MaxAmount |
| -35 | مبلغ از حد مجاز کمتر است | Amount < MinAmount (1000 ریال) |

#### 3.2. بررسی پاسخ API:

در لاگ‌ها دنبال کنید:
```
📥 ZarinPal: پاسخ دریافت شد - StatusCode: ..., Content: ...
```

مثال پاسخ موفق:
```json
{
  "data": {
    "code": 100,
    "message": "Success",
    "authority": "A00000000000000000000000000000000000000"
  }
}
```

مثال پاسخ ناموفق:
```json
{
  "data": {
    "code": -11,
    "message": "مرچنت کد فعال نیست"
  }
}
```

---

### مرحله 4: بررسی تنظیمات

#### 4.1. بررسی `Web.config`:

```xml
<!-- ✅ Merchant ID -->
<add key="ZarinpalMerchantId" value="YOUR_MERCHANT_ID"/>

<!-- ✅ Sandbox/Production -->
<add key="Zarinpal:IsSandbox" value="true"/> <!-- یا false برای Production -->

<!-- ✅ URLs -->
<add key="Zarinpal:RequestUrl" value="..."/>
```

#### 4.2. بررسی دیتابیس:

```sql
-- بررسی درگاه پرداخت
SELECT 
    PaymentGatewayId,
    Name,
    MerchantId,
    IsActive,
    IsDefault,
    CallbackUrl,
    GatewayUrl
FROM PaymentGateways
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;
```

---

### مرحله 5: بررسی مشکلات رایج

#### 5.1. مبلغ کمتر از 1000 ریال:

**مشکل:** زرین‌پال حداقل 1000 ریال را می‌پذیرد.

**راه‌حل:**
```sql
-- بررسی مبلغ نوبت
SELECT AppointmentId, Price
FROM Appointments
WHERE AppointmentId = 26;

-- اگر کمتر از 1000 ریال است، باید افزایش دهید
UPDATE Appointments
SET Price = 1000
WHERE AppointmentId = 26 AND Price < 1000;
```

#### 5.2. CallbackUrl نامعتبر:

**مشکل:** `CallbackUrl` باید یک URL کامل باشد (با `http://` یا `https://`).

**بررسی:**
- در لاگ‌ها دنبال کنید: `🔗 PAYMENT REQUEST: CallbackUrl تنظیم شد - ...`
- باید به صورت `https://example.com/Patient/AppointmentBooking/PaymentCallback` باشد

#### 5.3. MerchantId نامعتبر:

**مشکل:** `MerchantId` در `Web.config` یا دیتابیس نامعتبر است.

**راه‌حل:**
1. بررسی `Web.config`: `ZarinpalMerchantId`
2. بررسی دیتابیس: `PaymentGateways.MerchantId`
3. مطمئن شوید که `MerchantId` در پنل زرین‌پال فعال است

#### 5.4. خطای شبکه یا Timeout:

**مشکل:** ارتباط با API زرین‌پال برقرار نمی‌شود.

**بررسی:**
- در لاگ‌ها دنبال کنید: `❌ ZarinPal: خطا در ارتباط با درگاه پرداخت`
- بررسی کنید که سرور به اینترنت دسترسی دارد
- بررسی کنید که Firewall مانع ارتباط نمی‌شود

---

## 📋 چک‌لیست دیباگ

- [ ] لاگ‌های سرور را بررسی کنید
- [ ] `Amount` >= 1000 ریال است؟
- [ ] `CallbackUrl` کامل است (با `http://` یا `https://`)?
- [ ] `MerchantId` در `Web.config` تنظیم شده است؟
- [ ] `MerchantId` در دیتابیس درست است؟
- [ ] `IsSandbox` درست تنظیم شده است؟
- [ ] درگاه در دیتابیس `IsActive = 1` است؟
- [ ] پاسخ API زرین‌پال را بررسی کنید (کد خطا)

---

## 🔧 دستورات SQL برای بررسی

```sql
-- 1. بررسی نوبت
SELECT 
    AppointmentId,
    PatientId,
    DoctorId,
    AppointmentDate,
    Price,
    Status,
    PaymentTransactionId
FROM Appointments
WHERE AppointmentId = 26; -- شناسه نوبت

-- 2. بررسی OnlinePayment
SELECT 
    OnlinePaymentId,
    PaymentGatewayId,
    AppointmentId,
    PatientId,
    Amount,
    Status,
    PaymentToken,
    PaymentUrl,
    ErrorCode,
    ErrorMessage,
    CreatedAt
FROM OnlinePayments
WHERE AppointmentId = 26
ORDER BY CreatedAt DESC;

-- 3. بررسی درگاه پرداخت
SELECT 
    PaymentGatewayId,
    Name,
    GatewayType,
    MerchantId,
    IsActive,
    IsDefault,
    CallbackUrl,
    GatewayUrl
FROM PaymentGateways
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;
```

---

## ✅ راه‌حل‌های پیشنهادی

### راه‌حل 1: بررسی مبلغ

اگر مبلغ کمتر از 1000 ریال است:
```sql
UPDATE Appointments
SET Price = 1000
WHERE AppointmentId = 26 AND Price < 1000;
```

### راه‌حل 2: بررسی CallbackUrl

اگر `CallbackUrl` نسبی است، باید کامل شود (در کد انجام شده است).

### راه‌حل 3: بررسی MerchantId

اگر `MerchantId` نامعتبر است:
1. از پنل زرین‌پال `MerchantId` واقعی را دریافت کنید
2. در `Web.config` تنظیم کنید
3. Application را Restart کنید

### راه‌حل 4: بررسی Sandbox/Production

اگر در Production هستید:
```xml
<add key="Zarinpal:IsSandbox" value="false"/>
```

---

## 📊 نمونه لاگ‌های موفق

```
💰 PAYMENT REQUEST: درخواست پردازش پرداخت - AppointmentId: 26
🔗 PAYMENT REQUEST: CallbackUrl تنظیم شد - https://example.com/Patient/AppointmentBooking/PaymentCallback
💰 WEB PAYMENT: شروع ایجاد درخواست پرداخت در درگاه ZarinPal برای مبلغ 500000
🔧 WEB PAYMENT: فراخوانی Driver - Amount: 500000, CallbackUrl: https://..., Description: ...
💰 ZarinPal: شروع درخواست پرداخت - Amount: 500000, Description: ...
📤 ZarinPal: ارسال درخواست به https://sandbox.zarinpal.com/pg/v4/payment/request.json
📥 ZarinPal: پاسخ دریافت شد - StatusCode: OK, Content: {"data":{"code":100,"authority":"..."}}
✅ ZarinPal: درخواست پرداخت موفق - Authority: ..., PaymentUrl: ...
✅ WEB PAYMENT: Driver درخواست پرداخت موفق - Authority: ..., PaymentUrl: ...
```

---

## 📊 نمونه لاگ‌های ناموفق

```
💰 PAYMENT REQUEST: درخواست پردازش پرداخت - AppointmentId: 26
⚠️ ZarinPal: Validation ناموفق - حداقل مبلغ پرداخت 1000 ریال است
❌ WEB PAYMENT: Driver درخواست پرداخت ناموفق - Success: False, Message: ...
```

یا:

```
💰 ZarinPal: شروع درخواست پرداخت - Amount: 500000
📤 ZarinPal: ارسال درخواست به ...
📥 ZarinPal: پاسخ دریافت شد - StatusCode: OK, Content: {"data":{"code":-11,"message":"مرچنت کد فعال نیست"}}
⚠️ ZarinPal: درخواست پرداخت ناموفق - Code: -11, Message: مرچنت کد فعال نیست
❌ WEB PAYMENT: Driver درخواست پرداخت ناموفق - Success: False, Message: ...
```

---

## 🎯 نتیجه

بعد از بررسی لاگ‌های سرور، می‌توانید دقیقاً ببینید که:
1. آیا Validation خطا می‌دهد؟
2. آیا API زرین‌پال خطا می‌دهد؟
3. آیا خطای شبکه است؟
4. آیا `MerchantId` یا `CallbackUrl` نامعتبر است؟

**لطفاً لاگ‌های سرور را بررسی کنید و خطاهای مربوط به `ZarinPal` یا `WEB PAYMENT` را ارسال کنید.**

---

**وضعیت:** ⏳ در انتظار بررسی لاگ‌های سرور

