# 🔧 اسکریپت‌های کمکی برای Debug پرداخت

این فولدر شامل اسکریپت‌های PowerShell برای Debug و بررسی خطاهای پرداخت است.

---

## 📋 فایل‌های موجود

### 1. `FindPaymentError.ps1`
**هدف:** پیدا کردن خطای دقیق از لاگ‌ها با استفاده از CorrelationId

**استفاده:**
```powershell
.\Scripts\FindPaymentError.ps1 -CorrelationId "92c168d6-7a73-4f2e-bf84-1f0fc9e39822"
```

**خروجی:**
- نمایش تمام لاگ‌های مرتبط با CorrelationId
- Context (قبل و بعد) برای هر لاگ
- مسیر فایل لاگ

---

### 2. `CheckPaymentConfig.ps1`
**هدف:** بررسی تنظیمات پرداخت در Web.config

**استفاده:**
```powershell
.\Scripts\CheckPaymentConfig.ps1
```

**خروجی:**
- بررسی `Payment:BaseUrl`
- بررسی `ZarinpalMerchantId`
- بررسی `Zarinpal:IsSandbox`
- بررسی فایل‌های لاگ

---

## 🚀 نحوه استفاده

### در PowerShell (از مسیر ریشه پروژه):

```powershell
# 1. بررسی تنظیمات
.\Scripts\CheckPaymentConfig.ps1

# 2. پیدا کردن خطا با CorrelationId
.\Scripts\FindPaymentError.ps1 -CorrelationId "YOUR_CORRELATION_ID"
```

### در Visual Studio (Package Manager Console):

```powershell
# 1. بررسی تنظیمات
.\Scripts\CheckPaymentConfig.ps1

# 2. پیدا کردن خطا
.\Scripts\FindPaymentError.ps1 -CorrelationId "YOUR_CORRELATION_ID"
```

---

## 📊 مثال خروجی

### FindPaymentError.ps1:
```
🔍 جستجوی خطای پرداخت با CorrelationId: 92c168d6-7a73-4f2e-bf84-1f0fc9e39822
📁 مسیر لاگ: App_Data\Logs
✅ تعداد فایل‌های لاگ: 5

✅ خطا در فایل: clinicapp-20260106.log
📄 مسیر کامل: C:\...\App_Data\Logs\clinicapp-20260106.log

📋 لاگ‌های مرتبط:
🔗 PAYMENT REQUEST: CallbackUrl تنظیم شد - https://mehranyad.ir/...
❌ ZarinPal: خطای API - ErrorCode: -9, ErrorMessage: The callback URL domain...
```

### CheckPaymentConfig.ps1:
```
🔍 بررسی تنظیمات پرداخت

✅ Web.config یافت شد
✅ Payment:BaseUrl تنظیم شده است:
   مقدار: https://mehranyad.ir
✅ ZarinpalMerchantId تنظیم شده است:
   مقدار: 156be6cd...
✅ Zarinpal:IsSandbox تنظیم شده است:
   مقدار: false
```

---

## ⚠️ نکات مهم

1. **اجرای اسکریپت‌ها:**
   - اگر خطای "execution policy" می‌گیرید، از دستور زیر استفاده کنید:
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

2. **مسیر لاگ‌ها:**
   - لاگ‌ها در `App_Data\Logs\` ذخیره می‌شوند
   - فرمت: `clinicapp-{date}.log`

3. **CorrelationId:**
   - CorrelationId در پاسخ JSON به Frontend برگردانده می‌شود
   - از Console Browser می‌توانید آن را پیدا کنید

---

## 🔗 مراجع

- `Docs/PAYMENT_DEBUG_QUICK_FIX.md` - راهنمای سریع
- `Docs/PAYMENT_ERROR_DIAGNOSIS_STEPS.md` - راهنمای کامل Debug
- `Docs/PAYMENT_DEBUG_GUIDE.md` - راهنمای جامع

---

**نکته:** این اسکریپت‌ها برای Windows PowerShell طراحی شده‌اند. برای PowerShell Core (pwsh) ممکن است نیاز به تغییرات جزئی باشد.
