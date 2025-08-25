-- =========================================
-- Create System User for Medical Environment
-- ایجاد کاربر سیستم برای محیط درمانی
-- =========================================

-- Check if system user already exists
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 'system' OR UserName = '3031945451')
BEGIN
    -- Insert system user
    INSERT INTO AspNetUsers (
        Id,
        UserName,
        FullName,
        NationalCode,
        PhoneNumber,
        IsActive,
        IsDeleted,
        CreatedAt,
        CreatedByUserId,
        UpdatedAt,
        Email,
        EmailConfirmed,
        PasswordHash,
        SecurityStamp,
        PhoneNumberConfirmed,
        TwoFactorEnabled,
        LockoutEndDateUtc,
        LockoutEnabled,
        AccessFailedCount,
        UserNameForDisplay
    ) VALUES (
        'db1f8e8c-da85-455d-b0a7-aacf90f639af', -- System User ID
        'system',                               -- Username
        'System',                               -- Full Name
        '3031945451',                           -- National Code
        '09022487373',                          -- Phone Number
        1,                                      -- IsActive
        0,                                      -- IsDeleted
        GETUTCDATE(),                           -- CreatedAt
        'db1f8e8c-da85-455d-b0a7-aacf90f639af', -- CreatedByUserId (self)
        GETUTCDATE(),                           -- UpdatedAt
        'system@clinic.com',                    -- Email
        0,                                      -- EmailConfirmed
        NULL,                                   -- PasswordHash (no password for system user)
        'b59da7dd-b8b6-49f3-943f-4f3de8740d29', -- SecurityStamp
        0,                                      -- PhoneNumberConfirmed
        0,                                      -- TwoFactorEnabled
        NULL,                                   -- LockoutEndDateUtc
        0,                                      -- LockoutEnabled
        0,                                      -- AccessFailedCount
        '3031945451'                            -- UserNameForDisplay
    );
    
    PRINT 'System user created successfully.';
END
ELSE
BEGIN
    PRINT 'System user already exists.';
END

-- Verify system user exists
SELECT 
    Id,
    UserName,
    FullName,
    NationalCode,
    PhoneNumber,
    IsActive,
    IsDeleted,
    Email
FROM AspNetUsers 
WHERE UserName = 'system' OR UserName = '3031945451';
