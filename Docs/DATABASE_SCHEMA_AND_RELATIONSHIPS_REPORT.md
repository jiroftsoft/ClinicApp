# 📊 گزارش جامع ساختار دیتابیس و روابط بین موجودیت‌ها

**تاریخ بررسی:** 2025-01-27  
**نسخه:** 1.0.0  
**وضعیت:** ✅ بررسی کامل انجام شد  
**هدف:** مستندسازی کامل ساختار دیتابیس، موجودیت‌ها و روابط بین آن‌ها

---

## ✅ خلاصه اجرایی

این گزارش ساختار کامل دیتابیس ClinicApp را شامل می‌شود:
- **48 موجودیت اصلی** در 9 دسته‌بندی
- **روابط One-to-Many, Many-to-Many, One-to-One**
- **الگوهای طراحی:** ISoftDelete, ITrackable, AuditableEntity
- **ایندکس‌های بهینه‌سازی شده** برای Performance
- **Decimal Precision:** تمام مبالغ به ریال (decimal(18,0))

---

## 📋 ساختار کلی موجودیت‌ها

### 1️⃣ موجودیت‌های کلینیک (Clinic)

#### **Clinic** (کلینیک)
```csharp
- ClinicId (PK)
- Name
- Address
- PhoneNumber
- IsActive
- ISoftDelete, ITrackable
```

**روابط:**
- One-to-Many → Departments
- One-to-Many → Doctors (Optional)

#### **Department** (دپارتمان)
```csharp
- DepartmentId (PK)
- Name
- Code
- ClinicId (FK → Clinic)
- IsActive
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → Clinic
- Many-to-Many → Doctors (via DoctorDepartment)
- One-to-Many → ServiceCategories

#### **Service** (خدمت)
```csharp
- ServiceId (PK)
- Title
- ServiceCode (Unique)
- Price (decimal(18,0) - ریال)
- ServiceCategoryId (FK → ServiceCategory)
- IsHashtagged
- GroupCode (1-7)
- AgeMin, AgeMax, GenderLimit (Eligibility)
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → ServiceCategory
- One-to-Many → ReceptionItems
- One-to-Many → InsuranceTariffs
- One-to-Many → ServiceComponents
- Many-to-Many → Departments (via SharedService)

#### **ServiceCategory** (دسته‌بندی خدمات)
```csharp
- ServiceCategoryId (PK)
- Name
- DepartmentId (FK → Department)
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → Department
- One-to-Many → Services
- Many-to-Many → Doctors (via DoctorServiceCategory)

---

### 2️⃣ موجودیت‌های بیمار (Patient)

#### **Patient** (بیمار)
```csharp
- PatientId (PK)
- NationalCode (Unique, 10 digits)
- FirstName, LastName, FatherName
- BirthDate
- Gender
- PhoneNumber, Email, Address
- BloodType, Allergies, ChronicDiseases
- EmergencyContactName, EmergencyContactPhone
- ApplicationUserId (FK → ApplicationUser)
- ISoftDelete, ITrackable
```

**روابط:**
- One-to-One → ApplicationUser
- One-to-Many → Receptions
- One-to-Many → Appointments
- One-to-Many → PatientInsurances
- One-to-Many → MedicalHistories
- One-to-Many → TriageAssessments
- One-to-Many → TriageQueues

#### **PatientInsurance** (بیمه بیمار)
```csharp
- PatientInsuranceId (PK)
- PatientId (FK → Patient)
- InsurancePlanId (FK → InsurancePlan)
- InsuranceProviderId (FK → InsuranceProvider)
- PolicyNumber
- CardNumber
- StartDate, EndDate
- IsPrimary, IsActive
- Priority (InsurancePriority enum)
- SupplementaryInsuranceProviderId (Optional)
- SupplementaryInsurancePlanId (Optional)
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → Patient
- Many-to-One → InsurancePlan (Base)
- Many-to-One → InsurancePlan (Supplementary - Optional)
- Many-to-One → InsuranceProvider (Base)
- Many-to-One → InsuranceProvider (Supplementary - Optional)
- One-to-Many → InsuranceCalculations

---

### 3️⃣ موجودیت‌های پزشک (Doctor)

#### **Doctor** (پزشک)
```csharp
- DoctorId (PK)
- FirstName, LastName
- NationalCode, MedicalCouncilCode
- LicenseNumber
- Degree, GraduationYear, University
- Gender, DateOfBirth
- PhoneNumber, Email, Address
- ClinicId (FK → Clinic, Optional)
- IsActive
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → Clinic (Optional)
- One-to-Many → Receptions
- One-to-Many → Appointments
- Many-to-Many → Departments (via DoctorDepartment)
- Many-to-Many → ServiceCategories (via DoctorServiceCategory)
- Many-to-Many → Specializations (via DoctorSpecialization)
- One-to-Many → DoctorSchedules
- One-to-Many → TimeSlots
- One-to-Many → RecommendedTriageAssessments

#### **DoctorDepartment** (رابطه Many-to-Many پزشک-دپارتمان)
```csharp
- DoctorId (PK, FK → Doctor)
- DepartmentId (PK, FK → Department)
- Role
- IsActive
- StartDate, EndDate
- ITrackable (NOT ISoftDelete)
```

**روابط:**
- Many-to-One → Doctor
- Many-to-One → Department

#### **DoctorServiceCategory** (رابطه Many-to-Many پزشک-دسته خدمات)
```csharp
- DoctorId (PK, FK → Doctor)
- ServiceCategoryId (PK, FK → ServiceCategory)
- AuthorizationLevel
- IsActive
- GrantedDate, ExpiryDate
- CertificateNumber
- ITrackable, ISoftDelete
```

**روابط:**
- Many-to-One → Doctor
- Many-to-One → ServiceCategory

---

### 4️⃣ موجودیت‌های پذیرش (Reception)

#### **Reception** (پذیرش)
```csharp
- ReceptionId (PK)
- ReceptionNo (شماره پذیرش رسمی)
- ClinicId (FK → Clinic)
- DepartmentId (FK → Department)
- PatientId (FK → Patient)
- DoctorId (FK → Doctor)
- ReceptionDate
- FinancialYear
- Status (ReceptionStatus enum)
- Type (ReceptionType enum)
- Priority (AppointmentPriority enum)
- Gross, BasePay, SuppPay, PatientPay (decimal(18,0))
- TotalAmount, PatientCoPay, InsurerShareAmount
- ActivePatientInsuranceId (FK → PatientInsurance, Optional)
- BasePlanId, SupplementaryPlanId (Optional)
- PaymentMethod
- RowVersion (Concurrency Control)
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → Patient
- Many-to-One → Doctor
- Many-to-One → Clinic
- Many-to-One → Department
- Many-to-One → ActivePatientInsurance (Optional)
- One-to-Many → ReceptionItems (Cascade Delete)
- One-to-Many → Transactions
- One-to-Many → ReceiptPrints
- One-to-Many → InsuranceCalculations

#### **ReceptionItem** (آیتم پذیرش)
```csharp
- ReceptionItemId (PK)
- ReceptionId (FK → Reception)
- ServiceId (FK → Service)
- Quantity
- UnitPrice (decimal(18,0))
- PatientShareAmount (decimal(18,0))
- InsurerShareAmount (decimal(18,0))
- SnapshotJson (Immutable snapshot of calculations)
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → Reception (Cascade Delete)
- Many-to-One → Service

---

### 5️⃣ موجودیت‌های بیمه (Insurance)

#### **InsuranceProvider** (ارائه‌دهنده بیمه)
```csharp
- InsuranceProviderId (PK)
- Name
- Code
- ISoftDelete, ITrackable
```

**روابط:**
- One-to-Many → InsurancePlans
- One-to-Many → PatientInsurances (Base)
- One-to-Many → PatientInsurances (Supplementary)

#### **InsurancePlan** (طرح بیمه)
```csharp
- InsurancePlanId (PK)
- InsuranceProviderId (FK → InsuranceProvider)
- PlanCode
- Name
- CoveragePercent
- Deductible (decimal(18,0))
- ValidFrom, ValidTo
- InsuranceType (Base/Supplementary)
- IsActive
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → InsuranceProvider
- One-to-Many → PlanServices
- One-to-Many → PatientInsurances (Base)
- One-to-Many → PatientInsurances (Supplementary)

#### **PlanService** (خدمات طرح بیمه)
```csharp
- PlanServiceId (PK)
- InsurancePlanId (FK → InsurancePlan)
- ServiceId (FK → Service)
- CoveragePercent
- Deductible
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → InsurancePlan
- Many-to-One → Service

#### **InsuranceTariff** (تعرفه بیمه)
```csharp
- InsuranceTariffId (PK)
- ServiceId (FK → Service)
- InsuranceProviderId (FK → InsuranceProvider)
- TariffCode
- Amount (decimal(18,0))
- ValidFrom, ValidTo
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → Service
- Many-to-One → InsuranceProvider

---

### 6️⃣ موجودیت‌های پرداخت (Payment)

#### **PaymentTransaction** (تراکنش پرداخت)
```csharp
- PaymentTransactionId (PK)
- ReceptionId (FK → Reception)
- PosTerminalId (FK → PosTerminal, Optional)
- PaymentGatewayId (FK → PaymentGateway, Optional)
- OnlinePaymentId (FK → OnlinePayment, Optional)
- CashSessionId (FK → CashSession)
- Amount (decimal(18,0))
- Status (PaymentStatus enum)
- Method (PaymentMethod enum)
- TransactionId, ReferenceCode, ReceiptNo
- IdempotencyKey (برای جلوگیری از تکرار)
- TerminalId, CardLast4
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → Reception
- Many-to-One → PosTerminal (Optional)
- Many-to-One → PaymentGateway (Optional)
- Many-to-One → OnlinePayment (Optional)
- Many-to-One → CashSession

#### **PosTerminal** (دستگاه پوز)
```csharp
- PosTerminalId (PK)
- TerminalId
- ProviderType (PosProviderType enum)
- Protocol (PosProtocol enum)
- ISoftDelete, ITrackable
```

**روابط:**
- One-to-Many → Transactions

#### **CashSession** (شیفت صندوق)
```csharp
- CashSessionId (PK)
- SessionDate
- StartTime, EndTime
- OpeningBalance, ClosingBalance
- ISoftDelete, ITrackable
```

**روابط:**
- One-to-Many → Transactions

---

### 7️⃣ موجودیت‌های تریاژ (Triage)

#### **TriageAssessment** (ارزیابی تریاژ)
```csharp
- TriageAssessmentId (PK)
- PatientId (FK → Patient)
- AssessorId (FK → ApplicationUser)
- RecommendedDepartmentId (FK → Department, Optional)
- RecommendedDoctorId (FK → Doctor, Optional)
- Priority (TriagePriority enum)
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → Patient
- Many-to-One → Assessor (ApplicationUser)
- Many-to-One → RecommendedDepartment (Optional)
- Many-to-One → RecommendedDoctor (Optional)
- One-to-Many → VitalSigns
- One-to-Many → TriageQueues
- Many-to-Many → Protocols

#### **TriageQueue** (صف تریاژ)
```csharp
- TriageQueueId (PK)
- PatientId (FK → Patient)
- TriageAssessmentId (FK → TriageAssessment)
- TargetDepartmentId (FK → Department, Optional)
- TargetDoctorId (FK → Doctor, Optional)
- QueueNumber
- CalledByUserId, CompletedByUserId
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → Patient
- Many-to-One → TriageAssessment
- Many-to-One → TargetDepartment (Optional)
- Many-to-One → TargetDoctor (Optional)

---

### 8️⃣ موجودیت‌های نوبت‌دهی (Appointment)

#### **Appointment** (نوبت)
```csharp
- AppointmentId (PK)
- PatientId (FK → Patient, Optional)
- DoctorId (FK → Doctor)
- AppointmentDate
- Status (AppointmentStatus enum)
- Type (AppointmentType enum)
- Priority (AppointmentPriority enum)
- ISoftDelete, ITrackable
```

**روابط:**
- Many-to-One → Patient (Optional)
- Many-to-One → Doctor

---

### 9️⃣ موجودیت‌های دیگر

#### **ApplicationUser** (کاربر سیستم)
```csharp
- Id (PK, string)
- UserName, Email
- (از Identity Framework)
```

**روابط:**
- One-to-Many → Patients
- One-to-Many → Receptions (CreatedBy, UpdatedBy, DeletedBy)
- One-to-Many → Doctors (CreatedBy, UpdatedBy, DeletedBy)
- و سایر موجودیت‌ها برای Audit

---

## 🔗 نمودار روابط کلیدی

### روابط اصلی Reception:

```
Reception
├── Patient (Many-to-One, Required)
├── Doctor (Many-to-One, Required)
├── Clinic (Many-to-One, Required)
├── Department (Many-to-One, Required)
├── ActivePatientInsurance (Many-to-One, Optional)
├── ReceptionItems (One-to-Many, Cascade Delete)
├── Transactions (One-to-Many)
├── ReceiptPrints (One-to-Many)
└── InsuranceCalculations (One-to-Many)
```

### روابط اصلی Patient:

```
Patient
├── ApplicationUser (One-to-One, Required)
├── Receptions (One-to-Many)
├── Appointments (One-to-Many)
├── PatientInsurances (One-to-Many)
├── MedicalHistories (One-to-Many)
├── TriageAssessments (One-to-Many)
└── TriageQueues (One-to-Many)
```

### روابط اصلی Doctor:

```
Doctor
├── Clinic (Many-to-One, Optional)
├── Receptions (One-to-Many)
├── Appointments (One-to-Many)
├── DoctorDepartments (Many-to-Many via DoctorDepartment)
├── DoctorServiceCategories (Many-to-Many via DoctorServiceCategory)
├── DoctorSpecializations (Many-to-Many via DoctorSpecialization)
├── DoctorSchedules (One-to-Many)
└── TimeSlots (One-to-Many)
```

### روابط اصلی Service:

```
Service
├── ServiceCategory (Many-to-One, Required)
├── ReceptionItems (One-to-Many)
├── InsuranceTariffs (One-to-Many)
├── ServiceComponents (One-to-Many)
└── SharedServices (Many-to-Many via SharedService)
```

### روابط Many-to-Many:

1. **Doctor ↔ Department** (via DoctorDepartment)
   - Composite Key: (DoctorId, DepartmentId)
   - Fields: Role, IsActive, StartDate, EndDate

2. **Doctor ↔ ServiceCategory** (via DoctorServiceCategory)
   - Composite Key: (DoctorId, ServiceCategoryId)
   - Fields: AuthorizationLevel, IsActive, GrantedDate, ExpiryDate, CertificateNumber

3. **Doctor ↔ Specialization** (via DoctorSpecialization)
   - Composite Key: (DoctorId, SpecializationId)

4. **Service ↔ Department** (via SharedService)
   - برای خدمات مشترک بین دپارتمان‌ها

---

## 🎯 الگوهای طراحی استفاده شده

### 1️⃣ ISoftDelete (حذف نرم)
```csharp
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string DeletedByUserId { get; set; }
    ApplicationUser DeletedByUser { get; set; }
}
```

**موجودیت‌های دارای ISoftDelete:**
- ✅ Clinic, Department, Service, ServiceCategory
- ✅ Patient, PatientInsurance
- ✅ Doctor
- ✅ Reception, ReceptionItem
- ✅ InsuranceProvider, InsurancePlan, InsuranceTariff
- ✅ PaymentTransaction, PosTerminal, CashSession
- ✅ TriageAssessment, TriageQueue
- ✅ Appointment

**موجودیت‌های بدون ISoftDelete:**
- ❌ DoctorDepartment (فقط ITrackable)
- ❌ DoctorServiceCategory (دارای ISoftDelete)

### 2️⃣ ITrackable (ردیابی تغییرات)
```csharp
public interface ITrackable
{
    DateTime CreatedAt { get; set; }
    string CreatedByUserId { get; set; }
    ApplicationUser CreatedByUser { get; set; }
    DateTime? UpdatedAt { get; set; }
    string UpdatedByUserId { get; set; }
    ApplicationUser UpdatedByUser { get; set; }
}
```

**تمام موجودیت‌ها دارای ITrackable هستند.**

### 3️⃣ AuditableEntity (کلاس پایه)
```csharp
public abstract class AuditableEntity : ISoftDelete, ITrackable
{
    // پیاده‌سازی مشترک
}
```

**موجودیت‌های استفاده کننده:**
- Clinic

---

## 📊 ایندکس‌های بهینه‌سازی شده

### ایندکس‌های تک فیلدی:

**Reception:**
- `IX_Reception_ReceptionDate`
- `IX_Reception_Status`
- `IX_Reception_PatientId`
- `IX_Reception_DoctorId`
- `IX_Reception_ClinicId`
- `IX_Reception_DepartmentId`
- `IX_Reception_FinancialYear`
- `IX_Reception_IsDeleted`

**Patient:**
- `IX_Patient_NationalCode` (Unique)
- `IX_Patient_FirstName`
- `IX_Patient_LastName`
- `IX_Patient_PhoneNumber`
- `IX_Patient_Email`
- `IX_Patient_IsDeleted`

**Doctor:**
- `IX_Doctor_FirstName`
- `IX_Doctor_LastName`
- `IX_Doctor_NationalCode`
- `IX_Doctor_MedicalCouncilCode`
- `IX_Doctor_IsActive`
- `IX_Doctor_IsDeleted`

**Service:**
- `IX_Service_ServiceCode` (Unique)
- `IX_Service_Title`
- `IX_Service_Price`
- `IX_Service_IsHashtagged`
- `IX_Service_IsDeleted`

### ایندکس‌های ترکیبی:

**Reception:**
- `IX_Reception_PatientId_Date_Status`
- `IX_Reception_DoctorId_Date_Status`

**Patient:**
- `IX_Patient_LastName_FirstName`
- `IX_Patient_IsDeleted_CreatedAt`
- `IX_Patient_NationalCode_IsDeleted`
- `IX_Patient_PhoneNumber_IsDeleted`
- `IX_Patient_FirstName_LastName_IsDeleted`

**Doctor:**
- `IX_Doctor_LastName_FirstName`
- `IX_Doctor_ClinicId_IsActive_IsDeleted`
- `IX_Doctor_University_IsActive_IsDeleted`

**PaymentTransaction:**
- `IX_PaymentTransaction_CashSessionId_Status_CreatedAt`
- `IX_PaymentTransaction_ReceptionId_Status`

**PatientInsurance:**
- `IX_PatientInsurance_Patient_IsActive_IsDeleted`
- `IX_PatientInsurance_Patient_Priority_IsActive`
- `IX_PatientInsurance_PatientId_IsActive_Priority_IsDeleted`
- `IX_PatientInsurance_StartDate_EndDate_IsActive_IsDeleted`

**DoctorDepartment:**
- `IX_DoctorDepartment_DoctorId_IsActive`
- `IX_DoctorDepartment_DepartmentId_IsActive`
- `IX_DoctorDepartment_DoctorId_DepartmentId_IsActive`

**DoctorServiceCategory:**
- `IX_DoctorServiceCategory_DoctorId_IsActive`
- `IX_DoctorServiceCategory_ServiceCategoryId_IsActive`
- `IX_DoctorServiceCategory_DoctorId_ServiceCategoryId_IsActive`

---

## 💰 مدیریت مبالغ مالی

### Decimal Precision:

**تمام مبالغ به ریال (بدون اعشار):**
- `decimal(18, 0)` در دیتابیس
- استفاده از `HasPrecision(18, 0)` در Fluent API

**موجودیت‌های دارای مبلغ:**
- Reception: Gross, BasePay, SuppPay, PatientPay, TotalAmount, PatientCoPay, InsurerShareAmount
- ReceptionItem: UnitPrice, PatientShareAmount, InsurerShareAmount
- Service: Price
- InsurancePlan: Deductible
- InsuranceTariff: Amount
- PaymentTransaction: Amount
- CashSession: OpeningBalance, ClosingBalance

---

## 🔐 Foreign Key Constraints

### Cascade Delete:

**Cascade Delete = True:**
- Reception → ReceptionItems (حذف Reception باعث حذف Items می‌شود)

**Cascade Delete = False:**
- تمام روابط دیگر (برای حفظ یکپارچگی داده‌ها)

---

## 📈 خلاصه آمار

| دسته‌بندی | تعداد موجودیت | موجودیت‌های کلیدی |
|-----------|---------------|-------------------|
| **Clinic** | 8 | Clinic, Department, Service, ServiceCategory |
| **Patient** | 3 | Patient, PatientInsurance, MedicalHistory |
| **Doctor** | 12 | Doctor, DoctorDepartment, DoctorServiceCategory, DoctorSpecialization |
| **Reception** | 2 | Reception, ReceptionItem |
| **Insurance** | 7 | InsuranceProvider, InsurancePlan, InsuranceTariff, PlanService |
| **Payment** | 5 | PaymentTransaction, PosTerminal, CashSession, PaymentGateway, OnlinePayment |
| **Triage** | 5 | TriageAssessment, TriageQueue, TriageProtocol, TriageVitalSigns, TriageReassessment |
| **Appointment** | 2 | Appointment, AppointmentSlot |
| **Other** | 4 | ApplicationUser, NotificationHistory, ReceiptPrint, Report |
| **جمع کل** | **48** | |

---

## ✅ نتیجه‌گیری

### نقاط قوت:

1. ✅ **ساختار منظم:** موجودیت‌ها به درستی دسته‌بندی شده‌اند
2. ✅ **روابط صحیح:** Foreign Keys و Navigation Properties به درستی تعریف شده‌اند
3. ✅ **Audit Trail:** تمام موجودیت‌ها دارای ITrackable هستند
4. ✅ **Soft Delete:** اکثر موجودیت‌ها دارای ISoftDelete هستند
5. ✅ **Performance:** ایندکس‌های بهینه‌سازی شده برای Query های رایج
6. ✅ **Data Integrity:** Decimal Precision صحیح برای مبالغ مالی
7. ✅ **Concurrency:** استفاده از RowVersion در Reception

### توصیه‌ها:

1. ⚠️ **Documentation:** برخی موجودیت‌ها نیاز به XML Documentation بیشتر دارند
2. ⚠️ **Testing:** نیاز به Unit Tests برای روابط و Constraints
3. ⚠️ **Migration:** بررسی Migration ها برای اطمینان از صحت ساختار

---

**تهیه شده توسط:** AI Assistant (Senior .NET Architect & Healthcare Systems Specialist)  
**تاریخ:** 2025-01-27  
**نسخه گزارش:** 1.0.0

