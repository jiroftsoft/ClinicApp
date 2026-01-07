# 🔍 بررسی کامل سیستم پرداخت - از صفر تا صد

**تاریخ:** 2026-01-06  
**هدف:** بررسی کامل مسیر پرداخت برای شناسایی خطای "خطا در ایجاد درخواست پرداخت در درگاه"

---

## 📋 چک‌لیست بررسی

### 1. مسیر کد (Code Path)

#### مرحله 1: Frontend → ProcessPayment
```
✅ Frontend: confirm-booking.js → processPayment()
   ↓
✅ AJAX POST به /Patient/AppointmentBooking/ProcessPayment
   ↓
✅ Backend: AppointmentBookingController.ProcessPayment()
```

#### مرحله 2: ProcessPayment → WebPaymentService
```
✅ ProcessPayment: ساخت CreatePaymentRequest
   ↓
✅ فراخوانی: _webPaymentService.CreatePaymentRequestAsync(paymentRequest)
   ↓
✅ WebPaymentService: CreatePaymentRequestAsync()
```

#### مرحله 3: WebPaymentService → Gateway Driver
```
✅ WebPaymentService: دریافت Gateway
   ↓
✅ فراخوانی: CreateGatewayPaymentRequestAsync()
   ↓
✅ تبدیل CreatePaymentRequest به GatewayPaymentRequest
   ↓
✅ فراخوانی: _gatewayDriver.RequestPaymentAsync(driverRequest)
```

#### مرحله 4: ZarinPalDriver → API
```
✅ ZarinPalDriver: RequestPaymentAsync()
   ↓
✅ Validation: ValidatePaymentRequest()
   ↓
✅ ساخت Request Body
   ↓
✅ ارسال به https://api.zarinpal.com/pg/v4/payment/request.json
   ↓
✅ Parse Response
   ↓
✅ بررسی errors, data, code, authority
   ↓
✅ ساخت PaymentUrl
```

---

## 🔍 نقاط بررسی (Checkpoints)

### Checkpoint 1: ProcessPayment Action

**فایل:** `Areas/Patient/Controllers/AppointmentBookingController.cs`  
**خط:** 1219

```csharp
var paymentResult = await _webPaymentService.CreatePaymentRequestAsync(paymentRequest);

if (!paymentResult.Success || paymentResult.Data == null)
{
    // ❌ خطا در اینجا رخ می‌دهد
    return Json(new { success = false, message = paymentResult.Message ?? "خطا در ایجاد درخواست پرداخت" });
}
```

**بررسی:**
- [ ] آیا `paymentResult.Success` false است؟
- [ ] آیا `paymentResult.Data` null است؟
- [ ] آیا `paymentResult.Message` چیست؟

**لاگ:**
```
❌ PAYMENT REQUEST: خطا در ایجاد درخواست پرداخت - {ErrorMessage}
```

---

### Checkpoint 2: WebPaymentService.CreatePaymentRequestAsync

**فایل:** `Services/Payment/Web/WebPaymentService.cs`  
**خط:** 416

```csharp
var driverResult = await _gatewayDriver.RequestPaymentAsync(driverRequest);

if (!driverResult.Success || driverResult.Data == null)
{
    // ❌ خطا در اینجا رخ می‌دهد
    return ServiceResult<PaymentGatewayResponse>.Failed(...);
}
```

**بررسی:**
- [ ] آیا `driverResult.Success` false است؟
- [ ] آیا `driverResult.Data` null است؟
- [ ] آیا `driverResult.Message` چیست؟
- [ ] آیا `driverResult.Data.ErrorCode` و `ErrorMessage` چیست؟

**لاگ:**
```
❌ WEB PAYMENT: Driver درخواست پرداخت ناموفق - Success: {Success}, Message: {Message}
❌ WEB PAYMENT: Driver Error Details - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}
```

---

### Checkpoint 3: ZarinPalDriver.RequestPaymentAsync

**فایل:** `Services/Payment/Gateway/Drivers/ZarinPalDriver.cs`  
**خط:** 75

**بررسی Validation:**
- [ ] آیا `ValidatePaymentRequest()` موفق است؟
- [ ] آیا `Amount >= 1000` است؟
- [ ] آیا `CallbackUrl` کامل است (Absolute URI)؟

**بررسی Request:**
- [ ] آیا `merchant_id` درست است؟
- [ ] آیا `amount` به long تبدیل می‌شود؟
- [ ] آیا `callback_url` کامل است؟

**بررسی Response:**
- [ ] آیا `response.StatusCode` OK است؟
- [ ] آیا `responseContent` parse می‌شود؟
- [ ] آیا `zarinPalResponse.errors` null است؟
- [ ] آیا `zarinPalResponse.data` null است؟
- [ ] آیا `zarinPalResponse.data.code` 100 است؟
- [ ] آیا `zarinPalResponse.data.authority` null نیست؟

**لاگ:**
```
💰 ZarinPal: شروع درخواست پرداخت - Amount: {Amount}
📤 ZarinPal: ارسال درخواست به {Url} - MerchantId: {MerchantId}, Amount: {Amount}, CallbackUrl: {CallbackUrl}
📥 ZarinPal: پاسخ دریافت شد - StatusCode: {StatusCode}, Content: {Content}
❌ ZarinPal: خطای API - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}
❌ ZarinPal: data در پاسخ null است
❌ ZarinPal: code در پاسخ null است
⚠️ ZarinPal: درخواست پرداخت ناموفق - Code: {Code}, Message: {Message}
```

---

## 🔍 بررسی Validation

### 1. Amount Validation

**ZarinPalDriver.ValidatePaymentRequest():**
```csharp
if (request.Amount < 1000)
{
    return ServiceResult.Failed("مبلغ پرداخت باید حداقل 1000 ریال باشد");
}
```

**بررسی:**
- [ ] آیا `appointment.Price >= 1000` است؟
- [ ] آیا در `ProcessPayment` مبلغ به 1000 تنظیم می‌شود اگر کمتر باشد؟

---

### 2. CallbackUrl Validation

**ZarinPalDriver.ValidatePaymentRequest():**
```csharp
if (!Uri.IsWellFormedUriString(request.CallbackUrl, UriKind.Absolute))
{
    return ServiceResult.Failed("آدرس Callback نامعتبر است");
}
```

**بررسی:**
- [ ] آیا `callbackUrl` با `http://` یا `https://` شروع می‌شود؟
- [ ] آیا `callbackUrl` کامل است (با Host و Port)؟

**ساخت CallbackUrl در ProcessPayment:**
```csharp
var callbackUrl = Url.Action("PaymentCallback", "AppointmentBooking", new { area = "Patient" }, Request.Url.Scheme);
if (!callbackUrl.StartsWith("http://") && !callbackUrl.StartsWith("https://"))
{
    var scheme = Request.Url.Scheme;
    var host = Request.Url.Host;
    var port = Request.Url.Port != 80 && Request.Url.Port != 443 ? $":{Request.Url.Port}" : "";
    callbackUrl = $"{scheme}://{host}{port}{callbackUrl}";
}
```

---

### 3. Merchant ID Validation

**بررسی:**
- [ ] آیا `ZarinpalMerchantId` در `Web.config` تنظیم شده است؟
- [ ] آیا `MerchantId` فعال است؟
- [ ] آیا `MerchantId` برای Production است (نه Sandbox)؟

---

## 🔍 بررسی خطاهای احتمالی API

### خطاهای رایج زرین‌پال:

1. **-10: IP یا مرچنت کد صحیح نیست**
   - بررسی: آیا `MerchantId` درست است؟
   - بررسی: آیا IP سرور در Whitelist است؟

2. **-11: مرچنت کد فعال نیست**
   - بررسی: آیا `MerchantId` در پنل زرین‌پال فعال است؟

3. **-12: تلاش بیش از حد درخواست**
   - بررسی: آیا Rate Limiting فعال است؟

4. **-35: مبلغ از حد مجاز کمتر است**
   - بررسی: آیا `Amount >= 1000` است؟

5. **-33/-34: درخواست نامعتبر**
   - بررسی: آیا `CallbackUrl` معتبر است؟
   - بررسی: آیا `Description` خالی نیست؟

---

## 📊 لاگ‌های مورد نیاز

### 1. لاگ‌های ProcessPayment:
```
✅ PAYMENT REQUEST: شروع ProcessPayment - AppointmentId: {AppointmentId}
✅ PAYMENT REQUEST: Gateway دریافت شد - GatewayId: {GatewayId}
✅ PAYMENT REQUEST: OnlinePayment ایجاد شد - OnlinePaymentId: {OnlinePaymentId}
🔗 PAYMENT REQUEST: CallbackUrl تنظیم شد - {CallbackUrl}
💰 PAYMENT REQUEST: مبلغ پرداخت - AppointmentId: {AppointmentId}, Amount: {Amount}
❌ PAYMENT REQUEST: خطا در ایجاد درخواست پرداخت - {ErrorMessage}
```

### 2. لاگ‌های WebPaymentService:
```
🔧 WEB PAYMENT: شروع CreateGatewayPaymentRequestAsync
🔧 WEB PAYMENT: فراخوانی Driver - Amount: {Amount}, CallbackUrl: {CallbackUrl}
🔧 WEB PAYMENT: Driver Response - Success: {Success}, Message: {Message}
❌ WEB PAYMENT: Driver درخواست پرداخت ناموفق
❌ WEB PAYMENT: Driver Error Details - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}
```

### 3. لاگ‌های ZarinPalDriver:
```
💰 ZarinPal: شروع درخواست پرداخت - Amount: {Amount}
📤 ZarinPal: ارسال درخواست به {Url} - MerchantId: {MerchantId}, Amount: {Amount}, CallbackUrl: {CallbackUrl}
📥 ZarinPal: پاسخ دریافت شد - StatusCode: {StatusCode}, Content: {Content}
❌ ZarinPal: خطای API - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}
❌ ZarinPal: data در پاسخ null است
❌ ZarinPal: code در پاسخ null است
⚠️ ZarinPal: درخواست پرداخت ناموفق - Code: {Code}, Message: {Message}
✅ ZarinPal: درخواست پرداخت موفق - Authority: {Authority}, PaymentUrl: {PaymentUrl}
```

---

## 🎯 اقدامات بعدی

1. **بررسی لاگ‌های سرور:**
   - جستجو برای `PAYMENT REQUEST`
   - جستجو برای `WEB PAYMENT`
   - جستجو برای `ZarinPal`

2. **بررسی Response از API:**
   - آیا `StatusCode` OK است؟
   - آیا `Content` parse می‌شود؟
   - آیا `errors` یا `data` وجود دارد؟

3. **بررسی تنظیمات:**
   - آیا `IsSandbox = false` است؟
   - آیا `MerchantId` درست است؟
   - آیا `CallbackUrl` کامل است؟

4. **تست دستی:**
   - تست با Postman یا curl
   - بررسی Response از زرین‌پال

---

## ✅ نتیجه

**وضعیت:** در حال بررسی  
**اقدامات:** بررسی لاگ‌های سرور و Response از API

