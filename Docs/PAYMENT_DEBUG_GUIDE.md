# 🔍 راهنمای Debug خطای پرداخت

**تاریخ:** 2026-01-06  
**وضعیت:** 🔴 Active Debugging

---

## 📋 خلاصه مشکل

خطای "خطا در ایجاد درخواست پرداخت در درگاه" رخ می‌دهد. این خطا می‌تواند از چند منبع باشد:

1. ❌ **خطای CallbackUrl Domain Mismatch** (رایج‌ترین)
2. ❌ **خطای MerchantId نامعتبر**
3. ❌ **خطای Amount نامعتبر**
4. ❌ **خطای API ZarinPal**

---

## 🔍 مراحل Debug

### STEP 1: بررسی لاگ‌های سرور

لاگ‌های زیر را بررسی کنید:

#### 1.1. لاگ CallbackUrl
```
🔗 PAYMENT REQUEST: CallbackUrl تنظیم شد
```
**بررسی:**
- آیا `BaseUrl` درست است؟ (`https://mehranyad.ir`)
- آیا `CallbackUrl` کامل است؟ (مثلاً: `https://mehranyad.ir/Patient/AppointmentBooking/PaymentCallback`)
- آیا دامنه با دامنه ثبت شده در پنل ZarinPal مطابقت دارد؟

#### 1.2. لاگ ZarinPal Request
```
📤 ZarinPal: ارسال درخواست به {Url}
🔍 ZarinPal DEBUG: GatewayUrl={GatewayUrl}, IsSandbox={IsSandbox}, RequestUrl={RequestUrl}, CallbackUrl={CallbackUrl}
```
**بررسی:**
- آیا `IsSandbox=false` است؟ (Production Mode)
- آیا `RequestUrl` درست است؟ (`https://api.zarinpal.com/pg/v4/payment/request.json`)
- آیا `CallbackUrl` درست است؟

#### 1.3. لاگ ZarinPal Response
```
📥 ZarinPal: پاسخ دریافت شد
❌ ZarinPal: خطای API - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}
```
**بررسی:**
- کد خطا چیست؟
- پیام خطا چیست؟
- آیا `errors` در پاسخ وجود دارد؟

---

### STEP 2: بررسی تنظیمات

#### 2.1. Web.config
```xml
<add key="Payment:BaseUrl" value="https://mehranyad.ir"/>
<add key="ZarinpalMerchantId" value="156be6cd-e0a4-4af8-9113-83647771376f"/>
<add key="Zarinpal:IsSandbox" value="false"/>
```

**بررسی:**
- ✅ آیا `Payment:BaseUrl` تنظیم شده است؟
- ✅ آیا `ZarinpalMerchantId` درست است؟
- ✅ آیا `Zarinpal:IsSandbox` برابر `false` است؟

#### 2.2. Database (PaymentGateways)
```sql
SELECT PaymentGatewayId, Name, GatewayType, MerchantId, GatewayUrl, IsTestMode, IsActive, IsDefault, CallbackUrl
FROM PaymentGateways
WHERE GatewayType = 'ZarinPal' AND IsDeleted = 0;
```

**بررسی:**
- ✅ آیا `IsTestMode = 0` است؟ (Production)
- ✅ آیا `IsActive = 1` است؟
- ✅ آیا `IsDefault = 1` است؟
- ✅ آیا `GatewayUrl` درست است؟ (`https://www.zarinpal.com/pg/StartPay/`)
- ✅ آیا `MerchantId` درست است؟

---

### STEP 3: بررسی Application Restart

⚠️ **مهم:** بعد از تغییر `Web.config`، باید Application Restart شود!

**بررسی:**
1. آیا Application Restart شده است؟
2. آیا `AppSettings.Instance` جدید را می‌خواند؟
3. آیا `PaymentBaseUrl` در `AppSettings` تنظیم شده است؟

**تست:**
```csharp
// در Controller یا Service
var baseUrl = _appSettings.PaymentBaseUrl;
_logger.Information("🔍 DEBUG: PaymentBaseUrl = {BaseUrl}", baseUrl);
```

---

### STEP 4: بررسی خطاهای رایج ZarinPal

#### 4.1. خطای Domain Mismatch
```
خطا از درگاه پرداخت: The callback URL domain does not match the registered terminal domain.
```

**راه‌حل:**
1. ✅ بررسی `Payment:BaseUrl` در `Web.config`
2. ✅ بررسی دامنه ثبت شده در پنل ZarinPal
3. ✅ اطمینان از اینکه `CallbackUrl` با دامنه ثبت شده مطابقت دارد
4. ✅ Application Restart

#### 4.2. خطای MerchantId نامعتبر
```
خطا از درگاه پرداخت: Merchant ID is invalid.
```

**راه‌حل:**
1. ✅ بررسی `ZarinpalMerchantId` در `Web.config`
2. ✅ بررسی `MerchantId` در `PaymentGateways` table
3. ✅ اطمینان از اینکه MerchantId درست است

#### 4.3. خطای Amount نامعتبر
```
خطا از درگاه پرداخت: Amount is invalid.
```

**راه‌حل:**
1. ✅ بررسی مبلغ (باید >= 1000 تومان باشد)
2. ✅ بررسی اینکه مبلغ به Rials تبدیل شده است

---

## 🛠️ راه‌حل‌های پیشنهادی

### راه‌حل 1: Application Restart
```powershell
# در IIS
iisreset

# یا در Visual Studio
# Stop Debug → Start Debug
```

### راه‌حل 2: بررسی CallbackUrl
```csharp
// در AppointmentBookingController.cs
var callbackUrl = Helpers.PaymentUrlHelper.BuildPaymentCallbackUrl(callbackRelativePath, Request, _appSettings);
_logger.Information("🔍 DEBUG: CallbackUrl = {CallbackUrl}, BaseUrl = {BaseUrl}", 
    callbackUrl, _appSettings.PaymentBaseUrl);
```

### راه‌حل 3: بررسی Response از ZarinPal
```csharp
// در ZarinPalDriver.cs
_logger.Error("❌ ZarinPal: خطای API - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}, ResponseContent: {Content}",
    errorCode, errorMessage, responseContent);
```

---

## 📊 چک‌لیست Debug

- [ ] ✅ Application Restart شده است؟
- [ ] ✅ `Payment:BaseUrl` در `Web.config` تنظیم شده است؟
- [ ] ✅ `ZarinpalMerchantId` درست است؟
- [ ] ✅ `Zarinpal:IsSandbox` برابر `false` است؟
- [ ] ✅ `PaymentGateways` table به‌روزرسانی شده است؟
- [ ] ✅ `CallbackUrl` با دامنه ثبت شده در پنل ZarinPal مطابقت دارد؟
- [ ] ✅ لاگ‌های سرور بررسی شده‌اند؟
- [ ] ✅ خطای دقیق از ZarinPal شناسایی شده است؟

---

## 🔗 مراجع

- `Docs/PAYMENT_CALLBACK_URL_FIX.md` - رفع خطای CallbackUrl
- `Docs/PAYMENT_BASE_URL_CONFIGURED.md` - تنظیم PaymentBaseUrl
- `Docs/PAYMENT_GATEWAY_OPTIMIZATION_COMPLETE.md` - بهینه‌سازی درگاه پرداخت

---

**نکته:** اگر مشکل حل نشد، لاگ‌های کامل را بررسی کنید و خطای دقیق از ZarinPal را شناسایی کنید.

