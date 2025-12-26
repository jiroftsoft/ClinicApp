-- ═══════════════════════════════════════════════════════════════════════
-- 🔄 اسکریپت بازگردانی خدمات مشترک حذف شده
-- ═══════════════════════════════════════════════════════════════════════
-- تاریخ: 1404/10/05
-- هدف: بازگردانی (Restore) خدمات مشترک که به اشتباه حذف شده‌اند
-- نوع: Un-Soft Delete
-- ═══════════════════════════════════════════════════════════════════════

USE ClinicDb;
GO

PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '🔄 شروع فرآیند بازگردانی خدمات مشترک';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '';

-- ═══════════════════════════════════════════════════════════════════════
-- مرحله 1: بررسی خدمات مشترک حذف شده
-- ═══════════════════════════════════════════════════════════════════════

PRINT '📋 مرحله 1: بررسی خدمات مشترک حذف شده';
PRINT '';

-- پیدا کردن دپارتمان‌های مستثنی
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

-- لیست خدمات مشترک حذف شده
SELECT 
    ss.SharedServiceId as 'شناسه',
    s.ServiceCode as 'کد خدمت',
    s.Title as 'عنوان خدمت',
    d.Name as 'دپارتمان',
    ss.DeletedAt as 'تاریخ حذف',
    u.UserName as 'حذف شده توسط'
FROM SharedServices ss
INNER JOIN Services s ON ss.ServiceId = s.ServiceId
INNER JOIN Departments d ON ss.DepartmentId = d.DepartmentId
LEFT JOIN AspNetUsers u ON ss.DeletedByUserId = u.Id
WHERE ss.IsDeleted = 1
  AND ss.DepartmentId NOT IN (SELECT DepartmentId FROM @ExcludedDepartmentIds)
ORDER BY ss.DeletedAt DESC;

-- شمارش
DECLARE @TotalToRestore INT;
SELECT @TotalToRestore = COUNT(*)
FROM SharedServices ss
WHERE ss.IsDeleted = 1
  AND ss.DepartmentId NOT IN (SELECT DepartmentId FROM @ExcludedDepartmentIds);

PRINT '';
PRINT '📊 آمار:';
PRINT '   - تعداد خدمات مشترک حذف شده: ' + CAST(@TotalToRestore AS NVARCHAR(10));
PRINT '';

IF @TotalToRestore = 0
BEGIN
    PRINT '✅ هیچ خدمت مشترک حذف شده‌ای برای بازگردانی وجود ندارد.';
    RETURN;
END

-- ═══════════════════════════════════════════════════════════════════════
-- مرحله 2: تأییدیه
-- ═══════════════════════════════════════════════════════════════════════

PRINT '';
PRINT '⚠️  مرحله 2: تأییدیه';
PRINT '';
PRINT '⚠️  شما در حال بازگردانی ' + CAST(@TotalToRestore AS NVARCHAR(10)) + ' رکورد حذف شده هستید!';
PRINT '';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '❓ آیا مطمئن هستید؟';
PRINT '';
PRINT '⏸️  برای ادامه، کامنت CHECKPOINT را حذف کنید و دوباره اجرا کنید.';
PRINT '═══════════════════════════════════════════════════════════════════════';

-- ⛔ CHECKPOINT: این خط را کامنت کنید تا اسکریپت ادامه یابد
RETURN; -- ⛔ این خط را حذف کنید برای اجرای واقعی

-- ═══════════════════════════════════════════════════════════════════════
-- مرحله 3: بازگردانی
-- ═══════════════════════════════════════════════════════════════════════

PRINT '';
PRINT '🔄 مرحله 3: شروع Transaction';
PRINT '';

BEGIN TRANSACTION;

BEGIN TRY
    -- تنظیم متغیرهای Audit
    DECLARE @RestoredByUserId NVARCHAR(450) = 'SYSTEM_ADMIN'; -- 🔧 تغییر دهید به UserId واقعی
    DECLARE @RestoredAt DATETIME2 = GETDATE();
    
    PRINT '♻️  شروع بازگردانی خدمات مشترک...';
    
    -- بازگردانی (Un-Soft Delete)
    UPDATE SharedServices
    SET 
        IsDeleted = 0,
        DeletedAt = NULL,
        DeletedByUserId = NULL,
        UpdatedAt = @RestoredAt,
        UpdatedByUserId = @RestoredByUserId
    WHERE IsDeleted = 1
      AND DepartmentId NOT IN (SELECT DepartmentId FROM @ExcludedDepartmentIds);
    
    DECLARE @AffectedRows INT = @@ROWCOUNT;
    
    PRINT '✅ تعداد رکوردهای بازگردانده شده: ' + CAST(@AffectedRows AS NVARCHAR(10));
    PRINT '';
    
    -- بررسی نتیجه
    IF @AffectedRows = @TotalToRestore
    BEGIN
        PRINT '✅ تعداد رکوردهای بازگردانده شده با انتظار مطابقت دارد.';
        
        -- COMMIT Transaction
        COMMIT TRANSACTION;
        PRINT '';
        PRINT '✅ Transaction با موفقیت COMMIT شد.';
    END
    ELSE
    BEGIN
        PRINT '⚠️  تعداد رکوردهای بازگردانده شده با انتظار مطابقت ندارد!';
        PRINT '   - انتظار: ' + CAST(@TotalToRestore AS NVARCHAR(10));
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
END CATCH;

-- ═══════════════════════════════════════════════════════════════════════
-- مرحله 4: بررسی نهایی
-- ═══════════════════════════════════════════════════════════════════════

PRINT '';
PRINT '📊 مرحله 4: بررسی نهایی';
PRINT '';

-- آمار نهایی
SELECT 
    'کل خدمات مشترک' as 'وضعیت',
    COUNT(*) as 'تعداد'
FROM SharedServices
UNION ALL
SELECT 
    'خدمات مشترک فعال' as 'وضعیت',
    COUNT(*) as 'تعداد'
FROM SharedServices
WHERE IsDeleted = 0
UNION ALL
SELECT 
    'خدمات مشترک حذف شده' as 'وضعیت',
    COUNT(*) as 'تعداد'
FROM SharedServices
WHERE IsDeleted = 1;

PRINT '';
PRINT '✅ خدمات مشترک بازگردانده شده:';
SELECT 
    ss.SharedServiceId as 'شناسه',
    s.ServiceCode as 'کد خدمت',
    s.Title as 'عنوان خدمت',
    d.Name as 'دپارتمان',
    ss.IsActive as 'فعال',
    ss.UpdatedAt as 'تاریخ بازگردانی'
FROM SharedServices ss
INNER JOIN Services s ON ss.ServiceId = s.ServiceId
INNER JOIN Departments d ON ss.DepartmentId = d.DepartmentId
WHERE ss.IsDeleted = 0
  AND ss.DepartmentId NOT IN (SELECT DepartmentId FROM @ExcludedDepartmentIds)
  AND ss.UpdatedAt > DATEADD(MINUTE, -5, GETDATE()) -- آخرین 5 دقیقه
ORDER BY ss.UpdatedAt DESC;

PRINT '';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '✅ عملیات بازگردانی با موفقیت انجام شد!';
PRINT '═══════════════════════════════════════════════════════════════════════';

GO

