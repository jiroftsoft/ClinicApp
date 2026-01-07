-- =============================================
-- ایجاد Gateway تست (Sandbox) برای Development
-- =============================================
-- هدف: تست پرداخت بدون خطر پرداخت واقعی
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
    MerchantId,
    IsActive,
    IsDefault,
    IsTestMode,
    CallbackUrl
FROM PaymentGateways
WHERE GatewayType = 1  -- ZarinPal (Enum value)
ORDER BY IsDefault DESC, IsTestMode DESC;
GO

-- ✅ STEP 2: ایجاد Gateway Sandbox (اگر وجود ندارد)
PRINT '🔧 ایجاد Gateway Sandbox...';

-- ⚠️ نکته: Merchant ID Sandbox را از پنل ZarinPal دریافت کنید
-- https://next.zarinpal.com/ → Sandbox → Merchant ID

IF NOT EXISTS (
    SELECT 1 
    FROM PaymentGateways 
    WHERE Name = N'زرین‌پال (Sandbox - تست)' 
    AND GatewayType = 1  -- ZarinPal (Enum value)
)
BEGIN
    INSERT INTO PaymentGateways (
        Name,
        GatewayType,
        MerchantId,
        ApiKey,
        GatewayUrl,
        CallbackUrl,
        IsActive,
        IsDefault,
        IsTestMode,  -- ✅ true = Sandbox
        Description,
        CreatedAt
    )
    VALUES (
        N'زرین‌پال (Sandbox - تست)',
        1,  -- ZarinPal (Enum value)
        N'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx',  -- ⚠️ جایگزین کنید با Merchant ID Sandbox
        N'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx',
        N'https://sandbox.zarinpal.com/pg/StartPay/',
        N'/Patient/AppointmentBooking/PaymentCallback',
        1,  -- IsActive = true
        0,  -- IsDefault = false (Production را پیش‌فرض نگه دارید)
        1,  -- IsTestMode = true (Sandbox)
        N'درگاه تست برای Development - استفاده از Sandbox ZarinPal',
        GETUTCDATE()
    );
    
    PRINT '✅ Gateway Sandbox ایجاد شد!';
END
ELSE
BEGIN
    PRINT '⚠️ Gateway Sandbox از قبل وجود دارد.';
END
GO

-- ✅ STEP 3: بررسی نتیجه
PRINT '📊 نتیجه نهایی:';
SELECT 
    PaymentGatewayId,
    Name,
    GatewayType,
    LEFT(MerchantId, 10) + '...' AS MerchantIdPreview,
    IsActive,
    IsDefault,
    IsTestMode,
    CallbackUrl,
    CreatedAt
FROM PaymentGateways
WHERE GatewayType = 1  -- ZarinPal (Enum value)
ORDER BY IsDefault DESC, IsTestMode DESC;
GO

-- ✅ STEP 4: راهنمای استفاده
PRINT '';
PRINT '📋 راهنمای استفاده:';
PRINT '1. Merchant ID Sandbox را از پنل ZarinPal دریافت کنید';
PRINT '2. در Script بالا جایگزین کنید: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
PRINT '3. Script را دوباره اجرا کنید';
PRINT '4. Gateway Sandbox برای تست استفاده می‌شود';
PRINT '';
PRINT '🔗 لینک پنل ZarinPal: https://next.zarinpal.com/';
PRINT '';

