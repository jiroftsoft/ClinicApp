# 🔍 راهنمای گام‌به‌گام تشخیص خطای پرداخت

**تاریخ:** 2026-01-06  
**CorrelationId:** `92c168d6-7a73-4f2e-bf84-1f0fc9e39822`  
**AppointmentId:** 34  
**وضعیت:** 🔴 Active Debugging

---

## 🚨 مشکل

خطای "خطا در ایجاد درخواست پرداخت در درگاه" رخ می‌دهد.

**CorrelationId:** `92c168d6-7a73-4f2e-bf84-1f0fc9e39822`

---

## 📋 مراحل تشخیص (Step-by-Step)

### STEP 1: بررسی لاگ‌های سرور با CorrelationId

لاگ‌های سرور را با استفاده از CorrelationId جستجو کنید:

```sql
-- اگر لاگ‌ها در Database ذخیره می‌شوند
SELECT * FROM Logs 
WHERE Message LIKE '%92c168d6-7a73-4f2e-bf84-1f0fc9e39822%'
ORDER BY Timestamp DESC;
```

یا در فایل لاگ:
```powershell
# در PowerShell
Select-String -Path "*.log" -Pattern "92c168d6-7a73-4f2e-bf84-1f0fc9e39822"
```

**لاگ‌های مهم:**
1. `🔗 PAYMENT REQUEST: CallbackUrl تنظیم شد` - بررسی CallbackUrl
2. `📤 ZarinPal: ارسال درخواست به {Url}` - بررسی Request
3. `📥 ZarinPal: پاسخ دریافت شد` - بررسی Response
4. `❌ ZarinPal: خطای API` - خطای دقیق از ZarinPal

---

### STEP 2: بررسی CallbackUrl

**لاگ مورد نظر:**
```
🔗 PAYMENT REQUEST: CallbackUrl تنظیم شد - {CallbackUrl} (BaseUrl: {BaseUrl}, RelativePath: {RelativePath}, RequestUrl: {RequestUrl})
```

**بررسی:**
- ✅ آیا `BaseUrl` برابر `https://mehranyad.ir` است؟
- ✅ آیا `CallbackUrl` کامل است؟ (مثلاً: `https://mehranyad.ir/Patient/AppointmentBooking/PaymentCallback`)
- ✅ آیا دامنه با دامنه ثبت شده در پنل ZarinPal مطابقت دارد؟

**اگر CallbackUrl اشتباه است:**
1. بررسی `Web.config` → `Payment:BaseUrl`
2. Application Restart
3. تست مجدد

---

### STEP 3: بررسی Response از ZarinPal

**لاگ مورد نظر:**
```
📥 ZarinPal: پاسخ دریافت شد - StatusCode: {StatusCode}, Content: {Content}
❌ ZarinPal: خطای API - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}
```

**خطاهای رایج:**

#### خطا 1: Domain Mismatch
```
ErrorCode: -9
ErrorMessage: The callback URL domain does not match the registered terminal domain.
```

**راه‌حل:**
1. ✅ بررسی `Payment:BaseUrl` در `Web.config` (باید `https://mehranyad.ir` باشد)
2. ✅ بررسی دامنه ثبت شده در پنل ZarinPal (باید `mehranyad.ir` باشد)
3. ✅ Application Restart
4. ✅ تست مجدد

#### خطا 2: MerchantId نامعتبر
```
ErrorCode: -10
ErrorMessage: Merchant ID is invalid.
```

**راه‌حل:**
1. ✅ بررسی `ZarinpalMerchantId` در `Web.config`
2. ✅ بررسی `MerchantId` در `PaymentGateways` table
3. ✅ اطمینان از اینکه MerchantId درست است

#### خطا 3: Amount نامعتبر
```
ErrorCode: -11
ErrorMessage: Amount is invalid.
```

**راه‌حل:**
1. ✅ بررسی مبلغ (باید >= 1000 تومان باشد)
2. ✅ بررسی اینکه مبلغ به Rials تبدیل شده است

---

### STEP 4: بررسی تنظیمات

#### 4.1. Web.config
```xml
<add key="Payment:BaseUrl" value="https://mehranyad.ir"/>
<add key="ZarinpalMerchantId" value="156be6cd-e0a4-4af8-9113-83647771376f"/>
<add key="Zarinpal:IsSandbox" value="false"/>
```

**بررسی:**
- ✅ آیا `Payment:BaseUrl` تنظیم شده است؟
- ✅ آیا `ZarinpalMerchantId` درست است؟
- ✅ آیا `Zarinpal:IsSandbox` برابر `false` است؟

#### 4.2. Database (PaymentGateways)
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

### STEP 5: Application Restart ⚠️ **الزامی**

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

## 🔍 چک‌لیست Debug

- [ ] ✅ Application Restart شده است؟
- [ ] ✅ `Payment:BaseUrl` در `Web.config` تنظیم شده است؟
- [ ] ✅ `ZarinpalMerchantId` درست است؟
- [ ] ✅ `Zarinpal:IsSandbox` برابر `false` است؟
- [ ] ✅ `PaymentGateways` table به‌روزرسانی شده است؟
- [ ] ✅ `CallbackUrl` با دامنه ثبت شده در پنل ZarinPal مطابقت دارد؟
- [ ] ✅ لاگ‌های سرور با CorrelationId بررسی شده‌اند؟
- [ ] ✅ خطای دقیق از ZarinPal شناسایی شده است؟

---

## 📊 نمونه لاگ‌های مورد انتظار

### لاگ موفق:
```
🔗 PAYMENT REQUEST: CallbackUrl تنظیم شد - https://mehranyad.ir/Patient/AppointmentBooking/PaymentCallback (BaseUrl: https://mehranyad.ir, RelativePath: /Patient/AppointmentBooking/PaymentCallback, RequestUrl: http://localhost:3560/...)
📤 ZarinPal: ارسال درخواست به https://api.zarinpal.com/pg/v4/payment/request.json - MerchantId: 156be6cd..., Amount: 100000, CallbackUrl: https://mehranyad.ir/Patient/AppointmentBooking/PaymentCallback
🔍 ZarinPal DEBUG: IsSandbox=False, RequestUrl=https://api.zarinpal.com/pg/v4/payment/request.json, CallbackUrl=https://mehranyad.ir/Patient/AppointmentBooking/PaymentCallback
📥 ZarinPal: پاسخ دریافت شد - StatusCode: 200, Content: {"data":{"code":100,"authority":"A00000000000000000000000000000000000"},"errors":null}
✅ ZarinPal: درخواست پرداخت موفق - Authority: A00000000000000000000000000000000000
```

### لاگ خطا (Domain Mismatch):
```
🔗 PAYMENT REQUEST: CallbackUrl تنظیم شد - http://localhost:3560/Patient/AppointmentBooking/PaymentCallback (BaseUrl: Request.Url (Fallback), ...)
📤 ZarinPal: ارسال درخواست به https://api.zarinpal.com/pg/v4/payment/request.json - ...
📥 ZarinPal: پاسخ دریافت شد - StatusCode: 200, Content: {"errors":{"code":-9,"message":"The callback URL domain does not match the registered terminal domain."},"data":null}
❌ ZarinPal: خطای API - ErrorCode: -9, ErrorMessage: The callback URL domain does not match the registered terminal domain.
```

---

## 🛠️ راه‌حل‌های پیشنهادی

### راه‌حل 1: Application Restart
```powershell
iisreset
```

### راه‌حل 2: بررسی و اصلاح CallbackUrl
1. بررسی `Payment:BaseUrl` در `Web.config`
2. Application Restart
3. تست مجدد

### راه‌حل 3: بررسی تنظیمات Database
```sql
UPDATE PaymentGateways
SET IsTestMode = 0, IsActive = 1, IsDefault = 1
WHERE GatewayType = 'ZarinPal' AND IsDeleted = 0;
```

---

## 🔗 مراجع

- `Docs/PAYMENT_DEBUG_QUICK_FIX.md` - راهنمای سریع (3 مرحله)
- `Docs/PAYMENT_DEBUG_GUIDE.md` - راهنمای کامل Debug
- `Docs/PAYMENT_CALLBACK_URL_FIX.md` - رفع خطای CallbackUrl

---

## 📞 درخواست کمک

اگر مشکل حل نشد، لطفاً:
1. ✅ لاگ‌های کامل با CorrelationId `92c168d6-7a73-4f2e-bf84-1f0fc9e39822` را ارسال کنید
2. ✅ خطای دقیق از ZarinPal را ارسال کنید (ErrorCode و ErrorMessage)
3. ✅ CallbackUrl که به ZarinPal ارسال می‌شود را ارسال کنید

---

**نکته:** لاگ‌های سرور کلید حل مشکل هستند. حتماً آنها را بررسی کنید.

