-- 🔧 CRITICAL FIX: اصلاح مقادیر NULL در InsuranceType
-- این script تعرفه‌هایی که InsuranceType = NULL دارند را به Primary تبدیل می‌کند

-- بررسی وضعیت فعلی
SELECT 
    'Current Status' as Status,
    COUNT(*) as TotalTariffs,
    SUM(CASE WHEN InsuranceType = 1 THEN 1 ELSE 0 END) as PrimaryTariffs,
    SUM(CASE WHEN InsuranceType = 2 THEN 1 ELSE 0 END) as SupplementaryTariffs,
    SUM(CASE WHEN InsuranceType IS NULL THEN 1 ELSE 0 END) as NullTariffs
FROM InsuranceTariffs 
WHERE IsDeleted = 0;

-- اصلاح مقادیر NULL به Primary (بیمه پایه)
UPDATE InsuranceTariffs 
SET InsuranceType = 1  -- Primary
WHERE InsuranceType IS NULL 
  AND IsDeleted = 0;

-- بررسی وضعیت بعد از اصلاح
SELECT 
    'After Fix' as Status,
    COUNT(*) as TotalTariffs,
    SUM(CASE WHEN InsuranceType = 1 THEN 1 ELSE 0 END) as PrimaryTariffs,
    SUM(CASE WHEN InsuranceType = 2 THEN 1 ELSE 0 END) as SupplementaryTariffs,
    SUM(CASE WHEN InsuranceType IS NULL THEN 1 ELSE 0 END) as NullTariffs
FROM InsuranceTariffs 
WHERE IsDeleted = 0;

-- نمایش نمونه‌ای از تعرفه‌های اصلاح شده
SELECT TOP 10
    InsuranceTariffId,
    InsuranceType,
    TariffPrice,
    PatientShare,
    InsurerShare,
    CreatedAt
FROM InsuranceTariffs 
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;
