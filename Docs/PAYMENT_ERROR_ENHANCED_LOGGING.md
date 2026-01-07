# 🔍 بهبود لاگ‌گذاری خطای پرداخت - 2026-01-07

**تاریخ:** 2026-01-07  
**AppointmentId:** 39  
**CorrelationId:** `430ce701-fcea-4fdd-8202-0d9be63222d1`

---

## 📋 خلاصه مشکل

خطای عمومی "خطا در ایجاد درخواست پرداخت در درگاه" بدون جزئیات دقیق در لاگ‌ها ثبت می‌شد، که باعث می‌شد تشخیص علت دقیق خطا دشوار باشد.

---

## 🔧 تغییرات اعمال شده

### 1️⃣ بهبود لاگ‌گذاری در `CreatePaymentRequestAsync`

**فایل:** `Services/Payment/Web/WebPaymentService.cs`  
**خط:** 115-143

**تغییرات:**
1. ✅ افزودن لاگ قبل از فراخوانی `CreateGatewayPaymentRequestAsync`
2. ✅ افزودن لاگ بعد از دریافت پاسخ از `CreateGatewayPaymentRequestAsync`
3. ✅ بهبود لاگ خطا با جزئیات بیشتر (Success, HasData, Message, Code, DataErrorCode, DataErrorMessage)
4. ✅ برگرداندن پیام خطای دقیق‌تر از Driver به جای پیام عمومی

**کد اضافه شده:**
```csharp
// ✅ قبل از فراخوانی
_logger.Information("🔧 WEB PAYMENT: فراخوانی CreateGatewayPaymentRequestAsync - GatewayId: {GatewayId}, GatewayType: {GatewayType}, Amount: {Amount}, CallbackUrl: {CallbackUrl}",
    gateway.PaymentGatewayId, gateway.GatewayType, request.Amount, request.CallbackUrl);

// ✅ بعد از دریافت پاسخ
_logger.Information("📥 WEB PAYMENT: پاسخ CreateGatewayPaymentRequestAsync - Success: {Success}, HasData: {HasData}, Message: {Message}, Code: {Code}",
    gatewayResponse.Success, gatewayResponse.Data != null, gatewayResponse.Message, gatewayResponse.Code);

// ✅ لاگ خطای دقیق‌تر
_logger.Error("❌ WEB PAYMENT: خطا در ایجاد درخواست پرداخت در درگاه - Success: {Success}, Message: {Message}, Code: {Code}, HasData: {HasData}, DataErrorCode: {DataErrorCode}, DataErrorMessage: {DataErrorMessage}",
    gatewayResponse.Success, gatewayResponse.Message, gatewayResponse.Code, gatewayResponse.Data != null, 
    gatewayResponse.Data?.ErrorCode, gatewayResponse.Data?.ErrorMessage);

// ✅ برگرداندن پیام خطای دقیق‌تر
var errorMessage = gatewayResponse.Data?.ErrorMessage ?? gatewayResponse.Message ?? "خطا در ایجاد درخواست پرداخت در درگاه";
return ServiceResult<PaymentGatewayResponse>.Failed(errorMessage, gatewayResponse.Data?.ErrorCode ?? gatewayResponse.Code);
```

---

### 2️⃣ بهبود لاگ‌گذاری در `CreateGatewayPaymentRequestAsync`

**فایل:** `Services/Payment/Web/WebPaymentService.cs`  
**خط:** 514-527

**تغییرات:**
1. ✅ افزودن GatewayId و GatewayType به لاگ Exception
2. ✅ افزودن StackTrace برای InnerException
3. ✅ برگرداندن پیام خطای دقیق‌تر با InnerException

**کد اضافه شده:**
```csharp
_logger.Error(ex, "❌ WEB PAYMENT: خطای غیرمنتظره در CreateGatewayPaymentRequestAsync - ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}, GatewayId: {GatewayId}, GatewayType: {GatewayType}",
    ex.GetType().Name, ex.Message, ex.StackTrace, gateway?.PaymentGatewayId, gateway?.GatewayType);

if (ex.InnerException != null)
{
    _logger.Error("❌ WEB PAYMENT: InnerException - Type: {Type}, Message: {Message}, StackTrace: {StackTrace}",
        ex.InnerException.GetType().Name, ex.InnerException.Message, ex.InnerException.StackTrace);
}

// ✅ برگرداندن پیام خطای دقیق‌تر
var errorMessage = $"خطا در ایجاد درخواست پرداخت در درگاه: {ex.Message}";
if (ex.InnerException != null)
{
    errorMessage += $" (InnerException: {ex.InnerException.Message})";
}

return ServiceResult<PaymentGatewayResponse>.Failed(errorMessage, "GATEWAY_REQUEST_EXCEPTION");
```

---

### 3️⃣ بهبود لاگ‌گذاری در catch block اصلی `CreatePaymentRequestAsync`

**فایل:** `Services/Payment/Web/WebPaymentService.cs`  
**خط:** 139-143

**تغییرات:**
1. ✅ افزودن جزئیات بیشتر به لاگ Exception (ExceptionType, Message, StackTrace, GatewayType, Amount)
2. ✅ افزودن لاگ InnerException با StackTrace
3. ✅ برگرداندن پیام خطای دقیق‌تر با InnerException

**کد اضافه شده:**
```csharp
_logger.Error(ex, "❌ WEB PAYMENT: خطای غیرمنتظره در CreatePaymentRequestAsync - ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}, GatewayType: {GatewayType}, Amount: {Amount}",
    ex.GetType().Name, ex.Message, ex.StackTrace, request.GatewayType, request.Amount);

if (ex.InnerException != null)
{
    _logger.Error("❌ WEB PAYMENT: InnerException - Type: {Type}, Message: {Message}, StackTrace: {StackTrace}",
        ex.InnerException.GetType().Name, ex.InnerException.Message, ex.InnerException.StackTrace);
}

// ✅ برگرداندن پیام خطای دقیق‌تر
var errorMessage = $"خطا در ایجاد درخواست پرداخت در درگاه: {ex.Message}";
if (ex.InnerException != null)
{
    errorMessage += $" (InnerException: {ex.InnerException.Message})";
}

return ServiceResult<PaymentGatewayResponse>.Failed(errorMessage, "PAYMENT_REQUEST_EXCEPTION");
```

---

## 🎯 مزایا

### ✅ قبل از تغییرات:
- ❌ پیام خطای عمومی: "خطا در ایجاد درخواست پرداخت در درگاه"
- ❌ بدون جزئیات دقیق از Driver
- ❌ بدون StackTrace برای InnerException
- ❌ بدون GatewayId و GatewayType در لاگ Exception

### ✅ بعد از تغییرات:
- ✅ پیام خطای دقیق از Driver (ErrorMessage)
- ✅ ErrorCode از Driver
- ✅ جزئیات کامل در لاگ (Success, HasData, Message, Code, DataErrorCode, DataErrorMessage)
- ✅ StackTrace برای Exception و InnerException
- ✅ GatewayId و GatewayType در لاگ Exception
- ✅ Amount و CallbackUrl در لاگ

---

## 📝 لاگ‌های مورد انتظار

بعد از Restart Application، لاگ‌های زیر باید در فایل لاگ ظاهر شوند:

### ✅ در صورت موفقیت:
```
🔧 WEB PAYMENT: فراخوانی CreateGatewayPaymentRequestAsync - GatewayId: 2, GatewayType: ZarinPal, Amount: 100000, CallbackUrl: https://mehranyad.ir/...
📥 WEB PAYMENT: پاسخ CreateGatewayPaymentRequestAsync - Success: true, HasData: true, Message: ...
✅ WEB PAYMENT: Driver درخواست پرداخت موفق - Authority: ..., PaymentUrl: ...
```

### ❌ در صورت خطا:
```
🔧 WEB PAYMENT: فراخوانی CreateGatewayPaymentRequestAsync - GatewayId: 2, GatewayType: ZarinPal, Amount: 100000, CallbackUrl: https://mehranyad.ir/...
📥 WEB PAYMENT: پاسخ CreateGatewayPaymentRequestAsync - Success: false, HasData: true, Message: ..., Code: ...
❌ WEB PAYMENT: خطا در ایجاد درخواست پرداخت در درگاه - Success: false, Message: ..., Code: ..., HasData: true, DataErrorCode: ..., DataErrorMessage: ...
```

### ❌ در صورت Exception:
```
❌ WEB PAYMENT: خطای غیرمنتظره در CreatePaymentRequestAsync - ExceptionType: HttpRequestException, Message: ..., StackTrace: ..., GatewayType: ZarinPal, Amount: 100000
❌ WEB PAYMENT: InnerException - Type: ..., Message: ..., StackTrace: ...
```

---

## 🚀 مراحل بعدی

1. ✅ Application را Restart کنید
2. ✅ یک درخواست پرداخت جدید ایجاد کنید
3. ✅ لاگ‌های جدید را بررسی کنید
4. ✅ خطای دقیق را شناسایی کنید

---

## 🔗 مراجع

- `Services/Payment/Web/WebPaymentService.cs` - بهبود لاگ‌گذاری
- `Docs/PAYMENT_ERROR_DEBUGGING_GUIDE.md` - راهنمای دیباگ خطای پرداخت
- `Docs/CRITICAL_ISSUES_FIX_REPORT.md` - گزارش مشکلات بحرانی

---

**نکته:** این تغییرات برای بهبود دیباگ خطای پرداخت با CorrelationId `430ce701-fcea-4fdd-8202-0d9be63222d1` و AppointmentId 39 ایجاد شده است.

