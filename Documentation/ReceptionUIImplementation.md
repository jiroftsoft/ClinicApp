# 🏥 مستندات پیاده‌سازی UI برای محاسبه Real-Time بیمه

## 📋 خلاصه

این سند مستندات کامل پیاده‌سازی UI برای نمایش محاسبه real-time بیمه در ماژول پذیرش است.

---

## ✅ تغییرات انجام شده

### 1. JavaScript (`Scripts/reception.v2/service-lookup.js`)

#### 1.1 تابع `formatInsuranceCalculation`
- **هدف**: فرمت کردن اطلاعات محاسبه بیمه برای نمایش در UI
- **ورودی**: `insuranceCalc` (Object) - اطلاعات محاسبه بیمه
- **خروجی**: Object شامل:
  - `primaryCoverage`, `supplementaryCoverage`, `totalCoverage`, `patientShare`
  - `primaryCoverageStr`, `supplementaryCoverageStr`, `totalCoverageStr`, `patientShareStr`
  - `coverageStatus` - وضعیت پوشش (پوشش کامل، پوشش ناقص، بدون پوشش)
  - `statusClass` - کلاس CSS برای رنگ‌بندی
  - `statusBadge` - HTML badge برای نمایش وضعیت

#### 1.2 به‌روزرسانی `proceedWithAddItem`
- **افزودن Loading State**: نمایش "در حال افزودن..." هنگام درخواست
- **استخراج `InsuranceCalculation`**: از response استخراج می‌شود
- **استفاده از اطلاعات بیمه Real-Time**: در صورت موجود بودن، از `InsuranceCalculation` استفاده می‌شود
- **نمایش وضعیت پوشش**: badge رنگی در ستون جدید
- **ذخیره اطلاعات بیمه**: در `data('insurance')` ذخیره می‌شود

#### 1.3 بهبود Error Handling
- **پیام‌های خطای واضح**: نمایش پیام خطای دقیق
- **بازگرداندن دکمه**: در `finally` دکمه به حالت عادی برمی‌گردد

### 2. View (`Views/ReceptionV2/Partials/_ItemsGrid.cshtml`)

#### 2.1 افزودن ستون‌های جدید
- **ستون "وضعیت پوشش"**: نمایش badge وضعیت پوشش
- **ستون "عملیات"**: دکمه حذف

---

## 🎨 نمایش وضعیت پوشش

### Badge ها:
- **پوشش کامل**: `<span class="badge bg-success">پوشش کامل</span>`
- **پوشش ناقص**: `<span class="badge bg-warning">پوشش ناقص</span>`
- **بدون پوشش**: `<span class="badge bg-danger">بدون پوشش</span>`

### رنگ‌بندی ردیف:
- **پوشش کامل**: `text-success`
- **پوشش ناقص**: `text-warning`
- **بدون پوشش**: `text-danger`

---

## 📊 ساختار Response

```javascript
{
  "success": true,
  "data": {
    "ReceptionId": 123,
    "ServiceId": 456,
    "Quantity": 1,
    "UnitPrice": 2376000,
    "ItemTotal": 2376000,
    "InsuranceCalculation": {
      "PrimaryCoverage": 1663200,
      "SupplementaryCoverage": 0,
      "TotalInsuranceCoverage": 1663200,
      "PatientShare": 712800,
      "CoverageStatus": "پوشش ناقص",
      "PrimaryCoveragePercent": 70,
      "SupplementaryCoveragePercent": 0,
      "TotalCoveragePercent": 70
    },
    "ReceptionTotals": {
      "GrossAmount": 2376000,
      "BaseInsurancePayable": 1663200,
      "SupplementaryInsurancePayable": 0,
      "PatientPayable": 712800
    }
  }
}
```

---

## 🔄 Flow کامل

```
1. کاربر خدمت را انتخاب می‌کند
2. دکمه "افزودن" کلیک می‌شود
3. Loading state نمایش داده می‌شود
4. درخواست POST /item/add ارسال می‌شود
5. سرور محاسبه بیمه real-time انجام می‌دهد
6. Response شامل InsuranceCalculation است
7. JavaScript اطلاعات را استخراج می‌کند
8. ردیف جدید با اطلاعات کامل اضافه می‌شود
9. Badge وضعیت پوشش نمایش داده می‌شود
10. Totals به‌روزرسانی می‌شود
```

---

## 🧪 تست

### سناریو 1: پوشش کامل
- **ورودی**: خدمت با بیمه کامل
- **خروجی مورد انتظار**: Badge سبز "پوشش کامل"

### سناریو 2: پوشش ناقص
- **ورودی**: خدمت با بیمه ناقص
- **خروجی مورد انتظار**: Badge زرد "پوشش ناقص"

### سناریو 3: بدون پوشش
- **ورودی**: خدمت بدون بیمه
- **خروجی مورد انتظار**: Badge قرمز "بدون پوشش"

### سناریو 4: خطا در محاسبه
- **ورودی**: خطا در محاسبه بیمه
- **خروجی مورد انتظار**: Badge قرمز "بدون پوشش" + پیام خطا

---

## 📝 نکات مهم

1. **Fallback**: اگر `InsuranceCalculation` موجود نباشد، از `pricingData` استفاده می‌شود
2. **پشتیبانی از PascalCase و camelCase**: کد از هر دو ساختار پشتیبانی می‌کند
3. **Loading State**: دکمه در حین درخواست غیرفعال می‌شود
4. **Error Handling**: خطاها به درستی مدیریت می‌شوند

---

**تاریخ ایجاد**: 2025-01-27  
**وضعیت**: ✅ تکمیل شده  
**نسخه**: 1.0.0

