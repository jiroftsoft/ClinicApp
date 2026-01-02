-- ============================================================================
-- Script: ارسال اطلاعات ورود به بیماران Legacy
-- تاریخ: 1404/10/13
-- توضیح: این Script لیست بیماران قدیمی که User جدید گرفته‌اند را برمی‌گرداند
--         برای ارسال پیامک/ایمیل خوش‌آمدگویی
-- ============================================================================

-- STEP 1: لیست بیماران Legacy (که User از طریق Migration گرفته‌اند)
SELECT 
    p.PatientId,
    p.NationalCode,
    p.FirstName + ' ' + p.LastName AS FullName,
    p.PhoneNumber,
    p.Email,
    u.UserName,
    u.CreatedAt AS UserCreatedDate,
    u.CreatedByUserId
FROM 
    Patients p
    INNER JOIN AspNetUsers u ON p.ApplicationUserId = u.Id
WHERE 
    u.CreatedByUserId IS NULL  -- بیماران Legacy (از طریق Migration)
    AND u.PasswordHash IS NULL  -- بیماران که هنوز رمز تنظیم نکرده‌اند
    AND p.IsDeleted = 0
    AND u.IsDeleted = 0
ORDER BY 
    p.PatientId;

-- STEP 2: آمار کلی
SELECT 
    COUNT(*) AS TotalLegacyPatients,
    COUNT(CASE WHEN p.PhoneNumber IS NOT NULL THEN 1 END) AS PatientsWithPhone,
    COUNT(CASE WHEN p.Email IS NOT NULL THEN 1 END) AS PatientsWithEmail,
    COUNT(CASE WHEN p.PhoneNumber IS NULL AND p.Email IS NULL THEN 1 END) AS PatientsWithoutContact
FROM 
    Patients p
    INNER JOIN AspNetUsers u ON p.ApplicationUserId = u.Id
WHERE 
    u.CreatedByUserId IS NULL  -- بیماران Legacy
    AND u.PasswordHash IS NULL
    AND p.IsDeleted = 0
    AND u.IsDeleted = 0;

-- ============================================================================
-- نکات مهم:
-- 
-- 1. این بیماران هنوز PasswordHash ندارند
-- 2. باید یک "رمز موقت" یا "لینک فعال‌سازی" برای آن‌ها ارسال شود
-- 3. پیشنهاد: از OTP System استفاده کنید
-- 
-- نمونه پیامک:
-- "عزیز {نام بیمار}، به پورتال کلینیک شفا خوش آمدید!
--  برای ورود به سیستم:
--  کد ملی: {کد ملی}
--  برای دریافت رمز یکبار مصرف کلیک کنید: {لینک}
--  یا با شماره {شماره تماس کلینیک} تماس بگیرید."
-- ============================================================================

