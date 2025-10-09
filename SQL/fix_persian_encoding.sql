-- 🔧 تصحیح Encoding برای متن‌های فارسی در دیتابیس ClinicDb
-- این اسکریپت Collation جداول را به Persian_100_CI_AS تغییر می‌دهد

USE ClinicDb;
GO

-- بررسی Collation فعلی دیتابیس
SELECT DATABASEPROPERTYEX('ClinicDb', 'Collation') as CurrentCollation;
GO

-- تغییر Collation دیتابیس (نیاز به دسترسی DBA)
-- ALTER DATABASE ClinicDb COLLATE Persian_100_CI_AS;
-- GO

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
ALTER COLUMN PatientCode NVARCHAR(50) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Patients 
ALTER COLUMN Address NVARCHAR(500) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Patients 
ALTER COLUMN PhoneNumber NVARCHAR(20) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Patients 
ALTER COLUMN Email NVARCHAR(100) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Patients 
ALTER COLUMN BloodType NVARCHAR(50) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Patients 
ALTER COLUMN Allergies NVARCHAR(1000) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Patients 
ALTER COLUMN ChronicDiseases NVARCHAR(1000) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Patients 
ALTER COLUMN EmergencyContactName NVARCHAR(100) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Patients 
ALTER COLUMN EmergencyContactPhone NVARCHAR(20) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE Patients 
ALTER COLUMN EmergencyContactRelationship NVARCHAR(50) COLLATE Persian_100_CI_AS;
GO

-- جدول InsuranceProviders
ALTER TABLE InsuranceProviders 
ALTER COLUMN Name NVARCHAR(200) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE InsuranceProviders 
ALTER COLUMN Code NVARCHAR(50) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE InsuranceProviders 
ALTER COLUMN ContactInfo NVARCHAR(500) COLLATE Persian_100_CI_AS;
GO

-- جدول InsurancePlans
ALTER TABLE InsurancePlans 
ALTER COLUMN Name NVARCHAR(200) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE InsurancePlans 
ALTER COLUMN Code NVARCHAR(50) COLLATE Persian_100_CI_AS;
GO

ALTER TABLE InsurancePlans 
ALTER COLUMN Description NVARCHAR(1000) COLLATE Persian_100_CI_AS;
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

ALTER TABLE Services 
ALTER COLUMN Description NVARCHAR(1000) COLLATE Persian_100_CI_AS;
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

-- بررسی نتیجه
SELECT 
    COLUMN_NAME,
    COLLATION_NAME
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Patients' 
    AND COLLATION_NAME IS NOT NULL
ORDER BY COLUMN_NAME;
GO
