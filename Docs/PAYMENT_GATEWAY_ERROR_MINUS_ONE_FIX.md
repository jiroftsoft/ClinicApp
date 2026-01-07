# 🔧 رفع خطای "خطای نامشخص (کد: -1)" در درگاه پرداخت

**تاریخ:** 2026-01-06  
**مشکل:** کد خطای -1 از API زرین‌پال  
**AppointmentId:** 29

---

## 🔍 تحلیل مشکل

کد خطای `-1` به این معنی است که:
- `zarinPalResponse.data` null است
- یا `zarinPalResponse.data.code` null است
- یا پاسخ API زرین‌پال شامل `errors` است نه `data`

---

## ✅ بهبودهای اعمال شده

### 1. بررسی errors در پاسخ
```csharp
// ✅ CRITICAL FIX: بررسی errors در پاسخ (اگر API خطا بدهد، errors پر می‌شود)
if (zarinPalResponse.errors != null)
{
    var errorCode = zarinPalResponse.errors.code ?? "UNKNOWN";
    var errorMessage = zarinPalResponse.errors.message ?? "خطای نامشخص از درگاه پرداخت";
    
    _logger.Error("❌ ZarinPal: خطای API - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}, ResponseContent: {Content}",
        errorCode, errorMessage, responseContent);
    
    return ServiceResult<PaymentRequestResult>.Failed($"خطا از درگاه پرداخت: {errorMessage}");
}
```

### 2. بررسی null بودن data
```csharp
// ✅ CRITICAL FIX: بررسی null بودن data
if (zarinPalResponse.data == null)
{
    _logger.Error("❌ ZarinPal: data در پاسخ null است - ResponseContent: {Content}", responseContent);
    return ServiceResult<PaymentRequestResult>.Failed("پاسخ نامعتبر از درگاه پرداخت (data is null)");
}
```

### 3. بررسی null بودن code
```csharp
// ✅ CRITICAL FIX: بررسی null بودن code
if (!zarinPalResponse.data.code.HasValue)
{
    _logger.Error("❌ ZarinPal: code در پاسخ null است - ResponseContent: {Content}, DataMessage: {Message}",
        responseContent, zarinPalResponse.data.message);
    return ServiceResult<PaymentRequestResult>.Failed($"پاسخ نامعتبر از درگاه پرداخت: {zarinPalResponse.data.message ?? "کد خطا نامشخص است"}");
}
```

### 4. بررسی HTTP StatusCode
```csharp
// ✅ CRITICAL FIX: بررسی StatusCode HTTP
if (!response.IsSuccessStatusCode)
{
    _logger.Error("❌ ZarinPal: HTTP StatusCode ناموفق - StatusCode: {StatusCode}, Content: {Content}",
        response.StatusCode, responseContent);
    return ServiceResult<PaymentRequestResult>.Failed($"خطا در ارتباط با درگاه پرداخت (HTTP {response.StatusCode})");
}
```

---

## 📋 بررسی لاگ‌های سرور

### لاگ‌های مورد انتظار:

```
📤 ZarinPal: ارسال درخواست به https://api.zarinpal.com/pg/v4/payment/request.json - MerchantId: 156be6cd..., Amount: 500000, CallbackUrl: http://localhost:3560/Patient/AppointmentBooking/PaymentCallback
📥 ZarinPal: پاسخ دریافت شد - StatusCode: 200, Content: {...}
```

**اگر StatusCode != 200:**
- ❌ خطای HTTP (مثلاً 400, 401, 500)
- ✅ بررسی Content پاسخ

**اگر StatusCode = 200 اما data null است:**
- ❌ پاسخ API شامل `errors` است
- ✅ بررسی لاگ: `❌ ZarinPal: خطای API - ErrorCode: ..., ErrorMessage: ...`

**اگر StatusCode = 200 و data موجود است اما code null است:**
- ❌ ساختار پاسخ نامعتبر است
- ✅ بررسی لاگ: `❌ ZarinPal: code در پاسخ null است - ResponseContent: ...`

---

## 🔧 علل احتمالی

### 1. MerchantId نامعتبر
**بررسی:**
- ✅ MerchantId در Web.config: `156be6cd-e0a4-4af8-9113-83647771376f`
- ✅ آیا این MerchantId در پنل زرین‌پال فعال است؟
- ✅ آیا IP سرور در پنل زرین‌پال ثبت شده است؟

**راه‌حل:**
- بررسی پنل زرین‌پال
- ثبت IP سرور در پنل زرین‌پال
- فعال کردن MerchantId

### 2. CallbackUrl نامعتبر
**بررسی:**
- ✅ CallbackUrl: `http://localhost:3560/Patient/AppointmentBooking/PaymentCallback`
- ⚠️ **مشکل:** `localhost` برای Production معتبر نیست!

**راه‌حل:**
- در Production باید از Domain واقعی استفاده شود
- برای Sandbox، `localhost` ممکن است کار کند

### 3. مبلغ نامعتبر
**بررسی:**
- ✅ مبلغ: 500000 ریال (50000 تومان)
- ✅ مبلغ >= 1000 ریال ✓

**راه‌حل:**
- مبلغ معتبر است

### 4. خطای API زرین‌پال
**بررسی:**
- ✅ بررسی لاگ‌های سرور برای پاسخ کامل API
- ✅ بررسی کد خطا از `errors` در پاسخ

**راه‌حل:**
- بررسی لاگ‌های سرور
- بررسی کد خطا از پنل زرین‌پال

---

## 🎯 اقدامات بعدی

### 1. بررسی لاگ‌های سرور (الزامی)

در فایل لاگ دنبال کنید:
```
📥 ZarinPal: پاسخ دریافت شد - StatusCode: ..., Content: ...
❌ ZarinPal: خطای API - ErrorCode: ..., ErrorMessage: ...
❌ ZarinPal: data در پاسخ null است - ResponseContent: ...
❌ ZarinPal: code در پاسخ null است - ResponseContent: ...
```

### 2. بررسی پاسخ کامل API

اگر لاگ‌ها در دسترس نیستند، می‌توانید:
1. ✅ از Action دیباگ استفاده کنید: `/Patient/AppointmentBooking/TestPaymentProcess?appointmentId=29`
2. ✅ لاگ‌های سرور را بررسی کنید
3. ✅ پاسخ کامل API را در لاگ‌ها پیدا کنید

### 3. رفع مشکل بر اساس کد خطا

بعد از شناسایی کد خطا:
- `-10`: IP یا مرچنت کد صحیح نیست → بررسی IP و MerchantId
- `-11`: مرچنت کد فعال نیست → فعال کردن در پنل زرین‌پال
- `-15`: درگاه پرداخت فعال نیست → فعال کردن در پنل زرین‌پال
- سایر کدها: مراجعه به `GetZarinPalErrorMessage`

---

## 📌 نکته مهم

**مشکل احتمالی:** CallbackUrl با `localhost` برای Production معتبر نیست!

اگر در حالت Production هستید (`IsSandbox = false`)، باید:
1. ✅ از Domain واقعی استفاده کنید
2. ✅ CallbackUrl را در پنل زرین‌پال ثبت کنید
3. ✅ یا از Sandbox استفاده کنید (`IsSandbox = true`)

---

**📌 مرجع:** `Docs/PAYMENT_GATEWAY_CONNECTION_ANALYSIS.md`

