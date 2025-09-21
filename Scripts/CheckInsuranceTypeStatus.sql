-- بررسی وضعیت فعلی InsuranceType
SELECT 
    'وضعیت InsuranceType' as Status,
    InsuranceType,
    COUNT(*) as Count
FROM InsurancePlans 
WHERE IsActive = 1 AND IsDeleted = 0
GROUP BY InsuranceType
ORDER BY InsuranceType;

-- بررسی تفصیلی
SELECT 
    InsurancePlanId,
    Name,
    InsuranceProviderId,
    CoveragePercent,
    InsuranceType,
    CASE 
        WHEN InsuranceType = 1 THEN 'بیمه پایه'
        WHEN InsuranceType = 2 THEN 'بیمه تکمیلی'
        ELSE 'نامشخص'
    END as TypeDescription
FROM InsurancePlans 
WHERE IsActive = 1 AND IsDeleted = 0
ORDER BY InsuranceType, InsurancePlanId;
