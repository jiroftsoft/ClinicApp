# 🔍 راهنمای Debug صفحه Home

**تاریخ:** 2025-01-27  
**مشکل:** بخش‌های صفحه Index خالی هستند (به جز اسلایدر)  
**هدف:** بررسی ماژول به ماژول برای شناسایی مشکل

---

## 📋 فهرست بررسی

### 1. بررسی Controller Layer
- ✅ Exception Handling
- ✅ ViewModel Passing
- ✅ Debug Logging

### 2. بررسی Service Layer
- ✅ Data Loading
- ✅ Exception Handling
- ✅ Return Values

### 3. بررسی View Layer
- ✅ Model Null Checks
- ✅ Conditional Rendering
- ✅ Partial View Loading

---

## 🔍 مراحل Debug

### مرحله 1: بررسی Exception در Controller

**فایل:** `Controllers/HomeController.cs`

```csharp
catch (Exception ex)
{
    // لاگ خطا با جزئیات کامل
    System.Diagnostics.Debug.WriteLine($"❌ ERROR: {ex.Message}");
    // ...
}
```

**بررسی:**
1. Visual Studio Output Window را باز کنید
2. صفحه را Refresh کنید
3. بررسی کنید آیا Exception رخ می‌دهد یا نه

---

### مرحله 2: بررسی داده‌های لود شده

**Debug Output در Controller:**
```
=== HomePage Data Debug ===
Hero: Loaded / NULL
Services: Loaded (X items) / NULL
Doctors: Loaded (X items) / NULL
Contact: Loaded / NULL
Sidebar: Loaded / NULL
===========================
```

**بررسی:**
- اگر همه NULL هستند → مشکل در Service Layer
- اگر بعضی Loaded هستند → مشکل در Repository/Data Layer

---

### مرحله 3: بررسی Service Methods

#### Services Section:
```csharp
var services = await _serviceRepository.GetAllActiveServicesAsync();
```

**بررسی:**
- آیا `services` null است؟
- آیا `services` خالی است؟
- آیا Exception رخ می‌دهد؟

#### Doctors Section:
```csharp
var doctors = await _context.Doctors
    .AsNoTracking()
    .Where(d => !d.IsDeleted && d.IsActive && (d.ClinicId == effectiveClinicId || effectiveClinicId == 0))
```

**بررسی:**
- آیا `doctors` null است؟
- آیا `doctors` خالی است؟
- آیا `ClinicId` درست است؟

---

### مرحله 4: بررسی View Conditions

**فایل:** `Views/Home/Index.cshtml`

```razor
@if (Model.Services != null)
{
    @Html.Partial("...", Model.Services)
}
```

**مشکل احتمالی:**
- `Model.Services` null نیست اما `Model.Services.Services` خالی است
- باید چک شود: `Model.Services != null && Model.Services.Services != null && Model.Services.Services.Any()`

---

## 🐛 مشکلات احتمالی

### مشکل 1: Exception در Service Layer
**علت:** خطا در Repository یا Database
**راه‌حل:** بررسی Exception در Logs

### مشکل 2: داده‌های خالی در Database
**علت:** هیچ رکوردی در Database وجود ندارد
**راه‌حل:** بررسی Database و اضافه کردن داده‌های Test

### مشکل 3: ClinicId اشتباه
**علت:** `effectiveClinicId` با داده‌های Database مطابقت ندارد
**راه‌حل:** بررسی ClinicId در Database

### مشکل 4: Filter Conditions
**علت:** شرایط Filter (IsActive, IsDeleted) باعث می‌شود هیچ داده‌ای برگردانده نشود
**راه‌حل:** بررسی شرایط Filter

---

## ✅ چک‌لیست Debug

- [ ] بررسی Output Window برای Exception
- [ ] بررسی Debug Output برای داده‌های لود شده
- [ ] بررسی Database برای وجود داده
- [ ] بررسی Repository Methods
- [ ] بررسی Service Methods
- [ ] بررسی View Conditions
- [ ] بررسی Partial Views

---

## 🔧 راه‌حل‌های پیشنهادی

### 1. بهبود Exception Handling:
```csharp
catch (Exception ex)
{
    _logger.Error(ex, "خطا در دریافت داده‌های صفحه اصلی");
    // Return partial data instead of empty ViewModel
}
```

### 2. بهبود View Conditions:
```razor
@if (Model.Services != null && Model.Services.Services != null && Model.Services.Services.Any())
```

### 3. اضافه کردن Debug View:
```razor
@if (System.Diagnostics.Debugger.IsAttached)
{
    <div class="debug-info">
        <p>Services Count: @(Model.Services?.Services?.Count ?? 0)</p>
        <p>Doctors Count: @(Model.Doctors?.Doctors?.Count ?? 0)</p>
    </div>
}
```

---

**تهیه شده توسط:** AI Assistant  
**تاریخ:** 2025-01-27
