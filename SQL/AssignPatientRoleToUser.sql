-- ✅ Assign Patient role to existing user
-- Usage: Run this to assign Patient role to a user who doesn't have it

DECLARE @NationalCode NVARCHAR(10) = '5369873054'; -- ✅ Replace with your NationalCode
DECLARE @UserId NVARCHAR(128);
DECLARE @PatientRoleId NVARCHAR(128);

-- Step 1: Find user
SELECT @UserId = Id
FROM AspNetUsers
WHERE UserName = @NationalCode
  AND IsDeleted = 0;

IF @UserId IS NULL
BEGIN
    PRINT '❌ User not found with NationalCode: ' + @NationalCode
    RETURN;
END

-- Step 2: Find Patient role
SELECT @PatientRoleId = Id
FROM AspNetRoles
WHERE Name = 'Patient';

IF @PatientRoleId IS NULL
BEGIN
    PRINT '❌ Patient role not found in system'
    RETURN;
END

-- Step 3: Check if user already has Patient role
IF EXISTS (
    SELECT 1 FROM AspNetUserRoles
    WHERE UserId = @UserId AND RoleId = @PatientRoleId
)
BEGIN
    PRINT '✅ User already has Patient role'
    SELECT 
        u.UserName AS NationalCode,
        u.FirstName + ' ' + u.LastName AS FullName,
        r.Name AS RoleName
    FROM AspNetUsers u
    INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE u.Id = @UserId;
    RETURN;
END

-- Step 4: Assign Patient role
BEGIN TRANSACTION;

BEGIN TRY
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @PatientRoleId);

    COMMIT TRANSACTION;

    PRINT '✅ Patient role assigned successfully!'
    PRINT ''
    PRINT 'User Details:'
    SELECT 
        u.UserName AS NationalCode,
        u.FirstName + ' ' + u.LastName AS FullName,
        u.Email,
        u.PhoneNumber
    FROM AspNetUsers u
    WHERE u.Id = @UserId;
    
    PRINT ''
    PRINT 'All Roles for this user:'
    SELECT 
        r.Name AS RoleName,
        ur.UserId
    FROM AspNetUserRoles ur
    INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE ur.UserId = @UserId;

    PRINT ''
    PRINT '⚠️ IMPORTANT: User needs to logout and login again for role to take effect'
    PRINT '   Or Patient record will be auto-created on next Patient area access'
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '❌ Error assigning Patient role:'
    PRINT ERROR_MESSAGE();
END CATCH

