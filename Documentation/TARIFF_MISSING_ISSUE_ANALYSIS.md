# 🔍 گزارش بررسی مشکل عدم تعرفه در پذیرش

**تاریخ بررسی**: 2025-11-29  
**اهمیت**: 🔴 **حیاتی** - مستقیماً بر روی محاسبات مالی تأثیر می‌گذارد

---

## 📋 خلاصه مشکل

### مورد واقعی:
- **بیمار**: رضا باقری (کد ملی: 3020761190)
- **بیمه پایه**: خدمات درمانی - طرح پایه (70%)
- **بیمه تکمیلی**: بیمه تکمیلی پاسارگاد - پوشش کامل (100%)
- **خدمت انتخابی**: ویزیت پزشک عمومی (**ServiceCode: 970000**)

### مشکل شناسایی شده:
```
❌ در جدول InsuranceTariffs تعرفه برای این ترکیب وجود ندارد:
   - InsurancePlanId (بیمه پایه) + ServiceId (970000)
   - InsurancePlanId (بیمه تکمیلی) + ServiceId (970000)
```

### نتیجه:
```
✅ سیستم به جای خطا، از منطق Fallback استفاده می‌کند
✅ محاسبه از BasePrice + CoveragePercent انجام می‌شود
⚠️ اما این ممکن است دقیق نباشد!
```

---

## 1️⃣ تحلیل Data Flow

```
User Action: افزودن خدمت در فرم پذیرش
    ↓
ReceptionFacade.AddItemAsync()
    ↓
PricingEngine.QuoteAsync(serviceId, planIds)
    ↓
InsuranceCoverageProvider.GetPrimaryRuleAsync()
    ↓
InsuranceTariffRepository.GetByPlanAndServiceAsync(planId, serviceId)
    ↓
if (tariff found)
    استفاده از TariffPrice, PatientShare, InsurerShare
else
    ⚠️ FALLBACK: محاسبه از BasePrice + CoveragePercent
```

---

## 2️⃣ کد واقعی - PricingEngine.QuoteAsync

### مرحله 1: دریافت تعرفه مصوب

```csharp
// در PricingEngine.QuoteAsync():

// 1) محاسبه تعرفه مصوب (ApprovedTariff)
var approved = await _tariff.ResolveApprovedTariffAsync(
    r.ServiceId, 
    r.ClinicId, 
    r.DepartmentId, 
    fy, 
    ct
);

// این بر اساس BasePrice خدمت محاسبه می‌شود
// approved = Service.BasePrice × FactorSettings ×  RVU × ...
```

### مرحله 2: محاسبه سهم بیمه پایه

```csharp
// 3) PRIMARY: محاسبه سهم بیمه پایه
if (r.Primary?.InsurancePlanId.HasValue == true)
{
    // 🔍 این کلید است!
    primaryRule = await _coverage.GetPrimaryRuleAsync(
        r.Primary.InsurancePlanId.Value,
        r.ServiceId,
        r.DepartmentId,
        r.DoctorId,
        fy,
        ct
    );
    
    if (primaryRule.IsCovered && approved > 0)
    {
        // محاسبه سهم پایه
        var allowed = ApplyCap(approved, primaryRule.PerVisitCapIRR);
        primaryPays = Round(allowed * (primaryRule.CoveragePercent / 100m));
        patientAfterPrimary = approved - primaryPays;
    }
}
```

---

## 3️⃣ کد واقعی - InsuranceCoverageProvider.GetPrimaryRuleAsync

### منطق Lookup تعرفه و Fallback:

```csharp
public async Task<CoverageRule> GetPrimaryRuleAsync(
    int planId, 
    int serviceId, 
    ..., 
    CancellationToken ct)
{
    // 🔍 سعی کردن برای پیدا کردن تعرفه
    var tariff = await _tariffRepo.GetByPlanAndServiceAsync(
        planId, 
        serviceId, 
        includeInactive: false
    );
    
    if (tariff == null)
    {
        // ⚠️ FALLBACK: تعرفه نیست، از Plan استفاده کن
        var plan = await _planRepo.GetByIdAsync(planId);
        
        if (plan == null || !plan.IsActive)
            return CoverageRule.None(); // بیمه غیرفعال
        
        // ✅ محاسبه از Plan.CoveragePercent
        return new CoverageRule
        {
            IsCovered = true,
            CoveragePercent = plan.CoveragePercent, // مثلاً 70%
            PerVisitCapIRR = null, // سقف نیست
            // دیگر مقادیر از Plan
        };
    }
    
    // ✅ تعرفه موجود است
    return new CoverageRule
    {
        IsCovered = true,
        CoveragePercent = CalculateFromTariff(tariff),
        PerVisitCapIRR = tariff.SupplementaryMaxPayment,
        // محاسبه دقیق از tariff
    };
}
```

---

## 4️⃣ InsuranceTariffRepository.GetByPlanAndServiceAsync

### Query واقعی:

```csharp
public async Task<InsuranceTariff> GetByPlanAndServiceAsync(
    int planId, 
    int serviceId, 
    bool includeInactive = false)
{
    var query = _context.InsuranceTariffs
        .Include(t => t.Service)
        .Include(t => t.InsurancePlan)
        .Where(t => 
            t.InsurancePlanId == planId &&   // ✅
            t.ServiceId == serviceId &&       // ✅ این ServiceId است نه ServiceCode
            !t.IsDeleted                       // ✅
        );
    
    if (!includeInactive)
        query = query.Where(t => t.IsActive); // ✅
    
    return await query.FirstOrDefaultAsync();
}
```

### نکته مهم:
```
✅ Repository از ServiceId استفاده می‌کند (صحیح)
❌ مشکل این است که تعرفه‌ای با این ServiceId در DB نیست!
```

---

## 5️⃣ علت ریشه‌ای (Root Cause)

### مشکل اصلی:

**تعرفه برای ترکیب (InsurancePlanId + ServiceId) در دیتابیس وجود ندارد!**

### شواهد از DB:

```sql
-- تعرفه‌های موجود:
InsuranceTariffId | ServiceId | InsurancePlanId | InsuranceType
3208              | 1424      | 1012            | 1 (Primary)
3209              | 1424      | 1018            | 2 (Supplementary)

-- ServiceId مورد نیاز:
ServiceCode: 970000 → ServiceId: ??? (نامشخص، ولی حتماً 1424 نیست!)
```

### چرا این اتفاق افتاده؟

**سناریوهای محتمل**:

1. **تعرفه‌ها ناقص ساخته شده‌اند** ❌
   - Admin فقط برخی خدمات را تعریف کرده
   - "ویزیت پزشک عمومی" (970000) فراموش شده

2. **Service جدید اضافه شده** 🟡
   - خدمت جدید ایجاد شده ولی تعرفه‌ها تعریف نشده

3. **InsurancePlan جدید** 🟡
   - بیمه جدید اضافه شده ولی تعرفه‌ها bulk create نشده

4. **خطا در Seed Data** 🟡
   - سیستم Seed تمام ترکیبات را ایجاد نکرده

---

## 6️⃣ رفتار فعلی سیستم

### ✅ نقاط مثبت:

1. **سیستم Crash نمی‌کند** ✅
   - Fallback logic وجود دارد
   
2. **محاسبه انجام می‌شود** ✅
   - از Plan.CoveragePercent استفاده می‌کند
   
3. **کاربر می‌تواند ثبت کند** ✅
   - فرآیند مسدود نمی‌شود

### ⚠️ نقاط منفی:

1. **محاسبات ممکن است دقیق نباشند** ⚠️
   - هر خدمت ممکن است قیمت خاص داشته باشد
   - سقف‌ها و فرانشیزها اعمال نمی‌شوند
   
2. **هیچ هشداری به کاربر داده نمی‌شود** ⚠️
   - کاربر نمی‌داند تعرفه تعریف نشده
   
3. **ناسازگاری مالی** 🔴
   - قیمت‌های محاسبه شده با تعرفه واقعی فرق دارند

---

## 7️⃣ راه‌حل‌های پیشنهادی

### راه‌حل 1: **Validation & Warning** (توصیه می‌شود ⭐)

**مزایا**: 
- کاربر آگاه می‌شود
- می‌تواند تصمیم بگیرد
- داده‌های مالی شفاف می‌شوند

**پیاده‌سازی**:

```csharp
// در ReceptionFacade.AddItemAsync() - بعد از محاسبه و قبل از ذخیره:

var quoteResult = await _pricingEngine.QuoteAsync(quoteRequest);

// ✅ بررسی: آیا تعرفه موجود بود؟
bool hasTariff = await _tariffRepository.GetByPlanAndServiceAsync(
    draft.BasePlanId.Value, 
    serviceId
) != null;

if (!hasTariff)
{
    _logger.Warning(
        "⚠️ RECEPTION: تعرفه برای این ترکیب وجود ندارد - " +
        "PlanId: {PlanId}, ServiceId: {ServiceId}. " +
        "محاسبه از CoveragePercent استفاده شده.",
        draft.BasePlanId.Value, serviceId
    );
    
    // ✅ افزودن Warning به Notes یا Response
    var warningMessage = 
        "⚠️ توجه: تعرفه دقیق برای این خدمت تعریف نشده است. " +
        "محاسبه بر اساس درصد پوشش کلی انجام شده است.";
    
    // می‌توان به SnapshotJson اضافه کرد
    snapshot.Warning = warningMessage;
}
```

**در Frontend (JavaScript)**:

```javascript
// در reception-main.js:

if (itemData.hasWarning) {
    toastr.warning(
        'تعرفه دقیق برای این خدمت تعریف نشده است.<br>' +
        'محاسبه بر اساس درصد پوشش کلی انجام شده.',
        'هشدار',
        { timeOut: 8000 }
    );
}

// یا اضافه کردن نماد هشدار در جدول آیتم‌ها
```

---

### راه‌حل 2: **Pre-Validation قبل از انتخاب خدمت** (توصیه می‌شود ⭐⭐)

**مزایا**:
- جلوگیری از انتخاب خدمت بدون تعرفه
- تجربه کاربری بهتر
- دقت بیشتر

**پیاده‌سازی**:

```csharp
// API جدید برای بررسی تعرفه

[HttpPost]
[Route("api/v1/reception/service/validate-tariff")]
public async Task<JsonResult> ValidateServiceTariff(int serviceId)
{
    var receptionId = GetCurrentDraftId();
    var draft = await _context.Receptions.FindAsync(receptionId);
    
   if (draft == null)
        return Json(new { success = false });
    
    bool hasBaseTariff = true;
    bool hasSuppTariff = true;
    
    if (draft.BasePlanId.HasValue)
    {
        hasBaseTariff = await _tariffRepository.GetByPlanAndServiceAsync(
            draft.BasePlanId.Value, 
            serviceId
        ) != null;
    }
    
    if (draft.SupplementaryPlanId.HasValue)
    {
        hasSuppTariff = await _tariffRepository.GetByPlanAndServiceAsync(
            draft.SupplementaryPlanId.Value, 
            serviceId,
            insuranceType: InsuranceType.Supplementary
        ) != null;
    }
    
    return Json(new 
    { 
        success = true,
        hasBaseTariff,
        hasSuppTariff,
        warning = !hasBaseTariff || !hasSuppTariff 
            ? "تعرفه دقیق برای این خدمت تعریف نشده است."
            : null
    });
}
```

**در Frontend**:

```javascript
// قبل از افزودن خدمت:

$('#addServiceBtn').click(async function() {
    var serviceId = $('#serviceSelect').val();
    
    // بررسی تعرفه
    var validation = await $.post('/api/v1/reception/service/validate-tariff', {
        serviceId: serviceId
    });
    
    if (validation.warning) {
        // نمایش Modal تأیید
        if (!confirm(validation.warning + '\n\nآیا می‌خواهید ادامه دهید?')) {
            return;
        }
    }
    
    // ادامه فرآیند افزودن
    addService(serviceId);
});
```

---

### راه‌حل 3: **Auto-Create Tariffs** (بلند مدت)

**مزایا**:
- اطمینان از وجود تعرفه برای همه ترکیبات
- کاهش نگهداری دستی

**پیاده‌سازی**:

```csharp
// سرویس برای ایجاد خودکار تعرفه‌ها

public async Task<ServiceResult> EnsureTariffsForPlanAsync(int planId)
{
    var plan = await _planRepo.GetByIdAsync(planId);
    var allServices = await _serviceRepo.GetAllActiveAsync();
    var existingTariffs = await _tariffRepo.GetByPlanIdAsync(planId);
    
    var missingServices = allServices
        .Where(s => !existingTariffs.Any(t => t.ServiceId == s.ServiceId))
        .ToList();
    
    if (missingServices.Any())
    {
        _logger.Information(
            "ایجاد تعرفه‌های پیش‌فرض برای {Count} خدمت - PlanId: {PlanId}",
            missingServices.Count, planId
        );
        
        foreach (var service in missingServices)
        {
            var tariff = new InsuranceTariff
            {
                InsurancePlanId = planId,
                ServiceId = service.ServiceId,
                InsuranceType = plan.InsuranceType,
                // محاسبه از BasePrice + CoveragePercent
                TariffPrice = service.BasePrice,
                PatientShare = CalculatePatientShare(service.BasePrice, plan.CoveragePercent),
                InsurerShare = CalculateInsurerShare(service.BasePrice, plan.CoveragePercent),
                IsActive = true,
                Priority = 5 // پیش‌فرض
            };
            
            await _tariffRepo.AddAsync(tariff);
        }
        
        await _tariffRepo.SaveChangesAsync();
    }
    
    return ServiceResult.Successful("تعرفه‌ها ایجاد شدند");
}
```

---

### راه‌حل 4: **Admin Notification Dashboard**

**پیاده‌سازی**:

```csharp
// گزارش تعرفه‌های ناقص

public async Task<List<MissingTariffReport>> GetMissingTariffsReportAsync()
{
    var plans = await _planRepo.GetAllActiveAsync();
    var services = await _serviceRepo.GetAllActiveAsync();
    var tariffs = await _tariffRepo.GetAllAsync();
    
    var report = new List<MissingTariffReport>();
    
    foreach (var plan in plans)
    {
        var missingServices = services
            .Where(s => !tariffs.Any(t => 
                t.InsurancePlanId == plan.InsurancePlanId && 
                t.ServiceId == s.ServiceId
            ))
            .ToList();
        
        if (missingServices.Any())
        {
            report.Add(new MissingTariffReport
            {
                PlanName = plan.Name,
                PlanId = plan.InsurancePlanId,
                MissingCount = missingServices.Count,
                MissingServices = missingServices.Select(s => s.Title).ToList()
            });
        }
    }
    
    return report;
}
```

---

## 8️⃣ توصیه نهایی

### 🎯 اقدام فوری (این هفته):

1. **راه‌حل 1: افزودن Warning** ⭐⭐⭐
   - پیاده‌سازی سریع (2-3 ساعت)
   - بدون تغییر منطق موجود
   - شفافیت برای کاربر

2. **راه‌حل 4: گزارش Admin** ⭐⭐
   - شناسایی تعرفه‌های ناقص
   - اقدام دستی Admin

### 📅 میان‌مدت (ماه آینده):

3. **راه‌حل 2: Pre-Validation** ⭐⭐⭐
   - جلوگیری از مشکل
   - تجربه کاربری بهتر

### 🔮 بلند مدت (3 ماه):

4. **راه‌حل 3: Auto-Create** ⭐
   - اتوماسیون کامل
   - کاهش مشکلات آینده

---

## 9️⃣ کد Implementation پیشنهادی

### Step 1: افزودن Warning به AddItemAsync

```csharp
// در ReceptionFacade.AddItemAsync() - خط ~1400

// ... بعد از محاسبه quoteResult

// ✅ WARNING: بررسی وجود تعرفه
bool hasBaseTariff = true;
bool hasSuppTariff = true;

if (draft.BasePlanId.HasValue)
{
    var baseTariff = await _context.InsuranceTariffs
        .FirstOrDefaultAsync(t =>
            t.InsurancePlanId == draft.BasePlanId.Value &&
            t.ServiceId == request.ServiceId &&
            t.InsuranceType == InsuranceType.Primary &&
            t.IsActive && !t.IsDeleted
        );
    hasBaseTariff = (baseTariff != null);
}

if (draft.SupplementaryPlanId.HasValue)
{
    var suppTariff = await _context.InsuranceTariffs
        .FirstOrDefaultAsync(t =>
            t.InsurancePlanId == draft.SupplementaryPlanId.Value &&
            t.ServiceId == request.ServiceId &&
            t.InsuranceType == InsuranceType.Supplementary &&
            t.IsActive && !t.IsDeleted
        );
    hasSuppTariff = (suppTariff != null);
}

if (!hasBaseTariff || !hasSuppTariff)
{
    _logger.Warning(
        "⚠️ RECEPTION: تعرفه ناقص - ServiceId: {ServiceId}, " +
        "BaseTariff: {HasBase}, SuppTariff: {HasSupp}",
        request.ServiceId, hasBaseTariff, hasSuppTariff
    );
    
    // اضافه کردن به Snapshot
    snapshot.TariffWarning = !hasBaseTariff ? "تعرفه پایه تعریف نشده" :
                             !hasSuppTariff ? "تعرفه تکمیلی تعریف نشده" : null;
}
```

### Step 2: نمایش Warning در Frontend

```javascript
// در reception-main.js - تابع displayItems()

function displayItems(items) {
    var html = '';
    items.forEach(function(item) {
        html += '<tr>';
        html += '<td>' + item.code + '';
        
        // ✅ نمایش Warning
        if (item.tariffWarning) {
            html += ' <i class="fas fa-exclamation-triangle text-warning" ' +
                    'title="' + item.tariffWarning + '"></i>';
        }
        
        html += '</td>';
        // ... بقیه columns
    });
    
    $('#itemsTable tbody').html(html);
}
```

---

## 🎁 نتیجه‌گیری

### ✅ وضعیت فعلی:
- سیستم **کار می‌کند** ولی **دقت 100% ندارد**
- Fallback Logic **خوب** است ولی **کافی نیست**

### ⚠️ مشکل اصلی:
- **تعرفه‌ها ناقص** هستند در دیتابیس
- **هیچ Warning** به کاربر داده نمی‌شود

### 🎯 راه‌حل توصیه شده:
1. **فوری**: افزودن Warning (راه‌حل 1)
2. **میان‌مدت**: Pre-Validation (راه‌حل 2)
3. **بلند مدت**: Auto-Create + Dashboard (راه‌حل‌های 3 و 4)

---

**تاریخ**: 2025-11-29  
**تحلیلگر**: Senior .NET Architect  
**وضعیت**: ✅ تحلیل کامل - آماده پیاده‌سازی
