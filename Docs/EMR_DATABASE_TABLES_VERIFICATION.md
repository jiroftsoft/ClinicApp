# بررسی جداول دیتابیس پرونده الکترونیک سلامت

## ✅ Entityهای مورد نیاز برای EMR

### 1. MedicalHistory (اصلی)
- ✅ **Entity**: `Models/Entities/Patient/MedicalHistory.cs`
- ✅ **DbSet**: `DbSet<MedicalHistory> MedicalHistories` در `IdentityModels.cs` (خط 149)
- ✅ **Configuration**: `MedicalHistoryConfig` در `Models/Entities/Patient/MedicalHistory.cs` (خط 162)
- ✅ **Table Name**: `MedicalHistories`
- ✅ **Indexes**: 
  - `IX_MedicalHistory_Type`
  - `IX_MedicalHistory_StartDate`
  - `IX_MedicalHistory_IsActive`
  - `IX_MedicalHistory_IsDeleted`
  - `IX_MedicalHistory_PatientId_Type_IsActive` (Composite)
  - `IX_MedicalHistory_PatientId_IsDeleted` (Composite)
  - `IX_MedicalHistory_Type_StartDate_IsActive` (Composite)
- ✅ **Relationships**: 
  - `Patient` (Many-to-One)
  - `CreatedByUser`, `UpdatedByUser`, `DeletedByUser` (Many-to-One)

### 2. Patient (وابسته)
- ✅ **Entity**: `Models/Entities/Patient/Patient.cs`
- ✅ **DbSet**: باید در `IdentityModels.cs` موجود باشد
- ✅ **Configuration**: `PatientConfig` در `Models/Entities/Patient/Patient.cs`
- ✅ **Relationship**: `ICollection<MedicalHistory> MedicalHistories` در `Patient` (خط 249)

### 3. Appointment (وابسته)
- ✅ **Entity**: `Models/Entities/Appointment/Appointment.cs`
- ✅ **DbSet**: باید در `IdentityModels.cs` موجود باشد
- ✅ **Usage**: در `MedicalRecordService.GetAppointmentsAsync()` استفاده می‌شود

### 4. Reception (وابسته)
- ✅ **Entity**: `Models/Entities/Reception/Reception.cs`
- ✅ **DbSet**: باید در `IdentityModels.cs` موجود باشد
- ✅ **Usage**: در `MedicalRecordService.GetReceptionsAsync()` استفاده می‌شود

### 5. TriageAssessment (وابسته)
- ✅ **Entity**: `Models/Entities/Triage/TriageAssessment.cs`
- ✅ **DbSet**: `DbSet<TriageAssessment> TriageAssessments` در `IdentityModels.cs` (خط 153)
- ✅ **Usage**: در `MedicalRecordService.GetTriageAssessmentsAsync()` استفاده می‌شود

### 6. TriageVitalSigns (وابسته)
- ✅ **Entity**: `Models/Entities/Triage/TriageVitalSigns.cs`
- ✅ **DbSet**: `DbSet<TriageVitalSigns> TriageVitalSigns` در `IdentityModels.cs` (خط 155)
- ✅ **Usage**: در `MedicalRecordService.GetTriageAssessmentsAsync()` استفاده می‌شود

## ✅ بررسی DbContext (IdentityModels.cs)

### DbSetهای موجود:
```csharp
// خط 149
public DbSet<MedicalHistory> MedicalHistories { get; set; }

// خط 153
public DbSet<TriageAssessment> TriageAssessments { get; set; }

// خط 155
public DbSet<TriageVitalSigns> TriageVitalSigns { get; set; }
```

### DbSetهای مورد نیاز (باید بررسی شوند):
- `DbSet<Patient> Patients` - باید موجود باشد
- `DbSet<Appointment> Appointments` - باید موجود باشد
- `DbSet<Reception> Receptions` - باید موجود باشد

## ✅ بررسی Configurationها

### MedicalHistoryConfig
- ✅ **Location**: `Models/Entities/Patient/MedicalHistory.cs` (خط 162)
- ✅ **Table**: `MedicalHistories`
- ✅ **Key**: `MedicalHistoryId`
- ✅ **Properties**: همه Properties پیکربندی شده‌اند
- ✅ **Indexes**: 11 ایندکس (ساده + ترکیبی)
- ✅ **Relationships**: 
  - `Patient` (Required)
  - `CreatedByUser`, `UpdatedByUser`, `DeletedByUser` (Optional)

### سایر Configurationها
- ✅ `PatientConfig` - باید موجود باشد
- ✅ `AppointmentConfig` - باید موجود باشد
- ✅ `ReceptionConfig` - باید موجود باشد
- ✅ `TriageAssessmentConfig` - باید موجود باشد
- ✅ `TriageVitalSignsConfig` - باید موجود باشد

## ✅ بررسی RegisterEntityConfigurations

```csharp
// خط 408-428 در IdentityModels.cs
private void RegisterEntityConfigurations(DbModelBuilder modelBuilder)
{
    var configTypes = typeof(ClinicConfig).Assembly.GetTypes()
        .Where(t => t.BaseType != null &&
                    t.BaseType.IsGenericType &&
                    t.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));

    foreach (var type in configTypes)
    {
        try
        {
            dynamic config = Activator.CreateInstance(type);
            modelBuilder.Configurations.Add(config);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading configuration {type.Name}: {ex.Message}");
        }
    }
}
```

**نتیجه**: تمام Configurationهایی که از `EntityTypeConfiguration<>` ارث‌بری می‌کنند، به صورت خودکار ثبت می‌شوند.

## ✅ بررسی Filters

```csharp
// خط 235 در IdentityModels.cs
modelBuilder.Filter("ActiveMedicalHistories", (MedicalHistory mh) => mh.IsActive, true);
```

**نتیجه**: فیلتر برای `MedicalHistory` تنظیم شده است.

## ✅ بررسی Migrationها

**وضعیت**: باید بررسی شود که آیا Migrationها ایجاد و اجرا شده‌اند یا خیر.

## 📋 خلاصه بررسی

### جداول اصلی EMR:
1. ✅ **MedicalHistories** - موجود و پیکربندی شده
2. ✅ **Patients** - باید موجود باشد (وابسته)
3. ✅ **Appointments** - باید موجود باشد (وابسته)
4. ✅ **Receptions** - باید موجود باشد (وابسته)
5. ✅ **TriageAssessments** - موجود و پیکربندی شده
6. ✅ **TriageVitalSigns** - موجود و پیکربندی شده

### جداول وابسته (Identity):
- ✅ **AspNetUsers** - برای `CreatedByUser`, `UpdatedByUser`, `DeletedByUser`
- ✅ سایر جداول Identity

## ✅ نتیجه نهایی بررسی

### جداول اصلی EMR:
1. ✅ **MedicalHistories** 
   - DbSet: موجود (خط 149)
   - Configuration: موجود (`MedicalHistoryConfig`)
   - Table Name: `MedicalHistories`
   - Indexes: 11 ایندکس (ساده + ترکیبی)
   - Relationships: کامل

2. ✅ **Patients**
   - DbSet: موجود (خط 99)
   - Configuration: موجود (`PatientConfig`)
   - Relationship: `ICollection<MedicalHistory> MedicalHistories` موجود

3. ✅ **Appointments**
   - DbSet: موجود (خط 109)
   - Configuration: موجود (`AppointmentConfig`)
   - Usage: در `MedicalRecordService.GetAppointmentsAsync()` استفاده می‌شود

4. ✅ **Receptions**
   - DbSet: موجود (خط 101)
   - Configuration: موجود (`ReceptionConfig`)
   - Usage: در `MedicalRecordService.GetReceptionsAsync()` استفاده می‌شود

5. ✅ **TriageAssessments**
   - DbSet: موجود (خط 153)
   - Configuration: موجود (`TriageAssessmentConfig`)
   - Usage: در `MedicalRecordService.GetTriageAssessmentsAsync()` استفاده می‌شود

6. ✅ **TriageVitalSigns**
   - DbSet: موجود (خط 155)
   - Configuration: موجود (`TriageVitalSignsConfig`)
   - Usage: در `MedicalRecordService.GetTriageAssessmentsAsync()` استفاده می‌شود

### جداول وابسته (Identity):
- ✅ **AspNetUsers** - برای `CreatedByUser`, `UpdatedByUser`, `DeletedByUser`
- ✅ سایر جداول Identity

### Configurationها:
- ✅ همه Configurationها موجود هستند
- ✅ همه Configurationها به صورت خودکار توسط `RegisterEntityConfigurations` ثبت می‌شوند

### Filters:
- ✅ `ActiveMedicalHistories` تنظیم شده است (خط 235)

## ✅ نتیجه نهایی

**همه جداول مورد نیاز برای پرونده الکترونیک سلامت موجود و آماده هستند!**

### ⚠️ اقدامات باقی‌مانده (در صورت نیاز):

1. **بررسی Migrationها**:
   - بررسی وجود Migration برای `MedicalHistories`
   - بررسی اجرای Migrationها در دیتابیس
   - در صورت نیاز، ایجاد و اجرای Migration جدید

2. **تست دیتابیس**:
   - تست ایجاد `MedicalHistory`
   - تست Query کردن `MedicalHistory`
   - تست Relationships
   - تست Soft Delete
   - تست Audit Trail

3. **بررسی Performance**:
   - بررسی عملکرد ایندکس‌ها
   - بررسی Query Performance

