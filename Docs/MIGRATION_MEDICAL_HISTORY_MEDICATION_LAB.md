# مایگریشن — پرونده پزشکی استاندارد (دارو، آزمایش، بیمار، تاریخچه، پذیرش)

پس از تغییرات مدل‌ها، مایگریشن را خودتان اجرا کنید.

## دستور (Package Manager Console)

```powershell
Add-Migration MedicalRecordStandardFieldsAndTables
Update-Database
```

(در پروژه‌های EF6 با .NET Framework معمولاً از PMC استفاده می‌شود.)

---

## الف) جداول جدید

- **MedicalHistoryMedications**: Id, MedicalHistoryId (FK), DrugName, Dosage, DosageUnit, Frequency, Route, StartDate, EndDate, Indication, PrescribingDoctor, IsActive, DisplayOrder, CreatedAt, CreatedByUserId
- **MedicalHistoryLabResults**: Id, MedicalHistoryId (FK), LabName, Value, Unit, LabDate, ReferenceRange, CreatedAt, CreatedByUserId

حذف رکورد `MedicalHistory` به صورت Cascade باعث حذف رکوردهای مرتبط در این دو جدول می‌شود.

---

## ب) ستون‌های جدید (افزودنی، nullable/optional)

- **Patients**: MaritalStatus (nvarchar 20), GuardianName (nvarchar 100), GuardianPhone (nvarchar 50)
- **MedicalHistories**: IsCritical (bit, nullable) — برای آلرژی بحرانی
- **Receptions**: Diagnosis (nvarchar 500), DiagnosisCode (nvarchar 20), TreatmentPlan (nvarchar 2000)

همه اختیاری هستند تا جریان و ماژول‌های موجود شکسته نشوند.
