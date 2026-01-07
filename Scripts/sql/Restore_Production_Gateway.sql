-- =============================================
-- بازگردانی Gateway Production
-- =============================================
-- هدف: فعال کردن Gateway Production و غیرفعال کردن Sandbox
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
    LEFT(MerchantId, 10) + '...' AS MerchantIdPreview,
    IsActive,
    IsDefault,
    IsTestMode,
    CallbackUrl
FROM PaymentGateways
WHERE GatewayType = 1  -- ZarinPal
    AND IsDeleted = 0
ORDER BY IsDefault DESC, IsTestMode DESC;
GO

-- ✅ STEP 2: فعال‌سازی Gateway Production
PRINT '🔧 فعال‌سازی Gateway Production...';

UPDATE PaymentGateways
SET 
    IsActive = 1,
    IsDefault = 1,
    UpdatedAt = GETUTCDATE()
WHERE GatewayType = 1  -- ZarinPal
    AND IsTestMode = 0  -- Production
    AND IsDeleted = 0;

IF @@ROWCOUNT > 0
    PRINT '✅ Gateway Production فعال شد!';
ELSE
    PRINT '⚠️ Gateway Production یافت نشد.';
GO

-- ✅ STEP 3: غیرفعال کردن Gateway Sandbox
PRINT '🔧 غیرفعال کردن Gateway Sandbox...';

UPDATE PaymentGateways
SET 
    IsActive = 0,
    IsDefault = 0,
    UpdatedAt = GETUTCDATE()
WHERE Name = N'زرین‌پال (Sandbox - تست)'
    AND GatewayType = 1  -- ZarinPal
    AND IsDeleted = 0;

IF @@ROWCOUNT > 0
    PRINT '✅ Gateway Sandbox غیرفعال شد.';
ELSE
    PRINT '⚠️ Gateway Sandbox یافت نشد.';
GO

-- ✅ STEP 4: بررسی نتیجه نهایی
PRINT '📊 نتیجه نهایی:';
SELECT 
    PaymentGatewayId,
    Name,
    LEFT(MerchantId, 10) + '...' AS MerchantIdPreview,
    IsActive,
    IsDefault,
    IsTestMode,
    CallbackUrl,
    UpdatedAt
FROM PaymentGateways
WHERE GatewayType = 1  -- ZarinPal
    AND IsDeleted = 0
ORDER BY IsDefault DESC, IsTestMode DESC;
GO

PRINT '';
PRINT '✅ Gateway Production آماده است!';
PRINT '';

