-- ================================================================
-- Schema Verification Script for Authentication Module
-- Purpose: Check if OtpStates table and IdempotencyKey exist
-- Run this BEFORE Update-Database to understand current state
-- ================================================================

USE ClinicDb;
GO

PRINT '================================================================';
PRINT 'AUTHENTICATION MODULE SCHEMA VERIFICATION';
PRINT 'Date: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
PRINT '================================================================';
PRINT '';

-- ================================================================
-- CHECK 1: OtpStates Table
-- ================================================================
PRINT '1. Checking OtpStates table...';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OtpStates')
BEGIN
    PRINT '   [✓] OtpStates table EXISTS';
    
    -- Count records
    DECLARE @OtpCount INT;
    SELECT @OtpCount = COUNT(*) FROM OtpStates;
    PRINT '   [i] Current record count: ' + CAST(@OtpCount AS VARCHAR(10));
    
    -- Check structure
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH,
        IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'OtpStates'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT '   [✗] OtpStates table DOES NOT EXIST';
    PRINT '   [!] Action Required: Run Migration 202601011200000_AddOtpStatesTable';
END

PRINT '';

-- ================================================================
-- CHECK 2: UserLoginHistories.IdempotencyKey Column
-- ================================================================
PRINT '2. Checking UserLoginHistories.IdempotencyKey column...';

IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'UserLoginHistories' 
    AND COLUMN_NAME = 'IdempotencyKey'
)
BEGIN
    PRINT '   [✓] IdempotencyKey column EXISTS';
    
    -- Check if index exists
    IF EXISTS (
        SELECT * FROM sys.indexes 
        WHERE name = 'IX_UserLoginHistory_IdempotencyKey'
        AND object_id = OBJECT_ID('UserLoginHistories')
    )
    BEGIN
        PRINT '   [✓] Unique index IX_UserLoginHistory_IdempotencyKey EXISTS';
    END
    ELSE
    BEGIN
        PRINT '   [✗] Unique index IX_UserLoginHistory_IdempotencyKey MISSING';
    END
END
ELSE
BEGIN
    PRINT '   [✗] IdempotencyKey column DOES NOT EXIST';
    PRINT '   [!] Action Required: Run Migration 202601011200001_AddIdempotencyKeyToUserLoginHistory';
END

PRINT '';

-- ================================================================
-- CHECK 3: Related Tables (Sanity Check)
-- ================================================================
PRINT '3. Checking related tables...';

-- UserLoginHistories
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserLoginHistories')
BEGIN
    DECLARE @LoginHistoryCount INT;
    SELECT @LoginHistoryCount = COUNT(*) FROM UserLoginHistories;
    PRINT '   [✓] UserLoginHistories exists (' + CAST(@LoginHistoryCount AS VARCHAR(10)) + ' records)';
END
ELSE
BEGIN
    PRINT '   [✗] UserLoginHistories MISSING (CRITICAL ERROR)';
END

-- OtpRequests
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OtpRequests')
BEGIN
    DECLARE @OtpRequestCount INT;
    SELECT @OtpRequestCount = COUNT(*) FROM OtpRequests;
    PRINT '   [✓] OtpRequests exists (' + CAST(@OtpRequestCount AS VARCHAR(10)) + ' records)';
END
ELSE
BEGIN
    PRINT '   [✗] OtpRequests MISSING';
END

PRINT '';

-- ================================================================
-- CHECK 4: Pending Migrations (from __MigrationHistory)
-- ================================================================
PRINT '4. Checking migration history...';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory')
BEGIN
    -- Check if our migrations are applied
    IF EXISTS (
        SELECT * FROM __MigrationHistory 
        WHERE MigrationId LIKE '%AddOtpStatesTable%'
    )
    BEGIN
        PRINT '   [✓] Migration: AddOtpStatesTable is APPLIED';
    END
    ELSE
    BEGIN
        PRINT '   [!] Migration: AddOtpStatesTable is PENDING';
    END
    
    IF EXISTS (
        SELECT * FROM __MigrationHistory 
        WHERE MigrationId LIKE '%AddIdempotencyKeyToUserLoginHistory%'
    )
    BEGIN
        PRINT '   [✓] Migration: AddIdempotencyKeyToUserLoginHistory is APPLIED';
    END
    ELSE
    BEGIN
        PRINT '   [!] Migration: AddIdempotencyKeyToUserLoginHistory is PENDING';
    END
    
    -- Show last 5 migrations
    PRINT '';
    PRINT '   Last 5 applied migrations:';
    SELECT TOP 5 
        MigrationId,
        CONVERT(VARCHAR(20), CreatedOn, 120) as AppliedOn
    FROM __MigrationHistory
    ORDER BY CreatedOn DESC;
END
ELSE
BEGIN
    PRINT '   [✗] __MigrationHistory table not found (database may be corrupted)';
END

PRINT '';
PRINT '================================================================';
PRINT 'VERIFICATION COMPLETE';
PRINT '================================================================';
GO

