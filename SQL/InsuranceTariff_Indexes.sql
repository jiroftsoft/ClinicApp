-- =============================================
-- Database Indexes for InsuranceTariff Performance Optimization
-- طراحی شده برای سیستم‌های پزشکی کلینیک شفا
-- =============================================

-- Index برای جستجوی سریع بر اساس ServiceId و InsurancePlanId
-- این index برای جلوگیری از N+1 queries و بهبود performance
CREATE NONCLUSTERED INDEX [IX_InsuranceTariffs_ServiceId_InsurancePlanId] 
ON [dbo].[InsuranceTariffs] ([ServiceId], [InsurancePlanId])
INCLUDE ([InsuranceTariffId], [TariffPrice], [PatientShare], [InsurerShare], [IsActive], [IsDeleted])
WHERE [IsDeleted] = 0;

-- Index برای فیلتر بر اساس InsurancePlanId
-- بهبود performance برای لیست‌های تعرفه بر اساس طرح بیمه
CREATE NONCLUSTERED INDEX [IX_InsuranceTariffs_InsurancePlanId_IsActive] 
ON [dbo].[InsuranceTariffs] ([InsurancePlanId], [IsActive])
INCLUDE ([InsuranceTariffId], [ServiceId], [TariffPrice], [PatientShare], [InsurerShare], [CreatedAt])
WHERE [IsDeleted] = 0;

-- Index برای جستجوی متنی در Service Title
-- بهبود performance برای جستجو در نام خدمات
CREATE NONCLUSTERED INDEX [IX_InsuranceTariffs_SearchTerm] 
ON [dbo].[InsuranceTariffs] ([IsDeleted], [IsActive])
INCLUDE ([InsuranceTariffId], [ServiceId], [InsurancePlanId], [TariffPrice])
WHERE [IsDeleted] = 0;

-- Index برای آمارگیری و گزارش‌گیری
-- بهبود performance برای GetStatisticsProjectionAsync
CREATE NONCLUSTERED INDEX [IX_InsuranceTariffs_Statistics] 
ON [dbo].[InsuranceTariffs] ([IsDeleted], [IsActive], [CreatedAt])
INCLUDE ([InsuranceTariffId], [ServiceId], [InsurancePlanId], [TariffPrice])
WHERE [IsDeleted] = 0;

-- Index برای مرتب‌سازی بر اساس تاریخ ایجاد
-- بهبود performance برای OrderBy CreatedAt
CREATE NONCLUSTERED INDEX [IX_InsuranceTariffs_CreatedAt] 
ON [dbo].[InsuranceTariffs] ([CreatedAt] DESC)
INCLUDE ([InsuranceTariffId], [ServiceId], [InsurancePlanId], [IsActive], [IsDeleted])
WHERE [IsDeleted] = 0;

-- Index برای فیلتر بر اساس InsuranceProviderId
-- بهبود performance برای جستجو بر اساس ارائه‌دهنده بیمه
CREATE NONCLUSTERED INDEX [IX_InsuranceTariffs_ProviderId] 
ON [dbo].[InsuranceTariffs] ([InsurancePlanId])
INCLUDE ([InsuranceTariffId], [ServiceId], [TariffPrice], [PatientShare], [InsurerShare], [IsActive])
WHERE [IsDeleted] = 0;

-- Index برای Bulk Operations
-- بهبود performance برای عملیات گروهی
CREATE NONCLUSTERED INDEX [IX_InsuranceTariffs_BulkOperations] 
ON [dbo].[InsuranceTariffs] ([IsActive], [IsDeleted])
INCLUDE ([InsuranceTariffId], [ServiceId], [InsurancePlanId], [UpdatedAt])
WHERE [IsDeleted] = 0;

-- =============================================
-- Performance Monitoring Queries
-- =============================================

-- بررسی performance indexes
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates,
    s.last_user_seek,
    s.last_user_scan,
    s.last_user_lookup
FROM sys.indexes i
LEFT JOIN sys.dm_db_index_usage_stats s ON i.object_id = s.object_id AND i.index_id = s.index_id
WHERE i.object_id = OBJECT_ID('InsuranceTariffs')
ORDER BY s.user_seeks + s.user_scans + s.user_lookups DESC;

-- بررسی fragmentation
SELECT 
    i.name AS IndexName,
    f.avg_fragmentation_in_percent,
    f.page_count
FROM sys.dm_db_index_physical_stats(DB_ID(), OBJECT_ID('InsuranceTariffs'), NULL, NULL, 'LIMITED') f
JOIN sys.indexes i ON f.object_id = i.object_id AND f.index_id = i.index_id
WHERE f.avg_fragmentation_in_percent > 10
ORDER BY f.avg_fragmentation_in_percent DESC;
