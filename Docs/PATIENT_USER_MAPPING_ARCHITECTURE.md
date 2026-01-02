# 📋 Patient ↔ User Mapping Architecture

**تاریخ:** 2026-01-02  
**نویسنده:** AI Assistant  
**وضعیت:** ✅ Implemented & Verified

---

## 📖 **خلاصه**

این سیستم یک ارتباط **Optional 1:1** بین جداول `Patients` و `AspNetUsers` دارد.

```
AspNetUsers (1) ←→ (0..1) Patients
```

**قاعده:**
- ✅ یک `Patient` می‌تواند بدون `User` account وجود داشته باشد (`ApplicationUserId = NULL`)
- ✅ یک `Patient` می‌تواند به یک `User` متصل باشد (`ApplicationUserId = User.Id`)
- ❌ یک `User` نمی‌تواند به بیش از یک `Patient` متصل باشد (1:1 relationship)

---

## 🏗️ **معماری سیستم**

### **1️⃣ جدول Patients**

```sql
CREATE TABLE Patients (
    PatientId INT PRIMARY KEY IDENTITY,
    NationalCode NVARCHAR(20) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    ApplicationUserId NVARCHAR(128) NULL,  -- ✅ NULLABLE
    -- ... other fields
    CONSTRAINT FK_Patients_AspNetUsers 
        FOREIGN KEY (ApplicationUserId) 
        REFERENCES AspNetUsers(Id) 
        ON DELETE NO ACTION
);
```

**نکات مهم:**
- `ApplicationUserId` **NULLABLE** است (تغییر داده شده در Migration 202601020001)
- `NationalCode` **UNIQUE** است
- یک Patient می‌تواند بدون User account وجود داشته باشد

---

### **2️⃣ جدول AspNetUsers**

```sql
CREATE TABLE AspNetUsers (
    Id NVARCHAR(128) PRIMARY KEY,
    UserName NVARCHAR(256) NOT NULL UNIQUE,  -- = NationalCode
    NationalCode NVARCHAR(20) NOT NULL,
    PhoneNumber NVARCHAR(MAX),
    -- ... other Identity fields
);
```

**نکات مهم:**
- `UserName` = `NationalCode` (در این سیستم)
- یک User می‌تواند چندین Patient داشته باشد (1:N)، اما **در عمل** فقط باید یک Patient داشته باشد

---

## 🔄 **سناریوهای مختلف**

### **Scenario 1: منشی بیمار جدید پذیرش می‌کند**

**Flow:**
```
1. منشی اطلاعات بیمار را وارد می‌کند (NationalCode، Name، Phone، etc.)
2. PatientService.CreatePatientAsync() فراخوانی می‌شود
3. بررسی می‌شود که Patient با این NationalCode وجود ندارد
4. Patient جدید ایجاد می‌شود با ApplicationUserId = NULL
5. ✅ Patient ذخیره می‌شود (بدون User account)
```

**⚠️ مشکل فعلی:**
```csharp
// در Services/PatientService.cs خط 1074-1209
// CreatePatientAsync() همیشه User هم ایجاد می‌کند!
// این منطقی نیست برای بیماران حضوری
```

**راه‌حل:**
```csharp
// ✅ Option 1: Parameter اضافه کنید
CreatePatientAsync(model, createUserAccount: false)

// ✅ Option 2: دو Method جداگانه
CreatePatientWithoutUserAsync(model)  // برای منشی
CreatePatientWithUserAsync(model)     // برای ثبت‌نام سایت
```

---

### **Scenario 2: کاربر جدید از سایت ثبت‌نام می‌کند**

#### **2a: Patient با این NationalCode وجود ندارد**

**Flow:**
```
1. کاربر NationalCode و شماره موبایل را وارد می‌کند
2. PatientService.RegisterPatientAsync() فراخوانی می‌شود
3. بررسی می‌شود که Patient با این NationalCode وجود ندارد
4. User جدید ایجاد می‌شود در AspNetUsers
5. Patient جدید ایجاد می‌شود با ApplicationUserId = User.Id
6. ✅ هم User و هم Patient ذخیره می‌شوند (در یک Transaction)
```

**کد:**
```csharp
// Services/PatientService.cs خط 1074-1226
var newUser = new ApplicationUser { ... };
var patient = new Models.Entities.Patient.Patient { ... };

await _userManager.CreateAsync(newUser);
await _userManager.AddToRoleAsync(newUser.Id, AppRoles.Patient);

patient.ApplicationUserId = newUser.Id;  // ✅ Link Patient to User
_context.Patients.Add(patient);
await _context.SaveChangesAsync();
```

---

#### **2b: Patient با این NationalCode وجود دارد (بدون User)**

**Flow:**
```
1. کاربر NationalCode و شماره موبایل را وارد می‌کند
2. PatientService.RegisterPatientAsync() فراخوانی می‌شود
3. بررسی می‌شود که Patient با این NationalCode وجود دارد
4. بررسی می‌شود که Patient.ApplicationUserId = NULL (User ندارد)
5. User جدید ایجاد می‌شود در AspNetUsers
6. Patient.ApplicationUserId = User.Id تنظیم می‌شود
7. ✅ Patient موجود به User جدید متصل می‌شود
```

**کد:**
```csharp
// Services/PatientService.cs خط 376-429
var patientByNationalCode = await GetPatientByNationalCodeAsync(model.NationalCode);

if (patientByNationalCode != null && 
    string.IsNullOrEmpty(patientByNationalCode.ApplicationUserId))
{
    // Patient موجود است اما User ندارد
    var newUserForPatient = new ApplicationUser { ... };
    
    await _userManager.CreateAsync(newUserForPatient);
    await _userManager.AddToRoleAsync(newUserForPatient.Id, AppRoles.Patient);
    
    // ✅ Link existing Patient to new User
    patientByNationalCode.ApplicationUserId = newUserForPatient.Id;
    await _context.SaveChangesAsync();
}
```

---

### **Scenario 3: کاربر از سایت لاگین می‌کند**

**Flow:**
```
1. کاربر NationalCode را وارد می‌کند
2. OTP به شماره موبایل ارسال می‌شود
3. کاربر OTP را وارد می‌کند
4. AuthService.VerifyLoginOtpAsync() فراخوانی می‌شود
5. User authenticated می‌شود
6. BasePatientController.GetCurrentPatientIdAsync() فراخوانی می‌شود
7. Patient.ApplicationUserId == User.Id بررسی می‌شود
8. ✅ PatientId برگردانده می‌شود
```

**کد:**
```csharp
// Areas/Patient/Controllers/Base/BasePatientController.cs
protected async Task<int?> GetCurrentPatientIdAsync()
{
    var userId = User.Identity.GetUserId();
    
    var patient = await _context.Patients
        .Where(p => p.ApplicationUserId == userId && !p.IsDeleted)
        .Select(p => new { p.PatientId })
        .FirstOrDefaultAsync();
    
    return patient?.PatientId;
}
```

---

## 🚨 **مشکل قبلی و راه‌حل**

### **Problem: 7,108 بیمار به یک User نادرست متصل بودند**

**Root Cause:**
```sql
-- در Seed Data / Migration قدیمی:
-- همه Patients به یک ApplicationUserId default متصل شده بودند
-- چون ستون ApplicationUserId NOT NULL بود

UPDATE Patients
SET ApplicationUserId = 'ba1140f4-e1f0-43af-8387-ab4ea7e9f9c2'  -- ❌ Default User
WHERE ApplicationUserId IS NULL;
```

**نتیجه:**
```
User "3020094925" → 7,108 Patients  ❌
```

این یعنی:
- ❌ وقتی User "3020094925" login می‌کند، سیستم نمی‌تواند Patient صحیح او را تشخیص دهد
- ❌ این 7,108 بیمار نمی‌توانند از Dashboard استفاده کنند
- ❌ منطق User ↔ Patient کاملاً خراب است

---

### **Solution: ApplicationUserId را NULL کردیم**

**Migration Script:** `Migrations/202601020001_Make_Patient_ApplicationUserId_Nullable.sql`

```sql
-- ✅ Step 1: Make column NULLABLE
ALTER TABLE Patients 
ALTER COLUMN ApplicationUserId NVARCHAR(128) NULL;

-- ✅ Step 2: Unlink patients from incorrect User
UPDATE Patients 
SET ApplicationUserId = NULL
WHERE ApplicationUserId = 'ba1140f4-e1f0-43af-8387-ab4ea7e9f9c2' 
  AND NationalCode != '3020094925';

-- Result: 7,107 rows updated
```

**Entity Framework:**
```csharp
// Models/Entities/Patient/Patient.cs
HasOptional(p => p.ApplicationUser)  // ✅ Changed from HasRequired
    .WithMany(u => u.Patients)
    .HasForeignKey(p => p.ApplicationUserId)
    .WillCascadeOnDelete(false);
```

**نتیجه:**
```
✅ 7,107 Patients → ApplicationUserId = NULL (می‌توانند بعداً ثبت‌نام کنند)
✅ 7 Patients → ApplicationUserId = User.Id (دارای User account)
✅ 0 incorrect mappings
```

---

## 📊 **وضعیت فعلی Database**

```sql
SELECT 
    'Total Patients' AS Metric,
    COUNT(*) AS Count 
FROM Patients 
WHERE IsDeleted = 0;
-- Result: 7,114

SELECT 
    'Patients WITH User' AS Metric,
    COUNT(*) AS Count 
FROM Patients 
WHERE ApplicationUserId IS NOT NULL AND IsDeleted = 0;
-- Result: 7

SELECT 
    'Patients WITHOUT User' AS Metric,
    COUNT(*) AS Count 
FROM Patients 
WHERE ApplicationUserId IS NULL AND IsDeleted = 0;
-- Result: 7,107
```

---

## ✅ **بهترین روش‌ها (Best Practices)**

### **1️⃣ Patient Creation (منشی)**

```csharp
// ✅ DO: فقط Patient ایجاد کنید (بدون User)
var patient = new Patient
{
    NationalCode = "1234567890",
    FirstName = "علی",
    LastName = "محمدی",
    PhoneNumber = "+989123456789",
    ApplicationUserId = null  // ✅ No User account
};
_context.Patients.Add(patient);
await _context.SaveChangesAsync();
```

```csharp
// ❌ DON'T: User برای همه بیماران ایجاد نکنید
// همه بیماران نمی‌خواهند به سایت دسترسی داشته باشند
```

---

### **2️⃣ User Registration (سایت)**

```csharp
// ✅ DO: ابتدا بررسی کنید که Patient با این NationalCode وجود دارد یا خیر
var existingPatient = await _context.Patients
    .FirstOrDefaultAsync(p => p.NationalCode == nationalCode && !p.IsDeleted);

if (existingPatient != null && existingPatient.ApplicationUserId == null)
{
    // ✅ Patient موجود است، فقط User ایجاد کنید و به Patient متصل کنید
    var newUser = new ApplicationUser { ... };
    await _userManager.CreateAsync(newUser);
    
    existingPatient.ApplicationUserId = newUser.Id;
    await _context.SaveChangesAsync();
}
else
{
    // ✅ Patient موجود نیست، هم User و هم Patient ایجاد کنید
    var newUser = new ApplicationUser { ... };
    var newPatient = new Patient { ApplicationUserId = newUser.Id };
    // ...
}
```

---

### **3️⃣ Querying Patients**

```csharp
// ✅ DO: همیشه IsDeleted = 0 را check کنید
var patients = await _context.Patients
    .Where(p => !p.IsDeleted)
    .ToListAsync();

// ✅ DO: برای بیماران با User، از Include استفاده کنید
var patientsWithUsers = await _context.Patients
    .Include(p => p.ApplicationUser)
    .Where(p => !p.IsDeleted && p.ApplicationUserId != null)
    .ToListAsync();

// ✅ DO: برای بیماران بدون User
var patientsWithoutUsers = await _context.Patients
    .Where(p => !p.IsDeleted && p.ApplicationUserId == null)
    .ToListAsync();
```

---

## 🧪 **تست‌ها**

### **Test 1: کاربر 5369873054**

```sql
SELECT 
    u.UserName,
    p.PatientId,
    p.NationalCode,
    CASE 
        WHEN p.NationalCode = u.UserName THEN 'CORRECT' 
        ELSE 'MISMATCH' 
    END AS Status
FROM AspNetUsers u
INNER JOIN Patients p ON u.Id = p.ApplicationUserId
WHERE u.UserName = '5369873054';

-- Expected Result:
-- UserName: 5369873054
-- PatientId: 8128
-- NationalCode: 5369873054
-- Status: CORRECT ✅
```

---

### **Test 2: No Incorrect Mappings**

```sql
SELECT COUNT(*) AS IncorrectMappings
FROM Patients p
INNER JOIN AspNetUsers u ON p.ApplicationUserId = u.Id
WHERE p.NationalCode != u.UserName
  AND p.IsDeleted = 0
  AND u.IsDeleted = 0;

-- Expected Result: 0 ✅
```

---

## 📞 **پشتیبانی**

در صورت بروز مشکل:
1. بررسی کنید که `ApplicationUserId` NULLABLE است
2. بررسی کنید که هیچ Patient به User نادرست متصل نیست
3. بررسی کنید که Entity Framework Configuration از `HasOptional` استفاده می‌کند
4. لاگ‌های Serilog را بررسی کنید (`logs/`)

---

**تاریخ آخرین به‌روزرسانی:** 2026-01-02  
**نسخه:** 1.0  
**نویسنده:** ClinicApp Development Team

