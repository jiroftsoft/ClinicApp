# 🚀 راهنمای سریع تست پرداخت

**تاریخ:** 2026-01-06  
**هدف:** تست پرداخت با Merchant ID واقعی `mehranyad.ir` در Development

---

## ✅ راه‌حل توصیه شده: استفاده از ZarinPal Sandbox

### چرا Sandbox؟

1. ✅ **بدون خطر پرداخت واقعی** - تست کامل بدون پرداخت پول
2. ✅ **Merchant ID رایگان** - از پنل ZarinPal دریافت می‌کنید
3. ✅ تست کامل Flow
4. ✅ **بدون نیاز به تغییر کد** - فقط Database

---

## 📋 مراحل (5 دقیقه)

### STEP 1: دریافت Merchant ID Sandbox

1. وارد پنل ZarinPal شوید: **https://next.zarinpal.com/**
2. به بخش **Sandbox** بروید
3. **Merchant ID Sandbox** را کپی کنید
   - فرمت: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`

### STEP 2: اجرای SQL Script

**فایل:** `Scripts/sql/Create_Test_Gateway_Sandbox.sql`

```sql
USE ClinicDb;
GO

-- جایگزین کنید: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx با Merchant ID Sandbox
INSERT INTO PaymentGateways (
    Name,
    GatewayType,
    MerchantId,
    ApiKey,
    GatewayUrl,
    CallbackUrl,
    IsActive,
    IsDefault,
    IsTestMode,  -- ✅ true = Sandbox
    Description,
    CreatedAt
)
VALUES (
    N'زرین‌پال (Sandbox - تست)',
    'ZarinPal',
    N'YOUR_SANDBOX_MERCHANT_ID_HERE',  -- ⚠️ جایگزین کنید
    N'YOUR_SANDBOX_MERCHANT_ID_HERE',
    N'https://sandbox.zarinpal.com/pg/StartPay/',
    N'/Patient/AppointmentBooking/PaymentCallback',
    1,  -- IsActive = true
    0,  -- IsDefault = false (Production را پیش‌فرض نگه دارید)
    1,  -- IsTestMode = true (Sandbox)
    N'درگاه تست برای Development',
    GETUTCDATE()
);
GO
```

### STEP 3: بررسی

```sql
SELECT 
    PaymentGatewayId,
    Name,
    LEFT(MerchantId, 10) + '...' AS MerchantIdPreview,
    IsActive,
    IsDefault,
    IsTestMode,
    CallbackUrl
FROM PaymentGateways
WHERE GatewayType = 'ZarinPal'
ORDER BY IsDefault DESC, IsTestMode DESC;
```

**نتیجه مورد انتظار:**
- ✅ Gateway Production: `IsDefault = 1`, `IsTestMode = 0`
- ✅ Gateway Sandbox: `IsDefault = 0`, `IsTestMode = 1`

### STEP 4: تست

1. **Restart Application** (اگر در حال اجرا است)
2. **ایجاد نوبت** و تلاش برای پرداخت
3. **بررسی لاگ‌ها:**
   ```
   ✅ WEB PAYMENT: درگاه پیش‌فرض یافت شد
   ✅ ZarinPal: درخواست پرداخت موفق
   ```

---

## 🔄 انتخاب Gateway برای تست

### روش 1: تغییر IsDefault (ساده)

```sql
-- Gateway Sandbox را پیش‌فرض کنید
UPDATE PaymentGateways
SET IsDefault = 1
WHERE Name = N'زرین‌پال (Sandbox - تست)';

-- Gateway Production را غیر پیش‌فرض کنید
UPDATE PaymentGateways
SET IsDefault = 0
WHERE Name = N'زرین‌پال Production';
```

### روش 2: تغییر IsActive (توصیه می‌شود)

```sql
-- Gateway Production را غیرفعال کنید
UPDATE PaymentGateways
SET IsActive = 0
WHERE Name = N'زرین‌پال Production' AND IsTestMode = 0;

-- Gateway Sandbox را فعال کنید
UPDATE PaymentGateways
SET IsActive = 1
WHERE Name = N'زرین‌پال (Sandbox - تست)';
```

**مزایا:**
- ✅ Gateway Production دست‌نخورده می‌ماند
- ✅ فقط با تغییر `IsActive` می‌توانید بین تست و Production جابجا شوید

---

## ⚠️ نکات مهم

### 1. Callback URL

**Development (localhost):**
- Callback URL در Sandbox: `http://localhost:3560/Patient/AppointmentBooking/PaymentCallback`
- ✅ در پنل ZarinPal Sandbox ثبت کنید

**Production:**
- Callback URL: `https://mehranyad.ir/Patient/AppointmentBooking/PaymentCallback`
- ✅ در پنل ZarinPal Production ثبت کنید

### 2. PaymentBaseUrl

**Web.config:**
```xml
<!-- Development -->
<add key="Payment:BaseUrl" value="http://localhost:3560"/>

<!-- Production -->
<add key="Payment:BaseUrl" value="https://mehranyad.ir"/>
```

### 3. قبل از Deploy به Production

```sql
-- Gateway Production را فعال کنید
UPDATE PaymentGateways
SET IsActive = 1, IsDefault = 1
WHERE Name = N'زرین‌پال Production' AND IsTestMode = 0;

-- Gateway Sandbox را غیرفعال کنید
UPDATE PaymentGateways
SET IsActive = 0
WHERE Name = N'زرین‌پال (Sandbox - تست)';
```

---

## 🧪 تست کامل

### چک‌لیست:

- [ ] ✅ Gateway Sandbox در Database وجود دارد
- [ ] ✅ `IsActive = 1` است
- [ ] ✅ `IsTestMode = 1` است
- [ ] ✅ `MerchantId` درست است (Sandbox)
- [ ] ✅ `CallbackUrl` در پنل ZarinPal Sandbox ثبت شده است
- [ ] ✅ Application Restart شده است
- [ ] ✅ تست پرداخت انجام شده است
- [ ] ✅ لاگ‌ها بررسی شده است

---

## 🔗 مراجع

- `Docs/PAYMENT_TESTING_SOLUTIONS.md` - راهنمای کامل
- `Scripts/sql/Create_Test_Gateway_Sandbox.sql` - SQL Script
- `Docs/PAYMENT_GATEWAY_SETUP_GUIDE.md` - راهنمای تنظیم Gateway

---

**نکته:** برای Development، **Sandbox** توصیه می‌شود. برای Production Testing، از Gateway Production با احتیاط استفاده کنید.

