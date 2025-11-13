# 🏥 منطق محاسبه بیمه تکمیلی

## 📊 منطق صحیح

### فرمول محاسبه:

```
1. مبلغ کل خدمت = ServiceAmount
2. پوشش بیمه پایه = PrimaryCoverage (مثلاً 70%)
3. سهم بیمار از بیمه پایه = ServiceAmount - PrimaryCoverage
4. پوشش بیمه تکمیلی = (سهم بیمار از بیمه پایه) × (درصد پوشش بیمه تکمیلی)
5. سهم نهایی بیمار = سهم بیمار از بیمه پایه - پوشش بیمه تکمیلی
```

### مثال عملی:

**خدمت**: 2,376,000 ریال

**بیمه پایه (70%)**:
- پوشش بیمه پایه: 1,663,200 ریال
- سهم بیمار از بیمه پایه: 712,800 ریال

**بیمه تکمیلی (100% پوشش)**:
- پوشش بیمه تکمیلی: 712,800 × 100% = 712,800 ریال
- سهم نهایی بیمار: 712,800 - 712,800 = **0 ریال** ✅

## ✅ نکات مهم

1. **سهم بیمه تکمیلی = باقی‌مانده سهم بیمار** (نه سهم بیمه پایه)
   - `remainingAmount` = `ServiceAmount - PrimaryCoverage` = سهم بیمار از بیمه پایه
   - بیمه تکمیلی باید این مبلغ را پوشش دهد

2. **بیمه تکمیلی 100% سهم بیمار را پوشش می‌دهد**:
   - اگر `SupplementaryCoveragePercent = 100%` باشد
   - سهم نهایی بیمار = 0

3. **بیمه تکمیلی از سهم بیمه پایه استفاده نمی‌کند**:
   - بیمه تکمیلی فقط سهم بیمار را پوشش می‌دهد
   - نه سهم بیمه پایه

## 🔧 پیاده‌سازی در کد

```csharp
// محاسبه سهم بیمار از بیمه پایه
var remainingAmount = serviceAmount - primaryResult.InsuranceCoverage;

// محاسبه پوشش بیمه تکمیلی از سهم بیمار
var coveragePercent = supplementaryTariff.SupplementaryCoveragePercent.Value / 100m;
var supplementaryCoverage = remainingAmount * coveragePercent;

// سهم نهایی بیمار
var finalPatientShare = remainingAmount - supplementaryCoverage;
```

## 📝 لاگ‌های بهبود یافته

کد حالا لاگ‌های واضح‌تری دارد:
- `PatientShareFromPrimary`: سهم بیمار از بیمه پایه
- `SupplementaryCoverage`: پوشش بیمه تکمیلی (از سهم بیمار)
- `RemainingPatientShare`: سهم باقی‌مانده بیمار
- `FinalPatientShare`: سهم نهایی بیمار

---

**تاریخ ایجاد**: 2025-01-27  
**وضعیت**: ✅ تکمیل شده  
**نسخه**: 1.0.0

