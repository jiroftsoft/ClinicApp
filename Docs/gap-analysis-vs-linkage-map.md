# 🔍 تحلیل شکاف‌ها - مقایسه با نقشه پیوندی

**تاریخ ایجاد**: 2025-01-17  
**نسخه**: 1.0.0  
**نویسنده**: Senior Module Analyst & Architecture Specialist

---

## 🎯 هدف سند

این سند، **تفاوت‌های بین پیاده‌سازی فعلی** و **نقشه پیوندی** را شناسایی کرده و راه‌حل‌های پیشنهادی را ارائه می‌دهد.

---

## 📋 فهرست مطالب

1. [موجودیت‌های گم‌شده](#1-موجودیت‌های-گم‌شده)
2. [فیلدهای گم‌شده](#2-فیلدهای-گم‌شده)
3. [روابط گم‌شده](#3-روابط-گم‌شده)
4. [اعتبارسنجی‌های گم‌شده](#4-اعتبارسنجی‌های-گم‌شده)
5. [قواعد کسب‌وکار گم‌شده](#5-قواعد-کسب‌وکار-گم‌شده)
6. [راه‌حل‌های پیشنهادی](#6-راه‌حل‌های-پیشنهادی)

---

## 1. موجودیت‌های گم‌شده

### 1.1 DoctorClinic (Many-to-Many)

#### ❌ وضعیت فعلی:
```csharp
// Doctor.cs
public int? ClinicId { get; set; } // 1:N (Optional)
public virtual Clinic Clinic { get; set; }
```

#### ✅ طبق نقشه پیوندی:
```csharp
// DoctorClinic (Many-to-Many)
public class DoctorClinic : ITrackable
{
    public int DoctorId { get; set; }
    public int ClinicId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public virtual Doctor Doctor { get; set; }
    public virtual Clinic Clinic { get; set; }
}
```

#### 📝 پیشنهاد:
- اگر نیاز به **Many-to-Many** باشد (یک پزشک در چند کلینیک)، باید جدول `DoctorClinic` ایجاد شود
- فعلاً، ساختار **1:N** کافی است (هر پزشک به یک کلینیک اصلی تعلق دارد)

---

### 1.2 ClinicDepartment (Many-to-Many)

#### ❌ وضعیت فعلی:
```csharp
// Department.cs
public int ClinicId { get; set; } // 1:N (Required)
public virtual Clinic Clinic { get; set; }
```

#### ✅ طبق نقشه پیوندی:
```csharp
// ClinicDepartment (Many-to-Many)
public class ClinicDepartment : ITrackable
{
    public int ClinicId { get; set; }
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; }
    
    public virtual Clinic Clinic { get; set; }
    public virtual Department Department { get; set; }
}
```

#### 📝 پیشنهاد:
- اگر نیاز به **Many-to-Many** باشد (یک دپارتمان در چند کلینیک)، باید جدول `ClinicDepartment` ایجاد شود
- فعلاً، ساختار **1:N** کافی است (هر دپارتمان به یک کلینیک تعلق دارد)

---

### 1.3 DepartmentService (Many-to-Many مستقیم)

#### ❌ وضعیت فعلی:
```csharp
// Service → ServiceCategory → Department (1:N → 1:N)
// SharedService (Many-to-Many) برای خدمات مشترک
```

#### ✅ طبق نقشه پیوندی:
```csharp
// DepartmentService (Many-to-Many)
public class DepartmentService : ITrackable
{
    public int DepartmentId { get; set; }
    public int ServiceId { get; set; }
    public bool IsActive { get; set; }
    
    public virtual Department Department { get; set; }
    public virtual Service Service { get; set; }
}
```

#### 📝 پیشنهاد:
- اگر نیاز به **Many-to-Many مستقیم** باشد (بدون دسته‌بندی)، باید جدول `DepartmentService` ایجاد شود
- فعلاً، از طریق `ServiceCategory.DepartmentId` و `SharedService` قابل استفاده است

---

### 1.4 PlanCoverage

#### ❌ وضعیت فعلی:
```csharp
// InsurancePlan.cs
public decimal CoveragePercent { get; set; } // فقط درصد پوشش
public decimal Deductible { get; set; } // فرانشیز
```

#### ✅ طبق نقشه پیوندی:
```csharp
// PlanCoverage (1:N)
public class PlanCoverage : ITrackable
{
    public int PlanCoverageId { get; set; }
    public int PlanId { get; set; }
    public decimal FranchisePercent { get; set; }
    public decimal CeilingDaily { get; set; }
    public decimal CeilingMonthly { get; set; }
    public string ServiceGroupRulesJson { get; set; }
    
    public virtual InsurancePlan Plan { get; set; }
}
```

#### 📝 پیشنهاد:
- اگر نیاز به **سقف روزانه/ماهانه** و **قواعد گروه خدمات** باشد، باید جدول `PlanCoverage` ایجاد شود
- فعلاً، `CoveragePercent` و `Deductible` در `InsurancePlan` کافی است

---

## 2. فیلدهای گم‌شده

### 2.1 KaPro و KaTech در InsuranceTariff

#### ❌ وضعیت فعلی:
```csharp
// InsuranceTariff.cs
public decimal? TariffPrice { get; set; } // قیمت نهایی
public decimal? PatientShare { get; set; }
public decimal? InsurerShare { get; set; }
```

#### ✅ طبق نقشه پیوندی:
```csharp
// InsuranceTariff.cs
public decimal? KaPro { get; set; } // ضریب حرفه‌ای
public decimal? KaTech { get; set; } // ضریب فنی
public decimal? UnitPrice { get; set; } // قیمت واحد (محاسبه شده)
public bool IsHashed { get; set; }
public int Year { get; set; }
public DateTime? FromDate { get; set; }
public DateTime? ToDate { get; set; }
```

#### 📝 پیشنهاد:
- **استفاده از ServiceComponent**: فعلاً از `ServiceComponent` برای `KaPro` و `KaTech` استفاده می‌شود
- اگر نیاز به **تعرفه‌های مستقیم** باشد، باید فیلدهای `KaPro` و `KaTech` به `InsuranceTariff` اضافه شوند
- **از طریق ServiceCalculationEngine**: محاسبه قیمت با `ServiceComponents` و `FactorSetting` انجام می‌شود

---

### 2.2 SnapshotJson در ReceptionItem

#### ❌ وضعیت فعلی:
```csharp
// ReceptionItem.cs
public int ReceptionItemId { get; set; }
public int ReceptionId { get; set; }
public int ServiceId { get; set; }
public int Quantity { get; set; }
public decimal UnitPrice { get; set; }
public decimal PatientShareAmount { get; set; }
public decimal InsurerShareAmount { get; set; }
// ❌ SnapshotJson وجود ندارد
```

#### ✅ طبق نقشه پیوندی:
```csharp
// ReceptionItem.cs
public string SnapshotJson { get; set; } // تصویر immutable از محاسبات
```

#### 📝 محتوای پیشنهادی SnapshotJson:
```json
{
  "ServiceId": 123,
  "ServiceCode": "SVC-001",
  "ServiceName": "ویزیت پزشک",
  "Quantity": 1,
  "UnitPrice": 1000000,
  "KTech": 1.5,
  "KProf": 2.0,
  "CoefTech": 1000,
  "CoefProf": 2000,
  "BaseKaPriceIRR": 5500,
  "TechAmount": 1500,
  "ProfAmount": 4000,
  "GrossAmount": 1000000,
  "BaseInsuranceCoverage": 70.0,
  "SupplementaryCoverage": 20.0,
  "PatientShare": 300000,
  "InsurerShare": 700000,
  "RoundingMode": "RoundUp",
  "RoundingDelta": 100,
  "FactorSettingId": 1,
  "FinancialYear": 1403,
  "BasePlanId": 1,
  "SupplementaryPlanId": 2,
  "CalculatedAt": "2025-01-17T10:30:00Z"
}
```

#### 📝 پیشنهاد:
```csharp
// ReceptionItem.cs
[Column(TypeName = "nvarchar(MAX)")]
public string SnapshotJson { get; set; }
```

**پیاده‌سازی**:
- هنگام `AddItem`: ایجاد و ذخیره Snapshot
- هنگام `Reprice-on-Change`: به‌روزرسانی Snapshot
- استفاده از `Newtonsoft.Json.JsonConvert.SerializeObject`

---

### 2.3 AgeMin, AgeMax, GenderLimit در Service

#### ❌ وضعیت فعلی:
```csharp
// Service.cs
public int ServiceId { get; set; }
public string Title { get; set; }
public string ServiceCode { get; set; }
public decimal Price { get; set; }
public bool IsHashtagged { get; set; }
// ❌ AgeMin, AgeMax, GenderLimit وجود ندارد
```

#### ✅ طبق نقشه پیوندی:
```csharp
// Service.cs
public int? AgeMin { get; set; }
public int? AgeMax { get; set; }
public Gender? GenderLimit { get; set; } // فقط برای یک جنسیت خاص
public int? GroupCode { get; set; } // 1-7
```

#### 📝 پیشنهاد:
```csharp
// Service.cs
[Range(0, 150)]
public int? AgeMin { get; set; }

[Range(0, 150)]
public int? AgeMax { get; set; }

public Gender? GenderLimit { get; set; }

[Range(1, 7)]
public int? GroupCode { get; set; }
```

**اعتبارسنجی**:
- هنگام `AddItem`: بررسی `Age` و `Gender` بیمار
- اگر `Service.AgeMin` یا `Service.AgeMax` تعریف شده باشد
- اگر `Service.GenderLimit` تعریف شده باشد

---

## 3. روابط گم‌شده

### 3.1 DoctorClinic (Many-to-Many)

#### ❌ وضعیت فعلی:
- `Doctor.ClinicId` (1:N Optional)

#### ✅ طبق نقشه پیوندی:
- `DoctorClinic` (Many-to-Many)

#### 📝 پیاده‌سازی پیشنهادی:
```csharp
// Models/Entities/Doctor/DoctorClinic.cs
public class DoctorClinic : ITrackable
{
    public int DoctorId { get; set; }
    public int ClinicId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public virtual Doctor Doctor { get; set; }
    public virtual Clinic Clinic { get; set; }
}

// DoctorClinicConfig.cs
public class DoctorClinicConfig : EntityTypeConfiguration<DoctorClinic>
{
    public DoctorClinicConfig()
    {
        ToTable("DoctorClinics");
        HasKey(dc => new { dc.DoctorId, dc.ClinicId });
        
        HasRequired(dc => dc.Doctor)
            .WithMany(d => d.DoctorClinics)
            .HasForeignKey(dc => dc.DoctorId)
            .WillCascadeOnDelete(false);
        
        HasRequired(dc => dc.Clinic)
            .WithMany(c => c.DoctorClinics)
            .HasForeignKey(dc => dc.ClinicId)
            .WillCascadeOnDelete(false);
        
        HasIndex(dc => new { dc.DoctorId, dc.ClinicId, dc.IsActive })
            .HasName("IX_DoctorClinic_DoctorId_ClinicId_IsActive");
        
        HasIndex(dc => new { dc.StartDate, dc.EndDate })
            .HasName("IX_DoctorClinic_StartDate_EndDate");
    }
}
```

---

### 3.2 ClinicDepartment (Many-to-Many)

#### ❌ وضعیت فعلی:
- `Department.ClinicId` (1:N Required)

#### ✅ طبق نقشه پیوندی:
- `ClinicDepartment` (Many-to-Many)

#### 📝 پیاده‌سازی پیشنهادی:
```csharp
// Models/Entities/Clinic/ClinicDepartment.cs
public class ClinicDepartment : ITrackable
{
    public int ClinicId { get; set; }
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; }
    
    public virtual Clinic Clinic { get; set; }
    public virtual Department Department { get; set; }
}

// ClinicDepartmentConfig.cs
public class ClinicDepartmentConfig : EntityTypeConfiguration<ClinicDepartment>
{
    public ClinicDepartmentConfig()
    {
        ToTable("ClinicDepartments");
        HasKey(cd => new { cd.ClinicId, cd.DepartmentId });
        
        HasRequired(cd => cd.Clinic)
            .WithMany(c => c.ClinicDepartments)
            .HasForeignKey(cd => cd.ClinicId)
            .WillCascadeOnDelete(false);
        
        HasRequired(cd => cd.Department)
            .WithMany(d => d.ClinicDepartments)
            .HasForeignKey(cd => cd.DepartmentId)
            .WillCascadeOnDelete(false);
        
        HasIndex(cd => new { cd.ClinicId, cd.DepartmentId })
            .IsUnique()
            .HasName("IX_ClinicDepartment_ClinicId_DepartmentId_Unique");
    }
}
```

---

### 3.3 DepartmentService (Many-to-Many مستقیم)

#### ❌ وضعیت فعلی:
- از طریق `ServiceCategory.DepartmentId` و `SharedService`

#### ✅ طبق نقشه پیوندی:
- `DepartmentService` (Many-to-Many)

#### 📝 پیاده‌سازی پیشنهادی:
```csharp
// Models/Entities/Clinic/DepartmentService.cs
public class DepartmentService : ITrackable
{
    public int DepartmentId { get; set; }
    public int ServiceId { get; set; }
    public bool IsActive { get; set; }
    
    public virtual Department Department { get; set; }
    public virtual Service Service { get; set; }
}

// DepartmentServiceConfig.cs
public class DepartmentServiceConfig : EntityTypeConfiguration<DepartmentService>
{
    public DepartmentServiceConfig()
    {
        ToTable("DepartmentServices");
        HasKey(ds => new { ds.DepartmentId, ds.ServiceId });
        
        HasRequired(ds => ds.Department)
            .WithMany(d => d.DepartmentServices)
            .HasForeignKey(ds => ds.DepartmentId)
            .WillCascadeOnDelete(false);
        
        HasRequired(ds => ds.Service)
            .WithMany(s => s.DepartmentServices)
            .HasForeignKey(ds => ds.ServiceId)
            .WillCascadeOnDelete(false);
        
        HasIndex(ds => new { ds.DepartmentId, ds.ServiceId })
            .IsUnique()
            .HasName("IX_DepartmentService_DepartmentId_ServiceId_Unique");
    }
}
```

---

## 4. اعتبارسنجی‌های گم‌شده

### 4.1 Service Eligibility (Age/Gender)

#### ❌ وضعیت فعلی:
- هیچ اعتبارسنجی Age/Gender وجود ندارد

#### ✅ طبق نقشه پیوندی:
- بررسی `Service.AgeMin`, `Service.AgeMax`, `Service.GenderLimit`

#### 📝 پیاده‌سازی پیشنهادی:
```csharp
// ReceptionFacade.AddItemAsync
// بررسی Age
if (service.AgeMin.HasValue && patientAge < service.AgeMin.Value)
{
    return ServiceResult<ItemsAndTotalsDto>.Failed(
        $"حداقل سن برای این خدمت {service.AgeMin.Value} سال است.",
        "AGE_LIMIT");
}

if (service.AgeMax.HasValue && patientAge > service.AgeMax.Value)
{
    return ServiceResult<ItemsAndTotalsDto>.Failed(
        $"حداکثر سن برای این خدمت {service.AgeMax.Value} سال است.",
        "AGE_LIMIT");
}

// بررسی Gender
if (service.GenderLimit.HasValue && patient.Gender != service.GenderLimit.Value)
{
    return ServiceResult<ItemsAndTotalsDto>.Failed(
        $"این خدمت فقط برای {service.GenderLimit.Value} مجاز است.",
        "GENDER_LIMIT");
}
```

---

### 4.2 Service Limits (Daily/Monthly)

#### ❌ وضعیت فعلی:
- هیچ اعتبارسنجی Service Limits وجود ندارد

#### ✅ طبق نقشه پیوندی:
- بررسی محدودیت‌های روزانه/ماهانه بر اساس `PlanCoverage.ServiceGroupRulesJson`

#### 📝 پیاده‌سازی پیشنهادی:
```csharp
// ReceptionFacade.AddItemAsync
// بررسی Service Limits (Daily)
var dailyCount = await _context.ReceptionItems
    .Where(ri => ri.ServiceId == serviceId && 
                ri.Reception.PatientId == patientId &&
                ri.Reception.ReceptionDate.Date == DateTime.Now.Date &&
                !ri.IsDeleted)
    .CountAsync();

if (dailyCount >= planCoverage.CeilingDaily)
{
    return ServiceResult<ItemsAndTotalsDto>.Failed(
        $"حد مجاز روزانه این خدمت {planCoverage.CeilingDaily} بار است.",
        "SERVICE_LIMIT_DAILY");
}

// بررسی Service Limits (Monthly)
var monthlyCount = await _context.ReceptionItems
    .Where(ri => ri.ServiceId == serviceId && 
                ri.Reception.PatientId == patientId &&
                ri.Reception.ReceptionDate.Year == DateTime.Now.Year &&
                ri.Reception.ReceptionDate.Month == DateTime.Now.Month &&
                !ri.IsDeleted)
    .CountAsync();

if (monthlyCount >= planCoverage.CeilingMonthly)
{
    return ServiceResult<ItemsAndTotalsDto>.Failed(
        $"حد مجاز ماهانه این خدمت {planCoverage.CeilingMonthly} بار است.",
        "SERVICE_LIMIT_MONTHLY");
}
```

---

## 5. قواعد کسب‌وکار گم‌شده

### 5.1 Snapshot on AddItem

#### ❌ وضعیت فعلی:
- SnapshotJson ذخیره نمی‌شود

#### ✅ طبق نقشه پیوندی:
- ذخیره SnapshotJson هنگام AddItem

#### 📝 پیاده‌سازی پیشنهادی:
```csharp
// ReceptionFacade.AddItemAsync
var snapshot = new
{
    ServiceId = service.ServiceId,
    ServiceCode = service.ServiceCode,
    ServiceName = service.Title,
    Quantity = qty,
    UnitPrice = unit,
    KTech = factorSetting.KTech,
    KProf = factorSetting.KProf,
    CoefTech = serviceComponent?.Coefficient ?? 0,
    CoefProf = serviceComponent?.Coefficient ?? 0,
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
    FactorSettingId = factorSetting?.FactorSettingId,
    FinancialYear = year,
    BasePlanId = draft.BasePlanId,
    SupplementaryPlanId = draft.SupplementaryPlanId,
    CalculatedAt = DateTime.Now
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

---

### 5.2 Reprice-on-Change با Snapshot

#### ❌ وضعیت فعلی:
- فقط مقادیر به‌روزرسانی می‌شوند

#### ✅ طبق نقشه پیوندی:
- به‌روزرسانی SnapshotJson هنگام Reprice

#### 📝 پیاده‌سازی پیشنهادی:
```csharp
// ReceptionFacade.SetInsurancesAsync
foreach (var item in draft.ReceptionItems.Where(ri => !ri.IsDeleted))
{
    // محاسبه مجدد سهم‌ها
    var itemGross = item.UnitPrice * item.Quantity;
    var itemBasePay = itemGross * (baseCoveragePercent / 100m);
    var itemAfterBase = itemGross - itemBasePay;
    var itemSuppPay = itemAfterBase * (suppCoveragePercent / 100m);
    var itemPatientShare = itemAfterBase - itemSuppPay;
    
    // به‌روزرسانی Snapshot
    var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(item.SnapshotJson ?? "{}");
    snapshot.BaseInsuranceCoverage = baseCoveragePercent;
    snapshot.SupplementaryCoverage = suppCoveragePercent;
    snapshot.PatientShare = itemPatientShare;
    snapshot.InsurerShare = itemBasePay + itemSuppPay;
    snapshot.RepricedAt = DateTime.Now;
    
    item.PatientShareAmount = itemPatientShare;
    item.InsurerShareAmount = itemBasePay + itemSuppPay;
    item.SnapshotJson = Newtonsoft.Json.JsonConvert.SerializeObject(snapshot);
    item.UpdatedAt = DateTime.Now;
}
```

---

## 6. راه‌حل‌های پیشنهادی

### 6.1 اولویت بالا (Critical)

1. **اضافه کردن SnapshotJson به ReceptionItem**:
   - فیلد: `[Column(TypeName = "nvarchar(MAX)")] public string SnapshotJson { get; set; }`
   - پیاده‌سازی: ذخیره هنگام AddItem و به‌روزرسانی هنگام Reprice
   - Migration: ایجاد Migration برای اضافه کردن ستون

2. **اعتبارسنجی Service Eligibility (Age/Gender)**:
   - اضافه کردن فیلدهای `AgeMin`, `AgeMax`, `GenderLimit` به `Service`
   - پیاده‌سازی اعتبارسنجی در `ReceptionFacade.AddItemAsync`

---

### 6.2 اولویت متوسط (Important)

3. **ایجاد PlanCoverage**:
   - اگر نیاز به سقف روزانه/ماهانه و قواعد گروه خدمات باشد
   - ایجاد Entity و Migration

4. **اضافه کردن KaPro و KaTech به InsuranceTariff**:
   - اگر نیاز به تعرفه‌های مستقیم باشد
   - یا استفاده از ServiceComponent (فعلی)

---

### 6.3 اولویت پایین (Nice to Have)

5. **ایجاد DoctorClinic (Many-to-Many)**:
   - اگر نیاز به Many-to-Many باشد
   - فعلاً 1:N کافی است

6. **ایجاد ClinicDepartment (Many-to-Many)**:
   - اگر نیاز به Many-to-Many باشد
   - فعلاً 1:N کافی است

7. **ایجاد DepartmentService (Many-to-Many)**:
   - اگر نیاز به Many-to-Many مستقیم باشد
   - فعلاً از طریق ServiceCategory و SharedService قابل استفاده است

---

## 7. خلاصه

### ✅ موارد پیاده‌سازی شده:
- ✅ DoctorDepartment (Many-to-Many) با StartDate/EndDate
- ✅ اعتبارسنجی Doctor-Department-Clinic
- ✅ Reprice-on-Change در SetInsurancesAsync
- ✅ SharedService برای خدمات مشترک
- ✅ ServiceCalculationEngine برای محاسبه قیمت

### ⚠️ شکاف‌های شناسایی شده:
- ❌ SnapshotJson در ReceptionItem
- ❌ AgeMin, AgeMax, GenderLimit در Service
- ❌ PlanCoverage برای سقف روزانه/ماهانه
- ❌ Service Limits (Daily/Monthly) Validation
- ❌ DoctorClinic (Many-to-Many) - فعلاً 1:N
- ❌ ClinicDepartment (Many-to-Many) - فعلاً 1:N
- ❌ DepartmentService (Many-to-Many) - فعلاً از طریق ServiceCategory

---

## 8. توصیه‌ها

### 🔧 اقدامات فوری:
1. **اضافه کردن SnapshotJson** به `ReceptionItem` (اولویت 1)
2. **اضافه کردن AgeMin, AgeMax, GenderLimit** به `Service` (اولویت 2)
3. **پیاده‌سازی اعتبارسنجی Age/Gender** در `AddItemAsync` (اولویت 2)

### 📋 اقدامات آینده:
4. **ایجاد PlanCoverage** در صورت نیاز به سقف‌های روزانه/ماهانه
5. **ایجاد Many-to-Many Relationships** در صورت تغییر نیازهای کسب‌وکار

---

**تاریخ بررسی**: 2025-01-17  
**نسخه گزارش**: 1.0.0  
**وضعیت**: ⚠️ شکاف‌های شناسایی شده - نیاز به اقدام

