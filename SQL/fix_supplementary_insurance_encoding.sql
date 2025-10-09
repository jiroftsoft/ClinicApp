-- 🔧 تصحیح Encoding برای بیمه‌های تکمیلی
-- این اسکریپت نام‌های بیمه تکمیلی را تصحیح می‌کند

USE ClinicDb;
GO

PRINT '🚀 شروع تصحیح Encoding بیمه‌های تکمیلی...';
GO

-- تصحیح نام ارائه‌دهنده بیمه تکمیلی
UPDATE InsuranceProviders 
SET Name = N'بیمه تکمیلی نیروهای مسلح'
WHERE InsuranceProviderId = 1033;
GO

-- تصحیح نام طرح بیمه تکمیلی
UPDATE InsurancePlans 
SET Name = N'نیروهای مسلح - طرح تکمیلی'
WHERE InsurancePlanId = 1018;
GO

-- بررسی نتیجه
SELECT 
    pi.PatientInsuranceId,
    pi.PatientId,
    pi.IsPrimary,
    pi.SupplementaryInsuranceProviderId,
    pi.SupplementaryInsurancePlanId,
    pi.SupplementaryPolicyNumber,
    sip.Name as SupplementaryProviderName,
    sip2.Name as SupplementaryPlanName
FROM PatientInsurances pi 
LEFT JOIN InsuranceProviders sip ON pi.SupplementaryInsuranceProviderId = sip.InsuranceProviderId 
LEFT JOIN InsurancePlans sip2 ON pi.SupplementaryInsurancePlanId = sip2.InsurancePlanId 
WHERE pi.IsDeleted = 0;

PRINT '✅ تصحیح Encoding بیمه‌های تکمیلی انجام شد';
GO
