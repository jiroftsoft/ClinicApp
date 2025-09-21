-- اسکریپت تصحیح داده‌های InsuranceType
-- این اسکریپت بیمه‌های پایه و تکمیلی را بر اساس منطق کسب‌وکار تصحیح می‌کند

-- 1. تصحیح بیمه‌های پایه (Primary = 1)
UPDATE InsurancePlans 
SET InsuranceType = 1 
WHERE InsurancePlanId IN (
    -- تأمین اجتماعی
    2,  -- تأمین اجتماعی استاندارد
    1003, -- طرح امید
    
    -- نیروهای مسلح  
    3,  -- نیروهای مسلح پریمیوم
    
    -- بیمه سلامت
    4,  -- بیمه سلامت پایه
    
    -- بیمه آزاد
    1   -- بیمه آزاد پایه
);

-- 2. تصحیح بیمه‌های تکمیلی (Supplementary = 2)
UPDATE InsurancePlans 
SET InsuranceType = 2 
WHERE InsurancePlanId IN (
    -- بیمه تکمیلی دانا
    5,  -- بیمه تکمیلی پلاس
    
    -- بیمه تکمیلی ملت
    1004 -- VIPملت
);

-- 3. بررسی نتایج
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
