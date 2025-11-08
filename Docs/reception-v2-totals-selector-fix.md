# ✅ رفع مشکل نمایش جمع‌ها در Edit Mode

**تاریخ**: 1404/08/16  
**مشکل**: جمع‌ها در فرم ویرایش همه 0 نمایش داده می‌شدند  
**علت**: Selector های ID اشتباه در `reception-edit.js`

---

## 🐛 تشخیص مشکل

### View (_Totals.cshtml):
```html
<strong id="Gross">مبلغ کل</strong>
<strong id="InsurancePayable">سهم بیمه پایه</strong>
<strong id="SuppPayable">سهم بیمه تکمیلی</strong>
<strong id="PatientPayable">سهم بیمار</strong>
```

### JavaScript (قبل):
```javascript
$('#TotalAmount').text(...)        // ❌ ID وجود ندارد!
$('#InsurerShareAmount').text(...) // ❌ ID وجود ندارد!
$('#PatientCoPay').text(...)       // ❌ ID وجود ندارد!
```

**نتیجه**: جمع‌ها به‌روزرسانی نمی‌شدند چون selector ها اشتباه بودند.

---

## ✅ راه‌حل اعمال شده

### تغییرات در `reception-edit.js`:

#### 1. در `updateTotals()` - خط 270:
```javascript
// قبل:
$('#TotalAmount').text(formatIRR(data.TotalAmount || 0));
$('#InsurerShareAmount').text(formatIRR(data.InsurerShareAmount || 0));
$('#PatientCoPay').text(formatIRR(data.PatientCoPay || 0));

// بعد: ✅ Selector های صحیح
$('#Gross').text(formatIRR(data.TotalAmount || 0));
$('#InsurancePayable').text(formatIRR(data.InsurerShareAmount || 0));
$('#PatientPayable').text(formatIRR(data.PatientCoPay || 0));
```

#### 2. در `recalculateTotals()` - خط 506:
```javascript
// قبل:
$('#TotalAmount').text(formatIRR(totalAmount));
$('#InsurerShareAmount').text(formatIRR(totalInsurerShare));
$('#PatientCoPay').text(formatIRR(totalPatientShare));

// بعد: ✅ Selector های صحیح
$('#Gross').text(formatIRR(totalAmount));
$('#InsurancePayable').text(formatIRR(totalInsurerShare));
$('#PatientPayable').text(formatIRR(totalPatientShare));
```

#### 3. در `updateTotalsFromAPI()` - خط 517:
```javascript
// قبل:
$('#TotalAmount').text(formatIRR(totals.GrossAmount || 0));
$('#InsurerShareAmount').text(formatIRR(totals.BaseInsurancePayable || 0));
$('#PatientCoPay').text(formatIRR(totals.PatientPayable || 0));

// بعد: ✅ Selector های صحیح
$('#Gross').text(formatIRR(totals.GrossAmount || 0));
$('#InsurancePayable').text(formatIRR(totals.BaseInsurancePayable || 0));
$('#PatientPayable').text(formatIRR(totals.PatientPayable || 0));
```

---

## 📊 Mapping Selectors

| View ID | JavaScript Selector (قبل) ❌ | JavaScript Selector (بعد) ✅ |
|---------|------------------------------|------------------------------|
| `#Gross` | `#TotalAmount` | `#Gross` |
| `#InsurancePayable` | `#InsurerShareAmount` | `#InsurancePayable` |
| `#PatientPayable` | `#PatientCoPay` | `#PatientPayable` |
| `#SuppPayable` | — | `#SuppPayable` (آینده) |

---

## 🎯 نتیجه

- ✅ جمع‌ها در edit mode صحیح نمایش داده می‌شوند
- ✅ مبلغ کل، سهم بیمه، سهم بیمار به‌روز می‌شوند
- ✅ Consistency بین View و JavaScript

---

**✅ مشکل به طور کامل رفع شد.**

