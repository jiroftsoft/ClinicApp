-- تست Query برای بررسی InsuranceTariff تکمیلی
-- PlanId: 1018 (بیمه تکمیلی دانا)
-- ServiceId: 1424 (ویزیت پزشک عمومی)
-- InsuranceType: 2 (Supplementary)

SELECT 
    InsuranceTariffId,
    InsurancePlanId,
    ServiceId,
    InsuranceType,
    SupplementaryCoveragePercent,
    PatientShare,
    InsurerShare,
    IsActive,
    IsDeleted
FROM InsuranceTariffs
WHERE InsurancePlanId = 1018
  AND ServiceId = 1424
  AND InsuranceType = 2  -- Supplementary
  AND IsDeleted = 0
  AND IsActive = 1;

-- انتظار: باید InsuranceTariffId = 3209 با SupplementaryCoveragePercent = 100.00 برگردد

