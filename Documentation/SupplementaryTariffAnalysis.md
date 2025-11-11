# 📋 تحلیل کامل ماژول SupplementaryTariff

## 🎯 مقدمه

این سند شامل تحلیل کامل ماژول `SupplementaryTariff` است که شامل:
1. **منطق تعیین ست (TariffPrice) برای بیمه‌های تکمیلی**
2. **منطق محاسبه سهم بیمار و بیمه تکمیلی**
3. **منطق محاسبه پوشش تکمیلی (SupplementaryCoveragePercent)**
4. **منطق CreateInsuranceCombination**
5. **بررسی Views و Validation**
6. **مشکلات احتمالی و راه‌حل‌ها**

---

## 📌 تعریف "تعیین ست" برای بیمه‌های تکمیلی

### مفهوم:
**"تعیین ست"** برای بیمه‌های تکمیلی به معنای **تعیین قیمت تعرفه (TariffPrice)** و **تنظیمات پوشش تکمیلی** برای یک خدمت در ترکیب با یک بیمه پایه است.

### تفاوت با InsuranceTariff:
- **InsuranceTariff:** تعرفه برای بیمه پایه (Primary Insurance)
- **SupplementaryTariff:** تعرفه برای بیمه تکمیلی (Supplementary Insurance) که **بعد از بیمه پایه** اعمال می‌شود

### فیلدهای کلیدی:
```csharp
public decimal? TariffPrice { get; set; }              // قیمت تعرفه (ریال)
public decimal? PatientShare { get; set; }             // سهم بیمار (ریال)
public decimal? InsurerShare { get; set; }             // سهم بیمه تکمیلی (ریال)
public decimal? SupplementaryCoveragePercent { get; set; } // درصد پوشش تکمیلی
public decimal? SupplementaryMaxPayment { get; set; }  // سقف پرداخت تکمیلی
public int PrimaryInsurancePlanId { get; set; }        // شناسه بیمه پایه
public int InsurancePlanId { get; set; }              // شناسه بیمه تکمیلی
```

---

## 🔢 منطق تعیین ست (TariffPrice) برای بیمه‌های تکمیلی

### الگوریتم محاسبه:

#### 1. دریافت قیمت خدمت:
```csharp
// استفاده از قیمت ثابت یا محاسبه شده
if (service.Price > 0)
{
    actualServicePrice = service.Price; // استفاده از قیمت ثابت
}
else
{
    // محاسبه از اجزای فنی و حرفه‌ای
    actualServicePrice = _serviceCalculationService.CalculateServicePrice(service);
}
```

#### 2. محاسبه پوشش بیمه پایه:
```csharp
var primaryDeductible = primaryPlan.Deductible;
var primaryCoveragePercent = primaryPlan.CoveragePercent;
var primaryCoverableAmount = Math.Max(0, actualServicePrice - primaryDeductible);
var primaryInsuranceCoverage = primaryCoverableAmount * (primaryCoveragePercent / 100m);
var patientShareFromPrimary = actualServicePrice - primaryInsuranceCoverage;
```

#### 3. محاسبه پوشش بیمه تکمیلی:
```csharp
// 🔧 CRITICAL FIX: بیمه تکمیلی روی سهم باقی‌مانده بیمار اعمال می‌شود
var supplementaryCoveragePercent = supplementaryPlan.CoveragePercent;
var supplementaryCoverage = patientShareFromPrimary * (supplementaryCoveragePercent / 100m);
var finalPatientShare = patientShareFromPrimary - supplementaryCoverage;
```

#### 4. تعیین TariffPrice:
```csharp
// TariffPrice = مبلغ کل خدمت (قبل از اعمال بیمه‌ها)
TariffPrice = calculationResult.ServiceAmount; // مبلغ کل خدمت
PatientShare = calculationResult.FinalPatientShare; // سهم نهایی بیمار
InsurerShare = calculationResult.SupplementaryInsuranceCoverage; // سهم بیمه تکمیلی
```

### کلاس مسئول:
- **`SupplementaryTariffController.GetSmartFormData`**
- **`SupplementaryCombinationService.CreateCombinationAsync`**

### مثال:
```csharp
// خدمت: ویزیت پزشک عمومی (970000)
// قیمت پایه: 2,376,000 ریال
// 
// بیمه پایه (سلامت):
// - Deductible: 100,000 ریال
// - CoveragePercent: 90%
// - CoverableAmount: 2,376,000 - 100,000 = 2,276,000 ریال
// - PrimaryCoverage: 2,276,000 × 90% = 2,048,400 ریال
// - PatientShareFromPrimary: 2,376,000 - 2,048,400 = 327,600 ریال
// 
// بیمه تکمیلی (ملت VIP):
// - CoveragePercent: 50%
// - SupplementaryCoverage: 327,600 × 50% = 163,800 ریال
// - FinalPatientShare: 327,600 - 163,800 = 163,800 ریال
// 
// TariffPrice: 2,376,000 ریال
// PatientShare: 163,800 ریال
// InsurerShare: 163,800 ریال (سهم بیمه تکمیلی)
```

---

## 💰 منطق محاسبه سهم بیمار و بیمه تکمیلی

### فرمول پایه:
```
TariffPrice = PrimaryCoverage + SupplementaryCoverage + FinalPatientShare
```

### الگوریتم محاسبه:

#### 1. محاسبه پوشش بیمه پایه:
```csharp
var primaryDeductible = primaryPlan.Deductible;
var primaryCoveragePercent = primaryPlan.CoveragePercent;
var primaryCoverableAmount = Math.Max(0, serviceAmount - primaryDeductible);
var primaryCoverageAmount = primaryCoverableAmount * (primaryCoveragePercent / 100m);
```

#### 2. محاسبه سهم بیمار از بیمه پایه:
```csharp
var patientShareFromPrimary = serviceAmount - primaryCoverageAmount;
```

#### 3. محاسبه پوشش بیمه تکمیلی:
```csharp
// استفاده از سرویس تزریق شده
var calculationResult = _supplementaryCalculationService.CalculateForSpecificScenario(
    serviceAmount: serviceAmount,
    primaryCoverage: primaryCoverageAmount,
    supplementaryCoveragePercent: coveragePercent,
    supplementaryMaxPayment: maxPayment > 0 ? maxPayment : (decimal?)null);
```

#### 4. تعیین سهم‌ها:
```csharp
TariffPrice = calculationResult.ServiceAmount; // مبلغ کل خدمت
PatientShare = calculationResult.FinalPatientShare; // سهم نهایی بیمار
InsurerShare = calculationResult.SupplementaryInsuranceCoverage; // سهم بیمه تکمیلی
```

### کلاس مسئول:
- **`SupplementaryCombinationService.CreateCombinationAsync`**
- **`ISupplementaryInsuranceCalculationService.CalculateForSpecificScenario`**

### مثال:
```csharp
// ServiceAmount: 2,376,000 ریال
// PrimaryCoverage: 2,048,400 ریال
// PatientShareFromPrimary: 327,600 ریال
// SupplementaryCoveragePercent: 50%
// 
// SupplementaryCoverage: 327,600 × 50% = 163,800 ریال
// FinalPatientShare: 327,600 - 163,800 = 163,800 ریال
// 
// TariffPrice: 2,376,000 ریال
// PatientShare: 163,800 ریال
// InsurerShare: 163,800 ریال
```

---

## 🏥 منطق محاسبه پوشش تکمیلی (SupplementaryCoveragePercent)

### فرمول پایه:
```
SupplementaryCoverage = (RemainingAfterPrimary × SupplementaryCoveragePercent) / 100
```

### الگوریتم محاسبه:

#### 1. محاسبه باقیمانده بعد از بیمه پایه:
```csharp
var remainingAfterPrimary = Math.Max(0, tariffPrice - insurerShare);
// یا
var remainingAfterPrimary = patientShareFromPrimary;
```

#### 2. محاسبه پوشش تکمیلی:
```csharp
if (tariff.SupplementaryCoveragePercent.HasValue)
{
    supplementaryCoverage = remainingAmount * (tariff.SupplementaryCoveragePercent.Value / 100);
}
```

#### 3. اعمال سقف پرداخت:
```csharp
if (tariff.SupplementaryMaxPayment.HasValue && supplementaryCoverage > tariff.SupplementaryMaxPayment.Value)
{
    supplementaryCoverage = tariff.SupplementaryMaxPayment.Value;
}
```

#### 4. محاسبه سهم نهایی بیمار:
```csharp
var finalPatientShare = remainingAfterPrimary - supplementaryCoverage;
```

### کلاس مسئول:
- **`InsuranceTariffService.CalculateSupplementaryTariffAsync`**
- **`SupplementaryInsuranceService.CalculateSupplementaryCoverageAsync`**

### مثال:
```csharp
// RemainingAfterPrimary: 327,600 ریال
// SupplementaryCoveragePercent: 50%
// SupplementaryMaxPayment: null
// 
// SupplementaryCoverage: 327,600 × 50% = 163,800 ریال
// FinalPatientShare: 327,600 - 163,800 = 163,800 ریال
```

### مثال با سقف پرداخت:
```csharp
// RemainingAfterPrimary: 327,600 ریال
// SupplementaryCoveragePercent: 50%
// SupplementaryMaxPayment: 100,000 ریال
// 
// SupplementaryCoverage (بدون سقف): 327,600 × 50% = 163,800 ریال
// SupplementaryCoverage (با سقف): Min(163,800, 100,000) = 100,000 ریال
// FinalPatientShare: 327,600 - 100,000 = 227,600 ریال
```

---

## 🔧 منطق CreateInsuranceCombination

### هدف:
ایجاد ترکیب بیمه پایه و تکمیلی (مثل سلامت + ملت VIP)

### الگوریتم:

#### 1. Validation ورودی‌ها:
```csharp
if (serviceId <= 0) return Json(new { success = false, message = "شناسه خدمت نامعتبر است" });
if (primaryPlanId <= 0) return Json(new { success = false, message = "شناسه طرح بیمه اصلی نامعتبر است" });
if (supplementaryPlanId <= 0) return Json(new { success = false, message = "شناسه طرح بیمه تکمیلی نامعتبر است" });
if (coveragePercent < 0 || coveragePercent > 100) return Json(new { success = false, message = "درصد پوشش باید بین 0 تا 100 باشد" });
```

#### 2. دریافت اطلاعات طرح‌های بیمه:
```csharp
var planTasks = new[]
{
    _planService.GetPlanDetailsAsync(primaryPlanId),
    _planService.GetPlanDetailsAsync(supplementaryPlanId)
};
await Task.WhenAll(planTasks);
```

#### 3. ایجاد ترکیب:
```csharp
var savedTariff = await _supplementaryCombinationService.CreateCombinationAsync(
    serviceId, primaryPlanId, supplementaryPlanId, coveragePercent, maxPayment);
```

#### 4. بازگشت نتیجه:
```csharp
return Json(new { 
    success = true, 
    message = "تعیین ست بیمه با موفقیت ایجاد شد",
    data = new {
        tariffId = savedTariff.InsuranceTariffId,
        serviceId = savedTariff.ServiceId,
        insurancePlanId = savedTariff.InsurancePlanId,
        tariffPrice = savedTariff.TariffPrice,
        patientShare = savedTariff.PatientShare,
        insurerShare = savedTariff.InsurerShare,
        supplementaryCoveragePercent = savedTariff.SupplementaryCoveragePercent,
        supplementaryMaxPayment = savedTariff.SupplementaryMaxPayment
    }
});
```

### کلاس مسئول:
- **`SupplementaryTariffController.CreateInsuranceCombination`**
- **`SupplementaryCombinationService.CreateCombinationAsync`**

---

## 📊 بررسی Views

### 1. Index.cshtml
- **کاربرد:** نمایش لیست تعرفه‌های بیمه تکمیلی
- **ویژگی‌ها:**
  - فیلتر بر اساس ارائه‌دهنده، طرح، خدمت
  - نمایش آمار (تعداد کل، فعال، منقضی)
  - جستجوی پیشرفته

### 2. Create.cshtml
- **کاربرد:** ایجاد تعرفه بیمه تکمیلی جدید
- **ویژگی‌ها:**
  - انتخاب بیمه پایه و تکمیلی
  - انتخاب خدمت
  - ورود دستی TariffPrice (اختیاری)
  - محاسبه خودکار TariffPrice بر اساس FactorSettings
  - محاسبه خودکار PatientShare و InsurerShare
  - نمایش نتایج محاسبه در Real-time
  - Validation کامل

### 3. Edit.cshtml
- **کاربرد:** ویرایش تعرفه بیمه تکمیلی موجود
- **ویژگی‌ها:**
  - مشابه Create.cshtml
  - نمایش مقادیر فعلی
  - امکان تغییر TariffPrice، PatientShare، InsurerShare
  - محاسبه مجدد در صورت تغییر

### 4. Details.cshtml
- **کاربرد:** مشاهده جزئیات تعرفه بیمه تکمیلی
- **ویژگی‌ها:**
  - نمایش تمام اطلاعات تعرفه
  - نمایش محاسبات (TariffPrice، PatientShare، InsurerShare)
  - نمایش پوشش تکمیلی و سقف پرداخت

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
[Required(ErrorMessage = "قیمت تعرفه الزامی است")]
[Range(0, double.MaxValue, ErrorMessage = "قیمت تعرفه نمی‌تواند منفی باشد.")]
[RegularExpression(@"^\d+$", ErrorMessage = "قیمت تعرفه باید عدد صحیح مثبت باشد")]
public decimal? TariffPrice { get; set; }

[Required(ErrorMessage = "درصد پوشش تکمیلی الزامی است")]
[PercentageValidation(ErrorMessage = "درصد پوشش باید عددی بین 0 تا 100 باشد (حداکثر 2 رقم اعشار)")]
public decimal? SupplementaryCoveragePercent { get; set; }
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
- خطا در محاسبه پوشش تکمیلی
- عدم در نظر گیری سقف پرداخت

#### راه‌حل:
```csharp
// 🔧 CRITICAL FIX: تراز نهایی تضمین‌شده
finalPatientShare = remainingAfterPrimary - supplementaryCoverage;
```

### مشکل 2: استفاده از منطق قدیمی (ضرب) به جای منطق جدید (جمع)

#### علت:
- استفاده از `service.Price * technicalFactor * professionalFactor` به جای `CalculateServicePriceWithFactorSettings`

#### راه‌حل:
```csharp
// استفاده از ServiceCalculationService برای منطق یکسان
actualServicePrice = _serviceCalculationService.CalculateServicePrice(service);
```

### مشکل 3: عدم در نظر گیری سقف پرداخت

#### علت:
- محاسبه مستقیم SupplementaryCoverage بدون اعمال سقف

#### راه‌حل:
```csharp
if (tariff.SupplementaryMaxPayment.HasValue && supplementaryCoverage > tariff.SupplementaryMaxPayment.Value)
{
    supplementaryCoverage = tariff.SupplementaryMaxPayment.Value;
}
```

### مشکل 4: خطا در محاسبه پوشش تکمیلی

#### علت:
- محاسبه بر اساس TariffPrice به جای RemainingAfterPrimary

#### راه‌حل:
```csharp
// 🔧 CRITICAL FIX: بیمه تکمیلی روی سهم باقی‌مانده بیمار اعمال می‌شود
var supplementaryCoverage = patientShareFromPrimary * (supplementaryCoveragePercent / 100m);
```

---

## 📋 چک‌لیست صحت‌سنجی

### ✅ تعیین ست (TariffPrice):
- [ ] اگر TariffPrice موجود است، از آن استفاده می‌شود
- [ ] اگر TariffPrice null است، از `CalculateServicePrice` استفاده می‌شود
- [ ] اگر Service.Price موجود نیست، از ServiceComponents محاسبه می‌شود
- [ ] TariffPrice به ریال (بدون اعشار) گرد می‌شود

### ✅ محاسبه سهم بیمه تکمیلی (InsurerShare):
- [ ] از RemainingAfterPrimary برای محاسبه استفاده می‌شود
- [ ] SupplementaryCoveragePercent اعمال می‌شود
- [ ] سقف پرداخت (SupplementaryMaxPayment) اعمال می‌شود
- [ ] اگر مقدار دستی ارائه شده، از آن استفاده می‌شود

### ✅ محاسبه سهم بیمار (PatientShare):
- [ ] از RemainingAfterPrimary شروع می‌شود
- [ ] SupplementaryCoverage از آن کسر می‌شود
- [ ] تراز نهایی تضمین می‌شود: `PatientShare = RemainingAfterPrimary - SupplementaryCoverage`
- [ ] اگر مقدار دستی ارائه شده، از آن استفاده می‌شود

### ✅ محاسبه پوشش تکمیلی:
- [ ] پوشش تکمیلی بر اساس RemainingAfterPrimary محاسبه می‌شود
- [ ] سقف پرداخت اعمال می‌شود
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
// Service.Price: 2,376,000 ریال

// خروجی مورد انتظار:
// TariffPrice: 2,376,000 ریال
```

### تست 2: محاسبه پوشش تکمیلی با سقف
```csharp
// ورودی:
// TariffPrice: 2,376,000 ریال
// PrimaryCoverage: 2,048,400 ریال
// RemainingAfterPrimary: 327,600 ریال
// SupplementaryCoveragePercent: 50%
// SupplementaryMaxPayment: 100,000 ریال

// خروجی مورد انتظار:
// SupplementaryCoverage (بدون سقف): 163,800 ریال
// SupplementaryCoverage (با سقف): 100,000 ریال
// FinalPatientShare: 227,600 ریال
```

### تست 3: تراز نهایی
```csharp
// ورودی:
// TariffPrice: 2,376,000 ریال
// PrimaryCoverage: 2,048,400 ریال
// SupplementaryCoverage: 163,800 ریال

// خروجی مورد انتظار:
// PatientShare: 163,800 ریال
// InsurerShare: 163,800 ریال
// بررسی: 2,048,400 + 163,800 + 163,800 = 2,376,000 ✅
```

### تست 4: CreateInsuranceCombination
```csharp
// ورودی:
// ServiceId: 1
// PrimaryPlanId: 1 (سلامت)
// SupplementaryPlanId: 2 (ملت VIP)
// CoveragePercent: 50%
// MaxPayment: 100,000 ریال

// خروجی مورد انتظار:
// TariffPrice: 2,376,000 ریال
// PatientShare: 227,600 ریال
// InsurerShare: 100,000 ریال (با سقف)
// SupplementaryCoveragePercent: 50%
```

---

## 📚 منابع و مراجع

1. **`SupplementaryTariffController.cs`** - منطق Controller
2. **`SupplementaryCombinationService.cs`** - منطق ایجاد ترکیب
3. **`InsuranceTariffService.cs`** - منطق محاسبه تعرفه تکمیلی
4. **`SupplementaryInsuranceService.cs`** - منطق محاسبه پوشش تکمیلی
5. **`SupplementaryTariffCreateEditViewModel.cs`** - ViewModel برای Views
6. **`Create.cshtml`** - فرم ایجاد تعرفه
7. **`Edit.cshtml`** - فرم ویرایش تعرفه
8. **`Details.cshtml`** - نمایش جزئیات تعرفه

---

## ✅ خلاصه

### تعیین ست (TariffPrice):
- اگر موجود است → استفاده می‌شود
- اگر null است → از `CalculateServicePrice` محاسبه می‌شود
- Fallback → قیمت پایه Service

### محاسبه سهم‌ها:
- **PrimaryCoverage:** بر اساس Deductible و CoveragePercent
- **RemainingAfterPrimary:** `TariffPrice - PrimaryCoverage`
- **SupplementaryCoverage:** بر اساس `RemainingAfterPrimary × SupplementaryCoveragePercent`
- **FinalPatientShare:** `RemainingAfterPrimary - SupplementaryCoverage`

### محاسبه پوشش تکمیلی:
- بر اساس RemainingAfterPrimary
- با اعمال سقف پرداخت (SupplementaryMaxPayment)

### Validation:
- Client-Side: بررسی تراز
- Server-Side: بررسی مقادیر منفی و تراز نهایی

---

**⚠️ توجه:** این مستندات بر اساس کد فعلی سیستم تهیه شده است. در صورت تغییر منطق، این مستندات باید به‌روزرسانی شوند.

