# 🔗 راهنمای یکپارچه‌سازی ماژول پرداخت POS

**تاریخ:** 1404/09/11  
**نسخه:** 1.0.0  
**هدف:** راهنمای استفاده از ماژول در ماژول‌های پذیرش و صندوق

---

## 📋 فهرست مطالب

1. [یکپارچه‌سازی در ماژول پذیرش](#یکپارچه‌سازی-در-ماژول-پذیرش)
2. [یکپارچه‌سازی در ماژول صندوق](#یکپارچه‌سازی-در-ماژول-صندوق)
3. [مثال‌های کامل](#مثال‌های-کامل)
4. [Troubleshooting](#troubleshooting)

---

## 🏥 یکپارچه‌سازی در ماژول پذیرش

### مرحله 1: اضافه کردن Scripts به Layout یا View

```html
<!-- در Views/ReceptionV2/Index.cshtml یا _Layout.cshtml -->
@section Scripts {
    <script src="~/Scripts/jquery.signalR-2.4.2.min.js"></script>
    <script src="~/Scripts/pos-payment/pos-payment-client.js"></script>
    <script src="~/Scripts/pos-payment/pos-payment-ui.js"></script>
}
```

### مرحله 2: اضافه کردن Modal Component

```html
<!-- در Views/ReceptionV2/Index.cshtml -->
@Html.Partial("~/Views/Shared/Components/PosPaymentModal.cshtml")
```

### مرحله 3: اضافه کردن Button (اختیاری)

```html
<!-- در Views/ReceptionV2/Partials/_Payment.cshtml -->
@Html.Partial("~/Views/Shared/Components/PosPaymentButton.cshtml", new { 
    ButtonId = "BtnPayPOS",
    ButtonText = "پرداخت با POS",
    ButtonClass = "btn-primary",
    FullWidth = true
})
```

### مرحله 4: پیاده‌سازی JavaScript

```javascript
// در Scripts/reception.v2/payment-panel.js

// Initialize POS Payment Client
var posPaymentClient = null;
var posPaymentUI = null;

$(document).ready(function() {
    // تنظیمات SignalR URL
    var signalRUrl = '@(System.Configuration.ConfigurationManager.AppSettings["SamanKishSignalRUrl"] ?? "http://localhost:8080/signalr")';
    
    // ایجاد Instance از PosPaymentClient
    posPaymentClient = new PosPaymentClient({
        signalRUrl: signalRUrl,
        
        onConnecting: function() {
            posPaymentUI.showLoading('در حال اتصال...', 'در حال اتصال به دستگاه کارتخوان', 'لطفاً صبر کنید');
        },
        
        onConnected: function() {
            console.log('✅ Connected to POS Hub');
        },
        
        onCardSwiped: function(data) {
            posPaymentUI.showLoading('کارت کشیده شد', 'لطفاً رمز کارت را وارد کنید', '');
        },
        
        onSuccess: function(response) {
            posPaymentUI.showSuccess({
                rrn: response.rrn,
                traceNo: response.traceNo,
                terminalId: response.terminalId,
                cardLast4: response.cardLast4,
                amount: response.amount,
                txnDate: response.txnDate
            });
            
            // ثبت پرداخت در دیتابیس
            registerPosPayment(receptionId, response);
        },
        
        onCancel: function(response) {
            posPaymentUI.showCanceled();
        },
        
        onError: function(error) {
            posPaymentUI.showError(error.message, error.code);
        }
    });
    
    // ایجاد Instance از PosPaymentUI
    posPaymentUI = new PosPaymentUI({
        modalId: 'posPaymentModal',
        onConfirm: function() {
            // تأیید و نهایی‌سازی پذیرش
            finalizeReceptionAfterPayment(receptionId);
        },
        onPrint: function() {
            // چاپ قبض
            printReceipt(receptionId);
        },
        onRetry: function() {
            // تلاش مجدد
            retryPayment();
        },
        onCancel: function() {
            // انصراف
            posPaymentUI.close();
        }
    });
    
    // دکمه پرداخت POS
    $('#BtnPayPOS').on('click', function() {
        var receptionId = getCurrentReceptionId();
        var amount = getPatientPayableAmount();
        var terminalId = getSelectedTerminalId();
        var ipAddress = getTerminalIpAddress(terminalId);
        
        // نمایش Modal
        posPaymentUI.open();
        posPaymentUI.setPaymentInfo(amount, getTerminalName(terminalId));
        
        // شروع پرداخت
        posPaymentClient.processPayment(terminalId, amount, ipAddress);
    });
    
    // ثبت پرداخت در دیتابیس
    function registerPosPayment(receptionId, response) {
        $.ajax({
            url: '/api/v1/pos-payment/process',
            method: 'POST',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': getAntiForgeryToken()
            },
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
                    console.log('✅ Payment registered successfully');
                }
            },
            error: function(xhr, status, error) {
                console.error('❌ Error registering payment:', error);
            }
        });
    }
});
```

---

## 💰 یکپارچه‌سازی در ماژول صندوق

### مرحله 1: اضافه کردن Scripts

```html
<!-- در Views/Cashier/Index.cshtml -->
@section Scripts {
    <script src="~/Scripts/jquery.signalR-2.4.2.min.js"></script>
    <script src="~/Scripts/pos-payment/pos-payment-client.js"></script>
    <script src="~/Scripts/pos-payment/pos-payment-ui.js"></script>
}
```

### مرحله 2: اضافه کردن Modal

```html
@Html.Partial("~/Views/Shared/Components/PosPaymentModal.cshtml")
```

### مرحله 3: پیاده‌سازی JavaScript

```javascript
// در Scripts/cashier/payment.js

var cashierPosClient = new PosPaymentClient({
    signalRUrl: '@(System.Configuration.ConfigurationManager.AppSettings["SamanKishSignalRUrl"] ?? "http://localhost:8080/signalr")',
    
    onSuccess: function(response) {
        // ثبت تراکنش در صندوق
        registerCashierTransaction(response);
    },
    
    onError: function(error) {
        toastr.error('خطا در پرداخت: ' + error.message);
    }
});

function processCashierPayment(amount, terminalId) {
    var ipAddress = getTerminalIp(terminalId);
    cashierPosClient.processPayment(terminalId, amount, ipAddress);
}
```

---

## 📝 مثال کامل: استفاده در ماژول پذیرش

### فایل: `Scripts/reception.v2/pos-payment-integration.js`

```javascript
/**
 * Integration Module for POS Payment in Reception V2
 */

(function(API, U) {
    'use strict';
    
    var posPaymentClient = null;
    var posPaymentUI = null;
    var currentReceptionId = null;
    
    $(document).ready(function() {
        initializePosPayment();
    });
    
    function initializePosPayment() {
        var signalRUrl = '@(System.Configuration.ConfigurationManager.AppSettings["SamanKishSignalRUrl"] ?? "http://localhost:8080/signalr")';
        
        // Initialize Client
        posPaymentClient = new PosPaymentClient({
            signalRUrl: signalRUrl,
            onSuccess: handlePaymentSuccess,
            onError: handlePaymentError,
            onCancel: handlePaymentCancel,
            onCardSwiped: handleCardSwiped
        });
        
        // Initialize UI
        posPaymentUI = new PosPaymentUI({
            modalId: 'posPaymentModal',
            onConfirm: handlePaymentConfirm,
            onPrint: handlePaymentPrint,
            onRetry: handlePaymentRetry
        });
    }
    
    function openPosPayment(receptionId, amount) {
        currentReceptionId = receptionId;
        
        // Get terminal info
        var terminalId = getSelectedTerminalId();
        var ipAddress = getTerminalIpAddress(terminalId);
        var terminalName = getTerminalName(terminalId);
        
        // Show modal
        posPaymentUI.open();
        posPaymentUI.setPaymentInfo(amount, terminalName);
        
        // Start payment
        posPaymentClient.processPayment(terminalId, amount, ipAddress);
    }
    
    function handlePaymentSuccess(response) {
        posPaymentUI.showSuccess(response);
        
        // Register payment
        registerPayment(currentReceptionId, response);
    }
    
    function handlePaymentError(error) {
        posPaymentUI.showError(error.message, error.code);
    }
    
    function handlePaymentCancel(response) {
        posPaymentUI.showCanceled();
    }
    
    function handleCardSwiped(data) {
        posPaymentUI.showLoading('کارت کشیده شد', 'لطفاً رمز کارت را وارد کنید', '');
    }
    
    function handlePaymentConfirm() {
        // Finalize reception
        finalizeReception(currentReceptionId);
    }
    
    function handlePaymentPrint() {
        // Print receipt
        printReceipt(currentReceptionId);
    }
    
    function handlePaymentRetry() {
        // Retry payment
        var amount = getPatientPayableAmount();
        openPosPayment(currentReceptionId, amount);
    }
    
    function registerPayment(receptionId, response) {
        return API.ajaxWithFallback({
            url: '/api/v1/pos-payment/process',
            method: 'POST',
            data: {
                receptionId: receptionId,
                amountIRR: response.amount,
                terminalId: response.terminalId,
                rrn: response.rrn,
                traceNo: response.traceNo,
                cardLast4: response.cardLast4
            }
        });
    }
    
    // Export functions
    window.ReceptionPosPayment = {
        open: openPosPayment,
        client: function() { return posPaymentClient; },
        ui: function() { return posPaymentUI; }
    };
    
})(window.ReceptionAPI, window.Utilities);
```

---

## 🔧 Troubleshooting

### مشکل 1: SignalR Connection Failed

**علت:** سرویس SSP1126 در حال اجرا نیست

**راه حل:**
```powershell
Get-Service -Name "SSP1126Service1"
Start-Service -Name "SSP1126Service1"
```

### مشکل 2: CSP Error

**علت:** Content Security Policy مانع از بارگذاری Scripts می‌شود

**راه حل:** بررسی `Web.config` و `_Layout.cshtml` برای CSP Settings

### مشکل 3: Terminal Not Found

**علت:** ترمینال در دیتابیس یافت نشد یا Protocol = SignalR نیست

**راه حل:**
```sql
-- بررسی Protocol
SELECT PosTerminalId, TerminalId, Protocol, IsActive 
FROM PosTerminal 
WHERE PosTerminalId = 1;

-- تنظیم Protocol به SignalR
UPDATE PosTerminal 
SET Protocol = 4 
WHERE PosTerminalId = 1;
```

---

## ✅ Checklist یکپارچه‌سازی

- [ ] Scripts به Layout/View اضافه شده
- [ ] Modal Component اضافه شده
- [ ] Button Component اضافه شده (اختیاری)
- [ ] JavaScript Integration پیاده‌سازی شده
- [ ] Event Handlers تنظیم شده
- [ ] API Calls پیاده‌سازی شده
- [ ] Error Handling اضافه شده
- [ ] تست شده

---

**موفق باشید! 🚀**

