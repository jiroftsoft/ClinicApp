# 📦 ماژول پرداخت POS - JavaScript Modules

**نسخه:** 1.0.0  
**تاریخ:** 1404/09/11

---

## 📁 فایل‌ها

### 1. `pos-payment-client.js`
ماژول اصلی Client-Side برای ارتباط با SignalR Hub

**استفاده:**
```javascript
var posClient = new PosPaymentClient({
    signalRUrl: 'http://localhost:8080/signalr',
    onSuccess: function(response) { ... },
    onError: function(error) { ... }
});

posClient.processPayment(terminalId, amount, ipAddress);
```

### 2. `pos-payment-ui.js`
ماژول مدیریت UI و Modal

**استفاده:**
```javascript
var posUI = new PosPaymentUI({
    modalId: 'posPaymentModal',
    onConfirm: function() { ... }
});

posUI.showSuccess(data);
```

---

## 🔗 وابستگی‌ها

- jQuery 3.x
- jQuery SignalR 2.4.2
- Bootstrap 5.x (برای Modal)
- SweetAlert2 (اختیاری)

---

## 📚 مستندات کامل

برای مستندات کامل، به فایل‌های زیر مراجعه کنید:

- `Docs/POS_PAYMENT_MODULE_USAGE.md` - راهنمای استفاده
- `Docs/POS_PAYMENT_MODULE_INTEGRATION_GUIDE.md` - راهنمای یکپارچه‌سازی

---

**موفق باشید! 🚀**

