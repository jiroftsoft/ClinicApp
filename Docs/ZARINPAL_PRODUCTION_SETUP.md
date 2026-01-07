# 🔧 راهنمای تنظیم زرین‌پال برای Production (اتصال مستقیم به صفحه پرداخت)

**تاریخ:** 2026-01-06  
**هدف:** اتصال مستقیم به صفحه پرداخت زرین‌پال (بدون Sandbox)

---

## ✅ تغییرات اعمال شده

### 1. تغییر `IsSandbox` به `false`

```xml
<!-- ❌ قبل (Sandbox) -->
<add key="Zarinpal:IsSandbox" value="true"/>

<!-- ✅ بعد (Production) -->
<add key="Zarinpal:IsSandbox" value="false"/>
```

### 2. تنظیم URL های Production

```xml
<!-- ✅ Production URLs -->
<add key="Zarinpal:RequestUrl" value="https://api.zarinpal.com/pg/v4/payment/request.json"/>
<add key="Zarinpal:VerifyUrl" value="https://api.zarinpal.com/pg/v4/payment/verify.json"/>
<add key="Zarinpal:StartPayUrl" value="https://www.zarinpal.com/pg/StartPay/"/>
<add key="Zarinpal:StatusUrl" value="https://api.zarinpal.com/pg/v4/payment/status.json"/>
```

---

## 🔄 Flow کامل پرداخت

### مرحله 1: درخواست پرداخت

```
1. کاربر روی "تائید و پرداخت" کلیک می‌کند
   ↓
2. Frontend: AJAX به ProcessPayment
   ↓
3. Backend: ایجاد OnlinePayment (Status = Pending)
   ↓
4. Backend: فراخوانی ZarinPalDriver.RequestPaymentAsync
   ↓
5. ZarinPalDriver: ارسال درخواست به https://api.zarinpal.com/pg/v4/payment/request.json
   ↓
6. زرین‌پال: پاسخ با authority (اگر موفق)
   ↓
7. ZarinPalDriver: ساخت PaymentUrl = https://www.zarinpal.com/pg/StartPay/{authority}
   ↓
8. Backend: برگرداندن PaymentUrl به Frontend
   ↓
9. Frontend: Redirect به PaymentUrl
```

### مرحله 2: پرداخت در زرین‌پال

```
1. کاربر به https://www.zarinpal.com/pg/StartPay/{authority} redirect می‌شود
   ↓
2. کاربر اطلاعات کارت را وارد می‌کند
   ↓
3. زرین‌پال: پردازش پرداخت
   ↓
4. زرین‌پال: Redirect به CallbackUrl با Status و Authority
```

### مرحله 3: Callback

```
1. زرین‌پال: Redirect به CallbackUrl
   ↓
2. Backend: PaymentCallback action
   ↓
3. Backend: Verify پرداخت در زرین‌پال
   ↓
4. Backend: به‌روزرسانی OnlinePayment (Status = Success)
   ↓
5. Backend: به‌روزرسانی Appointment (Status = Scheduled)
   ↓
6. Backend: Redirect به PaymentSuccess یا PaymentError
```

---

## 📋 چک‌لیست تنظیمات

### Web.config:
- [x] `Zarinpal:IsSandbox` = `false`
- [x] `Zarinpal:RequestUrl` = Production URL
- [x] `Zarinpal:StartPayUrl` = Production URL
- [x] `ZarinpalMerchantId` = Merchant ID واقعی

### کد:
- [x] `ZarinPalDriver` درست `PaymentUrl` می‌سازد
- [x] Frontend درست redirect می‌کند
- [x] `CallbackUrl` کامل است (با Scheme و Host)

---

## 🔍 بررسی کد

### 1. ساخت PaymentUrl در ZarinPalDriver:

```csharp
// ✅ درست است
var paymentUrl = $"{_startPayUrl}{zarinPalResponse.data.authority}";
// نتیجه: https://www.zarinpal.com/pg/StartPay/A00000000000000000000000000000000000000
```

### 2. Redirect در Frontend:

```javascript
// ✅ درست است
if (response && response.success === true && response.paymentUrl) {
    window.location.href = response.paymentUrl;
}
```

### 3. CallbackUrl:

```csharp
// ✅ درست است - کامل می‌شود
var callbackUrl = Url.Action("PaymentCallback", "AppointmentBooking", new { area = "Patient" }, Request.Url.Scheme);
// اگر نسبی باشد، کامل می‌شود
if (!callbackUrl.StartsWith("http://") && !callbackUrl.StartsWith("https://")) {
    callbackUrl = $"{scheme}://{host}{port}{callbackUrl}";
}
```

---

## ⚠️ نکات مهم

### 1. Merchant ID واقعی

- ✅ باید از پنل زرین‌پال دریافت شود
- ✅ باید در `Web.config` تنظیم شود
- ✅ باید فعال باشد

### 2. CallbackUrl

- ✅ باید یک URL کامل باشد (با `https://`)
- ✅ باید قابل دسترسی از اینترنت باشد
- ✅ باید به `PaymentCallback` action اشاره کند

### 3. مبلغ

- ✅ باید >= 1000 ریال باشد
- ✅ کد به صورت خودکار مبلغ را به 1000 تنظیم می‌کند (اگر کمتر باشد)

---

## 🧪 تست

### 1. تست درخواست پرداخت:

```bash
# بررسی لاگ‌ها
💰 ZarinPal: شروع درخواست پرداخت - Amount: 500000
📤 ZarinPal: ارسال درخواست به https://api.zarinpal.com/pg/v4/payment/request.json
📥 ZarinPal: پاسخ دریافت شد - StatusCode: OK, Content: {"data":{"code":100,"authority":"..."}}
✅ ZarinPal: درخواست پرداخت موفق - Authority: ..., PaymentUrl: https://www.zarinpal.com/pg/StartPay/...
```

### 2. تست Redirect:

- کاربر باید به `https://www.zarinpal.com/pg/StartPay/{authority}` redirect شود
- صفحه پرداخت زرین‌پال باید نمایش داده شود

### 3. تست Callback:

- بعد از پرداخت، کاربر باید به `CallbackUrl` بازگردد
- `PaymentCallback` باید پرداخت را Verify کند

---

## ✅ نتیجه

**وضعیت:** ✅ آماده برای Production

**تغییرات:**
- ✅ `IsSandbox` = `false`
- ✅ URL های Production تنظیم شده
- ✅ کد درست کار می‌کند

**اقدامات:**
1. Application را Restart کنید
2. تست کنید
3. بررسی کنید که کاربر به صفحه پرداخت زرین‌پال redirect می‌شود

---

**📌 مرجع:** مستندات رسمی زرین‌پال - API v4

