-- ✅ به‌روزرسانی Gateway برای Production
-- تاریخ: 2026-01-06
-- هدف: تغییر Gateway از Sandbox به Production

USE ClinicDb;
GO

-- بررسی Gateway فعلی
SELECT 
    PaymentGatewayId,
    Name,
    GatewayType,
    MerchantId,
    GatewayUrl,
    IsActive,
    IsDefault,
    IsTestMode,
    IsDeleted,
    CreatedAt
FROM PaymentGateways
WHERE PaymentGatewayId = 2;
GO

-- ✅ به‌روزرسانی Gateway به Production
UPDATE PaymentGateways
SET 
    Name = N'زرین‌پال Production',
    GatewayUrl = N'https://www.zarinpal.com/pg/StartPay/',
    IsTestMode = 0, -- false (Production)
    IsDefault = 1,  -- true
    UpdatedAt = GETUTCDATE()
WHERE PaymentGatewayId = 2;
GO

-- بررسی نتیجه
SELECT 
    PaymentGatewayId,
    Name,
    GatewayType,
    MerchantId,
    GatewayUrl,
    IsActive,
    IsDefault,
    IsTestMode,
    IsDeleted,
    UpdatedAt
FROM PaymentGateways
WHERE PaymentGatewayId = 2;
GO

-- ✅ بررسی تمام Gateway های فعال
SELECT 
    PaymentGatewayId,
    Name,
    GatewayType,
    MerchantId,
    GatewayUrl,
    IsActive,
    IsDefault,
    IsTestMode,
    IsDeleted
FROM PaymentGateways
WHERE IsDeleted = 0
ORDER BY IsDefault DESC, CreatedAt DESC;
GO

