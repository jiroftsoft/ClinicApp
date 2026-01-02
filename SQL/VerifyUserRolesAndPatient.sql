-- ✅ DIAGNOSTIC SCRIPT: Verify User Roles and Patient Record
-- Run this to check if user 5369873054 has Patient role and record

USE ClinicAppDb;
GO

DECLARE @NationalCode NVARCHAR(10) = '5369873054';
DECLARE @UserId NVARCHAR(128);

-- Step 1: Find User
SELECT @UserId = Id 
FROM dbo.ApplicationUsers 
WHERE NationalCode = @NationalCode AND IsDeleted = 0;

PRINT '=== USER INFO ===';
SELECT 
    Id,
    UserName,
    NationalCode,
    FirstName,
    LastName,
    PhoneNumber,
    EmailConfirmed,
    PhoneNumberConfirmed,
    IsDeleted
FROM dbo.ApplicationUsers
WHERE NationalCode = @NationalCode;

-- Step 2: Check Roles
PRINT '';
PRINT '=== USER ROLES ===';
SELECT 
    r.Name as RoleName,
    r.Id as RoleId
FROM dbo.AspNetUserRoles ur
INNER JOIN dbo.AspNetRoles r ON ur.RoleId = r.Id
WHERE ur.UserId = @UserId;

-- Step 3: Check Patient Record
PRINT '';
PRINT '=== PATIENT RECORD ===';
SELECT 
    PatientId,
    ApplicationUserId,
    FirstName,
    LastName,
    NationalCode,
    PhoneNumber,
    CreatedAt,
    IsDeleted
FROM dbo.Patients
WHERE ApplicationUserId = @UserId;

-- Step 4: Summary
PRINT '';
PRINT '=== SUMMARY ===';
DECLARE @HasPatientRole BIT = 0;
DECLARE @HasPatientRecord BIT = 0;

IF EXISTS (
    SELECT 1 FROM dbo.AspNetUserRoles ur
    INNER JOIN dbo.AspNetRoles r ON ur.RoleId = r.Id
    WHERE ur.UserId = @UserId AND r.Name = 'Patient'
)
    SET @HasPatientRole = 1;

IF EXISTS (
    SELECT 1 FROM dbo.Patients
    WHERE ApplicationUserId = @UserId AND IsDeleted = 0
)
    SET @HasPatientRecord = 1;

SELECT 
    @UserId as UserId,
    @HasPatientRole as HasPatientRole,
    @HasPatientRecord as HasPatientRecord,
    CASE 
        WHEN @HasPatientRole = 1 AND @HasPatientRecord = 1 THEN '✅ OK - User is ready'
        WHEN @HasPatientRole = 0 THEN '❌ MISSING Patient Role'
        WHEN @HasPatientRecord = 0 THEN '❌ MISSING Patient Record'
        ELSE '❌ UNKNOWN ERROR'
    END as Status;

