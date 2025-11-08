-- ============================================
-- Verification: چک کردن Fix شدن Doctor.Degree
-- ============================================

USE [ClinicDb]
GO

PRINT '============================================';
PRINT '🔍 بررسی Fix شدن Doctor.Degree Column';
PRINT '============================================';
PRINT '';

-- 1️⃣ چک نوع ستون
PRINT '1️⃣ نوع ستون Degree:';
PRINT '--------------------------------------------';
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable,
    CASE 
        WHEN t.name = 'tinyint' THEN '✅ صحیح'
        ELSE '❌ نیاز به Fix دارد'
    END AS Status
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.Doctors')
    AND c.name = 'Degree';
PRINT '';
PRINT '';

-- 2️⃣ توزیع مقادیر Degree
PRINT '2️⃣ توزیع مقادیر Degree:';
PRINT '--------------------------------------------';
SELECT 
    Degree,
    CASE 
        WHEN Degree = 1 THEN 'GeneralPhysician (پزشک عمومی)'
        WHEN Degree = 2 THEN 'Specialist (متخصص)'
        WHEN Degree = 3 THEN 'SubSpecialist (فوق تخصص)'
        WHEN Degree = 4 THEN 'Dentist (دندانپزشک)'
        WHEN Degree = 5 THEN 'Pharmacist (داروساز)'
        WHEN Degree IS NULL THEN 'NULL'
        ELSE 'Invalid (' + CAST(Degree AS VARCHAR(10)) + ')'
    END AS DegreeName,
    COUNT(*) AS Count,
    CAST(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER() AS DECIMAL(5,2)) AS Percentage,
    CASE 
        WHEN Degree BETWEEN 1 AND 5 OR Degree IS NULL THEN '✅'
        ELSE '❌'
    END AS Status
FROM Doctors
GROUP BY Degree
ORDER BY Degree;
PRINT '';
PRINT '';

-- 3️⃣ مقادیر نامعتبر (اگر وجود دارند)
PRINT '3️⃣ چک مقادیر نامعتبر:';
PRINT '--------------------------------------------';
IF EXISTS (SELECT 1 FROM Doctors WHERE Degree NOT BETWEEN 1 AND 5 AND Degree IS NOT NULL)
BEGIN
    PRINT '❌ مقادیر نامعتبر یافت شد:';
    SELECT 
        DoctorId,
        FirstName + ' ' + LastName AS DoctorName,
        Degree,
        'نامعتبر' AS Status
    FROM Doctors
    WHERE Degree NOT BETWEEN 1 AND 5 AND Degree IS NOT NULL;
END
ELSE
BEGIN
    PRINT '✅ همه مقادیر معتبر هستند (1-5 یا NULL)';
END
PRINT '';
PRINT '';

-- 4️⃣ نمونه رکوردها
PRINT '4️⃣ نمونه 10 رکورد اول:';
PRINT '--------------------------------------------';
SELECT TOP 10
    DoctorId,
    FirstName + ' ' + LastName AS DoctorName,
    Degree,
    CASE 
        WHEN Degree = 1 THEN 'GeneralPhysician'
        WHEN Degree = 2 THEN 'Specialist'
        WHEN Degree = 3 THEN 'SubSpecialist'
        WHEN Degree = 4 THEN 'Dentist'
        WHEN Degree = 5 THEN 'Pharmacist'
        WHEN Degree IS NULL THEN 'NULL'
        ELSE 'Invalid'
    END AS DegreeName,
    CASE 
        WHEN Degree BETWEEN 1 AND 5 OR Degree IS NULL THEN '✅'
        ELSE '❌'
    END AS Status
FROM Doctors
ORDER BY DoctorId;
PRINT '';
PRINT '';

-- 5️⃣ آمار کلی
PRINT '5️⃣ آمار کلی:';
PRINT '--------------------------------------------';
SELECT 
    COUNT(*) AS TotalDoctors,
    COUNT(Degree) AS WithDegree,
    SUM(CASE WHEN Degree IS NULL THEN 1 ELSE 0 END) AS NullDegree,
    SUM(CASE WHEN Degree NOT BETWEEN 1 AND 5 AND Degree IS NOT NULL THEN 1 ELSE 0 END) AS InvalidDegree,
    CASE 
        WHEN SUM(CASE WHEN Degree NOT BETWEEN 1 AND 5 AND Degree IS NOT NULL THEN 1 ELSE 0 END) = 0 
        THEN '✅ همه مقادیر صحیح'
        ELSE '❌ مقادیر نامعتبر وجود دارد'
    END AS OverallStatus
FROM Doctors;
PRINT '';
PRINT '';

PRINT '============================================';
PRINT '✅ بررسی کامل شد!';
PRINT '============================================';
PRINT '';
PRINT '📝 مراحل بعدی:';
PRINT '   1. اگر Status ها ✅ هستند → Application را Restart کنید';
PRINT '   2. اگر Status ها ❌ هستند → با Developer تماس بگیرید';
PRINT '';
GO

