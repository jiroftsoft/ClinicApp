-- =============================================
-- به‌روزرسانی Gateway Production با Merchant ID واقعی
-- =============================================
-- Merchant ID: 156be6cd-e0a4-4af8-9113-83647771376f
-- Domain: mehranyad.ir
-- تاریخ: 2026-01-06
-- =============================================

USE ClinicDb;
GO

-- ✅ STEP 1: بررسی Gateway های موجود
PRINT '🔍 بررسی Gateway های موجود...';
SELECT 
    PaymentGatewayId,
    Name,
    GatewayType,
    LEFT(MerchantId, 20) + '...' AS MerchantIdPreview,
    IsActive,
    IsDefault,
    IsTestMode,
    CallbackUrl,
    GatewayUrl
FROM PaymentGateways
WHERE GatewayType = 1  -- ZarinPal
    AND IsDeleted = 0
ORDER BY IsDefault DESC, IsTestMode DESC;
GO

-- ✅ STEP 2: به‌روزرسانی Gateway Production (اگر وجود دارد)
PRINT '🔧 به‌روزرسانی Gateway Production...';

UPDATE PaymentGateways
SET 
    Name = N'زرین‌پال Production',
    MerchantId = N'156be6cd-e0a4-4af8-9113-83647771376f',
    ApiKey = N'156be6cd-e0a4-4af8-9113-83647771376f',
    GatewayUrl = N'https://www.zarinpal.com/pg/StartPay/',
    IsTestMode = 0,  -- Production
    IsActive = 1,
    IsDefault = 1,
    CallbackUrl = N'/Patient/AppointmentBooking/PaymentCallback',
    Description = N'درگاه پرداخت زرین‌پال Production - mehranyad.ir',
    UpdatedAt = GETUTCDATE()
WHERE GatewayType = 1  -- ZarinPal
    AND IsTestMode = 0  -- Production
    AND IsDeleted = 0;

IF @@ROWCOUNT > 0
    PRINT '✅ Gateway Production به‌روزرسانی شد!';
ELSE
    PRINT '⚠️ Gateway Production یافت نشد. در حال ایجاد...';
GO

-- ✅ STEP 3: ایجاد Gateway Production (اگر وجود ندارد)
IF NOT EXISTS (
    SELECT 1 
    FROM PaymentGateways 
    WHERE GatewayType = 1 
        AND IsTestMode = 0 
        AND IsDeleted = 0
)
BEGIN
    PRINT '🔧 ایجاد Gateway Production...';
    
    -- پاک کردن Gateway های پیش‌فرض قبلی
    UPDATE PaymentGateways
    SET IsDefault = 0
    WHERE GatewayType = 1 AND IsDeleted = 0;
    
    INSERT INTO PaymentGateways (
        Name,
        GatewayType,
        MerchantId,
        ApiKey,
        GatewayUrl,
        CallbackUrl,
        IsActive,
        IsDefault,
        IsTestMode,
        Description,
        CreatedAt
    )
    VALUES (
        N'زرین‌پال Production',
        1,  -- ZarinPal
        N'156be6cd-e0a4-4af8-9113-83647771376f',
        N'156be6cd-e0a4-4af8-9113-83647771376f',
        N'https://www.zarinpal.com/pg/StartPay/',
        N'/Patient/AppointmentBooking/PaymentCallback',
        1,  -- IsActive = true
        1,  -- IsDefault = true
        0,  -- IsTestMode = false (Production)
        N'درگاه پرداخت زرین‌پال Production - mehranyad.ir',
        GETUTCDATE()
    );
    
    PRINT '✅ Gateway Production ایجاد شد!';
END
GO

-- ✅ STEP 4: بررسی نتیجه نهایی
PRINT '📊 نتیجه نهایی:';
SELECT 
    PaymentGatewayId,
    Name,
    GatewayType,
    LEFT(MerchantId, 20) + '...' AS MerchantIdPreview,
    IsActive,
    IsDefault,
    IsTestMode,
    CallbackUrl,
    GatewayUrl,
    UpdatedAt
FROM PaymentGateways
WHERE GatewayType = 1  -- ZarinPal
    AND IsDeleted = 0
ORDER BY IsDefault DESC, IsTestMode DESC;
GO

PRINT '';
PRINT '✅ Gateway Production آماده است!';
PRINT '⚠️ نکته: Application را Restart کنید.';
PRINT '';

