# 🎨 Reception V2 - خلاصه بهبود UX

**تاریخ:** 2024  
**هدف:** پیاده‌سازی UX بهبود یافته برای Reception V2 شامل SummaryHeader، Identity Section، Coverage Modal، و همگام‌سازی State

---

## ✅ فایل‌های ایجاد شده

### 1. پارشال‌های جدید

#### `Views/ReceptionV2/Partials/_ReceptionSummaryHeader.cshtml`
- ✅ هدر خلاصه با قابلیت کلیک
- ✅ نمایش نام بیمار، کدملی، سن، آدرس
- ✅ نمایش دپارتمان، پزشک
- ✅ نمایش بیمه‌ها (پایه + تکمیلی) با Badge
- ✅ نمایش سال مالی
- ✅ کلیک روی نام بیمار → باز شدن پرونده بیمار
- ✅ کلیک روی دپارتمان/پزشک → اسکرول/فوکوس به انتخابگر
- ✅ کلیک روی Badge بیمه‌ها → باز شدن Coverage Modal

#### `Views/ReceptionV2/Partials/_IdentitySection.cshtml`
- ✅ بخش هویت با فیلدهای ReadOnly
- ✅ نمایش کدملی، نام، نام خانوادگی، موبایل، جنسیت، تاریخ تولد، آدرس
- ✅ دکمه «ویرایش در پرونده بیمار» (فعال فقط اگر PatientId موجود باشد)
- ✅ پیام راهنما: «ویرایش اطلاعات هویتی فقط از طریق صفحهٔ «پرونده بیمار» امکان‌پذیر است.»

#### `Views/ReceptionV2/Partials/_CoverageModal.cshtml`
- ✅ Modal جزئیات پوشش بیمه
- ✅ تب‌های: پایه، تکمیلی، مؤثر
- ✅ باکس پیش‌نمایش قیمت خدمت (کد/نام خدمت → محاسبه قیمت)

---

### 2. اسکریپت‌های جدید

#### `Scripts/reception.v2/summary-header.js`
- ✅ مدیریت State یکپارچه (`ClinicApp.ReceptionV2.state`)
- ✅ رندر خلاصه در هدر (بیمار، دپارتمان، پزشک، بیمه‌ها، سال مالی)
- ✅ پر کردن بخش هویت (فیلدهای ReadOnly)
- ✅ محاسبه سن از تاریخ تولد ISO
- ✅ فرمت جنسیت برای نمایش (Male → مرد، Female → زن)
- ✅ هندلرهای کلیک:
  - `[data-action="open-patient"]` → باز کردن پرونده بیمار
  - `[data-action="goto-dept"]` → اسکرول/فوکوس به انتخاب دپارتمان
  - `[data-action="goto-doctor"]` → اسکرول/فوکوس به انتخاب پزشک
  - `[data-action="open-coverage"]` → باز کردن Coverage Modal
- ✅ لیسنر `rv2:stateChanged` برای همگام‌سازی

#### `Scripts/reception.v2/coverage-modal.js`
- ✅ باز کردن Modal با رویداد `rv2:coverage:open`
- ✅ فراخوانی API `GET /insurance/coverage`
- ✅ رندر تب‌های: پایه، تکمیلی، مؤثر
- ✅ Price Preview: محاسبه قیمت خدمت بر اساس کد/نام
- ✅ فراخوانی API `GET /item/price/preview`
- ✅ فرمت IRR با جداکننده هزارگان

---

### 3. API Endpoints

#### `Controllers/Api/ReceptionApiV1Controller.cs`

**GET `/api/v1/reception/insurance/coverage`**
- ✅ پارامترها: `patientId`, `basePlanId`, `supplementaryPlanId`
- ✅ بازگشت: `InsuranceCoverageDto` (پایه + تکمیلی + مؤثر)

**GET `/api/v1/reception/item/price/preview`**
- ✅ پارامترها: `PricePreviewRequestDto` (patientId, departmentId, doctorId, basePlanId, supplementaryPlanId, serviceCodeOrName)
- ✅ بازگشت: `PricePreviewResultDto` (Price, PatientShare, EffectiveCoveragePercent, PriceStr, PatientShareStr)

---

### 4. Facade Methods

#### `Services/Reception/ReceptionFacade.cs`

**`GetInsuranceCoverageAsync`**
- ✅ بارگذاری بیمه پایه (نام، درصد پوشش، فرانشیز، سقف‌ها)
- ✅ بارگذاری بیمه تکمیلی (نام، درصد پوشش، فرانشیز، سقف‌ها)
- ✅ محاسبه پوشش مؤثر: قاعده ترکیب (ابتدا پایه، سپس تکمیلی روی سهم باقیمانده بیمار)
- ✅ فرمت سقف‌ها (CeilingPerServiceStr, CeilingPerVisitStr, CeilingMonthlyStr, RemainingCeilingStr)

**`PreviewItemPriceAsync`**
- ✅ یافتن خدمت بر اساس کد/نام
- ✅ محاسبه قیمت پایه با `ServiceCalculationEngine`
- ✅ دریافت پوشش مؤثر با `GetInsuranceCoverageAsync`
- ✅ محاسبه سهم بیمار: `Price * (1 - EffectiveCoveragePercent/100)`
- ✅ فرمت مبالغ با `ToString("N0") + " ریال"`

---

### 5. DTOs

#### `Controllers/Api/ReceptionApiDtos.cs`

**`InsuranceCoverageSliceDto`**
- PlanName, FranchisePercent, CoveragePercent
- CeilingPerService, CeilingPerVisit, CeilingMonthly, RemainingCeiling
- CeilingPerServiceStr, CeilingPerVisitStr, CeilingMonthlyStr, RemainingCeilingStr

**`InsuranceCoverageEffectiveDto`**
- EffectiveCoveragePercent, PatientSharePercent, Notes

**`InsuranceCoverageDto`**
- Base, Supplementary, Effective

**`PricePreviewRequestDto`**
- PatientId, DepartmentId, DoctorId, BasePlanId, SupplementaryPlanId, ServiceCodeOrName

**`PricePreviewResultDto`**
- Price, PatientShare, EffectiveCoveragePercent, PriceStr, PatientShareStr

---

## ✅ سیم‌کشی State Events

### ماژول‌های موجود که State را Trigger می‌کنند:

#### `Scripts/reception.v2/patient-lookup.js`
- ✅ بعد از موفقیت Lookup/Fast-Create:
```javascript
$(document).trigger('rv2:stateChanged', {
  patient: {
    PatientId, NationalCode, FirstName, LastName,
    Gender, GenderTitle, BirthDate, BirthDateIso, BirthDateShamsi,
    Address, Mobile
  }
});
```

#### `Scripts/reception.v2/insurance-panel.js`
- ✅ بعد از موفقیت `persist()`:
```javascript
$(document).trigger('rv2:stateChanged', {
  insurances: {
    BasePlanId, BasePlanName,
    SupplementaryPlanId, SupplementaryPlanName
  }
});
```

#### `Scripts/reception.v2/clinic-dept-doctor.js`
- ✅ بعد از تغییر دپارتمان:
```javascript
$(document).trigger('rv2:stateChanged', {
  department: {
    DepartmentId, Name
  },
  financialYear: {
    Year, YearTitle
  }
});
```
- ✅ بعد از تغییر پزشک:
```javascript
$(document).trigger('rv2:stateChanged', {
  doctor: {
    DoctorId, FullName, Name
  }
});
```
- ✅ بعد از بارگذاری Bootstrap (سال مالی):
```javascript
$(document).trigger('rv2:stateChanged', {
  financialYear: {
    Year, YearTitle
  }
});
```

---

## ✅ به‌روزرسانی‌ها

### `Views/ReceptionV2/Index.cshtml`
- ✅ اضافه شدن `_ReceptionSummaryHeader` قبل از فرم
- ✅ اضافه شدن `_IdentitySection` قبل از فرم
- ✅ اضافه شدن `_CoverageModal` در انتهای View

### `App_Start/BundleConfig.cs`
- ✅ اضافه شدن `summary-header.js` به bundle
- ✅ اضافه شدن `coverage-modal.js` به bundle
- ✅ ترتیب: `reception-api.js` → `summary-header.js` → `patient-lookup.js` → ...

---

## 📋 چک‌لیست تست

### 1. Summary Header
- [ ] نمایش داده می‌شود (حتی اگر داده‌ای نباشد → "—")
- [ ] کلیک روی نام بیمار → باز شدن پرونده بیمار در تب جدید
- [ ] کلیک روی دپارتمان → اسکرول/فوکوس به `#DepartmentId`
- [ ] کلیک روی پزشک → اسکرول/فوکوس به `#DoctorId`
- [ ] کلیک روی Badge بیمه‌ها → باز شدن Coverage Modal

### 2. Identity Section
- [ ] نمایش داده می‌شود
- [ ] فیلدها ReadOnly هستند
- [ ] بعد از Lookup بیمار → فیلدها پر می‌شوند
- [ ] دکمه «ویرایش در پرونده بیمار» فعال می‌شود (اگر PatientId موجود باشد)

### 3. Coverage Modal
- [ ] باز می‌شود (کلیک روی Badge بیمه‌ها)
- [ ] تب‌های: پایه، تکمیلی، مؤثر نمایش داده می‌شوند
- [ ] اگر بیمه تنظیم نشده → پیام «اطلاعاتی برای نمایش وجود ندارد»
- [ ] Price Preview: کد/نام خدمت → محاسبه قیمت/پوشش/سهم بیمار
- [ ] Enter key در باکس کد خدمت → محاسبه

### 4. State Synchronization
- [ ] Lookup بیمار → Summary Header و Identity Section آپدیت می‌شوند
- [ ] تغییر دپارتمان → Summary Header آپدیت می‌شود
- [ ] تغییر پزشک → Summary Header آپدیت می‌شود
- [ ] تغییر بیمه‌ها → Summary Header آپدیت می‌شود
- [ ] Bootstrap → سال مالی در Summary Header نمایش داده می‌شود

---

## 🎯 خلاصه تغییرات

| # | فایل | تغییرات | وضعیت |
|---|------|---------|-------|
| 1 | `_ReceptionSummaryHeader.cshtml` | جدید | ✅ |
| 2 | `_IdentitySection.cshtml` | جدید | ✅ |
| 3 | `_CoverageModal.cshtml` | جدید | ✅ |
| 4 | `summary-header.js` | جدید | ✅ |
| 5 | `coverage-modal.js` | جدید | ✅ |
| 6 | `ReceptionApiV1Controller.cs` | اضافه شدن 2 endpoint | ✅ |
| 7 | `ReceptionFacade.cs` | اضافه شدن 2 method | ✅ |
| 8 | `ReceptionApiDtos.cs` | اضافه شدن 5 DTO | ✅ |
| 9 | `patient-lookup.js` | سیم‌کشی state event | ✅ |
| 10 | `insurance-panel.js` | سیم‌کشی state event | ✅ |
| 11 | `clinic-dept-doctor.js` | سیم‌کشی state event | ✅ |
| 12 | `Index.cshtml` | اضافه شدن 3 پارشال | ✅ |
| 13 | `BundleConfig.cs` | اضافه شدن 2 اسکریپت | ✅ |

---

## 📝 توصیه‌ها

### کوتاه‌مدت:
1. ✅ تست سریع 4 سناریو بالا
2. ✅ بررسی fallback logic در Coverage Modal (اگر API خطا دهد)

### میان‌مدت:
1. 📝 بهبود محاسبه فرانشیز (اگر Deductible درصدی است)
2. 📝 بارگذاری سقف‌ها از `InsuranceTariff` یا `PatientInsurance` (RemainingCeiling)

### بلندمدت:
1. 📝 Caching برای Coverage data (اگر بیمار/بیمه تغییر نکرده)
2. 📝 Metrics برای تعداد فراخوانی Coverage/Price Preview

---

**تاریخ تکمیل:** 2024  
**وضعیت:** ✅ **تمام تغییرات اعمال شد و آماده تست است**

