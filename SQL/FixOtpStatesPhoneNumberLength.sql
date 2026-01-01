-- Fix OtpStates.PhoneNumber column length from 11 to 20
-- This fixes validation error when phone numbers include country code (+98...)

USE ClinicDb;
GO

PRINT '=== Fixing OtpStates.PhoneNumber Column Length ==='
PRINT ''

-- Check current column definition
PRINT '1. Current column definition:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'OtpStates' AND COLUMN_NAME = 'PhoneNumber';
PRINT ''

-- Alter column to nvarchar(20)
PRINT '2. Altering column to nvarchar(20)...'
ALTER TABLE dbo.OtpStates
ALTER COLUMN PhoneNumber nvarchar(20) NOT NULL;
PRINT '   ✓ Column altered successfully'
PRINT ''

-- Verify new column definition
PRINT '3. New column definition:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'OtpStates' AND COLUMN_NAME = 'PhoneNumber';
PRINT ''

PRINT '=== Fix Complete ==='
PRINT 'OtpStates.PhoneNumber now supports up to 20 characters (e.g., +989136381995)'
GO

