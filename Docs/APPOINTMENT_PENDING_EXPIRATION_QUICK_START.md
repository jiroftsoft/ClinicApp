# ⚡ راهنمای سریع: فعال‌سازی انقضای نوبت‌های Pending

**تاریخ:** 2026-01-06  
**وضعیت:** ✅ تنظیمات به Web.config اضافه شده است

---

## ✅ انجام شده

### 1. تنظیمات به Web.config اضافه شد

```xml
<!-- Appointment Settings (تنظیمات نوبت‌دهی) -->
<add key="Appointment:PendingExpirationMinutes" value="5" />
```

**مقدار فعلی:** `5` دقیقه (حداقل زمان ممکن)  
**محدوده مجاز:** `3` تا `60` دقیقه

---

## 🔄 مراحل بعدی

### 1. Restart Application

بعد از تغییر `Web.config`، باید Application را Restart کنید:

#### روش 1: IIS (Production)
1. باز کردن **IIS Manager**
2. پیدا کردن **Application Pool** مربوط به ClinicApp
3. کلیک راست → **Recycle** یا **Stop** سپس **Start**

#### روش 2: Visual Studio (Development)
1. توقف Debugging (Stop Debugging)
2. شروع مجدد Application (Start Debugging)

#### روش 3: Application Pool Recycle (توصیه می‌شود)
```powershell
# در PowerShell (به عنوان Administrator)
Import-Module WebAdministration
Restart-WebAppPool -Name "ClinicAppAppPool"
```

---

### 2. بررسی تنظیمات

بعد از Restart، لاگ‌های Application را بررسی کنید:

```
✅ تنظیم 'مدت زمان انقضای نوبت‌های Pending' به مقدار 5 بارگذاری شد.
```

یا در کد (برای تست):
```csharp
var settings = AppSettings.Instance;
var expirationMinutes = settings.PendingExpirationMinutes;
// باید 5 باشد
```

---

### 3. Migration (بعداً انجام دهید)

⚠️ **مهم:** قبل از استفاده، باید Migration را اجرا کنید:

```sql
-- اضافه کردن ستون PendingExpiresAt به جدول Appointments
ALTER TABLE Appointments
ADD PendingExpiresAt DATETIME NULL;
```

یا از Entity Framework Migration:
```bash
# در Package Manager Console
Add-Migration AddPendingExpiresAtToAppointments
Update-Database
```

---

## 📊 نحوه کار

1. کاربر نوبت را رزرو می‌کند → `Status = Pending`
2. `PendingExpiresAt = CreatedAt + 5 minutes` (از AppSettings)
3. کاربر به درگاه پرداخت هدایت می‌شود
4. اگر پرداخت موفق باشد → `Status = Scheduled`
5. اگر پرداخت نشود → بعد از 5 دقیقه، نوبت منقضی می‌شود
6. نوبت‌های منقضی شده در فیلترها در نظر گرفته نمی‌شوند → اسلات آزاد می‌شود

---

## ⚙️ تغییر مقدار

اگر می‌خواهید مقدار را تغییر دهید:

1. باز کردن `Web.config`
2. پیدا کردن:
   ```xml
   <add key="Appointment:PendingExpirationMinutes" value="5" />
   ```
3. تغییر `value` به مقدار مورد نظر (بین 3 تا 60)
4. Restart Application

**مثال:**
```xml
<!-- 10 دقیقه -->
<add key="Appointment:PendingExpirationMinutes" value="10" />

<!-- 3 دقیقه (حداقل) -->
<add key="Appointment:PendingExpirationMinutes" value="3" />
```

---

## ✅ چک‌لیست

- ✅ تنظیمات به Web.config اضافه شده
- ⏳ Restart Application (باید انجام دهید)
- ⏳ Migration (بعداً انجام دهید)
- ⏳ تست (بعد از Migration)

---

**وضعیت:** ✅ آماده برای Restart

