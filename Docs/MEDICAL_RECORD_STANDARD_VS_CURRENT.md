# مقایسه پرونده پزشکی استاندارد/قانونی با معماری فعلی کلینیک شفا

این سند نیازمندی‌های «پرونده پزشکی استاندارد و قابل دفاع» را با موجودیت‌ها و معماری فعلی مقایسه می‌کند و تغییرات **افزودنی و بدون شکست** را مشخص می‌کند.

---

## ۱. اطلاعات هویتی بیمار (Patient Identity)

| مورد استاندارد | وضعیت فعلی | اقدام |
|----------------|------------|--------|
| نام و نام خانوادگی | ✅ `Patient.FirstName`, `LastName` | — |
| کد ملی / پاسپورت | ✅ `NationalCode` (Unique) | — |
| شماره پرونده (MRN) | ✅ `PatientCode` (کد بیمار) | ایندکس یکتا اختیاری در مایگریشن |
| تاریخ تولد | ✅ `BirthDate` | — |
| جنسیت | ✅ `Gender` | — |
| وضعیت تأهل | ❌ | **اضافه:** `MaritalStatus` (اختیاری) |
| شماره تماس | ✅ `PhoneNumber` | — |
| آدرس کامل | ✅ `Address` | — |
| نام همراه/ولی قانونی | ⚠️ تماس اضطراری هست | **اضافه:** `GuardianName`, `GuardianPhone` (اختیاری) |
| تماس اضطراری | ✅ `EmergencyContactName`, `EmergencyContactPhone`, `EmergencyContactRelationship` | — |

---

## ۲. اطلاعات بیمه

| مورد استاندارد | وضعیت فعلی | اقدام |
|----------------|------------|--------|
| نوع بیمه | ✅ `InsurancePlan`, `InsuranceProvider` | — |
| شماره بیمه | ✅ `PolicyNumber`, `CardNumber` | — |
| اعتبار بیمه | ✅ `StartDate`, `EndDate`, `IsActive` | — |
| نوع پوشش (بستری/سرپایی) | در سطح طرح (InsurancePlan) | قابل افزودن در فاز بعد |
| فرانشیز | ✅ در `InsurancePlan` و محاسبات | — |

---

## ۳. سوابق پزشکی (Medical History)

| مورد استاندارد | وضعیت فعلی | اقدام |
|----------------|------------|--------|
| بیماری‌های قبلی/مزمن | ✅ `MedicalHistory` (Type=Disease) + `ChronicDiseases` روی Patient | — |
| سابقه جراحی | ✅ Type=Surgery | — |
| آلرژی‌ها | ✅ Type=Allergy؛ + `Patient.Allergies` (متن آزاد) | **اضافه:** `MedicalHistory.IsCritical` برای آلرژی بحرانی |
| سابقه خانوادگی | ✅ Type=FamilyHistory | — |
| داروهای مصرفی فعلی | ✅ `MedicalHistory` (Type=Medication) + `MedicalHistoryMedication` | — |

---

## ۴. ویزیت (Encounter / Visit)

| مورد استاندارد | وضعیت فعلی | اقدام |
|----------------|------------|--------|
| تاریخ/ساعت مراجعه | ✅ `Reception.ReceptionDate`؛ تریاژ: `ArrivalAt` | — |
| پزشک معالج | ✅ `Reception.DoctorId` | — |
| شکایت اصلی (Chief Complaint) | ✅ `TriageAssessment.ChiefComplaint`, `ChiefComplaintCode` | — |
| شرح حال (HPI) | ⚠️ در `Reception.Notes` یا تریاژ | — |
| تشخیص (Diagnosis / ICD) | ❌ | **اضافه:** `Reception.Diagnosis`, `DiagnosisCode` |
| طرح درمان | ❌ | **اضافه:** `Reception.TreatmentPlan` |
| دستورات پزشکی | در Notes یا آیتم‌ها | — |

**نکته:** `Reception` در این معماری نقش Encounter/Visit را دارد.

---

## ۵. نسخه و داروها

| مورد استاندارد | وضعیت فعلی | اقدام |
|----------------|------------|--------|
| نام دارو، دوز، تعداد، نحوه مصرف، مدت | ✅ `MedicalHistoryMedication` (و تاریخچه نوع دارو) | — |
| هشدار خاص | در توضیحات یا Indication | — |

نسخه رسمی (Prescription) به‌صورت موجودیت جدا در این معماری نیست؛ داروها در تاریخچه پزشکی و آیتم‌های پذیرش قابل ثبت هستند.

---

## ۶. نتایج آزمایش و پاراکلینیک

| مورد استاندارد | وضعیت فعلی | اقدام |
|----------------|------------|--------|
| نوع، تاریخ، نتیجه، ضمیمه | ✅ `MedicalHistoryLabResult` (نام، مقدار، واحد، تاریخ، محدوده مرجع) | ضمیمه PDF در فاز بعد |
| تایید پزشک | در فاز بعد | — |

---

## ۷. اطلاعات مالی

| مورد استاندارد | وضعیت فعلی | اقدام |
|----------------|------------|--------|
| نوع خدمت، هزینه، پرداختی، بدهی، روش پرداخت، تراکنش | ✅ `Reception`, `ReceptionItem`, `PaymentTransaction`, `InsuranceCalculation` | — |

---

## ۸. رضایت‌نامه‌ها (Consent)

| مورد استاندارد | وضعیت فعلی | اقدام |
|----------------|------------|--------|
| رضایت جراحی/بیهوشی/حریم/امضا | ❌ | فاز بعد — موجودیت جدا (PatientConsent یا ReceptionConsent) |

---

## ۹. وضعیت حقوقی و Audit

| مورد استاندارد | وضعیت فعلی | اقدام |
|----------------|------------|--------|
| امضای پزشک / مهر | در فاز بعد | — |
| تاریخ ثبت، ثبت‌کننده، لاگ تغییرات | ✅ `ITrackable`: `CreatedAt`, `CreatedByUserId`, `UpdatedAt`, `UpdatedByUserId`؛ `ISoftDelete` برای حذف نرم | — |

---

## خلاصه تغییرات پیاده‌سازی‌شده (بدون شکست ماژول)

- **Patient:** فیلدهای اختیاری `MaritalStatus`, `GuardianName`, `GuardianPhone`.
- **MedicalHistory:** فیلد اختیاری `IsCritical` (برای آلرژی بحرانی).
- **Reception:** فیلدهای اختیاری `Diagnosis`, `DiagnosisCode`, `TreatmentPlan`.

همه فیلدها **nullable/optional** هستند تا جریان موجود و مایگریشن شکسته نشوند. مایگریشن را خودتان اجرا می‌کنید.
