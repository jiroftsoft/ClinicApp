# 📊 گزارش بررسی ماژول پرداخت POS در ماژول پذیرش

**تاریخ:** 2025-01-27  
**هدف:** بررسی ارسال مبلغ پذیرش (سهم بیمار) به دستگاه کارت‌خوان  
**وضعیت:** ✅ بررسی کامل انجام شد

---

## 📋 خلاصه اجرایی

### ✅ نقاط قوت
1. **محاسبه صحیح مبلغ:** مبلغ `PatientPay` (سهم بیمار) به درستی محاسبه می‌شود
2. **اعتبارسنجی:** مبلغ ارسالی با مبلغ محاسبه شده مقایسه و اعتبارسنجی می‌شود
3. **جریان Frontend:** مبلغ از Frontend به درستی به Backend ارسال می‌شود

### ⚠️ مشکلات شناسایی شده
1. **شبیه‌سازی:** در `PosTerminalApiController.ProcessPayment` فقط شبیه‌سازی شده و واقعاً به دستگاه کارت‌خوان متصل نمی‌شود
2. **عدم ارسال واقعی:** مبلغ `AmountIRR` به دستگاه کارت‌خوان ارسال نمی‌شود

---

## 🔍 تحلیل جریان پرداخت POS

### 1️⃣ جریان Frontend → Backend

#### **مرحله 1: خواندن مبلغ از UI**
```javascript
// Scripts/reception.v2/payment-panel.js:181
let amountIRR = U.parseFaInt($("#PatientPayable").attr("data-value")) || 0;
```

**منبع مبلغ:**
- `#PatientPayable` با attribute `data-value` که از محاسبه `totals.PatientPayableIRR` تنظیم می‌شود
- این مبلغ همان `totals.Data.Totals.Patient` است که از `RecalculateDraftAsync` محاسبه می‌شود

#### **مرحله 2: باز کردن Modal پرداخت POS**
```javascript
// Scripts/reception.v2/payment-panel.js:565
function openPosPaymentModal(receptionId, amountIRR) {
  // نمایش مبلغ در Modal
  $('#posAmount').text(U.toIRR(amountIRR) + ' ریال');
}
```

#### **مرحله 3: پردازش پرداخت**
```javascript
// Scripts/reception.v2/payment-panel.js:651
function processPosPayment(receptionId, amountIRR, terminal) {
  $.ajax({
    url: '/api/v1/pos/process-payment',
    method: 'POST',
    data: JSON.stringify({
      ReceptionId: receptionId,
      AmountIRR: amountIRR,  // ✅ مبلغ سهم بیمار
      PosTerminalId: terminal.posTerminalId
    })
  });
}
```

**✅ نتیجه:** مبلغ `AmountIRR` (سهم بیمار) به درستی از Frontend به Backend ارسال می‌شود.

---

### 2️⃣ جریان Backend: دریافت و پردازش

#### **مرحله 1: دریافت درخواست در Controller**
```csharp
// Controllers/Payment/POS/PosTerminalApiController.cs:300
[HttpPost, Route("process-payment")]
public async Task<ActionResult> ProcessPayment(ProcessPosPaymentRequest request)
{
    // request.AmountIRR = مبلغ سهم بیمار
    if (request == null || request.ReceptionId <= 0 || request.AmountIRR <= 0)
    {
        return Json(ServiceResult.Failed("درخواست نامعتبر است"));
    }
}
```

#### **مرحله 2: دریافت ترمینال پیش‌فرض**
```csharp
// Controllers/Payment/POS/PosTerminalApiController.cs:310
var terminalResult = await _service.GetDefaultPosTerminalAsync();
var terminal = terminalResult.Data;
```

#### **مرحله 3: ⚠️ مشکل: فقط شبیه‌سازی**
```csharp
// Controllers/Payment/POS/PosTerminalApiController.cs:318
// TODO: اینجا باید با دستگاه کارتخوان ارتباط برقرار شود
// برای حال حاضر، یک شبیه‌سازی ساده انجام می‌دهیم

var simulatedResponse = new
{
    success = true,
    rrn = $"RRN{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}",
    traceNo = $"{DateTime.Now:HHmmss}{new Random().Next(100, 999)}",
    terminalId = terminal.TerminalId,
    cardLast4 = $"****{new Random().Next(1000, 9999)}",
    message = "پرداخت با موفقیت انجام شد"
};
```

**❌ مشکل:** مبلغ `request.AmountIRR` به دستگاه کارت‌خوان ارسال نمی‌شود!

---

### 3️⃣ جریان Finalize: نهایی‌سازی پس از پرداخت

#### **مرحله 1: ارسال درخواست Finalize**
```javascript
// Scripts/reception.v2/payment-panel.js:747
function finalizeAfterPayment(receptionId, amountIRR, posData) {
  const payload = {
    receptionId: receptionId,
    amountIRR: amountIRR,  // ✅ مبلغ سهم بیمار
    idempotencyKey: U.guid(),
    pos: {
      rrn: posData.rrn,
      traceNo: posData.traceNo,
      terminalId: posData.terminalId,
      cardLast4: posData.cardLast4
    }
  };
  finalizeReception(payload, true);
}
```

#### **مرحله 2: اعتبارسنجی در ReceptionFacade**
```csharp
// Services/Reception/ReceptionFacade.cs:2497
var totals = await RecalculateDraftAsync(draft);

// ✅ اعتبارسنجی تطابق مبلغ ارسالی با محاسبه شده
if (totals.Data.Totals.Patient != request.AmountIRR)
{
    _logger.Warning("⚠️ FACADE: مبلغ پرداخت با مجموع مطابقت ندارد (POS)");
    return ServiceResult<FinalizeResponse>.Failed("مبلغ پرداخت با مجموع محاسبه شده مطابقت ندارد");
}
```

**✅ نتیجه:** مبلغ در `FinalizePosAsync` به درستی اعتبارسنجی می‌شود.

#### **مرحله 3: ثبت PaymentTransaction**
```csharp
// Services/Reception/ReceptionFacade.cs:2608
var payment = new PaymentTransaction
{
    ReceptionId = request.ReceptionId,
    Amount = request.AmountIRR,  // ✅ مبلغ سهم بیمار
    Status = PaymentStatus.Success,
    Method = PaymentMethod.POS,
    ReferenceCode = request.Pos?.RRN,
    TransactionId = request.Pos?.TraceNo,
    TerminalId = request.Pos?.TerminalId,
    CardLast4 = request.Pos?.CardLast4,
    PosTerminalId = posTerminalId,
    CashSessionId = sessionResult.Data.CashSessionId
};
```

**✅ نتیجه:** مبلغ در دیتابیس به درستی ثبت می‌شود.

---

## ⚠️ مشکلات شناسایی شده

### 🔴 مشکل 1: عدم ارسال واقعی مبلغ به دستگاه کارت‌خوان

**موقعیت:** `Controllers/Payment/POS/PosTerminalApiController.cs:318`

**مشکل:**
```csharp
// TODO: اینجا باید با دستگاه کارتخوان ارتباط برقرار شود
// برای حال حاضر، یک شبیه‌سازی ساده انجام می‌دهیم
```

**تأثیر:**
- مبلغ `request.AmountIRR` به دستگاه کارت‌خوان ارسال نمی‌شود
- فقط یک پاسخ شبیه‌سازی شده برمی‌گردد
- در محیط Production، پرداخت واقعی انجام نمی‌شود

**راه‌حل پیشنهادی:**
1. ایجاد سرویس `IPosDeviceService` برای ارتباط با دستگاه کارت‌خوان
2. پیاده‌سازی سرویس برای انواع مختلف دستگاه‌ها (سامان کیش، آسان پرداخت، ...)
3. ارسال مبلغ `AmountIRR` به دستگاه از طریق این سرویس

---

## ✅ راه‌حل پیشنهادی

### 1️⃣ ایجاد Interface برای سرویس دستگاه کارت‌خوان

```csharp
// Interfaces/Payment/POS/IPosDeviceService.cs
public interface IPosDeviceService
{
    /// <summary>
    /// ارسال مبلغ به دستگاه کارت‌خوان و دریافت پاسخ
    /// </summary>
    Task<ServiceResult<PosPaymentResponse>> ProcessPaymentAsync(
        PosTerminal terminal, 
        decimal amountIRR, 
        int receptionId);
}

public class PosPaymentResponse
{
    public bool Success { get; set; }
    public string RRN { get; set; }
    public string TraceNo { get; set; }
    public string TerminalId { get; set; }
    public string CardLast4 { get; set; }
    public string Message { get; set; }
    public string ErrorCode { get; set; }
}
```

### 2️⃣ پیاده‌سازی سرویس برای انواع مختلف دستگاه‌ها

```csharp
// Services/Payment/POS/PosDeviceService.cs
public class PosDeviceService : IPosDeviceService
{
    private readonly ILogger _logger;
    
    public async Task<ServiceResult<PosPaymentResponse>> ProcessPaymentAsync(
        PosTerminal terminal, 
        decimal amountIRR, 
        int receptionId)
    {
        try
        {
            _logger.Information("🏥 POS: ارسال مبلغ {AmountIRR} ریال به دستگاه {TerminalId}", 
                amountIRR, terminal.TerminalId);
            
            // انتخاب پیاده‌سازی بر اساس Provider
            IPosDeviceDriver driver = GetDriver(terminal.Provider);
            
            // اتصال به دستگاه
            var connectResult = await driver.ConnectAsync(terminal);
            if (!connectResult.Success)
            {
                return ServiceResult<PosPaymentResponse>.Failed(
                    "خطا در اتصال به دستگاه کارت‌خوان: " + connectResult.Message);
            }
            
            // ارسال مبلغ به دستگاه
            var paymentResult = await driver.SendPaymentAsync(terminal, amountIRR);
            
            // قطع اتصال
            await driver.DisconnectAsync(terminal);
            
            if (!paymentResult.Success)
            {
                return ServiceResult<PosPaymentResponse>.Failed(
                    "خطا در پردازش پرداخت: " + paymentResult.Message);
            }
            
            return ServiceResult<PosPaymentResponse>.Successful(new PosPaymentResponse
            {
                Success = true,
                RRN = paymentResult.RRN,
                TraceNo = paymentResult.TraceNo,
                TerminalId = terminal.TerminalId,
                CardLast4 = paymentResult.CardLast4,
                Message = "پرداخت با موفقیت انجام شد"
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "❌ POS: خطا در پردازش پرداخت");
            return ServiceResult<PosPaymentResponse>.Failed("خطا در پردازش پرداخت POS");
        }
    }
    
    private IPosDeviceDriver GetDriver(PosProviderType provider)
    {
        switch (provider)
        {
            case PosProviderType.SamanKish:
                return new SamanKishDriver(_logger);
            case PosProviderType.AsanPardakht:
                return new AsanPardakhtDriver(_logger);
            // سایر Provider ها...
            default:
                throw new NotSupportedException($"Provider {provider} پشتیبانی نمی‌شود");
        }
    }
}
```

### 3️⃣ به‌روزرسانی Controller برای استفاده از سرویس واقعی

```csharp
// Controllers/Payment/POS/PosTerminalApiController.cs:300
[HttpPost, ValidateAntiForgeryTokenOnPosts, Route("process-payment")]
public async Task<ActionResult> ProcessPayment(ProcessPosPaymentRequest request)
{
    try
    {
        if (request == null || request.ReceptionId <= 0 || request.AmountIRR <= 0)
        {
            return Json(ServiceResult.Failed("درخواست نامعتبر است"));
        }

        // دریافت ترمینال
        var terminalResult = await _service.GetDefaultPosTerminalAsync();
        if (!terminalResult.Success || terminalResult.Data == null)
        {
            return Json(ServiceResult.Failed("ترمینال POS پیش‌فرض یافت نشد"));
        }

        var terminal = terminalResult.Data;

        // ✅ ارسال واقعی مبلغ به دستگاه کارت‌خوان
        var paymentResult = await _posDeviceService.ProcessPaymentAsync(
            terminal, 
            request.AmountIRR,  // ✅ مبلغ سهم بیمار
            request.ReceptionId);

        if (!paymentResult.Success)
        {
            return Json(ServiceResult.Failed(paymentResult.Message));
        }

        return Json(ServiceResult<object>.Successful(new
        {
            success = true,
            rrn = paymentResult.Data.RRN,
            traceNo = paymentResult.Data.TraceNo,
            terminalId = paymentResult.Data.TerminalId,
            cardLast4 = paymentResult.Data.CardLast4,
            message = paymentResult.Data.Message
        }));
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "POS: process payment error");
        return Json(ServiceResult.Failed("خطا در پردازش پرداخت POS"));
    }
}
```

### 4️⃣ ثبت سرویس در UnityConfig

```csharp
// App_Start/UnityConfig.cs
container.RegisterType<IPosDeviceService, PosDeviceService>(new PerRequestLifetimeManager());
```

---

## 📊 خلاصه بررسی

### ✅ موارد صحیح
1. **محاسبه مبلغ:** `PatientPay` به درستی محاسبه می‌شود
2. **ارسال Frontend:** مبلغ از Frontend به Backend به درستی ارسال می‌شود
3. **اعتبارسنجی:** مبلغ در `FinalizePosAsync` اعتبارسنجی می‌شود
4. **ثبت دیتابیس:** مبلغ در `PaymentTransaction` به درستی ثبت می‌شود

### ⚠️ موارد نیازمند اصلاح
1. **ارسال به دستگاه:** مبلغ به دستگاه کارت‌خوان ارسال نمی‌شود (فقط شبیه‌سازی)
2. **سرویس واقعی:** نیاز به پیاده‌سازی سرویس واقعی برای ارتباط با دستگاه

---

## 🎯 اقدامات بعدی

### اولویت بالا
1. ✅ ایجاد `IPosDeviceService` Interface
2. ✅ پیاده‌سازی `PosDeviceService` با پشتیبانی از Provider های مختلف
3. ✅ به‌روزرسانی `PosTerminalApiController.ProcessPayment` برای استفاده از سرویس واقعی
4. ✅ ثبت سرویس در UnityConfig

### اولویت متوسط
1. ⚠️ پیاده‌سازی Driver برای هر Provider (سامان کیش، آسان پرداخت، ...)
2. ⚠️ اضافه کردن Retry Logic برای اتصال به دستگاه
3. ⚠️ اضافه کردن Timeout برای عملیات پرداخت

### اولویت پایین
1. ⚠️ اضافه کردن Logging دقیق‌تر
2. ⚠️ اضافه کردن Monitoring برای تراکنش‌های POS
3. ⚠️ اضافه کردن Unit Tests

---

## 📝 نتیجه‌گیری

**وضعیت فعلی:**
- ✅ مبلغ `PatientPay` به درستی محاسبه و ارسال می‌شود
- ❌ مبلغ به دستگاه کارت‌خوان ارسال نمی‌شود (فقط شبیه‌سازی)

**اقدام لازم:**
- پیاده‌سازی سرویس واقعی برای ارتباط با دستگاه کارت‌خوان
- ارسال مبلغ `AmountIRR` به دستگاه از طریق این سرویس

---

**آماده برای پیاده‌سازی:** ✅

