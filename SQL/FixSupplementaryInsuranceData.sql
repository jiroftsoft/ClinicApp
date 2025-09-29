-- =============================================
-- اصلاح داده‌های بیمه تکمیلی - نسخه صحیح
-- تاریخ: 2025-01-27
-- توضیح: اصلاح منطق بیمه تکمیلی برای پوشش صحیح سهم بیمار
-- =============================================

-- 1. بررسی وضعیت فعلی رکورد InsuranceTariffId 1202
SELECT 
    InsuranceTariffId,
    ServiceId,
    TariffPrice,
    PatientShare,
    InsurerShare,
    InsuranceType,
    InsurancePlanId,
    SupplementaryCoveragePercent,
    SupplementaryMaxPayment,
    Priority,
    Notes
FROM InsuranceTariffs 
WHERE InsuranceTariffId = 1202;

-- 2. اصلاح رکورد InsuranceTariffId 1202 برای بیمه تکمیلی صحیح
UPDATE InsuranceTariffs 
SET 
    -- اصلاح سهم بیمار: باید 30% از کل مبلغ باشد (3,579,000 ریال)
    PatientShare = 3579000,
    
    -- اصلاح سهم بیمه: بیمه تکمیلی سهم بیمه ندارد
    InsurerShare = 0,
    
    -- اصلاح درصد پوشش: 100% از سهم بیمار را پوشش دهد
    SupplementaryCoveragePercent = 100.00,
    
    -- حذف سقف پرداخت (یا تنظیم به مقدار مناسب)
    SupplementaryMaxPayment = NULL,
    
    -- بروزرسانی یادداشت
    Notes = 'بیمه تکمیلی VIP ملت - پوشش 100% سهم بیمار از بیمه اصلی سلامت',
    
    -- بروزرسانی تاریخ
    UpdatedAt = GETDATE(),
    UpdatedByUserId = 'system-fix'
WHERE InsuranceTariffId = 1202;

-- 3. بررسی سایر رکوردهای بیمه تکمیلی که ممکن است نیاز به اصلاح داشته باشند
SELECT 
    InsuranceTariffId,
    ServiceId,
    TariffPrice,
    PatientShare,
    InsurerShare,
    InsuranceType,
    InsurancePlanId,
    SupplementaryCoveragePercent,
    Priority,
    Notes
FROM InsuranceTariffs 
WHERE InsuranceType = 2  -- Supplementary
  AND IsDeleted = 0
  AND IsActive = 1
ORDER BY InsuranceTariffId;

-- 4. اصلاح رکوردهای بیمه تکمیلی که منطق اشتباه دارند
-- (رکوردهایی که InsurerShare > 0 دارند - این اشتباه است)
UPDATE InsuranceTariffs 
SET 
    PatientShare = TariffPrice - InsurerShare,  -- سهم بیمار = کل - سهم بیمه
    InsurerShare = 0,                          -- بیمه تکمیلی سهم بیمه ندارد
    Notes = ISNULL(Notes, '') + ' [اصلاح شده: منطق صحیح بیمه تکمیلی]',
    UpdatedAt = GETDATE(),
    UpdatedByUserId = 'system-fix'
WHERE InsuranceType = 2  -- Supplementary
  AND InsurerShare > 0   -- رکوردهایی که سهم بیمه دارند (اشتباه)
  AND IsDeleted = 0
  AND IsActive = 1;

-- 5. بررسی نتیجه اصلاحات
SELECT 
    InsuranceTariffId,
    ServiceId,
    TariffPrice,
    PatientShare,
    InsurerShare,
    InsuranceType,
    InsurancePlanId,
    SupplementaryCoveragePercent,
    Priority,
    Notes,
    UpdatedAt
FROM InsuranceTariffs 
WHERE InsuranceTariffId = 1202;

-- 6. اعتبارسنجی: بررسی اینکه مجموع سهم‌ها برابر مبلغ کل است
SELECT 
    InsuranceTariffId,
    TariffPrice,
    PatientShare,
    InsurerShare,
    (PatientShare + InsurerShare) as TotalShares,
    (TariffPrice - (PatientShare + InsurerShare)) as Difference,
    CASE 
        WHEN ABS(TariffPrice - (PatientShare + InsurerShare)) < 0.01 
        THEN 'صحیح' 
        ELSE 'اشتباه' 
    END as Status
FROM InsuranceTariffs 
WHERE InsuranceTariffId = 1202;

-- 7. ایجاد ایندکس برای بهبود عملکرد
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_InsuranceTariffs_Supplementary_Type')
BEGIN
    CREATE INDEX IX_InsuranceTariffs_Supplementary_Type 
    ON InsuranceTariffs (InsuranceType, IsActive, IsDeleted)
    INCLUDE (InsurancePlanId, ServiceId, SupplementaryCoveragePercent);
END

-- 8. آمار نهایی
SELECT 
    'تعداد کل رکوردهای بیمه تکمیلی' as Description,
    COUNT(*) as Count
FROM InsuranceTariffs 
WHERE InsuranceType = 2 AND IsDeleted = 0

UNION ALL

SELECT 
    'رکوردهای با منطق صحیح (InsurerShare = 0)',
    COUNT(*)
FROM InsuranceTariffs 
WHERE InsuranceType = 2 
  AND IsDeleted = 0 
  AND InsurerShare = 0

UNION ALL

SELECT 
    'رکوردهای با منطق اشتباه (InsurerShare > 0)',
    COUNT(*)
FROM InsuranceTariffs 
WHERE InsuranceType = 2 
  AND IsDeleted = 0 
  AND InsurerShare > 0;
