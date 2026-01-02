-- ============================================================================
-- Script: حذف رکوردهای با شماره تلفن نامعتبر
-- تاریخ: 1404/10/13
-- هدف: حذف Users و Patients با شماره غیر موبایل یا نامعتبر
-- فرمت صحیح: +989XXXXXXXXX (13 کاراکتر)
-- ============================================================================

BEGIN TRANSACTION;

PRINT '📊 گزارش قبل از حذف:';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- شمارش Users نامعتبر
DECLARE @InvalidUsers INT;
SELECT @InvalidUsers = COUNT(*) 
FROM AspNetUsers 
WHERE PhoneNumber IS NOT NULL 
  AND PhoneNumber NOT LIKE '+989%' 
  AND IsDeleted = 0;

PRINT '❌ Users با شماره نامعتبر: ' + CAST(@InvalidUsers AS NVARCHAR(10));

-- شمارش Patients نامعتبر
DECLARE @InvalidPatients INT;
SELECT @InvalidPatients = COUNT(*) 
FROM Patients 
WHERE PhoneNumber IS NOT NULL 
  AND PhoneNumber NOT LIKE '+989%' 
  AND IsDeleted = 0;

PRINT '❌ Patients با شماره نامعتبر: ' + CAST(@InvalidPatients AS NVARCHAR(10));

-- نمونه شماره‌های نامعتبر
PRINT '';
PRINT '📋 نمونه شماره‌های نامعتبر:';
SELECT TOP 10 
    'User' AS Type,
    UserName AS Identifier,
    PhoneNumber,
    LEN(PhoneNumber) AS PhoneLength,
    CASE 
        WHEN PhoneNumber LIKE '021%' THEN 'Fixed Line Tehran'
        WHEN PhoneNumber LIKE '02%' THEN 'Fixed Line Other'
        WHEN PhoneNumber LIKE '9%' AND PhoneNumber NOT LIKE '+98%' THEN 'Mobile Without 0'
        WHEN PhoneNumber LIKE '%-%' THEN 'With Dash'
        ELSE 'Other Invalid'
    END AS InvalidReason
FROM AspNetUsers 
WHERE PhoneNumber IS NOT NULL 
  AND PhoneNumber NOT LIKE '+989%' 
  AND IsDeleted = 0
ORDER BY NEWID();

PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '';

-- ============================================================================
-- STEP 1: حذف Users با شماره نامعتبر
-- ============================================================================

PRINT '🗑️  حذف Users با شماره نامعتبر...';

-- ابتدا باید رکوردهای مرتبط در جداول دیگر را حذف کنیم
PRINT '  └─ حذف UserRoles...';
DELETE FROM AspNetUserRoles
WHERE UserId IN (
    SELECT Id FROM AspNetUsers 
    WHERE PhoneNumber IS NOT NULL 
      AND PhoneNumber NOT LIKE '+989%' 
      AND IsDeleted = 0
);

DECLARE @DeletedUserRoles INT = @@ROWCOUNT;
PRINT '     ✅ حذف شد: ' + CAST(@DeletedUserRoles AS NVARCHAR(10)) + ' UserRole';

-- حذف UserLogins
PRINT '  └─ حذف UserLogins...';
DELETE FROM AspNetUserLogins
WHERE UserId IN (
    SELECT Id FROM AspNetUsers 
    WHERE PhoneNumber IS NOT NULL 
      AND PhoneNumber NOT LIKE '+989%' 
      AND IsDeleted = 0
);

DECLARE @DeletedUserLogins INT = @@ROWCOUNT;
PRINT '     ✅ حذف شد: ' + CAST(@DeletedUserLogins AS NVARCHAR(10)) + ' UserLogin';

-- حذف UserClaims
PRINT '  └─ حذف UserClaims...';
DELETE FROM AspNetUserClaims
WHERE UserId IN (
    SELECT Id FROM AspNetUsers 
    WHERE PhoneNumber IS NOT NULL 
      AND PhoneNumber NOT LIKE '+989%' 
      AND IsDeleted = 0
);

DECLARE @DeletedUserClaims INT = @@ROWCOUNT;
PRINT '     ✅ حذف شد: ' + CAST(@DeletedUserClaims AS NVARCHAR(10)) + ' UserClaim';

-- حذف Patients مرتبط
PRINT '  └─ حذف Patients مرتبط...';
DELETE FROM Patients
WHERE ApplicationUserId IN (
    SELECT Id FROM AspNetUsers 
    WHERE PhoneNumber IS NOT NULL 
      AND PhoneNumber NOT LIKE '+989%' 
      AND IsDeleted = 0
);

DECLARE @DeletedPatientsRelated INT = @@ROWCOUNT;
PRINT '     ✅ حذف شد: ' + CAST(@DeletedPatientsRelated AS NVARCHAR(10)) + ' Patient مرتبط';

-- حالا حذف Users
PRINT '  └─ حذف Users...';
DELETE FROM AspNetUsers
WHERE PhoneNumber IS NOT NULL 
  AND PhoneNumber NOT LIKE '+989%' 
  AND IsDeleted = 0;

DECLARE @DeletedUsers INT = @@ROWCOUNT;
PRINT '     ✅ حذف شد: ' + CAST(@DeletedUsers AS NVARCHAR(10)) + ' User';

-- ============================================================================
-- STEP 2: حذف Patients با شماره نامعتبر (بدون User)
-- ============================================================================

PRINT '';
PRINT '🗑️  حذف Patients با شماره نامعتبر (بدون User)...';

DELETE FROM Patients
WHERE PhoneNumber IS NOT NULL 
  AND PhoneNumber NOT LIKE '+989%' 
  AND IsDeleted = 0;

DECLARE @DeletedPatients INT = @@ROWCOUNT;
PRINT '     ✅ حذف شد: ' + CAST(@DeletedPatients AS NVARCHAR(10)) + ' Patient';

-- ============================================================================
-- STEP 3: بررسی نهایی
-- ============================================================================

PRINT '';
PRINT '📊 گزارش بعد از حذف:';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

DECLARE @RemainingInvalidUsers INT;
SELECT @RemainingInvalidUsers = COUNT(*) 
FROM AspNetUsers 
WHERE PhoneNumber IS NOT NULL 
  AND PhoneNumber NOT LIKE '+989%' 
  AND IsDeleted = 0;

PRINT '📌 Users با شماره نامعتبر باقیمانده: ' + CAST(@RemainingInvalidUsers AS NVARCHAR(10));

DECLARE @RemainingInvalidPatients INT;
SELECT @RemainingInvalidPatients = COUNT(*) 
FROM Patients 
WHERE PhoneNumber IS NOT NULL 
  AND PhoneNumber NOT LIKE '+989%' 
  AND IsDeleted = 0;

PRINT '📌 Patients با شماره نامعتبر باقیمانده: ' + CAST(@RemainingInvalidPatients AS NVARCHAR(10));

DECLARE @ValidUsers INT;
SELECT @ValidUsers = COUNT(*) 
FROM AspNetUsers 
WHERE PhoneNumber LIKE '+989%' 
  AND LEN(PhoneNumber) = 13
  AND IsDeleted = 0;

PRINT '✅ Users با شماره معتبر (+989...): ' + CAST(@ValidUsers AS NVARCHAR(10));

DECLARE @ValidPatients INT;
SELECT @ValidPatients = COUNT(*) 
FROM Patients 
WHERE PhoneNumber LIKE '+989%' 
  AND LEN(PhoneNumber) = 13
  AND IsDeleted = 0;

PRINT '✅ Patients با شماره معتبر (+989...): ' + CAST(@ValidPatients AS NVARCHAR(10));

PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- ============================================================================
-- STEP 4: Validation
-- ============================================================================

IF @RemainingInvalidUsers > 0 OR @RemainingInvalidPatients > 0
BEGIN
    PRINT '';
    PRINT '⚠️  هنوز شماره‌های نامعتبر باقی مانده‌اند!';
    ROLLBACK TRANSACTION;
    RAISERROR('حذف ناقص - Transaction برگشت داده شد.', 16, 1);
    RETURN;
END

PRINT '';
PRINT '🎉 حذف با موفقیت انجام شد!';
PRINT '✅ Transaction تأیید می‌شود...';

COMMIT TRANSACTION;

PRINT '';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '📌 خلاصه:';
PRINT '   - حذف شده Users: ' + CAST(@DeletedUsers AS NVARCHAR(10));
PRINT '   - حذف شده Patients: ' + CAST(@DeletedPatients + @DeletedPatientsRelated AS NVARCHAR(10));
PRINT '   - فرمت صحیح: +989XXXXXXXXX (13 کاراکتر)';
PRINT '   - دیتابیس اکنون فقط شماره‌های معتبر دارد.';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

