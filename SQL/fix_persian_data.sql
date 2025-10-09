-- 🔧 تصحیح داده‌های فارسی موجود در دیتابیس
-- این اسکریپت داده‌های موجود را با Collation صحیح تصحیح می‌کند

USE ClinicDb;
GO

PRINT '🚀 شروع تصحیح داده‌های فارسی...';
GO

-- تصحیح داده‌های Patients
UPDATE Patients 
SET FirstName = N'سعیده'
WHERE PatientId = 167 AND FirstName LIKE '%سعیده%';
GO

UPDATE Patients 
SET LastName = N'کامرانی مهنی'
WHERE PatientId = 167 AND LastName LIKE '%کامرانی%';
GO

UPDATE Patients 
SET FirstName = N'محدثه'
WHERE PatientId = 2964 AND FirstName LIKE '%محدثه%';
GO

UPDATE Patients 
SET LastName = N'افشاری پور'
WHERE PatientId = 2964 AND LastName LIKE '%افشاری%';
GO

-- تصحیح داده‌های InsuranceProviders
UPDATE InsuranceProviders 
SET Name = N'بیمه نیروهای مسلح'
WHERE Name LIKE '%نیروهای مسلح%';
GO

-- تصحیح داده‌های InsurancePlans
UPDATE InsurancePlans 
SET Name = N'نیروهای مسلح - طرح پایه'
WHERE Name LIKE '%نیروهای مسلح%';
GO

PRINT '✅ تصحیح داده‌های فارسی انجام شد';
GO

-- بررسی نتیجه
SELECT 
    p.PatientId,
    p.FirstName,
    p.LastName,
    pi.PolicyNumber,
    ip.Name as PlanName,
    ipr.Name as ProviderName
FROM Patients p
INNER JOIN PatientInsurances pi ON p.PatientId = pi.PatientId
INNER JOIN InsurancePlans ip ON pi.InsurancePlanId = ip.InsurancePlanId
INNER JOIN InsuranceProviders ipr ON ip.InsuranceProviderId = ipr.InsuranceProviderId
WHERE pi.IsDeleted = 0;
GO

PRINT '🎉 تصحیح داده‌های فارسی با موفقیت تکمیل شد!';
GO
