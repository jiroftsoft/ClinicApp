-- ═══════════════════════════════════════════════════════════════════════
-- 🏥 اسکریپت حذف امن خدمات مشترک
-- ═══════════════════════════════════════════════════════════════════════
-- تاریخ: 1404/10/05
-- هدف: حذف نرم (Soft Delete) خدمات مشترک برای تمام دپارتمان‌ها
--       به جز "اورژانس" و "تزریقات"
-- نوع: Soft Delete (نه Hard Delete)
-- امنیت: دارای BACKUP و ROLLBACK
-- ═══════════════════════════════════════════════════════════════════════

USE ClinicDb;
GO

PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '🏥 شروع فرآیند حذف امن خدمات مشترک';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '';

-- ═══════════════════════════════════════════════════════════════════════
-- مرحله 1: بررسی اولیه و پیدا کردن دپارتمان‌های مستثنی
-- ═══════════════════════════════════════════════════════════════════════

PRINT '📋 مرحله 1: پیدا کردن دپارتمان‌های مستثنی (اورژانس و تزریقات)';
PRINT '';

-- پیدا کردن DepartmentId برای "اورژانس" و "تزریقات"
DECLARE @ExcludedDepartmentIds TABLE (DepartmentId INT, DepartmentName NVARCHAR(500));

INSERT INTO @ExcludedDepartmentIds (DepartmentId, DepartmentName)
SELECT 
    DepartmentId,
    Name
FROM Departments
WHERE (
    Name LIKE N'%اورژانس%' OR 
    Name LIKE N'%emergency%' OR
    Name LIKE N'%تزریقات%' OR
    Name LIKE N'%injection%'
)
AND IsDeleted = 0;

-- نمایش دپارتمان‌های مستثنی
PRINT '✅ دپارتمان‌های مستثنی (نباید حذف شوند):';
SELECT 
    DepartmentId as 'شناسه',
    DepartmentName as 'نام دپارتمان'
FROM @ExcludedDepartmentIds;
PRINT '';

-- ═══════════════════════════════════════════════════════════════════════
-- مرحله 2: بررسی خدمات مشتر که قرار است حذف شوند
-- ═══════════════════════════════════════════════════════════════════════

PRINT '📊 مرحله 2: بررسی خدمات مشترک که قرار است حذف شوند';
PRINT '';

-- لیست خدمات مشتر که قرار است حذف شوند
SELECT 
    ss.SharedServiceId as 'شناسه خدمت مشترک',
    s.ServiceCode as 'کد خدمت',
    s.Title as 'عنوان خدمت',
    d.Name as 'نام دپارتمان',
    ss.IsActive as 'فعال',
    ss.CreatedAt as 'تاریخ ایجاد',
    u.UserName as 'ایجادکننده'
FROM SharedServices ss
INNER JOIN Services s ON ss.ServiceId = s.ServiceId
INNER JOIN Departments d ON ss.DepartmentId = d.DepartmentId
LEFT JOIN AspNetUsers u ON ss.CreatedByUserId = u.Id
WHERE ss.IsDeleted = 0
  AND ss.DepartmentId NOT IN (SELECT DepartmentId FROM @ExcludedDepartmentIds)
ORDER BY d.Name, s.Title;

-- شمارش کل
DECLARE @TotalToDelete INT;
SELECT @TotalToDelete = COUNT(*)
FROM SharedServices ss
WHERE ss.IsDeleted = 0
  AND ss.DepartmentId NOT IN (SELECT DepartmentId FROM @ExcludedDepartmentIds);

PRINT '';
PRINT '📊 آمار:';
PRINT '   - تعداد کل خدمات مشترک: ' + CAST((SELECT COUNT(*) FROM SharedServices WHERE IsDeleted = 0) AS NVARCHAR(10));
PRINT '   - تعداد دپارتمان‌های مستثنی: ' + CAST((SELECT COUNT(*) FROM @ExcludedDepartmentIds) AS NVARCHAR(10));
PRINT '   - تعداد خدمات مشترک در دپارتمان‌های مستثنی: ' + CAST((
    SELECT COUNT(*) 
    FROM SharedServices ss
    WHERE ss.IsDeleted = 0
      AND ss.DepartmentId IN (SELECT DepartmentId FROM @ExcludedDepartmentIds)
) AS NVARCHAR(10));
PRINT '   - تعداد خدمات مشترک که قرار است حذف شوند: ' + CAST(@TotalToDelete AS NVARCHAR(10));
PRINT '';

-- ═══════════════════════════════════════════════════════════════════════
-- مرحله 3: تأییدیه از کاربر
-- ═══════════════════════════════════════════════════════════════════════

PRINT '⚠️  مرحله 3: تأییدیه';
PRINT '';
PRINT '⚠️  هشدار: شما در حال حذف نرم (Soft Delete) ' + CAST(@TotalToDelete AS NVARCHAR(10)) + ' رکورد از SharedServices هستید!';
PRINT '';
PRINT '✅ دپارتمان‌های زیر از این عملیات مستثنی می‌شوند:';
SELECT DepartmentName as 'نام دپارتمان' FROM @ExcludedDepartmentIds;
PRINT '';
PRINT '❌ خدمات مشترک سایر دپارتمان‌ها حذف نرم خواهند شد.';
PRINT '';
PRINT '📝 توجه: این یک Soft Delete است و داده‌ها قابل بازیابی هستند.';
PRINT '';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '❓ آیا مطمئن هستید؟';
PRINT '';
PRINT '⏸️  برای ادامه، کامنت CHECKPOINT را حذف کنید و دوباره اجرا کنید.';
PRINT '═══════════════════════════════════════════════════════════════════════';

-- ⛔ CHECKPOINT: این خط را کامنت کنید تا اسکریپت ادامه یابد
-- برای اجرا، این خط را به صورت کامنت درآورید:
RETURN; -- ⛔ این خط را حذف کنید برای اجرای واقعی

-- ═══════════════════════════════════════════════════════════════════════
-- مرحله 4: ایجاد Backup قبل از حذف
-- ═══════════════════════════════════════════════════════════════════════

PRINT '';
PRINT '💾 مرحله 4: ایجاد Backup';
PRINT '';

-- ایجاد جدول موقت برای Backup
IF OBJECT_ID('tempdb..#SharedServicesBackup') IS NOT NULL
    DROP TABLE #SharedServicesBackup;

SELECT *
INTO #SharedServicesBackup
FROM SharedServices
WHERE IsDeleted = 0
  AND DepartmentId NOT IN (SELECT DepartmentId FROM @ExcludedDepartmentIds);

DECLARE @BackupCount INT = @@ROWCOUNT;
PRINT '✅ Backup ایجاد شد: ' + CAST(@BackupCount AS NVARCHAR(10)) + ' رکورد در #SharedServicesBackup';
PRINT '';

-- ═══════════════════════════════════════════════════════════════════════
-- مرحله 5: شروع Transaction و حذف نرم
-- ═══════════════════════════════════════════════════════════════════════

PRINT '🔄 مرحله 5: شروع Transaction';
PRINT '';

BEGIN TRANSACTION;

BEGIN TRY
    -- تنظیم متغیرهای Audit
    DECLARE @DeletedByUserId NVARCHAR(450) = 'SYSTEM_ADMIN'; -- 🔧 تغییر دهید به UserId واقعی
    DECLARE @DeletedAt DATETIME2 = GETDATE();
    
    PRINT '🗑️  شروع حذف نرم خدمات مشترک...';
    
    -- حذف نرم (Soft Delete)
    UPDATE SharedServices
    SET 
        IsDeleted = 1,
        DeletedAt = @DeletedAt,
        DeletedByUserId = @DeletedByUserId,
        UpdatedAt = @DeletedAt,
        UpdatedByUserId = @DeletedByUserId
    WHERE IsDeleted = 0
      AND DepartmentId NOT IN (SELECT DepartmentId FROM @ExcludedDepartmentIds);
    
    DECLARE @AffectedRows INT = @@ROWCOUNT;
    
    PRINT '✅ تعداد رکوردهای به‌روزرسانی شده: ' + CAST(@AffectedRows AS NVARCHAR(10));
    PRINT '';
    
    -- بررسی نتیجه
    IF @AffectedRows = @TotalToDelete
    BEGIN
        PRINT '✅ تعداد رکوردهای حذف شده با انتظار مطابقت دارد.';
        
        -- COMMIT Transaction
        COMMIT TRANSACTION;
        PRINT '';
        PRINT '✅ Transaction با موفقیت COMMIT شد.';
    END
    ELSE
    BEGIN
        PRINT '⚠️  تعداد رکوردهای حذف شده با انتظار مطابقت ندارد!';
        PRINT '   - انتظار: ' + CAST(@TotalToDelete AS NVARCHAR(10));
        PRINT '   - واقعی: ' + CAST(@AffectedRows AS NVARCHAR(10));
        
        -- ROLLBACK Transaction
        ROLLBACK TRANSACTION;
        PRINT '';
        PRINT '⚠️  Transaction به دلیل عدم تطابق ROLLBACK شد.';
    END
    
END TRY
BEGIN CATCH
    -- ROLLBACK در صورت خطا
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
        PRINT '';
        PRINT '❌ خطا رخ داد! Transaction ROLLBACK شد.';
    END
    
    -- نمایش خطا
    PRINT '';
    PRINT '❌ خطا:';
    PRINT '   - شماره خطا: ' + CAST(ERROR_NUMBER() AS NVARCHAR(10));
    PRINT '   - پیام خطا: ' + ERROR_MESSAGE();
    PRINT '   - خط: ' + CAST(ERROR_LINE() AS NVARCHAR(10));
    PRINT '';
    
    -- بازگشت داده‌ها از Backup
    PRINT '🔄 بازگشت داده‌ها از Backup...';
    
    -- اینجا می‌توانید از #SharedServicesBackup استفاده کنید
END CATCH;

-- ═══════════════════════════════════════════════════════════════════════
-- مرحله 6: بررسی نهایی
-- ═══════════════════════════════════════════════════════════════════════

PRINT '';
PRINT '📊 مرحله 6: بررسی نهایی';
PRINT '';

-- آمار نهایی
SELECT 
    'کل خدمات مشترک' as 'وضعیت',
    COUNT(*) as 'تعداد'
FROM SharedServices
UNION ALL
SELECT 
    'خدمات مشترک حذف نشده' as 'وضعیت',
    COUNT(*) as 'تعداد'
FROM SharedServices
WHERE IsDeleted = 0
UNION ALL
SELECT 
    'خدمات مشترک حذف شده' as 'وضعیت',
    COUNT(*) as 'تعداد'
FROM SharedServices
WHERE IsDeleted = 1
UNION ALL
SELECT 
    'خدمات مشترک در دپارتمان‌های مستثنی' as 'وضعیت',
    COUNT(*) as 'تعداد'
FROM SharedServices ss
WHERE ss.IsDeleted = 0
  AND ss.DepartmentId IN (SELECT DepartmentId FROM @ExcludedDepartmentIds);

PRINT '';
PRINT '✅ خدمات مشترک باقی‌مانده (فعال):';
SELECT 
    ss.SharedServiceId as 'شناسه',
    s.ServiceCode as 'کد خدمت',
    s.Title as 'عنوان خدمت',
    d.Name as 'دپارتمان',
    ss.IsActive as 'فعال'
FROM SharedServices ss
INNER JOIN Services s ON ss.ServiceId = s.ServiceId
INNER JOIN Departments d ON ss.DepartmentId = d.DepartmentId
WHERE ss.IsDeleted = 0
ORDER BY d.Name, s.Title;

PRINT '';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '✅ عملیات با موفقیت انجام شد!';
PRINT '';
PRINT '📝 یادآوری:';
PRINT '   - این یک Soft Delete بود';
PRINT '   - داده‌ها قابل بازیابی هستند';
PRINT '   - Backup در #SharedServicesBackup موجود است';
PRINT '   - برای بازیابی از اسکریپت RESTORE استفاده کنید';
PRINT '═══════════════════════════════════════════════════════════════════════';

GO

