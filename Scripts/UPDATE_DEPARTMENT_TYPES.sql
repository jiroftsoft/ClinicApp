-- =============================================
-- بروزرسانی نوع دپارتمان‌های موجود
-- تاریخ: 1404/10/05
-- =============================================

USE [ClinicDb]
GO

BEGIN TRANSACTION;

PRINT '🔄 شروع بروزرسانی نوع دپارتمان‌ها...';

-- 1. دپارتمان‌های اورژانس
UPDATE Departments 
SET Type = 5 -- Emergency
WHERE (Name LIKE N'%اورژانس%' OR Name LIKE N'%emergency%' OR Name LIKE N'%Emergency%')
  AND IsDeleted = 0;
PRINT '✅ دپارتمان‌های اورژانس بروزرسانی شدند';

-- 2. دپارتمان‌های تزریقات
UPDATE Departments 
SET Type = 6 -- Injection
WHERE (Name LIKE N'%تزریقات%' OR Name LIKE N'%injection%' OR Name LIKE N'%Injection%')
  AND IsDeleted = 0;
PRINT '✅ دپارتمان‌های تزریقات بروزرسانی شدند';

-- 3. دپارتمان‌های اداری
UPDATE Departments 
SET Type = 2 -- Administrative
WHERE (Name LIKE N'%اداری%' OR Name LIKE N'%امور مالی%' OR Name LIKE N'%منابع انسانی%' 
       OR Name LIKE N'%IT%' OR Name LIKE N'%حسابداری%')
  AND IsDeleted = 0;
PRINT '✅ دپارتمان‌های اداری بروزرسانی شدند';

-- 4. دپارتمان‌های پاراکلینیک
UPDATE Departments 
SET Type = 4 -- Paraclinical
WHERE (Name LIKE N'%آزمایشگاه%' OR Name LIKE N'%رادیولوژی%' OR Name LIKE N'%سونوگرافی%' 
       OR Name LIKE N'%Lab%' OR Name LIKE N'%Laboratory%')
  AND IsDeleted = 0;
PRINT '✅ دپارتمان‌های پاراکلینیک بروزرسانی شدند';

-- 5. دپارتمان‌های جراحی
UPDATE Departments 
SET Type = 7 -- Surgery
WHERE (Name LIKE N'%جراحی%' OR Name LIKE N'%Surgery%')
  AND IsDeleted = 0;
PRINT '✅ دپارتمان‌های جراحی بروزرسانی شدند';

-- 6. دپارتمان‌های بستری
UPDATE Departments 
SET Type = 8 -- Inpatient
WHERE (Name LIKE N'%بستری%' OR Name LIKE N'%بیمارستان%' OR Name LIKE N'%Inpatient%')
  AND IsDeleted = 0;
PRINT '✅ دپارتمان‌های بستری بروزرسانی شدند';

-- 7. دپارتمان‌های پذیرش و ترخیص
UPDATE Departments 
SET Type = 3 -- AdmissionDischarge
WHERE (Name LIKE N'%پذیرش%' OR Name LIKE N'%ترخیص%' OR Name LIKE N'%Admission%')
  AND IsDeleted = 0;
PRINT '✅ دپارتمان‌های پذیرش و ترخیص بروزرسانی شدند';

-- 8. بقیه دپارتمان‌ها (درمانی)
UPDATE Departments 
SET Type = 1 -- Medical
WHERE Type = 0 OR Type IS NULL
  AND IsDeleted = 0;
PRINT '✅ سایر دپارتمان‌ها به عنوان درمانی تنظیم شدند';

-- گزارش نهایی
PRINT '';
PRINT '📊 گزارش نهایی:';
SELECT 
    CASE Type
        WHEN 1 THEN N'درمانی'
        WHEN 2 THEN N'اداری'
        WHEN 3 THEN N'پذیرش و ترخیص'
        WHEN 4 THEN N'پاراکلینیک'
        WHEN 5 THEN N'اورژانس'
        WHEN 6 THEN N'تزریقات'
        WHEN 7 THEN N'جراحی'
        WHEN 8 THEN N'بستری'
        WHEN 9 THEN N'توانبخشی'
        WHEN 10 THEN N'دارویی'
        ELSE N'سایر'
    END AS [نوع دپارتمان],
    COUNT(*) AS [تعداد]
FROM Departments
WHERE IsDeleted = 0
GROUP BY Type
ORDER BY Type;

COMMIT TRANSACTION;
PRINT '';
PRINT '✅ بروزرسانی با موفقیت انجام شد!';
GO

