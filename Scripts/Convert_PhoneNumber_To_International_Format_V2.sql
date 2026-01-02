-- ============================================================================
-- Script: تبدیل فرمت شماره موبایل به فرمت بین‌المللی (V2)
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

PRINT '📌 Users با فرمت 09...: ' + CAST(@TotalUsers09Format AS NVARCHAR(10));

DECLARE @TotalPatients09Format INT;
SELECT @TotalPatients09Format = COUNT(*) 
FROM Patients 
WHERE PhoneNumber LIKE '09%' AND IsDeleted = 0;

PRINT '📌 Patients با فرمت 09...: ' + CAST(@TotalPatients09Format AS NVARCHAR(10));

PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '';

-- ============================================================================
-- STEP 1: NULL کردن شماره‌های تکراری در AspNetUsers
-- ============================================================================

PRINT '🔄 NULL کردن شماره‌های تکراری در AspNetUsers...';

UPDATE AspNetUsers
SET PhoneNumber = NULL,
    PhoneNumberConfirmed = 0
WHERE PhoneNumber LIKE '09%'
  AND LEN(PhoneNumber) = 11
  AND IsDeleted = 0
  AND '+98' + SUBSTRING(PhoneNumber, 2, LEN(PhoneNumber) - 1) IN (
      SELECT PhoneNumber FROM AspNetUsers WHERE PhoneNumber LIKE '+98%'
  );

DECLARE @NulledDuplicates INT = @@ROWCOUNT;
IF @NulledDuplicates > 0
    PRINT '⚠️  NULL شد: ' + CAST(@NulledDuplicates AS NVARCHAR(10)) + ' شماره تکراری';

-- ============================================================================
-- STEP 2: تبدیل شماره‌های باقیمانده در AspNetUsers
-- ============================================================================

PRINT '🔄 تبدیل شماره‌های AspNetUsers...';

UPDATE AspNetUsers
SET PhoneNumber = '+98' + SUBSTRING(PhoneNumber, 2, LEN(PhoneNumber) - 1)
WHERE PhoneNumber LIKE '09%'
  AND LEN(PhoneNumber) = 11
  AND IsDeleted = 0;

DECLARE @UpdatedUsers INT = @@ROWCOUNT;
PRINT '✅ تبدیل شد: ' + CAST(@UpdatedUsers AS NVARCHAR(10)) + ' کاربر';

-- ============================================================================
-- STEP 3: حذف بیماران با شماره نامعتبر
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
-- STEP 4: تبدیل شماره‌های Patients
-- ============================================================================

PRINT '🔄 تبدیل شماره‌های Patients...';

UPDATE Patients
SET PhoneNumber = '+98' + SUBSTRING(PhoneNumber, 2, LEN(PhoneNumber) - 1)
WHERE PhoneNumber LIKE '09%'
  AND LEN(PhoneNumber) = 11
  AND IsDeleted = 0;

DECLARE @UpdatedPatients INT = @@ROWCOUNT;
PRINT '✅ تبدیل شد: ' + CAST(@UpdatedPatients AS NVARCHAR(10)) + ' بیمار';

-- ============================================================================
-- STEP 5: بررسی نهایی
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

DECLARE @NullPhoneUsers INT;
SELECT @NullPhoneUsers = COUNT(*) 
FROM AspNetUsers 
WHERE PhoneNumber IS NULL AND IsDeleted = 0;

PRINT '📌 Users با PhoneNumber = NULL: ' + CAST(@NullPhoneUsers AS NVARCHAR(10));

-- نمونه 5 شماره تبدیل شده
PRINT '';
PRINT '📋 نمونه شماره‌های تبدیل شده:';
SELECT TOP 5 UserName, PhoneNumber 
FROM AspNetUsers 
WHERE PhoneNumber LIKE '+98%' AND IsDeleted = 0
ORDER BY NEWID();

PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- ============================================================================
-- STEP 6: Validation
-- ============================================================================

IF @Remaining09Users > 0 OR @Remaining09Patients > 0
BEGIN
    PRINT '⚠️  هنوز شماره‌هایی با فرمت 09... باقی مانده‌اند!';
    ROLLBACK TRANSACTION;
    RAISERROR('تبدیل ناقص - Transaction برگشت داده شد.', 16, 1);
    RETURN;
END

PRINT '';
PRINT '🎉 Script با موفقیت اجرا شد!';
PRINT '✅ Transaction تأیید می‌شود...';

COMMIT TRANSACTION;

PRINT '';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '📌 نکته: از این به بعد، همه شماره‌های موبایل باید با +98 ذخیره شوند.';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

