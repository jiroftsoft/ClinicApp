# 🔍 رفتار خالی کردن بیمه تکمیلی در فرم پذیرش

**تاریخ ایجاد**: 2024-12-20  
**هدف**: مستندسازی رفتار سیستم هنگام خالی کردن فیلد بیمه تکمیلی

---

## 📋 مفهوم خالی کردن بیمه تکمیلی

**وقتی کاربر فیلد بیمه تکمیلی را خالی می‌کند (انتخاب "انتخاب کنید")**:
- ✅ به این معنی است که **بیمار بیمه تکمیلی ندارد**
- ✅ سیستم باید این حالت را در دیتابیس ذخیره کند:
  - `Reception.SupplementaryPlanId = null`
  - `PatientInsurance.SupplementaryInsurancePlanId = null`
  - `PatientInsurance.SupplementaryInsuranceProviderId = null`

---

## 🔄 جریان کار

### Frontend

#### 1. کاربر dropdown بیمه تکمیلی را خالی می‌کند

```javascript
// User action: انتخاب "انتخاب کنید" در dropdown
$suppPlan.val() → '' (empty string)
```

#### 2. Event Handler اجرا می‌شود

```javascript
$suppPlan.on('change', function() {
  const selectedValue = $suppPlan.val();
  
  // اگر dropdown خالی شد (انتخاب "انتخاب کنید")
  if (!selectedValue || selectedValue === '' || selectedValue === null) {
    console.log('🏥 V2: Supplementary plan cleared → Patient has NO supplementary insurance');
    
    // persist() را صدا بزن که مقدار null را به سرور بفرستد
    persist().then(function() {
      toastr.info('بیمه تکمیلی حذف شد. بیمار بیمه تکمیلی ندارد.');
    });
  }
});
```

#### 3. تابع `persist()` اجرا می‌شود

```javascript
function persist() {
  const suppPlanValue = $suppPlan.val(); // '' (empty string)
  
  // تبدیل به integer یا null (اگر خالی باشد)
  const supplementaryPlanId = (suppPlanValue && suppPlanValue !== '' && suppPlanValue !== null) 
    ? parseInt(suppPlanValue) 
    : null; // ← null خواهد بود
  
  const payload = {
    receptionId: parseInt(receptionId),
    basePlanId: basePlanId,
    supplementaryPlanId: null // ← null به سرور می‌فرستد
  };
  
  API.post('/insurances/set', payload);
}
```

---

### Backend

#### 1. Controller دریافت می‌کند

```csharp
// Controllers/Api/ReceptionApiV1Controller.cs
public async Task<ActionResult> SetInsurances(SetInsurancesRequestDto request)
{
    // request.SupplementaryPlanId = null (از frontend)
    
    var facadeRequest = new ViewModels.Reception.SetInsurancesRequest
    {
        ReceptionId = request.ReceptionId,
        BasePlanId = request.BasePlanId,
        SupplementaryPlanId = request.SupplementaryPlanId // null
    };
    
    var result = await _facade.SetInsurancesAsync(facadeRequest);
    return Json(result);
}
```

#### 2. Facade پردازش می‌کند

```csharp
// Services/Reception/ReceptionFacade.cs
public async Task<ServiceResult<ItemsAndTotalsDto>> SetInsurancesAsync(SetInsurancesRequest request)
{
    // اعمال تغییرات روی Reception
    draft.BasePlanId = request.BasePlanId;
    draft.SupplementaryPlanId = request.SupplementaryPlanId; // null
    draft.UpdatedAt = DateTime.Now;
    
    await _context.SaveChangesAsync(); // Reception به‌روزرسانی شد
    
    // به‌روزرسانی PatientInsurances
    var patientInsurance = await _context.PatientInsurances
        .FirstOrDefaultAsync(pi => pi.PatientId == patientId && pi.IsPrimary && pi.IsActive && !pi.IsDeleted);
    
    if (patientInsurance != null)
    {
        // چون request.SupplementaryPlanId null است
        // به else branch می‌رود
        if (request.SupplementaryPlanId.HasValue && suppPlan != null)
        {
            // این branch اجرا نمی‌شود
        }
        else
        {
            // ✅ این branch اجرا می‌شود
            if (patientInsurance.SupplementaryInsurancePlanId.HasValue)
            {
                // فیلدهای بیمه تکمیلی را null می‌کنیم
                patientInsurance.SupplementaryInsurancePlanId = null;
                patientInsurance.SupplementaryInsuranceProviderId = null;
                hasChanges = true;
                
                _logger.Information("🔄 FACADE: حذف بیمه تکمیلی از PatientInsurances - PatientId: {PatientId}", patientId);
            }
        }
        
        if (hasChanges)
        {
            patientInsurance.UpdatedAt = DateTime.Now;
            patientInsurance.UpdatedByUserId = userId;
            await _context.SaveChangesAsync(); // PatientInsurance به‌روزرسانی شد
        }
    }
}
```

---

## ✅ نتیجه

### وقتی فیلد بیمه تکمیلی خالی می‌شود:

1. ✅ **Frontend**: `supplementaryPlanId: null` به سرور می‌فرستد
2. ✅ **Backend - Reception**: `Reception.SupplementaryPlanId = null` ذخیره می‌شود
3. ✅ **Backend - PatientInsurance**: 
   - `PatientInsurance.SupplementaryInsurancePlanId = null`
   - `PatientInsurance.SupplementaryInsuranceProviderId = null`
4. ✅ **Totals**: بدون بیمه تکمیلی recalculate می‌شوند
5. ✅ **UI**: Totals به‌روزرسانی می‌شوند (سهم بیمه تکمیلی = 0)

---

## 🎯 سناریوهای کاربری

### سناریو 1: حذف بیمه تکمیلی از طریق Dropdown

```
کاربر:
  1. Dropdown بیمه تکمیلی را باز می‌کند
  2. "انتخاب کنید" را انتخاب می‌کند (فیلد خالی می‌شود)

سیستم:
  → Event 'change' trigger می‌شود
  → selectedValue === '' → persist() صدا زده می‌شود
  → payload: { supplementaryPlanId: null }
  → API.post('/insurances/set', payload)
  → Backend: Reception.SupplementaryPlanId = null
  → Backend: PatientInsurance.SupplementaryInsurancePlanId = null
  → Backend: PatientInsurance.SupplementaryInsuranceProviderId = null
  → Totals recalculate (بدون بیمه تکمیلی)
  → UI: Totals update → SuppPayable = 0
  → toastr.info('بیمه تکمیلی حذف شد. بیمار بیمه تکمیلی ندارد.')
```

### سناریو 2: حذف بیمه تکمیلی از طریق دکمه ❌

```
کاربر:
  1. روی دکمه ❌ کلیک می‌کند

سیستم:
  → removeSupplementary() اجرا می‌شود
  → $suppPlan.val('').trigger('change')
  → Event 'change' trigger می‌شود
  → (همانند سناریو 1)
```

---

## 📊 تأثیر بر Totals

### قبل از حذف بیمه تکمیلی:

```
Gross = 100,000 IRR
BasePlan Coverage = 70%
SuppPlan Coverage = 50%

basePay = 100,000 * 0.70 = 70,000
patientAfterBase = 100,000 - 70,000 = 30,000
suppPay = 30,000 * 0.50 = 15,000
patient = 30,000 - 15,000 = 15,000
```

### بعد از حذف بیمه تکمیلی:

```
Gross = 100,000 IRR
BasePlan Coverage = 70%
SuppPlan Coverage = 0% (null)

basePay = 100,000 * 0.70 = 70,000
patientAfterBase = 100,000 - 70,000 = 30,000
suppPay = 0 (چون بیمه تکمیلی نداریم)
patient = 30,000 (بیمار کل باقی‌مانده را پرداخت می‌کند)
```

---

## ✅ تست و اعتبارسنجی

### چک‌لیست تست:

1. ✅ **Frontend**: 
   - [x] Dropdown را خالی می‌کنیم → `supplementaryPlanId: null` به سرور می‌فرستد
   - [x] پیام واضح نمایش داده می‌شود: "بیمه تکمیلی حذف شد. بیمار بیمه تکمیلی ندارد."
   - [x] دکمه ❌ مخفی می‌شود

2. ✅ **Backend - Reception**:
   - [x] `Reception.SupplementaryPlanId = null` ذخیره می‌شود
   - [x] `Reception.UpdatedAt` به‌روزرسانی می‌شود

3. ✅ **Backend - PatientInsurance**:
   - [x] `PatientInsurance.SupplementaryInsurancePlanId = null`
   - [x] `PatientInsurance.SupplementaryInsuranceProviderId = null`
   - [x] `PatientInsurance.UpdatedAt` به‌روزرسانی می‌شود

4. ✅ **Totals**:
   - [x] Totals بدون بیمه تکمیلی recalculate می‌شوند
   - [x] `SuppPayable = 0` می‌شود
   - [x] `PatientPayable` افزایش می‌یابد

---

## 📝 نتیجه‌گیری

**وقتی فیلد بیمه تکمیلی در فرم پذیرش خالی می‌شود (انتخاب "انتخاب کنید")**:
- ✅ سیستم به درستی تشخیص می‌دهد که **بیمار بیمه تکمیلی ندارد**
- ✅ این حالت در دیتابیس ذخیره می‌شود (`null` values)
- ✅ Totals بدون بیمه تکمیلی recalculate می‌شوند
- ✅ UI به‌روزرسانی می‌شود
- ✅ پیام واضح به کاربر نمایش داده می‌شود

**همه چیز به درستی کار می‌کند!** ✅

---

**تاریخ آخرین به‌روزرسانی**: 2024-12-20  
**وضعیت**: ✅ Complete & Verified

