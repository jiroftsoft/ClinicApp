# ✅ رفع مشکل CSP برای SimulatedGateway

**تاریخ:** 2026-01-07  
**وضعیت:** ✅ رفع شد

---

## 🐛 **مشکل:**

```
Sending form data to '<URL>' violates the following Content Security Policy directive: 
"form-action 'self'". The request has been blocked.
```

**علت:** CSP `form-action 'self'` فقط اجازه ارسال فرم به همان دامنه را می‌دهد. اما فرم در حال ارسال به URL خارجی بود (یا JavaScript در حال تغییر `action` بود).

---

## ✅ **راه‌حل:**

### **1. تغییر View برای استفاده از JavaScript:**

**قبل:**
```html
<button type="submit" name="action" value="success">
```

**بعد:**
```html
<button type="button" class="btn btn-success btn-lg" data-action="success">
```

### **2. اضافه کردن JavaScript برای ارسال فرم:**

```javascript
$('#paymentForm button[data-action]').on('click', function(e) {
    e.preventDefault();
    
    var action = $(this).data('action'); // "success" یا "cancel"
    $('#paymentAction').val(action);
    
    // ✅ ارسال فرم به URL داخلی (ProcessPayment)
    // سپس از سرور redirect به callbackUrl انجام می‌شود
    $('#paymentForm').submit();
});
```

### **3. تغییر ساختار فرم:**

- تغییر `type="submit"` به `type="button"` برای دکمه‌ها
- اضافه کردن `data-action` attribute برای مشخص کردن action
- اضافه کردن hidden input برای `action` که توسط JavaScript تنظیم می‌شود

---

## 📋 **تغییرات انجام شده:**

| فایل | تغییرات |
|---|---|
| `Views/Payment/SimulatedGateway/Process.cshtml` | تغییر ساختار فرم و اضافه کردن JavaScript برای ارسال |

---

## 🔍 **توضیحات:**

1. **CSP `form-action 'self'`**: فقط اجازه ارسال فرم به همان دامنه را می‌دهد
2. **فرم به URL داخلی ارسال می‌شود**: `ProcessPayment` یک URL داخلی است
3. **Redirect از سرور**: پس از پردازش، سرور redirect به `callbackUrl` انجام می‌دهد
4. **JavaScript برای کنترل**: استفاده از JavaScript برای کنترل ارسال فرم و جلوگیری از مشکل CSP

---

## 🧪 **تست:**

پس از Restart Application:
1. URL: `http://localhost:3560/Payment/SimulatedGateway/Process?authority=xxx&amount=xxx&callbackUrl=xxx&correlationId=xxx`
2. باید صفحه شبیه‌سازی شده نمایش داده شود
3. روی "پرداخت موفق" کلیک کنید
4. فرم باید به `ProcessPayment` ارسال شود (بدون خطای CSP)
5. سپس باید به Callback URL هدایت شوید

---

**تاریخ ایجاد:** 2026-01-07  
**آخرین به‌روزرسانی:** 2026-01-07

