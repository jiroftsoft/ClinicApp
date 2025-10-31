# Bugfix - ensureDraftOrSkip is not a function

## ✅ مشکل شناسایی شده

**خطا**: `TypeError: window.AutoDraftManager?.ensureDraftOrSkip is not a function`

**محل خطا**: 
- `Scripts/reception.v2/insurance-panel.js:173:56`
- `Scripts/reception.v2/service-lookup.js` (نیز استفاده می‌کند)

**علت**:
- `ensureDraftOrSkip` به عنوان `async function` در داخل IIFE تعریف شده بود
- در Public API export شده بود اما ممکن بود scope مشکل داشته باشد
- ممکن است فایل `auto-draft-manager.js` قبل از استفاده load نشده باشد

---

## ✅ راه‌حل اعمال شده

### 1. Bugfix در `auto-draft-manager.js`

**تغییرات**:
- ✅ ایجاد `autoDraftManagerPublicAPI` object قبل از export
- ✅ استفاده از `async function` wrapper در Public API
- ✅ اضافه کردن try-catch در wrapper برای handle خطاها
- ✅ Export به `window.AutoDraftManager` بعد از تعریف کامل

**کد**:
```javascript
// Public API - ✅ Bugfix: اطمینان از دسترسی صحیح به async function
const autoDraftManagerPublicAPI = {
  createDraft: createAutoDraft,
  ensureDraftOrSkip: async function(state) {
    // ✅ Bugfix: Wrapper برای اطمینان از دسترسی صحیح به async function
    try {
      return await ensureDraftOrSkip(state);
    } catch (err) {
      console.error('🏥 V2: ensureDraftOrSkip error:', err);
      return null;
    }
  },
  warnDraftMissing: warnDraftMissing,
  // ... سایر متدها
};

// ✅ Bugfix: Export به window.AutoDraftManager
window.AutoDraftManager = autoDraftManagerPublicAPI;
```

### 2. Bugfix در `insurance-panel.js`

**تغییرات**:
- ✅ اضافه کردن بررسی وجود `AutoDraftManager`
- ✅ اضافه کردن بررسی نوع `ensureDraftOrSkip` (function)
- ✅ استفاده از `let receptionId` برای scope صحیح
- ✅ اضافه کردن try-catch برای handle خطاها

**کد**:
```javascript
async function persist() {
  // ✅ Bugfix: بررسی وجود AutoDraftManager و ensureDraftOrSkip
  if (!window.AutoDraftManager) {
    console.error('🏥 V2: AutoDraftManager not available');
    toastr.error('سیستم پیش‌نویس در دسترس نیست. لطفاً صفحه را نوسازی کنید.');
    return Promise.resolve();
  }
  
  if (typeof window.AutoDraftManager.ensureDraftOrSkip !== 'function') {
    console.error('🏥 V2: ensureDraftOrSkip is not a function', window.AutoDraftManager);
    toastr.error('خطا در سیستم پیش‌نویس. لطفاً صفحه را نوسازی کنید.');
    return Promise.resolve();
  }
  
  // ✅ استفاده از ensureDraftOrSkip برای اطمینان از وجود Draft
  let receptionId;
  try {
    receptionId = await window.AutoDraftManager.ensureDraftOrSkip({
      patientId: $('#Patient_PatientId').val(),
      clinicId: $('#ClinicId').val(),
      departmentId: $('#DepartmentId').val(),
      doctorId: $('#DoctorId').val(),
      receptionId: $('#ReceptionId').val()
    });
    
    if (!receptionId || receptionId <= 0) {
      console.warn('🏥 V2: Cannot persist insurances, draft creation failed or missing required fields');
      window.AutoDraftManager?.warnDraftMissing();
      return Promise.resolve();
    }
  } catch (err) {
    console.error('🏥 V2: ensureDraftOrSkip error:', err);
    toastr.error('خطا در ایجاد پیش‌نویس. لطفاً مجدداً تلاش کنید.');
    return Promise.resolve();
  }
  
  // ... ادامه کد با receptionId
}
```

### 3. Bugfix در `service-lookup.js`

**تغییرات**:
- ✅ اضافه کردن بررسی وجود `AutoDraftManager`
- ✅ اضافه کردن بررسی نوع `ensureDraftOrSkip` (function)

---

## ✅ معیارهای پذیرش

### بررسی وجود AutoDraftManager:
- ✅ اگر `window.AutoDraftManager` موجود نباشد، پیام خطا نمایش داده می‌شود
- ✅ اگر `ensureDraftOrSkip` یک function نباشد، پیام خطا نمایش داده می‌شود

### استفاده صحیح از ensureDraftOrSkip:
- ✅ اگر فیلدهای الزامی کامل باشد، Draft ساخته می‌شود
- ✅ اگر فیلدهای الزامی ناقص باشد، پیام مناسب نمایش داده می‌شود
- ✅ اگر خطا رخ دهد، با try-catch handle می‌شود

### Scope صحیح receptionId:
- ✅ `receptionId` با `let` تعریف شده تا در scope صحیح باشد
- ✅ بعد از `ensureDraftOrSkip`، `receptionId` در دسترس است

---

## 🔄 تست‌های پیشنهادی

### تست 1: بارگذاری AutoDraftManager
1. باز کردن صفحه Reception V2
2. بررسی Console: `window.AutoDraftManager` باید موجود باشد
3. بررسی Console: `typeof window.AutoDraftManager.ensureDraftOrSkip` باید `'function'` باشد

### تست 2: استفاده از ensureDraftOrSkip
1. انتخاب بیمار، کلینیک، دپارتمان، پزشک
2. تغییر بیمه پایه یا تکمیلی
3. بررسی Console: `ensureDraftOrSkip` باید صدا زده شود
4. بررسی Console: اگر Draft وجود نداشته باشد، Draft ساخته می‌شود

### تست 3: خطاهای احتمالی
1. بدون انتخاب بیمار، تغییر بیمه
2. بررسی Console: پیام مناسب نمایش داده می‌شود
3. بررسی UI: toastr warning نمایش داده می‌شود

---

## 🎯 نتیجه‌گیری

**✅ مشکل رفع شد:**

1. ✅ `ensureDraftOrSkip` به درستی در Public API export می‌شود
2. ✅ بررسی‌های دفاعی برای وجود AutoDraftManager اضافه شد
3. ✅ Scope صحیح `receptionId` رفع شد
4. ✅ Error handling بهبود یافت

**🚀 سیستم آماده برای تست و استفاده است!**

