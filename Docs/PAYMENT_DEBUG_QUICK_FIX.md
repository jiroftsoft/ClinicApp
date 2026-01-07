# ⚡ راهنمای سریع رفع خطای پرداخت

**تاریخ:** 2026-01-06  
**وضعیت:** 🔴 Active  
**CorrelationId:** `92c168d6-7a73-4f2e-bf84-1f0fc9e39822`  
**AppointmentId:** 34

---

## 🚨 مشکل فعلی

خطای "خطا در ایجاد درخواست پرداخت در درگاه" رخ می‌دهد.

**⚠️ مهم:** برای پیدا کردن خطای دقیق، باید لاگ‌های سرور را بررسی کنید!

---

## ✅ راه‌حل سریع (3 مرحله)

### STEP 1: Application Restart ⚠️ **الزامی**

بعد از تغییر `Web.config`، **حتماً** Application را Restart کنید:

```powershell
# در IIS
iisreset

# یا در Visual Studio
# Stop Debug → Start Debug
```

**چرا؟**  
`AppSettings.Instance` فقط یک بار در Startup بارگذاری می‌شود. اگر `Payment:BaseUrl` را تغییر داده‌اید، باید Restart کنید.

---

### STEP 2: بررسی لاگ‌های سرور ⚠️ **الزامی**

**برای پیدا کردن خطای دقیق، از اسکریپت PowerShell استفاده کنید:**

```powershell
# در PowerShell (از مسیر ریشه پروژه)
.\Scripts\FindPaymentError.ps1 -CorrelationId "92c168d6-7a73-4f2e-bf84-1f0fc9e39822"
```

**یا به صورت دستی:**

```powershell
# جستجو در فایل‌های لاگ
Select-String -Path "App_Data\Logs\*.log" -Pattern "92c168d6-7a73-4f2e-bf84-1f0fc9e39822" -Context 5, 5
```

**لاگ‌های زیر را بررسی کنید:**

#### 2.1. لاگ CallbackUrl
```
🔗 PAYMENT REQUEST: CallbackUrl تنظیم شد - {CallbackUrl} (BaseUrl: {BaseUrl}, RelativePath: {RelativePath}, RequestUrl: {RequestUrl})
```

**بررسی:**
- ✅ آیا `BaseUrl` برابر `https://mehranyad.ir` است؟
- ✅ آیا `CallbackUrl` کامل است؟ (مثلاً: `https://mehranyad.ir/Patient/AppointmentBooking/PaymentCallback`)

#### 2.2. لاگ ZarinPal Request
```
📤 ZarinPal: ارسال درخواست به {Url}
🔍 ZarinPal DEBUG: IsSandbox={IsSandbox}, RequestUrl={RequestUrl}, CallbackUrl={CallbackUrl}
```

**بررسی:**
- ✅ آیا `IsSandbox=false` است؟ (Production Mode)
- ✅ آیا `RequestUrl` درست است؟ (`https://api.zarinpal.com/pg/v4/payment/request.json`)
- ✅ آیا `CallbackUrl` با دامنه ثبت شده در پنل ZarinPal مطابقت دارد؟

#### 2.3. لاگ ZarinPal Response (مهم‌ترین)
```
📥 ZarinPal: پاسخ دریافت شد - StatusCode: {StatusCode}, Content: {Content}
❌ ZarinPal: خطای API - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}
```

**بررسی:**
- ✅ کد خطا چیست؟ (مثلاً: `-9` = Domain Mismatch)
- ✅ پیام خطا چیست؟ (مثلاً: "The callback URL domain does not match...")

---

### STEP 3: بررسی تنظیمات

#### 3.1. Web.config
```xml
<add key="Payment:BaseUrl" value="https://mehranyad.ir"/>
<add key="ZarinpalMerchantId" value="156be6cd-e0a4-4af8-9113-83647771376f"/>
<add key="Zarinpal:IsSandbox" value="false"/>
```

**بررسی:**
- ✅ آیا `Payment:BaseUrl` تنظیم شده است؟
- ✅ آیا `ZarinpalMerchantId` درست است؟
- ✅ آیا `Zarinpal:IsSandbox` برابر `false` است؟

#### 3.2. Database (PaymentGateways)
```sql
SELECT PaymentGatewayId, Name, GatewayType, MerchantId, GatewayUrl, IsTestMode, IsActive, IsDefault
FROM PaymentGateways
WHERE GatewayType = 'ZarinPal' AND IsDeleted = 0;
```

**بررسی:**
- ✅ آیا `IsTestMode = 0` است؟ (Production)
- ✅ آیا `IsActive = 1` است؟
- ✅ آیا `IsDefault = 1` است؟

---

## 🔍 خطاهای رایج و راه‌حل

### خطا 1: Domain Mismatch
```
خطا از درگاه پرداخت: The callback URL domain does not match the registered terminal domain.
```

**راه‌حل:**
1. ✅ بررسی `Payment:BaseUrl` در `Web.config` (باید `https://mehranyad.ir` باشد)
2. ✅ بررسی دامنه ثبت شده در پنل ZarinPal (باید `mehranyad.ir` باشد)
3. ✅ Application Restart
4. ✅ تست مجدد

### خطا 2: MerchantId نامعتبر
```
خطا از درگاه پرداخت: Merchant ID is invalid.
```

**راه‌حل:**
1. ✅ بررسی `ZarinpalMerchantId` در `Web.config`
2. ✅ بررسی `MerchantId` در `PaymentGateways` table
3. ✅ اطمینان از اینکه MerchantId درست است

### خطا 3: Amount نامعتبر
```
خطا از درگاه پرداخت: Amount is invalid.
```

**راه‌حل:**
1. ✅ بررسی مبلغ (باید >= 1000 تومان باشد)
2. ✅ بررسی اینکه مبلغ به Rials تبدیل شده است

---

## 📊 چک‌لیست سریع

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

- `Docs/PAYMENT_DEBUG_GUIDE.md` - راهنمای کامل Debug
- `Docs/PAYMENT_CALLBACK_URL_FIX.md` - رفع خطای CallbackUrl
- `Docs/PAYMENT_BASE_URL_CONFIGURED.md` - تنظیم PaymentBaseUrl

---

**نکته:** اگر مشکل حل نشد، لاگ‌های کامل را بررسی کنید و خطای دقیق از ZarinPal را شناسایی کنید.

