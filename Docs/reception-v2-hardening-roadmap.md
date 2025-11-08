# 🛡️ نقشه راه بهینه‌سازی و مقاوم‌سازی فرم پذیرش V2

**تاریخ ایجاد**: 1404/08/16  
**هدف**: مقاوم‌سازی فرم پذیرش برای استفاده مکرر توسط منشی‌های کلینیک  
**اولویت**: بالا (استفاده مکرر در محیط درمانی)

---

## 📋 خلاصه مشکلات شناسایی شده

### 1. ⚠️ مشکل Patient Lookup - بارگذاری تاخیری اطلاعات
**علائم**: 
- فقط کد ملی پر می‌شود
- اطلاعات بیمار فقط با `blur` از کد ملی لود می‌شود
- UX ضعیف برای منشی‌ها

**علت احتمالی**:
- Event handler برای `blur` یا `change` روی کد ملی
- عدم استفاده از debounce برای جلوگیری از درخواست‌های مکرر
- عدم نمایش loading state

**اولویت**: 🔴 بالا

---

### 2. ⚠️ مشکل Optimistic Concurrency Exception
**خطا**: 
```
Store update, insert, or delete statement affected an unexpected number of rows (0). 
Entities may have been modified or deleted since entities were loaded.
```

**محل**: `ReceptionFacade.SetInsurancesAsync` - خط 1965-2010

**علت**:
- `PatientInsurance` از context گرفته می‌شود
- بعد از `SaveChangesAsync()` در خط 1958، دوباره سعی می‌کند آن را update کند
- اگر entity در جای دیگری تغییر کرده باشد، `RowVersion` mismatch می‌شود

**اولویت**: 🔴 بالا

---

### 3. ⚠️ مشکل Race Condition در Reprice Token
**علائم**:
- چندین درخواست Reprice همزمان ارسال می‌شود
- `Reprice response ignored (outdated token)`
- درخواست‌های تکراری و غیرضروری

**محل**: `insurance-panel.js` - خط 230-240

**علت**:
- چندین event handler برای تغییر بیمه‌ها
- عدم debounce برای درخواست‌های Reprice
- Token race condition در `setInsurancesAndReprice`

**اولویت**: 🟡 متوسط

---

### 4. ⚠️ مشکل Draft Creation - ReceptionId already exists
**علائم**:
- `ReceptionId already exists, skipping draft creation`
- Draft creation چندین بار تلاش می‌شود

**اولویت**: 🟡 متوسط

---

## 🗺️ نقشه راه بهینه‌سازی

### فاز 1: رفع مشکلات بحرانی (اولویت بالا) ⏱️ 2-3 ساعت

#### 1.1 رفع Optimistic Concurrency Exception
**اقدامات**:
- [ ] بررسی `ReceptionFacade.SetInsurancesAsync` - خط 1965-2010
- [ ] استفاده از `AsNoTracking()` برای query اولیه `PatientInsurance`
- [ ] Reload entity قبل از update: `await _context.Entry(patientInsurance).ReloadAsync()`
- [ ] Handle `DbUpdateConcurrencyException` با retry logic
- [ ] استفاده از `RowVersion` برای optimistic concurrency

**فایل‌های مورد نیاز**:
- `Services/Reception/ReceptionFacade.cs` - خط 1911-2010

**تست**:
- تست با تغییر همزمان `PatientInsurance` از دو session
- تست با تغییر `Reception` و `PatientInsurance` همزمان

---

#### 1.2 بهبود Patient Lookup UX
**اقدامات**:
- [ ] بررسی event handlers روی کد ملی (`patient-lookup.js`)
- [ ] اضافه کردن debounce (300-500ms) برای lookup
- [ ] نمایش loading state هنگام lookup
- [ ] بهبود error handling و نمایش پیام‌های واضح
- [ ] اضافه کردن keyboard shortcut (Enter) برای lookup
- [ ] Cache کردن نتایج lookup برای کد ملی‌های اخیر

**فایل‌های مورد نیاز**:
- `Scripts/reception.v2/patient-lookup.js`
- `Views/ReceptionV2/Index.cshtml` (برای loading indicator)

**تست**:
- تست با تایپ سریع کد ملی
- تست با کد ملی نامعتبر
- تست با کد ملی که بیمار ندارد

---

### فاز 2: بهینه‌سازی Race Conditions (اولویت متوسط) ⏱️ 1-2 ساعت

#### 2.1 رفع Race Condition در Reprice
**اقدامات**:
- [ ] بررسی `insurance-panel.js` - خط 105-470
- [ ] اضافه کردن debounce (500ms) برای تغییر بیمه‌ها
- [ ] جلوگیری از درخواست‌های تکراری با flag `isRepricing`
- [ ] بهبود Token management در `reception-api.js`
- [ ] Cancel کردن درخواست‌های قبلی قبل از ارسال جدید

**فایل‌های مورد نیاز**:
- `Scripts/reception.v2/insurance-panel.js`
- `Scripts/reception.v2/reception-api.js`

**تست**:
- تست با تغییر سریع بیمه‌ها
- تست با چندین تغییر همزمان

---

#### 2.2 بهبود Draft Creation
**اقدامات**:
- [ ] بررسی `auto-draft-manager.js`
- [ ] اضافه کردن flag برای جلوگیری از ایجاد draft تکراری
- [ ] بررسی وجود draft قبل از ایجاد
- [ ] بهبود error handling

**فایل‌های مورد نیاز**:
- `Scripts/reception.v2/auto-draft-manager.js`

---

### فاز 3: مقاوم‌سازی کلی (اولویت پایین) ⏱️ 2-3 ساعت

#### 3.1 بهبود Error Handling
**اقدامات**:
- [ ] اضافه کردن global error handler
- [ ] بهبود پیام‌های خطا برای کاربر نهایی
- [ ] Logging بهتر برای debugging
- [ ] Retry logic برای خطاهای موقت

#### 3.2 بهبود Performance
**اقدامات**:
- [ ] Lazy loading برای داده‌های غیرضروری
- [ ] Cache کردن داده‌های static (بیمه‌ها، دپارتمان‌ها)
- [ ] بهینه‌سازی queries در backend

#### 3.3 بهبود UX
**اقدامات**:
- [ ] اضافه کردن keyboard shortcuts
- [ ] بهبود loading states
- [ ] اضافه کردن progress indicators
- [ ] بهبود responsive design

---

## 🔍 بررسی دقیق‌تر مشکلات

### مشکل 1: Patient Lookup - بررسی Event Handlers

**سوالات برای بررسی**:
1. آیا event handler برای `blur` روی کد ملی وجود دارد؟
2. آیا event handler برای `change` روی کد ملی وجود دارد؟
3. آیا debounce برای lookup استفاده می‌شود؟
4. آیا loading state نمایش داده می‌شود؟

**اقدامات پیشنهادی**:
```javascript
// اضافه کردن debounce
let lookupTimeout;
$('#Patient_NationalCode').on('input', function() {
    clearTimeout(lookupTimeout);
    lookupTimeout = setTimeout(function() {
        if ($('#Patient_NationalCode').val().length === 10) {
            lookup();
        }
    }, 500);
});

// اضافه کردن Enter key
$('#Patient_NationalCode').on('keypress', function(e) {
    if (e.which === 13) { // Enter
        e.preventDefault();
        lookup();
    }
});
```

---

### مشکل 2: Optimistic Concurrency - بررسی کد

**کد فعلی** (خط 1965-2010):
```csharp
var patientInsurance = await _context.PatientInsurances
    .FirstOrDefaultAsync(pi => pi.PatientId == patientId && pi.IsPrimary && pi.IsActive && !pi.IsDeleted);

if (patientInsurance != null)
{
    // ... تغییرات ...
    await _context.SaveChangesAsync(); // ⚠️ ممکن است RowVersion تغییر کرده باشد
}
```

**راه‌حل پیشنهادی**:
```csharp
// 1. استفاده از AsNoTracking برای query اولیه
var patientInsurance = await _context.PatientInsurances
    .AsNoTracking()
    .FirstOrDefaultAsync(pi => pi.PatientId == patientId && pi.IsPrimary && pi.IsActive && !pi.IsDeleted);

if (patientInsurance != null)
{
    // 2. Attach و Reload برای دریافت RowVersion به‌روز
    _context.PatientInsurances.Attach(patientInsurance);
    await _context.Entry(patientInsurance).ReloadAsync();
    
    // 3. اعمال تغییرات
    patientInsurance.InsurancePlanId = request.BasePlanId.Value;
    // ...
    
    // 4. Handle concurrency exception
    try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException ex)
    {
        // Retry logic یا نمایش خطا
        _logger.Warning("⚠️ Concurrency conflict in SetInsurances");
        throw new InvalidOperationException("اطلاعات بیمه در جای دیگری تغییر کرده است. لطفاً صفحه را نوسازی کنید.");
    }
}
```

---

### مشکل 3: Race Condition در Reprice

**کد فعلی** (`insurance-panel.js`):
```javascript
// خط 105-109: Base plan changed
$('#BasePlanId').on('change', function() {
    // ... trigger reprice ...
});

// خط 127-131: Supplementary plan changed  
$('#SupplementaryPlanId').on('change', function() {
    // ... trigger reprice ...
});
```

**راه‌حل پیشنهادی**:
```javascript
let repriceTimeout;
let isRepricing = false;

function triggerReprice() {
    if (isRepricing) {
        console.warn('🏥 V2: Reprice already in progress, skipping...');
        return;
    }
    
    clearTimeout(repriceTimeout);
    repriceTimeout = setTimeout(async function() {
        isRepricing = true;
        try {
            await persist();
        } finally {
            isRepricing = false;
        }
    }, 500); // Debounce 500ms
}

$('#BasePlanId, #SupplementaryPlanId').on('change', function() {
    triggerReprice();
});
```

---

## 📊 معیارهای موفقیت

### معیارهای عملکردی:
- ✅ عدم خطای Optimistic Concurrency در استفاده عادی
- ✅ Patient Lookup در کمتر از 500ms
- ✅ عدم درخواست‌های تکراری Reprice
- ✅ UX روان و بدون lag

### معیارهای کیفیت:
- ✅ Error handling جامع
- ✅ Logging مناسب برای debugging
- ✅ پیام‌های خطای واضح برای کاربر
- ✅ Performance قابل قبول در استفاده مکرر

---

## 🎯 اولویت‌بندی نهایی

1. **فوری** (امروز):
   - رفع Optimistic Concurrency Exception
   - بهبود Patient Lookup UX

2. **مهم** (این هفته):
   - رفع Race Condition در Reprice
   - بهبود Draft Creation

3. **بهبود** (هفته آینده):
   - مقاوم‌سازی کلی
   - بهبود Performance
   - بهبود UX

---

## ✅ تصمیمات نهایی (طبق درخواست کاربر)

### اصول کلی:
1. **Realtime Only**: هیچ cache در محیط درمانی - همه چیز realtime
2. **Best Practices**: استفاده از بهترین روش‌های حرفه‌ای
3. **Reliability**: مقاوم و مطمئن برای استفاده مکرر

### تصمیمات:
1. **Patient Lookup**: 
   - ✅ Lookup خودکار با تایپ 10 رقم (debounce 500ms)
   - ✅ Enter key برای lookup فوری
   - ✅ Blur fallback برای سازگاری
   - ❌ هیچ cache - همیشه realtime

2. **Debounce Timing**: 
   - ✅ 500ms برای Patient Lookup
   - ✅ 500ms برای Reprice
   - ✅ 300ms برای Auto-save

3. **Error Handling**: 
   - ✅ Retry logic برای Optimistic Concurrency (3 بار با exponential backoff)
   - ✅ نمایش خطای واضح به کاربر
   - ✅ Logging کامل برای debugging

4. **Loading States**: 
   - ✅ Loading indicators برای همه عملیات async
   - ✅ Disable buttons هنگام processing
   - ✅ Progress feedback

---

**🚀 شروع پیاده‌سازی**: طبق تصمیمات بالا، شروع به پیاده‌سازی می‌کنیم.

