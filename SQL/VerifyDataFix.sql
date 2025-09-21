-- اسکریپت بررسی نتایج پس از اصلاح داده‌ها
-- این اسکریپت نتایج اصلاح را بررسی می‌کند

-- 1. بررسی وضعیت نهایی InsuranceType
SELECT 
    'Final InsuranceType Status' as Status,
    InsuranceType,
    COUNT(*) as Count,
    'Records' as Unit
FROM InsuranceTariffs 
WHERE IsDeleted = 0
GROUP BY InsuranceType
ORDER BY InsuranceType;

-- 2. بررسی وضعیت نهایی بر اساس InsurancePlanId
SELECT 
    'Final Plan Status' as Status,
    InsurancePlanId,
    InsuranceType,
    COUNT(*) as Count,
    AVG(PatientShare) as AvgPatientShare,
    AVG(InsurerShare) as AvgInsurerShare
FROM InsuranceTariffs 
WHERE IsDeleted = 0
GROUP BY InsurancePlanId, InsuranceType
ORDER BY InsurancePlanId, InsuranceType;

-- 3. بررسی رکوردهای اصلاح شده
SELECT 
    'Fixed Records Summary' as Status,
    COUNT(*) as TotalFixed,
    SUM(CASE WHEN InsuranceType = 1 THEN 1 ELSE 0 END) as PrimaryFixed,
    SUM(CASE WHEN InsuranceType = 2 THEN 1 ELSE 0 END) as SupplementaryFixed
FROM InsuranceTariffs 
WHERE IsDeleted = 0;

-- 4. بررسی رکوردهای بیمه تکمیلی اصلاح شده
SELECT 
    'Supplementary Insurance Fixed' as Status,
    InsurancePlanId,
    COUNT(*) as Count,
    AVG(PatientShare) as AvgPatientShare,
    AVG(InsurerShare) as AvgInsurerShare,
    'Should be 30% Patient, 70% Insurer' as Expected
FROM InsuranceTariffs 
WHERE IsDeleted = 0 
  AND InsuranceType = 2
GROUP BY InsurancePlanId
ORDER BY InsurancePlanId;
