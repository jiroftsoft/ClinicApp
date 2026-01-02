-- ✅ PRODUCTION FIX: Ensure user has Patient role
-- Run this if user 5369873054 doesn't have Patient role

USE ClinicAppDb;
GO

DECLARE @NationalCode NVARCHAR(10) = '5369873054';
DECLARE @UserId NVARCHAR(128);
DECLARE @PatientRoleId NVARCHAR(128);

-- Step 1: Find User
SELECT @UserId = Id 
FROM dbo.ApplicationUsers 
WHERE NationalCode = @NationalCode AND IsDeleted = 0;

IF @UserId IS NULL
BEGIN
    PRINT '❌ ERROR: User not found with NationalCode: ' + @NationalCode;
    RETURN;
END

PRINT '✅ User found: ' + @UserId;

-- Step 2: Find Patient Role
SELECT @PatientRoleId = Id 
FROM dbo.AspNetRoles 
WHERE Name = 'Patient';

IF @PatientRoleId IS NULL
BEGIN
    PRINT '❌ ERROR: Patient role not found in database';
    RETURN;
END

PRINT '✅ Patient role found: ' + @PatientRoleId;

-- Step 3: Check if user already has Patient role
IF EXISTS (
    SELECT 1 FROM dbo.AspNetUserRoles 
    WHERE UserId = @UserId AND RoleId = @PatientRoleId
)
BEGIN
    PRINT '✅ User already has Patient role - no action needed';
END
ELSE
BEGIN
    -- Step 4: Assign Patient role
    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @PatientRoleId);
    
    PRINT '✅ Patient role assigned successfully!';
    PRINT '';
    PRINT '⚠️ IMPORTANT: User MUST logout and login again to get new role claims!';
END

-- Step 5: Verify
PRINT '';
PRINT '=== VERIFICATION ===';
SELECT 
    au.NationalCode,
    au.FirstName + ' ' + au.LastName as FullName,
    r.Name as RoleName
FROM dbo.ApplicationUsers au
INNER JOIN dbo.AspNetUserRoles ur ON au.Id = ur.UserId
INNER JOIN dbo.AspNetRoles r ON ur.RoleId = r.Id
WHERE au.NationalCode = @NationalCode;

