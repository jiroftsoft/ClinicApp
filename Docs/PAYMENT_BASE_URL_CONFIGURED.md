# ✅ تنظیم Payment Base URL

**تاریخ:** 2026-01-06  
**دامنه:** `mehranyad.ir`  
**وضعیت:** ✅ تنظیم شد

---

## 📋 تنظیمات اعمال شده

### Web.config:

```xml
<add key="Payment:BaseUrl" value="https://mehranyad.ir"/>
```

---

## ✅ نتیجه

### CallbackUrl ساخته شده:

```
https://mehranyad.ir/Patient/AppointmentBooking/PaymentCallback
```

---

## ⚠️ نکات مهم

### 1. ثبت دامنه در پنل ZarinPal:

- ✅ دامنه `mehranyad.ir` باید در پنل ZarinPal ثبت شود
- ✅ مسیر: پنل ZarinPal → تنظیمات → Callback URL → اضافه کردن `mehranyad.ir`

### 2. Restart Application:

- ✅ بعد از تغییر `Web.config`، Application را Restart کنید
- ✅ تنظیمات جدید اعمال می‌شود

### 3. تست:

- ✅ تست درخواست پرداخت
- ✅ بررسی CallbackUrl در لاگ‌ها
- ✅ اطمینان از عدم خطای "The callback URL domain does not match"

---

## 🔍 بررسی لاگ

بعد از Restart، در لاگ باید این پیام را ببینید:

```
✅ PaymentUrlHelper: CallbackUrl از PaymentBaseUrl ساخته شد - https://mehranyad.ir/Patient/AppointmentBooking/PaymentCallback
```

---

## 📌 مراحل بعدی

1. ✅ `Payment:BaseUrl` تنظیم شد
2. ⏳ ثبت دامنه `mehranyad.ir` در پنل ZarinPal
3. ⏳ Restart Application
4. ⏳ تست درخواست پرداخت

---

**آماده برای Production!** 🚀

