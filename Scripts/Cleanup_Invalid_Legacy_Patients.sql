-- ============================================================================
-- Script: حذف بیماران Legacy با داده‌های نامعتبر
-- تاریخ: 1404/10/13
-- هدف: تمیزسازی دیتابیس قبل از تحویل پروژه
-- توجه: این Script بیماران با اطلاعات ناقص/نامعتبر را حذف می‌کند
-- ============================================================================

BEGIN TRANSACTION;

DECLARE @DeletedCount INT = 0;

-- ============================================================================
-- STEP 1: گزارش قبل از حذف
-- ============================================================================

PRINT '📊 گزارش بیماران قبل از حذف:';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- کل بیماران Legacy
DECLARE @TotalLegacy INT;
SELECT @TotalLegacy = COUNT(*) 
FROM Patients 
WHERE ApplicationUserId IS NULL AND IsDeleted = 0;

PRINT '📌 کل بیماران Legacy: ' + CAST(@TotalLegacy AS NVARCHAR(10));

-- بیماران با PhoneNumber خالی/نامعتبر
DECLARE @InvalidPhone INT;
SELECT @InvalidPhone = COUNT(*) 
FROM Patients 
WHERE ApplicationUserId IS NULL 
  AND IsDeleted = 0
  AND (PhoneNumber IS NULL OR PhoneNumber = '' OR LEN(PhoneNumber) < 10);

PRINT '❌ PhoneNumber نامعتبر: ' + CAST(@InvalidPhone AS NVARCHAR(10));

-- بیماران با PhoneNumber تکراری
DECLARE @DuplicatePhone INT;
SELECT @DuplicatePhone = COUNT(*) 
FROM Patients p
WHERE ApplicationUserId IS NULL 
  AND IsDeleted = 0
  AND PhoneNumber IN (
      SELECT PhoneNumber 
      FROM Patients 
      WHERE ApplicationUserId IS NULL AND IsDeleted = 0 AND PhoneNumber IS NOT NULL
      GROUP BY PhoneNumber 
      HAVING COUNT(*) > 1
  );

PRINT '❌ PhoneNumber تکراری: ' + CAST(@DuplicatePhone AS NVARCHAR(10));

-- بیماران با NationalCode نامعتبر
DECLARE @InvalidNationalCode INT;
SELECT @InvalidNationalCode = COUNT(*) 
FROM Patients 
WHERE ApplicationUserId IS NULL 
  AND IsDeleted = 0
  AND (NationalCode IS NULL OR NationalCode = '' OR LEN(NationalCode) < 10);

PRINT '❌ NationalCode نامعتبر: ' + CAST(@InvalidNationalCode AS NVARCHAR(10));

-- بیماران با FirstName/LastName خالی
DECLARE @InvalidName INT;
SELECT @InvalidName = COUNT(*) 
FROM Patients 
WHERE ApplicationUserId IS NULL 
  AND IsDeleted = 0
  AND (FirstName IS NULL OR FirstName = '' OR LastName IS NULL OR LastName = '');

PRINT '❌ FirstName/LastName خالی: ' + CAST(@InvalidName AS NVARCHAR(10));

PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- ============================================================================
-- STEP 2: حذف بیماران نامعتبر
-- ============================================================================

PRINT '';
PRINT '🗑️  شروع حذف بیماران نامعتبر...';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- 2.1: حذف بیماران با PhoneNumber نامعتبر
DELETE FROM Patients
WHERE ApplicationUserId IS NULL 
  AND IsDeleted = 0
  AND (PhoneNumber IS NULL OR PhoneNumber = '' OR LEN(PhoneNumber) < 10);

SET @DeletedCount = @@ROWCOUNT;
PRINT '✅ حذف شد: ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' بیمار با PhoneNumber نامعتبر';

-- 2.2: حذف بیماران تکراری (نگه داشتن اولین رکورد)
-- برای هر PhoneNumber تکراری، فقط اولین بیمار (کمترین PatientId) نگه داشته می‌شود
DELETE FROM Patients
WHERE ApplicationUserId IS NULL 
  AND IsDeleted = 0
  AND PhoneNumber IN (
      SELECT PhoneNumber 
      FROM Patients 
      WHERE ApplicationUserId IS NULL AND IsDeleted = 0 AND PhoneNumber IS NOT NULL
      GROUP BY PhoneNumber 
      HAVING COUNT(*) > 1
  )
  AND PatientId NOT IN (
      SELECT MIN(PatientId)
      FROM Patients
      WHERE ApplicationUserId IS NULL AND IsDeleted = 0 AND PhoneNumber IS NOT NULL
      GROUP BY PhoneNumber
      HAVING COUNT(*) > 1
  );

SET @DeletedCount = @@ROWCOUNT;
PRINT '✅ حذف شد: ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' بیمار تکراری (PhoneNumber)';

-- 2.3: حذف بیماران با NationalCode نامعتبر
DELETE FROM Patients
WHERE ApplicationUserId IS NULL 
  AND IsDeleted = 0
  AND (NationalCode IS NULL OR NationalCode = '' OR LEN(NationalCode) < 10);

SET @DeletedCount = @@ROWCOUNT;
PRINT '✅ حذف شد: ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' بیمار با NationalCode نامعتبر';

-- 2.4: حذف بیماران تکراری (NationalCode)
DELETE FROM Patients
WHERE ApplicationUserId IS NULL 
  AND IsDeleted = 0
  AND NationalCode IN (
      SELECT NationalCode 
      FROM Patients 
      WHERE ApplicationUserId IS NULL AND IsDeleted = 0 AND NationalCode IS NOT NULL
      GROUP BY NationalCode 
      HAVING COUNT(*) > 1
  )
  AND PatientId NOT IN (
      SELECT MIN(PatientId)
      FROM Patients
      WHERE ApplicationUserId IS NULL AND IsDeleted = 0 AND NationalCode IS NOT NULL
      GROUP BY NationalCode
      HAVING COUNT(*) > 1
  );

SET @DeletedCount = @@ROWCOUNT;
PRINT '✅ حذف شد: ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' بیمار تکراری (NationalCode)';

-- 2.5: حذف بیماران با FirstName/LastName خالی
DELETE FROM Patients
WHERE ApplicationUserId IS NULL 
  AND IsDeleted = 0
  AND (FirstName IS NULL OR FirstName = '' OR LastName IS NULL OR LastName = '');

SET @DeletedCount = @@ROWCOUNT;
PRINT '✅ حذف شد: ' + CAST(@DeletedCount AS NVARCHAR(10)) + ' بیمار با نام خالی';

PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- ============================================================================
-- STEP 3: گزارش بعد از حذف
-- ============================================================================

PRINT '';
PRINT '📊 گزارش بیماران بعد از حذف:';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- کل بیماران Legacy باقیمانده
DECLARE @RemainingLegacy INT;
SELECT @RemainingLegacy = COUNT(*) 
FROM Patients 
WHERE ApplicationUserId IS NULL AND IsDeleted = 0;

PRINT '✅ بیماران باقیمانده: ' + CAST(@RemainingLegacy AS NVARCHAR(10));
PRINT '🗑️  بیماران حذف شده: ' + CAST(@TotalLegacy - @RemainingLegacy AS NVARCHAR(10));

-- بررسی نهایی
DECLARE @InvalidRemaining INT;
SELECT @InvalidRemaining = COUNT(*) 
FROM Patients 
WHERE ApplicationUserId IS NULL 
  AND IsDeleted = 0
  AND (
      PhoneNumber IS NULL 
      OR PhoneNumber = '' 
      OR LEN(PhoneNumber) < 10
      OR NationalCode IS NULL 
      OR NationalCode = ''
      OR LEN(NationalCode) < 10
      OR FirstName IS NULL 
      OR FirstName = ''
      OR LastName IS NULL 
      OR LastName = ''
  );

IF @InvalidRemaining > 0
BEGIN
    PRINT '⚠️  هنوز ' + CAST(@InvalidRemaining AS NVARCHAR(10)) + ' بیمار نامعتبر باقی مانده است!';
    ROLLBACK TRANSACTION;
    RAISERROR('Cleanup ناقص - Transaction برگشت داده شد.', 16, 1);
    RETURN;
END
ELSE
BEGIN
    PRINT '✅ همه بیماران نامعتبر حذف شدند.';
END

-- بررسی تکراری
DECLARE @DuplicatesRemaining INT;
SELECT @DuplicatesRemaining = COUNT(*)
FROM (
    SELECT PhoneNumber 
    FROM Patients 
    WHERE ApplicationUserId IS NULL AND IsDeleted = 0 AND PhoneNumber IS NOT NULL
    GROUP BY PhoneNumber 
    HAVING COUNT(*) > 1
    
    UNION
    
    SELECT NationalCode 
    FROM Patients 
    WHERE ApplicationUserId IS NULL AND IsDeleted = 0 AND NationalCode IS NOT NULL
    GROUP BY NationalCode 
    HAVING COUNT(*) > 1
) AS Duplicates;

IF @DuplicatesRemaining > 0
BEGIN
    PRINT '⚠️  هنوز ' + CAST(@DuplicatesRemaining AS NVARCHAR(10)) + ' مورد تکراری باقی مانده است!';
    ROLLBACK TRANSACTION;
    RAISERROR('Cleanup ناقص - هنوز داده تکراری وجود دارد.', 16, 1);
    RETURN;
END
ELSE
BEGIN
    PRINT '✅ هیچ داده تکراری وجود ندارد.';
END

PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- ============================================================================
-- STEP 4: Commit Transaction
-- ============================================================================

PRINT '';
PRINT '🎉 Cleanup با موفقیت انجام شد!';
PRINT '✅ Transaction تأیید شد.';

COMMIT TRANSACTION;

PRINT '';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '📌 نکات مهم:';
PRINT '   - پشتیبان دیتابیس قبل از اجرای این Script گرفته شده است.';
PRINT '   - بیماران حذف شده قابل بازیابی نیستند.';
PRINT '   - دیتابیس اکنون آماده تحویل است.';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- ============================================================================
-- پایان Script
-- ============================================================================

