-- Verify PhoneNumber fix is working
USE ClinicDb;
GO

PRINT '=== Verification: OtpStates PhoneNumber Fix ==='
PRINT ''

-- 1. Check column definition
PRINT '1. Column Definition:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'OtpStates' AND COLUMN_NAME = 'PhoneNumber';
PRINT ''

-- 2. Check if any OTP records exist
PRINT '2. OTP Records (if any):'
SELECT TOP 5
    Id,
    NationalCode,
    PhoneNumber,
    LEN(PhoneNumber) as PhoneLength,
    ExpiryUtc,
    CreatedAt
FROM dbo.OtpStates
ORDER BY CreatedAt DESC;
PRINT ''

-- 3. Check user phone numbers that would have failed before
PRINT '3. Users with Phone Numbers > 11 chars:'
SELECT 
    UserName,
    PhoneNumber,
    LEN(PhoneNumber) as PhoneLength
FROM AspNetUsers
WHERE LEN(PhoneNumber) > 11;
PRINT ''

PRINT '=== Verification Complete ==='
GO

