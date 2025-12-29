-- 🔍 اسکریپت بررسی DoctorTimeSlots
-- بررسی اسلات‌های زمانی پزشکان و شناسایی مشکلات احتمالی

USE ClinicDb;
GO

-- ✅ 1. بررسی کلی اسلات‌های موجود
PRINT '=== بررسی کلی اسلات‌های موجود ===';
SELECT 
    COUNT(*) as TotalSlots,
    COUNT(CASE WHEN IsDeleted = 0 THEN 1 END) as ActiveSlots,
    COUNT(CASE WHEN IsDeleted = 1 THEN 1 END) as DeletedSlots,
    COUNT(CASE WHEN Status = 0 THEN 1 END) as AvailableSlots, -- AppointmentStatus.Available = 0
    COUNT(CASE WHEN Status = 1 THEN 1 END) as BookedSlots, -- AppointmentStatus.Booked = 1
    COUNT(CASE WHEN Status = 2 THEN 1 END) as CompletedSlots, -- AppointmentStatus.Completed = 2
    COUNT(CASE WHEN Status = 3 THEN 1 END) as CancelledSlots -- AppointmentStatus.Cancelled = 3
FROM DoctorTimeSlots;
GO

-- ✅ 2. بررسی اسلات‌های تکراری (همان پزشک، همان تاریخ، همان زمان)
PRINT '=== بررسی اسلات‌های تکراری ===';
SELECT 
    DoctorId,
    CAST(AppointmentDate AS DATE) as AppointmentDate,
    StartTime,
    EndTime,
    COUNT(*) as DuplicateCount
FROM DoctorTimeSlots
WHERE IsDeleted = 0
GROUP BY DoctorId, CAST(AppointmentDate AS DATE), StartTime, EndTime
HAVING COUNT(*) > 1
ORDER BY DuplicateCount DESC, DoctorId, AppointmentDate;
GO

-- ✅ 3. بررسی اسلات‌های بدون نوبت رزرو شده (Status = Available اما AppointmentId دارد)
PRINT '=== بررسی اسلات‌های با وضعیت نامتناسب ===';
SELECT 
    TimeSlotId,
    DoctorId,
    AppointmentDate,
    StartTime,
    EndTime,
    Status,
    AppointmentId,
    IsDeleted
FROM DoctorTimeSlots
WHERE IsDeleted = 0
  AND Status = 0 -- Available
  AND AppointmentId IS NOT NULL
ORDER BY DoctorId, AppointmentDate, StartTime;
GO

-- ✅ 4. بررسی اسلات‌های رزرو شده بدون AppointmentId
PRINT '=== بررسی اسلات‌های رزرو شده بدون AppointmentId ===';
SELECT 
    TimeSlotId,
    DoctorId,
    AppointmentDate,
    StartTime,
    EndTime,
    Status,
    AppointmentId,
    IsDeleted
FROM DoctorTimeSlots
WHERE IsDeleted = 0
  AND Status = 1 -- Booked
  AND AppointmentId IS NULL
ORDER BY DoctorId, AppointmentDate, StartTime;
GO

-- ✅ 5. بررسی اسلات‌های برای یک پزشک خاص (مثال: DoctorId = X)
-- ⚠️ باید DoctorId واقعی را جایگزین کنید
PRINT '=== بررسی اسلات‌های یک پزشک خاص ===';
DECLARE @DoctorId INT = 1; -- ⚠️ تغییر دهید به DoctorId واقعی

SELECT 
    TimeSlotId,
    DoctorId,
    AppointmentDate,
    CAST(StartTime AS VARCHAR(8)) as StartTime,
    CAST(EndTime AS VARCHAR(8)) as EndTime,
    Duration,
    Status,
    AppointmentId,
    IsDeleted,
    CreatedAt,
    UpdatedAt
FROM DoctorTimeSlots
WHERE DoctorId = @DoctorId
  AND IsDeleted = 0
  AND AppointmentDate >= CAST(GETDATE() AS DATE)
ORDER BY AppointmentDate, StartTime;
GO

-- ✅ 6. بررسی اسلات‌های برای تاریخ‌های خاص (15/10، 18/10، 22/10)
-- ⚠️ باید تاریخ‌های میلادی را جایگزین کنید
PRINT '=== بررسی اسلات‌های برای تاریخ‌های خاص ===';
-- مثال: 1404/10/15 = 2025-12-06 (تقریبی)
-- باید تاریخ‌های دقیق را از PersianDateHelper محاسبه کنید

SELECT 
    TimeSlotId,
    DoctorId,
    AppointmentDate,
    CAST(StartTime AS VARCHAR(8)) as StartTime,
    CAST(EndTime AS VARCHAR(8)) as EndTime,
    Duration,
    Status,
    AppointmentId,
    IsDeleted
FROM DoctorTimeSlots
WHERE IsDeleted = 0
  AND CAST(AppointmentDate AS DATE) IN ('2025-12-06', '2025-12-09', '2025-12-13') -- ⚠️ تغییر دهید به تاریخ‌های واقعی
ORDER BY DoctorId, AppointmentDate, StartTime;
GO

-- ✅ 7. بررسی اسلات‌های با زمان نامعتبر (StartTime >= EndTime)
PRINT '=== بررسی اسلات‌های با زمان نامعتبر ===';
SELECT 
    TimeSlotId,
    DoctorId,
    AppointmentDate,
    CAST(StartTime AS VARCHAR(8)) as StartTime,
    CAST(EndTime AS VARCHAR(8)) as EndTime,
    Duration,
    Status,
    IsDeleted
FROM DoctorTimeSlots
WHERE IsDeleted = 0
  AND StartTime >= EndTime
ORDER BY DoctorId, AppointmentDate;
GO

-- ✅ 8. بررسی اسلات‌های با Duration نامعتبر
PRINT '=== بررسی اسلات‌های با Duration نامعتبر ===';
SELECT 
    TimeSlotId,
    DoctorId,
    AppointmentDate,
    CAST(StartTime AS VARCHAR(8)) as StartTime,
    CAST(EndTime AS VARCHAR(8)) as EndTime,
    Duration,
    DATEDIFF(MINUTE, StartTime, EndTime) as CalculatedDuration,
    Status,
    IsDeleted
FROM DoctorTimeSlots
WHERE IsDeleted = 0
  AND (Duration <= 0 OR Duration > 120 OR DATEDIFF(MINUTE, StartTime, EndTime) != Duration)
ORDER BY DoctorId, AppointmentDate;
GO

-- ✅ 9. بررسی اسلات‌های بدون DoctorId معتبر
PRINT '=== بررسی اسلات‌های بدون DoctorId معتبر ===';
SELECT 
    ts.TimeSlotId,
    ts.DoctorId,
    ts.AppointmentDate,
    ts.StartTime,
    ts.EndTime,
    ts.Status,
    ts.IsDeleted
FROM DoctorTimeSlots ts
LEFT JOIN Doctors d ON ts.DoctorId = d.DoctorId
WHERE ts.IsDeleted = 0
  AND d.DoctorId IS NULL
ORDER BY ts.DoctorId, ts.AppointmentDate;
GO

-- ✅ 10. گزارش خلاصه برای هر پزشک
PRINT '=== گزارش خلاصه برای هر پزشک ===';
SELECT 
    d.DoctorId,
    d.FullName,
    COUNT(ts.TimeSlotId) as TotalSlots,
    COUNT(CASE WHEN ts.IsDeleted = 0 THEN 1 END) as ActiveSlots,
    COUNT(CASE WHEN ts.IsDeleted = 0 AND ts.Status = 0 THEN 1 END) as AvailableSlots,
    COUNT(CASE WHEN ts.IsDeleted = 0 AND ts.Status = 1 THEN 1 END) as BookedSlots,
    MIN(ts.AppointmentDate) as EarliestSlot,
    MAX(ts.AppointmentDate) as LatestSlot
FROM Doctors d
LEFT JOIN DoctorTimeSlots ts ON d.DoctorId = ts.DoctorId
WHERE d.IsDeleted = 0
GROUP BY d.DoctorId, d.FullName
HAVING COUNT(ts.TimeSlotId) > 0
ORDER BY d.FullName;
GO

PRINT '✅ بررسی کامل شد!';
GO

