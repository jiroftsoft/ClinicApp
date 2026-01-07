USE ClinicDb;
GO

-- ✅ ایجاد درگاه شبیه‌سازی شده برای تست و توسعه
-- این درگاه بدون نیاز به اتصال واقعی به درگاه پرداخت، فرآیند پرداخت را شبیه‌سازی می‌کند

-- بررسی وجود Gateway قبلی
IF EXISTS (SELECT 1 FROM PaymentGateways WHERE GatewayType = 99 AND IsDeleted = 0)
BEGIN
    PRINT '⚠️ درگاه شبیه‌سازی شده قبلاً وجود دارد. در حال به‌روزرسانی...';
    
    UPDATE PaymentGateways
    SET 
        Name = N'درگاه شبیه‌سازی شده (تست)',
        Description = N'درگاه شبیه‌سازی شده برای تست و توسعه - بدون نیاز به اتصال واقعی',
        GatewayUrl = N'/Payment/SimulatedGateway/Process',
        IsTestMode = 1, -- Test Mode
        IsActive = 1,   -- فعال
        IsDefault = 0,  -- پیش‌فرض نیست (می‌توانید تغییر دهید)
        CallbackUrl = N'/Patient/AppointmentBooking/PaymentCallback',
        UpdatedAt = GETUTCDATE()
    WHERE GatewayType = 99 AND IsDeleted = 0;
    
    PRINT '✅ درگاه شبیه‌سازی شده به‌روزرسانی شد.';
END
ELSE
BEGIN
    PRINT '✅ در حال ایجاد درگاه شبیه‌سازی شده...';
    
    INSERT INTO PaymentGateways (
        Name,
        GatewayType,
        Description,
        MerchantId,
        ApiKey,
        GatewayUrl,
        IsTestMode,
        IsActive,
        IsDefault,
        IsDeleted,
        CallbackUrl,
        CreatedAt,
        UpdatedAt
    )
    VALUES (
        N'درگاه شبیه‌سازی شده (تست)',           -- Name
        99,                                      -- GatewayType (Simulated)
        N'درگاه شبیه‌سازی شده برای تست و توسعه - بدون نیاز به اتصال واقعی به درگاه پرداخت', -- Description
        N'SIMULATED-MERCHANT-ID',                -- MerchantId (شبیه‌سازی شده)
        N'SIMULATED-API-KEY',                    -- ApiKey (شبیه‌سازی شده)
        N'/Payment/SimulatedGateway/Process',     -- GatewayUrl
        1,                                       -- IsTestMode (true)
        1,                                       -- IsActive (true)
        0,                                       -- IsDefault (false - می‌توانید به 1 تغییر دهید)
        0,                                       -- IsDeleted (false)
        N'/Patient/AppointmentBooking/PaymentCallback', -- CallbackUrl
        GETUTCDATE(),                            -- CreatedAt
        GETUTCDATE()                             -- UpdatedAt
    );
    
    PRINT '✅ درگاه شبیه‌سازی شده با موفقیت ایجاد شد.';
END
GO

-- نمایش اطلاعات Gateway ایجاد شده
SELECT 
    PaymentGatewayId,
    Name,
    GatewayType,
    Description,
    IsTestMode,
    IsActive,
    IsDefault,
    GatewayUrl,
    CallbackUrl,
    CreatedAt,
    UpdatedAt
FROM PaymentGateways
WHERE GatewayType = 99 AND IsDeleted = 0;
GO

PRINT '';
PRINT '========================================';
PRINT '✅ درگاه شبیه‌سازی شده آماده استفاده است!';
PRINT '========================================';
PRINT '';
PRINT '📝 نکات مهم:';
PRINT '   1. برای استفاده از این درگاه، IsDefault را به 1 تغییر دهید';
PRINT '   2. یا در UI، این درگاه را به عنوان درگاه پیش‌فرض انتخاب کنید';
PRINT '   3. این درگاه همیشه موفق برمی‌گرداند (برای تست)';
PRINT '   4. برای Production، از درگاه واقعی استفاده کنید';
PRINT '';
GO

