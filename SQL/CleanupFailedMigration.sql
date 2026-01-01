-- ================================================================
-- Cleanup Failed Migration: 202601011951264_AddOtpStatesTable1011
-- Purpose: پاکسازی تغییرات نیمه‌تمام قبل از اجرای مجدد
-- ================================================================

USE ClinicDb;
GO

PRINT '================================================================';
PRINT 'CLEANUP FAILED MIGRATION';
PRINT 'Date: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
PRINT '================================================================';
PRINT '';

-- ================================================================
-- STEP 1: بررسی وضعیت فعلی
-- ================================================================
PRINT '1. Checking current state...';

DECLARE @OtpStatesExists BIT = 0;
DECLARE @IdempotencyKeyExists BIT = 0;
DECLARE @UniqueIndexExists BIT = 0;

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OtpStates')
    SET @OtpStatesExists = 1;

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'UserLoginHistories' AND COLUMN_NAME = 'IdempotencyKey')
    SET @IdempotencyKeyExists = 1;

IF EXISTS (SELECT * FROM sys.indexes 
           WHERE name = 'IX_UserLoginHistory_IdempotencyKey' 
           AND object_id = OBJECT_ID('UserLoginHistories'))
    SET @UniqueIndexExists = 1;

PRINT '   OtpStates table exists: ' + CASE WHEN @OtpStatesExists = 1 THEN 'YES' ELSE 'NO' END;
PRINT '   IdempotencyKey column exists: ' + CASE WHEN @IdempotencyKeyExists = 1 THEN 'YES' ELSE 'NO' END;
PRINT '   Unique Index exists: ' + CASE WHEN @UniqueIndexExists = 1 THEN 'YES' ELSE 'NO' END;
PRINT '';

-- ================================================================
-- STEP 2: پاکسازی (فقط اگر لازم باشد)
-- ================================================================
PRINT '2. Starting cleanup...';

-- حذف Unique Index (اگر وجود دارد)
IF @UniqueIndexExists = 1
BEGIN
    PRINT '   [!] Dropping existing Unique Index...';
    DROP INDEX [IX_UserLoginHistory_IdempotencyKey] ON [dbo].[UserLoginHistories];
    PRINT '   [✓] Unique Index dropped';
END

-- حذف ستون IdempotencyKey (اگر وجود دارد)
IF @IdempotencyKeyExists = 1
BEGIN
    PRINT '   [!] Dropping IdempotencyKey column...';
    ALTER TABLE [dbo].[UserLoginHistories] DROP COLUMN [IdempotencyKey];
    PRINT '   [✓] IdempotencyKey column dropped';
END

-- حذف Index های OtpStates (اگر وجود دارند)
IF @OtpStatesExists = 1
BEGIN
    PRINT '   [!] Dropping OtpStates indexes...';
    
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OtpState_Expiry' AND object_id = OBJECT_ID('OtpStates'))
        DROP INDEX [IX_OtpState_Expiry] ON [dbo].[OtpStates];
    
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OtpState_NationalCode_Expiry' AND object_id = OBJECT_ID('OtpStates'))
        DROP INDEX [IX_OtpState_NationalCode_Expiry] ON [dbo].[OtpStates];
    
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OtpState_SessionId_Expiry' AND object_id = OBJECT_ID('OtpStates'))
        DROP INDEX [IX_OtpState_SessionId_Expiry] ON [dbo].[OtpStates];
    
    PRINT '   [✓] OtpStates indexes dropped';
END

-- حذف جدول OtpStates (اگر وجود دارد)
IF @OtpStatesExists = 1
BEGIN
    PRINT '   [!] Dropping OtpStates table...';
    DROP TABLE [dbo].[OtpStates];
    PRINT '   [✓] OtpStates table dropped';
END

-- حذف رکورد Migration از __MigrationHistory (اگر وجود دارد)
IF EXISTS (SELECT * FROM [dbo].[__MigrationHistory] 
           WHERE [MigrationId] = '202601011951264_AddOtpStatesTable1011')
BEGIN
    PRINT '   [!] Removing migration from history...';
    DELETE FROM [dbo].[__MigrationHistory] 
    WHERE [MigrationId] = '202601011951264_AddOtpStatesTable1011';
    PRINT '   [✓] Migration removed from history';
END

PRINT '';
PRINT '================================================================';
PRINT 'CLEANUP COMPLETE - Ready for fresh migration';
PRINT '================================================================';
GO

