# ⚙️ تنظیمات انقضای نوبت‌های Pending

**تاریخ:** 2026-01-06  
**ماژول:** Appointment Booking / Payment Flow

---

## 📋 خلاصه

این سند نحوه تنظیم مدت زمان انقضای نوبت‌های `Pending` (در انتظار پرداخت) را توضیح می‌دهد.

---

## 🎯 هدف

نوبت‌های `Pending` بعد از مدت زمان مشخصی منقضی می‌شوند و اسلات آزاد می‌شود. این برای جلوگیری از اشغال اسلات‌ها توسط نوبت‌هایی است که پرداخت نشده‌اند.

---

## ⚙️ تنظیمات

### کلید تنظیمات در Web.config:

```xml
<appSettings>
  <!-- مدت زمان انقضای نوبت‌های Pending (به دقیقه) -->
  <!-- بعد از این مدت، نوبت‌های Pending منقضی می‌شوند و اسلات آزاد می‌شود -->
  <!-- مقدار پیش‌فرض: 5 دقیقه -->
  <!-- محدوده مجاز: 3 تا 60 دقیقه -->
  <add key="Appointment:PendingExpirationMinutes" value="5" />
</appSettings>
```

### پارامترها:

| پارامتر | مقدار پیش‌فرض | محدوده مجاز | توضیحات |
|---------|---------------|--------------|---------|
| `Appointment:PendingExpirationMinutes` | `5` | `3` تا `60` | مدت زمان انقضای نوبت‌های Pending (به دقیقه) |

---

## 🔧 نحوه تنظیم

### 1. باز کردن Web.config

فایل `Web.config` را در ریشه پروژه باز کنید.

### 2. اضافه کردن تنظیمات

در بخش `<appSettings>`، تنظیم زیر را اضافه کنید:

```xml
<appSettings>
  <!-- سایر تنظیمات... -->
  
  <!-- مدت زمان انقضای نوبت‌های Pending -->
  <add key="Appointment:PendingExpirationMinutes" value="5" />
</appSettings>
```

### 3. تنظیم مقدار

مقدار `value` را به مدت زمان مورد نظر (به دقیقه) تغییر دهید:

- **حداقل:** `3` دقیقه
- **پیش‌فرض:** `5` دقیقه
- **حداکثر:** `60` دقیقه

**مثال:**
```xml
<!-- 5 دقیقه (پیش‌فرض) -->
<add key="Appointment:PendingExpirationMinutes" value="5" />

<!-- 10 دقیقه -->
<add key="Appointment:PendingExpirationMinutes" value="10" />

<!-- 3 دقیقه (حداقل) -->
<add key="Appointment:PendingExpirationMinutes" value="3" />
```

### 4. ذخیره و Restart

بعد از تغییر `Web.config`، باید Application را Restart کنید تا تغییرات اعمال شوند.

---

## 📊 نحوه کار

1. کاربر نوبت را رزرو می‌کند → `Status = Pending`
2. `PendingExpiresAt = CreatedAt + PendingExpirationMinutes`
3. کاربر به درگاه پرداخت هدایت می‌شود
4. اگر پرداخت موفق باشد → `Status = Scheduled` (در `PaymentCallback`)
5. اگر پرداخت نشود → بعد از `PendingExpirationMinutes`، نوبت منقضی می‌شود
6. نوبت‌های منقضی شده در فیلترها در نظر گرفته نمی‌شوند → اسلات آزاد می‌شود

---

## ⚠️ نکات مهم

### 1. حداقل زمان (3 دقیقه)

- حداقل زمان 3 دقیقه است تا کاربر فرصت کافی برای پرداخت داشته باشد
- کمتر از 3 دقیقه ممکن است باعث مشکل در پرداخت شود

### 2. حداکثر زمان (60 دقیقه)

- حداکثر زمان 60 دقیقه است تا اسلات‌ها برای مدت طولانی اشغال نشوند
- بیشتر از 60 دقیقه ممکن است باعث مشکل در رقابت برای نوبت‌ها شود

### 3. مقدار پیش‌فرض (5 دقیقه)

- مقدار پیش‌فرض 5 دقیقه است (حداقل زمان ممکن)
- این مقدار برای اکثر موارد کافی است

### 4. Restart Application

- بعد از تغییر `Web.config`، باید Application را Restart کنید
- یا از `AppSettings.Instance.RefreshSettings()` استفاده کنید

---

## 🔍 بررسی تنظیمات

برای بررسی اینکه تنظیمات به درستی اعمال شده‌اند:

1. لاگ‌های Application را بررسی کنید:
   ```
   تنظیم 'مدت زمان انقضای نوبت‌های Pending' به مقدار 5 بارگذاری شد.
   ```

2. یا در کد:
   ```csharp
   var settings = AppSettings.Instance;
   var expirationMinutes = settings.PendingExpirationMinutes;
   Console.WriteLine($"PendingExpirationMinutes: {expirationMinutes}");
   ```

---

## 📝 مثال کامل Web.config

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <!-- سایر تنظیمات... -->
    
    <!-- تنظیمات نوبت‌دهی -->
    <add key="Appointment:AvailableDatesMaxCount" value="5" />
    <add key="Appointment:AvailableDatesDaysToCheck" value="60" />
    <add key="Appointment:DoctorsPageSize" value="20" />
    
    <!-- مدت زمان انقضای نوبت‌های Pending (به دقیقه) -->
    <add key="Appointment:PendingExpirationMinutes" value="5" />
  </appSettings>
  
  <!-- سایر تنظیمات... -->
</configuration>
```

---

## ✅ خلاصه

- ✅ تنظیمات در `Web.config` قابل تنظیم است
- ✅ مقدار پیش‌فرض: 5 دقیقه (حداقل زمان ممکن)
- ✅ محدوده مجاز: 3 تا 60 دقیقه
- ✅ بدون hardcode یا magic string
- ✅ قابل تغییر توسط مدیر سیستم

---

**وضعیت:** ✅ آماده برای استفاده

