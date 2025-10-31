# 🔄 به‌روزرسانی Frontend برای ویرایش بیمه در پذیرش

**تاریخ ایجاد**: 2024-12-20  
**هدف**: بهبود UI و UX برای ویرایش، تغییر و حذف بیمه‌های پایه و تکمیلی در فرم پذیرش

---

## ✅ تغییرات اعمال شده

### 1. **View**: `Views/ReceptionV2/Partials/_Insurance.cshtml`

#### افزودن دکمه حذف بیمه تکمیلی

```html
<div class="col-md-6">
  <div class="d-flex gap-2 align-items-end">
    <div class="flex-grow-1">
      @Html.LabelFor(m => m.SupplementaryPlanId, new { @class = "form-label" })
      @Html.DropDownListFor(m => m.SupplementaryPlanId, ...)
      @Html.ValidationMessageFor(...)
    </div>
    <!-- 🔥 دکمه حذف بیمه تکمیلی -->
    <button id="btnRemoveSupp" class="btn btn-outline-danger btn-sm" type="button" 
            title="حذف بیمه تکمیلی" style="display: none;">
      <i class="fas fa-times"></i>
    </button>
  </div>
</div>
```

**ویژگی‌ها**:
- ✅ دکمه در کنار dropdown بیمه تکمیلی
- ✅ به صورت پیش‌فرض مخفی است (`display: none`)
- ✅ فقط زمانی نمایش داده می‌شود که بیمه تکمیلی انتخاب شده باشد

---

### 2. **JavaScript**: `Scripts/reception.v2/insurance-panel.js`

#### الف) تابع `toggleRemoveButton()`

```javascript
function toggleRemoveButton() {
  const hasValue = $suppPlan.val() && $suppPlan.val() !== '';
  if ($btnRemoveSupp.length) {
    if (hasValue) {
      $btnRemoveSupp.show();
    } else {
      $btnRemoveSupp.hide();
    }
  }
}
```

**عملکرد**:
- ✅ بررسی می‌کند آیا dropdown بیمه تکمیلی مقدار دارد یا نه
- ✅ دکمه حذف را نمایش می‌دهد یا مخفی می‌کند

#### ب) بهبود `removeSupplementary()`

```javascript
function removeSupplementary() {
  console.log('🏥 V2: Removing supplementary insurance');
  
  // پاک کردن dropdown
  $suppPlan.val('').trigger('change'); // trigger change برای persist خودکار
  
  // مخفی کردن دکمه
  toggleRemoveButton();
  
  // پیام موفقیت
  toastr.info('بیمه تکمیلی حذف شد');
}
```

**بهبودها**:
- ✅ پس از پاک کردن dropdown، `change` event را trigger می‌کند
- ✅ این باعث می‌شود `persist()` خودکار صدا زده شود
- ✅ دکمه حذف را مخفی می‌کند

#### ج) بهبود Event Handler برای `SuppPlan`

```javascript
$suppPlan.on('change', function() {
  console.log('🏥 V2: Supplementary plan changed');
  
  // نمایش/مخفی کردن دکمه حذف
  toggleRemoveButton();
  
  // اگر dropdown خالی شد (انتخاب "انتخاب کنید")، بیمه تکمیلی را حذف کن
  const selectedValue = $suppPlan.val();
  if (!selectedValue || selectedValue === '') {
    console.log('🏥 V2: Supplementary plan cleared, removing insurance');
    // persist() را صدا بزن که مقدار null را به سرور بفرستد
    persist();
  } else {
    console.log('🏥 V2: Supplementary plan selected, persisting');
    persist();
  }
});
```

**ویژگی‌ها**:
- ✅ هنگام تغییر dropdown، دکمه حذف را به‌روزرسانی می‌کند
- ✅ اگر dropdown خالی شد، خودکار `persist()` را صدا می‌زند
- ✅ اگر مقدار انتخاب شد، `persist()` را صدا می‌زند

#### د) بهبود `set()` برای نمایش/مخفی کردن دکمه

```javascript
if (suppPlanIdToSet) {
  // ... set value
  toggleRemoveButton(); // Update button visibility
} else {
  $suppPlan.val(''); // Clear if no value
  toggleRemoveButton(); // Hide button
}
```

**ویژگی‌ها**:
- ✅ پس از set کردن بیمه تکمیلی، دکمه حذف را نمایش می‌دهد
- ✅ اگر بیمه تکمیلی نباشد، دکمه را مخفی می‌کند

#### ه) Initialization

```javascript
$(document).ready(function() {
  // بارگذاری لیست بیمه‌ها
  loadPlans().catch(function(err) {
    console.warn('🏥 V2: Failed to load insurance plans on init:', err);
  });
  
  // نمایش/مخفی کردن دکمه حذف بیمه تکمیلی بر اساس مقدار فعلی
  toggleRemoveButton();
});
```

**ویژگی‌ها**:
- ✅ هنگام بارگذاری صفحه، وضعیت دکمه حذف را بررسی می‌کند
- ✅ اگر بیمه تکمیلی قبلاً انتخاب شده باشد، دکمه را نمایش می‌دهد

---

## 🎯 سناریوهای کاربری

### سناریو 1: تغییر بیمه پایه

```
کاربر:
  1. Dropdown بیمه پایه را باز می‌کند
  2. بیمه جدید را انتخاب می‌کند

سیستم:
  → Event 'change' trigger می‌شود
  → persist() خودکار صدا زده می‌شود
  → API.post('/insurances/set') با BasePlanId جدید
  → Backend: Reception.BasePlanId و PatientInsurance.InsurancePlanId update می‌شوند
  → Totals recalculate می‌شوند
  → UI: Totals به‌روزرسانی می‌شوند
```

### سناریو 2: تغییر بیمه تکمیلی

```
کاربر:
  1. Dropdown بیمه تکمیلی را باز می‌کند
  2. بیمه جدید را انتخاب می‌کند

سیستم:
  → Event 'change' trigger می‌شود
  → toggleRemoveButton() اجرا می‌شود (دکمه حذف نمایش داده می‌شود)
  → persist() خودکار صدا زده می‌شود
  → API.post('/insurances/set') با SupplementaryPlanId جدید
  → Backend: Reception.SupplementaryPlanId و PatientInsurance.SupplementaryInsurancePlanId update می‌شوند
  → Totals recalculate می‌شوند
  → UI: Totals به‌روزرسانی می‌شوند
```

### سناریو 3: حذف بیمه تکمیلی (روش 1: دکمه حذف)

```
کاربر:
  1. روی دکمه ❌ (حذف) کلیک می‌کند

سیستم:
  → removeSupplementary() اجرا می‌شود
  → $suppPlan.val('') → dropdown خالی می‌شود
  → trigger('change') → Event 'change' trigger می‌شود
  → persist() خودکار صدا زده می‌شود
  → API.post('/insurances/set') با supplementaryPlanId = null
  → Backend: Reception.SupplementaryPlanId = null و PatientInsurance.SupplementaryInsurancePlanId = null
  → Totals recalculate می‌شوند
  → UI: Totals به‌روزرسانی می‌شوند
  → toggleRemoveButton() → دکمه حذف مخفی می‌شود
```

### سناریو 4: حذف بیمه تکمیلی (روش 2: انتخاب "انتخاب کنید")

```
کاربر:
  1. Dropdown بیمه تکمیلی را باز می‌کند
  2. "انتخاب کنید" را انتخاب می‌کند

سیستم:
  → Event 'change' trigger می‌شود
  → toggleRemoveButton() اجرا می‌شود (دکمه حذف مخفی می‌شود)
  → selectedValue === '' → persist() خودکار صدا زده می‌شود
  → API.post('/insurances/set') با supplementaryPlanId = null
  → Backend: Reception.SupplementaryPlanId = null و PatientInsurance.SupplementaryInsurancePlanId = null
  → Totals recalculate می‌شوند
  → UI: Totals به‌روزرسانی می‌شوند
```

---

## 🔄 جریان کامل داده

```
[User Action]
    ↓
[Frontend Event]
    ↓
[persist() called]
    ↓
[API.post('/insurances/set', payload)]
    ↓
[Backend: ReceptionFacade.SetInsurancesAsync]
    ↓
1. Validate BasePlanId & SuppPlanId
2. Update Reception.BasePlanId & SuppPlanId
3. Find PatientInsurance (Primary, Active)
4. Update PatientInsurance:
   - InsurancePlanId & InsuranceProviderId (if BasePlanId changed)
   - SupplementaryInsurancePlanId & SuppProviderId (if SuppPlanId changed)
   - Or clear supplementary fields (if SuppPlanId = null)
5. Save Changes
6. Recalculate Totals
    ↓
[Response: ServiceResult<ItemsAndTotalsDto>]
    ↓
[Frontend: Update UI Totals]
    ↓
[User sees updated totals]
```

---

## ✅ چک‌لیست عملکرد

### ✅ تغییر بیمه پایه
- [x] Dropdown تغییر می‌کند
- [x] Event 'change' trigger می‌شود
- [x] persist() خودکار صدا زده می‌شود
- [x] Backend: Reception.BasePlanId update می‌شود
- [x] Backend: PatientInsurance.InsurancePlanId update می‌شود
- [x] Totals recalculate می‌شوند
- [x] UI: Totals به‌روزرسانی می‌شوند

### ✅ تغییر بیمه تکمیلی
- [x] Dropdown تغییر می‌کند
- [x] Event 'change' trigger می‌شود
- [x] دکمه حذف نمایش داده می‌شود
- [x] persist() خودکار صدا زده می‌شود
- [x] Backend: Reception.SupplementaryPlanId update می‌شود
- [x] Backend: PatientInsurance.SupplementaryInsurancePlanId update می‌شود
- [x] Totals recalculate می‌شوند
- [x] UI: Totals به‌روزرسانی می‌شوند

### ✅ حذف بیمه تکمیلی (دکمه)
- [x] دکمه حذف نمایش داده می‌شود (وقتی بیمه تکمیلی انتخاب شده)
- [x] کلیک روی دکمه → removeSupplementary()
- [x] Dropdown خالی می‌شود
- [x] Event 'change' trigger می‌شود
- [x] persist() خودکار صدا زده می‌شود
- [x] Backend: Reception.SupplementaryPlanId = null
- [x] Backend: PatientInsurance.SupplementaryInsurancePlanId = null
- [x] Totals recalculate می‌شوند
- [x] UI: Totals به‌روزرسانی می‌شوند
- [x] دکمه حذف مخفی می‌شود

### ✅ حذف بیمه تکمیلی (Dropdown)
- [x] انتخاب "انتخاب کنید" در dropdown
- [x] Event 'change' trigger می‌شود
- [x] دکمه حذف مخفی می‌شود
- [x] persist() خودکار صدا زده می‌شود
- [x] Backend: Reception.SupplementaryPlanId = null
- [x] Backend: PatientInsurance.SupplementaryInsurancePlanId = null
- [x] Totals recalculate می‌شوند
- [x] UI: Totals به‌روزرسانی می‌شوند

---

## 🎨 بهبودهای UX

1. ✅ **دکمه حذف بصری**: دکمه ❌ برای حذف سریع‌تر
2. ✅ **نمایش هوشمند**: دکمه فقط زمانی نمایش داده می‌شود که بیمه تکمیلی انتخاب شده باشد
3. ✅ **حذف خودکار**: انتخاب "انتخاب کنید" در dropdown به معنی حذف بیمه تکمیلی است
4. ✅ **به‌روزرسانی خودکار Totals**: پس از هر تغییر، totals خودکار recalculate می‌شوند

---

## 📝 نتیجه

**همه سناریوهای ویرایش، تغییر و حذف بیمه‌های پایه و تکمیلی به طور کامل پیاده‌سازی شده است:**

1. ✅ تغییر بیمه پایه → خودکار persist می‌شود
2. ✅ تغییر بیمه تکمیلی → خودکار persist می‌شود + دکمه حذف نمایش داده می‌شود
3. ✅ حذف بیمه تکمیلی (دکمه) → خودکار persist می‌شود + دکمه مخفی می‌شود
4. ✅ حذف بیمه تکمیلی (Dropdown) → خودکار persist می‌شود + دکمه مخفی می‌شود

**Backend نیز به‌روزرسانی شده است:**
- ✅ Reception.BasePlanId & Reception.SupplementaryPlanId
- ✅ PatientInsurance.InsurancePlanId & InsuranceProviderId
- ✅ PatientInsurance.SupplementaryInsurancePlanId & SupplementaryInsuranceProviderId

---

**تاریخ آخرین به‌روزرسانی**: 2024-12-20  
**وضعیت**: ✅ Complete & Production Ready

