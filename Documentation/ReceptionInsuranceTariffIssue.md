# 🚨 گزارش مشکل: عدم استفاده از تعرفه‌های بیمه در فرم پذیرش

## 📋 مشکل گزارش شده

در فرم پذیرش، برای خدمت با کد `970000` (ویزیت پزشک عمومی):
- **سهم پایه:** 0 ریال ❌
- **سهم تکمیلی:** 0 ریال ❌
- **سهم بیمار:** 2,376,000 ریال ❌
- **وضعیت:** بدون پوشش ❌

اما در دیتابیس، تعرفه‌های بیمه به درستی تعریف شده‌اند:
- **بیمه پایه (InsurancePlanId: 1012):** TariffPrice = 2,376,000, InsurerShare = 1,663,200, PatientShare = 712,800
- **بیمه تکمیلی (InsurancePlanId: 1018):** TariffPrice = 2,376,000, PatientShare = 712,800, SupplementaryCoveragePercent = 100%

---

## 🔍 تحلیل مشکل

### 1. بررسی متد `GetByPlanAndServiceAsync`

**مکان:** `Repositories/Insurance/InsuranceTariffRepository.cs:338`

**کد فعلی:**
```csharp
public async Task<InsuranceTariff> GetByPlanAndServiceAsync(int planId, int serviceId)
{
    var tariff = await _context.InsuranceTariffs
        .AsNoTracking()
        .Where(t => t.InsurancePlanId == planId &&
                   t.ServiceId == serviceId &&
                   !t.IsDeleted)
        .FirstOrDefaultAsync();
    
    return tariff;
}
```

**مشکل:** ❌ شرط `IsActive` بررسی نمی‌شود!

**نتیجه:** اگر تعرفه `IsActive = 0` باشد، باز هم پیدا می‌شود، اما در `CalculatePrimaryInsuranceFallbackAsync` رد می‌شود.

### 2. بررسی متد `CalculatePrimaryInsuranceFallbackAsync`

**مکان:** `Services/Insurance/CombinedInsuranceCalculationService.cs:530`

**کد فعلی:**
```csharp
var primaryTariff = await _context.InsuranceTariffs
    .Where(t => t.ServiceId == serviceId && 
                t.InsurancePlanId == primaryInsurance.InsurancePlanId && 
                t.InsuranceType == InsuranceType.Primary &&
                !t.IsDeleted && t.IsActive)  // ✅ شرط IsActive بررسی می‌شود
    .FirstOrDefaultAsync();
```

**مشکل:** این متد مستقیماً از `_context` استفاده می‌کند، نه از `_insuranceTariffRepository.GetByPlanAndServiceAsync`!

### 3. بررسی متد `CalculateInsuranceCoverageWithTariffAsync`

**مکان:** `Services/Insurance/InsuranceCalculationService.cs:494`

**کد فعلی:**
```csharp
var tariff = await _insuranceTariffRepository.GetByPlanAndServiceAsync(insurancePlan.InsurancePlanId, serviceId);
```

**مشکل:** این متد از `GetByPlanAndServiceAsync` استفاده می‌کند که شرط `IsActive` را بررسی نمی‌کند!

---

## 🎯 علل احتمالی

### 1. عدم بررسی `IsActive` در Repository
- `GetByPlanAndServiceAsync` شرط `IsActive` را بررسی نمی‌کند
- اگر تعرفه `IsActive = 0` باشد، پیدا می‌شود اما استفاده نمی‌شود

### 2. عدم تطابق ServiceId
- در دیتابیس: `ServiceId = 1424`
- در فرم پذیرش: ممکن است `ServiceCode = 970000` استفاده شود
- باید بررسی شود که آیا `ServiceId` درست ارسال می‌شود یا نه

### 3. عدم تطابق InsurancePlanId
- در دیتابیس: `InsurancePlanId = 1012` (بیمه پایه سلامت)
- در فرم پذیرش: باید بررسی شود که آیا `InsurancePlanId` درست ارسال می‌شود یا نه

### 4. عدم بررسی `InsuranceType`
- در `GetByPlanAndServiceAsync`، شرط `InsuranceType` بررسی نمی‌شود
- ممکن است تعرفه‌های تکمیلی به جای پایه پیدا شوند

---

## ✅ راه‌حل‌های پیشنهادی

### راه‌حل 1: اصلاح `GetByPlanAndServiceAsync`

**افزودن شرط `IsActive`:**
```csharp
public async Task<InsuranceTariff> GetByPlanAndServiceAsync(int planId, int serviceId)
{
    var tariff = await _context.InsuranceTariffs
        .AsNoTracking()
        .Where(t => t.InsurancePlanId == planId &&
                   t.ServiceId == serviceId &&
                   !t.IsDeleted &&
                   t.IsActive)  // ✅ افزودن شرط IsActive
        .FirstOrDefaultAsync();
    
    return tariff;
}
```

### راه‌حل 2: افزودن متد جدید با `InsuranceType`

**افزودن متد جدید:**
```csharp
public async Task<InsuranceTariff> GetByPlanAndServiceAndTypeAsync(int planId, int serviceId, InsuranceType insuranceType)
{
    var tariff = await _context.InsuranceTariffs
        .AsNoTracking()
        .Where(t => t.InsurancePlanId == planId &&
                   t.ServiceId == serviceId &&
                   t.InsuranceType == insuranceType &&
                   !t.IsDeleted &&
                   t.IsActive)
        .FirstOrDefaultAsync();
    
    return tariff;
}
```

### راه‌حل 3: استفاده از `GetTariffByTypeAsync` موجود

**استفاده از متد موجود:**
```csharp
// در InsuranceCalculationService
var tariff = await _insuranceTariffRepository.GetTariffByTypeAsync(
    serviceId, 
    insurancePlan.InsurancePlanId, 
    InsuranceType.Primary);
```

---

## 🔍 بررسی‌های لازم

### 1. بررسی ServiceId
- آیا `ServiceId = 1424` درست است؟
- آیا در فرم پذیرش، `ServiceCode = 970000` به `ServiceId` تبدیل می‌شود؟

### 2. بررسی InsurancePlanId
- آیا `InsurancePlanId = 1012` (بیمه پایه سلامت) درست است؟
- آیا `InsurancePlanId = 1018` (بیمه تکمیلی دانا) درست است؟

### 3. بررسی IsActive
- آیا تعرفه‌ها `IsActive = 1` هستند؟
- آیا تعرفه‌ها `IsDeleted = 0` هستند؟

### 4. بررسی InsuranceType
- آیا `InsuranceType = 1` (Primary) برای بیمه پایه درست است؟
- آیا `InsuranceType = 2` (Supplementary) برای بیمه تکمیلی درست است؟

---

## 📝 اقدامات لازم

1. ✅ **اصلاح `GetByPlanAndServiceAsync`** - افزودن شرط `IsActive`
2. ✅ **اصلاح `CalculateInsuranceCoverageWithTariffAsync`** - استفاده از `GetTariffByTypeAsync` به جای `GetByPlanAndServiceAsync`
3. ✅ **بررسی لاگ‌ها** - بررسی اینکه آیا تعرفه‌ها پیدا می‌شوند یا نه
4. ✅ **تست** - تست با ServiceId = 1424 و InsurancePlanId = 1012

---

## 🧪 تست‌های پیشنهادی

### تست 1: بررسی پیدا شدن تعرفه
```sql
SELECT * FROM InsuranceTariffs 
WHERE ServiceId = 1424 
  AND InsurancePlanId = 1012 
  AND InsuranceType = 1 
  AND IsDeleted = 0 
  AND IsActive = 1;
```

### تست 2: بررسی لاگ‌های سیستم
- بررسی لاگ‌های `CalculatePrimaryInsuranceFallbackAsync`
- بررسی لاگ‌های `GetByPlanAndServiceAsync`
- بررسی لاگ‌های `CalculateInsuranceCoverageWithTariffAsync`

### تست 3: تست در فرم پذیرش
- اضافه کردن خدمت با کد `970000`
- بررسی اینکه آیا تعرفه پیدا می‌شود یا نه
- بررسی محاسبات سهم‌ها

---

**تاریخ بررسی:** 2025-01-11  
**اولویت:** 🔴 بالا  
**وضعیت:** در حال بررسی

