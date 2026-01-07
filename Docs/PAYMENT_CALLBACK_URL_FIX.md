# ✅ رفع خطای Callback URL در ZarinPal

**تاریخ:** 2026-01-06  
**مشکل:** "The callback URL domain does not match the registered terminal domain"  
**وضعیت:** ✅ رفع شد

---

## 🐛 مشکل

### خطای مشاهده شده:
```
خطا از درگاه پرداخت: The callback URL domain does not match the registered terminal domain.
```

### علت:
- CallbackUrl از `Request.Url` ساخته می‌شد که در Development = `http://localhost:3560`
- اما در پنل ZarinPal دامنه دیگری ثبت شده است (احتمالاً دامنه Production)
- ZarinPal فقط CallbackUrl هایی را می‌پذیرد که دامنه آن‌ها در پنل ثبت شده باشد

---

## ✅ راه‌حل

### تغییرات اعمال شده:

1. **اضافه شدن PaymentBaseUrl به تنظیمات:**
   - `IAppSettings.PaymentBaseUrl` - Property جدید
   - `AppSettings.PaymentBaseUrl` - پیاده‌سازی
   - خواندن از `Web.config` → `Payment:BaseUrl`

2. **ایجاد PaymentUrlHelper:**
   - `PaymentUrlHelper.BuildPaymentCallbackUrl()` - Helper برای ساخت CallbackUrl
   - منطق:
     - اگر `PaymentBaseUrl` تنظیم شده باشد → استفاده از آن
     - در غیر این صورت → Fallback به `Request.Url`

3. **تغییر AppointmentBookingController:**
   - استفاده از `PaymentUrlHelper` به جای ساخت دستی CallbackUrl
   - 2 محل تغییر یافت:
     - `ProcessPayment` action
     - `Diagnostic` action

4. **به‌روزرسانی Web.config:**
   - اضافه شدن `Payment:BaseUrl` (اختیاری)
   - اگر تنظیم نشده باشد، از `Request.Url` استفاده می‌شود

---

## 🔧 تنظیمات

### Web.config:

```xml
<!-- Payment Base URL Configuration -->
<!-- ✅ Base URL برای ساخت CallbackUrl در درگاه‌های پرداخت -->
<!-- این URL باید با دامنه ثبت شده در پنل ZarinPal مطابقت داشته باشد -->
<!-- مثال Production: https://yourdomain.com (بدون trailing slash) -->
<!-- اگر تنظیم نشده باشد، از Request.Url استفاده می‌شود (Fallback) -->
<!-- ⚠️ برای Development: این مقدار را خالی بگذارید یا localhost را در پنل ZarinPal ثبت کنید -->
<add key="Payment:BaseUrl" value="https://yourdomain.com"/>
```

### برای Production:
```xml
<add key="Payment:BaseUrl" value="https://yourdomain.com"/>
```

### برای Development:
```xml
<!-- خالی بگذارید یا localhost را در پنل ZarinPal ثبت کنید -->
<!-- <add key="Payment:BaseUrl" value=""/> -->
```

---

## 📝 نحوه استفاده

### در Controller:

```csharp
// ✅ قبل (مشکل‌دار):
var callbackUrl = Url.Action("PaymentCallback", "AppointmentBooking", new { area = "Patient" }, Request.Url.Scheme);
if (!callbackUrl.StartsWith("http://") && !callbackUrl.StartsWith("https://"))
{
    var scheme = Request.Url.Scheme;
    var host = Request.Url.Host;
    var port = Request.Url.Port != 80 && Request.Url.Port != 443 ? $":{Request.Url.Port}" : "";
    callbackUrl = $"{scheme}://{host}{port}{callbackUrl}";
}

// ✅ بعد (بهینه):
var callbackRelativePath = Url.Action("PaymentCallback", "AppointmentBooking", new { area = "Patient" });
var callbackUrl = PaymentUrlHelper.BuildPaymentCallbackUrl(callbackRelativePath, Request, _appSettings);
```

---

## 🎯 منطق کار

### PaymentUrlHelper.BuildPaymentCallbackUrl:

1. **بررسی PaymentBaseUrl:**
   - اگر در `Web.config` تنظیم شده باشد → استفاده از آن
   - مثال: `https://yourdomain.com/Patient/AppointmentBooking/PaymentCallback`

2. **Fallback به Request.Url:**
   - اگر `PaymentBaseUrl` تنظیم نشده باشد → استفاده از `Request.Url`
   - مثال: `http://localhost:3560/Patient/AppointmentBooking/PaymentCallback`

---

## ⚠️ نکات مهم

### 1. ثبت دامنه در پنل ZarinPal:

- ✅ دامنه Production باید در پنل ZarinPal ثبت شود
- ✅ برای Development: می‌توانید `localhost` را هم ثبت کنید (یا از Sandbox استفاده کنید)

### 2. تنظیم Payment:BaseUrl:

- ✅ برای Production: حتماً تنظیم کنید
- ✅ برای Development: می‌توانید خالی بگذارید (Fallback به Request.Url)

### 3. Format BaseUrl:

- ✅ باید کامل باشد: `https://yourdomain.com` (بدون trailing slash)
- ❌ نباید نسبی باشد: `/Patient/...`
- ❌ نباید trailing slash داشته باشد: `https://yourdomain.com/`

---

## 📊 فایل‌های تغییر یافته

1. `Interfaces/IAppSettings.cs` - اضافه شدن `PaymentBaseUrl`
2. `Helpers/AppSettings.cs` - پیاده‌سازی `PaymentBaseUrl`
3. `Helpers/PaymentUrlHelper.cs` - Helper جدید
4. `Areas/Patient/Controllers/AppointmentBookingController.cs` - استفاده از Helper
5. `Web.config` - اضافه شدن `Payment:BaseUrl`

---

## ✅ تست

### قبل از رفع:
- ❌ خطا: "The callback URL domain does not match the registered terminal domain"
- ❌ CallbackUrl: `http://localhost:3560/...` (در Development)

### بعد از رفع:
- ✅ CallbackUrl از `PaymentBaseUrl` ساخته می‌شود (اگر تنظیم شده باشد)
- ✅ Fallback به `Request.Url` (اگر تنظیم نشده باشد)
- ✅ لاگ کامل برای Debug

---

## 🔍 بررسی نهایی

### برای Production:

1. ✅ تنظیم `Payment:BaseUrl` در `Web.config`
2. ✅ ثبت دامنه در پنل ZarinPal
3. ✅ تست درخواست پرداخت
4. ✅ بررسی لاگ‌ها

### برای Development:

1. ✅ می‌توانید `Payment:BaseUrl` را خالی بگذارید
2. ✅ یا `localhost` را در پنل ZarinPal ثبت کنید
3. ✅ یا از Sandbox استفاده کنید

---

## 📌 نتیجه

✅ **مشکل رفع شد:**
- CallbackUrl از تنظیمات خوانده می‌شود (اگر تنظیم شده باشد)
- Fallback به Request.Url (اگر تنظیم نشده باشد)
- لاگ کامل برای Debug

**آماده برای Production!** 🚀

---

**مراجع:**
- `Helpers/PaymentUrlHelper.cs` - Helper جدید
- `Web.config` - تنظیمات `Payment:BaseUrl`
- `Docs/PAYMENT_GATEWAY_SETUP_GUIDE.md` - راهنمای تنظیم Gateway

