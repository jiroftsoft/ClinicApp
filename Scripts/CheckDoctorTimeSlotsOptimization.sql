-- =============================================
-- اسکریپت بررسی بهینه‌سازی DoctorTimeSlots
-- بررسی اعمال بهینه‌سازی‌های انجام شده در DoctorScheduleRepository
-- =============================================

USE ClinicDb;
GO

PRINT '========================================';
PRINT 'بررسی بهینه‌سازی DoctorTimeSlots';
PRINT '========================================';
PRINT '';

-- =============================================
-- 1. بررسی کلی جداول
-- =============================================
PRINT '1. بررسی کلی جداول:';
PRINT '';

SELECT 
    'DoctorSchedules' as TableName,
    COUNT(*) as TotalRecords,
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END) as DeletedRecords,
    SUM(CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END) as ActiveRecords,
    SUM(CASE WHEN IsActive = 1 AND IsDeleted = 0 THEN 1 ELSE 0 END) as ActiveAndNotDeleted
FROM DoctorSchedules
UNION ALL
SELECT 
    'DoctorTimeSlots',
    COUNT(*),
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END),
    SUM(CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END),
    SUM(CASE WHEN Status = 0 AND IsDeleted = 0 THEN 1 ELSE 0 END) -- Status = 0 = Available
FROM DoctorTimeSlots
UNION ALL
SELECT 
    'DoctorTimeRanges',
    COUNT(*),
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END),
    SUM(CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END),
    SUM(CASE WHEN IsActive = 1 AND IsDeleted = 0 THEN 1 ELSE 0 END)
FROM DoctorTimeRanges;
GO

PRINT '';
PRINT '========================================';
PRINT '';

-- =============================================
-- 2. بررسی اسلات‌های خارج از بازه (مشکل اصلی)
-- =============================================
PRINT '2. بررسی اسلات‌های خارج از بازه:';
PRINT '';

-- بررسی اسلات‌هایی که ممکن است خارج از TimeRange باشند
SELECT 
    dts.TimeSlotId,
    dts.DoctorId,
    dts.AppointmentDate,
    dts.StartTime,
    dts.EndTime,
    dts.Duration,
    dts.Status,
    dts.IsDeleted,
    dts.CreatedAt,
    dtr.StartTime as TimeRangeStartTime,
    dtr.EndTime as TimeRangeEndTime,
    CASE 
        WHEN dts.StartTime < dtr.StartTime THEN '❌ StartTime قبل از TimeRange'
        WHEN dts.EndTime > dtr.EndTime THEN '❌ EndTime بعد از TimeRange'
        WHEN dts.StartTime >= dtr.StartTime AND dts.EndTime <= dtr.EndTime THEN '✅ درون TimeRange'
        ELSE '⚠️ نامشخص'
    END as ValidationStatus
FROM DoctorTimeSlots dts
INNER JOIN DoctorSchedules ds ON dts.DoctorId = ds.DoctorId
INNER JOIN DoctorWorkDays dwd ON ds.ScheduleId = dwd.ScheduleId 
    AND DATEPART(WEEKDAY, dts.AppointmentDate) - 1 = dwd.DayOfWeek
    AND dwd.IsActive = 1 
    AND dwd.IsDeleted = 0
INNER JOIN DoctorTimeRanges dtr ON dwd.WorkDayId = dtr.WorkDayId
    AND dts.StartTime >= dtr.StartTime
    AND dts.EndTime <= dtr.EndTime
    AND dtr.IsActive = 1 
    AND dtr.IsDeleted = 0
WHERE dts.IsDeleted = 0
    AND dts.AppointmentDate >= DATEADD(DAY, -30, GETDATE()) -- فقط 30 روز اخیر
ORDER BY dts.AppointmentDate DESC, dts.StartTime;
GO

-- بررسی اسلات‌های مشکوک (که ممکن است خارج از بازه باشند)
PRINT '';
PRINT 'اسلات‌های مشکوک (خارج از بازه):';
PRINT '';

SELECT 
    dts.TimeSlotId,
    dts.DoctorId,
    dts.AppointmentDate,
    dts.StartTime,
    dts.EndTime,
    dts.Duration,
    dts.Status,
    dts.IsDeleted,
    '⚠️ اسلات بدون TimeRange معتبر' as Issue
FROM DoctorTimeSlots dts
LEFT JOIN DoctorSchedules ds ON dts.DoctorId = ds.DoctorId
LEFT JOIN DoctorWorkDays dwd ON ds.ScheduleId = dwd.ScheduleId 
    AND DATEPART(WEEKDAY, dts.AppointmentDate) - 1 = dwd.DayOfWeek
    AND dwd.IsActive = 1 
    AND dwd.IsDeleted = 0
LEFT JOIN DoctorTimeRanges dtr ON dwd.WorkDayId = dtr.WorkDayId
    AND dts.StartTime >= dtr.StartTime
    AND dts.EndTime <= dtr.EndTime
    AND dtr.IsActive = 1 
    AND dtr.IsDeleted = 0
WHERE dts.IsDeleted = 0
    AND dts.AppointmentDate >= DATEADD(DAY, -30, GETDATE())
    AND dtr.TimeRangeId IS NULL -- اسلات‌هایی که TimeRange معتبر ندارند
ORDER BY dts.AppointmentDate DESC, dts.StartTime;
GO

PRINT '';
PRINT '========================================';
PRINT '';

-- =============================================
-- 3. بررسی Soft Delete
-- =============================================
PRINT '3. بررسی Soft Delete:';
PRINT '';

SELECT 
    COUNT(*) as TotalDeletedSlots,
    MIN(DeletedAt) as FirstDeletedAt,
    MAX(DeletedAt) as LastDeletedAt,
    COUNT(DISTINCT DeletedByUserId) as UniqueDeleters
FROM DoctorTimeSlots
WHERE IsDeleted = 1;
GO

-- بررسی اسلات‌های حذف شده اخیر
PRINT '';
PRINT 'اسلات‌های حذف شده در 7 روز اخیر:';
PRINT '';

SELECT TOP 20
    TimeSlotId,
    DoctorId,
    AppointmentDate,
    StartTime,
    EndTime,
    Status,
    DeletedAt,
    DeletedByUserId,
    DATEDIFF(DAY, CreatedAt, DeletedAt) as DaysBeforeDeletion
FROM DoctorTimeSlots
WHERE IsDeleted = 1
    AND DeletedAt >= DATEADD(DAY, -7, GETDATE())
ORDER BY DeletedAt DESC;
GO

PRINT '';
PRINT '========================================';
PRINT '';

-- =============================================
-- 4. بررسی تداخل با ScheduleExceptions (زمان‌های بلاک شده)
-- =============================================
PRINT '4. بررسی تداخل با ScheduleExceptions:';
PRINT '';

SELECT 
    dts.TimeSlotId,
    dts.DoctorId as SlotDoctorId,
    dts.AppointmentDate,
    dts.StartTime,
    dts.EndTime,
    se.ExceptionId,
    se.Reason,
    se.StartTime as ExceptionStartTime,
    se.EndTime as ExceptionEndTime,
    '⚠️ اسلات در زمان بلاک شده' as Issue
FROM DoctorTimeSlots dts
INNER JOIN DoctorSchedules ds ON dts.DoctorId = ds.DoctorId
INNER JOIN ScheduleExceptions se ON ds.ScheduleId = se.ScheduleId
    AND CAST(dts.AppointmentDate AS DATE) >= CAST(se.StartDate AS DATE)
    AND (se.EndDate IS NULL OR CAST(dts.AppointmentDate AS DATE) <= CAST(se.EndDate AS DATE))
    AND se.IsActive = 1
    AND se.IsDeleted = 0
WHERE dts.IsDeleted = 0
    AND (
        (se.StartTime IS NULL AND se.EndTime IS NULL) -- استثنای تمام روز
        OR (
            se.StartTime IS NOT NULL 
            AND se.EndTime IS NOT NULL
            AND se.StartTime <= dts.StartTime 
            AND se.EndTime >= dts.EndTime -- اسلات کاملاً درون استثنا
        )
    )
    AND dts.AppointmentDate >= DATEADD(DAY, -30, GETDATE())
ORDER BY dts.AppointmentDate DESC, dts.StartTime;
GO

PRINT '';
PRINT '========================================';
PRINT '';

-- =============================================
-- 5. بررسی اسلات‌های معتبر (برای مقایسه)
-- =============================================
PRINT '5. بررسی اسلات‌های معتبر:';
PRINT '';

SELECT 
    COUNT(*) as ValidSlotsCount,
    MIN(AppointmentDate) as EarliestDate,
    MAX(AppointmentDate) as LatestDate,
    COUNT(DISTINCT DoctorId) as UniqueDoctors,
    COUNT(DISTINCT CAST(AppointmentDate AS DATE)) as UniqueDates
FROM DoctorTimeSlots dts
INNER JOIN DoctorSchedules ds ON dts.DoctorId = ds.DoctorId
INNER JOIN DoctorWorkDays dwd ON ds.ScheduleId = dwd.ScheduleId 
    AND DATEPART(WEEKDAY, dts.AppointmentDate) - 1 = dwd.DayOfWeek
    AND dwd.IsActive = 1 
    AND dwd.IsDeleted = 0
INNER JOIN DoctorTimeRanges dtr ON dwd.WorkDayId = dtr.WorkDayId
    AND dts.StartTime >= dtr.StartTime
    AND dts.EndTime <= dtr.EndTime
    AND dtr.IsActive = 1 
    AND dtr.IsDeleted = 0
WHERE dts.IsDeleted = 0
    AND dts.AppointmentDate >= DATEADD(DAY, -30, GETDATE());
GO

-- نمونه اسلات‌های معتبر
PRINT '';
PRINT 'نمونه اسلات‌های معتبر (10 مورد اخیر):';
PRINT '';

SELECT TOP 10
    dts.TimeSlotId,
    dts.DoctorId,
    dts.AppointmentDate,
    dts.StartTime,
    dts.EndTime,
    dts.Duration,
    dts.Status,
    dtr.StartTime as TimeRangeStartTime,
    dtr.EndTime as TimeRangeEndTime,
    '✅ معتبر' as ValidationStatus
FROM DoctorTimeSlots dts
INNER JOIN DoctorSchedules ds ON dts.DoctorId = ds.DoctorId
INNER JOIN DoctorWorkDays dwd ON ds.ScheduleId = dwd.ScheduleId 
    AND DATEPART(WEEKDAY, dts.AppointmentDate) - 1 = dwd.DayOfWeek
    AND dwd.IsActive = 1 
    AND dwd.IsDeleted = 0
INNER JOIN DoctorTimeRanges dtr ON dwd.WorkDayId = dtr.WorkDayId
    AND dts.StartTime >= dtr.StartTime
    AND dts.EndTime <= dtr.EndTime
    AND dtr.IsActive = 1 
    AND dtr.IsDeleted = 0
WHERE dts.IsDeleted = 0
    AND dts.AppointmentDate >= DATEADD(DAY, -30, GETDATE())
ORDER BY dts.AppointmentDate DESC, dts.StartTime;
GO

PRINT '';
PRINT '========================================';
PRINT '';

-- =============================================
-- 6. خلاصه گزارش
-- =============================================
PRINT '6. خلاصه گزارش:';
PRINT '';

SELECT 
    'کل اسلات‌ها' as Metric,
    COUNT(*) as Value
FROM DoctorTimeSlots
WHERE AppointmentDate >= DATEADD(DAY, -30, GETDATE())
UNION ALL
SELECT 
    'اسلات‌های فعال',
    COUNT(*)
FROM DoctorTimeSlots
WHERE IsDeleted = 0
    AND AppointmentDate >= DATEADD(DAY, -30, GETDATE())
UNION ALL
SELECT 
    'اسلات‌های حذف شده (Soft Delete)',
    COUNT(*)
FROM DoctorTimeSlots
WHERE IsDeleted = 1
    AND AppointmentDate >= DATEADD(DAY, -30, GETDATE())
UNION ALL
SELECT 
    'اسلات‌های مشکوک (بدون TimeRange معتبر)',
    COUNT(*)
FROM DoctorTimeSlots dts
LEFT JOIN DoctorSchedules ds ON dts.DoctorId = ds.DoctorId
LEFT JOIN DoctorWorkDays dwd ON ds.ScheduleId = dwd.ScheduleId 
    AND DATEPART(WEEKDAY, dts.AppointmentDate) - 1 = dwd.DayOfWeek
    AND dwd.IsActive = 1 
    AND dwd.IsDeleted = 0
LEFT JOIN DoctorTimeRanges dtr ON dwd.WorkDayId = dtr.WorkDayId
    AND dts.StartTime >= dtr.StartTime
    AND dts.EndTime <= dtr.EndTime
    AND dtr.IsActive = 1 
    AND dtr.IsDeleted = 0
WHERE dts.IsDeleted = 0
    AND dts.AppointmentDate >= DATEADD(DAY, -30, GETDATE())
    AND dtr.TimeRangeId IS NULL;
GO

PRINT '';
PRINT '========================================';
PRINT 'بررسی کامل شد!';
PRINT '========================================';
GO

