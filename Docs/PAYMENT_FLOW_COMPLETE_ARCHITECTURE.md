# 🏗️ معماری کامل فرآیند پرداخت - از صفر تا صد

**تاریخ:** 2026-01-07  
**هدف:** طراحی معماری بهتر با لاگ‌گذاری کامل برای شناسایی دقیق خطاها

---

## 📋 فرآیند کامل پرداخت (Complete Payment Flow)

### مرحله 1: Frontend → Controller

```
1. User clicks "تائید و پرداخت" در confirm-booking.js
   ↓
2. confirm-booking.js → processPayment()
   ↓
3. AJAX POST به /Patient/AppointmentBooking/ProcessPayment
   ↓
4. AppointmentBookingController.ProcessPayment()
```

**لاگ‌های مورد نیاز:**
- ✅ CorrelationId ایجاد شده
- ✅ Request دریافت شده (AppointmentId, Amount)
- ✅ Idempotency Check
- ✅ Appointment Validation
- ✅ Security Validation

---

### مرحله 2: Controller → WebPaymentService

```
5. Controller: ساخت CreatePaymentRequest
   ↓
6. Controller: فراخوانی _webPaymentService.CreatePaymentRequestAsync()
   ↓
7. WebPaymentService.CreatePaymentRequestAsync()
   - ValidateCreatePaymentRequestAsync()
   - GetDefaultPaymentGatewayAsync()
   - CreateGatewayPaymentRequestAsync()
```

**لاگ‌های مورد نیاز:**
- ✅ CreatePaymentRequest ساخته شده
- ✅ Validation Result
- ✅ Gateway Selection Result
- ✅ Gateway Details (GatewayId, GatewayType, MerchantId, IsSandbox)

---

### مرحله 3: WebPaymentService → Gateway Driver

```
8. WebPaymentService.CreateGatewayPaymentRequestAsync()
   - Validation (Gateway, CallbackUrl, Amount)
   - تبدیل CreatePaymentRequest به GatewayPaymentRequest
   - انتخاب Driver از Factory
   - فراخوانی driver.RequestPaymentAsync()
```

**لاگ‌های مورد نیاز:**
- ✅ Gateway Validation
- ✅ CallbackUrl Validation
- ✅ Amount Validation
- ✅ Driver Selection
- ✅ GatewayPaymentRequest ساخته شده

---

### مرحله 4: Gateway Driver → ZarinPal API

```
9. ZarinPalDriver.RequestPaymentAsync()
   - ValidatePaymentRequest()
   - ساخت Request Body
   - ارسال HTTP POST به https://api.zarinpal.com/pg/v4/payment/request.json
   - Parse Response
   - بررسی errors, data, code, authority
   - ساخت PaymentUrl
```

**لاگ‌های مورد نیاز:**
- ✅ Request Validation
- ✅ Request Body ساخته شده (بدون MerchantId کامل)
- ✅ HTTP Request Details (URL, Method, Headers)
- ✅ HTTP Response Details (StatusCode, Content)
- ✅ Response Parse Result
- ✅ Errors Check
- ✅ Data Check
- ✅ Code Check
- ✅ Authority Check
- ✅ PaymentUrl ساخته شده

---

### مرحله 5: Gateway Driver → WebPaymentService

```
10. ZarinPalDriver: برگرداندن PaymentRequestResult
    ↓
11. WebPaymentService: تبدیل PaymentRequestResult به PaymentGatewayResponse
    ↓
12. WebPaymentService: برگرداندن ServiceResult<PaymentGatewayResponse>
```

**لاگ‌های مورد نیاز:**
- ✅ Driver Response (Success, HasData, Message, ErrorCode, ErrorMessage)
- ✅ PaymentGatewayResponse ساخته شده
- ✅ ServiceResult برگردانده شده

---

### مرحله 6: WebPaymentService → Controller

```
13. Controller: دریافت ServiceResult<PaymentGatewayResponse>
    ↓
14. Controller: بررسی Success
    ↓
15. Controller: به‌روزرسانی OnlinePayment
    ↓
16. Controller: برگرداندن JSON Response
```

**لاگ‌های مورد نیاز:**
- ✅ PaymentResult دریافت شده
- ✅ OnlinePayment به‌روزرسانی شده
- ✅ JSON Response برگردانده شده

---

### مرحله 7: Controller → Frontend

```
17. Frontend: دریافت JSON Response
    ↓
18. Frontend: بررسی success
    ↓
19. Frontend: Redirect به PaymentUrl (اگر موفق)
    یا نمایش خطا (اگر ناموفق)
```

**لاگ‌های مورد نیاز:**
- ✅ JSON Response ارسال شده
- ✅ Frontend Response دریافت شده

---

## 🔍 نقاط بحرانی (Critical Checkpoints)

### Checkpoint 1: Gateway Selection
**مکان:** `WebPaymentService.GetDefaultPaymentGatewayAsync()`  
**لاگ‌های مورد نیاز:**
- ✅ Default Gateway یافت شده؟
- ✅ ZarinPal Gateway یافت شده؟
- ✅ First Active Gateway یافت شده؟
- ✅ Auto-Create از Web.config انجام شد؟

### Checkpoint 2: Driver Selection
**مکان:** `GatewayDriverFactory.GetDriver()`  
**لاگ‌های مورد نیاز:**
- ✅ Driver Type
- ✅ Gateway Configuration (MerchantId, GatewayUrl, IsSandbox)

### Checkpoint 3: HTTP Request
**مکان:** `ZarinPalDriver.RequestPaymentAsync()`  
**لاگ‌های مورد نیاز:**
- ✅ Request URL
- ✅ Request Body (بدون MerchantId کامل)
- ✅ Request Headers
- ✅ Response StatusCode
- ✅ Response Content

### Checkpoint 4: Response Parsing
**مکان:** `ZarinPalDriver.RequestPaymentAsync()`  
**لاگ‌های مورد نیاز:**
- ✅ Response Parse موفق؟
- ✅ Errors موجود؟
- ✅ Data موجود؟
- ✅ Code موجود؟
- ✅ Authority موجود؟

---

## 🚨 خطاهای احتمالی و محل رخداد

### خطا 1: "خطا در درخواست پرداخت"
**محل احتمالی:**
1. ❌ ZarinPalDriver.RequestPaymentAsync() - خط 308
2. ❌ WebPaymentService.CreateGatewayPaymentRequestAsync() - خط 455
3. ❌ WebPaymentService.CreatePaymentRequestAsync() - خط 132

**لاگ‌های مورد نیاز برای تشخیص:**
- ✅ Exception Type
- ✅ Exception Message
- ✅ Exception StackTrace
- ✅ InnerException (اگر موجود)
- ✅ Request Details (Amount, CallbackUrl, MerchantId)
- ✅ Response Details (اگر HTTP Request انجام شد)

### خطا 2: "درگاه پرداخت پیش‌فرض یافت نشد"
**محل احتمالی:**
1. ❌ WebPaymentService.GetDefaultPaymentGatewayAsync() - خط 1181

**لاگ‌های مورد نیاز برای تشخیص:**
- ✅ Default Gateway Query Result
- ✅ ZarinPal Gateway Query Result
- ✅ First Active Gateway Query Result
- ✅ Auto-Create Attempt Result

### خطا 3: "پاسخ نامعتبر از درگاه پرداخت"
**محل احتمالی:**
1. ❌ ZarinPalDriver.RequestPaymentAsync() - خط 216

**لاگ‌های مورد نیاز برای تشخیص:**
- ✅ Response Content
- ✅ Response StatusCode
- ✅ Parse Exception (اگر موجود)

---

## 🎯 بهبودهای پیشنهادی

### 1. افزودن لاگ‌گذاری کامل در ZarinPalDriver

**قبل:**
```csharp
_logger.Information("💰 ZarinPal: شروع درخواست پرداخت - Amount: {Amount}, Description: {Description}", 
    request.Amount, request.Description);
```

**بعد:**
```csharp
_logger.Information("💰 ZarinPal REQUEST: شروع درخواست پرداخت - Amount: {Amount}, Description: {Description}, CallbackUrl: {CallbackUrl}, Mobile: {Mobile}, Email: {Email}, CorrelationId: {CorrelationId}", 
    request.Amount, request.Description, request.CallbackUrl, request.Mobile, request.Email, correlationId);
```

### 2. افزودن لاگ‌گذاری کامل در WebPaymentService

**قبل:**
```csharp
_logger.Information("💰 WEB PAYMENT: شروع ایجاد درخواست پرداخت در درگاه {GatewayType} برای مبلغ {Amount}", 
    request.GatewayType, request.Amount);
```

**بعد:**
```csharp
_logger.Information("💰 WEB PAYMENT REQUEST: شروع ایجاد درخواست پرداخت - GatewayType: {GatewayType}, Amount: {Amount}, OnlinePaymentId: {OnlinePaymentId}, CallbackUrl: {CallbackUrl}, CorrelationId: {CorrelationId}", 
    request.GatewayType, request.Amount, request.OnlinePaymentId, request.CallbackUrl, correlationId);
```

### 3. افزودن CorrelationId به تمام لاگ‌ها

**قبل:**
```csharp
_logger.Error("❌ ZarinPal: خطا در ارتباط با درگاه پرداخت");
```

**بعد:**
```csharp
_logger.Error("❌ ZarinPal HTTP ERROR: خطا در ارتباط با درگاه پرداخت - ExceptionType: {ExceptionType}, Message: {Message}, RequestUrl: {RequestUrl}, CorrelationId: {CorrelationId}", 
    ex.GetType().Name, ex.Message, _requestUrl, correlationId);
```

---

## 📝 چک‌لیست لاگ‌گذاری

### ✅ لاگ‌های الزامی در هر مرحله:

1. **شروع فرآیند:**
   - ✅ CorrelationId
   - ✅ Input Parameters
   - ✅ Timestamp

2. **Validation:**
   - ✅ Validation Result
   - ✅ Validation Errors (اگر موجود)

3. **External API Call:**
   - ✅ Request URL
   - ✅ Request Body (بدون اطلاعات حساس کامل)
   - ✅ Request Headers
   - ✅ Response StatusCode
   - ✅ Response Content
   - ✅ Response Time

4. **Error Handling:**
   - ✅ Exception Type
   - ✅ Exception Message
   - ✅ Exception StackTrace
   - ✅ InnerException (اگر موجود)
   - ✅ Context (Request Details, Gateway Details)

5. **پایان فرآیند:**
   - ✅ Success/Failure
   - ✅ Result Data (اگر موفق)
   - ✅ Error Message (اگر ناموفق)
   - ✅ Processing Time

---

## 🔗 مراجع

- `Services/Payment/Web/WebPaymentService.cs` - WebPaymentService
- `Services/Payment/Gateway/Drivers/ZarinPalDriver.cs` - ZarinPalDriver
- `Areas/Patient/Controllers/AppointmentBookingController.cs` - Controller
- `Docs/PAYMENT_ERROR_ENHANCED_LOGGING.md` - بهبود لاگ‌گذاری

---

**نکته:** این مستند برای طراحی معماری بهتر و بهبود لاگ‌گذاری ایجاد شده است.

