# 🔍 تحلیل کامل فرایند اتصال به درگاه پرداخت

**تاریخ:** 2026-01-06  
**ماژول:** Payment Gateway Integration  
**نوع:** Bug Analysis & Fix  
**شدت:** High

---

## 📋 خلاصه مشکل

خطای "خطا در ایجاد درخواست پرداخت در درگاه" در فرایند پرداخت نوبت رخ می‌دهد.

**Flow:**
1. ✅ نوبت با موفقیت رزرو می‌شود (AppointmentId: 28)
2. ❌ درخواست پرداخت به درگاه ارسال می‌شود اما خطا می‌دهد

---

## 🔄 فرایند کامل اتصال به درگاه

### مرحله 1: فراخوانی ProcessPayment
**فایل:** `Areas/Patient/Controllers/AppointmentBookingController.cs:972`

```csharp
[HttpPost]
public async Task<ActionResult> ProcessPayment(int appointmentId, ...)
{
    // 1. دریافت نوبت
    var appointment = await _context.Appointments...FirstOrDefaultAsync(...);
    
    // 2. دریافت درگاه پیش‌فرض
    var defaultGatewayResult = await _webPaymentService.GetDefaultPaymentGatewayAsync();
    
    // 3. ایجاد OnlinePayment
    var onlinePayment = new OnlinePayment { ... };
    
    // 4. ایجاد درخواست پرداخت
    var paymentResult = await _webPaymentService.CreatePaymentRequestAsync(paymentRequest);
}
```

### مرحله 2: GetDefaultPaymentGatewayAsync
**فایل:** `Services/Payment/Web/WebPaymentService.cs:862`

**منطق Fallback:**
1. ✅ جستجوی درگاه پیش‌فرض (IsDefault = true)
2. ✅ جستجوی درگاه ZarinPal فعال
3. ✅ جستجوی اولین درگاه فعال
4. ✅ ایجاد خودکار از Web.config (اگر MerchantId موجود باشد)
5. ❌ خطا اگر همه تلاش‌ها ناموفق بود

**نکات مهم:**
- اگر درگاه یافت نشود، خطای "درگاه پرداخت پیش‌فرض یافت نشد" برمی‌گرداند
- اگر MerchantId در Web.config موجود باشد، درگاه خودکار ایجاد می‌شود

### مرحله 3: CreatePaymentRequestAsync
**فایل:** `Services/Payment/Web/WebPaymentService.cs:73`

**مراحل:**
1. ✅ Validation درخواست
2. ✅ دریافت درگاه از Repository (با Cache)
3. ✅ بررسی فعال بودن درگاه
4. ✅ فراخوانی `CreateGatewayPaymentRequestAsync`

### مرحله 4: CreateGatewayPaymentRequestAsync
**فایل:** `Services/Payment/Web/WebPaymentService.cs:346`

**مراحل:**
1. ✅ Validation Gateway (null, IsActive, IsDeleted)
2. ✅ Validation CallbackUrl (null, absolute URI)
3. ✅ Validation Amount (> 0, >= 1000)
4. ✅ تبدیل به `GatewayPaymentRequest`
5. ✅ فراخوانی `_gatewayDriver.RequestPaymentAsync`
6. ✅ بررسی پاسخ Driver
7. ✅ تبدیل به `PaymentGatewayResponse`

**نکات مهم:**
- اگر Driver خطا بدهد، پیام خطای دقیق‌تر برمی‌گرداند
- اگر PaymentUrl یا Authority خالی باشد، خطا می‌دهد

### مرحله 5: ZarinPalDriver.RequestPaymentAsync
**فایل:** `Services/Payment/Gateway/Drivers/ZarinPalDriver.cs:75`

**مراحل:**
1. ✅ Validation درخواست (Amount, CallbackUrl)
2. ✅ ساخت Request Body (merchant_id, amount, description, callback_url)
3. ✅ ارسال HTTP POST به API زرین‌پال
4. ✅ Parse Response
5. ✅ بررسی Status Code (100 = Success)
6. ✅ ساخت PaymentUrl و Authority

**نکات مهم:**
- MerchantId از `ZarinPalHelper.GetMerchantId()` خوانده می‌شود
- اگر MerchantId در Web.config نباشد، Exception می‌دهد
- اگر API خطا بدهد، کد خطا به پیام فارسی تبدیل می‌شود

---

## 🔍 نقاط خطای احتمالی

### 1. درگاه پرداخت یافت نشد
**علت:** 
- هیچ درگاه فعالی در دیتابیس وجود ندارد
- MerchantId در Web.config موجود نیست

**راه‌حل:**
- بررسی وجود درگاه در دیتابیس
- بررسی وجود `ZarinpalMerchantId` در Web.config

### 2. Validation خطا
**علت:**
- Amount < 1000 ریال
- CallbackUrl نامعتبر (نسبی به جای absolute)
- Gateway غیرفعال یا حذف شده

**راه‌حل:**
- ✅ Amount به حداقل 1000 تنظیم می‌شود
- ✅ CallbackUrl کامل می‌شود (با Scheme و Host)
- ✅ Gateway validation اضافه شده

### 3. خطای API زرین‌پال
**علت:**
- MerchantId نامعتبر
- IP نامعتبر
- خطای شبکه یا Timeout

**راه‌حل:**
- بررسی لاگ‌های سرور برای کد خطای دقیق
- بررسی تنظیمات MerchantId
- بررسی اتصال به اینترنت

### 4. خطای Parse Response
**علت:**
- پاسخ API نامعتبر است
- JSON parse خطا می‌دهد

**راه‌حل:**
- بررسی لاگ‌های سرور برای Content پاسخ
- بررسی ساختار JSON پاسخ

---

## ✅ بهبودهای اعمال شده

### 1. بهبود Validation در CreateGatewayPaymentRequestAsync
- ✅ بررسی null بودن Gateway
- ✅ بررسی IsActive و IsDeleted
- ✅ بررسی CallbackUrl (null و absolute URI)
- ✅ بررسی Amount (> 0)
- ✅ بررسی PaymentUrl و Authority در پاسخ

### 2. بهبود Logging
- ✅ لاگ‌های بیشتر در هر مرحله
- ✅ لاگ کردن Exception و InnerException
- ✅ لاگ کردن جزئیات خطا (ErrorCode, ErrorMessage)

### 3. بهبود Error Messages
- ✅ پیام‌های خطای دقیق‌تر
- ✅ برگرداندن ErrorCode از Driver
- ✅ نمایش پیام خطای دقیق از API زرین‌پال

---

## 📝 چک‌لیست بررسی

### قبل از تست:
- [ ] بررسی وجود درگاه پرداخت در دیتابیس
- [ ] بررسی وجود `ZarinpalMerchantId` در Web.config
- [ ] بررسی مقدار `Zarinpal:IsSandbox` در Web.config
- [ ] بررسی اتصال به اینترنت

### در حین تست:
- [ ] بررسی لاگ‌های سرور برای هر مرحله
- [ ] بررسی پاسخ API زرین‌پال
- [ ] بررسی کد خطا (اگر وجود دارد)

### بعد از تست:
- [ ] بررسی لاگ‌های کامل
- [ ] بررسی موفقیت‌آمیز بودن درخواست
- [ ] بررسی دریافت PaymentUrl و Authority

---

## 🔧 دستورالعمل دیباگ

### 1. بررسی لاگ‌های سرور

دنبال این لاگ‌ها باشید:

```
💰 PAYMENT REQUEST: درخواست پردازش پرداخت - AppointmentId: 28
🔍 WEB PAYMENT: شروع جستجوی درگاه پرداخت پیش‌فرض...
✅ WEB PAYMENT: درگاه پیش‌فرض یافت شد - GatewayId: X, Name: Y
💰 WEB PAYMENT: شروع ایجاد درخواست پرداخت در درگاه ZarinPal
🔧 WEB PAYMENT: شروع CreateGatewayPaymentRequestAsync - GatewayId: X, Amount: Y
🔧 WEB PAYMENT: فراخوانی Driver - Amount: Y, CallbackUrl: Z
💰 ZarinPal: شروع درخواست پرداخت - Amount: Y
📤 ZarinPal: ارسال درخواست به https://...
📥 ZarinPal: پاسخ دریافت شد - StatusCode: 200, Content: {...}
```

### 2. بررسی خطاهای احتمالی

**اگر "درگاه پرداخت پیش‌فرض یافت نشد":**
- بررسی وجود درگاه در دیتابیس
- بررسی وجود MerchantId در Web.config

**اگر "خطا در درخواست پرداخت":**
- بررسی لاگ‌های ZarinPal Driver
- بررسی کد خطای API زرین‌پال
- بررسی MerchantId و IP

**اگر "خطا در ارتباط با درگاه پرداخت":**
- بررسی اتصال به اینترنت
- بررسی Timeout
- بررسی Firewall

### 3. بررسی تنظیمات Web.config

```xml
<appSettings>
  <add key="ZarinpalMerchantId" value="YOUR_MERCHANT_ID" />
  <add key="Zarinpal:IsSandbox" value="true" />
</appSettings>
```

---

## 📌 مراجع

- `Services/Payment/Web/WebPaymentService.cs`
- `Services/Payment/Gateway/Drivers/ZarinPalDriver.cs`
- `Helpers/ZarinPalHelper.cs`
- `Areas/Patient/Controllers/AppointmentBookingController.cs`
- `Docs/PAYMENT_ISSUE_PREFLIGHT_ANALYSIS.md`
- `Docs/PAYMENT_DEBUGGING_GUIDE.md`

---

## ✅ نتیجه

**وضعیت:** ✅ بهبودهای اعمال شده

**اقدامات انجام شده:**
- ✅ بهبود Validation در CreateGatewayPaymentRequestAsync
- ✅ بهبود Logging در تمام مراحل
- ✅ بهبود Error Messages
- ✅ ایجاد سند تحلیل کامل

**اقدامات باقی‌مانده:**
- ⏳ بررسی لاگ‌های سرور برای شناسایی خطای دقیق
- ⏳ تست کامل فرایند پرداخت
- ⏳ رفع مشکل بر اساس لاگ‌ها

