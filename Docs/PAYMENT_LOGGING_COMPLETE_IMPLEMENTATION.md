# ✅ پیاده‌سازی کامل لاگ‌گذاری فرآیند پرداخت

**تاریخ:** 2026-01-07  
**هدف:** افزودن لاگ‌گذاری کامل با CorrelationId در تمام لایه‌ها

---

## 🎯 تغییرات اعمال شده

### 1️⃣ افزودن CorrelationId به تمام لایه‌ها

#### ✅ CreatePaymentRequest
**فایل:** `Interfaces/Payment/Web/IWebPaymentService.cs`

```csharp
public class CreatePaymentRequest
{
    // ... existing properties ...
    /// <summary>
    /// CorrelationId برای Tracing در لاگ‌ها
    /// </summary>
    public string CorrelationId { get; set; }
}
```

#### ✅ PaymentRequest (Gateway Driver)
**فایل:** `Interfaces/Payment/Gateway/Drivers/IGatewayDriver.cs`

```csharp
public class PaymentRequest
{
    // ... existing properties ...
    /// <summary>
    /// CorrelationId برای Tracing در لاگ‌ها
    /// </summary>
    public string CorrelationId { get; set; }
}
```

---

### 2️⃣ انتقال CorrelationId از Controller به Driver

#### ✅ Controller → WebPaymentService
**فایل:** `Areas/Patient/Controllers/AppointmentBookingController.cs`

```csharp
var paymentRequest = new CreatePaymentRequest
{
    // ... existing properties ...
    CorrelationId = correlationId, // ✅ انتقال CorrelationId
};
```

#### ✅ WebPaymentService → Gateway Driver
**فایل:** `Services/Payment/Web/WebPaymentService.cs`

```csharp
var driverRequest = new GatewayPaymentRequest
{
    // ... existing properties ...
    CorrelationId = request.CorrelationId // ✅ انتقال CorrelationId به Driver
};
```

---

### 3️⃣ بهبود لاگ‌گذاری در ZarinPalDriver

#### ✅ لاگ‌های اضافه شده:

1. **شروع فرآیند:**
   ```
   💰 ZarinPal REQUEST: شروع درخواست پرداخت - Amount, Description, CallbackUrl, CorrelationId
   ```

2. **Validation:**
   ```
   ✅ ZarinPal VALIDATION: Validation موفق - Amount, CallbackUrl, CorrelationId
   ⚠️ ZarinPal VALIDATION: Validation ناموفق - Message, CorrelationId
   ```

3. **HTTP Request:**
   ```
   📤 ZarinPal HTTP REQUEST: ارسال درخواست - URL, MerchantId, Amount, CallbackUrl, CorrelationId
   📤 ZarinPal REQUEST BODY: Request Body - RequestBody, CorrelationId
   🔍 ZarinPal CONFIG: IsSandbox, RequestUrl, CallbackUrl, MerchantIdPrefix, GatewayUrl, CorrelationId
   ```

4. **HTTP Response:**
   ```
   📥 ZarinPal HTTP RESPONSE: پاسخ دریافت شد - StatusCode, IsSuccessStatusCode, ContentLength, Duration, Content, CorrelationId
   ```

5. **Response Parsing:**
   ```
   ✅ ZarinPal PARSE: Response Parse موفق - HasErrors, HasData, CorrelationId
   ❌ ZarinPal PARSE ERROR: پاسخ نامعتبر - Content, CorrelationId
   ❌ ZarinPal API ERROR: خطای API - ErrorCode, ErrorMessage, ResponseContent, CorrelationId
   ❌ ZarinPal DATA NULL: data در پاسخ null است - ResponseContent, CorrelationId
   ❌ ZarinPal CODE NULL: code در پاسخ null است - ResponseContent, DataMessage, CorrelationId
   ✅ ZarinPal CODE: Code دریافت شد - Code, Message, CorrelationId
   ```

6. **Success/Failure:**
   ```
   ✅ ZarinPal SUCCESS: درخواست پرداخت موفق - Authority, PaymentUrl, ProcessingTime, CorrelationId
   ⚠️ ZarinPal FAILED: درخواست پرداخت ناموفق - Code, Message, ApiMessage, ResponseContent, ProcessingTime, CorrelationId
   ❌ ZarinPal AUTHORITY NULL: authority در پاسخ null یا خالی است - ResponseContent, CorrelationId
   ```

7. **Exception Handling:**
   ```
   ❌ ZarinPal HTTP EXCEPTION: خطا در ارتباط با درگاه پرداخت - ExceptionType, Message, RequestUrl, ProcessingTime, CorrelationId
   ❌ ZarinPal HTTP EXCEPTION INNER: InnerException - Type, Message, CorrelationId
   ❌ ZarinPal EXCEPTION: خطای غیرمنتظره - ExceptionType, Message, StackTrace, ProcessingTime, CorrelationId
   ❌ ZarinPal EXCEPTION INNER: InnerException - Type, Message, StackTrace, CorrelationId
   ```

---

### 4️⃣ بهبود لاگ‌گذاری در WebPaymentService

#### ✅ لاگ‌های اضافه شده:

1. **شروع فرآیند:**
   ```
   💰 WEB PAYMENT REQUEST: شروع ایجاد درخواست پرداخت - GatewayType, Amount, OnlinePaymentId, CallbackUrl, CorrelationId
   ```

2. **Validation:**
   ```
   ✅ WEB PAYMENT VALIDATION: اعتبارسنجی موفق - Amount, CallbackUrl, CorrelationId
   ⚠️ WEB PAYMENT VALIDATION: اعتبارسنجی ناموفق - Message, CorrelationId
   ```

3. **Gateway Selection:**
   ```
   🔍 WEB PAYMENT GATEWAY SELECTION: شروع انتخاب Gateway - CorrelationId
   ✅ WEB PAYMENT GATEWAY SELECTION: Gateway انتخاب شد - GatewayId, GatewayType, Name, IsSandbox, IsActive, CorrelationId
   ❌ WEB PAYMENT GATEWAY SELECTION: درگاه پرداخت پیش‌فرض یافت نشد - ErrorMessage, CorrelationId
   ```

4. **Driver Support:**
   ```
   ✅ WEB PAYMENT DRIVER SUPPORT: Driver برای GatewayType پشتیبانی می‌شود - GatewayType, CorrelationId
   ❌ WEB PAYMENT DRIVER SUPPORT: GatewayType پشتیبانی نمی‌شود - GatewayType, CorrelationId
   ```

5. **Driver Call:**
   ```
   🔧 WEB PAYMENT DRIVER CALL: فراخوانی CreateGatewayPaymentRequestAsync - GatewayId, GatewayType, Amount, CallbackUrl, CorrelationId
   🔧 WEB PAYMENT GATEWAY REQUEST: شروع CreateGatewayPaymentRequestAsync - GatewayId, GatewayType, Amount, CallbackUrl, CorrelationId
   🔧 WEB PAYMENT DRIVER SELECTED: Driver انتخاب شد - GatewayId, GatewayType, Amount, CallbackUrl, Description, Mobile, Email, CorrelationId
   ```

6. **Driver Response:**
   ```
   🔧 WEB PAYMENT DRIVER RESPONSE: Driver Response - Success, Message, HasData, DataSuccess, ErrorCode, ErrorMessage, Duration, CorrelationId
   📥 WEB PAYMENT DRIVER RESPONSE: پاسخ CreateGatewayPaymentRequestAsync - Success, HasData, Message, Code, ProcessingTime, CorrelationId
   ```

7. **Success/Failure:**
   ```
   ✅ WEB PAYMENT SUCCESS: درخواست پرداخت با موفقیت ایجاد شد - Authority, PaymentUrl, ProcessingTime, CorrelationId
   ❌ WEB PAYMENT DRIVER ERROR: خطا در ایجاد درخواست پرداخت - Success, Message, Code, HasData, DataErrorCode, DataErrorMessage, ProcessingTime, CorrelationId
   ❌ WEB PAYMENT DRIVER FAILED: Driver درخواست پرداخت ناموفق - Success, Message, HasData, Duration, CorrelationId
   ❌ WEB PAYMENT DRIVER DATA FAILED: Driver Data.Success is false - ErrorCode, ErrorMessage, CorrelationId
   ❌ WEB PAYMENT DRIVER ERROR DETAILS: Driver Error Details - ErrorCode, ErrorMessage, CorrelationId
   ```

8. **Validation:**
   ```
   ✅ WEB PAYMENT VALIDATION: PaymentUrl و Authority معتبر هستند - Authority, PaymentUrl, CorrelationId
   ❌ WEB PAYMENT VALIDATION: PaymentUrl is null or empty - Authority, CorrelationId
   ❌ WEB PAYMENT VALIDATION: Authority is null or empty - PaymentUrl, CorrelationId
   ```

9. **Exception Handling:**
   ```
   ❌ WEB PAYMENT EXCEPTION: خطای غیرمنتظره در CreatePaymentRequestAsync - ExceptionType, Message, StackTrace, GatewayType, Amount, ProcessingTime, CorrelationId
   ❌ WEB PAYMENT EXCEPTION INNER: InnerException - Type, Message, StackTrace, CorrelationId
   ❌ WEB PAYMENT GATEWAY EXCEPTION: خطای غیرمنتظره در CreateGatewayPaymentRequestAsync - ExceptionType, Message, StackTrace, GatewayId, GatewayType, ProcessingTime, CorrelationId
   ❌ WEB PAYMENT GATEWAY EXCEPTION INNER: InnerException - Type, Message, StackTrace, CorrelationId
   ```

---

## 📊 لاگ‌های مورد انتظار

### ✅ در صورت موفقیت:

```
💰 WEB PAYMENT REQUEST: شروع ایجاد درخواست پرداخت - CorrelationId: xxx
✅ WEB PAYMENT VALIDATION: اعتبارسنجی موفق - CorrelationId: xxx
🔍 WEB PAYMENT GATEWAY SELECTION: شروع انتخاب Gateway - CorrelationId: xxx
✅ WEB PAYMENT GATEWAY SELECTION: Gateway انتخاب شد - CorrelationId: xxx
✅ WEB PAYMENT DRIVER SUPPORT: Driver پشتیبانی می‌شود - CorrelationId: xxx
🔧 WEB PAYMENT DRIVER CALL: فراخوانی CreateGatewayPaymentRequestAsync - CorrelationId: xxx
🔧 WEB PAYMENT GATEWAY REQUEST: شروع CreateGatewayPaymentRequestAsync - CorrelationId: xxx
🔧 WEB PAYMENT DRIVER SELECTED: Driver انتخاب شد - CorrelationId: xxx
💰 ZarinPal REQUEST: شروع درخواست پرداخت - CorrelationId: xxx
✅ ZarinPal VALIDATION: Validation موفق - CorrelationId: xxx
📤 ZarinPal HTTP REQUEST: ارسال درخواست - CorrelationId: xxx
📥 ZarinPal HTTP RESPONSE: پاسخ دریافت شد - CorrelationId: xxx
✅ ZarinPal PARSE: Response Parse موفق - CorrelationId: xxx
✅ ZarinPal CODE: Code دریافت شد - CorrelationId: xxx
✅ ZarinPal SUCCESS: درخواست پرداخت موفق - CorrelationId: xxx
🔧 WEB PAYMENT DRIVER RESPONSE: Driver Response - CorrelationId: xxx
✅ WEB PAYMENT VALIDATION: PaymentUrl و Authority معتبر هستند - CorrelationId: xxx
✅ WEB PAYMENT GATEWAY SUCCESS: Driver درخواست پرداخت موفق - CorrelationId: xxx
✅ WEB PAYMENT SUCCESS: درخواست پرداخت با موفقیت ایجاد شد - CorrelationId: xxx
```

### ❌ در صورت خطا:

```
💰 WEB PAYMENT REQUEST: شروع ایجاد درخواست پرداخت - CorrelationId: xxx
✅ WEB PAYMENT VALIDATION: اعتبارسنجی موفق - CorrelationId: xxx
🔍 WEB PAYMENT GATEWAY SELECTION: شروع انتخاب Gateway - CorrelationId: xxx
✅ WEB PAYMENT GATEWAY SELECTION: Gateway انتخاب شد - CorrelationId: xxx
✅ WEB PAYMENT DRIVER SUPPORT: Driver پشتیبانی می‌شود - CorrelationId: xxx
🔧 WEB PAYMENT DRIVER CALL: فراخوانی CreateGatewayPaymentRequestAsync - CorrelationId: xxx
🔧 WEB PAYMENT GATEWAY REQUEST: شروع CreateGatewayPaymentRequestAsync - CorrelationId: xxx
🔧 WEB PAYMENT DRIVER SELECTED: Driver انتخاب شد - CorrelationId: xxx
💰 ZarinPal REQUEST: شروع درخواست پرداخت - CorrelationId: xxx
✅ ZarinPal VALIDATION: Validation موفق - CorrelationId: xxx
📤 ZarinPal HTTP REQUEST: ارسال درخواست - CorrelationId: xxx
📥 ZarinPal HTTP RESPONSE: پاسخ دریافت شد - CorrelationId: xxx
✅ ZarinPal PARSE: Response Parse موفق - CorrelationId: xxx
✅ ZarinPal CODE: Code دریافت شد - CorrelationId: xxx
⚠️ ZarinPal FAILED: درخواست پرداخت ناموفق - Code, Message, CorrelationId: xxx
🔧 WEB PAYMENT DRIVER RESPONSE: Driver Response - CorrelationId: xxx
❌ WEB PAYMENT DRIVER ERROR: خطا در ایجاد درخواست پرداخت - CorrelationId: xxx
❌ WEB PAYMENT DRIVER ERROR: خطا در ایجاد درخواست پرداخت در درگاه - CorrelationId: xxx
```

---

## 🔍 نحوه استفاده از CorrelationId

### در لاگ‌ها:

```powershell
# جستجوی لاگ‌های مربوط به یک CorrelationId
Get-Content 'App_Data\Logs\clinicapp-*.log' | Select-String -Pattern '32eb5965-5100-41ca-a50b-1d928ce10e1f'
```

### در کد:

```csharp
// در Controller
var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString("N");

// در WebPaymentService
var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString("N");

// در ZarinPalDriver
var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString("N");
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
   - ✅ Processing Time

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
- `Docs/PAYMENT_FLOW_COMPLETE_ARCHITECTURE.md` - معماری کامل فرآیند پرداخت

---

**نکته:** تمام لاگ‌ها اکنون شامل CorrelationId هستند و می‌توانید تمام مراحل یک درخواست پرداخت را با استفاده از CorrelationId دنبال کنید.

