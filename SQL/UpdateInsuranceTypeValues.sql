-- =============================================
-- Script: Update InsuranceType Values
-- Description: به‌روزرسانی مقادیر InsuranceType در جدول InsurancePlans
-- Author: ClinicApp Development Team
-- Date: 2025-01-07
-- =============================================

-- 🔧 CRITICAL FIX: به‌روزرسانی بیمه‌های پایه (Primary = 1)
UPDATE InsurancePlans 
SET InsuranceType = 1 
WHERE PlanCode IN (
    'FREE_BASIC',
    'SSO_BASIC', 
    'SALAMAT_BASIC',
    'MILITARY_BASIC',
    'KHADAMAT_BASIC',
    'BANK_MELLI_BASIC',
    'BANK_SADERAT_BASIC',
    'BANK_SEPAH_BASIC'
);

-- 🔧 CRITICAL FIX: به‌روزرسانی بیمه‌های تکمیلی (Supplementary = 2)
UPDATE InsurancePlans 
SET InsuranceType = 2 
WHERE PlanCode IN (
    'DANA_SUPPLEMENTARY',
    'BIME_MA_SUPPLEMENTARY',
    'BIME_DEY_SUPPLEMENTARY',
    'BIME_ALBORZ_SUPPLEMENTARY',
    'BIME_PASARGAD_SUPPLEMENTARY',
    'BIME_ASIA_SUPPLEMENTARY'
);

-- 🔍 VERIFICATION: بررسی نتایج
SELECT 
    InsurancePlanId,
    PlanCode,
    Name,
    InsuranceType,
    CASE 
        WHEN InsuranceType = 1 THEN 'بیمه پایه'
        WHEN InsuranceType = 2 THEN 'بیمه تکمیلی'
        ELSE 'نامشخص'
    END AS InsuranceTypeDescription
FROM InsurancePlans 
WHERE IsActive = 1 AND IsDeleted = 0
ORDER BY InsuranceType, PlanCode;

-- 📊 STATISTICS: آمار به‌روزرسانی
SELECT 
    InsuranceType,
    COUNT(*) as Count,
    CASE 
        WHEN InsuranceType = 1 THEN 'بیمه پایه'
        WHEN InsuranceType = 2 THEN 'بیمه تکمیلی'
        ELSE 'نامشخص'
    END AS TypeDescription
FROM InsurancePlans 
WHERE IsActive = 1 AND IsDeleted = 0
GROUP BY InsuranceType
ORDER BY InsuranceType;
