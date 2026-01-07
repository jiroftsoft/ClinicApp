# 🔍 چک‌لیست دیباگ خطای پرداخت - AppointmentId: 29

**تاریخ:** 2026-01-06  
**خطا:** "خطا در ایجاد درخواست پرداخت در درگاه"  
**AppointmentId:** 29

---

## ✅ مرحله 0: استفاده از Action دیباگ (سریع‌ترین روش)

### URL دیباگ:

```
GET /Patient/AppointmentBooking/CheckPaymentGateway
```

**این Action بررسی می‌کند:**
1. ✅ درگاه پیش‌فرض یافت می‌شود؟
2. ✅ Web.config تنظیمات درست است؟
3. ✅ درگاه‌های موجود در دیتابیس

**مثال پاسخ موفق:**
```json
{
  "success": true,
  "message": "درگاه پرداخت پیش‌فرض یافت شد",
  "diagnostic": {
    "steps": [
      {
        "step": 1,
        "name": "GetDefaultPaymentGatewayAsync",
        "status": "success",
        "data": {
          "gatewayId": 1,
          "name": "زرین‌پال (Production)",
          "isActive": true,
          "isDefault": true
        }
      }
    ]
  }
}
```

**مثال پاسخ خطا:**
```json
{
  "success": false,
  "message": "درگاه پرداخت پیش‌فرض یافت نشد",
  "diagnostic": {
    "steps": [
      {
        "step": 1,
        "status": "failed",
        "error": "درگاه پرداخت پیش‌فرض یافت نشد..."
      },
      {
        "step": 2,
        "name": "CheckWebConfig",
        "status": "success",
        "data": {
          "merchantIdExists": true,
          "merchantIdLength": 36
        }
      },
      {
        "step": 3,
        "name": "CheckDatabaseGateways",
        "status": "success",
        "data": {
          "totalCount": 0,
          "activeCount": 0
        }
      }
    ]
  }
}
```

---

## ✅ مرحله 1: بررسی لاگ‌های سرور

### لاگ‌های مورد انتظار:

```
💰 PAYMENT REQUEST: درخواست پردازش پرداخت - AppointmentId: 29
🔍 WEB PAYMENT: شروع جستجوی درگاه پرداخت پیش‌فرض...
```

**اگر این لاگ‌ها وجود ندارند:**
- ❌ درخواست به سرور نرسیده است
- ✅ بررسی Network Tab در Browser DevTools

---

## ✅ مرحله 2: بررسی درگاه پرداخت در دیتابیس

### SQL Query برای بررسی:

```sql
-- بررسی وجود درگاه‌های پرداخت
SELECT 
    PaymentGatewayId,
    Name,
    GatewayType,
    IsActive,
    IsDeleted,
    IsDefault,
    MerchantId,
    CallbackUrl
FROM PaymentGateways
WHERE IsDeleted = 0
ORDER BY IsDefault DESC, IsActive DESC;
```

**اگر هیچ درگاهی وجود ندارد:**
- ✅ درگاه باید خودکار از Web.config ایجاد شود
- ✅ بررسی لاگ: `🔍 WEB PAYMENT: STEP 4 - MerchantId از Web.config`

**اگر درگاه وجود دارد اما IsActive = 0:**
- ❌ درگاه غیرفعال است
- ✅ باید IsActive = 1 شود

**اگر درگاه وجود دارد اما IsDeleted = 1:**
- ❌ درگاه حذف شده است
- ✅ باید IsDeleted = 0 شود

---

## ✅ مرحله 3: بررسی تنظیمات Web.config

### بررسی وجود MerchantId:

```xml
<add key="ZarinpalMerchantId" value="156be6cd-e0a4-4af8-9113-83647771376f"/>
```

**اگر MerchantId موجود نیست:**
- ❌ خطا: "ZarinPal Merchant ID در Web.config یافت نشد"
- ✅ باید MerchantId اضافه شود

**اگر MerchantId موجود است:**
- ✅ بررسی لاگ: `✅ ZarinPal Driver initialized - MerchantId: ...`

---

## ✅ مرحله 4: بررسی Driver Initialization

### لاگ مورد انتظار:

```
✅ ZarinPal Driver initialized - MerchantId: 156be6cd..., IsSandbox: false
```

**اگر این لاگ وجود ندارد:**
- ❌ Driver initialize نشده است
- ✅ بررسی Exception در لاگ‌ها

---

## ✅ مرحله 5: بررسی درخواست به API زرین‌پال

### لاگ‌های مورد انتظار:

```
💰 ZarinPal: شروع درخواست پرداخت - Amount: X, Description: Y
📤 ZarinPal: ارسال درخواست به https://api.zarinpal.com/pg/v4/payment/request.json
📥 ZarinPal: پاسخ دریافت شد - StatusCode: 200, Content: {...}
```

**اگر StatusCode != 200:**
- ❌ خطای HTTP (مثلاً 400, 401, 500)
- ✅ بررسی Content پاسخ برای کد خطا

**اگر StatusCode = 200 اما code != 100:**
- ❌ خطای API زرین‌پال
- ✅ بررسی کد خطا:
  - `-10`: IP یا مرچنت کد صحیح نیست
  - `-11`: مرچنت کد فعال نیست
  - `-15`: درگاه پرداخت فعال نیست
  - سایر کدها: مراجعه به `GetZarinPalErrorMessage`

---

## ✅ مرحله 6: بررسی Validation

### Validation های انجام شده:

1. ✅ Gateway != null
2. ✅ Gateway.IsActive == true
3. ✅ Gateway.IsDeleted == false
4. ✅ CallbackUrl != null && IsAbsoluteUri
5. ✅ Amount > 0
6. ✅ Amount >= 1000 (حداقل)

**اگر یکی از این Validation ها خطا می‌دهد:**
- ✅ بررسی لاگ: `❌ WEB PAYMENT: ...` برای جزئیات

---

## 🔧 راه‌حل‌های احتمالی

### راه‌حل 1: درگاه پرداخت در دیتابیس وجود ندارد

**SQL برای ایجاد درگاه:**

```sql
INSERT INTO PaymentGateways (
    Name,
    GatewayType,
    MerchantId,
    ApiKey,
    GatewayUrl,
    CallbackUrl,
    IsActive,
    IsDefault,
    IsDeleted,
    CreatedAt
)
VALUES (
    'زرین‌پال (Production)',
    1, -- ZarinPal
    '156be6cd-e0a4-4af8-9113-83647771376f',
    '156be6cd-e0a4-4af8-9113-83647771376f',
    'https://www.zarinpal.com/pg/StartPay/',
    '/Patient/AppointmentBooking/PaymentCallback',
    1, -- IsActive
    1, -- IsDefault
    0, -- IsDeleted
    GETUTCDATE()
);
```

---

### راه‌حل 2: درگاه غیرفعال است

**SQL برای فعال کردن:**

```sql
UPDATE PaymentGateways
SET IsActive = 1
WHERE GatewayType = 1 -- ZarinPal
  AND IsDeleted = 0;
```

---

### راه‌حل 3: MerchantId نامعتبر است

**بررسی:**
1. ✅ MerchantId در Web.config درست است؟
2. ✅ MerchantId در پنل زرین‌پال فعال است؟
3. ✅ IP سرور در پنل زرین‌پال ثبت شده است؟

---

### راه‌حل 4: خطای API زرین‌پال

**بررسی کد خطا:**
- `-10`: IP یا مرچنت کد صحیح نیست → بررسی IP و MerchantId
- `-11`: مرچنت کد فعال نیست → فعال کردن در پنل زرین‌پال
- `-15`: درگاه پرداخت فعال نیست → فعال کردن در پنل زرین‌پال

---

## 📋 چک‌لیست نهایی

- [ ] لاگ‌های سرور بررسی شده
- [ ] درگاه پرداخت در دیتابیس وجود دارد
- [ ] درگاه پرداخت فعال است (IsActive = 1)
- [ ] درگاه پرداخت حذف نشده است (IsDeleted = 0)
- [ ] MerchantId در Web.config موجود است
- [ ] Driver initialize شده است
- [ ] درخواست به API زرین‌پال ارسال شده است
- [ ] پاسخ API بررسی شده است
- [ ] کد خطا (اگر وجود دارد) شناسایی شده است

---

## 🎯 اقدامات بعدی

بعد از بررسی این چک‌لیست:
1. ✅ لاگ‌های سرور را بررسی کنید
2. ✅ مشکل دقیق را شناسایی کنید
3. ✅ راه‌حل مناسب را اعمال کنید
4. ✅ تست کنید

---

**📌 مرجع:** `Docs/PAYMENT_GATEWAY_CONNECTION_ANALYSIS.md`

