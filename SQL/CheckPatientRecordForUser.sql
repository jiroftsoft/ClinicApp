-- ✅ Check if Patient record exists for authenticated user
-- Usage: Replace '5369873054' with your NationalCode

PRINT '========================================='
PRINT '🔍 PATIENT RECORD CHECK'
PRINT '========================================='
PRINT ''

-- Step 1: Find your user by NationalCode
PRINT 'Step 1: Finding user...'
SELECT 
    Id AS UserId,
    UserName AS NationalCode,
    FirstName,
    LastName,
    PhoneNumber,
    Email,
    EmailConfirmed,
    IsDeleted
FROM AspNetUsers
WHERE UserName = '5369873054' -- ✅ Replace with your NationalCode
  AND IsDeleted = 0;

PRINT ''

-- Step 2: Check if Patient record exists for this user
-- Replace 'YOUR_USER_ID' with the Id from Step 1
DECLARE @UserId NVARCHAR(128) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = '5369873054' AND IsDeleted = 0);

SELECT 
    p.PatientId,
    p.ApplicationUserId,
    p.FirstName,
    p.LastName,
    p.NationalCode,
    p.PhoneNumber,
    p.Gender,
    p.CreatedAt,
    p.IsDeleted,
    u.UserName
FROM Patients p
INNER JOIN AspNetUsers u ON p.ApplicationUserId = u.Id
WHERE p.ApplicationUserId = @UserId
  AND p.IsDeleted = 0;

-- Step 3: Check user roles
SELECT 
    u.UserName AS NationalCode,
    r.Name AS RoleName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName = '5369873054'
  AND u.IsDeleted = 0;

-- Step 4: If Patient record doesn't exist, this query will show why
-- (User might not have Patient role)
IF NOT EXISTS (
    SELECT 1 FROM Patients p
    WHERE p.ApplicationUserId = @UserId AND p.IsDeleted = 0
)
BEGIN
    PRINT '⚠️ Patient record NOT found for this user!'
    
    -- Check if user has Patient role
    IF EXISTS (
        SELECT 1 FROM AspNetUserRoles ur
        INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
        WHERE ur.UserId = @UserId AND r.Name = 'Patient'
    )
    BEGIN
        PRINT '✅ User HAS Patient role - Record should be auto-created on next login or Patient area access'
    END
    ELSE
    BEGIN
        PRINT '❌ User does NOT have Patient role - Patient record will not be created'
        PRINT '    To fix: Assign Patient role to this user in Admin panel'
    END
END
ELSE
BEGIN
    PRINT '✅ Patient record EXISTS for this user'
END

