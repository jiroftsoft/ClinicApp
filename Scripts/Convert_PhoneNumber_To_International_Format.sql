-- ============================================================================
-- Script: تبدیل فرمت شماره موبایل به فرمت بین‌المللی
-- تاریخ: 1404/10/13
-- هدف: تبدیل 09XXXXXXXXX به +989XXXXXXXXX
-- ============================================================================

BEGIN TRANSACTION;

PRINT '📊 گزارش قبل از تبدیل:';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

DECLARE @TotalUsers09Format INT;
SELECT @TotalUsers09Format = COUNT(*) 
FROM AspNetUsers 
WHERE PhoneNumber LIKE '09%' AND IsDeleted = 0;

PRINT '📌 شماره‌های با فرمت 09...: ' + CAST(@TotalUsers09Format AS NVARCHAR(10));

DECLARE @TotalPatients09Format INT;
SELECT @TotalPatients09Format = COUNT(*) 
FROM Patients 
WHERE PhoneNumber LIKE '09%' AND IsDeleted = 0;

PRINT '📌 بیماران با فرمت 09...: ' + CAST(@TotalPatients09Format AS NVARCHAR(10));

PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '';

-- ============================================================================
-- STEP 1: تبدیل شماره‌ها در جدول AspNetUsers
-- ============================================================================

PRINT '🔄 تبدیل شماره‌ها در AspNetUsers...';

-- حذف شماره‌های تکراری قبل از تبدیل
DELETE FROM AspNetUsers
WHERE PhoneNumber LIKE '09%'
  AND LEN(PhoneNumber) = 11
  AND IsDeleted = 0
  AND '+98' + SUBSTRING(PhoneNumber, 2, LEN(PhoneNumber) - 1) IN (
      SELECT PhoneNumber FROM AspNetUsers WHERE PhoneNumber LIKE '+98%'
  );

DECLARE @DeletedDuplicates INT = @@ROWCOUNT;
IF @DeletedDuplicates > 0
    PRINT '⚠️  حذف شد: ' + CAST(@DeletedDuplicates AS NVARCHAR(10)) + ' کاربر با شماره تکراری';

-- تبدیل شماره‌های باقیمانده
UPDATE AspNetUsers
SET PhoneNumber = '+98' + SUBSTRING(PhoneNumber, 2, LEN(PhoneNumber) - 1)
WHERE PhoneNumber LIKE '09%'
  AND LEN(PhoneNumber) = 11
  AND IsDeleted = 0;

DECLARE @UpdatedUsers INT = @@ROWCOUNT;
PRINT '✅ تبدیل شد: ' + CAST(@UpdatedUsers AS NVARCHAR(10)) + ' کاربر';

-- ============================================================================
-- STEP 2: حذف بیماران با شماره نامعتبر (< 11 digit)
-- ============================================================================

PRINT '🗑️  حذف بیماران با شماره نامعتبر...';

DELETE FROM Patients
WHERE PhoneNumber LIKE '09%'
  AND LEN(PhoneNumber) < 11
  AND IsDeleted = 0;

DECLARE @DeletedInvalidPatients INT = @@ROWCOUNT;
IF @DeletedInvalidPatients > 0
    PRINT '⚠️  حذف شد: ' + CAST(@DeletedInvalidPatients AS NVARCHAR(10)) + ' بیمار با شماره نامعتبر';

-- ============================================================================
-- STEP 3: تبدیل شماره‌ها در جدول Patients
-- ============================================================================

PRINT '🔄 تبدیل شماره‌ها در Patients...';

UPDATE Patients
SET PhoneNumber = '+98' + SUBSTRING(PhoneNumber, 2, LEN(PhoneNumber) - 1)
WHERE PhoneNumber LIKE '09%'
  AND LEN(PhoneNumber) = 11
  AND IsDeleted = 0;

DECLARE @UpdatedPatients INT = @@ROWCOUNT;
PRINT '✅ تبدیل شد: ' + CAST(@UpdatedPatients AS NVARCHAR(10)) + ' بیمار';

-- ============================================================================
-- STEP 4: بررسی نهایی
-- ============================================================================

PRINT '';
PRINT '📊 گزارش بعد از تبدیل:';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

DECLARE @Remaining09Users INT;
SELECT @Remaining09Users = COUNT(*) 
FROM AspNetUsers 
WHERE PhoneNumber LIKE '09%' AND IsDeleted = 0;

PRINT '📌 Users با فرمت 09... باقیمانده: ' + CAST(@Remaining09Users AS NVARCHAR(10));

DECLARE @Remaining09Patients INT;
SELECT @Remaining09Patients = COUNT(*) 
FROM Patients 
WHERE PhoneNumber LIKE '09%' AND IsDeleted = 0;

PRINT '📌 Patients با فرمت 09... باقیمانده: ' + CAST(@Remaining09Patients AS NVARCHAR(10));

DECLARE @International98Users INT;
SELECT @International98Users = COUNT(*) 
FROM AspNetUsers 
WHERE PhoneNumber LIKE '+98%' AND IsDeleted = 0;

PRINT '✅ Users با فرمت +98...: ' + CAST(@International98Users AS NVARCHAR(10));

DECLARE @International98Patients INT;
SELECT @International98Patients = COUNT(*) 
FROM Patients 
WHERE PhoneNumber LIKE '+98%' AND IsDeleted = 0;

PRINT '✅ Patients با فرمت +98...: ' + CAST(@International98Patients AS NVARCHAR(10));

-- نمونه 5 شماره تبدیل شده
PRINT '';
PRINT '📋 نمونه شماره‌های تبدیل شده:';
SELECT TOP 5 UserName, PhoneNumber 
FROM AspNetUsers 
WHERE PhoneNumber LIKE '+98%' AND IsDeleted = 0
ORDER BY NEWID();

PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- ============================================================================
-- STEP 4: Validation
-- ============================================================================

IF @Remaining09Users > 0 OR @Remaining09Patients > 0
BEGIN
    PRINT '⚠️  هنوز شماره‌هایی با فرمت 09... باقی مانده‌اند!';
    PRINT '💡 احتمالاً شماره‌هایی با طول نامعتبر (≠ 11) وجود دارند.';
    
    -- نمایش شماره‌های نامعتبر
    SELECT TOP 5 
        'User' AS TableName,
        UserName COLLATE SQL_Latin1_General_CP1_CI_AS AS Identifier, 
        PhoneNumber COLLATE SQL_Latin1_General_CP1_CI_AS AS PhoneNumber, 
        LEN(PhoneNumber) AS PhoneLength
    FROM AspNetUsers 
    WHERE PhoneNumber LIKE '09%' AND IsDeleted = 0
    
    UNION ALL
    
    SELECT TOP 5 
        'Patient' AS TableName,
        CAST(PatientId AS NVARCHAR(10)) COLLATE SQL_Latin1_General_CP1_CI_AS AS Identifier, 
        PhoneNumber COLLATE SQL_Latin1_General_CP1_CI_AS AS PhoneNumber, 
        LEN(PhoneNumber) AS PhoneLength
    FROM Patients 
    WHERE PhoneNumber LIKE '09%' AND IsDeleted = 0;
END
ELSE
BEGIN
    PRINT '✅ تمام شماره‌ها با موفقیت تبدیل شدند!';
END

PRINT '';
PRINT '🎉 Script با موفقیت اجرا شد!';
PRINT '✅ Transaction تأیید می‌شود...';

COMMIT TRANSACTION;

PRINT '';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '📌 نکته: از این به بعد، همه شماره‌های موبایل باید با +98 ذخیره شوند.';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

