-- اسکریپت اصلاح داده‌های بیمه - مرحله 1
-- این اسکریپت داده‌های نامعتبر را اصلاح می‌کند

-- 1. اصلاح InsuranceType = 0 به مقادیر صحیح
UPDATE InsuranceTariffs 
SET InsuranceType = 1  -- Primary
WHERE InsurancePlanId = 2 AND InsuranceType = 0;

UPDATE InsuranceTariffs 
SET InsuranceType = 2  -- Supplementary  
WHERE InsurancePlanId = 1 AND InsuranceType = 0;

UPDATE InsuranceTariffs 
SET InsuranceType = 2  -- Supplementary
WHERE InsurancePlanId = 4 AND InsuranceType = 1;

-- 2. اصلاح منطق بیمه تکمیلی (بیمه تکمیلی باید پوشش اضافی ارائه دهد)
UPDATE InsuranceTariffs 
SET PatientShare = 30,  -- بیمار 30% بپردازد
    InsurerShare = 70   -- بیمه تکمیلی 70% بپردازد
WHERE InsurancePlanId = 1 AND InsuranceType = 2 AND PatientShare = 100;

-- 3. بررسی نتایج
SELECT 
    InsurancePlanId,
    InsuranceType,
    COUNT(*) as Count,
    AVG(PatientShare) as AvgPatientShare,
    AVG(InsurerShare) as AvgInsurerShare
FROM InsuranceTariffs 
WHERE IsDeleted = 0
GROUP BY InsurancePlanId, InsuranceType
ORDER BY InsurancePlanId, InsuranceType;
