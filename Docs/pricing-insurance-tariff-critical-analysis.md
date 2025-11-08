# تحلیل حیاتی: سیستم Pricing, Insurance & Tariff

**تاریخ:** 2025-11-07  
**اولویت:** 🔴 **P0 - CRITICAL**  
**وضعیت:** ⚠️ **نیازمند بهبود فوری**

---

## 🚨 **خلاصه اجرایی**

سیستم محاسبه pricing و insurance در حال حاضر **validation ناکافی** دارد که می‌تواند منجر به **محاسبات مالی نادرست** شود.

**خطر اصلی:** یک خدمت (Service) می‌تواند بدون وجود تعرفه (Tariff) برای بیمه پایه یا تکمیلی افزوده شود، که منجر به محاسبه اشتباه سهم بیمار می‌شود.

---

## 🔍 **معماری فعلی**

### **1. جریان محاسبه Pricing:**

```
کاربر → ReceptionFacade.SetInsurancesAsync()
         ↓
         PricingEngine.QuoteAsync()
         ↓
         InsuranceCoverageProvider.GetRuleCoreAsync()
         ↓
         بررسی InsuranceTariff در database
         ↓
         [اگر tariff وجود نداشت؟ استفاده از default coverage از InsurancePlan!]
         ↓
         محاسبه سهم بیمار (ممکن است نادرست باشد!)
```

### **2. Components کلیدی:**

| Component | مسئولیت | وضعیت |
|-----------|---------|-------|
| `ReceptionFacade` | Orchestrator اصلی | ⚠️ **بدون validation** |
| `PricingEngine` | محاسبات pricing | ✅ کار می‌کند |
| `InsuranceCoverageProvider` | دریافت coverage rules | ⚠️ Fallback به default |
| `ReceptionPricingService` | Validation helper | ✅ دارد اما استفاده نمی‌شود! |
| `InsuranceTariffService` | مدیریت tariffs | ✅ کار می‌کند |

---

## ❌ **مشکلات شناسایی شده**

### **مشکل 1: عدم Validation قبل از اضافه کردن Service**

**کد فعلی در `ReceptionFacade.cs` (خط ~1207-1240):**

```csharp
public async Task<ServiceResult<AddItemResultDto>> AddItemToReceptionAsync(
    int receptionId, int serviceId, int quantity)
{
    // 1. محاسبه قیمت پایه خدمت
    var unitPrice = await _serviceCalculationEngine.CalculateUnitPriceIRRAsync(serviceId, year);
    
    // 2. افزودن به پذیرش
    var addResult = await _receptionWorkflowService.AddItemAsync(receptionId, serviceId, quantity, unitPrice);
    
    // 3. محاسبه مجدد مجموع‌ها
    var totalsResult = await _receptionRepository.RecalculateTotalsAsync(receptionId);
    
    // ❌ هیچ validation برای tariff وجود ندارد!
}
```

**مشکل:** اگر برای این `serviceId` و بیمه‌های reception، tariff تعریف نشده باشد، خدمت اضافه می‌شود و محاسبه اشتباه انجام می‌شود.

---

### **مشکل 2: Fallback به Default Coverage**

**کد در `InsuranceCoverageProvider.cs` (خط ~56-63):**

```csharp
if (tariff != null)
{
    // استفاده از tariff
    var coveragePercent = isSupplementary && tariff.SupplementaryCoveragePercent.HasValue
        ? tariff.SupplementaryCoveragePercent.Value
        : (tariff.PatientShare.HasValue || tariff.InsurerShare.HasValue)
            ? CalculateCoverageFromShares(tariff.PatientShare, tariff.InsurerShare)
            : await GetCoverageFromPlanAsync(planId, ct) ?? 0m;  // ❌ Fallback!
}
```

**مشکل:** اگر `tariff = null` باشد، سیستم به coverage درصد کلی `InsurancePlan` (مثلاً 70%) fallback می‌کند که ممکن است برای آن خدمت خاص صحیح نباشد.

---

### **مشکل 3: عدم استفاده از Validation موجود**

**کد موجود در `ReceptionPricingService.cs` (خط ~334-455):**

```csharp
/// <summary>
/// بررسی موجود بودن تعرفه برای یک خدمت و بیمه‌ها
/// </summary>
public async Task<(bool Success, string Code, string Message, Dictionary<string, object> Meta)> 
    ValidateTariffAvailability(int serviceId, int? basePlanId, int? suppPlanId, int financialYearId)
{
    // ✅ چک می‌کند که آیا InsuranceTariff برای base و supplementary وجود دارد
    
    if (baseTariff == null)
    {
        missing.Add("BASE");
    }
    
    if (suppTariff == null)
    {
        missing.Add("SUPP");
    }
    
    if (missing.Any())
    {
        return (false, "INSURANCE_SET_MISSING", 
            $"برای این خدمت تعیین‌ست بیمه‌ای یافت نشد. ({missingList})", 
            meta);
    }
}
```

**مشکل:** این متد **هیچ‌جا استفاده نمی‌شود**! 🤦‍♂️

```bash
$ grep -r "ValidateTariffAvailability" Services/Reception/
# نتیجه: فقط در ReceptionPricingService.cs تعریف شده، هیچ استفاده‌ای ندارد!
```

---

## 📊 **سناریوهای خطرناک**

### **سناریو 1: بیمار با بیمه سلامت + بیمه دانا**

**انتظار:**
- خدمت: ویزیت روانپزشکی (970040)
- بیمه پایه (سلامت): 70% = 2,695,700 ریال
- بیمه تکمیلی (دانا): 100% از باقیمانده = 1,155,300 ریال
- **سهم بیمار: 0 ریال** ✅

**اگر Tariff تکمیلی تعریف نشده:**
- بیمه پایه: 70% = 2,695,700 ریال
- بیمه تکمیلی: Fallback به 0% یا default = 0 ریال ❌
- **سهم بیمار: 1,155,300 ریال** ❌ (اشتباه!)

---

### **سناریو 2: بیمار فقط با بیمه پایه**

**انتظار:**
- خدمت: سونوگرافی (قیمت: 5,000,000 ریال)
- بیمه پایه: 70% = 3,500,000 ریال
- **سهم بیمار: 1,500,000 ریال** (30%) ✅

**اگر Tariff پایه تعریف نشده:**
- بیمه پایه: Fallback به 70% کلی = 3,500,000 ریال (شاید صحیح)
- **اما:** اگر برای این خدمت خاص، سهم بیمار 40% باشد؟
- **سهم بیمار محاسبه شده: 1,500,000** ❌ (باید 2,000,000 باشد!)

---

## 🎯 **راه‌حل پیشنهادی**

### **فاز 1: Validation فوری (P0 - Critical)**

#### **1.1. اضافه کردن Validation به `AddServiceToReception`**

```csharp
public async Task<ServiceResult<ItemsAndTotalsDto>> AddServiceToReceptionAsync(
    AddServiceRequest request)
{
    try
    {
        // ... existing code ...
        
        // ✅ بررسی وجود tariff BEFORE adding service
        if (draft.BasePlanId.HasValue || draft.SupplementaryPlanId.HasValue)
        {
            var tariffValidation = await _receptionPricingService.ValidateTariffAvailability(
                service.ServiceId,
                draft.BasePlanId,
                draft.SupplementaryPlanId,
                year
            );
            
            if (!tariffValidation.Success)
            {
                _logger.Warning("⚠️ FACADE: تعرفه بیمه‌ای برای خدمت یافت نشد - ServiceId: {ServiceId}, Code: {Code}", 
                    service.ServiceId, tariffValidation.Code);
                
                return ServiceResult<ItemsAndTotalsDto>.Failed(
                    tariffValidation.Message,
                    tariffValidation.Code,
                    ErrorCategory.Validation,
                    tariffValidation.Meta
                );
            }
        }
        
        // ... continue with adding service ...
    }
    catch (Exception ex)
    {
        // ... error handling ...
    }
}
```

#### **1.2. اضافه کردن Validation به `SetInsurances`**

```csharp
public async Task<ServiceResult<ItemsAndTotalsDto>> SetInsurancesAsync(
    SetInsurancesRequest request)
{
    try
    {
        // ... existing code ...
        
        // ✅ بررسی تمام services موجود در reception
        if (draft.ReceptionItems != null && draft.ReceptionItems.Any())
        {
            var missingTariffs = new List<string>();
            
            foreach (var item in draft.ReceptionItems.Where(i => !i.IsDeleted))
            {
                var tariffValidation = await _receptionPricingService.ValidateTariffAvailability(
                    item.ServiceId,
                    request.BasePlanId,
                    request.SupplementaryPlanId,
                    year
                );
                
                if (!tariffValidation.Success)
                {
                    var service = await _context.Services
                        .Where(s => s.ServiceId == item.ServiceId)
                        .Select(s => s.Title)
                        .FirstOrDefaultAsync();
                    
                    missingTariffs.Add($"{service} ({item.ServiceId})");
                }
            }
            
            if (missingTariffs.Any())
            {
                var errorMessage = $"برای خدمات زیر تعرفه بیمه‌ای تعریف نشده است:\n{string.Join("\n", missingTariffs)}";
                
                _logger.Warning("⚠️ FACADE: تعرفه‌های بیمه‌ای ناقص - ReceptionId: {ReceptionId}, Missing: {Missing}", 
                    request.ReceptionId, string.Join(", ", missingTariffs));
                
                return ServiceResult<ItemsAndTotalsDto>.Failed(
                    errorMessage,
                    "INSURANCE_TARIFFS_MISSING",
                    ErrorCategory.Validation,
                    new Dictionary<string, object>
                    {
                        ["missingServices"] = missingTariffs
                    }
                );
            }
        }
        
        // ... continue with setting insurances ...
    }
    catch (Exception ex)
    {
        // ... error handling ...
    }
}
```

---

### **فاز 2: بهبود PricingEngine (P1 - High)**

#### **2.1. حذف Fallback خطرناک**

```csharp
// در InsuranceCoverageProvider.cs

private async Task<CoverageRule> GetRuleCoreAsync(...)
{
    var tariff = await _db.InsuranceTariffs
        .AsNoTracking()
        .Where(t => t.InsurancePlanId == planId &&
                   t.ServiceId == serviceId &&
                   !t.IsDeleted &&
                   t.IsActive &&
                   t.InsuranceType == (isSupplementary ? InsuranceType.Supplementary : InsuranceType.Primary))
        .FirstOrDefaultAsync(ct);

    if (tariff == null)
    {
        // ❌ قبل: Fallback به coverage کلی
        // var coveragePercent = await GetCoverageFromPlanAsync(planId, ct) ?? 0m;
        
        // ✅ بعد: خطای واضح
        _log.Error("🚨 PRICING: تعرفه بیمه‌ای یافت نشد - PlanId: {PlanId}, ServiceId: {ServiceId}, IsSupplementary: {IsSupplementary}",
            planId, serviceId, isSupplementary);
        
        throw new InvalidOperationException(
            $"تعرفه بیمه‌ای برای این خدمت تعریف نشده است. PlanId: {planId}, ServiceId: {serviceId}"
        );
    }
    
    // ... continue with tariff ...
}
```

---

### **فاز 3: UI/UX Improvements (P2 - Medium)**

#### **3.1. نمایش Warning در Frontend**

```javascript
// در service-lookup.js

async function validateServiceTariff(serviceId, basePlanId, suppPlanId) {
    try {
        const response = await ReceptionAPI.post('/validate-tariff', {
            serviceId: serviceId,
            basePlanId: basePlanId,
            supplementaryPlanId: suppPlanId
        });
        
        if (!response.Success) {
            if (response.Code === 'INSURANCE_SET_MISSING') {
                // نمایش warning به کاربر
                toastr.warning(
                    response.Message + '\n' +
                    'لطفاً ابتدا تعرفه را تعریف کنید.',
                    'تعرفه بیمه‌ای یافت نشد',
                    {
                        timeOut: 0,  // نمایش تا زمانی که کاربر ببندد
                        closeButton: true
                    }
                );
                
                // نمایش لینک برای ایجاد تعرفه
                if (response.Meta && response.Meta.createTariffUrl) {
                    toastr.info(
                        `<a href="${response.Meta.createTariffUrl}" target="_blank">ایجاد تعرفه</a>`,
                        'راه‌حل',
                        {
                            timeOut: 0,
                            escapeHtml: false,
                            closeButton: true
                        }
                    );
                }
                
                return false;  // جلوگیری از اضافه کردن service
            }
        }
        
        return true;  // OK to add service
    } catch (err) {
        console.error('خطا در validation تعرفه:', err);
        return true;  // در صورت خطا، اجازه اضافه کردن (بررسی در backend)
    }
}
```

#### **3.2. نمایش Status Icon در Service List**

```javascript
// در service-lookup.js

function renderServiceOption(service) {
    let statusIcon = '';
    
    if (service.hasTariff === false) {
        statusIcon = '<i class="fas fa-exclamation-triangle text-warning" title="تعرفه بیمه‌ای تعریف نشده"></i>';
    } else if (service.hasTariff === true) {
        statusIcon = '<i class="fas fa-check-circle text-success" title="تعرفه موجود"></i>';
    }
    
    return `
        <option value="${service.serviceId}" data-has-tariff="${service.hasTariff}">
            ${statusIcon} ${service.title} (${service.code})
        </option>
    `;
}
```

---

### **فاز 4: Monitoring & Alerting (P3 - Low)**

#### **4.1. Serilog Enrichment**

```csharp
public class TariffValidationEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.MessageTemplate.Text.Contains("INSURANCE_SET_MISSING"))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "AlertType", "CRITICAL_TARIFF_MISSING"));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "RequiresAction", true));
        }
    }
}
```

#### **4.2. Dashboard Metrics**

- تعداد دفعات "INSURANCE_SET_MISSING" در 24 ساعت گذشته
- لیست services بدون tariff که بیشترین تلاش برای استفاده داشته‌اند
- لیست insurance plans بدون tariff کامل

---

## 🧪 **تست‌های پیشنهادی**

### **Test 1: تلاش برای اضافه کردن Service بدون Tariff**

```csharp
[Test]
public async Task AddService_WithoutTariff_ShouldFail()
{
    // Arrange
    var receptionId = 1000;
    var serviceId = 487;  // Service بدون tariff
    var basePlanId = 1012;  // بیمه سلامت
    
    // Act
    var result = await _facade.AddServiceToReceptionAsync(new AddServiceRequest
    {
        ReceptionId = receptionId,
        ServiceId = serviceId,
        Quantity = 1
    });
    
    // Assert
    Assert.IsFalse(result.Success);
    Assert.AreEqual("INSURANCE_SET_MISSING", result.Code);
    Assert.That(result.Message, Does.Contain("تعرفه بیمه‌ای یافت نشد"));
}
```

### **Test 2: محاسبه صحیح سهم بیمار با بیمه پایه + تکمیلی**

```csharp
[Test]
public async Task Calculate_WithBaseAndSupplementary_PatientShareShouldBeZero()
{
    // Arrange
    var serviceId = 487;  // ویزیت روانپزشکی
    var basePlanId = 1012;  // بیمه سلامت (70%)
    var suppPlanId = 1018;  // بیمه دانا (100%)
    
    // Act
    var result = await _pricingEngine.QuoteAsync(new QuoteRequestDto
    {
        ServiceId = serviceId,
        Primary = new PartyInsuranceDto { InsurancePlanId = basePlanId },
        Supplementary = new PartyInsuranceDto { InsurancePlanId = suppPlanId }
    });
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(0, result.PatientShare);  // سهم بیمار = 0
    Assert.Greater(result.PrimaryCovered, 0);  // بیمه پایه پوشش داده
    Assert.Greater(result.SupplementaryCovered, 0);  // بیمه تکمیلی پوشش داده
}
```

---

## 📋 **Checklist پیاده‌سازی**

### **فوری (این هفته):**
- [ ] اضافه کردن validation به `AddServiceToReception`
- [ ] اضافه کردن validation به `SetInsurances`
- [ ] تست manual با سناریوهای مختلف
- [ ] نوشتن Unit Tests

### **کوتاه‌مدت (این ماه):**
- [ ] حذف fallback خطرناک از `InsuranceCoverageProvider`
- [ ] اضافه کردن UI warnings
- [ ] نمایش status icons برای services
- [ ] ایجاد API endpoint برای validation

### **میان‌مدت (3 ماه):**
- [ ] ایجاد dashboard برای monitoring
- [ ] پیاده‌سازی alerting
- [ ] ایجاد bulk tariff creation tool
- [ ] Training برای admin users

---

## 🎓 **آموزش برای Admins**

### **قوانین طلایی:**

1. ✅ **برای هر خدمت که در reception استفاده می‌شود، حتماً tariff تعریف کنید**
2. ✅ **اگر بیمار بیمه تکمیلی دارد، حتماً tariff تکمیلی هم تعریف کنید**
3. ✅ **قبل از تغییر بیمه، تمام services را چک کنید که tariff دارند**
4. ❌ **هرگز به صورت دستی مبالغ را تغییر ندهید** (اعتماد به سیستم محاسبه)

---

## 📊 **تأثیر مالی**

**قبل از Fix:**
- احتمال محاسبه اشتباه: **بالا** 🔴
- خطر مالی برای کلینیک: **متوسط تا بالا** ⚠️
- خطر مالی برای بیمار: **بالا** 🔴

**بعد از Fix:**
- احتمال محاسبه اشتباه: **خیلی پایین** 🟢
- خطر مالی برای کلینیک: **خیلی پایین** ✅
- خطر مالی برای بیمار: **صفر** ✅

---

## 🔗 **منابع مرتبط**

- `Services/Reception/ReceptionFacade.cs`
- `Services/Reception/ReceptionPricingService.cs`
- `Services/Pricing/Coverage/InsuranceCoverageProvider.cs`
- `Services/Insurance/InsuranceTariffService.cs`
- `Areas/Admin/Controllers/Insurance/InsuranceTariffController.cs`

---

**تهیه شده توسط:** AI Assistant  
**تأیید شده توسط:** Development Team  
**وضعیت:** ✅ Ready for Implementation

