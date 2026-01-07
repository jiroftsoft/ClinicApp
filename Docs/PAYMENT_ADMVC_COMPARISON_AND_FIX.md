# 🔍 مقایسه ADMVC و ClinicApp - راه‌حل استفاده از Merchant ID واقعی

**تاریخ:** 2026-01-06  
**Merchant ID:** `156be6cd-e0a4-4af8-9113-83647771376f`  
**هدف:** استفاده از Merchant ID واقعی `mehranyad.ir` در ClinicApp

---

## 📊 مقایسه معماری

### ADMVC (پروژه قبلی - کار می‌کند ✅)

**ساختار:**
```
PaymentController
  └─ IPaymentService
      └─ PaymentService
          └─ IPaymentGateway (DI)
              └─ ZarinpalGateway
```

**ویژگی‌های کلیدی:**
1. ✅ **ساده و مستقیم** - بدون لایه‌های اضافی
2. ✅ **خواندن از Web.config** - `ConfigurationManager.AppSettings["ZarinpalMerchantId"]`
3. ✅ **URL های Hard-coded** - `https://api.zarinpal.com/pg/v4/payment/request.json`
4. ✅ **Callback URL ساده** - `UrlHelper.Action("Callback", "Payment", null, "http")`
5. ✅ **بدون Database** - همه چیز از Web.config

**کد ADMVC:**
```csharp
// ZarinpalGateway.cs
private readonly string _merchantId = ConfigurationManager.AppSettings["ZarinpalMerchantId"];

public async Task<PaymentRequestResult> RequestPayment(decimal amount, string description, string callbackUrl)
{
    var zarinpalUrl = "https://api.zarinpal.com/pg/v4/payment/request.json";
    
    var requestBody = new
    {
        merchant_id = _merchantId,
        amount = (int)amount,
        callback_url = callbackUrl,
        description = description
    };
    
    // ارسال درخواست...
}
```

---

### ClinicApp (پروژه فعلی - مشکل دارد ❌)

**ساختار:**
```
AppointmentBookingController
  └─ IWebPaymentService
      └─ WebPaymentService
          └─ IGatewayDriverFactory
              └─ GatewayDriverFactory
                  └─ IGatewayDriver
                      └─ ZarinPalDriver
```

**ویژگی‌های کلیدی:**
1. ✅ **Database-Driven** - تنظیمات از `PaymentGateways` table
2. ✅ **Factory Pattern** - انتخاب Gateway از Database
3. ✅ **ServiceResult Pattern** - پاسخ استاندارد
4. ✅ **Logging کامل** - Serilog با Correlation ID
5. ❌ **پیچیده‌تر** - لایه‌های بیشتر

**مشکل احتمالی:**
- Gateway از Database یافت نمی‌شود
- یا تنظیمات در Database درست نیست

---

## 🔧 راه‌حل: استفاده از Merchant ID واقعی

### روش 1: استفاده از Database (توصیه می‌شود)

#### STEP 1: بررسی Gateway در Database

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
    GatewayUrl
FROM PaymentGateways
WHERE GatewayType = 1  -- ZarinPal
    AND IsDeleted = 0;
```

#### STEP 2: به‌روزرسانی Gateway Production

```sql
USE ClinicDb;
GO

-- به‌روزرسانی Gateway Production با Merchant ID واقعی
UPDATE PaymentGateways
SET 
    MerchantId = N'156be6cd-e0a4-4af8-9113-83647771376f',
    ApiKey = N'156be6cd-e0a4-4af8-9113-83647771376f',
    GatewayUrl = N'https://www.zarinpal.com/pg/StartPay/',
    IsTestMode = 0,  -- Production
    IsActive = 1,
    IsDefault = 1,
    CallbackUrl = N'/Patient/AppointmentBooking/PaymentCallback',
    UpdatedAt = GETUTCDATE()
WHERE GatewayType = 1  -- ZarinPal
    AND IsTestMode = 0  -- Production
    AND IsDeleted = 0;

-- اگر Gateway وجود ندارد، ایجاد کنید
IF NOT EXISTS (
    SELECT 1 
    FROM PaymentGateways 
    WHERE GatewayType = 1 AND IsTestMode = 0 AND IsDeleted = 0
)
BEGIN
    INSERT INTO PaymentGateways (
        Name,
        GatewayType,
        MerchantId,
        ApiKey,
        GatewayUrl,
        CallbackUrl,
        IsActive,
        IsDefault,
        IsTestMode,
        Description,
        CreatedAt
    )
    VALUES (
        N'زرین‌پال Production',
        1,  -- ZarinPal
        N'156be6cd-e0a4-4af8-9113-83647771376f',
        N'156be6cd-e0a4-4af8-9113-83647771376f',
        N'https://www.zarinpal.com/pg/StartPay/',
        N'/Patient/AppointmentBooking/PaymentCallback',
        1,  -- IsActive = true
        1,  -- IsDefault = true
        0,  -- IsTestMode = false (Production)
        N'درگاه پرداخت زرین‌پال Production - mehranyad.ir',
        GETUTCDATE()
    );
END
GO
```

#### STEP 3: بررسی نتیجه

```sql
SELECT 
    PaymentGatewayId,
    Name,
    LEFT(MerchantId, 20) + '...' AS MerchantIdPreview,
    IsActive,
    IsDefault,
    IsTestMode,
    CallbackUrl
FROM PaymentGateways
WHERE GatewayType = 1
    AND IsDeleted = 0
ORDER BY IsDefault DESC, IsTestMode DESC;
```

---

### روش 2: استفاده از Web.config (Fallback)

**اگر Gateway در Database یافت نشد، سیستم از Web.config استفاده می‌کند:**

```xml
<!-- Web.config -->
<appSettings>
    <add key="ZarinpalMerchantId" value="156be6cd-e0a4-4af8-9113-83647771376f"/>
    <add key="Zarinpal:IsSandbox" value="false"/>
    <add key="Zarinpal:RequestUrl" value="https://api.zarinpal.com/pg/v4/payment/request.json"/>
    <add key="Zarinpal:VerifyUrl" value="https://api.zarinpal.com/pg/v4/payment/verify.json"/>
    <add key="Zarinpal:StartPayUrl" value="https://www.zarinpal.com/pg/StartPay/"/>
    <add key="Payment:BaseUrl" value="https://mehranyad.ir"/>
</appSettings>
```

**نکته:** سیستم به صورت خودکار Gateway را از Web.config ایجاد می‌کند اگر در Database یافت نشود.

---

## 🔍 تفاوت‌های کلیدی

### 1. Callback URL

**ADMVC:**
```csharp
var callbackUrl = new UrlHelper(System.Web.HttpContext.Current.Request.RequestContext)
    .Action("Callback", "Payment", null, "http");
// نتیجه: http://localhost:3560/Payment/Callback
```

**ClinicApp:**
```csharp
var callbackRelativePath = Url.Action("PaymentCallback", "AppointmentBooking", new { area = "Patient" });
var callbackUrl = Helpers.PaymentUrlHelper.BuildPaymentCallbackUrl(callbackRelativePath, Request, _appSettings);
// نتیجه: https://mehranyad.ir/Patient/AppointmentBooking/PaymentCallback
```

**مزیت ClinicApp:**
- ✅ پشتیبانی از `PaymentBaseUrl` از `Web.config`
- ✅ پشتیبانی از Production Domain

---

### 2. Error Handling

**ADMVC:**
```csharp
if (result.data != null && result.data.code == 100)
{
    return new PaymentRequestResult { IsSuccess = true, ... };
}
string errorMessage = result.errors.message ?? "An unknown error occurred.";
return new PaymentRequestResult { IsSuccess = false, ErrorMessage = $"Zarinpal Error: {errorMessage}" };
```

**ClinicApp:**
```csharp
// بررسی errors در پاسخ
if (zarinPalResponse.errors != null)
{
    var errorCode = zarinPalResponse.errors.code ?? "UNKNOWN";
    var errorMessage = zarinPalResponse.errors.message ?? "خطای نامشخص از درگاه پرداخت";
    _logger.Error("❌ ZarinPal: خطای API - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}", ...);
    return ServiceResult<PaymentRequestResult>.Failed($"خطا از درگاه پرداخت: {errorMessage}");
}

// بررسی data
if (zarinPalResponse.data == null)
{
    _logger.Error("❌ ZarinPal: data در پاسخ null است");
    return ServiceResult<PaymentRequestResult>.Failed("پاسخ نامعتبر از درگاه پرداخت");
}

// بررسی code
if (!zarinPalResponse.data.code.HasValue)
{
    _logger.Error("❌ ZarinPal: code در پاسخ null است");
    return ServiceResult<PaymentRequestResult>.Failed("پاسخ نامعتبر از درگاه پرداخت");
}

// بررسی authority
if (string.IsNullOrWhiteSpace(zarinPalResponse.data.authority))
{
    _logger.Error("❌ ZarinPal: authority در پاسخ null یا خالی است");
    return ServiceResult<PaymentRequestResult>.Failed("کد Authority از درگاه پرداخت دریافت نشد");
}
```

**مزیت ClinicApp:**
- ✅ Error Handling کامل‌تر
- ✅ Logging دقیق‌تر
- ✅ بررسی تمام حالات ممکن

---

## ✅ راه‌حل نهایی

### برای استفاده از Merchant ID واقعی:

1. **Gateway را در Database به‌روزرسانی کنید** (SQL Script بالا)
2. **Web.config را بررسی کنید** (Fallback)
3. **Application را Restart کنید**
4. **تست کنید**

---

## 📋 چک‌لیست

- [ ] ✅ Gateway در Database وجود دارد؟
- [ ] ✅ `MerchantId` درست است؟ (`156be6cd-e0a4-4af8-9113-83647771376f`)
- [ ] ✅ `IsActive = 1` است؟
- [ ] ✅ `IsDefault = 1` است؟
- [ ] ✅ `IsTestMode = 0` است؟ (Production)
- [ ] ✅ `CallbackUrl` درست است؟
- [ ] ✅ `Payment:BaseUrl` در `Web.config` تنظیم شده است؟
- [ ] ✅ Application Restart شده است؟

---

## 🔗 فایل‌های مرتبط

- `Scripts/sql/Update_PaymentGateway_To_Production.sql` - به‌روزرسانی Gateway
- `Docs/PAYMENT_GATEWAY_SETUP_GUIDE.md` - راهنمای تنظیم Gateway
- `Docs/PAYMENT_DEBUG_GUIDE.md` - راهنمای Debug

---

**نکته:** ClinicApp پیچیده‌تر است اما قابلیت‌های بیشتری دارد (Database-Driven, Logging, Error Handling). برای استفاده از Merchant ID واقعی، Gateway را در Database به‌روزرسانی کنید.

