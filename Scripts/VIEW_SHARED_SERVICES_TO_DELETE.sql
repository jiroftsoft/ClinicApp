-- ═══════════════════════════════════════════════════════════════════════
-- 👁️ اسکریپت مشاهده خدمات مشترک که قرار است حذف شوند
-- ═══════════════════════════════════════════════════════════════════════
-- تاریخ: 1404/10/05
-- هدف: نمایش اطلاعات خدمات مشترک قبل از حذف
-- نوع: VIEW ONLY (بدون تغییر در دیتابیس)
-- ═══════════════════════════════════════════════════════════════════════

USE ClinicDb;
GO

PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '👁️  گزارش خدمات مشترک - فقط مشاهده';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '';

-- ═══════════════════════════════════════════════════════════════════════
-- بخش 1: پیدا کردن دپارتمان‌های مستثنی
-- ═══════════════════════════════════════════════════════════════════════

PRINT '📋 بخش 1: دپارتمان‌های مستثنی (اورژانس و تزریقات)';
PRINT '';

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

SELECT 
    DepartmentId as 'شناسه دپارتمان',
    DepartmentName as 'نام دپارتمان'
FROM @ExcludedDepartmentIds;

DECLARE @ExcludedCount INT = (SELECT COUNT(*) FROM @ExcludedDepartmentIds);
PRINT '';
PRINT '✅ تعداد دپارتمان‌های مستثنی: ' + CAST(@ExcludedCount AS NVARCHAR(10));
PRINT '';

-- ═══════════════════════════════════════════════════════════════════════
-- بخش 2: آمار کلی
-- ═══════════════════════════════════════════════════════════════════════

PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '📊 بخش 2: آمار کلی';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '';

SELECT 
    'کل خدمات مشترک فعال' as 'نوع',
    COUNT(*) as 'تعداد'
FROM SharedServices
WHERE IsDeleted = 0
UNION ALL
SELECT 
    'خدمات مشترک در دپارتمان‌های مستثنی' as 'نوع',
    COUNT(*) as 'تعداد'
FROM SharedServices ss
WHERE ss.IsDeleted = 0
  AND ss.DepartmentId IN (SELECT DepartmentId FROM @ExcludedDepartmentIds)
UNION ALL
SELECT 
    '⚠️ خدمات مشترک که حذف خواهند شد' as 'نوع',
    COUNT(*) as 'تعداد'
FROM SharedServices ss
WHERE ss.IsDeleted = 0
  AND ss.DepartmentId NOT IN (SELECT DepartmentId FROM @ExcludedDepartmentIds);

PRINT '';

-- ═══════════════════════════════════════════════════════════════════════
-- بخش 3: لیست تفصیلی دپارتمان‌ها
-- ═══════════════════════════════════════════════════════════════════════

PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '📋 بخش 3: لیست تمام دپارتمان‌ها و تعداد خدمات مشترک';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '';

SELECT 
    d.DepartmentId as 'شناسه',
    d.Name as 'نام دپارتمان',
    COUNT(ss.SharedServiceId) as 'تعداد خدمات مشترک',
    CASE 
        WHEN d.DepartmentId IN (SELECT DepartmentId FROM @ExcludedDepartmentIds) 
        THEN '✅ محفوظ'
        ELSE '❌ حذف می‌شود'
    END as 'وضعیت'
FROM Departments d
LEFT JOIN SharedServices ss ON d.DepartmentId = ss.DepartmentId AND ss.IsDeleted = 0
WHERE d.IsDeleted = 0
GROUP BY d.DepartmentId, d.Name
ORDER BY 
    CASE 
        WHEN d.DepartmentId IN (SELECT DepartmentId FROM @ExcludedDepartmentIds) THEN 0
        ELSE 1
    END,
    d.Name;

PRINT '';

-- ═══════════════════════════════════════════════════════════════════════
-- بخش 4: خدمات مشترکی که حذف خواهند شد
-- ═══════════════════════════════════════════════════════════════════════

PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '❌ بخش 4: خدمات مشترکی که حذف خواهند شد';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '';

SELECT 
    ss.SharedServiceId as 'شناسه',
    d.Name as 'دپارتمان',
    s.ServiceCode as 'کد خدمت',
    s.Title as 'عنوان خدمت',
    s.Price as 'قیمت پایه',
    CASE WHEN ss.IsActive = 1 THEN 'فعال' ELSE 'غیرفعال' END as 'وضعیت',
    CASE 
        WHEN ss.OverrideTechnicalFactor IS NOT NULL OR ss.OverrideProfessionalFactor IS NOT NULL 
        THEN '✓'
        ELSE ''
    END as 'Override',
    ss.CreatedAt as 'تاریخ ایجاد',
    u.UserName as 'ایجادکننده',
    ss.DepartmentSpecificNotes as 'توضیحات'
FROM SharedServices ss
INNER JOIN Services s ON ss.ServiceId = s.ServiceId
INNER JOIN Departments d ON ss.DepartmentId = d.DepartmentId
LEFT JOIN AspNetUsers u ON ss.CreatedByUserId = u.Id
WHERE ss.IsDeleted = 0
  AND ss.DepartmentId NOT IN (SELECT DepartmentId FROM @ExcludedDepartmentIds)
ORDER BY d.Name, s.ServiceCode;

PRINT '';

-- ═══════════════════════════════════════════════════════════════════════
-- بخش 5: خدمات مشترکی که محفوظ می‌مانند
-- ═══════════════════════════════════════════════════════════════════════

PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '✅ بخش 5: خدمات مشترکی که محفوظ می‌مانند (اورژانس و تزریقات)';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '';

SELECT 
    ss.SharedServiceId as 'شناسه',
    d.Name as 'دپارتمان',
    s.ServiceCode as 'کد خدمت',
    s.Title as 'عنوان خدمت',
    s.Price as 'قیمت پایه',
    CASE WHEN ss.IsActive = 1 THEN 'فعال' ELSE 'غیرفعال' END as 'وضعیت',
    ss.CreatedAt as 'تاریخ ایجاد'
FROM SharedServices ss
INNER JOIN Services s ON ss.ServiceId = s.ServiceId
INNER JOIN Departments d ON ss.DepartmentId = d.DepartmentId
WHERE ss.IsDeleted = 0
  AND ss.DepartmentId IN (SELECT DepartmentId FROM @ExcludedDepartmentIds)
ORDER BY d.Name, s.ServiceCode;

PRINT '';

-- ═══════════════════════════════════════════════════════════════════════
-- بخش 6: بررسی وابستگی‌ها (اختیاری - برای اطمینان)
-- ═══════════════════════════════════════════════════════════════════════

PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '🔗 بخش 6: بررسی وابستگی‌ها';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '';
PRINT '⚠️  توجه: بررسی اینکه آیا خدمات مشترک در جای دیگری استفاده شده‌اند';
PRINT '';

-- بررسی استفاده در ReceptionItems
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ReceptionItems')
BEGIN
    SELECT 
        d.Name as 'دپارتمان',
        s.ServiceCode as 'کد خدمت',
        s.Title as 'عنوان خدمت',
        COUNT(DISTINCT ri.ReceptionItemId) as 'تعداد استفاده در پذیرش‌ها'
    FROM SharedServices ss
    INNER JOIN Services s ON ss.ServiceId = s.ServiceId
    INNER JOIN Departments d ON ss.DepartmentId = d.DepartmentId
    LEFT JOIN ReceptionItems ri ON ss.ServiceId = ri.ServiceId
    WHERE ss.IsDeleted = 0
      AND ss.DepartmentId NOT IN (SELECT DepartmentId FROM @ExcludedDepartmentIds)
    GROUP BY d.Name, s.ServiceCode, s.Title
    HAVING COUNT(DISTINCT ri.ReceptionItemId) > 0
    ORDER BY COUNT(DISTINCT ri.ReceptionItemId) DESC;
    
    PRINT '';
    PRINT '✅ بررسی وابستگی با ReceptionItems انجام شد.';
END
ELSE
BEGIN
    PRINT '⚠️  جدول ReceptionItems یافت نشد.';
END

PRINT '';

-- ═══════════════════════════════════════════════════════════════════════
-- خلاصه نهایی
-- ═══════════════════════════════════════════════════════════════════════

PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '📊 خلاصه نهایی';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '';

DECLARE @TotalActive INT = (SELECT COUNT(*) FROM SharedServices WHERE IsDeleted = 0);
DECLARE @ToBeDeleted INT = (
    SELECT COUNT(*) 
    FROM SharedServices ss
    WHERE ss.IsDeleted = 0
      AND ss.DepartmentId NOT IN (SELECT DepartmentId FROM @ExcludedDepartmentIds)
);
DECLARE @ToBeKept INT = @TotalActive - @ToBeDeleted;

PRINT '📊 آمار:';
PRINT '   - کل خدمات مشترک فعال: ' + CAST(@TotalActive AS NVARCHAR(10));
PRINT '   - تعداد دپارتمان‌های مستثنی: ' + CAST(@ExcludedCount AS NVARCHAR(10));
PRINT '   ✅ خدمات مشترک محفوظ: ' + CAST(@ToBeKept AS NVARCHAR(10));
PRINT '   ❌ خدمات مشترک که حذف می‌شوند: ' + CAST(@ToBeDeleted AS NVARCHAR(10));
PRINT '';
PRINT '✅ دپارتمان‌های محفوظ:';

SELECT '   - ' + DepartmentName as 'دپارتمان'
FROM @ExcludedDepartmentIds;

PRINT '';
PRINT '═══════════════════════════════════════════════════════════════════════';
PRINT '✅ گزارش فقط مشاهده تمام شد';
PRINT '';
PRINT '📝 برای اجرای حذف، از اسکریپت DELETE_SHARED_SERVICES_SAFE.sql استفاده کنید';
PRINT '═══════════════════════════════════════════════════════════════════════';

GO

