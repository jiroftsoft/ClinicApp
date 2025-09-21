-- اسکریپت بررسی وضعیت فعلی داده‌های بیمه
-- این اسکریپت وضعیت فعلی داده‌ها را قبل از اصلاح نشان می‌دهد

-- 1. بررسی وضعیت فعلی InsuranceType
SELECT 
    'Current InsuranceType Status' as Status,
    InsuranceType,
    COUNT(*) as Count,
    'Records' as Unit
FROM InsuranceTariffs 
WHERE IsDeleted = 0
GROUP BY InsuranceType
ORDER BY InsuranceType;

-- 2. بررسی وضعیت فعلی بر اساس InsurancePlanId
SELECT 
    'Current Plan Status' as Status,
    InsurancePlanId,
    InsuranceType,
    COUNT(*) as Count,
    AVG(PatientShare) as AvgPatientShare,
    AVG(InsurerShare) as AvgInsurerShare
FROM InsuranceTariffs 
WHERE IsDeleted = 0
GROUP BY InsurancePlanId, InsuranceType
ORDER BY InsurancePlanId, InsuranceType;

-- 3. بررسی رکوردهای مشکل‌دار
SELECT 
    'Problematic Records' as Status,
    InsuranceTariffId,
    InsurancePlanId,
    InsuranceType,
    PatientShare,
    InsurerShare,
    'Needs Fix' as Action
FROM InsuranceTariffs 
WHERE IsDeleted = 0 
  AND (
    InsuranceType = 0 OR 
    (InsuranceType = 2 AND PatientShare = 100) OR
    (InsuranceType = 1 AND InsurancePlanId = 4)
  )
ORDER BY InsurancePlanId, InsuranceType;
