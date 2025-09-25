-- 🏥 MEDICAL: ایندکس‌های بهینه برای Zero-Cache Strategy
-- Real-time data access optimization for clinical safety

-- =============================================
-- Insurance Tariffs Optimization Indexes
-- =============================================

-- ایندکس ترکیبی برای جستجوی تعرفه‌ها
CREATE NONCLUSTERED INDEX IX_InsuranceTariffs_Plan_Service_Active 
ON InsuranceTariffs (InsurancePlanId, ServiceId, IsActive, IsDeleted)
INCLUDE (TariffPrice, PatientShare, InsurerShare, CreatedAt, UpdatedAt)
WHERE IsDeleted = 0;

-- ایندکس برای فیلتر بر اساس ارائه‌دهنده بیمه
CREATE NONCLUSTERED INDEX IX_InsuranceTariffs_Provider_Active 
ON InsuranceTariffs (InsuranceProviderId, IsActive, IsDeleted)
INCLUDE (InsurancePlanId, ServiceId, TariffPrice, PatientShare, InsurerShare)
WHERE IsDeleted = 0;

-- ایندکس برای جستجوی متنی
CREATE NONCLUSTERED INDEX IX_InsuranceTariffs_Search_Text 
ON InsuranceTariffs (IsActive, IsDeleted, CreatedAt)
INCLUDE (InsurancePlanId, ServiceId, TariffPrice, PatientShare, InsurerShare)
WHERE IsDeleted = 0;

-- =============================================
-- Insurance Plans Optimization Indexes
-- =============================================

-- ایندکس برای طرح‌های بیمه فعال
CREATE NONCLUSTERED INDEX IX_InsurancePlans_Provider_Active 
ON InsurancePlans (InsuranceProviderId, IsActive, IsDeleted)
INCLUDE (PlanName, CoveragePercent, CreatedAt, UpdatedAt)
WHERE IsDeleted = 0;

-- ایندکس برای جستجوی طرح‌های بیمه
CREATE NONCLUSTERED INDEX IX_InsurancePlans_Search_Text 
ON InsurancePlans (IsActive, IsDeleted, PlanName)
INCLUDE (InsuranceProviderId, CoveragePercent, CreatedAt)
WHERE IsDeleted = 0;

-- =============================================
-- Services Optimization Indexes
-- =============================================

-- ایندکس برای خدمات فعال
CREATE NONCLUSTERED INDEX IX_Services_Category_Active 
ON Services (ServiceCategoryId, IsActive, IsDeleted)
INCLUDE (ServiceName, BasePrice, CreatedAt, UpdatedAt)
WHERE IsDeleted = 0;

-- ایندکس برای جستجوی خدمات
CREATE NONCLUSTERED INDEX IX_Services_Search_Text 
ON Services (IsActive, IsDeleted, ServiceName)
INCLUDE (ServiceCategoryId, BasePrice, CreatedAt)
WHERE IsDeleted = 0;

-- =============================================
-- Service Categories Optimization Indexes
-- =============================================

-- ایندکس برای دسته‌بندی‌های خدمات
CREATE NONCLUSTERED INDEX IX_ServiceCategories_Department_Active 
ON ServiceCategories (DepartmentId, IsActive, IsDeleted)
INCLUDE (CategoryName, CreatedAt, UpdatedAt)
WHERE IsDeleted = 0;

-- =============================================
-- Departments Optimization Indexes
-- =============================================

-- ایندکس برای دپارتمان‌های فعال
CREATE NONCLUSTERED INDEX IX_Departments_Active 
ON Departments (IsActive, IsDeleted)
INCLUDE (DepartmentName, CreatedAt, UpdatedAt)
WHERE IsDeleted = 0;

-- =============================================
-- Insurance Providers Optimization Indexes
-- =============================================

-- ایندکس برای ارائه‌دهندگان بیمه
CREATE NONCLUSTERED INDEX IX_InsuranceProviders_Type_Active 
ON InsuranceProviders (InsuranceType, IsActive, IsDeleted)
INCLUDE (ProviderName, CreatedAt, UpdatedAt)
WHERE IsDeleted = 0;

-- =============================================
-- Factor Settings Optimization Indexes
-- =============================================

-- ایندکس برای تنظیمات ضرایب
CREATE NONCLUSTERED INDEX IX_FactorSettings_Type_Year_Active 
ON FactorSettings (FactorType, FinancialYear, IsActive, IsDeleted)
INCLUDE (Value, EffectiveFrom, EffectiveTo)
WHERE IsDeleted = 0 AND IsActive = 1;

-- =============================================
-- Service Components Optimization Indexes
-- =============================================

-- ایندکس برای اجزای خدمات
CREATE NONCLUSTERED INDEX IX_ServiceComponents_Service_Type 
ON ServiceComponents (ServiceId, ComponentType, IsActive, IsDeleted)
INCLUDE (Coefficient, CreatedAt, UpdatedAt)
WHERE IsDeleted = 0;

-- =============================================
-- Statistics and Reporting Optimization
-- =============================================

-- ایندکس برای آمار تعرفه‌ها
CREATE NONCLUSTERED INDEX IX_InsuranceTariffs_Stats_Date 
ON InsuranceTariffs (CreatedAt, IsActive, IsDeleted)
INCLUDE (InsurancePlanId, ServiceId, TariffPrice)
WHERE IsDeleted = 0;

-- ایندکس برای آمار طرح‌های بیمه
CREATE NONCLUSTERED INDEX IX_InsurancePlans_Stats_Date 
ON InsurancePlans (CreatedAt, IsActive, IsDeleted)
INCLUDE (InsuranceProviderId, CoveragePercent)
WHERE IsDeleted = 0;

-- =============================================
-- Performance Monitoring
-- =============================================

-- نمایش ایندکس‌های موجود
SELECT 
    i.name AS IndexName,
    t.name AS TableName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    i.is_primary_key AS IsPrimaryKey
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('InsuranceTariffs', 'InsurancePlans', 'Services', 'ServiceCategories', 'Departments', 'InsuranceProviders', 'FactorSettings', 'ServiceComponents')
ORDER BY t.name, i.name;

-- =============================================
-- Query Performance Analysis
-- =============================================

-- کوئری برای بررسی عملکرد ایندکس‌ها
-- این کوئری‌ها باید با SET STATISTICS IO ON اجرا شوند

-- تست عملکرد جستجوی تعرفه‌ها
-- SET STATISTICS IO ON;
-- SELECT t.InsuranceTariffId, t.TariffPrice, t.PatientShare, t.InsurerShare
-- FROM InsuranceTariffs t
-- WHERE t.InsurancePlanId = 1 
--   AND t.ServiceId = 1 
--   AND t.IsActive = 1 
--   AND t.IsDeleted = 0;

-- تست عملکرد جستجوی طرح‌های بیمه
-- SELECT p.InsurancePlanId, p.PlanName, p.CoveragePercent
-- FROM InsurancePlans p
-- WHERE p.InsuranceProviderId = 1 
--   AND p.IsActive = 1 
--   AND p.IsDeleted = 0;

-- =============================================
-- Maintenance Recommendations
-- =============================================

-- توصیه‌های نگهداری:
-- 1. اجرای UPDATE STATISTICS هفتگی
-- 2. بررسی fragmentation ماهانه
-- 3. مانیتورینگ performance کوئری‌ها
-- 4. بررسی missing indexes

-- اسکریپت بررسی fragmentation
-- SELECT 
--     OBJECT_NAME(ips.object_id) AS TableName,
--     i.name AS IndexName,
--     ips.avg_fragmentation_in_percent
-- FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
-- INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
-- WHERE ips.avg_fragmentation_in_percent > 10
-- ORDER BY ips.avg_fragmentation_in_percent DESC;
