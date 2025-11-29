# 🔍 گزارش جامع مدیریت Draft های ناقص در پذیرش

**تاریخ بررسی**: 2025-11-29  
**اهمیت**: 🔴 **حیاتی** - تأثیر مستقیم بر Data Quality و UX

---

## 📋 خلاصه مشکل

### سناریوی واقعی:
```
مثال از جدول:
شماره پذیرش: 1404-0908-00001
بیمار: اسماعیل پرتون
وضعیت: در انتظار  ← این Draft است!
مبلغ کل: ۲٬۳۷۶٬۰۰۰ ریال
```

### مشکل:
- منشی Draft ایجاد میکند
- بیمار منصرف می‌شود
-  منشی فرم را می‌بندد بدون "ذخیره و پذیرش"
- **Draft در DB باقی می‌ماند** 🔴
- در لیست به عنوان "در انتظار" نمایش داده می‌شود

### نیاز:
**حذف کامل (Physical Delete) بلافاصله**، نه Soft Delete!

---

## 1️⃣ تحلیل کد فعلی

### A. Backend: DeleteIncompleteDraftAsync ✅

```csharp
// در ReceptionFacade.cs - خط 1696

public async Task<ServiceResult> DeleteIncompleteDraftAsync(int receptionId)
{
    // 1. بررسی وجود Draft
    var draft = await _context.Receptions
        .Include(r => r.ReceptionItems)
        .FirstOrDefaultAsync(r => r.ReceptionId == receptionId && !r.IsDeleted);
    
    if (draft == null)
        return ServiceResult.Failed("پذیرش یافت نشد.", "NOT_FOUND");
    
    // 2. بررسی وضعیت: فقط Pending قابل حذف
    if (draft.Status != ReceptionStatus.Pending)
    {
        _logger.Warning("Draft نهایی است و قابل حذف نیست");
        return ServiceResult.Failed("این پذیرش نهایی شده است...");
    }
    
    // 3. بررسی دسترسی: فقط Creator می‌تواند حذف کند
    if (draft.CreatedByUserId != _currentUserService.UserId)
    {
        return ServiceResult.Failed("شما مجاز به حذف نیستید.");
    }
    
    // 4. ✅ Physical Delete (نه Soft Delete!)
    // حذف ReceptionItems
    if (draft.ReceptionItems != null && draft.ReceptionItems.Any())
    {
        foreach (var item in draft.ReceptionItems.ToList())
        {
            _context.ReceptionItems.Remove(item); // Physical Delete
        }
    }
    
    // حذف Reception
    _context.Receptions.Remove(draft); // ✅ Physical Delete
    await _context.SaveChangesAsync();
    
    _logger.Information("✅ Draft به صورت کامل حذف شد (Hard Delete)");
    return ServiceResult.Successful("پیش‌نویس با موفقیت حذف شد.");
}
```

**ارزیابی**: ✅ **عالی** - Physical Delete صحیح

---

### B. Backend: CleanupOldIncompleteDraftsAsync ⚠️

```csharp
// در ReceptionFacade.cs - خط 1779

public async Task<ServiceResult<int>> CleanupOldIncompleteDraftsAsync(int hoursOld = 24)
{
    var cutoffDate = DateTime.Now.AddHours(-hoursOld);
    
    // پیدا کردن Drafts قدیمی
    var incompleteDrafts = await _context.Receptions
        .Include(r => r.ReceptionItems)
        .Where(r => 
            r.Status == ReceptionStatus.Pending &&
            r.TotalAmount == 0 &&
            !r.IsDeleted &&
            r.CreatedAt < cutoffDate &&
            (r.ReceptionItems == null || !r.ReceptionItems.Any(ri => !ri.IsDeleted)))
        .ToListAsync();
    
    // ❌ SOFT DELETE (نه Physical!)
    foreach (var draft in incompleteDrafts)
    {
        draft.IsDeleted = true;          // ❌ Soft Delete
        draft.DeletedAt = DateTime.Now;
        draft.DeletedByUserId = "system";
    }
    
    await _context.SaveChangesAsync();
}
```

**مشکل**: ❌ **Soft Delete** - باید Physical Delete باشد!

---

### C. Frontend: form-change-detector.js ⚠️

```javascript
// Scripts/reception.v2/form-change-detector.js

const FormChangeDetector = {
    _isDirty: false,
    _draftId: null,
    
    init: function(draftId) {
        this._draftId = draftId;
        this._setupFormTracking();
    },
    
    _setupFormTracking: function() {
        // Track form changes
        $('form').on('change', () => {
            this._isDirty = true;
        });
    },
    
    // حذف Draft هنگام لغو
    deleteDraft: async function() {
        if (!this._draftId) return;
        
        try {
            const url = '/api/v1/reception/draft/delete-incomplete?receptionId=' + this._draftId;
            await $.post(url);
            console.log('✅ Draft deleted successfully');
        } catch (error) {
            console.error('❌ Failed to delete draft:', error);
        }
    }
};
```

**مشکل**: ⚠️ **window.onbeforeunload وجود ندارد!**

---

## 2️⃣ سناریوهای مختلف لغو

### سناریو 1: دکمه "لغو" یا "بستن" ✅

```javascript
// کاربر روی دکمه لغو کلیک می‌کند
$('#cancelBtn').click(function() {
    if (confirm('آیا مطمئن هستید؟')) {
        FormChangeDetector.deleteDraft(); // ✅ فراخوانی می‌شود
        window.location.href = '/ReceptionV2/Index';
    }
});
```

**وضعیت**: ✅ کار می‌کند

---

### سناریو 2: بستن Tab یا Browser ❌

```javascript
// ❌ وجود ندارد!
window.onbeforeunload = function(e) {
    if (FormChangeDetector._isDirty && FormChangeDetector._draftId) {
        // حذف Draft
        FormChangeDetector.deleteDraft();
    }
};
```

**وضعیت**: ❌ پیاده‌سازی نشده!

---

### سناریو 3: Navigation به صفحه دیگر ❌

```javascript
// کاربر روی لینک دیگری کلیک می‌کند
// ❌ Draft حذف نمی‌شود
```

**وضعیت**: ❌ مشکل دارد

---

### سناریو 4: Session Timeout / Logout ❌

```javascript
// Session منقضی می‌شود
// ❌ Draft باقی می‌ماند
```

**وضعیت**: ❌ مشکل دارد

---

### سناریو 5: Server Error / Network Issue ❌

```javascript
// خطای شبکه هنگام حذف
// ❌ Draft ممکن است باقی بماند
```

**وضعیت**: ❌ نیاز به Retry Logic

---

## 3️⃣ مشکلات شناسایی شده

### 🔴 مشکل 1: CleanupOldIncompleteDraftsAsync - Soft Delete

```csharp
// کد فعلی:
draft.IsDeleted = true; // ❌ Soft Delete

// باید باشد:
_context.Receptions.Remove(draft); // ✅ Physical Delete
```

---

### 🔴 مشکل 2: window.onbeforeunload وجود ندارد

```javascript
// ❌ وقتی کاربر Tab را می‌بندد، Draft حذف نمی‌شود!
```

---

### 🔴 مشکل 3: Navigation Guard ندارد

```javascript
// ❌ وقتی کاربر به صفحه دیگر می‌رود، Draft باقی می‌ماند
```

---

### 🟡 مشکل 4: Condition برای "ناقص" محدود است

```csharp
// کد فعلی فقط Drafts بدون خدمت را پاک می‌کند:
r.TotalAmount == 0 &&
(r.ReceptionItems == null || !r.ReceptionItems.Any())

// ولی Draft ممکن است خدمت داشته باشد ولی هنوز نهایی نشده!
```

---

## 4️⃣ راه‌حل‌های پیشنهادی

### راه‌حل 1: تصحیح CleanupOldIncompleteDraftsAsync ⭐⭐⭐

**تغییر از Soft Delete به Physical Delete**:

```csharp
public async Task<ServiceResult<int>> CleanupOldIncompleteDraftsAsync(int hoursOld = 24)
{
    try
    {
        var cutoffDate = DateTime.Now.AddHours(-hoursOld);
        
        _logger.Information("🏥 FACADE: شروع پاکسازی Draft های ناقص قدیمی - CutoffDate: {CutoffDate}", cutoffDate);

        // ✅ پیدا کردن Drafts قدیمی که هنوز Pending هستند
        var incompleteDrafts = await _context.Receptions
            .Include(r => r.ReceptionItems)
            .Where(r => 
                r.Status == ReceptionStatus.Pending &&  // ✅ وضعیت Pending
                !r.IsDeleted &&
                r.CreatedAt < cutoffDate               // ✅ قدیمی‌تر از cutoff
            )
            .ToListAsync();

        var count = 0;
        foreach (var draft in incompleteDrafts)
        {
            // ✅ حذف ReceptionItems
            if (draft.ReceptionItems != null && draft.ReceptionItems.Any())
            {
                foreach (var item in draft.ReceptionItems.ToList())
                {
                    _context.ReceptionItems.Remove(item); // ✅ Physical Delete
                }
            }
            
            // ✅ حذف Reception
            _context.Receptions.Remove(draft); // ✅ Physical Delete (نه Soft!)
            count++;
        }

        if (count > 0)
        {
            await _context.SaveChangesAsync();
            _logger.Information("✅ FACADE: {Count} Draft ناقص قدیمی به صورت کامل حذف شد (Physical Delete)", count);
        }
        else
        {
            _logger.Information("ℹ️ FACADE: هیچ Draft ناقص قدیمی یافت نشد");
        }

        return ServiceResult<int>.Successful(count, $"{count} Draft ناقص قدیمی حذف شد");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "❌ FACADE: خطا در پاکسازی Draft های ناقص قدیمی");
        return ServiceResult<int>.Failed("خطا در پاکسازی: " + ex.Message);
    }
}
```

**مزایا**:
- ✅ Physical Delete
- ✅ حذف ReceptionItems نیز
- ✅ شرط ساده‌تر: فقط Status = Pending

---

### راه‌حل 2: افزودن window.onbeforeunload ⭐⭐⭐

**پیاده‌سازی در form-change-detector.js**:

```javascript
// Scripts/reception.v2/form-change-detector.js

const FormChangeDetector = {
    _isDirty: false,
    _draftId: null,
    _isSubmitting: false, // Flag برای جلوگیری از حذف هنگام Submit
    
    init: function(draftId) {
        this._draftId = draftId;
        this._setupFormTracking();
        this._setupBeforeUnload(); // ✅ جدید
    },
    
    _setupFormTracking: function() {
        $('form').on('change', () => {
            this._isDirty = true;
        });
        
        // ✅ هنگام Submit، flag را set کن
        $('form').on('submit', () => {
            this._isSubmitting = true;
        });
    },
    
    // ✅ جدید: نصب window.onbeforeunload
    _setupBeforeUnload: function() {
        const self = this;
        
        window.addEventListener('beforeunload', function(e) {
            // اگر در حال Submit است، نباید حذف کنیم
            if (self._isSubmitting) {
                console.log('ℹ️ Form is being submitted, skip draft deletion');
                return;
            }
            
            // اگر Draft وجود دارد، حذف کن
            if (self._draftId) {
                console.log('🗑️ Deleting draft before unload:', self._draftId);
                
                // ✅ استفاده از Beacon API برای Asynchronous Delete
                // (بهتر از AJAX sync که browser را block می‌کند)
                const url = '/api/v1/reception/draft/delete-incomplete';
                const data = new Blob(
                    [JSON.stringify({ receptionId: self._draftId })],
                    { type: 'application/json' }
                );
                
                navigator.sendBeacon(url, data);
                
                // ⚠️ یا می‌توان از AJAX Sync استفاده کرد (قدیمی‌تر ولی مطمئن‌تر)
                // $.ajax({
                //     url: url,
                //     type: 'POST',
                //     async: false, // Synchronous
                //     data: { receptionId: self._draftId }
                // });
            }
        });
    },
    
    deleteDraft: async function() {
        if (!this._draftId) return;
        
        try {
            const url = '/api/v1/reception/draft/delete-incomplete';
            await $.post(url, { receptionId: this._draftId });
            console.log('✅ Draft deleted successfully');
            this._draftId = null; // Clear draft ID
        } catch (error) {
            console.error('❌ Failed to delete draft:', error);
        }
    },
    
    // ✅ تابع برای Disable کردن beforeunload (برای Submit)
    disableBeforeUnload: function() {
        this._isSubmitting = true;
    }
};

// ✅ استفاده:
// در فرم پذیرش:
$(document).ready(function() {
    const draftId = @Model.ReceptionId; // از Server
    FormChangeDetector.init(draftId);
    
    // هنگام Submit موفق، disable کن
    $('#submitBtn').click(function() {
        FormChangeDetector.disableBeforeUnload();
    });
});
```

**مزایا**:
- ✅ حذف Draft هنگام بستن Tab/Browser
- ✅ استفاده از Beacon API (بهتر از AJAX sync)
- ✅ جلوگیری از حذف هنگام Submit

**نکته مهم**: 
```javascript
// Beacon API بهتر از AJAX Synchronous است
// چون browser را block نمی‌کند
navigator.sendBeacon(url, data);
```

---

### راه‌حل 3: Navigation Guard با SPA Router ⭐

**اگر از SPA استفاده می‌کنید** (مثلاً Vue Router):

```javascript
// در Vue Router:
router.beforeEach((to, from, next) => {
    if (FormChangeDetector._draftId && FormChangeDetector._isDirty) {
        if (confirm('پیش‌نویس ذخیره نشده است. آیا می‌خواهید حذف شود?')) {
            await FormChangeDetector.deleteDraft();
            next();
        } else {
            next(false);
        }
    } else {
        next();
    }
});
```

---

## 5️⃣ Backend API تکمیل‌شده

### API Endpoint برای حذف با SendBeacon

```csharp
// در ReceptionApiV1Controller.cs

[HttpPost]
[Route("api/v1/reception/draft/delete-incomplete")]
public async Task<ActionResult> DeleteIncompleteDraft(int? receptionId)
{
    try
    {
        if (!receptionId.HasValue || receptionId.Value <= 0)
        {
            return Json(new { success = false, message = "شناسه پذیرش نامعتبر است" });
        }

        var result = await _receptionFacade.DeleteIncompleteDraftAsync(receptionId.Value);

        return Json(new 
        { 
            success = result.Success,
            message = result.Message
        });
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در حذف Draft");
        return Json(new { success = false, message = "خطای سیستمی" });
    }
}
```

---

## 6️⃣ Scheduled Job برای Cleanup

### استفاده از Hangfire یا Windows Task Scheduler

```csharp
// در Startup.cs یا Global.asax

// با Hangfire:
RecurringJob.AddOrUpdate(
    "cleanup-incomplete-drafts",
    () => _receptionFacade.CleanupOldIncompleteDraftsAsync(24),
    Cron.Hourly // هر ساعت
);

// یا با Windows Task Scheduler:
// Schedule: هر 1 ساعت
// Command: curl -X POST https://yoursite.com/api/admin/cleanup-drafts
```

---

## 7️⃣ Testing Scenarios

### Test 1: دکمه لغو
```
1. ایجاد Draft جدید
2. افزودن خدمت
3. کلیک روی "لغو"
4. بررسی: Draft باید از DB حذف شود (Physical)
```

### Test 2: بستن Browser
```
1. ایجاد Draft جدید
2. افزودن خدمت
3. بستن Tab
4. بررسی: Draft باید از DB حذف شود
```

### Test 3: Cleanup Job
```
1. ایجاد Draft قدیمی (بیش از 24 ساعت)
2. اجرای CleanupOldIncompleteDraftsAsync()
3. بررسی: Draft باید به صورت Physical حذف شود
```

---

## 8️⃣ خلاصه تغییرات پیشنهادی

### Backend:

| فایل | متد | تغییر | اولویت |
|------|-----|-------|--------|
| ReceptionFacade.cs | CleanupOldIncompleteDraftsAsync | Soft Delete → Physical Delete | 🔴 بالا |
| ReceptionApiV1Controller.cs | DeleteIncompleteDraft | تکمیل API | 🟡 متوسط |

### Frontend:

| فایل | تغییر | اولویت |
|------|-------|--------|
| form-change-detector.js | افزودن window.onbeforeunload | 🔴 بالا |
| reception-main.js | Integration با FormChangeDetector | 🟡 متوسط |

---

## 🎯 نتیجه‌گیری

### ✅ نقاط قوت فعلی:
- DeleteIncompleteDraftAsync **به درستی Physical Delete** انجام می‌دهد
- Frontend integration موجود است (4 فایل JS)

### ❌ نقاط ضعف:
- CleanupOldIncompleteDraftsAsync **Soft Delete** انجام می‌دهد (باید Physical باشد)
- **window.onbeforeunload وجود ندارد** (حذف هنگام بستن browser)
- Navigation Guard ندارد

### 🎯 اقدامات فوری (این هفته):

1. **تصحیح CleanupOldIncompleteDraftsAsync** ⭐⭐⭐
   - تغییر به Physical Delete
   - زمان: 1 ساعت

2. **افزودن window.onbeforeunload** ⭐⭐⭐
   - استفاده از Beacon API
   - زمان: 2-3 ساعت

3. **Testing** ⭐⭐
   - تست سناریوهای مختلف
   - زمان: 2 ساعت

---

**تاریخ**: 2025-11-29  
**تحلیلگر**: Senior .NET Architect  
**وضعیت**: ✅ تحلیل کامل - آماده پیاده‌سازی
