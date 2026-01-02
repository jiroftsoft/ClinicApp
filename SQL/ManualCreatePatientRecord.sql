-- ✅ Manually create Patient record for existing authenticated user
-- Usage: Run this if Patient record doesn't exist and you want to create it immediately
-- Note: The system will auto-create on next access, but this is for immediate fix

DECLARE @NationalCode NVARCHAR(10) = '5369873054'; -- Replace with your NationalCode
DECLARE @UserId NVARCHAR(128);

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

-- Step 2: Check if Patient record already exists
IF EXISTS (SELECT 1 FROM Patients WHERE ApplicationUserId = @UserId AND IsDeleted = 0)
BEGIN
    PRINT '✅ Patient record already exists for this user'
    SELECT 
        PatientId,
        ApplicationUserId,
        FirstName,
        LastName,
        NationalCode,
        CreatedAt
    FROM Patients
    WHERE ApplicationUserId = @UserId AND IsDeleted = 0;
    RETURN;
END

-- Step 3: Check if user has Patient role
IF NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles ur
    INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE ur.UserId = @UserId AND r.Name = 'Patient'
)
BEGIN
    PRINT '❌ User does NOT have Patient role - Cannot create Patient record'
    PRINT '   To fix: Assign Patient role first in Admin panel'
    RETURN;
END

-- Step 4: Create Patient record
BEGIN TRANSACTION;

BEGIN TRY
    INSERT INTO Patients (
        ApplicationUserId,
        FirstName,
        LastName,
        NationalCode,
        PhoneNumber,
        Gender,
        CreatedAt,
        UpdatedByUserId,
        IsDeleted
    )
    SELECT 
        u.Id,
        u.FirstName,
        u.LastName,
        u.NationalCode,
        u.PhoneNumber,
        u.Gender,
        GETUTCDATE(),
        u.Id,
        0
    FROM AspNetUsers u
    WHERE u.Id = @UserId;

    COMMIT TRANSACTION;

    PRINT '✅ Patient record created successfully!'
    
    -- Show the created record
    SELECT 
        PatientId,
        ApplicationUserId,
        FirstName,
        LastName,
        NationalCode,
        PhoneNumber,
        Gender,
        CreatedAt
    FROM Patients
    WHERE ApplicationUserId = @UserId AND IsDeleted = 0;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '❌ Error creating Patient record:'
    PRINT ERROR_MESSAGE();
END CATCH

