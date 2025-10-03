-- =====================================================
-- اسکریپت اصلاح داده‌های InsuranceTariffs
-- تاریخ: 2025-01-03
-- هدف: اصلاح TariffPrice و PatientShare بیمه تکمیلی
-- =====================================================

-- بررسی داده‌های فعلی
SELECT 
    InsuranceTariffId,
    ServiceId,
    TariffPrice,
    PatientShare,
    InsurerShare,
    InsuranceType,
    SupplementaryCoveragePercent,
    'قبل از اصلاح' as Status
FROM InsuranceTariffs 
WHERE ServiceId = 474 
ORDER BY InsuranceType;

-- اصلاح TariffPrice بیمه تکمیلی (تقسیم بر 100)
UPDATE InsuranceTariffs 
SET 
    TariffPrice = TariffPrice / 100,
    UpdatedAt = GETDATE(),
    UpdatedByUserId = '90ff4742-a2ed-4d1f-8037-92f7cb343d95'
WHERE 
    InsuranceTariffId = 2208 
    AND InsuranceType = 2 
    AND ServiceId = 474;

-- اصلاح PatientShare بیمه تکمیلی (تقسیم بر 100)
UPDATE InsuranceTariffs 
SET 
    PatientShare = PatientShare / 100,
    UpdatedAt = GETDATE(),
    UpdatedByUserId = '90ff4742-a2ed-4d1f-8037-92f7cb343d95'
WHERE 
    InsuranceTariffId = 2208 
    AND InsuranceType = 2 
    AND ServiceId = 474;

-- بررسی داده‌های پس از اصلاح
SELECT 
    InsuranceTariffId,
    ServiceId,
    TariffPrice,
    PatientShare,
    InsurerShare,
    InsuranceType,
    SupplementaryCoveragePercent,
    'پس از اصلاح' as Status
FROM InsuranceTariffs 
WHERE ServiceId = 474 
ORDER BY InsuranceType;

-- محاسبه و بررسی صحت داده‌ها
SELECT 
    'بیمه اصلی' as Type,
    TariffPrice,
    PatientShare,
    InsurerShare,
    CASE 
        WHEN (PatientShare + InsurerShare) = TariffPrice THEN '✅ صحیح'
        ELSE '❌ اشتباه'
    END as CalculationCheck
FROM InsuranceTariffs 
WHERE InsuranceTariffId = 2207

UNION ALL

SELECT 
    'بیمه تکمیلی' as Type,
    TariffPrice,
    PatientShare,
    InsurerShare,
    CASE 
        WHEN PatientShare = (TariffPrice * 0.3) THEN '✅ صحیح'
        ELSE '❌ اشتباه'
    END as CalculationCheck
FROM InsuranceTariffs 
WHERE InsuranceTariffId = 2208;
