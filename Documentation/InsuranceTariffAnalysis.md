# 📋 تحلیل کامل ماژول InsuranceTariff

## 🎯 مقدمه

این سند شامل تحلیل کامل ماژول `InsuranceTariff` است که شامل:
1. **منطق تعیین ست (TariffPrice)**
2. **منطق محاسبه سهم بیمار و بیمه**
3. **منطق محاسبه پوشش بیمه تکمیلی**
4. **بررسی Views و Validation**
5. **مشکلات احتمالی و راه‌حل‌ها**

---

## 📌 تعریف "تعیین ست" (TariffPrice)

### مفهوم:
**"تعیین ست"** به معنای **تعیین قیمت تعرفه (TariffPrice)** برای یک خدمت در یک طرح بیمه خاص است.

### فیلد TariffPrice:
```csharp
/// <summary>
/// مبلغ مشخص‌شده برای خدمت تحت پوشش این بیمه (اگر null باشد، از قیمت پایه Service استفاده می‌شود).
/// </summary>
public decimal? TariffPrice { get; set; }
```

### ویژگی‌ها:
- **نوع:** `decimal?` (Nullable)
- **واحد:** ریال (بدون اعشار)
- **اختیاری:** بله (اگر null باشد، از قیمت پایه `Service.Price` استفاده می‌شود)

---

## 🔢 منطق تعیین ست (TariffPrice)

### الگوریتم محاسبه:

#### 1. بررسی قیمت موجود:
```csharp
// اگر قیمت فعلی موجود است، از آن استفاده کن
if (currentTariffPrice.HasValue && currentTariffPrice.Value > 0)
{
    return currentTariffPrice.Value;
}
```

#### 2. محاسبه بر اساس FactorSettings:
```csharp
// استفاده از ServiceCalculationService برای منطق یکسان
calculatedPrice = _serviceCalculationService.CalculateServicePriceWithFactorSettings(
    service, _context, DateTime.Now);
```

#### 3. Fallback به منطق قدیمی:
```csharp
// اگر ServiceCalculationService موجود نباشد
var basePrice = service.Price;
calculatedPrice = basePrice * technicalFactor.Value * professionalFactor.Value;
```

#### 4. Fallback نهایی:
```csharp
// اگر هیچ کدام موجود نباشد، از قیمت پایه خدمت استفاده کن
return service.Price;
```

### کلاس مسئول:
- **`InsuranceTariffCalculationService.CalculateTariffPriceWithFactorSettingAsync`**

### مثال:
```csharp
// خدمت: ویزیت پزشک عمومی (970000)
// قیمت پایه: 2,376,000 ریال
// TariffPrice: null → محاسبه می‌شود → 2,376,000 ریال
// TariffPrice: 2,500,000 → استفاده می‌شود → 2,500,000 ریال
```

---

## 💰 منطق محاسبه سهم بیمار و بیمه

### فرمول پایه:
```
TariffPrice = PatientShare + InsurerShare
```

### الگوریتم محاسبه سهم بیمه (InsurerShare):

#### 1. استفاده از PlanService:
```csharp
// دریافت اطلاعات PlanService برای این خدمت و طرح
var planService = await _planServiceRepository.GetPlanServiceAsync(
    insurancePlanId, serviceId);
```

#### 2. محاسبه بر اساس درصد پوشش:
```csharp
// اگر PlanService موجود است
if (planService != null && planService.CoveragePercent.HasValue)
{
    var deductible = await GetDeductibleAsync(insurancePlanId, correlationId);
    var coverableAmount = Math.Max(0, tariffPrice - deductible);
    insurerShare = (coverableAmount * planService.CoveragePercent.Value) / 100m;
}
```

#### 3. استفاده از مقدار دستی:
```csharp
// اگر currentInsurerShare ارائه شده باشد
if (currentInsurerShare.HasValue && currentInsurerShare.Value > 0)
{
    insurerShare = currentInsurerShare.Value;
}
```

### الگوریتم محاسبه سهم بیمار (PatientShare):

#### 1. محاسبه با فرانشیز:
```csharp
var deductible = await GetDeductibleAsync(insurancePlanId, correlationId);
var coverableAmount = Math.Max(0, tariffPrice - deductible);
var insurerShareFromCoverable = Math.Min(insurerShare, coverableAmount);
var patientShareRaw = deductible + (coverableAmount - insurerShareFromCoverable);
```

#### 2. تراز نهایی:
```csharp
// 🔧 CRITICAL FIX: تراز نهایی تضمین‌شده
// سهم بیمار = مبلغ کل - سهم بیمه (تراز خودکار)
calculatedShare = tariffPrice - insurerShare;
```

#### 3. استفاده از مقدار دستی:
```csharp
// اگر currentPatientShare ارائه شده باشد
if (currentPatientShare.HasValue && currentPatientShare.Value > 0)
{
    patientShare = currentPatientShare.Value;
}
```

### کلاس مسئول:
- **`InsuranceTariffController.CalculateInsurerShareWithPlanServiceAsync`**
- **`InsuranceTariffController.CalculatePatientShareAsync`**

### مثال:
```csharp
// TariffPrice: 2,376,000 ریال
// Deductible: 100,000 ریال
// CoveragePercent: 90%
// 
// CoverableAmount: 2,376,000 - 100,000 = 2,276,000 ریال
// InsurerShare: 2,276,000 × 90% = 2,048,400 ریال
// PatientShare: 2,376,000 - 2,048,400 = 327,600 ریال
```

---

## 🏥 منطق محاسبه پوشش بیمه تکمیلی

### فرمول پایه:
```
SupplementaryCoverage = (RemainingAfterPrimary × SupplementaryCoveragePercent) / 100
```

### الگوریتم محاسبه:

#### 1. استفاده از مقدار موجود:
```csharp
// اگر درصد پوشش تکمیلی موجود است، از آن استفاده کن
if (supplementaryCoveragePercent.HasValue && supplementaryCoveragePercent.Value > 0)
{
    return supplementaryCoveragePercent.Value;
}
```

#### 2. محاسبه بر اساس باقیمانده:
```csharp
// پوشش تکمیلی درصدی از مبلغ باقی‌مانده بعد از بیمه پایه است
var remainingAfterPrimary = Math.Max(0, tariffPrice - insurerShare);
var supplementaryAmount = (remainingAfterPrimary * supplementaryCoveragePercent) / 100m;
var supplementaryCoveragePercentOfTotal = tariffPrice > 0 
    ? (supplementaryAmount / tariffPrice) * 100m 
    : 0m;
```

#### 3. محاسبه پوشش کل:
```csharp
// پوشش کل = پوشش اولیه + پوشش تکمیلی (با سقف 100%)
var primaryCoveragePercent = (insurerShare / tariffPrice) * 100m;
var totalCoverage = Math.Min(primaryCoveragePercent + supplementaryCoveragePercentOfTotal, 100m);
```

### کلاس مسئول:
- **`InsuranceTariffController.CalculateSupplementaryCoverageAsync`**
- **`InsuranceTariffController.CalculateTotalCoverageAsync`**

### مثال:
```csharp
// TariffPrice: 2,376,000 ریال
// InsurerShare: 2,048,400 ریال (86.2%)
// SupplementaryCoveragePercent: 50%
// 
// RemainingAfterPrimary: 2,376,000 - 2,048,400 = 327,600 ریال
// SupplementaryAmount: 327,600 × 50% = 163,800 ریال
// SupplementaryCoveragePercentOfTotal: (163,800 / 2,376,000) × 100 = 6.9%
// TotalCoverage: 86.2% + 6.9% = 93.1%
```

---

## 📊 بررسی Views

### 1. Index.cshtml
- **کاربرد:** نمایش لیست تعرفه‌های بیمه
- **ویژگی‌ها:**
  - فیلتر بر اساس ارائه‌دهنده، طرح، خدمت
  - صفحه‌بندی
  - جستجوی پیشرفته
  - نمایش آمار (تعداد کل، فعال، غیرفعال)

### 2. Create.cshtml
- **کاربرد:** ایجاد تعرفه جدید
- **ویژگی‌ها:**
  - انتخاب ارائه‌دهنده و طرح بیمه
  - انتخاب خدمت (تکی یا گروهی)
  - ورود دستی TariffPrice (اختیاری)
  - محاسبه خودکار TariffPrice بر اساس FactorSettings
  - محاسبه خودکار PatientShare و InsurerShare
  - نمایش نتایج محاسبه در Real-time
  - Validation کامل

### 3. Edit.cshtml
- **کاربرد:** ویرایش تعرفه موجود
- **ویژگی‌ها:**
  - مشابه Create.cshtml
  - نمایش مقادیر فعلی
  - امکان تغییر TariffPrice، PatientShare، InsurerShare
  - محاسبه مجدد در صورت تغییر

### 4. Details.cshtml
- **کاربرد:** مشاهده جزئیات تعرفه
- **ویژگی‌ها:**
  - نمایش تمام اطلاعات تعرفه
  - نمایش تاریخ ایجاد/ویرایش
  - نمایش اطلاعات کاربر ایجاد/ویرایش کننده
  - نمایش محاسبات (TariffPrice، PatientShare، InsurerShare)

---

## ✅ بررسی Validation

### 1. Client-Side Validation (JavaScript):

#### بررسی تراز:
```javascript
// سهم بیمار + سهم بیمه باید دقیقاً برابر قیمت تعرفه باشد
if (t > 0 && p >= 0 && i >= 0 && t !== (p + i)) {
    alert('سهم بیمار + سهم بیمه باید دقیقاً برابر قیمت تعرفه (به ریال) باشد.');
    isValid = false;
}
```

#### بررسی مقادیر منفی:
```javascript
// استفاده از HTML5 validation
<input type="number" min="0" max="2000000000" step="1" />
```

### 2. Server-Side Validation (C#):

#### در ViewModel:
```csharp
[Range(0, double.MaxValue, ErrorMessage = "قیمت تعرفه نمی‌تواند منفی باشد.")]
public decimal? TariffPrice { get; set; }

[Range(0, double.MaxValue, ErrorMessage = "سهم بیمار نمی‌تواند منفی باشد.")]
public decimal? PatientShare { get; set; }

[Range(0, double.MaxValue, ErrorMessage = "سهم بیمه نمی‌تواند منفی باشد.")]
public decimal? InsurerShare { get; set; }
```

#### در Controller:
```csharp
// بررسی تراز نهایی
if (tariffPrice > 0 && patientShare.HasValue && insurerShare.HasValue)
{
    var total = patientShare.Value + insurerShare.Value;
    if (Math.Abs(total - tariffPrice) > 0.01m) // تحمل خطای 0.01 ریال
    {
        ModelState.AddModelError("", "سهم بیمار + سهم بیمه باید برابر قیمت تعرفه باشد.");
    }
}
```

---

## 🔍 مشکلات احتمالی و راه‌حل‌ها

### مشکل 1: عدم تراز TariffPrice با PatientShare + InsurerShare

#### علت:
- محاسبه دستی بدون بررسی تراز
- خطا در محاسبه فرانشیز
- خطا در محاسبه درصد پوشش

#### راه‌حل:
```csharp
// 🔧 CRITICAL FIX: تراز نهایی تضمین‌شده
calculatedShare = tariffPrice - insurerShare;
```

### مشکل 2: استفاده از منطق قدیمی (ضرب) به جای منطق جدید (جمع)

#### علت:
- استفاده از `service.Price * technicalFactor * professionalFactor` به جای `CalculateServicePriceWithFactorSettings`

#### راه‌حل:
```csharp
// استفاده از ServiceCalculationService برای منطق یکسان
calculatedPrice = _serviceCalculationService.CalculateServicePriceWithFactorSettings(
    service, _context, DateTime.Now);
```

### مشکل 3: عدم در نظر گیری فرانشیز در محاسبه

#### علت:
- محاسبه مستقیم InsurerShare بدون کسر فرانشیز

#### راه‌حل:
```csharp
var deductible = await GetDeductibleAsync(insurancePlanId, correlationId);
var coverableAmount = Math.Max(0, tariffPrice - deductible);
insurerShare = (coverableAmount * coveragePercent) / 100m;
```

### مشکل 4: خطا در محاسبه پوشش تکمیلی

#### علت:
- محاسبه بر اساس TariffPrice به جای RemainingAfterPrimary

#### راه‌حل:
```csharp
// پوشش تکمیلی درصدی از مبلغ باقی‌مانده بعد از بیمه پایه است
var remainingAfterPrimary = Math.Max(0, tariffPrice - insurerShare);
var supplementaryAmount = (remainingAfterPrimary * supplementaryCoveragePercent) / 100m;
```

---

## 📋 چک‌لیست صحت‌سنجی

### ✅ تعیین ست (TariffPrice):
- [ ] اگر TariffPrice موجود است، از آن استفاده می‌شود
- [ ] اگر TariffPrice null است، از `CalculateServicePriceWithFactorSettings` استفاده می‌شود
- [ ] اگر FactorSettings موجود نیست، از قیمت پایه Service استفاده می‌شود
- [ ] TariffPrice به ریال (بدون اعشار) گرد می‌شود

### ✅ محاسبه سهم بیمه (InsurerShare):
- [ ] از PlanService برای دریافت CoveragePercent استفاده می‌شود
- [ ] فرانشیز از مبلغ قابل پوشش کسر می‌شود
- [ ] InsurerShare بر اساس CoverableAmount محاسبه می‌شود
- [ ] اگر مقدار دستی ارائه شده، از آن استفاده می‌شود

### ✅ محاسبه سهم بیمار (PatientShare):
- [ ] فرانشیز در محاسبه در نظر گرفته می‌شود
- [ ] تراز نهایی تضمین می‌شود: `PatientShare = TariffPrice - InsurerShare`
- [ ] اگر مقدار دستی ارائه شده، از آن استفاده می‌شود

### ✅ محاسبه پوشش تکمیلی:
- [ ] پوشش تکمیلی بر اساس RemainingAfterPrimary محاسبه می‌شود
- [ ] پوشش کل با سقف 100% محاسبه می‌شود
- [ ] اگر مقدار دستی ارائه شده، از آن استفاده می‌شود

### ✅ Validation:
- [ ] Client-Side: بررسی تراز TariffPrice = PatientShare + InsurerShare
- [ ] Server-Side: بررسی مقادیر منفی
- [ ] Server-Side: بررسی تراز نهایی
- [ ] نمایش پیام‌های خطای واضح

---

## 🧪 تست‌های پیشنهادی

### تست 1: محاسبه TariffPrice
```csharp
// ورودی:
// ServiceId: 1 (ویزیت پزشک عمومی)
// TariffPrice: null

// خروجی مورد انتظار:
// TariffPrice: 2,376,000 ریال (محاسبه شده)
```

### تست 2: محاسبه InsurerShare با فرانشیز
```csharp
// ورودی:
// TariffPrice: 2,376,000 ریال
// Deductible: 100,000 ریال
// CoveragePercent: 90%

// خروجی مورد انتظار:
// CoverableAmount: 2,276,000 ریال
// InsurerShare: 2,048,400 ریال
```

### تست 3: تراز نهایی
```csharp
// ورودی:
// TariffPrice: 2,376,000 ریال
// InsurerShare: 2,048,400 ریال

// خروجی مورد انتظار:
// PatientShare: 327,600 ریال
// بررسی: 2,048,400 + 327,600 = 2,376,000 ✅
```

### تست 4: محاسبه پوشش تکمیلی
```csharp
// ورودی:
// TariffPrice: 2,376,000 ریال
// InsurerShare: 2,048,400 ریال
// SupplementaryCoveragePercent: 50%

// خروجی مورد انتظار:
// RemainingAfterPrimary: 327,600 ریال
// SupplementaryAmount: 163,800 ریال
// TotalCoverage: 93.1%
```

---

## 📚 منابع و مراجع

1. **`InsuranceTariffCalculationService.cs`** - منطق محاسبه TariffPrice
2. **`InsuranceTariffController.cs`** - منطق محاسبه سهم‌ها و پوشش
3. **`InsuranceTariff.cs`** - مدل تعرفه بیمه
4. **`InsuranceTariffViewModels.cs`** - ViewModels برای Views
5. **`Create.cshtml`** - فرم ایجاد تعرفه
6. **`Edit.cshtml`** - فرم ویرایش تعرفه
7. **`Details.cshtml`** - نمایش جزئیات تعرفه

---

## ✅ خلاصه

### تعیین ست (TariffPrice):
- اگر موجود است → استفاده می‌شود
- اگر null است → از `CalculateServicePriceWithFactorSettings` محاسبه می‌شود
- Fallback → قیمت پایه Service

### محاسبه سهم‌ها:
- **InsurerShare:** بر اساس CoveragePercent و فرانشیز
- **PatientShare:** تراز خودکار با `TariffPrice - InsurerShare`

### محاسبه پوشش تکمیلی:
- بر اساس RemainingAfterPrimary
- با سقف 100% برای پوشش کل

### Validation:
- Client-Side: بررسی تراز
- Server-Side: بررسی مقادیر منفی و تراز نهایی

---

**⚠️ توجه:** این مستندات بر اساس کد فعلی سیستم تهیه شده است. در صورت تغییر منطق، این مستندات باید به‌روزرسانی شوند.

