# 📊 گزارش وضعیت اتصال به دیتابیس ClinicApp

**تاریخ اتصال:** 2025-01-27  
**وضعیت:** ✅ **اتصال موفق**  
**روش اتصال:** SQL Server Command Line (sqlcmd) با Windows Authentication

---

## ✅ اطلاعات اتصال

### Connection Details:
- **Server:** DESKTOP-HGABNCN (Local SQL Server)
- **Database:** ClinicDb
- **Authentication:** Windows Authentication (Integrated Security)
- **Connection String:** `Data Source=.;Initial Catalog=ClinicDb;Integrated Security=True;MultipleActiveResultSets=true;Persist Security Info=True;`

---

## 📊 آمار کلی دیتابیس

### تعداد جداول:
- **کل جداول:** 58 جدول
- **جداول اصلی:** 58 جدول (بدون جداول سیستم)

### تعداد رکوردها در جداول اصلی:

| جدول | تعداد رکورد | وضعیت |
|------|-------------|-------|
| **Patients** | 7,112 | ✅ داده موجود |
| **Departments** | 40 | ✅ داده موجود |
| **Services** | 27 | ✅ داده موجود |
| **InsuranceProviders** | 14 | ✅ داده موجود |
| **InsurancePlans** | 14 | ✅ داده موجود |
| **Clinics** | 1 | ✅ داده موجود |
| **Doctors** | 2 | ✅ داده موجود |
| **PatientInsurances** | 2 | ✅ داده موجود |
| **Receptions** | 0 | ⚠️ بدون داده |
| **ReceptionItems** | 0 | ⚠️ بدون داده |

---

## 📋 لیست کامل جداول (58 جدول)

### جداول اصلی سیستم:
1. `__MigrationHistory` - تاریخچه Migration ها
2. `AspNetRoles` - نقش‌های کاربری
3. `AspNetUserClaims` - Claims کاربران
4. `AspNetUserLogins` - Login های کاربران
5. `AspNetUserRoles` - نقش‌های کاربران
6. `AspNetUsers` - کاربران سیستم
7. `DatabaseVersion` - نسخه دیتابیس

### جداول Clinic & Department:
8. `Clinics` - کلینیک‌ها (1 رکورد)
9. `Departments` - دپارتمان‌ها (40 رکورد)
10. `Services` - خدمات (27 رکورد)
11. `ServiceCategories` - دسته‌بندی خدمات
12. `ServiceComponents` - اجزای خدمات
13. `ServiceTemplates` - قالب‌های خدمات
14. `SharedServices` - خدمات مشترک

### جداول Doctor:
15. `Doctors` - پزشکان (2 رکورد)
16. `DoctorDepartments` - انتساب پزشکان به دپارتمان‌ها
17. `DoctorServiceCategories` - صلاحیت پزشکان برای دسته‌بندی خدمات
18. `DoctorSpecializations` - تخصص‌های پزشکان
19. `DoctorSchedules` - برنامه‌های زمانی پزشکان
20. `DoctorTimeSlots` - بازه‌های زمانی پزشکان
21. `DoctorTimeRanges` - محدوده‌های زمانی پزشکان
22. `DoctorWorkDays` - روزهای کاری پزشکان
23. `DoctorAssignmentHistories` - تاریخچه انتساب‌های پزشکان
24. `Specializations` - تخصص‌ها

### جداول Patient:
25. `Patients` - بیماران (7,112 رکورد)
26. `PatientInsurances` - بیمه‌های بیماران (2 رکورد)
27. `MedicalHistories` - سوابق پزشکی

### جداول Reception:
28. `Receptions` - پذیرش‌ها (0 رکورد)
29. `ReceptionItems` - اقلام پذیرش (0 رکورد)

### جداول Insurance:
30. `InsuranceProviders` - بیمه‌گذاران (14 رکورد)
31. `InsurancePlans` - طرح‌های بیمه (14 رکورد)
32. `InsuranceTariffs` - تعرفه‌های بیمه
33. `InsuranceCalculations` - محاسبات بیمه
34. `PlanServices` - خدمات طرح‌های بیمه
35. `BusinessRules` - قواعد کسب‌وکار

### جداول Payment:
36. `PaymentTransactions` - تراکنش‌های پرداخت
37. `OnlinePayments` - پرداخت‌های آنلاین
38. `PaymentGateways` - درگاه‌های پرداخت
39. `PosTerminals` - ترمینال‌های POS
40. `CashSessions` - جلسات نقدی

### جداول Appointment:
41. `Appointments` - نوبت‌ها
42. `AppointmentSlots` - بازه‌های نوبت

### جداول Triage:
43. `TriageProtocols` - پروتکل‌های تریاژ
44. `TriageAssessments` - ارزیابی‌های تریاژ
45. `TriageQueues` - صف‌های تریاژ
46. `TriageReassessments` - ارزیابی‌های مجدد تریاژ
47. `TriageVitalSigns` - علائم حیاتی تریاژ
48. `TriageAssessmentProtocols` - پروتکل‌های ارزیابی تریاژ

### جداول Notification:
49. `NotificationHistories` - تاریخچه اعلان‌ها
50. `NotificationTemplates` - قالب‌های اعلان

### جداول دیگر:
51. `FactorSettings` - تنظیمات ضرایب
52. `ReceiptPrints` - چاپ رسیدها
53. `Reports` - گزارش‌ها
54. `OtpRequests` - درخواست‌های OTP
55. `NameGenderMap` - نقشه نام-جنسیت
56. `ScheduleTemplates` - قالب‌های برنامه
57. `ScheduleExceptions` - استثناهای برنامه
58. `sysdiagrams` - نمودارهای سیستم

---

## 🔍 بررسی‌های انجام شده

### ✅ اتصال موفق:
- اتصال به دیتابیس با موفقیت برقرار شد
- Server Name: DESKTOP-HGABNCN
- Database Name: ClinicDb
- Current Time: 2025-11-07 09:31:20.933

### ✅ ساختار دیتابیس:
- 58 جدول موجود است
- جداول اصلی (Clinic, Department, Doctor, Patient, Reception, Insurance) موجود هستند
- جداول سیستم (AspNet*, __MigrationHistory) موجود هستند

### ⚠️ نکات:
- جداول `Receptions` و `ReceptionItems` خالی هستند (0 رکورد)
- این طبیعی است اگر هنوز پذیرشی ثبت نشده باشد
- جداول `Patients` دارای 7,112 رکورد است (داده‌های تست/تولید موجود است)

---

## 📝 دستورات مفید برای بررسی بیشتر

### بررسی ساختار یک جدول خاص:
```sql
-- مثال: بررسی ساختار جدول Receptions
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Receptions'
ORDER BY ORDINAL_POSITION;
```

### بررسی روابط Foreign Key:
```sql
-- بررسی Foreign Key های جدول Receptions
SELECT 
    fk.name AS ForeignKeyName,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.tables tp ON fkc.parent_object_id = tp.object_id
INNER JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
INNER JOIN sys.tables tr ON fkc.referenced_object_id = tr.object_id
INNER JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
WHERE tp.name = 'Receptions';
```

### بررسی Index ها:
```sql
-- بررسی Index های جدول Receptions
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique,
    i.is_primary_key,
    STRING_AGG(c.name, ', ') AS Columns
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('Receptions')
GROUP BY i.name, i.type_desc, i.is_unique, i.is_primary_key;
```

---

## ✅ بررسی ساختار جداول کلیدی

### جدول Receptions (33 ستون):
- ✅ `ReceptionId` (PK)
- ✅ `PatientId`, `DoctorId`, `ClinicId`, `DepartmentId`
- ✅ `BasePlanId`, `SupplementaryPlanId` (Nullable)
- ✅ `FinancialYear`
- ✅ `TotalAmount`, `PatientCoPay`, `InsurerShareAmount`
- ✅ `Gross`, `BasePay`, `SuppPay`, `PatientPay`
- ✅ `Status`, `Type`, `Priority`
- ✅ `RowVersion` (Concurrency Control)
- ✅ `IsDeleted`, `CreatedAt`, `UpdatedAt` (Audit Fields)

### جدول ReceptionItems (15 ستون):
- ✅ `ReceptionItemId` (PK)
- ✅ `ReceptionId` (FK)
- ✅ `ServiceId` (FK)
- ✅ `Quantity`, `UnitPrice`
- ✅ `PatientShareAmount`, `InsurerShareAmount`
- ✅ **`SnapshotJson` (nvarchar(MAX), Nullable)** ✅ **موجود است**
- ✅ `IsDeleted`, `CreatedAt`, `UpdatedAt` (Audit Fields)

### جدول Services - فیلدهای Eligibility:
- ✅ **`AgeMin` (int, Nullable)** ✅ **موجود است**
- ✅ **`AgeMax` (int, Nullable)** ✅ **موجود است**
- ✅ **`GenderLimit` (tinyint, Nullable)** ✅ **موجود است**
- ✅ **`GroupCode` (int, Nullable)** ✅ **موجود است**

---

## ✅ نتیجه‌گیری

**وضعیت اتصال:** ✅ **موفق**  
**وضعیت دیتابیس:** ✅ **سالم**  
**ساختار:** ✅ **کامل**  
**Migration ها:** ✅ **اعمال شده** (SnapshotJson و فیلدهای Eligibility موجود هستند)  
**داده‌ها:** ✅ **موجود** (به جز Receptions که طبیعی است)

**دیتابیس آماده برای استفاده است!** 🚀

### ✅ تأیید Migration ها:
- ✅ `SnapshotJson` در `ReceptionItems` موجود است
- ✅ `AgeMin`, `AgeMax`, `GenderLimit`, `GroupCode` در `Services` موجود هستند
- ✅ ساختار دیتابیس با کد همخوان است

---

**تاریخ بررسی:** 2025-01-27  
**نسخه گزارش:** 1.0.0  
**وضعیت:** ✅ اتصال موفق و بررسی کامل انجام شد

