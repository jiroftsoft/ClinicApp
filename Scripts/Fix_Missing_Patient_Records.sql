/*
 * Fix Missing Patient Records
 * 
 * Purpose: Create Patient records for Users with "Patient" role but no Patient record
 * Date: 2026-01-02
 * Severity: HIGH
 * 
 * This script will:
 * 1. Find all Users with "Patient" role
 * 2. Check if they have a Patient record
 * 3. Create missing Patient records with data from AspNetUsers
 * 
 * IMPORTANT: Review before running in production!
 */

-- ✅ Step 1: Identify the problem
SELECT 
    u.Id AS UserId,
    u.UserName,
    u.Email,
    u.PhoneNumber,
    CASE WHEN p.PatientId IS NULL THEN 'MISSING' ELSE 'OK' END AS PatientStatus,
    p.PatientId
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id AND r.Name = 'Patient'
LEFT JOIN Patients p ON u.Id = p.ApplicationUserId AND p.IsDeleted = 0
WHERE u.IsDeleted = 0
ORDER BY PatientStatus DESC, u.UserName;

-- ✅ Step 2: Create missing Patient records
INSERT INTO Patients (
    ApplicationUserId,
    NationalCode,
    FirstName,
    LastName,
    PhoneNumber,
    Email,
    CreatedAt,
    CreatedByUserId,
    IsDeleted
)
SELECT 
    u.Id,
    ISNULL(u.PhoneNumber, '0000000000') AS NationalCode,  -- ⚠️ Placeholder - باید بعداً توسط user تکمیل شود
    ISNULL(
        SUBSTRING(u.UserName, 1, CHARINDEX(' ', u.UserName + ' ') - 1),
        u.UserName
    ) AS FirstName,
    ISNULL(
        SUBSTRING(u.UserName, CHARINDEX(' ', u.UserName + ' ') + 1, LEN(u.UserName)),
        ''
    ) AS LastName,
    u.PhoneNumber,
    u.Email,
    GETDATE(),
    u.Id,
    0
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id AND r.Name = 'Patient'
LEFT JOIN Patients p ON u.Id = p.ApplicationUserId AND p.IsDeleted = 0
WHERE u.IsDeleted = 0 
  AND p.PatientId IS NULL;

-- ✅ Step 3: Verify
SELECT 
    u.Id AS UserId,
    u.UserName,
    p.PatientId,
    p.FirstName,
    p.LastName,
    p.NationalCode,
    p.CreatedAt
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id AND r.Name = 'Patient'
INNER JOIN Patients p ON u.Id = p.ApplicationUserId AND p.IsDeleted = 0
WHERE u.IsDeleted = 0
ORDER BY p.CreatedAt DESC;

-- ✅ Step 4: Check dashboard access
PRINT '✅ Missing Patient records created successfully!';
PRINT 'ℹ️ Users should now be able to access Patient Dashboard.';
PRINT '⚠️ Users with placeholder NationalCode (0000000000) should update their profile.';

