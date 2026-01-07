# 🎭 راهنمای استفاده از درگاه پرداخت شبیه‌سازی شده

## 📋 **خلاصه**

درگاه پرداخت شبیه‌سازی شده برای **تست و توسعه** بدون نیاز به اتصال واقعی به درگاه پرداخت طراحی شده است.

---

## ✅ **ویژگی‌ها**

- ✅ **همیشه موفق برمی‌گرداند** (برای تست)
- ✅ **بدون نیاز به اتصال واقعی** به درگاه پرداخت
- ✅ **صفحه شبیه‌سازی شده** برای تست UI
- ✅ **لاگ‌گذاری کامل** برای Debug
- ✅ **سازگار با معماری موجود** (Factory Pattern)

---

## 🚀 **نصب و راه‌اندازی**

### 1️⃣ **اجرای SQL Script**

```sql
-- اجرای اسکریپت SQL
-- فایل: Scripts/sql/Create_Simulated_Gateway.sql
```

این اسکریپت:
- درگاه شبیه‌سازی شده را در دیتابیس ایجاد می‌کند
- `GatewayType = 99` (Simulated)
- `IsActive = 1`
- `IsDefault = 0` (می‌توانید به 1 تغییر دهید)

### 2️⃣ **فعال‌سازی درگاه (اختیاری)**

اگر می‌خواهید این درگاه به عنوان درگاه پیش‌فرض استفاده شود:

```sql
-- فعال‌سازی به عنوان درگاه پیش‌فرض
UPDATE PaymentGateways
SET IsDefault = 1
WHERE GatewayType = 99 AND IsDeleted = 0;

-- غیرفعال کردن درگاه‌های دیگر (اختیاری)
UPDATE PaymentGateways
SET IsDefault = 0
WHERE GatewayType != 99 AND IsDeleted = 0;
```

### 3️⃣ **Restart Application**

Application را Restart کنید تا تغییرات اعمال شوند.

---

## 🎯 **استفاده**

### **روش 1: استفاده به عنوان درگاه پیش‌فرض**

اگر `IsDefault = 1` باشد، به صورت خودکار استفاده می‌شود.

### **روش 2: انتخاب دستی در UI**

از طریق UI مدیریت درگاه‌ها، درگاه شبیه‌سازی شده را انتخاب کنید.

---

## 🔄 **فرآیند پرداخت**

### **1. ایجاد درخواست پرداخت**

```
User → AppointmentBookingController.ProcessPayment
     → WebPaymentService.CreatePaymentRequestAsync
     → SimulatedGatewayDriver.RequestPaymentAsync
     → بازگشت Authority و PaymentUrl
```

### **2. هدایت به صفحه شبیه‌سازی شده**

```
User → /Payment/SimulatedGateway/Process?authority=xxx
     → نمایش صفحه شبیه‌سازی شده
```

### **3. انتخاب نتیجه (موفق/لغو)**

```
User → کلیک روی "پرداخت موفق" یا "لغو پرداخت"
     → SimulatedGatewayController.ProcessPayment
     → Redirect به Callback
```

### **4. Callback و تأیید**

```
Callback → AppointmentBookingController.PaymentCallback
         → WebPaymentService.ProcessPaymentCallbackAsync
         → SimulatedGatewayDriver.VerifyPaymentAsync
         → همیشه موفق برمی‌گرداند
```

---

## 📁 **فایل‌های ایجاد شده**

### **1. Driver**
- `Services/Payment/Gateway/Drivers/SimulatedGatewayDriver.cs`
  - پیاده‌سازی `IGatewayDriver`
  - همیشه موفق برمی‌گرداند
  - لاگ‌گذاری کامل

### **2. Factory**
- `Services/Payment/Gateway/Drivers/GatewayDriverFactory.cs`
  - ثبت `SimulatedGatewayDriver` در Factory
  - پشتیبانی از `PaymentGatewayType.Simulated`

### **3. Enum**
- `Models/Enums/PaymentGatewayType.cs`
  - افزودن `Simulated = 99`

### **4. Controller**
- `Controllers/Payment/SimulatedGatewayController.cs`
  - `Process`: نمایش صفحه شبیه‌سازی شده
  - `ProcessPayment`: پردازش نتیجه (موفق/لغو)

### **5. View**
- `Views/Payment/SimulatedGateway/Process.cshtml`
  - صفحه شبیه‌سازی شده درگاه پرداخت
  - دکمه‌های "پرداخت موفق" و "لغو پرداخت"

### **6. SQL Script**
- `Scripts/sql/Create_Simulated_Gateway.sql`
  - ایجاد درگاه در دیتابیس

---

## 🧪 **تست**

### **سناریو 1: پرداخت موفق**

1. یک نوبت رزرو کنید
2. به صفحه پرداخت بروید
3. روی "پرداخت موفق" کلیک کنید
4. باید به صفحه موفقیت هدایت شوید

### **سناریو 2: لغو پرداخت**

1. یک نوبت رزرو کنید
2. به صفحه پرداخت بروید
3. روی "لغو پرداخت" کلیک کنید
4. باید به صفحه خطا هدایت شوید

---

## 📊 **لاگ‌ها**

### **لاگ‌های موفق:**
- `🎭 SIMULATED REQUEST: شروع درخواست پرداخت شبیه‌سازی شده`
- `✅ SIMULATED SUCCESS: درخواست پرداخت شبیه‌سازی شده موفق`
- `✅ SIMULATED VERIFY SUCCESS: تأیید پرداخت شبیه‌سازی شده موفق`

### **لاگ‌های خطا:**
- `❌ SIMULATED REQUEST: PaymentRequest is null`
- `❌ SIMULATED EXCEPTION: خطای غیرمنتظره`

---

## ⚠️ **نکات مهم**

### **1. فقط برای تست**
این درگاه **فقط برای تست و توسعه** است. در Production از درگاه واقعی استفاده کنید.

### **2. همیشه موفق**
این درگاه **همیشه موفق برمی‌گرداند**. برای تست سناریوهای خطا، از درگاه واقعی استفاده کنید.

### **3. امنیت**
در Production، این درگاه را **غیرفعال** کنید:

```sql
UPDATE PaymentGateways
SET IsActive = 0
WHERE GatewayType = 99 AND IsDeleted = 0;
```

---

## 🔧 **تنظیمات**

### **تغییر URL صفحه شبیه‌سازی شده**

در `SimulatedGatewayDriver.cs`:

```csharp
_simulatedPaymentUrl = $"{baseUrl}/Payment/SimulatedGateway/Process?authority={{0}}";
```

### **تغییر Callback URL**

در SQL Script:

```sql
CallbackUrl = N'/Patient/AppointmentBooking/PaymentCallback'
```

---

## 📝 **مثال استفاده**

### **1. فعال‌سازی درگاه**

```sql
-- فعال‌سازی به عنوان درگاه پیش‌فرض
UPDATE PaymentGateways
SET IsDefault = 1, IsActive = 1
WHERE GatewayType = 99 AND IsDeleted = 0;
```

### **2. تست پرداخت**

1. Application را Restart کنید
2. یک نوبت رزرو کنید
3. به صفحه پرداخت بروید
4. باید صفحه شبیه‌سازی شده نمایش داده شود

---

## ✅ **چک‌لیست**

- [ ] SQL Script اجرا شده است
- [ ] درگاه در دیتابیس ایجاد شده است
- [ ] Application Restart شده است
- [ ] درگاه فعال است (`IsActive = 1`)
- [ ] درگاه به عنوان پیش‌فرض تنظیم شده است (اختیاری)
- [ ] تست پرداخت موفق انجام شده است
- [ ] تست لغو پرداخت انجام شده است

---

## 🆘 **عیب‌یابی**

### **مشکل: درگاه نمایش داده نمی‌شود**

**راه‌حل:**
1. بررسی کنید که `IsActive = 1` باشد
2. بررسی کنید که `IsDefault = 1` باشد (یا در UI انتخاب شده باشد)
3. Application را Restart کنید

### **مشکل: خطای 404 در صفحه شبیه‌سازی شده**

**راه‌حل:**
1. بررسی کنید که `SimulatedGatewayController.cs` در پروژه باشد
2. بررسی کنید که `Views/Payment/SimulatedGateway/Process.cshtml` وجود داشته باشد
3. Route را بررسی کنید

### **مشکل: Callback کار نمی‌کند**

**راه‌حل:**
1. بررسی کنید که `CallbackUrl` در دیتابیس صحیح باشد
2. بررسی کنید که `PaymentCallback` در `AppointmentBookingController` وجود داشته باشد

---

**تاریخ ایجاد:** 2026-01-07
**آخرین به‌روزرسانی:** 2026-01-07

