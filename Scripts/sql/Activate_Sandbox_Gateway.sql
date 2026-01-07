-- =============================================
-- فعال‌سازی Gateway Sandbox برای تست
-- =============================================
-- هدف: فعال کردن Gateway Sandbox و غیرفعال کردن Production
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

-- ✅ STEP 2: فعال‌سازی Gateway Sandbox
PRINT '🔧 فعال‌سازی Gateway Sandbox...';

UPDATE PaymentGateways
SET 
    IsActive = 1,
    UpdatedAt = GETUTCDATE()
WHERE Name = N'زرین‌پال (Sandbox - تست)'
    AND GatewayType = 1  -- ZarinPal
    AND IsDeleted = 0;

IF @@ROWCOUNT > 0
    PRINT '✅ Gateway Sandbox فعال شد!';
ELSE
    PRINT '⚠️ Gateway Sandbox یافت نشد. ابتدا Script Create_Test_Gateway_Sandbox.sql را اجرا کنید.';
GO

-- ✅ STEP 3: غیرفعال کردن Gateway Production (موقتاً)
PRINT '🔧 غیرفعال کردن Gateway Production (موقتاً)...';

UPDATE PaymentGateways
SET 
    IsActive = 0,
    IsDefault = 0,
    UpdatedAt = GETUTCDATE()
WHERE GatewayType = 1  -- ZarinPal
    AND IsTestMode = 0  -- Production
    AND IsDeleted = 0;

IF @@ROWCOUNT > 0
    PRINT '✅ Gateway Production غیرفعال شد (موقتاً).';
ELSE
    PRINT '⚠️ Gateway Production یافت نشد.';
GO

-- ✅ STEP 4: تنظیم Gateway Sandbox به عنوان پیش‌فرض
PRINT '🔧 تنظیم Gateway Sandbox به عنوان پیش‌فرض...';

UPDATE PaymentGateways
SET 
    IsDefault = 1,
    UpdatedAt = GETUTCDATE()
WHERE Name = N'زرین‌پال (Sandbox - تست)'
    AND GatewayType = 1  -- ZarinPal
    AND IsDeleted = 0;

IF @@ROWCOUNT > 0
    PRINT '✅ Gateway Sandbox به عنوان پیش‌فرض تنظیم شد!';
ELSE
    PRINT '⚠️ Gateway Sandbox یافت نشد.';
GO

-- ✅ STEP 5: بررسی نتیجه نهایی
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
PRINT '✅ آماده برای تست!';
PRINT '⚠️ نکته: بعد از تست، Script Restore_Production_Gateway.sql را اجرا کنید.';
PRINT '';

