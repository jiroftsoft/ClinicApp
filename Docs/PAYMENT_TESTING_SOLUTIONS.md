# 🧪 راه‌حل‌های تست پرداخت با Merchant ID واقعی

**تاریخ:** 2026-01-06  
**مشکل:** می‌خواهید با Merchant ID واقعی `mehranyad.ir` در محیط Development تست کنید

---

## 🎯 راه‌حل‌های پیشنهادی

### ✅ راه‌حل 1: استفاده از ZarinPal Sandbox (توصیه می‌شود)

**مزایا:**
- ✅ بدون خطر پرداخت واقعی
- ✅ تست کامل Flow
- ✅ بدون نیاز به تغییر کد

**مراحل:**

#### STEP 1: ایجاد Gateway تست در Database

```sql
USE ClinicDb;
GO

-- ایجاد Gateway تست (Sandbox)
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
    'ZarinPal',  -- یا 1 اگر Enum است
    N'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx',  -- ✅ Merchant ID Sandbox از پنل ZarinPal
    N'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx',
    N'https://sandbox.zarinpal.com/pg/StartPay/',
    N'/Patient/AppointmentBooking/PaymentCallback',
    1,  -- IsActive = true
    0,  -- IsDefault = false (Production را پیش‌فرض نگه دارید)
    1,  -- IsTestMode = true (Sandbox)
    N'درگاه تست برای Development - استفاده از Sandbox',
    GETUTCDATE()
);
GO

-- بررسی
SELECT PaymentGatewayId, Name, IsTestMode, IsActive, IsDefault
FROM PaymentGateways
WHERE GatewayType = 'ZarinPal'
ORDER BY IsDefault DESC, IsTestMode DESC;
GO
```

#### STEP 2: دریافت Merchant ID Sandbox

1. وارد پنل ZarinPal شوید: https://next.zarinpal.com/
2. به بخش **Sandbox** بروید
3. Merchant ID Sandbox را کپی کنید
4. در SQL Script بالا جایگزین کنید

#### STEP 3: تست

- Gateway Sandbox برای تست استفاده می‌شود
- Gateway Production برای Production استفاده می‌شود

---

### ✅ راه‌حل 2: استفاده از Simulated Gateway (برای تست کامل)

**مزایا:**
- ✅ بدون نیاز به اتصال به ZarinPal
- ✅ تست سریع و بدون محدودیت
- ✅ کنترل کامل بر پاسخ‌ها

**مراحل:**

#### STEP 1: ایجاد SimulatedGateway Driver

```csharp
// Services/Payment/Gateway/Drivers/SimulatedGatewayDriver.cs
public class SimulatedGatewayDriver : IGatewayDriver
{
    private readonly ILogger _logger;
    
    public SimulatedGatewayDriver(PaymentGateway gateway, ILogger logger)
    {
        _logger = logger;
    }
    
    public async Task<ServiceResult<PaymentRequestResult>> RequestPaymentAsync(PaymentRequest request)
    {
        _logger.Information("🎭 SIMULATED: درخواست پرداخت شبیه‌سازی شد - Amount: {Amount}", request.Amount);
        
        // شبیه‌سازی Authority
        var authority = $"A{DateTime.UtcNow.Ticks}";
        var paymentUrl = $"https://localhost:3560/SimulatedPayment?authority={authority}";
        
        return ServiceResult<PaymentRequestResult>.Successful(new PaymentRequestResult
        {
            Success = true,
            Authority = authority,
            PaymentUrl = paymentUrl
        });
    }
    
    // ... سایر متدها
}
```

#### STEP 2: ثبت در Factory

```csharp
// Services/Payment/Gateway/Drivers/GatewayDriverFactory.cs
case PaymentGatewayType.Simulated:
    driver = new SimulatedGatewayDriver(gateway, _logger);
    break;
```

#### STEP 3: ایجاد Gateway در Database

```sql
INSERT INTO PaymentGateways (
    Name,
    GatewayType,
    MerchantId,
    IsActive,
    IsDefault,
    IsTestMode,
    Description
)
VALUES (
    N'Simulated Gateway (تست)',
    'Simulated',  -- یا Enum جدید
    N'TEST-MERCHANT-ID',
    1,
    0,
    1,
    N'درگاه شبیه‌سازی شده برای تست'
);
```

---

### ✅ راه‌حل 3: تست با مبالغ کم در Production (⚠️ با احتیاط)

**مزایا:**
- ✅ تست واقعی با Merchant ID واقعی
- ✅ تست کامل Flow

**معایب:**
- ⚠️ پرداخت واقعی انجام می‌شود
- ⚠️ نیاز به کارت بانکی واقعی

**مراحل:**

#### STEP 1: تنظیم Gateway Production

```sql
-- بررسی Gateway Production
SELECT PaymentGatewayId, Name, MerchantId, IsTestMode, IsActive, IsDefault
FROM PaymentGateways
WHERE GatewayType = 'ZarinPal' AND IsTestMode = 0;
```

#### STEP 2: تست با مبالغ کم

- مبلغ: **1000 تومان** (حداقل ZarinPal)
- استفاده از کارت تست بانکی (اگر دارید)

---

## 🎯 راه‌حل توصیه شده

### برای Development: **راه‌حل 1 (Sandbox)**

**دلایل:**
1. ✅ بدون خطر پرداخت واقعی
2. ✅ تست کامل Flow
3. ✅ بدون نیاز به تغییر کد
4. ✅ Merchant ID Sandbox رایگان است

### برای Production Testing: **راه‌حل 3 (با احتیاط)**

**دلایل:**
1. ✅ تست واقعی با Merchant ID واقعی
2. ✅ تست کامل در محیط Production
3. ⚠️ نیاز به احتیاط (پرداخت واقعی)

---

## 📋 چک‌لیست تست

### قبل از تست:

- [ ] ✅ Gateway در Database وجود دارد؟
- [ ] ✅ `IsActive = 1` است؟
- [ ] ✅ `IsTestMode` درست تنظیم شده است؟
- [ ] ✅ `MerchantId` درست است؟
- [ ] ✅ `CallbackUrl` درست است؟
- [ ] ✅ `Payment:BaseUrl` در `Web.config` تنظیم شده است؟

### بعد از تست:

- [ ] ✅ لاگ‌های `WEB PAYMENT` وجود دارد؟
- [ ] ✅ لاگ‌های `ZarinPal` وجود دارد؟
- [ ] ✅ `Authority` دریافت شده است؟
- [ ] ✅ `PaymentUrl` درست است؟
- [ ] ✅ Callback کار می‌کند؟

---

## 🔗 مراجع

- `Docs/PAYMENT_GATEWAY_SETUP_GUIDE.md` - راهنمای تنظیم Gateway
- `Docs/PAYMENT_DEBUG_GUIDE.md` - راهنمای Debug
- `Docs/PAYMENT_ARCHITECTURE_COMPARISON_ADMVC.md` - مقایسه با ADMVC

---

**نکته:** برای Development، **راه‌حل 1 (Sandbox)** توصیه می‌شود. برای Production Testing، **راه‌حل 3** با احتیاط استفاده کنید.

