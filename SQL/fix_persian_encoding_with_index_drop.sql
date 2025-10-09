-- 🔧 تصحیح Encoding برای متن‌های فارسی - نسخه با حذف Index ها
-- این اسکریپت ابتدا Index ها را حذف می‌کند، سپس Collation را تغییر می‌دهد

USE ClinicDb;
GO

PRINT '🚀 شروع تصحیح Encoding دیتابیس...';
GO

-- حذف Index های مربوط به FirstName و LastName در جدول Patients
DROP INDEX IF EXISTS IX_Patient_FirstName ON Patients;
DROP INDEX IF EXISTS IX_Patient_LastName ON Patients;
DROP INDEX IF EXISTS IX_Patient_LastName_FirstName ON Patients;
DROP INDEX IF EXISTS IX_Patient_FirstName_LastName_IsDeleted ON Patients;
DROP INDEX IF EXISTS IX_Patient_NationalCode ON Patients;
DROP INDEX IF EXISTS IX_Patient_NationalCode_IsDeleted ON Patients;
DROP INDEX IF EXISTS IX_Patient_PhoneNumber ON Patients;
DROP INDEX IF EXISTS IX_Patient_PhoneNumber_IsDeleted ON Patients;
DROP INDEX IF EXISTS IX_Patient_Email ON Patients;
GO

-- حذف Index های مربوط به InsuranceProviders
DROP INDEX IF EXISTS IX_InsuranceProvider_Name ON InsuranceProviders;
DROP INDEX IF EXISTS IX_InsuranceProvider_Code ON InsuranceProviders;
DROP INDEX IF EXISTS IX_InsuranceProvider_Code_IsActive ON InsuranceProviders;
GO

-- حذف Index های مربوط به InsurancePlans
DROP INDEX IF EXISTS IX_InsurancePlan_Name ON InsurancePlans;
GO

-- حذف Index های مربوط به PatientInsurances
DROP INDEX IF EXISTS IX_PatientInsurance_PolicyNumber ON PatientInsurances;
DROP INDEX IF EXISTS IX_PatientInsurance_SupplementaryPolicyNumber ON PatientInsurances;
GO

-- حذف Index های مربوط به Services
DROP INDEX IF EXISTS IX_Service_Title ON Services;
DROP INDEX IF EXISTS IX_Service_ServiceCode ON Services;
GO

-- حذف Index های مربوط به Doctors
DROP INDEX IF EXISTS IX_Doctor_FirstName ON Doctors;
DROP INDEX IF EXISTS IX_Doctor_LastName ON Doctors;
DROP INDEX IF EXISTS IX_Doctor_LastName_FirstName ON Doctors;
DROP INDEX IF EXISTS IX_Doctor_Degree ON Doctors;
DROP INDEX IF EXISTS IX_Doctor_MedicalCouncilCode ON Doctors;
DROP INDEX IF EXISTS IX_Doctor_PhoneNumber ON Doctors;
DROP INDEX IF EXISTS IX_Doctor_NationalCode ON Doctors;
DROP INDEX IF EXISTS IX_Doctor_Email ON Doctors;
GO

PRINT '✅ Index ها حذف شدند';
GO

-- تصحیح Collation جداول اصلی
-- جدول Patients
ALTER TABLE Patients 
ALTER COLUMN FirstName NVARCHAR(100) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Patients 
ALTER COLUMN LastName NVARCHAR(100) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Patients 
ALTER COLUMN NationalCode NVARCHAR(20) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Patients 
ALTER COLUMN PhoneNumber NVARCHAR(20) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Patients 
ALTER COLUMN Email NVARCHAR(100) COLLATE Persian_100_CI_AS;
GO

-- جدول InsuranceProviders
ALTER TABLE InsuranceProviders 
ALTER COLUMN Name NVARCHAR(200) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE InsuranceProviders 
ALTER COLUMN Code NVARCHAR(50) COLLATE Persian_100_CI_AS;
GO

-- جدول InsurancePlans
ALTER TABLE InsurancePlans 
ALTER COLUMN Name NVARCHAR(200) COLLATE Persian_100_CI_AS;
GO

-- جدول PatientInsurances
ALTER TABLE PatientInsurances 
ALTER COLUMN PolicyNumber NVARCHAR(100) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE PatientInsurances 
ALTER COLUMN SupplementaryPolicyNumber NVARCHAR(100) COLLATE Persian_100_CI_AS;
GO

-- جدول Services
ALTER TABLE Services 
ALTER COLUMN Title NVARCHAR(200) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Services 
ALTER COLUMN ServiceCode NVARCHAR(50) COLLATE Persian_100_CI_AS;
GO

-- جدول Doctors
ALTER TABLE Doctors 
ALTER COLUMN FirstName NVARCHAR(100) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Doctors 
ALTER COLUMN LastName NVARCHAR(100) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Doctors 
ALTER COLUMN Degree NVARCHAR(100) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Doctors 
ALTER COLUMN MedicalCouncilCode NVARCHAR(50) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Doctors 
ALTER COLUMN PhoneNumber NVARCHAR(20) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Doctors 
ALTER COLUMN NationalCode NVARCHAR(20) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Doctors 
ALTER COLUMN Email NVARCHAR(100) COLLATE Persian_100_CI_AS;
GO

PRINT '✅ تصحیح Collation جداول با موفقیت انجام شد';
GO

-- ایجاد مجدد Index ها
-- Index های Patients
CREATE INDEX IX_Patient_FirstName ON Patients(FirstName);
CREATE INDEX IX_Patient_LastName ON Patients(LastName);
CREATE INDEX IX_Patient_LastName_FirstName ON Patients(LastName, FirstName);
CREATE INDEX IX_Patient_FirstName_LastName_IsDeleted ON Patients(FirstName, LastName, IsDeleted);
CREATE INDEX IX_Patient_NationalCode ON Patients(NationalCode);
CREATE INDEX IX_Patient_NationalCode_IsDeleted ON Patients(NationalCode, IsDeleted);
CREATE INDEX IX_Patient_PhoneNumber ON Patients(PhoneNumber);
CREATE INDEX IX_Patient_PhoneNumber_IsDeleted ON Patients(PhoneNumber, IsDeleted);
CREATE INDEX IX_Patient_Email ON Patients(Email);
GO

-- Index های InsuranceProviders
CREATE INDEX IX_InsuranceProvider_Name ON InsuranceProviders(Name);
CREATE INDEX IX_InsuranceProvider_Code ON InsuranceProviders(Code);
CREATE INDEX IX_InsuranceProvider_Code_IsActive ON InsuranceProviders(Code, IsActive);
GO

-- Index های InsurancePlans
CREATE INDEX IX_InsurancePlan_Name ON InsurancePlans(Name);
GO

-- Index های PatientInsurances
CREATE INDEX IX_PatientInsurance_PolicyNumber ON PatientInsurances(PolicyNumber);
CREATE INDEX IX_PatientInsurance_SupplementaryPolicyNumber ON PatientInsurances(SupplementaryPolicyNumber);
GO

-- Index های Services
CREATE INDEX IX_Service_Title ON Services(Title);
CREATE INDEX IX_Service_ServiceCode ON Services(ServiceCode);
GO

-- Index های Doctors
CREATE INDEX IX_Doctor_FirstName ON Doctors(FirstName);
CREATE INDEX IX_Doctor_LastName ON Doctors(LastName);
CREATE INDEX IX_Doctor_LastName_FirstName ON Doctors(LastName, FirstName);
CREATE INDEX IX_Doctor_Degree ON Doctors(Degree);
CREATE INDEX IX_Doctor_MedicalCouncilCode ON Doctors(MedicalCouncilCode);
CREATE INDEX IX_Doctor_PhoneNumber ON Doctors(PhoneNumber);
CREATE INDEX IX_Doctor_NationalCode ON Doctors(NationalCode);
CREATE INDEX IX_Doctor_Email ON Doctors(Email);
GO

PRINT '✅ Index ها مجدداً ایجاد شدند';
GO

-- بررسی نتیجه
SELECT 
    COLUMN_NAME,
    COLLATION_NAME
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Patients' 
    AND COLLATION_NAME IS NOT NULL
ORDER BY COLUMN_NAME;
GO

PRINT '🎉 تصحیح Encoding با موفقیت تکمیل شد!';
GO
