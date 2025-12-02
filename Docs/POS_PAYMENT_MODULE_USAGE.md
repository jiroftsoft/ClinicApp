# 📚 راهنمای استفاده از ماژول پرداخت POS

**تاریخ:** 1404/09/11  
**نسخه:** 1.0.0  
**وضعیت:** ✅ Production-Ready

---

## 📋 فهرست مطالب

1. [معرفی](#معرفی)
2. [نصب و راه‌اندازی](#نصب-و-راه‌اندازی)
3. [استفاده در Frontend](#استفاده-در-frontend)
4. [استفاده در Backend](#استفاده-در-backend)
5. [مثال‌های کاربردی](#مثال‌های-کاربردی)
6. [مدیریت خطاها](#مدیریت-خطاها)
7. [بهترین روش‌ها](#بهترین-روش‌ها)

---

## 🎯 معرفی

ماژول پرداخت POS یک راه‌حل کامل و Production-Ready برای پرداخت POS است که می‌تواند در ماژول‌های مختلف (پذیرش، صندوق، و غیره) استفاده شود.

### ✨ ویژگی‌ها

- ✅ **قابل استفاده مجدد**: استفاده در ماژول‌های مختلف
- ✅ **Production-Ready**: آماده برای محیط Production
- ✅ **Client-Side SignalR**: ارتباط مستقیم با SignalR Hub
- ✅ **Error Handling**: مدیریت کامل خطاها
- ✅ **Logging**: لاگ کامل تمام مراحل
- ✅ **User-Friendly**: پیام‌های قابل فهم برای کاربر

---

## 🚀 نصب و راه‌اندازی

### 1. اضافه کردن Scripts به Layout

```html
<!-- در Views/Shared/_Layout.cshtml -->
<script src="~/Scripts/jquery.signalR-2.4.2.min.js"></script>
<script src="~/Scripts/pos-payment/pos-payment-client.js"></script>
```

### 2. تنظیمات در Web.config

```xml
<appSettings>
    <add key="SamanKishSignalRUrl" value="http://localhost:8080/signalr" />
</appSettings>
```

### 3. Dependency Injection (در Startup یا Global.asax)

```csharp
// ثبت Services
container.RegisterType<IPosPaymentService, PosPaymentService>();
container.RegisterType<PosPaymentOrchestrator, PosPaymentOrchestrator>();
```

---

## 💻 استفاده در Frontend

### مثال ساده

```javascript
// ایجاد Instance از PosPaymentClient
var posClient = new PosPaymentClient({
    signalRUrl: 'http://localhost:8080/signalr',
    onSuccess: function(response) {
        console.log('پرداخت موفق!', response);
        // response.rrn, response.traceNo, response.cardLast4, etc.
    },
    onError: function(error) {
        console.error('خطا در پرداخت:', error);
    },
    onCancel: function(response) {
        console.log('پرداخت لغو شد');
    }
});

// پردازش پرداخت
posClient.processPayment(
    terminalId: 1,        // Terminal ID
    amount: 100000,       // Amount in Rials
    ipAddress: '192.168.1.104'  // Terminal IP
);
```

### مثال کامل با UI

```javascript
$(document).ready(function() {
    var posClient = new PosPaymentClient({
        signalRUrl: 'http://localhost:8080/signalr',
        
        onConnecting: function() {
            Swal.fire({
                title: 'در حال اتصال...',
                html: '<div class="text-center"><i class="fas fa-spinner fa-spin fa-3x"></i></div>',
                allowOutsideClick: false,
                showConfirmButton: false
            });
        },
        
        onConnected: function() {
            console.log('✅ Connected to POS Hub');
        },
        
        onCardSwiped: function(data) {
            Swal.update({
                title: 'کارت کشیده شد',
                html: '<div class="text-center"><i class="fas fa-credit-card fa-3x text-success"></i><p class="mt-3">لطفاً رمز کارت را وارد کنید...</p></div>'
            });
        },
        
        onSuccess: function(response) {
            Swal.close();
            
            // نمایش نتیجه موفق
            Swal.fire({
                icon: 'success',
                title: 'پرداخت موفق',
                html: '<div style="text-align: right;">' +
                      '<p><strong>RRN:</strong> ' + response.rrn + '</p>' +
                      '<p><strong>Trace No:</strong> ' + response.traceNo + '</p>' +
                      '<p><strong>Card Last 4:</strong> ' + response.cardLast4 + '</p>' +
                      '</div>',
                confirmButtonText: 'تأیید'
            });
            
            // ثبت پرداخت در دیتابیس
            registerPayment(response);
        },
        
        onCancel: function(response) {
            Swal.close();
            
            Swal.fire({
                icon: 'warning',
                title: 'لغو شد',
                text: 'عملیات توسط کاربر لغو شد',
                confirmButtonText: 'تأیید'
            });
        },
        
        onError: function(error) {
            Swal.close();
            
            Swal.fire({
                icon: 'error',
                title: 'خطا',
                text: error.message,
                confirmButtonText: 'تأیید'
            });
        }
    });
    
    // دکمه پرداخت
    $('#btnPayPOS').on('click', function() {
        var terminalId = $('#terminalSelect').val();
        var amount = parseFloat($('#amountInput').val());
        var ipAddress = $('#terminalSelect').find('option:selected').data('ip');
        
        posClient.processPayment(terminalId, amount, ipAddress);
    });
    
    // ثبت پرداخت در دیتابیس
    function registerPayment(response) {
        $.ajax({
            url: '/api/v1/pos-payment/process',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                receptionId: receptionId,
                amountIRR: response.amount,
                terminalId: response.terminalId,
                rrn: response.rrn,
                traceNo: response.traceNo,
                cardLast4: response.cardLast4
            }),
            success: function(result) {
                if (result.Success) {
                    console.log('پرداخت ثبت شد');
                }
            }
        });
    }
});
```

---

## 🔧 استفاده در Backend

### استفاده از Service

```csharp
public class ReceptionPaymentController : Controller
{
    private readonly IPosPaymentService _posPaymentService;
    
    public ReceptionPaymentController(IPosPaymentService posPaymentService)
    {
        _posPaymentService = posPaymentService;
    }
    
    [HttpPost]
    public async Task<JsonResult> ProcessPosPayment(int receptionId, decimal amount, int? terminalId = null)
    {
        var request = new PosPaymentRequest
        {
            ReceptionId = receptionId,
            AmountIRR = amount,
            TerminalId = terminalId,
            UserId = User.Identity.GetUserId()
        };
        
        var result = await _posPaymentService.ProcessPaymentAsync(request);
        
        if (result.Success)
        {
            return Json(new
            {
                success = true,
                rrn = result.Data.RRN,
                traceNo = result.Data.TraceNo,
                cardLast4 = result.Data.CardLast4
            });
        }
        else
        {
            return Json(new
            {
                success = false,
                message = result.Message
            });
        }
    }
}
```

### استفاده از API Controller

```javascript
// Frontend
$.ajax({
    url: '/api/v1/pos-payment/process',
    method: 'POST',
    contentType: 'application/json',
    data: JSON.stringify({
        receptionId: 123,
        amountIRR: 100000,
        terminalId: 1,
        description: 'پرداخت پذیرش'
    }),
    success: function(response) {
        if (response.Success) {
            console.log('پرداخت موفق:', response.Data);
        }
    }
});
```

---

## 📝 مثال‌های کاربردی

### مثال 1: استفاده در ماژول پذیرش

```javascript
// در Scripts/reception.v2/payment-panel.js

var posPaymentClient = new PosPaymentClient({
    signalRUrl: '@(System.Configuration.ConfigurationManager.AppSettings["SamanKishSignalRUrl"] ?? "http://localhost:8080/signalr")',
    
    onSuccess: function(response) {
        // ثبت پرداخت و نهایی‌سازی پذیرش
        finalizeReceptionAfterPayment(receptionId, response);
    },
    
    onError: function(error) {
        toastr.error('خطا در پرداخت: ' + error.message);
    }
});

function openPosPaymentModal(receptionId, amountIRR) {
    // نمایش Modal
    $('#posPaymentModal').modal('show');
    
    // شروع پرداخت
    var terminalId = getSelectedTerminalId();
    var ipAddress = getTerminalIpAddress(terminalId);
    
    posPaymentClient.processPayment(terminalId, amountIRR, ipAddress);
}
```

### مثال 2: استفاده در ماژول صندوق

```javascript
// در Scripts/cashier/payment.js

var cashierPosClient = new PosPaymentClient({
    signalRUrl: '@(System.Configuration.ConfigurationManager.AppSettings["SamanKishSignalRUrl"] ?? "http://localhost:8080/signalr")',
    
    onSuccess: function(response) {
        // ثبت تراکنش در صندوق
        registerCashierTransaction(response);
    }
});

function processCashierPayment(amount, terminalId) {
    cashierPosClient.processPayment(terminalId, amount, getTerminalIp(terminalId));
}
```

---

## ⚠️ مدیریت خطاها

### کدهای خطای رایج

```javascript
// در onError callback
onError: function(error) {
    switch(error.code) {
        case 'SIGNALR_NOT_LOADED':
            // کتابخانه SignalR بارگذاری نشده
            break;
        case 'HUBS_LOAD_FAILED':
            // بارگذاری Hubs ناموفق - سرویس SSP1126 در حال اجرا نیست
            break;
        case 'NOT_CONNECTED':
            // اتصال به Hub برقرار نیست
            break;
        case 'INITIAL_FAILED':
            // مقداردهی اولیه ناموفق
            break;
        case 'PAYMENT_FAILED':
            // پرداخت ناموفق
            break;
        default:
            // خطای نامشخص
            break;
    }
}
```

### Response Codes از POS

```javascript
// در onSuccess, onCancel, onError
// response.responseCode می‌تواند یکی از این مقادیر باشد:

'0'  یا '00'  // موفق
'98'         // لغو توسط کاربر
'55'         // رمز نامعتبر
'51'         // موجودی ناکافی
'54'         // کارت منقضی شده
'61'         // مبلغ بیش از حد مجاز
'75'         // تعداد تلاش بیش از حد
```

---

## 🎯 بهترین روش‌ها

### 1. مدیریت Connection State

```javascript
var posClient = new PosPaymentClient({...});

// بررسی اتصال قبل از پرداخت
if (!posClient.isConnected) {
    // تلاش برای اتصال مجدد
    posClient._connectToHub();
}
```

### 2. مدیریت Timeout

```javascript
var posClient = new PosPaymentClient({
    paymentTimeout: 120000, // 2 minutes
    // ...
});
```

### 3. Logging

```javascript
var posClient = new PosPaymentClient({
    enableLogging: true,
    enableConsoleLog: true,
    // ...
});
```

### 4. Error Recovery

```javascript
onError: function(error) {
    // Log error
    console.error('Payment error:', error);
    
    // Show user-friendly message
    Swal.fire({
        icon: 'error',
        title: 'خطا',
        text: error.message
    });
    
    // Retry logic (optional)
    if (error.code === 'NOT_CONNECTED' && retryCount < 3) {
        setTimeout(function() {
            retryPayment();
        }, 2000);
    }
}
```

---

## 📞 پشتیبانی

برای سوالات و مشکلات:
- بررسی لاگ‌ها در `C:\Log\`
- بررسی Console در Browser
- بررسی Service Logs

---

**موفق باشید! 🚀**

