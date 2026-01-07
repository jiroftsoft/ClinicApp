# 🏭 Best Practices برای تنظیم Gateway در Production

**تاریخ:** 2026-01-06  
**هدف:** راهنمای کامل Best Practices برای تنظیم Gateway در محیط Production

---

## 📋 خلاصه اجرایی

### ✅ وضعیت فعلی:

از بررسی دیتابیس:
- Gateway با ID=2 وجود دارد
- `IsActive = true` ✅
- `IsDefault = true` ✅
- `MerchantId` تنظیم شده ✅
- `CallbackUrl` تنظیم شده ✅

### ❌ مشکلات شناسایی شده:

1. **GatewayUrl هنوز Sandbox است:**
   - فعلی: `https://sandbox.zarinpal.com/pg/StartPay/`
   - باید: `https://www.zarinpal.com/pg/StartPay/`

2. **IsTestMode = true:**
   - فعلی: `IsTestMode = 1` (true)
   - باید: `IsTestMode = 0` (false)

3. **Name: "زرین‌پال (Sandbox)":**
   - باید: "زرین‌پال Production"

4. **GatewayUrl در Entity استفاده نمی‌شود:**
   - `ZarinPalDriver` از `Web.config` استفاده می‌کند
   - `PaymentGateway.GatewayUrl` نادیده گرفته می‌شود

---

## 🔍 تحلیل استفاده از PaymentGateway

### 1. آیا از جدول PaymentGateways استفاده می‌شود؟

**پاسخ: ✅ بله، اما ناقص!**

**استفاده می‌شود برای:**
- ✅ انتخاب Gateway (GetDefaultPaymentGatewayAsync)
- ✅ بررسی IsActive
- ✅ بررسی IsDefault
- ✅ بررسی GatewayType
- ✅ ذخیره MerchantId (اما استفاده نمی‌شود!)

**استفاده نمی‌شود برای:**
- ❌ `MerchantId` - از `Web.config` خوانده می‌شود
- ❌ `GatewayUrl` - از `Web.config` خوانده می‌شود
- ❌ `ApiKey` - استفاده نمی‌شود
- ❌ `PrivateKey` - استفاده نمی‌شود

---

### 2. ZarinPalDriver از کجا تنظیمات را می‌خواند؟

**فایل:** `Services/Payment/Gateway/Drivers/ZarinPalDriver.cs`

```csharp
public ZarinPalDriver(ILogger logger)
{
    // ❌ مشکل: از Web.config می‌خواند، نه از PaymentGateway Entity!
    _merchantId = ZarinPalHelper.GetMerchantId(); // از Web.config
    _isSandbox = ZarinPalHelper.IsSandbox(); // از Web.config
    _requestUrl = ZarinPalHelper.GetRequestUrl(); // از Web.config
    _verifyUrl = ZarinPalHelper.GetVerifyUrl(); // از Web.config
    _startPayUrl = ZarinPalHelper.GetStartPayUrl(); // از Web.config
}
```

**مشکل:**
- `ZarinPalDriver` به `PaymentGateway` Entity دسترسی ندارد
- همه تنظیمات از `Web.config` خوانده می‌شود
- `PaymentGateway` Entity فقط برای انتخاب Gateway استفاده می‌شود

---

## 🎯 Best Practice: استفاده کامل از PaymentGateway Entity

### معماری پیشنهادی:

```
PaymentGateway Entity (Database)
  ├─ MerchantId
  ├─ ApiKey
  ├─ GatewayUrl
  ├─ CallbackUrl
  └─ IsTestMode
        ↓
ZarinPalDriver
  ├─ خواندن MerchantId از PaymentGateway
  ├─ خواندن URLs از PaymentGateway
  └─ استفاده از IsTestMode از PaymentGateway
```

**مزایا:**
- ✅ تنظیمات در دیتابیس (قابل تغییر بدون Restart)
- ✅ چند Gateway مختلف با تنظیمات مختلف
- ✅ مدیریت از طریق UI
- ✅ Audit Trail کامل

---

## 🔧 راه‌حل: تغییر ZarinPalDriver برای استفاده از PaymentGateway

### تغییر 1: ZarinPalDriver Constructor

**قبل:**
```csharp
public ZarinPalDriver(ILogger logger)
{
    _merchantId = ZarinPalHelper.GetMerchantId(); // از Web.config
    _isSandbox = ZarinPalHelper.IsSandbox(); // از Web.config
    // ...
}
```

**بعد:**
```csharp
public ZarinPalDriver(PaymentGateway gateway, ILogger logger)
{
    if (gateway == null)
        throw new ArgumentNullException(nameof(gateway));
    
    _merchantId = gateway.MerchantId; // ✅ از Entity
    _isSandbox = gateway.IsTestMode; // ✅ از Entity
    _requestUrl = gateway.GatewayUrl ?? ZarinPalHelper.GetRequestUrl(); // ✅ از Entity با Fallback
    _startPayUrl = gateway.GatewayUrl ?? ZarinPalHelper.GetStartPayUrl(); // ✅ از Entity با Fallback
    // ...
}
```

**مشکل:**
- `ZarinPalDriver` در Factory ایجاد می‌شود
- Factory نمی‌داند کدام Gateway استفاده می‌شود
- باید Factory را تغییر دهیم

---

## 🏗️ Best Practice: Gateway-Aware Driver Factory

### معماری پیشنهادی:

```
WebPaymentService
  ├─ GetDefaultPaymentGatewayAsync() → PaymentGateway
  ├─ IGatewayDriverFactory.GetDriver(gateway) → IGatewayDriver
  └─ Driver از PaymentGateway تنظیمات را می‌خواند
```

**تغییرات لازم:**

1. **تغییر IGatewayDriverFactory:**
```csharp
public interface IGatewayDriverFactory
{
    IGatewayDriver GetDriver(PaymentGateway gateway); // ✅ دریافت Gateway
    bool IsSupported(PaymentGatewayType gatewayType);
}
```

2. **تغییر GatewayDriverFactory:**
```csharp
public IGatewayDriver GetDriver(PaymentGateway gateway)
{
    switch (gateway.GatewayType)
    {
        case PaymentGatewayType.ZarinPal:
            return new ZarinPalDriver(gateway, _logger); // ✅ Gateway را می‌دهد
        // ...
    }
}
```

3. **تغییر ZarinPalDriver:**
```csharp
public ZarinPalDriver(PaymentGateway gateway, ILogger logger)
{
    _merchantId = gateway.MerchantId;
    _isSandbox = gateway.IsTestMode;
    _requestUrl = gateway.GatewayUrl ?? ZarinPalHelper.GetRequestUrl();
    // ...
}
```

---

## 📊 مقایسه معماری فعلی با Best Practice

| جنبه | معماری فعلی | Best Practice | وضعیت |
|------|------------|--------------|-------|
| **MerchantId** | از Web.config | از PaymentGateway | ❌ |
| **GatewayUrl** | از Web.config | از PaymentGateway | ❌ |
| **IsTestMode** | از Web.config | از PaymentGateway | ❌ |
| **CallbackUrl** | از PaymentGateway | از PaymentGateway | ✅ |
| **Gateway Selection** | از PaymentGateway | از PaymentGateway | ✅ |
| **چند Gateway** | ❌ | ✅ | ❌ |
| **تغییر بدون Restart** | ❌ | ✅ | ❌ |

---

## 🔧 راه‌حل سریع: به‌روزرسانی Gateway موجود

### برای Production:

```sql
UPDATE PaymentGateways
SET 
    Name = N'زرین‌پال Production',
    GatewayUrl = N'https://www.zarinpal.com/pg/StartPay/',
    IsTestMode = 0, -- false
    IsDefault = 1, -- true
    UpdatedAt = GETUTCDATE()
WHERE PaymentGatewayId = 2;
```

**یا از طریق UI:**
1. رفتن به `Admin Panel` → `Payment` → `Gateway Management`
2. ویرایش Gateway با ID=2
3. تغییر:
   - Name: "زرین‌پال Production"
   - GatewayUrl: `https://www.zarinpal.com/pg/StartPay/`
   - IsTestMode: ❌ false
   - IsDefault: ✅ true

---

## ⚠️ مشکل فعلی: GatewayUrl استفاده نمی‌شود

**مشکل:**
- `PaymentGateway.GatewayUrl` در دیتابیس تنظیم شده است
- اما `ZarinPalDriver` از `Web.config` می‌خواند
- `GatewayUrl` نادیده گرفته می‌شود!

**راه‌حل موقت:**
- تنظیم `Web.config` برای Production:
```xml
<add key="Zarinpal:IsSandbox" value="false"/>
<add key="Zarinpal:StartPayUrl" value="https://www.zarinpal.com/pg/StartPay/"/>
```

**راه‌حل کامل:**
- تغییر `ZarinPalDriver` برای استفاده از `PaymentGateway` Entity

---

## ✅ Best Practices برای Production

### 1. تنظیمات دیتابیس:

```sql
-- Gateway Production
UPDATE PaymentGateways
SET 
    Name = N'زرین‌پال Production',
    GatewayType = 1, -- ZarinPal
    MerchantId = N'156be6cd-e0a4-4af8-9113-83647771376f', -- Merchant ID واقعی
    ApiKey = N'156be6cd-e0a4-4af8-9113-83647771376f',
    GatewayUrl = N'https://www.zarinpal.com/pg/StartPay/',
    CallbackUrl = N'/Patient/AppointmentBooking/PaymentCallback',
    IsActive = 1, -- true
    IsDefault = 1, -- true
    IsTestMode = 0, -- false (Production)
    IsDeleted = 0, -- false
    Description = N'درگاه پرداخت زرین‌پال - Production',
    UpdatedAt = GETUTCDATE()
WHERE PaymentGatewayId = 2;
```

---

### 2. تنظیمات Web.config:

```xml
<!-- ✅ Production Mode -->
<add key="ZarinpalMerchantId" value="156be6cd-e0a4-4af8-9113-83647771376f"/>
<add key="Zarinpal:IsSandbox" value="false"/>

<!-- ✅ Production URLs -->
<add key="Zarinpal:RequestUrl" value="https://api.zarinpal.com/pg/v4/payment/request.json"/>
<add key="Zarinpal:VerifyUrl" value="https://api.zarinpal.com/pg/v4/payment/verify.json"/>
<add key="Zarinpal:StartPayUrl" value="https://www.zarinpal.com/pg/StartPay/"/>
<add key="Zarinpal:StatusUrl" value="https://api.zarinpal.com/pg/v4/payment/status.json"/>
```

---

### 3. چک‌لیست Production:

- [ ] `IsTestMode = false` در دیتابیس
- [ ] `IsSandbox = false` در Web.config
- [ ] `GatewayUrl` = Production URL
- [ ] `MerchantId` = Merchant ID واقعی
- [ ] `IsActive = true`
- [ ] `IsDefault = true`
- [ ] `CallbackUrl` تنظیم شده
- [ ] Application Restart شده است

---

## 🔍 بررسی استفاده فعلی

### آیا از PaymentGateway استفاده می‌شود؟

**پاسخ: ✅ بله، اما ناقص**

**استفاده می‌شود:**
1. ✅ `GetDefaultPaymentGatewayAsync()` - انتخاب Gateway
2. ✅ `GatewayType` - برای Factory
3. ✅ `IsActive` - بررسی فعال بودن
4. ✅ `IsDefault` - انتخاب پیش‌فرض
5. ✅ `CallbackUrl` - در Controller استفاده می‌شود

**استفاده نمی‌شود:**
1. ❌ `MerchantId` - از Web.config خوانده می‌شود
2. ❌ `GatewayUrl` - از Web.config خوانده می‌شود
3. ❌ `IsTestMode` - از Web.config خوانده می‌شود
4. ❌ `ApiKey` - استفاده نمی‌شود

---

## 🎯 توصیه‌های نهایی

### برای Production فعلی:

1. ✅ **به‌روزرسانی Gateway در دیتابیس:**
   - `IsTestMode = false`
   - `GatewayUrl` = Production URL
   - `Name` = "زرین‌پال Production"

2. ✅ **به‌روزرسانی Web.config:**
   - `IsSandbox = false`
   - URLs = Production URLs

3. ✅ **Restart Application**

### برای آینده (Best Practice):

1. ✅ **تغییر ZarinPalDriver:**
   - دریافت `PaymentGateway` در Constructor
   - استفاده از تنظیمات از Entity

2. ✅ **تغییر Factory:**
   - دریافت `PaymentGateway` در `GetDriver`
   - پاس دادن Gateway به Driver

3. ✅ **حذف وابستگی به Web.config:**
   - همه تنظیمات از دیتابیس
   - Web.config فقط برای Fallback

---

## 📌 نتیجه‌گیری

**وضعیت فعلی:**
- ✅ Gateway در دیتابیس وجود دارد
- ✅ از Gateway برای انتخاب استفاده می‌شود
- ❌ تنظیمات (MerchantId, URLs) از Web.config خوانده می‌شود
- ❌ `IsTestMode` در دیتابیس درست نیست

**اقدامات لازم:**
1. ✅ به‌روزرسانی Gateway در دیتابیس (IsTestMode = false)
2. ✅ به‌روزرسانی Web.config (IsSandbox = false)
3. ⚠️ (آینده) تغییر Driver برای استفاده از Entity

---

**📌 مرجع:** 
- Best Practices: Microsoft Payment Gateway Integration Guide
- Architecture: Clean Architecture Principles

