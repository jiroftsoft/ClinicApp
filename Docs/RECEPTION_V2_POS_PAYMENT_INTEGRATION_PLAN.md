# 🎯 برنامه یکپارچه‌سازی پرداخت POS در ماژول پذیرش V2

**تاریخ:** 1404/09/11  
**وضعیت:** 🔄 در حال اجرا

---

## 📊 تحلیل وضعیت

### ✅ Backend (نیاز به تغییر ندارد)
- `ReceptionFacade.FinalizePosAsync` فقط `PaymentTransaction` را ثبت می‌کند
- اطلاعات پرداخت از Frontend دریافت می‌شود (RRN, TraceNo, TerminalId, CardLast4)
- منطق پرداخت در Frontend انجام می‌شود

### ❌ Frontend (نیاز به تغییر دارد)
- استفاده از AJAX مستقیم (`/api/v1/pos/process-payment`)
- عدم استفاده از `PosPaymentClient` (SignalR Client-Side)
- Modal قدیمی استفاده می‌شود
- عدم استفاده از Retry Logic و Error Handling حرفه‌ای

---

## 🎯 استراتژی یکپارچه‌سازی

### رویکرد: Client-Side SignalR (طبق مستندات SSP1126)

**جریان جدید:**
```
1. User clicks "ذخیره پذیرش"
   ↓
2. Reception saved
   ↓
3. If POS selected:
   - Initialize PosPaymentClient (SignalR)
   - Open PosPaymentModal (new component)
   ↓
4. User clicks "پرداخت با POS"
   ↓
5. PosPaymentClient.processPayment()
   - Connect to SignalR Hub (localhost:8080)
   - Invoke Initial()
   - Invoke SendAmount1Step()
   ↓
6. POS Device Response (via SignalR callbacks)
   - GetTransactionResponse callback
   - Show success/error/cancel in Modal
   ↓
7. If success:
   - Call PosPaymentApiController to save payment
   - Call ReceptionApiV1Controller to finalize
   ↓
8. Reception finalized
```

---

## 📝 مراحل اجرا

### مرحله 1: اضافه کردن Scripts به BundleConfig ✅
- [x] بررسی BundleConfig
- [ ] اضافه کردن `pos-payment-client.js`
- [ ] اضافه کردن `pos-payment-ui.js`
- [ ] اضافه کردن `jquery.signalR-2.4.2.min.js` (اگر موجود نیست)

### مرحله 2: جایگزینی Modal
- [ ] حذف `_PosPaymentModal.cshtml` قدیمی
- [ ] استفاده از `PosPaymentModal.cshtml` جدید در `Index.cshtml`

### مرحله 3: Refactor payment-panel.js
- [ ] حذف `processPosPayment` (AJAX)
- [ ] حذف `openPosPaymentModal` (قدیمی)
- [ ] اضافه کردن Initialize `PosPaymentClient`
- [ ] اضافه کردن Initialize `PosPaymentUI`
- [ ] استفاده از `PosPaymentClient.processPayment()`
- [ ] استفاده از `PosPaymentUI` برای مدیریت Modal

### مرحله 4: تست
- [ ] تست اتصال SignalR
- [ ] تست پرداخت موفق
- [ ] تست لغو پرداخت
- [ ] تست خطاهای مختلف
- [ ] تست Retry Logic

---

## 🔧 تغییرات فنی

### 1. BundleConfig.cs
```csharp
// اضافه کردن به reception.v2 bundle:
"~/Scripts/jquery.signalR-2.4.2.min.js",
"~/Scripts/pos-payment/pos-payment-client.js",
"~/Scripts/pos-payment/pos-payment-ui.js"
```

### 2. Index.cshtml
```html
@* جایگزینی Modal قدیمی با Modal جدید *@
@Html.Partial("~/Views/Shared/Components/PosPaymentModal.cshtml")
```

### 3. payment-panel.js
```javascript
// Initialize در document.ready:
var posPaymentClient = new PosPaymentClient({
    signalRUrl: '@(System.Configuration.ConfigurationManager.AppSettings["SamanKishSignalRUrl"] ?? "http://localhost:8080/signalr")',
    onSuccess: function(response) {
        // پرداخت موفق - ثبت و نهایی‌سازی
    },
    onError: function(error) {
        // خطا
    }
});

var posPaymentUI = new PosPaymentUI({
    modalId: 'posPaymentModal',
    onConfirm: function() {
        // تأیید و نهایی‌سازی
    }
});

// در openPosPaymentModal:
posPaymentUI.open();
posPaymentUI.setPaymentInfo(amount, terminalName);
posPaymentClient.processPayment(terminalId, amount, ipAddress);
```

---

**آماده برای شروع! 🚀**

