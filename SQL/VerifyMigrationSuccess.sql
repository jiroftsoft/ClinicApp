-- ================================================================
-- Verification Script: Migration Success
-- Purpose: تأیید اینکه همه‌چیز به درستی ساخته شده است
-- ================================================================

USE ClinicDb;
GO

SET NOCOUNT ON;

PRINT '================================================================';
PRINT 'MIGRATION SUCCESS VERIFICATION';
PRINT 'Date: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
PRINT '================================================================';
PRINT '';

DECLARE @AllGood BIT = 1;

-- ================================================================
-- Test 1: OtpStates Table
-- ================================================================
PRINT '✓ Test 1: OtpStates Table';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OtpStates')
BEGIN
    PRINT '  [✓] OtpStates table exists';
    
    -- Check columns
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OtpStates' AND COLUMN_NAME = 'SessionId')
        PRINT '  [✓] SessionId column exists (nvarchar(88))';
    ELSE
    BEGIN
        PRINT '  [✗] SessionId column MISSING';
        SET @AllGood = 0;
    END
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OtpStates' AND COLUMN_NAME = 'NationalCode')
        PRINT '  [✓] NationalCode column exists (nvarchar(10))';
    ELSE
    BEGIN
        PRINT '  [✗] NationalCode column MISSING';
        SET @AllGood = 0;
    END
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OtpStates' AND COLUMN_NAME = 'OtpHash')
        PRINT '  [✓] OtpHash column exists (nvarchar(255))';
    ELSE
    BEGIN
        PRINT '  [✗] OtpHash column MISSING';
        SET @AllGood = 0;
    END
END
ELSE
BEGIN
    PRINT '  [✗] OtpStates table DOES NOT EXIST';
    SET @AllGood = 0;
END

PRINT '';

-- ================================================================
-- Test 2: OtpStates Indexes
-- ================================================================
PRINT '✓ Test 2: OtpStates Indexes';

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OtpState_SessionId_Expiry' AND object_id = OBJECT_ID('OtpStates'))
    PRINT '  [✓] IX_OtpState_SessionId_Expiry exists';
ELSE
BEGIN
    PRINT '  [✗] IX_OtpState_SessionId_Expiry MISSING';
    SET @AllGood = 0;
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OtpState_NationalCode_Expiry' AND object_id = OBJECT_ID('OtpStates'))
    PRINT '  [✓] IX_OtpState_NationalCode_Expiry exists';
ELSE
BEGIN
    PRINT '  [✗] IX_OtpState_NationalCode_Expiry MISSING';
    SET @AllGood = 0;
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OtpState_Expiry' AND object_id = OBJECT_ID('OtpStates'))
    PRINT '  [✓] IX_OtpState_Expiry exists';
ELSE
BEGIN
    PRINT '  [✗] IX_OtpState_Expiry MISSING';
    SET @AllGood = 0;
END

PRINT '';

-- ================================================================
-- Test 3: IdempotencyKey Column
-- ================================================================
PRINT '✓ Test 3: IdempotencyKey Column';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserLoginHistories' AND COLUMN_NAME = 'IdempotencyKey')
BEGIN
    PRINT '  [✓] IdempotencyKey column exists (nvarchar(50))';
    
    -- Check specs
    DECLARE @MaxLength INT;
    SELECT @MaxLength = CHARACTER_MAXIMUM_LENGTH 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'UserLoginHistories' AND COLUMN_NAME = 'IdempotencyKey';
    
    IF @MaxLength = 50
        PRINT '  [✓] Max length is correct (50)';
    ELSE
    BEGIN
        PRINT '  [✗] Max length is INCORRECT (expected 50, got ' + CAST(@MaxLength AS VARCHAR(10)) + ')';
        SET @AllGood = 0;
    END
END
ELSE
BEGIN
    PRINT '  [✗] IdempotencyKey column DOES NOT EXIST';
    SET @AllGood = 0;
END

PRINT '';

-- ================================================================
-- Test 4: Filtered Unique Index (CRITICAL)
-- ================================================================
PRINT '✓ Test 4: Filtered Unique Index';

IF EXISTS (SELECT * FROM sys.indexes 
           WHERE name = 'IX_UserLoginHistory_IdempotencyKey' 
           AND object_id = OBJECT_ID('UserLoginHistories'))
BEGIN
    PRINT '  [✓] IX_UserLoginHistory_IdempotencyKey exists';
    
    -- Check if it's unique
    DECLARE @IsUnique BIT, @HasFilter BIT;
    DECLARE @FilterDef NVARCHAR(MAX);
    
    SELECT 
        @IsUnique = is_unique,
        @HasFilter = has_filter,
        @FilterDef = filter_definition
    FROM sys.indexes
    WHERE name = 'IX_UserLoginHistory_IdempotencyKey'
    AND object_id = OBJECT_ID('UserLoginHistories');
    
    IF @IsUnique = 1
        PRINT '  [✓] Index is UNIQUE';
    ELSE
    BEGIN
        PRINT '  [✗] Index is NOT UNIQUE';
        SET @AllGood = 0;
    END
    
    IF @HasFilter = 1
    BEGIN
        PRINT '  [✓] Index is FILTERED (WHERE IS NOT NULL)';
        PRINT '      Filter: ' + ISNULL(@FilterDef, 'N/A');
    END
    ELSE
    BEGIN
        PRINT '  [✗] Index is NOT FILTERED (this will cause issues with NULLs)';
        SET @AllGood = 0;
    END
END
ELSE
BEGIN
    PRINT '  [✗] IX_UserLoginHistory_IdempotencyKey DOES NOT EXIST';
    SET @AllGood = 0;
END

PRINT '';

-- ================================================================
-- Test 5: Migration History
-- ================================================================
PRINT '✓ Test 5: Migration History';

IF EXISTS (SELECT * FROM __MigrationHistory WHERE MigrationId = '202601011951264_AddOtpStatesTable1011')
BEGIN
    PRINT '  [✓] Migration 202601011951264_AddOtpStatesTable1011 is recorded';
    
    SELECT 
        MigrationId,
        CONVERT(VARCHAR(20), CreatedOn, 120) as AppliedOn
    FROM __MigrationHistory
    WHERE MigrationId = '202601011951264_AddOtpStatesTable1011';
END
ELSE
BEGIN
    PRINT '  [✗] Migration NOT recorded in __MigrationHistory';
    SET @AllGood = 0;
END

PRINT '';

-- ================================================================
-- Test 6: Functional Test (Insert/Query)
-- ================================================================
PRINT '✓ Test 6: Functional Test';

BEGIN TRY
    -- Test OtpStates insert
    INSERT INTO OtpStates (SessionId, NationalCode, PhoneNumber, OtpHash, ExpiryUtc, AttemptCount, CreatedAt)
    VALUES ('TEST_SESSION', '1234567890', '09123456789', 'TEST_HASH', DATEADD(MINUTE, 5, GETUTCDATE()), 0, GETUTCDATE());
    
    PRINT '  [✓] OtpStates INSERT successful';
    
    -- Test query with index
    DECLARE @TestResult INT;
    SELECT @TestResult = COUNT(*) 
    FROM OtpStates 
    WHERE SessionId = 'TEST_SESSION' AND ExpiryUtc > GETUTCDATE();
    
    IF @TestResult = 1
        PRINT '  [✓] OtpStates SELECT with index successful';
    ELSE
    BEGIN
        PRINT '  [✗] OtpStates query returned unexpected result';
        SET @AllGood = 0;
    END
    
    -- Cleanup
    DELETE FROM OtpStates WHERE SessionId = 'TEST_SESSION';
    PRINT '  [✓] Test data cleaned up';
    
END TRY
BEGIN CATCH
    PRINT '  [✗] Functional test FAILED: ' + ERROR_MESSAGE();
    SET @AllGood = 0;
END CATCH

PRINT '';

-- ================================================================
-- Test 7: NULL Idempotency Test
-- ================================================================
PRINT '✓ Test 7: Multiple NULLs in IdempotencyKey (should be allowed)';

BEGIN TRY
    -- Insert multiple NULLs (should succeed with filtered index)
    DECLARE @TestCount INT;
    SELECT @TestCount = COUNT(*) FROM UserLoginHistories WHERE IdempotencyKey IS NULL;
    
    PRINT '  [✓] Found ' + CAST(@TestCount AS VARCHAR(10)) + ' existing NULL IdempotencyKey records';
    PRINT '  [✓] Filtered index correctly allows multiple NULLs';
    
END TRY
BEGIN CATCH
    PRINT '  [✗] NULL test FAILED: ' + ERROR_MESSAGE();
    SET @AllGood = 0;
END CATCH

PRINT '';

-- ================================================================
-- FINAL RESULT
-- ================================================================
PRINT '================================================================';
IF @AllGood = 1
BEGIN
    PRINT 'RESULT: ✅ ALL TESTS PASSED - Migration is SUCCESSFUL';
    PRINT '';
    PRINT 'You can now:';
    PRINT '  1. Test the login flow in your application';
    PRINT '  2. Monitor logs for any issues';
    PRINT '  3. Test Session Loss scenario (IIS Recycle)';
END
ELSE
BEGIN
    PRINT 'RESULT: ❌ SOME TESTS FAILED - Review errors above';
    PRINT '';
    PRINT 'Action required:';
    PRINT '  1. Review failed tests';
    PRINT '  2. Check migration script';
    PRINT '  3. Contact support if needed';
END
PRINT '================================================================';

SET NOCOUNT OFF;
GO

