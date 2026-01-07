# 🔍 راهنمای عیب‌یابی بحرانی پرداخت

## ⚠️ **مهم: قبل از هر چیز**

**Application باید Restart شود** تا تغییرات لاگ‌گذاری اعمال شوند!

---

## 📋 **مراحل عیب‌یابی**

### 1️⃣ **Restart Application**
```powershell
# در IIS یا Visual Studio
# Application را Restart کنید
```

### 2️⃣ **جستجوی لاگ‌ها با CorrelationId**

```powershell
# استفاده از اسکریپت موجود
.\Scripts\FindPaymentError.ps1 -CorrelationId "YOUR_CORRELATION_ID"

# یا دستی
Get-Content 'App_Data\Logs\*.log' | Select-String -Pattern "YOUR_CORRELATION_ID" | Select-String -Pattern "ZarinPal|WEB PAYMENT|Driver|HTTP|Exception|ERROR|FAILED"
```

### 3️⃣ **بررسی لاگ‌های کلیدی**

#### ✅ **لاگ‌های موفق:**
- `💰 WEB PAYMENT REQUEST: شروع ایجاد درخواست پرداخت`
- `🔧 WEB PAYMENT GATEWAY REQUEST: شروع CreateGatewayPaymentRequestAsync`
- `💰 ZarinPal REQUEST: شروع درخواست پرداخت`
- `📤 ZarinPal HTTP REQUEST: ارسال درخواست به`
- `📥 ZarinPal HTTP RESPONSE: پاسخ دریافت شد`
- `✅ ZarinPal SUCCESS: درخواست پرداخت موفق`

#### ❌ **لاگ‌های خطا:**
- `❌ WEB PAYMENT DRIVER FAILED: Driver درخواست پرداخت ناموفق`
- `❌ ZarinPal API ERROR: خطای API`
- `❌ ZarinPal HTTP EXCEPTION: خطا در ارتباط با درگاه پرداخت`
- `❌ ZarinPal EXCEPTION: خطای غیرمنتظره`

---

## 🔍 **تحلیل لاگ‌ها**

### **سناریو 1: خطای HTTP**
```
❌ ZarinPal HTTP EXCEPTION: خطا در ارتباط با درگاه پرداخت
```
**علت:** مشکل در ارتباط با API زرین‌پال
**راه‌حل:** 
- بررسی `GatewayUrl` در دیتابیس
- بررسی اتصال اینترنت
- بررسی Firewall

### **سناریو 2: خطای API**
```
❌ ZarinPal API ERROR: خطای API - ErrorCode: X, ErrorMessage: Y
```
**علت:** خطا از سمت API زرین‌پال
**راه‌حل:**
- بررسی `MerchantId` در دیتابیس
- بررسی `CallbackUrl` (باید با دامنه ثبت شده در پنل زرین‌پال مطابقت داشته باشد)
- بررسی `Amount` (باید حداقل 1000 تومان باشد)

### **سناریو 3: خطای Parse**
```
❌ ZarinPal PARSE ERROR: پاسخ نامعتبر
```
**علت:** پاسخ API زرین‌پال قابل Parse نیست
**راه‌حل:**
- بررسی `ResponseContent` در لاگ
- بررسی تغییرات API زرین‌پال

### **سناریو 4: خطای Driver**
```
❌ WEB PAYMENT DRIVER FAILED: Driver درخواست پرداخت ناموفق
```
**علت:** Driver خطا برگردانده است
**راه‌حل:**
- بررسی `Driver Error Details` در لاگ
- بررسی `ErrorCode` و `ErrorMessage`

---

## 📊 **اطلاعات کلیدی در لاگ‌ها**

هر لاگ شامل این اطلاعات است:
- `CorrelationId`: برای ردیابی درخواست
- `Duration`: زمان پردازش (ms)
- `RequestUrl`: URL درخواست
- `ResponseContent`: محتوای پاسخ
- `ErrorCode`: کد خطا
- `ErrorMessage`: پیام خطا

---

## 🚨 **اقدامات فوری**

1. **Application را Restart کنید**
2. **یک درخواست پرداخت جدید ایجاد کنید**
3. **CorrelationId را از پاسخ دریافت کنید**
4. **لاگ‌ها را با CorrelationId جستجو کنید**
5. **لاگ‌های خطا را بررسی کنید**

---

## 📝 **مثال جستجوی لاگ**

```powershell
# جستجوی کامل
$correlationId = "5f7f66e3-76de-446c-9d81-ca7cb5825f46"
Get-Content 'App_Data\Logs\*.log' | Select-String -Pattern $correlationId | Select-String -Pattern "ZarinPal|WEB PAYMENT|Driver|HTTP|Exception|ERROR|FAILED" | Format-List
```

---

## ✅ **چک‌لیست**

- [ ] Application Restart شده است
- [ ] لاگ‌های جدید در فایل لاگ هستند
- [ ] CorrelationId در تمام لاگ‌ها موجود است
- [ ] لاگ‌های خطا شناسایی شده‌اند
- [ ] علت ریشه‌ای خطا مشخص شده است

---

**تاریخ ایجاد:** 2026-01-07
**آخرین به‌روزرسانی:** 2026-01-07

