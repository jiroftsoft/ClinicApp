# ✅ خلاصه پیاده‌سازی موارد ضروری

**تاریخ**: 2025-01-17  
**وضعیت**: ✅ کامل

---

## 🎯 موارد ضروری پیاده‌سازی شده

### 1. ✅ SnapshotJson در ReceptionItem

#### فیلد اضافه شده:
```csharp
// Models/Entities/Reception/ReceptionItem.cs (خط 77-78)
[Column(TypeName = "nvarchar(MAX)")]
public string SnapshotJson { get; set; }
```

#### Fluent API Configuration:
```csharp
// ReceptionItemConfig.cs (خط 191-193)
Property(ri => ri.SnapshotJson)
    .IsOptional()
    .HasMaxLength(int.MaxValue);
```

#### استفاده در AddItemAsync:
```csharp
// ReceptionFacade.cs (خطوط 1142-1204)
// دریافت ServiceComponents و FactorSetting برای Snapshot
var serviceComponents = await _context.ServiceComponents
    .Where(sc => sc.ServiceId == service.ServiceId && sc.IsActive && !sc.IsDeleted)
    .Select(sc => new { sc.ComponentType, sc.Coefficient })
    .ToListAsync();

var techComponent = serviceComponents.FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Technical);
var profComponent = serviceComponents.FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Professional);

var factors = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(ServiceComponentType.Technical, service.IsHashtagged, year);
var profFactor = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(ServiceComponentType.Professional, service.IsHashtagged, year);

var coefTech = techComponent?.Coefficient ?? 0m;
var coefProf = profComponent?.Coefficient ?? 0m;
var kTech = factors?.Value ?? 0m;
var kProf = profFactor?.Value ?? 0m;

// محاسبه TechAmount و ProfAmount
var techAmount = coefTech * kTech;
var profAmount = coefProf * kProf;
var baseKaPriceIRR = techAmount + profAmount;

// ایجاد Snapshot
var snapshot = new
{
    ServiceId = service.ServiceId,
    ServiceCode = service.ServiceCode,
    ServiceName = service.Title,
    Quantity = qty,
    UnitPrice = unit,
    KTech = kTech,
    KProf = kProf,
    CoefTech = coefTech,
    CoefProf = coefProf,
    BaseKaPriceIRR = unit,
    TechAmount = techAmount,
    ProfAmount = profAmount,
    GrossAmount = total,
    BaseInsuranceCoverage = itemBasePercent,
    SupplementaryCoverage = itemSuppPercent,
    PatientShare = itemPatientShare,
    InsurerShare = itemBasePay + itemSuppPay,
    RoundingMode = "RoundUp",
    RoundingDelta = 100,
    FactorSettingId = factors?.FactorSettingId,
    FinancialYear = year,
    BasePlanId = draft.BasePlanId,
    SupplementaryPlanId = draft.SupplementaryPlanId,
    CalculatedAt = DateTime.Now,
    GroupCode = service.GroupCode,
    IsHashtagged = service.IsHashtagged
};

var item = new ReceptionItem
{
    ReceptionId = draft.ReceptionId,
    ServiceId = service.ServiceId,
    Quantity = qty,
    UnitPrice = unit,
    PatientShareAmount = itemPatientShare,
    InsurerShareAmount = itemBasePay + itemSuppPay,
    SnapshotJson = Newtonsoft.Json.JsonConvert.SerializeObject(snapshot)
};
```

#### استفاده در Reprice-on-Change (SetInsurancesAsync):
```csharp
// ReceptionFacade.cs (خطوط 1416-1429)
// ✅ طبق نقشه پیوندی: به‌روزرسانی SnapshotJson هنگام Reprice
if (!string.IsNullOrWhiteSpace(item.SnapshotJson))
{
    try
    {
        var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(item.SnapshotJson);
        snapshot.BaseInsuranceCoverage = baseCoveragePercent;
        snapshot.SupplementaryCoverage = suppCoveragePercent;
        snapshot.PatientShare = itemPatientShare;
        snapshot.InsurerShare = itemBasePay + itemSuppPay;
        snapshot.BasePlanId = basePlan?.InsurancePlanId;
        snapshot.SupplementaryPlanId = suppPlan?.InsurancePlanId;
        snapshot.RepricedAt = DateTime.Now;
        item.SnapshotJson = Newtonsoft.Json.JsonConvert.SerializeObject(snapshot);
    }
    catch (Exception snapEx)
    {
        _logger.Warning(snapEx, "⚠️ FACADE: خطا در به‌روزرسانی SnapshotJson - ItemId: {ItemId}", item.ReceptionItemId);
        // ادامه می‌دهیم حتی اگر Snapshot به‌روزرسانی نشد
    }
}
```

---

### 2. ✅ AgeMin, AgeMax, GenderLimit, GroupCode در Service

#### فیلدهای اضافه شده:
```csharp
// Models/Entities/Clinic/Service.cs (خطوط 85-107)
/// <summary>
/// گروه خدمات (۱–۷)
/// طبق نقشه پیوندی: برای قواعد هشتگ‌دار و تعرفه
/// </summary>
[Range(1, 7, ErrorMessage = "گروه خدمات باید بین 1 تا 7 باشد.")]
public int? GroupCode { get; set; }

/// <summary>
/// حداقل سن برای استفاده از این خدمت (اختیاری)
/// طبق نقشه پیوندی: قید سن برای Eligibility
/// </summary>
[Range(0, 150, ErrorMessage = "حداقل سن باید بین 0 تا 150 سال باشد.")]
public int? AgeMin { get; set; }

/// <summary>
/// حداکثر سن برای استفاده از این خدمت (اختیاری)
/// طبق نقشه پیوندی: قید سن برای Eligibility
/// </summary>
[Range(0, 150, ErrorMessage = "حداکثر سن باید بین 0 تا 150 سال باشد.")]
public int? AgeMax { get; set; }

/// <summary>
/// محدودیت جنسیت برای این خدمت (اختیاری)
/// طبق نقشه پیوندی: قید جنسیت برای Eligibility
/// اگر null باشد، برای همه جنسیت‌ها قابل استفاده است
/// </summary>
public Gender? GenderLimit { get; set; }
```

#### Fluent API Configuration:
```csharp
// ServiceConfig.cs (خطوط 262-278)
// ✅ طبق نقشه پیوندی: قیود Eligibility
Property(s => s.GroupCode)
    .IsOptional()
    .HasColumnAnnotation("Index",
        new IndexAnnotation(new IndexAttribute("IX_Service_GroupCode")));

Property(s => s.AgeMin)
    .IsOptional()
    .HasColumnAnnotation("Index",
        new IndexAnnotation(new IndexAttribute("IX_Service_AgeMin")));

Property(s => s.AgeMax)
    .IsOptional()
    .HasColumnAnnotation("Index",
        new IndexAnnotation(new IndexAttribute("IX_Service_AgeMax")));

Property(s => s.GenderLimit)
    .IsOptional()
    .HasColumnAnnotation("Index",
        new IndexAnnotation(new IndexAttribute("IX_Service_GenderLimit")));
```

---

### 3. ✅ اعتبارسنجی Age/Gender در AddItemAsync

#### پیاده‌سازی:
```csharp
// ReceptionFacade.cs (خطوط 1038-1086)
// ✅ طبق نقشه پیوندی: اعتبارسنجی Service Eligibility (Age/Gender)
// دریافت اطلاعات بیمار
var patient = await _context.Patients
    .Where(p => p.PatientId == draft.PatientId && !p.IsDeleted)
    .Select(p => new { p.PatientId, p.BirthDate, p.Gender })
    .FirstOrDefaultAsync();

if (patient == null)
    return ServiceResult<ItemsAndTotalsDto>.Failed("اطلاعات بیمار یافت نشد");

// محاسبه سن بیمار
int? patientAge = null;
if (patient.BirthDate.HasValue)
{
    var today = DateTime.Today;
    patientAge = today.Year - patient.BirthDate.Value.Year;
    if (patient.BirthDate.Value.Date > today.AddYears(-patientAge.Value))
        patientAge--;
}

// بررسی AgeMin
if (service.AgeMin.HasValue && (!patientAge.HasValue || patientAge.Value < service.AgeMin.Value))
{
    _logger.Warning("⚠️ FACADE: حداقل سن برای این خدمت {AgeMin} سال است - ServiceId: {ServiceId}, PatientAge: {PatientAge}", 
        service.AgeMin.Value, service.ServiceId, patientAge);
    return ServiceResult<ItemsAndTotalsDto>.Failed(
        $"حداقل سن برای این خدمت {service.AgeMin.Value} سال است.", 
        "AGE_LIMIT");
}

// بررسی AgeMax
if (service.AgeMax.HasValue && (!patientAge.HasValue || patientAge.Value > service.AgeMax.Value))
{
    _logger.Warning("⚠️ FACADE: حداکثر سن برای این خدمت {AgeMax} سال است - ServiceId: {ServiceId}, PatientAge: {PatientAge}", 
        service.AgeMax.Value, service.ServiceId, patientAge);
    return ServiceResult<ItemsAndTotalsDto>.Failed(
        $"حداکثر سن برای این خدمت {service.AgeMax.Value} سال است.", 
        "AGE_LIMIT");
}

// بررسی GenderLimit
if (service.GenderLimit.HasValue && patient.Gender != service.GenderLimit.Value)
{
    _logger.Warning("⚠️ FACADE: این خدمت فقط برای {GenderLimit} مجاز است - ServiceId: {ServiceId}, PatientGender: {PatientGender}", 
        service.GenderLimit.Value, service.ServiceId, patient.Gender);
    return ServiceResult<ItemsAndTotalsDto>.Failed(
        $"این خدمت فقط برای {service.GenderLimit.Value} مجاز است.", 
        "GENDER_LIMIT");
}
```

---

## 📊 خلاصه آماری

| مورد | وضعیت | فایل | خطوط |
|-----|-------|------|------|
| SnapshotJson در ReceptionItem | ✅ کامل | ReceptionItem.cs | 77-78 |
| SnapshotJson Fluent API | ✅ کامل | ReceptionItemConfig.cs | 191-193 |
| SnapshotJson در AddItemAsync | ✅ کامل | ReceptionFacade.cs | 1142-1204 |
| SnapshotJson در Reprice-on-Change | ✅ کامل | ReceptionFacade.cs | 1416-1429 |
| AgeMin, AgeMax, GenderLimit در Service | ✅ کامل | Service.cs | 85-107 |
| Eligibility Fluent API | ✅ کامل | ServiceConfig.cs | 262-278 |
| اعتبارسنجی Age/Gender | ✅ کامل | ReceptionFacade.cs | 1038-1086 |

---

## ✅ وضعیت نهایی

همه موارد ضروری طبق نقشه پیوندی **با موفقیت پیاده‌سازی شده‌اند**:

1. ✅ **SnapshotJson**: ایجاد و ذخیره هنگام AddItem، به‌روزرسانی هنگام Reprice-on-Change
2. ✅ **AgeMin, AgeMax, GenderLimit**: فیلدها اضافه شده و در Fluent API پیکربندی شده‌اند
3. ✅ **اعتبارسنجی Age/Gender**: بررسی کامل در AddItemAsync

---

## 🔧 نیاز به Migration

برای اضافه کردن فیلدهای جدید به دیتابیس، باید Migration ایجاد شود:

```bash
# ایجاد Migration
Add-Migration AddSnapshotJsonAndServiceEligibilityFields

# اعمال Migration
Update-Database
```

---

**تاریخ تکمیل**: 2025-01-17  
**وضعیت**: ✅ تمام موارد ضروری پیاده‌سازی شده

