# راهنمای Best Practices برای استفاده از SignalR Driver (SSP1126)
**تاریخ:** 1402-06-29  
**کد درخواست:** SSP1126(WEB)

---

## 📋 خلاصه

این راهنما شامل بهترین روش‌های استفاده از SignalR Driver برای پوز سامان کیش در سیستم کلینیک است.

---

## 🎯 1. معماری و طراحی

### 1.1. استفاده از PosPaymentOrchestrator

**✅ بهترین روش:**
```csharp
// استفاده از PosPaymentOrchestrator (Production-Ready)
var paymentResult = await _paymentOrchestrator.ProcessPaymentAsync(
    receptionId: receptionId,
    amountIRR: amountIRR,
    terminalId: terminalId,
    userId: userId);
```

**مزایا:**
- ✅ Retry Logic خودکار (3 بار تلاش)
- ✅ Exponential Backoff
- ✅ Logging کامل
- ✅ Error Handling جامع
- ✅ Transaction Tracking

**❌ روش نامناسب:**
```csharp
// استفاده مستقیم از Driver (نه توصیه می‌شود)
var driver = new SamanKishSignalRDriver(_logger);
await driver.ConnectAsync(terminal);
await driver.SendPaymentAsync(terminal, amount);
```

### 1.2. انتخاب Driver خودکار

**✅ بهترین روش:**
```csharp
// PosDeviceService به صورت خودکار Driver مناسب را انتخاب می‌کند
// بر اساس Protocol ترمینال (TCP/IP یا SignalR)
var paymentResult = await _posDeviceService.ProcessPaymentAsync(
    terminal, 
    amountIRR, 
    receptionId);
```

**مزایا:**
- ✅ انتخاب خودکار Driver
- ✅ پشتیبانی از چندین Protocol
- ✅ قابل توسعه برای Driver های جدید

---

## 🔌 2. مدیریت Connection

### 2.1. Connection Lifecycle

**✅ بهترین روش:**
```csharp
// Driver به صورت خودکار Connection را مدیریت می‌کند
// هر تراکنش یک Connection جدید ایجاد می‌کند و بعد از اتمام قطع می‌شود
```

**مزایا:**
- ✅ جلوگیری از Connection Leak
- ✅ اطمینان از Connection Fresh
- ✅ مدیریت خودکار Resource

### 2.2. Connection Pooling (برای آینده)

**💡 پیشنهاد برای آینده:**
```csharp
// می‌توان Connection Pool ایجاد کرد برای بهبود Performance
// اما فعلاً هر تراکنش یک Connection جدید ایجاد می‌کند (ساده‌تر و امن‌تر)
```

---

## ⚙️ 3. Configuration

### 3.1. تنظیمات SignalR URL

**✅ بهترین روش:**
```xml
<!-- در Web.config -->
<appSettings>
    <add key="SamanKishSignalRUrl" value="http://192.168.1.103:8080/signalr" />
</appSettings>
```

**نکات:**
- ✅ استفاده از IP واقعی به جای localhost در Production
- ✅ Port: 8080 (پیش‌فرض)
- ✅ URL باید قابل دسترس باشد

### 3.2. تنظیمات ترمینال

**✅ بهترین روش:**
```csharp
// در دیتابیس
Terminal.Protocol = PosProtocol.SignalR;  // 4
Terminal.IpAddress = "192.168.1.104";     // IP دستگاه POS
Terminal.TerminalId = "02184080";         // شماره ترمینال
Terminal.MerchantId = "43264519";         // شماره پذیرنده
Terminal.IsActive = true;
```

---

## 🔄 4. Retry Logic

### 4.1. استفاده از PosPaymentOrchestrator

**✅ بهترین روش:**
```csharp
// PosPaymentOrchestrator به صورت خودکار Retry می‌کند
// 3 بار تلاش با Exponential Backoff (1s, 2s, 4s)
var result = await _paymentOrchestrator.ProcessPaymentAsync(...);
```

**مزایا:**
- ✅ Retry خودکار برای خطاهای موقت
- ✅ Exponential Backoff
- ✅ Logging هر تلاش

### 4.2. خطاهای قابل Retry

**✅ خطاهای قابل Retry:**
- Connection Timeout
- Network Error
- Service Unavailable
- Timeout در دریافت پاسخ

**❌ خطاهای غیرقابل Retry:**
- Invalid Amount
- Invalid Terminal
- Card Declined (51, 55, etc.)
- User Cancelled (98)

---

## ⏱️ 5. Timeout Management

### 5.1. Timeout های پیش‌فرض

**✅ Timeout های بهینه:**
```csharp
// در SamanKishSignalRDriver
ConnectionTimeoutMs = 10000;      // 10 seconds
InitializationDelayMs = 1000;      // 1 second
TransactionTimeoutMs = 60000;     // 60 seconds
```

**نکات:**
- ✅ Connection: 10 ثانیه (کافی برای اتصال)
- ✅ Initialization: 3 ثانیه کل (1s delay + 2s wait)
- ✅ Transaction: 60 ثانیه (کافی برای کشیدن کارت و وارد کردن رمز)

### 5.2. تنظیم Timeout در Production

**💡 پیشنهاد:**
```csharp
// می‌توان Timeout ها را از Config خواند
var connectionTimeout = int.Parse(ConfigurationManager.AppSettings["SamanKishConnectionTimeout"] ?? "10000");
```

---

## 🛡️ 6. Error Handling

### 6.1. مدیریت خطاها

**✅ بهترین روش:**
```csharp
try
{
    var result = await _paymentOrchestrator.ProcessPaymentAsync(...);
    if (!result.Success)
    {
        // نمایش پیام خطا به کاربر
        // Log کردن خطا
        // ذخیره در دیتابیس برای بررسی
    }
}
catch (Exception ex)
{
    // Log کردن Exception
    // نمایش پیام عمومی به کاربر
    // Alert به Admin
}
```

### 6.2. پیام‌های خطا

**✅ بهترین روش:**
```csharp
// استفاده از پیام‌های فارسی از ResponseCode
// Driver به صورت خودکار کد خطا را به پیام فارسی تبدیل می‌کند
var errorMessage = GetErrorMessageFromResponseCode(responseCode);
```

**مثال‌ها:**
- "51": "موجودی کافی نمی‌باشد"
- "55": "رمز کارت نامعتبر است"
- "97": "عدم ارتباط با مرکز"

---

## 📊 7. Logging

### 7.1. Logging Strategy

**✅ بهترین روش:**
```csharp
// Driver به صورت خودکار تمام مراحل را Log می‌کند
_logger.Information("🏥 SamanKish SignalR: Starting payment - Amount: {Amount}", amount);
_logger.Error("❌ SamanKish SignalR: Payment failed - ErrorCode: {ErrorCode}", errorCode);
```

**سطح‌های Logging:**
- ✅ Information: مراحل عادی
- ✅ Warning: هشدارها
- ✅ Error: خطاها
- ✅ Debug: جزئیات (فقط در Development)

### 7.2. Logging در Production

**✅ بهترین روش:**
```csharp
// استفاده از Structured Logging (Serilog)
_logger.Information("🏥 Payment - TerminalId: {TerminalId}, Amount: {Amount}, RRN: {RRN}",
    terminalId, amount, rrn);
```

**مزایا:**
- ✅ قابل جستجو
- ✅ قابل فیلتر
- ✅ قابل تحلیل

---

## 🔒 8. Security

### 8.1. Anti-Forgery Token

**✅ بهترین روش:**
```csharp
// در Controller
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<JsonResult> ProcessPayment(...)
{
    // ...
}
```

### 8.2. Validation

**✅ بهترین روش:**
```csharp
// Validation در چند لایه
// 1. Client-side (JavaScript)
// 2. Controller (Model Validation)
// 3. Service (Business Logic)
// 4. Driver (Device Validation)
```

---

## 🧪 9. Testing

### 9.1. Unit Testing

**✅ بهترین روش:**
```csharp
// Mock کردن IPosDeviceService
var mockService = new Mock<IPosDeviceService>();
mockService.Setup(x => x.ProcessPaymentAsync(...))
    .ReturnsAsync(ServiceResult<PosPaymentResponse>.Successful(...));
```

### 9.2. Integration Testing

**✅ بهترین روش:**
```csharp
// استفاده از PosTestController
// 1. تست اتصال
// 2. تست پرداخت با مبلغ کم
// 3. بررسی لاگ‌ها
```

### 9.3. Test Environment

**✅ بهترین روش:**
```csharp
// استفاده از Test Terminal
// Protocol: SignalR
// IP: Test Device IP
// Amount: 1000 Rials (حداقل)
```

---

## 🚀 10. Performance

### 10.1. Connection Reuse (برای آینده)

**💡 پیشنهاد:**
```csharp
// می‌توان Connection Pool ایجاد کرد
// اما فعلاً هر تراکنش یک Connection جدید (ساده‌تر)
```

### 10.2. Async/Await

**✅ بهترین روش:**
```csharp
// همیشه از async/await استفاده کنید
await _paymentOrchestrator.ProcessPaymentAsync(...);
```

**❌ روش نامناسب:**
```csharp
// استفاده از .Result یا .Wait() (Deadlock Risk)
var result = _paymentOrchestrator.ProcessPaymentAsync(...).Result;
```

### 10.3. ConfigureAwait

**✅ بهترین روش:**
```csharp
// در Library Code
await SomeMethodAsync().ConfigureAwait(false);

// در Application Code (UI)
await SomeMethodAsync(); // ConfigureAwait(true) by default
```

---

## 📝 11. Code Organization

### 11.1. Separation of Concerns

**✅ بهترین روش:**
```
Controllers/
  └── ReceptionPaymentController.cs    // HTTP Layer
Services/
  └── Reception/
      └── ReceptionPaymentService.cs    // Business Logic
  └── Payment/
      └── POS/
          └── PosPaymentOrchestrator.cs // Payment Orchestration
          └── PosDeviceService.cs      // Device Communication
          └── Drivers/
              └── SamanKishSignalRDriver.cs // SignalR Implementation
```

### 11.2. Dependency Injection

**✅ بهترین روش:**
```csharp
// در UnityConfig یا DI Container
container.RegisterType<IPosDeviceService, PosDeviceService>();
container.RegisterType<PosPaymentOrchestrator>();
```

---

## 🔍 12. Monitoring

### 12.1. Health Checks

**✅ بهترین روش:**
```csharp
// ایجاد Health Check Endpoint
[HttpGet]
[Route("api/pos/health")]
public async Task<JsonResult> HealthCheck()
{
    // بررسی اتصال به SignalR Hub
    // بررسی وضعیت Windows Service
    // بررسی ترمینال‌های فعال
}
```

### 12.2. Metrics

**✅ بهترین روش:**
```csharp
// ثبت Metrics
- تعداد تراکنش‌های موفق/ناموفق
- زمان متوسط تراکنش
- تعداد Retry
- خطاهای رایج
```

---

## ⚠️ 13. Common Pitfalls

### 13.1. ❌ استفاده مستقیم از Driver

**❌ روش نامناسب:**
```csharp
var driver = new SamanKishSignalRDriver(_logger);
// مشکل: بدون Retry, بدون Error Handling مناسب
```

**✅ روش صحیح:**
```csharp
var result = await _paymentOrchestrator.ProcessPaymentAsync(...);
// مزایا: Retry, Error Handling, Logging
```

### 13.2. ❌ عدم Dispose

**❌ روش نامناسب:**
```csharp
var driver = new SamanKishSignalRDriver(_logger);
// مشکل: Connection Leak
```

**✅ روش صحیح:**
```csharp
using (var driver = new SamanKishSignalRDriver(_logger))
{
    // استفاده
}
// یا استفاده از PosDeviceService که خودش Dispose می‌کند
```

### 13.3. ❌ Blocking Calls

**❌ روش نامناسب:**
```csharp
var result = _paymentOrchestrator.ProcessPaymentAsync(...).Result;
// مشکل: Deadlock Risk
```

**✅ روش صحیح:**
```csharp
var result = await _paymentOrchestrator.ProcessPaymentAsync(...);
```

---

## 📋 14. Checklist برای Production

### 14.1. قبل از Deploy

- ✅ Windows Service نصب و اجرا شده است
- ✅ Port 8080 باز است
- ✅ Firewall تنظیم شده است
- ✅ SignalR URL در Config تنظیم شده است
- ✅ ترمینال‌ها با Protocol = SignalR تنظیم شده‌اند
- ✅ Logging فعال است
- ✅ Health Check Endpoint ایجاد شده است

### 14.2. Monitoring

- ✅ لاگ‌ها بررسی می‌شوند
- ✅ Metrics ثبت می‌شوند
- ✅ Alert برای خطاهای مهم تنظیم شده است

---

## 🎯 15. توصیه‌های نهایی

### 15.1. برای فرم پذیرش

**✅ بهترین روش (استفاده از PosPaymentOrchestrator):**
```csharp
// در Controller یا Service
public async Task<JsonResult> ProcessPosPayment(int receptionId, decimal amountIRR, int? terminalId = null)
{
    try
    {
        // استفاده از PosPaymentOrchestrator (Production-Ready)
        var result = await _paymentOrchestrator.ProcessPaymentAsync(
            receptionId: receptionId,
            amountIRR: amountIRR,
            terminalId: terminalId,
            userId: _currentUserService.UserId);

        if (result.Success)
        {
            // ذخیره اطلاعات پرداخت در دیتابیس
            var paymentTransaction = new PaymentTransaction
            {
                ReceptionId = receptionId,
                Amount = amountIRR,
                Method = PaymentMethod.POS,
                Status = PaymentStatus.Completed,
                ReferenceCode = result.RRN,
                TransactionId = result.TraceNo,
                PosTerminalId = result.TerminalId,
                CreatedByUserId = _currentUserService.UserId,
                CreatedAt = DateTime.UtcNow
            };
            
            await _paymentTransactionRepository.AddAsync(paymentTransaction);
            await _paymentTransactionRepository.SaveChangesAsync();

            // به‌روزرسانی وضعیت پذیرش
            var reception = await _receptionRepository.GetByIdAsync(receptionId);
            if (reception != null)
            {
                var totalPaid = await CalculateTotalPaidAsync(receptionId);
                if (totalPaid >= reception.TotalAmount)
                {
                    reception.Status = ReceptionStatus.Completed;
                    await _receptionRepository.SaveChangesAsync();
                }
            }

            return Json(ServiceResult<object>.Successful(new
            {
                success = true,
                rrn = result.RRN,
                traceNo = result.TraceNo,
                cardLast4 = result.CardLast4,
                message = result.Message
            }));
        }
        else
        {
            // نمایش پیام خطا به کاربر
            _logger.Error("❌ POS Payment Failed - ReceptionId: {ReceptionId}, Error: {Error}",
                receptionId, result.Message);
            
            return Json(ServiceResult.Failed(result.Message));
        }
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "❌ POS Payment Exception - ReceptionId: {ReceptionId}", receptionId);
        return Json(ServiceResult.Failed("خطای غیرمنتظره در پردازش پرداخت"));
    }
}
```

**مزایا:**
- ✅ Retry Logic خودکار (3 بار)
- ✅ Logging کامل
- ✅ Error Handling جامع
- ✅ Transaction Tracking
- ✅ User-Friendly Messages

### 15.2. برای Frontend (JavaScript)

**✅ بهترین روش:**
```javascript
// در payment-panel.js
function processPosPayment(receptionId, amountIRR) {
    // نمایش Loading
    showPosPaymentLoading();
    
    // فراخوانی API
    $.ajax({
        url: '/api/v1/pos/process-payment',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            receptionId: receptionId,
            amountIRR: amountIRR,
            posTerminalId: null // استفاده از ترمینال پیش‌فرض
        }),
        headers: {
            'X-Requested-With': 'XMLHttpRequest',
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        timeout: 120000, // 2 minutes (برای پرداخت POS)
        success: function(response) {
            if (response.success) {
                // نمایش موفقیت
                showPosPaymentSuccess({
                    rrn: response.data.rrn,
                    traceNo: response.data.traceNo,
                    cardLast4: response.data.cardLast4,
                    message: response.data.message
                });
                
                // ذخیره اطلاعات پرداخت
                savePaymentTransaction(receptionId, amountIRR, response.data);
            } else {
                // نمایش خطا
                showPosPaymentError(response.message || 'پرداخت ناموفق بود');
            }
        },
        error: function(xhr, status, error) {
            let errorMessage = 'خطا در ارتباط با سرور';
            
            if (status === 'timeout') {
                errorMessage = 'زمان پردازش پرداخت به پایان رسید. لطفاً مجدداً تلاش کنید.';
            } else if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            }
            
            showPosPaymentError(errorMessage);
        }
    });
}
```

**نکات مهم:**
- ✅ Timeout: 120 ثانیه (کافی برای پرداخت POS)
- ✅ Anti-Forgery Token: همیشه ارسال شود
- ✅ Error Handling: مدیریت خطاهای مختلف
- ✅ User Feedback: نمایش وضعیت به کاربر

### 15.3. برای صندوق

**✅ بهترین روش:**
```csharp
// همان روش فرم پذیرش
// اما با ReceptionId = 0 (برای پرداخت مستقیم)
var result = await _paymentOrchestrator.ProcessPaymentAsync(
    receptionId: 0, // برای پرداخت مستقیم
    amountIRR: amountIRR,
    terminalId: terminalId,
    userId: userId);
```

### 15.3. برای تست

**✅ بهترین روش:**
```csharp
// استفاده از PosTestController
// /PosTest → تست اتصال و پرداخت
```

---

## 📚 16. منابع

- ✅ مستندات SSP1126 (PDF): `Infrastructure/SSP1126(WEB)/SSP1126-WebBased(SignalR)_1_2_1.pdf`
- ✅ نمونه HTML (Web Tester): `Infrastructure/SSP1126(WEB)/Web Tester_1402-06-29/Sample(SSP1126)Page.html`
- ✅ گزارش پیاده‌سازی: `Docs/SAMAN_KISH_POS_DOCUMENTATION_REVIEW.md`
- ✅ کد منبع Driver: `Services/Payment/POS/Drivers/SamanKishSignalRDriver.cs`
- ✅ PosPaymentOrchestrator: `Services/Payment/POS/PosPaymentOrchestrator.cs`
- ✅ PosDeviceService: `Services/Payment/POS/PosDeviceService.cs`
- ✅ PosTestController: `Controllers/Payment/POS/PosTestController.cs`

---

## 🎓 17. خلاصه Quick Reference

### 17.1. ✅ DO (انجام دهید)

1. **استفاده از PosPaymentOrchestrator**
   ```csharp
   var result = await _paymentOrchestrator.ProcessPaymentAsync(...);
   ```

2. **تنظیم Protocol = SignalR در دیتابیس**
   ```sql
   UPDATE PosTerminal SET Protocol = 4 WHERE TerminalId = ...
   ```

3. **تنظیم SignalR URL در Config**
   ```xml
   <add key="SamanKishSignalRUrl" value="http://192.168.1.103:8080/signalr" />
   ```

4. **استفاده از Timeout مناسب در Frontend**
   ```javascript
   timeout: 120000 // 2 minutes
   ```

5. **Logging کامل**
   ```csharp
   _logger.Information("🏥 Payment - Amount: {Amount}", amount);
   ```

### 17.2. ❌ DON'T (انجام ندهید)

1. **استفاده مستقیم از Driver**
   ```csharp
   // ❌ بد
   var driver = new SamanKishSignalRDriver(_logger);
   ```

2. **استفاده از .Result یا .Wait()**
   ```csharp
   // ❌ بد
   var result = _paymentOrchestrator.ProcessPaymentAsync(...).Result;
   ```

3. **عدم Dispose**
   ```csharp
   // ❌ بد
   var driver = new SamanKishSignalRDriver(_logger);
   // بدون Dispose
   ```

4. **Timeout کوتاه**
   ```javascript
   // ❌ بد
   timeout: 10000 // 10 seconds (خیلی کوتاه)
   ```

5. **عدم استفاده از Anti-Forgery Token**
   ```javascript
   // ❌ بد
   // بدون RequestVerificationToken
   ```

---

## 📊 18. مثال کامل End-to-End

### 18.1. Backend (Controller)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
[Route("api/v1/pos/process-payment")]
public async Task<ActionResult> ProcessPayment(ProcessPosPaymentRequest request)
{
    try
    {
        // Validation
        if (request == null || request.ReceptionId <= 0 || request.AmountIRR <= 0)
        {
            return Json(ServiceResult.Failed("درخواست نامعتبر است"));
        }

        // دریافت ترمینال
        var terminalResult = await _posManagementService.GetDefaultPosTerminalAsync();
        if (!terminalResult.Success || terminalResult.Data == null)
        {
            return Json(ServiceResult.Failed("ترمینال POS پیش‌فرض یافت نشد"));
        }

        // پردازش پرداخت با PosPaymentOrchestrator
        var paymentResult = await _paymentOrchestrator.ProcessPaymentAsync(
            receptionId: request.ReceptionId,
            amountIRR: request.AmountIRR,
            terminalId: request.PosTerminalId,
            userId: _currentUserService.UserId);

        if (!paymentResult.Success)
        {
            return Json(ServiceResult.Failed(paymentResult.Message, paymentResult.ErrorCode));
        }

        // ذخیره تراکنش در دیتابیس
        var transaction = new PaymentTransaction
        {
            ReceptionId = request.ReceptionId,
            Amount = request.AmountIRR,
            Method = PaymentMethod.POS,
            Status = PaymentStatus.Completed,
            ReferenceCode = paymentResult.RRN,
            TransactionId = paymentResult.TraceNo,
            PosTerminalId = paymentResult.TerminalId,
            CreatedByUserId = _currentUserService.UserId,
            CreatedAt = DateTime.UtcNow
        };
        
        await _paymentTransactionRepository.AddAsync(transaction);
        await _paymentTransactionRepository.SaveChangesAsync();

        // Response
        return Json(ServiceResult<object>.Successful(new
        {
            success = true,
            rrn = paymentResult.RRN,
            traceNo = paymentResult.TraceNo,
            cardLast4 = paymentResult.CardLast4,
            message = paymentResult.Message
        }));
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "❌ POS Payment Exception");
        return Json(ServiceResult.Failed("خطای غیرمنتظره در پردازش پرداخت"));
    }
}
```

### 18.2. Frontend (JavaScript)

```javascript
function processPosPayment(receptionId, amountIRR) {
    // نمایش Loading
    showPosPaymentLoading();
    
    // فراخوانی API
    $.ajax({
        url: '/api/v1/pos/process-payment',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            receptionId: receptionId,
            amountIRR: amountIRR,
            posTerminalId: null
        }),
        headers: {
            'X-Requested-With': 'XMLHttpRequest',
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        timeout: 120000,
        success: function(response) {
            if (response.success) {
                showPosPaymentSuccess({
                    rrn: response.data.rrn,
                    traceNo: response.data.traceNo,
                    cardLast4: response.data.cardLast4,
                    message: response.data.message
                });
            } else {
                showPosPaymentError(response.message || 'پرداخت ناموفق بود');
            }
        },
        error: function(xhr, status, error) {
            let errorMessage = 'خطا در ارتباط با سرور';
            if (status === 'timeout') {
                errorMessage = 'زمان پردازش پرداخت به پایان رسید';
            } else if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            }
            showPosPaymentError(errorMessage);
        }
    });
}
```

---

**تاریخ:** 1402-06-29  
**وضعیت:** ✅ کامل و آماده استفاده  
**نسخه:** 1.0.0

