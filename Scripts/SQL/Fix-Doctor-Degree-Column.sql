-- ============================================
-- Fix Doctor.Degree Column
-- تبدیل String/NULL به Integer (Enum Values)
-- ============================================

USE [ClinicDb]  -- ✅ Database name از Connection String
GO

-- 1️⃣ چک کردن نوع فعلی ستون
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.Doctors')
    AND c.name = 'Degree';
GO

-- 2️⃣ بررسی مقادیر فعلی در جدول
SELECT 
    Degree,
    COUNT(*) AS Count
FROM Doctors
GROUP BY Degree
ORDER BY Count DESC;
GO

-- 3️⃣ Backup جدول (برای احتیاط)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Doctors_Backup_Degree')
BEGIN
    SELECT * INTO Doctors_Backup_Degree FROM Doctors;
    PRINT '✅ Backup created: Doctors_Backup_Degree';
END
ELSE
BEGIN
    PRINT '⚠️ Backup already exists. Skipping...';
END
GO

-- 4️⃣ تبدیل مقادیر String به Integer
-- اگر ستون String است، این کار را انجام دهید:
BEGIN TRANSACTION;

-- Update string values to integers
UPDATE Doctors 
SET Degree = CASE 
    WHEN Degree = 'GeneralPhysician' OR Degree = '1' THEN 1
    WHEN Degree = 'Specialist' OR Degree = '2' THEN 2
    WHEN Degree = 'SubSpecialist' OR Degree = '3' THEN 3
    WHEN Degree = 'Dentist' OR Degree = '4' THEN 4
    WHEN Degree = 'Pharmacist' OR Degree = '5' THEN 5
    WHEN Degree IS NULL THEN NULL
    ELSE NULL  -- برای مقادیر نامعتبر
END
WHERE Degree IS NOT NULL OR Degree IN ('GeneralPhysician', 'Specialist', 'SubSpecialist', 'Dentist', 'Pharmacist', '1', '2', '3', '4', '5');

PRINT '✅ Updated ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' records';

COMMIT TRANSACTION;
GO

-- 5️⃣ بررسی نتیجه
SELECT 
    CASE 
        WHEN Degree = 1 THEN 'GeneralPhysician'
        WHEN Degree = 2 THEN 'Specialist'
        WHEN Degree = 3 THEN 'SubSpecialist'
        WHEN Degree = 4 THEN 'Dentist'
        WHEN Degree = 5 THEN 'Pharmacist'
        ELSE 'NULL/Unknown'
    END AS DegreeName,
    COUNT(*) AS Count
FROM Doctors
GROUP BY Degree
ORDER BY Count DESC;
GO

-- 6️⃣ تغییر نوع ستون به tinyint (اگر هنوز string است)
-- ⚠️ فقط در صورتی که ستون String است این را اجرا کنید:
/*
BEGIN TRANSACTION;

-- Drop constraint if exists
IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Doctors_Degree')
BEGIN
    ALTER TABLE Doctors DROP CONSTRAINT DF_Doctors_Degree;
END

-- Drop index if exists
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Doctor_Degree')
BEGIN
    DROP INDEX IX_Doctor_Degree ON Doctors;
END

-- Alter column type
ALTER TABLE Doctors 
ALTER COLUMN Degree TINYINT NULL;

-- Recreate index
CREATE NONCLUSTERED INDEX IX_Doctor_Degree 
ON Doctors (Degree);

PRINT '✅ Column type changed to TINYINT';

COMMIT TRANSACTION;
*/
GO

PRINT '============================================';
PRINT '✅ Script completed successfully!';
PRINT '============================================';
GO

