# ✅ فعال‌سازی درگاه شبیه‌سازی شده

**تاریخ:** 2026-01-07  
**وضعیت:** ✅ فعال شده

---

## 📋 **اقدامات انجام شده**

### ✅ **1. درگاه شبیه‌سازی شده ایجاد شد**
- PaymentGatewayId: 4
- GatewayType: 99 (Simulated)
- IsActive: 1 (فعال)
- IsDefault: 1 (پیش‌فرض) ✅

### ✅ **2. درگاه ZarinPal Production غیرفعال شد**
- PaymentGatewayId: 2
- IsDefault: 0 (غیرفعال)

---

## 🎯 **وضعیت فعلی درگاه‌ها**

| ID | نام | نوع | فعال | پیش‌فرض | Test Mode |
|---|---|---|---|---|---|---|
| 2 | ZarinPal Production | ZarinPal | ✅ | ❌ | ❌ |
| 4 | درگاه شبیه‌سازی شده (تست) | Simulated | ✅ | ✅ | ✅ |

---

## ⚠️ **اقدام بعدی: Restart Application**

**مهم:** Application باید Restart شود تا تغییرات اعمال شوند!

### **روش Restart:**
1. در Visual Studio: Stop و سپس Start کنید
2. در IIS: Application Pool را Recycle کنید

---

## 🧪 **تست**

پس از Restart:
1. یک نوبت رزرو کنید
2. به صفحه پرداخت بروید
3. باید صفحه شبیه‌سازی شده نمایش داده شود
4. روی "پرداخت موفق" کلیک کنید
5. باید به صفحه موفقیت هدایت شوید

---

## 📝 **نکات مهم**

### ✅ **برای تست:**
- درگاه شبیه‌سازی شده به عنوان پیش‌فرض تنظیم شده است
- همیشه موفق برمی‌گرداند
- بدون نیاز به اتصال واقعی به درگاه پرداخت

### ⚠️ **برای Production:**
```sql
-- غیرفعال کردن درگاه شبیه‌سازی شده
UPDATE PaymentGateways
SET IsActive = 0, IsDefault = 0
WHERE GatewayType = 99 AND IsDeleted = 0;

-- فعال کردن درگاه ZarinPal Production
UPDATE PaymentGateways
SET IsDefault = 1
WHERE GatewayType = 1 AND IsDeleted = 0;
```

---

**تاریخ ایجاد:** 2026-01-07  
**آخرین به‌روزرسانی:** 2026-01-07

